'use client';

import { InterimErrorFallback } from '@/components/layout/interim-shell';

/** Dev 3 thay bằng `ErrorFallback`. */
export default function AuthError(props: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return <InterimErrorFallback {...props} />;
}
