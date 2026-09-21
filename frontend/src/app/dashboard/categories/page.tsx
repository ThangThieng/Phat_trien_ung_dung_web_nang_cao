'use client';

import { useMemo, useState } from 'react';
import Link from 'next/link';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Loader2, Pencil, Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import CategoryFormDialog from '@/features/categories/components/CategoryFormDialog';
import {
  CATEGORIES_QUERY_KEY,
  createCategory,
  deleteCategory,
  getCategoriesForAdmin,
  updateCategory,
} from '@/features/categories/admin-api';
import type { CategoryPayload } from '@/features/categories/admin-api';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import { useAuth } from '@/features/auth/auth-context';
import type { Category } from '@/types/api';

type DialogState = { mode: 'create' } | { mode: 'edit'; category: Category } | null;

/** FR-CAT-003/004/005 – trang quản trị danh mục (chỉ Admin). SRS §5.1 route /dashboard/categories. */
export default function CategoriesDashboardPage() {
  const { accessToken, user, isAuthenticated, isReady } = useAuth();
  const queryClient = useQueryClient();
  const [dialog, setDialog] = useState<DialogState>(null);
  const [pendingDelete, setPendingDelete] = useState<Category | null>(null);

  // Phân quyền 2 tầng của SRS §2.3: role Admin (role KHÔNG phân cấp – tài khoản Admin được seed cả Author lẫn Admin).
  const isAdmin = useMemo(() => user?.roles.includes('Admin') ?? false, [user]);

  const categoriesQuery = useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: ({ signal }) => getCategoriesForAdmin(signal),
    enabled: isAdmin,
  });

  const refreshCategories = () => queryClient.invalidateQueries({ queryKey: CATEGORIES_QUERY_KEY });

  const saveMutation = useMutation({
    mutationFn: (payload: CategoryPayload) =>
      dialog?.mode === 'edit'
        ? updateCategory(dialog.category.id, payload, accessToken)
        : createCategory(payload, accessToken),
    onSuccess: async () => {
      toast.success(dialog?.mode === 'edit' ? 'Đã cập nhật danh mục.' : 'Đã tạo danh mục mới.');
      setDialog(null);
      await refreshCategories();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (category: Category) => deleteCategory(category.id, accessToken),
    onSuccess: async () => {
      toast.success('Đã xóa danh mục.');
      setPendingDelete(null);
      await refreshCategories();
    },
    onError: (error: unknown) => {
      // FR-CAT-005: 409 kèm số lượng công thức – hiển thị nguyên văn detail để Admin biết quy mô cần chuyển,
      // không thay bằng thông báo chung chung.
      if (error instanceof ApiError && error.code === 'CATEGORY_DELETE_HAS_RECIPES') {
        toast.error(error.message, { duration: 8000 });
        setPendingDelete(null);
        return;
      }
      toast.error(getErrorMessage(error));
    },
  });

  if (!isReady) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center text-gray-500">
        <Loader2 aria-hidden className="mx-auto h-6 w-6 animate-spin" />
        <p className="mt-2 text-sm">Đang kiểm tra phiên đăng nhập…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Quản lý danh mục</h1>
        <p className="mt-2 text-gray-600">Bạn cần đăng nhập bằng tài khoản quản trị để tiếp tục.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/categories"
          className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white hover:bg-orange-700"
        >
          Đăng nhập
        </Link>
      </div>
    );
  }

  if (!isAdmin) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Không có quyền truy cập</h1>
        <p className="mt-2 text-gray-600">
          Trang quản lý danh mục chỉ dành cho quản trị viên. Tài khoản của bạn không có quyền này.
        </p>
        <Link href="/" className="mt-6 inline-block text-orange-700 underline">
          Về trang chủ
        </Link>
      </div>
    );
  }

  const categories = categoriesQuery.data ?? [];

  return (
    <div className="mx-auto max-w-5xl px-4 py-10">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quản lý danh mục</h1>
          <p className="mt-1 text-sm text-gray-600">
            Tạo, sửa và xóa danh mục công thức. Danh mục còn công thức sẽ không xóa được.
          </p>
        </div>
        <button
          type="button"
          onClick={() => setDialog({ mode: 'create' })}
          className="flex items-center gap-2 rounded-lg bg-orange-600 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-700"
        >
          <Plus aria-hidden className="h-4 w-4" /> Thêm danh mục
        </button>
      </div>

      {categoriesQuery.isPending && (
        <p className="mt-10 text-center text-sm text-gray-500">Đang tải danh sách…</p>
      )}

      {categoriesQuery.isError && (
        <p role="alert" className="mt-10 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">
          {getErrorMessage(categoriesQuery.error)}
        </p>
      )}

      {categoriesQuery.isSuccess && categories.length === 0 && (
        <p className="mt-10 text-center text-sm text-gray-500">Chưa có danh mục nào.</p>
      )}

      {categories.length > 0 && (
        <div className="mt-6 overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table className="w-full text-left text-sm">
            <caption className="sr-only">Danh sách danh mục công thức</caption>
            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th scope="col" className="px-4 py-3">Tên</th>
                <th scope="col" className="px-4 py-3">Đường dẫn</th>
                <th scope="col" className="px-4 py-3 text-right">Thứ tự</th>
                <th scope="col" className="px-4 py-3 text-right">Công thức</th>
                <th scope="col" className="px-4 py-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {categories.map((category) => (
                <tr key={category.id}>
                  <td className="px-4 py-3 font-medium text-gray-900">{category.name}</td>
                  <td className="px-4 py-3 text-gray-500">/{category.slug}</td>
                  <td className="px-4 py-3 text-right text-gray-700">{category.orderIndex}</td>
                  <td className="px-4 py-3 text-right text-gray-700">{category.recipeCount}</td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-1">
                      <button
                        type="button"
                        onClick={() => setDialog({ mode: 'edit', category })}
                        aria-label={`Sửa danh mục ${category.name}`}
                        className="flex items-center gap-1 rounded-md px-2 py-1 text-orange-700 hover:bg-orange-50"
                      >
                        <Pencil aria-hidden className="h-4 w-4" /> Sửa
                      </button>
                      <button
                        type="button"
                        onClick={() => setPendingDelete(category)}
                        aria-label={`Xóa danh mục ${category.name}`}
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

      {dialog && (
        <CategoryFormDialog
          category={dialog.mode === 'edit' ? dialog.category : null}
          accessToken={accessToken}
          onSubmit={(payload) => saveMutation.mutateAsync(payload).then(() => undefined)}
          onClose={() => setDialog(null)}
        />
      )}

      {pendingDelete && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-description"
            className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl"
          >
            <h2 id="delete-dialog-title" className="text-lg font-bold text-gray-900">
              Xóa danh mục?
            </h2>
            <p id="delete-dialog-description" className="mt-2 text-sm text-gray-600">
              Danh mục <strong>{pendingDelete.name}</strong> sẽ bị ẩn khỏi hệ thống. Nếu vẫn còn công
              thức thuộc danh mục này, thao tác sẽ bị từ chối.
            </p>
            <div className="mt-6 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setPendingDelete(null)}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 hover:bg-gray-50"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={() => deleteMutation.mutate(pendingDelete)}
                disabled={deleteMutation.isPending}
                className="flex items-center gap-2 rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700 disabled:opacity-60"
              >
                {deleteMutation.isPending && <Loader2 aria-hidden className="h-4 w-4 animate-spin" />}
                Xóa danh mục
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
