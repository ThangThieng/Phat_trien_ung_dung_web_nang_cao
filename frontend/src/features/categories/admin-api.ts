import { apiFetch } from '@/lib/api-client';
import type { Category } from '@/types/api';

/**
 * FR-CAT-003/004/005 – lời gọi API GHI cho trang quản trị (Bearer token, không cache).
 * Tách khỏi api.ts: file đó phục vụ trang công khai với next.revalidate, không dùng cho mutation.
 */

/** Query key dùng chung cho dữ liệu danh mục phía client – trùng tên với cache tag "categories" ở api.ts. */
export const CATEGORIES_QUERY_KEY = ['categories'] as const;

/** Body dùng chung cho POST và PUT – SRS §8.2. */
export interface CategoryPayload {
  name: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
}

/** Danh sách cho màn quản trị: luôn lấy bản mới nhất, không dùng Data Cache của Next.js. */
export function getCategoriesForAdmin(signal?: AbortSignal) {
  return apiFetch<Category[]>('/categories', { cache: 'no-store', signal });
}

export function createCategory(payload: CategoryPayload, accessToken: string | null) {
  return apiFetch<Category>('/categories', { method: 'POST', body: payload, accessToken });
}

export function updateCategory(id: string, payload: CategoryPayload, accessToken: string | null) {
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
