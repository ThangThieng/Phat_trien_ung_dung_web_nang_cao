/**
 * @jest-environment node
 */
import ky from 'ky';

/**
 * Hai điểm kiểm chứng bắt buộc khi cài `ky` (kế hoạch Buổi 5 — Dev 4), trước khi Buổi 6 thay lớp gọi API:
 * 1. Option `next` của Next.js phải tới được `fetch` – rơi mất thì mọi trang dùng Data Cache mất `revalidate`
 *    (SRS §5.1, NFR-PERF-003) mà không có lỗi nào.
 * 2. Jest biên dịch được `ky` (gói ESM thuần) – file này import được `ky` là bằng chứng.
 */
describe('ky on top of the Next.js fetch', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    global.fetch = originalFetch;
  });

  function mockFetch() {
    const fetchMock = jest.fn(async () => Response.json({ ok: true }));
    global.fetch = fetchMock as unknown as typeof fetch;
    return fetchMock;
  }

  it('forwards the next.revalidate option to the global fetch that Next.js patches', async () => {
    const fetchMock = mockFetch();

    await ky.get('http://api.test/api/v1/categories', { next: { revalidate: 300 } }).json();

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [request, init] = fetchMock.mock.calls[0] as unknown as [Request, { next?: NextFetchRequestConfig }];
    expect(request.url).toBe('http://api.test/api/v1/categories');
    expect(init.next).toEqual({ revalidate: 300 });
  });

  it('forwards next.tags together with revalidate and keeps request headers', async () => {
    const fetchMock = mockFetch();

    await ky.get('http://api.test/api/v1/recipes', {
      headers: { Authorization: 'Bearer token' },
      next: { revalidate: 60, tags: ['recipes'] },
    }).json();

    const [request, init] = fetchMock.mock.calls[0] as unknown as [Request, { next?: NextFetchRequestConfig }];
    expect(request.headers.get('Authorization')).toBe('Bearer token');
    expect(init.next).toEqual({ revalidate: 60, tags: ['recipes'] });
  });
});
