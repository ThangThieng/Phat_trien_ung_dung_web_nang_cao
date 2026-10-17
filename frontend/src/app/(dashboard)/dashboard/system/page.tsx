'use client';

import Link from 'next/link';
import { Loader2, RefreshCw } from 'lucide-react';
import { useAuth } from '@/features/auth/auth-context';
import HealthIndicator, {
  STATUS_DOT,
  useSystemHealth,
} from '@/features/system/components/HealthIndicator';
import {
  DEPENDENCY_LABEL,
  HEALTH_POLL_INTERVAL_MS,
  READY_TAG,
  STATUS_LABEL,
} from '@/features/system/health';

const STATUS_TEXT = {
  Healthy: 'text-green-800 bg-green-50 ring-green-200',
  Degraded: 'text-amber-900 bg-amber-50 ring-amber-200',
  Unhealthy: 'text-red-800 bg-red-50 ring-red-200',
} as const;

function formatDuration(ms: number | null): string {
  if (ms === null) return '—';
  return ms < 1_000
    ? `${ms.toLocaleString('vi-VN', { maximumFractionDigits: 1 })} ms`
    : `${(ms / 1_000).toFixed(2)} s`;
}

function SystemHealthReport() {
  const { data, isPending, isError, isFetching, refetch } = useSystemHealth();

  return (
    <div className="mx-auto max-w-5xl px-4 py-10">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Tình trạng hệ thống</h1>
          <p className="mt-1 text-sm text-gray-600">
            Kết quả health check của API, tự làm mới mỗi {HEALTH_POLL_INTERVAL_MS / 1_000} giây.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <HealthIndicator showLabel />
          <button
            type="button"
            onClick={() => refetch()}
            disabled={isFetching}
            className="flex items-center gap-2 rounded-lg border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
          >
            <RefreshCw aria-hidden className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
            Kiểm tra lại
          </button>
        </div>
      </div>

      {isPending && (
        <p className="mt-8 flex items-center gap-2 text-sm text-gray-500">
          <Loader2 aria-hidden className="h-4 w-4 animate-spin" /> Đang hỏi API…
        </p>
      )}

      {isError && !data && (
        <p role="alert" className="mt-8 rounded-lg bg-red-50 p-4 text-sm text-red-800">
          Không gọi được máy chủ giao diện để lấy tình trạng hệ thống. Kiểm tra container frontend.
        </p>
      )}

      {data && (
        <>
          <dl className="mt-6 grid gap-4 sm:grid-cols-3">
            <div className="rounded-xl border border-gray-200 bg-white p-4">
              <dt className="text-xs font-medium text-gray-500 uppercase">Tổng thể</dt>
              <dd className="mt-1 font-semibold text-gray-900">{STATUS_LABEL[data.status]}</dd>
            </div>
            <div className="rounded-xl border border-gray-200 bg-white p-4">
              <dt className="text-xs font-medium text-gray-500 uppercase">
                Tổng thời gian kiểm tra
              </dt>
              <dd className="mt-1 font-semibold text-gray-900">
                {formatDuration(data.totalDurationMs)}
              </dd>
            </div>
            <div className="rounded-xl border border-gray-200 bg-white p-4">
              <dt className="text-xs font-medium text-gray-500 uppercase">Lần kiểm tra gần nhất</dt>
              <dd className="mt-1 font-semibold text-gray-900">
                {new Date(data.checkedAt).toLocaleTimeString('vi-VN')}
              </dd>
            </div>
          </dl>

          <div className="mt-6 overflow-x-auto rounded-xl border border-gray-200 bg-white">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <caption className="sr-only">Trạng thái từng phụ thuộc của hệ thống</caption>
              <thead className="bg-gray-50 text-left text-xs font-semibold text-gray-600 uppercase">
                <tr>
                  <th scope="col" className="px-4 py-3">
                    Thành phần
                  </th>
                  <th scope="col" className="px-4 py-3">
                    Trạng thái
                  </th>
                  <th scope="col" className="px-4 py-3">
                    Thời gian phản hồi
                  </th>
                  <th scope="col" className="px-4 py-3">
                    Khi lỗi
                  </th>
                  <th scope="col" className="px-4 py-3">
                    Chi tiết
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.entries.map((entry) => (
                  <tr key={entry.name}>
                    <th
                      scope="row"
                      className="px-4 py-3 text-left font-medium whitespace-nowrap text-gray-900"
                    >
                      {DEPENDENCY_LABEL[entry.name] ?? entry.name}
                    </th>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap ring-1 ${STATUS_TEXT[entry.status]}`}
                      >
                        <span
                          aria-hidden
                          className={`h-2 w-2 rounded-full ${STATUS_DOT[entry.status]}`}
                        />
                        {STATUS_LABEL[entry.status]}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-700 tabular-nums">
                      {formatDuration(entry.durationMs)}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {entry.tags.includes(READY_TAG)
                        ? 'API ngừng nhận traffic (/health/ready trả 503)'
                        : 'Chỉ suy giảm — ảnh không tải lên/hiển thị được'}
                    </td>
                    <td className="max-w-xs px-4 py-3 break-words text-gray-600">
                      {entry.description ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {!data.apiReachable && (
            <p className="mt-4 text-sm text-gray-600">
              Không hỏi được API nên chưa biết trạng thái của PostgreSQL, Redis và MinIO.
            </p>
          )}
        </>
      )}
    </div>
  );
}

/** FR-OBS-001 — trang tình trạng hệ thống (chỉ Admin), SRS §5.1 nhóm /dashboard. Buổi 5 — Dev 4. */
export default function SystemHealthPage() {
  const { user, isAuthenticated, isReady } = useAuth();
  const isAdmin = user?.roles.includes('Admin') ?? false;

  if (!isReady) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-16 text-center text-gray-500">
        <Loader2 aria-hidden className="mx-auto h-6 w-6 animate-spin" />
        <p className="mt-2 text-sm">Đang kiểm tra phiên đăng nhập…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Tình trạng hệ thống</h1>
        <p className="mt-2 text-gray-600">Bạn cần đăng nhập bằng tài khoản quản trị để tiếp tục.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/system"
          className="mt-6 inline-block rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white hover:bg-orange-700"
        >
          Đăng nhập
        </Link>
      </div>
    );
  }

  if (!isAdmin) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Không có quyền truy cập</h1>
        <p className="mt-2 text-gray-600">
          Trang tình trạng hệ thống chỉ dành cho quản trị viên. Tài khoản của bạn không có quyền
          này.
        </p>
        <Link href="/" className="mt-6 inline-block text-orange-700 underline">
          Về trang chủ
        </Link>
      </div>
    );
  }

  return <SystemHealthReport />;
}
