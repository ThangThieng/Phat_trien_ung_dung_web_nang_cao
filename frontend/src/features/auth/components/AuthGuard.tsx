'use client';

import { useEffect, type ReactNode } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import LoadingSkeleton from '@/components/shared/LoadingSkeleton';
import { useAuth } from '../auth-context';

/** Client navigation guard; authorization remains enforced by the backend. */
export function AuthGuard({ children }: { children: ReactNode }) {
  const { isAuthenticated, isReady } = useAuth();
  const pathname = usePathname();
  const router = useRouter();

  useEffect(() => {
    if (!isReady || isAuthenticated) return;
    const callbackUrl = `${pathname}${window.location.search}`;
    router.replace(`/auth/login?callbackUrl=${encodeURIComponent(callbackUrl)}`);
  }, [isAuthenticated, isReady, pathname, router]);

  if (!isReady || !isAuthenticated) return <LoadingSkeleton variant="table" label="Đang kiểm tra phiên đăng nhập…" />;
  return children;
}
