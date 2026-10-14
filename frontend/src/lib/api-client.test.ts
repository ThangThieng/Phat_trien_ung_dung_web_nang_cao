import { waitFor } from '@testing-library/react';
import { apiFetch, configureAuthCallbacks } from './api-client';

jest.mock('./config', () => ({ getApiBaseUrl: () => 'http://localhost/api/v1' }));

function response(status: number, body: unknown): Response {
  return { ok: status >= 200 && status < 300, status, json: async () => body } as Response;
}

describe('automatic access token refresh', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    configureAuthCallbacks(null);
    global.fetch = originalFetch;
  });

  it('shares one refresh across concurrent expired requests and retries with the new token', async () => {
    let completeRefresh: (token: string) => void = () => {};
    const refresh = jest.fn(() => new Promise<string>((resolve) => { completeRefresh = resolve; }));
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh, onRefreshFailure });
    const fetchMock = jest.fn(async (_url: unknown, options: RequestInit) => (
      (options.headers as Record<string, string>).Authorization === 'Bearer renewed'
        ? response(200, { success: true })
        : response(401, { type: 'AUTH_TOKEN_EXPIRED' })
    ));
    global.fetch = fetchMock as typeof fetch;
    const pending = Array.from({ length: 5 }, () => apiFetch('/auth/me'));
    // Let all original requests reach the shared refresh promise before resolving it.
    await waitFor(() => expect(refresh).toHaveBeenCalledTimes(1));
    completeRefresh('renewed');
    await expect(Promise.all(pending)).resolves.toEqual(Array(5).fill({ success: true }));
    expect(fetchMock).toHaveBeenCalledTimes(10);
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('clears the session when refresh fails and does not retry the request', async () => {
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({
      getAccessToken: () => 'expired',
      refresh: jest.fn().mockRejectedValue(new Error('revoked')),
      onRefreshFailure,
    });
    global.fetch = jest.fn().mockResolvedValue(response(401, { type: 'AUTH_TOKEN_EXPIRED' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ status: 401 });
    expect(onRefreshFailure).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledTimes(1);
  });

  it('does not refresh invalid tokens or enter a loop when the retry fails', async () => {
    const refresh = jest.fn().mockResolvedValue('renewed');
    configureAuthCallbacks({ getAccessToken: () => 'invalid', refresh, onRefreshFailure: jest.fn() });
    global.fetch = jest.fn().mockResolvedValue(response(401, { type: 'AUTH_TOKEN_INVALID' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ code: 'AUTH_TOKEN_INVALID' });
    expect(refresh).not.toHaveBeenCalled();

    global.fetch = jest.fn().mockResolvedValue(response(401, { type: 'AUTH_TOKEN_EXPIRED' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ status: 401 });
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledTimes(2);
  });

  it.each([403, 404, 409, 500])('preserves retry status %s without invalidating auth', async (status) => {
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh: jest.fn().mockResolvedValue('renewed'), onRefreshFailure });
    global.fetch = jest.fn().mockResolvedValueOnce(response(401, { type: 'AUTH_TOKEN_EXPIRED' }))
      .mockResolvedValueOnce(response(status, { type: 'BUSINESS_ERROR', detail: 'retry failure' }));
    await expect(apiFetch('/resource')).rejects.toMatchObject({ status, message: 'retry failure' });
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('preserves a retry network error without invalidating auth', async () => {
    const onRefreshFailure = jest.fn();
    const networkError = new TypeError('network down');
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh: jest.fn().mockResolvedValue('renewed'), onRefreshFailure });
    global.fetch = jest.fn().mockResolvedValueOnce(response(401, { type: 'AUTH_TOKEN_EXPIRED' })).mockRejectedValueOnce(networkError);
    await expect(apiFetch('/resource')).rejects.toBe(networkError);
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('invalidates an authenticated invalid token without refreshing', async () => {
    const refresh = jest.fn();
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'invalid', refresh, onRefreshFailure });
    global.fetch = jest.fn().mockResolvedValue(response(401, { type: 'AUTH_TOKEN_INVALID' }));
    await expect(apiFetch('/resource')).rejects.toMatchObject({ code: 'AUTH_TOKEN_INVALID' });
    expect(refresh).not.toHaveBeenCalled();
    expect(onRefreshFailure).toHaveBeenCalledTimes(1);
  });

  it('retries a late expired response with the already renewed token', async () => {
    const refresh = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'renewed', refresh, onRefreshFailure: jest.fn() });
    global.fetch = jest.fn().mockResolvedValueOnce(response(401, { type: 'AUTH_TOKEN_EXPIRED' })).mockResolvedValueOnce(response(200, {}));
    await apiFetch('/resource', { accessToken: 'expired' });
    expect(refresh).not.toHaveBeenCalled();
    expect(global.fetch).toHaveBeenLastCalledWith(expect.any(String), expect.objectContaining({ headers: expect.objectContaining({ Authorization: 'Bearer renewed' }) }));
  });
});
