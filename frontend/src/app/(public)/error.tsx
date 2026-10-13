'use client';

import { InterimErrorFallback } from '@/components/layout/interim-shell';

/** Lỗi khi render trang công khai — header/footer của group vẫn giữ nguyên. Dev 3 thay bằng `ErrorFallback`. */
export default function PublicError(props: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <InterimErrorFallback {...props} />;
}
