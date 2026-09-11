'use client';

import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import FormField from '@/components/ui/FormField';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import { useAuth } from '../auth-context';
import { loginSchema, type LoginFormValues } from '../schemas';

/** Chỉ cho phép redirect nội bộ (chống open redirect). */
function safeCallbackUrl(value: string | null): string {
  return value && value.startsWith('/') && !value.startsWith('//') ? value : '/recipes';
}

export default function LoginForm() {
  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = handleSubmit(async (values) => {
    try {
      const user = await login(values.email, values.password);
      toast.success(`Chào mừng trở lại, ${user.fullName}!`);
      router.replace(safeCallbackUrl(searchParams.get('callbackUrl')));
    } catch (error) {
      if (error instanceof ApiError && error.status === 422) {
        Object.entries(error.fieldErrors).forEach(([field, messages]) => {
          if (field === 'email' || field === 'password') setError(field, { message: messages[0] });
        });
        return;
      }
      // 401 / 403 / 423 – hiển thị message từ Problem Details
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
        label="Email"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register('email')}
      />
      <FormField
        label="Mật khẩu"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
        {...register('password')}
      />
      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-lg bg-orange-600 px-4 py-2.5 font-semibold text-white transition hover:bg-orange-700 focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 disabled:opacity-60"
      >
        {isSubmitting ? 'Đang đăng nhập…' : 'Đăng nhập'}
      </button>
      <p className="text-center text-sm text-gray-600">
        Chưa có tài khoản?{' '}
        <Link href="/auth/register" className="font-medium text-orange-700 hover:underline">
          Đăng ký ngay
        </Link>
      </p>
    </form>
  );
}
