'use client';

import Link from 'next/link';
import { PenSquare } from 'lucide-react';
import { useAuth } from '@/features/auth/auth-context';
import HealthIndicator from '@/features/system/components/HealthIndicator';
import UserMenu from './UserMenu';

/**
 * Phần phụ thuộc phiên đăng nhập của `Header` (Client Component nhỏ — phần còn lại của header render ở server):
 * nút "Viết công thức" (đã đăng nhập), chấm sức khỏe hệ thống (chỉ Admin — FR-OBS-001, chỉ khi đó mới poll `/api/health`),
 * và `UserMenu`.
 */
export default function HeaderActions() {
  const { user, isReady } = useAuth();
  const signedIn = isReady && user !== null;

  return (
    <div className="flex items-center gap-2">
      {signedIn && (
        <Link
          href="/dashboard/recipes/new"
          className="hidden items-center gap-1 rounded-md px-2 py-1 text-sm text-gray-700 hover:text-orange-700 lg:flex"
        >
          <PenSquare aria-hidden className="h-4 w-4" />
          Viết công thức
        </Link>
      )}
      {isReady && user?.roles.includes('Admin') && <HealthIndicator />}
      <UserMenu />
    </div>
  );
}
