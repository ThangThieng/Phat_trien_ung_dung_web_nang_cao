import type { NextConfig } from 'next';

// D-14 (Buổi 7): URL công khai của ảnh là <site>/media (Nginx → bucket MinIO), không còn localhost:9000/culinary-blog.
// Tên biến giữ nguyên để không đổi build arg; đường dẫn cho phép suy ra từ chính URL. Gỡ `unoptimized` là việc Buổi 8 — Dev 3.
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
    ],
  },
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
