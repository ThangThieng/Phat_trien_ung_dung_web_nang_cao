'use client';

import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type FormEvent,
  type MouseEvent,
} from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import clsx from 'clsx';
import { Filter, Search, SearchX } from 'lucide-react';
import type { Category } from '@/types/category';
import { MIN_SEARCH_LENGTH } from '@/types/search';
import EmptyState from '@/components/shared/EmptyState';
import ErrorFallback from '@/components/shared/ErrorFallback';
import Pagination from '@/components/shared/Pagination';
import { Badge } from '@/components/ui/badge';
import {
  Sheet,
  SheetContent,
  SheetFooter,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import { useMediaQuery } from '@/hooks/use-media-query';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { searchRecipes, toRecipeSearchParams } from '@/features/recipes/api';
import { splitSearchTerms } from '@/features/recipes/highlight';
import {
  buildHref,
  countActiveFilters,
  fromUrlSearchParams,
  parseSearchState,
} from '@/features/recipes/search-params';
import {
  CLEAR_ALL_FILTERS,
  formatResultRange,
  getActiveFilters,
  popularCategories,
} from '../filters';
import { SEARCH_PATHNAME, SEARCH_STALE_TIME_MS, searchKeys } from '../query-keys';
import ActiveFilterChips from './ActiveFilterChips';
import SearchFilterPanel from './SearchFilterPanel';

const PAGINATION_SELECTOR = 'nav[aria-label="Phân trang"]';

function ResultsSkeleton() {
  return (
    <div
      role="status"
      aria-busy="true"
      className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3"
    >
      <span className="sr-only">Đang tìm công thức…</span>
      {Array.from({ length: 6 }, (_, i) => (
        <div key={i} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
          <div className="aspect-[4/3] w-full animate-pulse bg-gray-200" />
          <div className="space-y-2 p-4">
            <div className="h-5 w-3/4 animate-pulse rounded bg-gray-200" />
            <div className="h-4 w-full animate-pulse rounded bg-gray-100" />
            <div className="h-4 w-2/3 animate-pulse rounded bg-gray-100" />
          </div>
        </div>
      ))}
    </div>
  );
}

/**
 * Trang `/search` phía client (FR-SRCH-001/002/004 — Lab 7). Lần tải đầu, Server Component đã prefetch đúng truy vấn này và
 * gửi kèm HTML qua `HydrationBoundary`, nên `useQuery` có dữ liệu ngay, không gọi API lần hai. Từ đó:
 * - **URL là nguồn sự thật duy nhất**: trạng thái đọc từ `useSearchParams()`; đổi bộ lọc/trang/từ khóa = `history.pushState`.
 *   Next.js đồng bộ `pushState` với `useSearchParams` mà KHÔNG render lại Server Component → mỗi lần đổi đúng một request
 *   JSON `GET /api/v1/recipes/search`, không tải RSC payload của cả trang. F5, chia sẻ link, back/forward đều đúng.
 * - `placeholderData: keepPreviousData` → trong lúc chờ, kết quả cũ hiện mờ thay vì trang trắng.
 * - `staleTime` 60 giây = TTL `search:` ở Redis; back/forward trong khoảng đó lấy ngay từ cache.
 * - Mọi lựa chọn bộ lọc, nhãn gỡ bộ lọc, link phân trang đều là link thật (chạy khi chưa có JavaScript, crawler đi theo
 *   được); có JavaScript thì một bộ xử lý duy nhất ở gốc component chặn cú bấm và đổi URL tại chỗ.
 */
export default function SearchResults({ categories }: { categories: Category[] }) {
  const searchParams = useSearchParams();
  const state = useMemo(() => parseSearchState(fromUrlSearchParams(searchParams)), [searchParams]);
  const q = state.q ?? '';
  const canSearch = q.length >= MIN_SEARCH_LENGTH;
  const params = useMemo(() => toRecipeSearchParams(state), [state]);

  const { data, isError, error, isPlaceholderData, refetch } = useQuery({
    queryKey: searchKeys.results(params),
    queryFn: ({ signal }) => searchRecipes(params, signal),
    enabled: canSearch,
    placeholderData: keepPreviousData,
    staleTime: SEARCH_STALE_TIME_MS,
    // Lỗi hiển thị ngay trong khung kết quả (ErrorFallback) — không toast thêm lần nữa.
    meta: { suppressGlobalError: true },
  });

  const [sheetOpen, setSheetOpen] = useState(false);
  const isDesktop = useMediaQuery('(min-width: 1024px)');
  const resultsRef = useRef<HTMLElement>(null);

  // Phóng to cửa sổ tới `lg` khi drawer đang mở: bộ lọc thành cột cố định, đóng drawer để không kẹt trạng thái mở.
  useEffect(() => {
    if (isDesktop) setSheetOpen(false);
  }, [isDesktop]);

  // Server chỉ đặt <title> ở lần tải đầu; sau pushState thì cập nhật ở đây (cùng mẫu '%s | Culinary Blog' của root layout).
  useEffect(() => {
    document.title = `${q ? `Tìm kiếm: ${q}` : 'Tìm kiếm công thức'} | Culinary Blog`;
  }, [q]);

  const navigate = useCallback((href: string, scrollToResults = false) => {
    if (href !== `${window.location.pathname}${window.location.search}`) {
      window.history.pushState(null, '', href);
    }
    const top = resultsRef.current?.getBoundingClientRect().top;
    if (scrollToResults && top !== undefined && top < 0) {
      resultsRef.current?.scrollIntoView({ block: 'start' });
    }
  }, []);

  /** Một chỗ chặn mọi link `/search` bên trong (kể cả link `next/link` của `Pagination` và trong drawer qua portal). */
  const handleClickCapture = (event: MouseEvent<HTMLDivElement>) => {
    if (
      event.defaultPrevented ||
      event.button !== 0 ||
      event.metaKey ||
      event.ctrlKey ||
      event.shiftKey ||
      event.altKey
    ) {
      return;
    }
    const anchor = (event.target as Element).closest?.('a[href]');
    if (!(anchor instanceof HTMLAnchorElement)) return;
    if ((anchor.target && anchor.target !== '_self') || anchor.hasAttribute('download')) return;
    const url = new URL(anchor.href, window.location.href);
    if (url.origin !== window.location.origin || url.pathname !== SEARCH_PATHNAME) return;
    event.preventDefault();
    navigate(`${url.pathname}${url.search}`, anchor.closest(PAGINATION_SELECTOR) !== null);
  };

  const handleSearchSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const input = event.currentTarget.elements.namedItem('q');
    const term = input instanceof HTMLInputElement ? input.value.trim() : '';
    navigate(buildHref(SEARCH_PATHNAME, state, { q: term || undefined, page: 1 }));
  };

  const terms = useMemo(() => splitSearchTerms(q), [q]);
  const activeCount = countActiveFilters(state);
  const activeFilters = getActiveFilters(state, categories);

  const filterPanel = (
    <SearchFilterPanel state={state} categories={categories} onNavigate={navigate} />
  );

  const renderResults = () => {
    if (!data) {
      return isError ? (
        <div className="mt-6">
          <ErrorFallback
            variant="inline"
            title="Không tải được kết quả tìm kiếm"
            error={error}
            reset={() => {
              refetch().catch(() => undefined);
            }}
          />
        </div>
      ) : (
        <ResultsSkeleton />
      );
    }

    if (data.items.length === 0 && data.totalCount > 0) {
      // Trang vượt quá số trang (URL gõ tay hoặc link cũ sau khi dữ liệu đổi).
      return (
        <EmptyState
          className="mt-6"
          icon={SearchX}
          title={`Trang ${data.page} không có kết quả`}
          description={`Kết quả cho “${q}” chỉ có ${data.totalPages} trang.`}
          action={
            <a
              href={buildHref(SEARCH_PATHNAME, state, { page: data.totalPages })}
              className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
            >
              Về trang {data.totalPages}
            </a>
          }
        />
      );
    }

    if (data.items.length === 0) {
      // FR-SRCH-001 A2: 200 với items rỗng → gợi ý gỡ bớt bộ lọc và vài danh mục phổ biến.
      const suggested = popularCategories(categories);
      return (
        <EmptyState
          className="mt-6"
          icon={SearchX}
          title="Không có kết quả phù hợp"
          description={
            <div className="text-left">
              {activeFilters.length > 0 ? (
                <>
                  <p>Thử gỡ bớt bộ lọc đang bật:</p>
                  <ul className="mt-2 flex flex-wrap gap-2">
                    {activeFilters.map(({ key, label }) => (
                      <li key={key}>
                        <a
                          href={buildHref(SEARCH_PATHNAME, state, { [key]: undefined, page: 1 })}
                          className="inline-flex rounded-full border border-orange-200 bg-orange-50 px-3 py-1 text-orange-900 hover:border-orange-400"
                        >
                          Bỏ “{label}”
                        </a>
                      </li>
                    ))}
                  </ul>
                </>
              ) : (
                <p>
                  Kiểm tra lại chính tả hoặc thử từ khóa ngắn, phổ biến hơn. Gõ không dấu cũng được
                  — “pho bo” và “phở bò” cho cùng kết quả.
                </p>
              )}
              {suggested.length > 0 && (
                <>
                  <p className="mt-4">Hoặc xem các danh mục nhiều công thức:</p>
                  <ul className="mt-2 flex flex-wrap gap-2">
                    {suggested.map((category) => (
                      <li key={category.id}>
                        <Link
                          href={`/categories/${category.slug}`}
                          prefetch={false}
                          className="inline-flex items-center gap-1.5 rounded-full border border-gray-300 px-3 py-1 text-gray-700 hover:border-orange-400"
                        >
                          {category.name}
                          <Badge variant="secondary">{category.recipeCount}</Badge>
                        </Link>
                      </li>
                    ))}
                  </ul>
                </>
              )}
            </div>
          }
          action={
            activeFilters.length > 0 ? (
              <a
                href={buildHref(SEARCH_PATHNAME, state, CLEAR_ALL_FILTERS)}
                className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
              >
                Xóa tất cả bộ lọc
              </a>
            ) : undefined
          }
        />
      );
    }

    return (
      <div
        aria-busy={isPlaceholderData}
        className={clsx(
          'mt-6 grid grid-cols-1 gap-6 transition-opacity sm:grid-cols-2 xl:grid-cols-3',
          isPlaceholderData && 'opacity-60',
        )}
      >
        {data.items.map((recipe) => (
          <RecipeCard key={recipe.id} recipe={recipe} terms={terms} prefetch={false} />
        ))}
      </div>
    );
  };

  return (
    // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions -- chỉ chặn cú bấm của các link con (Enter trên link cũng phát sự kiện click), không phải phần tử tương tác.
    <div className="mx-auto max-w-6xl px-4 py-10" onClickCapture={handleClickCapture}>
      {/* Sheet (Radix Dialog.Root) không dựng phần tử DOM nào — bọc cả trang để nút "Bộ lọc" làm SheetTrigger được. */}
      <Sheet open={sheetOpen} onOpenChange={setSheetOpen}>
        <h1 className="text-3xl font-bold text-gray-900">Tìm kiếm công thức</h1>

        {/* Form GET: chạy cả khi chưa có JavaScript và trên điện thoại (SearchBar ở header chỉ hiện từ md). */}
        <form
          key={q}
          action={SEARCH_PATHNAME}
          method="get"
          role="search"
          onSubmit={handleSearchSubmit}
          className="mt-4 flex max-w-xl gap-2"
        >
          <input
            aria-label="Từ khóa tìm kiếm"
            type="search"
            name="q"
            defaultValue={q}
            placeholder="Ví dụ: pho bo, gà nướng, chè…"
            autoComplete="off"
            className="min-w-0 flex-1 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300"
          />
          {state.categoryId && <input type="hidden" name="categoryId" value={state.categoryId} />}
          {state.difficulty && <input type="hidden" name="difficulty" value={state.difficulty} />}
          {state.maxCookTime !== undefined && (
            <input type="hidden" name="maxCookTime" value={state.maxCookTime} />
          )}
          {state.maxPrepTime !== undefined && (
            <input type="hidden" name="maxPrepTime" value={state.maxPrepTime} />
          )}
          {state.minServings !== undefined && (
            <input type="hidden" name="minServings" value={state.minServings} />
          )}
          <button
            type="submit"
            className="flex items-center gap-1.5 rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
          >
            <Search aria-hidden className="h-4 w-4" />
            Tìm
          </button>
        </form>

        {!canSearch ? (
          <p className="mt-10 text-gray-600">
            {q.length === 0
              ? 'Nhập từ khóa để tìm công thức. Có thể gõ không dấu, ví dụ “pho” vẫn ra “Phở”.'
              : `Từ khóa cần ít nhất ${MIN_SEARCH_LENGTH} ký tự.`}
          </p>
        ) : (
          <div className="mt-8 flex flex-col gap-6 lg:grid lg:grid-cols-[17rem_1fr] lg:items-start lg:gap-8">
            <aside
              aria-label="Bộ lọc công thức"
              className="hidden rounded-2xl border border-gray-200 bg-white p-4 lg:block"
            >
              <h2 className="mb-4 text-base font-semibold text-gray-900">Bộ lọc</h2>
              {filterPanel}
            </aside>

            <section ref={resultsRef} aria-label="Kết quả tìm kiếm" className="scroll-mt-20">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div aria-live="polite">
                  <p className="text-gray-800">
                    {!data && 'Đang tìm…'}
                    {data &&
                      (data.totalCount > 0
                        ? `Tìm thấy ${data.totalCount} công thức cho “${q}”.`
                        : `Không tìm thấy công thức nào cho “${q}”.`)}
                  </p>
                  {data && data.items.length > 0 && (
                    <p className="mt-1 text-sm text-gray-600">
                      {formatResultRange(
                        data.page,
                        data.pageSize,
                        data.items.length,
                        data.totalCount,
                      )}
                    </p>
                  )}
                </div>
                {/* SheetTrigger: Radix trả focus về đúng nút này khi đóng Sheet (Escape, bấm nền, nút đóng). */}
                <SheetTrigger asChild>
                  <button
                    type="button"
                    className="flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:border-orange-400 lg:hidden"
                  >
                    <Filter aria-hidden className="h-4 w-4" />
                    Bộ lọc
                    {activeCount > 0 && (
                      <Badge aria-label={`${activeCount} bộ lọc đang bật`}>{activeCount}</Badge>
                    )}
                  </button>
                </SheetTrigger>
              </div>

              <ActiveFilterChips state={state} categories={categories} />

              {renderResults()}

              {data && data.items.length > 0 && (
                <Pagination
                  page={data.page}
                  totalPages={data.totalPages}
                  hasPreviousPage={data.hasPreviousPage}
                  hasNextPage={data.hasNextPage}
                  buildHref={(page) => buildHref(SEARCH_PATHNAME, state, { page })}
                />
              )}
            </section>
          </div>
        )}

        <SheetContent side="left" aria-describedby={undefined} className="overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Bộ lọc</SheetTitle>
          </SheetHeader>
          <div className="px-4">{filterPanel}</div>
          <SheetFooter className="border-t border-gray-200">
            <button
              type="button"
              onClick={() => setSheetOpen(false)}
              className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
            >
              {data ? `Xem ${data.totalCount} kết quả` : 'Đóng'}
            </button>
          </SheetFooter>
        </SheetContent>
      </Sheet>
    </div>
  );
}
