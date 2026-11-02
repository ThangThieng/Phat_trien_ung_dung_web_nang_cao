import type { RecipeSearchParams } from '@/types/search';

/**
 * Query key của tìm kiếm (FR-SRCH-001). Khóa chứa ĐỦ tham số gửi lên API — hai bộ lọc khác nhau không bao giờ dùng chung một
 * mục cache (cùng nguyên tắc với khóa `search:{queryHash}` ở Redis). Kết quả tìm kiếm là dữ liệu công khai nên nằm ngoài nhánh
 * `'private'` của `recipeKeys`.
 */
export const searchKeys = {
  all: ['search'] as const,
  results: (params: RecipeSearchParams) => ['search', 'results', params] as const,
};

/** = TTL `search:{queryHash}` 1 phút (NFR-PERF-003): trình duyệt không giữ kết quả lâu hơn backend. */
export const SEARCH_STALE_TIME_MS = 60_000;

export const SEARCH_PATHNAME = '/search';
