import { apiFetch } from '@/lib/api-client';
import type { Category, CategoryDetail } from '@/types/api';
import type { SortField, SortOrder } from '@/features/recipes/search-params';

/** FR-CAT-001 – dữ liệu revalidate mỗi 3600s (SRS §5.1). Backend cache Redis 60 phút. */
export function getCategories() {
  return apiFetch<Category[]>('/categories', { next: { revalidate: 3600, tags: ['categories'] } });
}

export interface CategoryDetailParams {
  page?: number;
  pageSize?: number;
  sortBy?: SortField;
  sortOrder?: SortOrder;
}

/**
 * FR-CAT-002 – SSR + Data Cache `revalidate: 120` (SRS §5.1 / MT-57: thời gian tái sinh ≤ TTL cache API
 * `categories:detail` = 2 phút). SRS §8.2: chỉ nhận page, pageSize, sortBy, sortOrder.
 */
export function getCategoryBySlug(slug: string, params: CategoryDetailParams = {}) {
  const query = new URLSearchParams({
    page: String(params.page ?? 1),
    pageSize: String(params.pageSize ?? 12),
  });
  if (params.sortBy) query.set('sortBy', params.sortBy);
  if (params.sortOrder) query.set('sortOrder', params.sortOrder);

  return apiFetch<CategoryDetail>(`/categories/${encodeURIComponent(slug)}?${query.toString()}`, {
    next: { revalidate: 120, tags: ['categories', `category:${slug}`] },
  });
}
