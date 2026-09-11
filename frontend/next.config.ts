import type { NextConfig } from 'next';

const minioUrl = new URL(process.env.NEXT_PUBLIC_MINIO_PUBLIC_URL ?? 'http://localhost:9000');

const nextConfig: NextConfig = {
  // Docker multi-stage build (CONS-009) – chỉ copy .next/standalone vào image runtime
  output: 'standalone',
  images: {
    // Ảnh công thức phục vụ từ MinIO (bucket culinary-blog, public-read)
    remotePatterns: [
      {
        protocol: minioUrl.protocol.replace(':', '') as 'http' | 'https',
        hostname: minioUrl.hostname,
        port: minioUrl.port,
        pathname: '/culinary-blog/**',
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
