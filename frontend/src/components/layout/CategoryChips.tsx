import Link from 'next/link';
import type { Category } from '@/types/category';

interface CategoryChipsProps {
  categories: readonly Category[];
  /** Slug của danh mục đang xem — nhãn đó có `aria-current="page"`. */
  activeSlug?: string;
  className?: string;
}

/**
 * Hàng nhãn danh mục cuộn ngang — thay cho `SidebarCategory` dưới 1200 px (tablet, điện thoại: SRS NFR-USE-001), nơi không đủ
 * chỗ cho cột phụ. Trang đặt `min-[1200px]:hidden` cho khối này và `hidden min-[1200px]:block` cho sidebar nên mỗi lúc chỉ một
 * trong hai hiện ra (phần ẩn bằng `display: none` cũng bị trình đọc màn hình bỏ qua).
 *
 * Server Component thuần, không JavaScript. Cuộn ngang nằm TRONG `<ul>` (`overflow-x-auto`), không làm trang cuộn ngang; các
 * liên kết vẫn focus được bằng Tab nên vùng cuộn dùng được bằng bàn phím (axe `scrollable-region-focusable`).
 */
export default function CategoryChips({
  categories,
  activeSlug,
  className = '',
}: CategoryChipsProps) {
  if (categories.length === 0) return null;

  return (
    <nav aria-label="Danh mục nhanh" className={className}>
      {/* `relative`: chữ `sr-only` là `position: absolute` — không có tổ tiên định vị thì nó thoát khỏi vùng cuộn và làm
          cả TRANG cuộn ngang (đã đo ở 320 px: +290 px). */}
      <ul className="relative -mx-4 flex gap-2 overflow-x-auto px-4 pb-2">
        {categories.map((category) => {
          const active = category.slug === activeSlug;
          return (
            <li key={category.id} className="shrink-0">
              <Link
                href={`/categories/${category.slug}`}
                aria-current={active ? 'page' : undefined}
                className={`block rounded-full border px-3.5 py-1.5 text-sm whitespace-nowrap focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600 ${
                  active
                    ? 'border-orange-700 bg-orange-700 font-semibold text-white'
                    : 'border-orange-200 bg-white text-gray-800 hover:bg-orange-50'
                }`}
              >
                {category.name}
                <span className="sr-only">, {category.recipeCount} công thức</span>
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
