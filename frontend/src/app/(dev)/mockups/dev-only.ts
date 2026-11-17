import { notFound } from 'next/navigation';

/**
 * Chặn mockup ở production. Gọi ở CẢ layout lẫn từng trang: App Router render layout và page SONG SONG, nên `notFound()` chỉ ở
 * layout vẫn trả 404 nhưng nội dung trang đã kịp nằm trong payload RSC của bản dựng sẵn (đã đo: `next build && next start`,
 * `/mockups/home` → 404 mà HTML vẫn chứa dữ liệu mockup). Gọi trước khi dựng nội dung thì trang không có gì để lộ.
 */
export function assertDevOnly() {
  if (process.env.NODE_ENV === 'production') notFound();
}
