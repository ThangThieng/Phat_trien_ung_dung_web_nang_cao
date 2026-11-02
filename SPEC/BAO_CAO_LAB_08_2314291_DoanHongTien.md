# BÁO CÁO KẾT QUẢ BUỔI 8 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (Danh mục, Tìm kiếm FTS & SEO)
**Nhánh:** `2314291_DoanHongTien_buoiso8` (PR → `develop`, không vào `main`). Đã hợp nhất `develop` ngày 03/11/2026 (Buổi 7 của cả nhóm đã merge) và giải xung đột — PR không còn kéo theo commit Buổi 7.
**Phạm vi (KE_HOACH_PHAT_TRIEN_8_BUOI.md — Buổi 8, Dev 3):** JSON-LD Recipe, metadata/Open Graph/noindex, sitemap + robots, retrofit D-14 phía Next.js, responsive 320/768/1200, WCAG 2.1 AA, Lighthouse CI Core Web Vitals, E2E tìm kiếm/bộ lọc.
**Báo cáo chi tiết:** `docs/seo-report.md`, `docs/a11y-report.md`, `docs/lighthouse-report.md`. Ảnh: `docs/lab08-dev3/` (15 ảnh bố cục chụp lại trên bản đã hợp nhất + `og-sample.png`).

---

## 0. Phần việc của Dev 3 qua 8 buổi (đối chiếu kế hoạch và `develop`)

| Buổi | Việc của Dev 3 | Trạng thái |
|---|---|---|
| 1 | Đọc SRS, tìm hiểu module Danh mục / Tìm kiếm / SEO | ✅ |
| 2 | Lát cắt Category: API + trang danh mục | ✅ có trên `develop` |
| 3 | CRUD danh mục [Admin] FR-CAT-003/004/005, Category exceptions & repository, D-13 | ✅ nhánh `buoiso3` đã vào `develop` |
| 4 | API truy vấn công thức: FTS (FR-SRCH-001), lọc/sắp xếp/phân trang (FR-SRCH-002/003/004, D-10), cách ly Public/Private (D-4, D-5, D-6), `/recipes/mine`, `GET /recipes/sitemap`, D-18 | ✅ nhánh `buoiso4` đã vào `develop` |
| 5 | Chuẩn hóa cấu trúc Frontend, kiểu TS Category/Search, composite index, D-10 phía FE, D-7 `/categories/[slug]` | ✅ merge `3766b38` |
| 6 | Giao diện tĩnh & component dùng chung, bộ lọc thời gian cho `/recipes/search` (MT-66) | ✅ merge `adae8aa` |
| 7 | Trang chủ, `/categories`, `/categories/[slug]`, hoàn tất D-7 | ✅ merge `6adbfd8` |
| **8** | **SEO (JSON-LD, OG, sitemap, robots), D-14 phía Next.js, responsive, WCAG 2.1 AA, Lighthouse CI, E2E tìm kiếm** | **Báo cáo này** |

FR thuộc Dev 3 (FR-CAT-001 → 005, FR-SRCH-001 → 004) đã xong ở Buổi 2 – 7. Buổi 8 hoàn tất NFR-SEO-001/002/003, NFR-USE-001/002, NFR-PERF-005 và D-14 phía Next.js.

## 1. Tóm tắt Buổi 8

| # | Hạng mục | Kết quả |
|---|---|---|
| 1 | JSON-LD `Recipe` (NFR-SEO-001): đủ trường, `HowToStep` có `url`, dinh dưỡng, ảnh tuyệt đối, ISO 8601; **không** `aggregateRating` (MT-24); thoát `<` chống XSS. Thêm `BreadcrumbList`, `WebSite` + `SearchAction` | ✅ 30 test đơn vị + `seo.spec.ts`; gắn lại vào trang chi tiết mới của Dev 2 sau hợp nhất |
| 2 | Metadata (SEO-002): title ≤ 60, description ≤ 160 (MT-37), canonical, Open Graph `article`, Twitter, **ảnh OG 1200×630** có chữ tiếng Việt; `/search?q=` noindex (gắn vào trang `/search` mới của Dev 4); `(auth)`, `(dashboard)`, `(dev)` noindex | ✅ |
| 3 | `sitemap.xml` + `robots.txt` bằng Next.js (MT-28, không ping Google); nguồn `GET /recipes/sitemap`, cache 3600 s (MT-48); Draft/Archived → 404 (MT-34) | ✅ |
| 4 | Retrofit D-14: bỏ `unoptimized` (kể cả `RecipeGallery`, `RecipeLightbox` mới), `remotePatterns` theo `NEXT_PUBLIC_MINIO_PUBLIC_URL` = `<site>/media` của Dev 4, `sizes`, `priority`; lưới dùng `ThumbnailUrl` (API trả sẵn) | ✅ (chưa chạy qua Nginx thật) |
| 5 | Responsive 320/768/1200 (USE-001): 0 px cuộn ngang; lưới 1/2/2/≥3 cột; **ảnh lấp đầy khung dưới CSP production** (kiểm thử mới) | ✅ Chromium giả lập |
| 6 | WCAG 2.1 AA (USE-002): axe **0 vi phạm mọi mức** trên 7 route công khai; E2E 0 serious/critical trên 9 route công khai + 2 loại 404 + 5 route dashboard | ✅ |
| 7 | Lighthouse CI (PERF-005): `lighthouserc.json` theo ngưỡng SRS; LCP `/recipes` 2,92 → 2,30 s (30/10) | ⚠️ Đạt trên máy đo 30/10; máy đo 03/11 yếu hơn nên LCP không đạt ở cả bản cũ lẫn bản mới (mục 3) |
| 8 | E2E `search.spec.ts`: "pho" → "Phở"; bộ lọc thời gian ở `/recipes` **và `/search` (MT-66)** giữ URL, F5, back/forward; phân trang | ✅ 6 ca giao diện đạt; 2 ca hợp đồng API (400, 409) cần API thật |

## 2. Mâu thuẫn & giải pháp đã áp dụng

| Mục | Giải pháp |
|---|---|
| MT-24 | JSON-LD không có `aggregateRating`; có test khẳng định |
| MT-28 / MT-48 | Sitemap bằng `app/sitemap.ts`; dữ liệu từ `GET /recipes/sitemap`, `revalidate: 3600` (đúng TTL Redis phía API). Ngoại lệ **một file** trong `revalidate-ttl.test.ts` kèm test khẳng định đúng 3600 |
| MT-34 | Draft/Archived → 404 thật; không vào sitemap |
| MT-37 | Cắt title/description là quy tắc *render* (cắt ở ranh từ); title dùng `absolute` để không lặp hậu tố |
| MT-66 | E2E kiểm bộ lọc thời gian trên cả `/recipes` và `/search` |
| MT-67 | Trang vẫn `force-dynamic` (CSP nonce); cache bằng `next.revalidate` trên `fetch` |
| D-14 | Ảnh đi qua Next Image Optimizer từ `/media/**` (Nginx → MinIO) |

## 3. Số đo

**Lần 1 (30/10, máy đo `benchmarkIndex` ≈ 3 100, Lighthouse mặc định):** `/` LCP 2,35 s · `/recipes` 2,30 s · `/recipes/{slug}` 2,23 s · CLS 0 · TBT 44–79 ms · JS 181–187 KB · Accessibility 100 · SEO 100.

**Lần 2 (03/11, sau hợp nhất `develop`, máy đo `benchmarkIndex` ≈ 1 100–1 600, CPU chậm 1,7× cho cả hai bản để so sánh):**

| Trang | Bản 30/10 | Bản đã hợp nhất |
|---|---|---|
| `/` | LCP 2,67 – 3,10 s | LCP 2,64 – 2,76 s |
| `/recipes` | LCP 2,81 – 3,25 s | LCP 2,80 – 3,11 s |
| `/recipes/{slug}` | LCP 1,54 – 2,91 s | LCP 3,46 – 3,80 s (trang chi tiết mới của Dev 2: 287 KB JS gồm chunk tải lười; First Load JS theo `next build` 194 kB) |

CLS 0, TBT < 100 ms, Accessibility 100, SEO 100 ở mọi trang. Con số tuyệt đối phụ thuộc máy đo — cần đo lại bằng `npm run lighthouse` trên máy demo.

**Kiểm thử trên bản đã hợp nhất:** Jest **61 suite / 417 test** đạt; ESLint 0 lỗi; `tsc` sạch; Playwright **36 đạt**, 1 đỏ (`csp.spec.ts` — mục 5), 2 bỏ qua (cần API thật); ca phân trang chạy khi dữ liệu đủ 2 trang (đã chạy với dữ liệu 20 công thức: đạt).

## 4. Phát hiện khi kiểm chứng và đã sửa

- Tương phản chữ trắng trên `orange-600` (3,56 : 1) → `orange-700` (5,18 : 1): 13 file, cộng 2 form mới của Buổi 7 (`ForgotPasswordForm`, `ResetPasswordForm`).
- `heading-order` ở `/recipes`; ngăn kéo bộ lọc không trả/giữ focus; thiếu `prefers-reduced-motion`.
- Ảnh OG vỡ chữ Việt (nhúng Inter latin + vietnamese, `runtime='edge'`); title lặp hậu tố.
- LCP `/recipes` 2,92 → 2,30 s: bỏ `loading.tsx` bọc cả trang (ảnh LCP bị giấu trong Suspense tới cuối luồng).
- Metadata bị stream xuống `<body>` (Lighthouse SEO 92) → `htmlLimitedBots` → SEO 100.
- **Sau hợp nhất:** CSP production của `develop` chặn thuộc tính `style` → mọi ảnh `next/image fill` vỡ (cao ~24 px). Sửa phía ảnh: `width`/`height` + lớp `absolute inset-0 h-full w-full`; thêm kiểm thử. Không đụng `csp.ts` của Dev 1.
- **Sau hợp nhất:** E2E bộ lọc `/search` viết lại cho giao diện mới của Dev 4 (nhóm liên kết, chọn là áp dụng, không có nút "Áp dụng").

## 5. Tồn đọng — ghi thẳng

1. **LCP:** đạt 2,23–2,35 s trên máy đo 30/10, không đạt trên máy đo 03/11 (cả bản cũ). Trang chi tiết sau hợp nhất chậm hơn ~0,7 s và tổng JS 287 KB → cần Dev 2 xem phần tải sau hydrate.
2. **`e2e/csp.spec.ts`:** thẻ `<style>` của Sonner bị CSP production chặn. Dev 1 và Dev 4 đang sửa bằng hash trên nhánh Buổi 8 của họ.
3. **`notFound()` của Next 15.5.25:** HTML đầu là khung `<html id="__next_error__">` chưa có `lang`; sau hydrate đúng. Không do mã dự án.
4. **Hợp đồng API 400 / 409 trong E2E:** cần API thật (`E2E_API_URL`). Hành vi đã có integration test phía Backend (`RecipesEndpointsTests`, `RecipeQueryEndpointsTests`, `CategoryAdminEndpointsTests`) từ Buổi 3 – 4.
5. **Cần môi trường/người thật:** stack Docker/Nginx (D-14 `/media`, ảnh thật), Google Rich Results Test, Facebook/Zalo debugger, NVDA/VoiceOver, BrowserStack.
6. Bỏ `loading.tsx` của `/recipes` nghĩa là không còn khung chờ khi API chậm.

## 6. Cách chạy lại

```bash
cd frontend && npm run build && npm run start          # hoặc stack Docker
E2E_BASE_URL=http://localhost:3000 npm run e2e          # + E2E_API_URL, E2E_ADMIN_EMAIL/PASSWORD cho ca 400/409
npm run lighthouse                                      # lhci autorun theo lighthouserc.json
npx jest
```

## 7. File thay đổi (tóm tắt)

Mới: `src/lib/seo/*` (site, metadata, recipe-json-ld, sitemap-source + test), `src/lib/images.ts`, `src/components/seo/JsonLd.tsx`, `src/app/{sitemap,robots}.ts` (+ test), `src/app/(public)/recipes/[slug]/opengraph-image.tsx`, `src/assets/fonts/*`, `e2e/{a11y,seo,responsive,search,support}.ts`, `lighthouserc.json`, `docs/{a11y,lighthouse,seo}-report.md`, `docs/lab08-dev3/*`.
Sửa: trang công khai (home, recipes, recipes/[slug], categories/[slug], search), `RecipeCard`, `CategoryCard`, `RecipeGallery`, `RecipeLightbox`, `StepChecklist`, `FilterPanel`, màu nút (15 file), `globals.css`, `next.config.ts`, `Dockerfile`, `docker-compose.yml`, `.env.example`, `package.json`, `playwright.config.ts`, `revalidate-ttl.test.ts`; xóa `recipes/(list)/loading.tsx`.

## 8. Commit

Các commit trên `2314291_DoanHongTien_buoiso8` (Conventional Commits) + một merge commit từ `develop`; squash khi merge PR.
