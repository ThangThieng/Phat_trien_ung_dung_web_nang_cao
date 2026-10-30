import type { Metadata } from 'next';
import Link from 'next/link';
import { BookOpen, Search } from 'lucide-react';
import JsonLd from '@/components/seo/JsonLd';
import CategoryChips from '@/components/layout/CategoryChips';
import SidebarCategory from '@/components/layout/SidebarCategory';
import EmptyState from '@/components/shared/EmptyState';
import { getCategories } from '@/features/categories/api';
import { getLatestRecipes } from '@/features/recipes/api';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { buildWebSiteJsonLd } from '@/lib/seo/recipe-json-ld';
import type { Category } from '@/types/category';
import { MIN_SEARCH_LENGTH } from '@/types/search';

export const metadata: Metadata = {
  title: { absolute: 'Culinary Blog – Công thức nấu ăn từ bếp nhà Việt' },
  description:
    'Khám phá công thức nấu ăn mới xuất bản mỗi ngày: đủ nguyên liệu, định lượng và từng bước, tìm không dấu cũng ra.',
  alternates: { canonical: '/' },
};

/*
 * SRS §5.1 `/`: SSR + Data Cache — mỗi `fetch` mang `revalidate` của chính dữ liệu đó (MT-67, MT-33.3):
 *  - công thức mới xuất bản: 120 s  (TTL `recipes:list` 2 phút)  → `getLatestRecipes()`
 *  - danh mục:               1800 s (TTL `categories:all` 30 phút) → `getCategories()`
 * Không khai báo `export const revalidate` ở trang (CSP nonce buộc render theo request); `api.test.ts` của hai feature
 * khẳng định các con số này. `force-dynamic`: `next build` của image Docker chạy khi API chưa có, không prerender được.
 */

export const dynamic = 'force-dynamic';

export default async function HomePage() {
  // Hai nguồn độc lập → gọi song song. Danh mục chỉ là phần phụ (sidebar): lỗi thì trang vẫn hiện công thức.
  const [latest, categories] = await Promise.all([
    getLatestRecipes(),
    getCategories().catch((): Category[] => []),
  ]);
  const recipes = latest.items;

  return (
    <>
      <JsonLd data={buildWebSiteJsonLd()} />
      <section className="bg-gradient-to-br from-orange-100 via-amber-50 to-white">
        <div className="mx-auto max-w-6xl px-4 py-12 sm:py-16">
          <h1 className="max-w-2xl text-3xl font-bold text-gray-900 sm:text-4xl">
            Nấu ngon mỗi ngày với công thức từ bếp nhà Việt
          </h1>
          <p className="mt-3 max-w-xl text-gray-700">
            Đủ nguyên liệu, định lượng và từng bước — tìm không dấu cũng ra.
          </p>
          {/* Form GET thuần tới /search: chạy cả khi JavaScript chưa tải xong. `minLength` khớp ngưỡng 2 ký tự của API. */}
          <form action="/search" method="get" role="search" className="mt-6 flex max-w-xl gap-2">
            <label htmlFor="home-search" className="min-w-0 flex-1">
              <span className="sr-only">Tìm công thức</span>
              <input
                id="home-search"
                name="q"
                type="search"
                required
                minLength={MIN_SEARCH_LENGTH}
                autoComplete="off"
                placeholder="Tìm món ăn, nguyên liệu…"
                className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-gray-900 placeholder:text-gray-500 focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300"
              />
            </label>
            <button
              type="submit"
              className="flex shrink-0 items-center gap-2 rounded-lg bg-orange-700 px-4 py-2.5 font-semibold text-white hover:bg-orange-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-700"
            >
              <Search aria-hidden className="h-4 w-4" />
              Tìm
            </button>
          </form>
        </div>
      </section>

      <div className="mx-auto max-w-6xl px-4 py-10">
        {/* Dưới 1200 px: hàng nhãn cuộn ngang thay cho cột phụ. */}
        <CategoryChips categories={categories} className="mb-8 min-[1200px]:hidden" />

        <div
          className={`grid gap-8 ${categories.length > 0 ? 'min-[1200px]:grid-cols-[minmax(0,1fr)_16rem]' : ''}`}
        >
          <section aria-labelledby="latest-heading" className="min-w-0">
            <div className="flex items-baseline justify-between gap-4">
              <h2 id="latest-heading" className="text-2xl font-bold text-gray-900">
                Công thức mới xuất bản
              </h2>
              <Link
                href="/recipes"
                className="shrink-0 text-sm font-medium text-orange-700 hover:text-orange-900 hover:underline"
              >
                Xem tất cả
                <span className="sr-only"> công thức</span>
              </Link>
            </div>

            {recipes.length === 0 ? (
              <EmptyState
                className="mt-6"
                icon={BookOpen}
                title="Chưa có công thức nào được xuất bản"
                description="Hãy quay lại sau, hoặc xem các danh mục để biết nội dung sắp có."
                action={
                  <Link
                    href="/categories"
                    className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
                  >
                    Xem danh mục
                  </Link>
                }
              />
            ) : (
              <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
                {recipes.map((recipe, index) => (
                  // Ảnh thẻ đầu tiên tải trước (LCP); các thẻ khác lazy mặc định.
                  <RecipeCard key={recipe.id} recipe={recipe} priority={index === 0} />
                ))}
              </div>
            )}
          </section>

          {categories.length > 0 && (
            <SidebarCategory
              categories={categories}
              className="hidden h-fit min-[1200px]:sticky min-[1200px]:top-20 min-[1200px]:block"
            />
          )}
        </div>
      </div>
    </>
  );
}
