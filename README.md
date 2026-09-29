# Culinary Blog – Nhóm 20

Blog ẩm thực & nấu ăn: người dùng đăng ký, viết công thức (nguyên liệu, các bước, ảnh), xuất bản, tìm kiếm tiếng Việt không dấu; Admin quản trị danh mục và tài khoản.
**Công nghệ:** .NET 10 Minimal API (Clean Architecture + CQRS/MediatR) · Next.js 15 App Router · PostgreSQL 16 · Redis 7 · MinIO · Hangfire · Nginx · Docker Compose.
**Tài liệu gốc:** [`SPEC/SRS_Culinary_Blog_v1.2.2.md`](SPEC/SRS_Culinary_Blog_v1.2.2.md) (yêu cầu) · [`SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`](SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md) (kế hoạch đầy đủ) · [`SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`](SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md) (58 mâu thuẫn đã giải quyết).

## Thành viên

| Vai trò | Thành viên | MSSV | Phụ trách | Nhánh Git (buổi n) |
|---|---|---|---|---|
| Dev 1 | **Hoàng Bình Quân** | 2314236 | Xác thực, phân quyền, quản lý người dùng | `2314236_HoangBinhQuan_buoiso{n}` |
| Dev 2 | **Nguyễn Hồng Phúc Thọ** | 2312758 | Công thức nấu ăn (lõi nghiệp vụ) | `2312758_NguyenHongPhucTho_buoiso{n}` |
| Dev 3 | **Đoàn Hồng Tiến** | 2314291 | Danh mục, tìm kiếm, SEO | `2314291_DoanHongTien_buoiso{n}` |
| Dev 4 | **Nguyễn Thăng Thiêng** (trưởng nhóm) | 2312755 | Tệp tin, job nền, quan sát hệ thống, hạ tầng | `2312755_NguyenThangThieng_buoiso{n}` |

## Mục lục

- [Phần 1 – Toàn bộ đầu việc xác định được sau khi phân tích yêu cầu](#phần-1--toàn-bộ-đầu-việc-xác-định-được-sau-khi-phân-tích-yêu-cầu)
- [Phần 2 – Chia việc tổng thể](#phần-2--chia-việc-tổng-thể)
- [Phần 3 – Chia việc chi tiết từng buổi](#phần-3--chia-việc-chi-tiết-từng-buổi)
- [Phụ lục – Cài đặt & chạy dự án](#phụ-lục--cài-đặt--chạy-dự-án)

---

# Phần 1 – Toàn bộ đầu việc xác định được sau khi phân tích yêu cầu

## 1.1 Tổng quan con số

| Hạng mục | Số lượng | Ghi chú |
|---|---|---|
| Yêu cầu chức năng (FR) | **37** | 7 nhóm: AUTH 9 · CAT 5 · RCP 11 · SRCH 4 · FILE 2 · JOB 3 · OBS 3 |
| Yêu cầu phi chức năng (NFR) | **30** | Hiệu năng 5 · Bảo mật 7 · Khả dụng 4 · Tin cậy 3 · Bảo trì 4 · Mở rộng 3 · SEO 4 |
| Ràng buộc thiết kế (CONS) | **10** | Kiến trúc, pattern, framework, bảo mật, API, DB, upload, validation, container, logging |
| Endpoint REST | **45** | SRS Chương 8 (v1.2.2 thêm `GET /recipes/mine/{id}` — CR-2026-04); **hoàn thành toàn bộ ở Buổi 4** |
| Mã lỗi ứng dụng | **29** | SRS Phụ lục B – mỗi mã phải có ít nhất 1 test (v1.2.2 thêm `AUTH_USER_NOT_FOUND`, `CONCURRENCY_CONFLICT`) |
| Mâu thuẫn trong SRS đã giải quyết | **63** | MT-01 → MT-63 (CR-2026, CR-2026-02, CR-2026-03 và CR-2026-04) |
| Dữ liệu mẫu (SRS §2.6.1, CR-2026-03) | **≥ 20 / ≥ 100** | ≥ 20 danh mục, ≥ 100 công thức; mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước chế biến |

Mức ưu tiên (MoSCoW): **M** = bắt buộc · **S** = nên có · **C** = có thì tốt.

## 1.2 Yêu cầu chức năng (37 FR)

**Xác thực & Quản lý người dùng – FR-AUTH (9)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-AUTH-001 | Đăng ký tài khoản (tự đăng nhập ngay, gửi email chào mừng) | M |
| FR-AUTH-002 | Đăng nhập email/mật khẩu (khóa 15 phút sau 5 lần sai) | M |
| FR-AUTH-003 | Đăng nhập / đăng ký bằng Google (ID Token) | M |
| FR-AUTH-004 | Làm mới access token (rotation + phát hiện dùng lại token) | M |
| FR-AUTH-005 | Đăng xuất, thu hồi refresh token | M |
| FR-AUTH-006 | Xem hồ sơ cá nhân (`/auth/me`) | M |
| FR-AUTH-007 | Cập nhật hồ sơ & avatar | M |
| FR-AUTH-008 | [Admin] Danh sách người dùng, khóa / mở khóa tài khoản | S |
| FR-AUTH-009 | Quản lý phiên đăng nhập của chính mình (thiết bị, thu hồi) | C |

**Danh mục – FR-CAT (5)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-CAT-001 | Xem danh sách danh mục (cache Redis) | M |
| FR-CAT-002 | Xem chi tiết danh mục kèm công thức đã xuất bản | M |
| FR-CAT-003 | [Admin] Tạo danh mục (slug tự sinh, tên trùng → 409) | M |
| FR-CAT-004 | [Admin] Cập nhật danh mục (slug giữ nguyên) | M |
| FR-CAT-005 | [Admin] Xóa mềm danh mục (còn công thức → 409) | M |

**Công thức nấu ăn – FR-RCP (11)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-RCP-001 | Danh sách công thức (chỉ Published, phân trang/lọc/sắp xếp) | M |
| FR-RCP-002 | Chi tiết công thức theo slug | M |
| FR-RCP-003 | Tạo công thức mới (luôn là Draft) | M |
| FR-RCP-004 | Cập nhật công thức (chống ghi đè đồng thời bằng RowVersion) | M |
| FR-RCP-005 | Xuất bản / hủy xuất bản | M |
| FR-RCP-006 | Lưu trữ / khôi phục (Archive / Unarchive) | M |
| FR-RCP-007 | Xóa mềm công thức | M |
| FR-RCP-008 | Quản lý ảnh công thức (upload, ảnh chính, alt text, thứ tự, xóa) | M |
| FR-RCP-009 | Quản lý nguyên liệu (định lượng số + định lượng nguyên văn) | M |
| FR-RCP-010 | Quản lý các bước nấu (server tự đánh số, kéo-thả sắp xếp) | M |
| FR-RCP-011 | Danh sách công thức của tôi (`/recipes/mine`, cấm cache) | M |

**Tìm kiếm – FR-SRCH (4)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-SRCH-001 | Tìm kiếm toàn văn tiếng Việt không dấu ("pho" → "Phở") | M |
| FR-SRCH-002 | Lọc đa tiêu chí (danh mục, độ khó, thời gian nấu/chuẩn bị, khẩu phần) | M |
| FR-SRCH-003 | Sắp xếp `sortBy` + `sortOrder` (whitelist 5 trường) | M |
| FR-SRCH-004 | Phân trang offset (mặc định 12, tối đa 50) | M |

**Tệp tin – FR-FILE (2) · Job nền – FR-JOB (3) · Quan sát – FR-OBS (3)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-FILE-001 | Upload ảnh lên MinIO (≤ 5MB, JPEG/PNG/WebP/AVIF, kiểm tra magic bytes) | M |
| FR-FILE-002 | Xóa ảnh khỏi MinIO (idempotent, chỉ chủ file hoặc Admin) | M |
| FR-JOB-001 | Job gửi email chào mừng (Hangfire, retry 1′/5′/30′) | S |
| FR-JOB-002 | Job resize ảnh: medium 800×600, thumbnail 300×300 | S |
| FR-JOB-003 | Job dọn vĩnh viễn dữ liệu đã xóa mềm quá 30 ngày (03:30 UTC) | S |
| FR-OBS-001 | Health check `/health`, `/health/live`, `/health/ready` | M |
| FR-OBS-002 | Log có cấu trúc (Serilog + CorrelationId) | M |
| FR-OBS-003 | Tracing & metrics (OpenTelemetry) | C |

## 1.3 Yêu cầu phi chức năng (30 NFR)

| Nhóm | Mã | Chỉ tiêu cần đạt |
|---|---|---|
| **Hiệu năng** | PERF-001 | GET có cache: p50 ≤ 150ms · p95 ≤ 500ms · p99 ≤ 1000ms |
| | PERF-002 | ≥ 100 người dùng đồng thời; thêm instance thì thông lượng tăng tuyến tính |
| | PERF-003 | Redis cache-aside là cơ chế cache duy nhất; hit rate ≥ 80%; theo bảng TTL chuẩn |
| | PERF-004 | Không N+1; cảnh báo query > 100ms; index chứng minh bằng `EXPLAIN ANALYZE` |
| | PERF-005 | LCP ≤ 2.5s · CLS ≤ 0.1 · INP ≤ 200ms · First Load JS ≤ 200KB |
| **Bảo mật** | SEC-001 | Mật khẩu PBKDF2 qua Identity; ≥ 8 ký tự hoa/thường/số/đặc biệt; khóa sau 5 lần sai |
| | SEC-002 | JWT HS256 15 phút; refresh token 256-bit, chỉ lưu SHA-256; access token chỉ trong bộ nhớ |
| | SEC-003 | Rate limit theo IP thật: auth 10/phút, upload 5/phút, chung 100/phút |
| | SEC-004 | Validate mọi input ở tầng Application; upload kiểm tra kích thước + magic bytes |
| | SEC-005 | HTTPS TLS 1.2+, HSTS, CORS whitelist, CSP |
| | SEC-006 | Phân quyền ở tầng Application; không cache dữ liệu phụ thuộc danh tính |
| | SEC-007 | Không commit secret; không ghi mật khẩu/token vào log |
| **Khả dụng** | USE-001 | Responsive 320 / 768 / 1200px |
| | USE-002 | WCAG 2.1 AA |
| | USE-003 | Lỗi RFC 7807, hiện lỗi từng ô trên form |
| | USE-004 | Skeleton, toast, thanh tiến trình % khi upload |
| **Tin cậy** | REL-001 | Uptime ≥ 99.5%, có health check |
| | REL-002 | Redis chết → đọc thẳng DB; MinIO chết → chỉ upload lỗi 503 |
| | REL-003 | Xóa mềm, giữ 30 ngày; dữ liệu bền qua restart |
| **Bảo trì** | MAINT-001 | Build 0 warning (StyleCop, SonarAnalyzer, ESLint) |
| | MAINT-002 | Unit ≥ 80% tầng Application; mọi endpoint có test đúng + test lỗi; 5 luồng E2E |
| | MAINT-003 | README chạy được < 5 phút, ADR, CHANGELOG, tài liệu API (Scalar) |
| | MAINT-004 | Tuân thủ Clean Architecture (kiểm bằng ArchUnit) |
| **Mở rộng** | SCALE-001 | API stateless, state dùng chung qua Redis, khóa phân tán cho job |
| | SCALE-002 | Connection pooling, index đúng hình dạng truy vấn |
| | SCALE-003 | `--scale api=3` sau Nginx |
| **SEO** | SEO-001 | JSON-LD Schema.org Recipe, pass Google Rich Results Test |
| | SEO-002 | `<title>` ≤ 60 ký tự, meta description 150–160, Open Graph, `noindex` trang riêng tư |
| | SEO-003 | `sitemap.xml` + `robots.txt` do Next.js sinh |
| | SEO-004 | URL thân thiện `/recipes/{slug}`, danh sách slug dành riêng |

## 1.4 Ràng buộc thiết kế (10 CONS)

| Mã | Ràng buộc |
|---|---|
| CONS-001 | Clean Architecture 4 tầng: Domain · Application · Infrastructure · API (Domain không dùng NuGet) |
| CONS-002 | CQRS + MediatR; pipeline 4 behavior: Logging → Validation → Caching → CacheInvalidation |
| CONS-003 | .NET 10 Minimal API (không Controller); Next.js App Router (không Pages Router) |
| CONS-004 | JWT stateless (15 phút / 7 ngày); mật khẩu qua ASP.NET Core Identity |
| CONS-005 | REST, tiền tố `/api/v1`, lỗi RFC 7807 với `type` = mã lỗi ứng dụng |
| CONS-006 | PostgreSQL duy nhất; migration EF Core Code-First (DDL đặc thù Postgres được phép trong migration) |
| CONS-007 | Upload ≤ 5MB, 4 định dạng ảnh, kiểm tra magic bytes |
| CONS-008 | Validation bằng FluentValidation qua pipeline, không validate trong endpoint |
| CONS-009 | Docker multi-stage, chạy user non-root |
| CONS-010 | Serilog; mọi dòng log có CorrelationId, RequestPath, UserId |

## 1.5 Hạ tầng & việc kỹ thuật chung (không gắn với một FR)

- Dữ liệu mẫu theo SRS §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức có nội dung thật, 5 tác giả; seeder idempotent, tự bù cho database đang có.
- Solution 4 tầng + 4 project test, `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion), `AuditInterceptor`, `GlobalExceptionMiddleware`, `PagedResult<T>`.
- Docker Compose 8 service (`nginx, api, hangfire, frontend, postgres, redis, minio, seq`) + `mailhog` cho dev; bản production `docker-compose.prod.yml` (scale `api`×3).
- Nginx: reverse proxy, rate limit vòng ngoài, resolver DNS động, `/hangfire` có Basic Auth, `/media/` phục vụ ảnh, SSL/HSTS.
- Frontend dùng chung: `api-client` (Bearer, tự refresh 401, đọc ProblemDetails), TanStack Query, React Hook Form + Zod, `ImageUploader`.
- Bộ integration test dùng chung (WebApplicationFactory + Testcontainers), E2E Playwright, load test k6, Lighthouse, axe.
- Quy trình: gitleaks pre-commit, Definition of Done mỗi commit, thứ tự merge migration, 6 ADR, CHANGELOG, tag `v1.0.0`.

## 1.6 45 endpoint theo module

| Module | Endpoint |
|---|---|
| Auth (12) | `POST /auth/register` · `POST /auth/login` · `POST /auth/google` · `POST /auth/refresh` · `POST /auth/logout` · `GET /auth/me` · `PATCH /auth/me` · `GET /users` · `PATCH /users/{id}/status` · `GET /auth/sessions` · `DELETE /auth/sessions/{id}` · `POST /auth/sessions/revoke-all` |
| Categories (5) | `GET /categories` · `GET /categories/{slug}` · `POST /categories` · `PUT /categories/{id}` · `DELETE /categories/{id}` |
| Recipes (13) | `GET /recipes` · `GET /recipes/mine` · `GET /recipes/mine/{id}` · `GET /recipes/search` · `GET /recipes/sitemap` · `GET /recipes/{slug}` · `POST /recipes` · `PUT /recipes/{id}` · `PATCH /recipes/{id}/publish` · `…/unpublish` · `…/archive` · `…/unarchive` · `DELETE /recipes/{id}` |
| Ảnh công thức (3) | `POST /recipes/{id}/images` · `PATCH /recipes/{id}/images/{imageId}` · `DELETE /recipes/{id}/images/{imageId}` |
| Các bước (4) | `POST /recipes/{id}/steps` · `PUT …/steps/{stepId}` · `PATCH …/steps/reorder` · `DELETE …/steps/{stepId}` |
| Nguyên liệu (3) | `POST /recipes/{id}/ingredients` · `PUT …/ingredients/{ingId}` · `DELETE …/ingredients/{ingId}` |
| Files (2) | `POST /files/upload` · `DELETE /files/{**key}` |
| Health (3) | `GET /health` · `GET /health/live` · `GET /health/ready` |

---

# Phần 2 – Chia việc tổng thể

**Nguyên tắc:** Buổi 1 cả nhóm đọc đặc tả và tìm hiểu tính năng; từ Buổi 2 mỗi người giữ **một mảng cố định suốt 7 buổi còn lại** và làm **trọn từ database → API → giao diện** cho mảng đó. Cuối mỗi buổi hệ thống phải chạy được.

| Thành viên | Làm gì trên trang web | FR |
|---|---|---|
| **Dev 1 – Hoàng Bình Quân** | Mọi thứ về tài khoản: đăng ký, đăng nhập (email + Google), giữ phiên đăng nhập, hồ sơ cá nhân, Admin khóa tài khoản, bảo mật chung | FR-AUTH-001 → 009 |
| **Dev 2 – Nguyễn Hồng Phúc Thọ** | Mọi thứ về công thức: xem, viết, sửa, ảnh, nguyên liệu, các bước, xuất bản, lưu trữ, xóa, "công thức của tôi" | FR-RCP-001 → 011 |
| **Dev 3 – Đoàn Hồng Tiến** | Danh mục, tìm kiếm – lọc – sắp xếp – phân trang, trang chủ, SEO, sitemap, trải nghiệm & chuẩn truy cập | FR-CAT-001 → 005, FR-SRCH-001 → 004 |
| **Dev 4 – Nguyễn Thăng Thiêng** | Upload/xóa ảnh, job nền, health check, log & tracing, Docker, Nginx, cache, hạ tầng test, đóng gói | FR-FILE, FR-JOB, FR-OBS, DevOps |

> **Ngoại lệ ở Buổi 4 (29/09/2026):** để hoàn thành toàn bộ API trong một buổi mà vẫn chia tải đều, **API** xuất bản / lưu trữ / xóa mềm công thức do Thiêng làm, **API** "công thức của tôi" và sitemap do Tiến làm; **giao diện** của các chức năng này vẫn do Thọ làm (sitemap do Tiến).

**Lịch 8 buổi (tóm tắt):**

| Buổi | Dev 1 – Quân | Dev 2 – Thọ | Dev 3 – Tiến | Dev 4 – Thiêng |
|---|---|---|---|---|
| 1 (đã hoàn thành) | Đọc SRS phần xác thực, bảo mật | Đọc SRS phần công thức, mô hình dữ liệu | Đọc SRS phần danh mục, tìm kiếm, SEO | Đọc SRS phần kiến trúc, hạ tầng, tệp, job, quan sát |
| 2 (đã hoàn thành) | Đăng ký, đăng nhập | Danh sách & chi tiết công thức | Danh sách & chi tiết danh mục | Docker + upload/xóa ảnh; **yêu cầu bổ sung:** dữ liệu mẫu 20 danh mục / 100 công thức |
| 3 | Google, đăng xuất + lỗi Auth, `IUserRepository` | Tạo công thức nháp + ảnh + lỗi Recipe, `IRecipeRepository` | Admin CRUD danh mục + lỗi Category, `ICategoryRepository` | Sửa mã lỗi 422→400, **nền exception / repository & Unit of Work / middleware**, health check (3 endpoint), dashboard Hangfire, bộ test tích hợp |
| 4 — **toàn bộ API** | API: gia hạn phiên, hồ sơ, Admin quản lý tài khoản, quản lý phiên | API: nguyên liệu, các bước, sửa công thức | API: tìm kiếm, lọc/sắp xếp, "công thức của tôi", sitemap; vá rò rỉ bản nháp | API: xuất bản, lưu trữ, xóa mềm; job resize, job dọn dữ liệu; test 45/45 endpoint |
| 5 | Giao diện hồ sơ, giữ phiên (D-12) | Giao diện nguyên liệu, các bước, sửa, xuất bản | Giao diện tìm kiếm, bộ lọc; composite index | Health check: nối Docker, failover Nginx, giao diện |
| 6 | Rate limit, CSP, chặn route | Giao diện "công thức của tôi", xóa mềm | Trang chủ, SEO, JSON-LD | Log có cấu trúc, tracing |
| 7 | Giao diện Admin quản lý tài khoản, quản lý phiên | Giao diện lưu trữ / khôi phục | Sitemap, robots, tối ưu ảnh | Nginx production, SSL, cache, scale |
| 8 | Test E2E + bảo mật | Test E2E + đồng thời | Test E2E + WCAG + Lighthouse | Load test, đóng gói, `v1.0.0` |

**Quy trình chung:** mỗi buổi mỗi người tạo nhánh `{mssv}_{HoTenKhongDau}_buoiso{n}` từ `develop` (ví dụ `2312755_NguyenThangThieng_buoiso3`) → **1 commit / người / buổi** → mở **Pull Request vào `develop`** (không bao giờ vào `main`) → merge vào `develop` theo thứ tự **Dev 4 → Dev 1 → Dev 3 → Dev 2** → hết buổi merge `develop` vào `main`. Commit chỉ được merge khi đạt Definition of Done (build 0 warning, test xanh, lint + build frontend xanh, `docker compose up` chạy, đã test tay). Commit phải mang **họ tên và email GitHub thật** của người làm (`git config user.name` / `user.email`); **không push code chưa build được**; báo cáo cá nhân đặt tên `SPEC/BAO_CAO_LAB_0{n}_{MSSV}_{HoTenKhongDau}.md` với n = số buổi, nằm cùng nhánh với code.

---

# Phần 3 – Chia việc chi tiết từng buổi

> **Cách đọc mỗi phần việc:**
> **Chức năng** – làm gì · **Hướng đi** – các bước làm theo thứ tự · **Vì sao** – lý do chọn hướng đó · **Xong khi** – điều kiện kiểm chứng · **Commit** – thông điệp commit.
> Chi tiết kỹ thuật đầy đủ (đoạn code mẫu, phân tích phương án bị loại) nằm trong [`SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`](SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md).

---

## BUỔI 1 – Đọc đặc tả yêu cầu hệ thống & tìm hiểu các tính năng cần xây dựng (đã hoàn thành)

**Mục tiêu:** cả nhóm nắm được phạm vi sản phẩm, lớp người dùng, kiến trúc và toàn bộ yêu cầu; mỗi người hiểu sâu phần mình sẽ phụ trách từ Buổi 2. Buổi này không viết code.

| Thành viên | Phần SRS đọc kỹ | Cần trả lời được sau buổi |
|---|---|---|
| **Dev 1 – Hoàng Bình Quân** | FR-AUTH-001 → 009, §2.3 lớp người dùng, NFR-SEC, API Auth (§8.1) | Luồng đăng ký, đăng nhập, Google, gia hạn phiên, đăng xuất; role Author/Admin; mã lỗi `AUTH_*` |
| **Dev 2 – Nguyễn Hồng Phúc Thọ** | FR-RCP-001 → 011, Chương 7 mô hình dữ liệu, API Recipe (§8.3 – §8.6) | Máy trạng thái Draft/Published/Archived; nguyên liệu, các bước, ảnh; xóa mềm; chống ghi đè đồng thời |
| **Dev 3 – Đoàn Hồng Tiến** | FR-CAT-001 → 005, FR-SRCH-001 → 004, NFR-SEO, route frontend (§5.1) | Tìm kiếm tiếng Việt không dấu; lọc, sắp xếp, phân trang; SSR/ISR từng trang; SEO |
| **Dev 4 – Nguyễn Thăng Thiêng** | FR-FILE, FR-JOB, FR-OBS, Chương 6 kiến trúc, CONS-001 → 010, NFR-PERF/REL/SCALE, §2.6 giả định | Clean Architecture + CQRS; 8 service Docker; cache Redis; job nền; health check, log, tracing |

- **Hướng đi:** (1) cả nhóm đọc Chương 1 – 2; (2) mỗi người đọc sâu phần của mình theo bảng trên; (3) gom thành bảng đầu việc chung — **Phần 1 của README này** (37 FR, 30 NFR, 10 CONS, 44 endpoint, 27 mã lỗi); (4) ghi mọi chỗ SRS chưa rõ hoặc tự mâu thuẫn vào [`SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`](SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md); (5) chốt công nghệ – phiên bản ([`SPEC/CONG_NGHE_VA_PHIEN_BAN.md`](SPEC/CONG_NGHE_VA_PHIEN_BAN.md)) và phân công cố định.
- **Vì sao cần một buổi riêng:** schema, pipeline MediatR, cache và topology Docker dựng ở Buổi 2 đều khó sửa khi đã có dữ liệu; đọc kỹ SRS trước giúp dựng đúng một lần và phát hiện mâu thuẫn trên giấy thay vì khi đã code.
- **Xong khi:** mỗi người trình bày được chức năng của module mình và module liên quan; bảng đầu việc và danh sách câu hỏi/mâu thuẫn được cả nhóm thống nhất.

---

## BUỔI 2 – Nền móng & lát cắt đầu tiên (đã hoàn thành, 11/09 – 14/09/2026; bổ sung dữ liệu mẫu 21/09/2026)

**Mục tiêu:** có khung dự án chạy được bằng Docker và 4 lát cắt đầu tiên: đăng ký/đăng nhập, xem công thức, xem danh mục, upload ảnh.

**Năm yêu cầu của giảng viên cho Buổi 2:**

| # | Yêu cầu | Trạng thái | Nơi thực hiện |
|---|---|---|---|
| 1 | Tạo cấu trúc dự án backend theo Clean Architecture | Đã hoàn thành | Commit khung `b058654`: 4 tầng Domain / Application / Infrastructure / API + 4 project test, kiểm bằng `LayerDependencyTests` |
| 2 | Cài đặt các gói thư viện cần thiết | Đã hoàn thành | EF Core + Npgsql, Identity, MediatR, FluentValidation, Bogus, Hangfire, Redis, AWSSDK.S3, Serilog, OpenTelemetry… (`CONG_NGHE_VA_PHIEN_BAN.md`) |
| 3 | Cài đặt các lớp entities, configuration, DbContext | Đã hoàn thành | `Domain/Entities`, `Infrastructure/Persistence/Configurations`, `CulinaryBlogDbContext` |
| 4 | Tạo migration, cài đặt các lớp để tạo dữ liệu ngẫu nhiên | Đã hoàn thành | Migration `B1_InitialSchema`, `DatabaseSeeder` (Bogus) |
| 5 | CSDL có dữ liệu ngẫu nhiên cho ít nhất 20 categories, 100 recipes; mỗi recipe ít nhất 10 nguyên liệu và 5 bước chế biến | **Yêu cầu bổ sung, khác SRS v1.2.0 — đã hoàn thành 21/09/2026** | Mục "Yêu cầu bổ sung" của Dev 4 bên dưới; SRS v1.2.1 §2.6.1 (CR-2026-03) |

**Tiến trình đã thực hiện:** 30 phút đầu Dev 4 đẩy commit khung (`b058654`) → 4 người tách nhánh làm song song → merge vào `develop` theo thứ tự Dev 4 → Dev 1 → Dev 3 → Dev 2 → merge `main` (`d2d8243`) → rà soát chạy container và sửa lỗi (`064f582`, `6137d3e`).

### Dev 1 – Hoàng Bình Quân · FR-AUTH-001, FR-AUTH-002 (đã hoàn thành)
- **Đã làm:** `ApplicationUser` + `RefreshToken` (chỉ lưu SHA-256); Identity PBKDF2 100.000 vòng, mật khẩu mạnh, khóa 5 lần/15 phút; `RegisterUserCommand`, `LoginUserCommand`; `POST /auth/register` (201), `POST /auth/login` (200); trang `/auth/login`, `/auth/register` (React Hook Form + Zod).
- **Vì sao:** dùng ASP.NET Core Identity thay vì tự viết bảng user để có sẵn băm mật khẩu chuẩn, khóa tài khoản, role; lưu hash của refresh token ngay từ đầu để lộ DB cũng không mạo danh được ai.
- **Còn nợ:** D-1 (Buổi 3), D-2 và D-12 (Buổi 4); integration test đăng ký/đăng nhập viết khi có bộ test ở Buổi 3.
- **Commit:** `3d40be6 feat(auth): complete FR-AUTH-001 & 002 register login flow`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-001, FR-RCP-002 (đã hoàn thành)
- **Đã làm:** toàn bộ schema công thức (Recipe, Step, Ingredient, Image, Nutrition dạng owned); seed 50 công thức / 5 tác giả bằng Bogus (nâng lên 100 công thức ở yêu cầu bổ sung); `GET /recipes` (phân trang, lọc), `GET /recipes/{slug}`; trang `/recipes` (SSR) và `/recipes/[slug]` (ISR 300s).
- **Vì sao:** dựng đủ schema ngay buổi đầu vì schema là thứ đắt nhất để sửa khi đã có dữ liệu; truy vấn đọc dùng `AsNoTracking` + projection để tránh N+1 từ gốc.
- **Còn nợ:** D-3, D-4, D-6 (Buổi 6), D-8 (Buổi 3), D-15 (Buổi 5), D-16 (Buổi 4).
- **Commit:** `0fc95d1 feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view`

### Dev 3 – Đoàn Hồng Tiến · FR-CAT-001, FR-CAT-002 (đã hoàn thành)
- **Đã làm:** entity `Category` + seed 8 danh mục (nâng lên 20 ở yêu cầu bổ sung); `SlugHelper` bỏ dấu tiếng Việt; `RedisCacheService` (Redis lỗi thì đọc DB); `GET /categories`, `GET /categories/{slug}`; trang `/categories`, `/categories/[slug]`.
- **Vì sao:** chọn Redis ngay từ đầu (không `IMemoryCache`) vì chạy nhiều instance thì cache trong bộ nhớ không invalidate được.
- **Còn nợ:** D-13 (Buổi 3), D-18 (Buổi 4), D-5, D-7 (Buổi 6); Value Object `Slug` và component `CategoryNav` bổ sung ở Buổi 3.
- **Commit:** `fd27a71 feat(category): complete FR-CAT-001 & 002 public categories API and UI`

### Dev 4 – Nguyễn Thăng Thiêng · Hạ tầng + FR-FILE-001, FR-FILE-002 (đã hoàn thành)
- **Đã làm:** commit khung cho cả nhóm; Docker Compose 8 service + mailhog, Dockerfile multi-stage non-root; `MinioFileStorageService` (AWSSDK.S3); kiểm tra magic bytes; `POST /files/upload`, `DELETE /files/{**key}`; component `ImageUploader` (kéo-thả, % tiến trình); làm sớm FR-JOB-001 (email chào mừng qua Hangfire).
- **Vì sao:** dùng AWSSDK.S3 để sau này đổi sang AWS S3 thật chỉ cần đổi cấu hình; kiểm magic bytes vì `Content-Type` do client khai là giả mạo được; tên file do server sinh (`{Guid}`) để chặn path traversal.
- **Sửa sau buổi:** chờ DB sẵn sàng khi khởi động, Nginx 502 khi container api đổi IP, hook gitleaks trên Windows (`064f582`); ảnh gần 5MB bị Nginx chặn, log khởi động (`6137d3e`).
- **Commit:** `b058654 chore: bootstrap…` · `9dc3d81 feat(infra): complete docker compose setup and FR-FILE minio upload component`

### Dev 4 – Nguyễn Thăng Thiêng · Yêu cầu bổ sung: dữ liệu mẫu (đã hoàn thành, 21/09/2026)
> **Yêu cầu mới, khác file SRS v1.2.0:** SRS v1.2.0 §2.6.1 chỉ yêu cầu 50 công thức và 5 tác giả mẫu. Giảng viên yêu cầu CSDL có ít nhất **20 danh mục, 100 công thức; mỗi công thức ít nhất 10 nguyên liệu và 5 bước chế biến**. Yêu cầu đã được ghi vào **SRS v1.2.1** (CR-2026-03, MT-58) trước khi code.
- **Đã làm:** catalog dữ liệu thật trong `Infrastructure/Persistence/Seed/Data/` — 20 danh mục (8 cũ + 12 mới: Cơm & Xôi, Lẩu, Hải sản, Gỏi & Salad, Món cuốn, Món hấp, Món chiên, Ăn vặt, Món Hàn Quốc, Món Nhật Bản, Món Thái, Món Âu) và 100 món (50 món cũ giữ nguyên tên, slug, danh mục + 50 món mới), mỗi món 10 – 14 nguyên liệu có định lượng và 5 – 6 bước có mô tả, thời gian; `RecipeSeedCatalog` đọc catalog và khai báo ngưỡng; `DatabaseSeeder` idempotent, tự bù; xóa cache danh mục và Output Cache công thức khi dữ liệu thay đổi; `RecipeSeedCatalogTests` (5 test).
- **Vì sao:** Bogus chọn ngẫu nhiên từ danh sách nguyên liệu chung sẽ ra món sai (sinh tố có nước mắm, bước nấu bằng chữ Latin), trong khi dữ liệu này dùng để kiểm thử tìm kiếm, đo hiệu năng và trình diễn. Vì vậy nội dung món ăn viết tay; Bogus (seed cố định) chỉ sinh tác giả, trạng thái, ngày xuất bản, dinh dưỡng. Seeder tự bù để không ai phải xóa volume: công thức mẫu cũ chưa từng bị sửa được thay bằng nội dung đúng, dữ liệu người dùng không bị động tới.
- **Xong khi (đã kiểm chứng trên Docker):** database cũ → log *"50 recipes created, 50 recipes repaired"*; database trống → *"100 recipes created"*; khởi động lại → *"0 created, 0 repaired"*; SQL: 20 danh mục, 100 công thức, tối thiểu 10 nguyên liệu và 5 bước mỗi món, số bước liên tục, không còn Lorem ipsum; `dotnet test` 49/49.
- **Commit:** `feat(seed): seed 20 categories and 100 real recipes per CR-2026-03` (nhánh `2312755_NguyenThangThieng_buoiso2`)

---

## BUỔI 3 – Nền tảng xử lý lỗi & truy cập dữ liệu, Google / Đăng xuất, tạo công thức nháp, CRUD danh mục, health check, hạ tầng kiểm thử

**Mục tiêu:**
- Đủ đường vào hệ thống (email, Google, đăng xuất); Author tạo được công thức nháp kèm ảnh; Admin quản trị danh mục.
- Xem được trạng thái hệ thống qua health check và dashboard job an toàn; **cả nhóm có bộ integration test**.
- **Bổ sung 29/09/2026:** cả nhóm dựng xong **Domain Exceptions**, **Repository & Unit of Work**, **Global Exception Middleware**, và **mỗi người giao ≥ 2 API chạy thật**.

**Yêu cầu của giảng viên (29/09/2026) — chia cho cả 4 người theo mảng mình phụ trách:**

| Yêu cầu | Quân – Auth | Thọ – Công thức | Tiến – Danh mục | Thiêng – Hạ tầng |
|---|---|---|---|---|
| Domain exceptions | Cụm lỗi Auth (6 lớp) | Cụm lỗi Recipe (5 lớp) | Cụm lỗi Category (3 lớp) | Lớp gốc `DomainException` + lỗi dùng chung |
| Repository & Unit of Work | `IUserRepository` | `IRecipeRepository` | `ICategoryRepository` | `IRepository<T>`, `IUnitOfWork`, `UnitOfWork` (transaction) |
| Middleware Problem Details | Ánh xạ lỗi Auth → HTTP | Ánh xạ lỗi Recipe → HTTP | Ánh xạ lỗi Category → HTTP | `GlobalExceptionMiddleware` + bảng ánh xạ + test hợp đồng |
| API mới (≥ 2) | 2: `POST /auth/google`, `POST /auth/logout` | 4: `POST /recipes`, `POST`/`PATCH`/`DELETE /recipes/{id}/images…` | 3: `POST`/`PUT`/`DELETE /categories` | 3: `GET /health`, `/health/live`, `/health/ready` |

Tổng **12 API mới**. `POST /files/upload` đã làm ở Buổi 2 nên không tính.

**Tiến trình trong buổi:**
1. **CR-2026-04 — ✅ đã áp dụng (SRS v1.2.2):** 2 mã lỗi `AUTH_USER_NOT_FOUND` (404) và `CONCURRENCY_CONFLICT` (409) đã có trong Phụ lục B. Chi tiết ở kế hoạch §4.5.
2. **Commit nền D-11 (422 → 400)** — ✅ đã xong (commit `67d29c0`).
3. **Mốc M1 (chậm nhất giữa buổi):** Thiêng merge **commit nền kiến trúc** vào `develop`. Trong lúc chờ, 3 người còn lại làm phần không phụ thuộc:
   - Quân: D-1 + nút Google;
   - Thọ: D-8 + validator + wizard;
   - Tiến: validator + trang quản trị + D-13.
4. **Sau M1:** 3 người rebase, viết exception + repository + file ánh xạ của mảng mình, rồi handler và test.
5. **Cuối buổi:** merge Thiêng → Quân → Tiến → Thọ.
   - `IUnitOfWork.cs`, `UnitOfWork.cs` chỉ được **thêm dòng** vào phần của mảng mình; `ErrorCodes.cs` đã có đủ 29 mã từ commit nền của Thiêng.
   - File ánh xạ lỗi và bộ dịch lỗi DB mỗi mảng một file riêng, được quét tự động.

### Commit nền – Nguyễn Thăng Thiêng dẫn · D-11 (422 → 400) — ✅ đã xong
- **Hướng đi:**
  - `GlobalExceptionMiddleware` trả **400** + `type = VALIDATION_ERROR`; đổi mọi `Status422UnprocessableEntity` trong endpoint.
  - Frontend: `LoginForm`, `RegisterForm` đổi `status === 422` → `400`; gom việc gắn lỗi vào từng ô thành helper `mapProblemDetailsToForm()` trong `lib/api-client.ts`.
  - Sửa test cũ; thêm test chặn chuỗi `Status422` quay lại.
- **Vì sao làm trước tiên:** mọi endpoint mới của buổi đều trả lỗi validation. Nếu đổi backend mà quên frontend, form sẽ **âm thầm mất lỗi từng ô** – không crash, không log, chỉ người dùng nhập sai mới phát hiện.
- **Commit:** gộp trong `feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness` (`67d29c0`).

### Dev 1 – Hoàng Bình Quân · FR-AUTH-003 Google + FR-AUTH-005 Đăng xuất (+ D-1, lỗi Auth, `IUserRepository`)
- **Hướng đi:**
  1. **Lỗi Auth** (`Domain/Exceptions/Auth/`) — lớp gốc `AuthDomainException` và 6 lớp con:

     | Lớp | HTTP | Ghi chú |
     |---|---|---|
     | `InvalidCredentialsException` | 401 | |
     | `InvalidTokenException` | 401 | |
     | `AccountDisabledException` | 403 | |
     | `UserNotFoundException` | 404 | dùng ở Buổi 4, mã `AUTH_USER_NOT_FOUND` (SRS v1.2.2) |
     | `EmailAlreadyExistsException` | 409 | |
     | `AccountLockedException` | **423** | kèm `lockoutEnd` — đúng Phụ lục B, không phải 403 |

     Đăng ký ánh xạ trong `AuthExceptionMappings`. Chuyển các chỗ ném lỗi cũ của đăng ký/đăng nhập sang các lớp này.
  2. **`IUserRepository`** (thay `IRefreshTokenRepository` cũ):
     - thêm/tìm refresh token để đăng xuất;
     - tra tên đăng nhập theo tiền tố cho bộ sinh `UserName`.

     Ghi user (tạo, liên kết Google, khóa) vẫn qua `UserManager`. `RefreshToken.Revoke()` kéo từ Buổi 4 lên.
  3. **D-1:** `IUserNameGenerator` sinh `UserName` từ email (trùng thì thêm `2, 3…`); đăng ký nhận `{ email, password, displayName }`; `UserDto` đổi sang `displayName`, thêm `bio`; bỏ ô "Tên đăng nhập" trên form.
  4. **FE Google:** cài `@react-oauth/google`, dùng **component `<GoogleLogin>`** để lấy **ID Token** rồi gửi `POST /auth/google`. Không dùng hook `useGoogleLogin` (hook đó trả access token, backend không verify được).
  5. **`GoogleLoginCommand`:** verify qua `IGoogleIdTokenValidator` (bọc `GoogleJsonWebSignature.ValidateAsync`, kiểm chữ ký, `iss`, `aud`, `exp`).
     - Đã liên kết Google → đăng nhập.
     - Email đã tồn tại → **chỉ liên kết khi `EmailVerified == true`**.
     - Mới hoàn toàn → tạo user role Author.
     - Lỗi: token thiếu dữ liệu 400, chữ ký sai 401, không lấy được khóa Google 502, tài khoản bị vô hiệu 403.
  6. **`LogoutCommand`:** hash token nhận được → `Revoke()`; không tìm thấy vẫn trả 204.
  7. **API + UI + test:**
     - API: `POST /auth/google` (200), `POST /auth/logout` (204).
     - UI: nút Google ở `/auth/login` kèm thông báo khi Google lỗi; menu "Đăng xuất" gọi API rồi mới xóa phiên phía client.
     - Test (Google giả trong test): 200 / 400 / 401 / 403 / 502; 2 email cùng prefix → `an.nguyen`, `an.nguyen2`; đăng xuất 2 lần đều 204; khóa do sai mật khẩu vẫn 423.
- **Vì sao:**
  - **ID Token flow** giữ backend stateless: không lưu state/PKCE verifier, không cần route callback hay client secret.
  - **Backend bắt buộc tự verify chữ ký**: tin email do FE gửi lên thì ai cũng `curl` chiếm được tài khoản người khác.
  - **Chỉ liên kết khi email đã xác minh**: nếu không, kẻ xấu tạo tài khoản Google gắn email nạn nhân là chiếm được tài khoản.
  - **Đăng xuất luôn 204**: trả 404 sẽ biến endpoint thành công cụ dò token hợp lệ.
  - **Không ghi thẳng bảng user**: `UserManager` lo chuẩn hóa email, `SecurityStamp`, đếm lần sai để khóa — tự ghi bảng sẽ bỏ sót mà không báo lỗi.
  - **D-1 làm ngay buổi này** vì Google tạo user mà không ai nhập `userName`, và `UserDto` đổi sớm thì ít màn hình phải sửa.
- **Xong khi:** đăng nhập Google tạo/liên kết đúng tài khoản; đăng xuất thu hồi refresh token trong DB; form đăng ký không còn ô tên đăng nhập; integration test xanh.
- **Commit:** `feat(auth): add auth domain exceptions, user repository and complete google sign-in & logout APIs`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-003 Tạo nháp + FR-RCP-008 Ảnh (+ D-8, lỗi Recipe, `IRecipeRepository`)
- **Hướng đi:**
  1. **D-8 trước tiên:** migration `B3_Recipe_InstructionsNullable` cho `Instructions` NULL (nếu không, mọi request thiếu trường này lỗi DB).
  2. **Lỗi Recipe** (`Domain/Exceptions/Recipes/`) — lớp gốc `RecipeDomainException` và 5 lớp con:

     | Lớp | HTTP | Ghi chú |
     |---|---|---|
     | `RecipeNotFoundException` | 404 | |
     | `RecipeImageNotFoundException` | 404 | |
     | `RecipeConcurrencyException` | 409 | `RECIPE_CONCURRENCY_CONFLICT` |
     | `InvalidRecipeStatusException` | 409 | dùng ở Buổi 4 (API vòng đời — Thiêng) |
     | `RecipeSlugConflictException` | 409 | |

     Đăng ký trong `RecipeExceptionMappings`.
  3. **`IRecipeRepository`:**
     - `GetByIdWithDetailsAsync` (nạp bước, nguyên liệu, ảnh; dinh dưỡng là owned entity nên tự nạp);
     - `GetByIdWithImagesAsync`;
     - `SlugExistsAsync`.

     Kèm bộ dịch lỗi DB: xung đột `RowVersion` → 409 `RECIPE_CONCURRENCY_CONFLICT`, trùng slug do tranh chấp → 409 `RECIPE_SLUG_EXISTS`.
  4. **`CreateRecipeCommand` + Validator:**
     - `title` 5–200, `description` 20–2000, `prepTime > 0`, `cookTime ≥ 0`, `servings > 0`;
     - `categoryId` phải tồn tại (kiểm bằng `IRepository<Category>` dùng chung) → sai trả 400 ở ô danh mục;
     - dùng `Recipe.Create(...)` (luôn Draft); slug từ `SlugHelper`, trùng thì `-2`, `-3`;
     - **cấm slug trùng từ khóa dành riêng** `search, mine, sitemap, new, edit`;
     - nutrition gửi kèm body (không có endpoint riêng).
  5. **`RecipeAuthorizationHandler`:** qua khi là chủ công thức hoặc Admin – dùng lại cho **mọi** FR-RCP về sau.
  6. **Ảnh:**
     - `UploadRecipeImageCommand`: dùng lại `IFileStorageService` + `ImageFileInspector` của Buổi 2, ảnh đầu tiên tự là ảnh chính, **để sẵn chỗ `BackgroundJob.Enqueue` cho job resize**;
     - `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`;
     - `DeleteRecipeImageCommand`: xóa ảnh chính thì ảnh có `OrderIndex` nhỏ nhất lên thay.
     - Đổi ảnh chính làm **2 lần `SaveChanges` trong 1 transaction** (`ExecuteInTransactionAsync`).
     - Mọi command xóa cache `recipes:list:*` + `recipe:{slug}`.
  7. **API + UI:**
     - API: `POST /recipes` (201), `POST/PATCH/DELETE /recipes/{id}/images…` (MinIO lỗi → 503).
     - Trang `/dashboard/recipes/new` dạng wizard: bước 1 thông tin + dinh dưỡng, bước 2 thư viện ảnh (chọn ảnh chính bằng radio).
     - Test: đổi ảnh chính 10 lần liên tiếp không lần nào vỡ unique index; hai phiên cùng sửa một công thức → 409.
- **Vì sao:**
  - **Luôn tạo ở trạng thái Draft** để có điểm chặn kiểm tra điều kiện xuất bản (đủ bước + nguyên liệu) ở Buổi 4 (API publish).
  - **Nutrition đi kèm công thức** vì nó chỉ là nhóm cột trong bảng `Recipes`; endpoint riêng sẽ tạo hai đường ghi cùng một dữ liệu.
  - **Repository theo aggregate, không mở `IQueryable`**: không handler nào tự quyết `Include` rồi quên nạp bước/nguyên liệu.
  - **Lỗi xung đột dịch ở `UnitOfWork`**: tầng Application không biết EF, nên handler không phải (và không thể) tự bắt `DbUpdateConcurrencyException`.
  - **Handler phân quyền dựng ngay từ FR ghi đầu tiên** để không handler nào tự viết `if` rồi quên.
  - **Đổi ảnh chính 2 bước** vì index "chỉ 1 ảnh chính" là index một phần – PostgreSQL không cho hoãn kiểm tra, còn EF không đảm bảo thứ tự 2 lệnh UPDATE.
  - Mảng `steps?`/`ingredients?` trong body tạo công thức **để sang Buổi 4** để validator nguyên liệu/bước chỉ viết một lần.
- **Xong khi:** Author tạo được công thức nháp kèm nhiều ảnh, đổi được ảnh chính; slug dành riêng bị chặn; integration test xanh.
- **Commit:** `feat(recipe): add recipe domain exceptions, recipe repository and complete create draft & recipe image APIs`

### Dev 3 – Đoàn Hồng Tiến · FR-CAT-003/004/005 CRUD danh mục Admin (+ D-13, lỗi Category, `ICategoryRepository`)
- **Hướng đi:**
  1. **Lỗi Category** (`Domain/Exceptions/Categories/`) — lớp gốc `CategoryDomainException` và 3 lớp con:

     | Lớp | HTTP | Ghi chú |
     |---|---|---|
     | `CategoryNotFoundException` | 404 | |
     | `CategoryNameAlreadyExistsException` | 409 | |
     | `CategoryHasRecipesException` | 409 | kèm `recipeCount` |

     Đăng ký trong `CategoryExceptionMappings`.
  2. **`ICategoryRepository`:**
     - `ExistsByNameAsync(name, excludeId?)`, `SlugExistsAsync`, `CountActiveRecipesAsync`;
     - bộ dịch lỗi DB: PostgreSQL `23505` trên tên danh mục → 409 (hai Admin tạo trùng cùng lúc);
     - repository **chỉ dùng EF Core**, không tự cache.
  3. **`CreateCategoryCommand`:** `name` 2–100, `imageUrl?` URL hợp lệ, `orderIndex ≥ 0`; **kiểm tra tên trùng trước khi ghi** → 409 `CATEGORY_NAME_EXISTS`; slug tự sinh; trả 201 + header `Location`.
  4. **`UpdateCategoryCommand`:** cùng validator, cũng kiểm tra tên trùng; **slug không đổi khi đổi tên**.
  5. **`DeleteCategoryCommand`:** còn công thức → 409 `CATEGORY_DELETE_HAS_RECIPES` kèm **số công thức**; không còn → **xóa mềm** (`Category.SoftDelete()`).
  6. **Cache + API + UI:**
     - D-13 đổi TTL `categories:all` 60 → **30 phút**; 3 command xóa `categories:all` + `categories:detail:*`.
     - `POST/PUT/DELETE /categories` chỉ Admin (Author → 403).
     - Trang `/dashboard/categories`: bảng, form modal (có `ImageUploader`), hộp xác nhận xóa, thông báo 409 dễ hiểu.
     - Bổ sung Value Object `Slug` và component `CategoryNav` còn thiếu từ Buổi 2.
     - Test: 201 / 403 / 409 trùng tên / 2 request trùng tên song song → một 201, một 409, không có 500 / 409 còn công thức.
- **Vì sao:**
  - **Kiểm tra tên trùng ở tầng Application** để người dùng nhận thông báo nghiệp vụ rõ ràng thay vì "Lỗi hệ thống 500"; vẫn giữ lớp bắt `23505` vì kiểm tra trước không loại được hoàn toàn race condition.
  - **Bắt `23505` ở tầng Infrastructure** (bộ dịch của mảng danh mục), không ở middleware: chi tiết riêng của PostgreSQL không lọt lên tầng API, và không ai phải sửa một file middleware dùng chung.
  - **Repository không tự cache**: hệ thống chỉ có một cơ chế cache (`ICacheable`/`ICacheInvalidator` trong pipeline MediatR); thêm tầng cache thứ hai là thêm chỗ quên xóa cache.
  - **Slug giữ nguyên khi đổi tên** để link đã chia sẻ và URL Google đã lập chỉ mục không chết.
  - **Xóa mềm** đúng thiết kế `BaseEntity` toàn hệ thống.
  - **Báo số công thức cụ thể** để Admin biết khối lượng việc cần chuyển trước khi xóa.
- **Xong khi:** Admin tạo/sửa/xóa danh mục trên giao diện; Author bị chặn 403; cache được xóa đúng sau mỗi thao tác.
- **Commit:** `feat(category): add category domain exceptions, category repository and complete create & update admin category APIs` (gồm cả `DELETE`)

### Dev 4 – Nguyễn Thăng Thiêng · Nền exception / repository / middleware + health check + dashboard Hangfire + bộ integration test
- **Hướng đi:**
  1. **Nền exception:**
     - `DomainException` thành lớp gốc trừu tượng (mang `Code`, không mang mã HTTP), thêm `BusinessRuleViolationException`, `ConcurrencyConflictException`;
     - chuyển `ErrorCodes` về Domain;
     - thêm `BadGatewayException` (502).
  2. **Nền repository & Unit of Work:**
     - `IRepository<T>` (không có xóa cứng, không trả `IQueryable`), `IUnitOfWork`, `UnitOfWork`;
     - `ExecuteInTransactionAsync` chạy qua execution strategy vì DbContext bật retry;
     - `UnitOfWork` dịch lỗi ghi DB qua bộ dịch của từng mảng.
  3. **Middleware:**
     - `GlobalExceptionMiddleware` tra `ExceptionStatusMap` (mỗi mảng một file ánh xạ, quét tự động);
     - test kiến trúc: exception nào chưa đăng ký ánh xạ là test đỏ;
     - test hợp đồng Problem Details cho 400/401/403/404/409/423/500/502/503;
     - test rollback transaction trên PostgreSQL thật.
  4. **Health check (FR-OBS-001, kéo từ Buổi 5 lên):**
     - `/health` (tất cả, 503 khi lỗi); `/health/live` (luôn 200 khi process sống); `/health/ready` (PostgreSQL + Redis);
     - MinIO chỉ làm `/health` thành `Degraded`;
     - test: Redis sai địa chỉ → ready 503, live vẫn 200.
  5. ✅ **Dashboard:** `MapHangfireDashboard("/hangfire")` (chỉ ở container `api`) với filter `NginxGateDashboardFilter`: chỉ cho qua khi header `X-Hangfire-Gate` khớp secret (so sánh `FixedTimeEquals`).
  6. ✅ **Nginx:**
     - `location /hangfire` có **Basic Auth** (`htpasswd`, không commit) và gắn header gate;
     - chuyển `nginx.conf` sang thư mục `templates/` để nạp secret bằng `envsubst`;
     - thêm biến giả vào `.env.example`.
  7. ✅ **Bộ test tích hợp** trong `tests/CulinaryBlog.API.IntegrationTests`:
     - `CulinaryBlogApiFactory` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, chạy migration, tắt Hangfire server, thay `IFileStorageService`/`IBackgroundJobClient` bằng bản giả;
     - `Respawn` dọn DB giữa các test;
     - helper `CreateClientAs(role)` phát JWT thật.
  8. ✅ **Trả nợ test Buổi 2:** integration test cho mọi endpoint Buổi 2; viết sẵn test tái hiện lỗ hổng rò rỉ bản nháp (đánh dấu `Skip` – Tiến gỡ ở Buổi 4).
- **Vì sao:**
  - **Thiêng làm phần nền, chủ mảng làm phần của mảng mình**: phần không thuộc mảng nào chỉ nên có một kiểu viết; còn "tên danh mục trùng trả mã gì" là kiến thức của Tiến và Tiến dùng lại nó tới Buổi 7.
  - **Exception không mang mã HTTP**: Domain không được biết mình chạy sau một web API; tầng API quyết định mã HTTP.
  - **Ánh xạ theo kiểu, mỗi mảng một file**: trình biên dịch kiểm tra tên lớp, không ai sửa chung một `switch` lớn, và test bắt được lớp bị quên.
  - **Không có xóa cứng trong repository**: mọi xóa là xóa mềm; xóa vật lý chỉ ở job dọn dữ liệu (Buổi 4).
  - **Health check lên Buổi 3**: FR mức M vốn của Thiêng, và kiểm chứng cuối mỗi buổi đã gọi `/health` trong khi chưa có endpoint nào. Không đi qua MediatR vì Docker gọi mỗi 10 giây, cần ít phụ thuộc nhất.
  - **Dựng bộ test ngay Buổi 3** vì Definition of Done đòi test xanh ở mọi commit; để tới cuối dự án thì 5 buổi liền không ai viết được integration test.
  - **Testcontainers thay vì EF In-Memory** vì In-Memory không có unique, FK, `tsvector`, index một phần, concurrency token, transaction – đúng những thứ cần kiểm nhất.
  - **Hai lớp bảo vệ dashboard:** trình duyệt không gửi được Bearer token khi mở trang, nên dùng Basic Auth ở Nginx; header gate chặn trường hợp gọi thẳng cổng `5000` bỏ qua Nginx.
- **Xong khi:**
  - `http://localhost/hangfire` hỏi mật khẩu rồi vào được; `http://localhost:5000/hangfire` trả 401;
  - `curl http://localhost/health` trả `Healthy`;
  - test kiến trúc + test hợp đồng Problem Details + integration test xanh trên máy có Docker.
- **Commit:**
  - `feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness` — ✅ `67d29c0`
  - `feat(infra): add base domain exception, unit of work, global exception middleware and health check APIs` — ✅ code xong và kiểm chứng 29/09/2026 (117 test xanh; `/health` thử MinIO/Redis sập trên `docker compose`), chờ commit

**Kiểm chứng cuối Buổi 3:**
- Có đủ 3 cụm exception theo mảng + lớp gốc; không handler nào tham chiếu `DbContext`.
- Mọi lỗi trả `application/problem+json` đúng `type`; lỗi 500 không lộ stack trace.
- 12 API mới đều có integration test.
- Không còn chuỗi `Status422` / `status === 422` trong code; form đăng ký hiện lỗi từng ô với 400.
- 2 email cùng prefix sinh `UserName` có hậu tố số.
- `/hangfire` được bảo vệ; `/health` trả `Healthy`.

---

## BUỔI 4 – Hoàn thành toàn bộ API: phiên & tài khoản, nội dung công thức, truy vấn & tìm kiếm, vòng đời công thức, job nền

**Mục tiêu (yêu cầu của giảng viên, 29/09/2026): "Hoàn thành việc cài đặt tất cả API endpoints".**
- Cuối buổi, **45/45 endpoint** của SRS v1.2.2 Chương 8 cài xong **đúng hợp đồng**: mã HTTP, mã lỗi, DTO, cache.
- Mọi endpoint có integration test và hiện đủ trên Scalar.
- Hai job nền còn lại chạy thật.
- **Giao diện** của các chức năng này làm ở Buổi 5 → 7.

**Kết quả kiểm tra kế hoạch cũ — không đạt:**

| # | Vấn đề | Hệ quả |
|---|---|---|
| 1 | Buổi 4 cũ chỉ thêm 9 endpoint | Hết Buổi 4 mới có **29/44**. Còn thiếu 15: `/auth/me` ×2, `/users` ×2, `/auth/sessions` ×3, `PUT /recipes/{id}`, publish, unpublish, archive, unarchive, `DELETE /recipes/{id}`, `/recipes/mine`, `/recipes/sitemap` — nằm rải ở Buổi 5, 6, 7 |
| 2 | 3 endpoint đã có còn sai hợp đồng tới Buổi 5–6 | `GET /recipes` rò bản nháp (MT-34) và dùng `sort=-field`; `GET /recipes/{slug}` trả 403 cho bản nháp; `GET /categories/{slug}` lọc theo danh tính |
| 3 | SRS thiếu endpoint cho trang sửa bản nháp | Cài đủ 44 endpoint thì Author vẫn không sửa được bản nháp → bổ sung **`GET /recipes/mine/{id}`** qua CR-2026-04 (SRS v1.2.2, **45 endpoint**) |
| 4 | Dồn endpoint theo đúng module cũ | Thọ phải làm 15 endpoint, Thiêng 0 |

**Chia việc mới (API-first, cân bằng theo năng lực):**

| Người | Nhóm việc | Endpoint mới | Sửa hợp đồng | Kèm theo |
|---|---|---|---|---|
| Quân | Phiên & tài khoản (FR-AUTH-004, 006–009) | 8 | — | D-2, IP thật, claim `sid` |
| Thọ | Nội dung công thức (FR-RCP-009, 010, 004) | 8 | `POST /recipes` (mảng bước/nguyên liệu) | D-16 |
| Tiến | Truy vấn công khai & riêng tư (FR-SRCH-001–004, FR-RCP-011, sitemap) | 4 | `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` | D-18, **vá MT-34** (D-4, D-5, D-6), D-10 |
| Thiêng | Vòng đời công thức & job nền (FR-RCP-005, 006, 007, FR-JOB-002, 003) | 5 | — | D-3, D-15, test bề mặt API 45/45 |

**Kết quả:** 20 (Buổi 2 + 3) + 25 = **45/45 endpoint**.

- **Giao diện vẫn theo mảng cố định:** Thọ làm giao diện xuất bản, lưu trữ, xóa và "công thức của tôi" (Buổi 5–7); Tiến làm sitemap Next.js.
- **Hợp đồng giữa người làm API và người làm giao diện:** SRS Chương 8 + integration test.

**Tiến trình trong buổi:**
1. **Đầu buổi — Thiêng làm và đẩy lên `develop` ngay:**
   - đổi `Recipe` thành `partial class` (`Recipe.Lifecycle.cs` do Thiêng giữ);
   - tách endpoint `/recipes` thành 3 file: `RecipesEndpoints` (Thọ), `RecipeQueryEndpoints` (Tiến), `RecipeLifecycleEndpoints` (Thiêng).

   Ba người cùng sửa module Recipe mà không đụng file của nhau.
2. **Tiến vá MT-34 trước tiên** (lỗ hổng rò rỉ bản nháp đang chạy trên `develop`).
3. **Ba migration trên bảng `Recipes`** merge theo thứ tự Thiêng (`B4_Recipe_PartialUniqueSlug`) → Tiến (`B4_Search_FTS`) → Thọ (`B4_Recipe_IngredientStep`).
4. **Cuối buổi:** merge Thiêng → Quân → Tiến → Thọ; `ApiSurfaceTests` phải xanh.

### Dev 1 – Hoàng Bình Quân · API phiên & tài khoản: FR-AUTH-004, 006, 007, 008, 009 (+ D-2, IP thật, `sid`)
- **Hướng đi:**
  1. **D-2:** refresh token 64 → **32 byte (256-bit)**.
     - `RefreshToken` dùng lại `IsActive(now)` và `Revoke(...)`.
     - `IUserRepository` thêm tra theo hash, thu hồi tất cả, danh sách phiên còn hiệu lực.
     - `InvalidTokenException` thêm lỗi token hết hạn / đã thu hồi.
  2. **`RefreshTokenCommand`:**
     - không thấy → 401 `AUTH_TOKEN_INVALID`; hết hạn → 401 `AUTH_REFRESH_TOKEN_EXPIRED`;
     - **token đã thu hồi mà vẫn được dùng** → lưu việc thu hồi toàn bộ token của user **trước**, log cảnh báo, rồi 401 `AUTH_REFRESH_TOKEN_REVOKED`;
     - tài khoản bị khóa → 403;
     - hợp lệ → trong 1 transaction thu hồi token cũ và phát cặp mới.
  3. **Claim `sid`** (id phiên) trong mọi access token; **`UseForwardedHeaders`** với `KnownIPNetworks`.
  4. **Hồ sơ (kéo từ Buổi 5):** `GET /auth/me` (không trả trường nhạy cảm; user không còn → 401); `PATCH /auth/me` (partial; avatar phải là URL MinIO của hệ thống; không đổi email/username).
  5. **Quản lý tài khoản [Admin] (kéo từ Buổi 7):**
     - `GET /users` (phân trang, tìm kiếm, không cache);
     - `PATCH /users/{id}/status` — tự khóa mình → 403; user không tồn tại → 404 `AUTH_USER_NOT_FOUND`; khóa thì thu hồi mọi refresh token trong cùng transaction; ghi audit log.
  6. **Quản lý phiên (kéo từ Buổi 7):**
     - `GET /auth/sessions` (không bao giờ trả `tokenHash`; `isCurrent` theo `sid`);
     - `DELETE /auth/sessions/{id}` (phiên của người khác → 404);
     - `POST /auth/sessions/revoke-all`.
  7. **API — 8 endpoint mới:** `POST /auth/refresh`, `GET/PATCH /auth/me`, `GET /users`, `PATCH /users/{id}/status`, `GET /auth/sessions`, `DELETE /auth/sessions/{id}`, `POST /auth/sessions/revoke-all`. Giao diện (D-12, interceptor, `/profile`, `/dashboard/users`, tab phiên) ở Buổi 5 và 7.
- **Vì sao:**
  - **Rotation + phát hiện dùng lại** là cách duy nhất trong JWT stateless để biết token bị đánh cắp.
  - **Chỉ lưu hash**: lộ DB cũng không có token dùng được.
  - **Gom 4 FR tài khoản vào cùng buổi với refresh**: tất cả xoay quanh một cặp *user + tập refresh token*. Các hàm thu hồi và danh sách phiên được thiết kế một lần cho mọi nhu cầu, thay vì vá dần qua 3 buổi.
  - **Bật IP thật ngay buổi này**: từ đây mỗi lần refresh ghi `CreatedByIp`, và danh sách phiên hiển thị IP đó.
  - **Danh sách người dùng / phiên không cache**: dữ liệu riêng tư theo người gọi.
- **Xong khi:**
  - dùng lại token cũ → cả họ token bị thu hồi;
  - khóa user → refresh của họ trả 403;
  - phiên của người khác → 404;
  - `/auth/me` không có trường nhạy cảm.
- **Commit:** `feat(auth): complete FR-AUTH-004 refresh rotation and FR-AUTH-006/007/008/009 profile, admin user and session APIs`

### Dev 2 – Nguyễn Hồng Phúc Thọ · API nội dung công thức: FR-RCP-009, 010, 004 (+ D-16)
- **Hướng đi:**
  1. **Migration `B4_Recipe_IngredientStep`:** thêm `QuantityText varchar(50) NULL`, `CHECK (Quantity IS NULL OR Quantity > 0)`. **D-16:** unique index `(RecipeId, StepNumber)` → **unique constraint `DEFERRABLE INITIALLY DEFERRED`**.
  2. **Domain nguyên liệu:**
     - thêm `IngredientQuantityRequiredException` + một dòng ánh xạ 400;
     - thêm `AddIngredient/UpdateIngredient/RemoveIngredient`;
     - không được trống cả `Quantity`, `QuantityText` và `Unit` → 400.
  3. **Domain các bước:**
     - `AddStep` tự đánh số (đã có); thêm `RemoveStep` (đánh số lại 1..N) và `ReorderSteps(stepIds)` (tập id phải khớp chính xác);
     - mọi lần đánh số lại nằm trong **một `SaveChanges`**;
     - bật mảng `steps?`/`ingredients?` của `POST /recipes` bằng cách gọi lại đúng các hàm này.
  4. **Cập nhật công thức (kéo từ Buổi 5):** `PUT /recipes/{id}` nhận `rowVersion` (body hoặc `If-Match`).
     - `SetOriginalRowVersion` qua repository (handler không đụng `DbContext`).
     - Xung đột → 409 `RECIPE_CONCURRENCY_CONFLICT` (bộ dịch lỗi của Buổi 3 tự lo, handler không `try/catch`).
     - Slug chỉ đổi khi còn Draft.
     - Trả `rowVersion` mới + `ETag`.
  5. **API — 8 endpoint mới:** `POST/PUT/DELETE /recipes/{id}/ingredients/{ingId?}`, `POST/PUT/DELETE /recipes/{id}/steps/{stepId?}` (**body không có `stepNumber`**), `PATCH /recipes/{id}/steps/reorder`, `PUT /recipes/{id}`. Wizard bước 3–4 và trang sửa ở Buổi 5.
- **Vì sao:**
  - **2 cột định lượng**: tiếng Việt có "1/2 muỗng", "vừa đủ" không quy ra số được.
  - **Server tự đánh số bước**: API nhận *ý định* (thứ tự mới), không nhận giá trị cột.
  - **D-16 bắt buộc**: PostgreSQL chỉ cho *constraint* hoãn kiểm tra; không đổi thì reorder luôn lỗi `23505`.
  - **`PUT` cùng buổi với bước/nguyên liệu**: cùng aggregate, cùng hàm nạp dữ liệu, cùng phân quyền → viết một lần.
- **Xong khi:**
  - đổi chỗ bước 2 và 3 bằng reorder → 200, không lỗi 23505;
  - `PUT` với `rowVersion` cũ → 409;
  - `POST /recipes` kèm 10 nguyên liệu + 5 bước → 201.
- **Commit:** `feat(recipes): complete FR-RCP-009/010 ingredients & server-assigned steps with deferrable reorder and FR-RCP-004 rowversion update API`

### Dev 3 – Đoàn Hồng Tiến · API truy vấn: FR-SRCH-001–004, FR-RCP-011, sitemap (+ vá MT-34: D-4, D-5, D-6; D-10, D-18)
- **Hướng đi:**
  1. **Vá MT-34 trước tiên (kéo từ Buổi 6):**
     - `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` **chỉ trả Published** cho mọi người gọi;
     - bản nháp qua `/recipes/{slug}` → **404** (không phải 403);
     - bỏ Output Cache, chuyển sang cache Redis `ICacheable` (2′ danh sách, 5′ chi tiết, 2′ chi tiết danh mục);
     - gỡ `Skip` khỏi test tái hiện lỗ hổng.
  2. **Migration `B4_Search_FTS`** (thứ tự bắt buộc): `unaccent`, `pg_trgm` → hàm `unaccent_immutable` → cột **generated `STORED`** `SearchVector` → **GIN index**. **D-18:** xóa `vietnamese_unaccent` khỏi `init.sql`.
  3. **Lọc / sắp xếp / phân trang (kéo từ Buổi 5):**
     - `RecipeFilterSpec` dùng chung cho 4 truy vấn (danh sách, tìm kiếm, danh mục, của tôi);
     - `SortMapper` whitelist 5 trường; ngoài whitelist → 400;
     - `pageSize` mặc định 12, tối đa 50;
     - **D-10 phía API**: `sort=-field` → `sortBy` + `sortOrder`; thêm `maxPrepTime`, `minServings`.
  4. **`SearchRecipesQuery`:** `q` ≥ 2 ký tự; ghép prefix `pho:* & bo:*`; xếp theo `ts_rank`; chỉ Published; cache 1 phút.
  5. **Công thức của tôi (kéo từ Buổi 6):** `GET /recipes/mine` — không cache, `Cache-Control: no-store`; Author truyền `authorId` người khác → 403.
  6. **Chi tiết bản nháp (mới — CR-2026-04):** `GET /recipes/mine/{id}` — chủ hoặc Admin, mọi trạng thái, trả `rowVersion` + `ETag`, không cache; công thức của người khác → 404.
  7. **Sitemap (kéo từ Buổi 7):** `GET /recipes/sitemap` → `{ slug, updatedAt }[]` của mọi công thức Published, cache 1 giờ.
  8. **API — 4 endpoint mới:** `GET /recipes/search`, `GET /recipes/mine`, `GET /recipes/mine/{id}`, `GET /recipes/sitemap`. Thêm 3 endpoint sửa hợp đồng. Mọi `GET` của `/recipes` gom vào `RecipeQueryEndpoints`.
- **Vì sao:**
  - **Vá MT-34 sớm hai buổi**: endpoint còn rò dữ liệu thì chưa thể gọi là "cài xong"; và vì `/recipes/mine` làm cùng buổi nên Author không mất đường xem bản nháp.
  - **Tiến giữ toàn bộ phía đọc**: bốn truy vấn dùng chung một bộ lọc. Không còn cảnh "sửa `GetRecipesQuery` của Thọ" (Buổi 5 cũ) hay "chờ Thọ làm endpoint sitemap" (Buổi 7 cũ).
  - **`/recipes/mine/{id}` dưới tiền tố `/mine`, trả 404 cho người khác**: mọi endpoint riêng tư nhận ra được ngay từ URL; không dò được id. Cho `/recipes/{slug}` trả bản nháp sẽ mở lại MT-34.
  - **Generated column thay vì trigger**; **`simple` + `unaccent`** vì PostgreSQL không có sẵn cấu hình tiếng Việt; **D-18** vì `init.sql` không chạy trong Testcontainers.
- **Xong khi:**
  - Guest không thấy bản nháp dù Admin vừa gọi cùng URL;
  - gõ "pho" ra "Phở bò";
  - `sort=-title` → 400;
  - `/recipes/mine` có `no-store`; `/recipes/mine/{id}` của người khác → 404.
- **Commit:** `fix(search)!: close MT-34 draft leak; complete FR-SRCH-001..004 query APIs, FR-RCP-011 private my-recipes and sitemap source`

### Dev 4 – Nguyễn Thăng Thiêng · API vòng đời công thức (FR-RCP-005, 006, 007) + FR-JOB-002, FR-JOB-003 (+ D-3, D-15, test bề mặt API)
- **Hướng đi:**
  1. **Máy trạng thái `Recipe.Lifecycle.cs`:**
     - bảng `RecipeStatusTransitions`: Draft ⇄ Published, (Draft | Published) → Archived, Archived → **Draft**;
     - ngoài bảng → 409 `RECIPE_INVALID_STATE_TRANSITION`;
     - **D-15:** publish chỉ từ Draft; thiếu bước/nguyên liệu → `RecipePublishIncompleteException` 400; `PublishedAt` chỉ gán lần đầu;
     - thêm `SoftDelete()`.
  2. **5 command** publish / unpublish / archive / unarchive / delete:
     - nạp đủ bước + nguyên liệu, qua `RecipeAuthorizationHandler`;
     - xóa cache danh sách, chi tiết, danh mục và sitemap;
     - xóa mềm **không** xóa con và **không** xóa file.
  3. **D-3 (kéo từ Buổi 6):** migration `B4_Recipe_PartialUniqueSlug` — slug unique **chỉ trên công thức chưa xóa**; bỏ `IgnoreQueryFilters()` ở `SlugExistsAsync`.
  4. **API — 5 endpoint mới** trong `RecipeLifecycleEndpoints`: `PATCH /recipes/{id}/publish`, `/unpublish`, `/archive`, `/unarchive`, `DELETE /recipes/{id}`.
  5. **`ImageResizeJob`:** medium 800×600 + thumbnail 300×300, retry 3 lần, lỗi thì giữ ảnh gốc; cắm vào upload ảnh của Thọ.
  6. **`PermanentPurgeJob`** (03:30 UTC):
     - xóa cứng công thức đã xóa mềm > 30 ngày;
     - **commit DB trước, xóa file MinIO sau**;
     - khóa phân tán RedLock.
  7. **Test:**
     - ma trận **12 tổ hợp** trạng thái × hành động;
     - xóa rồi tạo lại cùng tên được slug cũ;
     - job dọn đúng bản ghi và file;
     - **`ApiSurfaceTests`**: đọc route thật của ứng dụng, so với **45 endpoint** của SRS — thiếu hoặc thừa là đỏ.
- **Vì sao:**
  - **Vòng đời trọn vẹn trong tay một người**: trạng thái → lưu trữ → xóa mềm → dọn sau 30 ngày là một chuỗi liền mạch. Tách cho hai người là tạo lại đúng loại khe hở đã phải sửa trong SRS (Archived vào được không ra được; xóa mềm không ai dọn).
  - **Cân bằng tải**: Thọ giữ 8 endpoint nội dung, không phải gánh 15.
  - **`partial class` + 3 file endpoint**: ba người cùng sửa module Recipe trong một buổi mà không xung đột merge.
  - **`ApiSurfaceTests`**: biến "đã cài tất cả API" thành test chạy ở mọi commit tới Buổi 8. Chiều "thừa" chặn endpoint ngoài SRS.
  - **Commit DB trước, xóa file sau**: sai thứ tự thì có thể mất dữ liệu vĩnh viễn; đúng thứ tự thì tệ nhất chỉ còn file rác.
- **Xong khi:**
  - ma trận 12 tổ hợp xanh;
  - publish công thức thiếu nguyên liệu → 400; publish công thức Archived → 409;
  - `ApiSurfaceTests` khớp 45/45;
  - purge job dọn đúng DB **và** MinIO.
- **Commit:** `feat(recipes,jobs): complete FR-RCP-005/006/007 lifecycle APIs with closed state machine, FR-JOB-002 image resize & FR-JOB-003 purge job, api surface test 45/45`

**Kiểm chứng cuối Buổi 4:**
- `ApiSurfaceTests` xanh — **45/45 endpoint**, không thiếu, không thừa.
- Mọi endpoint có ≥ 1 test thành công + ≥ 1 test lỗi; mọi lỗi là `application/problem+json` đúng mã Phụ lục B.
- Test MT-34 xanh, không còn `CacheOutput` trong code.
- Tìm "pho" ra "Phở bò"; kéo-thả bước không lỗi 23505.
- Dùng lại refresh token cũ → cả họ token bị thu hồi.
- Scalar hiển thị đủ 45 endpoint.

---

## BUỔI 5 – Giao diện: phiên & hồ sơ, soạn/sửa/xuất bản công thức, tìm kiếm & bộ lọc; index & health check

**Mục tiêu:** toàn bộ API đã xong ở Buổi 4 — buổi này dựng **giao diện** trên các API đó:
- người dùng giữ phiên an toàn và sửa được hồ sơ;
- Author soạn trọn công thức (nguyên liệu, các bước), sửa an toàn khi nhiều người cùng sửa, xuất bản có checklist;
- người đọc tìm kiếm, lọc, sắp xếp được;
- truy vấn danh sách được chứng minh dùng index; hệ thống tự báo sức khỏe.

**Tiến trình trong buổi:**
1. Các phần việc độc lập — API đã có sẵn và có test từ Buổi 4.
2. Thọ dùng API của cả Tiến (`/recipes/mine/{id}`) và Thiêng (publish); **lệch hợp đồng thì người làm API sửa**, Thọ không sửa handler của người khác.
3. Cuối buổi merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · Giao diện hồ sơ + phiên đăng nhập phía FE (D-12)
- **Hướng đi:**
  1. **API đã xong ở Buổi 4:** `GET/PATCH /auth/me`, `POST /auth/refresh`.
  2. **D-12 + single-flight (chuyển từ Buổi 4):**
     - access token **chỉ giữ trong bộ nhớ**, chỉ refresh token được lưu; tải lại trang thì gọi refresh một lần;
     - `api-client` gặp 401 → gọi refresh **đúng một lần dù nhiều request song song** rồi gửi lại;
     - refresh lỗi → xóa phiên, về trang đăng nhập.
  3. **UI `/profile`:**
     - form + `ImageUploader` upload qua `POST /files/upload` → PATCH `/auth/me`;
     - cập nhật avatar trên header ngay (optimistic, lỗi thì hoàn tác);
     - **xóa avatar cũ sau khi PATCH thành công**.
- **Vì sao:**
  - `/auth/me` là **cách duy nhất FE biết `roles`**; không tự giải mã JWT vì quyền bị thu hồi sẽ còn hiệu lực tới 15 phút.
  - **Single-flight**: 5 request cùng nhận 401 mà mỗi cái tự refresh sẽ kích hoạt nhầm cơ chế phát hiện dùng lại → bị đăng xuất vô cớ.
  - **Access token trong bộ nhớ** giảm thứ XSS lấy được.
  - **PATCH từng phần** để đổi avatar không vô tình ghi đè `bio`.
- **Xong khi:**
  - 5 request song song khi token hết hạn → đúng 1 lần refresh;
  - `localStorage` không còn access token;
  - đổi avatar thấy ngay trên header.
- **Commit:** `feat(auth-ui): in-memory access token with single-flight refresh (D-12) and profile page with validated avatar upload`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Giao diện soạn & sửa công thức: nguyên liệu, các bước, trang sửa, xuất bản
- **Hướng đi:**
  1. **API đã xong ở Buổi 4:**
     - nguyên liệu, các bước, `PUT` — Thọ;
     - `GET /recipes/mine/{id}` — Tiến;
     - publish/unpublish — Thiêng.
  2. **Wizard bước 3 "Nguyên liệu" (chuyển từ Buổi 4):** mỗi dòng có ô số và ô nguyên văn; lỗi gắn đúng dòng.
  3. **Wizard bước 4 "Các bước" (chuyển từ Buổi 4):** kéo-thả bằng `dnd-kit` → gọi reorder; ảnh minh họa; hẹn giờ; **không gửi `stepNumber`**.
  4. **Trang `/dashboard/recipes/[id]/edit`:**
     - nạp bằng **`GET /recipes/mine/{id}`**, gửi kèm `rowVersion`;
     - gặp 409 → hộp thoại "Dữ liệu đã được người khác cập nhật – Tải lại";
     - công thức đã từng xuất bản thì ghi chú "đổi tiêu đề không đổi đường dẫn".
  5. **Nút Xuất bản** có **checklist điều kiện** (✓ N bước, ✗ chưa có nguyên liệu); vẫn xử lý 400/409 từ server.
- **Vì sao:**
  - **Optimistic concurrency thay vì khóa bản ghi**: blog hiếm khi xung đột, chỉ cần phát hiện lúc lưu.
  - **`PublishedAt` chỉ gán lần đầu**: nếu ghi đè, bài cũ sẽ nhảy lên đầu trang chủ.
  - **Khóa slug sau khi xuất bản**: không có bảng slug cũ để chuyển hướng.
  - **Checklist trước khi bấm** tốt hơn báo lỗi sau khi bấm (server vẫn kiểm tra lại).
- **Xong khi:**
  - hai tab sửa cùng lúc → tab sau nhận 409;
  - kéo-thả bước, F5 vẫn đúng thứ tự;
  - thiếu nguyên liệu → nút xuất bản bị khóa kèm lý do.
- **Commit:** `feat(recipes-ui): ingredient & step wizard with drag-and-drop reorder, edit page with 409 handling and publish checklist`

### Dev 3 – Đoàn Hồng Tiến · Giao diện tìm kiếm & bộ lọc (+ D-10 phía FE) + composite index
- **Hướng đi:**
  1. **API đã xong ở Buổi 4:** `RecipeFilterSpec`, `SortMapper`, phân trang, search.
  2. **Index:** migration `B5_Search_CompositeIndexes` tạo 3 composite index; đo `EXPLAIN ANALYZE` trên **≥ 10.000 công thức** (`PerformanceSeeder` bật bằng cờ); ghi kết quả `docs/explain-analyze-b5.md`.
  3. **UI tìm kiếm (chuyển từ Buổi 4):** `SearchBar` trên header (debounce 300ms); trang `/search` (SSR) tô sáng từ khóa, có trạng thái rỗng kèm gợi ý.
  4. **UI bộ lọc:**
     - `FilterPanel`, `SortSelect` (cột + chiều), `Pagination`;
     - **toàn bộ trạng thái nằm trên URL**;
     - **D-10 phía FE:** bỏ `sort=-createdAt`, chuyển sang `sortBy`/`sortOrder`;
     - dùng chung cho `/recipes`, `/search`, `/categories/[slug]`; mobile dùng drawer.
- **Vì sao:**
  - **`sortBy` + `sortOrder`**: dấu `-` dễ mất khi quên encode URL → sắp sai mà không báo lỗi.
  - **Whitelist là rào chắn bảo mật**: tên cột không tham số hóa được trong SQL.
  - **Trạng thái trên URL**: chia sẻ link được, back/forward đúng, SSR đọc được ngay.
  - **Đo trên 10.000 bản ghi**: với 100 bản ghi PostgreSQL luôn quét tuần tự, đo không chứng minh được gì.
- **Xong khi:**
  - tìm "pho" trên giao diện ra "Phở bò";
  - F5 và back/forward giữ nguyên bộ lọc;
  - `EXPLAIN ANALYZE` cho Index Scan.
- **Commit:** `feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze`

### Dev 1 – Hoàng Bình Quân · FR-AUTH-006 Xem hồ sơ + FR-AUTH-007 Cập nhật hồ sơ
- **Hướng đi:**
  1. **`GetCurrentUserQuery`:** lấy `UserId` từ `ICurrentUser`, trả `UserDto`; **không** trả `PasswordHash`, `SecurityStamp`, `UserName`, `emailConfirmed`. User không còn tồn tại → **401**.
  2. **`UpdateProfileCommand` (PATCH từng phần):** `displayName` 2–100, `bio` ≤ 1000, `avatarUrl` ≤ 500 **và phải thuộc bucket MinIO của hệ thống**; không cho đổi email, `UserName`.
  3. **API:** `GET /auth/me`, `PATCH /auth/me`.
  4. **UI `/profile`:** form + `ImageUploader` upload qua `POST /files/upload` → PATCH `/auth/me`; cập nhật avatar trên header ngay (optimistic, lỗi thì hoàn tác); **xóa avatar cũ sau khi PATCH thành công**.
  5. **Test:** JSON `/auth/me` không có trường nhạy cảm; `avatarUrl` domain ngoài → 400; user đã bị xóa → 401.
- **Vì sao:**
  - `/auth/me` là **cách duy nhất FE biết `roles`**; không cho FE tự giải mã JWT vì quyền bị thu hồi sẽ còn hiệu lực tới 15 phút.
  - **PATCH từng phần** để đổi avatar không vô tình ghi đè `bio`.
  - **Chỉ nhận ảnh trong bucket của hệ thống** để không thành nơi nhúng nội dung tùy ý.
  - Đổi email là luồng nhạy cảm riêng (xác minh, liên kết Google) nên tách khỏi form hồ sơ.
- **Xong khi:** xem và sửa được hồ sơ, đổi avatar thấy ngay trên header; test xanh.
- **Commit:** `feat(auth): complete FR-AUTH-006 & FR-AUTH-007 profile management with validated avatar upload`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-004 Cập nhật (RowVersion) + FR-RCP-005 Xuất bản (+ D-15)
- **Hướng đi:**
  1. Thêm mã lỗi: `RECIPE_PUBLISH_INCOMPLETE` → 400, `RECIPE_INVALID_STATE_TRANSITION` → 409.
  2. **Chống ghi đè đồng thời:** DTO chi tiết trả `rowVersion` + header `ETag`; `UpdateRecipeCommand` nhận `rowVersion` (body hoặc `If-Match`), gán làm giá trị gốc trước khi lưu; `DbUpdateConcurrencyException` → **409 `RECIPE_CONCURRENCY_CONFLICT`**.
  3. **Update:** qua handler phân quyền (403); **slug chỉ sinh lại khi còn Draft**, đã từng Published thì khóa vĩnh viễn.
  4. **D-15 trên `Recipe.Publish()`:** chỉ Draft mới publish được (Archived hay Published → 409); thiếu bước/nguyên liệu → 400; giữ `PublishedAt ??= now`. Thêm `Unpublish()` (Published → Draft). Handler phải `Include` Steps + Ingredients trước khi gọi.
  5. **API + UI:** `PUT /recipes/{id}`, `PATCH /recipes/{id}/publish`, `…/unpublish`; trang `/dashboard/recipes/[id]/edit` dùng lại wizard, gặp 409 hiện hộp thoại "Dữ liệu đã được người khác cập nhật – Tải lại"; nút Xuất bản có **checklist điều kiện** (✓ N bước, ✗ chưa có nguyên liệu).
- **Vì sao:**
  - **Optimistic concurrency thay vì khóa bản ghi**: khóa sẽ giữ transaction suốt lúc người dùng điền form (vài phút) → nghẽn kết nối; blog hiếm khi xung đột nên chỉ cần phát hiện lúc lưu.
  - **`PublishedAt` chỉ gán lần đầu**: nếu ghi đè, hủy xuất bản rồi xuất bản lại sẽ làm bài cũ nhảy lên đầu trang chủ và sai `datePublished` trong JSON-LD.
  - **Khóa slug sau khi xuất bản**: hệ thống không có bảng lưu slug cũ để chuyển hướng, đổi slug là chết link.
  - **Checklist trước khi bấm** tốt hơn báo lỗi sau khi bấm (server vẫn kiểm tra lại).
- **Xong khi:** hai người sửa cùng lúc thì người sau nhận 409; publish công thức Archived → 409; thiếu nguyên liệu → 400.
- **Commit:** `feat(recipes): complete FR-RCP-004 rowversion concurrency & FR-RCP-005 publish with state-guarded transitions`

### Dev 3 – Đoàn Hồng Tiến · FR-SRCH-002/003/004 Lọc, sắp xếp, phân trang (+ D-10)
- **Hướng đi:**
  1. **`RecipeFilterSpec` dùng chung** cho `GetRecipesQuery`, `SearchRecipesQuery`, `GetCategoryBySlugQuery`: `categoryId`, `difficulty` (Easy/Medium/Hard/Expert, ngoài enum → 400), `maxCookTime`, `maxPrepTime`, `minServings` – kết hợp AND.
  2. **`SortMapper`** – dictionary whitelist 5 trường `createdAt, publishedAt, title, cookTime, prepTime`; ngoài whitelist → **400**; `sortOrder ∈ {asc, desc}`; mặc định `createdAt desc`. **D-10:** xóa `RecipeSortParser` và tham số `sort=-field`.
  3. **Phân trang:** `page` ≥ 1, `pageSize` mặc định 12, tối đa 50 → sai 400.
  4. **Index:** migration `B5_Search_CompositeIndexes` tạo 3 composite index; đo `EXPLAIN ANALYZE` trên **≥ 10.000 công thức** (`PerformanceSeeder` bật bằng cờ); ghi kết quả `docs/explain-analyze-b5.md`.
  5. **UI:** `FilterPanel`, `SortSelect` (cột + chiều), `Pagination`; **toàn bộ trạng thái nằm trên URL**; dùng chung cho `/recipes`, `/search`, `/categories/[slug]`; mobile dùng drawer.
- **Vì sao:**
  - **`sortBy` + `sortOrder`** dễ validate từng tham số; dấu `-` dễ mất khi quên encode URL → sắp sai thứ tự mà không báo lỗi.
  - **Whitelist là rào chắn bảo mật**: tên cột không tham số hóa được trong SQL, ghép chuỗi là SQL injection.
  - **Báo 400 thay vì im lặng dùng mặc định** để FE gửi sai biết ngay.
  - **Một spec dùng chung** để 3 màn hình không có 3 bộ lọc hơi khác nhau.
  - **Đo trên 10.000 bản ghi** vì với 50 bản ghi PostgreSQL luôn quét tuần tự, đo không chứng minh được gì.
  - `pageSize = 12` chia hết cho lưới 2, 3, 4 cột.
- **Xong khi:** `?sort=-title` → 400, `?sortBy=title&sortOrder=desc` → 200; `EXPLAIN ANALYZE` cho Index Scan; F5 và back/forward giữ nguyên bộ lọc.
- **Commit:** `feat(search): complete FR-SRCH-002/003/004 sortby-sortorder whitelist, composite indexes and url-synced pagination`

### Dev 4 – Nguyễn Thăng Thiêng · FR-OBS-001 Health check (hoàn tất)
- **Hướng đi:**
  1. **Check:** đã có từ Buổi 3 — `AspNetCore.HealthChecks.NpgSql`, `.Redis` (gắn tag `ready`); `MinioHealthCheck` tự viết (không gắn `ready`).
  2. **Endpoint:** đã có từ Buổi 3 — `/health` (tất cả, 503 khi lỗi), `/health/live` (luôn 200 khi process sống), `/health/ready` (chỉ check tag `ready`).
  3. **Docker:** thêm healthcheck cho `api` (`curl /health/ready` – cài `curl` trong Dockerfile vì image aspnet không có) và `frontend` (`wget`); Nginx `depends_on: api healthy` và `proxy_next_upstream error timeout http_502 http_503 http_504`.
  4. **UI:** `HealthIndicator` (chấm xanh/vàng/đỏ, cập nhật 30 giây) trên dashboard Admin; trang `/dashboard/system`.
  5. **Kiểm chứng:** tắt Redis → `/health/ready` 503, container `api` unhealthy, `/health/live` vẫn 200; bật lại tự hồi phục.
  6. **FE (chuyển từ Buổi 4):** `RecipeCard` dùng `ThumbnailUrl` của job resize (chưa có thì dùng ảnh gốc).
- **Vì sao:**
  - **Live và Ready tách riêng**: Redis chết không được làm container API bị restart liên tục – restart không sửa được Redis.
  - **MinIO không gắn `ready`**: mất MinIO chỉ không tải được ảnh, công thức vẫn đọc được – là suy giảm, không phải sập.
  - **Khối `healthcheck:` bắt buộc**: Docker Compose không tự gọi `/health/ready`; thiếu nó thì 3 endpoint chỉ là URL đẹp.
- **Xong khi:** kịch bản tắt/bật Redis ở bước 5 cho đúng kết quả.
- **Commit:** `feat(obs): complete FR-OBS-001 docker healthcheck wiring, nginx failover and health ui indicator`

**Kiểm chứng cuối Buổi 5:**
- Index Scan trên ≥ 10.000 bản ghi.
- Sửa đồng thời → hộp thoại 409; wizard soạn trọn công thức.
- Refresh single-flight hoạt động.
- Tắt Redis → ready 503, live 200.

---

## BUỔI 6 – Gia cố bảo mật, dashboard "công thức của tôi" & xóa mềm (giao diện), SEO, log có cấu trúc

**Mục tiêu:** khóa chặt bề mặt tấn công; Author quản lý công thức của mình trên dashboard riêng tư; trang công khai đạt chuẩn SEO; hệ thống quan sát được từ bên ngoài.
> Lỗ hổng rò rỉ bản nháp (D-4, D-5, D-6) và D-3 **đã vá ở Buổi 4** — sớm hơn hai buổi. Buổi này chỉ còn D-7 phía FE.

**Tiến trình trong buổi:** các phần việc độc lập; cuối buổi merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · Rate limit, phân quyền, CSP, chặn route
- **Hướng đi:**
  1. **Rate limit** (`AddRateLimiter`): `auth` sliding window 10 req/phút/IP, `upload` 5 req/phút/IP, chung 100 req/phút/IP; bị chặn → 429 + `Retry-After` + `RATE_LIMIT_EXCEEDED` + header `X-RateLimit-*`. Thứ tự pipeline: `UseForwardedHeaders` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization` (có test kiến trúc giữ thứ tự).
  2. Ghi rõ trong `nginx.conf`: ngưỡng Nginx ≥ ngưỡng ứng dụng.
  3. **Rà soát phân quyền:** mọi endpoint ghi có `AuthorPolicy`/`AdminPolicy`; không còn `VerifiedAuthor`; kiểm tra `IsActive` ở cả login lẫn refresh.
  4. **Header bảo mật:** CSP chặt (`script-src 'self'` + nonce, `object-src 'none'`, `frame-ancestors 'none'`), `nosniff`, `Referrer-Policy`; rà CORS không có `*`.
  5. **Chặn route FE** (`middleware.ts`): `/dashboard/**`, `/profile` cần đăng nhập; `/dashboard/categories`, `/dashboard/users` chỉ Admin; component `<RequireRole>`. Test: request đăng nhập thứ 11 → 429; Author gọi API Admin → 403.
- **Vì sao:**
  - **Rate limit phải dựa trên IP thật** (bật ở Buổi 4): nếu không, mọi người dùng chung IP của Nginx → 10 req/phút cho *cả hệ thống*.
  - **Chỉ tin `X-Forwarded-For` từ mạng nội bộ**: header này giả mạo được, tin bừa thì né được rate limit.
  - **Sliding window cho `/auth`** chặn kiểu bắn 10 request cuối cửa sổ + 10 request đầu cửa sổ sau.
  - **CSP là lớp phòng vệ cuối cho refresh token**; chặn route FE chỉ là trải nghiệm, quyền thật kiểm ở backend.
- **Xong khi:** request thứ 11 tới `/auth/login` trong 1 phút → 429; Author → 403 ở mọi API Admin; trang dashboard không vào được khi chưa đăng nhập.
- **Commit:** `feat(security): ip-based rate limiting, content security policy, authorization audit and frontend route guards`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Giao diện "Công thức của tôi" + xóa mềm
- **Hướng đi:**
  1. **API đã xong ở Buổi 4:** `GET /recipes/mine` (Tiến); `DELETE /recipes/{id}`, publish/unpublish (Thiêng).
  2. **UI `/dashboard/recipes`:**
     - gọi `/recipes/mine` (không bao giờ gọi `/recipes`);
     - tab Draft/Published/Archived;
     - nút Sửa/Xuất bản/Hủy/Xóa — xóa phải xác nhận bằng cách gõ tên công thức;
     - optimistic update + hoàn tác khi lỗi;
     - không dùng chung cache TanStack Query giữa `/recipes/mine` và `/recipes`.
  3. **UI `/dashboard`:** thống kê theo trạng thái — đọc `totalCount` của `/recipes/mine?status=X&pageSize=1`, không cần API thống kê riêng.
- **Vì sao:**
  - **Dữ liệu riêng đi đường riêng và cấm cache**, kể cả cache phía trình duyệt: cache nằm trước bước kiểm tra quyền.
  - **Xác nhận xóa bằng cách gõ tên**: xóa mềm vẫn làm công thức biến mất khỏi trang công khai ngay.
- **Xong khi:**
  - xóa công thức → biến mất khỏi dashboard và `/recipes`;
  - tạo lại cùng tên vẫn được slug cũ;
  - `/recipes/mine` có `no-store`.
- **Commit:** `feat(recipes-ui): my-recipes dashboard with status tabs, confirmed soft delete and per-status stats`

### Dev 3 – Đoàn Hồng Tiến · Trang chủ, ISR, JSON-LD, Open Graph (+ D-7)
- **Hướng đi:**
  1. **D-5 đã xong ở Buổi 4** (trang danh mục chỉ còn công thức Published).
  2. **D-7:** `revalidate` của `fetch` trên `/categories` 3600 → **1800**, `/categories/[slug]` 600 → **120**; `/recipes/[slug]` giữ 300; trang chủ 120.
  3. **Trang chủ `/`:** hero + "Công thức mới xuất bản" (`sortBy=publishedAt&sortOrder=desc&pageSize=8`) + lưới danh mục; `next/image` có `sizes`, `priority` cho ảnh LCP.
  4. **JSON-LD** (`lib/seo/recipeJsonLd.ts`): `@type: Recipe` với name, description, image, author, `datePublished`, thời gian dạng ISO 8601 (`PT15M`), `recipeYield`, `recipeIngredient[]`, `recipeInstructions[]` dạng `HowToStep`, `nutrition`. **Không** nhúng `aggregateRating`.
  5. **Metadata:** `generateMetadata()` – `<title>` "{Tên} | Culinary Blog" cắt ở 60 ký tự khi render, description 160 ký tự đầu, Open Graph 1200×630, Twitter card, canonical. Draft trả 404 nên không bị lập chỉ mục; dashboard đặt `noindex`. Kiểm bằng Google Rich Results Test.
- **Vì sao:**
  - **Giới hạn SEO là quy tắc hiển thị, không phải ràng buộc dữ liệu**: không bắt người dùng đặt tên món ≤ 44 ký tự.
  - **Thời gian tái sinh trang ≤ TTL cache API**: nếu dài hơn, Next.js thành tầng cache cũ nhất và mọi invalidate ở backend vô nghĩa.
  - **"Nổi bật" = mới xuất bản nhất** vì không có cột lượt xem/đánh giá; đếm lượt xem sẽ ghi DB mỗi lượt truy cập.
  - **Không nhúng rating giả** – vi phạm chính sách structured data của Google.
- **Xong khi:** Rich Results Test pass loại Recipe; trang danh mục không còn hiện Draft; `<title>` thực tế ≤ 60 ký tự.
- **Commit:** `feat(seo): homepage and category isr aligned with cache ttl, json-ld recipe schema and open graph metadata`

### Dev 4 – Nguyễn Thăng Thiêng · FR-OBS-002 Log có cấu trúc + FR-OBS-003 Tracing
- **Hướng đi:**
  1. **Serilog** cho cả `api` và `hangfire`: Console (JSON), File (xoay theo ngày), Seq; `UseSerilogRequestLogging()`.
  2. **CorrelationId:** `CorrelationIdMiddleware` đọc `X-Correlation-ID` Nginx đã gửi (tự sinh nếu gọi thẳng), trả lại trong response, đẩy `CorrelationId`, `RequestPath`, `UserId` vào mọi dòng log; truyền sang job Hangfire.
  3. **`LoggingBehavior`** cảnh báo request > 500ms, ghi audit mọi command ghi; **`DbCommandInterceptor`** cảnh báo query > 100ms.
  4. **Lọc bí mật:** loại `password`, `refreshToken`, `idToken`, header `Authorization` khỏi mọi sink.
  5. **OpenTelemetry:** trace AspNetCore/HttpClient/EF, metrics `recipes_created_total`, `recipes_published_total`, thời gian request, tỉ lệ lỗi → Seq; FE gửi `X-Correlation-ID` và hiện mã này trên toast lỗi.
- **Vì sao:**
  - **CorrelationId biến log thành công cụ điều tra**: người dùng đọc mã trên toast, dev dán vào Seq là ra toàn bộ chuỗi log của request đó.
  - **Log JSON truy vấn được** (`UserId = x and Elapsed > 500`).
  - **Hai ngưỡng hai chỗ**: behavior chỉ thấy tổng thời gian handler, không thấy từng câu SQL.
  - **Lọc bí mật khỏi log** – nếu không, công sức hash token ở DB thành vô ích vì log lưu lâu hơn và nhiều người đọc hơn DB.
- **Xong khi:** mọi dòng log trên Seq có 3 trường bắt buộc; grep log không thấy token/mật khẩu; xem được trace một request xuyên các tầng.
- **Commit:** `feat(obs): complete FR-OBS-002 serilog correlation logging & FR-OBS-003 opentelemetry with secret redaction`

**Kiểm chứng cuối Buổi 6:**
- Dashboard "công thức của tôi" chạy.
- Request thứ 11 tới `/auth/login` → 429.
- Rich Results Test pass.
- Log đủ trường, không lộ bí mật.

---

## BUỔI 7 – Quản lý người dùng, khép kín máy trạng thái, sitemap, hạ tầng production

**Mục tiêu:** xong giao diện của 3 FR cuối (`FR-AUTH-008`, `FR-AUTH-009`, `FR-RCP-006` — API đã có từ Buổi 4) → **37/37 FR**; sitemap đúng chỗ; hạ tầng sẵn sàng cho load test.

**Tiến trình trong buổi:**
1. Thiêng làm Nginx `/media/` và **merge trước**; Tiến gỡ `unoptimized` rồi kiểm chứng sau khi rebase.
2. `GET /recipes/sitemap` đã có từ Buổi 4 (Tiến tự làm) — không còn phải chờ ai.
3. Cuối buổi merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · Giao diện Admin quản lý tài khoản + quản lý phiên
- **Hướng đi:**
  1. **API đã xong ở Buổi 4:** `GET /users`, `PATCH /users/{id}/status`, `/auth/sessions` ×3, claim `sid`.
  2. **UI `/dashboard/users` (Admin):**
     - bảng, tìm kiếm, lọc trạng thái;
     - khóa/mở khóa **bắt buộc nhập lý do**;
     - ẩn nút khóa ở dòng của chính Admin (server vẫn chặn 403).
  3. **UI `/profile` — tab "Phiên đăng nhập":**
     - IP + thời gian, đánh dấu "Thiết bị này" theo `isCurrent`;
     - đăng xuất từng thiết bị / tất cả.
- **Vì sao:**
  - **Khóa phải thu hồi token** (API làm ở Buổi 4), không chỉ gán cờ.
  - Access token cũ còn sống tối đa 15 phút là **đánh đổi chấp nhận được** của JWT stateless.
  - **Chặn Admin tự khóa** để hệ thống không rơi vào cảnh không còn ai mở khóa được.
  - **Nhận diện phiên hiện tại bằng `sid`**: client không phải gửi thêm dữ liệu nhạy cảm.
- **Xong khi:**
  - Admin khóa Author → phiên của Author hết đường gia hạn;
  - tab phiên đánh dấu đúng thiết bị hiện tại.
- **Commit:** `feat(auth-ui): admin user management page and profile sessions tab for FR-AUTH-008/009`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Giao diện lưu trữ / khôi phục + nút theo máy trạng thái
- **Hướng đi:**
  1. **API đã xong ở Buổi 4 (Thiêng):** archive/unarchive, bảng `RecipeStatusTransitions`, test 12 tổ hợp.

     | Từ | Hành động | Sang |
     |---|---|---|
     | Draft | publish | Published |
     | Published | unpublish | Draft |
     | Draft hoặc Published | archive | Archived |
     | Archived | unarchive | Draft |

  2. **Hằng số FE** `statusTransitions.ts` sao đúng bảng trên, kèm test đối chiếu với lỗi 409 của API.
  3. **UI:**
     - nút "Lưu trữ" và "Khôi phục về nháp"; badge màu theo trạng thái;
     - **chỉ hiện nút hợp lệ**;
     - gặp 409 thì giải thích và tải lại.
- **Vì sao:**
  - **Unarchive về Draft** buộc tác giả rà lại nội dung trước khi công khai.
  - **Nút sinh từ cùng bảng với server**: không bao giờ hiện một nút mà bấm vào chắc chắn lỗi.
- **Xong khi:**
  - archive → công thức rời trang công khai, hiện ở tab Archived;
  - khôi phục → về Draft.
- **Commit:** `feat(recipes-ui): archive/unarchive actions and state-aware buttons generated from the shared transition table`

### Dev 3 – Đoàn Hồng Tiến · Sitemap, robots.txt, tối ưu ảnh (+ D-14 phía Next.js)
- **Hướng đi:**
  1. **`app/sitemap.ts`:** toàn bộ công thức Published + danh mục + trang tĩnh; `revalidate = 3600`.
  2. **Nguồn dữ liệu** `lib/seo/sitemapSource.ts` gọi `GET /recipes/sitemap` (Tiến đã làm ở Buổi 4, một request), không duyệt `GET /recipes` theo trang.
  3. **`app/robots.ts`:** cho phép `/`, chặn `/dashboard/`, `/profile`, `/api/`, `/hangfire`, khai báo sitemap. **Không ping Google.**
  4. **D-14:** sau khi Thiêng có `/media/`, đổi URL ảnh sang `https://<domain>/media`, **gỡ `unoptimized`**, khai báo `remotePatterns`; rà mọi `<Image>` (`sizes`, `priority`), lưới danh sách dùng thumbnail/medium.
  5. **Kiểm chứng:** `/sitemap.xml`, `/robots.txt` trả ở **domain chính**; Lighthouse: LCP ≤ 2.5s, CLS ≤ 0.1, INP ≤ 200ms, JS ≤ 200KB.
- **Vì sao:**
  - **Sitemap phải nằm nơi crawler tìm** – domain chính do Next.js phục vụ; sinh ở backend thì nằm sai địa chỉ, mất khi deploy, lệch giữa các instance.
  - **Bỏ ping Google** vì Google đã ngừng hỗ trợ từ 06/2023.
  - **Dùng thumbnail ở lưới**: 12 ảnh gốc 5MB = 60MB, 12 thumbnail chỉ vài trăm KB.
- **Xong khi:** sitemap không chứa Draft/Archived; ảnh được tối ưu; Lighthouse đạt chỉ tiêu.
- **Commit:** `feat(seo): nextjs sitemap and robots routes, drop deprecated google ping, re-enable next/image optimization`

### Dev 4 – Nguyễn Thăng Thiêng · Cache, Nginx production, SSL, scale (+ D-14 Nginx, D-17, audit D-6)
- **Hướng đi:**
  1. **Audit cache:** lập bảng mọi command ↔ khóa cache phải xóa, đối chiếu bảng TTL chuẩn; thêm `RemoveByPrefixAsync` dùng **`SCAN`** (không dùng `KEYS`), nhớ tiền tố `culinaryblog:`; xác nhận không còn Output Cache, không khóa riêng tư nào vào cache.
  2. **`docker-compose.prod.yml`:** bỏ `ports` của `api`, `replicas: 3`; `hangfire` 1 replica; bỏ `seq`, `mailhog`. File dev giữ nguyên cổng.
  3. **Nginx production:** giữ **resolver động** (không dùng khối `upstream` – sẽ gây lại lỗi 502); giữ `proxy_next_upstream`, `X-Forwarded-*`, `X-Correlation-ID`, `/hangfire`; thêm gzip, cache `_next/static` 1 năm `immutable`.
  4. **D-14 – `/media/`:** Nginx proxy `/media/` → MinIO bucket, cache 30 ngày; ảnh có **một địa chỉ công khai duy nhất**; đóng cổng 9000 ở production.
  5. **SSL + D-17:** HTTP → HTTPS, HSTS 1 năm, TLS 1.2+; `ProtectKeysWithCertificate` cho khóa DataProtection (dùng chung chứng chỉ, không commit). Đo **cache hit rate ≥ 80%** bằng `redis-cli INFO stats`, ghi `docs/cache-report.md`.
- **Vì sao:**
  - **`SCAN` thay `KEYS`**: `KEYS` khóa toàn bộ Redis khi quét – chạy tốt lúc dev, sập khi có tải.
  - **Bỏ `ports` là điều kiện để scale**: 3 container không thể cùng chiếm cổng 5000 của máy.
  - **Resolver động** tự nhận replica mới trong 10 giây; khối `upstream` chỉ phân giải DNS một lần (đã gây 502 ở Buổi 2).
  - **Một địa chỉ ảnh qua Nginx** xóa bỏ việc `localhost:9000` đúng với trình duyệt nhưng sai với container.
  - **D-17 cùng buổi SSL** vì cùng cần chứng chỉ.
- **Xong khi:** `docker compose -f docker-compose.prod.yml up --scale api=3` chạy; tạo lại một container `api` không gây 502; ảnh tải qua `/media/`; hit rate được đo và ghi lại.
- **Commit:** `chore(devops): production nginx with ssl and /media proxy, prod compose scaling, dataprotection keys and cache audit`

**Kiểm chứng cuối Buổi 7:**
- 37/37 FR có cả API lẫn giao diện.
- Nút trạng thái khớp bảng chuyển trạng thái.
- Khóa user thu hồi phiên.
- Sitemap ở domain chính.
- `--scale api=3` không 502.

---

## BUỔI 8 – Kiểm thử toàn diện, WCAG, load test, đóng gói & bảo vệ

**Mục tiêu:** **không viết tính năng mới** – chứng minh hệ thống đạt mọi chỉ tiêu trong SRS và đóng gói để bảo vệ.
**Điều kiện đầu vào:** 37/37 FR xong, 17/17 nợ kỹ thuật cần sửa code đã trả.

**Tiến trình trong buổi:**
1. **Đầu buổi Thiêng dựng môi trường giống production** để cả 4 người chạy E2E trên cùng một môi trường.
2. 3 người còn lại viết E2E/kiểm thử trên môi trường đó; lỗi phát hiện được sửa ngay trong nhánh của người phụ trách mảng.
3. Cuối buổi: merge, chạy toàn bộ checklist nghiệm thu, gắn tag `v1.0.0`.

### Dev 1 – Hoàng Bình Quân · E2E xác thực + kiểm thử bảo mật
- **Hướng đi:**
  1. **Playwright 5 luồng:** đăng ký → tự đăng nhập → vào dashboard; sai 5 lần → 423; Google (mock); access token hết hạn → tự refresh không gián đoạn; Admin khóa → user bị đăng xuất ở lần refresh kế tiếp.
  2. **Integration test** lấp chỗ thiếu: mọi endpoint `/auth/*`, `/users/*` có test đúng + test lỗi; phát hiện dùng lại token; `IsActive = false` → 403 ở login lẫn refresh.
  3. **Checklist bảo mật:** giả mạo `X-Forwarded-For` vào thẳng cổng API bị bỏ qua; `/hangfire` không qua Nginx hoặc không Basic Auth → 401; request 11 → 429 + `Retry-After`; Author gọi API Admin/sửa công thức người khác → 403; `/auth/me`, `/auth/sessions` không lộ hash; **grep log + Seq không có token, mật khẩu, header `Authorization`**.
  4. **Coverage** tầng Application module Auth ≥ 80%; mọi luồng phát token đều qua `AuthResponseFactory`; XML doc cho Scalar.
  5. **Báo cáo** `docs/security-test-report.md` kèm ảnh chụp.
- **Vì sao:** kiểm giả mạo `X-Forwarded-For` là mục quan trọng nhất – cấu hình sai thì kém an toàn hơn cả khi chưa bật; kiểm log là mắt xích cuối của chuỗi bảo vệ token. Dùng **27 mã lỗi Phụ lục B làm checklist** để không sót test lỗi nào.
- **Xong khi:** E2E xanh, checklist đạt hết, coverage ≥ 80%.
- **Commit:** `test(auth): e2e auth flows, security vulnerability checklist and 80% coverage with token issuer refactor`

### Dev 2 – Nguyễn Hồng Phúc Thọ · E2E công thức + kiểm thử đồng thời
- **Hướng đi:**
  1. **Playwright vòng đời đầy đủ:** tạo qua wizard 4 bước → xuất bản → thấy ở trang công khai → lưu trữ → khôi phục về nháp → xóa mềm → biến mất khỏi mọi endpoint; kéo-thả bước → số thứ tự 1..N.
  2. **Đồng thời:** 2 client cùng `rowVersion` → người sau 409; **10 request `PUT` song song (`Task.WhenAll`) → đúng 1 thành công, 9 nhận 409, không request nào 500**.
  3. **Cách ly công khai/riêng tư (bắt buộc):** Draft không thấy ở `/recipes` và `/recipes/{slug}` với khách; Admin gọi trước rồi khách gọi đúng URL → khách vẫn chỉ thấy Published; `/recipes/mine` có `no-store`, xem của người khác → 403.
  4. **Unit test Domain:** publish thiếu điều kiện → 400; ma trận 12 tổ hợp; đánh số lại bước; 1 ảnh chính; hậu tố slug; `PublishedAt` không bị ghi đè.
  5. **Kiểm N+1:** đếm số query của `GetRecipeBySlug` (kỳ vọng 1–2), dùng `AsSplitQuery()` nếu cần.
- **Vì sao:** lỗi rò rỉ bản nháp không biểu hiện khi dùng bình thường, chỉ lộ đúng trình tự – phải có test tái hiện để không quay lại khi ai đó "tối ưu" cache; test song song để chứng minh **không có 500**; đếm query biến N+1 thành lỗi build.
- **Xong khi:** E2E, test đồng thời, test cách ly đều xanh.
- **Commit:** `test(recipes): e2e recipe lifecycle, concurrency conflict and public/private cache isolation regression tests`

### Dev 3 – Đoàn Hồng Tiến · E2E tìm kiếm + WCAG 2.1 AA + Lighthouse
- **Hướng đi:**
  1. **Playwright:** "pho" → "Phở bò"; lọc + sắp xếp + phân trang giữ nguyên trên URL, F5 và back/forward đúng; `sortBy=maliciousColumn` → 400; Admin CRUD danh mục + 409 khi xóa danh mục còn công thức.
  2. **Accessibility** `@axe-core/playwright` quét mọi route: **0 lỗi mức serious/critical**; rà thủ công semantic HTML, ARIA, focus trong modal, điều hướng bàn phím, tương phản ≥ 4.5:1, alt text.
  3. **Responsive** 320 / 768 / 1200px; drawer lọc dùng được bằng chạm và bàn phím.
  4. **Lighthouse CI** trang chủ, danh sách, chi tiết: LCP ≤ 2.5s, CLS ≤ 0.1, INP ≤ 200ms, JS ≤ 200KB.
  5. **SEO:** Rich Results Test pass; `<title>` ≤ 60 ký tự sau khi cắt; slug Draft trả HTTP 404; sitemap không có Draft/Archived. Ghi `docs/a11y-report.md`, `docs/lighthouse-report.md`.
- **Vì sao:** test `sortBy` sai là **test bảo mật** (chống SQL injection qua tên cột); ngưỡng serious/critical là tiêu chí đạt được và có ý nghĩa; đo `<title>` trên HTML thật mới chứng minh quy tắc cắt chuỗi chạy.
- **Xong khi:** axe 0 lỗi nghiêm trọng, Lighthouse đạt, Rich Results pass.
- **Commit:** `test(search,a11y): e2e search and filter tests, wcag 2.1 aa compliance and lighthouse performance fixes`

### Dev 4 – Nguyễn Thăng Thiêng · Gia cố Docker, load test, đóng gói `v1.0.0`
- **Hướng đi:**
  1. **Gia cố image:** tách layer restore/`npm ci`, rà `.dockerignore`, **ghim tag image** (bỏ `latest` của minio, seq), quét lỗ hổng bằng `docker scout`/Trivy (không chấp nhận mức Critical).
  2. **Kiểm chứng toàn hệ thống:** dev 9 container `healthy`; prod `--scale api=3` đều `healthy`, Nginx luân phiên 3 instance (xem CorrelationId trong log), tạo lại container `api` không 502.
  3. **k6 – 3 kịch bản:** smoke (1 user) → load (**100 user**, 5 phút) → stress; tỉ lệ 70% đọc, 20% tìm kiếm, 10% ghi. Chỉ tiêu: **p50 ≤ 150ms, p95 ≤ 500ms, p99 ≤ 1000ms, hit rate ≥ 80%**.
  4. **Chịu lỗi khi đang có tải:** tắt Redis → đọc từ DB, không exception; tắt MinIO → công thức vẫn đọc được, upload trả 503.
  5. **Đóng gói:** `docs/load-test-report.md`, README < 5 phút, `CHANGELOG.md`, **6 ADR** (xóa mềm, chuẩn hóa mã lỗi, Redis, cách ly cache, Hangfire worker riêng, Nginx resolver động), tag **`v1.0.0`**.
- **Vì sao:** kịch bản tải phải giống thực tế (blog chủ yếu là khách đọc) mới ra con số có nghĩa; thử chịu lỗi **khi đang có tải** mới chứng minh được hệ thống suy giảm có kiểm soát; ADR giữ lại *lý do* để người sau không "sửa" ngược làm sống lại lỗi cũ. Nếu hit rate < 80% thì ghi nhận trung thực kèm phân tích, **không nới TTL cho đạt**.
- **Xong khi:** k6 đạt chỉ tiêu, kịch bản chịu lỗi đúng, đủ tài liệu, tag `v1.0.0`.
- **Commit:** `chore(release): harden docker images, k6 load tests, resilience verification and v1.0.0 release packaging`

**Checklist nghiệm thu cuối dự án:** 37/37 FR · 45/45 endpoint trên Scalar (khóa bởi `ApiSurfaceTests` từ Buổi 4) · 17/17 nợ kỹ thuật đã trả · 29/29 mã lỗi có test · 0 chỗ dùng mã 422 · bảng TTL khớp 100% SRS · Nginx/healthcheck khớp SRS §6.5 · 6 ADR · tag `v1.0.0`, CHANGELOG, README < 5 phút.

---

# Phụ lục – Cài đặt & chạy dự án

**Yêu cầu:** .NET SDK 10.0.x · Node.js 20+ · Docker Desktop 4.x (Compose v2) · `dotnet tool install --global dotnet-ef`.

```bash
cp .env.example .env                  # điền JWT_SIGNING_KEY, AUTH_SECRET... (openssl rand -base64 64)
git config core.hooksPath .githooks   # bật pre-commit gitleaks
docker compose --profile dev up -d --build
```

| Service | URL |
|---|---|
| Nginx | http://localhost |
| Frontend | http://localhost:3000 |
| API + Scalar | http://localhost:5000/scalar |
| MinIO Console | http://localhost:9001 |
| Seq | http://localhost:5341 |
| Mailhog | http://localhost:8025 |

Tài khoản seed: `admin@culinaryblog.local` (mật khẩu `SEED_ADMIN_PASSWORD`), `author1..5@culinaryblog.local` (`SEED_AUTHOR_PASSWORD`). Dữ liệu mẫu: 20 danh mục, 100 công thức (~85% đã xuất bản) — API tự seed/bù khi khởi động, không cần xóa volume. Máy đã có PostgreSQL chiếm cổng 5432 → đặt `POSTGRES_HOST_PORT=5434`.

**Definition of Done mỗi commit:**
```bash
cd backend  && dotnet build && dotnet test
cd frontend && npm run lint && npm run typecheck && npm test && npm run build
```
