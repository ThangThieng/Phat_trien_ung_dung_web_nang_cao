import { apiFetch } from '@/lib/api-client';
import { HOME_RECIPE_COUNT, getLatestRecipes, getRecipeBySlug, getRecipes } from './api';

jest.mock('@/lib/api-client', () => ({ apiFetch: jest.fn().mockResolvedValue({}) }));

const mockedFetch = jest.mocked(apiFetch);

/** TTL Redis phía API (giây) — SRS NFR-PERF-003, xem `features/categories/api.test.ts`. */
const API_TTL_SECONDS = { 'recipes:list': 2 * 60, recipe: 5 * 60 } as const;

const lastCall = () => {
  const [path, options] = mockedFetch.mock.calls[mockedFetch.mock.calls.length - 1];
  return { path, options };
};

beforeEach(() => mockedFetch.mockClear());

describe('features/recipes/api – revalidate bám TTL cache API', () => {
  it('getLatestRecipes (trang chủ): next.revalidate === 120, ≤ TTL recipes:list', async () => {
    await getLatestRecipes();

    const { options } = lastCall();
    expect(options?.next?.revalidate).toBe(120);
    expect(options?.next?.revalidate).toBeLessThanOrEqual(API_TTL_SECONDS['recipes:list']);
    expect(options?.cache).toBeUndefined();
  });

  it('getLatestRecipes: "nổi bật" = mới xuất bản nhất — sortBy=publishedAt&sortOrder=desc&pageSize=8 (MT-23)', async () => {
    await getLatestRecipes();

    const url = new URL(lastCall().path, 'http://api.test');
    expect(url.pathname).toBe('/recipes');
    expect(Object.fromEntries(url.searchParams)).toEqual({
      pageSize: '8',
      sortBy: 'publishedAt',
      sortOrder: 'desc',
    });
    expect(HOME_RECIPE_COUNT).toBe(8);
  });

  it('getRecipeBySlug (trang chi tiết): next.revalidate === 300, ≤ TTL recipe:{slug}', async () => {
    await getRecipeBySlug('pho-bo');

    const { path, options } = lastCall();
    expect(path).toBe('/recipes/pho-bo');
    expect(options?.next?.revalidate).toBe(300);
    expect(options?.next?.revalidate).toBeLessThanOrEqual(API_TTL_SECONDS.recipe);
  });

  it('getRecipes (/recipes, có bộ lọc do người dùng chọn) vẫn no-store — không cache ở tầng Next.js', async () => {
    await getRecipes({ page: 2 });

    const { options } = lastCall();
    expect(options?.cache).toBe('no-store');
    expect(options?.next).toBeUndefined();
  });
});
