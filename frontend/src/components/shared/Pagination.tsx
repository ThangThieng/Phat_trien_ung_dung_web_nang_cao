import Link from 'next/link';
import { ChevronLeft, ChevronRight } from 'lucide-react';

export type PageItem = number | 'ellipsis-start' | 'ellipsis-end';

/** Tổng số ô số trang luôn hiển thị khi có nhiều trang: đầu + cuối + trang hiện tại ± 1 + 2 dấu `…`. */
const SLOTS = 7;

/**
 * Danh sách ô của thanh phân trang. Từ 8 trang trở lên luôn trả đúng 7 ô (số hoặc `…`) ở MỌI vị trí, nên thanh phân trang
 * không đổi bề rộng khi chuyển trang (CLS — NFR-PERF-005):
 * - gần đầu:  1 2 3 4 5 … 20
 * - ở giữa:   1 … 9 10 11 … 20
 * - gần cuối: 1 … 16 17 18 19 20
 */
export function getPageItems(page: number, totalPages: number): PageItem[] {
  if (totalPages <= SLOTS) return Array.from({ length: totalPages }, (_, i) => i + 1);

  const current = Math.min(Math.max(page, 1), totalPages);
  const range = (from: number, to: number) =>
    Array.from({ length: to - from + 1 }, (_, i) => from + i);

  if (current <= 4) return [...range(1, 5), 'ellipsis-end', totalPages];
  if (current >= totalPages - 3) return [1, 'ellipsis-start', ...range(totalPages - 4, totalPages)];
  return [1, 'ellipsis-start', current - 1, current, current + 1, 'ellipsis-end', totalPages];
}

interface PaginationProps {
  /** Trang hiện tại (1-based) — `PagedResult.page`. */
  page: number;
  /** `PagedResult.totalPages`. */
  totalPages: number;
  /** Mặc định suy từ `page`/`totalPages`; truyền `PagedResult.hasPreviousPage`/`hasNextPage` khi có. */
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
  /** Tạo href cho trang `page` — PHẢI giữ nguyên các tham số lọc khác trên URL (ví dụ `buildHref` của search-params). */
  buildHref: (page: number) => string;
  className?: string;
}

const BASE = 'flex h-9 min-w-9 items-center justify-center rounded-lg border px-3 text-sm';
const IDLE = `${BASE} border-gray-300 bg-white text-gray-800 hover:bg-orange-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600`;
const DISABLED = `${BASE} border-gray-200 text-gray-400`;

/**
 * Phân trang offset (FR-SRCH-004) bằng LINK THẬT (`<Link href>`): chạy được khi JavaScript chưa tải xong, crawler đi theo
 * được (SEO), F5/chia sẻ link giữ nguyên trang và bộ lọc. `<nav aria-label="Phân trang">`, trang hiện tại có
 * `aria-current="page"` (NFR-USE-002). Một trang hoặc không có trang nào → không hiển thị.
 */
export default function Pagination({
  page,
  totalPages,
  hasPreviousPage = page > 1,
  hasNextPage = page < totalPages,
  buildHref,
  className = 'mt-10',
}: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav aria-label="Phân trang" className={className}>
      <ul className="flex flex-wrap items-center justify-center gap-2">
        <li>
          {hasPreviousPage ? (
            <Link href={buildHref(page - 1)} className={IDLE} aria-label="Trang trước" rel="prev">
              <ChevronLeft aria-hidden className="h-4 w-4" />
            </Link>
          ) : (
            <span className={DISABLED} aria-disabled="true" aria-label="Trang trước">
              <ChevronLeft aria-hidden className="h-4 w-4" />
            </span>
          )}
        </li>

        {getPageItems(page, totalPages).map((item) =>
          typeof item === 'number' ? (
            <li key={item}>
              <Link
                href={buildHref(item)}
                aria-current={item === page ? 'page' : undefined}
                aria-label={item === page ? `Trang ${item}, trang hiện tại` : `Trang ${item}`}
                className={
                  item === page
                    ? `${BASE} border-orange-700 bg-orange-700 font-semibold text-white`
                    : IDLE
                }
              >
                {item}
              </Link>
            </li>
          ) : (
            <li
              key={item}
              aria-hidden
              className="flex h-9 min-w-9 items-center justify-center text-gray-500"
            >
              …
            </li>
          ),
        )}

        <li>
          {hasNextPage ? (
            <Link href={buildHref(page + 1)} className={IDLE} aria-label="Trang sau" rel="next">
              <ChevronRight aria-hidden className="h-4 w-4" />
            </Link>
          ) : (
            <span className={DISABLED} aria-disabled="true" aria-label="Trang sau">
              <ChevronRight aria-hidden className="h-4 w-4" />
            </span>
          )}
        </li>
      </ul>
      <p className="sr-only" aria-live="polite">
        Trang {page} trên {totalPages}
      </p>
    </nav>
  );
}
