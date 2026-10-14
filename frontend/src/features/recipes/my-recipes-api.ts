import { apiFetch } from '@/lib/api-client';
import type {
  MyRecipesParams,
  PagedResult,
  RecipeDetail,
  RecipeStatus,
  RecipeSummary,
} from '@/types/api';

export const myRecipesKey = (params: MyRecipesParams = {}) => ['recipes', 'mine', params] as const;

function queryString(params: Record<string, unknown>): string {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value));
  });
  const value = search.toString();
  return value ? `?${value}` : '';
}

/** FR-RCP-011: dữ liệu phụ thuộc Author nên browser/proxy/TanStack không được dùng chung với recipe công khai. */
export function getMyRecipes(params: MyRecipesParams, accessToken: string, signal?: AbortSignal) {
  const { sortBy, sortOrder, ...rest } = params;
  const sort = sortBy ? `${sortOrder === 'desc' ? '-' : ''}${sortBy}` : undefined;
  return apiFetch<PagedResult<RecipeSummary>>(
    `/recipes/mine${queryString({ ...rest, sort })}`,
    {
      accessToken,
      cache: 'no-store',
      signal,
    },
  );
}

export function deleteRecipe(recipeId: string, accessToken: string) {
  return apiFetch<void>(`/recipes/${recipeId}`, { method: 'DELETE', accessToken });
}

export function changeRecipeStatus(
  recipe: Pick<RecipeDetail, 'id'>,
  action: Extract<RecipeStatus, 'Draft' | 'Published'>,
  accessToken: string,
) {
  const endpoint = action === 'Published' ? 'publish' : 'unpublish';
  return apiFetch<RecipeDetail>(`/recipes/${recipe.id}/${endpoint}`, {
    method: 'PATCH',
    accessToken,
  });
}
