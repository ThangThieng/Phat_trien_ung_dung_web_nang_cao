import type { NextConfig } from 'next';

const minioUrl = new URL(process.env.NEXT_PUBLIC_MINIO_PUBLIC_URL ?? 'http://localhost:9000');
// D-14: ảnh công thức phục vụ qua Nginx `/media/` (cùng domain với site) — SRS §6.5
const siteUrl = new URL(process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000');
const protocolOf = (url: URL) => url.protocol.replace(':', '') as 'http' | 'https';

const nextConfig: NextConfig = {
  // Docker multi-stage build (CONS-009) – chỉ copy .next/standalone vào image runtime
  output: 'standalone',
  images: {
    // Ảnh công thức phục vụ từ MinIO (bucket culinary-blog, public-read)
    remotePatterns: [
      // Ảnh công thức qua Nginx `/media/` → MinIO (D-14)
      { protocol: protocolOf(siteUrl), hostname: siteUrl.hostname, port: siteUrl.port, pathname: '/media/**' },
      // Dự phòng khi chạy dev, truy cập MinIO trực tiếp
      {
        protocol: protocolOf(minioUrl),
        hostname: minioUrl.hostname,
        port: minioUrl.port,
        pathname: '/culinary-blog/**',
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
