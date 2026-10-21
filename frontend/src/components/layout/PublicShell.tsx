import type { ReactNode } from 'react';
import { getCategories } from '@/features/categories/api';
import type { Category } from '@/types/category';
import Footer from './Footer';
import Header from './Header';

/**
 * Khung của trang công khai: `Header` + `<main id="main-content">` + `Footer` (Buổi 6 — Dev 3 thay `SiteHeader` và khối
 * `<footer>` tạm). Dùng ở `app/(public)/layout.tsx` và `app/not-found.tsx` (404 nằm ngoài route group nhưng vẫn cần khung).
 *
 * Danh mục được lấy MỘT lần ở server rồi truyền cho cả header lẫn footer — Data Cache `revalidate: 1800` (SRS §5.1 / bảng TTL
 * `categories:all` 30 phút). API lỗi (Backend đang khởi động, build Docker không có API) → mảng rỗng: menu danh mục trên
 * header tự tải lại ở trình duyệt, khung trang không bao giờ hỏng vì danh mục.
 */
export default async function PublicShell({ children }: { children: ReactNode }) {
  const categories = await getCategories().catch((): Category[] => []);
  return (
    <>
      <Header categories={categories} />
      <main id="main-content" className="flex-1">
        {children}
      </main>
      <Footer categories={categories} />
    </>
  );
}
