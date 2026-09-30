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

/**
 * MT-01 / D-10: backend Buổi 4 chỉ nhận `sortBy` + `sortOrder` (tham số `sort` cũ → 400).
 * Cầu nối tối thiểu cho trang /recipes hiện tại ("-createdAt" → sortBy=createdAt&sortOrder=desc) để hệ thống vẫn chạy cuối
 * Buổi 4; bộ lọc/sắp xếp đầy đủ phía giao diện (FilterPanel, D-10 FE) làm ở Buổi 5 — Dev 3.
 */
function toSortParams(sort: string): [string, string][] {
  const descending = sort.startsWith('-');
  return [
    ['sortBy', descending ? sort.slice(1) : sort],
    ['sortOrder', descending ? 'desc' : 'asc'],
  ];
}

function toQuery(params: RecipeListParams): string {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    if (key === 'sort') {
      toSortParams(String(value)).forEach(([name, text]) => query.set(name, text));
    } else {
      query.set(key, String(value));
    }
  });
  const text = query.toString();
  return text ? `?${text}` : '';
}

/** FR-RCP-001 – GET /recipes (SSR dynamic: không cache ở Next; backend cache Redis 2 phút — NFR-PERF-003). */
export function getRecipes(params: RecipeListParams = {}) {
  return apiFetch<PagedResult<RecipeSummary>>(`/recipes${toQuery(params)}`, { cache: 'no-store' });
}

/** FR-RCP-002 – GET /recipes/{slug} (ISR revalidate 300s – SRS §5.1). */
export function getRecipeBySlug(slug: string) {
  return apiFetch<RecipeDetail>(`/recipes/${encodeURIComponent(slug)}`, {
    next: { revalidate: 300 },
  });
}
