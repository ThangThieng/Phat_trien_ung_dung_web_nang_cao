'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import FormField from '@/components/shared/FormField';
import { apiFetch, getErrorMessage, mapProblemDetailsToForm } from '@/lib/api-client';
import Link from 'next/link';
import { toast } from 'sonner';
import { resetPasswordSchema, type ResetPasswordFormValues } from '../schemas';

const FIELDS = ['newPassword', 'confirmPassword'] as const;
export default function ResetPasswordForm() {
  const router = useRouter(); const params = useSearchParams(); const [credentials, setCredentials] = useState<{email:string;token:string}|null>(null);
  useEffect(() => { const email=params.get('email'); const token=params.get('token'); if (email && token) { setCredentials({email,token}); window.history.replaceState(null, '', '/auth/reset-password'); } }, [params]);
  const { register, handleSubmit, setError, formState:{errors,isSubmitting} } = useForm<ResetPasswordFormValues>({resolver:zodResolver(resetPasswordSchema)});
  const submit=handleSubmit(async ({newPassword})=>{ if(!credentials){setError('root',{message:'Liên kết đã hết hạn hoặc đã được dùng'});return;} try{await apiFetch('/auth/reset-password',{method:'POST',body:{...credentials,newPassword},accessToken:null});toast.success('Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.');router.replace('/auth/login');}catch(error){const detail=error as {code?:string};if(detail?.code==='AUTH_PASSWORD_RESET_TOKEN_INVALID'){setError('root',{message:'Liên kết đã hết hạn hoặc đã được dùng'});return;}if(mapProblemDetailsToForm(error,FIELDS,setError))return;setError('root',{message:getErrorMessage(error)});}});
  return <form onSubmit={submit} noValidate className="flex flex-col gap-4">{errors.root&&<p role="alert">{errors.root.message} <Link href="/auth/forgot-password" className="underline">Quên mật khẩu?</Link></p>}<FormField label="Mật khẩu mới" type="password" autoComplete="new-password" error={errors.newPassword?.message} {...register('newPassword')}/><FormField label="Xác nhận mật khẩu" type="password" autoComplete="new-password" error={errors.confirmPassword?.message} {...register('confirmPassword')}/><button type="submit" disabled={isSubmitting||!credentials} className="rounded-lg bg-orange-600 px-4 py-2.5 font-semibold text-white disabled:opacity-60">{isSubmitting?'Đang đặt lại…':'Đặt lại mật khẩu'}</button></form>;
}
