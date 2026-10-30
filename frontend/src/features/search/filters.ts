import type { Category } from '@/types/category';
import type { RecipeQueryState } from '@/types/search';
import { DIFFICULTY_LABEL, formatMinutes } from '@/features/recipes/format';

/**
 * Bộ lọc đa tiêu chí của `/search` (FR-SRCH-002 + CR-2026-05 / MT-66). Ba tiêu chí thời gian/khẩu phần dùng các mốc cố định
 * — người dùng chọn một nút, không phải gõ số. URL gõ tay với giá trị ngoài mốc (ví dụ `maxCookTime=45`) vẫn hợp lệ: API lọc
 * đúng giá trị đó, nhãn bộ lọc đang bật vẫn hiện, chỉ là không nút mốc nào được đánh dấu.
 */
export const COOK_TIME_OPTIONS = [15, 30, 60, 120] as const;
export const PREP_TIME_OPTIONS = [15, 30, 60] as const;
export const SERVINGS_OPTIONS = [1, 2, 4, 6] as const;

/** Radix Select không cho giá trị rỗng — "tất cả danh mục" dùng giá trị riêng, URL thì không có `categoryId`. */
export const ALL_CATEGORIES = 'all';

export type SearchFilterKey =
  'categoryId' | 'difficulty' | 'maxCookTime' | 'maxPrepTime' | 'minServings';

/** Thứ tự cố định của nhãn bộ lọc đang bật — cùng thứ tự với bảng bộ lọc. */
export const SEARCH_FILTER_KEYS: readonly SearchFilterKey[] = [
  'categoryId',
  'difficulty',
  'maxCookTime',
  'maxPrepTime',
  'minServings',
];

/** Gỡ mọi bộ lọc, giữ `q`, về trang 1. */
export const CLEAR_ALL_FILTERS: Partial<RecipeQueryState> = {
  categoryId: undefined,
  difficulty: undefined,
  maxCookTime: undefined,
  maxPrepTime: undefined,
  minServings: undefined,
  page: 1,
};

const minutes = (value: number) => (value > 0 ? formatMinutes(value) : '0 phút');

export const formatCookTimeLimit = (value: number) => `Nấu ≤ ${minutes(value)}`;
export const formatPrepTimeLimit = (value: number) => `Chuẩn bị ≤ ${minutes(value)}`;
export const formatServingsMin = (value: number) => `Từ ${value} khẩu phần`;

export interface ActiveFilter {
  key: SearchFilterKey;
  /** Nhãn hiển thị trên chip, ví dụ "Độ khó: Dễ", "Nấu ≤ 30 phút". */
  label: string;
}

/** Danh sách bộ lọc đang bật để hiện thành nhãn có nút gỡ (và liệt kê trong gợi ý khi không có kết quả). */
export function getActiveFilters(
  state: Partial<RecipeQueryState>,
  categories: readonly Category[],
): ActiveFilter[] {
  const labels: Record<SearchFilterKey, string | undefined> = {
    categoryId:
      state.categoryId === undefined
        ? undefined
        : `Danh mục: ${categories.find((c) => c.id === state.categoryId)?.name ?? 'đã chọn'}`,
    difficulty:
      state.difficulty === undefined ? undefined : `Độ khó: ${DIFFICULTY_LABEL[state.difficulty]}`,
    maxCookTime:
      state.maxCookTime === undefined ? undefined : formatCookTimeLimit(state.maxCookTime),
    maxPrepTime:
      state.maxPrepTime === undefined ? undefined : formatPrepTimeLimit(state.maxPrepTime),
    minServings: state.minServings === undefined ? undefined : formatServingsMin(state.minServings),
  };
  return SEARCH_FILTER_KEYS.flatMap((key) => {
    const label = labels[key];
    return label === undefined ? [] : [{ key, label }];
  });
}

/** "Hiển thị 13–24 trên 57 kết quả" (FR-SRCH-004) — tính từ `page`, `pageSize` và số phần tử thật của trang. */
export function formatResultRange(
  page: number,
  pageSize: number,
  itemCount: number,
  totalCount: number,
): string {
  if (itemCount === 0) return `0 trên ${totalCount} kết quả`;
  const from = (page - 1) * pageSize + 1;
  return `Hiển thị ${from}–${from + itemCount - 1} trên ${totalCount} kết quả`;
}

/** Vài danh mục nhiều công thức nhất — gợi ý khi không có kết quả. */
export function popularCategories(categories: readonly Category[], count = 4): Category[] {
  return [...categories]
    .filter((category) => category.recipeCount > 0)
    .sort((a, b) => b.recipeCount - a.recipeCount || a.name.localeCompare(b.name, 'vi'))
    .slice(0, count);
}
