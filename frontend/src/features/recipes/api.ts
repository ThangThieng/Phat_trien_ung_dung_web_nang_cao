import { apiFetch } from '@/lib/api-client';
import type { PagedResult } from '@/types/api';
import type { RecipeDetail, RecipeSummary } from '@/types/recipe';
import type {
  RecipeListParams,
  RecipeQueryState,
  RecipeSearchParams,
  RecipeSearchResult,
} from '@/types/search';
import { DEFAULT_PAGE_SIZE, DEFAULT_SORT_BY, DEFAULT_SORT_ORDER } from '@/types/search';

function toQuery(params: object): string {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    query.set(key, String(value));
  });
  const text = query.toString();
  return text ? `?${text}` : '';
}

/** Trạng thái URL của /recipes → tham số API (retrofit D-10 phía FE: không còn `sort=-field`). */
export function toRecipeListParams(state: RecipeQueryState): RecipeListParams {
  return {
    page: state.page,
    pageSize: DEFAULT_PAGE_SIZE,
    categoryId: state.categoryId,
    difficulty: state.difficulty,
    maxCookTime: state.maxCookTime,
    maxPrepTime: state.maxPrepTime,
    minServings: state.minServings,
    sortBy: state.sortBy ?? DEFAULT_SORT_BY,
    sortOrder: state.sortOrder ?? DEFAULT_SORT_ORDER,
  };
}

/** FR-RCP-001 – GET /recipes (SSR dynamic: không cache ở Next; backend cache Redis 2 phút — NFR-PERF-003). */
export function getRecipes(params: RecipeListParams = {}) {
  return apiFetch<PagedResult<RecipeSummary>>(`/recipes${toQuery(params)}`, { cache: 'no-store' });
}

/** FR-SRCH-001 – GET /recipes/search (SSR dynamic; backend cache Redis 1 phút, xếp theo ts_rank). */
export function searchRecipes(params: RecipeSearchParams) {
  return apiFetch<RecipeSearchResult>(`/recipes/search${toQuery(params)}`, {
    cache: 'no-store',
  });
}

/**
 * FR-RCP-002 – GET /recipes/{slug}. Trang vẫn render theo request vì CSP nonce,
 * còn riêng dữ liệu được Next Data Cache trong tối đa 5 phút (MT-67).
 */
export function getRecipeBySlug(slug: string) {
  return apiFetch<RecipeDetail>(`/recipes/${encodeURIComponent(slug)}`, {
    next: { revalidate: 300, tags: [`recipe:${slug}`] },
  });
}
