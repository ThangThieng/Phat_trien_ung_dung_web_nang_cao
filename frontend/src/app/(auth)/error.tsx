'use client';

import ErrorFallback from '@/components/shared/ErrorFallback';

/** Lỗi trong trang đăng nhập/đăng ký. Câu thân thiện + mã tra cứu + "Thử lại" (NFR-USE-003). */
export default function AuthError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <ErrorFallback error={error} reset={reset} />;
}
