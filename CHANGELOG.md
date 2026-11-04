# Changelog

Mọi thay đổi đáng kể của Culinary Blog được ghi ở đây. Định dạng theo [Keep a Changelog 1.1.0](https://keepachangelog.com/vi/1.1.0/), đánh số
phiên bản theo [Semantic Versioning 2.0.0](https://semver.org/lang/vi/) (NFR-MAINT-003).

Số phiên bản ở đây là phiên bản **phần mềm**, độc lập với số phiên bản tài liệu đặc tả (SRS v1.2.4). Trước `1.0.0` dự án chưa phát hành bản nào;
mỗi mục ghi buổi làm việc (B2 → B8) để truy ngược về `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md` và lịch sử git.

## [Unreleased] — sẽ phát hành thành [1.0.0]

**Điều kiện gắn tag `v1.0.0`** (kế hoạch §7 — Verification B8): cả 4 nhánh Buổi 8 đã vào `develop` theo thứ tự merge chuẩn, `develop` → `main`, kiểm
chứng cuối dự án xanh (39/39 FR, 48/48 endpoint, `dotnet test`, `npm test`, `npx playwright test`, 8 service healthy). Gắn tag trên commit merge vào
`main`: `git tag -a v1.0.0 -m "Culinary Blog 1.0.0"`, rồi đổi tiêu đề mục này thành `## [1.0.0] - YYYY-MM-DD`.

Các mục còn chờ trước khi phát hành: Lab 8 của Dev 1, Dev 2, Dev 3 (quản lý người dùng; biểu mẫu công thức + máy trạng thái; SEO + WCAG). Lab 7 của
cả 4 dev đã vào `develop` ngày 30/10/2026.

### Added

- **Log có cấu trúc (B8 — FR-OBS-002, CONS-010):** Serilog cho cả container `api` và `hangfire` — Console (JSON `CompactJsonFormatter`), File
  (`logs/culinaryblog-YYYYMMDD.json`, xoay vòng theo ngày, giữ 14 file), Seq (development); mỗi request một dòng log có method, path, status, thời gian
  xử lý, `UserId`, IP thật, `CorrelationId`, phiên bản API; 5xx → Error, > 500 ms → Warning, `/health*` → Verbose.
- **`CorrelationIdMiddleware` (B8 — SRS §5.2, §6.2):** nhận `X-Correlation-ID` khi gồm ≤ 64 ký tự `[A-Za-z0-9-]`, ngược lại sinh UUID mới; trả lại trong
  header và trong Problem Details (`correlationId`); mọi log của request mang `CorrelationId`, `RequestPath`, `UserId` (khi đã xác thực).
- **CorrelationId đi theo job Hangfire (B8):** `CorrelationIdJobFilter` lưu mã vào tham số job lúc enqueue, đặt lại vào log context lúc worker chạy.
- **Audit log (B8 — NFR-SEC-006):** `LoggingBehavior` ghi `AUDIT {Command} by {UserId} succeeded/failed with {ErrorCode}` cho mọi command, không
  bao giờ ghi payload; `SlowQueryInterceptor` cảnh báo câu SQL > 100 ms (không ghi giá trị tham số).
- **OpenTelemetry (B8 — FR-OBS-003, mức C):** trace ASP.NET Core, HttpClient, PostgreSQL (Npgsql); metric thời gian request, `recipes_created_total`,
  `recipes_published_total`; xuất OTLP/HTTP tới Seq ở development; log mang `TraceId`/`SpanId`.
- **Load test k6 (B8 — NFR-PERF-001/002):** `tests/load/{smoke,load,stress}.js` (70 % đọc / 20 % tìm kiếm / 10 % ghi), môi trường đo
  `docker-compose.loadtest.yml`, báo cáo `docs/load-test-report.md`, `docs/cache-report.md`.
- **Môi trường E2E giống production (B8):** `docker-compose.e2e.yml` — MailHog, access token 1 phút, dữ liệu mẫu, rate limit thật.
- **Healthcheck cho đủ 8 service (B8 — CONS-009):** thêm `nginx` (`/nginx-health`), `hangfire` (`/health/ready`), `seq` (`/health`).
- **10 ADR (B8 — NFR-MAINT-003):** `docs/adr/0001` → `0010`.
- **Lab 7 của nhóm (B7, vào `develop` 30/10/2026):** đổi mật khẩu, quên/đặt lại mật khẩu qua email (FR-AUTH-010/011 — CR-2026-05; 45 → 48 endpoint,
  mã lỗi thứ 30 `AUTH_PASSWORD_RESET_TOKEN_INVALID`), trang `/profile` với tab phiên đăng nhập (Dev 1); thư viện shadcn/ui, trang Tổng quan, trang chi
  tiết công thức (điều chỉnh khẩu phần, thư viện ảnh, in, chia sẻ, 404 thật cho bản nháp) và `/dashboard/recipes` qua `/recipes/mine` (Dev 2); trang chủ,
  `/categories`, `/categories/[slug]` với `revalidate` khớp TTL API — hoàn tất D-7 (Dev 3).
- **Production (B7 — NFR-SEC-005, NFR-SCALE-003):** `docker-compose.prod.yml` — Nginx HTTPS (TLS 1.2/1.3) + HSTS, ảnh qua `/media/` (D-14), 3 bản
  sao `api`, service `migrate` chạy một lần, `tools/deploy/rolling-update-api.sh` cập nhật không gián đoạn, khóa DataProtection mã hóa bằng chứng chỉ (D-17).
- **Trang `/search` (B7 — Lab 7):** SSR lần đầu + đổi bộ lọc phía client (`history.pushState`, một request JSON mỗi lần lọc), bộ lọc đa tiêu chí (danh
  mục, độ khó, thời gian nấu/chuẩn bị, khẩu phần — MT-66), phân trang offset.
- **Bộ lọc thời gian cho `GET /recipes/search` (B6 — CR-2026-05, MT-66):** `maxCookTime`, `maxPrepTime`, `minServings`.
- **Frontend nền (B5–B6 — Lab 5, Lab 6):** kiểu TypeScript theo miền + kiểm thử chống lệch mã lỗi với `ErrorCodes.cs`; lớp gọi API `ky` (Bearer, refresh
  401 single-flight, `X-Correlation-ID`, chỉ thử lại `GET`), bộ xử lý lỗi toàn cục, route group `(public)`/`(auth)`/`(dashboard)`; Zustand auth store;
  chặn route phía client; khung trang và component dùng chung (phân trang, skeleton, trạng thái rỗng, error boundary).
- **Bảo mật API (B5 — Lab 5):** CORS theo danh sách cho phép, rate limit theo IP thật (sliding 10/phút `/auth/*`, fixed 100/phút toàn cục, fixed 5/phút
  upload), API versioning theo đoạn URL (`Asp.Versioning`).
- **Tài liệu API (B5 — Lab 5):** OpenAPI sinh từ code + Scalar `/scalar/v1` cho đủ 45 endpoint, ảnh chụp hợp đồng `docs/api/openapi.v1.json`.
- **Giao diện làm sớm (B5):** biểu mẫu soạn/sửa/xuất bản công thức; trang tìm kiếm + bộ lọc đồng bộ URL; composite index chứng minh bằng `EXPLAIN ANALYZE`
  trên ≥ 10.000 bản ghi (`docs/explain-analyze-b5.md`); FR-OBS-001 nối vào healthcheck Docker, failover Nginx và chỉ báo trên giao diện.
- **Hoàn thành toàn bộ API (B4):** refresh token xoay vòng + phát hiện dùng lại, hồ sơ, quản lý tài khoản [Admin], quản lý phiên (FR-AUTH-004, 006 → 009);
  nguyên liệu, bước (đánh số do server, đổi chỗ), cập nhật có `RowVersion` (FR-RCP-009, 010, 004); tìm kiếm toàn văn, lọc/sắp xếp/phân trang,
  `/recipes/mine`, sitemap (FR-SRCH-001 → 004, FR-RCP-011); vòng đời xuất bản/hủy/lưu trữ/khôi phục/xóa mềm với máy trạng thái khép kín (FR-RCP-005,
  006, 007); job đổi kích thước ảnh và job dọn dữ liệu vĩnh viễn (FR-JOB-002, 003); `ApiSurfaceTests` khóa 45/45 endpoint.
- **Nền tảng (B3):** domain exception, Repository + Unit of Work, `GlobalExceptionMiddleware` (RFC 7807), ba endpoint health check, dashboard Hangfire sau
  Basic Auth của Nginx, bộ integration test trên Testcontainers; đăng nhập Google (ID Token flow), đăng xuất; tạo bản nháp + quản lý ảnh; CRUD danh mục [Admin].
- **Lát cắt đầu tiên (B2):** solution Clean Architecture + Next.js, Docker Compose 8 service + MailHog, đăng ký/đăng nhập (FR-AUTH-001/002), danh sách và
  chi tiết công thức (FR-RCP-001/002), danh mục công khai + Redis (FR-CAT-001/002), tải lên/xóa tệp MinIO (FR-FILE-001/002), email chào mừng qua Hangfire
  (FR-JOB-001); dữ liệu mẫu 20 danh mục, 100 công thức thật (CR-2026-03).

### Changed

- **Xóa cache theo tiền tố bằng `SCAN` (B8 — audit cache):** khóa kết thúc bằng `:` được xóa cả họ; danh sách công khai phản ánh thay đổi ngay thay vì chờ
  hết TTL 2 phút. Bộ khóa cần xóa khớp cột "Invalidate khi" của NFR-PERF-003: thêm `categories:detail:` khi nội dung công thức đổi, `categories:all` khi
  công thức đổi danh mục; bỏ `search:` (SRS: hết hạn tự nhiên).
- **Image ghim đúng phiên bản (B8):** `nginx:1.31.6-alpine` (dựng thành `culinaryblog-nginx` kèm bản vá Alpine), `postgres:16.15-alpine`, `redis:7.4.11-alpine`, `datalust/seq:2026.1.17114`,
  `mailhog/mailhog:v1.0.1`, `mcr.microsoft.com/dotnet/sdk:10.0.402`, `aspnet:10.0.13`, `node:22.23.2-alpine` (trước đó `latest`/`alpine`/`10.0`).
- **Seq nhận log ở cổng ingest `seq:5341`** đúng SRS §5.3 (trước đó `seq:80`).
- Mọi trang HTML render theo request vì CSP có nonce; độ tươi dữ liệu đặt trên từng `fetch` (B6–B7 — MT-67, ADR-0010).
- Ảnh phục vụ qua `<site>/media/` thay cho `localhost:9000` (B7 — D-14); script `tools/media-url-migration/` đổi URL đã lưu.
- Rate limit `/auth/*` và upload được đếm thêm ở Nginx cho cả cụm khi chạy nhiều bản sao (B7, ADR-0009).
- **`/search` dùng `Sheet`, `Select`, `Badge` thật của shadcn/ui (B8):** gỡ bản tạm `components/shared/interim-ui.tsx` của Buổi 7. Bộ lọc danh mục vẫn
  chạy khi tắt JavaScript (`<select>` gốc trong HTML do server render, `Select` của shadcn nạp muộn sau khi hydrate — First Load JS `/search` 186 kB).

### Removed

- Output Cache của .NET (B4 — D-6); gói `next-auth` không dùng (B5); mã lỗi `AUTH_USERNAME_EXISTS` (B3 — D-1).

### Fixed

- **Redis tắt làm mỗi request chậm ~5,6 giây (B8 — NFR-REL-002, đo dưới tải):** mỗi lệnh GET/SET tới Redis đã tắt chờ hết timeout 2 giây mới thành
  cache-miss. `RedisCacheService` nay ngắt mạch: bỏ qua Redis ngay khi mất kết nối và 10 giây sau một lần lỗi (lệnh xóa cache không bị ngắt theo thời
  gian). k6 100 người dùng, tắt Redis 60 giây: p99 5,73 s → 24 ms, 0 lỗi.
- **CSP chặn `<style>` của thư viện ở production (B8):** toast Sonner mất CSS trên mọi trang; `Select`/`Sheet`/`Dialog` của shadcn bị chặn style khi mở.
  `style-src` production nay có nonce của request + hash cố định cho CSS của Sonner — vẫn không có `'unsafe-inline'`; đo trên Edge: 0 vi phạm.
- **PostgreSQL từ chối kết nối dưới tải cao (B8 — stress test):** ở 1.000 người dùng đồng thời, 3 bản sao `api` + `hangfire` (pool Npgsql
  mặc định 100 mỗi tiến trình) vượt `max_connections = 100` → 27.429 lần "too many clients", p99 6 s, 277 lỗi. Nay pool tối đa 25 mỗi tiến
  trình (`DB_MAX_POOL_SIZE`) và PostgreSQL production `max_connections = 200`; đo lại: 516.677 request, 0 lỗi, p95 64 ms.
- Request logging của Serilog ghi qua logger của chính host (không qua `Log.Logger` tĩnh) — nhiều host trong cùng tiến trình không chiếm log
  của nhau (B8).
- Build đỏ ở mọi nhánh vì advisory mới của ImageSharp GHSA-v76p-62qx-wwq2 (decoder TIFF) — miễn trừ kèm bằng chứng job chỉ giải mã JPEG/PNG/WebP
  (B8, cùng commit trên `develop`).
- `ImageResizeJob` xóa cache công khai của công thức sau khi có thumbnail — trước đó trang chi tiết/danh sách giữ bản không có thumbnail tới hết TTL (B8).
- Khóa Data Cache của Next.js ổn định (không mang `X-Correlation-ID` ở request phía server dùng cache): 5 lượt tải `/categories` từ 15 xuống 1 request tới API (B7).
- Bản sao lưu `pg_dump` khôi phục được — `unaccent_immutable` gọi `unaccent` kèm schema (B7, migration `B7_Search_UnaccentSchemaQualified`).
- Kiểm thử ảnh chụp OpenAPI so sánh với xuống dòng đã chuẩn hóa — không còn đỏ trên bản clone Windows (B7).
- Lỗi kiểm tra hợp lệ trả 400 thay cho 422 ở mọi endpoint (B3 — D-11, ADR-0002); Nginx phân giải tên `api` động, hết 502 sau khi tạo lại container
  (B2 — MT-50, ADR-0006); tải ảnh gần 5 MB qua Nginx không bị 413 (B2).

### Security

- **Quét Trivy mọi image (B8):** vá các lỗ hổng Critical có bản vá — `next` 15.5.25 → 15.5.28 (GHSA-2464-fhr4-w9xw: thực thi mã từ xa qua Image
  Optimization API với ảnh SVG), gỡ npm khỏi image chạy của frontend (CVE-2026-59873, `tar`), `nginx` 1.31.5 → 1.31.6 + vá `brotli-libs` trong `nginx/image/Dockerfile` (CVE-2026-111497).
  Phần chưa có bản vá hoặc chỉ dùng ở dev được ghi nhận rủi ro trong `docs/load-test-report.md` §7.
- **Không ghi bí mật vào log (B8 — NFR-SEC-007):** che `password`, `currentPassword`, `newPassword`, `refreshToken`, `accessToken`, `idToken`, `token`,
  `authorization` ở mọi sink; `LogRedactionTests` gọi thật đăng ký, đăng nhập (đúng/sai), refresh, Google, đăng xuất, đổi mật khẩu, quên mật khẩu (chạy cả
  job gửi email) và đặt lại mật khẩu, khẳng định không log nào chứa mật khẩu, token hay link đặt lại mật khẩu.
- CorrelationId nhận từ client được kiểm định dạng — chặn làm ngập log và chèn dòng log giả (B8).
- Vá rò rỉ bản nháp qua cache công khai (B4 — MT-34, D-4, D-5, ADR-0004); access token chỉ trong bộ nhớ (B4 — D-12); refresh token 256-bit, chỉ lưu hash,
  xoay vòng + thu hồi cả họ khi bị dùng lại (B4); CSP có nonce + `'strict-dynamic'` (B6); HTTPS + HSTS, khóa DataProtection mã hóa bằng chứng chỉ (B7 — D-17).
