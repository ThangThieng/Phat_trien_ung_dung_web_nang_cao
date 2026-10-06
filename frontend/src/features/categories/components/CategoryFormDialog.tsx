'use client';

import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2, X } from 'lucide-react';
import FormField from '@/components/ui/FormField';
import ImageUploader from '@/components/ui/ImageUploader';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import type { Category, UploadedFile } from '@/types/api';
import type { CategoryPayload } from '../admin-api';

/**
 * Mirror đúng validator backend (SRS §7.9 – MT-37): name 2–100, description ≤ 2000, imageUrl ≤ 500 và là URL hợp lệ.
 * Hai bên PHẢI bằng nhau; khi đổi giới hạn thì đổi cả CategoryLimits.cs lẫn schema này.
 */
const categorySchema = z.object({
  name: z
    .string()
    .trim()
    .min(2, 'Tên danh mục phải từ 2 đến 100 ký tự.')
    .max(100, 'Tên danh mục phải từ 2 đến 100 ký tự.')
    .refine((v) => !v.includes('<') && !v.includes('>'), 'Tên danh mục không được chứa ký tự < hoặc >.'),
  description: z.string().trim().max(2000, 'Mô tả tối đa 2000 ký tự.'),
  orderIndex: z.string().trim().regex(/^\d+$/, 'Thứ tự hiển thị phải là số nguyên lớn hơn hoặc bằng 0.'),
});

type CategoryFormValues = z.infer<typeof categorySchema>;

interface CategoryFormDialogProps {
  /** null = tạo mới; có giá trị = sửa danh mục đó. */
  category: Category | null;
  accessToken: string | null;
  onSubmit: (payload: CategoryPayload) => Promise<void>;
  onClose: () => void;
}

/** Form tạo/sửa danh mục (FR-CAT-003 / FR-CAT-004) trong hộp thoại. */
export default function CategoryFormDialog({
  category,
  accessToken,
  onSubmit,
  onClose,
}: CategoryFormDialogProps) {
  const isEditing = category !== null;
  const firstFieldRef = useRef<HTMLInputElement | null>(null);
  const [image, setImage] = useState<UploadedFile | null>(
    category?.imageUrl
      ? { key: '', url: category.imageUrl, contentType: '', size: 0 }
      : null,
  );
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<CategoryFormValues>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: category?.name ?? '',
      description: category?.description ?? '',
      orderIndex: String(category?.orderIndex ?? 0),
    },
  });

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handleEscape);
    firstFieldRef.current?.focus();
    return () => window.removeEventListener('keydown', handleEscape);
  }, [onClose]);

  const submit = handleSubmit(async (values) => {
    setFormError(null);
    try {
      await onSubmit({
        name: values.name,
        description: values.description.length > 0 ? values.description : null,
        imageUrl: image?.url ?? null,
        orderIndex: Number(values.orderIndex),
      });
    } catch (error) {
      // Backend trả RFC 7807: 409 = trùng tên (CATEGORY_NAME_EXISTS), errors{} = lỗi từng field.
      if (error instanceof ApiError) {
        if (error.code === 'CATEGORY_NAME_EXISTS') {
          setError('name', { message: 'Tên danh mục này đã tồn tại. Vui lòng chọn tên khác.' });
          return;
        }
        const fieldErrors = Object.entries(error.fieldErrors);
        if (fieldErrors.length > 0) {
          fieldErrors.forEach(([field, messages]) => {
            if (field === 'name' || field === 'description' || field === 'orderIndex') {
              setError(field, { message: messages[0] });
            }
          });
          return;
        }
      }
      setFormError(getErrorMessage(error));
    }
  });

  const { ref: nameRef, ...nameField } = register('name');

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="category-dialog-title"
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-xl"
      >
        <div className="flex items-start justify-between gap-4">
          <h2 id="category-dialog-title" className="text-lg font-bold text-gray-900">
            {isEditing ? 'Sửa danh mục' : 'Thêm danh mục'}
          </h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="Đóng"
            className="rounded-md p-1 text-gray-500 hover:bg-gray-100"
          >
            <X aria-hidden className="h-5 w-5" />
          </button>
        </div>

        {isEditing && (
          <p className="mt-1 text-xs text-gray-500">
            Đường dẫn <code className="rounded bg-gray-100 px-1">/{category.slug}</code> không đổi khi
            sửa tên, để link đã chia sẻ không bị hỏng.
          </p>
        )}

        <form onSubmit={submit} className="mt-4 flex flex-col gap-4" noValidate>
          <FormField
            label="Tên danh mục"
            error={errors.name?.message}
            hint="2–100 ký tự. Đường dẫn được sinh tự động từ tên."
            {...nameField}
            ref={(element) => {
              nameRef(element);
              firstFieldRef.current = element;
            }}
          />

          <div className="flex flex-col gap-1">
            <label htmlFor="category-description" className="flex flex-col gap-1">
              <span className="text-sm font-medium text-gray-800">Mô tả</span>
              <textarea
                id="category-description"
                rows={3}
                aria-invalid={errors.description ? true : undefined}
                aria-describedby={errors.description ? 'category-description-error' : undefined}
                className={`rounded-lg border px-3 py-2 text-sm shadow-sm transition outline-none focus:ring-2 focus:ring-orange-500 ${
                  errors.description ? 'border-red-500' : 'border-gray-300'
                }`}
                {...register('description')}
              />
            </label>
            {errors.description && (
              <p id="category-description-error" role="alert" className="text-sm text-red-600">
                {errors.description.message}
              </p>
            )}
          </div>

          <FormField
            label="Thứ tự hiển thị"
            type="number"
            min={0}
            error={errors.orderIndex?.message}
            hint="Số nhỏ hiển thị trước."
            {...register('orderIndex')}
          />

          <ImageUploader
            accessToken={accessToken}
            value={image}
            onChange={setImage}
            label="Ảnh đại diện danh mục"
            // Ảnh của danh mục đang sửa có thể đang được dùng ở nơi khác → bỏ chọn chỉ gỡ khỏi form,
            // không xóa file trên MinIO.
            deleteOnRemove={!isEditing}
          />

          {formError && (
            <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
              {formError}
            </p>
          )}

          <div className="mt-2 flex justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex items-center gap-2 rounded-lg bg-orange-600 px-4 py-2 text-sm font-semibold text-white hover:bg-orange-700 disabled:opacity-60"
            >
              {isSubmitting && <Loader2 aria-hidden className="h-4 w-4 animate-spin" />}
              {isEditing ? 'Lưu thay đổi' : 'Tạo danh mục'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
