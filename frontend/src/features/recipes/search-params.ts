import type { RecipeDifficulty } from '@/types/api';

/**
 * FR-SRCH-002/003/004 (Buổi 5 — Dev 3): TOÀN BỘ trạng thái lọc / sắp xếp / phân trang nằm trong `searchParams` của URL —
 * link chia sẻ được, back/forward và F5 giữ nguyên bộ lọc, SSR đọc được ngay. Mọi trang danh sách (/recipes, /search,
 * /categories/[slug]) và mọi component (FilterPanel, SortSelect, Pagination, SearchBar) dùng chung file này, nên không có
 * nơi nào tự parse hay tự ghép query string theo cách riêng (SRS §3.4: "một specification lọc dùng chung").
 */

/** Whitelist 5 cột sắp xếp (FR-SRCH-003) — khớp `SortMapper` phía backend. */
export const SORT_FIELDS = ['createdAt', 'publishedAt', 'title', 'cookTime', 'prepTime'] as const;
export type SortField = (typeof SORT_FIELDS)[number];
export type SortOrder = 'asc' | 'desc';
export const SORT_ORDERS: readonly SortOrder[] = ['asc', 'desc'];

/** Mặc định của SRS FR-SRCH-003: mới nhất trước. */
export const DEFAULT_SORT_BY: SortField = 'createdAt';
export const DEFAULT_SORT_ORDER: SortOrder = 'desc';

/** FR-SRCH-004: pageSize mặc định 12 (chia hết cho lưới 2/3/4 cột). */
export const DEFAULT_PAGE_SIZE = 12;

/** FR-SRCH-001: `q` tối thiểu 2 ký tự, dưới ngưỡng này API trả 400. */
export const MIN_SEARCH_LENGTH = 2;

/** FR-SRCH-002: 4 mức độ khó, gồm cả `Expert`. */
export const DIFFICULTIES: readonly RecipeDifficulty[] = ['Easy', 'Medium', 'Hard', 'Expert'];

export type RawSearchParams = Record<string, string | string[] | undefined>;

export interface RecipeQueryState {
  q?: string;
  page: number;
  categoryId?: string;
  difficulty?: RecipeDifficulty;
  maxCookTime?: number;
  maxPrepTime?: number;
  minServings?: number;
  sortBy?: SortField;
  sortOrder?: SortOrder;
}

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const MAX_MINUTES = 100_000;
const MAX_PAGE = 100_000;

const first = (value: string | string[] | undefined): string | undefined =>
  Array.isArray(value) ? value[0] : value;

/** Số nguyên không âm (>= `min`), giới hạn trên để không vượt kiểu `int` của backend; không hợp lệ → undefined. */
export function toOptionalInt(
  value: string | undefined,
  min = 0,
  max = MAX_MINUTES,
): number | undefined {
  if (value === undefined || !/^\d+$/.test(value.trim())) return undefined;
  const parsed = Number(value.trim());
  return Number.isSafeInteger(parsed) && parsed >= min && parsed <= max ? parsed : undefined;
}

function parsePage(value: string | undefined): number {
  return toOptionalInt(value, 1, MAX_PAGE) ?? 1;
}

function parseCategoryId(value: string | undefined): string | undefined {
  return value !== undefined && GUID_PATTERN.test(value) ? value : undefined;
}

function parseDifficulty(value: string | undefined): RecipeDifficulty | undefined {
  return DIFFICULTIES.find((d) => d.toLowerCase() === value?.toLowerCase());
}

function parseSortBy(value: string | undefined): SortField {
  return SORT_FIELDS.find((f) => f.toLowerCase() === value?.toLowerCase()) ?? DEFAULT_SORT_BY;
}

function parseSortOrder(value: string | undefined): SortOrder {
  return SORT_ORDERS.find((o) => o === value?.toLowerCase()) ?? DEFAULT_SORT_ORDER;
}

/**
 * /recipes — đủ 5 bộ lọc + sắp xếp + trang. Giá trị hỏng trong URL gõ tay được đưa về mặc định ở phía giao diện để người đọc
 * không rơi vào trang lỗi; phía API vẫn trả 400 cho client gọi trực tiếp (FR-SRCH-003), nên không có gì bị "nuốt" ở API.
 */
export function parseRecipeListState(raw: RawSearchParams): RecipeQueryState {
  return {
    page: parsePage(first(raw.page)),
    categoryId: parseCategoryId(first(raw.categoryId)),
    difficulty: parseDifficulty(first(raw.difficulty)),
    maxCookTime: toOptionalInt(first(raw.maxCookTime)),
    maxPrepTime: toOptionalInt(first(raw.maxPrepTime)),
    minServings: toOptionalInt(first(raw.minServings), 1),
    sortBy: parseSortBy(first(raw.sortBy)),
    sortOrder: parseSortOrder(first(raw.sortOrder)),
  };
}

/** /categories/[slug] — SRS §8.2 chỉ nhận `page`, `pageSize`, `sortBy`, `sortOrder`. */
export function parseCategoryState(raw: RawSearchParams): RecipeQueryState {
  return {
    page: parsePage(first(raw.page)),
    sortBy: parseSortBy(first(raw.sortBy)),
    sortOrder: parseSortOrder(first(raw.sortOrder)),
  };
}

/** /search — SRS §8.3 chỉ nhận `q`, `page`, `pageSize`, `categoryId`, `difficulty` (kết quả xếp theo `ts_rank`, không có sort). */
export function parseSearchState(raw: RawSearchParams): RecipeQueryState {
  return {
    q: first(raw.q)?.trim() ?? '',
    page: parsePage(first(raw.page)),
    categoryId: parseCategoryId(first(raw.categoryId)),
    difficulty: parseDifficulty(first(raw.difficulty)),
  };
}

/**
 * Ghép query string từ `state` + `overrides` (`undefined` trong overrides = xóa tham số). Bỏ qua giá trị mặc định
 * (trang 1, `createdAt`, `desc`) để URL ngắn và ổn định; thứ tự khóa cố định để cùng trạng thái luôn cho cùng URL.
 */
export function buildQueryString(
  state: Partial<RecipeQueryState>,
  overrides: Partial<RecipeQueryState> = {},
): string {
  const merged = { ...state, ...overrides };
  const query = new URLSearchParams();

  if (merged.q) query.set('q', merged.q);
  if (merged.categoryId) query.set('categoryId', merged.categoryId);
  if (merged.difficulty) query.set('difficulty', merged.difficulty);
  if (merged.maxCookTime !== undefined) query.set('maxCookTime', String(merged.maxCookTime));
  if (merged.maxPrepTime !== undefined) query.set('maxPrepTime', String(merged.maxPrepTime));
  if (merged.minServings !== undefined) query.set('minServings', String(merged.minServings));
  if (merged.sortBy && merged.sortBy !== DEFAULT_SORT_BY) query.set('sortBy', merged.sortBy);
  if (merged.sortOrder && merged.sortOrder !== DEFAULT_SORT_ORDER) {
    query.set('sortOrder', merged.sortOrder);
  }
  if (merged.page !== undefined && merged.page > 1) query.set('page', String(merged.page));

  return query.toString();
}

export function buildHref(
  pathname: string,
  state: Partial<RecipeQueryState>,
  overrides: Partial<RecipeQueryState> = {},
): string {
  const query = buildQueryString(state, overrides);
  return query ? `${pathname}?${query}` : pathname;
}

/** Số bộ lọc đang bật (không tính `q`, trang, sắp xếp) — hiện trên nút mở drawer. */
export function countActiveFilters(state: Partial<RecipeQueryState>): number {
  return [
    state.categoryId,
    state.difficulty,
    state.maxCookTime,
    state.maxPrepTime,
    state.minServings,
  ].filter((value) => value !== undefined).length;
}
