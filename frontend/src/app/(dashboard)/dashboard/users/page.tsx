'use client';

/* eslint-disable @next/next/no-img-element -- Avatar URLs are served by configured media storage. */

import { useMemo, useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import Pagination from '@/components/shared/Pagination';
import { useDebouncedValue } from '@/hooks/use-debounced-value';
import { getErrorMessage } from '@/lib/api-client';
import { useAuth } from '@/features/auth/auth-context';
import { RequireRole } from '@/features/auth/components/RequireRole';
import { getUsers, setUserStatus } from '@/features/users/api';
import { userKeys } from '@/features/users/query-keys';
import type { UserAdmin } from '@/types/auth';

const PAGE_SIZE = 12;

function UsersPageContent() {
  const { user, accessToken } = useAuth();
  const queryClient = useQueryClient();
  const [searchInput, setSearchInput] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>();
  const [page, setPage] = useState(1);
  const [target, setTarget] = useState<UserAdmin | null>(null);
  const [reason, setReason] = useState('');
  const search = useDebouncedValue(searchInput, 300);
  const params = useMemo(() => ({ page, pageSize: PAGE_SIZE, search: search || undefined, isActive }), [page, search, isActive]);
  const users = useQuery({ queryKey: userKeys.list(params), queryFn: ({ signal }) => getUsers(params, signal), placeholderData: keepPreviousData, staleTime: 0, meta: { suppressGlobalError: true } });
  const mutation = useMutation({
    mutationFn: ({ targetUser, active, actionReason }: { targetUser: UserAdmin; active: boolean; actionReason: string }) => setUserStatus(targetUser.id, { isActive: active, reason: actionReason }, accessToken),
    onMutate: async ({ targetUser, active }) => {
      await queryClient.cancelQueries({ queryKey: userKeys.all });
      const snapshots = queryClient.getQueriesData({ queryKey: userKeys.all });
      queryClient.setQueriesData({ queryKey: userKeys.all }, (old: typeof users.data | undefined) => old ? { ...old, items: old.items.map((item) => item.id === targetUser.id ? { ...item, isActive: active } : item) } : old);
      return { snapshots };
    },
    onError: (error, _variables, context) => { context?.snapshots.forEach(([key, value]) => queryClient.setQueryData(key, value)); toast.error(getErrorMessage(error)); },
    onSuccess: (_result, { active }) => toast.success(active ? 'Đã mở khóa tài khoản.' : 'Đã khóa tài khoản và thu hồi phiên.'),
    onSettled: async () => { setTarget(null); setReason(''); await queryClient.invalidateQueries({ queryKey: userKeys.all }); },
  });
  const result = users.data;
  const updateFilter = (next: boolean | undefined) => { setIsActive(next); setPage(1); };

  return <section className="mx-auto max-w-7xl px-4 py-10">
    <h1 className="text-2xl font-bold text-gray-900">Quản lý người dùng</h1>
    <p className="mt-1 text-sm text-gray-600">Tìm kiếm, khóa hoặc mở khóa tài khoản người dùng.</p>
    <div className="mt-6 flex flex-wrap gap-3"><input aria-label="Tìm kiếm người dùng" value={searchInput} onChange={(event) => { setSearchInput(event.target.value); setPage(1); }} placeholder="Tìm email hoặc tên" className="rounded-lg border border-gray-300 px-3 py-2" /><select aria-label="Trạng thái" value={isActive === undefined ? 'all' : String(isActive)} onChange={(event) => updateFilter(event.target.value === 'all' ? undefined : event.target.value === 'true')} className="rounded-lg border border-gray-300 px-3 py-2"><option value="all">Tất cả trạng thái</option><option value="true">Đang hoạt động</option><option value="false">Đã khóa</option></select></div>
    {users.isError && <p role="alert" className="mt-6 text-red-700">{getErrorMessage(users.error)}</p>}
    {users.isPending ? <p className="mt-6">Đang tải danh sách…</p> : <div className="mt-6 overflow-x-auto rounded-xl border border-gray-200"><table className="w-full text-left text-sm"><caption className="sr-only">Danh sách người dùng</caption><thead className="bg-gray-50"><tr><th className="p-3">Người dùng</th><th className="p-3">Vai trò</th><th className="p-3">Trạng thái</th><th className="p-3">Ngày tạo</th><th className="p-3">Công thức</th><th className="p-3">Thao tác</th></tr></thead><tbody>{result?.items.map((item) => <tr key={item.id} className="border-t"><td className="p-3"><div className="flex items-center gap-3">{item.avatarUrl ? <><span className="sr-only">Ảnh đại diện của {item.displayName}</span><img src={item.avatarUrl} alt="" className="h-9 w-9 rounded-full object-cover" /></> : <span aria-label={`Ảnh đại diện mặc định của ${item.displayName}`} className="flex h-9 w-9 items-center justify-center rounded-full bg-orange-100 font-semibold text-orange-800">{item.displayName.slice(0, 1).toLocaleUpperCase('vi')}</span>}<div><p className="font-medium">{item.displayName}</p><p className="text-gray-600">{item.email}</p></div></div></td><td className="p-3">{item.roles.join(', ')}</td><td className="p-3">{item.isActive ? 'Đang hoạt động' : 'Đã khóa'}</td><td className="p-3">{new Date(item.createdAt).toLocaleDateString('vi-VN')}</td><td className="p-3">{item.recipeCount}</td><td className="p-3">{item.id !== user?.id && <button type="button" onClick={() => setTarget(item)} className="rounded-md border px-3 py-1 text-orange-700">{item.isActive ? 'Khóa' : 'Mở khóa'}</button>}</td></tr>)}</tbody></table></div>}
    {result && <Pagination page={result.page} totalPages={result.totalPages} hasNextPage={result.hasNextPage} hasPreviousPage={result.hasPreviousPage} buildHref={(nextPage) => { const query = new URLSearchParams({ page: String(nextPage), pageSize: String(PAGE_SIZE) }); if (search) query.set('search', search); if (isActive !== undefined) query.set('isActive', String(isActive)); return `?${query.toString()}`; }} />}
    {target && <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"><div role="alertdialog" aria-modal="true" aria-labelledby="user-status-title" className="w-full max-w-md rounded-xl bg-white p-6"><h2 id="user-status-title" className="text-lg font-bold">{target.isActive ? 'Khóa' : 'Mở khóa'} {target.displayName}?</h2><span id="status-reason-label" className="mt-4 block text-sm font-medium">Lý do</span><textarea id="status-reason" aria-labelledby="status-reason-label" value={reason} onChange={(event) => setReason(event.target.value)} minLength={5} maxLength={500} className="mt-1 w-full rounded border p-2" /><div className="mt-5 flex justify-end gap-2"><button type="button" onClick={() => setTarget(null)} className="rounded border px-3 py-2">Hủy</button><button type="button" disabled={reason.trim().length < 5 || reason.trim().length > 500 || mutation.isPending} onClick={() => mutation.mutate({ targetUser: target, active: !target.isActive, actionReason: reason.trim() })} className="rounded bg-orange-600 px-3 py-2 text-white">Xác nhận</button></div></div></div>}
  </section>;
}

export default function UsersPage() { return <RequireRole roles={['Admin']}><UsersPageContent /></RequireRole>; }
