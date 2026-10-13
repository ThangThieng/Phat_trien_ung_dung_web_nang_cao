'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Copy } from 'lucide-react';
import { toast } from 'sonner';
import ImageUploader from '@/components/ui/ImageUploader';
import { useAuth } from '@/features/auth/auth-context';
import type { UploadedFile } from '@/types/api';

/** Thư viện ảnh: upload ảnh lên MinIO và lấy URL công khai (FR-FILE-001/002). */
export default function MediaPage() {
  const { accessToken, isAuthenticated, isReady } = useAuth();
  const [current, setCurrent] = useState<UploadedFile | null>(null);
  const [history, setHistory] = useState<UploadedFile[]>([]);

  const handleChange = (file: UploadedFile | null) => {
    if (file) {
      setHistory((items) => [file, ...items]);
      toast.success('Tải ảnh lên thành công.');
    } else if (current) {
      setHistory((items) => items.filter((item) => item.key !== current.key));
      toast.info('Đã xóa ảnh khỏi kho lưu trữ.');
    }
    setCurrent(file);
  };

  const copyUrl = async (url: string) => {
    await navigator.clipboard.writeText(url);
    toast.success('Đã sao chép URL ảnh.');
  };

  if (isReady && !isAuthenticated) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Thư viện ảnh</h1>
        <p className="mt-2 text-gray-600">Bạn cần đăng nhập để tải ảnh lên.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/media"
          className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white hover:bg-orange-700"
        >
          Đăng nhập
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900">Thư viện ảnh</h1>
      <p className="mt-1 text-sm text-gray-600">
        Ảnh được lưu trên MinIO (bucket public-read). Hỗ trợ JPEG, PNG, WebP, AVIF – tối đa 5MB.
      </p>

      <div className="mt-6">
        <ImageUploader
          accessToken={accessToken}
          value={current}
          onChange={handleChange}
          label="Tải ảnh mới"
        />
      </div>

      {history.length > 0 && (
        <section aria-labelledby="uploaded-heading" className="mt-10">
          <h2 id="uploaded-heading" className="text-lg font-semibold">
            Ảnh đã tải lên trong phiên này
          </h2>
          <ul className="mt-3 divide-y divide-gray-200 rounded-xl border border-gray-200 bg-white">
            {history.map((file) => (
              <li key={file.key} className="flex items-center gap-3 p-3">
                {/* eslint-disable-next-line @next/next/no-img-element -- ảnh từ MinIO, thumbnail nhỏ */}
                <img src={file.url} alt="" className="h-12 w-12 rounded object-cover" />
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-medium text-gray-900">{file.key}</p>
                  <p className="text-xs text-gray-500">
                    {file.contentType} · {(file.size / 1024).toFixed(1)} KB
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => copyUrl(file.url)}
                  className="flex items-center gap-1 rounded-md px-2 py-1 text-sm text-orange-700 hover:bg-orange-50"
                >
                  <Copy aria-hidden className="h-4 w-4" /> Sao chép URL
                </button>
              </li>
            ))}
          </ul>
        </section>
      )}
    </div>
  );
}
