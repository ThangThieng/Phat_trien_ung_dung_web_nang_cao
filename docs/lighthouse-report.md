# Báo cáo Core Web Vitals — Buổi 8, Dev 3 (NFR-PERF-005)

**Công cụ:** Lighthouse CI (`@lhci/cli` 0.15) — cấu hình `frontend/lighthouserc.json` (3 lần chạy/URL, lấy **median**) · **Ngày đo:** 30/10/2026
**Ngưỡng SRS:** LCP ≤ 2,5 s · CLS ≤ 0,1 · INP ≤ 200 ms · First Load JS ≤ 200 KB (gzip).

## 1. Môi trường đo — giới hạn của con số

- `next build && next start` (production) + API giả; ảnh công thức là **PNG 800×600 nhỏ (~1 KB)** do API giả phục vụ. **Ảnh thật ~60–150 KB sẽ làm LCP dài hơn** — con số dưới đây là *sàn* chứ không phải kết quả cuối. Phải đo lại trên stack Docker thật với dữ liệu mẫu có ảnh (đầu Buổi 8, Dev 4 dựng).
- **Mobile** = cấu hình mặc định của Lighthouse (Moto G Power, mô phỏng 4G chậm, CPU chậm 4×) — là bài đo khắt khe nhất và là cách Google chấm Core Web Vitals trên di động. **Desktop** = `preset: desktop`.
- **INP không đo được trong Lighthouse** (chỉ có ở dữ liệu thực địa/đo tương tác). Dùng **Total Blocking Time ≤ 200 ms** làm chỉ báo thay thế trong phòng thí nghiệm — cách Google khuyến nghị.
- First Load JS đo bằng `resource-summary:script` (dung lượng truyền, đã nén).

## 2. Kết quả (median của 3 lần)

### Mobile (mô phỏng)

| Trang | LCP | CLS | TBT (≈ INP) | JS truyền (KB) | Performance | Accessibility | SEO | Best Practices |
|---|---|---|---|---|---|---|---|---|
| `/` | **2,35 s** ✅ | 0 ✅ | 44 ms ✅ | 181 ✅ | 96 | 100 | 100 | 96 |
| `/recipes` | **2,30 s** ✅ | 0 ✅ | 79 ms ✅ | 187 ✅ | 96 | 100 | 100 | 96 |
| `/recipes/{slug}` | **2,23 s** ✅ | 0 ✅ | 70 ms ✅ | 184 ✅ | 98 | 100 | 100 | 96 |

### Desktop

| Trang | LCP | CLS | TBT | Performance | A11y | SEO | BP |
|---|---|---|---|---|---|---|---|
| `/` | 0,62 s | 0,001 | 0 ms | 100 | 100 | 100 | 100 |
| `/recipes` | 0,63 s | 0,001 | 0 ms | 100 | 100 | 100 | 100 |
| `/recipes/{slug}` | 0,61 s | 0,001 | 0 ms | 100 | 100 | 100 | 100 |

**Đạt:** CLS, TBT/INP, First Load JS (181–187 KB < 200 KB), toàn bộ ngưỡng ở desktop, LCP của trang chủ và trang chi tiết trên mobile.
**Mọi ngưỡng đạt** ở lần đo cuối (median 3 lần). Lưu ý nhiễu: LCP từng lần dao động lớn (`/recipes`: 1,27 / 2,30 / 2,71 s) nên median chỉ cách ngưỡng ~0,2 s — cần đo lại trên stack thật. Best Practices 96 (trước 100) do môi trường đo bị proxy chặn các kết nối ra Google.

## 3. Việc đã sửa nhờ đo (trước → sau)

| Phát hiện | Trước | Sau |
|---|---|---|
| **SEO 92 điểm** trên cả 3 trang: *"Document does not have a meta description"*. Nguyên nhân: Next 15.5 **stream** metadata vào cuối `<body>` cho UA trình duyệt thường (thẻ chỉ lên `<head>` sau khi JS chạy) — mọi bộ cào không chạy JS (trình xem trước liên kết Zalo/Facebook, nhiều công cụ SEO) cũng không thấy `og:*`. | SEO 92; `<title>`/`description` ở byte 14 717, sau `</head>` (byte 3 110) | `htmlLimitedBots: /.*/` trong `next.config.ts`: **SEO 100**, metadata nằm trong `<head>` của HTML đầu tiên. Đã kiểm trang 404 sau hydrate vẫn đúng với cấu hình này |
| Trang chủ: LCP vượt 2,5 s (ảnh thẻ đầu đã `priority`) | 2,58 s | 2,42 s sau khi `/recipes`, danh mục, tìm kiếm cũng `priority` thẻ đầu và gỡ `unoptimized` (ảnh đi qua tối ưu hóa, `sizes` đúng breakpoint) |
| `heading-order` trên `/recipes` | a11y 99 | 100 (xem `a11y-report.md` #2) |

## 4. LCP `/recipes` trên mobile mô phỏng: 2,92 s → 2,30 s

Phân rã trước khi sửa: TTFB 452 ms · Load Delay 228 ms · Load Time 97 ms · **Render Delay 2 159 ms (74%)**. Ảnh tải xong sớm nhưng chỉ được hiển thị muộn; main thread không quá tải (long task ≤ 128 ms). Nguyên nhân: `recipes/(list)/loading.tsx` bọc trang trong Suspense — HTML gửi khung xương (`animate-pulse`), nội dung thật (kèm ảnh LCP) nằm trong `<div hidden>` và chỉ được "lật" ra bởi script `$RC` ở cuối luồng.

**Đã sửa:** bỏ `recipes/(list)/loading.tsx` (trang đã `force-dynamic` và gọi API có Data Cache, nên TTFB vẫn thấp). Trang trả HTML đầy đủ một lần, ảnh `priority` hiện ngay. Kết quả: LCP `/recipes` median **2,30 s**; kiểm thử Jest 44 suite và E2E (a11y, SEO, responsive, search) vẫn đạt.

Đánh đổi: khi API chậm, `/recipes` không còn khung chờ mà đứng yên tới khi có dữ liệu. `categories/(list)/loading.tsx` giữ nguyên (chưa phát hiện vấn đề LCP). Nếu cần khung chờ, đặt Suspense quanh phần lưới thay vì cả trang.

## 4b. Đo lại sau khi hợp nhất `develop` (03/11/2026 — Buổi 7 của cả nhóm đã merge)

Nhánh Buổi 8 được hợp nhất với `develop` (trang `/search` mới của Dev 4, trang chi tiết mới của Dev 2, CSP của Dev 1). Lần đo này chạy trên **máy yếu hơn**: Lighthouse báo `benchmarkIndex` ≈ 1 100–1 600, so với ≈ 3 100 ở lần đo 30/10 — dưới mức 1 300 Lighthouse coi là kém tin cậy. Vì vậy hạ `cpuSlowdownMultiplier` xuống 1,7 (≈ 4 × 1 300 / 3 100) cho **cả hai bản** và chỉ so sánh tương đối:

| Trang | Bản 30/10 (trước hợp nhất), máy này | Bản đã hợp nhất, máy này | Nhận xét |
|---|---|---|---|
| `/` | LCP 2,67 / 3,10 / 2,91 s · TBT 60 ms | LCP 2,76 / 2,76 / 2,64 s · TBT 77 ms | Ngang nhau |
| `/recipes` | LCP 2,81 / 3,00 / 3,25 s · TBT 77 ms | LCP 2,87 / 2,80 / 3,11 s · TBT 93 ms | Ngang nhau |
| `/recipes/{slug}` | LCP 2,91 / 2,78 / 1,54 s · TBT 151 ms | LCP 3,46 / 3,64 / 3,80 s · TBT 87 ms | **Chậm hơn ~0,7 s** |

CLS = 0 ở mọi lần; Accessibility 100, SEO 100 cả ba trang.

**Trang chi tiết sau hợp nhất:** trang mới của Dev 2 (thư viện ảnh, lightbox, khẩu phần, in/chia sẻ) tải tổng **287 KB** JS theo Lighthouse (gồm cả chunk tải lười sau hydrate), so với 184 KB trước đây. Theo thước đo của kế hoạch — **First Load JS** do `next build` báo — trang này là **194 kB**, vẫn ≤ 200 kB. Vì `resource-summary:script:size` của Lighthouse tính cả chunk tải lười, ngưỡng này sẽ đỏ ở trang chi tiết; cần Dev 2 xem lại phần nào tải ngay sau hydrate. LCP là ảnh chính, thời gian nằm ở *render delay* (ảnh chỉ được vẽ sau khi luồng chính hydrate xong).

**Kết luận trung thực:** trên máy đo lần này, **không trang nào đạt LCP ≤ 2,5 s — kể cả bản 30/10 từng đạt 2,23–2,35 s trên máy nhanh hơn**. Con số tuyệt đối phụ thuộc máy đo; cần đo lại bằng `npm run lighthouse` trên máy demo với stack thật và ảnh thật.

**Phát hiện trong lúc đo — ảnh vỡ dưới CSP production:** CSP của `develop` (`style-src 'self'`, không `'unsafe-inline'` ở production) chặn thuộc tính `style` trong HTML. `next/image` với `fill` dựa vào `style="position:absolute;…"` nên mọi ảnh thẻ/ảnh chính rơi về `position: static`, cao ~24 px. Đã sửa ở phía ảnh (không đụng CSP của Dev 1): bỏ `fill`, dùng `width`/`height` + lớp `absolute inset-0 h-full w-full` (`RecipeCard`, `CategoryCard`, `RecipeGallery`, `RecipeLightbox`); thêm kiểm thử *"ảnh lấp đầy khung"* trong `e2e/responsive.spec.ts`.

## 4c. Đo lại trên hệ thống thật sau khi phát hành v1.0.0 (18–19/11/2026)

**Khác các lần trước:** đo thẳng trên Docker dev (`http://localhost`, qua Nginx): bản build production của Next.js, **API thật, ảnh thật**
(WebP 800×600, 10–30 KB), giao diện navy – vàng (font Be Vietnam Pro + Playfair Display, logo). Lighthouse 12.6.1 trong Edge headless,
3 lần/URL, median; `benchmarkIndex` ≈ 3 100 – 3 500.

| Trang | Mobile LCP (ban đầu) | Mobile LCP (sau sửa) | Desktop LCP (sau sửa) | CLS | TBT mobile | A11y / SEO / BP |
|---|---|---|---|---|---|---|
| `/` | 3,96 s ❌ | 3,96 s ❌ | 0,96 s ✅ | 0 | 28 ms ✅ | 100 / 100 / 100 |
| `/recipes` | 4,54 s ❌ | 4,11 s ❌ | 0,94 s ✅ | 0 | 28 ms ✅ | 100 / 100 / 100 |
| `/recipes/{slug}` | 3,65 s ❌ | 3,99 s ❌ | 0,83 s ✅ | 0 – 0,013 | 48 ms ✅ | 100 / 100 / 100 |

**Kết luận trung thực:** desktop đạt mọi ngưỡng; **mobile mô phỏng (4G chậm, CPU chậm 4×) chưa đạt LCP ≤ 2,5 s**. CLS, TBT (≈ INP),
Accessibility, SEO, Best Practices đều đạt. First Load JS trang chi tiết 290 KB (> 200 KB, đã ghi ở §4b).

**Đã sửa** (giữ lại vì đúng, dù không đổi được LCP mobile một cách đo được):
- Logo PNG 256×256 (92 KB, hiển thị 52 – 92 px) → WebP 128/256 px (16 – 48 KB). Logo header và logo lớn ở hero (ẩn trên mobile) bỏ `priority`,
  nên không còn được tải sẵn trước ảnh LCP.
- Ảnh LCP (thẻ đầu tiên, ảnh chính trang chi tiết) có `fetchPriority="high"`: `priority` của Next 15.5 chỉ tạo preload trong header `Link`,
  xếp sau 8 font. Thời gian chờ trước khi tải ảnh LCP giảm từ 0,84 – 1,37 s xuống 0,37 – 0,65 s.
- Playfair Display (font tiêu đề) không preload (`preload: false`); CLS vẫn 0 nhờ fallback đã chỉnh cỡ của `next/font`.

**Vì sao vẫn chậm trên mobile:** khi chạy thật không giới hạn mạng, trang vẽ ảnh LCP ở **265 ms**; Lighthouse mô phỏng 4G chậm và coi ảnh LCP
phụ thuộc mọi tệp được yêu cầu trước thời điểm đó — 13 – 15 tệp font (157 – 172 KB), 186 – 290 KB JavaScript và các ảnh thẻ khác. Hai phép thử:
- **Bỏ hẳn web font** (bản build thử, không giữ): LCP mobile 2,84 – 3,10 s → font chiếm khoảng 0,8 – 1,0 s, nhưng bỏ font vẫn chưa đạt.
- **Ở `localhost` ảnh không qua bộ tối ưu** (`lib/images.ts`), điện thoại nhận ảnh 800 px gốc; trên tên miền thật bộ tối ưu trả ảnh nhỏ hơn
  (thử: 27 KB → 13 KB). Chưa đo được trọn vẹn: dựng tên miền giả `host.docker.internal` qua HTTPS thì bộ tối ưu trong container từ chối
  chứng chỉ tự ký; tắt kiểm chứng chứng chỉ để đo bị chặn vì lý do an toàn. **Cần đo lại trên máy demo có tên miền và chứng chỉ thật.**

**Cập nhật 19/11/2026 — bộ tối ưu ảnh chạy ở mọi môi trường.** Ảnh của hệ thống đưa cho `next/image` dưới dạng đường dẫn tương đối
`/media/…`; bộ tối ưu đọc ảnh qua rewrite nội bộ `/media/*` → MinIO (`next.config.ts`, `lib/images.ts`), nên ở `localhost` cũng nhận
WebP đã thu nhỏ (ảnh thẻ đầu 27 KB → 13 KB). Hai cách đo:

| Trang | Mobile, `simulate` (mặc định) | Mobile, `devtools` (làm chậm thật: 1,6 Mbps, RTT 150 ms, CPU 4×) | Desktop |
|---|---|---|---|
| `/` | 4,12 s | **2,52 s** · CLS 0 · Perf 93 | 1,00 s |
| `/recipes` | 4,09 s | **2,49 s** · CLS **0,218** · Perf 83 | 0,95 s |
| `/recipes/{slug}` | 3,34 s | **2,52 s** · CLS 0 · Perf 92 | 0,84 s |

- Chế độ `simulate` ước lượng từ một lượt tải không giới hạn: máy chủ chạy ngay trên máy nên mọi tệp xong trong ~0,25 s và Lighthouse coi ảnh
  LCP phụ thuộc toàn bộ font + JavaScript → số liệu bi quan. Chế độ `devtools` làm chậm mạng và CPU thật: LCP ≈ 2,5 s, **đúng ngưỡng**.
- Thử bớt Be Vietnam Pro còn 2 độ đậm (font 157 → 112 KB): LCP không đổi trong nhiễu → **không** đổi thiết kế.
- **CLS 0,218 ở `/recipes` khi mạng chậm**: font Be Vietnam Pro tải muộn, chữ đổi kích thước làm khung lọc và lưới bị đẩy (Lighthouse chỉ
  đúng 2 tệp `woff2` là nguyên nhân). Chưa sửa trong lượt này — hướng sửa: `display: 'optional'` cho font thân bài.
- **React #418 (hydration)** xuất hiện ở 4/9 lượt `devtools` (trang chủ, trang chi tiết); không tái hiện được bằng Playwright (Edge, cùng
  user-agent Moto G, DPR 1,75, làm chậm mạng/CPU) hay bản `next dev`. React tự dựng lại phần lệch nên người dùng không thấy lỗi; ghi lại để theo dõi.

**Lưu ý về các lượt đo 18 – 19/11/2026:** image `culinaryblog-web` dùng để đo được build tay bằng Git Bash, nên `NEXT_PUBLIC_API_BASE_URL=/api/v1`
bị đổi thành đường dẫn Windows và mọi lời gọi API **từ trình duyệt** thất bại (phát hiện nhờ E2E ngày 19/11). Ba trang được đo render phía server
(`API_INTERNAL_BASE_URL` không bị ảnh hưởng) nên ảnh, chữ và LCP đúng; phần JavaScript chạy sau hydrate có thể ít việc hơn thực tế. Build lại
đúng (`MSYS_NO_PATHCONV=1`): E2E **49/49** trên stack production. Nên đo lại một lượt trên máy demo.

**Sửa CLS ở `/recipes` (19/11/2026):** font thân bài Be Vietnam Pro chuyển `display: 'optional'` (`app/layout.tsx`) — chỉ dùng font nếu về kịp
lần vẽ đầu (font được preload nên thường kịp), không kịp thì giữ chữ dự phòng đến hết lượt xem, không đổi font giữa chừng. Đo lại bằng
`devtools` (mạng chậm thật), image build đúng:

| Trang | LCP (3 lần) | CLS | TBT | Performance |
|---|---|---|---|---|
| `/` | 2,81 / 2,51 / 2,43 s (median 2,51) | **0** | 97 ms | 93 |
| `/recipes` | 2,44 / 2,48 / 2,47 s (median 2,47) | **0** (trước 0,218) | 79 ms | 94 (trước 83) |
| `/recipes/{slug}` | 8 lần: 2,49 – 2,88 s | **0** | 128 ms hoặc 390 – 452 ms | 81 – 92 |

Trang chi tiết cho hai nhóm kết quả: TBT ~128 ms (LCP ~2,5 s) hoặc ~400 ms (LCP ~2,75 s) — phần JavaScript tải lười sau hydrate (thư viện ảnh,
khẩu phần, in/chia sẻ, 290 KB) có lúc rơi vào cửa sổ đo. Các lượt trước ngày 19/11 cho TBT thấp hơn vì image khi đó không gọi được API từ trình
duyệt (xem lưu ý trên). Chưa đạt ổn định LCP ≤ 2,5 s và First Load JS ≤ 200 KB ở trang chi tiết — việc còn lại: tách nhỏ JavaScript trang chi tiết.

Hướng còn lại nếu cần đạt 2,5 s trên mobile: giảm bộ font (bớt độ đậm 500 hoặc 700 của Be Vietnam Pro, hoặc bỏ Playfair) — là thay đổi thiết kế,
cần nhóm quyết định; và đo lại khi bộ tối ưu ảnh hoạt động.

## 5. Cách chạy lại

```bash
cd frontend
npm run build && npm start &
npx lhci autorun                                   # mobile, 3 lần/URL, kiểm ngưỡng SRS
CHROME_PATH=/đường/dẫn/chrome npx lhci autorun     # nếu cần chỉ định trình duyệt
```
Báo cáo HTML/JSON ghi vào `frontend/.lighthouseci/` (đã `.gitignore`).
