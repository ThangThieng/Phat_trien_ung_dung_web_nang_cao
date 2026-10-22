import type { Metadata } from 'next';
import { Suspense } from 'react';
import AuthCard from '@/features/auth/components/AuthCard';
import ResetPasswordForm from '@/features/auth/components/ResetPasswordForm';

export const metadata: Metadata = { title: 'Đặt lại mật khẩu', robots: { index: false }, referrer: 'no-referrer' };

export default function ResetPasswordPage() {
  return <AuthCard title="Đặt lại mật khẩu" subtitle="Chọn mật khẩu mới cho tài khoản."><Suspense><ResetPasswordForm /></Suspense></AuthCard>;
}
