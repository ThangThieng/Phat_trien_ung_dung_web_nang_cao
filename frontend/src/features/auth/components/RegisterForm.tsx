'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import FormField from '@/components/ui/FormField';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import { useAuth } from '../auth-context';
import { registerSchema, type RegisterFormValues } from '../schemas';

const FIELDS = ['fullName', 'email', 'userName', 'password'] as const;
type ServerField = (typeof FIELDS)[number];

const isServerField = (field: string): field is ServerField =>
  (FIELDS as readonly string[]).includes(field);

export default function RegisterForm() {
  const { register: registerAccount } = useAuth();
  const router = useRouter();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });

  const onSubmit = handleSubmit(async ({ confirmPassword: _confirm, ...values }) => {
    try {
      const user = await registerAccount(values);
      toast.success(`Đăng ký thành công! Chào mừng ${user.fullName}. Email chào mừng đã được gửi.`);
      router.replace('/recipes');
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.code === 'AUTH_EMAIL_EXISTS') {
          setError('email', { message: error.message });
          return;
        }
        if (error.code === 'AUTH_USERNAME_EXISTS') {
          setError('userName', { message: error.message });
          return;
        }
        if (error.status === 422) {
          Object.entries(error.fieldErrors).forEach(([field, messages]) => {
            if (isServerField(field)) setError(field, { message: messages[0] });
          });
          return;
        }
      }
      setError('root', { message: getErrorMessage(error) });
    }
  });

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      {errors.root && (
        <div
          role="alert"
          className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
        >
          {errors.root.message}
        </div>
      )}
      <FormField
        label="Họ và tên"
        autoComplete="name"
        error={errors.fullName?.message}
        {...register('fullName')}
      />
      <FormField
        label="Email"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register('email')}
      />
      <FormField
        label="Tên đăng nhập"
        autoComplete="username"
        hint="3–50 ký tự: chữ, số, dấu chấm, gạch dưới."
        error={errors.userName?.message}
        {...register('userName')}
      />
      <FormField
        label="Mật khẩu"
        type="password"
        autoComplete="new-password"
        hint="Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt."
        error={errors.password?.message}
        {...register('password')}
      />
      <FormField
        label="Xác nhận mật khẩu"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register('confirmPassword')}
      />
      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-lg bg-orange-600 px-4 py-2.5 font-semibold text-white transition hover:bg-orange-700 focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 disabled:opacity-60"
      >
        {isSubmitting ? 'Đang tạo tài khoản…' : 'Đăng ký'}
      </button>
      <p className="text-center text-sm text-gray-600">
        Đã có tài khoản?{' '}
        <Link href="/auth/login" className="font-medium text-orange-700 hover:underline">
          Đăng nhập
        </Link>
      </p>
    </form>
  );
}
