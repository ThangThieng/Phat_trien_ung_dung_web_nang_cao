import type { MetadataRoute } from 'next';
import { apiFetch } from '@/lib/api-client';
import type { Category } from '@/types/category';
import { absoluteUrl } from './site';

/** SRS §8.3 `GET /recipes/sitemap` — `{ slug, updatedAt }` của MỌI công thức Published, không phân trang. */
export interface SitemapRecipe {
  slug: string;
  updatedAt: string | null;
}

/** Cache 1 giờ = TTL Redis của endpoint = chu kỳ sinh lại sitemap (MT-48). Tách riêng để phần sinh XML kiểm thử độc lập. */
export function getSitemapRecipes() {
  return apiFetch<SitemapRecipe[]>('/recipes/sitemap', { next: { revalidate: 3600, tags: ['recipes'] } });
}

const STATIC_PAGES: { path: string; changeFrequency: 'daily' | 'weekly'; priority: number }[] = [
  { path: '/', changeFrequency: 'daily', priority: 1 },
  { path: '/recipes', changeFrequency: 'daily', priority: 0.8 },
  { path: '/categories', changeFrequency: 'weekly', priority: 0.7 },
  { path: '/search', changeFrequency: 'weekly', priority: 0.3 },
];

export function buildSitemapEntries(input: {
  recipes: readonly SitemapRecipe[];
  categories: readonly Pick<Category, 'slug'>[];
  now?: Date;
}): MetadataRoute.Sitemap {
  const now = input.now ?? new Date();
  return [
    ...STATIC_PAGES.map((page) => ({
      url: absoluteUrl(page.path),
      lastModified: now,
      changeFrequency: page.changeFrequency,
      priority: page.priority,
    })),
    ...input.categories.map((category) => ({
      url: absoluteUrl(`/categories/${category.slug}`),
      lastModified: now,
      changeFrequency: 'weekly' as const,
      priority: 0.6,
    })),
    ...input.recipes.map((recipe) => ({
      url: absoluteUrl(`/recipes/${recipe.slug}`),
      lastModified: recipe.updatedAt ? new Date(recipe.updatedAt) : now,
      changeFrequency: 'weekly' as const,
      priority: 0.9,
    })),
  ];
}
