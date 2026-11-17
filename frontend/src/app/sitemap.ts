import type { MetadataRoute } from 'next';
import { getCategories } from '@/features/categories/api';
import { buildSitemapEntries, getSitemapRecipes } from '@/lib/seo/sitemap-source';

/*
 * NFR-SEO-003: sitemap do Next.js sinh, phục vụ ở đúng `https://domain/sitemap.xml` (nơi crawler tìm), không phải job Backend (MT-28).
 * Tần suất sinh lại = 1 giờ nhờ Data Cache của `getSitemapRecipes()` (xem lib/seo/sitemap-source.ts). Đặt `force-dynamic`
 * thay cho cấu hình `revalidate` cấp route vì `next build` của image Docker chạy khi API chưa có — cùng lý do ba trang công khai khác.
 * Slug Draft/Archived không bao giờ có mặt: endpoint chỉ trả Published (MT-34).
 */
export const dynamic = 'force-dynamic';

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [recipes, categories] = await Promise.all([
    getSitemapRecipes(), // lỗi thì để 500: sitemap rỗng âm thầm còn tệ hơn để crawler thử lại sau
    getCategories().catch(() => []),
  ]);
  return buildSitemapEntries({ recipes, categories });
}
