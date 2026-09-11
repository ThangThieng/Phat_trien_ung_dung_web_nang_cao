import Link from 'next/link';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  page: number;
  totalPages: number;
  /** Tạo href cho trang (giữ nguyên các query param khác). */
  buildHref: (page: number) => string;
}

/** Offset pagination (FR-SRCH-004): Trước / số trang / Sau – dùng Link để crawler đi được (SEO). */
export default function Pagination({ page, totalPages, buildHref }: PaginationProps) {
  if (totalPages <= 1) return null;

  const start = Math.max(1, page - 2);
  const end = Math.min(totalPages, start + 4);
  const pages = Array.from({ length: end - start + 1 }, (_, i) => start + i);
  const base = 'flex h-9 min-w-9 items-center justify-center rounded-lg border px-3 text-sm';

  return (
    <nav aria-label="Phân trang" className="mt-10 flex items-center justify-center gap-2">
      {page > 1 ? (
        <Link
          href={buildHref(page - 1)}
          className={`${base} border-gray-300 hover:bg-orange-50`}
          aria-label="Trang trước"
        >
          <ChevronLeft aria-hidden className="h-4 w-4" />
        </Link>
      ) : (
        <span className={`${base} border-gray-200 text-gray-300`} aria-hidden>
          <ChevronLeft className="h-4 w-4" />
        </span>
      )}
      {pages.map((p) => (
        <Link
          key={p}
          href={buildHref(p)}
          aria-current={p === page ? 'page' : undefined}
          className={`${base} ${
            p === page
              ? 'border-orange-600 bg-orange-600 font-semibold text-white'
              : 'border-gray-300 hover:bg-orange-50'
          }`}
        >
          {p}
        </Link>
      ))}
      {page < totalPages ? (
        <Link
          href={buildHref(page + 1)}
          className={`${base} border-gray-300 hover:bg-orange-50`}
          aria-label="Trang sau"
        >
          <ChevronRight aria-hidden className="h-4 w-4" />
        </Link>
      ) : (
        <span className={`${base} border-gray-200 text-gray-300`} aria-hidden>
          <ChevronRight className="h-4 w-4" />
        </span>
      )}
    </nav>
  );
}
