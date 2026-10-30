import { expect, test } from '@playwright/test';
import { HAS_STACK, PUBLIC_ROUTES, STACK_SKIP_REASON, VIEWPORTS, discoverRoutes } from './support';

/** NFR-USE-001 — 320 / 768 / 1200 px trên mọi route công khai: không cuộn ngang, có đủ landmark, đúng một <h1>. */
test.skip(!HAS_STACK, STACK_SKIP_REASON);

for (const viewport of VIEWPORTS) {
  test(`${viewport.name}px: không cuộn ngang, đủ landmark, một h1`, async ({ page, request }) => {
    test.setTimeout(120_000);
    await page.setViewportSize({ width: viewport.width, height: viewport.height });
    const routes = PUBLIC_ROUTES(await discoverRoutes(request)).filter((r) => !r.startsWith('/auth/'));
    for (const route of routes) {
      await test.step(route, async () => {
        await page.goto(route, { waitUntil: 'networkidle' });
        const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth);
        expect(overflow, 'cuộn ngang (px)').toBeLessThanOrEqual(0);
        await expect(page.locator('header').first()).toBeVisible();
        await expect(page.locator('main')).toHaveCount(1);
        await expect(page.locator('footer').first()).toBeAttached();
        await expect(page.locator('h1')).toHaveCount(1);
      });
    }
  });
}

test('lưới công thức trang chủ: 1 cột ở 320, 2 cột ở 768, 2 cột ở 1200 (còn cột phụ danh mục), 3 cột ở ≥ 1280', async ({ page }) => {
  const columns = async () =>
    page.evaluate(() => {
      const cards = [...document.querySelectorAll('main article')] as HTMLElement[];
      return new Set(cards.map((c) => Math.round(c.getBoundingClientRect().left))).size;
    });
  const expected = [
    [320, (n: number) => n === 1],
    [768, (n: number) => n === 2],
    [1200, (n: number) => n === 2],
    [1440, (n: number) => n >= 3],
  ] as const;
  for (const [width, ok] of expected) {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/', { waitUntil: 'networkidle' });
    const n = await columns();
    if (n === 0) test.skip(true, 'Không có thẻ công thức để đếm cột');
    expect(ok(n), `${width}px có ${n} cột`).toBe(true);
  }
});

test('bộ lọc /recipes dạng Sheet dùng được bằng bàn phím ở 320 px', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  await page.goto('/recipes', { waitUntil: 'networkidle' });
  const open = page.getByRole('button', { name: /bộ lọc/i });
  await expect(open).toBeVisible();
  await open.focus();
  await page.keyboard.press('Enter');
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(dialog).toBeHidden();
  await expect(open).toBeFocused();
});
