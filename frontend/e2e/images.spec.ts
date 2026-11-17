import { expect, test } from '@playwright/test';
import { HAS_STACK, STACK_SKIP_REASON, discoverRoutes } from './support';

/** Khung ảnh đúng cỡ chưa đủ: ảnh phải tải được thật, gồm cả đường qua `/_next/image` lẫn `/media/`. */
test.skip(!HAS_STACK, STACK_SKIP_REASON);

test('ảnh trên các trang công khai tải được thật', async ({ page, request }) => {
  test.setTimeout(120_000);
  const found = await discoverRoutes(request);
  const failed: string[] = [];
  page.on('response', (response) => {
    if (response.request().resourceType() === 'image' && response.status() >= 400) failed.push(`${response.status()} ${response.url()}`);
  });

  let loaded = 0;
  for (const route of ['/', '/recipes', '/search?q=pho', found.category, '/recipes/arroz-negro-com-muc-den-kieu-valencia']) {
    // eslint-disable-next-line no-await-in-loop -- kiểm từng trang
    await page.goto(route, { waitUntil: 'networkidle' });
    // eslint-disable-next-line no-await-in-loop -- như trên
    const images = await page.locator('main img').evaluateAll((items) => {
      items.forEach((item) => (item as HTMLImageElement).scrollIntoView());
      return items.map((item) => (item as HTMLImageElement).src);
    });
    // eslint-disable-next-line no-await-in-loop -- như trên
    await page.waitForLoadState('networkidle');
    // eslint-disable-next-line no-await-in-loop -- như trên
    const broken = await page.locator('main img').evaluateAll((items) =>
      items
        .map((item) => item as HTMLImageElement)
        .filter((item) => item.complete && item.naturalWidth === 0)
        .map((item) => item.src),
    );
    expect(broken, route).toEqual([]);
    loaded += images.length;
  }

  expect(failed).toEqual([]);
  expect(loaded).toBeGreaterThan(0);
});
