'use client';

import { useState } from 'react';
import Link from 'next/link';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';
import RecipeBasicsForm from '@/features/recipes/components/RecipeBasicsForm';
import RecipeImageGallery from '@/features/recipes/components/RecipeImageGallery';
import type { RecipeDetail } from '@/types/api';

const STEPS = ['Thông tin & dinh dưỡng', 'Ảnh món ăn'] as const;

/**
 * FR-RCP-003 + FR-RCP-008 – trình soạn công thức nhiều bước (Buổi 3): bước 1 tạo bản nháp, bước 2 tải ảnh và chọn ảnh
 * chính. Nguyên liệu, các bước nấu và xuất bản thuộc Buổi 4–5 (FR-RCP-009/010/005).
 */
export default function NewRecipePage() {
  const { accessToken, isAuthenticated, isReady } = useAuth();
  const [draft, setDraft] = useState<RecipeDetail | null>(null);
  const step = draft ? 1 : 0;

  if (isReady && !isAuthenticated) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Viết công thức mới</h1>
        <p className="mt-2 text-gray-600">Bạn cần đăng nhập để viết công thức.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/recipes/new"
          className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white hover:bg-orange-700"
        >
          Đăng nhập
        </Link>
      </div>
    );
  }

  const handleCreated = (recipe: RecipeDetail) => {
    setDraft(recipe);
    toast.success(`Đã lưu nháp "${recipe.title}". Tiếp theo: thêm ảnh món ăn.`);
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900">Viết công thức mới</h1>

      <ol className="mt-6 flex gap-4" aria-label="Các bước soạn công thức">
        {STEPS.map((label, index) => (
          <li
            key={label}
            aria-current={index === step ? 'step' : undefined}
            className={`flex items-center gap-2 text-sm ${index === step ? 'font-semibold text-orange-700' : 'text-gray-500'}`}
          >
            <span
              className={`flex h-6 w-6 items-center justify-center rounded-full text-xs ${
                index <= step ? 'bg-orange-600 text-white' : 'bg-gray-200 text-gray-600'
              }`}
            >
              {index + 1}
            </span>
            {label}
          </li>
        ))}
      </ol>

      <div className="mt-6 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        {accessToken && !draft && (
          <RecipeBasicsForm accessToken={accessToken} onCreated={handleCreated} />
        )}
        {accessToken && draft && (
          <>
            <p className="mb-4 text-sm text-gray-600">
              Bản nháp <span className="font-semibold text-gray-900">{draft.title}</span> đã được
              lưu (chưa công khai).
            </p>
            <RecipeImageGallery
              recipeId={draft.id}
              recipeTitle={draft.title}
              accessToken={accessToken}
            />
          </>
        )}
      </div>
    </div>
  );
}
