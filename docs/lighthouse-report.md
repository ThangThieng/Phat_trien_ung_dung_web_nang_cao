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

## 5. Cách chạy lại

```bash
cd frontend
npm run build && npm start &
npx lhci autorun                                   # mobile, 3 lần/URL, kiểm ngưỡng SRS
CHROME_PATH=/đường/dẫn/chrome npx lhci autorun     # nếu cần chỉ định trình duyệt
```
Báo cáo HTML/JSON ghi vào `frontend/.lighthouseci/` (đã `.gitignore`).
