import Image from 'next/image';
import Link from 'next/link';
import type { Category } from '@/types/category';

/** Biểu tượng khi danh mục chưa có ảnh — khóa là slug của 20 danh mục mẫu (seed `categories.json`). */
export const CATEGORY_EMOJI: Record<string, string> = {
  'mon-khai-vi': '🥗',
  'mon-chinh': '🍛',
  'canh-sup': '🍲',
  'mon-chay': '🥦',
  'mon-nuong': '🍢',
  'bun-pho': '🍜',
  banh: '🥐',
  'trang-mieng-do-uong': '🍧',
  // 12 danh mục bổ sung theo CR-2026-03 (≥ 20 danh mục) — trước Buổi 7 đều rơi về biểu tượng chung 🍽️.
  'com-xoi': '🍚',
  lau: '🫕',
  'hai-san': '🦐',
  'goi-salad': '🥬',
  'mon-cuon': '🌯',
  'mon-hap': '🥟',
  'mon-chien': '🍤',
  'an-vat': '🍡',
  'mon-han-quoc': '🥢',
  'mon-nhat-ban': '🍣',
  'mon-thai': '🌶️',
  'mon-au': '🍝',
};

/**
 * Thẻ danh mục (FR-CAT-001): ảnh, tên, mô tả tối đa hai dòng, huy hiệu số công thức. `recipeCount` chỉ đếm công thức Published
 * — Backend đã tính. Huy hiệu là `<span>` Tailwind có cùng dáng với `Badge` của shadcn/ui (chưa vào `develop` — xem báo cáo
 * Lab 07); khi có thì thay đúng phần này, chỗ dùng không đổi. `h-full` để các thẻ cùng hàng cao bằng nhau.
 */
export default function CategoryCard({ category }: { category: Category }) {
  return (
    <Link
      href={`/categories/${category.slug}`}
      className="group flex h-full flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="relative flex aspect-[16/9] items-center justify-center bg-gradient-to-br from-orange-100 via-amber-50 to-white">
        {category.imageUrl ? (
          <Image
            src={category.imageUrl}
            alt=""
            fill
            unoptimized
            sizes="(max-width: 768px) 100vw, 25vw"
            className="object-cover"
          />
        ) : (
          <span aria-hidden className="text-5xl transition group-hover:scale-110">
            {CATEGORY_EMOJI[category.slug] ?? '🍽️'}
          </span>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-1 p-4">
        <h2 className="text-lg font-semibold text-gray-900 group-hover:text-orange-700">
          {category.name}
        </h2>
        {category.description && (
          <p className="line-clamp-2 text-sm text-gray-600">{category.description}</p>
        )}
        <p className="mt-auto pt-3">
          <span className="inline-block rounded-full bg-orange-50 px-2.5 py-0.5 text-xs font-semibold text-orange-800 ring-1 ring-orange-200 ring-inset">
            {category.recipeCount} công thức
          </span>
        </p>
      </div>
    </Link>
  );
}
