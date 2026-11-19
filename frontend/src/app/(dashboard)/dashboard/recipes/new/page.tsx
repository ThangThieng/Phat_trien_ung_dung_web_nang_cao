'use client';

import Link from 'next/link';
import { useAuth } from '@/features/auth/auth-context';
import RecipeWizard from '@/features/recipes/components/RecipeWizard';

/** FR-RCP-003 – five-step wizard; POST once with ingredients and steps before the image step. */
export default function NewRecipePage() {
  const { accessToken, isAuthenticated, isReady } = useAuth();
  if (isReady && !isAuthenticated)
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Viết công thức mới</h1>
        <p className="text-ink-soft mt-2">Bạn cần đăng nhập để viết công thức.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/recipes/new"
          className="bg-navy mt-6 inline-block rounded-lg px-4 py-2 font-semibold text-white"
        >
          Đăng nhập
        </Link>
      </div>
    );
  if (!accessToken)
    return (
      <p className="text-ink-soft mx-auto max-w-4xl px-4 py-10">Đang khởi tạo phiên đăng nhập…</p>
    );
  return (
    <div className="mx-auto max-w-4xl px-4 py-10">
      <h1 className="text-ink text-2xl font-bold">Viết công thức mới</h1>
      <p className="text-ink-soft mt-2">
        Lưu một lần đầy đủ thông tin, nguyên liệu và các bước; sau đó tải ảnh và xem lại.
      </p>
      <div className="border-line bg-paper mt-6 rounded-2xl border p-6 shadow-sm">
        <RecipeWizard accessToken={accessToken} />
      </div>
    </div>
  );
}
