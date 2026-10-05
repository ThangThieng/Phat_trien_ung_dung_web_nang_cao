'use client';

import { useId, useRef, useState } from 'react';
import type { ChangeEvent } from 'react';
import { ImagePlus, Loader2, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { ACCEPTED_IMAGE_TYPES, MAX_IMAGE_BYTES } from '@/components/ui/ImageUploader';
import { getErrorMessage } from '@/lib/api-client';
import type { RecipeImageResult } from '@/types/api';
import { deleteRecipeImage, updateRecipeImage, uploadRecipeImage } from '../write-api';

interface RecipeImageGalleryProps {
  recipeId: string;
  recipeTitle: string;
  accessToken: string;
}

/**
 * Wizard bước 2 (FR-RCP-008): thư viện ảnh của công thức. Ảnh đầu tiên tự thành ảnh chính; chọn ảnh chính bằng radio
 * (PATCH isPrimary — server bảo đảm chỉ một ảnh chính); xóa ảnh chính thì server tự chọn ảnh thay thế nên danh sách
 * được đồng bộ lại theo quy tắc OrderIndex nhỏ nhất.
 */
export default function RecipeImageGallery({
  recipeId,
  recipeTitle,
  accessToken,
}: RecipeImageGalleryProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const [images, setImages] = useState<RecipeImageResult[]>([]);
  const [uploading, setUploading] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const handleFiles = async (event: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    if (inputRef.current) inputRef.current.value = '';
    setUploading(true);
    try {
      // Tuần tự: ảnh đầu tiên của công thức phải là ảnh chính — tải song song thì thứ tự không xác định.
      // eslint-disable-next-line no-restricted-syntax -- cần await tuần tự theo thứ tự người dùng chọn
      for (const file of files) {
        if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
          toast.error(`${file.name}: chỉ chấp nhận JPEG, PNG, WebP hoặc AVIF.`);
        } else if (file.size > MAX_IMAGE_BYTES) {
          toast.error(`${file.name}: vượt quá giới hạn 5MB.`);
        } else {
          // eslint-disable-next-line no-await-in-loop -- xem chú thích phía trên
          const uploaded = await uploadRecipeImage(recipeId, file, accessToken, {
            altText: recipeTitle,
          });
          setImages((current) => [...current, uploaded]);
        }
      }
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setUploading(false);
    }
  };

  const makePrimary = async (imageId: string) => {
    setBusyId(imageId);
    try {
      await updateRecipeImage(recipeId, imageId, { isPrimary: true }, accessToken);
      setImages((current) =>
        current.map((image) => ({ ...image, isPrimary: image.imageId === imageId })),
      );
      toast.success('Đã đổi ảnh chính.');
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setBusyId(null);
    }
  };

  const remove = async (target: RecipeImageResult) => {
    setBusyId(target.imageId);
    try {
      await deleteRecipeImage(recipeId, target.imageId, accessToken);
      setImages((current) => {
        const rest = current.filter((image) => image.imageId !== target.imageId);
        if (!target.isPrimary || rest.length === 0) return rest;
        const next = [...rest].sort((a, b) => a.orderIndex - b.orderIndex)[0];
        return rest.map((image) => ({ ...image, isPrimary: image.imageId === next.imageId }));
      });
      toast.info('Đã xóa ảnh.');
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setBusyId(null);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <label
        htmlFor={inputId}
        className="flex cursor-pointer flex-col items-center gap-2 rounded-xl border-2 border-dashed border-orange-200 bg-orange-50/40 p-6 text-center text-sm text-gray-700 hover:border-orange-400"
      >
        {uploading ? (
          <Loader2 aria-hidden className="h-6 w-6 animate-spin text-orange-600" />
        ) : (
          <ImagePlus aria-hidden className="h-6 w-6 text-orange-600" />
        )}
        <span>
          {uploading
            ? 'Đang tải ảnh lên…'
            : 'Chọn một hoặc nhiều ảnh (JPEG, PNG, WebP, AVIF – tối đa 5MB)'}
        </span>
        <input
          ref={inputRef}
          id={inputId}
          type="file"
          multiple
          accept={ACCEPTED_IMAGE_TYPES.join(',')}
          disabled={uploading}
          onChange={handleFiles}
          className="sr-only"
        />
      </label>

      {images.length === 0 ? (
        <p className="text-sm text-gray-500">
          Chưa có ảnh nào. Ảnh đầu tiên sẽ tự động là ảnh chính.
        </p>
      ) : (
        <fieldset>
          <legend className="mb-2 text-sm font-semibold text-gray-800">Chọn ảnh chính</legend>
          <ul className="grid gap-3 sm:grid-cols-3">
            {images.map((image) => (
              <li
                key={image.imageId}
                className="overflow-hidden rounded-xl border border-gray-200 bg-white"
              >
                {/* eslint-disable-next-line @next/next/no-img-element -- ảnh gốc trên MinIO, biến thể resize chưa có */}
                <img
                  src={image.originalUrl}
                  alt={image.altText ?? ''}
                  className="aspect-video w-full object-cover"
                />
                <div className="flex items-center justify-between gap-2 p-2">
                  <label
                    htmlFor={`primary-${image.imageId}`}
                    className="flex items-center gap-2 text-sm"
                  >
                    <input
                      id={`primary-${image.imageId}`}
                      type="radio"
                      name={`primary-${recipeId}`}
                      checked={image.isPrimary}
                      disabled={busyId !== null}
                      onChange={() => makePrimary(image.imageId)}
                    />
                    Ảnh chính
                  </label>
                  <button
                    type="button"
                    onClick={() => remove(image)}
                    disabled={busyId !== null}
                    aria-label="Xóa ảnh"
                    className="rounded-md p-1.5 text-gray-500 hover:bg-red-50 hover:text-red-600 disabled:opacity-50"
                  >
                    <Trash2 aria-hidden className="h-4 w-4" />
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </fieldset>
      )}
    </div>
  );
}
