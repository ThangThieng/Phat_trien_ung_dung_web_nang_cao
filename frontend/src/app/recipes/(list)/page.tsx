import type { Metadata } from 'next';
import Link from 'next/link';
import Pagination from '@/components/ui/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { getRecipes } from '@/features/recipes/api';

export const metadata: Metadata = {
  title: 'Công thức nấu ăn',
  description:
    'Khám phá hàng trăm công thức nấu ăn Việt Nam và quốc tế – từ món khai vị, món chính đến tráng miệng.',
};

// SRS §5.1: /recipes render SSR dynamic
export const dynamic = 'force-dynamic';

const SORTS = [
  { value: '-createdAt', label: 'Mới nhất' },
  { value: 'title', label: 'Tên A → Z' },
  { value: 'cookTime', label: 'Nấu nhanh' },
];

type SearchParams = Promise<Record<string, string | string[] | undefined>>;

const first = (value: string | string[] | undefined) => (Array.isArray(value) ? value[0] : value);

export default async function RecipesPage({ searchParams }: { searchParams: SearchParams }) {
  const params = await searchParams;
  const page = Math.max(1, Number(first(params.page)) || 1);
  const sort = SORTS.some((s) => s.value === first(params.sort))
    ? first(params.sort)!
    : '-createdAt';

  const result = await getRecipes({ page, pageSize: 12, sort });

  const buildHref = (p: number, s = sort) => {
    const query = new URLSearchParams({ page: String(p) });
    if (s !== '-createdAt') query.set('sort', s);
    return `/recipes?${query.toString()}`;
  };

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Công thức nấu ăn</h1>
          <p className="mt-1 text-gray-600">{result.totalCount} công thức đang chờ bạn khám phá.</p>
        </div>
        <nav aria-label="Sắp xếp" className="flex gap-2">
          {SORTS.map((s) => (
            <Link
              key={s.value}
              href={buildHref(1, s.value)}
              aria-current={s.value === sort ? 'true' : undefined}
              className={`rounded-full border px-3 py-1 text-sm ${
                s.value === sort
                  ? 'border-orange-600 bg-orange-600 text-white'
                  : 'border-gray-300 text-gray-700 hover:border-orange-400'
              }`}
            >
              {s.label}
            </Link>
          ))}
        </nav>
      </div>

      {result.items.length === 0 ? (
        <p className="mt-16 text-center text-gray-600">Chưa có công thức nào.</p>
      ) : (
        <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {result.items.map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} />
          ))}
        </div>
      )}

      <Pagination
        page={result.page}
        totalPages={result.totalPages}
        buildHref={(p) => buildHref(p)}
      />
    </div>
  );
}
