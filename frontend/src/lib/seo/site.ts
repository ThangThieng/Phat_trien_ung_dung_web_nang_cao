import { SITE_NAME } from '@/lib/config';

export { SITE_NAME };

/** Địa chỉ gốc công khai của site (không có dấu `/` cuối) — dùng cho canonical, sitemap, robots và JSON-LD. */
export const SITE_URL = (process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000').replace(/\/+$/, '');

/** Đường dẫn tương đối → URL tuyệt đối. URL đã tuyệt đối được giữ nguyên. */
export function absoluteUrl(pathOrUrl: string): string {
  if (/^https?:\/\//i.test(pathOrUrl)) return pathOrUrl;
  return `${SITE_URL}${pathOrUrl.startsWith('/') ? '' : '/'}${pathOrUrl}`;
}
