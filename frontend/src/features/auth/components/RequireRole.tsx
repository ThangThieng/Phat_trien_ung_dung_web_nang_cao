'use client';

import type { ReactNode } from 'react';
import Link from 'next/link';
import { ShieldAlert } from 'lucide-react';
import type { Role } from '@/types/auth';
import { useAuth } from '../auth-context';

export function RequireRole({ roles, children }: { roles: readonly Role[]; children: ReactNode }) {
  const { user } = useAuth();
  if (user?.roles.some((role) => roles.includes(role))) return children;
  return (
    <div className="mx-auto max-w-xl px-4 py-16 text-center">
      <ShieldAlert aria-hidden className="mx-auto h-10 w-10 text-orange-700" />
      <h1 className="mt-4 text-2xl font-bold text-gray-900">Không có quyền truy cập</h1>
      <p className="mt-2 text-gray-600">Trang này chỉ dành cho quản trị viên.</p>
      <Link href="/" className="mt-6 inline-block text-orange-700 underline">Về trang chủ</Link>
    </div>
  );
}
