import Image from 'next/image';
import Link from 'next/link';
import type { Category } from '@/types/api';

const EMOJI: Record<string, string> = {
  'mon-khai-vi': '🥗',
  'mon-chinh': '🍛',
  'canh-sup': '🍲',
  'mon-chay': '🥦',
  'mon-nuong': '🍢',
  'bun-pho': '🍜',
  banh: '🥐',
  'trang-mieng-do-uong': '🍧',
};

export default function CategoryCard({ category }: { category: Category }) {
  return (
    <Link
      href={`/categories/${category.slug}`}
      className="group flex flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
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
            {EMOJI[category.slug] ?? '🍽️'}
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
        <p className="mt-auto pt-2 text-sm font-medium text-orange-700">
          {category.recipeCount} công thức
        </p>
      </div>
    </Link>
  );
}
