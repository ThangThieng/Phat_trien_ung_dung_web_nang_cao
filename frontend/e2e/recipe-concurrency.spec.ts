import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { HAS_STACK, STACK_SKIP_REASON } from './support';

/** Buổi 8 — hai tab mở cùng công thức: lưu tab 1 rồi tab 2 phải hiện dialog 409, xem được khác biệt và tải lại được (FR-RCP-004). */
const AUTHOR = process.env.E2E_AUTHOR_EMAIL ?? 'author1@culinaryblog.local';
const PASSWORD = process.env.E2E_AUTHOR_PASSWORD;

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

async function createDraft(request: APIRequestContext, title: string) {
  const session = await request.post('/api/v1/auth/login', { data: { email: AUTHOR, password: PASSWORD } });
  expect(session.ok()).toBeTruthy();
  const { accessToken } = (await session.json()) as { accessToken: string };
  const headers = { Authorization: `Bearer ${accessToken}` };
  const categories = (await (await request.get('/api/v1/categories')).json()) as { id: string }[];
  const created = await request.post('/api/v1/recipes', {
    headers,
    data: {
      title,
      description: 'Công thức do kiểm thử E2E tạo để kiểm tra xung đột hai tab.',
      categoryId: categories[0].id,
      prepTime: 10,
      cookTime: 20,
      servings: 2,
      difficulty: 'Easy',
      ingredients: [{ name: 'Gạo', quantity: 1, unit: 'bát', orderIndex: 0 }],
      steps: [{ title: 'Nấu', description: 'Nấu cho chín.' }],
    },
  });
  expect(created.status()).toBe(201);
  const { id } = (await created.json()) as { id: string };
  return { id, headers };
}

async function goToSave(page: Page, id: string) {
  await page.goto(`/dashboard/recipes/${id}/edit`, { waitUntil: 'networkidle' });
  await page.getByRole('button', { name: 'Tiếp tục' }).click();
  await page.getByRole('button', { name: 'Tiếp tục' }).click();
  await expect(page.getByRole('button', { name: 'Lưu và sang bước ảnh' })).toBeVisible();
}

test('hai tab cùng sửa: tab lưu sau thấy dialog 409, xem khác biệt, tải lại được', async ({ browser, request }) => {
  test.skip(!HAS_STACK || !PASSWORD, `${STACK_SKIP_REASON}; đặt thêm E2E_AUTHOR_PASSWORD`);
  test.setTimeout(180_000);
  const { id, headers } = await createDraft(request, `E2E hai tab ${Date.now()}`);

  const context = await browser.newContext({ ignoreHTTPSErrors: true, baseURL: process.env.E2E_BASE_URL });
  const first = await context.newPage();
  await login(first);
  const second = await context.newPage();
  await goToSave(first, id);
  await goToSave(second, id);

  // Tab 1 đổi và lưu trước.
  await first.getByRole('button', { name: 'Quay lại' }).click();
  await first.getByRole('button', { name: 'Quay lại' }).click();
  await first.getByLabel('Khẩu phần').fill('5');
  await first.getByRole('button', { name: 'Tiếp tục' }).click();
  await first.getByRole('button', { name: 'Tiếp tục' }).click();
  await first.getByRole('button', { name: 'Lưu và sang bước ảnh' }).click();
  await expect(first.getByText('Đã lưu các thay đổi.')).toBeVisible();

  // Tab 2 còn rowVersion cũ.
  await second.getByRole('button', { name: 'Lưu và sang bước ảnh' }).click();
  const dialog = second.getByRole('alertdialog');
  await expect(dialog.getByText('Công thức đã được cập nhật ở nơi khác')).toBeVisible();
  await dialog.getByRole('button', { name: 'Xem khác biệt' }).click();
  await expect(dialog.getByText(/Khẩu phần: bạn nhập "2", trên máy chủ là "5"/)).toBeVisible();
  await dialog.getByRole('button', { name: 'Tải lại' }).click();
  await expect(second.getByRole('alertdialog')).toHaveCount(0);

  await context.close();
  expect((await request.delete(`/api/v1/recipes/${id}`, { headers })).ok()).toBeTruthy();
});
