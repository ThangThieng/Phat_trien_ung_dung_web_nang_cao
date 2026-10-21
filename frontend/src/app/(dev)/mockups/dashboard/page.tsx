import Link from 'next/link';
import { BookOpen, ChefHat } from 'lucide-react';
import DashboardSidebar from '@/components/layout/DashboardSidebar';
import EmptyState from '@/components/shared/EmptyState';
import LoadingSkeleton from '@/components/shared/LoadingSkeleton';
import { formatDate } from '@/features/recipes/format';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import { MockupBar } from '../MockupShell';
import { assertDevOnly } from '../dev-only';

const STATUS_LABEL = { Draft: 'Bản nháp', Published: 'Đã xuất bản', Archived: 'Lưu trữ' } as const;

/**
 * Bố cục dashboard (khung sidebar + thanh trên + bảng "Công thức của tôi"). Không bọc `DashboardShell` vì nó chặn khi chưa
 * đăng nhập — mockup ráp lại khung bằng chính `DashboardSidebar` (vai trò Admin để thấy đủ mục).
 */
export default function DashboardMockup() {
  assertDevOnly();
  const rows = mockRecipeSummaries.map((recipe, i) => ({
    ...recipe,
    status: i === 1 ? ('Draft' as const) : recipe.status,
  }));
  return (
    <>
      <MockupBar />
      <div className="flex flex-1">
        <aside className="hidden w-60 shrink-0 border-r border-orange-100 bg-white md:block">
          <DashboardSidebar roles={['Author', 'Admin']} />
        </aside>
        <div className="flex min-w-0 flex-1 flex-col">
          <header className="flex h-14 items-center gap-3 border-b border-orange-100 bg-white px-4">
            <Link
              href="/mockups/home"
              className="flex items-center gap-2 font-bold text-orange-700"
            >
              <ChefHat aria-hidden className="h-5 w-5" />
              Culinary Blog
            </Link>
          </header>
          <main id="main-content" className="mx-auto w-full max-w-5xl px-4 py-8">
            <h1 className="text-2xl font-bold text-gray-900">Công thức của tôi</h1>
            <div className="mt-6 overflow-x-auto rounded-xl border border-gray-200 bg-white">
              <table className="w-full min-w-[36rem] text-left text-sm">
                <thead className="bg-gray-50 text-gray-700">
                  <tr>
                    <th scope="col" className="px-4 py-3 font-semibold">
                      Tên món
                    </th>
                    <th scope="col" className="px-4 py-3 font-semibold">
                      Danh mục
                    </th>
                    <th scope="col" className="px-4 py-3 font-semibold">
                      Trạng thái
                    </th>
                    <th scope="col" className="px-4 py-3 font-semibold">
                      Ngày tạo
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((row) => (
                    <tr key={row.id} className="border-t border-gray-100">
                      <td className="px-4 py-3 font-medium text-gray-900">{row.title}</td>
                      <td className="px-4 py-3 text-gray-700">{row.category.name}</td>
                      <td className="px-4 py-3">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                            row.status === 'Published'
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {STATUS_LABEL[row.status]}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-gray-700">{formatDate(row.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <h2 className="mt-12 text-lg font-semibold text-gray-900">
              Trạng thái: chưa có công thức
            </h2>
            <EmptyState
              className="mt-4"
              icon={BookOpen}
              headingLevel="h3"
              title="Bạn chưa có công thức nào"
              description="Chia sẻ món tủ đầu tiên của bạn."
              action={
                <Link
                  href="/mockups/dashboard"
                  className="rounded-lg bg-orange-700 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-800"
                >
                  Viết công thức
                </Link>
              }
            />
          </main>
          <section
            aria-label="Trạng thái đang tải"
            className="border-t border-dashed border-amber-300"
          >
            <LoadingSkeleton variant="table" count={4} label="Ví dụ khung chờ bảng" />
          </section>
        </div>
      </div>
    </>
  );
}
