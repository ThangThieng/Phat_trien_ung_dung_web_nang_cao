import { expect, test } from '@playwright/test';
import { HAS_STACK, STACK_SKIP_REASON } from './support';

/**
 * Lab 8 Dev 3 — E2E tìm kiếm / bộ lọc (FR-SRCH-001 → 004, MT-66).
 *
 *  - Phần giao diện chạy với `E2E_BASE_URL`.
 *  - Phần hợp đồng API (400 / 409) cần API thật: đặt `E2E_API_URL` (vd `https://localhost/api/v1`) và, cho phần 409,
 *    `E2E_ADMIN_EMAIL` + `E2E_ADMIN_PASSWORD`. Thiếu thì tự bỏ qua — API giả không đủ để khẳng định hành vi của Backend.
 */
const TERM = process.env.E2E_SEARCH_TERM ?? 'pho';
const EXPECT = new RegExp(process.env.E2E_SEARCH_EXPECT ?? 'Phở', 'i');
const API = process.env.E2E_API_URL;

test.describe('giao diện', () => {
  test.skip(!HAS_STACK, STACK_SKIP_REASON);

  test('gõ không dấu "pho" vẫn thấy "Phở…" (unaccent)', async ({ page }) => {
    // networkidle: ô tìm kiếm ở header là client component — gõ Enter trước khi hydrate xong thì không điều hướng.
    await page.goto('/', { waitUntil: 'networkidle' });
    const box = page.getByRole('searchbox').first();
    await box.fill(TERM);
    await box.press('Enter');
    await expect(page).toHaveURL(new RegExp(`/search\\?q=${TERM}`));
    await expect(page.getByRole('heading', { name: EXPECT }).first()).toBeVisible();
    await expect(page.getByText(/Tìm thấy \d+ công thức/)).toBeVisible();
  });

  test('từ khóa quá ngắn không gọi API và báo lỗi nhẹ nhàng', async ({ page }) => {
    await page.goto('/search?q=p');
    await expect(page.getByText(/ít nhất 2 ký tự/)).toBeVisible();
  });

  test('bộ lọc thời gian ở /recipes: URL giữ trạng thái, F5 và back/forward đúng', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto('/recipes');
    const cookTime = page.getByLabel('Thời gian nấu tối đa (phút)');
    await cookTime.fill('45');
    await page.getByRole('button', { name: 'Áp dụng' }).click();
    await expect(page).toHaveURL(/maxCookTime=45/);

    await page.reload();
    await expect(page).toHaveURL(/maxCookTime=45/);
    await expect(page.getByLabel('Thời gian nấu tối đa (phút)')).toHaveValue('45');

    await page.getByLabel('Thời gian chuẩn bị tối đa (phút)').fill('20');
    await page.getByRole('button', { name: 'Áp dụng' }).click();
    await expect(page).toHaveURL(/maxPrepTime=20/);

    await page.goBack();
    await expect(page).not.toHaveURL(/maxPrepTime=20/);
    await expect(page).toHaveURL(/maxCookTime=45/);
    await page.goForward();
    await expect(page).toHaveURL(/maxPrepTime=20/);
  });

  test('bộ lọc độ khó + từ khóa ở /search giữ q khi đổi bộ lọc', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(`/search?q=${TERM}`);
    await page.getByLabel('Độ khó').selectOption({ index: 1 });
    await page.getByRole('button', { name: 'Áp dụng' }).click();
    await expect(page).toHaveURL(new RegExp(`q=${TERM}`));
    await expect(page).toHaveURL(/difficulty=/);
  });

  test('bộ lọc thời gian trên /search (MT-66) — chỉ kiểm khi giao diện Buổi 7 của Dev 4 đã có', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(`/search?q=${TERM}`);
    const field = page.getByLabel('Thời gian nấu tối đa (phút)');
    test.skip((await field.count()) === 0, 'Chưa có ô lọc thời gian trên /search (Dev 4 — Buổi 7)');
    await field.fill('45');
    await page.getByRole('button', { name: 'Áp dụng' }).click();
    await expect(page).toHaveURL(/maxCookTime=45/);
    await expect(page).toHaveURL(new RegExp(`q=${TERM}`));
  });

  test('phân trang giữ trạng thái URL (nếu đủ dữ liệu để có trang 2)', async ({ page }) => {
    await page.goto('/recipes?pageSize=1');
    const next = page.getByRole('link', { name: /trang sau|sau|tiếp/i }).first();
    test.skip((await next.count()) === 0, 'Không đủ công thức để có trang 2');
    await next.click();
    await expect(page).toHaveURL(/page=2/);
    await page.reload();
    await expect(page).toHaveURL(/page=2/);
  });
});

test.describe('hợp đồng API (cần API thật)', () => {
  test.skip(!API, 'Đặt E2E_API_URL để kiểm hợp đồng của Backend');
  test.use({ ignoreHTTPSErrors: true });

  test('GET /recipes?sortBy=maliciousColumn → 400 (danh sách trắng cột sắp xếp, FR-SRCH-003)', async ({ request }) => {
    const response = await request.get(`${API}/recipes?sortBy=maliciousColumn`);
    expect(response.status()).toBe(400);
  });

  test('Admin xóa danh mục còn công thức → 409 (MT-36)', async ({ request }) => {
    const email = process.env.E2E_ADMIN_EMAIL;
    const password = process.env.E2E_ADMIN_PASSWORD;
    test.skip(!email || !password, 'Đặt E2E_ADMIN_EMAIL và E2E_ADMIN_PASSWORD');

    const login = await request.post(`${API}/auth/login`, { data: { email, password } });
    expect(login.status()).toBe(200);
    const { accessToken } = await login.json();
    const headers = { Authorization: `Bearer ${accessToken}` };

    const categories = (await (await request.get(`${API}/categories`)).json()) as { id: string; recipeCount: number }[];
    const used = categories.find((c) => c.recipeCount > 0);
    test.skip(!used, 'Không có danh mục nào còn công thức');
    const response = await request.delete(`${API}/categories/${used!.id}`, { headers });
    expect(response.status()).toBe(409);
  });
});
