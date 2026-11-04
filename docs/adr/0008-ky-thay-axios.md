# ADR-0008 — Lớp gọi API dùng `ky` (bọc `fetch`), không dùng `axios`

- **Trạng thái:** Đã chấp nhận (Buổi 5 cài gói — Lab 5; Buổi 6 thay lớp gọi API — Lab 6; Buổi 7 sửa khóa Data Cache)
- **Liên quan:** NFR-PERF-003, quy tắc vàng §2.3 của kế hoạch, MT-33.3, MT-67, X-10

## Bối cảnh

Lab 5 cho chọn `axios` hoặc `ky`. SRS không chỉ định thư viện. Next.js App Router gắn cơ chế cache của nó — Data Cache với `revalidate`, gom request
trùng trong một lần render — **vào hàm `fetch`**. Từ MT-67 (ADR-0010) Data Cache là tầng cache duy nhất của trang công khai.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. `ky` — lớp mỏng bọc `fetch` | Giữ nguyên option `next: { revalidate }` của Next.js; có hook `beforeRequest`/`afterResponse`/`beforeError`, retry tôn trọng `Retry-After` | Gói ESM thuần — Jest phải biên dịch riêng |
| B. `axios` | Quen thuộc, interceptor | Không đi qua `fetch` (trình duyệt: XHR, server: `http` của Node) → mất `revalidate` và gom request trùng; mỗi lượt xem trang gọi thẳng API |
| C. `fetch` tự bọc (như Buổi 2) | Không thêm gói | Tự viết lại retry, hook, timeout |

## Quyết định

**A.** `lib/api-client.ts` là instance `ky` dùng chung: gắn Bearer + `X-Correlation-ID`, refresh 401 single-flight, đổi lỗi sang `ApiError`, chỉ tự
thử lại `GET` (không bao giờ thử lại `POST`). API công khai của module (`apiFetch`, `ApiError`) giữ nguyên chữ ký để code các module khác không phải sửa.

## Hệ quả

- **Bài học Buổi 7 (đã đo):** `ky` truyền một đối tượng `Request` cho `fetch`, và Next.js 15.5 lấy **toàn bộ header** làm khóa Data Cache.
  `X-Correlation-ID` ngẫu nhiên khiến Data Cache không bao giờ trúng: 5 lượt tải `/categories` → API nhận **15** request dù `revalidate: 1800`; sau khi
  bỏ header này ở request phía server dùng Data Cache: **1** request. Chọn sai thư viện hay cấu hình sai ở đây đều là lỗi im lặng — trang vẫn
  hiển thị, chỉ có cache không hoạt động — nên phải có kiểm thử đo số request.
- Request phía server không cache chuyển tiếp `X-Forwarded-For` của trình duyệt để rate limit tính theo IP thật (Buổi 7).

## Bằng chứng

`frontend/src/lib/api-client.ts`, `ky-next-fetch.test.ts` (option `next` tới được `fetch`), `api-client.server.test.ts`; số đo 15 → 1 ở ghi chú Buổi 7 của
kế hoạch (commit `4bd5100`).
