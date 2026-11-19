/**
 * @jest-environment node
 */
import { ApiError, apiFetch, configureServerRequestContext, RENDER_GATE_HEADER } from './api-client';

jest.mock('./config', () => ({ getApiBaseUrl: () => 'http://api:8080/api/v1' }));

/**
 * CR-2026-10 (SRS v1.2.8, MT-73): request nạp Data Cache phía server mang bí mật render để API dùng hạn mức render chung thay
 * vì hạn mức theo IP của container Frontend; API trả 429 khi đang render → chuyển sang trang 429 thật thay vì trang 500.
 */
describe('apiFetch render gate and 429 while rendering', () => {
  const originalFetch = global.fetch;
  const originalSecret = process.env.RENDER_GATE_SECRET;

  afterEach(() => {
    global.fetch = originalFetch;
    configureServerRequestContext(null);
    if (originalSecret === undefined) delete process.env.RENDER_GATE_SECRET;
    else process.env.RENDER_GATE_SECRET = originalSecret;
  });

  const requestsOf = (mock: jest.Mock) => mock.mock.calls.map((call) => (call as unknown as [Request])[0]);

  it('sends the render secret only on Data Cache requests, never on per-viewer requests', async () => {
    process.env.RENDER_GATE_SECRET = 'render-secret';
    configureServerRequestContext({ forwardedFor: async () => '203.0.113.7' });
    const fetchMock = jest.fn(async () => Response.json([]));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/categories', { next: { revalidate: 1800 } });
    await apiFetch('/recipes/search?q=pho', { cache: 'no-store' });

    const [cached, perViewer] = requestsOf(fetchMock);
    expect(cached.headers.get(RENDER_GATE_HEADER)).toBe('render-secret');
    expect(cached.headers.has('X-Forwarded-For')).toBe(false);
    expect(perViewer.headers.has(RENDER_GATE_HEADER)).toBe(false);
    expect(perViewer.headers.get('X-Forwarded-For')).toBe('203.0.113.7');
  });

  it('sends no render header when the secret is not configured', async () => {
    delete process.env.RENDER_GATE_SECRET;
    const fetchMock = jest.fn(async () => Response.json([]));
    global.fetch = fetchMock as unknown as typeof fetch;

    await apiFetch('/categories', { next: { revalidate: 1800 } });

    expect(requestsOf(fetchMock)[0].headers.has(RENDER_GATE_HEADER)).toBe(false);
  });

  it('hands a 429 to the server rate-limit handler (which redirects) instead of throwing it to the page', async () => {
    const redirected = new Error('NEXT_REDIRECT');
    const onRateLimited = jest.fn(async () => {
      throw redirected;
    });
    configureServerRequestContext({ forwardedFor: async () => null, onRateLimited });
    global.fetch = jest.fn(async () =>
      Response.json(
        { type: 'RATE_LIMIT_EXCEEDED', status: 429 },
        { status: 429, headers: { 'Retry-After': '42' } },
      ),
    ) as unknown as typeof fetch;

    await expect(apiFetch('/recipes/pho-bo', { next: { revalidate: 300 } })).rejects.toBe(redirected);
    expect(onRateLimited).toHaveBeenCalledTimes(1);
    const [error] = onRateLimited.mock.calls[0] as unknown as [ApiError];
    expect(error).toBeInstanceOf(ApiError);
    expect([error.status, error.retryAfterSeconds]).toEqual([429, 42]);
  });

  it('keeps other errors unchanged', async () => {
    const onRateLimited = jest.fn();
    configureServerRequestContext({ forwardedFor: async () => null, onRateLimited });
    global.fetch = jest.fn(async () => Response.json({ type: 'RECIPE_NOT_FOUND' }, { status: 404 })) as unknown as typeof fetch;

    await expect(apiFetch('/recipes/missing')).rejects.toMatchObject({ status: 404 });
    expect(onRateLimited).not.toHaveBeenCalled();
  });
});
