import type { APIRequestContext, Page } from '@playwright/test';

/** Spec cần dữ liệu thật chỉ chạy khi có môi trường dựng sẵn (`E2E_BASE_URL`). */
export const HAS_STACK = Boolean(process.env.E2E_BASE_URL);
export const STACK_SKIP_REASON = 'Cần môi trường đã dựng sẵn: đặt E2E_BASE_URL (xem playwright.config.ts)';

export const VIEWPORTS = [
  { name: '320', width: 320, height: 800 },
  { name: '768', width: 768, height: 1024 },
  { name: '1200', width: 1200, height: 900 },
] as const;

/**
 * Lấy một slug công thức và một slug danh mục THẬT từ chính sitemap.xml — spec không phụ thuộc dữ liệu mẫu cụ thể và đồng
 * thời kiểm luôn sitemap hoạt động.
 */
export async function discoverRoutes(request: APIRequestContext) {
  const xml = await (await request.get('/sitemap.xml')).text();
  const locs = [...xml.matchAll(/<loc>(.*?)<\/loc>/g)].map((m) => new URL(m[1]).pathname);
  const recipe = locs.find((p) => p.startsWith('/recipes/'));
  const category = locs.find((p) => p.startsWith('/categories/'));
  if (!recipe || !category) throw new Error('sitemap.xml không có công thức/danh mục nào để kiểm thử');
  return { recipe, category };
}

export const PUBLIC_ROUTES = (found: { recipe: string; category: string }) => [
  '/',
  '/recipes',
  found.recipe,
  '/categories',
  found.category,
  '/search',
  '/search?q=pho',
  '/auth/login',
  '/auth/register',
];

/** Dashboard cần đăng nhập: giả lập phiên Admin như `csp.spec.ts` (refresh token trong localStorage + API refresh giả). */
export async function signInAsAdmin(page: Page) {
  await page.addInitScript(() => {
    localStorage.setItem(
      'culinaryblog.auth',
      JSON.stringify({
        state: {
          refreshToken: 'refresh-token',
          user: { id: 'u1', email: 'admin@example.com', displayName: 'Admin', avatarUrl: null, bio: null, roles: ['Admin', 'Author'] },
        },
        version: 1,
      }),
    );
  });
  const admin = { id: 'u1', email: 'admin@example.com', displayName: 'Admin', avatarUrl: null, bio: null, roles: ['Admin', 'Author'] };
  // Khi API ở origin khác (môi trường dựng sẵn không qua Nginx) trình duyệt đòi CORS: trả đủ header và xử lý preflight.
  const cors = {
    'access-control-allow-origin': '*',
    'access-control-allow-headers': '*',
    'access-control-allow-methods': 'GET,POST,PUT,PATCH,DELETE,OPTIONS',
  };
  await page.route('**/api/v1/**', (route) => {
    const request = route.request();
    if (request.method() === 'OPTIONS') return route.fulfill({ status: 204, headers: cors });
    const { pathname } = new URL(request.url());
    // Dashboard còn gọi nhiều endpoint khác: trả danh sách rỗng để trang dựng được khung mà không phụ thuộc Backend.
    const body = pathname.endsWith('/auth/refresh')
      ? { accessToken: 'access-token', refreshToken: 'replacement-token', expiresAt: '2026-12-31T00:00:00Z', expiresIn: 900, user: admin }
      : [];
    return route.fulfill({ status: 200, contentType: 'application/json', headers: cors, body: JSON.stringify(body) });
  });
}

export const DASHBOARD_ROUTES = ['/dashboard', '/dashboard/categories', '/dashboard/recipes/new', '/dashboard/system', '/dashboard/media'];
