'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription,
  AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Button } from '@/components/ui/button';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import type { Recipe, RecipeStatusAction } from '@/types/recipe';
import { allowedRecipeActions } from '../status-transitions';
import { archiveRecipe, deleteRecipe, publishRecipe, unarchiveRecipe, unpublishRecipe } from '../write-api';

const labels: Record<RecipeStatusAction, string> = {
  publish: 'Xuất bản', unpublish: 'Hủy xuất bản', archive: 'Lưu trữ', unarchive: 'Khôi phục về nháp', delete: 'Xóa công thức',
};

export default function RecipeLifecycleActions({ recipe, accessToken, onChanged, onConflict, canPublish }: {
  recipe: Recipe; accessToken: string; onChanged: (value: Recipe) => void; onConflict: () => void; canPublish: boolean;
}) {
  const router = useRouter();
  const [busy, setBusy] = useState<RecipeStatusAction | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [confirmTitle, setConfirmTitle] = useState('');
  const [conflictOpen, setConflictOpen] = useState(false);
  const run = async (action: Exclude<RecipeStatusAction, 'delete'>) => {
    setBusy(action);
    try {
      const request = { publish: publishRecipe, unpublish: unpublishRecipe, archive: archiveRecipe, unarchive: unarchiveRecipe }[action];
      onChanged(await request(recipe.id, accessToken));
      toast.success(`Đã ${labels[action].toLowerCase()}.`);
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) setConflictOpen(true);
      else toast.error(getErrorMessage(error));
    } finally { setBusy(null); }
  };
  const remove = async () => {
    setBusy('delete');
    try {
      await deleteRecipe(recipe.id, accessToken);
      toast.success('Đã xóa mềm công thức.');
      router.replace('/dashboard/recipes');
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) setConflictOpen(true);
      else toast.error(getErrorMessage(error));
    } finally { setBusy(null); setDeleteOpen(false); }
  };
  const actions = allowedRecipeActions(recipe.status);
  return <section className="rounded-xl border border-gray-200 bg-white p-5" aria-labelledby="lifecycle-heading">
    <h2 id="lifecycle-heading" className="font-semibold text-gray-900">Trạng thái công thức</h2>
    <p className="mt-1 text-sm text-gray-600">Chỉ các thao tác hợp lệ với trạng thái hiện tại được hiển thị.</p>
    <div className="mt-4 flex flex-wrap gap-2">
      {actions.filter((action) => action !== 'delete').map((action) => <Button key={action} type="button" variant={action === 'archive' ? 'outline' : 'default'} disabled={action === 'publish' && !canPublish} isLoading={busy === action} onClick={() => run(action)}>{labels[action]}</Button>)}
      {actions.includes('delete') && <Button type="button" variant="destructive" onClick={() => setDeleteOpen(true)}>Xóa</Button>}
    </div>
    <AlertDialog open={deleteOpen} onOpenChange={setDeleteOpen}>
      <AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Xóa mềm công thức?</AlertDialogTitle><AlertDialogDescription>Gõ chính xác tên công thức để xác nhận: <strong>{recipe.title}</strong>.</AlertDialogDescription></AlertDialogHeader>
        <input value={confirmTitle} onChange={(event) => setConfirmTitle(event.target.value)} aria-label="Tên công thức để xác nhận xóa" className="rounded border border-gray-300 px-3 py-2" />
        <AlertDialogFooter><AlertDialogCancel>Hủy</AlertDialogCancel><AlertDialogAction disabled={confirmTitle !== recipe.title || busy === 'delete'} onClick={() => { remove().catch(() => undefined); }}>Xóa công thức</AlertDialogAction></AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
    <AlertDialog open={conflictOpen} onOpenChange={setConflictOpen}>
      <AlertDialogContent><AlertDialogHeader><AlertDialogTitle>Công thức đã thay đổi ở nơi khác</AlertDialogTitle><AlertDialogDescription>Thao tác chưa được áp dụng. Hãy tải lại dữ liệu mới nhất trước khi so sánh và thực hiện lại.</AlertDialogDescription></AlertDialogHeader><AlertDialogFooter><AlertDialogCancel>Ở lại</AlertDialogCancel><AlertDialogAction onClick={() => { onConflict(); }}>Tải phiên bản mới</AlertDialogAction></AlertDialogFooter></AlertDialogContent>
    </AlertDialog>
  </section>;
}
