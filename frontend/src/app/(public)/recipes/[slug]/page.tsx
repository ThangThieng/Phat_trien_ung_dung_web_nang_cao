import type { Metadata } from 'next';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { cache } from 'react';
import { ApiError } from '@/lib/api-client';
import { getRecipeBySlug } from '@/features/recipes/api';
import IngredientList from '@/features/recipes/components/IngredientList';
import NutritionTable from '@/features/recipes/components/NutritionTable';
import RecipeActions from '@/features/recipes/components/RecipeActions';
import RecipeGallery from '@/features/recipes/components/RecipeGallery';
import RecipeHero from '@/features/recipes/components/RecipeHero';
import StepChecklist from '@/features/recipes/components/StepChecklist';

// CSP nonce của root layout khiến route này render theo request. Không khai báo
// `revalidate`/`generateStaticParams`: Data Cache 300 giây nằm ở getRecipeBySlug.
// Cũng cố ý không có loading.tsx ở nhánh [slug], để notFound() còn gửi HTTP 404 thật.
type Params = Promise<{ slug: string }>;

/** Draft/Archived bị API chặn bằng 403 nhưng với người xem công khai là không tồn tại (MT-34). */
const loadRecipe = cache(async (slug: string) => {
  try {
    return await getRecipeBySlug(slug);
  } catch (error) {
    if (error instanceof ApiError && (error.status === 404 || error.status === 403)) return null;
    throw error;
  }
});

export async function generateMetadata({ params }: { params: Params }): Promise<Metadata> {
  const recipe = await loadRecipe((await params).slug);
  if (!recipe) return { title: 'Không tìm thấy công thức', robots: { index: false } };
  return {
    title: recipe.title,
    description: recipe.description.slice(0, 160),
    alternates: { canonical: `/recipes/${recipe.slug}` },
  };
}

export default async function RecipeDetailPage({ params }: { params: Params }) {
  const recipe = await loadRecipe((await params).slug);
  if (!recipe) notFound();

  return (
    <article className="mx-auto max-w-6xl px-4 py-10">
      <nav aria-label="Breadcrumb" className="recipe-breadcrumb text-sm text-gray-500">
        <Link href="/recipes" className="hover:text-orange-700">
          Công thức
        </Link>
        <span aria-hidden> / </span>
        <Link href={`/categories/${recipe.category.slug}`} className="hover:text-orange-700">
          {recipe.category.name}
        </Link>
      </nav>
      <header className="mt-4 grid gap-8 lg:grid-cols-2">
        <RecipeGallery images={recipe.images} title={recipe.title} />
        <RecipeHero recipe={recipe} />
      </header>
      <div className="mt-6">
        <RecipeActions title={recipe.title} description={recipe.description} />
      </div>
      <div className="mt-12 grid gap-10 lg:grid-cols-[1fr_2fr]">
        <aside className="flex flex-col gap-8">
          <IngredientList ingredients={recipe.ingredients} servings={recipe.servings} />
          {recipe.nutrition && (
            <section
              aria-labelledby="nutrition-heading"
              className="rounded-2xl border border-gray-200 bg-white p-5"
            >
              <h2 id="nutrition-heading" className="text-xl font-bold text-gray-900">
                Thông tin dinh dưỡng
              </h2>
              <div className="mt-3">
                <NutritionTable nutrition={recipe.nutrition} />
              </div>
            </section>
          )}
        </aside>
        <section aria-labelledby="steps-heading">
          <h2 id="steps-heading" className="mb-4 text-xl font-bold text-gray-900">
            Các bước thực hiện
          </h2>
          <StepChecklist steps={recipe.steps} />
        </section>
      </div>
    </article>
  );
}
