/**
 * Font cho ảnh Open Graph (`ImageResponse` của `next/og`, chạy runtime edge).
 *
 * Máy chủ Next.js trong container không ra Internet, còn satori mặc định tải font động từ Google khi gặp ký tự lạ → chữ tiếng
 * Việt có dấu thiếu glyph. Nhúng sẵn Inter Bold (OFL, `src/assets/fonts`) hai tập con khác tên, ghép bằng
 * `font-family: Inter, InterVietnamese` — satori không tự dự phòng giữa hai font cùng tên.
 * `fetch(new URL('…', import.meta.url))` là cách Next.js hướng dẫn: bundler edge đổi thành tài nguyên đóng gói sẵn.
 */
export const OG_FONT_FAMILY = 'Inter, InterVietnamese';

export async function loadOgFonts() {
  const [latin, vietnamese] = await Promise.all([
    fetch(new URL('../../assets/fonts/inter-latin-700.woff', import.meta.url)).then((r) =>
      r.arrayBuffer(),
    ),
    fetch(new URL('../../assets/fonts/inter-vietnamese-700.woff', import.meta.url)).then((r) =>
      r.arrayBuffer(),
    ),
  ]);
  return [
    { name: 'Inter', data: latin, weight: 700 as const, style: 'normal' as const },
    { name: 'InterVietnamese', data: vietnamese, weight: 700 as const, style: 'normal' as const },
  ];
}
