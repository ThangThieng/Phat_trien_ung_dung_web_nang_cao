'use client';

import { useEffect } from 'react';
import type { ReactNode } from 'react';
import { useRouter } from 'next/navigation';
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
    if (isReady && isAuthenticated) router.replace('/recipes');
  }, [isReady, isAuthenticated, router]);

  return (
    <div className="mx-auto flex w-full max-w-md flex-col gap-6 px-4 py-12">
      <div className="text-center">
        <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
        <p className="mt-1 text-sm text-gray-600">{subtitle}</p>
      </div>
      <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">{children}</div>
    </div>
  );
}
