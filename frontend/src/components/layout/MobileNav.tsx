'use client';

import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { usePathname } from 'next/navigation';
import { Menu, Search, X } from 'lucide-react';
import { useAuth } from '@/features/auth/auth-context';
import type { Category } from '@/types/category';
import NavLink from './NavLink';

const PANEL_ID = 'site-mobile-nav';

const LINK =
  'block rounded-lg px-3 py-2 text-base font-medium text-gray-800 hover:bg-orange-50 hover:text-orange-800';
const ACTIVE = 'bg-orange-50 text-orange-800';

/**
 * Menu điện thoại của header công khai (< 768 px): nút ☰ mở ngăn phủ cố định ngay dưới header (top-16). Lớp phủ cố định chứ
 * không chèn vào luồng trang — chèn vào luồng thì trình duyệt giữ vị trí nội dung bằng cách cuộn xuống đúng chiều cao menu
 * (scroll anchoring) và người dùng bấm mở mà không thấy gì (Dev 4 đã đo ở dashboard, commit `c4807d3`). Escape, đổi trang
 * đều đóng.
 * Ngăn phủ gắn thẳng vào `document.body` (portal): header có `backdrop-blur`, mà `backdrop-filter` biến header thành khối
 * chứa của mọi con `position: fixed` — để trong header thì ngăn phủ bị giam trong 64 px của header và bị nội dung trang đè lên
 * (đã chụp ở 320 px). Khi Dev 2 merge shadcn/ui, có thể thay bằng `Sheet`.
 */
export default function MobileNav({ categories }: { categories: readonly Category[] }) {
  const [open, setOpen] = useState(false);
  // Mép dưới thật của header lúc mở — ngăn phủ bắt đầu ngay dưới đó, không che nút đóng (64 px khi header ở đỉnh trang).
  const [top, setTop] = useState(64);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const pathname = usePathname();
  const { isReady, user } = useAuth();

  useEffect(() => setOpen(false), [pathname]);

  useEffect(() => {
    if (!open) return undefined;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false);
    };
    document.addEventListener('keydown', onKeyDown);
    // Khóa cuộn trang nền khi menu mở trên điện thoại.
    const { overflow } = document.body.style;
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', onKeyDown);
      document.body.style.overflow = overflow;
    };
  }, [open]);

  const close = () => setOpen(false);

  return (
    <>
      <button
        ref={buttonRef}
        type="button"
        className="rounded-md p-2 text-gray-700 hover:bg-orange-50 md:hidden"
        aria-expanded={open}
        aria-controls={PANEL_ID}
        aria-label={open ? 'Đóng menu' : 'Mở menu'}
        onClick={() => {
          const header = buttonRef.current?.closest('header');
          if (header) setTop(Math.max(0, header.getBoundingClientRect().bottom));
          setOpen((value) => !value);
        }}
      >
        {open ? <X aria-hidden className="h-5 w-5" /> : <Menu aria-hidden className="h-5 w-5" />}
      </button>

      {open &&
        createPortal(
          <div
            id={PANEL_ID}
            style={{ top }}
            className="fixed inset-x-0 bottom-0 z-40 overflow-y-auto border-t border-orange-100 bg-white px-4 py-4 md:hidden"
          >
            {/* Form GET thuần: tìm được cả khi JavaScript chưa tải xong. */}
            <form action="/search" method="get" role="search" className="relative">
              <Search
                aria-hidden
                className="pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-500"
              />
              <input
                type="search"
                name="q"
                aria-label="Tìm công thức"
                placeholder="Tìm công thức…"
                autoComplete="off"
                className="w-full rounded-full border border-gray-300 bg-white py-2 pr-3 pl-9 text-base focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300"
              />
            </form>

            <nav aria-label="Điều hướng chính (điện thoại)" className="mt-4">
              <ul className="space-y-1">
                <li>
                  <NavLink href="/" exact className={LINK} activeClassName={ACTIVE} onClick={close}>
                    Trang chủ
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    href="/recipes"
                    className={LINK}
                    activeClassName={ACTIVE}
                    onClick={close}
                  >
                    Công thức
                  </NavLink>
                </li>
                <li>
                  <NavLink
                    href="/categories"
                    exact
                    className={LINK}
                    activeClassName={ACTIVE}
                    onClick={close}
                  >
                    Tất cả danh mục
                  </NavLink>
                </li>
              </ul>
            </nav>

            {categories.length > 0 && (
              <nav
                aria-label="Danh mục (điện thoại)"
                className="mt-4 border-t border-gray-100 pt-4"
              >
                <p className="px-3 text-xs font-semibold tracking-wide text-gray-600 uppercase">
                  Danh mục
                </p>
                <ul className="mt-2 grid grid-cols-2 gap-1">
                  {categories.map((category) => (
                    <li key={category.id}>
                      <NavLink
                        href={`/categories/${category.slug}`}
                        className="block truncate rounded-lg px-3 py-2 text-sm text-gray-700 hover:bg-orange-50 hover:text-orange-800"
                        activeClassName={ACTIVE}
                        onClick={close}
                      >
                        {category.name}
                      </NavLink>
                    </li>
                  ))}
                </ul>
              </nav>
            )}
            {isReady && !user && (
              <div className="mt-4 border-t border-gray-100 pt-4 sm:hidden">
                <NavLink
                  href="/auth/register"
                  className={LINK}
                  activeClassName={ACTIVE}
                  onClick={close}
                >
                  Đăng ký tài khoản
                </NavLink>
              </div>
            )}
          </div>,
          document.body,
        )}
    </>
  );
}
