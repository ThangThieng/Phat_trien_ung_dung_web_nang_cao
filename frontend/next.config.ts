import type { NextConfig } from 'next';

// D-14 (Buổi 7 — Dev 4): URL công khai của ảnh là <site>/media (Nginx → bucket MinIO), không còn localhost:9000/culinary-blog.
// Tên biến giữ nguyên để không đổi build arg; đường dẫn cho phép suy ra từ chính URL. Buổi 8 — Dev 3 gỡ `unoptimized` (lib/images.ts).
const mediaUrl = new URL(process.env.NEXT_PUBLIC_MINIO_PUBLIC_URL ?? 'http://localhost/media');

const nextConfig: NextConfig = {
  // Docker multi-stage build (CONS-009) – chỉ copy .next/standalone vào image runtime
  output: 'standalone',
  images: {
    // Ảnh công thức: MinIO (bucket culinary-blog, public-read) phục vụ qua Nginx /media/
    remotePatterns: [
      {
        protocol: mediaUrl.protocol.replace(':', '') as 'http' | 'https',
        hostname: mediaUrl.hostname,
        port: mediaUrl.port,
        pathname: `${mediaUrl.pathname.replace(/\/$/, '')}/**`,
      },
      // Ảnh đại diện tài khoản Google (FR-AUTH-003)
      { protocol: 'https', hostname: 'lh3.googleusercontent.com', pathname: '/**' },
    ],
  },
  // NFR-SEO-002: Next 15.5 mặc định STREAM metadata (title, description, canonical, Open Graph) vào cuối <body> cho trình duyệt
  // thường — thẻ chỉ lên <head> sau khi JavaScript chạy. Lighthouse báo "không có meta description" và mọi bộ cào không chạy JS
  // (trình xem trước liên kết Zalo/Facebook, nhiều công cụ SEO) không thấy `og:*`. Ép chặn render tới khi có metadata cho MỌI
  // user-agent: thẻ luôn nằm trong <head> của HTML đầu tiên. Cái giá: TTFB chờ `generateMetadata`, mà lời gọi API của nó đã được
  // `cache()` + Data Cache dùng chung với trang nên không thêm lượt gọi nào.
  htmlLimitedBots: /.*/,
  eslint: { dirs: ['src'] },
  // Route chính thức theo SRS §5.1 là /auth/login, /auth/register – giữ alias ngắn
  async redirects() {
    return [
      { source: '/login', destination: '/auth/login', permanent: true },
      { source: '/register', destination: '/auth/register', permanent: true },
    ];
  },
};

export default nextConfig;
