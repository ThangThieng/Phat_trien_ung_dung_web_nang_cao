import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { cache } from 'react';
import { BookOpen } from 'lucide-react';
import JsonLd from '@/components/seo/JsonLd';
import CategoryChips from '@/components/layout/CategoryChips';
import SidebarCategory from '@/components/layout/SidebarCategory';
import EmptyState from '@/components/shared/EmptyState';
import Pagination from '@/components/shared/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import SortSelect from '@/features/recipes/components/SortSelect';
import { getCategories, getCategoryBySlug } from '@/features/categories/api';
import { buildHref, parseCategoryState } from '@/features/recipes/search-params';
import type { Category } from '@/types/category';
import type { RecipeQueryState } from '@/types/search';
import { ApiError } from '@/lib/api-client';
import { buildCategoryMetadata } from '@/lib/seo/metadata';
import { buildBreadcrumbJsonLd } from '@/lib/seo/recipe-json-ld';

type Params = Promise<{ slug: string }>;
type SearchParams = Promise<Record<string, string | string[] | undefined>>;

// SRS §5.1 / MT-57: /categories/[slug] – SSR + Data Cache `revalidate: 120` (xem features/categories/api.ts)
export const dynamic = 'force-dynamic';

const loadCategory = cache(async (slug: string, state: string) => {
  const { page, sortBy, sortOrder } = JSON.parse(state) as RecipeQueryState;
  try {
    return await getCategoryBySlug(slug, { page, sortBy, sortOrder });
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
});

// `cache()` của React so khớp đối số bằng ===, nên truyền trạng thái dạng chuỗi để generateMetadata và trang dùng chung 1 lần gọi API.
const stateKey = (raw: Record<string, string | string[] | undefined>) =>
  JSON.stringify(parseCategoryState(raw));

export async function generateMetadata({
  params,
  searchParams,
}: {
  params: Params;
  searchParams: SearchParams;
}): Promise<Metadata> {
  const detail = await loadCategory((await params).slug, stateKey(await searchParams));
  if (!detail) return { title: 'Không tìm thấy danh mục', robots: { index: false } };
  return buildCategoryMetadata(detail.category);
}

export default async function CategoryDetailPage({
  params,
  searchParams,
}: {
  params: Params;
  searchParams: SearchParams;
}) {
  const { slug } = await params;
  const rawParams = await searchParams;
  const state = parseCategoryState(rawParams);
  // Danh mục chi tiết (120 s) và danh sách danh mục cho sidebar (1800 s, dùng chung Data Cache với header/footer) gọi song song.
  // Danh sách danh mục chỉ là phần phụ: lỗi thì bỏ sidebar chứ không làm hỏng trang.
  const [detail, categories] = await Promise.all([
    loadCategory(slug, stateKey(rawParams)),
    getCategories().catch((): Category[] => []),
  ]);
  if (!detail) notFound();

  const { category, recipes } = detail;
  const pathname = `/categories/${category.slug}`;
  const hasSidebar = categories.length > 0;
  // Trang vượt quá tổng số trang (URL gõ tay / danh mục vừa bớt công thức): khác với danh mục chưa có công thức nào.
  const pastLastPage = recipes.items.length === 0 && recipes.page > 1;

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <JsonLd
        data={buildBreadcrumbJsonLd([
          { name: 'Trang chủ', path: '/' },
          { name: 'Danh mục', path: '/categories' },
          { name: category.name, path: pathname },
        ])}
      />
      <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
        <Link href="/categories" className="hover:text-orange-700">
          Danh mục
        </Link>
        <span aria-hidden> / </span>
        <span className="text-gray-700">{category.name}</span>
      </nav>

      <div
        className={`mt-4 grid gap-8 ${hasSidebar ? 'min-[1200px]:grid-cols-[16rem_minmax(0,1fr)]' : ''}`}
      >
        {hasSidebar && (
          <SidebarCategory
            categories={categories}
            activeSlug={category.slug}
            className="hidden h-fit min-[1200px]:sticky min-[1200px]:top-20 min-[1200px]:block"
          />
        )}

        <div className="min-w-0">
          {/* Dưới 1200 px: hàng nhãn cuộn ngang thay cho cột phụ. */}
          <CategoryChips
            categories={categories}
            activeSlug={category.slug}
            className="mb-6 min-[1200px]:hidden"
          />

          <header className="rounded-2xl bg-gradient-to-r from-orange-100 to-amber-50 p-8">
            <h1 className="text-3xl font-bold text-gray-900">{category.name}</h1>
            {category.description && (
              <p className="mt-2 max-w-2xl text-gray-700">{category.description}</p>
            )}
            <p className="mt-3 text-sm font-medium text-orange-800">
              {recipes.totalCount} công thức
            </p>
          </header>

          {/* FR-CAT-002 / SRS §8.2: danh mục chỉ nhận sortBy + sortOrder (không có bộ lọc khác). */}
          <div className="mt-6 flex justify-end">
            <SortSelect pathname={pathname} state={state} />
          </div>

          {recipes.items.length === 0 ? (
            <EmptyState
              className="mt-8"
              icon={BookOpen}
              title={
                pastLastPage
                  ? 'Trang này không có công thức nào'
                  : 'Danh mục này chưa có công thức nào'
              }
              description={
                pastLastPage
                  ? 'Có thể danh mục đã thay đổi kể từ lần bạn mở liên kết này.'
                  : 'Hãy quay lại sau hoặc khám phá các công thức khác.'
              }
              action={
                pastLastPage ? (
                  <Link
                    href={buildHref(pathname, state, { page: 1 })}
                    className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
                  >
                    Về trang đầu
                  </Link>
                ) : (
                  <>
                    <Link
                      href="/recipes"
                      className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
                    >
                      Xem tất cả công thức
                    </Link>
                    <Link
                      href="/categories"
                      className="rounded-lg border border-orange-300 px-4 py-2 text-sm font-semibold text-orange-800 hover:bg-orange-50"
                    >
                      Xem danh mục khác
                    </Link>
                  </>
                )
              }
            />
          ) : (
            <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
              <h2 className="sr-only">Danh sách công thức</h2>
              {recipes.items.map((recipe, index) => (
                <RecipeCard key={recipe.id} recipe={recipe} priority={index === 0} />
              ))}
            </div>
          )}

          <Pagination
            page={recipes.page}
            totalPages={recipes.totalPages}
            hasPreviousPage={recipes.hasPreviousPage}
            hasNextPage={recipes.hasNextPage}
            buildHref={(p) => buildHref(pathname, state, { page: p })}
          />
        </div>
      </div>
    </div>
  );
}
