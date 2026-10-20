'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { InterimPageLoading } from '@/components/layout/interim-shell';
import { toSafeInternalPath } from '@/lib/safe-redirect';
import { useAuth } from '../auth-context';

export function GuestOnly({ children }: { children: ReactNode }) {
  const { isAuthenticated, isReady } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isReady || !isAuthenticated) return;
    router.replace(toSafeInternalPath(new URLSearchParams(window.location.search).get('callbackUrl')));
  }, [isAuthenticated, isReady, router]);

  if (!isReady || isAuthenticated) return <InterimPageLoading label="Đang kiểm tra phiên đăng nhập…" />;
  return children;
}
