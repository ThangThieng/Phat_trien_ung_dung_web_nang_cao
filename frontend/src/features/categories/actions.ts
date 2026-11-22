'use server';

import { revalidateTag } from 'next/cache';

/**
 * D-7: Data Cache của Next.js giữ /categories 30 phút và /categories/[slug] 2 phút (features/categories/api.ts, tag "categories").
 * Backend đã xoá cache Redis ngay khi Admin tạo/sửa/xoá danh mục, nhưng Data Cache của Next.js thì không tự xoá
 * → trang công khai hiện dữ liệu cũ. Server Action này xoá mọi bản cache gắn tag "categories" để trang công khai đổi ngay.
 * Chỉ làm mới cache, không đọc hay sửa dữ liệu.
 */
export async function revalidateCategories(): Promise<void> {
  revalidateTag('categories');
}
