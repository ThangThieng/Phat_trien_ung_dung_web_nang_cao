import { apiFetch } from '@/lib/api-client';
import type { Category, CategoryDetail } from '@/types/api';

/** FR-CAT-001 – dữ liệu revalidate mỗi 3600s (SRS §5.1). Backend cache Redis 60 phút. */
export function getCategories() {
  return apiFetch<Category[]>('/categories', { next: { revalidate: 3600, tags: ['categories'] } });
}

/** FR-CAT-002 – dữ liệu revalidate mỗi 600s (SRS §5.1). */
export function getCategoryBySlug(slug: string, page = 1, pageSize = 12) {
  return apiFetch<CategoryDetail>(
    `/categories/${encodeURIComponent(slug)}?page=${page}&pageSize=${pageSize}`,
    { next: { revalidate: 600, tags: ['categories', `category:${slug}`] } },
  );
}
