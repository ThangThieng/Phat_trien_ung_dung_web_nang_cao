import { headers } from 'next/headers';
import { redirect } from 'next/navigation';
import { configureServerRequestContext } from './api-client';
import { rateLimitedPath } from './rate-limited';

/**
 * Chỉ import từ Server Component (root layout) — `next/headers` không được vào bundle trình duyệt. Cho `apiFetch` phía server
 * đọc `X-Forwarded-For` của request trình duyệt đang được render để chuyển tiếp cho API (rate limit theo IP thật —
 * NFR-SEC-003). Xem `forwardClientAddress` trong `lib/api-client.ts`.
 *
 * CR-2026-10: API trả 429 khi đang render → chuyển sang `/too-many-requests` (Route Handler trả đúng mã 429 + `Retry-After`).
 * Trang của App Router không đặt được mã 429, nên trước đây lỗi này thành trang 500 — bộ thu thập (Googlebot) hiểu là máy chủ hỏng.
 */
configureServerRequestContext({
  forwardedFor: async () => (await headers()).get('x-forwarded-for'),
  onRateLimited: async (error) => {
    const from = (await headers()).get('x-request-path');
    redirect(rateLimitedPath(error.retryAfterSeconds, from));
  },
});
