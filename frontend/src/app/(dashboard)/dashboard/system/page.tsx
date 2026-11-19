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
          <h1 className="text-ink text-2xl font-bold">Tình trạng hệ thống</h1>
          <p className="text-ink-soft mt-1 text-sm">
            Kết quả kiểm tra tình trạng của API, tự làm mới mỗi {HEALTH_POLL_INTERVAL_MS / 1_000}{' '}
            giây.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <HealthIndicator showLabel />
          <button
            type="button"
            onClick={() => refetch()}
            disabled={isFetching}
            className="border-line-strong text-ink hover:bg-paper flex items-center gap-2 rounded-lg border px-3 py-1.5 text-sm font-medium disabled:opacity-60"
          >
            <RefreshCw aria-hidden className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
            Kiểm tra lại
          </button>
        </div>
      </div>

      {isPending && (
        <p className="text-ink-soft mt-8 flex items-center gap-2 text-sm">
          <Loader2 aria-hidden className="h-4 w-4 animate-spin" /> Đang hỏi API…
        </p>
      )}

      {isError && !data && (
        <p role="alert" className="mt-8 rounded-lg bg-red-50 p-4 text-sm text-red-800">
          Không gọi được máy chủ giao diện để lấy tình trạng hệ thống. Kiểm tra dịch vụ giao diện.
        </p>
      )}

      {data && (
        <>
          <dl className="mt-6 grid gap-4 sm:grid-cols-3">
            <div className="border-line bg-paper rounded-xl border p-4">
              <dt className="text-ink-soft text-xs font-medium uppercase">Tổng thể</dt>
              <dd className="text-ink mt-1 font-semibold">{STATUS_LABEL[data.status]}</dd>
            </div>
            <div className="border-line bg-paper rounded-xl border p-4">
              <dt className="text-ink-soft text-xs font-medium uppercase">
                Tổng thời gian kiểm tra
              </dt>
              <dd className="text-ink mt-1 font-semibold">
                {formatDuration(data.totalDurationMs)}
              </dd>
            </div>
            <div className="border-line bg-paper rounded-xl border p-4">
              <dt className="text-ink-soft text-xs font-medium uppercase">Lần kiểm tra gần nhất</dt>
              <dd className="text-ink mt-1 font-semibold">
                {new Date(data.checkedAt).toLocaleTimeString('vi-VN')}
              </dd>
            </div>
          </dl>

          <div className="border-line bg-paper mt-6 overflow-x-auto rounded-xl border">
            <table className="divide-line min-w-full divide-y text-sm">
              <caption className="sr-only">Trạng thái từng phụ thuộc của hệ thống</caption>
              <thead className="bg-paper text-ink-soft text-left text-xs font-semibold uppercase">
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
              <tbody className="divide-line divide-y">
                {data.entries.map((entry) => (
                  <tr key={entry.name}>
                    <th
                      scope="row"
                      className="text-ink px-4 py-3 text-left font-medium whitespace-nowrap"
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
                    <td className="text-ink px-4 py-3 tabular-nums">
                      {formatDuration(entry.durationMs)}
                    </td>
                    <td className="text-ink-soft px-4 py-3">
                      {entry.tags.includes(READY_TAG)
                        ? 'API ngừng nhận yêu cầu (/health/ready trả 503)'
                        : 'Chỉ suy giảm — ảnh không tải lên/hiển thị được'}
                    </td>
                    <td className="text-ink-soft max-w-xs px-4 py-3 break-words">
                      {entry.description ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {!data.apiReachable && (
            <p className="text-ink-soft mt-4 text-sm">
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
      <div className="text-ink-soft mx-auto max-w-5xl px-4 py-16 text-center">
        <Loader2 aria-hidden className="mx-auto h-6 w-6 animate-spin" />
        <p className="mt-2 text-sm">Đang kiểm tra phiên đăng nhập…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center">
        <h1 className="text-2xl font-bold">Tình trạng hệ thống</h1>
        <p className="text-ink-soft mt-2">Bạn cần đăng nhập bằng tài khoản quản trị để tiếp tục.</p>
        <Link
          href="/auth/login?callbackUrl=/dashboard/system"
          className="bg-navy hover:bg-navy-2 mt-6 inline-block rounded-lg px-4 py-2 font-semibold text-white"
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
        <p className="text-ink-soft mt-2">
          Trang tình trạng hệ thống chỉ dành cho quản trị viên. Tài khoản của bạn không có quyền
          này.
        </p>
        <Link href="/" className="text-gold-ink mt-6 inline-block underline">
          Về trang chủ
        </Link>
      </div>
    );
  }

  return <SystemHealthReport />;
}
