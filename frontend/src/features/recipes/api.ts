import { apiFetch } from '@/lib/api-client';
import type { PagedResult, RecipeDetail, RecipeSearchResult, RecipeSummary } from '@/types/api';
import type { RecipeQueryState, SortField, SortOrder } from './search-params';
import { DEFAULT_PAGE_SIZE, DEFAULT_SORT_BY, DEFAULT_SORT_ORDER } from './search-params';

/** FR-RCP-001 / FR-SRCH-002..004 – tham số `GET /recipes`. Sắp xếp là CẶP `sortBy` + `sortOrder` (MT-01, D-10). */
export interface RecipeListParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: string;
  maxCookTime?: number;
  maxPrepTime?: number;
  minServings?: number;
  sortBy?: SortField;
  sortOrder?: SortOrder;
}

/** FR-SRCH-001 – tham số `GET /recipes/search` (SRS §8.3: chỉ q, page, pageSize, categoryId, difficulty). */
export interface RecipeSearchParams {
  q: string;
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: string;
}

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
  return apiFetch<PagedResult<RecipeSearchResult>>(`/recipes/search${toQuery(params)}`, {
    cache: 'no-store',
  });
}

/** FR-RCP-002 – GET /recipes/{slug} (ISR revalidate 300s – SRS §5.1). */
export function getRecipeBySlug(slug: string) {
  return apiFetch<RecipeDetail>(`/recipes/${encodeURIComponent(slug)}`, {
    next: { revalidate: 300 },
  });
}
