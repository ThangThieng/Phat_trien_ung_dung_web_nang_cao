# ADR-0009 — `Asp.Versioning` theo đoạn URL; rate limit theo thuật toán của SRS, đếm thêm ở Nginx khi chạy nhiều bản sao

- **Trạng thái:** Đã chấp nhận (Buổi 5 — Lab 5, Dev 1; lớp Nginx: Buổi 7 — Dev 4, trưởng nhóm chọn 21/10/2026)
- **Liên quan:** NFR-SEC-003, SRS §5.2 (Versioning, Rate Limiting), X-6 (kế hoạch §3), MT-68

## Bối cảnh

- **Versioning:** SRS §5.2 dùng tiền tố `/api/v1/…`; Lab 5 yêu cầu API versioning thật (không chỉ chuỗi cố định trong route).
- **Rate limit:** Lab 5 ghi *"`FixedWindowLimiter` cho Auth, `SlidingWindow` cho API"*; NFR-SEC-003 quy định **ngược lại** — sliding window
  10/phút cho `/auth/*`, fixed window 100/phút toàn cục, fixed 5/phút cho tải ảnh lên.
- Buổi 7 chạy 3 bản sao `api`: middleware rate limit của ASP.NET Core đếm **trong bộ nhớ từng bản sao**.

## Phương án

| Vấn đề | Phương án | Được | Mất |
|---|---|---|---|
| Versioning | **A. `Asp.Versioning` theo đoạn URL** (`/api/v{version:apiVersion}`) | URL thực tế không đổi; header `api-supported-versions`; phiên bản không hỗ trợ → 400 theo hợp đồng lỗi | Thêm gói; `ApiSurfaceTests` phải chuẩn hóa route |
| | B. Header/query string | URL gọn | Khó cache/khó đọc log; khác SRS |
| Thuật toán | **A. Theo SRS** (sliding cho auth) | Đúng SRS; vẫn dùng đủ hai loại limiter Lab yêu cầu | Khác chữ của Lab |
| | B. Theo Lab (fixed cho auth) | Đúng chữ Lab | Fixed window cho phép dồn gấp đôi lượt thử mật khẩu quanh ranh giới cửa sổ |
| Nhiều bản sao | **A. Thêm lớp đếm ở Nginx** (điểm vào duy nhất) | Đúng hạn mức cho cả cụm | Hai nơi cấu hình hạn mức |
| | B. Rate limit dùng Redis | Một nơi đếm | Thêm một round-trip Redis cho mọi request; Redis sập thì chọn mở hay chặn |

## Quyết định

- **Versioning A.** `/api/v1/categories` trả `api-supported-versions: 1.0`; `/api/v2/…` → 400 `VALIDATION_ERROR`.
- **Thuật toán theo SRS** (X-6): sliding 10/phút `/auth/*`, fixed 100/phút toàn cục, fixed 5/phút upload; theo **IP thật** sau
  `UseForwardedHeaders` với `KnownIPNetworks` là dải mạng Docker (không tin `X-Forwarded-For` từ ngoài). 429 theo hợp đồng `RATE_LIMIT_EXCEEDED` +
  `Retry-After` + `X-RateLimit-*` (expose qua CORS — MT-68).
- **Nhiều bản sao:** Nginx đếm thêm 10/phút cho `/api/v*/auth/*` (regex không phân biệt hoa/thường — định tuyến ASP.NET cũng vậy) và 5/phút cho
  `POST` tải ảnh, trả cùng hợp đồng 429; middleware của API giữ nguyên làm lớp thứ hai.

## Hệ quả

- Đo Buổi 7 với 3 bản sao: chỉ middleware → đăng nhập sai tới lần **23** mới gặp 429 (27/40 lần lọt); thêm Nginx → 429 ở lần **12** (15/40 lọt =
  10 lượt + 1 lượt mỗi 6 giây).
- Đo tải (Buổi 8) phải nâng hạn mức ở **cả hai lớp** vì k6 chạy từ một IP: `docker-compose.loadtest.yml` thay snippet vùng rate limit của Nginx và đặt
  `RateLimiting__*__PermitLimit` — môi trường E2E/bảo mật giữ hạn mức thật.

## Bằng chứng

`API/Extensions/{ApiVersioningExtensions,SecurityExtensions}.cs`, `nginx/templates/snippets/rate-limit-zones.conf.template`, kiểm thử `RateLimitingTests`,
`ApiVersioningTests`, `SecurityMiddlewareTests` (giả mạo `X-Forwarded-For` không né được hạn mức).
