/**
 * Retrofit D-14 (SRS NFR-PERF-005): ảnh đi qua trình tối ưu của `next/image` (Nginx `/media/` → MinIO).
 *
 * Mặc định BẬT tối ưu. Đặt `NEXT_PUBLIC_IMAGE_UNOPTIMIZED=true` chỉ khi chạy dev mà container Next.js không với tới được
 * địa chỉ ảnh (ví dụ MinIO ở `localhost:9000`) — đúng lý do `unoptimized` từng được thêm ở Buổi 2.
 */
export const IMAGE_UNOPTIMIZED = process.env.NEXT_PUBLIC_IMAGE_UNOPTIMIZED === 'true';

/** `sizes` cho lưới thẻ công thức: 4 cột ≥ 1200 px là 25vw, 2 cột ≥ 768 px là 50vw, còn lại 100vw. */
export const GRID_IMAGE_SIZES = '(min-width: 1200px) 25vw, (min-width: 768px) 50vw, 100vw';

/*
 * Không dùng `fill` cho next/image: `fill` dựa vào thuộc tính `style="position:absolute;…"` trong HTML, mà CSP production
 * (`style-src` không có `'unsafe-inline'`) chặn thuộc tính style → ảnh rơi về `position: static`, cao vài chục px, vỡ thẻ và
 * gây CLS. Thay bằng `width`/`height` (tỉ lệ thật, để Next sinh srcset) + lớp Tailwind `absolute inset-0 h-full w-full`
 * trong khung `relative` có `aspect-*`. Kiểm bằng `e2e/responsive.spec.ts` ("ảnh lấp đầy khung").
 */
