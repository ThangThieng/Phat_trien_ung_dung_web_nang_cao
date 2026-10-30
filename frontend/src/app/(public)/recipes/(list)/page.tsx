import type { Metadata } from 'next';
import Link from 'next/link';
import { BookOpen, SearchX } from 'lucide-react';
import EmptyState from '@/components/shared/EmptyState';
import Pagination from '@/components/shared/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import FilterPanel from '@/features/recipes/components/FilterPanel';
import SortSelect from '@/features/recipes/components/SortSelect';
import { getCategories } from '@/features/categories/api';
import { getRecipes, toRecipeListParams } from '@/features/recipes/api';
import {
  buildHref,
  countActiveFilters,
  parseRecipeListState,
} from '@/features/recipes/search-params';
import type { Category } from '@/types/category';

export const metadata: Metadata = {
  title: 'Công thức nấu ăn',
  description:
    'Khám phá hàng trăm công thức nấu ăn Việt Nam và quốc tế – từ món khai vị, món chính đến tráng miệng.',
};

// SRS §5.1: /recipes render SSR dynamic
export const dynamic = 'force-dynamic';

type SearchParams = Promise<Record<string, string | string[] | undefined>>;

const PATHNAME = '/recipes';

export default async function RecipesPage({ searchParams }: { searchParams: SearchParams }) {
  // FR-SRCH-002/003/004: toàn bộ state nằm trong URL — SSR đọc được ngay, link chia sẻ được, back/forward đúng.
  const state = parseRecipeListState(await searchParams);

  const [result, categories] = await Promise.all([
    getRecipes(toRecipeListParams(state)),
    // Lỗi tải danh mục chỉ làm ô "Danh mục" trong bộ lọc trống, không làm hỏng cả trang.
    getCategories().catch((): Category[] => []),
  ]);

  const filtered = countActiveFilters(state) > 0;

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Công thức nấu ăn</h1>
          <p className="mt-1 text-gray-600">
            {filtered
              ? `${result.totalCount} công thức phù hợp bộ lọc.`
              : `${result.totalCount} công thức đang chờ bạn khám phá.`}
          </p>
        </div>
        <SortSelect pathname={PATHNAME} state={state} />
      </div>

      <div className="mt-8 flex flex-col gap-6 lg:grid lg:grid-cols-[16rem_1fr] lg:items-start">
        <FilterPanel
          pathname={PATHNAME}
          state={state}
          categories={categories}
          fields={['category', 'difficulty', 'cookTime', 'prepTime', 'servings']}
        />

        <div>
          {result.items.length === 0 ? (
            <EmptyState
              icon={filtered ? SearchX : BookOpen}
              title={filtered ? 'Không có công thức nào phù hợp bộ lọc' : 'Chưa có công thức nào'}
              description={
                filtered
                  ? 'Thử nới bộ lọc thời gian, khẩu phần hoặc chọn danh mục khác.'
                  : undefined
              }
              action={
                filtered && (
                  <Link
                    href={buildHref(PATHNAME, state, {
                      categoryId: undefined,
                      difficulty: undefined,
                      maxCookTime: undefined,
                      maxPrepTime: undefined,
                      minServings: undefined,
                      page: 1,
                    })}
                    className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
                  >
                    Xóa bộ lọc
                  </Link>
                )
              }
            />
          ) : (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
              {/* WCAG 1.3.1: tiêu đề thẻ là h3 nên cần một h2 phía trên — bảng lọc (h2) bị ẩn dưới 1024 px. */}
              <h2 className="sr-only">Danh sách công thức</h2>
              {result.items.map((recipe, index) => (
                <RecipeCard key={recipe.id} recipe={recipe} priority={index === 0} />
              ))}
            </div>
          )}

          <Pagination
            page={result.page}
            totalPages={result.totalPages}
            hasPreviousPage={result.hasPreviousPage}
            hasNextPage={result.hasNextPage}
            buildHref={(p) => buildHref(PATHNAME, state, { page: p })}
          />
        </div>
      </div>
    </div>
  );
}
