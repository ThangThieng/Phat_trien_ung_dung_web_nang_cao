import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { Inter } from 'next/font/google';
import SiteHeader from '@/components/layout/SiteHeader';
import Providers from './providers';
import './globals.css';

// Subset 'vietnamese' để hiển thị đầy đủ dấu tiếng Việt
const inter = Inter({ subsets: ['latin', 'vietnamese'], variable: '--font-inter' });

export const metadata: Metadata = {
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000'),
  title: { default: 'Culinary Blog', template: '%s | Culinary Blog' },
  description: 'Blog ẩm thực – chia sẻ và khám phá công thức nấu ăn.',
};

export default function RootLayout({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <html lang="vi">
      <body
        className={`${inter.variable} flex min-h-screen flex-col bg-orange-50/30 font-sans antialiased`}
      >
        <Providers>
          <a
            href="#main-content"
            className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:rounded focus:bg-white focus:px-3 focus:py-2"
          >
            Bỏ qua điều hướng
          </a>
          <SiteHeader />
          <main id="main-content" className="flex-1">
            {children}
          </main>
          <footer className="border-t border-orange-100 bg-white py-6 text-center text-sm text-gray-500">
            © {new Date().getFullYear()} Culinary Blog – Nhóm 20
          </footer>
        </Providers>
      </body>
    </html>
  );
}
