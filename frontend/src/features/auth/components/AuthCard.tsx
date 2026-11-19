'use client';

import { useEffect, type ReactNode } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { toSafeInternalPath } from '@/lib/safe-redirect';
import { useAuth } from '../auth-context';

interface AuthCardProps {
  title: string;
  subtitle: string;
  children: ReactNode;
}

/** Khung chung cho /auth/login và /auth/register – đã đăng nhập thì chuyển hướng (SRS §5.1). */
export default function AuthCard({ title, subtitle, children }: AuthCardProps) {
  const { isAuthenticated, isReady } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (isReady && isAuthenticated) {
      router.replace(
        toSafeInternalPath(new URLSearchParams(window.location.search).get('callbackUrl')),
      );
    }
  }, [isReady, isAuthenticated, router]);

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6 px-4 py-12">
      <div className="text-center">
        <h1 className="text-ink text-2xl font-bold">{title}</h1>
        <p className="text-ink-soft mt-1 text-sm">{subtitle}</p>
      </div>
      <div className="border-line bg-paper rounded-2xl border p-6 shadow-sm">{children}</div>
      {/* SRS §2.3: khách xem, tìm kiếm công thức không cần tài khoản — tài khoản chỉ cần để viết công thức. */}
      <p className="text-ink-soft text-center text-sm">
        Chỉ muốn xem công thức?{' '}
        <Link href="/" className="text-gold-ink font-semibold underline-offset-4 hover:underline">
          Tiếp tục với tư cách khách
        </Link>
      </p>
    </div>
  );
}
