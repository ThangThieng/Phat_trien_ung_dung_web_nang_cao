import type { ReactNode } from 'react';
import PublicShell from '@/components/layout/PublicShell';

/**
 * Trang công khai (`/`, `/recipes/**`, `/categories/**`, `/search`): header/footer đầy đủ cho SEO.
 * Không có `loading.tsx` ở cấp group này: nó sẽ bao cả `/recipes/[slug]` và `/categories/[slug]`, bật streaming và
 * làm trang không tồn tại trả 200 thay vì 404 (Buổi 7 — Dev 2). Khung chờ đặt riêng ở `recipes/(list)`, `categories/(list)`.
 */
export default function PublicLayout({ children }: { children: ReactNode }) {
  return <PublicShell>{children}</PublicShell>;
}
