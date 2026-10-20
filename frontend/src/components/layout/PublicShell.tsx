import type { ReactNode } from 'react';
import SiteHeader from './SiteHeader';

/**
 * Khung của trang công khai: header + `<main id="main-content">` + footer. Dùng ở `app/(public)/layout.tsx` và ở
 * `app/not-found.tsx` (trang 404 nằm ngoài mọi route group nhưng vẫn cần header/footer như trang công khai).
 * Dev 3 (Lab 6) thay `SiteHeader` bằng `Header` và khối `<footer>` bằng `Footer` — chỉ sửa ở file này.
 */
export default function PublicShell({ children }: { children: ReactNode }) {
  return (
    <>
      <SiteHeader />
      <main id="main-content" className="flex-1">
        {children}
      </main>
      <footer className="border-t border-orange-100 bg-white py-6 text-center text-sm text-gray-500">
        © {new Date().getFullYear()} Culinary Blog – Nhóm 20
      </footer>
    </>
  );
}
