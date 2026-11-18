'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import '@/lib/zod-locale';
import { toast } from 'sonner';
import { KeyRound, Loader2, LogOut, MonitorSmartphone, UserRound } from 'lucide-react';
import FormField from '@/components/shared/FormField';
import { apiFetch, getErrorMessage, mapProblemDetailsToForm } from '@/lib/api-client';
import { useAuth } from '@/features/auth/auth-context';
import { useAuthStoreApi } from '@/stores/auth-store-provider';
import { authKeys } from '@/features/auth/query-keys';
import { registerSchema } from '@/features/auth/schemas';
import type { Session, User } from '@/types/auth';
import ImageUploader from '@/components/shared/ImageUploader';
import { initialsOf } from '@/components/layout/UserMenu';
import type { UploadedFile } from '@/types/api';

type Tab = 'Thông tin' | 'Bảo mật' | 'Phiên';
const tabs: Tab[] = ['Thông tin', 'Bảo mật', 'Phiên'];
const tabIcons = { 'Thông tin': UserRound, 'Bảo mật': KeyRound, Phiên: MonitorSmartphone } as const;

const profileSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(2, 'Tên hiển thị cần ít nhất 2 ký tự.')
    .max(100, 'Tên hiển thị tối đa 100 ký tự.'),
  bio: z.string().max(1000, 'Giới thiệu tối đa 1000 ký tự.'),
});
const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Nhập mật khẩu hiện tại.'),
    newPassword: registerSchema.shape.password,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu xác nhận không khớp.',
  });

const deleteFile = (file: UploadedFile) =>
  apiFetch(`/files/${file.key.split('/').map(encodeURIComponent).join('/')}`, {
    method: 'DELETE',
  }).catch(() => undefined);

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });

const primaryButton =
  'inline-flex items-center justify-center gap-2 rounded-lg bg-navy px-4 py-2 text-sm font-semibold text-white hover:bg-navy-2 disabled:opacity-60';
const dangerButton =
  'inline-flex items-center justify-center gap-1 rounded-md px-3 py-1.5 text-sm font-medium text-red-700 hover:bg-red-50';
const card = 'rounded-xl border border-line bg-paper p-5 sm:p-6';

export function ProfilePage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const store = useAuthStoreApi();
  const { logout, accessToken } = useAuth();
  const [tab, setTab] = useState<Tab>('Thông tin');
  const [message, setMessage] = useState('');
  const [newAvatar, setNewAvatar] = useState<UploadedFile | null>(null);
  const [avatarBeforeUpload, setAvatarBeforeUpload] = useState<User | null>(null);
  const info = useForm<z.infer<typeof profileSchema>>({ resolver: zodResolver(profileSchema) });
  const security = useForm<z.infer<typeof passwordSchema>>({
    resolver: zodResolver(passwordSchema),
  });
  const me = useQuery({
    queryKey: authKeys.me(),
    queryFn: () => apiFetch<User>('/auth/me'),
    meta: { suppressGlobalError: true },
  });
  const sessionsQuery = useQuery({
    queryKey: authKeys.sessions(),
    queryFn: () => apiFetch<Session[]>('/auth/sessions'),
    enabled: tab === 'Phiên',
    meta: { suppressGlobalError: true },
  });

  useEffect(() => {
    if (me.data) info.reset({ displayName: me.data.displayName, bio: me.data.bio ?? '' });
  }, [info, me.data]);
  useEffect(() => {
    if (me.error || sessionsQuery.error)
      setMessage(getErrorMessage(me.error ?? sessionsQuery.error));
  }, [me.error, sessionsQuery.error]);

  const changeAvatar = (file: UploadedFile | null) => {
    setNewAvatar(file);
    if (file && me.data) {
      setAvatarBeforeUpload((current) => current ?? me.data!);
      const next = { ...me.data, avatarUrl: file.url };
      queryClient.setQueryData(authKeys.me(), next);
      store.getState().setUser(next);
    }
  };

  const save = info.handleSubmit(async (values) => {
    const previous = avatarBeforeUpload ?? me.data;
    if (!previous) return;
    const next = {
      ...previous,
      displayName: values.displayName.trim(),
      bio: values.bio.trim() || null,
      avatarUrl: newAvatar?.url ?? previous.avatarUrl,
    };
    queryClient.setQueryData(authKeys.me(), next);
    store.getState().setUser(next);
    try {
      const updated = await apiFetch<User>('/auth/me', {
        method: 'PATCH',
        body: { displayName: next.displayName, bio: next.bio, avatarUrl: next.avatarUrl ?? '' },
      });
      queryClient.setQueryData(authKeys.me(), updated);
      store.getState().setUser(updated);
      setNewAvatar(null);
      setAvatarBeforeUpload(null);
      setMessage('Đã lưu hồ sơ.');
    } catch (error) {
      queryClient.setQueryData(authKeys.me(), previous);
      store.getState().setUser(previous);
      if (newAvatar) await deleteFile(newAvatar);
      if (!mapProblemDetailsToForm(error, ['displayName', 'bio'], info.setError))
        setMessage(getErrorMessage(error));
    }
  });

  const change = security.handleSubmit(async (values) => {
    try {
      await apiFetch('/auth/change-password', {
        method: 'POST',
        body: { currentPassword: values.currentPassword, newPassword: values.newPassword },
      });
      security.reset();
      const text = 'Đã đổi mật khẩu. Các thiết bị khác đã bị đăng xuất.';
      toast.success(text);
      setMessage(text);
    } catch (error) {
      if (!mapProblemDetailsToForm(error, ['currentPassword', 'newPassword'], security.setError))
        security.setError('root', { message: getErrorMessage(error) });
    }
  });

  const revoke = async (s: Session) => {
    if (!window.confirm('Bạn có chắc muốn thu hồi phiên này?')) return;
    try {
      await apiFetch(`/auth/sessions/${s.id}`, { method: 'DELETE' });
      if (s.isCurrent) {
        await logout();
        router.replace('/auth/login');
      } else queryClient.invalidateQueries({ queryKey: authKeys.sessions() });
    } catch (error) {
      setMessage(getErrorMessage(error));
    }
  };

  const revokeAll = async () => {
    if (!window.confirm('Bạn có chắc muốn thu hồi tất cả phiên?')) return;
    try {
      await apiFetch('/auth/sessions/revoke-all', { method: 'POST' });
      await logout();
      router.replace('/auth/login');
    } catch (error) {
      setMessage(getErrorMessage(error));
    }
  };

  const onKey = (e: React.KeyboardEvent<HTMLButtonElement>, i: number) => {
    if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
      e.preventDefault();
      setTab(tabs[(i + (e.key === 'ArrowRight' ? 1 : tabs.length - 1)) % tabs.length]);
    }
  };

  const bioError = info.formState.errors.bio?.message;
  const sessions = sessionsQuery.data ?? [];

  return (
    <main className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-ink text-2xl font-bold">Hồ sơ</h1>
      <p className="text-ink-soft mt-1 text-sm">
        Cập nhật thông tin cá nhân, đổi mật khẩu và quản lý các thiết bị đang đăng nhập.
      </p>

      {me.data && (
        <div className={`${card} mt-6 flex items-center gap-4`}>
          {me.data.avatarUrl ? (
            // eslint-disable-next-line @next/next/no-img-element -- ảnh đại diện nhỏ, có thể vừa tải lên MinIO
            <img
              src={me.data.avatarUrl}
              alt=""
              className="border-line h-16 w-16 shrink-0 rounded-full border object-cover"
            />
          ) : (
            <span
              aria-hidden
              className="bg-gold text-navy flex h-16 w-16 shrink-0 items-center justify-center rounded-full text-lg font-bold"
            >
              {initialsOf(me.data.displayName)}
            </span>
          )}
          <div className="min-w-0">
            <p className="font-heading text-ink truncate text-lg font-semibold">
              {me.data.displayName}
            </p>
            <p className="text-ink-soft truncate text-sm">{me.data.email}</p>
          </div>
        </div>
      )}

      <div
        className="border-line bg-paper mt-6 inline-flex gap-1 rounded-xl border p-1"
        role="tablist"
        aria-label="Hồ sơ"
      >
        {tabs.map((item, i) => {
          const Icon = tabIcons[item];
          const active = tab === item;
          return (
            <button
              id={`profile-tab-${i}`}
              key={item}
              type="button"
              role="tab"
              aria-selected={active}
              aria-controls={`profile-panel-${i}`}
              tabIndex={active ? 0 : -1}
              onKeyDown={(e) => onKey(e, i)}
              onClick={() => setTab(item)}
              className={`focus-visible:ring-gold-ink inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition outline-none focus-visible:ring-2 ${
                active
                  ? 'bg-navy text-white shadow-sm'
                  : 'text-ink-soft hover:bg-gold-soft hover:text-ink'
              }`}
            >
              <Icon aria-hidden className="h-4 w-4" />
              {item}
            </button>
          );
        })}
      </div>

      {message && (
        <p
          role="status"
          className="border-line bg-gold-soft text-ink mt-4 rounded-lg border px-4 py-3 text-sm"
        >
          {message}
        </p>
      )}

      {tab === 'Thông tin' && (
        <section
          role="tabpanel"
          id="profile-panel-0"
          aria-labelledby="profile-tab-0"
          className={`${card} mt-4`}
        >
          <form onSubmit={save} className="flex flex-col gap-5">
            <div className="max-w-sm">
              <ImageUploader
                accessToken={accessToken}
                value={newAvatar}
                onChange={changeAvatar}
                deleteOnRemove={false}
                label="Ảnh đại diện mới"
              />
            </div>
            <FormField
              label="Tên hiển thị"
              error={info.formState.errors.displayName?.message}
              {...info.register('displayName')}
            />
            <div className="flex flex-col gap-1">
              <label
                htmlFor="profile-bio"
                className="text-ink flex flex-col gap-1 text-sm font-medium"
              >
                Tiểu sử
                <textarea
                  id="profile-bio"
                  rows={4}
                  aria-invalid={bioError ? true : undefined}
                  aria-describedby={bioError ? 'profile-bio-error' : 'profile-bio-hint'}
                  className={`focus:ring-gold-ink rounded-lg border px-3 py-2 text-sm shadow-sm transition outline-none focus:ring-2 ${
                    bioError ? 'border-red-500' : 'border-line-strong'
                  }`}
                  {...info.register('bio')}
                />
              </label>
              {bioError ? (
                <p id="profile-bio-error" role="alert" className="text-sm text-red-600">
                  {bioError}
                </p>
              ) : (
                <p id="profile-bio-hint" className="text-ink-soft text-xs">
                  Tối đa 1000 ký tự.
                </p>
              )}
            </div>
            <div className="flex justify-end">
              <button
                type="submit"
                disabled={info.formState.isSubmitting}
                className={primaryButton}
              >
                {info.formState.isSubmitting && (
                  <Loader2 aria-hidden className="h-4 w-4 animate-spin" />
                )}
                Lưu thay đổi
              </button>
            </div>
          </form>
        </section>
      )}

      {tab === 'Bảo mật' && (
        <section
          role="tabpanel"
          id="profile-panel-1"
          aria-labelledby="profile-tab-1"
          className={`${card} mt-4`}
        >
          <h2 className="font-heading text-ink text-lg font-semibold">Mật khẩu</h2>
          <p className="text-ink-soft mt-1 text-sm">
            Sau khi đổi, các thiết bị khác sẽ bị đăng xuất; thiết bị này vẫn giữ đăng nhập.
          </p>
          <form onSubmit={change} className="mt-5 flex flex-col gap-4">
            {security.formState.errors.root && (
              <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
                {security.formState.errors.root.message}
              </p>
            )}
            <FormField
              label="Mật khẩu hiện tại"
              type="password"
              autoComplete="current-password"
              error={security.formState.errors.currentPassword?.message}
              {...security.register('currentPassword')}
            />
            <FormField
              label="Mật khẩu mới"
              type="password"
              autoComplete="new-password"
              error={security.formState.errors.newPassword?.message}
              {...security.register('newPassword')}
            />
            <FormField
              label="Xác nhận mật khẩu mới"
              type="password"
              autoComplete="new-password"
              error={security.formState.errors.confirmPassword?.message}
              {...security.register('confirmPassword')}
            />
            <div className="flex justify-end">
              <button
                type="submit"
                disabled={security.formState.isSubmitting}
                className={primaryButton}
              >
                {security.formState.isSubmitting && (
                  <Loader2 aria-hidden className="h-4 w-4 animate-spin" />
                )}
                Đổi mật khẩu
              </button>
            </div>
          </form>
        </section>
      )}

      {tab === 'Phiên' && (
        <section
          role="tabpanel"
          id="profile-panel-2"
          aria-labelledby="profile-tab-2"
          className={`${card} mt-4`}
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h2 className="font-heading text-ink text-lg font-semibold">Phiên đăng nhập</h2>
              <p className="text-ink-soft mt-1 text-sm">
                Các thiết bị đang đăng nhập bằng tài khoản này.
              </p>
            </div>
            <button
              type="button"
              onClick={revokeAll}
              className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm font-medium text-red-700 hover:bg-red-50"
            >
              <LogOut aria-hidden className="h-4 w-4" />
              Thu hồi tất cả
            </button>
          </div>

          {sessionsQuery.isPending && (
            <p className="text-ink-soft mt-5 text-sm">Đang tải danh sách phiên…</p>
          )}
          {sessionsQuery.isSuccess && sessions.length === 0 && (
            <p className="text-ink-soft mt-5 text-sm">Không có phiên nào đang hoạt động.</p>
          )}
          {sessions.length > 0 && (
            <ul className="divide-line border-line mt-5 divide-y overflow-hidden rounded-lg border">
              {sessions.map((s) => (
                <li
                  key={s.id}
                  className="bg-cream/40 flex flex-wrap items-center justify-between gap-3 px-4 py-3"
                >
                  <div className="flex items-start gap-3">
                    <MonitorSmartphone
                      aria-hidden
                      className="text-gold-ink mt-0.5 h-5 w-5 shrink-0"
                    />
                    <div className="text-sm">
                      <p className="text-ink font-medium">
                        {s.isCurrent && (
                          <span className="bg-gold-soft text-gold-ink mr-2 rounded-full px-2 py-0.5 text-xs font-semibold">
                            Thiết bị này
                          </span>
                        )}
                        IP: {s.createdByIp ?? 'Không rõ'}
                      </p>
                      <p className="text-ink-soft">
                        Tạo: {formatDateTime(s.createdAt)} · Hết hạn: {formatDateTime(s.expiresAt)}
                      </p>
                    </div>
                  </div>
                  <button type="button" onClick={() => revoke(s)} className={dangerButton}>
                    Thu hồi
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}
    </main>
  );
}
