import { apiFetch } from '@/lib/api-client';
import type { PagedResult } from '@/types/api';
import type { MyRecipesParams, RecipeSummary } from '@/types/recipe';

export async function getMyRecipes(params: MyRecipesParams): Promise<PagedResult<RecipeSummary>> {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined) query.set(key, String(value));
  });
  return apiFetch<PagedResult<RecipeSummary>>(`/recipes/mine?${query.toString()}`);
}
