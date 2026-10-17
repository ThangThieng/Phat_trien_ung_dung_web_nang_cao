'use client';

/**
 * BẢN TẠM (seam) của Buổi 6 — Dev 4. Bố cục route group cần component mà ba dev khác làm trong CÙNG buổi; trong lúc
 * chờ merge, file này cung cấp bản tối giản dựng trên `useAuth()` hiện có để bố cục chạy và dashboard vẫn được chặn.
 * Khi merge, chủ component đổi đúng một dòng import ở nơi dùng rồi xóa phần tương ứng (xóa cả file khi hết chỗ dùng):
 *
 * | Bản tạm               | Thay bằng                                              | Ai     |
 * |-----------------------|--------------------------------------------------------|--------|
 * | `InterimAuthGuard`    | `features/auth/components/AuthGuard`                   | Dev 1  |
 * | `InterimRequireRole`  | `features/auth/components/RequireRole`                 | Dev 1  |
 *
 * Buổi 6 — Dev 3 đã thay xong phần của mình: `InterimUserMenu` → `components/layout/UserMenu`, `InterimErrorFallback` →
 * `components/shared/ErrorFallback`, `InterimPageLoading` → `components/shared/LoadingSkeleton`.
 *
 * Chặn route ở client là TRẢI NGHIỆM, không phải bảo mật — quyền thật luôn kiểm ở Backend (NFR-SEC-006).
 */
import { useEffect } from 'react';
import type { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { ShieldAlert } from 'lucide-react';
import LoadingSkeleton from '@/components/shared/LoadingSkeleton';
import { useAuth } from '@/features/auth/auth-context';
import type { Role } from '@/types/auth';

/** Chưa đăng nhập → `/auth/login?callbackUrl=<trang đang mở, đã mã hóa>`. Đang khôi phục phiên → khung chờ. */
export function InterimAuthGuard({ children }: { children: ReactNode }) {
  const { isReady, isAuthenticated } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (!isReady || isAuthenticated) return;
    // Đọc query từ window (chỉ chạy ở client) thay vì useSearchParams — không ép mọi trang dashboard vào Suspense.
    const callbackUrl = `${pathname}${window.location.search}`;
    router.replace(`/auth/login?callbackUrl=${encodeURIComponent(callbackUrl)}`);
  }, [isReady, isAuthenticated, pathname, router]);

  if (!isReady || !isAuthenticated)
    return <LoadingSkeleton variant="table" label="Đang kiểm tra phiên đăng nhập…" />;
  return children;
}

/** Thiếu vai trò → màn hình 403 trong khung dashboard (Author mở trang chỉ dành cho Admin). */
export function InterimRequireRole({
  roles,
  children,
}: {
  roles: readonly Role[];
  children: ReactNode;
}) {
  const { user } = useAuth();
  if (user?.roles.some((role) => roles.includes(role))) return children;
  return (
    <div className="mx-auto max-w-xl px-4 py-16 text-center">
      <ShieldAlert aria-hidden className="mx-auto h-10 w-10 text-orange-700" />
      <h1 className="mt-4 text-2xl font-bold text-gray-900">Không có quyền truy cập</h1>
      <p className="mt-2 text-gray-600">Trang này chỉ dành cho quản trị viên.</p>
      <Link href="/" className="mt-6 inline-block text-orange-700 underline">
        Về trang chủ
      </Link>
    </div>
  );
}
