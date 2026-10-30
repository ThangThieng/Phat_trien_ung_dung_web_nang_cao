'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import FormField from '@/components/shared/FormField';
import { apiFetch, getErrorMessage } from '@/lib/api-client';
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../schemas';

export default function ForgotPasswordForm() {
  const [sent, setSent] = useState(false);
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<ForgotPasswordFormValues>({ resolver: zodResolver(forgotPasswordSchema) });
  const submit = handleSubmit(async ({ email }) => { try { await apiFetch('/auth/forgot-password', { method: 'POST', body: { email }, accessToken: null }); setSent(true); } catch (error) { setError('root', { message: getErrorMessage(error) }); } });
  if (sent) return <p role="status" className="rounded-lg bg-green-50 p-3 text-sm text-green-800">Nếu email hợp lệ, liên kết đặt lại mật khẩu đã được gửi.</p>;
  return <form onSubmit={submit} noValidate className="flex flex-col gap-4">{errors.root && <p role="alert">{errors.root.message}</p>}<FormField label="Email" type="email" autoComplete="email" error={errors.email?.message} {...register('email')} /><button type="submit" disabled={isSubmitting} className="rounded-lg bg-orange-600 px-4 py-2.5 font-semibold text-white disabled:opacity-60">{isSubmitting ? 'Đang gửi…' : 'Gửi liên kết đặt lại'}</button></form>;
}
