import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { assertDevOnly } from './mockups/dev-only';

/**
 * Giao diện tĩnh (mockup) của Buổi 6 — Dev 3 (Lab 6): ráp component THẬT với dữ liệu mẫu THẬT (`src/mocks/fixtures`, lấy từ
 * seed của Backend) để rà bố cục 320 / 768 / 1200 px trước khi Buổi 7 nối API.
 *
 * - Chỉ tồn tại ở môi trường phát triển: bản build production trả 404 cho mọi URL `/mockups/**` (`assertDevOnly()` ở layout
 *   và ở từng trang — xem `mockups/dev-only.ts`);
 *   `robots.ts` (Buổi 8) chặn thêm `/mockups`.
 * - `noindex`: không trang mockup nào được lập chỉ mục kể cả khi chạy ở máy demo.
 * - Không dùng Storybook: mockup dùng chính layout, CSS, provider của ứng dụng, và gãy biên dịch khi kiểu dữ liệu đổi.
 */
export const metadata: Metadata = {
  title: 'Mockup',
  robots: { index: false, follow: false },
};

export default function DevLayout({ children }: { children: ReactNode }) {
  assertDevOnly();
  return children;
}
