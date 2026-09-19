# Culinary Blog – Nhóm 20

Blog ẩm thực & nấu ăn: người dùng đăng ký, viết công thức (nguyên liệu, các bước, ảnh), xuất bản, tìm kiếm tiếng Việt không dấu; Admin quản trị danh mục và tài khoản.
**Công nghệ:** .NET 10 Minimal API (Clean Architecture + CQRS/MediatR) · Next.js 15 App Router · PostgreSQL 16 · Redis 7 · MinIO · Hangfire · Nginx · Docker Compose.
**Tài liệu gốc:** [`SPEC/SRS_Culinary_Blog_v1.2.0.md`](SPEC/SRS_Culinary_Blog_v1.2.0.md) (yêu cầu) · [`SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md`](SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md) (kế hoạch đầy đủ) · [`SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`](SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md) (57 mâu thuẫn đã giải quyết).

## Thành viên

| Vai trò | Thành viên | MSSV | Phụ trách | Nhánh Git (buổi n) |
|---|---|---|---|---|
| Dev 1 | **Hoàng Bình Quân** | 2314236 | Xác thực, phân quyền, quản lý người dùng | `2314236_quan_buoi{n}` |
| Dev 2 | **Nguyễn Hồng Phúc Thọ** | 2312758 | Công thức nấu ăn (lõi nghiệp vụ) | `2312758_tho_buoi{n}` |
| Dev 3 | **Đoàn Hồng Tiến** | 2314291 | Danh mục, tìm kiếm, SEO | `2314291_tien_buoi{n}` |
| Dev 4 | **Nguyễn Thăng Thiêng** (trưởng nhóm) | 2312755 | Tệp tin, job nền, quan sát hệ thống, hạ tầng | `2312755_thieng_buoi{n}` |

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
| Endpoint REST | **44** | SRS Chương 8 |
| Mã lỗi ứng dụng | **27** | SRS Phụ lục B – mỗi mã phải có ít nhất 1 test |
| Mâu thuẫn trong SRS đã giải quyết | **57** | MT-01 → MT-57 (CR-2026 và CR-2026-02) |

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

- Solution 4 tầng + 4 project test, `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion), `AuditInterceptor`, `GlobalExceptionMiddleware`, `PagedResult<T>`.
- Docker Compose 8 service (`nginx, api, hangfire, frontend, postgres, redis, minio, seq`) + `mailhog` cho dev; bản production `docker-compose.prod.yml` (scale `api`×3).
- Nginx: reverse proxy, rate limit vòng ngoài, resolver DNS động, `/hangfire` có Basic Auth, `/media/` phục vụ ảnh, SSL/HSTS.
- Frontend dùng chung: `api-client` (Bearer, tự refresh 401, đọc ProblemDetails), TanStack Query, React Hook Form + Zod, `ImageUploader`.
- Bộ integration test dùng chung (WebApplicationFactory + Testcontainers), E2E Playwright, load test k6, Lighthouse, axe.
- Quy trình: gitleaks pre-commit, Definition of Done mỗi commit, thứ tự merge migration, 6 ADR, CHANGELOG, tag `v1.0.0`.

## 1.6 44 endpoint theo module

| Module | Endpoint |
|---|---|
| Auth (12) | `POST /auth/register` · `POST /auth/login` · `POST /auth/google` · `POST /auth/refresh` · `POST /auth/logout` · `GET /auth/me` · `PATCH /auth/me` · `GET /users` · `PATCH /users/{id}/status` · `GET /auth/sessions` · `DELETE /auth/sessions/{id}` · `POST /auth/sessions/revoke-all` |
| Categories (5) | `GET /categories` · `GET /categories/{slug}` · `POST /categories` · `PUT /categories/{id}` · `DELETE /categories/{id}` |
| Recipes (12) | `GET /recipes` · `GET /recipes/mine` · `GET /recipes/search` · `GET /recipes/sitemap` · `GET /recipes/{slug}` · `POST /recipes` · `PUT /recipes/{id}` · `PATCH /recipes/{id}/publish` · `…/unpublish` · `…/archive` · `…/unarchive` · `DELETE /recipes/{id}` |
| Ảnh công thức (3) | `POST /recipes/{id}/images` · `PATCH /recipes/{id}/images/{imageId}` · `DELETE /recipes/{id}/images/{imageId}` |
| Các bước (4) | `POST /recipes/{id}/steps` · `PUT …/steps/{stepId}` · `PATCH …/steps/reorder` · `DELETE …/steps/{stepId}` |
| Nguyên liệu (3) | `POST /recipes/{id}/ingredients` · `PUT …/ingredients/{ingId}` · `DELETE …/ingredients/{ingId}` |
| Files (2) | `POST /files/upload` · `DELETE /files/{**key}` |
| Health (3) | `GET /health` · `GET /health/live` · `GET /health/ready` |

---

# Phần 2 – Chia việc tổng thể

**Nguyên tắc:** mỗi người giữ **một mảng cố định suốt 7 buổi** và làm **trọn từ database → API → giao diện** cho mảng đó. Cuối mỗi buổi hệ thống phải chạy được.

| Thành viên | Làm gì trên trang web | FR |
|---|---|---|
| **Dev 1 – Hoàng Bình Quân** | Mọi thứ về tài khoản: đăng ký, đăng nhập (email + Google), giữ phiên đăng nhập, hồ sơ cá nhân, Admin khóa tài khoản, bảo mật chung | FR-AUTH-001 → 009 |
| **Dev 2 – Nguyễn Hồng Phúc Thọ** | Mọi thứ về công thức: xem, viết, sửa, ảnh, nguyên liệu, các bước, xuất bản, lưu trữ, xóa, "công thức của tôi" | FR-RCP-001 → 011 |
| **Dev 3 – Đoàn Hồng Tiến** | Danh mục, tìm kiếm – lọc – sắp xếp – phân trang, trang chủ, SEO, sitemap, trải nghiệm & chuẩn truy cập | FR-CAT-001 → 005, FR-SRCH-001 → 004 |
| **Dev 4 – Nguyễn Thăng Thiêng** | Upload/xóa ảnh, job nền, health check, log & tracing, Docker, Nginx, cache, hạ tầng test, đóng gói | FR-FILE, FR-JOB, FR-OBS, DevOps |

**Lịch 7 buổi (tóm tắt):**

| Buổi | Dev 1 – Quân | Dev 2 – Thọ | Dev 3 – Tiến | Dev 4 – Thiêng |
|---|---|---|---|---|
| 1 ✅ | Đăng ký, đăng nhập | Danh sách & chi tiết công thức | Danh sách & chi tiết danh mục | Docker + upload/xóa ảnh |
| 2 | Google, đăng xuất | Tạo công thức nháp + ảnh | Admin CRUD danh mục | Sửa mã lỗi 422→400, dashboard Hangfire, bộ test tích hợp |
| 3 | Gia hạn phiên an toàn | Nguyên liệu & các bước | Tìm kiếm tiếng Việt | Job resize ảnh, job dọn dữ liệu |
| 4 | Hồ sơ cá nhân | Sửa công thức, xuất bản | Lọc, sắp xếp, phân trang | Health check |
| 5 | Rate limit, CSP, chặn route | Xóa mềm, "công thức của tôi", vá rò rỉ bản nháp | Trang chủ, SEO, JSON-LD | Log có cấu trúc, tracing |
| 6 | Admin quản lý tài khoản, quản lý phiên | Lưu trữ / khôi phục | Sitemap, robots, tối ưu ảnh | Nginx production, SSL, cache, scale |
| 7 | Test E2E + bảo mật | Test E2E + đồng thời | Test E2E + WCAG + Lighthouse | Load test, đóng gói, `v1.0.0` |

**Quy trình chung:** mỗi buổi mỗi người tạo nhánh `{mssv}_{tên}_buoi{n}` từ `develop` → **1 commit / người / buổi** → merge vào `develop` theo thứ tự **Dev 4 → Dev 1 → Dev 3 → Dev 2** → hết buổi merge `develop` vào `main`. Commit chỉ được merge khi đạt Definition of Done (build 0 warning, test xanh, lint + build frontend xanh, `docker compose up` chạy, đã test tay).

---

# Phần 3 – Chia việc chi tiết từng buổi

> **Cách đọc mỗi phần việc:**
> **Chức năng** – làm gì · **Hướng đi** – các bước làm theo thứ tự · **Vì sao** – lý do chọn hướng đó · **Xong khi** – điều kiện kiểm chứng · **Commit** – thông điệp commit.
> Chi tiết kỹ thuật đầy đủ (đoạn code mẫu, phân tích phương án bị loại) nằm trong [`SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md`](SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md).

---

## BUỔI 1 – Nền móng & lát cắt đầu tiên ✅ (đã hoàn thành, 11/09 – 14/09/2026)

**Mục tiêu:** có khung dự án chạy được bằng Docker và 4 lát cắt đầu tiên: đăng ký/đăng nhập, xem công thức, xem danh mục, upload ảnh.

**Tiến trình đã thực hiện:** 30 phút đầu Dev 4 đẩy commit khung (`b058654`) → 4 người tách nhánh làm song song → merge vào `develop` theo thứ tự Dev 4 → Dev 1 → Dev 3 → Dev 2 → merge `main` (`d2d8243`) → rà soát chạy container và sửa lỗi (`064f582`, `6137d3e`).

### Dev 1 – Hoàng Bình Quân · FR-AUTH-001, FR-AUTH-002 ✅
- **Đã làm:** `ApplicationUser` + `RefreshToken` (chỉ lưu SHA-256); Identity PBKDF2 100.000 vòng, mật khẩu mạnh, khóa 5 lần/15 phút; `RegisterUserCommand`, `LoginUserCommand`; `POST /auth/register` (201), `POST /auth/login` (200); trang `/auth/login`, `/auth/register` (React Hook Form + Zod).
- **Vì sao:** dùng ASP.NET Core Identity thay vì tự viết bảng user để có sẵn băm mật khẩu chuẩn, khóa tài khoản, role; lưu hash của refresh token ngay từ đầu để lộ DB cũng không mạo danh được ai.
- **Còn nợ:** D-1 (Buổi 2), D-2 và D-12 (Buổi 3); integration test đăng ký/đăng nhập viết khi có bộ test ở Buổi 2.
- **Commit:** `3d40be6 feat(auth): complete FR-AUTH-001 & 002 register login flow`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-001, FR-RCP-002 ✅
- **Đã làm:** toàn bộ schema công thức (Recipe, Step, Ingredient, Image, Nutrition dạng owned); seed 50 công thức / 5 tác giả bằng Bogus; `GET /recipes` (phân trang, lọc), `GET /recipes/{slug}`; trang `/recipes` (SSR) và `/recipes/[slug]` (ISR 300s).
- **Vì sao:** dựng đủ schema ngay buổi đầu vì schema là thứ đắt nhất để sửa khi đã có dữ liệu; truy vấn đọc dùng `AsNoTracking` + projection để tránh N+1 từ gốc.
- **Còn nợ:** D-3, D-4, D-6 (Buổi 5), D-8 (Buổi 2), D-15 (Buổi 4), D-16 (Buổi 3).
- **Commit:** `0fc95d1 feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view`

### Dev 3 – Đoàn Hồng Tiến · FR-CAT-001, FR-CAT-002 ✅
- **Đã làm:** entity `Category` + seed 8 danh mục; `SlugHelper` bỏ dấu tiếng Việt; `RedisCacheService` (Redis lỗi thì đọc DB); `GET /categories`, `GET /categories/{slug}`; trang `/categories`, `/categories/[slug]`.
- **Vì sao:** chọn Redis ngay từ đầu (không `IMemoryCache`) vì chạy nhiều instance thì cache trong bộ nhớ không invalidate được.
- **Còn nợ:** D-13 (Buổi 2), D-18 (Buổi 3), D-5, D-7 (Buổi 5); Value Object `Slug` và component `CategoryNav` bổ sung ở Buổi 2.
- **Commit:** `fd27a71 feat(category): complete FR-CAT-001 & 002 public categories API and UI`

### Dev 4 – Nguyễn Thăng Thiêng · Hạ tầng + FR-FILE-001, FR-FILE-002 ✅
- **Đã làm:** commit khung cho cả nhóm; Docker Compose 8 service + mailhog, Dockerfile multi-stage non-root; `MinioFileStorageService` (AWSSDK.S3); kiểm tra magic bytes; `POST /files/upload`, `DELETE /files/{**key}`; component `ImageUploader` (kéo-thả, % tiến trình); làm sớm FR-JOB-001 (email chào mừng qua Hangfire).
- **Vì sao:** dùng AWSSDK.S3 để sau này đổi sang AWS S3 thật chỉ cần đổi cấu hình; kiểm magic bytes vì `Content-Type` do client khai là giả mạo được; tên file do server sinh (`{Guid}`) để chặn path traversal.
- **Sửa sau buổi:** chờ DB sẵn sàng khi khởi động, Nginx 502 khi container api đổi IP, hook gitleaks trên Windows (`064f582`); ảnh gần 5MB bị Nginx chặn, log khởi động (`6137d3e`).
- **Commit:** `b058654 chore: bootstrap…` · `9dc3d81 feat(infra): complete docker compose setup and FR-FILE minio upload component`

---

## BUỔI 2 – Google / Đăng xuất, tạo công thức nháp, CRUD danh mục, hạ tầng kiểm thử

**Mục tiêu:** đủ đường vào hệ thống (email, Google, đăng xuất); Author tạo được công thức nháp kèm ảnh; Admin quản trị danh mục; job nền xem được qua dashboard an toàn; **cả nhóm có bộ integration test** để mọi buổi sau viết test thật.

**Tiến trình trong buổi:**
1. **30 phút đầu – commit nền D-11 (Thiêng làm, cả nhóm review):** đổi mọi lỗi validation 422 → 400 ở backend và frontend. Merge vào `develop` rồi 4 người mới tách nhánh `…_buoi2`.
2. **Trước giữa buổi:** Thiêng đẩy bộ test (`CulinaryBlogApiFactory`, `CreateClientAs(role)`) để 3 người còn lại viết integration test.
3. Quân làm D-1 (đổi `UserDto`) **sớm và merge trước** Thọ, Tiến vì component FE nào đọc `user.fullName` cũng phải đổi theo.
4. Cuối buổi: merge Thiêng → Quân → Tiến → Thọ, chạy kiểm chứng cuối buổi, merge `develop` → `main`.

### Commit nền – Nguyễn Thăng Thiêng dẫn · D-11 (422 → 400)
- **Hướng đi:** `GlobalExceptionMiddleware` trả **400** + `type = VALIDATION_ERROR`; đổi mọi `Status422UnprocessableEntity` trong endpoint; frontend `LoginForm`, `RegisterForm` đổi `status === 422` → `400` và gom việc gắn lỗi vào từng ô thành helper `mapProblemDetailsToForm()` trong `lib/api-client.ts`; sửa test cũ; thêm test chặn chuỗi `Status422` quay lại.
- **Vì sao làm trước tiên:** mọi endpoint mới của buổi đều trả lỗi validation. Nếu đổi backend mà quên frontend, form sẽ **âm thầm mất lỗi từng ô** – không crash, không log, chỉ người dùng nhập sai mới phát hiện.
- **Commit:** `refactor(api): unify validation errors to 400 per SRS v1.1.0 MT-08`

### Dev 1 – Hoàng Bình Quân · FR-AUTH-003 Google + FR-AUTH-005 Đăng xuất (+ D-1)
- **Hướng đi:**
  1. **D-1:** tạo `IUserNameGenerator` sinh `UserName` từ phần trước `@` của email (trùng thì thêm `2`, `3`…). Đăng ký nhận `{ email, password, displayName }`; `UserDto` = `{ id, email, displayName, avatarUrl, bio, roles }`; xóa mã `AUTH_USERNAME_EXISTS`; form đăng ký bỏ ô "Tên đăng nhập".
  2. **FE Google:** cài `@react-oauth/google`, dùng **component `<GoogleLogin>`** để lấy **ID Token** rồi gửi `POST /auth/google`. Không dùng hook `useGoogleLogin` (hook đó trả access token, backend không verify được).
  3. **`GoogleLoginCommand`:** `GoogleJsonWebSignature.ValidateAsync` kiểm chữ ký, `iss`, `aud`, `exp`. Đã liên kết Google → đăng nhập; email đã tồn tại → **chỉ liên kết khi `EmailVerified == true`**; mới hoàn toàn → tạo user role Author. Lỗi: token sai định dạng 400, chữ ký sai 401, không lấy được khóa Google 502, tài khoản bị khóa 403.
  4. **`LogoutCommand`:** hash token nhận được, gán `RevokedAt`; không tìm thấy vẫn trả 204.
  5. **API + UI + test:** `POST /auth/google` (200), `POST /auth/logout` (204); nút Google ở `/auth/login` kèm thông báo khi Google lỗi; menu "Đăng xuất" gọi API rồi mới xóa phiên phía client. Test: 2 email cùng prefix → `an.nguyen`, `an.nguyen2`; đăng xuất 2 lần đều 204.
- **Vì sao:**
  - **ID Token flow** giữ backend stateless: không lưu state/PKCE verifier, không cần route callback hay client secret.
  - **Backend bắt buộc tự verify chữ ký**: tin email do FE gửi lên thì ai cũng `curl` chiếm được tài khoản người khác.
  - **Chỉ liên kết khi email đã xác minh**: nếu không, kẻ xấu tạo tài khoản Google gắn email nạn nhân là chiếm được tài khoản.
  - **Đăng xuất luôn 204**: trả 404 sẽ biến endpoint thành công cụ dò token hợp lệ.
  - **D-1 làm ngay buổi này** vì Google tạo user mà không ai nhập `userName`, và `UserDto` đổi sớm thì ít màn hình phải sửa.
- **Xong khi:** đăng nhập Google tạo/liên kết đúng tài khoản; đăng xuất thu hồi refresh token trong DB; form đăng ký không còn ô tên đăng nhập; integration test xanh.
- **Commit:** `feat(auth): complete FR-AUTH-003 google id-token sign-in, FR-AUTH-005 logout revocation and displayName contract`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-003 Tạo nháp + FR-RCP-008 Ảnh (+ D-8)
- **Hướng đi:**
  1. **D-8 trước tiên:** migration `B2_Recipe_InstructionsNullable` cho `Instructions` NULL (nếu không, mọi request thiếu trường này lỗi DB).
  2. **`CreateRecipeCommand` + Validator:** `title` 5–200, `description` 20–2000, `prepTime > 0`, `cookTime ≥ 0`, `servings > 0`, `categoryId` phải tồn tại → sai trả 400. Dùng `Recipe.Create(...)` (luôn Draft); slug từ `SlugHelper`, trùng thì `-2`, `-3`; **cấm slug trùng từ khóa dành riêng** `search, mine, sitemap, new, edit`; nutrition gửi kèm body (không có endpoint riêng).
  3. **`RecipeAuthorizationHandler`:** qua khi là chủ công thức hoặc Admin – dùng lại cho **mọi** FR-RCP về sau.
  4. **Ảnh:** `UploadRecipeImageCommand` (dùng lại `IFileStorageService` + `ImageFileInspector` của Buổi 1, ảnh đầu tiên tự là ảnh chính, **để sẵn chỗ `BackgroundJob.Enqueue` cho job resize**); `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`; `DeleteRecipeImageCommand` (xóa ảnh chính thì ảnh có `OrderIndex` nhỏ nhất lên thay). Đổi ảnh chính làm **2 lần `SaveChanges` trong 1 transaction**. Mọi command xóa cache `recipes:list:*` + `recipe:{slug}`.
  5. **API + UI:** `POST /recipes` (201), `POST/PATCH/DELETE /recipes/{id}/images…` (MinIO lỗi → 503); trang `/dashboard/recipes/new` dạng wizard: bước 1 thông tin + dinh dưỡng, bước 2 thư viện ảnh (chọn ảnh chính bằng radio). Test: đổi ảnh chính 10 lần liên tiếp không lần nào vỡ unique index.
- **Vì sao:**
  - **Luôn tạo ở trạng thái Draft** để có điểm chặn kiểm tra điều kiện xuất bản (đủ bước + nguyên liệu) ở Buổi 4.
  - **Nutrition đi kèm công thức** vì nó chỉ là nhóm cột trong bảng `Recipes`; endpoint riêng sẽ tạo hai đường ghi cùng một dữ liệu.
  - **Handler phân quyền dựng ngay từ FR ghi đầu tiên** để không handler nào tự viết `if` rồi quên.
  - **Đổi ảnh chính 2 bước** vì index "chỉ 1 ảnh chính" là index một phần – PostgreSQL không cho hoãn kiểm tra, còn EF không đảm bảo thứ tự 2 lệnh UPDATE.
  - Mảng `steps?`/`ingredients?` trong body tạo công thức **để sang Buổi 3** để validator nguyên liệu/bước chỉ viết một lần.
- **Xong khi:** Author tạo được công thức nháp kèm nhiều ảnh, đổi được ảnh chính; slug dành riêng bị chặn; integration test xanh.
- **Commit:** `feat(recipes): complete FR-RCP-003 create draft recipe & FR-RCP-008 image management with merged patch metadata`

### Dev 3 – Đoàn Hồng Tiến · FR-CAT-003/004/005 CRUD danh mục Admin (+ D-13)
- **Hướng đi:**
  1. **`CreateCategoryCommand`:** `name` 2–100, `imageUrl?` URL hợp lệ, `orderIndex ≥ 0`; **kiểm tra tên trùng trước khi ghi** → 409 `CATEGORY_NAME_EXISTS`; slug tự sinh; trả 201 + header `Location`.
  2. **`UpdateCategoryCommand`:** cùng validator, cũng kiểm tra tên trùng; **slug không đổi khi đổi tên**.
  3. **`DeleteCategoryCommand`:** còn công thức → 409 `CATEGORY_DELETE_HAS_RECIPES` kèm **số công thức** trong thông báo; không còn → **xóa mềm**.
  4. **Lớp phòng vệ thứ hai:** middleware bắt lỗi PostgreSQL `23505` (trùng unique) → 409 thay vì 500 (hai Admin tạo trùng cùng lúc).
  5. **Cache + API + UI:** D-13 đổi TTL `categories:all` 60 → **30 phút**; 3 command xóa `categories:all` + `categories:detail:*`. `POST/PUT/DELETE /categories` chỉ Admin (Author → 403). Trang `/dashboard/categories`: bảng, form modal (có `ImageUploader`), hộp xác nhận xóa, thông báo 409 dễ hiểu. Bổ sung Value Object `Slug` và component `CategoryNav` còn thiếu từ Buổi 1. Test: 201 / 403 / 409 trùng tên / 409 còn công thức.
- **Vì sao:**
  - **Kiểm tra tên trùng ở tầng Application** để người dùng nhận thông báo nghiệp vụ rõ ràng thay vì "Lỗi hệ thống 500"; vẫn giữ lớp bắt `23505` vì kiểm tra trước không loại được hoàn toàn race condition.
  - **Slug giữ nguyên khi đổi tên** để link đã chia sẻ và URL Google đã lập chỉ mục không chết.
  - **Xóa mềm** đúng thiết kế `BaseEntity` toàn hệ thống.
  - **Báo số công thức cụ thể** để Admin biết khối lượng việc cần chuyển trước khi xóa.
- **Xong khi:** Admin tạo/sửa/xóa danh mục trên giao diện; Author bị chặn 403; cache được xóa đúng sau mỗi thao tác.
- **Commit:** `feat(categories): complete FR-CAT-003/004/005 admin crud with proactive name conflict check and soft delete`

### Dev 4 – Nguyễn Thăng Thiêng · Dashboard Hangfire an toàn + bộ integration test
- **Hướng đi:**
  1. **Dashboard:** `MapHangfireDashboard("/hangfire")` (chỉ ở container `api`) với filter `NginxGateDashboardFilter`: chỉ cho qua khi header `X-Hangfire-Gate` khớp secret (so sánh `FixedTimeEquals`).
  2. **Nginx:** `location /hangfire` có **Basic Auth** (`htpasswd`, không commit) và gắn header gate; chuyển `nginx.conf` sang thư mục `templates/` để nạp secret bằng `envsubst`; thêm biến giả vào `.env.example`.
  3. **Bộ test tích hợp** trong `tests/CulinaryBlog.API.IntegrationTests`: `CulinaryBlogApiFactory` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, chạy migration, tắt Hangfire server, thay `IFileStorageService`/`IBackgroundJobClient` bằng bản giả; `Respawn` dọn DB giữa các test; helper `CreateClientAs(role)` phát JWT thật.
  4. **Trả nợ test Buổi 1:** integration test cho mọi endpoint Buổi 1; viết sẵn test tái hiện lỗ hổng rò rỉ bản nháp (đánh dấu `Skip` – Thọ gỡ ở Buổi 5).
  5. Giảm `QueuePollInterval` Hangfire xuống 1 giây ở dev.
- **Vì sao:**
  - **Dựng bộ test ngay Buổi 2** vì Definition of Done đòi test xanh ở mọi commit; để tới cuối dự án thì 5 buổi liền không ai viết được integration test.
  - **Testcontainers thay vì EF In-Memory** vì In-Memory không có unique, FK, `tsvector`, index một phần, concurrency token – đúng những thứ cần kiểm nhất.
  - **Hai lớp bảo vệ dashboard:** trình duyệt không gửi được Bearer token khi mở trang, nên dùng Basic Auth ở Nginx; header gate chặn trường hợp gọi thẳng cổng `5000` bỏ qua Nginx.
- **Xong khi:** `http://localhost/hangfire` hỏi mật khẩu rồi vào được; `http://localhost:5000/hangfire` trả 401; `dotnet test` chạy integration test xanh trên máy có Docker.
- **Commit:** `feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness`

**Kiểm chứng cuối Buổi 2:** không còn chuỗi `Status422` / `status === 422` trong code; form đăng ký hiện lỗi từng ô với 400; 2 email cùng prefix sinh `UserName` có hậu tố số; `/hangfire` được bảo vệ; integration test chạy trên Testcontainers.

---

## BUỔI 3 – Gia hạn phiên an toàn, nguyên liệu & các bước, tìm kiếm tiếng Việt, job nền

**Mục tiêu:** phiên đăng nhập tự gia hạn an toàn; công thức có đủ nguyên liệu + các bước; tìm "pho" ra "Phở"; hai job nền còn lại chạy thật.

**Tiến trình trong buổi:**
1. Thọ và Tiến **cùng tạo migration trên bảng `Recipes`** → Tiến merge trước, Thọ rebase rồi chạy lại `dotnet ef migrations add`.
2. Thiêng cắm `ImageResizeJob` vào chỗ `BackgroundJob.Enqueue` Thọ đã để sẵn ở Buổi 2.
3. Quân bật `UseForwardedHeaders` – Nginx đã gửi `X-Forwarded-For` từ Buổi 1 nên không phụ thuộc ai.
4. Cuối buổi merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · FR-AUTH-004 Refresh token rotation (+ D-2, D-12, IP thật)
- **Hướng đi:**
  1. **D-2:** refresh token 64 → **32 byte (256-bit)**. Entity `RefreshToken` thêm `Revoke(now, replacedByHash)` và `IsUsable(now)`; repository thêm `GetByHashAsync`, `RevokeAllForUserAsync`.
  2. **`RefreshTokenCommand`:** không thấy → 401 `AUTH_TOKEN_INVALID`; hết hạn → 401 `AUTH_REFRESH_TOKEN_EXPIRED`; **token đã bị thu hồi mà vẫn được dùng → thu hồi toàn bộ token của user**, log cảnh báo bảo mật, 401 `AUTH_REFRESH_TOKEN_REVOKED`; tài khoản bị khóa → 403; hợp lệ → trong 1 transaction thu hồi token cũ (`ReplacedByTokenHash`) và phát cặp mới.
  3. **IP thật:** bật `UseForwardedHeaders` với **`KnownIPNetworks`** (dải mạng Docker, cấu hình được), đặt đầu pipeline. Không dùng `KnownNetworks` (obsolete trên .NET 10 → gãy build).
  4. **API:** `POST /auth/refresh` (không cần Bearer).
  5. **UI – D-12:** access token **chỉ giữ trong bộ nhớ**, chỉ refresh token được lưu; tải lại trang thì gọi refresh một lần. `api-client` gặp 401 → gọi refresh **đúng một lần dù nhiều request song song** (single-flight) rồi gửi lại; refresh lỗi → xóa phiên, về trang đăng nhập.
- **Vì sao:**
  - **Rotation** biến refresh token thành bí mật dùng một lần; **phát hiện dùng lại** là cách duy nhất trong JWT stateless để biết token bị đánh cắp.
  - **Chỉ lưu hash**: lộ DB cũng không có token dùng được; SHA-256 đủ vì token là chuỗi ngẫu nhiên 256-bit, không phải mật khẩu người đặt.
  - **Single-flight**: 5 request cùng nhận 401 mà mỗi cái tự refresh sẽ kích hoạt nhầm cơ chế phát hiện dùng lại → người dùng bị đăng xuất vô cớ.
  - **Access token trong bộ nhớ** giảm thứ XSS lấy được; SRS cấm cookie nên refresh token vẫn phải ở nơi JS đọc được – bù lại bằng rotation + CSP (Buổi 5).
  - **Bật IP thật ngay buổi này** vì từ đây mỗi lần refresh ghi `CreatedByIp`; để muộn là ghi sai IP vĩnh viễn.
- **Xong khi:** dùng lại token cũ sau khi rotate → cả họ token bị thu hồi; `localStorage` không còn access token; `CreatedByIp` là IP thật.
- **Commit:** `feat(auth): complete FR-AUTH-004 refresh rotation with reuse detection, in-memory access token and forwarded headers`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-009 Nguyên liệu + FR-RCP-010 Các bước (+ D-16)
- **Hướng đi:**
  1. **Migration `B3_Recipe_IngredientStep`:** thêm `QuantityText varchar(50) NULL`, `CHECK (Quantity IS NULL OR Quantity > 0)`. **D-16:** bỏ unique index `(RecipeId, StepNumber)`, thay bằng **unique constraint `DEFERRABLE INITIALLY DEFERRED`**.
  2. **Domain nguyên liệu:** `DomainException` thêm `Code` + bảng ánh xạ mã → HTTP trong middleware; `AddIngredient/UpdateIngredient/RemoveIngredient`; không được trống cả `Quantity`, `QuantityText` và `Unit` → 400.
  3. **Domain các bước:** `AddStep` tự gán `StepNumber = Max + 1` (đã có); thêm `RemoveStep` (đánh số lại 1..N) và `ReorderSteps(stepIds)` (tập id phải khớp chính xác). Mọi lần đánh số lại nằm trong **một `SaveChanges`**. Bật mảng `steps?`/`ingredients?` của `POST /recipes` bằng cách gọi lại đúng các hàm này.
  4. **Command + validator:** nguyên liệu `name` 1–200, `quantityText` ≤ 50; bước `title` 1–200 bắt buộc, `description` ≤ 2000, `timerMinutes ≥ 0`; `ReorderStepsCommand`. Tất cả qua `RecipeAuthorizationHandler` và xóa cache.
  5. **API + UI:** CRUD `/recipes/{id}/ingredients`, `/recipes/{id}/steps` (**body không có `stepNumber`**), `PATCH /recipes/{id}/steps/reorder`. Wizard bước 3 "Nguyên liệu" (mỗi dòng có ô số và ô nguyên văn), bước 4 "Các bước" (kéo-thả bằng `dnd-kit`, ảnh minh họa, hẹn giờ).
- **Vì sao:**
  - **2 cột định lượng**: tiếng Việt có "1/2 muỗng", "vừa đủ" không quy ra số được. Chỉ số thì mất nguyên văn; chỉ chữ thì mất khả năng nhân khẩu phần và JSON-LD. Hai cột giữ được cả hai.
  - **Server tự đánh số bước**: client gửi số thì hai request đồng thời hoặc nhập trùng sẽ vỡ unique → lỗi 500. API nhận *ý định* ("thứ tự mới là mảng id này"), không nhận giá trị cột.
  - **D-16 là điều kiện bắt buộc**: PostgreSQL chỉ cho *constraint* hoãn kiểm tra, không cho *index*; không đổi thì kéo-thả luôn lỗi `23505`.
- **Xong khi:** thêm/sửa/xóa nguyên liệu và bước trên wizard; xóa bước giữa và kéo-thả vẫn cho số thứ tự liên tục 1..N, không lỗi 23505.
- **Commit:** `feat(recipes): complete FR-RCP-009 dual-column quantity & FR-RCP-010 server-assigned steps with deferrable reorder`

### Dev 3 – Đoàn Hồng Tiến · FR-SRCH-001 Tìm kiếm toàn văn tiếng Việt (+ D-18)
- **Hướng đi:**
  1. **Migration `B3_Search_FTS`** (thứ tự bắt buộc): `CREATE EXTENSION unaccent, pg_trgm` → hàm `unaccent_immutable` → cột **generated `STORED`** `SearchVector = to_tsvector('simple', unaccent_immutable(Title || ' ' || Description))` → **GIN index**. **D-18:** xóa khối `vietnamese_unaccent` khỏi `init.sql`.
  2. **`SearchRecipesQuery`:** `q` ≥ 2 ký tự (sai → 400); làm sạch ký tự đặc biệt, ghép prefix `pho:* & bo:*`; xếp theo `ts_rank`; **chỉ Published**; trả `relevanceScore`; cache `search:{hash}` 1 phút.
  3. **API:** `GET /recipes/search?q&page&pageSize&categoryId&difficulty` (slug `search` đã bị cấm ở Buổi 2).
  4. **Kiểm chứng DB:** 50 công thức seed tự có vector ngay khi migration chạy; test chạy trên Testcontainers.
  5. **UI:** `SearchBar` trên header (debounce 300ms), trang `/search` (SSR) có tô sáng từ khóa, trạng thái rỗng kèm gợi ý.
- **Vì sao:**
  - **Generated column thay vì trigger**: PostgreSQL tự đảm bảo vector luôn khớp dữ liệu, không đường ghi nào quên cập nhật, và khai báo được trong EF.
  - **`simple` + `unaccent`**: PostgreSQL không có sẵn cấu hình `vietnamese`; tiếng Việt không biến cách nên mất stemming gần như không ảnh hưởng.
  - Không dùng `ILIKE` vì mất `ts_rank` mà FR bắt buộc trả độ liên quan.
  - **D-18:** `init.sql` không chạy trong Testcontainers → mọi thứ code cần phải nằm trong migration.
  - **Cache 1 phút, không invalidate**: truy vấn tìm kiếm rất đa dạng, invalidate tốn hơn lợi.
- **Xong khi:** gõ "pho" ra "Phở bò", "bánh" ra "Bánh mì" – chạy được trên Testcontainers.
- **Commit:** `feat(search): complete FR-SRCH-001 fts with stored generated column simple config and unaccent`

### Dev 4 – Nguyễn Thăng Thiêng · FR-JOB-002 Resize ảnh + FR-JOB-003 Dọn dữ liệu
- **Hướng đi:**
  1. **`ImageResizeJob(imageId)`:** ImageSharp tạo medium 800×600 và thumbnail 300×300, upload MinIO, lưu `MediumUrl`/`ThumbnailUrl`; retry 3 lần, lỗi thì giữ ảnh gốc. Cắm vào `UploadRecipeImageCommand` của Thọ.
  2. **`PermanentPurgeJob`** (03:30 UTC): tìm công thức `IsDeleted` quá 30 ngày bằng `IgnoreQueryFilters()` → **(1)** gom URL ảnh **(2)** xóa cứng trong transaction (cascade dọn con) **(3)** **chỉ sau khi commit** mới xóa file MinIO.
  3. **Khóa phân tán** RedLock `lock:purge-job` 10 phút + `[DisableConcurrentExecution]`; không lấy được khóa thì bỏ lượt.
  4. Đăng ký recurring job, retry 2 lần, log số công thức và số file đã dọn.
  5. FE: `RecipeCard` dùng `ThumbnailUrl` (chưa có thì dùng ảnh gốc).
- **Vì sao:**
  - **Resize bất đồng bộ**: resize 1 ảnh tốn 1–3 giây CPU; resize lỗi cũng không làm hỏng upload.
  - **Commit DB trước, xóa file sau**: nếu ngược lại mà DB rollback thì công thức trỏ tới file đã mất vĩnh viễn; làm đúng thứ tự thì tệ nhất chỉ còn file rác.
  - **Khóa phân tán**: Hangfire không chặn 2 *lượt* của cùng một job chạy chồng (retry, chạy tay, nhiều worker).
  - Job nặng chạy ở **container worker** nên không tranh CPU với request của người đọc.
- **Xong khi:** MinIO có đủ 3 phiên bản ảnh; công thức giả lập xóa 40 ngày trước biến mất khỏi DB **và** MinIO sau khi chạy job từ `/hangfire`.
- **Commit:** `feat(jobs): complete FR-JOB-002 async image resize & FR-JOB-003 permanent purge job with distributed lock`

**Kiểm chứng cuối Buổi 3:** kéo-thả bước không lỗi 23505; tìm "pho" ra "Phở bò" trên Testcontainers; `localStorage` không còn access token; `CreatedByIp` là IP thật.

---

## BUỔI 4 – Hồ sơ cá nhân, sửa & xuất bản công thức, bộ lọc nâng cao, health check

**Mục tiêu:** người dùng quản lý hồ sơ; công thức đi hết vòng đời tới Published an toàn khi nhiều người sửa cùng lúc; lọc/sắp xếp/phân trang hoàn chỉnh; hệ thống tự báo sức khỏe.

**Tiến trình trong buổi:**
1. **Đầu buổi:** Tiến và Thọ **thống nhất interface `RecipeFilterSpec`** – Tiến sẽ sửa `GetRecipesQuery` và trang `/recipes` (code của Thọ); trong buổi này Thọ không sửa hai file đó.
2. Các phần việc còn lại độc lập; cuối buổi merge Thiêng → Quân → Tiến → Thọ.

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
  4. **Index:** migration `B4_Search_CompositeIndexes` tạo 3 composite index; đo `EXPLAIN ANALYZE` trên **≥ 10.000 công thức** (`PerformanceSeeder` bật bằng cờ); ghi kết quả `docs/explain-analyze-b4.md`.
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

### Dev 4 – Nguyễn Thăng Thiêng · FR-OBS-001 Health check
- **Hướng đi:**
  1. **Check:** `AspNetCore.HealthChecks.NpgSql`, `.Redis` (gắn tag `ready`); tự viết `MinioHealthCheck` (không gắn `ready`).
  2. **Endpoint:** `/health` (tất cả, 503 khi lỗi), `/health/live` (không chạy check nào, luôn 200 khi process sống), `/health/ready` (chỉ check tag `ready`).
  3. **Docker:** thêm healthcheck cho `api` (`curl /health/ready` – cài `curl` trong Dockerfile vì image aspnet không có) và `frontend` (`wget`); Nginx `depends_on: api healthy` và `proxy_next_upstream error timeout http_502 http_503 http_504`.
  4. **UI:** `HealthIndicator` (chấm xanh/vàng/đỏ, cập nhật 30 giây) trên dashboard Admin; trang `/dashboard/system`.
  5. **Kiểm chứng:** tắt Redis → `/health/ready` 503, container `api` unhealthy, `/health/live` vẫn 200; bật lại tự hồi phục.
- **Vì sao:**
  - **Live và Ready tách riêng**: Redis chết không được làm container API bị restart liên tục – restart không sửa được Redis.
  - **MinIO không gắn `ready`**: mất MinIO chỉ không tải được ảnh, công thức vẫn đọc được – là suy giảm, không phải sập.
  - **Khối `healthcheck:` bắt buộc**: Docker Compose không tự gọi `/health/ready`; thiếu nó thì 3 endpoint chỉ là URL đẹp.
- **Xong khi:** kịch bản tắt/bật Redis ở bước 5 cho đúng kết quả.
- **Commit:** `feat(obs): complete FR-OBS-001 health checks with docker healthcheck wiring and ui indicator`

**Kiểm chứng cuối Buổi 4:** Index Scan trên ≥ 10.000 bản ghi; `sort=-title` → 400; publish Archived → 409; tắt Redis → ready 503, live 200.

---

## BUỔI 5 – Gia cố bảo mật, xóa mềm & cách ly dữ liệu riêng tư, SEO, log có cấu trúc

**Mục tiêu:** khóa chặt bề mặt tấn công; vá lỗ hổng rò rỉ bản nháp; trang công khai đạt chuẩn SEO; hệ thống quan sát được từ bên ngoài.
> ⚠️ Buổi nặng nhất về trả nợ: D-3 → D-7, trong đó **D-4, D-5 là lỗi bảo mật** (bản nháp bị lộ qua cache công khai).

**Tiến trình trong buổi:**
1. **Việc đầu tiên của Thọ là vá lỗ hổng** (D-4, D-6), sau đó mới làm tính năng mới.
2. Thọ và Tiến cùng gỡ kiểu `RecipeVisibility`: **Tiến gỡ ở query danh mục và merge trước**, Thọ xóa hẳn kiểu này khi rebase. Output Cache chỉ Thọ gỡ.
3. Cuối buổi merge Thiêng → Quân → Tiến → Thọ; test tái hiện lỗ hổng (viết từ Buổi 2) phải chuyển sang xanh.

### Dev 1 – Hoàng Bình Quân · Rate limit, phân quyền, CSP, chặn route
- **Hướng đi:**
  1. **Rate limit** (`AddRateLimiter`): `auth` sliding window 10 req/phút/IP, `upload` 5 req/phút/IP, chung 100 req/phút/IP; bị chặn → 429 + `Retry-After` + `RATE_LIMIT_EXCEEDED` + header `X-RateLimit-*`. Thứ tự pipeline: `UseForwardedHeaders` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization` (có test kiến trúc giữ thứ tự).
  2. Ghi rõ trong `nginx.conf`: ngưỡng Nginx ≥ ngưỡng ứng dụng.
  3. **Rà soát phân quyền:** mọi endpoint ghi có `AuthorPolicy`/`AdminPolicy`; không còn `VerifiedAuthor`; kiểm tra `IsActive` ở cả login lẫn refresh.
  4. **Header bảo mật:** CSP chặt (`script-src 'self'` + nonce, `object-src 'none'`, `frame-ancestors 'none'`), `nosniff`, `Referrer-Policy`; rà CORS không có `*`.
  5. **Chặn route FE** (`middleware.ts`): `/dashboard/**`, `/profile` cần đăng nhập; `/dashboard/categories`, `/dashboard/users` chỉ Admin; component `<RequireRole>`. Test: request đăng nhập thứ 11 → 429; Author gọi API Admin → 403.
- **Vì sao:**
  - **Rate limit phải dựa trên IP thật** (bật ở Buổi 3): nếu không, mọi người dùng chung IP của Nginx → 10 req/phút cho *cả hệ thống*.
  - **Chỉ tin `X-Forwarded-For` từ mạng nội bộ**: header này giả mạo được, tin bừa thì né được rate limit.
  - **Sliding window cho `/auth`** chặn kiểu bắn 10 request cuối cửa sổ + 10 request đầu cửa sổ sau.
  - **CSP là lớp phòng vệ cuối cho refresh token**; chặn route FE chỉ là trải nghiệm, quyền thật kiểm ở backend.
- **Xong khi:** request thứ 11 tới `/auth/login` trong 1 phút → 429; Author → 403 ở mọi API Admin; trang dashboard không vào được khi chưa đăng nhập.
- **Commit:** `feat(security): ip-based rate limiting, content security policy, authorization audit and frontend route guards`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-007 Xóa mềm + FR-RCP-011 Công thức của tôi (+ D-3, D-4, D-6)
- **Hướng đi:**
  1. **Vá lỗ hổng trước (D-4 + D-6):** `RecipeReadRepository` bỏ lọc theo Guest/Author/Admin → **chỉ Published cho mọi người**; query không inject `ICurrentUser` nữa; Draft/Archived qua `/recipes/{slug}` → **404** (thay vì 403). Xóa Output Cache, chuyển sang Redis: `recipes:list:{hash}` 2 phút, `recipe:{slug}` 5 phút. Gỡ `Skip` khỏi test tái hiện lỗ hổng.
  2. **`GetMyRecipesQuery` (FR-RCP-011):** **không cache**; Author chỉ xem của mình, Admin truyền được `authorId`; Author xem của người khác → 403; lọc theo `status`; header `Cache-Control: no-store`.
  3. **Xóa mềm + D-3:** migration `B5_Recipe_PartialUnique` – slug unique chỉ trong bản ghi chưa xóa; `DeleteRecipeCommand` chỉ gán `IsDeleted = true`, **không xóa bản ghi con, không xóa file MinIO** (việc của job dọn ở Buổi 3).
  4. **API:** `DELETE /recipes/{id}` (204), `GET /recipes/mine`.
  5. **UI `/dashboard/recipes`:** gọi `/recipes/mine`, tab Draft/Published/Archived; nút Sửa/Xuất bản/Hủy/Xóa (xác nhận bằng cách gõ tên công thức); `/dashboard` thống kê theo trạng thái.
- **Vì sao:**
  - **Cache nằm trước handler**: response đã vào cache thì request sau không bao giờ chạy tới đoạn kiểm tra quyền → bản nháp Admin xem sẽ lộ cho khách trong suốt thời gian cache. Vì vậy **endpoint công khai chỉ trả dữ liệu công khai (cache thoải mái)**, **dữ liệu riêng đi đường riêng và cấm cache**.
  - Không thêm `userId` vào khóa cache: hit rate sụp và chỉ cần quên ở một endpoint là lỗ hổng quay lại.
  - **Draft trả 404 thay vì 403**: 403 tiết lộ "ở đây có một bản nháp".
  - **Slug unique từng phần** để xóa xong tạo lại được công thức cùng tên; **không xóa file khi xóa mềm** để còn khôi phục được.
  - FR-RCP-006 (Archive) dời trọn sang Buổi 6 để buổi này tập trung vá lỗ hổng.
- **Xong khi:** Admin gọi `GET /recipes` rồi khách gọi lại đúng URL → khách **không** thấy Draft; `/recipes/mine` có `no-store`; xóa `pho-bo` rồi tạo lại vẫn được slug `pho-bo`.
- **Commit:** `fix(recipes)!: close MT-34 draft leak; complete FR-RCP-007 soft delete & FR-RCP-011 private my-recipes`

### Dev 3 – Đoàn Hồng Tiến · Trang chủ, ISR, JSON-LD, Open Graph (+ D-5, D-7)
- **Hướng đi:**
  1. **D-5:** `GetCategoryBySlugQuery` chỉ trả công thức Published, không đọc `ICurrentUser`; cache `categories:detail:{slug}:{hash}` 2 phút.
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

**Kiểm chứng cuối Buổi 5:** test tái hiện lỗ hổng xanh; Draft → 404; `/recipes/mine` có `no-store`; request thứ 11 tới `/auth/login` → 429; log đủ trường, không lộ bí mật.

---

## BUỔI 6 – Quản lý người dùng, khép kín máy trạng thái, sitemap, hạ tầng production

**Mục tiêu:** xong 3 FR cuối (`FR-AUTH-008`, `FR-AUTH-009`, `FR-RCP-006`) → **37/37 FR**; sitemap đúng chỗ; hạ tầng sẵn sàng cho load test.

**Tiến trình trong buổi:**
1. **Đầu buổi Thọ làm `GET /recipes/sitemap` và merge sớm** (không có migration nên không vướng thứ tự merge); trong lúc chờ, Tiến viết `sitemapSource.ts` theo hợp đồng `{ slug, updatedAt }[]` với dữ liệu giả.
2. Thiêng làm Nginx `/media/` và **merge trước**, Tiến gỡ `unoptimized` rồi kiểm chứng sau khi rebase.
3. Chỉ Quân sửa `AuthResponseFactory` (thêm claim `sid`) trong buổi này.
4. Cuối buổi merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · FR-AUTH-008 Admin quản lý tài khoản + FR-AUTH-009 Quản lý phiên
- **Hướng đi:**
  1. **Claim `sid`** trong `AuthResponseFactory` = Id bản ghi refresh token phát cùng cặp.
  2. **FR-AUTH-008:** `GetUsersQuery` (phân trang, tìm theo email/tên, lọc `isActive`, không cache) và `SetUserStatusCommand { userId, isActive, reason? }`: **Admin tự khóa mình → 403**; khóa thì **thu hồi toàn bộ refresh token** của người đó; ghi audit log.
  3. **FR-AUTH-009:** `GetMySessionsQuery` (phiên còn hiệu lực, `isCurrent` so với `sid`, **không bao giờ trả `TokenHash`**); `RevokeSessionCommand` (phiên của người khác → **404**); `RevokeAllSessionsCommand`.
  4. **API:** `GET /users`, `PATCH /users/{id}/status` (Admin); `GET /auth/sessions`, `DELETE /auth/sessions/{id}`, `POST /auth/sessions/revoke-all` – dữ liệu cá nhân gắn `no-store`.
  5. **UI:** `/dashboard/users` (bảng, tìm kiếm, khóa/mở khóa có nhập lý do); `/profile` thêm tab "Phiên đăng nhập" (IP + thời gian, đánh dấu "Thiết bị này", đăng xuất từng thiết bị / tất cả).
- **Vì sao:**
  - **Khóa phải thu hồi token**, không chỉ gán cờ – nếu không người bị khóa vẫn gia hạn phiên bình thường.
  - Access token cũ còn sống tối đa 15 phút là **đánh đổi chấp nhận được** của JWT stateless (thu hồi tức thời phải tra DB ở mọi request).
  - **Chặn Admin tự khóa** để hệ thống không rơi vào cảnh không còn ai mở khóa được.
  - **Nhận diện phiên hiện tại bằng `sid`** (cách của OpenID Connect) – client không phải gửi thêm dữ liệu nhạy cảm.
  - Trả 404 cho phiên người khác để không dò được id.
- **Xong khi:** khóa user → refresh của họ trả 403; Admin tự khóa → 403; `/auth/sessions` đánh dấu đúng thiết bị hiện tại và không chứa `tokenHash`.
- **Commit:** `feat(auth): complete FR-AUTH-008 admin user management & FR-AUTH-009 session management with sid claim`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-006 Lưu trữ / Khôi phục + khép kín máy trạng thái
- **Hướng đi:**
  1. **Bảng chuyển trạng thái tập trung** `RecipeStatusTransitions`:

     | Từ | Hành động | Sang |
     |---|---|---|
     | Draft | publish | Published |
     | Published | unpublish | Draft |
     | Draft hoặc Published | archive | Archived |
     | Archived | unarchive | Draft |

     Ngoài bảng → 409 `RECIPE_INVALID_STATE_TRANSITION`. Sửa `Publish()`/`Unpublish()` (Buổi 4) đi qua cùng bảng.
  2. **Command + API:** `PATCH /recipes/{id}/archive`, `…/unarchive`; xóa cache `recipe:{slug}`, `recipes:list:*`, `categories:detail:*`. **`GET /recipes/sitemap`**: `{ slug, updatedAt }[]` của mọi Published, cache 1 giờ.
  3. **Test đủ 12 tổ hợp** (3 trạng thái × 4 hành động): 5 hợp lệ ra đúng trạng thái, 7 còn lại phải lỗi.
  4. Test `publish → unpublish → publish` giữ nguyên `PublishedAt` lần đầu.
  5. **UI:** nút "Lưu trữ" và "Khôi phục về nháp"; badge màu theo trạng thái; **chỉ hiện nút hợp lệ** (đọc từ cùng bảng, xuất sang FE thành hằng số); gặp 409 thì giải thích và tải lại.
- **Vì sao:**
  - Bản cũ của SRS có trạng thái Archived **vào được mà không ra được** – ẩn vĩnh viễn thì không khác gì xóa.
  - **Unarchive về Draft** (không về trạng thái cũ) để không cần thêm cột và buộc tác giả rà lại nội dung trước khi công khai.
  - **Luật gom vào một bảng** để thêm trạng thái mới chỉ là thêm dòng.
  - **Test cả 12 tổ hợp** vì lỗi kiểu "có một chuyển đổi không tồn tại" chỉ lộ ra khi liệt kê toàn bộ ma trận.
- **Xong khi:** 12/12 tổ hợp đúng; Tiến dùng được `GET /recipes/sitemap`.
- **Commit:** `feat(recipes): complete FR-RCP-006 archive/unarchive closing the recipe status machine with transition matrix tests`

### Dev 3 – Đoàn Hồng Tiến · Sitemap, robots.txt, tối ưu ảnh (+ D-14 phía Next.js)
- **Hướng đi:**
  1. **`app/sitemap.ts`:** toàn bộ công thức Published + danh mục + trang tĩnh; `revalidate = 3600`.
  2. **Nguồn dữ liệu** `lib/seo/sitemapSource.ts` gọi `GET /recipes/sitemap` (một request), không duyệt `GET /recipes` theo trang.
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
  - **Resolver động** tự nhận replica mới trong 10 giây; khối `upstream` chỉ phân giải DNS một lần (đã gây 502 ở Buổi 1).
  - **Một địa chỉ ảnh qua Nginx** xóa bỏ việc `localhost:9000` đúng với trình duyệt nhưng sai với container.
  - **D-17 cùng buổi SSL** vì cùng cần chứng chỉ.
- **Xong khi:** `docker compose -f docker-compose.prod.yml up --scale api=3` chạy; tạo lại một container `api` không gây 502; ảnh tải qua `/media/`; hit rate được đo và ghi lại.
- **Commit:** `chore(devops): production nginx with ssl and /media proxy, prod compose scaling, dataprotection keys and cache audit`

**Kiểm chứng cuối Buổi 6:** 37/37 FR; 12 tổ hợp trạng thái đúng; khóa user thu hồi phiên; sitemap ở domain chính; `--scale api=3` không 502.

---

## BUỔI 7 – Kiểm thử toàn diện, WCAG, load test, đóng gói & bảo vệ

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

**Checklist nghiệm thu cuối dự án:** 37/37 FR · 44/44 endpoint trên Scalar · 17/17 nợ kỹ thuật đã trả · 27/27 mã lỗi có test · 0 chỗ dùng mã 422 · bảng TTL khớp 100% SRS · Nginx/healthcheck khớp SRS §6.5 · 6 ADR · tag `v1.0.0`, CHANGELOG, README < 5 phút.

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

Tài khoản seed: `admin@culinaryblog.local` (mật khẩu `SEED_ADMIN_PASSWORD`), `author1..5@culinaryblog.local` (`SEED_AUTHOR_PASSWORD`). Máy đã có PostgreSQL chiếm cổng 5432 → đặt `POSTGRES_HOST_PORT=5434`.

**Definition of Done mỗi commit:**
```bash
cd backend  && dotnet build && dotnet test
cd frontend && npm run lint && npm run typecheck && npm test && npm run build
```
