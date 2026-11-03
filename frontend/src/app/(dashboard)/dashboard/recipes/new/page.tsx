'use client';

import Link from 'next/link';
import { useAuth } from '@/features/auth/auth-context';
import RecipeWizard from '@/features/recipes/components/RecipeWizard';

/** FR-RCP-003 – five-step wizard; POST once with ingredients and steps before the image step. */
export default function NewRecipePage() {
  const { accessToken, isAuthenticated, isReady } = useAuth();
  if (isReady && !isAuthenticated) return <div className="mx-auto max-w-xl px-4 py-16 text-center"><h1 className="text-2xl font-bold">Viết công thức mới</h1><p className="mt-2 text-gray-600">Bạn cần đăng nhập để viết công thức.</p><Link href="/auth/login?callbackUrl=/dashboard/recipes/new" className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white">Đăng nhập</Link></div>;
  if (!accessToken) return <p className="mx-auto max-w-4xl px-4 py-10 text-gray-600">Đang khởi tạo phiên đăng nhập…</p>;
  return <main className="mx-auto max-w-4xl px-4 py-10"><h1 className="text-2xl font-bold text-gray-900">Viết công thức mới</h1><p className="mt-2 text-gray-600">Lưu một lần đầy đủ thông tin, nguyên liệu và các bước; sau đó tải ảnh và xem lại.</p><div className="mt-6 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm"><RecipeWizard accessToken={accessToken} /></div></main>;
}
