import type { ReactNode } from 'react';
import Link from 'next/link';
import Footer from '@/components/layout/Footer';
import Header from '@/components/layout/Header';
import { mockCategories } from '@/mocks/fixtures/categories';
import { assertDevOnly } from './dev-only';

const PAGES = [
  { href: '/mockups/home', label: 'Trang chủ' },
  { href: '/mockups/recipe', label: 'Chi tiết công thức' },
  { href: '/mockups/category', label: 'Trang danh mục' },
  { href: '/mockups/search', label: 'Tìm kiếm' },
  { href: '/mockups/dashboard', label: 'Dashboard' },
];

/** Thanh chuyển giữa các mockup — chỉ có ở môi trường phát triển. */
export function MockupBar() {
  assertDevOnly();
  return (
    <div className="border-b border-dashed border-amber-300 bg-amber-50 px-4 py-2 text-xs text-amber-900">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-4 gap-y-1">
        <strong>MOCKUP (chỉ môi trường dev)</strong>
        {PAGES.map((page) => (
          <Link key={page.href} href={page.href} className="underline hover:text-amber-700">
            {page.label}
          </Link>
        ))}
      </div>
    </div>
  );
}

/** Khung công khai giống `PublicShell` nhưng danh mục lấy từ dữ liệu mẫu — không gọi API. */
export default function MockupShell({ children }: { children: ReactNode }) {
  assertDevOnly();
  return (
    <>
      <MockupBar />
      <Header categories={mockCategories} />
      <main id="main-content" className="flex-1">
        {children}
      </main>
      <Footer categories={mockCategories} />
    </>
  );
}
