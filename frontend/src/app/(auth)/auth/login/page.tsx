import type { Metadata } from 'next';
import { Suspense } from 'react';
import LoginForm from '@/features/auth/components/LoginForm';
import AuthCard from '@/features/auth/components/AuthCard';

export const metadata: Metadata = {
  title: 'Đăng nhập',
  robots: { index: false },
};

export default function LoginPage() {
  return (
    <AuthCard title="Đăng nhập" subtitle="Tiếp tục chia sẻ công thức nấu ăn của bạn.">
      <Suspense>
        <LoginForm />
      </Suspense>
    </AuthCard>
  );
}
