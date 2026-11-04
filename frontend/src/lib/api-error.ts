import type { ProblemDetails } from '@/types/api';

/** Thông tin đọc từ response, không nằm trong body Problem Details. */
export interface ApiErrorMeta {
  /**
   * `X-Correlation-ID` Backend trả lại (Buổi 8). Không đọc được header (ví dụ XHR không có quyền) thì lấy `correlationId` của
   * Problem Details — cùng giá trị — rồi mới tới `traceId`.
   */
  correlationId?: string;
  /** Header `Retry-After` của 429/503 đã đổi ra giây. */
  retryAfterSeconds?: number;
}

/** Lỗi HTTP từ Backend: giữ nguyên Problem Details (RFC 7807, `type` = Application Error Code – SRS Phụ lục B). */
export class ApiError extends Error {
  readonly status: number;

  readonly code: string;

  readonly fieldErrors: Record<string, string[]>;

  readonly problem: ProblemDetails;

  readonly correlationId?: string;

  readonly retryAfterSeconds?: number;

  constructor(status: number, problem: ProblemDetails, meta: ApiErrorMeta = {}) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.code = problem.type ?? 'UNKNOWN_ERROR';
    this.fieldErrors = problem.errors ?? {};
    this.problem = problem;
    this.correlationId =
      meta.correlationId ??
      (typeof problem.correlationId === 'string' ? problem.correlationId : undefined) ??
      (typeof problem.traceId === 'string' ? problem.traceId : undefined);
    this.retryAfterSeconds = meta.retryAfterSeconds;
  }
}

/**
 * `Retry-After` (RFC 9110 §10.2.3): số giây hoặc một mốc HTTP-date. Trả về số giây (làm tròn lên, không âm),
 * `undefined` khi thiếu hoặc sai định dạng.
 */
export function parseRetryAfter(
  value: string | null | undefined,
  now = Date.now(),
): number | undefined {
  if (!value) return undefined;
  const trimmed = value.trim();
  if (/^\d+$/.test(trimmed)) return Number(trimmed);
  const date = Date.parse(trimmed);
  if (Number.isNaN(date)) return undefined;
  return Math.max(0, Math.ceil((date - now) / 1000));
}

/** Request bị hủy chủ động (rời trang, TanStack Query hủy truy vấn, người dùng bấm hủy tải lên) — không phải lỗi để báo. */
export function isAbortError(error: unknown): boolean {
  return (
    typeof error === 'object' &&
    error !== null &&
    (error as { name?: unknown }).name === 'AbortError'
  );
}
