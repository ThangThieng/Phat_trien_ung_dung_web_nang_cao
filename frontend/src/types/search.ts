/**
 * Kiểu miền Search (tham số lọc/sắp xếp/phân trang của `GET /recipes`, `GET /recipes/search`). Chủ file: Dev 3.
 * Khớp backend: `RecipeQueryRules` (pageSize 12/50, `q` ≥ 2), `SortMapper` (5 cột), `RecipeSearchResultDto`.
 * Thư mục `types/` không import từ thư mục khác ngoài chính nó (ESLint `import/no-restricted-paths`).
 */

import type { PagedResult } from './api';
import type { RecipeDifficulty, RecipeSummary } from './recipe';

/** Whitelist 5 cột sắp xếp (FR-SRCH-003) — khớp `SortMapper` phía backend. */
export const SORT_FIELDS = ['createdAt', 'publishedAt', 'title', 'cookTime', 'prepTime'] as const;
export type SortBy = (typeof SORT_FIELDS)[number];

export const SORT_ORDERS = ['asc', 'desc'] as const;
export type SortOrder = (typeof SORT_ORDERS)[number];

/** Mặc định của SRS FR-SRCH-003: mới nhất trước. */
export const DEFAULT_SORT_BY: SortBy = 'createdAt';
export const DEFAULT_SORT_ORDER: SortOrder = 'desc';

/** FR-SRCH-004: pageSize mặc định 12 (chia hết cho lưới 2/3/4 cột); backend chặn tối đa 50. */
export const DEFAULT_PAGE_SIZE = 12;
export const MAX_PAGE_SIZE = 50;

/** FR-SRCH-001: `q` tối thiểu 2 ký tự, dưới ngưỡng này API trả 400. */
export const MIN_SEARCH_LENGTH = 2;

/** FR-SRCH-002: 4 mức độ khó, gồm cả `Expert`. */
export const DIFFICULTIES: readonly RecipeDifficulty[] = ['Easy', 'Medium', 'Hard', 'Expert'];

/** FR-RCP-001 / FR-SRCH-002..004 – tham số `GET /recipes`. Sắp xếp là CẶP `sortBy` + `sortOrder` (MT-01, D-10). */
export interface RecipeListParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: string;
  maxCookTime?: number;
  maxPrepTime?: number;
  minServings?: number;
  sortBy?: SortBy;
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

/**
 * Một kết quả tìm kiếm = RecipeSummary + `relevanceScore` (ts_rank). Kế hoạch ghi `RecipeSearchResult = PagedResult<RecipeSummary>`
 * nhưng backend (`RecipeSearchResultDto`) và SRS §8.3 trả thêm `relevanceScore` cho từng phần tử nên kiểu phải có trường này.
 */
export interface RecipeSearchHit extends RecipeSummary {
  relevanceScore: number;
}

export type RecipeSearchResult = PagedResult<RecipeSearchHit>;

/** Trạng thái lọc/sắp xếp/phân trang đã parse từ URL (xem `features/recipes/search-params.ts`). */
export interface RecipeQueryState {
  q?: string;
  page: number;
  categoryId?: string;
  difficulty?: RecipeDifficulty;
  maxCookTime?: number;
  maxPrepTime?: number;
  minServings?: number;
  sortBy?: SortBy;
  sortOrder?: SortOrder;
}
