'use client';

import { useCallback, useEffect, useId, useRef, useState } from 'react';
import type { ChangeEvent, DragEvent, KeyboardEvent } from 'react';
import { ImagePlus, Loader2, Trash2 } from 'lucide-react';
import { getApiBaseUrl } from '@/lib/config';
import { ApiError, apiFetch, getErrorMessage } from '@/lib/api-client';
import type { ProblemDetails } from '@/lib/api-client';
import type { UploadedFile } from '@/types/api';

/** Ràng buộc CONS-007 (kiểm tra sớm phía client; backend vẫn kiểm tra lại MIME + magic bytes). */
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/avif'];

export interface ImageUploaderProps {
  /** Access token (Bearer) – endpoint upload yêu cầu đăng nhập. */
  accessToken: string | null;
  /** Ảnh hiện tại (controlled). */
  value?: UploadedFile | null;
  onChange?: (file: UploadedFile | null) => void;
  label?: string;
  /** Có gọi API xóa file trên MinIO khi bấm nút xóa hay không (mặc định: có). */
  deleteOnRemove?: boolean;
  disabled?: boolean;
}

function validateFile(file: File): string | null {
  if (!ACCEPTED_IMAGE_TYPES.includes(file.type))
    return 'Chỉ chấp nhận ảnh JPEG, PNG, WebP hoặc AVIF.';
  if (file.size > MAX_IMAGE_BYTES) return 'Kích thước ảnh vượt quá giới hạn 5MB.';
  return null;
}

/** Upload qua XMLHttpRequest để có tiến trình % realtime (NFR-USE-004) – fetch() chưa hỗ trợ upload progress. */
function uploadWithProgress(
  file: File,
  accessToken: string,
  onProgress: (percent: number) => void,
): { promise: Promise<UploadedFile>; abort: () => void } {
  const xhr = new XMLHttpRequest();
  const promise = new Promise<UploadedFile>((resolve, reject) => {
    xhr.open('POST', `${getApiBaseUrl()}/files/upload`);
    xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
    xhr.responseType = 'json';
    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) onProgress(Math.round((event.loaded / event.total) * 100));
    };
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve(xhr.response as UploadedFile);
      else
        reject(
          new ApiError(xhr.status, (xhr.response ?? { status: xhr.status }) as ProblemDetails),
        );
    };
    xhr.onerror = () => reject(new Error('network'));
    xhr.onabort = () => reject(new DOMException('Upload bị hủy', 'AbortError'));

    const form = new FormData();
    form.append('file', file);
    xhr.send(form);
  });
  return { promise, abort: () => xhr.abort() };
}

/**
 * FR-FILE-001/002 – component upload ảnh tái sử dụng: kéo-thả / chọn file, xem trước, tiến trình %, xóa ảnh.
 * Dùng cho: ảnh công thức (FR-RCP-008), avatar (FR-AUTH-007), ảnh bước (FR-RCP-010), ảnh danh mục (FR-CAT-003).
 */
export default function ImageUploader({
  accessToken,
  value = null,
  onChange,
  label = 'Ảnh',
  deleteOnRemove = true,
  disabled = false,
}: ImageUploaderProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const abortRef = useRef<(() => void) | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const isUploading = progress !== null;
  const isDisabled = disabled || !accessToken || isUploading || isDeleting;
  const imageSrc = preview ?? value?.url ?? null;

  // Giải phóng object URL của ảnh preview
  useEffect(
    () => () => {
      if (preview) URL.revokeObjectURL(preview);
    },
    [preview],
  );

  useEffect(() => () => abortRef.current?.(), []);

  const startUpload = useCallback(
    async (file: File) => {
      if (!accessToken) return;
      const validationError = validateFile(file);
      if (validationError) {
        setError(validationError);
        return;
      }

      setError(null);
      setPreview(URL.createObjectURL(file));
      setProgress(0);

      const { promise, abort } = uploadWithProgress(file, accessToken, setProgress);
      abortRef.current = abort;
      try {
        const uploaded = await promise;
        onChange?.(uploaded);
      } catch (err) {
        if (!(err instanceof DOMException && err.name === 'AbortError'))
          setError(getErrorMessage(err));
        setPreview(null);
      } finally {
        abortRef.current = null;
        setProgress(null);
      }
    },
    [accessToken, onChange],
  );

  const handleFiles = (files: FileList | null) => {
    const file = files?.[0];
    if (file) startUpload(file);
  };

  const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
    handleFiles(event.target.files);
    if (inputRef.current) inputRef.current.value = '';
  };

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setIsDragging(false);
    if (!isDisabled) handleFiles(event.dataTransfer.files);
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      if (!isDisabled) inputRef.current?.click();
    }
  };

  const handleRemove = async () => {
    if (!value) {
      setPreview(null);
      return;
    }
    setError(null);
    if (deleteOnRemove && accessToken) {
      setIsDeleting(true);
      try {
        await apiFetch<void>(`/files/${value.key.split('/').map(encodeURIComponent).join('/')}`, {
          method: 'DELETE',
          accessToken,
        });
      } catch (err) {
        setError(getErrorMessage(err));
        setIsDeleting(false);
        return;
      }
      setIsDeleting(false);
    }
    setPreview(null);
    onChange?.(null);
  };

  return (
    <div className="flex flex-col gap-2">
      <span id={`${inputId}-label`} className="text-sm font-medium text-gray-800">
        {label}
      </span>

      {imageSrc ? (
        <div className="relative overflow-hidden rounded-xl border border-gray-200 bg-gray-50">
          {/* eslint-disable-next-line @next/next/no-img-element -- preview blob: URL không đi qua next/image */}
          <img src={imageSrc} alt="Ảnh xem trước" className="aspect-video w-full object-cover" />
          {isUploading && (
            <div className="absolute inset-x-0 bottom-0 bg-black/60 px-3 py-2">
              <div
                role="progressbar"
                aria-label="Tiến trình tải ảnh lên"
                aria-valuemin={0}
                aria-valuemax={100}
                aria-valuenow={progress ?? 0}
                className="h-2 overflow-hidden rounded-full bg-white/30"
              >
                <div
                  className="h-full bg-orange-500 transition-all"
                  style={{ width: `${progress ?? 0}%` }}
                />
              </div>
              <p className="mt-1 text-xs text-white">Đang tải lên… {progress}%</p>
            </div>
          )}
          {!isUploading && (
            <button
              type="button"
              onClick={handleRemove}
              disabled={isDeleting}
              aria-label="Xóa ảnh"
              className="absolute top-2 right-2 rounded-full bg-white/90 p-2 text-red-600 shadow hover:bg-white disabled:opacity-60"
            >
              {isDeleting ? (
                <Loader2 aria-hidden className="h-4 w-4 animate-spin" />
              ) : (
                <Trash2 aria-hidden className="h-4 w-4" />
              )}
            </button>
          )}
        </div>
      ) : (
        <div
          role="button"
          tabIndex={isDisabled ? -1 : 0}
          aria-labelledby={`${inputId}-label`}
          aria-describedby={`${inputId}-hint`}
          aria-disabled={isDisabled}
          onClick={() => !isDisabled && inputRef.current?.click()}
          onKeyDown={handleKeyDown}
          onDragOver={(event) => {
            event.preventDefault();
            if (!isDisabled) setIsDragging(true);
          }}
          onDragLeave={() => setIsDragging(false)}
          onDrop={handleDrop}
          className={`flex aspect-video cursor-pointer flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed px-4 text-center transition focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:outline-none ${
            isDragging
              ? 'border-orange-500 bg-orange-50'
              : 'border-gray-300 bg-gray-50 hover:border-orange-400'
          } ${isDisabled ? 'cursor-not-allowed opacity-60' : ''}`}
        >
          <ImagePlus aria-hidden className="h-8 w-8 text-orange-600" />
          <p className="text-sm font-medium text-gray-800">Kéo thả ảnh vào đây hoặc bấm để chọn</p>
          <p id={`${inputId}-hint`} className="text-xs text-gray-500">
            {accessToken
              ? 'JPEG, PNG, WebP, AVIF – tối đa 5MB'
              : 'Vui lòng đăng nhập để tải ảnh lên'}
          </p>
        </div>
      )}

      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept={ACCEPTED_IMAGE_TYPES.join(',')}
        className="sr-only"
        tabIndex={-1}
        aria-hidden
        onChange={handleInputChange}
        disabled={isDisabled}
      />

      {error && (
        <p role="alert" className="text-sm text-red-600">
          {error}
        </p>
      )}
    </div>
  );
}
