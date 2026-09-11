import type { Metadata } from 'next';
import Image from 'next/image';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { cache } from 'react';
import { ChefHat, Clock, Flame, Users } from 'lucide-react';
import { ApiError } from '@/lib/api-client';
import { getRecipeBySlug } from '@/features/recipes/api';
import NutritionTable from '@/features/recipes/components/NutritionTable';
import StepChecklist from '@/features/recipes/components/StepChecklist';
import {
  DIFFICULTY_LABEL,
  DIFFICULTY_STYLE,
  formatDate,
  formatMinutes,
  formatQuantity,
} from '@/features/recipes/format';

// SRS §5.1: /recipes/[slug] – ISR revalidate=300. Không prerender lúc build, sinh theo yêu cầu rồi cache.
export const revalidate = 300;
export const dynamicParams = true;
export function generateStaticParams() {
  return [];
}

type Params = Promise<{ slug: string }>;

/** 404 (không tồn tại) và 403 (Draft/Archived) đều hiển thị trang không tìm thấy cho khách. */
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

  const primaryImage = recipe.images.find((image) => image.isPrimary) ?? recipe.images[0];

  return (
    <article className="mx-auto max-w-6xl px-4 py-10">
      <nav aria-label="Breadcrumb" className="text-sm text-gray-500">
        <Link href="/recipes" className="hover:text-orange-700">
          Công thức
        </Link>
        <span aria-hidden> / </span>
        <Link href={`/categories/${recipe.category.slug}`} className="hover:text-orange-700">
          {recipe.category.name}
        </Link>
      </nav>

      <header className="mt-4 grid gap-8 lg:grid-cols-2">
        <div className="relative aspect-[4/3] overflow-hidden rounded-2xl bg-gradient-to-br from-orange-100 to-amber-50">
          {primaryImage ? (
            <Image
              src={primaryImage.mediumUrl ?? primaryImage.originalUrl}
              alt={primaryImage.altText ?? recipe.title}
              fill
              priority
              unoptimized
              sizes="(max-width: 1024px) 100vw, 50vw"
              className="object-cover"
            />
          ) : (
            <span
              aria-hidden
              className="absolute inset-0 flex items-center justify-center text-8xl"
            >
              🍲
            </span>
          )}
        </div>

        <div className="flex flex-col gap-4">
          <span
            className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${DIFFICULTY_STYLE[recipe.difficulty]}`}
          >
            Độ khó: {DIFFICULTY_LABEL[recipe.difficulty]}
          </span>
          <h1 className="text-3xl font-bold text-gray-900 sm:text-4xl">{recipe.title}</h1>
          <p className="text-lg leading-relaxed text-gray-700">{recipe.description}</p>
          <p className="flex items-center gap-2 text-sm text-gray-600">
            <ChefHat aria-hidden className="h-4 w-4 text-orange-600" />
            <span>
              Tác giả <strong className="text-gray-900">{recipe.author.displayName}</strong>
              {recipe.publishedAt && <> · {formatDate(recipe.publishedAt)}</>}
            </span>
          </p>
          <dl className="mt-2 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {[
              { icon: Clock, label: 'Chuẩn bị', value: formatMinutes(recipe.prepTimeMinutes) },
              { icon: Flame, label: 'Nấu', value: formatMinutes(recipe.cookTimeMinutes) },
              {
                icon: Clock,
                label: 'Tổng thời gian',
                value: formatMinutes(recipe.totalTimeMinutes),
              },
              { icon: Users, label: 'Khẩu phần', value: `${recipe.servings} người` },
            ].map(({ icon: Icon, label, value }) => (
              <div key={label} className="rounded-xl border border-orange-100 bg-orange-50/60 p-3">
                <dt className="flex items-center gap-1 text-xs text-gray-600">
                  <Icon aria-hidden className="h-3.5 w-3.5" /> {label}
                </dt>
                <dd className="mt-1 font-semibold text-gray-900">{value}</dd>
              </div>
            ))}
          </dl>
        </div>
      </header>

      <div className="mt-12 grid gap-10 lg:grid-cols-[1fr_2fr]">
        <aside className="flex flex-col gap-8">
          <section
            aria-labelledby="ingredients-heading"
            className="rounded-2xl border border-gray-200 bg-white p-5"
          >
            <h2 id="ingredients-heading" className="text-xl font-bold text-gray-900">
              Nguyên liệu
            </h2>
            <ul className="mt-4 divide-y divide-gray-100">
              {recipe.ingredients.map((ingredient) => (
                <li
                  key={ingredient.id}
                  className="flex items-baseline justify-between gap-3 py-2 text-sm"
                >
                  <span className="text-gray-900">
                    {ingredient.name}
                    {ingredient.notes && (
                      <span className="block text-xs text-gray-500">{ingredient.notes}</span>
                    )}
                  </span>
                  <span className="shrink-0 font-medium text-gray-700">
                    {formatQuantity(ingredient.quantity, ingredient.unit)}
                  </span>
                </li>
              ))}
            </ul>
          </section>

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
