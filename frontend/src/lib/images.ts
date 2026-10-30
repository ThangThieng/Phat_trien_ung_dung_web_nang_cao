/**
 * Retrofit D-14 (SRS NFR-PERF-005): ảnh đi qua trình tối ưu của `next/image` (Nginx `/media/` → MinIO).
 *
 * Mặc định BẬT tối ưu. Đặt `NEXT_PUBLIC_IMAGE_UNOPTIMIZED=true` chỉ khi chạy dev mà container Next.js không với tới được
 * địa chỉ ảnh (ví dụ MinIO ở `localhost:9000`) — đúng lý do `unoptimized` từng được thêm ở Buổi 2.
 */
export const IMAGE_UNOPTIMIZED = process.env.NEXT_PUBLIC_IMAGE_UNOPTIMIZED === 'true';

/** `sizes` cho lưới thẻ công thức: 4 cột ≥ 1200 px là 25vw, 2 cột ≥ 768 px là 50vw, còn lại 100vw. */
export const GRID_IMAGE_SIZES = '(min-width: 1200px) 25vw, (min-width: 768px) 50vw, 100vw';
