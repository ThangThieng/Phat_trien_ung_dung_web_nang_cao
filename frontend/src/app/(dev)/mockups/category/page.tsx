import SidebarCategory from '@/components/layout/SidebarCategory';
import Pagination from '@/components/shared/Pagination';
import EmptyState from '@/components/shared/EmptyState';
import LoadingSkeleton from '@/components/shared/LoadingSkeleton';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { mockCategories } from '@/mocks/fixtures/categories';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import MockupShell from '../MockupShell';
import { assertDevOnly } from '../dev-only';

const category = mockCategories[0];

/** Bố cục trang danh mục: sidebar có mục đang xem, lưới, phân trang nhiều trang (thấy dấu `…`), trạng thái rỗng và đang tải. */
export default function CategoryMockup() {
  assertDevOnly();
  // Công thức của danh mục đứng trước, rồi tới các công thức mẫu còn lại — đủ để thấy lưới nhiều hàng.
  const grid = [
    ...mockRecipeSummaries.filter((r) => r.category.slug === category.slug),
    ...mockRecipeSummaries.filter((r) => r.category.slug !== category.slug),
  ];
  return (
    <MockupShell>
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 lg:grid-cols-[16rem_1fr]">
        <SidebarCategory
          categories={mockCategories}
          activeSlug={category.slug}
          className="h-fit lg:sticky lg:top-20"
        />
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{category.name}</h1>
          <p className="mt-2 text-gray-700">{category.description}</p>
          <p className="mt-3 text-sm font-medium text-orange-800">
            {category.recipeCount} công thức
          </p>

          <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-3">
            {grid.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
          <Pagination page={6} totalPages={12} buildHref={(p) => `/mockups/category?page=${p}`} />

          <h2 className="mt-16 text-lg font-semibold text-gray-900">Trạng thái: danh mục rỗng</h2>
          <EmptyState
            className="mt-4"
            title="Danh mục này chưa có công thức nào"
            description="Hãy quay lại sau hoặc khám phá các danh mục khác."
            headingLevel="h3"
          />
        </div>
      </div>
      <section aria-label="Trạng thái đang tải" className="border-t border-dashed border-amber-300">
        <LoadingSkeleton variant="recipe-grid" count={3} label="Ví dụ khung chờ lưới công thức" />
      </section>
    </MockupShell>
  );
}
