import Image from 'next/image';
import Link from 'next/link';
import { Clock, Users } from 'lucide-react';
import type { RecipeSummary } from '@/types/api';
import { DIFFICULTY_LABEL, DIFFICULTY_STYLE, formatMinutes } from '../format';

/** Card công thức: ảnh chính, tiêu đề, thời gian chuẩn bị/nấu, độ khó (FR-RCP-001). */
export default function RecipeCard({ recipe }: { recipe: RecipeSummary }) {
  return (
    <article className="group flex flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md">
      <Link
        href={`/recipes/${recipe.slug}`}
        className="flex flex-1 flex-col focus-visible:outline-none"
      >
        <div className="relative aspect-[4/3] w-full overflow-hidden bg-gradient-to-br from-orange-100 to-amber-50">
          {recipe.primaryImageUrl ? (
            <Image
              src={recipe.primaryImageUrl}
              alt={recipe.title}
              fill
              unoptimized
              sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
              className="object-cover transition group-hover:scale-105"
            />
          ) : (
            <span
              aria-hidden
              className="absolute inset-0 flex items-center justify-center text-5xl"
            >
              🍲
            </span>
          )}
          <span
            className={`absolute top-3 left-3 rounded-full px-2.5 py-0.5 text-xs font-semibold ${DIFFICULTY_STYLE[recipe.difficulty]}`}
          >
            {DIFFICULTY_LABEL[recipe.difficulty]}
          </span>
          {recipe.status !== 'Published' && (
            <span className="absolute top-3 right-3 rounded-full bg-gray-900/80 px-2.5 py-0.5 text-xs font-semibold text-white">
              {recipe.status === 'Draft' ? 'Bản nháp' : 'Lưu trữ'}
            </span>
          )}
        </div>
        <div className="flex flex-1 flex-col gap-2 p-4">
          <p className="text-xs font-medium tracking-wide text-orange-700 uppercase">
            {recipe.category.name}
          </p>
          <h3 className="line-clamp-2 text-lg font-semibold text-gray-900 group-hover:text-orange-700">
            {recipe.title}
          </h3>
          <p className="line-clamp-2 text-sm text-gray-600">{recipe.description}</p>
          <dl className="mt-auto flex flex-wrap gap-x-4 gap-y-1 pt-2 text-xs text-gray-600">
            <div className="flex items-center gap-1">
              <Clock aria-hidden className="h-3.5 w-3.5" />
              <dt className="sr-only">Chuẩn bị</dt>
              <dd>Chuẩn bị {formatMinutes(recipe.prepTimeMinutes)}</dd>
            </div>
            <div className="flex items-center gap-1">
              <dt className="sr-only">Nấu</dt>
              <dd>· Nấu {formatMinutes(recipe.cookTimeMinutes)}</dd>
            </div>
            <div className="flex items-center gap-1">
              <Users aria-hidden className="h-3.5 w-3.5" />
              <dt className="sr-only">Khẩu phần</dt>
              <dd>{recipe.servings} người</dd>
            </div>
          </dl>
        </div>
      </Link>
    </article>
  );
}
