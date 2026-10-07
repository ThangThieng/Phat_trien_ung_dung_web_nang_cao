/**
 * FR-OBS-001 — Health UI (Buổi 5 — Dev 4).
 *
 * API trả báo cáo theo định dạng của HealthChecks.UI.Client (`/health`):
 * `{ status, totalDuration: "00:00:00.0123456", entries: { database: { status, duration, tags, description? } } }`.
 * Route Handler `/api/health` chuẩn hóa nó về {@link SystemHealth} để giao diện không phụ thuộc định dạng TimeSpan của .NET,
 * và để trạng thái "không gọi được API" có cùng hình dạng với một báo cáo bình thường.
 */

export type HealthStatus = 'Healthy' | 'Degraded' | 'Unhealthy';

export interface HealthEntry {
  /** Tên check đăng ký ở API: `database`, `redis`, `minio` (hoặc `api` khi không gọi được API). */
  name: string;
  status: HealthStatus;
  /** Thời gian phản hồi của riêng check này, mili giây. */
  durationMs: number | null;
  description: string | null;
  tags: string[];
}

export interface SystemHealth {
  status: HealthStatus;
  totalDurationMs: number | null;
  /** Thời điểm Route Handler hỏi API (ISO 8601). */
  checkedAt: string;
  /** false khi Next.js không gọi được API (container api dừng, timeout, response không phải JSON). */
  apiReachable: boolean;
  entries: HealthEntry[];
}

export const SYSTEM_HEALTH_QUERY_KEY = ['system-health'] as const;

/**
 * Tiêu chí nghiệm thu Buổi 5 — Dev 4: mất Redis thì chấm chuyển đỏ trong ≤ 30s. Thời gian phát hiện xấu nhất = chu kỳ poll
 * + thời gian chính lần kiểm tra đó (check Redis chờ hết timeout 3s mới báo lỗi). Poll đúng 30s thì đo thực tế ra 32,4s,
 * nên chọn 25s: 25s + 3s + độ trễ mạng vẫn dưới 30s.
 */
export const HEALTH_POLL_INTERVAL_MS = 25_000;

/** Check có tag `ready` là phụ thuộc bắt buộc của `/health/ready` (PostgreSQL, Redis). */
export const READY_TAG = 'ready';

export const DEPENDENCY_LABEL: Record<string, string> = {
  api: 'API (.NET)',
  database: 'PostgreSQL',
  redis: 'Redis',
  minio: 'MinIO (lưu trữ ảnh)',
};

export const STATUS_LABEL: Record<HealthStatus, string> = {
  Healthy: 'Hoạt động bình thường',
  Degraded: 'Suy giảm',
  Unhealthy: 'Gián đoạn',
};

const ENTRY_ORDER = ['api', 'database', 'redis', 'minio'];

const STATUSES: readonly HealthStatus[] = ['Healthy', 'Degraded', 'Unhealthy'];

/** Trạng thái lạ hoặc thiếu coi như Unhealthy — báo động nhầm an toàn hơn im lặng bỏ qua lỗi. */
function toStatus(value: unknown): HealthStatus {
  return STATUSES.find((status) => status === value) ?? 'Unhealthy';
}

/** TimeSpan dạng "c" của .NET (`[d.]hh:mm:ss[.fffffff]`) → mili giây; sai định dạng → null. */
export function parseTimeSpanMs(value: unknown): number | null {
  if (typeof value !== 'string') return null;
  const match = /^(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.(\d{1,7}))?$/.exec(value.trim());
  if (!match) return null;
  const [, days = '0', hours, minutes, seconds, fraction = ''] = match;
  const wholeSeconds =
    Number(days) * 86_400 + Number(hours) * 3_600 + Number(minutes) * 60 + Number(seconds);
  const fractionMs = fraction ? Number(`0.${fraction}`) * 1_000 : 0;
  return Math.round((wholeSeconds * 1_000 + fractionMs) * 100) / 100;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function orderOf(name: string): number {
  const index = ENTRY_ORDER.indexOf(name);
  return index === -1 ? ENTRY_ORDER.length : index;
}

/** Next.js không gọi được API: chính API là phụ thuộc đang hỏng, còn trạng thái DB/Redis/MinIO là chưa biết. */
export function unreachableHealth(reason: string, checkedAt: Date): SystemHealth {
  return {
    status: 'Unhealthy',
    totalDurationMs: null,
    checkedAt: checkedAt.toISOString(),
    apiReachable: false,
    entries: [
      {
        name: 'api',
        status: 'Unhealthy',
        durationMs: null,
        description: reason,
        tags: [READY_TAG],
      },
    ],
  };
}

/** Báo cáo `/health` của API → {@link SystemHealth}. Không bao giờ ném lỗi: dữ liệu không đọc được cũng là một trạng thái. */
export function normalizeHealthReport(raw: unknown, checkedAt: Date): SystemHealth {
  if (!isRecord(raw))
    return unreachableHealth('API trả về dữ liệu sức khỏe không đọc được.', checkedAt);

  const entries = Object.entries(isRecord(raw.entries) ? raw.entries : {})
    .map(([name, value]): HealthEntry => {
      const entry = isRecord(value) ? value : {};
      return {
        name,
        status: toStatus(entry.status),
        durationMs: parseTimeSpanMs(entry.duration),
        description:
          typeof entry.description === 'string' && entry.description ? entry.description : null,
        tags: Array.isArray(entry.tags)
          ? entry.tags.filter((tag): tag is string => typeof tag === 'string')
          : [],
      };
    })
    .sort((a, b) => orderOf(a.name) - orderOf(b.name) || a.name.localeCompare(b.name));

  return {
    status: toStatus(raw.status),
    totalDurationMs: parseTimeSpanMs(raw.totalDuration),
    checkedAt: checkedAt.toISOString(),
    apiReachable: true,
    entries,
  };
}

/**
 * Trình duyệt gọi Route Handler `/api/health` (cùng origin; Nginx chuyển `= /api/health` về Next.js). Route trả 503 khi
 * Unhealthy nhưng body vẫn là {@link SystemHealth}, nên đọc body ở mọi mã trạng thái. Lỗi mạng (Next.js cũng chết)
 * để TanStack Query nhận như lỗi truy vấn.
 */
export async function fetchSystemHealth(signal?: AbortSignal): Promise<SystemHealth> {
  const response = await fetch('/api/health', {
    cache: 'no-store',
    signal,
    headers: { Accept: 'application/json' },
  });
  try {
    return (await response.json()) as SystemHealth;
  } catch {
    return unreachableHealth(`Máy chủ giao diện trả về mã ${response.status}.`, new Date());
  }
}
