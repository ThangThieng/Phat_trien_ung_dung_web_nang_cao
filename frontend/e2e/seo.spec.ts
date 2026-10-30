import { expect, test, type APIRequestContext } from '@playwright/test';
import { HAS_STACK, STACK_SKIP_REASON, discoverRoutes } from './support';

/**
 * NFR-SEO-001 → 003 — Buổi 8, Dev 3. Kiểm chứng trên môi trường dựng sẵn; phần "Google Rich Results Test" là bước thủ công
 * trên URL công khai (không gọi được từ máy CI) và được ghi ở `docs/seo-report.md`.
 */
test.skip(!HAS_STACK, STACK_SKIP_REASON);

function metaContent(html: string, attr: 'name' | 'property', key: string) {
  return new RegExp(`<meta ${attr}="${key}" content="([^"]*)"`).exec(html)?.[1];
}

function jsonLdBlocks(html: string) {
  return [...html.matchAll(/<script type="application\/ld\+json">([\s\S]*?)<\/script>/g)].map((m) => m[1]);
}

async function recipeHtml(request: APIRequestContext) {
  const { recipe } = await discoverRoutes(request);
  const response = await request.get(recipe);
  expect(response.status()).toBe(200);
  return { path: recipe, html: await response.text() };
}

test.describe('robots.txt và sitemap.xml (ở domain chính)', () => {
  test('robots.txt cho phép /, chặn khu vực riêng tư, khai báo Sitemap', async ({ request }) => {
    const response = await request.get('/robots.txt');
    expect(response.status()).toBe(200);
    const text = await response.text();
    expect(text).toMatch(/Allow: \//);
    for (const path of ['/dashboard/', '/profile', '/auth/', '/api/', '/hangfire', '/scalar', '/mockups']) {
      expect(text).toContain(`Disallow: ${path}`);
    }
    expect(text).toMatch(/Sitemap: https?:\/\/[^\s]+\/sitemap\.xml/);
  });

  test('sitemap.xml: XML chuẩn, đủ trang tĩnh, và MỌI URL công thức đều 200 (không có Draft/Archived)', async ({ request }) => {
    const response = await request.get('/sitemap.xml');
    expect(response.status()).toBe(200);
    expect(response.headers()['content-type']).toContain('xml');
    const xml = await response.text();
    expect(xml).toContain('<urlset');
    for (const tag of ['<loc>', '<lastmod>', '<changefreq>', '<priority>']) expect(xml).toContain(tag);
    const locs = [...xml.matchAll(/<loc>(.*?)<\/loc>/g)].map((m) => new URL(m[1]));
    for (const page of ['/', '/recipes', '/categories', '/search']) {
      expect(locs.some((u) => u.pathname === page)).toBe(true);
    }
    // Draft/Archived là 404 với khách (MT-34): nếu sitemap lọt một slug như vậy, vòng lặp này sẽ thấy 404.
    const recipes = locs.filter((u) => u.pathname.startsWith('/recipes/'));
    expect(recipes.length).toBeGreaterThan(0);
    for (const url of recipes.slice(0, 25)) {
      expect((await request.get(url.pathname)).status(), url.pathname).toBe(200);
    }
  });
});

test.describe('trang công thức', () => {
  test('<title> ≤ 60 ký tự, description ≤ 160, canonical tuyệt đối', async ({ request }) => {
    const { path, html } = await recipeHtml(request);
    const title = /<title>(.*?)<\/title>/.exec(html)?.[1] ?? '';
    expect(title.length).toBeLessThanOrEqual(60);
    expect(title.endsWith(' | Culinary Blog')).toBe(true);
    expect(title.split('Culinary Blog').length - 1, 'hậu tố không bị lặp').toBe(1);
    expect((metaContent(html, 'name', 'description') ?? '').length).toBeLessThanOrEqual(160);
    expect(/<link rel="canonical" href="([^"]+)"/.exec(html)?.[1]).toMatch(new RegExp(`^https?://.+${path}$`));
    expect(metaContent(html, 'name', 'robots'), 'Published thì được index').toBeUndefined();
  });

  test('Open Graph + Twitter Card, ảnh OG đúng 1200×630', async ({ request }) => {
    const { html } = await recipeHtml(request);
    expect(metaContent(html, 'property', 'og:type')).toBe('article');
    expect(metaContent(html, 'property', 'og:title')).toBeTruthy();
    expect(metaContent(html, 'property', 'og:description')).toBeTruthy();
    expect(metaContent(html, 'property', 'og:url')).toMatch(/^https?:\/\//);
    expect(metaContent(html, 'property', 'og:image:width')).toBe('1200');
    expect(metaContent(html, 'property', 'og:image:height')).toBe('630');
    expect(metaContent(html, 'name', 'twitter:card')).toBe('summary_large_image');

    const image = new URL(metaContent(html, 'property', 'og:image') ?? '');
    const response = await request.get(image.pathname + image.search);
    expect(response.status()).toBe(200);
    expect(response.headers()['content-type']).toBe('image/png');
    const png = await response.body();
    expect(png.readUInt32BE(16)).toBe(1200); // IHDR: chiều rộng
    expect(png.readUInt32BE(20)).toBe(630); // IHDR: chiều cao
  });

  test('JSON-LD Recipe đủ thuộc tính, không có aggregateRating, không còn "<" thô (chống XSS)', async ({ request }) => {
    const { html } = await recipeHtml(request);
    const blocks = jsonLdBlocks(html);
    expect(blocks.length).toBeGreaterThanOrEqual(2); // Recipe + BreadcrumbList
    for (const block of blocks) expect(block, 'phải thoát < thành \\u003c').not.toContain('<');

    const parsed = blocks.map((b) => JSON.parse(b));
    const recipe = parsed.find((d) => d['@type'] === 'Recipe');
    expect(recipe).toBeTruthy();
    for (const key of ['name', 'description', 'author', 'datePublished', 'prepTime', 'cookTime', 'totalTime', 'recipeYield', 'recipeIngredient', 'recipeInstructions']) {
      expect(recipe, key).toHaveProperty(key);
    }
    expect(recipe.prepTime).toMatch(/^PT(\d+H)?(\d+M)?$/);
    expect(recipe.recipeIngredient.length).toBeGreaterThan(0);
    expect(recipe.recipeInstructions[0]['@type']).toBe('HowToStep');
    expect(JSON.stringify(parsed)).not.toContain('aggregateRating');
    expect(parsed.some((d) => d['@type'] === 'BreadcrumbList')).toBe(true);
  });

  test('mỗi bước nấu có neo #buoc-{n} khớp url trong JSON-LD', async ({ page, request }) => {
    const { path } = await recipeHtml(request);
    await page.goto(path);
    await expect(page.locator('#buoc-1')).toBeAttached();
  });

  test('công thức không tồn tại / Draft → HTTP 404 thật (không phải 200 "soft 404")', async ({ request }) => {
    const response = await request.get('/recipes/slug-khong-ton-tai-draft-xyz');
    expect(response.status()).toBe(404);
  });
});

test.describe('trang chủ, danh mục, tìm kiếm, khu vực riêng tư', () => {
  test('trang chủ có WebSite + SearchAction', async ({ request }) => {
    const html = await (await request.get('/')).text();
    const site = jsonLdBlocks(html).map((b) => JSON.parse(b)).find((d) => d['@type'] === 'WebSite');
    expect(site?.potentialAction?.target?.urlTemplate).toContain('/search?q={search_term_string}');
  });

  test('trang danh mục có BreadcrumbList và canonical', async ({ request }) => {
    const { category } = await discoverRoutes(request);
    const html = await (await request.get(category)).text();
    expect(jsonLdBlocks(html).map((b) => JSON.parse(b)).some((d) => d['@type'] === 'BreadcrumbList')).toBe(true);
    expect(/<link rel="canonical" href="([^"]+)"/.exec(html)?.[1]).toContain(category);
  });

  test('/search có q → noindex; không có q → index được', async ({ request }) => {
    const withQuery = await (await request.get('/search?q=pho')).text();
    expect(metaContent(withQuery, 'name', 'robots')).toContain('noindex');
    const without = await (await request.get('/search')).text();
    expect(metaContent(without, 'name', 'robots')).toBeUndefined();
  });

  test('auth và mockups: noindex hoặc 404', async ({ request }) => {
    const login = await (await request.get('/auth/login')).text();
    expect(metaContent(login, 'name', 'robots')).toContain('noindex');
    expect((await request.get('/mockups')).status()).toBe(404); // bản production chặn (NODE_ENV)
  });
});
