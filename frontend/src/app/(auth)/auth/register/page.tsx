import type { Metadata } from 'next';
import RegisterForm from '@/features/auth/components/RegisterForm';
import AuthCard from '@/features/auth/components/AuthCard';

export const metadata: Metadata = {
  title: 'Đăng ký',
  robots: { index: false },
};

export default function RegisterPage() {
  return (
    <AuthCard
      title="Tạo tài khoản tác giả"
      subtitle="Đăng ký miễn phí để đăng tải công thức của riêng bạn."
    >
      <RegisterForm />
    </AuthCard>
  );
}
