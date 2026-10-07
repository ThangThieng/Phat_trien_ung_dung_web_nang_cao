'use client';

import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';
import RecipeBasicsForm from '@/features/recipes/components/RecipeBasicsForm';
import RecipeContentEditor from '@/features/recipes/components/RecipeContentEditor';
import RecipePublishActions from '@/features/recipes/components/RecipePublishActions';
import { getErrorMessage } from '@/lib/api-client';
import { getMyRecipe } from '@/features/recipes/write-api';
import type { RecipeDetail } from '@/types/api';

/** FR-RCP-004/005/009/010 - màn hình riêng tư để soạn, quản lý nội dung và xuất bản công thức. */
export default function EditRecipePage() {
  const params = useParams<{ id: string }>();
  const { accessToken, isAuthenticated, isReady } = useAuth();
  const [recipe, setRecipe] = useState<RecipeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!accessToken || !params.id) return;
    setLoading(true);
    try { setRecipe(await getMyRecipe(params.id, accessToken)); setError(null); }
    catch (reason) { setError(getErrorMessage(reason)); }
    finally { setLoading(false); }
  }, [accessToken, params.id]);

  useEffect(() => { void load(); }, [load]);

  if (isReady && !isAuthenticated) return <div className="mx-auto max-w-xl px-4 py-16 text-center"><h1 className="text-2xl font-bold">Chỉnh sửa công thức</h1><p className="mt-2 text-gray-600">Bạn cần đăng nhập để tiếp tục.</p><Link href={`/auth/login?callbackUrl=/dashboard/recipes/${params.id}/edit`} className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white">Đăng nhập</Link></div>;
  if (loading) return <div className="mx-auto max-w-4xl px-4 py-12 text-gray-600">Đang tải công thức…</div>;
  if (!recipe || !accessToken) return <div className="mx-auto max-w-2xl px-4 py-12"><p role="alert" className="rounded-lg bg-red-50 p-4 text-red-700">{error ?? 'Không tìm thấy công thức.'}</p><Link href="/dashboard/recipes/new" className="mt-4 inline-block text-orange-700 underline">Tạo công thức mới</Link></div>;

  return <main className="mx-auto max-w-5xl px-4 py-10"><div className="mb-6 flex flex-wrap items-center justify-between gap-3"><div><p className="text-sm font-semibold text-orange-700">{recipe.status === 'Draft' ? 'Bản nháp' : recipe.status === 'Published' ? 'Đã xuất bản' : 'Lưu trữ'}</p><h1 className="text-2xl font-bold text-gray-900">Chỉnh sửa công thức</h1></div><Link href="/dashboard/recipes/new" className="text-sm font-semibold text-orange-700 underline">Tạo công thức khác</Link></div><div className="space-y-8"><section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm"><h2 className="mb-4 text-lg font-semibold">Thông tin công thức</h2><RecipeBasicsForm key={recipe.rowVersion} recipe={recipe} accessToken={accessToken} onUpdated={(next) => { setRecipe(next); toast.success('Đã lưu thay đổi.'); }} /></section><section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm"><RecipeContentEditor recipeId={recipe.id} accessToken={accessToken} ingredients={recipe.ingredients} steps={recipe.steps} onChanged={load} /></section><RecipePublishActions recipe={recipe} accessToken={accessToken} onChanged={setRecipe} /></div></main>;
}
