import type { Metadata } from 'next';
import { FolderOpen } from 'lucide-react';
import EmptyState from '@/components/shared/EmptyState';
import CategoryCard from '@/features/categories/components/CategoryCard';
import { getCategories } from '@/features/categories/api';

export const metadata: Metadata = {
  title: 'Danh mục công thức',
  description:
    'Duyệt công thức theo danh mục: món khai vị, món chính, canh súp, món chay, bánh, tráng miệng…',
};

/*
 * SRS §5.1: /categories – SSR + Data Cache `revalidate: 1800` (MT-67).
 * Trang render theo request, dữ liệu lấy từ Data Cache của Next.js (`getCategories()` — revalidate 1800s) nên API chỉ bị
 * gọi tối đa 1 lần / 30 phút. Không prerender lúc `next build` vì khi build image Docker, backend API chưa chạy.
 */
export const dynamic = 'force-dynamic';

export default async function CategoriesPage() {
  const categories = await getCategories();

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <h1 className="text-3xl font-bold text-gray-900">Danh mục công thức</h1>
      <p className="mt-1 text-gray-600">Chọn một danh mục để khám phá các món ngon.</p>

      {categories.length === 0 ? (
        <EmptyState className="mt-8" icon={FolderOpen} title="Chưa có danh mục nào" />
      ) : (
        <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {categories.map((category) => (
            <CategoryCard key={category.id} category={category} />
          ))}
        </div>
      )}
    </div>
  );
}
