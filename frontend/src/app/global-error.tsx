'use client';

import ErrorFallback from '@/components/shared/ErrorFallback';
import './globals.css';

/**
 * Lỗi ở chính root layout (`app/layout.tsx`) — lúc đó không còn layout nào bọc ngoài, nên file này tự dựng `<html>`/`<body>`
 * và tự nạp CSS. Không có Providers (Query, Auth, toast) ở đây: `ErrorFallback` chỉ dùng hàm thuần `normalizeError`.
 */
export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <html lang="vi">
      <body className="flex min-h-screen flex-col bg-orange-50/30 font-sans antialiased">
        <main id="main-content" className="flex-1">
          <ErrorFallback error={error} reset={reset} />
        </main>
      </body>
    </html>
  );
}
