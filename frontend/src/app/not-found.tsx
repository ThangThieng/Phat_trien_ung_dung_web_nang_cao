import Link from 'next/link';
import { SearchX } from 'lucide-react';
import EmptyState from '@/components/shared/EmptyState';
import PublicShell from '@/components/layout/PublicShell';

/**
 * 404 cho cả URL không khớp route nào lẫn `notFound()` ném từ trang. File này nằm ngoài route group nên tự bọc
 * `PublicShell` — không có nó, trang 404 mất header/footer kể từ khi root layout không còn chứa chúng (Buổi 6).
 */
export default function NotFound() {
  return (
    <PublicShell>
      <div className="mx-auto max-w-xl px-4 py-20">
        <EmptyState
          icon={SearchX}
          headingLevel="h1"
          title="Không tìm thấy trang"
          description="Nội dung bạn tìm không tồn tại hoặc chưa được xuất bản."
          action={
            <>
              <Link
                href="/recipes"
                className="rounded-lg bg-orange-700 px-4 py-2 font-semibold text-white hover:bg-orange-800"
              >
                Xem công thức
              </Link>
              <Link href="/search" className="font-medium text-orange-700 hover:underline">
                Tìm kiếm
              </Link>
            </>
          }
        />
      </div>
    </PublicShell>
  );
}
