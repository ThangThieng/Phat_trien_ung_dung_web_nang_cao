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
 * | `InterimUserMenu`     | `UserMenu` (DropdownMenu shadcn)                       | Dev 3  |
 * | `InterimErrorFallback`| `components/shared/ErrorFallback`                      | Dev 3  |
 * | `InterimPageLoading`  | `components/shared/LoadingSkeleton`                    | Dev 3  |
 *
 * Chặn route ở client là TRẢI NGHIỆM, không phải bảo mật — quyền thật luôn kiểm ở Backend (NFR-SEC-006).
 */
import { useEffect } from 'react';
import type { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { LogOut, ShieldAlert } from 'lucide-react';
import { toast } from 'sonner';
import { useAuth } from '@/features/auth/auth-context';
import { normalizeError } from '@/lib/error-handler';
import type { Role } from '@/types/auth';

/** Khung chờ chung — `aria-busy` + chữ cho trình đọc màn hình (NFR-USE-004). */
export function InterimPageLoading({ label = 'Đang tải…' }: { label?: string }) {
  return (
    <div className="mx-auto w-full max-w-5xl px-4 py-10" aria-busy="true" aria-live="polite">
      <span className="sr-only">{label}</span>
      <div className="h-8 w-56 animate-pulse rounded bg-gray-200" />
      <div className="mt-6 space-y-3">
        <div className="h-4 w-full animate-pulse rounded bg-gray-100" />
        <div className="h-4 w-5/6 animate-pulse rounded bg-gray-100" />
        <div className="h-4 w-2/3 animate-pulse rounded bg-gray-100" />
      </div>
    </div>
  );
}

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
    return <InterimPageLoading label="Đang kiểm tra phiên đăng nhập…" />;
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

/** Tên người dùng + nút đăng xuất trên thanh trên cùng của dashboard. */
export function InterimUserMenu() {
  const { user, logout } = useAuth();
  if (!user) return null;
  const handleLogout = async () => {
    await logout();
    toast.info('Bạn đã đăng xuất.');
  };
  return (
    <div className="flex items-center gap-2">
      <span className="hidden text-sm font-medium text-gray-900 sm:inline" title={user.email}>
        {user.displayName}
      </span>
      <button
        type="button"
        onClick={handleLogout}
        aria-label="Đăng xuất"
        className="rounded-md p-2 text-gray-600 hover:bg-orange-50 hover:text-orange-700"
      >
        <LogOut aria-hidden className="h-4 w-4" />
      </button>
    </div>
  );
}

/**
 * Giao diện lỗi của `error.tsx`: câu thân thiện theo mã lỗi + mã tra cứu, không lộ stack trace (NFR-USE-003).
 * Lỗi ném từ Server Component bị Next.js ẩn nội dung ở production, chỉ còn `digest` — dùng làm mã tra cứu.
 */
export function InterimErrorFallback({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const appError = normalizeError(error);
  const lookupCode = appError.correlationId ?? error.digest;
  return (
    <div
      role="alert"
      className="mx-auto flex max-w-xl flex-col items-center px-4 py-20 text-center"
    >
      <h1 className="text-2xl font-bold text-gray-900">Đã có lỗi xảy ra</h1>
      <p className="mt-2 text-gray-600">{appError.message}</p>
      {lookupCode && (
        <p className="mt-2 text-xs text-gray-500">
          Mã tra cứu: <code className="font-mono">{lookupCode}</code>
        </p>
      )}
      <button
        type="button"
        onClick={reset}
        className="mt-6 rounded-lg bg-orange-700 px-4 py-2 font-semibold text-white hover:bg-orange-800"
      >
        Thử lại
      </button>
    </div>
  );
}
