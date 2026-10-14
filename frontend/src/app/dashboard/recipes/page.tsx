'use client';

import Link from 'next/link';
import { useMemo, useState } from 'react';
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query';
import { Loader2, Pencil, Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';
import {
  changeRecipeStatus,
  deleteRecipe,
  getMyRecipes,
  myRecipesKey,
} from '@/features/recipes/my-recipes-api';
import { getErrorMessage } from '@/lib/api-client';
import type { RecipeStatus, RecipeSummary } from '@/types/api';

const TABS: readonly { value: RecipeStatus; label: string }[] = [
  { value: 'Draft', label: 'Bản nháp' },
  { value: 'Published', label: 'Đã xuất bản' },
  { value: 'Archived', label: 'Lưu trữ' },
];

/** FR-RCP-011 + FR-RCP-007 – dashboard riêng tư, tách hẳn query key khỏi danh sách công khai. */
export default function MyRecipesDashboardPage() {
  const { accessToken, isAuthenticated, isReady } = useAuth();
  const queryClient = useQueryClient();
  const [status, setStatus] = useState<RecipeStatus>('Draft');
  const [pendingDelete, setPendingDelete] = useState<RecipeSummary | null>(null);
  const [typedTitle, setTypedTitle] = useState('');
  const params = useMemo(
    () => ({
      status,
      page: 1,
      pageSize: 24,
      sortBy: 'createdAt' as const,
      sortOrder: 'desc' as const,
    }),
    [status],
  );

  const recipesQuery = useQuery({
    queryKey: myRecipesKey(params),
    queryFn: ({ signal }) => getMyRecipes(params, accessToken!, signal),
    enabled: Boolean(accessToken),
  });
  const statsQueries = useQueries({
    queries: TABS.map(({ value }) => ({
      queryKey: myRecipesKey({ status: value, page: 1, pageSize: 1 }),
      queryFn: ({ signal }) =>
        getMyRecipes({ status: value, page: 1, pageSize: 1 }, accessToken!, signal),
      enabled: Boolean(accessToken),
      staleTime: 30_000,
    })),
  });

  const refresh = async () => queryClient.invalidateQueries({ queryKey: ['recipes', 'mine'] });
  const deleteMutation = useMutation({
    mutationFn: (recipe: RecipeSummary) => deleteRecipe(recipe.id, accessToken!),
    onMutate: async (recipe) => {
      await queryClient.cancelQueries({ queryKey: myRecipesKey(params) });
      const previous = queryClient.getQueryData(myRecipesKey(params));
      queryClient.setQueryData(
        myRecipesKey(params),
        (current: { items: RecipeSummary[]; totalCount: number } | undefined) =>
          current
            ? {
                ...current,
                items: current.items.filter((item) => item.id !== recipe.id),
                totalCount: current.totalCount - 1,
              }
            : current,
      );
      return { previous };
    },
    onError: (error, _recipe, context) => {
      queryClient.setQueryData(myRecipesKey(params), context?.previous);
      toast.error(getErrorMessage(error));
    },
    onSuccess: () =>
      toast.success('Đã xóa mềm công thức. Công thức sẽ không còn xuất hiện công khai.'),
    onSettled: async () => {
      setPendingDelete(null);
      setTypedTitle('');
      await refresh();
    },
  });
  const statusMutation = useMutation({
    mutationFn: ({
      recipe,
      nextStatus,
    }: {
      recipe: RecipeSummary;
      nextStatus: 'Draft' | 'Published';
    }) => changeRecipeStatus(recipe, nextStatus, accessToken!),
    onSuccess: async () => {
      toast.success('Đã cập nhật trạng thái công thức.');
      await refresh();
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error)),
  });

  if (!isReady) return <Loading />;
  if (!isAuthenticated) return <LoginPrompt />;

  const recipes = recipesQuery.data?.items ?? [];
  return (
    <main className="mx-auto max-w-6xl px-4 py-10">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Công thức của tôi</h1>
          <p className="mt-1 text-sm text-gray-600">
            Quản lý bản nháp, công thức đã xuất bản và nội dung lưu trữ.
          </p>
        </div>
        <Link
          href="/dashboard/recipes/new"
          className="flex items-center gap-2 rounded-lg bg-orange-600 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-700"
        >
          <Plus aria-hidden className="h-4 w-4" /> Viết công thức
        </Link>
      </div>

      <section aria-label="Thống kê công thức" className="mt-6 grid gap-3 sm:grid-cols-3">
        {TABS.map(({ value, label }, index) => (
          <button
            key={value}
            type="button"
            onClick={() => setStatus(value)}
            className={`rounded-xl border p-4 text-left ${status === value ? 'border-orange-400 bg-orange-50' : 'border-gray-200 bg-white hover:border-orange-200'}`}
          >
            <span className="text-sm text-gray-600">{label}</span>
            <strong className="mt-1 block text-2xl text-gray-900">
              {statsQueries[index]?.data?.totalCount ?? '–'}
            </strong>
          </button>
        ))}
      </section>

      <div
        role="tablist"
        aria-label="Trạng thái công thức"
        className="mt-8 flex gap-2 border-b border-gray-200"
      >
        {TABS.map(({ value, label }) => (
          <button
            key={value}
            role="tab"
            aria-selected={status === value}
            type="button"
            onClick={() => setStatus(value)}
            className={`border-b-2 px-3 py-2 text-sm font-semibold ${status === value ? 'border-orange-600 text-orange-700' : 'border-transparent text-gray-600 hover:text-orange-700'}`}
          >
            {label}
          </button>
        ))}
      </div>

      {recipesQuery.isPending && <Loading />}
      {recipesQuery.isError && (
        <p role="alert" className="mt-6 rounded-lg bg-red-50 p-4 text-sm text-red-700">
          {getErrorMessage(recipesQuery.error)}
        </p>
      )}
      {recipesQuery.isSuccess && recipes.length === 0 && (
        <p className="mt-10 text-center text-sm text-gray-500">
          Chưa có công thức ở trạng thái này.
        </p>
      )}
      {recipes.length > 0 && (
        <div className="mt-5 overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table className="w-full text-left text-sm">
            <caption className="sr-only">Danh sách công thức của tôi</caption>
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
              <tr>
                <th className="px-4 py-3">Công thức</th>
                <th className="px-4 py-3">Danh mục</th>
                <th className="px-4 py-3">Trạng thái</th>
                <th className="px-4 py-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {recipes.map((recipe) => (
                <tr key={recipe.id}>
                  <td className="px-4 py-3">
                    <strong className="block text-gray-900">{recipe.title}</strong>
                    <span className="text-xs text-gray-500">
                      {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút · {recipe.servings}{' '}
                      phần
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{recipe.category.name}</td>
                  <td className="px-4 py-3 text-gray-600">
                    {status === 'Draft'
                      ? 'Bản nháp'
                      : status === 'Published'
                        ? 'Đã xuất bản'
                        : 'Lưu trữ'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-1">
                      <Link
                        href={`/dashboard/recipes/${recipe.id}/edit`}
                        className="flex items-center gap-1 rounded-md px-2 py-1 text-orange-700 hover:bg-orange-50"
                      >
                        <Pencil aria-hidden className="h-4 w-4" /> Sửa
                      </Link>
                      {status !== 'Archived' && (
                        <button
                          type="button"
                          disabled={statusMutation.isPending}
                          onClick={() =>
                            statusMutation.mutate({
                              recipe,
                              nextStatus: status === 'Draft' ? 'Published' : 'Draft',
                            })
                          }
                          className="rounded-md px-2 py-1 text-blue-700 hover:bg-blue-50"
                        >
                          {status === 'Draft' ? 'Xuất bản' : 'Hủy xuất bản'}
                        </button>
                      )}
                      <button
                        type="button"
                        onClick={() => setPendingDelete(recipe)}
                        className="flex items-center gap-1 rounded-md px-2 py-1 text-red-700 hover:bg-red-50"
                      >
                        <Trash2 aria-hidden className="h-4 w-4" /> Xóa
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {pendingDelete && (
        <DeleteDialog
          recipe={pendingDelete}
          typedTitle={typedTitle}
          onTypedTitle={setTypedTitle}
          busy={deleteMutation.isPending}
          onClose={() => {
            setPendingDelete(null);
            setTypedTitle('');
          }}
          onConfirm={() => deleteMutation.mutate(pendingDelete)}
        />
      )}
    </main>
  );
}

function Loading() {
  return (
    <div className="py-12 text-center text-sm text-gray-500">
      <Loader2 aria-hidden className="mx-auto h-5 w-5 animate-spin" />{' '}
      <span className="mt-2 block">Đang tải…</span>
    </div>
  );
}
function LoginPrompt() {
  return (
    <main className="mx-auto max-w-xl px-4 py-16 text-center">
      <h1 className="text-2xl font-bold">Công thức của tôi</h1>
      <p className="mt-2 text-gray-600">Bạn cần đăng nhập để xem dashboard riêng tư.</p>
      <Link
        href="/auth/login?callbackUrl=/dashboard/recipes"
        className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white"
      >
        Đăng nhập
      </Link>
    </main>
  );
}
function DeleteDialog({
  recipe,
  typedTitle,
  onTypedTitle,
  busy,
  onClose,
  onConfirm,
}: {
  recipe: RecipeSummary;
  typedTitle: string;
  onTypedTitle: (value: string) => void;
  busy: boolean;
  onClose: () => void;
  onConfirm: () => void;
}) {
  const confirmed = typedTitle === recipe.title;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="delete-title"
        className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
      >
        <h2 id="delete-title" className="text-lg font-bold">
          Xóa mềm công thức?
        </h2>
        <p className="mt-2 text-sm text-gray-600">
          Để xác nhận, hãy gõ chính xác <strong>{recipe.title}</strong>. Công thức sẽ biến mất khỏi
          dashboard và trang công khai.
        </p>
        <label className="mt-4 block text-sm font-medium">
          Tên công thức
          <input
            value={typedTitle}
            onChange={(event) => onTypedTitle(event.target.value)}
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2"
            autoFocus
          />
        </label>
        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold"
          >
            Hủy
          </button>
          <button
            type="button"
            disabled={!confirmed || busy}
            onClick={onConfirm}
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white disabled:opacity-50"
          >
            {busy ? 'Đang xóa…' : 'Xóa công thức'}
          </button>
        </div>
      </div>
    </div>
  );
}
