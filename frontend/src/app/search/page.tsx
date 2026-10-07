import type { Metadata } from 'next';
import Link from 'next/link';
import { Search } from 'lucide-react';
import Pagination from '@/components/ui/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import FilterPanel from '@/features/recipes/components/FilterPanel';
import { getCategories } from '@/features/categories/api';
import { searchRecipes } from '@/features/recipes/api';
import { splitSearchTerms } from '@/features/recipes/highlight';
import {
  buildHref,
  countActiveFilters,
  DEFAULT_PAGE_SIZE,
  MIN_SEARCH_LENGTH,
  parseSearchState,
  type RawSearchParams,
} from '@/features/recipes/search-params';
import type { Category } from '@/types/api';

// SRS §5.1: /search render SSR
export const dynamic = 'force-dynamic';

type SearchParams = Promise<RawSearchParams>;

const PATHNAME = '/search';

export async function generateMetadata({
  searchParams,
}: {
  searchParams: SearchParams;
}): Promise<Metadata> {
  const { q } = parseSearchState(await searchParams);
  return { title: q ? `Tìm kiếm: ${q}` : 'Tìm kiếm công thức' };
}

export default async function SearchPage({ searchParams }: { searchParams: SearchParams }) {
  const state = parseSearchState(await searchParams);
  const q = state.q ?? '';
  const canSearch = q.length >= MIN_SEARCH_LENGTH;

  const [result, categories] = await Promise.all([
    // FR-SRCH-001: q < 2 ký tự là 400 ở API nên không gọi.
    canSearch
      ? searchRecipes({
          q,
          page: state.page,
          pageSize: DEFAULT_PAGE_SIZE,
          categoryId: state.categoryId,
          difficulty: state.difficulty,
        })
      : null,
    getCategories().catch((): Category[] => []),
  ]);

  const terms = splitSearchTerms(q);
  const filtered = countActiveFilters(state) > 0;

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <h1 className="text-3xl font-bold text-gray-900">Tìm kiếm công thức</h1>

      {/* Form GET thuần: dùng được cả khi chưa tải JS và trên mobile (SearchBar trên header chỉ hiện từ md). */}
      <form action={PATHNAME} method="get" role="search" className="mt-4 flex max-w-xl gap-2">
        <input
          id="search-page-q"
          aria-label="Từ khóa tìm kiếm"
          type="search"
          name="q"
          defaultValue={q}
          placeholder="Ví dụ: pho bo, gà nướng, chè…"
          autoComplete="off"
          className="min-w-0 flex-1 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300"
        />
        <button
          type="submit"
          className="flex items-center gap-1.5 rounded-lg bg-orange-600 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-700"
        >
          <Search aria-hidden className="h-4 w-4" />
          Tìm
        </button>
      </form>

      {!canSearch || result === null ? (
        <p className="mt-10 text-gray-600">
          {q.length === 0
            ? 'Nhập từ khóa để tìm công thức. Có thể gõ không dấu, ví dụ “pho” vẫn ra “Phở”.'
            : `Từ khóa cần ít nhất ${MIN_SEARCH_LENGTH} ký tự.`}
        </p>
      ) : (
        <div className="mt-8 flex flex-col gap-6 lg:grid lg:grid-cols-[16rem_1fr] lg:items-start">
          <FilterPanel
            pathname={PATHNAME}
            state={state}
            categories={categories}
            fields={['category', 'difficulty']}
          />

          <div>
            <p aria-live="polite" className="text-gray-600">
              {result.totalCount > 0
                ? `Tìm thấy ${result.totalCount} công thức cho “${q}”.`
                : `Không tìm thấy công thức nào cho “${q}”.`}
            </p>

            {result.items.length === 0 ? (
              // FR-SRCH-001 A2: 200 với items rỗng + gợi ý.
              <div className="mt-6 rounded-2xl border border-orange-100 bg-white p-6 text-gray-700">
                <p className="font-medium text-gray-900">Bạn thử cách sau nhé:</p>
                <ul className="mt-2 list-disc space-y-1 pl-5 text-sm">
                  <li>
                    Kiểm tra lại chính tả hoặc thử từ khóa ngắn, phổ biến hơn (ví dụ “gà”, “phở”).
                  </li>
                  <li>Tìm không dấu cũng được — “pho bo” và “phở bò” cho cùng kết quả.</li>
                  {filtered && (
                    <li>
                      <Link
                        href={buildHref(PATHNAME, state, {
                          categoryId: undefined,
                          difficulty: undefined,
                          page: 1,
                        })}
                        className="font-medium text-orange-700 hover:underline"
                      >
                        Bỏ bộ lọc
                      </Link>{' '}
                      để mở rộng kết quả.
                    </li>
                  )}
                  <li>
                    Xem{' '}
                    <Link href="/recipes" className="font-medium text-orange-700 hover:underline">
                      tất cả công thức
                    </Link>{' '}
                    hoặc duyệt theo{' '}
                    <Link
                      href="/categories"
                      className="font-medium text-orange-700 hover:underline"
                    >
                      danh mục
                    </Link>
                    .
                  </li>
                </ul>
              </div>
            ) : (
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
                {result.items.map((recipe) => (
                  <RecipeCard key={recipe.id} recipe={recipe} terms={terms} />
                ))}
              </div>
            )}

            <Pagination
              page={result.page}
              totalPages={result.totalPages}
              buildHref={(p) => buildHref(PATHNAME, state, { page: p })}
            />
          </div>
        </div>
      )}
    </div>
  );
}
