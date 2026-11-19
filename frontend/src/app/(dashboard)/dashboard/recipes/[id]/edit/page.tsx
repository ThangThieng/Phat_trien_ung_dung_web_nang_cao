'use client';

import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useAuth } from '@/features/auth/auth-context';
import RecipeWizard from '@/features/recipes/components/RecipeWizard';
import { getMyRecipe } from '@/features/recipes/write-api';
import { getErrorMessage } from '@/lib/api-client';
import type { Recipe } from '@/types/recipe';

/** FR-RCP-004 – reads only the private endpoint and delegates all diffs to RecipeWizard. */
export default function EditRecipePage() {
  const { id } = useParams<{ id: string }>();
  const { accessToken, isAuthenticated, isReady } = useAuth();
  const [recipe, setRecipe] = useState<Recipe>();
  const [error, setError] = useState<string>();
  const load = useCallback(async () => {
    if (!accessToken || !id) return;
    try {
      setRecipe(await getMyRecipe(id, accessToken));
    } catch (reason) {
      setError(getErrorMessage(reason));
    }
  }, [accessToken, id]);
  useEffect(() => {
    load().catch(() => undefined);
  }, [load]);
  if (isReady && !isAuthenticated)
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Chỉnh sửa công thức</h1>
        <Link
          href={`/auth/login?callbackUrl=/dashboard/recipes/${id}/edit`}
          className="bg-navy mt-6 inline-block rounded-lg px-4 py-2 font-semibold text-white"
        >
          Đăng nhập
        </Link>
      </div>
    );
  if (error)
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <p role="alert" className="rounded bg-red-50 p-4 text-red-700">
          {error}
        </p>
      </div>
    );
  if (!recipe || !accessToken)
    return <p className="text-ink-soft mx-auto max-w-4xl px-4 py-10">Đang tải công thức…</p>;
  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <h1 className="text-ink text-2xl font-bold">Chỉnh sửa: {recipe.title}</h1>
      <p className="text-ink-soft mt-2">
        Lưu thông tin cha trước để phát hiện xung đột 409 trước mọi thay đổi nguyên liệu, bước hoặc
        ảnh.
      </p>
      <div className="border-line bg-paper mt-6 rounded-2xl border p-6 shadow-sm">
        <RecipeWizard accessToken={accessToken} initialRecipe={recipe} />
      </div>
    </div>
  );
}
