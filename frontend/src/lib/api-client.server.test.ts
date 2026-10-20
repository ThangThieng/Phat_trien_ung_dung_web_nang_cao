/**
 * @jest-environment node
 */
import { apiFetch, configureAuthCallbacks } from './api-client';

jest.mock('./config', () => ({ getApiBaseUrl: () => 'http://api:8080/api/v1' }));

/**
 * MT-34 ở Frontend: Server Component chỉ gọi endpoint công khai. Kể cả khi có ai đó truyền token hoặc callback phiên
 * còn sót trong process Node, request đi từ server không bao giờ mang `Authorization` và không bao giờ refresh.
 */
describe('apiFetch on the server (Server Components, Route Handlers)', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    configureAuthCallbacks(null);
    global.fetch = originalFetch;
  });

  it('never sends Authorization, even when a token is passed explicitly', async () => {
    configureAuthCallbacks({
      getAccessToken: () => 'leaked',
      refresh: jest.fn(),
      onRefreshFailure: jest.fn(),
    });
    const fetchMock = jest.fn(async () => Response.json([]));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/categories', { next: { revalidate: 1800 } });
    await apiFetch('/recipes/mine', { accessToken: 'explicit-token' });

    const requests = fetchMock.mock.calls.map((call) => (call as unknown as [Request])[0]);
    // URL nội bộ của container API (API_INTERNAL_BASE_URL), không qua Nginx.
    const url = new URL(requests[0].url);
    expect([url.host, url.pathname]).toEqual(['api:8080', '/api/v1/categories']);
    requests.forEach((request) => {
      expect(request.headers.has('Authorization')).toBe(false);
      expect(request.headers.get('X-Correlation-ID')).toBeTruthy();
    });
  });

  it('does not try to refresh on 401', async () => {
    const refresh = jest.fn();
    configureAuthCallbacks({
      getAccessToken: () => 'leaked',
      refresh,
      onRefreshFailure: jest.fn(),
    });
    global.fetch = jest.fn(async () =>
      Response.json({ type: 'AUTH_TOKEN_EXPIRED' }, { status: 401 }),
    ) as unknown as typeof fetch;

    await expect(apiFetch('/recipes/mine')).rejects.toMatchObject({ status: 401 });
    expect(refresh).not.toHaveBeenCalled();
  });
});
