# ADR-0010 — CSP có nonce ⇒ mọi trang HTML render theo request; độ tươi dữ liệu đặt ở Data Cache

- **Trạng thái:** Đã chấp nhận (SRS v1.2.3 — CR-2026-05, MT-67; hiện thực Buổi 6 — Dev 1; các trang Buổi 7)
- **Liên quan:** NFR-SEC-005, NFR-SEC-002, NFR-PERF-003/005, SRS §5.1, O-7 (đánh đổi còn mở)

## Bối cảnh

NFR-SEC-005 yêu cầu `script-src 'self'` **+ nonce**; SRS §5.1 (trước v1.2.3) yêu cầu **ISR** cho `/` (120 s), `/recipes/[slug]` (300 s), `/categories`
(1800 s). Nonce phải mới cho mỗi lượt xem; Next.js chỉ gắn được nonce khi trang **render theo request**, còn trang ISR được dựng sẵn trước khi có request.
Refresh token nằm trong `localStorage` (ADR-0007), nên CSP là lớp chặn XSS bảo vệ nó.

## Thực nghiệm (12/10/2026 — Next.js 15.5.25, `next build && next start`, Edge headless qua Playwright)

| Phương án | Kiểu route | Script inline | Vi phạm CSP | Hydrate | Data Cache |
|---|---|---|---|---|---|
| A. CSP hash qua `experimental.sri` + `script-src 'self'` | tĩnh/ISR | 6 thẻ dữ liệu RSC **không** có `integrity` (SRI chỉ gắn cho file script ngoài) | **6** | **Không** | — |
| B. nonce + `'strict-dynamic'` sinh ở `middleware.ts`, root layout đọc `headers()` | động | 4 thẻ, cả 4 mang nonce | **0** | **Có** | **Giữ**: tải trang 3 lần, API không bị gọi thêm |
| C. `'unsafe-inline'`, giữ ISR | tĩnh/ISR | — | 0 | Có | — |

## Quyết định

**B.** Mọi trang HTML render theo request; giá trị `revalidate` (≤ TTL cache API — NFR-PERF-003) đặt trên **từng lời gọi `fetch`** (Data Cache), không đặt
`export const revalidate` ở trang. `sitemap.xml`, `robots.txt` không chứa script nên vẫn sinh định kỳ. Phương án A không chạy được; phương án C cho mọi
script inline chạy — kể cả script kẻ tấn công chèn vào — tức là mất đúng tác dụng của CSP.

## Hệ quả

- Container `frontend` tốn CPU ở mỗi lượt xem; HTML trả `Cache-Control: private, no-store` nên Nginx/CDN không cache được trang (O-7). Đổi lại **không tốn
  thêm lượt gọi API** — với điều kiện khóa Data Cache ổn định (bài học Buổi 7 ở ADR-0008: header ngẫu nhiên trong request làm Data Cache không bao giờ trúng).
- Không trang nào được khai báo `export const revalidate`/`generateStaticParams` có dữ liệu: root layout đọc nonce nên trang ISR trả 500
  `DYNAMIC_SERVER_USAGE` (gặp khi merge Buổi 6).
- Xét lại khi Next.js có Partial Prerendering tương thích nonce, hoặc khi refresh token chuyển sang cookie `HttpOnly` qua một CR riêng (O-2).

## Bằng chứng

`frontend/src/middleware.ts`, `frontend/e2e/csp.spec.ts` (0 vi phạm CSP trên mọi route, hai lần tải có nonce khác nhau); kiểm chứng Buổi 6: tải trang chủ
nhiều lần trong 2 phút không sinh request mới tới API.
