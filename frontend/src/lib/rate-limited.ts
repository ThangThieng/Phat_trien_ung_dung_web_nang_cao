/** CR-2026-10: dựng và đọc tham số của trang `/too-many-requests` (dùng chung cho server-request-context và Route Handler). */
export const RATE_LIMITED_PATH = '/too-many-requests';

const MAX_RETRY_AFTER_SECONDS = 3600;

/** Số giây chờ hợp lệ: số nguyên 1 … 3600; thiếu hoặc sai → 60 (cửa sổ hạn mức của NFR-SEC-003). */
export function clampRetryAfter(value: number | string | null | undefined): number {
  const seconds = typeof value === 'string' ? Number.parseInt(value, 10) : value;
  if (seconds === undefined || seconds === null || !Number.isFinite(seconds) || seconds < 1) return 60;
  return Math.min(Math.ceil(seconds), MAX_RETRY_AFTER_SECONDS);
}

/** Chỉ nhận đường dẫn nội bộ ("/…", không "//…", không "\\") — tránh dùng trang này để chuyển người xem ra site khác. */
export function safeReturnPath(value: string | null | undefined): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.includes('\\')) return '/';
  if (value.startsWith(RATE_LIMITED_PATH)) return '/';
  return value;
}

export function rateLimitedPath(retryAfterSeconds: number | undefined, from: string | null | undefined): string {
  const params = new URLSearchParams({
    retryAfter: String(clampRetryAfter(retryAfterSeconds)),
    from: safeReturnPath(from),
  });
  return `${RATE_LIMITED_PATH}?${params.toString()}`;
}
