'use client';

import ErrorFallback from '@/components/shared/ErrorFallback';

/** Lỗi trong một trang dashboard — sidebar và thanh trên cùng vẫn dùng được. Câu thân thiện + mã tra cứu + "Thử lại" (NFR-USE-003). */
export default function DashboardError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <ErrorFallback error={error} reset={reset} />;
}
