'use client';

import { useCallback, useEffect, useId, useRef, useState } from 'react';
import type { KeyboardEvent as ReactKeyboardEvent } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  ChevronDown,
  FolderTree,
  ImageUp,
  LayoutDashboard,
  LogOut,
  UserRound,
  Users,
  type LucideIcon,
} from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';

interface MenuLink {
  href: string;
  label: string;
  icon: LucideIcon;
}

const USER_LINKS: readonly MenuLink[] = [
  { href: '/profile', label: 'Hồ sơ', icon: UserRound },
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
];

/** Chỉ Admin. Ẩn mục chỉ là trải nghiệm — Backend vẫn trả 403 cho Author (NFR-SEC-006). */
const ADMIN_LINKS: readonly MenuLink[] = [
  { href: '/dashboard/categories', label: 'Quản lý danh mục', icon: FolderTree },
  { href: '/dashboard/users', label: 'Người dùng', icon: Users },
  // Có trong `SiteHeader` cũ (Buổi 2 – 6); trang tạm, xóa ở Buổi 8 khi uploader nằm trong biểu mẫu thật.
  { href: '/dashboard/media', label: 'Thư viện ảnh', icon: ImageUp },
];

/** Hai chữ cái đầu của tên hiển thị — "Nguyễn Văn Lan" → "NL". */
export function initialsOf(displayName: string): string {
  const words = displayName.trim().split(/\s+/).filter(Boolean);
  if (words.length === 0) return '?';
  const first = words[0][0];
  const last = words.length > 1 ? words[words.length - 1][0] : '';
  return `${first}${last}`.toLocaleUpperCase('vi');
}

const ITEM =
  'flex w-full items-center gap-3 px-4 py-2 text-left text-sm text-gray-800 hover:bg-orange-50 hover:text-orange-800 focus:bg-orange-50 focus:text-orange-800 focus:outline-none';

/**
 * Menu tài khoản trên thanh trên cùng (header công khai + thanh trên của dashboard). Đọc `useAuth()` — chữ ký giữ nguyên khi
 * Dev 1 chuyển phần bên trong sang Zustand. Thay `InterimUserMenu` (bản tạm của Dev 4).
 *
 * - Khách: link Đăng nhập / Đăng ký.
 * - Đã đăng nhập: nút avatar mở menu: Hồ sơ, Dashboard, (Admin) Quản lý danh mục, Người dùng, Đăng xuất.
 * - Đang khôi phục phiên: ô giữ chỗ cùng kích thước — không nhảy bố cục khi phiên về (CLS).
 *
 * Mẫu WAI-ARIA "menu button": `aria-haspopup="menu"`, mũi tên lên/xuống/Home/End di chuyển giữa các mục, Escape đóng và trả
 * focus về nút, bấm ra ngoài / Tab / đổi trang đều đóng. Khi Dev 2 merge shadcn/ui, phần khung này có thể thay bằng
 * `DropdownMenu` (Radix) mà không đổi chỗ dùng.
 */
export default function UserMenu() {
  const { user, isReady, logout } = useAuth();
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLUListElement>(null);
  const menuId = useId();

  const items = useCallback(
    () => Array.from(menuRef.current?.querySelectorAll<HTMLElement>('[role="menuitem"]') ?? []),
    [],
  );

  const close = useCallback((returnFocus: boolean) => {
    setOpen(false);
    if (returnFocus) buttonRef.current?.focus();
  }, []);

  // Đổi trang (kể cả Back) thì đóng menu.
  useEffect(() => setOpen(false), [pathname]);

  useEffect(() => {
    if (!open) return undefined;
    items()[0]?.focus();
    const onPointerDown = (event: MouseEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', onPointerDown);
    return () => document.removeEventListener('mousedown', onPointerDown);
  }, [open, items]);

  if (!isReady) {
    return <span aria-hidden className="block h-9 w-24 animate-pulse rounded-full bg-gray-100" />;
  }

  if (!user) {
    return (
      <div className="flex items-center gap-1">
        <Link
          href="/auth/login"
          className="rounded-md px-3 py-1.5 text-sm font-medium whitespace-nowrap text-gray-700 hover:text-orange-700"
        >
          Đăng nhập
        </Link>
        <Link
          href="/auth/register"
          // 320 px: chỉ giữ "Đăng nhập" (trang đăng nhập có link đăng ký; menu điện thoại cũng có).
          className="hidden rounded-md bg-orange-700 px-3 py-1.5 text-sm font-semibold whitespace-nowrap text-white hover:bg-orange-800 sm:inline-block"
        >
          Đăng ký
        </Link>
      </div>
    );
  }

  const isAdmin = user.roles.includes('Admin');
  const links = isAdmin ? [...USER_LINKS, ...ADMIN_LINKS] : USER_LINKS;

  const handleLogout = async () => {
    setOpen(false);
    await logout();
    toast.info('Bạn đã đăng xuất.');
  };

  const onMenuKeyDown = (event: ReactKeyboardEvent<HTMLUListElement>) => {
    const all = items();
    const index = all.indexOf(document.activeElement as HTMLElement);
    const focusAt = (i: number) => all[(i + all.length) % all.length]?.focus();
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        focusAt(index + 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        focusAt(index - 1);
        break;
      case 'Home':
        event.preventDefault();
        focusAt(0);
        break;
      case 'End':
        event.preventDefault();
        focusAt(all.length - 1);
        break;
      case 'Escape':
        event.preventDefault();
        close(true);
        break;
      case 'Tab':
        close(false);
        break;
      default:
    }
  };

  return (
    <div ref={containerRef} className="relative">
      <button
        ref={buttonRef}
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={open ? menuId : undefined}
        aria-label={`Tài khoản: ${user.displayName}`}
        onClick={() => setOpen((value) => !value)}
        onKeyDown={(event) => {
          if (event.key === 'ArrowDown' && !open) {
            event.preventDefault();
            setOpen(true);
          }
        }}
        className="flex items-center gap-2 rounded-full py-1 pr-2 pl-1 text-sm font-medium text-gray-800 hover:bg-orange-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-700"
      >
        <span
          aria-hidden
          className="flex h-8 w-8 items-center justify-center rounded-full bg-orange-700 text-xs font-semibold text-white"
        >
          {initialsOf(user.displayName)}
        </span>
        <span className="hidden max-w-[10rem] truncate md:inline">{user.displayName}</span>
        <ChevronDown aria-hidden className={`h-4 w-4 transition ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <ul
          ref={menuRef}
          id={menuId}
          role="menu"
          aria-label="Tài khoản"
          onKeyDown={onMenuKeyDown}
          className="absolute top-full right-0 z-50 mt-2 w-60 overflow-hidden rounded-xl border border-orange-100 bg-white py-1 shadow-lg"
        >
          <li role="presentation" className="border-b border-gray-100 px-4 py-2">
            <p className="truncate text-sm font-semibold text-gray-900">{user.displayName}</p>
            <p className="truncate text-xs text-gray-600">{user.email}</p>
          </li>
          {links.map(({ href, label, icon: Icon }) => (
            <li key={href} role="presentation">
              <Link
                href={href}
                role="menuitem"
                tabIndex={-1}
                onClick={() => setOpen(false)}
                className={ITEM}
              >
                <Icon aria-hidden className="h-4 w-4 shrink-0" />
                {label}
              </Link>
            </li>
          ))}
          <li role="presentation" className="mt-1 border-t border-gray-100 pt-1">
            <button
              type="button"
              role="menuitem"
              tabIndex={-1}
              onClick={handleLogout}
              className={ITEM}
            >
              <LogOut aria-hidden className="h-4 w-4 shrink-0" />
              Đăng xuất
            </button>
          </li>
        </ul>
      )}
    </div>
  );
}
