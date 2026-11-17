import type { Metadata } from 'next';
import AuthCard from '@/features/auth/components/AuthCard';
import ForgotPasswordForm from '@/features/auth/components/ForgotPasswordForm';

export const metadata: Metadata = { title: 'Quên mật khẩu', robots: { index: false } };

export default function ForgotPasswordPage() {
  return <AuthCard title="Quên mật khẩu" subtitle="Nhập email để nhận liên kết đặt lại."><ForgotPasswordForm /></AuthCard>;
}
