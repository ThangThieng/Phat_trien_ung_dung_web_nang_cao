import { apiFetch } from '@/lib/api-client';
import type { PagedResult, RecipeDetail, RecipeSummary } from '@/types/api';

export interface RecipeListParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: string;
  maxCookTime?: number;
  sort?: string;
}

function toQuery(params: RecipeListParams): string {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, String(value));
  });
  const text = query.toString();
  return text ? `?${text}` : '';
}

/** FR-RCP-001 – GET /recipes (SSR dynamic: không cache ở Next, backend đã có Output Cache 15 phút). */
export function getRecipes(params: RecipeListParams = {}) {
  return apiFetch<PagedResult<RecipeSummary>>(`/recipes${toQuery(params)}`, { cache: 'no-store' });
}

/** FR-RCP-002 – GET /recipes/{slug} (ISR revalidate 300s – SRS §5.1). */
export function getRecipeBySlug(slug: string) {
  return apiFetch<RecipeDetail>(`/recipes/${encodeURIComponent(slug)}`, {
    next: { revalidate: 300 },
  });
}
