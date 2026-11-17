import Link from 'next/link';
import { SearchX } from 'lucide-react';
import EmptyState from '@/components/shared/EmptyState';
import ErrorFallback from '@/components/shared/ErrorFallback';
import Pagination from '@/components/shared/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import MockupShell from '../MockupShell';
import { assertDevOnly } from '../dev-only';

/** Bố cục trang tìm kiếm (giao diện bộ lọc thời gian MT-66 là việc của Dev 4 ở Buổi 7) + trạng thái rỗng và lỗi. */
export default function SearchMockup() {
  assertDevOnly();
  return (
    <MockupShell>
      <div className="mx-auto max-w-6xl px-4 py-10">
        <h1 className="text-3xl font-bold text-gray-900">Tìm kiếm công thức</h1>
        <p className="mt-4 text-gray-700">Tìm thấy 23 công thức cho “pho”.</p>
        <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {mockRecipeSummaries.slice(0, 3).map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} terms={['pho']} />
          ))}
        </div>
        <Pagination page={1} totalPages={2} buildHref={(p) => `/mockups/search?page=${p}`} />

        <div className="mt-16 grid gap-8 lg:grid-cols-2">
          <section aria-labelledby="empty-heading">
            <h2 id="empty-heading" className="text-lg font-semibold text-gray-900">
              Trạng thái: không có kết quả
            </h2>
            <EmptyState
              className="mt-4"
              icon={SearchX}
              headingLevel="h3"
              title="Không có kết quả phù hợp"
              description="Thử từ khóa ngắn hơn, hoặc bỏ bộ lọc thời gian."
              action={
                <Link
                  href="/mockups/search"
                  className="font-medium text-orange-700 hover:underline"
                >
                  Bỏ bộ lọc
                </Link>
              }
            />
          </section>
          <section aria-labelledby="error-heading">
            <h2 id="error-heading" className="text-lg font-semibold text-gray-900">
              Trạng thái: lỗi máy chủ
            </h2>
            <div className="mt-4">
              <ErrorFallback
                variant="inline"
                title="Không tải được kết quả"
                error={{ name: 'Error', message: 'mock', digest: 'mock-4f2a9c' }}
              />
            </div>
          </section>
        </div>
      </div>
    </MockupShell>
  );
}
