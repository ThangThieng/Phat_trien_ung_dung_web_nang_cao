import type { ReactNode } from 'react';
import { Inbox, type LucideIcon } from 'lucide-react';

interface EmptyStateProps {
  /** Biểu tượng `lucide-react` (chỉ trang trí — `aria-hidden`). */
  icon?: LucideIcon;
  title: string;
  description?: ReactNode;
  /** Khe hành động: nút/link gợi ý bước tiếp theo (bỏ bộ lọc, viết công thức đầu tiên…). */
  action?: ReactNode;
  /** Cấp tiêu đề theo ngữ cảnh — trang 404 dùng `h1`, khối trong trang dùng `h2` (mặc định) để không nhảy cấp (WCAG 1.3.1). */
  headingLevel?: 'h1' | 'h2' | 'h3';
  className?: string;
}

/**
 * Trạng thái rỗng dùng chung (NFR-USE-004 — không để màn hình trống trơn): không có kết quả tìm kiếm, danh mục chưa có công
 * thức, chưa viết công thức nào, trang 404. Server Component — không gửi JavaScript xuống trình duyệt.
 */
export default function EmptyState({
  icon: Icon = Inbox,
  title,
  description,
  action,
  headingLevel: Heading = 'h2',
  className = '',
}: EmptyStateProps) {
  return (
    <div
      className={`flex flex-col items-center rounded-2xl border border-dashed border-orange-200 bg-white px-6 py-12 text-center ${className}`}
    >
      <span className="flex h-14 w-14 items-center justify-center rounded-full bg-orange-50 text-orange-700">
        <Icon aria-hidden className="h-7 w-7" />
      </span>
      <Heading className="mt-4 text-lg font-semibold text-gray-900">{title}</Heading>
      {description && <div className="mt-2 max-w-md text-sm text-gray-600">{description}</div>}
      {action && (
        <div className="mt-6 flex flex-wrap items-center justify-center gap-3">{action}</div>
      )}
    </div>
  );
}
