'use client';

import ErrorFallback from '@/components/shared/ErrorFallback';

/** Lỗi khi render trang công khai — header/footer của group vẫn giữ nguyên. Câu thân thiện + mã tra cứu + "Thử lại" (NFR-USE-003). */
export default function PublicError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <ErrorFallback error={error} reset={reset} />;
}
