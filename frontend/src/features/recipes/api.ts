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

/**
 * Trạng thái URL của /search → tham số API (SRS v1.2.3 §8.3; ba bộ lọc thời gian/khẩu phần của CR-2026-05 / MT-66). Server
 * (prefetch lúc SSR) và trình duyệt (`useQuery` khi đổi bộ lọc) cùng gọi hàm này nên query key hai phía luôn trùng nhau.
 */
export function toRecipeSearchParams(state: RecipeQueryState): RecipeSearchParams {
  return {
    q: state.q ?? '',
    page: state.page,
    pageSize: DEFAULT_PAGE_SIZE,
    categoryId: state.categoryId,
    difficulty: state.difficulty,
    maxCookTime: state.maxCookTime,
    maxPrepTime: state.maxPrepTime,
    minServings: state.minServings,
  };
}

/**
 * FR-SRCH-001 – GET /recipes/search. Không dùng Data Cache của Next (`no-store`): backend đã cache Redis 1 phút
 * (`search:{queryHash}`), còn phía trình duyệt TanStack Query giữ kết quả 60 giây (`staleTime`). `signal` để TanStack Query
 * hủy request cũ khi người dùng đổi bộ lọc liên tiếp.
 */
export function searchRecipes(params: RecipeSearchParams, signal?: AbortSignal) {
  return apiFetch<RecipeSearchResult>(`/recipes/search${toQuery(params)}`, {
    cache: 'no-store',
    signal,
  });
}

/** FR-RCP-002 – GET /recipes/{slug} (ISR revalidate 300s – SRS §5.1). */
export function getRecipeBySlug(slug: string) {
  return apiFetch<RecipeDetail>(`/recipes/${encodeURIComponent(slug)}`, {
    next: { revalidate: 300 },
  });
}
