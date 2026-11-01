import { expect, test, type Page } from '@playwright/test';

const password = () => `E2e#${crypto.randomUUID()}Aa1`;
const unique = (prefix: string) => `${prefix}-${crypto.randomUUID()}@e2e.local`;
const mailhog = process.env.E2E_MAILHOG_URL ?? 'http://127.0.0.1:8125';

function decodeMailBody(body: string, encoding?: string) {
  if (encoding?.toLowerCase() === 'base64') return Buffer.from(body, 'base64').toString('utf8');
  return body.replace(/=\r?\n/g, '').replace(/=([A-Fa-f0-9]{2})/g, (_, value) => String.fromCharCode(Number.parseInt(value, 16)));
}

async function register(page: Page, email: string, value: string, name = 'E2E Author') {
  await page.goto('/auth/register');
  await page.getByLabel('Tên hiển thị').fill(name);
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Mật khẩu', { exact: true }).fill(value);
  await page.getByLabel('Xác nhận mật khẩu').fill(value);
  const response = page.waitForResponse(
    (item) => item.url().endsWith('/api/v1/auth/register') && item.request().method() === 'POST',
  );
  await page.getByRole('button', { name: 'Đăng ký' }).click();
  expect([200, 201]).toContain((await response).status());
}

async function login(page: Page, email: string, value: string) {
  await page.goto('/auth/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Mật khẩu').fill(value);
  const response = page.waitForResponse(
    (item) => item.url().endsWith('/api/v1/auth/login') && item.request().method() === 'POST',
  );
  await page.getByRole('button', { name: 'Đăng nhập' }).click();
  expect((await response).ok()).toBeTruthy();
  await expect(page.getByRole('button', { name: /Tài khoản:/ })).toBeVisible();
}

test('registers a fresh author and reaches an authenticated dashboard', async ({ page }) => {
  const email = unique('register');
  await register(page, email, password(), 'Fresh E2E Author');
  await expect(page.getByRole('button', { name: 'Tài khoản: Fresh E2E Author' })).toBeVisible();
  await page.goto('/dashboard');
  await expect(page.getByRole('heading', { name: 'Bảng điều khiển' })).toBeVisible();
});

test('locks an account after five wrong passwords and returns 423', async ({ page, context }) => {
  const email = unique('lockout');
  const valid = password();
  await register(page, email, valid, 'Lockout E2E');
  await expect(page.getByRole('button', { name: 'Tài khoản: Lockout E2E' })).toBeVisible();
  await context.clearCookies();
  await page.evaluate(() => localStorage.clear());
  for (let attempt = 1; attempt <= 5; attempt += 1) {
    await page.goto('/auth/login');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Mật khẩu').fill(password());
    const response = page.waitForResponse((item) => item.url().endsWith('/api/v1/auth/login') && item.request().method() === 'POST');
    await page.getByRole('button', { name: 'Đăng nhập' }).click();
    const status = (await response).status();
    expect(status).toBe(attempt === 5 ? 423 : 401);
  }
  await expect(page.getByText(/Tài khoản tạm bị khóa/i)).toBeVisible();
});

test('refreshes transparently after the local one-minute access token expires', async ({ page }) => {
  test.setTimeout(90_000);
  const email = unique('refresh');
  const value = password();
  await page.goto('/auth/register');
  await page.getByLabel('Tên hiển thị').fill('Refresh E2E');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Mật khẩu', { exact: true }).fill(value);
  await page.getByLabel('Xác nhận mật khẩu').fill(value);
  const registered = page.waitForResponse((item) => item.url().endsWith('/api/v1/auth/register') && item.request().method() === 'POST');
  await page.getByRole('button', { name: 'Đăng ký' }).click();
  const expiresAt = Date.parse((await (await registered).json()).expiresAt);
  await expect.poll(() => Date.now() > expiresAt, { timeout: 75_000, intervals: [1_000] }).toBe(true);
  const refreshed = page.waitForResponse((item) => item.url().endsWith('/api/v1/auth/refresh') && item.request().method() === 'POST');
  await page.goto('/profile');
  expect((await refreshed).status()).toBe(200);
  await expect(page.getByRole('button', { name: 'Tài khoản: Refresh E2E' })).toBeVisible();
  await expect(page.getByLabel('Tên hiển thị')).toBeVisible();
});

test('admin locks an author in the UI and the author is logged out on next refresh', async ({ browser }) => {
  test.setTimeout(60_000);
  test.skip(!process.env.E2E_ADMIN_PASSWORD, 'E2E_ADMIN_PASSWORD is required from local scratch env.');
  const admin = await browser.newContext(); const author = await browser.newContext();
  const adminPage = await admin.newPage(); const authorPage = await author.newPage();
  const email = unique('author-lock'); const authorPassword = password();
  await register(authorPage, email, authorPassword, 'Author To Lock');
  await expect(authorPage.getByRole('button', { name: 'Tài khoản: Author To Lock' })).toBeVisible();
  await login(adminPage, 'admin@culinaryblog.local', process.env.E2E_ADMIN_PASSWORD!);
  const usersRequests: string[] = [];
  adminPage.on('request', (request) => {
    const url = new URL(request.url());
    if (request.method() === 'GET' && url.pathname === '/api/v1/users') usersRequests.push(`${url.pathname}?${url.searchParams.toString()}`);
  });
  await adminPage.goto('/dashboard/users');
  const searchInput = adminPage.getByLabel('Tìm kiếm người dùng');
  await expect(searchInput).toBeVisible();
  await expect(searchInput).toBeEnabled();
  await searchInput.fill(email);
  await expect(searchInput).toHaveValue(email);
  const authorRow = adminPage.getByRole('row').filter({ hasText: email });
  await expect(authorRow, `Observed users requests: ${usersRequests.join(', ')}`).toBeVisible();
  await authorRow.getByRole('button', { name: 'Khóa' }).click();
  await adminPage.getByRole('textbox', { name: 'Lý do' }).fill('E2E security lock');
  const lockResponse = adminPage.waitForResponse((response) =>
    response.request().method() === 'PATCH' && response.url().includes('/api/v1/users/') && response.url().endsWith('/status'),
  );
  await adminPage.getByRole('button', { name: 'Xác nhận' }).click();
  expect((await lockResponse).ok()).toBeTruthy();
  await expect(authorRow.getByRole('button', { name: 'Mở khóa' })).toBeVisible();
  await test.step('trigger author revalidation and assert logout', async () => {
    const refreshResponse = authorPage.waitForResponse((response) =>
      response.request().method() === 'POST' && response.url().endsWith('/api/v1/auth/refresh'),
    );
    await authorPage.reload({ waitUntil: 'commit', timeout: 10_000 });
    expect((await refreshResponse).status()).toBe(403);
    await expect(authorPage).toHaveURL(/\/auth\/login/, { timeout: 10_000 });
    await expect(authorPage.getByRole('heading', { name: 'Đăng nhập' })).toBeVisible({ timeout: 10_000 });
    await expect(authorPage.getByRole('button', { name: 'Tài khoản: Author To Lock' })).toHaveCount(0);
  });
  await test.step('close independent browser contexts', async () => {
    await Promise.all([admin.close(), author.close()]);
  });
});

test('resets a password through scratch MailHog without persisting the reset token', async ({ page, request, context }) => {
  const email = unique('reset'); const oldPassword = password(); const newPassword = password();
  await register(page, email, oldPassword, 'Reset E2E');
  await context.clearCookies(); await page.evaluate(() => localStorage.clear());
  await page.goto('/auth/forgot-password');
  await page.getByLabel('Email').fill(email);
  await page.getByRole('button', { name: 'Gửi liên kết đặt lại' }).click();
  await expect(page.getByRole('status')).toContainText('Nếu email hợp lệ');
  await expect.poll(async () => {
    const response = await request.get(`${mailhog}/api/v2/messages`); const body = await response.json();
    return body.items.some((item: { Content?: { Body?: string; Headers?: { To?: string[]; 'Content-Transfer-Encoding'?: string[] } } }) =>
      item.Content?.Headers?.To?.some((to) => to.includes(email)) &&
      decodeMailBody(item.Content?.Body ?? '', item.Content?.Headers?.['Content-Transfer-Encoding']?.[0]).includes('/auth/reset-password?'),
    );
  }, { timeout: 20_000 }).toBe(true);
  const messages = await (await request.get(`${mailhog}/api/v2/messages`)).json();
  const message = messages.items.find((item: { Content?: { Body?: string; Headers?: { To?: string[]; 'Content-Transfer-Encoding'?: string[] } } }) =>
    item.Content?.Headers?.To?.some((to) => to.includes(email)) &&
    decodeMailBody(item.Content?.Body ?? '', item.Content?.Headers?.['Content-Transfer-Encoding']?.[0]).includes('/auth/reset-password?'),
  );
  const html = decodeMailBody(message?.Content?.Body ?? '', message?.Content?.Headers?.['Content-Transfer-Encoding']?.[0]);
  const resetUrl = html.replaceAll('&amp;', '&').match(/https?:\/\/[^"'<\s]+\/auth\/reset-password\?[^"'<\s]+/)?.[0];
  expect(resetUrl).toBeTruthy();
  await page.goto(resetUrl!);
  await expect(page).toHaveURL(/\/auth\/reset-password$/);
  const resetTokenPersisted = await page.evaluate((url) => {
    const token = new URL(url).searchParams.get('token');
    return [localStorage, sessionStorage].some((storage) =>
      Object.values(storage).some((value) => token !== null && value.includes(token)),
    );
  }, resetUrl!);
  expect(resetTokenPersisted).toBe(false);
  await page.getByLabel('Mật khẩu mới').fill(newPassword);
  await page.getByLabel('Xác nhận mật khẩu').fill(newPassword);
  await page.getByRole('button', { name: 'Đặt lại mật khẩu' }).click();
  await expect(page).toHaveURL(/\/auth\/login$/);
  await login(page, email, newPassword);
  await expect(page.getByRole('button', { name: 'Tài khoản: Reset E2E' })).toBeVisible();
});
