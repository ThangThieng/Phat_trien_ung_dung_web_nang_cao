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
 *
 * Bọc trong `<aside>` (Buổi 7): đây là cột phụ đứng cạnh nội dung chính — NFR-USE-002 liệt kê `<aside>` trong các phần tử
 * ngữ nghĩa bắt buộc; trình đọc màn hình nhảy được thẳng tới vùng "nội dung bổ trợ" thay vì chỉ thấy thêm một `<nav>`.
 * `className` (ẩn/hiện theo breakpoint, sticky) đặt lên `<aside>` để cả landmark cùng ẩn dưới 1200 px.
 */
export default function SidebarCategory({
  categories,
  activeSlug,
  className = '',
}: SidebarCategoryProps) {
  return (
    <aside
      aria-label="Danh mục công thức"
      className={`rounded-2xl border border-line bg-paper p-4 ${className}`}
    >
      <nav aria-label="Danh mục">
        <h2 className="px-2 text-sm font-semibold tracking-wide text-ink uppercase">
          Danh mục
        </h2>
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
                      ? 'bg-gold-soft font-semibold text-gold-ink'
                      : 'text-ink hover:bg-gold-soft hover:text-gold-ink'
                  }`}
                >
                  <span className="truncate">{category.name}</span>
                  <span
                    className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-medium ${
                      active ? 'bg-navy text-white' : 'bg-gold-soft text-ink'
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
          className="mt-3 block rounded-lg px-2 py-2 text-sm font-medium text-gold-ink hover:bg-gold-soft"
        >
          Xem tất cả danh mục →
        </Link>
      </nav>
    </aside>
  );
}
