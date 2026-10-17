import { apiFetch } from '@/lib/api-client';
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from '@/types/category';

/**
 * FR-CAT-003/004/005 – lời gọi API GHI cho trang quản trị (Bearer token, không cache).
 * Tách khỏi api.ts: file đó phục vụ trang công khai với next.revalidate, không dùng cho mutation.
 */

/** Query key dùng chung cho dữ liệu danh mục phía client – trùng tên với cache tag "categories" ở api.ts. */
export const CATEGORIES_QUERY_KEY = ['categories'] as const;

/** Danh sách cho màn quản trị: luôn lấy bản mới nhất, không dùng Data Cache của Next.js. */
export function getCategoriesForAdmin(signal?: AbortSignal) {
  return apiFetch<Category[]>('/categories', { cache: 'no-store', signal });
}

export function createCategory(payload: CreateCategoryRequest, accessToken: string | null) {
  return apiFetch<Category>('/categories', { method: 'POST', body: payload, accessToken });
}

export function updateCategory(
  id: string,
  payload: UpdateCategoryRequest,
  accessToken: string | null,
) {
  return apiFetch<Category>(`/categories/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: payload,
    accessToken,
  });
}

export function deleteCategory(id: string, accessToken: string | null) {
  return apiFetch<void>(`/categories/${encodeURIComponent(id)}`, {
    method: 'DELETE',
    accessToken,
  });
}
