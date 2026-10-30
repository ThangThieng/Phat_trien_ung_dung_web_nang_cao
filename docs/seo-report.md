# Báo cáo kiểm chứng SEO — Buổi 8, Dev 3 (NFR-SEO-001 → 003)

**Dev 3:** Đoàn Hồng Tiến — 2314291 · **Ngày đo:** 30/10/2026 · **Spec tự động:** `frontend/e2e/seo.spec.ts` (+ 30 bài kiểm thử đơn vị ở `src/lib/seo/*.test.ts`, `src/app/robots.test.ts`)

## 1. Môi trường

`next build && next start` (production, Next.js 15.5.25) + API giả đúng hợp đồng SRS (dữ liệu mẫu của repo). Chưa chạy trên stack Docker/HTTPS thật và **chưa chạy Google Rich Results Test** (cần URL công khai) — hai việc này nằm ở §5.

## 2. Kết quả theo hạng mục của kế hoạch (§ Buổi 8 — Dev 3, mục 9)

| Hạng mục | Kết quả | Bằng chứng |
|---|---|---|
| `<title>` thực tế ≤ 60 ký tự sau cắt, đúng một hậu tố | ✅ "Phở bò Hà Nội \| Culinary Blog" (30); tên dài bị cắt theo ranh giới từ + `…`, tổng ≤ 60 | `metadata.test.ts`, `seo.spec.ts` |
| `<meta description>` ≤ 160 ký tự, cắt theo từ | ✅ | idem |
| Canonical tuyệt đối theo slug | ✅ `http://…/recipes/pho-bo-ha-noi` | `seo.spec.ts` |
| Open Graph `article` + Twitter `summary_large_image` | ✅ `og:title/description/url/type`, `article:published_time` | `seo.spec.ts` |
| **Ảnh OG đúng 1200×630** | ✅ `og:image:width=1200`, `height=630`; tải về là PNG 1200×630 (đọc IHDR) — cả bản `next start` lẫn `node .next/standalone/server.js` | `seo.spec.ts`; `docs/lab08-dev3/og-sample.png` |
| JSON-LD `Recipe` đủ `name, description, image, author, datePublished, prepTime, cookTime, totalTime, recipeYield, recipeIngredient[], recipeInstructions[], nutrition` | ✅ thời gian dạng ISO 8601 (`PT40M`, `PT6H`), bước là `HowToStep` có `url …#buoc-{n}` (neo `id="buoc-{n}"` đã thêm vào `StepChecklist`) | `recipe-json-ld.test.ts`, `seo.spec.ts` |
| **Không có `aggregateRating`** (MT-24) | ✅ khẳng định cả ở kiểm thử đơn vị và E2E | idem |
| `BreadcrumbList` (công thức, danh mục), `WebSite` + `SearchAction` (trang chủ) | ✅ | `seo.spec.ts` |
| **Chống XSS qua JSON-LD** — tiêu đề `</script><script>alert(1)</script>` | ✅ `<` được thoát thành `<`; HTML không còn `<` thô trong thẻ ld+json; `JSON.parse` ngược vẫn ra đúng chuỗi | `recipe-json-ld.test.ts`, `seo.spec.ts` |
| `/recipes/<slug-Draft>` → HTTP **404** | ✅ 404 thật (không phải 200 "soft 404"); Draft/Archived cũng 404 ở API (MT-34) | `seo.spec.ts` |
| `/sitemap.xml` không chứa slug Draft/Archived | ✅ endpoint chỉ trả Published; E2E request **từng URL công thức trong sitemap** (tối đa 25) và đều 200 | `seo.spec.ts` |
| `/robots.txt`, `/sitemap.xml` ở domain chính | ✅ `Sitemap: <SITE_URL>/sitemap.xml`; có `<loc> <lastmod> <changefreq> <priority>`; trang tĩnh `/ /recipes /categories /search` + danh mục + công thức | `seo.spec.ts`, `sitemap-source.test.ts`, `robots.test.ts` |
| `robots` cho phép `/`, chặn `/dashboard/ /profile /auth/ /api/ /hangfire /scalar /mockups`; **không ping Google** | ✅ | `robots.test.ts` |
| `/search?q=…` → `noindex, follow`; `/search` trống → index được | ✅ | `seo.spec.ts` |
| `(auth)` `noindex`; `/mockups` → 404 ở bản production | ✅ | `seo.spec.ts` |

## 3. Phát hiện khi kiểm chứng và đã sửa

| # | Phát hiện | Cách xử lý |
|---|---|---|
| 1 | **Metadata nằm sau `</head>`** (stream vào body) — `<title>`, `description`, canonical, `og:*` ở byte 14 717 trong khi `</head>` ở byte 3 110. Lighthouse SEO 92; bộ cào không chạy JS không thấy `og:*` (Zalo/Facebook không dựng được thẻ chia sẻ). | `htmlLimitedBots: /.*/` — metadata chặn render tới khi sẵn sàng, luôn ở `<head>`. Cái giá: TTFB chờ `generateMetadata`, vốn dùng chung `cache()`/Data Cache với trang nên không thêm lượt gọi API. Lighthouse SEO → 100. Trang 404 từ `notFound()` kiểm lại sau hydrate: vẫn đúng. |
| 2 | **Ảnh OG vỡ chữ tiếng Việt.** `ImageResponse` (satori) không có glyph tiếng Việt và cố tải font động từ Google Fonts (*"Failed to load dynamic font"*) — máy chủ Next.js trong container không ra Internet. Lần dựng đầu chữ hiển thị "NO GLYPH". | Nhúng Inter Bold (OFL) hai tập con latin + vietnamese vào `src/assets/fonts/` (38 KB), ghép bằng `font-family: Inter, InterVietnamese` (satori không dự phòng giữa hai font **cùng tên**; đã thử). |
| 3 | `fetch(new URL('./font.woff', import.meta.url))` lỗi `Invalid URL` ở runtime Node. | Route OG dùng `runtime = 'edge'` — cách Next.js hướng dẫn nhúng font; không còn `Buffer` nên mã hóa base64 ảnh nền bằng `btoa`. Kiểm cả chế độ `standalone`. |
| 4 | `title` bị **lặp hậu tố** nếu tự thêm "\| Culinary Blog" (root layout đã có `template`). | `title: { absolute }`, cắt tên theo `60 − len(" \| Culinary Blog")` = 44 ký tự để hậu tố thương hiệu không bị cắt. |
| 5 | `revalidate-ttl.test.ts` (quy tắc vàng "≤ 1800 s") đỏ vì `sitemap-source.ts` dùng **3600**. | 3600 là **đúng**: là TTL Redis của chính `GET /recipes/sitemap` (MT-48). Kiểm thử có ngoại lệ **một file**, kèm một bài khẳng định file đó đúng 3600 — để ai đó đổi số cũng bị bắt. |

## 4. Điểm lệch so với kế hoạch

| Kế hoạch ghi | Thực tế | Lý do |
|---|---|---|
| `app/sitemap.ts` có `revalidate = 3600` | `export const dynamic = 'force-dynamic'`; 1 giờ nhờ Data Cache của `getSitemapRecipes()` | `next build` trong image Docker chạy khi API chưa có — cùng lý do ba trang công khai khác (Buổi 7). Hành vi cho crawler giống hệt. |
| `opengraph-image.tsx` dùng `ImageResponse` | Thêm `runtime = 'edge'`, font nhúng, ảnh nền tải trước dạng data URI (PNG/JPEG) | Mục 3 #2–3. Ảnh WebP (nếu có) không vẽ được trong satori → rơi về nền gradient thay vì làm hỏng thẻ. |
| `images.remotePatterns` cho `/media/**` | Thêm `/media/**` (theo `NEXT_PUBLIC_SITE_URL`), giữ `/culinary-blog/**` dự phòng, `lh3.googleusercontent.com`; biến `NEXT_PUBLIC_IMAGE_UNOPTIMIZED` mặc định `false` | `/media/` là việc của Dev 4 (Buổi 7) chưa vào `develop`; biến cho phép dev local không có Nginx vẫn xem được ảnh thay vì gỡ hẳn lối thoát. |

## 5. Việc chưa làm / cần người

| Việc | Vì sao chưa | Cách làm |
|---|---|---|
| **Google Rich Results Test** (NFR-SEO-001 "pass 100%") | Cần URL công khai; công cụ không gọi được từ máy đo | Dán URL một công thức đã deploy vào https://search.google.com/test/rich-results, hoặc dán nguyên HTML. Cấu trúc JSON-LD đã khớp Schema.org `Recipe` (ISO 8601, `HowToStep`, không `aggregateRating`). |
| Chạy `seo.spec.ts` trên stack Docker/HTTPS tự ký | Backend/Docker chưa có ở máy đo | `E2E_BASE_URL=https://localhost` + `NODE_TLS_REJECT_UNAUTHORIZED=0` hoặc cấu hình `ignoreHTTPSErrors` |
| Ảnh OG với **ảnh món thật** từ `/media/` | API giả chỉ có một ảnh PNG nhỏ | Mở `/recipes/{slug}/opengraph-image-…` trên stack thật |
