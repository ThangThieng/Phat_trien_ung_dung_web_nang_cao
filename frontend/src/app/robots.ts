import type { MetadataRoute } from 'next';
import { SITE_URL } from '@/lib/seo/site';

/** NFR-SEO-003: cho phép crawler, chặn khu vực riêng tư, khai báo Sitemap. KHÔNG ping Google (đã khai tử 6/2023). */
export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/dashboard/', '/profile', '/auth/', '/api/', '/hangfire', '/scalar', '/mockups'],
      },
    ],
    sitemap: `${SITE_URL}/sitemap.xml`,
  };
}
