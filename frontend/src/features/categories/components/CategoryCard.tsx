import { IMAGE_UNOPTIMIZED } from '@/lib/images';
import Image from 'next/image';
import Link from 'next/link';
import type { Category } from '@/types/category';

/** Biểu tượng khi danh mục chưa có ảnh — khóa là slug của các danh mục mẫu (seed `categories.json`). */
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
  // 6 danh mục cho công thức bổ sung từ Wikibooks (CR-2026-07).
  'mi-pasta': '🍝',
  'mon-ham-ca-ri': '🍛',
  'mon-an-do': '🫓',
  'mon-my': '🍔',
  'mon-chau-a-khac': '🥡',
  'mon-chau-phi': '🫘',
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
      className="group flex h-full flex-col overflow-hidden rounded-2xl border border-line bg-paper shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="relative flex aspect-[16/9] items-center justify-center bg-gradient-to-br from-gold-soft via-paper to-white">
        {category.imageUrl ? (
          <Image
            src={category.imageUrl}
            alt=""
            width={800}
            height={450}
            unoptimized={IMAGE_UNOPTIMIZED}
            sizes="(max-width: 768px) 100vw, 25vw"
            className="absolute inset-0 h-full w-full object-cover"
          />
        ) : (
          <span aria-hidden className="text-5xl transition group-hover:scale-110">
            {CATEGORY_EMOJI[category.slug] ?? '🍽️'}
          </span>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-1 p-4">
        <h2 className="text-lg font-semibold text-ink group-hover:text-gold-ink">
          {category.name}
        </h2>
        {category.description && (
          <p className="line-clamp-2 text-sm text-ink-soft">{category.description}</p>
        )}
        <p className="mt-auto pt-3">
          <span className="inline-block rounded-full bg-gold-soft px-2.5 py-0.5 text-xs font-semibold text-gold-ink ring-1 ring-line ring-inset">
            {category.recipeCount} công thức
          </span>
        </p>
      </div>
    </Link>
  );
}
