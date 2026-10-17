import Link from 'next/link';
import SidebarCategory from '@/components/layout/SidebarCategory';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { mockCategories } from '@/mocks/fixtures/categories';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import MockupShell from '../MockupShell';
import { assertDevOnly } from '../dev-only';

/** Bố cục trang chủ (Buổi 7 — Dev 3 nối `GET /recipes?sortBy=publishedAt&sortOrder=desc&pageSize=8`). */
export default function HomeMockup() {
  assertDevOnly();
  return (
    <MockupShell>
      <section className="bg-gradient-to-br from-orange-100 via-amber-50 to-white">
        <div className="mx-auto max-w-6xl px-4 py-12 sm:py-16">
          <h1 className="max-w-2xl text-3xl font-bold text-gray-900 sm:text-4xl">
            Nấu ngon mỗi ngày với công thức từ bếp nhà Việt
          </h1>
          <p className="mt-3 max-w-xl text-gray-700">
            Đủ nguyên liệu, định lượng và từng bước — tìm không dấu cũng ra.
          </p>
          <Link
            href="/mockups/search"
            className="mt-6 inline-block rounded-lg bg-orange-700 px-5 py-2.5 font-semibold text-white hover:bg-orange-800"
          >
            Tìm công thức
          </Link>
        </div>
      </section>

      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 lg:grid-cols-[1fr_16rem]">
        <section aria-labelledby="latest-heading">
          <h2 id="latest-heading" className="text-2xl font-bold text-gray-900">
            Công thức mới xuất bản
          </h2>
          <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
            {mockRecipeSummaries.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
        </section>
        <SidebarCategory categories={mockCategories} className="h-fit lg:sticky lg:top-20" />
      </div>
    </MockupShell>
  );
}
