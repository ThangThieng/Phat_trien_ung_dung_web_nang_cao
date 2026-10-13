import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import DashboardShell from '@/components/layout/DashboardShell';

/**
 * NFR-SEO-002: trang riêng tư không được lập chỉ mục. Layout phải là Server Component mới export được `metadata`
 * (Client Component thì không), nên phần tương tác — chặn đăng nhập, sidebar, menu điện thoại — nằm trong `DashboardShell`.
 */
export const metadata: Metadata = {
  robots: { index: false, follow: false },
};

export default function DashboardLayout({ children }: { children: ReactNode }) {
  return <DashboardShell>{children}</DashboardShell>;
}
