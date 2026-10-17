import type { ReactNode } from 'react';

export type LoadingSkeletonVariant = 'recipe-grid' | 'recipe-detail' | 'table' | 'form';

interface LoadingSkeletonProps {
  variant: LoadingSkeletonVariant;
  /** Số thẻ (`recipe-grid`) hoặc số dòng (`table`). */
  count?: number;
  /** Chữ cho trình đọc màn hình. */
  label?: string;
}

/** Một khối xương. Màu nhạt + `animate-pulse` (tự tắt với `prefers-reduced-motion` qua `motion-reduce:`). */
function Bone({ className }: { className: string }) {
  return (
    <div className={`animate-pulse rounded bg-gray-200 motion-reduce:animate-none ${className}`} />
  );
}

/** Khung ngoài chung: `role="status"` + `aria-busy` + chữ ẩn — trình đọc màn hình báo "Đang tải" (NFR-USE-004). */
function Frame({
  label,
  className,
  children,
}: {
  label: string;
  className: string;
  children: ReactNode;
}) {
  return (
    <div role="status" aria-busy="true" aria-live="polite" className={className}>
      <span className="sr-only">{label}</span>
      {children}
    </div>
  );
}

/**
 * Khung chờ có KÍCH THƯỚC BẰNG ĐÚNG nội dung thật — cùng container, cùng lưới, cùng `aspect-[4/3]` ảnh với `RecipeCard`, cùng
 * chiều cao dòng chữ — nên khi dữ liệu về, trang không nhảy bố cục (CLS ≤ 0,1 — NFR-PERF-005, NFR-USE-004).
 * Thay `RecipeGridSkeleton` (Buổi 2) và `InterimPageLoading` (bản tạm Buổi 6 của Dev 4).
 */
export default function LoadingSkeleton({
  variant,
  count,
  label = 'Đang tải…',
}: LoadingSkeletonProps) {
  if (variant === 'recipe-grid') {
    return (
      <Frame label={label} className="mx-auto max-w-6xl px-4 py-10">
        <Bone className="h-9 w-64" />
        <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: count ?? 6 }, (_, i) => (
            <div key={i} className="overflow-hidden rounded-2xl border border-gray-200 bg-white">
              <Bone className="aspect-[4/3] w-full rounded-none" />
              <div className="space-y-2 p-4">
                <Bone className="h-3 w-20" />
                <Bone className="h-5 w-3/4" />
                <Bone className="h-4 w-full bg-gray-100" />
                <Bone className="h-4 w-2/3 bg-gray-100" />
                <Bone className="mt-3 h-3 w-1/2 bg-gray-100" />
              </div>
            </div>
          ))}
        </div>
      </Frame>
    );
  }

  if (variant === 'recipe-detail') {
    return (
      <Frame label={label} className="mx-auto max-w-5xl px-4 py-10">
        <Bone className="h-4 w-32" />
        <Bone className="mt-3 h-10 w-3/4" />
        <Bone className="mt-4 h-4 w-full bg-gray-100" />
        <Bone className="mt-2 h-4 w-5/6 bg-gray-100" />
        <Bone className="mt-6 aspect-[4/3] w-full rounded-2xl md:aspect-[16/9]" />
        <div className="mt-8 grid gap-8 md:grid-cols-[1fr_2fr]">
          <div className="space-y-3">
            <Bone className="h-6 w-32" />
            {Array.from({ length: 8 }, (_, i) => (
              <Bone key={i} className="h-4 w-full bg-gray-100" />
            ))}
          </div>
          <div className="space-y-4">
            <Bone className="h-6 w-40" />
            {Array.from({ length: 4 }, (_, i) => (
              <Bone key={i} className="h-16 w-full bg-gray-100" />
            ))}
          </div>
        </div>
      </Frame>
    );
  }

  if (variant === 'table') {
    return (
      <Frame label={label} className="mx-auto w-full max-w-5xl px-4 py-8">
        <div className="flex items-center justify-between gap-4">
          <Bone className="h-8 w-56" />
          <Bone className="h-9 w-32 rounded-lg" />
        </div>
        <div className="mt-6 overflow-hidden rounded-xl border border-gray-200 bg-white">
          <Bone className="h-11 w-full rounded-none bg-gray-100" />
          {Array.from({ length: count ?? 6 }, (_, i) => (
            <div key={i} className="flex items-center gap-4 border-t border-gray-100 px-4 py-3">
              <Bone className="h-10 w-10 shrink-0 rounded-lg" />
              <Bone className="h-4 flex-1" />
              <Bone className="hidden h-4 w-24 sm:block" />
              <Bone className="h-4 w-16" />
            </div>
          ))}
        </div>
      </Frame>
    );
  }

  // form — khớp `AuthCard` của /auth/login, /auth/register: tiêu đề + phụ đề ở trên, thẻ trắng chứa các ô nhập.
  return (
    <Frame label={label} className="mx-auto flex w-full max-w-md flex-col gap-6 px-4 py-12">
      <div className="flex flex-col items-center">
        <Bone className="h-8 w-48" />
        <Bone className="mt-2 h-4 w-64 bg-gray-100" />
      </div>
      <div className="space-y-5 rounded-2xl border border-gray-200 bg-white p-6">
        {Array.from({ length: count ?? 3 }, (_, i) => (
          <div key={i} className="space-y-2">
            <Bone className="h-4 w-24" />
            <Bone className="h-10 w-full rounded-lg bg-gray-100" />
          </div>
        ))}
        <Bone className="h-11 w-full rounded-lg" />
      </div>
    </Frame>
  );
}
