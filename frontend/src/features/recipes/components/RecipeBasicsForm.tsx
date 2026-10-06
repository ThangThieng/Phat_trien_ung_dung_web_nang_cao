'use client';

import { useEffect, useId, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import FormField from '@/components/ui/FormField';
import { apiFetch, getErrorMessage, mapProblemDetailsToForm } from '@/lib/api-client';
import type { Category, RecipeDetail } from '@/types/api';
import { DIFFICULTY_LABEL } from '../format';
import {
  DIFFICULTIES,
  NUTRITION_FIELDS,
  RECIPE_BASICS_FIELDS,
  recipeBasicsSchema,
  toCreateRecipeRequest,
  type RecipeBasicsValues,
} from '../schemas';
import { createRecipe } from '../write-api';

interface RecipeBasicsFormProps {
  accessToken: string;
  onCreated: (recipe: RecipeDetail) => void;
}

const selectClass =
  'rounded-lg border px-3 py-2 text-sm shadow-sm outline-none focus:ring-2 focus:ring-orange-500';

/**
 * Wizard bước 1 (FR-RCP-003): thông tin cơ bản + dinh dưỡng, tạo công thức ở trạng thái Draft.
 * Dinh dưỡng gửi cùng body (Owned Entity — MT-02), không có bước/endpoint riêng.
 */
export default function RecipeBasicsForm({ accessToken, onCreated }: RecipeBasicsFormProps) {
  const [categories, setCategories] = useState<Category[]>([]);
  const ids = {
    category: useId(),
    difficulty: useId(),
    description: useId(),
    instructions: useId(),
  };
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RecipeBasicsValues>({
    resolver: zodResolver(recipeBasicsSchema),
    defaultValues: { difficulty: 'Easy', instructions: '', categoryId: '' },
  });

  useEffect(() => {
    apiFetch<Category[]>('/categories')
      .then(setCategories)
      .catch(() => setCategories([]));
  }, []);

  const onSubmit = handleSubmit(async (values) => {
    try {
      onCreated(await createRecipe(toCreateRecipeRequest(values), accessToken));
    } catch (error) {
      // D-11: lỗi 400 gắn vào đúng ô (kể cả categoryId không tồn tại — FR-RCP-003 A2).
      if (mapProblemDetailsToForm(error, RECIPE_BASICS_FIELDS, setError)) return;
      setError('root', { message: getErrorMessage(error) });
    }
  });

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      {errors.root && (
        <div
          role="alert"
          className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
        >
          {errors.root.message}
        </div>
      )}

      <FormField label="Tiêu đề" error={errors.title?.message} {...register('title')} />

      <div className="flex flex-col gap-1">
        <label
          htmlFor={ids.description}
          className="flex flex-col gap-1 text-sm font-medium text-gray-800"
        >
          <span>Mô tả</span>
          <textarea
            id={ids.description}
            rows={3}
            aria-invalid={errors.description ? true : undefined}
            className={`${selectClass} ${errors.description ? 'border-red-500' : 'border-gray-300'}`}
            {...register('description')}
          />
        </label>
        {errors.description && (
          <p role="alert" className="text-sm text-red-600">
            {errors.description.message}
          </p>
        )}
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="flex flex-col gap-1">
          <label
            htmlFor={ids.category}
            className="flex flex-col gap-1 text-sm font-medium text-gray-800"
          >
            <span>Danh mục</span>
            <select
              id={ids.category}
              aria-invalid={errors.categoryId ? true : undefined}
              className={`${selectClass} ${errors.categoryId ? 'border-red-500' : 'border-gray-300'}`}
              {...register('categoryId')}
            >
              <option value="">— Chọn danh mục —</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </label>
          {errors.categoryId && (
            <p role="alert" className="text-sm text-red-600">
              {errors.categoryId.message}
            </p>
          )}
        </div>
        <div className="flex flex-col gap-1">
          <label
            htmlFor={ids.difficulty}
            className="flex flex-col gap-1 text-sm font-medium text-gray-800"
          >
            <span>Độ khó</span>
            <select
              id={ids.difficulty}
              className={`${selectClass} border-gray-300`}
              {...register('difficulty')}
            >
              {DIFFICULTIES.map((difficulty) => (
                <option key={difficulty} value={difficulty}>
                  {DIFFICULTY_LABEL[difficulty]}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <FormField
          label="Chuẩn bị (phút)"
          type="number"
          min={1}
          error={errors.prepTime?.message}
          {...register('prepTime', { valueAsNumber: true })}
        />
        <FormField
          label="Nấu (phút)"
          type="number"
          min={0}
          error={errors.cookTime?.message}
          {...register('cookTime', { valueAsNumber: true })}
        />
        <FormField
          label="Khẩu phần"
          type="number"
          min={1}
          error={errors.servings?.message}
          {...register('servings', { valueAsNumber: true })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor={ids.instructions}
          className="flex flex-col gap-1 text-sm font-medium text-gray-800"
        >
          <span>Ghi chú hướng dẫn chung (tùy chọn)</span>
          <textarea
            id={ids.instructions}
            rows={3}
            className={`${selectClass} ${errors.instructions ? 'border-red-500' : 'border-gray-300'}`}
            {...register('instructions')}
          />
        </label>
        <p className="text-xs text-gray-500">
          Các bước nấu chi tiết được thêm riêng sau khi tạo nháp.
        </p>
        {errors.instructions && (
          <p role="alert" className="text-sm text-red-600">
            {errors.instructions.message}
          </p>
        )}
      </div>

      <fieldset className="rounded-xl border border-gray-200 p-4">
        <legend className="px-1 text-sm font-semibold text-gray-800">
          Dinh dưỡng mỗi khẩu phần (tùy chọn)
        </legend>
        <div className="grid gap-3 sm:grid-cols-3">
          {NUTRITION_FIELDS.map(({ key, label }) => (
            <FormField
              key={key}
              label={label}
              type="number"
              min={0}
              step="0.1"
              error={errors[key]?.message}
              {...register(key, { valueAsNumber: true })}
            />
          ))}
        </div>
      </fieldset>

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-end rounded-lg bg-orange-600 px-5 py-2.5 font-semibold text-white transition hover:bg-orange-700 focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 disabled:opacity-60"
      >
        {isSubmitting ? 'Đang lưu nháp…' : 'Lưu nháp & tiếp tục'}
      </button>
    </form>
  );
}
