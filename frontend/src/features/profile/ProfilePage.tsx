'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import FormField from '@/components/shared/FormField';
import { apiFetch, getErrorMessage, mapProblemDetailsToForm } from '@/lib/api-client';
import { useAuth } from '@/features/auth/auth-context';
import { registerSchema } from '@/features/auth/schemas';
import type { Session, User } from '@/types/auth';
import ImageUploader from '@/components/shared/ImageUploader';
import type { UploadedFile } from '@/types/api';

type Tab = 'Thông tin' | 'Bảo mật' | 'Phiên';
const profileSchema = z.object({ displayName: z.string().trim().min(2).max(100), bio: z.string().max(500), avatarUrl: z.string().url().or(z.literal('')) });
const passwordSchema = z.object({ currentPassword: z.string().min(1), newPassword: registerSchema.shape.password, confirmPassword: z.string() }).refine(v => v.newPassword === v.confirmPassword, { path: ['confirmPassword'], message: 'Mật khẩu xác nhận không khớp.' });

export function ProfilePage() {
  const router = useRouter(); const { logout, accessToken } = useAuth();
  const [tab, setTab] = useState<Tab>('Thông tin'); const [sessions, setSessions] = useState<Session[]>([]); const [message, setMessage] = useState('');
  const info = useForm<z.infer<typeof profileSchema>>({ resolver: zodResolver(profileSchema) });
  const [newAvatar, setNewAvatar] = useState<UploadedFile | null>(null);
  const security = useForm<z.infer<typeof passwordSchema>>({ resolver: zodResolver(passwordSchema) });
  useEffect(() => { apiFetch<User>('/auth/me').then(u => info.reset({ displayName: u.displayName, bio: u.bio ?? '', avatarUrl: u.avatarUrl ?? '' })).catch(e => setMessage(getErrorMessage(e))); }, [info]);
  const loadSessions = () => apiFetch<Session[]>('/auth/sessions').then(setSessions).catch(e => setMessage(getErrorMessage(e)));
  useEffect(() => { if (tab === 'Phiên') loadSessions(); }, [tab]);
  const save = info.handleSubmit(async values => { try { await apiFetch('/auth/me', { method: 'PATCH', body: { ...values, avatarUrl: newAvatar?.url ?? values.avatarUrl } }); setMessage('Đã lưu hồ sơ.'); } catch (e) { if (newAvatar) await apiFetch(`/files/${newAvatar.key.split('/').map(encodeURIComponent).join('/')}`, { method: 'DELETE' }).catch(() => undefined); if (!mapProblemDetailsToForm(e, ['displayName', 'bio', 'avatarUrl'], info.setError)) setMessage(getErrorMessage(e)); } });
  const change = security.handleSubmit(async values => { try { await apiFetch('/auth/change-password', { method: 'POST', body: { currentPassword: values.currentPassword, newPassword: values.newPassword } }); await logout(); router.replace('/auth/login'); } catch (e) { if (!mapProblemDetailsToForm(e, ['currentPassword', 'newPassword'], security.setError)) security.setError('root', { message: getErrorMessage(e) }); } });
  const revoke = async (session: Session) => { try { await apiFetch(`/auth/sessions/${session.id}`, { method: 'DELETE' }); if (session.isCurrent) { await logout(); router.replace('/auth/login'); } else loadSessions(); } catch (e) { setMessage(getErrorMessage(e)); } };
  const revokeAll = async () => { try { await apiFetch('/auth/sessions/revoke-all', { method: 'POST' }); await logout(); router.replace('/auth/login'); } catch (e) { setMessage(getErrorMessage(e)); } };
  return <main className="mx-auto max-w-2xl p-6"><h1 className="text-2xl font-bold">Hồ sơ</h1><div className="my-4 flex gap-2">{(['Thông tin', 'Bảo mật', 'Phiên'] as Tab[]).map(item => <button key={item} type="button" onClick={() => setTab(item)}>{item}</button>)}</div>{message && <p role="status">{message}</p>}{tab === 'Thông tin' && <form onSubmit={save}><ImageUploader accessToken={accessToken} value={newAvatar} onChange={setNewAvatar} deleteOnRemove={false} /><FormField label="Tên hiển thị" error={info.formState.errors.displayName?.message} {...info.register('displayName')} /><FormField label="Tiểu sử" error={info.formState.errors.bio?.message} {...info.register('bio')} /><FormField label="URL ảnh đại diện" error={info.formState.errors.avatarUrl?.message} {...info.register('avatarUrl')} /><button type="submit" disabled={info.formState.isSubmitting}>Lưu thay đổi</button></form>}{tab === 'Bảo mật' && <form onSubmit={change}>{security.formState.errors.root && <p role="alert">{security.formState.errors.root.message}</p>}<FormField label="Mật khẩu hiện tại" type="password" error={security.formState.errors.currentPassword?.message} {...security.register('currentPassword')} /><FormField label="Mật khẩu mới" type="password" error={security.formState.errors.newPassword?.message} {...security.register('newPassword')} /><FormField label="Xác nhận mật khẩu mới" type="password" error={security.formState.errors.confirmPassword?.message} {...security.register('confirmPassword')} /><button type="submit" disabled={security.formState.isSubmitting}>Đổi mật khẩu</button></form>}{tab === 'Phiên' && <section>{sessions.map(s => <div key={s.id}><span>{new Date(s.createdAt).toLocaleString()} {s.createdByIp ?? ''} {s.isCurrent ? '(Phiên hiện tại)' : ''}</span><button type="button" onClick={() => revoke(s)}>Thu hồi</button></div>)}<button type="button" onClick={revokeAll}>Thu hồi tất cả</button></section>}</main>;
}
