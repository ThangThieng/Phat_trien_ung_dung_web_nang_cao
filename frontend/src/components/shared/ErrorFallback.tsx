'use client';

import { AlertTriangle, RotateCcw } from 'lucide-react';
import { normalizeError } from '@/lib/error-handler';

interface ErrorFallbackProps {
  error: unknown;
  /** Bấm "Thử lại": `reset` của `error.tsx` hoặc của `ErrorBoundary` (đã nối với `QueryErrorResetBoundary`). */
  reset?: () => void;
  title?: string;
  /** `page` (mặc định) cho `error.tsx` — `h1`, nhiều khoảng trắng; `inline` cho một khối trong trang — `h2`, gọn. */
  variant?: 'page' | 'inline';
}

/**
 * Giao diện lỗi dùng chung (NFR-USE-003): câu thân thiện theo mã lỗi (`normalizeError` — bộ xử lý lỗi của Dev 4), MÃ TRA CỨU
 * để báo cho quản trị viên, nút thử lại. Không bao giờ hiển thị stack trace hay thông điệp kỹ thuật.
 * Mã tra cứu: `correlationId` mà `ApiError` mang theo; lỗi ném từ Server Component bị Next.js ẩn nội dung ở production, chỉ
 * còn `digest` — dùng làm mã tra cứu (tra được trong log của máy chủ Next.js).
 */
export default function ErrorFallback({
  error,
  reset,
  title = 'Đã có lỗi xảy ra',
  variant = 'page',
}: ErrorFallbackProps) {
  const appError = normalizeError(error);
  const digest =
    typeof error === 'object' && error !== null && 'digest' in error
      ? String((error as { digest?: unknown }).digest ?? '')
      : '';
  const lookupCode = appError.correlationId ?? (digest || undefined);
  const Heading = variant === 'page' ? 'h1' : 'h2';

  return (
    <div
      role="alert"
      className={
        variant === 'page'
          ? 'mx-auto flex max-w-xl flex-col items-center px-4 py-20 text-center'
          : 'flex flex-col items-center rounded-2xl border border-red-100 bg-red-50/40 px-6 py-8 text-center'
      }
    >
      <AlertTriangle aria-hidden className="h-10 w-10 text-orange-700" />
      <Heading className="mt-4 text-2xl font-bold text-gray-900">{title}</Heading>
      <p className="mt-2 text-gray-700">{appError.message}</p>
      {lookupCode && (
        <p className="mt-2 text-xs text-gray-600">
          Mã tra cứu: <code className="font-mono select-all">{lookupCode}</code>
        </p>
      )}
      {reset && (
        <button
          type="button"
          onClick={reset}
          className="mt-6 inline-flex items-center gap-2 rounded-lg bg-orange-700 px-4 py-2 font-semibold text-white hover:bg-orange-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-700"
        >
          <RotateCcw aria-hidden className="h-4 w-4" />
          Thử lại
        </button>
      )}
    </div>
  );
}
