import type { Metadata } from 'next';
import type { ReactNode } from 'react';

/** WCAG 2.4.2 (NFR-USE-002): mỗi trang có tiêu đề riêng — page là Client Component nên tiêu đề đặt ở layout của segment. */
// Chuỗi tiêu đề đơn ở layout trung gian làm trang con mất mẫu '%s | Culinary Blog' của layout gốc, nên khai báo lại mẫu ở đây.
export const metadata: Metadata = {
  title: { default: 'Viết công thức mới', template: '%s | Culinary Blog' },
};

export default function Layout({ children }: { children: ReactNode }) {
  return children;
}
