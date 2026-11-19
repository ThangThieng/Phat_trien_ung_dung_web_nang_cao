import type { NextRequest } from 'next/server';
import { clampRetryAfter, safeReturnPath } from '@/lib/rate-limited';

/**
 * CR-2026-10 (SRS v1.2.8, MT-73): trang báo "quá nhiều yêu cầu" với đúng mã HTTP 429 + `Retry-After` (NFR-SEC-003).
 *
 * Trang của App Router không đặt được mã 429 — API trả 429 khi đang render thì trước đây người xem nhận trang 500, còn bộ thu
 * thập (Googlebot) hiểu là máy chủ hỏng. Route Handler đặt được mã trạng thái, nên `apiFetch` phía server chuyển sang đây.
 * HTML tối giản, không cần JavaScript; style mang nonce của CSP do middleware cấp.
 */
export const dynamic = 'force-dynamic';

const escapeHtml = (value: string) =>
  value.replace(/[&<>"']/g, (char) => `&#${char.charCodeAt(0)};`);

export function GET(request: NextRequest) {
  const retryAfter = clampRetryAfter(request.nextUrl.searchParams.get('retryAfter'));
  const from = escapeHtml(safeReturnPath(request.nextUrl.searchParams.get('from')));
  const nonce = escapeHtml(request.headers.get('x-nonce') ?? '');

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta name="robots" content="noindex">
<title>Quá nhiều yêu cầu | Culinary Blog</title>
<style nonce="${nonce}">
body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:#f3ebd6;color:#1a2432;font:16px/1.6 system-ui,sans-serif}
main{max-width:30rem;margin:1.5rem;padding:2rem;border:1px solid #dccfa9;border-radius:1rem;background:#fbf6e9;text-align:center}
h1{margin:0 0 .5rem;font:700 1.6rem Georgia,serif}
p{margin:.5rem 0;color:#5a5444}
a{display:inline-block;margin-top:1rem;padding:.6rem 1.2rem;border-radius:.5rem;background:#0c1f38;color:#fff;font-weight:600;text-decoration:none}
a:focus-visible{outline:2px solid #7a5614;outline-offset:2px}
</style>
</head>
<body>
<main>
<h1>Bạn thao tác hơi nhanh</h1>
<p>Hệ thống đang nhận quá nhiều yêu cầu từ mạng của bạn.</p>
<p>Vui lòng đợi khoảng <strong>${retryAfter} giây</strong> rồi thử lại.</p>
<a href="${from}">Thử lại</a>
</main>
</body>
</html>`;

  return new Response(html, {
    status: 429,
    headers: {
      'Content-Type': 'text/html; charset=utf-8',
      'Retry-After': String(retryAfter),
      'Cache-Control': 'no-store',
      'X-Robots-Tag': 'noindex',
    },
  });
}
