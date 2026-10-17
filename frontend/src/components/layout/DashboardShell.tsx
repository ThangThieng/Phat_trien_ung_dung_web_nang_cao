'use client';

import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ChefHat, Menu, X } from 'lucide-react';
import { useAuth } from '@/features/auth/auth-context';
import HealthIndicator from '@/features/system/components/HealthIndicator';
import DashboardSidebar from './DashboardSidebar';
import { InterimAuthGuard } from './interim-shell';
import UserMenu from './UserMenu';

const MOBILE_NAV_ID = 'dashboard-mobile-nav';

function DashboardFrame({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const pathname = usePathname();
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const roles = user?.roles ?? [];

  // Đổi trang (kể cả bấm Back) thì đóng menu điện thoại.
  useEffect(() => setMobileNavOpen(false), [pathname]);

  // Escape đóng menu như một ngăn kéo thật.
  useEffect(() => {
    if (!mobileNavOpen) return undefined;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setMobileNavOpen(false);
    };
    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, [mobileNavOpen]);

  return (
    <div className="flex flex-1">
      <aside className="hidden w-60 shrink-0 border-r border-orange-100 bg-white md:block">
        <DashboardSidebar roles={roles} />
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b border-orange-100 bg-white/90 px-4 backdrop-blur">
          {/* Bản tạm: nút mở/đóng danh sách; Dev 2 có `Sheet` (shadcn) thì thay bằng ngăn kéo trượt. */}
          <button
            type="button"
            className="rounded-md p-2 text-gray-700 hover:bg-orange-50 md:hidden"
            aria-expanded={mobileNavOpen}
            aria-controls={MOBILE_NAV_ID}
            aria-label={mobileNavOpen ? 'Đóng menu' : 'Mở menu'}
            onClick={() => setMobileNavOpen((open) => !open)}
          >
            {mobileNavOpen ? (
              <X aria-hidden className="h-5 w-5" />
            ) : (
              <Menu aria-hidden className="h-5 w-5" />
            )}
          </button>
          <Link href="/" className="flex items-center gap-2 font-bold text-orange-700">
            <ChefHat aria-hidden className="h-5 w-5" />
            Culinary Blog
          </Link>
          <div className="ml-auto flex items-center gap-2">
            {/* FR-OBS-001: chỉ Admin thấy (và chỉ khi đó mới poll /api/health) */}
            {roles.includes('Admin') && <HealthIndicator />}
            <UserMenu />
          </div>
        </header>

        {/* Lớp phủ cố định ngay dưới thanh trên cùng, không chèn vào luồng trang: chèn vào luồng thì trình duyệt giữ
            nguyên vị trí nội dung (scroll anchoring) bằng cách cuộn xuống đúng chiều cao menu — bấm mở mà không thấy menu. */}
        {mobileNavOpen && (
          <div
            id={MOBILE_NAV_ID}
            className="fixed inset-x-0 top-14 bottom-0 z-40 overflow-y-auto border-t border-orange-100 bg-white md:hidden"
          >
            <DashboardSidebar roles={roles} onNavigate={() => setMobileNavOpen(false)} />
          </div>
        )}

        <main id="main-content" className="flex-1">
          {children}
        </main>
      </div>
    </div>
  );
}

/** Khung chung của mọi trang `(dashboard)`: chặn khi chưa đăng nhập, sidebar theo vai trò, thanh trên cùng. */
export default function DashboardShell({ children }: { children: ReactNode }) {
  return (
    <InterimAuthGuard>
      <DashboardFrame>{children}</DashboardFrame>
    </InterimAuthGuard>
  );
}
