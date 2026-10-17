import { apiFetch } from '@/lib/api-client';
import type { Category, CategoryDetail, CategoryDetailParams } from '@/types/category';
import { DEFAULT_PAGE_SIZE } from '@/types/search';

/**
 * FR-CAT-001 – Data Cache `revalidate: 1800` (SRS §5.1 `/categories`; ≤ TTL Redis `categories:all` 30 phút — NFR-PERF-003).
 * Trước Buổi 6 là 3600 (lệch D-7); header/footer công khai dùng hàm này từ Buổi 6 nên sửa giá trị ngay tại đây.
 */
export function getCategories() {
  return apiFetch<Category[]>('/categories', { next: { revalidate: 1800, tags: ['categories'] } });
}

/**
 * FR-CAT-002 – SSR + Data Cache `revalidate: 120` (SRS §5.1 / MT-57: thời gian tái sinh ≤ TTL cache API
 * `categories:detail` = 2 phút). SRS §8.2: chỉ nhận page, pageSize, sortBy, sortOrder.
 */
export function getCategoryBySlug(slug: string, params: CategoryDetailParams = {}) {
  const query = new URLSearchParams({
    page: String(params.page ?? 1),
    pageSize: String(params.pageSize ?? DEFAULT_PAGE_SIZE),
  });
  if (params.sortBy) query.set('sortBy', params.sortBy);
  if (params.sortOrder) query.set('sortOrder', params.sortOrder);

  return apiFetch<CategoryDetail>(`/categories/${encodeURIComponent(slug)}?${query.toString()}`, {
    next: { revalidate: 120, tags: ['categories', `category:${slug}`] },
  });
}
