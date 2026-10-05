import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
import ImageUploader from './ImageUploader';

jest.mock('@/lib/config', () => ({ getApiBaseUrl: () => 'http://localhost/api/v1' }));

describe('upload token refresh', () => {
  const originalFetch = global.fetch;
  const originalCreateUrl = URL.createObjectURL;
  const originalRevokeUrl = URL.revokeObjectURL;
  const attempts: Array<{
    status: number;
    response: unknown;
    onload: (() => void) | null;
    onabort: (() => void) | null;
    upload: { onprogress: ((event: ProgressEvent) => void) | null };
    setRequestHeader: jest.Mock;
  }> = [];

  beforeEach(() => {
    attempts.length = 0;
    URL.createObjectURL = jest.fn(() => 'blob:preview');
    URL.revokeObjectURL = jest.fn();
    jest.spyOn(window, 'XMLHttpRequest').mockImplementation(() => {
      const xhr = {
        status: 0, response: null as unknown, responseType: '', onload: null, onabort: null,
        upload: { onprogress: null }, open: jest.fn(), send: jest.fn(), setRequestHeader: jest.fn(),
        abort: () => { xhr.onabort?.(new ProgressEvent('abort')); },
      } as unknown as XMLHttpRequest;
      attempts.push(xhr as unknown as typeof attempts[number]);
      return xhr;
    });
  });

  afterEach(() => {
    cleanup();
    configureAuthCallbacks(null);
    global.fetch = originalFetch;
    URL.createObjectURL = originalCreateUrl;
    URL.revokeObjectURL = originalRevokeUrl;
    jest.restoreAllMocks();
  });

  function complete(index: number, status: number, body: unknown) {
    act(() => {
      attempts[index].status = status;
      attempts[index].response = body;
      attempts[index].onload?.();
    });
  }

  it('shares refresh with fetch, retries upload once and preserves progress', async () => {
    let token = 'expired';
    let finishRefresh: (value: string) => void = () => {};
    const refresh = jest.fn(() => new Promise<string>((resolve) => { finishRefresh = resolve; }));
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => token, refresh, onRefreshFailure });
    global.fetch = jest.fn(async (_url: unknown, options: RequestInit) => {
      const renewed = (options.headers as Record<string, string>).Authorization === 'Bearer renewed';
      return { ok: renewed, status: renewed ? 200 : 401, json: async () => (renewed ? {} : { type: 'AUTH_TOKEN_EXPIRED' }) } as Response;
    }) as typeof fetch;
    const onChange = jest.fn();
    const { container } = render(<ImageUploader accessToken="expired" onChange={onChange} />);
    fireEvent.change(container.querySelector('input')!, { target: { files: [new File(['image'], 'food.png', { type: 'image/png' })] } });
    complete(0, 401, { type: 'AUTH_TOKEN_EXPIRED' });
    const fetches = Array.from({ length: 5 }, () => apiFetch('/auth/me'));
    await waitFor(() => expect(refresh).toHaveBeenCalledTimes(1));
    await act(async () => { token = 'renewed'; finishRefresh(token); await Promise.all(fetches); });
    await waitFor(() => expect(attempts).toHaveLength(2));
    expect(attempts[1].setRequestHeader).toHaveBeenCalledWith('Authorization', 'Bearer renewed');
    act(() => { attempts[1].upload.onprogress?.({ lengthComputable: true, loaded: 5, total: 10 } as ProgressEvent); });
    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '50');
    const uploaded = { key: 'food.png', url: 'http://localhost/food.png' };
    complete(1, 200, uploaded);
    await waitFor(() => expect(onChange).toHaveBeenCalledWith(uploaded));
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('does not loop when the retried upload is also expired', async () => {
    const refresh = jest.fn().mockResolvedValue('renewed');
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh, onRefreshFailure: jest.fn() });
    const { container } = render(<ImageUploader accessToken="expired" />);
    fireEvent.change(container.querySelector('input')!, { target: { files: [new File(['image'], 'food.png', { type: 'image/png' })] } });
    complete(0, 401, { type: 'AUTH_TOKEN_EXPIRED' });
    await waitFor(() => expect(attempts).toHaveLength(2));
    complete(1, 401, { type: 'AUTH_TOKEN_EXPIRED', detail: 'Expired retry' });
    await screen.findByRole('alert');
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(attempts).toHaveLength(2);
  });
});
