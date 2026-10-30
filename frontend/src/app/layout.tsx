import type { Metadata } from 'next';
import { headers } from 'next/headers';
import type { ReactNode } from 'react';
import { Inter, Geist } from 'next/font/google';
import { cn } from '@/lib/utils';
import Providers from './providers';
// Phía server: apiFetch chuyển tiếp X-Forwarded-For của trình duyệt cho API (rate limit theo IP thật — NFR-SEC-003).
import '@/lib/server-request-context';
import './globals.css';

const geist = Geist({subsets:['latin'],variable:'--font-sans'});


// Subset 'vietnamese' để hiển thị đầy đủ dấu tiếng Việt
const inter = Inter({ subsets: ['latin', 'vietnamese'], variable: '--font-inter' });

export const metadata: Metadata = {
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000'),
  title: { default: 'Culinary Blog', template: '%s | Culinary Blog' },
  description: 'Blog ẩm thực – chia sẻ và khám phá công thức nấu ăn.',
};

export default async function RootLayout({ children }: Readonly<{ children: ReactNode }>) {
  const nonce = (await headers()).get('x-nonce') ?? undefined;
  return (
    <html lang="vi" className={cn("font-sans", geist.variable)}>
      <body
        className={`${inter.variable} flex min-h-screen flex-col bg-orange-50/30 font-sans antialiased`}
      >
        <Providers nonce={nonce}>
          <a
            href="#main-content"
            className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50 focus:rounded focus:bg-white focus:px-3 focus:py-2"
          >
            Bỏ qua điều hướng
          </a>
          {/* Không có header/footer ở đây: mỗi route group — (public), (auth), (dashboard) — có khung riêng và tự
              đặt <main id="main-content"> làm đích cho link "Bỏ qua điều hướng". */}
          {children}
        </Providers>
      </body>
    </html>
  );
}
