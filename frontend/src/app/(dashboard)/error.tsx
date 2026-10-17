'use client';

import { InterimErrorFallback } from '@/components/layout/interim-shell';

/** Lỗi trong một trang dashboard — sidebar và thanh trên cùng vẫn dùng được. Dev 3 thay bằng `ErrorFallback`. */
export default function DashboardError(props: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <InterimErrorFallback {...props} />;
}
