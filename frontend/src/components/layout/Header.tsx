import { Suspense } from 'react';
import Link from 'next/link';
import { ChefHat, Search } from 'lucide-react';
import CategoryNav from '@/features/categories/components/CategoryNav';
import SearchBar from '@/features/recipes/components/SearchBar';
import type { Category } from '@/types/category';
import HeaderActions from './HeaderActions';
import MobileNav from './MobileNav';
import NavLink from './NavLink';

const NAV_LINK =
  'rounded-md px-2 py-1 text-sm font-medium whitespace-nowrap text-gray-700 hover:text-orange-700';

/**
 * Header công khai (Server Component — thay `SiteHeader` của Buổi 2). Phần lớn header render ở server và KHÔNG gửi
 * JavaScript; chỉ các mảnh tương tác nhỏ là Client Component: `NavLink` (aria-current), `CategoryNav` (menu thả xuống),
 * `SearchBar` (debounce), `HeaderActions` + `UserMenu` (phiên đăng nhập), `MobileNav` (menu điện thoại).
 * Danh sách danh mục do `PublicShell` lấy MỘT lần ở server (`getCategories()`, Data Cache `revalidate: 1800` — SRS §5.1) rồi
 * truyền cho cả header lẫn footer.
 */
export default function Header({ categories }: { categories: Category[] }) {
  return (
    <header className="sticky top-0 z-40 border-b border-orange-100 bg-white/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center gap-2 px-4 sm:gap-4">
        <MobileNav categories={categories} />

        <Link
          href="/"
          aria-label="Culinary Blog – Trang chủ"
          className="flex shrink-0 items-center gap-2 text-lg font-bold text-orange-700"
        >
          <ChefHat aria-hidden className="h-6 w-6" />
          {/* 320 px: chỉ còn biểu tượng để đủ chỗ cho menu, tìm kiếm và nút đăng nhập/đăng ký (NFR-USE-001). */}
          <span className="hidden sm:inline">Culinary Blog</span>
        </Link>

        <nav aria-label="Điều hướng chính" className="hidden items-center gap-2 md:flex lg:gap-4">
          <NavLink href="/recipes" className={NAV_LINK} activeClassName="text-orange-700">
            Công thức
          </NavLink>
          <CategoryNav initialCategories={categories} />
        </nav>

        <div className="ml-auto flex items-center justify-end gap-2">
          {/* FR-SRCH-001: SearchBar đọc useSearchParams → cần Suspense để trang tĩnh vẫn build được.
              Ô tìm kiếm chỉ hiện từ 1024 px: ở 768 px đặt cùng menu + nút đăng nhập/đăng ký thì chữ bị xuống dòng (đã chụp). */}
          <Suspense fallback={<span aria-hidden className="hidden h-8 w-64 lg:block" />}>
            <SearchBar className="hidden lg:block" />
          </Suspense>
          <Link
            href="/search"
            aria-label="Tìm kiếm"
            className="rounded-md p-2 text-gray-600 hover:bg-orange-50 hover:text-orange-700 lg:hidden"
          >
            <Search aria-hidden className="h-5 w-5" />
          </Link>
          <HeaderActions />
        </div>
      </div>
    </header>
  );
}
