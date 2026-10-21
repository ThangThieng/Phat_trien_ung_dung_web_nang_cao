import Link from 'next/link';
import type { Category } from '@/types/category';

interface SidebarCategoryProps {
  categories: readonly Category[];
  /** Slug của danh mục đang xem — mục đó có `aria-current="page"`. */
  activeSlug?: string;
  className?: string;
}

/**
 * Cột danh mục bên cạnh lưới công thức (trang chủ, trang danh mục — Buổi 7). Server Component thuần: dữ liệu truyền vào từ
 * trang (đã lấy ở server), không có state, không gửi JavaScript xuống trình duyệt.
 * `<nav aria-label="Danh mục">`, mục đang xem `aria-current="page"`, số công thức đã xuất bản hiển thị dạng huy hiệu (badge)
 * kèm chữ ẩn cho trình đọc màn hình (NFR-USE-002).
 */
export default function SidebarCategory({
  categories,
  activeSlug,
  className = '',
}: SidebarCategoryProps) {
  return (
    <nav
      aria-label="Danh mục"
      className={`rounded-2xl border border-orange-100 bg-white p-4 ${className}`}
    >
      <h2 className="px-2 text-sm font-semibold tracking-wide text-gray-900 uppercase">Danh mục</h2>
      <ul className="mt-3 space-y-0.5">
        {categories.map((category) => {
          const active = category.slug === activeSlug;
          return (
            <li key={category.id}>
              <Link
                href={`/categories/${category.slug}`}
                aria-current={active ? 'page' : undefined}
                className={`flex items-center justify-between gap-2 rounded-lg px-2 py-2 text-sm ${
                  active
                    ? 'bg-orange-50 font-semibold text-orange-800'
                    : 'text-gray-700 hover:bg-orange-50 hover:text-orange-800'
                }`}
              >
                <span className="truncate">{category.name}</span>
                <span
                  className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
                    active ? 'bg-orange-700 text-white' : 'bg-gray-100 text-gray-700'
                  }`}
                >
                  {category.recipeCount}
                  <span className="sr-only"> công thức</span>
                </span>
              </Link>
            </li>
          );
        })}
      </ul>
      <Link
        href="/categories"
        className="mt-3 block rounded-lg px-2 py-2 text-sm font-medium text-orange-700 hover:bg-orange-50"
      >
        Xem tất cả danh mục →
      </Link>
    </nav>
  );
}
