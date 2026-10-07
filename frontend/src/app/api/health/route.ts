import { getApiBaseUrl } from '@/lib/config';
import { normalizeHealthReport, unreachableHealth } from '@/features/system/health';
import type { SystemHealth } from '@/features/system/health';

/**
 * FR-OBS-001 — proxy `/health` của API cho Health UI (Buổi 5 — Dev 4).
 *
 * Gọi từ server Next.js qua địa chỉ nội bộ (`http://api:8080`), nên vẫn trả được một báo cáo có cấu trúc khi chính API
 * đã chết — điều mà trình duyệt gọi thẳng `/health` qua Nginx không làm được (Nginx chỉ trả trang 502 HTML).
 */
export const dynamic = 'force-dynamic';

/** Health check của API tự có timeout 3s/check (MinIO); 5s đủ cho cả ba check chạy song song cộng độ trễ mạng. */
const API_TIMEOUT_MS = 5_000;

/** `/health` nằm ở gốc API, ngoài `/api/v1` — lấy origin của base URL phía server. */
function apiHealthUrl(): string {
  return new URL('/health', getApiBaseUrl()).toString();
}

export async function GET(): Promise<Response> {
  const checkedAt = new Date();
  let report: SystemHealth;
  try {
    const response = await fetch(apiHealthUrl(), {
      cache: 'no-store',
      headers: { Accept: 'application/json' },
      signal: AbortSignal.timeout(API_TIMEOUT_MS),
    });
    // 503 khi Unhealthy vẫn mang báo cáo JSON đầy đủ — đọc body ở mọi mã trạng thái.
    report = normalizeHealthReport(await response.json(), checkedAt);
  } catch (error) {
    const timedOut = error instanceof DOMException && error.name === 'TimeoutError';
    report = unreachableHealth(
      timedOut
        ? `API không phản hồi trong ${API_TIMEOUT_MS / 1_000} giây.`
        : 'Không kết nối được tới API.',
      checkedAt,
    );
  }

  return Response.json(report, {
    status: report.status === 'Unhealthy' ? 503 : 200,
    headers: { 'Cache-Control': 'no-store' },
  });
}
