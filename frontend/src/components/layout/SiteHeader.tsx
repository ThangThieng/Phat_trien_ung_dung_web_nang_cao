'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ChefHat, LogOut, ImageUp } from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';

const NAV = [
  { href: '/recipes', label: 'Công thức' },
  { href: '/categories', label: 'Danh mục' },
];

export default function SiteHeader() {
  const pathname = usePathname();
  const { user, isReady, logout } = useAuth();

  const handleLogout = () => {
    logout();
    toast.info('Bạn đã đăng xuất.');
  };

  return (
    <header className="sticky top-0 z-40 border-b border-orange-100 bg-white/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Link href="/" className="flex items-center gap-2 text-lg font-bold text-orange-700">
          <ChefHat aria-hidden className="h-6 w-6" />
          Culinary Blog
        </Link>

        <nav aria-label="Điều hướng chính" className="flex items-center gap-1 sm:gap-4">
          {NAV.map((item) => {
            const active = pathname.startsWith(item.href);
            return (
              <Link
                key={item.href}
                href={item.href}
                aria-current={active ? 'page' : undefined}
                className={`rounded-md px-2 py-1 text-sm font-medium ${
                  active ? 'text-orange-700' : 'text-gray-700 hover:text-orange-700'
                }`}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="flex min-w-[9rem] items-center justify-end gap-2">
          {isReady && user && (
            <>
              <Link
                href="/dashboard/media"
                className="hidden items-center gap-1 rounded-md px-2 py-1 text-sm text-gray-700 hover:text-orange-700 sm:flex"
              >
                <ImageUp aria-hidden className="h-4 w-4" />
                Thư viện ảnh
              </Link>
              <span
                className="hidden text-sm font-medium text-gray-900 md:inline"
                title={user.email}
              >
                {user.fullName}
              </span>
              <button
                type="button"
                onClick={handleLogout}
                aria-label="Đăng xuất"
                className="rounded-md p-2 text-gray-600 hover:bg-orange-50 hover:text-orange-700"
              >
                <LogOut aria-hidden className="h-4 w-4" />
              </button>
            </>
          )}
          {isReady && !user && (
            <>
              <Link
                href="/auth/login"
                className="rounded-md px-3 py-1.5 text-sm font-medium text-gray-700 hover:text-orange-700"
              >
                Đăng nhập
              </Link>
              <Link
                href="/auth/register"
                className="rounded-md bg-orange-600 px-3 py-1.5 text-sm font-semibold text-white hover:bg-orange-700"
              >
                Đăng ký
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
