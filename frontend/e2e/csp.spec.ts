import { expect, test } from '@playwright/test';

function scriptSource(policy: string | null) {
  return policy?.split(';').find((directive) => directive.trim().startsWith('script-src')) ?? '';
}

test('login responds with a nonce CSP and hydrates without CSP violations', async ({ page, request }) => {
  const [first, second] = await Promise.all([request.get('/auth/login'), request.get('/auth/login')]);
  const firstPolicy = first.headers()['content-security-policy'];
  const secondPolicy = second.headers()['content-security-policy'];
  expect(scriptSource(firstPolicy)).toContain("'strict-dynamic'");
  expect(scriptSource(firstPolicy)).toContain('https://accounts.google.com/gsi/client');
  expect(scriptSource(firstPolicy)).not.toContain("'unsafe-inline'");
  expect(firstPolicy).not.toBe(secondPolicy);

  const violations: string[] = [];
  page.on('console', (message) => {
    if (message.type() === 'error' && /content security policy/i.test(message.text())) violations.push(message.text());
  });
  await page.goto('/auth/login');
  await expect(page.getByRole('button', { name: 'Đăng nhập' })).toBeVisible();
  expect(violations).toEqual([]);
});

test('auth, public, and guarded Dev1 routes do not emit CSP violations', async ({ page }) => {
  const violations: string[] = [];
  page.on('console', (message) => {
    if (message.type() === 'error' && /content security policy/i.test(message.text())) violations.push(message.text());
  });

  for (const route of ['/auth/login', '/auth/register', '/auth/forgot-password', '/auth/reset-password', '/dashboard/users', '/profile', '/recipes']) {
    await page.goto(route);
    await expect(page.locator('body')).toBeVisible();
  }

  expect(violations).toEqual([]);
});

test('a guest dashboard visit keeps the encoded internal callback', async ({ page }) => {
  await page.goto('/dashboard');
  await expect(page).toHaveURL(/\/auth\/login\?callbackUrl=%2Fdashboard$/);
});

test('an authenticated visitor with an external callback is sent to the safe fallback', async ({ page }) => {
  await page.addInitScript(() => {
    localStorage.setItem('culinaryblog.auth', JSON.stringify({
      state: {
        refreshToken: 'refresh-token',
        user: { id: 'u1', email: 'user@example.com', displayName: 'User', avatarUrl: null, bio: null, roles: ['Author'] },
      },
      version: 1,
    }));
  });
  await page.route('**/api/v1/auth/refresh', async (route) => {
    await route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({
        accessToken: 'access-token', refreshToken: 'replacement-token', expiresAt: '2026-12-31T00:00:00Z', expiresIn: 900,
        user: { id: 'u1', email: 'user@example.com', displayName: 'User', avatarUrl: null, bio: null, roles: ['Author'] },
      }),
    });
  });
  await page.goto('/auth/login?callbackUrl=https%3A%2F%2Fevil.example');
  await expect(page).toHaveURL(/\/dashboard$/);
});
