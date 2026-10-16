import type { ReactNode } from 'react';
import Link from 'next/link';
import { ChefHat } from 'lucide-react';

/**
 * Trang đăng nhập/đăng ký: khung tối giản căn giữa, không header/footer đầy đủ. Đã đăng nhập thì `AuthCard` chuyển
 * hướng (Buổi 2); Dev 1 (Lab 6) bọc `{children}` bằng `GuestOnly` để chuyển hướng về `callbackUrl` an toàn.
 */
export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="flex flex-1 flex-col items-center px-4 pt-10">
      <Link href="/" className="flex items-center gap-2 text-lg font-bold text-orange-700">
        <ChefHat aria-hidden className="h-6 w-6" />
        Culinary Blog
      </Link>
      <main id="main-content" className="flex w-full flex-1 justify-center">
        {children}
      </main>
    </div>
  );
}
