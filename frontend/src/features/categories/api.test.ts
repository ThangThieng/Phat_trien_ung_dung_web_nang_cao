import { apiFetch } from '@/lib/api-client';
import { getCategories, getCategoryBySlug } from './api';

jest.mock('@/lib/api-client', () => ({ apiFetch: jest.fn().mockResolvedValue([]) }));

const mockedFetch = jest.mocked(apiFetch);

/**
 * TTL của cache Redis phía API, tính bằng giây — SRS NFR-PERF-003 là nguồn sự thật duy nhất. Đổi bảng TTL ở Backend thì phải
 * đổi cả ở đây, và kiểm thử đỏ nếu `revalidate` của Frontend vượt TTL (quy tắc vàng, kế hoạch §2.3).
 */
const API_TTL_SECONDS = { 'categories:all': 30 * 60, 'categories:detail': 2 * 60 } as const;

const lastCall = () => {
  const [path, options] = mockedFetch.mock.calls[mockedFetch.mock.calls.length - 1];
  return { path, next: options?.next };
};

beforeEach(() => mockedFetch.mockClear());

describe('features/categories/api – revalidate bám TTL cache API (D-7)', () => {
  it('getCategories: gọi /categories với next.revalidate === 1800 (30 phút)', async () => {
    await getCategories();

    const { path, next } = lastCall();
    expect(path).toBe('/categories');
    expect(next?.revalidate).toBe(1800);
    expect(next?.revalidate).toBeLessThanOrEqual(API_TTL_SECONDS['categories:all']);
  });

  it('getCategoryBySlug: next.revalidate === 120 (2 phút), ≤ TTL categories:detail', async () => {
    await getCategoryBySlug('mon-chinh');

    const { next } = lastCall();
    expect(next?.revalidate).toBe(120);
    expect(next?.revalidate).toBeLessThanOrEqual(API_TTL_SECONDS['categories:detail']);
  });

  it('getCategoryBySlug: chỉ gửi page, pageSize, sortBy, sortOrder (SRS §8.2 — không có bộ lọc)', async () => {
    await getCategoryBySlug('mon-chinh', { page: 2, sortBy: 'title', sortOrder: 'asc' });

    const url = new URL(lastCall().path, 'http://api.test');
    expect(url.pathname).toBe('/categories/mon-chinh');
    expect(Object.fromEntries(url.searchParams)).toEqual({
      page: '2',
      pageSize: '12',
      sortBy: 'title',
      sortOrder: 'asc',
    });
  });

  it('getCategoryBySlug: mã hóa slug trong đường dẫn', async () => {
    await getCategoryBySlug('a/b?c');

    expect(lastCall().path.startsWith('/categories/a%2Fb%3Fc?')).toBe(true);
  });
});
