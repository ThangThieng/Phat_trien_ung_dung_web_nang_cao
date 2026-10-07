'use client';

import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import {
  HEALTH_POLL_INTERVAL_MS,
  STATUS_LABEL,
  SYSTEM_HEALTH_QUERY_KEY,
  fetchSystemHealth,
} from '../health';
import type { HealthStatus } from '../health';

export const STATUS_DOT: Record<HealthStatus, string> = {
  Healthy: 'bg-green-500',
  Degraded: 'bg-amber-400',
  Unhealthy: 'bg-red-600',
};

/** Poll định kỳ; dùng chung query key với trang /dashboard/system nên hai nơi luôn hiển thị cùng một kết quả. */
export function useSystemHealth() {
  return useQuery({
    queryKey: SYSTEM_HEALTH_QUERY_KEY,
    queryFn: ({ signal }) => fetchSystemHealth(signal),
    refetchInterval: HEALTH_POLL_INTERVAL_MS,
    // Mỗi lần poll là một lần đo mới — không thử lại (thử lại chỉ làm chậm việc báo đỏ) và không coi kết quả cũ là "tươi".
    staleTime: 0,
    retry: false,
  });
}

/**
 * FR-OBS-001 — chấm xanh/vàng/đỏ cho Admin (Buổi 5 — Dev 4): xanh = mọi phụ thuộc bình thường, vàng = Degraded
 * (MinIO lỗi — ảnh không tải được nhưng vẫn đọc/ghi công thức), đỏ = Unhealthy (PostgreSQL/Redis/API lỗi).
 * Next.js cũng không phản hồi (truy vấn lỗi) → đỏ.
 */
export default function HealthIndicator({ showLabel = false }: { showLabel?: boolean }) {
  const { data, isError } = useSystemHealth();
  const status: HealthStatus | null = data?.status ?? (isError ? 'Unhealthy' : null);
  const label = status ? STATUS_LABEL[status] : 'Đang kiểm tra…';

  return (
    <Link
      href="/dashboard/system"
      title={`Tình trạng hệ thống: ${label}`}
      className="flex items-center gap-2 rounded-md px-2 py-1 text-sm text-gray-700 hover:text-orange-700"
    >
      <span
        aria-hidden
        data-testid="health-dot"
        data-status={status ?? 'pending'}
        className={`h-2.5 w-2.5 rounded-full ${status ? STATUS_DOT[status] : 'animate-pulse bg-gray-300'}`}
      />
      <span role="status" aria-live="polite" className={showLabel ? '' : 'sr-only'}>
        {`Tình trạng hệ thống: ${label}`}
      </span>
    </Link>
  );
}
