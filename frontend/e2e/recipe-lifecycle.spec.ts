import { expect, test, type Page } from '@playwright/test';
import { HAS_STACK, STACK_SKIP_REASON } from './support';

/**
 * Buổi 8 — vòng đời công thức trên stack thật: tạo qua wizard → ảnh → xuất bản → công khai → lưu trữ → khôi phục → xóa.
 * Cần `E2E_BASE_URL` và `E2E_AUTHOR_PASSWORD` (mật khẩu tác giả mẫu, SEED_AUTHOR_PASSWORD). Tạo dữ liệu riêng và tự xóa ở cuối.
 */
const AUTHOR = process.env.E2E_AUTHOR_EMAIL ?? 'author1@culinaryblog.local';
const PASSWORD = process.env.E2E_AUTHOR_PASSWORD;
// PNG 1×1 hợp lệ; server chỉ kiểm tra magic bytes và kích thước tệp.
const PIXEL = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==',
  'base64',
);

async function login(page: Page) {
  await page.goto('/auth/login');
  await page.getByLabel('Email').fill(AUTHOR);
  await page.getByLabel('Mật khẩu').fill(PASSWORD!);
  const signIn = page.getByRole('button', { name: 'Đăng nhập' });
  await signIn.click();
  // Hạn mức thật 10 lần/phút cho /auth/* dùng chung với các spec khác: gặp 429 thì chờ hết cửa sổ rồi thử lại.
  const signedIn = page.getByRole('button', { name: /Tài khoản:/ });
  const limited = page.getByText(/thao tác quá nhanh/);
  await expect(signedIn.or(limited)).toBeVisible();
  if (await limited.isVisible()) {
    await page.waitForTimeout(61_000);
    await signIn.click();
  }
  await expect(signedIn).toBeVisible();
}

test.describe('vòng đời công thức', () => {
  test.skip(!HAS_STACK || !PASSWORD, `${STACK_SKIP_REASON}; đặt thêm E2E_AUTHOR_PASSWORD`);
  test.setTimeout(180_000);

  test('wizard báo lỗi tại ô sai và đi hết vòng đời', async ({ page, browser }) => {
    const title = `E2E vòng đời ${Date.now()}`;
    await login(page);
    await page.goto('/dashboard/recipes/new', { waitUntil: 'networkidle' });

    // Bước 1: bỏ trống phải hiện lỗi ngay tại ô, không được chặn im lặng.
    await page.getByRole('button', { name: 'Tiếp tục' }).click();
    await expect(page.getByText('Tiêu đề phải từ 5 đến 200 ký tự.')).toBeVisible();

    await page.getByLabel('Tiêu đề').fill(title);
    await page.getByLabel('Mô tả').fill('Công thức do kiểm thử E2E tạo để kiểm tra vòng đời.');
    await page.getByLabel('Danh mục').selectOption({ index: 1 });
    await page.getByLabel('Chuẩn bị (phút)').fill('10');
    await page.getByLabel('Nấu (phút)').fill('20');
    await page.getByLabel('Khẩu phần').fill('2');
    await page.getByRole('button', { name: 'Tiếp tục' }).click();

    await page.getByRole('button', { name: 'Thêm nguyên liệu' }).click();
    await page.getByLabel('Tên nguyên liệu 1').fill('Gạo');
    await page.getByLabel('Đơn vị').fill('bát');
    await page.getByRole('button', { name: 'Tiếp tục' }).click();

    await page.getByRole('button', { name: 'Thêm bước' }).click();
    await page.getByLabel('Tiêu đề bước 1').fill('Nấu');
    await page.getByLabel('Mô tả bước 1').fill('Nấu cho chín.');
    const created = page.waitForResponse(
      (item) => item.url().endsWith('/api/v1/recipes') && item.request().method() === 'POST',
    );
    await page.getByRole('button', { name: 'Tạo công thức' }).click();
    const recipe = (await (await created).json()) as { slug: string };
    expect(recipe.slug).toBeTruthy();

    // Bước ảnh.
    const uploaded = page.waitForResponse((item) => /\/recipes\/[^/]+\/images$/.test(item.url()) && item.request().method() === 'POST');
    await page.locator('input[type="file"]').first().setInputFiles({ name: 'mon.png', mimeType: 'image/png', buffer: PIXEL });
    expect((await uploaded).ok()).toBeTruthy();
    await page.getByRole('button', { name: 'Tiếp tục' }).click();

    // Xuất bản → khách xem được.
    await page.getByRole('button', { name: 'Xuất bản', exact: true }).click();
    await expect(page.getByRole('button', { name: 'Hủy xuất bản' })).toBeVisible();
    const guest = await browser.newContext({ ignoreHTTPSErrors: true });
    const guestPage = await guest.newPage();
    const baseURL = process.env.E2E_BASE_URL;
    expect((await guestPage.goto(`${baseURL}/recipes/${recipe.slug}`))?.status()).toBe(200);

    // Lưu trữ → API công khai trả 404 ngay; trang HTML theo sau trong TTL Data Cache 300 giây (MT-67).
    await page.getByRole('button', { name: 'Lưu trữ' }).click();
    await expect(page.getByRole('button', { name: 'Khôi phục về nháp' })).toBeVisible();
    expect((await guest.request.get(`${baseURL}/api/v1/recipes/${recipe.slug}`)).status()).toBe(404);
    await page.getByRole('button', { name: 'Khôi phục về nháp' }).click();
    await expect(page.getByRole('button', { name: 'Xuất bản', exact: true })).toBeVisible();

    // Xóa: nút chỉ bật khi gõ đúng tên.
    await page.getByRole('button', { name: 'Xóa', exact: true }).click();
    const confirm = page.getByRole('button', { name: 'Xóa công thức' });
    await expect(confirm).toBeDisabled();
    await page.getByLabel('Tên công thức để xác nhận xóa').fill(title);
    await confirm.click();
    await expect(page).toHaveURL(/\/dashboard\/recipes$/);
    await expect(page.getByRole('link', { name: title })).toHaveCount(0);
    await guest.close();
  });
});
