import { waitFor } from '@testing-library/react';
import { ApiError, apiFetch, configureAuthCallbacks, createCorrelationId, mapProblemDetailsToForm } from './api-client';

jest.mock('./config', () => ({ getApiBaseUrl: () => 'http://localhost/api/v1' }));

/** `ky` gửi `Request` thật và đọc `Response` thật — mỗi lần gọi phải là một Response mới (body chỉ đọc được một lần). */
function response(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/problem+json' } });
}

const authorizationOf = (input: unknown) => (input as Request).headers.get('Authorization');

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
    const fetchMock = jest.fn(async (input: unknown) => (
      authorizationOf(input) === 'Bearer renewed'
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
    global.fetch = jest.fn().mockImplementation(async () => response(401, { type: 'AUTH_TOKEN_EXPIRED' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ status: 401 });
    expect(onRefreshFailure).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledTimes(1);
  });

  it('does not refresh invalid tokens or enter a loop when the retry fails', async () => {
    const refresh = jest.fn().mockResolvedValue('renewed');
    configureAuthCallbacks({ getAccessToken: () => 'invalid', refresh, onRefreshFailure: jest.fn() });
    global.fetch = jest.fn().mockImplementation(async () => response(401, { type: 'AUTH_TOKEN_INVALID' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ code: 'AUTH_TOKEN_INVALID' });
    expect(refresh).not.toHaveBeenCalled();

    global.fetch = jest.fn().mockImplementation(async () => response(401, { type: 'AUTH_TOKEN_EXPIRED' }));
    await expect(apiFetch('/auth/me')).rejects.toMatchObject({ status: 401 });
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledTimes(2);
  });

  it.each([403, 404, 409, 500])('preserves retry status %s without invalidating auth', async (status) => {
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh: jest.fn().mockResolvedValue('renewed'), onRefreshFailure });
    global.fetch = jest.fn().mockImplementationOnce(async () => response(401, { type: 'AUTH_TOKEN_EXPIRED' }))
      .mockImplementationOnce(async () => response(status, { type: 'BUSINESS_ERROR', detail: 'retry failure' }));
    await expect(apiFetch('/resource')).rejects.toMatchObject({ status, message: 'retry failure' });
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('preserves a retry network error without invalidating auth', async () => {
    const onRefreshFailure = jest.fn();
    const networkError = new TypeError('network down');
    configureAuthCallbacks({ getAccessToken: () => 'expired', refresh: jest.fn().mockResolvedValue('renewed'), onRefreshFailure });
    global.fetch = jest.fn().mockImplementationOnce(async () => response(401, { type: 'AUTH_TOKEN_EXPIRED' })).mockRejectedValueOnce(networkError);
    await expect(apiFetch('/resource')).rejects.toBe(networkError);
    expect(onRefreshFailure).not.toHaveBeenCalled();
  });

  it('invalidates an authenticated invalid token without refreshing', async () => {
    const refresh = jest.fn();
    const onRefreshFailure = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'invalid', refresh, onRefreshFailure });
    global.fetch = jest.fn().mockImplementation(async () => response(401, { type: 'AUTH_TOKEN_INVALID' }));
    await expect(apiFetch('/resource')).rejects.toMatchObject({ code: 'AUTH_TOKEN_INVALID' });
    expect(refresh).not.toHaveBeenCalled();
    expect(onRefreshFailure).toHaveBeenCalledTimes(1);
  });

  it('retries a late expired response with the already renewed token', async () => {
    const refresh = jest.fn();
    configureAuthCallbacks({ getAccessToken: () => 'renewed', refresh, onRefreshFailure: jest.fn() });
    global.fetch = jest.fn().mockImplementationOnce(async () => response(401, { type: 'AUTH_TOKEN_EXPIRED' })).mockImplementationOnce(async () => response(200, {}));
    await apiFetch('/resource', { accessToken: 'expired' });
    expect(refresh).not.toHaveBeenCalled();
    const lastCall = (global.fetch as jest.Mock).mock.calls.at(-1) as [Request];
    expect(authorizationOf(lastCall[0])).toBe('Bearer renewed');
  });
});


const FIELDS = ['email', 'password'] as const;

function makeValidationError(status: number, errors: Record<string, string[]>) {
  return new ApiError(status, {
    type: 'VALIDATION_ERROR',
    title: 'Dữ liệu không hợp lệ',
    status,
    detail: 'Một hoặc nhiều trường không hợp lệ. Xem "errors".',
    errors,
  });
}

describe('mapProblemDetailsToForm (D-11 / MT-08)', () => {
  it('ánh xạ errors{} của lỗi 400 vào đúng từng ô của form', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(400, { email: ['Email không hợp lệ.'], password: ['Mật khẩu quá ngắn.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(true);
    expect(setError).toHaveBeenCalledWith('email', { message: 'Email không hợp lệ.' });
    expect(setError).toHaveBeenCalledWith('password', { message: 'Mật khẩu quá ngắn.' });
  });

  it('bỏ qua field mà form không khai báo', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(400, { userName: ['Không hợp lệ.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('không xử lý 422 nữa – mã này đã bị loại bỏ khỏi hệ thống', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(422, { email: ['Email không hợp lệ.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('trả false cho lỗi không phải validation (401) để caller hiện lỗi chung', () => {
    const setError = jest.fn();
    const error = new ApiError(401, {
      type: 'AUTH_INVALID_CREDENTIALS',
      detail: 'Email hoặc mật khẩu không đúng.',
    });

    expect(mapProblemDetailsToForm(error, FIELDS, setError)).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('trả false cho lỗi mạng (không phải ApiError)', () => {
    const setError = jest.fn();

    expect(mapProblemDetailsToForm(new TypeError('fetch failed'), FIELDS, setError)).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });
});

describe('ky-based client (Lab 6 – Dev 4)', () => {
  const originalFetch = global.fetch;
  const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;

  afterEach(() => {
    configureAuthCallbacks(null);
    global.fetch = originalFetch;
  });

  function mockFetch(handler: (request: Request, call: number) => Response | Promise<Response>) {
    let call = 0;
    const fetchMock = jest.fn(async (input: unknown) => {
      call += 1;
      return handler(input as Request, call);
    });
    global.fetch = fetchMock as unknown as typeof fetch;
    return fetchMock;
  }

  const requestOf = (fetchMock: jest.Mock, index: number) => fetchMock.mock.calls[index][0] as Request;

  it('sends a fresh UUID X-Correlation-ID with every request', async () => {
    const fetchMock = mockFetch(() => Response.json({}));
    await Promise.all([apiFetch('/categories'), apiFetch('/recipes'), apiFetch('/recipes', { method: 'POST', body: {} })]);
    const ids = fetchMock.mock.calls.map(([input]) => (input as Request).headers.get('X-Correlation-ID'));
    ids.forEach((id) => expect(id).toMatch(UUID));
    expect(new Set(ids).size).toBe(3);
  });

  it('builds a UUID v4 without crypto.randomUUID (http://<LAN IP> is not a secure context)', () => {
    const original = crypto.randomUUID;
    Object.defineProperty(crypto, 'randomUUID', { value: undefined, configurable: true });
    try {
      expect(createCorrelationId()).toMatch(UUID);
    } finally {
      Object.defineProperty(crypto, 'randomUUID', { value: original, configurable: true });
    }
  });

  it('refreshes once for 5 parallel expired requests and retries each with the new token', async () => {
    let token = 'expired';
    const refresh = jest.fn(async () => { token = 'renewed'; return token; });
    configureAuthCallbacks({ getAccessToken: () => token, refresh, onRefreshFailure: jest.fn() });
    const fetchMock = mockFetch((request) => (
      request.headers.get('Authorization') === 'Bearer renewed'
        ? Response.json({ ok: true })
        : response(401, { type: 'AUTH_TOKEN_EXPIRED' })
    ));
    await expect(Promise.all(Array.from({ length: 5 }, () => apiFetch('/auth/me'))))
      .resolves.toEqual(Array(5).fill({ ok: true }));
    expect(refresh).toHaveBeenCalledTimes(1);
    const retried = fetchMock.mock.calls.slice(5).map(([input]) => (input as Request).headers.get('Authorization'));
    expect(retried).toEqual(Array(5).fill('Bearer renewed'));
  });

  it('retries a POST exactly once after refreshing an expired token and keeps its JSON body', async () => {
    let token = 'expired';
    configureAuthCallbacks({
      getAccessToken: () => token,
      refresh: jest.fn(async () => { token = 'renewed'; return token; }),
      onRefreshFailure: jest.fn(),
    });
    const bodies: unknown[] = [];
    const fetchMock = mockFetch(async (request) => {
      bodies.push(await request.json());
      return request.headers.get('Authorization') === 'Bearer renewed'
        ? Response.json({ id: 'r1' }, { status: 201 })
        : response(401, { type: 'AUTH_TOKEN_EXPIRED' });
    });
    await expect(apiFetch('/recipes', { method: 'POST', body: { title: 'Phở bò' } })).resolves.toEqual({ id: 'r1' });
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(bodies).toEqual([{ title: 'Phở bò' }, { title: 'Phở bò' }]);
  });

  it('waits for Retry-After and retries a GET that got 429 once', async () => {
    const fetchMock = mockFetch((_request, call) => (call === 1
      ? new Response(JSON.stringify({ type: 'RATE_LIMIT_EXCEEDED' }), { status: 429, headers: { 'Retry-After': '1' } })
      : Response.json({ items: [] })));
    const started = Date.now();
    await expect(apiFetch('/recipes')).resolves.toEqual({ items: [] });
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(Date.now() - started).toBeGreaterThanOrEqual(900);
  });

  it('gives up on 429 when Retry-After is longer than 5 seconds and reports how long to wait', async () => {
    const fetchMock = mockFetch(() => new Response(
      JSON.stringify({ type: 'RATE_LIMIT_EXCEEDED', status: 429 }),
      { status: 429, headers: { 'Retry-After': '30' } },
    ));
    await expect(apiFetch('/recipes')).rejects.toMatchObject({ status: 429, code: 'RATE_LIMIT_EXCEEDED', retryAfterSeconds: 30 });
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it('retries a GET on 503 but never retries a POST', async () => {
    const getMock = mockFetch((_request, call) => (call < 3 ? response(503, { type: 'FILE_STORAGE_UNAVAILABLE' }) : Response.json([])));
    await expect(apiFetch('/categories')).resolves.toEqual([]);
    expect(getMock).toHaveBeenCalledTimes(3);

    const postMock = mockFetch(() => response(503, { type: 'FILE_STORAGE_UNAVAILABLE' }));
    await expect(apiFetch('/recipes', { method: 'POST', body: { title: 'x' } })).rejects.toMatchObject({ status: 503 });
    expect(postMock).toHaveBeenCalledTimes(1);
  });

  it('does not retry 500 because a server bug answers the same way twice', async () => {
    const fetchMock = mockFetch(() => response(500, { type: 'INTERNAL_ERROR' }));
    await expect(apiFetch('/recipes')).rejects.toMatchObject({ status: 500 });
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it('passes the Next.js next.revalidate option through to fetch', async () => {
    const fetchMock = mockFetch(() => Response.json([]));
    await apiFetch('/categories', { next: { revalidate: 1800, tags: ['categories'] } });
    const init = (fetchMock.mock.calls[0] as unknown[])[1] as { next?: NextFetchRequestConfig } | undefined;
    expect(init?.next).toEqual({ revalidate: 1800, tags: ['categories'] });
  });

  it('exposes the correlation id from the response header, falling back to the Problem Details traceId', async () => {
    mockFetch(() => new Response(
      JSON.stringify({ type: 'INTERNAL_ERROR', traceId: '00-trace-01' }),
      { status: 500, headers: { 'X-Correlation-ID': 'corr-123' } },
    ));
    await expect(apiFetch('/recipes')).rejects.toMatchObject({ correlationId: 'corr-123' });

    mockFetch(() => response(500, { type: 'INTERNAL_ERROR', traceId: '00-trace-01' }));
    await expect(apiFetch('/recipes')).rejects.toMatchObject({ correlationId: '00-trace-01' });
  });

  it('turns a non-JSON proxy error page into an ApiError with the HTTP status', async () => {
    mockFetch(() => new Response('<html>502 Bad Gateway</html>', { status: 502, statusText: 'Bad Gateway' }));
    const error = await apiFetch('/auth/me', { method: 'POST' }).catch((e: unknown) => e);
    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 502, code: 'UNKNOWN_ERROR' });
  });

  it('sends FormData as-is and returns undefined for 204', async () => {
    const fetchMock = mockFetch(() => new Response(null, { status: 204 }));
    const form = new FormData();
    form.append('altText', 'Phở');
    await expect(apiFetch('/recipes/r1/images', { method: 'POST', body: form })).resolves.toBeUndefined();
    expect(requestOf(fetchMock, 0).headers.get('Content-Type')).toMatch(/^multipart\/form-data; boundary=/);
  });

  it('sends Authorization only when there is a token', async () => {
    configureAuthCallbacks({ getAccessToken: () => 'session-token', refresh: jest.fn(), onRefreshFailure: jest.fn() });
    const fetchMock = mockFetch(() => Response.json({}));
    await apiFetch('/auth/me');
    await apiFetch('/auth/refresh', { method: 'POST', body: {}, accessToken: null });
    expect(requestOf(fetchMock, 0).headers.get('Authorization')).toBe('Bearer session-token');
    expect(requestOf(fetchMock, 1).headers.has('Authorization')).toBe(false);
  });
});
