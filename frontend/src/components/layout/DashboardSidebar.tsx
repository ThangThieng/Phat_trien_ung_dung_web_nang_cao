'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  Activity,
  BookOpen,
  FolderTree,
  LayoutDashboard,
  PenSquare,
  UserRound,
  Users,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { Role } from '@/types/auth';

export interface DashboardNavItem {
  href: string;
  label: string;
  icon: LucideIcon;
}

/** SRS §5.1 — trang riêng tư của mọi người dùng đã đăng nhập. */
export const AUTHOR_NAV: readonly DashboardNavItem[] = [
  { href: '/dashboard', label: 'Tổng quan', icon: LayoutDashboard },
  { href: '/dashboard/recipes', label: 'Công thức của tôi', icon: BookOpen },
  { href: '/dashboard/recipes/new', label: 'Viết công thức', icon: PenSquare },
  { href: '/profile', label: 'Hồ sơ', icon: UserRound },
];

/** SRS §5.1 — chỉ Admin. Ẩn mục chỉ là trải nghiệm; Backend vẫn trả 403 cho Author (NFR-SEC-006). */
export const ADMIN_NAV: readonly DashboardNavItem[] = [
  { href: '/dashboard/categories', label: 'Danh mục', icon: FolderTree },
  { href: '/dashboard/users', label: 'Người dùng', icon: Users },
  { href: '/dashboard/system', label: 'Hệ thống', icon: Activity },
];

const matches = (pathname: string, href: string) =>
  pathname === href || pathname.startsWith(`${href}/`);

/** Mục khớp dài nhất thắng: ở `/dashboard/recipes/new` chỉ "Viết công thức" sáng, không sáng cả "Công thức của tôi". */
export function activeNavHref(
  pathname: string,
  items: readonly DashboardNavItem[],
): string | undefined {
  return items
    .filter((item) => matches(pathname, item.href))
    .sort((a, b) => b.href.length - a.href.length)[0]?.href;
}

interface DashboardSidebarProps {
  roles: readonly Role[];
  /** Đóng menu trên điện thoại sau khi chọn trang. */
  onNavigate?: () => void;
}

export default function DashboardSidebar({ roles, onNavigate }: DashboardSidebarProps) {
  const pathname = usePathname();
  const isAdmin = roles.includes('Admin');
  const items = isAdmin ? [...AUTHOR_NAV, ...ADMIN_NAV] : AUTHOR_NAV;
  const active = activeNavHref(pathname, items);

  const renderItem = (item: DashboardNavItem) => {
    const Icon = item.icon;
    const current = item.href === active;
    return (
      <li key={item.href}>
        <Link
          href={item.href}
          onClick={onNavigate}
          aria-current={current ? 'page' : undefined}
          className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium ${
            current
              ? 'bg-orange-50 text-orange-800'
              : 'text-gray-700 hover:bg-gray-50 hover:text-orange-800'
          }`}
        >
          <Icon aria-hidden className="h-4 w-4 shrink-0" />
          {item.label}
        </Link>
      </li>
    );
  };

  return (
    <nav aria-label="Bảng điều khiển" className="flex flex-col gap-6 p-4">
      <ul className="space-y-1">{AUTHOR_NAV.map(renderItem)}</ul>
      {isAdmin && (
        <div>
          <p
            id="dashboard-admin-nav"
            className="px-3 text-xs font-semibold tracking-wide text-gray-500 uppercase"
          >
            Quản trị
          </p>
          <ul aria-labelledby="dashboard-admin-nav" className="mt-2 space-y-1">
            {ADMIN_NAV.map(renderItem)}
          </ul>
        </div>
      )}
    </nav>
  );
}
