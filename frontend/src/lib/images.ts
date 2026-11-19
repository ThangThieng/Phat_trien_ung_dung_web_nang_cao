/**
 * Retrofit D-14 (SRS NFR-PERF-005): ảnh đi qua trình tối ưu của `next/image` (Nginx `/media/` → MinIO).
 *
 * Mặc định BẬT tối ưu. `NEXT_PUBLIC_IMAGE_UNOPTIMIZED=true` tắt hẳn (chạy `next dev` ngoài Docker, không có MinIO).
 */
export const IMAGE_UNOPTIMIZED = process.env.NEXT_PUBLIC_IMAGE_UNOPTIMIZED === 'true';

/**
 * Đường dẫn ảnh đưa cho `next/image` (CR-2026-10 — đo Lighthouse 18/11/2026).
 *
 * Bộ tối ưu chạy TRONG container Next.js: với URL tuyệt đối `http://localhost/media/…` nó gọi chính container (localhost) chứ
 * không tới Nginx → `/_next/image` trả 500, nên trước đây ở `localhost` ảnh bỏ qua bộ tối ưu và điện thoại nhận ảnh 800 px gốc
 * (LCP mobile ~4 s). Đổi ảnh của chính hệ thống (cùng tiền tố `NEXT_PUBLIC_MINIO_PUBLIC_URL`) thành đường dẫn tương đối
 * `/media/…`: bộ tối ưu đọc ảnh qua rewrite nội bộ `/media/*` → MinIO (`next.config.ts`), chạy được ở mọi môi trường.
 * URL khác (ảnh Google…) giữ nguyên, đi theo `remotePatterns`.
 */
export function toImageSrc(url: string, mediaBaseUrl = process.env.NEXT_PUBLIC_MINIO_PUBLIC_URL): string {
  if (!mediaBaseUrl) return url;
  const base = mediaBaseUrl.replace(/\/$/, '');
  if (url !== base && !url.startsWith(`${base}/`)) return url;
  return `/media${url.slice(base.length)}`;
}

/** `sizes` cho lưới thẻ công thức: 4 cột ≥ 1200 px là 25vw, 2 cột ≥ 768 px là 50vw, còn lại 100vw. */
export const GRID_IMAGE_SIZES = '(min-width: 1200px) 25vw, (min-width: 768px) 50vw, 100vw';

/*
 * Không dùng `fill` cho next/image: `fill` dựa vào thuộc tính `style="position:absolute;…"` trong HTML, mà CSP production
 * (`style-src` không có `'unsafe-inline'`) chặn thuộc tính style → ảnh rơi về `position: static`, cao vài chục px, vỡ thẻ và
 * gây CLS. Thay bằng `width`/`height` (tỉ lệ thật, để Next sinh srcset) + lớp Tailwind `absolute inset-0 h-full w-full`
 * trong khung `relative` có `aspect-*`. Kiểm bằng `e2e/responsive.spec.ts` ("ảnh lấp đầy khung").
 */
