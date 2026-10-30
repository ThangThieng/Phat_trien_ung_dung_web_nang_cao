# BÁO CÁO KẾT QUẢ BUỔI 8 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (SEO / a11y / hiệu năng)
**Nhánh:** `2314291_DoanHongTien_buoiso8` (PR → `develop`, không vào `main`). Nhánh tách từ nhánh Buổi 7 nên chứa các commit Buổi 7 chưa merge.
**Phạm vi (KE_HOACH_PHAT_TRIEN_8_BUOI.md — Buổi 8, Dev 3):** JSON-LD Recipe, metadata/Open Graph/noindex, sitemap + robots, retrofit D-14 (ảnh tối ưu qua `/media`), responsive 320/768/1200, WCAG 2.1 AA, Lighthouse CI Core Web Vitals, E2E tìm kiếm.
**Báo cáo chi tiết:** `docs/seo-report.md`, `docs/a11y-report.md`, `docs/lighthouse-report.md`. Ảnh: `docs/lab08-dev3/` (15 ảnh bố cục + `og-sample.png`).

---

## 1. Tóm tắt

| # | Hạng mục | Kết quả |
|---|---|---|
| 1 | JSON-LD `Recipe` (NFR-SEO-001): đủ trường, `HowToStep` có `url`, dinh dưỡng, ảnh tuyệt đối, ISO 8601; **không** `aggregateRating` (MT-24); thoát `<` chống XSS. Thêm `BreadcrumbList`, `WebSite`+`SearchAction` | ✅ 30 test đơn vị + `seo.spec.ts` |
| 2 | Metadata (SEO-002): title ≤ 60, description ≤ 160 (MT-37), canonical, Open Graph `article`, Twitter, **ảnh OG 1200×630** có chữ tiếng Việt; `/search?q=` noindex; `(auth)` noindex | ✅ |
| 3 | `sitemap.xml` + `robots.txt` bằng Next.js (MT-28, không ping Google); nguồn `GET /recipes/sitemap`, cache 3600 s (MT-48); Draft/Archived → 404 (MT-34) | ✅ |
| 4 | Retrofit D-14: bỏ `unoptimized` mặc định, `remotePatterns` cho `/media/**`, `sizes`, `priority` thẻ đầu. Cờ `NEXT_PUBLIC_IMAGE_UNOPTIMIZED` (Dockerfile + compose) cho trường hợp demo | ✅ (chưa chạy qua Nginx thật) |
| 5 | Responsive 320/768/1200 (USE-001): 0 px cuộn ngang; lưới 1/2/2/≥3 cột | ✅ Chromium giả lập |
| 6 | WCAG 2.1 AA (USE-002): axe **0 vi phạm mọi mức** trên 7 route công khai; E2E 0 serious/critical trên 9 route công khai + 2 loại 404 + 5 route dashboard (phiên Admin giả lập) | ✅ (404 từ `notFound()` đạt sau hydrate — mục 5) |
| 7 | Lighthouse CI (PERF-005) | ✅ Mobile median: LCP 2,23–2,35 s, CLS 0, TBT ≤ 79 ms, JS < 200 KB. LCP `/recipes` 2,92 → 2,30 s nhờ bỏ `loading.tsx` bọc cả trang |
| 8 | E2E `search.spec.ts` | ✅ (ca cần API thật / bộ lọc thời gian của Dev 4 tự `skip`) |

## 2. Mâu thuẫn & giải pháp

| Mục | Giải pháp áp dụng |
|---|---|
| MT-24 | JSON-LD không có `aggregateRating`; có test khẳng định |
| MT-28 / MT-48 | Sitemap tự dựng bằng `app/sitemap.ts`; dữ liệu từ `GET /recipes/sitemap`, `revalidate: 3600` (đúng TTL Redis phía API). Ngoại lệ **một file** trong `revalidate-ttl.test.ts` (quy tắc chung ≤ 1800 s) kèm test khẳng định đúng 3600 |
| MT-34 | Draft/Archived → 404 thật; không vào sitemap |
| MT-37 | Cắt title/description là quy tắc *render* (cắt ở ranh từ), không phải ràng buộc dữ liệu; title dùng `absolute` để không lặp hậu tố |
| MT-67 | Trang vẫn `force-dynamic` (CSP nonce); `sitemap.ts` cũng `force-dynamic`, cache bằng `next.revalidate` trên `fetch` |
| D-14 | Ảnh đi qua Next Image Optimizer từ `/media/**` (Nginx → MinIO) |

## 3. Số đo (bản production + API giả lập, Chromium)

Lighthouse mobile (median 3 lần): `/` LCP 2,35 s · `/recipes` 2,30 s (trước sửa 2,92 s) · `/recipes/{slug}` 2,23 s · CLS 0 · TBT 44–79 ms · JS 181–187 KB (< 200) · Performance 96–98 · Accessibility 100 · SEO 100 · Best Practices 96. Desktop: LCP ≈ 0,6 s, Performance 100. Nhiễu giữa các lần chạy lớn (LCP `/recipes` 1,27–2,71 s).
Kiểm thử: Jest **44 suite / 340 test** đạt; Playwright (public, SEO, responsive, search, a11y) đạt, ca cần API thật `skip`.

## 4. Phát hiện khi kiểm chứng và đã sửa

Tương phản chữ trắng trên `orange-600` (3,56 : 1) → `orange-700` (5,18 : 1), 13 file; `heading-order` ở `/recipes`; ngăn kéo bộ lọc không trả/giữ focus; thiếu `prefers-reduced-motion`; ảnh OG vỡ chữ Việt (nhúng Inter latin+vietnamese, `runtime='edge'`); title lặp hậu tố. Chi tiết trong ba báo cáo `docs/`.

## 5. Tồn đọng / điểm lệch — ghi thẳng

1. **LCP có biên an toàn nhỏ:** median các trang 2,23–2,35 s so với ngưỡng 2,5 s, từng lần chạy dao động tới 2,71 s. Cần đo lại trên stack thật với ảnh thật. Bỏ `loading.tsx` của `/recipes` nghĩa là không còn khung chờ khi API chậm.
2. **Metadata stream (Next 15.5):** mặc định thẻ SEO nằm sau `</head>` với UA trình duyệt thường (Lighthouse SEO 92). Đã đặt `htmlLimitedBots: /.*/` để metadata luôn ở `<head>` → SEO 100, kèm cái giá TTFB chờ `generateMetadata` (dùng chung cache). **Đề xuất O-9** ghi nhận quyết định này cho cả nhóm.
3. **Hành vi Next 15.5.25 (có sẵn từ Buổi 7):** `notFound()` từ trang trả HTML đầu là khung `<html id="__next_error__">` chưa có `lang`; sau hydrate trình duyệt hiển thị đúng trang 404 (`lang="vi"`). Tái hiện cả trên trang tối thiểu chỉ gọi `notFound()`, nên không do mã dự án. Test chờ hydrate rồi quét axe (đạt). Còn lại: client không chạy JS thấy khung rỗng.
4. `e2e/csp.spec.ts` (Dev 1) thất bại khi chạy với server dựng sẵn + API khác origin; chưa đối chiếu bản gốc.
5. **Chưa làm:** chạy trên stack Docker/Nginx thật (D-14 `/media`, ảnh thật), Google Rich Results Test (cần URL công khai), Facebook Sharing Debugger/Zalo, NVDA/VoiceOver, BrowserStack. Các ca `/search` bộ lọc thời gian và phân trang thuộc Buổi 7 Dev 4 (nhánh `2312755_NguyenThangThieng_buoiso7` chưa merge) → `skip` khi thiếu.
6. Ảnh dùng cho đo LCP là ảnh giả lập nhỏ; số LCP thật phải đo lại với ảnh MinIO.

## 6. Cách chạy lại

```bash
cd frontend && npm run build && npm run start          # hoặc dùng stack Docker
E2E_BASE_URL=http://localhost:3000 npm run e2e          # thêm E2E_CHROMIUM_PATH nếu trình duyệt cài sẵn khác phiên bản
npm run lighthouse                                      # lhci autorun theo lighthouserc.json
npx jest                                                # 44 suite
```

## 7. File thay đổi (tóm tắt)

Mới: `src/lib/seo/*` (site, metadata, recipe-json-ld, sitemap-source + test), `src/lib/images.ts`, `src/components/seo/JsonLd.tsx`, `src/app/{sitemap,robots}.ts(+test)`, `src/app/(public)/recipes/[slug]/opengraph-image.tsx`, `src/assets/fonts/*`, `e2e/{a11y,seo,responsive,search,support}.ts`, `lighthouserc.json`, `docs/{a11y,lighthouse,seo}-report.md`, `docs/lab08-dev3/*`.
Sửa: trang công khai (home, recipes, recipes/[slug], categories/[slug], search), `RecipeCard`, `CategoryCard`, `StepChecklist`, `FilterPanel` (focus), màu `orange-600→700` (13 file), `globals.css`, `next.config.ts`, `Dockerfile`, `docker-compose.yml`, `package.json` (script `lighthouse`), `playwright.config.ts`, `revalidate-ttl.test.ts`.

## 8. Commit

Một commit squash trên `2314291_DoanHongTien_buoiso8` (Conventional Commits).
