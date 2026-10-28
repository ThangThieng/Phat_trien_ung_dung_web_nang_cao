import { Badge } from '@/components/ui/badge';
import type { Recipe } from '@/types/recipe';
import { DIFFICULTY_LABEL, formatDate, formatMinutes } from '../format';

/** Phần thông tin ngữ nghĩa đầu trang công thức (FR-RCP-002). */
export default function RecipeHero({ recipe }: { recipe: Recipe }) {
  const facts = [
    { symbol: '◷', label: 'Chuẩn bị', value: formatMinutes(recipe.prepTimeMinutes) },
    { symbol: '♨', label: 'Nấu', value: formatMinutes(recipe.cookTimeMinutes) },
    { symbol: '◷', label: 'Tổng thời gian', value: formatMinutes(recipe.totalTimeMinutes) },
    { symbol: '♟', label: 'Khẩu phần', value: `${recipe.servings} người` },
    { symbol: '★', label: 'Độ khó', value: DIFFICULTY_LABEL[recipe.difficulty] },
  ];

  return (
    <div className="flex flex-col gap-4">
      <Badge variant="secondary" className="w-fit">
        {recipe.category.name}
      </Badge>
      <h1 className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
        {recipe.title}
      </h1>
      <p className="text-lg leading-relaxed text-gray-700">{recipe.description}</p>
      <p className="flex items-center gap-2 text-sm text-gray-600">
        <span
          aria-hidden
          className="flex size-8 items-center justify-center rounded-full bg-orange-100 font-semibold text-orange-800"
        >
          {recipe.author.displayName.slice(0, 1).toUpperCase()}
        </span>
        <span>
          Tác giả <strong className="text-gray-900">{recipe.author.displayName}</strong>
          {recipe.publishedAt && (
            <>
              <span aria-hidden> · </span>
              <time dateTime={recipe.publishedAt}>{formatDate(recipe.publishedAt)}</time>
            </>
          )}
        </span>
      </p>
      <dl className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">
        {facts.map(({ symbol, label, value }) => (
          <div key={label} className="rounded-xl border border-orange-100 bg-orange-50/60 p-3">
            <dt className="flex items-center gap-1 text-xs text-gray-600">
              <span aria-hidden className="inline-flex size-3.5 items-center justify-center text-sm">
                {symbol}
              </span>
              {label}
            </dt>
            <dd className="mt-1 font-semibold text-gray-900">{value}</dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
