/**
 * @jest-environment node
 */
import {
  apiFetch,
  configureAuthCallbacks,
  configureServerRequestContext,
  usesDataCache,
} from './api-client';

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
    requests.forEach((request) => expect(request.headers.has('Authorization')).toBe(false));
  });

  /**
   * Khóa Data Cache của Next.js gồm cả header: một `X-Correlation-ID` mới cho mỗi lời gọi làm mọi lần gọi là một khóa mới →
   * `revalidate` vô tác dụng. Request dùng Data Cache vì vậy không mang header này; request không cache vẫn có.
   */
  it('keeps the Data Cache key stable: no X-Correlation-ID on revalidate requests, a fresh one otherwise', async () => {
    const fetchMock = jest.fn(async () => Response.json([]));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/categories', { next: { revalidate: 1800 } });
    await apiFetch('/categories', { next: { revalidate: 1800 } });
    await apiFetch('/recipes', { cache: 'no-store' });
    await apiFetch('/recipes', { next: { revalidate: 0 } });

    const [cachedA, cachedB, noStore, revalidateZero] = fetchMock.mock.calls.map(
      (call) => (call as unknown as [Request])[0],
    );
    expect(cachedA.headers.has('X-Correlation-ID')).toBe(false);
    expect([...cachedB.headers]).toEqual([...cachedA.headers]);
    expect(noStore.headers.get('X-Correlation-ID')).toBeTruthy();
    expect(revalidateZero.headers.get('X-Correlation-ID')).toBeTruthy();
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

/**
 * Buổi 7 — đo trên Docker trước khi sửa: Next.js lấy header của `Request` làm khóa Data Cache, nên `X-Correlation-ID` ngẫu
 * nhiên khiến 5 lượt tải `/categories` gọi API 15 lần dù `revalidate: 1800`; và mọi request SSR mang IP của container
 * `frontend` nên mọi người dùng chung một hạn mức rate limit.
 */
describe('apiFetch on the server — Data Cache key and client address', () => {
  const originalFetch = global.fetch;
  const headersOf = (fetchMock: jest.Mock, index: number) =>
    Object.fromEntries((fetchMock.mock.calls[index] as unknown as [Request])[0].headers);

  afterEach(() => {
    configureServerRequestContext(null);
    global.fetch = originalFetch;
  });

  it('cached requests carry identical headers every time (no random X-Correlation-ID, no per-user header)', async () => {
    configureServerRequestContext({ forwardedFor: async () => '203.0.113.7' });
    const fetchMock = jest.fn(async () => Response.json([]));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/categories', { next: { revalidate: 1800, tags: ['categories'] } });
    await apiFetch('/categories', { next: { revalidate: 1800, tags: ['categories'] } });

    expect(headersOf(fetchMock, 0)).toEqual(headersOf(fetchMock, 1));
    expect(headersOf(fetchMock, 0)).not.toHaveProperty('x-correlation-id');
    expect(headersOf(fetchMock, 0)).not.toHaveProperty('x-forwarded-for');
  });

  it('uncached requests keep a correlation id and forward the browser X-Forwarded-For chain unchanged', async () => {
    configureServerRequestContext({ forwardedFor: async () => '198.51.100.9, 203.0.113.7' });
    const fetchMock = jest.fn(async () => Response.json({ items: [] }));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/recipes/search?q=ga', { cache: 'no-store' });

    const headers = headersOf(fetchMock, 0);
    expect(headers['x-correlation-id']).toMatch(/^[0-9a-f-]{36}$/);
    // API lấy giá trị ngoài cùng bên phải (do Nginx thêm) — phần client tự khai không được tin.
    expect(headers['x-forwarded-for']).toBe('198.51.100.9, 203.0.113.7');
  });

  it('without a browser request (e.g. container calling port 3000 directly) nothing is forwarded', async () => {
    configureServerRequestContext({ forwardedFor: async () => null });
    const fetchMock = jest.fn(async () => Response.json({}));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/recipes/search?q=ga', { cache: 'no-store' });
    expect(headersOf(fetchMock, 0)).not.toHaveProperty('x-forwarded-for');
  });

  it.each([
    [{ next: { revalidate: 300 } }, true],
    [{ next: { revalidate: false as const } }, true],
    [{ cache: 'force-cache' as const }, true],
    [{ next: { revalidate: 0 } }, false],
    [{ cache: 'no-store' as const }, false],
    [{ next: { revalidate: 300 }, cache: 'no-store' as const }, false],
    [{}, false],
  ])('usesDataCache(%o) = %s', (options, expected) => {
    expect(usesDataCache(options)).toBe(expected);
  });
});
