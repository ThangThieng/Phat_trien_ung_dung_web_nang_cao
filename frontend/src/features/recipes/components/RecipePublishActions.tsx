'use client';

import { useState } from 'react';
import { toast } from 'sonner';
import type { RecipeDetail } from '@/types/api';
import { getErrorMessage } from '@/lib/api-client';
import { publishRecipe, unpublishRecipe } from '../write-api';

export default function RecipePublishActions({ recipe, accessToken, onChanged }: { recipe: RecipeDetail; accessToken: string; onChanged: (recipe: RecipeDetail) => void }) {
  const [busy, setBusy] = useState(false);
  const isDraft = recipe.status === 'Draft';
  const run = async () => {
    setBusy(true);
    try {
      const next = isDraft ? await publishRecipe(recipe.id, accessToken) : await unpublishRecipe(recipe.id, accessToken);
      onChanged(next);
      toast.success(isDraft ? 'Công thức đã được xuất bản.' : 'Công thức đã trở về bản nháp.');
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally { setBusy(false); }
  };

  if (recipe.status === 'Archived') return <p className="text-sm text-gray-500">Công thức đang lưu trữ; hãy khôi phục trước khi chỉnh sửa hoặc xuất bản.</p>;
  return (
    <div className="flex flex-wrap items-center gap-3 rounded-xl border border-orange-200 bg-orange-50 p-4">
      <p className="flex-1 text-sm text-orange-900">{isDraft ? 'Cần ít nhất một nguyên liệu và một bước nấu để xuất bản.' : 'Công thức đang công khai. Bạn có thể đưa về nháp để tiếp tục hoàn thiện.'}</p>
      <button type="button" disabled={busy || (isDraft && (!recipe.ingredients.length || !recipe.steps.length))} onClick={() => void run()} className="rounded-lg bg-orange-600 px-4 py-2 text-sm font-semibold text-white disabled:opacity-50">
        {busy ? 'Đang xử lý…' : isDraft ? 'Xuất bản' : 'Hủy xuất bản'}
      </button>
    </div>
  );
}
