import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';
import { DASHBOARD_ROUTES, HAS_STACK, PUBLIC_ROUTES, STACK_SKIP_REASON, discoverRoutes, signInAsAdmin } from './support';

/**
 * NFR-USE-002 (WCAG 2.1 AA) — Buổi 8, Dev 3. Ngưỡng: 0 vi phạm mức serious/critical trên mọi route công khai và dashboard.
 * axe còn báo mục "cần đánh giá thủ công" và cảnh báo minor không áp dụng cho ngữ cảnh; ngưỡng này đạt được và có nghĩa,
 * thay vì một con số lý tưởng khiến cả nhóm bỏ cuộc. Phần còn lại (NVDA/VoiceOver) rà thủ công — `docs/a11y-report.md`.
 */
test.skip(!HAS_STACK, STACK_SKIP_REASON);

const TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

async function scan(page: import('@playwright/test').Page, path: string) {
  // Dashboard có truy vấn định kỳ nên không bao giờ "networkidle": chờ `load` + landmark chính rồi để React Query ổn định.
  if (path.startsWith('/dashboard')) {
    await page.goto(path, { waitUntil: 'load' });
    await page.locator('main').first().waitFor();
    await page.waitForTimeout(800);
  } else {
    await page.goto(path, { waitUntil: 'networkidle' });
  }
  // Route cần đăng nhập mà bị chuyển về /auth/login thì đang quét nhầm trang đăng nhập — phải báo lỗi chứ không "xanh giả".
  if (path.startsWith('/dashboard')) expect(new URL(page.url()).pathname, 'bị chuyển hướng khỏi dashboard').toBe(path);
  const { violations } = await new AxeBuilder({ page }).withTags(TAGS).analyze();
  const blocking = violations.filter((v) => v.impact === 'serious' || v.impact === 'critical');
  const summary = violations.map((v) => `${v.impact}: ${v.id} (${v.nodes.length})`);
  test.info().annotations.push({ type: 'axe', description: summary.length ? summary.join('; ') : 'không có vi phạm' });
  expect(blocking.map((v) => `${v.id}: ${v.nodes.map((n) => n.target.join(' ')).join(' | ')}`)).toEqual([]);
}

test.describe('axe — route công khai', () => {
  test('mọi route công khai: 0 vi phạm serious/critical', async ({ page, request }) => {
    test.setTimeout(180_000);
    const routes = PUBLIC_ROUTES(await discoverRoutes(request));
    for (const route of routes) {
      await test.step(route, () => scan(page, route));
    }
  });

  test('trang 404 (URL không khớp route)', async ({ page }) => {
    await scan(page, '/trang-khong-ton-tai-123');
  });

  // `notFound()` ném từ trang: HTML đầu tiên là khung lỗi của Next (`<html id="__next_error__">`, chưa có `lang`); sau khi hydrate,
  // router phía client dựng lại trang 404 trong root layout (`lang="vi"`). Phải chờ bước đó trước khi quét, nếu không axe thấy khung tạm.
  test('trang công thức không tồn tại: sau hydrate có lang và giao diện 404', async ({ page }) => {
    await page.goto('/recipes/khong-ton-tai-123', { waitUntil: 'networkidle' });
    await expect(page.locator('html')).toHaveAttribute('lang', 'vi');
    await expect(page.getByRole('heading', { name: 'Không tìm thấy trang' })).toBeVisible();
    await scan(page, '/recipes/khong-ton-tai-123');
  });
});

test.describe('axe — dashboard (phiên Admin giả lập)', () => {
  // Mỗi route một test (trang mới): quét tuần tự nhiều route trên cùng một trang từng bị treo ngẫu nhiên.
  for (const route of DASHBOARD_ROUTES) {
    test(`${route}: 0 vi phạm serious/critical`, async ({ page }) => {
      await signInAsAdmin(page);
      await scan(page, route);
    });
  }
});

test.describe('điều hướng bàn phím', () => {
  test('liên kết "Bỏ qua điều hướng" là phần tử focus đầu tiên và nhảy tới main', async ({ page }) => {
    await page.goto('/');
    await page.keyboard.press('Tab');
    const skip = page.getByRole('link', { name: 'Bỏ qua điều hướng' });
    await expect(skip).toBeFocused();
    await expect(skip).toBeVisible();
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/#main-content$/);
  });

  test('vòng focus nhìn thấy được trên liên kết điều hướng', async ({ page }) => {
    await page.goto('/');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    const outline = await page.evaluate(() => {
      const el = document.activeElement as HTMLElement | null;
      if (!el) return null;
      const style = getComputedStyle(el);
      return { outlineStyle: style.outlineStyle, outlineWidth: parseFloat(style.outlineWidth), boxShadow: style.boxShadow };
    });
    expect(outline).not.toBeNull();
    const visible = outline!.boxShadow !== 'none' || (outline!.outlineStyle !== 'none' && outline!.outlineWidth > 0);
    expect(visible).toBe(true);
  });
});

test.describe('ngôn ngữ và chuyển động', () => {
  test('lang="vi"', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('html')).toHaveAttribute('lang', 'vi');
  });

  test('prefers-reduced-motion: không còn transition/animation kéo dài', async ({ browser }) => {
    const context = await browser.newContext({ reducedMotion: 'reduce' });
    const page = await context.newPage();
    await page.goto('/');
    const longest = await page.evaluate(() => {
      const parse = (v: string) => Math.max(...v.split(',').map((s) => (s.trim().endsWith('ms') ? parseFloat(s) : parseFloat(s) * 1000)));
      let max = 0;
      document.querySelectorAll('*').forEach((el) => {
        const s = getComputedStyle(el);
        max = Math.max(max, parse(s.transitionDuration), parse(s.animationDuration));
      });
      return max;
    });
    await context.close();
    expect(longest).toBeLessThanOrEqual(10); // ≤ 10 ms = coi như tắt
  });
});
