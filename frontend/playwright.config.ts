import { defineConfig, devices } from '@playwright/test';

const port = 3100;
/**
 * `E2E_BASE_URL`: chạy kiểm thử trên một môi trường đã dựng sẵn (stack production giống thật — `docker-compose.prod.yml`,
 * Buổi 8 đầu buổi, hoặc `next start` + API). Không đặt thì Playwright tự chạy `next dev` — đủ cho `csp.spec.ts`, còn các
 * spec cần dữ liệu (a11y, responsive, seo, search) tự bỏ qua vì không có API.
 */
const external = process.env.E2E_BASE_URL;

export default defineConfig({
  testDir: './e2e',
  timeout: 45_000,
  // Quét axe mọi route khá nặng; chạy tuần tự trên môi trường dùng chung để số đo ổn định.
  workers: external ? 2 : undefined,
  reporter: [['list']],
  use: {
    baseURL: external ?? `http://127.0.0.1:${port}`,
    trace: 'retain-on-failure',
    // Trình duyệt cài sẵn khác phiên bản Playwright của dự án (CI, container): trỏ thẳng tới file chạy.
    launchOptions: { executablePath: process.env.E2E_CHROMIUM_PATH || undefined },
  },
  webServer: external
    ? undefined
    : {
        command: `node node_modules/next/dist/bin/next dev --port ${port}`,
        url: `http://127.0.0.1:${port}`,
        reuseExistingServer: !process.env.CI,
      },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
