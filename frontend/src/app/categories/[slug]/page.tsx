import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { cache } from 'react';
import Pagination from '@/components/ui/Pagination';
import RecipeCard from '@/features/recipes/components/RecipeCard';
import { getCategoryBySlug } from '@/features/categories/api';
import { ApiError } from '@/lib/api-client';

type Params = Promise<{ slug: string }>;
type SearchParams = Promise<Record<string, string | string[] | undefined>>;

// SRS §5.1: /categories/[slug] – dữ liệu revalidate 600s (fetch Data Cache, xem features/categories/api.ts)
export const dynamic = 'force-dynamic';

const loadCategory = cache(async (slug: string, page: number) => {
  try {
    return await getCategoryBySlug(slug, page);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
});

const parsePage = (value: string | string[] | undefined) =>
  Math.max(1, Number(Array.isArray(value) ? value[0] : value) || 1);

export async function generateMetadata({
  params,
  searchParams,
}: {
  params: Params;
  searchParams: SearchParams;
}): Promise<Metadata> {
  const detail = await loadCategory((await params).slug, parsePage((await searchParams).page));
  if (!detail) return { title: 'Không tìm thấy danh mục', robots: { index: false } };
  return {
    title: detail.category.name,
    description: detail.category.description ?? `Công thức thuộc danh mục ${detail.category.name}.`,
    alternates: { canonical: `/categories/${detail.category.slug}` },
  };
}

export default async function CategoryDetailPage({
  params,
  searchParams,
}: {
  params: Params;
  searchParams: SearchParams;
}) {
  const { slug } = await params;
  const detail = await loadCategory(slug, parsePage((await searchParams).page));
  if (!detail) notFound();

  const { category, recipes } = detail;

  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
        <Link href="/categories" className="hover:text-orange-700">
          Danh mục
        </Link>
        <span aria-hidden> / </span>
        <span className="text-gray-700">{category.name}</span>
      </nav>

      <header className="mt-4 rounded-2xl bg-gradient-to-r from-orange-100 to-amber-50 p-8">
        <h1 className="text-3xl font-bold text-gray-900">{category.name}</h1>
        {category.description && (
          <p className="mt-2 max-w-2xl text-gray-700">{category.description}</p>
        )}
        <p className="mt-3 text-sm font-medium text-orange-800">{recipes.totalCount} công thức</p>
      </header>

      {recipes.items.length === 0 ? (
        <p className="mt-16 text-center text-gray-600">Danh mục này chưa có công thức nào.</p>
      ) : (
        <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {recipes.items.map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} />
          ))}
        </div>
      )}

      <Pagination
        page={recipes.page}
        totalPages={recipes.totalPages}
        buildHref={(p) => `/categories/${category.slug}?page=${p}`}
      />
    </div>
  );
}
