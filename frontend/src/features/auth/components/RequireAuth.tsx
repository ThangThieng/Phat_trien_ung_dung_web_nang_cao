'use client';

import type { ReactNode } from 'react';
import { useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { useAuth } from '@/features/auth/auth-context';

/** UI guard only: API authorization remains the security boundary. */
export default function RequireAuth({ children, roles }: { children: ReactNode; roles?: readonly string[] }) {
  const { user, isReady, isAuthenticated } = useAuth();
  const pathname = usePathname();
  const router = useRouter();

  useEffect(() => {
    if (isReady && !isAuthenticated) {
      router.replace(`/auth/login?callbackUrl=${encodeURIComponent(pathname)}`);
    }
  }, [isReady, isAuthenticated, pathname, router]);

  if (!isReady || !isAuthenticated) {
    return <p role="status" className="px-4 py-10 text-center">Đang kiểm tra phiên đăng nhập…</p>;
  }
  if (roles && !roles.some((role) => user?.roles.includes(role))) {
    return <p role="alert" className="px-4 py-10 text-center">Bạn không có quyền truy cập trang này.</p>;
  }
  return children;
}
