import { headers } from 'next/headers';
import { configureServerRequestContext } from './api-client';

/**
 * Chỉ import từ Server Component (root layout) — `next/headers` không được vào bundle trình duyệt. Cho `apiFetch` phía server
 * đọc `X-Forwarded-For` của request trình duyệt đang được render để chuyển tiếp cho API (rate limit theo IP thật —
 * NFR-SEC-003). Xem `forwardClientAddress` trong `lib/api-client.ts`.
 */
configureServerRequestContext({
  forwardedFor: async () => (await headers()).get('x-forwarded-for'),
});
