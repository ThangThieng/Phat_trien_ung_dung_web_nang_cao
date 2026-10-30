# Báo cáo WCAG 2.1 AA & Responsive — Buổi 8, Dev 3 (NFR-USE-001, NFR-USE-002)

**Dev 3:** Đoàn Hồng Tiến — 2314291 · **Ngày đo:** 30/10/2026 · **Spec:** `frontend/e2e/a11y.spec.ts`, `e2e/responsive.spec.ts`

## 1. Môi trường đo (đọc trước khi tin số liệu)

- `next build && next start` (Next.js 15.5.25, bản production) + **API giả** trả đúng hợp đồng SRS §8.2/§8.3 bằng dữ liệu mẫu của repo (`src/mocks/fixtures`: 6 danh mục, 4 công thức, một công thức có ảnh). Backend .NET và `docker-compose.prod.yml` **chưa chạy** trong lần đo này (máy đo không có .NET 10/Docker).
- Trình duyệt: Chromium 1243 qua Playwright 1.63 + `@axe-core/playwright` 4.13, thẻ `wcag2a, wcag2aa, wcag21a, wcag21aa`.
- Dashboard được quét bằng **phiên Admin giả lập** (refresh token trong `localStorage` + API refresh giả như `csp.spec.ts`); API còn lại của dashboard không có nên các màn hình hiển thị trạng thái lỗi/rỗng — vẫn quét được khung, điều hướng, biểu mẫu, nhưng **không** quét được trạng thái có dữ liệu thật.
- Cần chạy lại trên stack Docker thật ở buổi demo: `E2E_BASE_URL=https://localhost npx playwright test e2e/a11y.spec.ts e2e/responsive.spec.ts`.

## 2. Kết quả axe — **0 vi phạm** (mọi mức) sau khi sửa

| Nhóm route | Số route | serious/critical | Mọi mức (moderate, minor…) |
|---|---|---|---|
| Công khai: `/`, `/recipes`, `/recipes/{slug}`, `/categories`, `/categories/{slug}`, `/search`, `/search?q=pho`, `/auth/login`, `/auth/register` | 9 | **0** | 0 (quét riêng ở 1200 px) |
| Trang 404 (`/recipes/khong-ton-tai-123`) | 1 | **0** | — |
| Dashboard: `/dashboard`, `/categories`, `/recipes/new`, `/system`, `/media` | 5 | **0** | — |

Ngưỡng nghiệm thu là **0 serious/critical** (kế hoạch Buổi 8): axe còn báo mục "cần đánh giá thủ công" và cảnh báo minor không áp dụng cho ngữ cảnh; ngưỡng này đạt được và có nghĩa.

## 3. Lỗi tìm thấy khi kiểm chứng và đã sửa

Kế hoạch dự đoán "phần lớn đã đúng từ Buổi 6 — buổi này là kiểm chứng và vá nốt". Thực tế axe và kiểm thử bàn phím tìm ra 5 điểm:

| # | Phát hiện | Mức | Cách sửa |
|---|---|---|---|
| 1 | **Tương phản chữ trắng trên `bg-orange-600` chỉ 3,56 : 1** (cần ≥ 4,5 : 1) — nút "Tìm", "Áp dụng", "Đăng nhập", "Đăng ký", nút trong dashboard… (`/recipes`, `/dashboard`). Kế hoạch Buổi 6 ghi lỗi này "đã sửa ở token" nhưng `globals.css` hiện chưa có token (commit shadcn của Dev 2 chưa vào `develop`) nên class màu gắn cứng vẫn còn. | serious (WCAG 1.4.3) | `bg-orange-600` → `bg-orange-700` (**5,18 : 1**), `hover:bg-orange-700` → `hover:bg-orange-800` (7,31 : 1) ở 13 file (18 dòng). Khi token CSS của Dev 2 vào, thay một lần ở đó. |
| 2 | **Thứ tự heading nhảy cấp** trên `/recipes`: `h1` → `h3` (tiêu đề thẻ). Bảng lọc có `h2` nhưng bị ẩn dưới 1024 px. Lighthouse báo `heading-order`; axe ở 1200 px không thấy vì `h2` của bảng lọc hiện. | moderate (WCAG 1.3.1) | Thêm `<h2 class="sr-only">Danh sách công thức</h2>` trước lưới ở `/recipes`, `/categories/[slug]`, `/search`. |
| 3 | **Drawer "Bộ lọc" không trả focus** về nút mở khi đóng bằng Escape, và Tab thoát được ra ngoài dialog `aria-modal` (`FilterPanel` tự viết, không phải Radix như kế hoạch giả định). | WCAG 2.4.3, 2.1.2 | Lưu ref nút mở và `focus()` lại khi đóng; bẫy focus Tab/Shift+Tab trong bảng. Có kiểm thử bàn phím ở `responsive.spec.ts`. |
| 4 | **`prefers-reduced-motion` không được tôn trọng**: tồn tại animation 2000 ms (skeleton/xoay). | WCAG 2.3.3 | Khối `@media (prefers-reduced-motion: reduce)` trong `globals.css`. Có kiểm thử đo `transition/animation-duration` ≤ 10 ms. |
| 5 | `<title>` của trang công thức bị **lặp hậu tố** ("… \| Culinary Blog \| Culinary Blog") nếu `buildRecipeMetadata` tự thêm hậu tố, vì root layout đã có `template`. | SEO (không phải a11y) | `title: { absolute }` — ghi ở `seo-report.md`. |

## 4. Responsive (NFR-USE-001) — 320 / 768 / 1200 px

Đo bằng Playwright trên 7 route (`/`, `/recipes`, công thức, `/categories`, danh mục, `/search?q=pho`, `/auth/login`):

| Kiểm tra | 320 px | 768 px | 1200 px |
|---|---|---|---|
| Cuộn ngang (`scrollWidth − innerWidth`) | 0 | 0 | 0 |
| Đủ landmark `header` / `main` / `footer`, đúng một `h1` | ✅ | ✅ | ✅ |
| Lưới công thức trang chủ | 1 cột | 2 cột | 2 cột (còn cột phụ danh mục 16 rem) → 3 cột từ 1280 px |

Ảnh chụp: `docs/lab08-dev3/{home,recipe,category,categories,search}-{320,768,1200}.png` (15 ảnh).
Bộ lọc dạng drawer dưới 1024 px dùng được bằng chạm và bàn phím (mở → Escape → focus về nút mở).

## 5. Rà thủ công — kết quả và phần **chưa làm**

| Hạng mục | Kết quả |
|---|---|
| Landmark `header / nav / main / aside / footer` | ✅ `SidebarCategory` là `<aside>` bọc `<nav>` (Buổi 7); mỗi nhóm route có `<main id="main-content">` |
| Liên kết "Bỏ qua điều hướng" | ✅ là phần tử focus đầu tiên, nhảy tới `#main-content` |
| Vòng focus nhìn thấy | ✅ (`outline` hoặc `box-shadow` ≠ none trên phần tử focus đầu) |
| `lang="vi"`, `alt` ảnh theo `altText`, nhãn ô nhập | ✅ |
| Tương phản ≥ 4,5 : 1 | ✅ sau sửa #1 (axe `color-contrast` = 0) |
| **NVDA (Windows) / VoiceOver (macOS, iOS)** — NFR-USE-002 yêu cầu | ⏳ **Chưa làm** — cần người dùng thật trên máy có trình đọc màn hình. Kịch bản gợi ý: trang chủ → tìm "pho" → mở công thức → đánh dấu bước nấu → mở/đóng bộ lọc. |
| **BrowserStack (iOS, Android)** — NFR-USE-001 | ⏳ **Chưa làm** — chỉ có Chromium giả lập viewport. |
| Dashboard với dữ liệu thật | ⏳ quét lại trên stack Docker |
| `notFound()` từ trang (`/recipes/<slug-không-có>`, `/categories/<slug-không-có>`) | ⚠️ HTTP 404 đúng. Next 15.5 trả khung lỗi `<html id="__next_error__">` (chưa có `lang`) trong HTML đầu tiên, rồi router phía client dựng lại trang 404 trong root layout (`lang="vi"`, có header/footer) sau hydrate — đã kiểm trong Chromium trên bản production. Lỗi có sẵn từ Buổi 7, tái hiện y hệt trên mã `HEAD` và trên một trang tối thiểu chỉ gọi `notFound()` (nên là hành vi của Next 15.5.25, không phải mã dự án). Người dùng trình duyệt không bị ảnh hưởng; bộ cào/trình đọc màn hình không chạy JS sẽ thấy khung rỗng. Bài kiểm tra chờ hydrate rồi mới quét axe (đạt). Phiên bản Next khác có thể sửa — cần thử khi nâng cấp. |
| `target-size` (WCAG 2.2, Lighthouse ≥ 13) | ⚠️ Liên kết danh mục ở footer/sidebar bị cảnh báo kích thước chạm. Nằm ngoài 2.1 AA; lhci 0.15 (đang dùng) không kiểm. |
| `e2e/csp.spec.ts` (Dev 1, Buổi 6) | ⚠️ Viết cho `next dev` cùng origin; thất bại khi chạy với server dựng sẵn + API ở origin khác (đăng nhập không hiện nút, callback ngoài). Chưa đối chiếu với bản gốc và chưa chạy trên stack Docker/Nginx — nhiều khả năng do môi trường đo, cần xác minh. |

## 6. Cách chạy lại

```bash
cd frontend
npm run build && npm start          # hoặc stack Docker prod
E2E_BASE_URL=http://127.0.0.1:3000 npx playwright test e2e/a11y.spec.ts e2e/responsive.spec.ts
# trình duyệt cài sẵn khác phiên bản Playwright: thêm E2E_CHROMIUM_PATH=/đường/dẫn/chrome
```
