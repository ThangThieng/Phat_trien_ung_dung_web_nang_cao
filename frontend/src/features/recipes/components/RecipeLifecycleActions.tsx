'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Button } from '@/components/ui/button';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import type { Recipe, RecipeStatus, RecipeStatusAction } from '@/types/recipe';
import { allowedRecipeActions } from '../status-transitions';
import { archiveRecipe, deleteRecipe, publishRecipe, unarchiveRecipe, unpublishRecipe } from '../write-api';

const LABELS: Record<RecipeStatusAction, string> = {
  publish: 'Xuất bản',
  unpublish: 'Hủy xuất bản',
  archive: 'Lưu trữ',
  unarchive: 'Khôi phục về nháp',
  delete: 'Xóa công thức',
};

const REQUESTS = {
  publish: publishRecipe,
  unpublish: unpublishRecipe,
  archive: archiveRecipe,
  unarchive: unarchiveRecipe,
};

/**
 * Máy trạng thái khép kín (FR-RCP-005 → 007): chỉ hiện hành động hợp lệ với trạng thái hiện tại. `compact` dùng ở từng dòng của
 * /dashboard/recipes, nơi không biết đủ nguyên liệu/bước nên server trả 400 RECIPE_PUBLISH_INCOMPLETE khi thiếu.
 */
export default function RecipeLifecycleActions({
  recipe,
  accessToken,
  onChanged,
  onConflict,
  canPublish = true,
  compact = false,
}: {
  recipe: { id: string; title: string; status: RecipeStatus };
  accessToken: string;
  onChanged?: (value: Recipe) => void;
  onConflict?: () => void;
  canPublish?: boolean;
  compact?: boolean;
}) {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState<RecipeStatusAction | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [confirmTitle, setConfirmTitle] = useState('');
  const [conflictOpen, setConflictOpen] = useState(false);

  const refreshLists = () => queryClient.invalidateQueries({ queryKey: ['recipes', 'private'] });
  const handleError = (error: unknown) => {
    if (error instanceof ApiError && error.status === 409) setConflictOpen(true);
    else toast.error(getErrorMessage(error));
  };

  const run = async (action: keyof typeof REQUESTS) => {
    setBusy(action);
    try {
      onChanged?.(await REQUESTS[action](recipe.id, accessToken));
      await refreshLists();
      toast.success(`Đã ${LABELS[action].toLowerCase()}.`);
    } catch (error) {
      handleError(error);
    } finally {
      setBusy(null);
    }
  };

  const remove = async () => {
    setBusy('delete');
    try {
      await deleteRecipe(recipe.id, accessToken);
      await refreshLists();
      toast.success('Đã xóa công thức.');
      if (!compact) router.replace('/dashboard/recipes');
    } catch (error) {
      handleError(error);
    } finally {
      setBusy(null);
      setDeleteOpen(false);
      setConfirmTitle('');
    }
  };

  const actions = allowedRecipeActions(recipe.status);
  const buttons = (
    <div className={compact ? 'flex flex-wrap justify-end gap-1' : 'mt-4 flex flex-wrap gap-2'}>
      {actions
        .filter((action): action is keyof typeof REQUESTS => action !== 'delete')
        .map((action) => (
          <Button
            key={action}
            type="button"
            size={compact ? 'sm' : 'default'}
            variant={compact || action === 'archive' ? 'outline' : 'default'}
            aria-label={compact ? `${LABELS[action]} ${recipe.title}` : undefined}
            disabled={(action === 'publish' && !canPublish) || busy !== null}
            onClick={() => {
              run(action).catch(() => undefined);
            }}
          >
            {LABELS[action]}
          </Button>
        ))}
      <Button
        type="button"
        size={compact ? 'sm' : 'default'}
        variant="destructive"
        aria-label={compact ? `Xóa ${recipe.title}` : undefined}
        disabled={busy !== null}
        onClick={() => setDeleteOpen(true)}
      >
        Xóa
      </Button>
    </div>
  );

  const dialogs = (
    <>
      <AlertDialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Xóa công thức?</AlertDialogTitle>
            <AlertDialogDescription>
              Gõ chính xác tên công thức để xác nhận: <strong>{recipe.title}</strong>.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <input
            value={confirmTitle}
            onChange={(event) => setConfirmTitle(event.target.value)}
            aria-label="Tên công thức để xác nhận xóa"
            className="rounded border border-gray-300 px-3 py-2"
          />
          <AlertDialogFooter>
            <AlertDialogCancel>Hủy</AlertDialogCancel>
            <AlertDialogAction
              disabled={confirmTitle !== recipe.title || busy === 'delete'}
              onClick={() => {
                remove().catch(() => undefined);
              }}
            >
              Xóa công thức
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <AlertDialog open={conflictOpen} onOpenChange={setConflictOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Trạng thái công thức vừa thay đổi ở nơi khác</AlertDialogTitle>
            <AlertDialogDescription>Thao tác chưa được áp dụng. Hãy tải lại để xem trạng thái mới.</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogAction
              onClick={() => {
                onConflict?.();
                refreshLists().catch(() => undefined);
              }}
            >
              Tải lại
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );

  if (compact) {
    return (
      <>
        {buttons}
        {dialogs}
      </>
    );
  }

  return (
    <section className="rounded-xl border border-gray-200 bg-white p-5" aria-labelledby="lifecycle-heading">
      <h2 id="lifecycle-heading" className="font-semibold text-gray-900">
        Trạng thái công thức
      </h2>
      <p className="mt-1 text-sm text-gray-600">Chỉ các thao tác hợp lệ với trạng thái hiện tại được hiển thị.</p>
      {buttons}
      {dialogs}
    </section>
  );
}
