# Culinary Blog – Nhóm 20

Blog ẩm thực & nấu ăn: người dùng đăng ký, viết công thức (nguyên liệu, các bước, ảnh), xuất bản, tìm kiếm tiếng Việt không dấu; Admin quản trị danh mục và tài khoản.
**Công nghệ:** .NET 10 Minimal API (Clean Architecture + CQRS/MediatR) · Next.js 15 App Router · PostgreSQL 16 · Redis 7 · MinIO · Hangfire · Nginx · Docker Compose.
**Tài liệu gốc:** [`SPEC/SRS_Culinary_Blog_v1.2.4.md`](SPEC/SRS_Culinary_Blog_v1.2.4.md) (yêu cầu) · [`SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`](SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md) (kế hoạch đầy đủ) · [`SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`](SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md) (69 mâu thuẫn đã giải quyết).

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
| Yêu cầu chức năng (FR) | **39** | 7 nhóm: AUTH 11 · CAT 5 · RCP 11 · SRCH 4 · FILE 2 · JOB 3 · OBS 3 (v1.2.3 thêm FR-AUTH-010, 011 — CR-2026-05) |
| Yêu cầu phi chức năng (NFR) | **30** | Hiệu năng 5 · Bảo mật 7 · Khả dụng 4 · Tin cậy 3 · Bảo trì 4 · Mở rộng 3 · SEO 4 |
| Ràng buộc thiết kế (CONS) | **10** | Kiến trúc, pattern, framework, bảo mật, API, DB, tải lên, kiểm tra hợp lệ (validation), container, logging |
| Endpoint REST | **48** | SRS Chương 8: 45 endpoint **hoàn thành ở Buổi 4**; v1.2.3 thêm `change-password`, `forgot-password`, `reset-password` (CR-2026-05) — làm ở **Buổi 7** |
| Mã lỗi ứng dụng | **30** | SRS Phụ lục B – mỗi mã phải có ít nhất 1 kiểm thử (v1.2.2 thêm `AUTH_USER_NOT_FOUND`, `CONCURRENCY_CONFLICT`; v1.2.3 thêm `AUTH_PASSWORD_RESET_TOKEN_INVALID`) |
| Mâu thuẫn trong SRS đã giải quyết | **69** | MT-01 → MT-69 (CR-2026, CR-2026-02 → CR-2026-06) |
| Dữ liệu mẫu (SRS §2.6.1, CR-2026-03) | **≥ 20 / ≥ 100** | ≥ 20 danh mục, ≥ 100 công thức; mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước chế biến |

Mức ưu tiên (MoSCoW): **M** = bắt buộc · **S** = nên có · **C** = có thì tốt.

## 1.2 Yêu cầu chức năng (39 FR)

**Xác thực & Quản lý người dùng – FR-AUTH (11)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-AUTH-001 | Đăng ký tài khoản (tự đăng nhập ngay, gửi email chào mừng) | M |
| FR-AUTH-002 | Đăng nhập email/mật khẩu (khóa 15 phút sau 5 lần sai) | M |
| FR-AUTH-003 | Đăng nhập / đăng ký bằng Google (ID Token) | M |
| FR-AUTH-004 | Làm mới access token (xoay vòng – rotation + phát hiện dùng lại token) | M |
| FR-AUTH-005 | Đăng xuất, thu hồi refresh token | M |
| FR-AUTH-006 | Xem hồ sơ cá nhân (`/auth/me`) | M |
| FR-AUTH-007 | Cập nhật hồ sơ & ảnh đại diện | M |
| FR-AUTH-008 | [Admin] Danh sách người dùng, khóa / mở khóa tài khoản | S |
| FR-AUTH-009 | Quản lý phiên đăng nhập của chính mình (thiết bị, thu hồi) | C |
| FR-AUTH-010 | Đổi mật khẩu (thu hồi mọi phiên khác) — *mới ở v1.2.3* | M |
| FR-AUTH-011 | Quên & đặt lại mật khẩu qua email (luôn 202, token 30 phút dùng một lần) — *mới ở v1.2.3* | M |

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
| FR-SRCH-002 | Lọc đa tiêu chí (danh mục, độ khó, thời gian nấu/chuẩn bị, khẩu phần) — từ v1.2.3 áp cho cả tìm kiếm toàn văn | M |
| FR-SRCH-003 | Sắp xếp `sortBy` + `sortOrder` (danh sách cho phép 5 trường) | M |
| FR-SRCH-004 | Phân trang offset (mặc định 12, tối đa 50) | M |

**Tệp tin – FR-FILE (2) · Job nền – FR-JOB (3) · Quan sát – FR-OBS (3)**

| Mã | Chức năng | Ưu tiên |
|---|---|---|
| FR-FILE-001 | Tải lên ảnh lên MinIO (≤ 5MB, JPEG/PNG/WebP/AVIF, kiểm tra magic bytes) | M |
| FR-FILE-002 | Xóa ảnh khỏi MinIO (idempotent, chỉ chủ file hoặc Admin) | M |
| FR-JOB-001 | Job gửi email chào mừng (Hangfire, thử lại 1′/5′/30′) | S |
| FR-JOB-002 | Job đổi kích thước (resize) ảnh: medium 800×600, ảnh thu nhỏ (thumbnail) 300×300 | S |
| FR-JOB-003 | Job dọn vĩnh viễn dữ liệu đã xóa mềm quá 30 ngày (03:30 UTC) | S |
| FR-OBS-001 | Health check `/health`, `/health/live`, `/health/ready` | M |
| FR-OBS-002 | Log có cấu trúc (Serilog + CorrelationId) | M |
| FR-OBS-003 | Tracing & metrics (OpenTelemetry) | C |

## 1.3 Yêu cầu phi chức năng (30 NFR)

| Nhóm | Mã | Chỉ tiêu cần đạt |
|---|---|---|
| **Hiệu năng** | PERF-001 | GET có cache: p50 ≤ 150ms · p95 ≤ 500ms · p99 ≤ 1000ms |
| | PERF-002 | ≥ 100 người dùng đồng thời; thêm instance thì thông lượng tăng tuyến tính |
| | PERF-003 | Redis cache-aside là cơ chế cache duy nhất; hit rate ≥ 80%; theo bảng TTL chuẩn — từ v1.2.4 chỉ xóa cache công khai khi thay đổi chạm tới tập Published (CR-2026-06) |
| | PERF-004 | Không N+1; cảnh báo query > 100ms; index chứng minh bằng `EXPLAIN ANALYZE` |
| | PERF-005 | LCP ≤ 2.5s · CLS ≤ 0.1 · INP ≤ 200ms · First Load JS ≤ 200KB |
| **Bảo mật** | SEC-001 | Mật khẩu PBKDF2 qua Identity; ≥ 8 ký tự hoa/thường/số/đặc biệt; khóa sau 5 lần sai |
| | SEC-002 | JWT HS256 15 phút; refresh token 256-bit, chỉ lưu SHA-256; access token chỉ trong bộ nhớ |
| | SEC-003 | Rate limit theo IP thật: auth 10/phút, tải lên 5/phút, chung 100/phút |
| | SEC-004 | Validate mọi input ở tầng Application; tải lên kiểm tra kích thước + magic bytes |
| | SEC-005 | HTTPS TLS 1.2+, HSTS, danh sách cho phép của CORS (whitelist, expose `ETag`/`Retry-After`), CSP có nonce (mọi trang render theo request — MT-67) |
| | SEC-006 | Phân quyền ở tầng Application; không cache dữ liệu phụ thuộc danh tính |
| | SEC-007 | Không commit secret; không ghi mật khẩu/token vào log |
| **Khả dụng** | USE-001 | Responsive 320 / 768 / 1200px |
| | USE-002 | WCAG 2.1 AA |
| | USE-003 | Lỗi RFC 7807, hiện lỗi từng ô trên form |
| | USE-004 | Skeleton, toast, thanh tiến trình % khi tải lên |
| **Tin cậy** | REL-001 | Uptime ≥ 99.5%, có health check |
| | REL-002 | Redis chết → đọc thẳng DB; MinIO chết → chỉ tải lên lỗi 503 |
| | REL-003 | Xóa mềm, giữ 30 ngày; dữ liệu bền qua khởi động lại |
| **Bảo trì** | MAINT-001 | Build 0 cảnh báo (StyleCop, SonarAnalyzer, ESLint) |
| | MAINT-002 | Unit ≥ 80% tầng Application; mọi endpoint có kiểm thử đúng + kiểm thử lỗi; 5 luồng E2E |
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
| CONS-002 | CQRS + MediatR; pipeline 4 behavior: Logging → Kiểm tra hợp lệ → Caching → CacheInvalidation |
| CONS-003 | .NET 10 Minimal API (không Controller); Next.js App Router (không Pages Router) |
| CONS-004 | JWT stateless (15 phút / 7 ngày); mật khẩu qua ASP.NET Core Identity |
| CONS-005 | REST, tiền tố `/api/v1`, lỗi RFC 7807 với `type` = mã lỗi ứng dụng |
| CONS-006 | PostgreSQL duy nhất; migration EF Core Code-First (DDL đặc thù Postgres được phép trong migration) |
| CONS-007 | Tải lên ≤ 5MB, 4 định dạng ảnh, kiểm tra magic bytes |
| CONS-008 | Kiểm tra hợp lệ bằng FluentValidation qua pipeline, không kiểm tra hợp lệ (validate) trong endpoint |
| CONS-009 | Docker multi-stage, chạy người dùng non-root |
| CONS-010 | Serilog; mọi dòng log có CorrelationId, RequestPath, UserId |

## 1.5 Hạ tầng & việc kỹ thuật chung (không gắn với một FR)

- Dữ liệu mẫu theo SRS §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức có nội dung thật, 5 tác giả; bộ sinh dữ liệu mẫu (seeder) idempotent, tự bù cho cơ sở dữ liệu đang có.
- Solution 4 tầng + 4 project kiểm thử, `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion), `AuditInterceptor`, `GlobalExceptionMiddleware`, `PagedResult<T>`.
- Docker Compose 8 service (`nginx, api, hangfire, frontend, postgres, redis, minio, seq`) + `mailhog` cho dev; bản production `docker-compose.prod.yml` (scale `api`×3).
- Nginx: reverse proxy, rate limit vòng ngoài, resolver DNS động, `/hangfire` có Basic Auth, `/media/` phục vụ ảnh, SSL/HSTS.
- Frontend dùng chung: `api-client` (Bearer, tự refresh 401, đọc ProblemDetails — từ Buổi 6 chạy trên `ky`), bộ xử lý lỗi theo mã, TanStack Query (dữ liệu server), Zustand (phiên đăng nhập, chỉ lưu refresh token), React Hook Form + Zod, shadcn/ui, bố cục theo route group `(public)`/`(auth)`/`(dashboard)`, kiểu TypeScript theo miền + kiểm thử chống lệch với Backend, `ImageUploader`.
- Bảo mật & tài liệu API: CORS (expose `ETag`, `Retry-After`), rate limit theo IP thật, API versioning (`Asp.Versioning`), OpenAPI + Scalar `/scalar/v1` với ảnh chụp hợp đồng.
- Bộ integration test dùng chung (WebApplicationFactory + Testcontainers), E2E Playwright, load test k6, Lighthouse, axe.
- Quy trình: gitleaks pre-commit, Definition of Done mỗi commit, thứ tự merge migration, 10 ADR, CHANGELOG, tag `v1.0.0`.

## 1.6 48 endpoint theo module

| Module | Endpoint |
|---|---|
| Auth (15) | `POST /auth/register` · `POST /auth/login` · `POST /auth/google` · `POST /auth/refresh` · `POST /auth/logout` · `GET /auth/me` · `PATCH /auth/me` · `GET /users` · `PATCH /users/{id}/status` · `GET /auth/sessions` · `DELETE /auth/sessions/{id}` · `POST /auth/sessions/revoke-all` · `POST /auth/change-password` · `POST /auth/forgot-password` · `POST /auth/reset-password` *(3 endpoint cuối: v1.2.3)* |
| Categories (5) | `GET /categories` · `GET /categories/{slug}` · `POST /categories` · `PUT /categories/{id}` · `DELETE /categories/{id}` |
| Recipes (13) | `GET /recipes` · `GET /recipes/mine` · `GET /recipes/mine/{id}` · `GET /recipes/search` · `GET /recipes/sitemap` · `GET /recipes/{slug}` · `POST /recipes` · `PUT /recipes/{id}` · `PATCH /recipes/{id}/publish` · `…/unpublish` · `…/archive` · `…/unarchive` · `DELETE /recipes/{id}` |
| Ảnh công thức (3) | `POST /recipes/{id}/images` · `PATCH /recipes/{id}/images/{imageId}` · `DELETE /recipes/{id}/images/{imageId}` |
| Các bước (4) | `POST /recipes/{id}/steps` · `PUT …/steps/{stepId}` · `PATCH …/steps/reorder` · `DELETE …/steps/{stepId}` |
| Nguyên liệu (3) | `POST /recipes/{id}/ingredients` · `PUT …/ingredients/{ingId}` · `DELETE …/ingredients/{ingId}` |
| Files (2) | `POST /files/upload` · `DELETE /files/{**key}` |
| Health (3) | `GET /health` · `GET /health/live` · `GET /health/ready` |

---

# Phần 2 – Chia việc tổng thể

**Nguyên tắc:** Buổi 1 cả nhóm đọc đặc tả và tìm hiểu tính năng; từ Buổi 2 mỗi người giữ **một mảng cố định suốt 7 buổi còn lại** và làm **trọn từ cơ sở dữ liệu → API → giao diện** cho mảng đó. Cuối mỗi buổi hệ thống phải chạy được.

| Thành viên | Làm gì trên trang web | FR |
|---|---|---|
| **Dev 1 – Hoàng Bình Quân** | Mọi thứ về tài khoản: đăng ký, đăng nhập (email + Google), giữ phiên đăng nhập, đổi/quên mật khẩu, hồ sơ cá nhân, Admin khóa tài khoản, bảo mật chung | FR-AUTH-001 → 011 |
| **Dev 2 – Nguyễn Hồng Phúc Thọ** | Mọi thứ về công thức: xem, viết, sửa, ảnh, nguyên liệu, các bước, xuất bản, lưu trữ, xóa, "công thức của tôi" | FR-RCP-001 → 011 |
| **Dev 3 – Đoàn Hồng Tiến** | Danh mục, tìm kiếm – lọc – sắp xếp – phân trang, trang chủ, SEO, sitemap, trải nghiệm & chuẩn truy cập | FR-CAT-001 → 005, FR-SRCH-001 → 004 |
| **Dev 4 – Nguyễn Thăng Thiêng** | Tải lên/xóa ảnh, job nền, health check, log & tracing, Docker, Nginx, cache, hạ tầng kiểm thử, lớp gọi API & bố cục Frontend dùng chung, đóng gói | FR-FILE, FR-JOB, FR-OBS, DevOps |

> **Ngoại lệ ở Buổi 4 (29/09/2026):** để hoàn thành toàn bộ API trong một buổi mà vẫn chia tải đều, **API** xuất bản / lưu trữ / xóa mềm công thức do Thiêng làm, **API** "công thức của tôi" và sitemap do Tiến làm; **giao diện** của các chức năng này vẫn do Thọ làm (sitemap do Tiến).
>
> **Buổi 5 → 8 theo yêu cầu tối thiểu Lab 5 → Lab 8 (12/10/2026)** — kế hoạch §0.3:
> - phần Buổi 5 đã làm trước ngày 12/10 được **giữ nguyên** và bổ sung việc Lab 5;
> - Lab 7/8 giao lại tính năng đã có thì **nâng cấp**, không làm lại;
> - giao diện `/search` ở Buổi 7 do Thiêng làm (theo Lab 7); kiểu TypeScript chia theo Lab 5;
> - đổi/quên mật khẩu và lọc thời gian trên `/search` đi qua **CR-2026-05** (SRS v1.2.3);
> - đo tải Buổi 8 cho thấy hai dòng của NFR-PERF-003 mâu thuẫn nhau (hit rate 75,4 % khi xóa cache công khai ở cả thay đổi bản nháp) → **CR-2026-06** (SRS v1.2.4, MT-69).

**Bảng theo dõi trên GitHub:** [Project "Culinary Blog – Nhóm 20"](https://github.com/users/ThangThieng/projects/9) — mỗi thành viên một issue cha "Họ tên - MSSV", mỗi phần việc từng buổi ở Phần 3 là một sub-issue (Todo / In Progress / Done). Bảng được sinh từ chính README này bằng `python tools/github_board/sync_board.py` (xem trước: `--dry-run`); sửa README rồi chạy lại tool, không sửa tay issue. Khi mở PR, ghi `Closes #<số issue>` để PR tự gắn vào issue.

**Lịch 8 buổi (tóm tắt):**

| Buổi | Dev 1 – Quân | Dev 2 – Thọ | Dev 3 – Tiến | Dev 4 – Thiêng |
|---|---|---|---|---|
| 1 (đã hoàn thành) | Đọc SRS phần xác thực, bảo mật | Đọc SRS phần công thức, mô hình dữ liệu | Đọc SRS phần danh mục, tìm kiếm, SEO | Đọc SRS phần kiến trúc, hạ tầng, tệp, job, quan sát |
| 2 (đã hoàn thành) | Đăng ký, đăng nhập | Danh sách & chi tiết công thức | Danh sách & chi tiết danh mục | Docker + tải lên/xóa ảnh; **yêu cầu bổ sung:** dữ liệu mẫu 20 danh mục / 100 công thức |
| 3 | Google, đăng xuất + lỗi Auth, `IUserRepository` | Tạo công thức nháp + ảnh + lỗi Recipe, `IRecipeRepository` | Admin CRUD danh mục + lỗi Category, `ICategoryRepository` | Sửa mã lỗi 422→400, **nền exception / repository & Unit of Work / middleware**, health check (3 endpoint), dashboard Hangfire, bộ kiểm thử tích hợp |
| 4 — **toàn bộ API** | API: gia hạn phiên, hồ sơ, Admin quản lý tài khoản, quản lý phiên | API: nguyên liệu, các bước, sửa công thức | API: tìm kiếm, lọc/sắp xếp, "công thức của tôi", sitemap; vá rò rỉ bản nháp | API: xuất bản, lưu trữ, xóa mềm; job đổi kích thước, job dọn dữ liệu; kiểm thử 45/45 endpoint |
| 5 — **Lab 5** | CORS, rate limit, API versioning | Tài liệu Scalar `/scalar/v1` + kiểu Recipe; *(đã làm: giao diện soạn/sửa/xuất bản)* | Chuẩn hóa cấu trúc Frontend + kiểu Category/Search; *(đã làm: trang tìm kiếm, composite index)* | Gói `zustand`, `ky` + kiểu Auth/API; *(đã làm: healthcheck Docker, Health UI)* |
| 6 — **Lab 6** | Zustand auth store, chặn route, CSP | shadcn/ui + trang tổng quan dashboard | Giao diện tĩnh, component dùng chung; API lọc thời gian cho tìm kiếm | Lớp gọi API `ky`, xử lý lỗi toàn cục, bố cục route group |
| 7 — **Lab 7** | Đăng nhập/đăng ký, đổi & quên mật khẩu (API + UI), trang hồ sơ + phiên | Trang chi tiết công thức, "công thức của tôi" | Trang chủ, trang danh mục (D-7) | Trang tìm kiếm + bộ lọc; Nginx production, SSL, `/media/`, scale |
| 8 — **Lab 8** | Trang quản lý người dùng; E2E + kiểm thử bảo mật | Biểu mẫu thêm/sửa (RHF + `useFieldArray`), ảnh, máy trạng thái; E2E + đồng thời | JSON-LD, Open Graph, sitemap, WCAG, Lighthouse; E2E tìm kiếm | Serilog, load test k6, đóng gói 8 service, `v1.0.0` |

**Quy trình chung:** mỗi buổi mỗi người tạo nhánh `{mssv}_{HoTenKhongDau}_buoiso{n}` từ `develop` (ví dụ `2312755_NguyenThangThieng_buoiso3`) → **1 commit / người / buổi** → mở **Pull Request vào `develop`** (không bao giờ vào `main`) → merge vào `develop` theo thứ tự **Dev 4 → Dev 1 → Dev 3 → Dev 2** → hết buổi merge `develop` vào `main`. Commit chỉ được merge khi đạt Definition of Done (build 0 cảnh báo, kiểm thử xanh, lint + build frontend xanh, `docker compose up` chạy, đã kiểm thử tay). Commit phải mang **họ tên và email GitHub thật** của người làm (`git config user.name` / `user.email`); **không push code chưa build được**; báo cáo cá nhân đặt tên `SPEC/BAO_CAO_LAB_0{n}_{MSSV}_{HoTenKhongDau}.md` với n = số buổi, nằm cùng nhánh với code.

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

**Mục tiêu:** có khung dự án chạy được bằng Docker và 4 lát cắt đầu tiên: đăng ký/đăng nhập, xem công thức, xem danh mục, tải ảnh lên.

**Năm yêu cầu của giảng viên cho Buổi 2:**

| # | Yêu cầu | Trạng thái | Nơi thực hiện |
|---|---|---|---|
| 1 | Tạo cấu trúc dự án backend theo Clean Architecture | Đã hoàn thành | Commit khung `b058654`: 4 tầng Domain / Application / Infrastructure / API + 4 project kiểm thử, kiểm bằng `LayerDependencyTests` |
| 2 | Cài đặt các gói thư viện cần thiết | Đã hoàn thành | EF Core + Npgsql, Identity, MediatR, FluentValidation, Bogus, Hangfire, Redis, AWSSDK.S3, Serilog, OpenTelemetry… (`CONG_NGHE_VA_PHIEN_BAN.md`) |
| 3 | Cài đặt các lớp entities, configuration, DbContext | Đã hoàn thành | `Domain/Entities`, `Infrastructure/Persistence/Configurations`, `CulinaryBlogDbContext` |
| 4 | Tạo migration, cài đặt các lớp để tạo dữ liệu ngẫu nhiên | Đã hoàn thành | Migration `B1_InitialSchema`, `DatabaseSeeder` (Bogus) |
| 5 | CSDL có dữ liệu ngẫu nhiên cho ít nhất 20 categories, 100 recipes; mỗi recipe ít nhất 10 nguyên liệu và 5 bước chế biến | **Yêu cầu bổ sung, khác SRS v1.2.0 — đã hoàn thành 21/09/2026** | Mục "Yêu cầu bổ sung" của Dev 4 bên dưới; SRS v1.2.1 §2.6.1 (CR-2026-03) |

**Tiến trình đã thực hiện:** 30 phút đầu Dev 4 đẩy commit khung (`b058654`) → 4 người tách nhánh làm song song → merge vào `develop` theo thứ tự Dev 4 → Dev 1 → Dev 3 → Dev 2 → merge `main` (`d2d8243`) → rà soát chạy container và sửa lỗi (`064f582`, `6137d3e`).

### Dev 1 – Hoàng Bình Quân · FR-AUTH-001, FR-AUTH-002 (đã hoàn thành)
- **Đã làm:** `ApplicationUser` + `RefreshToken` (chỉ lưu SHA-256); Identity PBKDF2 100.000 vòng, mật khẩu mạnh, khóa 5 lần/15 phút; `RegisterUserCommand`, `LoginUserCommand`; `POST /auth/register` (201), `POST /auth/login` (200); trang `/auth/login`, `/auth/register` (React Hook Form + Zod).
- **Vì sao:** dùng ASP.NET Core Identity thay vì tự viết bảng người dùng để có sẵn băm mật khẩu chuẩn, khóa tài khoản, role; lưu hash của refresh token ngay từ đầu để lộ DB cũng không mạo danh được ai.
- **Còn nợ:** D-1 (Buổi 3), D-2 và D-12 (Buổi 4); integration test đăng ký/đăng nhập viết khi có bộ kiểm thử ở Buổi 3.
- **Commit:** `3d40be6 feat(auth): complete FR-AUTH-001 & 002 register login flow`

### Dev 2 – Nguyễn Hồng Phúc Thọ · FR-RCP-001, FR-RCP-002 (đã hoàn thành)
- **Đã làm:** toàn bộ schema công thức (Recipe, Step, Ingredient, Image, Nutrition dạng owned); tạo mẫu (seed) 50 công thức / 5 tác giả bằng Bogus (nâng lên 100 công thức ở yêu cầu bổ sung); `GET /recipes` (phân trang, lọc), `GET /recipes/{slug}`; trang `/recipes` (SSR) và `/recipes/[slug]` (ISR 300s).
- **Vì sao:** dựng đủ schema ngay buổi đầu vì schema là thứ đắt nhất để sửa khi đã có dữ liệu; truy vấn đọc dùng `AsNoTracking` + projection để tránh N+1 từ gốc.
- **Còn nợ:** D-3, D-4, D-6 (Buổi 6), D-8 (Buổi 3), D-15 (Buổi 5), D-16 (Buổi 4).
- **Commit:** `0fc95d1 feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view`

### Dev 3 – Đoàn Hồng Tiến · FR-CAT-001, FR-CAT-002 (đã hoàn thành)
- **Đã làm:** entity `Category` + tạo mẫu 8 danh mục (nâng lên 20 ở yêu cầu bổ sung); `SlugHelper` bỏ dấu tiếng Việt; `RedisCacheService` (Redis lỗi thì đọc DB); `GET /categories`, `GET /categories/{slug}`; trang `/categories`, `/categories/[slug]`.
- **Vì sao:** chọn Redis ngay từ đầu (không `IMemoryCache`) vì chạy nhiều instance thì cache trong bộ nhớ không xóa cache (invalidate) được.
- **Còn nợ:** D-13 (Buổi 3), D-18 (Buổi 4), D-5, D-7 (Buổi 6); Value Object `Slug` và component `CategoryNav` bổ sung ở Buổi 3.
- **Commit:** `fd27a71 feat(category): complete FR-CAT-001 & 002 public categories API and UI`

### Dev 4 – Nguyễn Thăng Thiêng · Hạ tầng + FR-FILE-001, FR-FILE-002 (đã hoàn thành)
- **Đã làm:** commit khung cho cả nhóm; Docker Compose 8 service + mailhog, Dockerfile multi-stage non-root; `MinioFileStorageService` (AWSSDK.S3); kiểm tra magic bytes; `POST /files/upload`, `DELETE /files/{**key}`; component `ImageUploader` (kéo-thả, % tiến trình); làm sớm FR-JOB-001 (email chào mừng qua Hangfire).
- **Vì sao:** dùng AWSSDK.S3 để sau này đổi sang AWS S3 thật chỉ cần đổi cấu hình; kiểm magic bytes vì `Content-Type` do client khai là giả mạo được; tên file do server sinh (`{Guid}`) để chặn path traversal.
- **Sửa sau buổi:** chờ DB sẵn sàng khi khởi động, Nginx 502 khi container api đổi IP, hook gitleaks trên Windows (`064f582`); ảnh gần 5MB bị Nginx chặn, log khởi động (`6137d3e`).
- **Commit:** `b058654 chore: bootstrap…` · `9dc3d81 feat(infra): complete docker compose setup and FR-FILE minio upload component`

### Dev 4 – Nguyễn Thăng Thiêng · Yêu cầu bổ sung: dữ liệu mẫu (đã hoàn thành, 21/09/2026)
> **Yêu cầu mới, khác file SRS v1.2.0:** SRS v1.2.0 §2.6.1 chỉ yêu cầu 50 công thức và 5 tác giả mẫu. Giảng viên yêu cầu CSDL có ít nhất **20 danh mục, 100 công thức; mỗi công thức ít nhất 10 nguyên liệu và 5 bước chế biến**. Yêu cầu đã được ghi vào **SRS v1.2.1** (CR-2026-03, MT-58) trước khi code.
- **Đã làm:** catalog dữ liệu thật trong `Infrastructure/Persistence/Seed/Data/` — 20 danh mục (8 cũ + 12 mới: Cơm & Xôi, Lẩu, Hải sản, Gỏi & Salad, Món cuốn, Món hấp, Món chiên, Ăn vặt, Món Hàn Quốc, Món Nhật Bản, Món Thái, Món Âu) và 100 món (50 món cũ giữ nguyên tên, slug, danh mục + 50 món mới), mỗi món 10 – 14 nguyên liệu có định lượng và 5 – 6 bước có mô tả, thời gian; `RecipeSeedCatalog` đọc catalog và khai báo ngưỡng; `DatabaseSeeder` idempotent, tự bù; xóa cache danh mục và Output Cache công thức khi dữ liệu thay đổi; `RecipeSeedCatalogTests` (5 kiểm thử).
- **Vì sao:** Bogus chọn ngẫu nhiên từ danh sách nguyên liệu chung sẽ ra món sai (sinh tố có nước mắm, bước nấu bằng chữ Latin), trong khi dữ liệu này dùng để kiểm thử tìm kiếm, đo hiệu năng và trình diễn. Vì vậy nội dung món ăn viết tay; Bogus (seed cố định) chỉ sinh tác giả, trạng thái, ngày xuất bản, dinh dưỡng. Bộ sinh dữ liệu mẫu tự bù để không ai phải xóa volume: công thức mẫu cũ chưa từng bị sửa được thay bằng nội dung đúng, dữ liệu người dùng không bị động tới.
- **Xong khi (đã kiểm chứng trên Docker):** cơ sở dữ liệu cũ → log *"50 recipes created, 50 recipes repaired"*; cơ sở dữ liệu trống → *"100 recipes created"*; khởi động lại → *"0 created, 0 repaired"*; SQL: 20 danh mục, 100 công thức, tối thiểu 10 nguyên liệu và 5 bước mỗi món, số bước liên tục, không còn Lorem ipsum; `dotnet test` 49/49.
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
| Middleware Problem Details | Ánh xạ lỗi Auth → HTTP | Ánh xạ lỗi Recipe → HTTP | Ánh xạ lỗi Category → HTTP | `GlobalExceptionMiddleware` + bảng ánh xạ + kiểm thử hợp đồng |
| API mới (≥ 2) | 2: `POST /auth/google`, `POST /auth/logout` | 4: `POST /recipes`, `POST`/`PATCH`/`DELETE /recipes/{id}/images…` | 3: `POST`/`PUT`/`DELETE /categories` | 3: `GET /health`, `/health/live`, `/health/ready` |

Tổng **12 API mới**. `POST /files/upload` đã làm ở Buổi 2 nên không tính.

**Tiến trình trong buổi:**
1. **CR-2026-04 — ✅ đã áp dụng (SRS v1.2.2):** 2 mã lỗi `AUTH_USER_NOT_FOUND` (404) và `CONCURRENCY_CONFLICT` (409) đã có trong Phụ lục B. Chi tiết ở kế hoạch §4.5.
2. **Commit nền D-11 (422 → 400)** — ✅ đã xong (commit `67d29c0`).
3. **Mốc M1 (chậm nhất giữa buổi):** Thiêng merge **commit nền kiến trúc** vào `develop`. Trong lúc chờ, 3 người còn lại làm phần không phụ thuộc:
   - Quân: D-1 + nút Google;
   - Thọ: D-8 + validator + biểu mẫu nhiều bước (wizard);
   - Tiến: validator + trang quản trị + D-13.
4. **Sau M1:** 3 người rebase, viết exception + repository + file ánh xạ của mảng mình, rồi handler và kiểm thử.
5. **Cuối buổi:** merge Thiêng → Quân → Tiến → Thọ.
   - `IUnitOfWork.cs`, `UnitOfWork.cs` chỉ được **thêm dòng** vào phần của mảng mình; `ErrorCodes.cs` đã có đủ 29 mã từ commit nền của Thiêng.
   - File ánh xạ lỗi và bộ dịch lỗi DB mỗi mảng một file riêng, được quét tự động.

### Commit nền – Nguyễn Thăng Thiêng dẫn · D-11 (422 → 400) — ✅ đã xong
- **Hướng đi:**
  - `GlobalExceptionMiddleware` trả **400** + `type = VALIDATION_ERROR`; đổi mọi `Status422UnprocessableEntity` trong endpoint.
  - Frontend: `LoginForm`, `RegisterForm` đổi `status === 422` → `400`; gom việc gắn lỗi vào từng ô thành helper `mapProblemDetailsToForm()` trong `lib/api-client.ts`.
  - Sửa kiểm thử cũ; thêm kiểm thử chặn chuỗi `Status422` quay lại.
- **Vì sao làm trước tiên:** mọi endpoint mới của buổi đều trả lỗi kiểm tra hợp lệ. Nếu đổi backend mà quên frontend, form sẽ **âm thầm mất lỗi từng ô** – không sập, không log, chỉ người dùng nhập sai mới phát hiện.
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

     Ghi người dùng (tạo, liên kết Google, khóa) vẫn qua `UserManager`. `RefreshToken.Revoke()` kéo từ Buổi 4 lên.
  3. **D-1:** `IUserNameGenerator` sinh `UserName` từ email (trùng thì thêm `2, 3…`); đăng ký nhận `{ email, password, displayName }`; `UserDto` đổi sang `displayName`, thêm `bio`; bỏ ô "Tên đăng nhập" trên form.
  4. **FE Google:** cài `@react-oauth/google`, dùng **component `<GoogleLogin>`** để lấy **ID Token** rồi gửi `POST /auth/google`. Không dùng hook `useGoogleLogin` (hook đó trả access token, backend không xác minh được).
  5. **`GoogleLoginCommand`:** xác minh qua `IGoogleIdTokenValidator` (bọc `GoogleJsonWebSignature.ValidateAsync`, kiểm chữ ký, `iss`, `aud`, `exp`).
     - Đã liên kết Google → đăng nhập.
     - Email đã tồn tại → **chỉ liên kết khi `EmailVerified == true`**.
     - Mới hoàn toàn → tạo người dùng role Author.
     - Lỗi: token thiếu dữ liệu 400, chữ ký sai 401, không lấy được khóa Google 502, tài khoản bị vô hiệu 403.
  6. **`LogoutCommand`:** hash token nhận được → `Revoke()`; không tìm thấy vẫn trả 204.
  7. **API + UI + kiểm thử:**
     - API: `POST /auth/google` (200), `POST /auth/logout` (204).
     - UI: nút Google ở `/auth/login` kèm thông báo khi Google lỗi; menu "Đăng xuất" gọi API rồi mới xóa phiên phía client.
     - Test (Google giả trong kiểm thử): 200 / 400 / 401 / 403 / 502; 2 email cùng tiền tố (prefix) → `an.nguyen`, `an.nguyen2`; đăng xuất 2 lần đều 204; khóa do sai mật khẩu vẫn 423.
- **Vì sao:**
  - **ID Token flow** giữ backend stateless: không lưu state/PKCE verifier, không cần route callback hay client secret.
  - **Backend bắt buộc tự xác minh chữ ký**: tin email do FE gửi lên thì ai cũng `curl` chiếm được tài khoản người khác.
  - **Chỉ liên kết khi email đã xác minh**: nếu không, kẻ xấu tạo tài khoản Google gắn email nạn nhân là chiếm được tài khoản.
  - **Đăng xuất luôn 204**: trả 404 sẽ biến endpoint thành công cụ dò token hợp lệ.
  - **Không ghi thẳng bảng người dùng**: `UserManager` lo chuẩn hóa email, `SecurityStamp`, đếm lần sai để khóa — tự ghi bảng sẽ bỏ sót mà không báo lỗi.
  - **D-1 làm ngay buổi này** vì Google tạo người dùng mà không ai nhập `userName`, và `UserDto` đổi sớm thì ít màn hình phải sửa.
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
     - dinh dưỡng gửi kèm body (không có endpoint riêng).
  5. **`RecipeAuthorizationHandler`:** qua khi là chủ công thức hoặc Admin – dùng lại cho **mọi** FR-RCP về sau.
  6. **Ảnh:**
     - `UploadRecipeImageCommand`: dùng lại `IFileStorageService` + `ImageFileInspector` của Buổi 2, ảnh đầu tiên tự là ảnh chính, **để sẵn chỗ `BackgroundJob.Enqueue` cho job đổi kích thước**;
     - `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`;
     - `DeleteRecipeImageCommand`: xóa ảnh chính thì ảnh có `OrderIndex` nhỏ nhất lên thay.
     - Đổi ảnh chính làm **2 lần `SaveChanges` trong 1 transaction** (`ExecuteInTransactionAsync`).
     - Mọi command xóa cache `recipes:list:*` + `recipe:{slug}`.
  7. **API + UI:**
     - API: `POST /recipes` (201), `POST/PATCH/DELETE /recipes/{id}/images…` (MinIO lỗi → 503).
     - Trang `/dashboard/recipes/new` dạng biểu mẫu nhiều bước: bước 1 thông tin + dinh dưỡng, bước 2 thư viện ảnh (chọn ảnh chính bằng radio).
     - Kiểm thử: đổi ảnh chính 10 lần liên tiếp không lần nào vỡ unique index; hai phiên cùng sửa một công thức → 409.
- **Vì sao:**
  - **Luôn tạo ở trạng thái Draft** để có điểm chặn kiểm tra điều kiện xuất bản (đủ bước + nguyên liệu) ở Buổi 4 (API xuất bản).
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
     - Kiểm thử: 201 / 403 / 409 trùng tên / 2 request trùng tên song song → một 201, một 409, không có 500 / 409 còn công thức.
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
     - `ExecuteInTransactionAsync` chạy qua execution strategy vì DbContext bật thử lại;
     - `UnitOfWork` dịch lỗi ghi DB qua bộ dịch của từng mảng.
  3. **Middleware:**
     - `GlobalExceptionMiddleware` tra `ExceptionStatusMap` (mỗi mảng một file ánh xạ, quét tự động);
     - kiểm thử kiến trúc: exception nào chưa đăng ký ánh xạ là kiểm thử đỏ;
     - kiểm thử hợp đồng Problem Details cho 400/401/403/404/409/423/500/502/503;
     - kiểm thử rollback transaction trên PostgreSQL thật.
  4. **Health check (FR-OBS-001, kéo từ Buổi 5 lên):**
     - `/health` (tất cả, 503 khi lỗi); `/health/live` (luôn 200 khi process sống); `/health/ready` (PostgreSQL + Redis);
     - MinIO chỉ làm `/health` thành `Degraded`;
     - kiểm thử: Redis sai địa chỉ → ready 503, live vẫn 200.
  5. ✅ **Dashboard:** `MapHangfireDashboard("/hangfire")` (chỉ ở container `api`) với filter `NginxGateDashboardFilter`: chỉ cho qua khi header `X-Hangfire-Gate` khớp secret (so sánh `FixedTimeEquals`).
  6. ✅ **Nginx:**
     - `location /hangfire` có **Basic Auth** (`htpasswd`, không commit) và gắn header gate;
     - chuyển `nginx.conf` sang thư mục `templates/` để nạp secret bằng `envsubst`;
     - thêm biến giả vào `.env.example`.
  7. ✅ **Bộ kiểm thử tích hợp** trong `tests/CulinaryBlog.API.IntegrationTests`:
     - `CulinaryBlogApiFactory` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, chạy migration, tắt Hangfire server, thay `IFileStorageService`/`IBackgroundJobClient` bằng bản giả;
     - `Respawn` dọn DB giữa các kiểm thử;
     - helper `CreateClientAs(role)` phát JWT thật.
  8. ✅ **Trả nợ kiểm thử Buổi 2:** integration test cho mọi endpoint Buổi 2; viết sẵn kiểm thử tái hiện lỗ hổng rò rỉ bản nháp (đánh dấu `Skip` – Tiến gỡ ở Buổi 4).
- **Vì sao:**
  - **Thiêng làm phần nền, chủ mảng làm phần của mảng mình**: phần không thuộc mảng nào chỉ nên có một kiểu viết; còn "tên danh mục trùng trả mã gì" là kiến thức của Tiến và Tiến dùng lại nó tới Buổi 7.
  - **Exception không mang mã HTTP**: Domain không được biết mình chạy sau một web API; tầng API quyết định mã HTTP.
  - **Ánh xạ theo kiểu, mỗi mảng một file**: trình biên dịch kiểm tra tên lớp, không ai sửa chung một `switch` lớn, và kiểm thử bắt được lớp bị quên.
  - **Không có xóa cứng trong repository**: mọi xóa là xóa mềm; xóa vật lý chỉ ở job dọn dữ liệu (Buổi 4).
  - **Health check lên Buổi 3**: FR mức M vốn của Thiêng, và kiểm chứng cuối mỗi buổi đã gọi `/health` trong khi chưa có endpoint nào. Không đi qua MediatR vì Docker gọi mỗi 10 giây, cần ít phụ thuộc nhất.
  - **Dựng bộ kiểm thử ngay Buổi 3** vì Definition of Done đòi kiểm thử xanh ở mọi commit; để tới cuối dự án thì 5 buổi liền không ai viết được integration test.
  - **Testcontainers thay vì EF In-Memory** vì In-Memory không có unique, FK, `tsvector`, index một phần, concurrency token, transaction – đúng những thứ cần kiểm nhất.
  - **Hai lớp bảo vệ dashboard:** trình duyệt không gửi được Bearer token khi mở trang, nên dùng Basic Auth ở Nginx; header gate chặn trường hợp gọi thẳng cổng `5000` bỏ qua Nginx.
- **Xong khi:**
  - `http://localhost/hangfire` hỏi mật khẩu rồi vào được; `http://localhost:5000/hangfire` trả 401;
  - `curl http://localhost/health` trả `Healthy`;
  - kiểm thử kiến trúc + kiểm thử hợp đồng Problem Details + integration test xanh trên máy có Docker.
- **Commit:**
  - `feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness` — ✅ `67d29c0`
  - `feat(infra): add base domain exception, unit of work, global exception middleware and health check APIs` — ✅ code xong và kiểm chứng 29/09/2026 (117 kiểm thử xanh; `/health` thử MinIO/Redis sập trên `docker compose`), chờ commit

**Kiểm chứng cuối Buổi 3:**
- Có đủ 3 cụm exception theo mảng + lớp gốc; không handler nào tham chiếu `DbContext`.
- Mọi lỗi trả `application/problem+json` đúng `type`; lỗi 500 không lộ stack trace.
- 12 API mới đều có integration test.
- Không còn chuỗi `Status422` / `status === 422` trong code; form đăng ký hiện lỗi từng ô với 400.
- 2 email cùng tiền tố sinh `UserName` có hậu tố số.
- `/hangfire` được bảo vệ; `/health` trả `Healthy`.

---

## BUỔI 4 – Hoàn thành toàn bộ API: phiên & tài khoản, nội dung công thức, truy vấn & tìm kiếm, vòng đời công thức, job nền

**Mục tiêu (yêu cầu của giảng viên, 29/09/2026): "Hoàn thành việc cài đặt tất cả API endpoints".**
- Cuối buổi, **45/45 endpoint** của SRS v1.2.2 Chương 8 cài xong **đúng hợp đồng**: mã HTTP, mã lỗi, DTO, cache.
- Mọi endpoint có integration test và hiện đủ trên Scalar.
- Hai job nền còn lại chạy thật.
- **Giao diện** của các chức năng này làm ở Buổi 5 → 8 (phân bổ lại theo yêu cầu Lab 5 → Lab 8, 12/10/2026).

**Kết quả kiểm tra kế hoạch cũ — không đạt:**

| # | Vấn đề | Hệ quả |
|---|---|---|
| 1 | Buổi 4 cũ chỉ thêm 9 endpoint | Hết Buổi 4 mới có **29/44**. Còn thiếu 15: `/auth/me` ×2, `/users` ×2, `/auth/sessions` ×3, `PUT /recipes/{id}`, xuất bản, hủy xuất bản, lưu trữ, khôi phục, `DELETE /recipes/{id}`, `/recipes/mine`, `/recipes/sitemap` — nằm rải ở Buổi 5, 6, 7 |
| 2 | 3 endpoint đã có còn sai hợp đồng tới Buổi 5–6 | `GET /recipes` rò bản nháp (MT-34) và dùng `sort=-field`; `GET /recipes/{slug}` trả 403 cho bản nháp; `GET /categories/{slug}` lọc theo danh tính |
| 3 | SRS thiếu endpoint cho trang sửa bản nháp | Cài đủ 44 endpoint thì Author vẫn không sửa được bản nháp → bổ sung **`GET /recipes/mine/{id}`** qua CR-2026-04 (SRS v1.2.2, **45 endpoint**) |
| 4 | Dồn endpoint theo đúng module cũ | Thọ phải làm 15 endpoint, Thiêng 0 |

**Chia việc mới (API-first, cân bằng theo năng lực):**

| Người | Nhóm việc | Endpoint mới | Sửa hợp đồng | Kèm theo |
|---|---|---|---|---|
| Quân | Phiên & tài khoản (FR-AUTH-004, 006–009) | 8 | — | D-2, IP thật, claim `sid` |
| Thọ | Nội dung công thức (FR-RCP-009, 010, 004) | 8 | `POST /recipes` (mảng bước/nguyên liệu) | D-16 |
| Tiến | Truy vấn công khai & riêng tư (FR-SRCH-001–004, FR-RCP-011, sitemap) | 4 | `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` | D-18, **vá MT-34** (D-4, D-5, D-6), D-10 |
| Thiêng | Vòng đời công thức & job nền (FR-RCP-005, 006, 007, FR-JOB-002, 003) | 5 | — | D-3, D-15, kiểm thử bề mặt API 45/45 |

**Kết quả:** 20 (Buổi 2 + 3) + 25 = **45/45 endpoint**.

- **Giao diện vẫn theo mảng cố định:** Thọ làm giao diện xuất bản, lưu trữ, xóa và "công thức của tôi" (Buổi 5 – 8); Tiến làm sitemap Next.js (Buổi 8).
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
     - **token đã thu hồi mà vẫn được dùng** → lưu việc thu hồi toàn bộ token của người dùng **trước**, log cảnh báo, rồi 401 `AUTH_REFRESH_TOKEN_REVOKED`;
     - tài khoản bị khóa → 403;
     - hợp lệ → trong 1 transaction thu hồi token cũ và phát cặp mới.
  3. **Claim `sid`** (id phiên) trong mọi access token; **`UseForwardedHeaders`** với `KnownIPNetworks`.
  4. **Hồ sơ (kéo từ Buổi 5):** `GET /auth/me` (không trả trường nhạy cảm; người dùng không còn → 401); `PATCH /auth/me` (partial; ảnh đại diện phải là URL MinIO của hệ thống; không đổi email/username).
  5. **Quản lý tài khoản [Admin] (kéo từ Buổi 7):**
     - `GET /users` (phân trang, tìm kiếm, không cache);
     - `PATCH /users/{id}/status` — tự khóa mình → 403; người dùng không tồn tại → 404 `AUTH_USER_NOT_FOUND`; khóa thì thu hồi mọi refresh token trong cùng transaction; ghi audit log.
  6. **Quản lý phiên (kéo từ Buổi 7):**
     - `GET /auth/sessions` (không bao giờ trả `tokenHash`; `isCurrent` theo `sid`);
     - `DELETE /auth/sessions/{id}` (phiên của người khác → 404);
     - `POST /auth/sessions/revoke-all`.
  7. **API — 8 endpoint mới:** `POST /auth/refresh`, `GET/PATCH /auth/me`, `GET /users`, `PATCH /users/{id}/status`, `GET /auth/sessions`, `DELETE /auth/sessions/{id}`, `POST /auth/sessions/revoke-all`. Giao diện: D-12 + interceptor đã làm sớm ngay ở Buổi 4; `/profile` + tab phiên ở Buổi 7; `/dashboard/users` ở Buổi 8 (cập nhật 12/10/2026).
- **Vì sao:**
  - **Rotation + phát hiện dùng lại** là cách duy nhất trong JWT stateless để biết token bị đánh cắp.
  - **Chỉ lưu hash**: lộ DB cũng không có token dùng được.
  - **Gom 4 FR tài khoản vào cùng buổi với refresh**: tất cả xoay quanh một cặp *người dùng + tập refresh token*. Các hàm thu hồi và danh sách phiên được thiết kế một lần cho mọi nhu cầu, thay vì vá dần qua 3 buổi.
  - **Bật IP thật ngay buổi này**: từ đây mỗi lần refresh ghi `CreatedByIp`, và danh sách phiên hiển thị IP đó.
  - **Danh sách người dùng / phiên không cache**: dữ liệu riêng tư theo người gọi.
- **Xong khi:**
  - dùng lại token cũ → cả họ token bị thu hồi;
  - khóa người dùng → refresh của họ trả 403;
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
  - **D-16 bắt buộc**: PostgreSQL chỉ cho *constraint* hoãn kiểm tra; không đổi thì sắp xếp lại luôn lỗi `23505`.
  - **`PUT` cùng buổi với bước/nguyên liệu**: cùng aggregate, cùng hàm nạp dữ liệu, cùng phân quyền → viết một lần.
- **Xong khi:**
  - đổi chỗ bước 2 và 3 bằng sắp xếp lại → 200, không lỗi 23505;
  - `PUT` với `rowVersion` cũ → 409;
  - `POST /recipes` kèm 10 nguyên liệu + 5 bước → 201.
- **Commit:** `feat(recipes): complete FR-RCP-009/010 ingredients & server-assigned steps with deferrable reorder and FR-RCP-004 rowversion update API`

### Dev 3 – Đoàn Hồng Tiến · API truy vấn: FR-SRCH-001–004, FR-RCP-011, sitemap (+ vá MT-34: D-4, D-5, D-6; D-10, D-18)
- **Hướng đi:**
  1. **Vá MT-34 trước tiên (kéo từ Buổi 6):**
     - `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` **chỉ trả Published** cho mọi người gọi;
     - bản nháp qua `/recipes/{slug}` → **404** (không phải 403);
     - bỏ Output Cache, chuyển sang cache Redis `ICacheable` (2′ danh sách, 5′ chi tiết, 2′ chi tiết danh mục);
     - gỡ `Skip` khỏi kiểm thử tái hiện lỗ hổng.
  2. **Migration `B4_Search_FTS`** (thứ tự bắt buộc): `unaccent`, `pg_trgm` → hàm `unaccent_immutable` → cột **generated `STORED`** `SearchVector` → **GIN index**. **D-18:** xóa `vietnamese_unaccent` khỏi `init.sql`.
  3. **Lọc / sắp xếp / phân trang (kéo từ Buổi 5):**
     - `RecipeFilterSpec` dùng chung cho 4 truy vấn (danh sách, tìm kiếm, danh mục, của tôi);
     - `SortMapper` danh sách cho phép 5 trường; ngoài danh sách cho phép → 400;
     - `pageSize` mặc định 12, tối đa 50;
     - **D-10 phía API**: `sort=-field` → `sortBy` + `sortOrder`; thêm `maxPrepTime`, `minServings`.
  4. **`SearchRecipesQuery`:** `q` ≥ 2 ký tự; ghép tiền tố `pho:* & bo:*`; xếp theo `ts_rank`; chỉ Published; cache 1 phút.
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

### Dev 4 – Nguyễn Thăng Thiêng · API vòng đời công thức (FR-RCP-005, 006, 007) + FR-JOB-002, FR-JOB-003 (+ D-3, D-15, kiểm thử bề mặt API)
- **Hướng đi:**
  1. **Máy trạng thái `Recipe.Lifecycle.cs`:**
     - bảng `RecipeStatusTransitions`: Draft ⇄ Published, (Draft | Published) → Archived, Archived → **Draft**;
     - ngoài bảng → 409 `RECIPE_INVALID_STATE_TRANSITION`;
     - **D-15:** xuất bản chỉ từ Draft; thiếu bước/nguyên liệu → `RecipePublishIncompleteException` 400; `PublishedAt` chỉ gán lần đầu;
     - thêm `SoftDelete()`.
  2. **5 command** xuất bản / hủy xuất bản / lưu trữ / khôi phục / delete:
     - nạp đủ bước + nguyên liệu, qua `RecipeAuthorizationHandler`;
     - xóa cache danh sách, chi tiết, danh mục và sitemap;
     - xóa mềm **không** xóa con và **không** xóa file.
  3. **D-3 (kéo từ Buổi 6):** migration `B4_Recipe_PartialUniqueSlug` — slug unique **chỉ trên công thức chưa xóa**; bỏ `IgnoreQueryFilters()` ở `SlugExistsAsync`.
  4. **API — 5 endpoint mới** trong `RecipeLifecycleEndpoints`: `PATCH /recipes/{id}/publish`, `/unpublish`, `/archive`, `/unarchive`, `DELETE /recipes/{id}`.
  5. **`ImageResizeJob`:** medium 800×600 + ảnh thu nhỏ 300×300, thử lại 3 lần, lỗi thì giữ ảnh gốc; cắm vào tải ảnh lên của Thọ.
  6. **`PermanentPurgeJob`** (03:30 UTC):
     - xóa cứng công thức đã xóa mềm > 30 ngày;
     - **commit DB trước, xóa file MinIO sau**;
     - khóa phân tán RedLock.
  7. **Kiểm thử:**
     - ma trận **12 tổ hợp** trạng thái × hành động;
     - xóa rồi tạo lại cùng tên được slug cũ;
     - job dọn đúng bản ghi và file;
     - **`ApiSurfaceTests`**: đọc route thật của ứng dụng, so với **45 endpoint** của SRS — thiếu hoặc thừa là đỏ.
- **Vì sao:**
  - **Vòng đời trọn vẹn trong tay một người**: trạng thái → lưu trữ → xóa mềm → dọn sau 30 ngày là một chuỗi liền mạch. Tách cho hai người là tạo lại đúng loại khe hở đã phải sửa trong SRS (Archived vào được không ra được; xóa mềm không ai dọn).
  - **Cân bằng tải**: Thọ giữ 8 endpoint nội dung, không phải gánh 15.
  - **`partial class` + 3 file endpoint**: ba người cùng sửa module Recipe trong một buổi mà không xung đột merge.
  - **`ApiSurfaceTests`**: biến "đã cài tất cả API" thành kiểm thử chạy ở mọi commit tới Buổi 8. Chiều "thừa" chặn endpoint ngoài SRS.
  - **Commit DB trước, xóa file sau**: sai thứ tự thì có thể mất dữ liệu vĩnh viễn; đúng thứ tự thì tệ nhất chỉ còn file rác.
- **Xong khi:**
  - ma trận 12 tổ hợp xanh;
  - xuất bản công thức thiếu nguyên liệu → 400; xuất bản công thức Archived → 409;
  - `ApiSurfaceTests` khớp 45/45;
  - purge job dọn đúng DB **và** MinIO.
- **Commit:** `feat(recipes,jobs): complete FR-RCP-005/006/007 lifecycle APIs with closed state machine, FR-JOB-002 image resize & FR-JOB-003 purge job, api surface test 45/45`

**Kiểm chứng cuối Buổi 4:**
- `ApiSurfaceTests` xanh — **45/45 endpoint**, không thiếu, không thừa.
- Mọi endpoint có ≥ 1 kiểm thử thành công + ≥ 1 kiểm thử lỗi; mọi lỗi là `application/problem+json` đúng mã Phụ lục B.
- Test MT-34 xanh, không còn `CacheOutput` trong code.
- Tìm "pho" ra "Phở bò"; kéo-thả bước không lỗi 23505.
- Dùng lại refresh token cũ → cả họ token bị thu hồi.
- Scalar hiển thị đủ 45 endpoint.

---

## BUỔI 5 – Lab 5: bảo mật API, tài liệu API, nền TypeScript Frontend (+ giao diện đã làm sớm)

**Yêu cầu tối thiểu Lab 5 (giảng viên, 12/10/2026):** hoàn thiện bảo mật API Backend, tài liệu hóa API, dựng nền TypeScript cho Frontend.

**Buổi 5 đã được làm một phần trước khi có yêu cầu Lab 5:** ngày 07/10 – 12/10 cả nhóm đã làm Buổi 5 theo kế hoạch cũ — trang tìm kiếm + index, trang soạn/sửa/xuất bản, healthcheck Docker. Trưởng nhóm quyết định **giữ phần đã làm và bổ sung Lab 5**. Mỗi người có hai mục:
- **Phần đã làm** — đánh dấu đã hoàn thành (giữ đúng issue Buổi 5 cũ trên bảng GitHub);
- **Lab 5** — việc mới.

Lab 7/8 giao lại tính năng nào đã có thì buổi đó **nâng cấp lên nền mới**, không làm lại (kế hoạch §0.3).

**Lab khác SRS ở 3 chỗ của buổi này — theo SRS** (kế hoạch §3):
- rate limit dùng sliding window cho `/auth/*`, fixed window cho API chung (X-6);
- không khởi tạo lại Next.js — dự án đã chạy từ Buổi 2 (X-9);
- chọn `ky` chứ không chọn `axios` — để giữ Data Cache của Next.js (X-10).

**Tiến trình trong buổi:**
1. **30 phút đầu — Thiêng:** tách `types/api.ts` thành 5 file theo miền (chỉ chuyển code, không sửa), đẩy lên `develop`.
2. Bốn người làm song song, mỗi người một file kiểu TypeScript.
3. Cuối buổi merge Thiêng → Quân → Tiến → Thọ. Thọ merge cuối để gắn tài liệu OpenAPI cho **mọi** endpoint sau khi code của cả nhóm đã vào.

### Dev 1 – Hoàng Bình Quân · Phần đã làm: gia cố API phiên & tài khoản của Buổi 4 (đã hoàn thành)
- **Hướng đi:** `7e24c92` (kiểm tra hợp lệ + kiểm thử API tài khoản), `a8ec784` (`UserTokenTransaction`, `TokenRevocationConcurrencyTests` — thu hồi phiên song song, `AuthRequestContractTests`). **D-12 đã xong sớm ở Buổi 4** (`5239618`). Giao diện hồ sơ của kế hoạch cũ chưa làm → chuyển sang Buổi 7.
- **Xong khi:** kiểm thử đồng thời thu hồi phiên xanh.
- **Commit:** `feat(auth): harden session revocation concurrency and auth contracts`

### Dev 1 – Hoàng Bình Quân · Lab 5: CORS, rate limit, API versioning
- **Hướng đi:**
  1. **CORS** (`CorsExtensions`):
     - named policy, danh sách origin đọc từ cấu hình, **kiểm lúc khởi động** (`*` hoặc sai định dạng → không chạy);
     - cho phép header `If-Match` (trang sửa công thức cần);
     - expose `ETag`, `Retry-After`, `X-RateLimit-*` để JavaScript đọc được;
     - không `AllowCredentials`.
  2. **Rate limit** (`RateLimiting/`) — đúng SRS NFR-SEC-003:
     - **sliding window 10/phút** cho cả group `/auth/*`;
     - **fixed window 100/phút** toàn cục (trừ `/health*`, `/hangfire`);
     - **fixed window 5/phút** cho upload.
     - Phân vùng theo IP thật (nhờ `UseForwardedHeaders` của Buổi 4).
     - Vượt hạn mức → 429 `RATE_LIMIT_EXCEEDED` + `Retry-After` + `X-RateLimit-*`.
     - Hạn mức cấu hình được; bộ integration test tắt rate limit, riêng `RateLimitingTests` bật hạn mức nhỏ.
  3. **API Versioning:** gói `Asp.Versioning.Http`; route `/api/v{version:apiVersion}` — URL vẫn là `/api/v1`; header `api-supported-versions`; `/api/v2/...` → 400 `VALIDATION_ERROR`; thêm bước chuẩn hóa route trong `ApiSurfaceTests` (Thiêng duyệt).
- **Vì sao:**
  - Fixed window cho phép dồn 20 lần đoán mật khẩu trong 2 giây ở ranh giới cửa sổ; sliding window chặn được — nên đặt nó ở `/auth`, vẫn dùng đủ hai loại limiter như Lab yêu cầu.
  - Rate limit không có IP thật sẽ thành giới hạn chung cho **cả hệ thống**.
  - Không expose header thì trình duyệt giấu `ETag` khỏi code → cơ chế chống ghi đè đồng thời hỏng âm thầm.
  - `Asp.Versioning` có sẵn header ngừng hỗ trợ cho cam kết "v1 giữ ≥ 6 tháng" của SRS §5.2.
- **Xong khi:**
  - request đăng nhập thứ 11/phút → 429, IP khác vẫn 200;
  - preflight từ `localhost:3000` cho phép `If-Match` và expose `ETag`;
  - `/api/v2/...` → 400;
  - `ApiSurfaceTests` vẫn 45/45.
- **Commit:** `feat(security): cors allowlist with exposed headers, ip-based rate limiting (sliding auth, fixed global/upload) and url-segment api versioning`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Phần đã làm: giao diện soạn & sửa công thức, xuất bản (đã hoàn thành)
- **Hướng đi:** `79d0132`:
  - `RecipeContentEditor` (nguyên liệu, các bước, đổi thứ tự bằng nút ↑/↓);
  - trang `/dashboard/recipes/[id]/edit` nạp bằng `GET /recipes/mine/{id}`;
  - `RecipePublishActions` có danh sách kiểm tra.

  Mỗi dòng đang tự gọi API → Buổi 8 chuyển sang một biểu mẫu React Hook Form + `useFieldArray`.
- **Xong khi:** sửa ở hai tab → tab sau nhận 409; xuất bản khi thiếu nguyên liệu bị chặn.
- **Commit:** `feat(recipes): complete Buoi 5 recipe editor and publishing flow`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Lab 5: tài liệu API Scalar `/scalar/v1` + kiểu TypeScript miền Recipe
- **Hướng đi:**
  1. **`AddCulinaryOpenApi()`** — OpenAPI 3.1 của .NET 10 với 4 transformer:
     - thông tin API;
     - security scheme Bearer;
     - security + 401/403 cho endpoint cần đăng nhập;
     - mọi lỗi 4xx/5xx là `application/problem+json` + schema danh sách mã lỗi Phụ lục B.
  2. **Scalar `/scalar/v1`:** nút Authorize dùng Bearer; bật/tắt bằng `OpenApi:Enabled`; thêm `location /scalar/`, `/openapi/` vào Nginx (Thiêng duyệt).
  3. **Metadata cho đủ 45 endpoint** (merge cuối): `WithName` (operationId ổn định), `WithSummary`, `WithDescription` (mã FR + mục SRS), `WithTags`, `Produces`/`ProducesProblem`. Chủ từng module duyệt phần của mình.
  4. **`OpenApiDocumentTests`:**
     - số operation = danh sách của `ApiSurfaceTests`;
     - mọi operation có summary/tag/operationId;
     - security khớp `RequireAuthorization`;
     - ảnh chụp `docs/api/openapi.v1.json` chặn thay đổi hợp đồng không chủ đích.
  5. **`types/recipe.ts`:**
     - `RECIPE_DIFFICULTIES`, `RECIPE_STATUSES` (`as const`);
     - `Recipe`, `RecipeSummary`, `RecipeIngredient`, `RecipeStep`, `Nutrition`, `RecipeImage`;
     - request: `CreateRecipeRequest`, `UpdateRecipeRequest`, `IngredientInput`, `StepInput` (không có `stepNumber`), `ReorderStepsRequest`, `MyRecipesParams`.
     - Kiểm thử hợp đồng: `satisfies Record<keyof Recipe, true>` + so với `openapi.v1.json`.
- **Vì sao:**
  - Tài liệu sinh từ code thì không thể lệch code; ảnh chụp biến mọi thay đổi API thành một diff phải được duyệt.
  - Một người gắn metadata cho cả 45 endpoint thì cùng một giọng văn, cùng quy tắc tag.
  - Kiểu viết tay đọc dễ và có ghi chú SRS; kiểm thử hợp đồng giữ nó không lệch Backend.
- **Xong khi:**
  - `http://localhost/scalar/v1` hiện đủ 45 endpoint có mô tả, thử được với Bearer;
  - `OpenApiDocumentTests` xanh;
  - kiểm thử hợp đồng kiểu Recipe xanh.
- **Commit:** `feat(api-docs,types): scalar /scalar/v1 openapi documentation for all endpoints with contract snapshot and recipe domain typescript types`

### Dev 3 – Đoàn Hồng Tiến · Phần đã làm: trang tìm kiếm & bộ lọc (D-10 FE), composite index (đã hoàn thành)
- **Hướng đi:** `bd9c0f7`, `23bf8c7`, `99798b8`:
  - migration `B5_Search_CompositeIndexes` (3 index);
  - `PerformanceSeeder` 10.000 công thức, `EXPLAIN ANALYZE` → không còn Seq Scan (`docs/explain-analyze-b5.md`);
  - `SearchBar`, `/search` (SSR, highlight không dấu), `FilterPanel`, `SortSelect`, phân trang theo URL;
  - D-10 phía FE; `getCategoryBySlug` `revalidate: 120` (một phần D-7).
- **Xong khi:** "pho" ra "Phở bò"; F5/back giữ bộ lọc; báo cáo Lab 05 có kết quả 319/319 kiểm thử.
- **Commit:** `feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze`

### Dev 3 – Đoàn Hồng Tiến · Lab 5: chuẩn hóa cấu trúc Frontend + kiểu TypeScript Category/Search
- **Hướng đi:**
  1. **Không khởi tạo lại** — kiểm chứng Next.js 15.5.25 App Router + TS strict + Tailwind v4 + ESLint Airbnb, ghi vào `frontend/README.md`.
  2. **Cấu trúc** `app/ · components/{ui,layout,shared} · features/ · hooks/ · lib/ · stores/ · types/`:
     - `git mv` các component tự viết từ `components/ui/` sang `components/shared/` (dành `ui/` cho shadcn — tránh `Pagination.tsx` trùng `pagination.tsx` trên Windows);
     - tách `use-debounced-value`, `use-media-query` vào `hooks/`.
  3. **Quy tắc import bằng ESLint `import/no-restricted-paths`:** `types/`, `lib/` không import ngược lên; không ai import `app/`.
  4. **`types/category.ts`:** `Category`, `CategoryRef`, `CategoryDetail`, `Create/UpdateCategoryRequest`, `CategoryDetailParams`.
  5. **`types/search.ts`:** `SORT_FIELDS`, `SORT_ORDERS`, `DEFAULT_PAGE_SIZE = 12`, `MAX_PAGE_SIZE = 50`, `MIN_SEARCH_LENGTH = 2`, `RecipeListParams`, `RecipeSearchParams`, `RecipeSearchResult`. Kiểm thử hợp đồng như Thọ.
- **Vì sao:**
  - Khởi tạo lại là xóa 4 buổi code của cả nhóm.
  - `features/` theo đúng ranh giới phân công 4 người; 6 thư mục Lab yêu cầu chứa phần dùng chung.
  - Quy tắc lint là bản Frontend của kiểm thử kiến trúc Backend — chiều phụ thuộc được máy kiểm.
- **Xong khi:**
  - `npm run lint` bắt được import sai chiều;
  - `npm run build` xanh sau khi chuyển file;
  - kiểm thử hợp đồng Category/Search xanh.
- **Commit:** `refactor(frontend): standardize app/components/lib/stores/types/hooks structure with lint-enforced import boundaries and category & search types`

### Dev 4 – Nguyễn Thăng Thiêng · Phần đã làm: FR-OBS-001 hoàn tất (đã hoàn thành)
- **Hướng đi:** `62aaf61` (`ApiSurfaceTests` 45/45), `b3cbe28`:
  - healthcheck Docker cho `api` (`curl` `/health/ready`) và `frontend` (`wget`);
  - `nginx` chờ `api` healthy; `proxy_next_upstream`;
  - `HealthIndicator` + `/dashboard/system`;
  - `RecipeCard` dùng `ThumbnailUrl`.
- **Xong khi:** `stop redis` → `/health/ready` 503, `/health/live` 200, chỉ báo chuyển đỏ ≤ 30 giây.
- **Commit:** `feat(obs): complete FR-OBS-001 docker healthcheck wiring, nginx failover and health ui indicator`

### Dev 4 – Nguyễn Thăng Thiêng · Lab 5: gói thư viện Frontend + kiểu TypeScript Auth/API
- **Hướng đi:**
  1. **Rà gói thư viện:**
     - `@tanstack/react-query`, `lucide-react`, `clsx`, `tailwind-merge`, `zod` đã có từ Buổi 2;
     - **thêm** `zustand` 5.0.15 và **`ky`** 2.2.0 (cài bằng `npx npm@10`);
     - **gỡ `next-auth`** (0 chỗ dùng — SRS đã bỏ Auth.js ở MT-11).
  2. **Kiểm chứng `ky` ngay:** option `next.revalidate` tới được `fetch`; Jest biên dịch được `ky` (`transformIgnorePatterns`).
  3. **Commit nền:** tách `types/api.ts` thành 5 file.
  4. **`types/api.ts`:** `PagedResult`, `ProblemDetails`, `ValidationProblemDetails`, **`ERROR_CODES`** (29 mã Backend đang có; mã thứ 30 của SRS v1.2.3 thêm ở Buổi 7 cùng API — Quân) → `ErrorCode`, `UploadedFile`.
  5. **`types/auth.ts`:** `ROLES`, `User`, `AuthResponse`, các request đăng nhập/đăng ký/Google/refresh/hồ sơ, `Session`, `UserAdmin`, `UserListParams`, `SetUserStatusRequest`, kiểu của CR-2026-05 (`ChangePasswordRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`).
  6. **`error-codes.test.ts`:** đọc `ErrorCodes.cs`, so tập hợp với `ERROR_CODES` → lệch là đỏ.
- **Vì sao:**
  - `axios` không đi qua `fetch` nên mất Data Cache của Next.js (mỗi lượt xem trang sẽ gọi thẳng API); `ky` bọc `fetch` và có hook cho refresh token.
  - Gói không dùng vẫn bị cài vào image và gây hiểu nhầm.
  - Danh sách mã lỗi có ba bản sao (SRS, C#, TypeScript) — kiểm thử giữ chúng khớp nhau.
- **Xong khi:**
  - `package.json` có `zustand`, `ky`, không còn `next-auth`;
  - kiểm thử `next.revalidate` xanh;
  - `error-codes.test.ts` xanh.
- **Commit:** `chore(frontend): add zustand and ky, drop unused next-auth, split shared api/auth typescript types with backend error-code drift test`

**Kiểm chứng cuối Buổi 5:**
- 429 đúng hạn mức theo IP;
- `/scalar/v1` đủ 45 endpoint;
- `OpenApiDocumentTests`, kiểm thử hợp đồng kiểu, `error-codes.test.ts` xanh;
- `npm run lint` chặn import sai chiều;
- phần đã làm: Index Scan trên 10.000 bản ghi, healthcheck chuyển traffic đúng.

---

## BUỔI 6 – Lab 6: lớp gọi API, shadcn/ui, Zustand store, giao diện tĩnh & bố cục dùng chung

**Yêu cầu tối thiểu Lab 6:** dựng khung giao diện dùng chung, quản lý state phía client và thư viện UI component.

**Phân vai state cả nhóm tuân thủ:**
- dữ liệu từ server → **TanStack Query**;
- phiên đăng nhập → **Zustand**;
- biểu mẫu → **React Hook Form**;
- bộ lọc/trang → **URL**.

Không đưa dữ liệu server vào Zustand.

**Lab khác SRS:**
- *"persist token vào HTTP-only cookie / localStorage"* → chỉ lưu **refresh token** vào `localStorage`, access token chỉ trong bộ nhớ, không cookie (NFR-SEC-002/005 — X-7);
- `Toast` của shadcn đã ngừng phát triển → dùng `sonner` (X-11).

**Tiến trình trong buổi:**
1. **30 – 45 phút đầu, hai commit nền vào `develop`:**
   - Thiêng chuyển route vào `(public)`, `(auth)`, `(dashboard)` (chỉ `git mv`, URL không đổi);
   - Thọ chạy `shadcn init` (`components.json`, `cn()`, token màu).
2. Bốn người làm song song.
3. Merge Thiêng → Quân → Tiến → Thọ.

### Dev 1 – Hoàng Bình Quân · Zustand auth store, chặn route phía client, CSP
- **Hướng đi:**
  1. **`stores/auth-store.ts`:**
     - `createAuthStore()` với `persist` + **`partialize` chỉ lưu `{ refreshToken, user }`**, `skipHydration`;
     - `AuthStoreProvider` tạo store một lần mỗi cây React (không dùng biến toàn cục — tránh lộ dữ liệu giữa request khi render trên server);
     - adapter đọc dữ liệu định dạng Buổi 4 → người đang đăng nhập không bị đăng xuất;
     - đồng bộ giữa các tab bằng sự kiện `storage` + Web Locks.
  2. **Giữ nguyên chữ ký `useAuth()`** — trang của Thọ, Tiến, Thiêng không phải sửa.
  3. **`AuthGuard`, `RequireRole`, `GuestOnly`** + `lib/safe-redirect.ts` (chặn `callbackUrl=//evil.com`). Không chặn route bằng `middleware.ts`: server không thấy token trong bộ nhớ/`localStorage`. (`middleware.ts` chỉ sinh nonce CSP — bước 4.)
  4. **CSP có nonce** (đã chốt bằng thực nghiệm — CR-2026-05 / MT-67):
     - `src/middleware.ts` sinh nonce mới mỗi request → `script-src 'self' 'nonce-…' 'strict-dynamic'` + nguồn Google Identity Services, **không** `'unsafe-inline'`;
     - root layout đọc `headers()` để Next.js gắn nonce → mọi trang render theo request; độ tươi dữ liệu đặt trên từng `fetch` (Data Cache);
     - Report-Only 1 ngày rồi chặn thật; Playwright `frontend/e2e/csp.spec.ts` kiểm 0 vi phạm + trang hydrate trên mọi route.
- **Vì sao:**
  - Context re-render mọi component mỗi lần token xoay vòng; Zustand chỉ re-render đúng phần đọc.
  - Cookie HTTP-only là thay đổi hợp đồng Backend mà SRS bắt buộc phải qua Change Request.
  - CSP là lớp chặn cuối cho refresh token nằm ở nơi JavaScript đọc được — làm cùng lúc với chỗ lưu token.
  - Đã thử: CSP bằng hash (`experimental.sri`) chặn 6 script inline dữ liệu RSC của Next.js → trang không hydrate; `'unsafe-inline'` thì không còn chặn được XSS. Nonce chạy (0 vi phạm) và Data Cache vẫn giữ — API không bị gọi thêm.
- **Xong khi:**
  - `localStorage` không có access token;
  - người đăng nhập trước khi cập nhật vẫn giữ phiên;
  - `/dashboard` khi chưa đăng nhập → `/auth/login?callbackUrl=…`;
  - `callbackUrl` ngoài site bị chặn;
  - CSP 0 vi phạm và trang hydrate trên mọi route; hai lần tải có nonce khác nhau.
- **Commit:** `feat(auth-state): zustand auth store with refresh-token-only persistence, legacy session migration, client route guards and nonce-based content security policy`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Thư viện UI shadcn/ui + trang tổng quan dashboard
- **Hướng đi:**
  1. **`npx shadcn@4.21.4 init`:**
     - token màu Tailwind v4;
     - **màu chính đổi `orange-600` → `orange-700`** vì chữ trắng trên `orange-600` chỉ đạt tỉ lệ tương phản 3,56 : 1 (WCAG AA cần 4,5 : 1).
  2. **8 component Lab:** `button` (thêm `isLoading`), `input`, `dialog`, `dropdown-menu`, `sonner`, `form`, `card`, `badge`.
  3. **Phụ trợ:** `label`, `textarea`, `select`, `alert-dialog`, `sheet`, `tabs`, `skeleton`, `table`.
  4. **`form`** tự gắn `aria-invalid`/`aria-describedby`, dùng được với `mapProblemDetailsToForm` (lỗi 400 hiện đúng ô).
  5. **`RecipeStatusBadge`**; trang **`/dashboard`** (3 thẻ số lượng Draft/Published/Archived lấy `totalCount` từ `/recipes/mine?status=X&pageSize=1`).
- **Vì sao:**
  - shadcn viết bằng chính class Tailwind (đúng NFR-USE-001) và dùng Radix cho bàn phím/focus/ARIA (WCAG).
  - Code nằm trong repo nên sửa được.
  - Token màu: đổi một biến thay vì 40 file.
- **Xong khi:**
  - 8 component có trong `components/ui/`;
  - `Dialog` đóng bằng Escape và trả focus;
  - lỗi form có `aria-invalid`;
  - `/dashboard` hiện đúng số lượng.
- **Commit:** `feat(ui): shadcn/ui primitives (button, input, dialog, dropdown-menu, sonner, form, card, badge) on wcag-checked theme tokens and dashboard overview`

### Dev 3 – Đoàn Hồng Tiến · Giao diện tĩnh, component dùng chung + bộ lọc thời gian cho `/recipes/search` (CR-2026-05)
- **Hướng đi:**
  1. **Khung trang:**
     - `Header` (thay `SiteHeader`: logo, `CategoryNav`, `SearchBar`, `UserMenu`, menu điện thoại dạng `Sheet`);
     - `Footer`;
     - `SidebarCategory` (Server Component, `aria-current`).
  2. **Dùng chung:**
     - `Pagination` (`<nav>`, link thật giữ tham số lọc, chạy không cần JavaScript);
     - `LoadingSkeleton` (đúng kích thước thật → không nhảy bố cục);
     - `EmptyState`;
     - `ErrorBoundary` (class component + `QueryErrorResetBoundary`) và `ErrorFallback` (hiện mã tra cứu).
  3. **Giao diện tĩnh:**
     - route `/mockups/*` dựng bằng component thật + **công thức thật lấy từ dữ liệu mẫu** (`satisfies Recipe[]`);
     - bản build production trả 404;
     - rà 320/768/1200 px.
  4. **MT-66 — API:** `GET /recipes/search` nhận thêm `maxCookTime`, `maxPrepTime`, `minServings` qua `RecipeFilterSpec` dùng chung; validator dùng lại quy tắc của `GET /recipes`; kiểm thử khóa cache khác nhau khi bộ lọc khác nhau.
- **Vì sao:**
  - Mockup trong app dùng đúng CSS/provider thật và gãy biên dịch khi hợp đồng đổi; Storybook nặng và phải cấu hình riêng.
  - Khung trang ở server không tốn JavaScript.
  - Lọc thời gian phải chạy ở cơ sở dữ liệu — lọc trên trang đã phân trang sai `totalCount`.
- **Xong khi:**
  - mockup chạy ở dev, 404 ở production;
  - `Pagination` giữ bộ lọc;
  - `q=pho&maxCookTime=30` chỉ ra món ≤ 30 phút;
  - `maxCookTime=-1` → 400.
- **Commit:** `feat(ui-shell,search): shared layout and accessible pagination, skeleton, empty state and error boundary with typed static mockups; search time filters (CR-2026-05 MT-66)`

### Dev 4 – Nguyễn Thăng Thiêng · Lớp gọi API `ky`, bộ xử lý lỗi toàn cục, bố cục theo route group
- **Hướng đi:**
  1. **Commit nền:** route group.
  2. **`lib/api-client.ts` viết lại bằng `ky`, giữ nguyên `apiFetch`/`ApiError`:**
     - hook gắn `X-Correlation-ID` + Bearer;
     - 401 `AUTH_TOKEN_EXPIRED` → refresh **một lần** cho mọi request song song rồi thử lại;
     - lỗi → `ApiError`;
     - **chỉ tự thử lại `GET`** (429 chờ `Retry-After` ≤ 5 giây);
     - chạy trên server không bao giờ gắn token.
  3. **`lib/error-handler.ts` + `lib/error-messages.ts`:**
     - thông báo tiếng Việt cho **mọi** mã lỗi (`satisfies Record<ErrorCode, string>`);
     - `handleApiError`: lỗi từng ô / toast / mã tra cứu;
     - gắn vào `QueryCache`/`MutationCache`; không thử lại lỗi 4xx.
  4. **Bố cục:**
     - Root (`<html lang="vi">`, providers);
     - `(public)` (Header + Footer);
     - `(auth)` (khung tối giản + `GuestOnly`);
     - `(dashboard)` (Server Component có `robots: noindex` + `DashboardShell`: `AuthGuard`, sidebar theo vai trò, `Sheet` trên điện thoại);
     - `error.tsx` mỗi nhóm; `loading.tsx` ở `(auth)`, `(dashboard)` — `(public)` giữ khung chờ ở từng trang danh sách để `/recipes/[slug]` vẫn trả 404 thật.
- **Vì sao:**
  - Giữ chữ ký `apiFetch` thì hơn 40 chỗ gọi không phải sửa, và kiểm thử cũ thành kiểm thử hồi quy.
  - Tự thử lại lệnh ghi có thể tạo công thức trùng.
  - Xử lý lỗi một chỗ thì không màn hình nào quên 429.
  - Route group: mỗi nhóm trang chỉ tải code của layout mình.
- **Xong khi:**
  - kiểm thử cũ của `api-client` xanh;
  - 5 request song song → 1 lần refresh;
  - `POST` không tự thử lại;
  - mọi mã lỗi có thông báo;
  - dashboard có `noindex`.
- **Commit:** `feat(frontend-core): ky-based api client with bearer, single-flight refresh, correlation id and safe retry, central error handler and public/auth/dashboard route-group layouts`

**Kiểm chứng cuối Buổi 6:**
- đăng nhập → F5 vẫn giữ phiên, `localStorage` không có access token;
- CSP có nonce, 0 vi phạm; tải trang chủ nhiều lần không sinh request mới tới API (Data Cache);
- mockup ở 3 kích thước;
- `ApiSurfaceTests` vẫn 45/45.

---

## BUỔI 7 – Lab 7: xác thực & hồ sơ, trang chủ, danh mục, chi tiết công thức, tìm kiếm (+ hạ tầng production)

**Yêu cầu tối thiểu Lab 7:** các trang công khai và nghiệp vụ người dùng chạy end-to-end với Backend.

**CR-2026-05 (SRS v1.2.3)** cho đổi mật khẩu, quên/đặt lại mật khẩu và lọc thời gian trên `/search` — những chức năng SRS v1.2.2 chưa có:
- **48 endpoint** (thêm `change-password`, `forgot-password`, `reset-password`);
- **30 mã lỗi** (thêm `AUTH_PASSWORD_RESET_TOKEN_INVALID`).

**Nâng cấp, không làm lại:** đăng nhập/đăng ký (Buổi 2 – 3), trang chi tiết và trang danh mục (Buổi 2), trang tìm kiếm (Buổi 5) được dựng lại trên nền Buổi 5 – 6.

**Ngoại lệ phân công (đúng theo Lab 7):** giao diện `/search` do Thiêng làm; API tìm kiếm vẫn của Tiến.

### Dev 1 – Hoàng Bình Quân · Đăng ký/đăng nhập/đăng xuất, đổi mật khẩu, quên/đặt lại mật khẩu, trang hồ sơ
- **Hướng đi:**
  1. **API mới (CR-2026-05):**
     - **`POST /auth/change-password`:** sai mật khẩu hiện tại → 400 ở ô `currentPassword`; tài khoản Google chưa có mật khẩu → 400; thành công **thu hồi mọi phiên khác** (giữ phiên hiện tại nhờ claim `sid`).
     - **`POST /auth/forgot-password`:** **luôn 202**; job Hangfire chỉ nhận `userId`, worker tự sinh token (30 phút, dùng một lần) và gửi email qua MailKit.
     - **`POST /auth/reset-password`:** một mã lỗi `AUTH_PASSWORD_RESET_TOKEN_INVALID` cho mọi token hỏng; thành công thu hồi **mọi** phiên và xóa khóa đăng nhập sai.
     - Thêm mã vào `ErrorCodes.cs` (Thiêng duyệt) + `ERROR_CODES`/`ERROR_MESSAGES`, gỡ mã khỏi `PendingSrsCodes` của kiểm thử kiến trúc; `ApiSurfaceTests` → **48**.
  2. **Nâng cấp form đăng nhập/đăng ký:** shadcn `Form` + Zod, `GuestOnly`, `callbackUrl` an toàn. Đăng xuất: `POST /auth/logout` → xóa store → `queryClient.clear()`.
  3. **Trang `/auth/forgot-password`:** cùng một thông báo cho mọi email.
  4. **Trang `/auth/reset-password`:** xóa token khỏi thanh địa chỉ ngay khi đọc; `noindex` + `no-referrer`.
  5. **`/profile`** — 3 tab:
     - **Thông tin** (`GET/PATCH /auth/me`, ảnh đại diện qua `ImageUploader`, optimistic + hoàn tác, xóa ảnh cũ **sau** khi PATCH thành công);
     - **Bảo mật** (đổi mật khẩu);
     - **Phiên đăng nhập** (FR-AUTH-009: "Thiết bị này", thu hồi một/tất cả).
- **Vì sao:**
  - Trả 202 cho mọi email để không ai dò được email nào đã đăng ký.
  - Hangfire lưu tham số job dạng rõ trong cơ sở dữ liệu và hiện trên dashboard → không được truyền token vào job.
  - Đổi mật khẩu thường vì nghi bị lộ → phải đá các phiên khác ra.
  - Sai mật khẩu hiện tại là lỗi dữ liệu (400), không phải "chưa đăng nhập" (401).
  - `/auth/me` là nguồn sự thật về vai trò.
- **Xong khi:**
  - đổi mật khẩu → phiên khác 401, phiên hiện tại vẫn chạy;
  - email đặt lại có trong MailHog → đặt lại → đăng nhập được, dùng lại link → 400;
  - tham số job không chứa token;
  - `/profile` đổi ảnh thấy ngay trên header;
  - `ApiSurfaceTests` 48/48.
- **Commit:** `feat(auth): change, forgot and reset password (CR-2026-05) with session revocation, hardened login/register forms and profile page with avatar upload and sessions tab`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Trang chi tiết công thức + danh sách "Công thức của tôi"
- **Hướng đi:**
  1. **`/recipes/[slug]`:**
     - render theo request (CSP nonce — MT-67), dữ liệu qua Data Cache `revalidate: 300` (= TTL cache 5 phút) — không gọi lại API mỗi lượt xem;
     - **không có `loading.tsx`** để bản nháp trả đúng **HTTP 404**.
  2. **Thành phần:**
     - `RecipeHero` (`<dl>` thông số);
     - `RecipeGallery` (ảnh 800×600 `priority`);
     - **`IngredientList` điều chỉnh khẩu phần** (nhân `quantity`, giữ nguyên `quantityText`);
     - `StepChecklist` + hẹn giờ;
     - `NutritionTable`;
     - **`RecipeActions`:** In bằng `@media print`; Chia sẻ bằng `navigator.share`, không có thì sao chép link.
  3. **`/dashboard/recipes`:**
     - tab trạng thái trên URL, `Table` + `RecipeStatusBadge`, phân trang;
     - **chỉ gọi `/recipes/mine`**;
     - query key nhánh `'private'` tách khỏi `'public'`, `staleTime: 0`.
- **Vì sao:**
  - `loading.tsx` bật streaming nên status 200 được gửi trước khi biết dữ liệu có hay không — bản nháp sẽ thành trang 200 rỗng.
  - Định lượng hai cột (MT-04) mới cho phép nhân khẩu phần.
  - In bằng CSS không cần thư viện PDF.
  - Cache phía client cũng không được trộn dữ liệu riêng tư với công khai.
- **Xong khi:**
  - `curl -I` slug bản nháp → 404;
  - 2 → 4 khẩu phần nhân đúng;
  - in ra không có header/nút;
  - `/dashboard/recipes` lọc đúng tab.
- **Commit:** `feat(recipes-ui): recipe detail page with servings scaler, gallery, nutrition, print and share returning real 404 for drafts, and my-recipes dashboard over /recipes/mine`

### Dev 3 – Đoàn Hồng Tiến · Trang chủ, danh sách danh mục, trang danh mục (+ hoàn tất D-7)
- **Hướng đi:**
  1. **`/`** (SSR + Data Cache):
     - gọi song song 8 công thức mới xuất bản (`revalidate: 120`) + danh mục (`revalidate: 1800`);
     - khối giới thiệu có ô tìm kiếm (form GET);
     - lưới công thức + `SidebarCategory` (≥ 1200 px) / nhãn cuộn ngang (điện thoại).
  2. **`/categories`** (SSR + Data Cache): **D-7: `revalidate` 3600 → 1800**; `CategoryCard` trên shadcn `Card`.
  3. **`/categories/[slug]`** (SSR + Data Cache 120 — đã đặt ở Buổi 5): sidebar `activeSlug`, `SortSelect` (danh mục chỉ có sắp xếp + phân trang theo SRS §8.2), `EmptyState`.
  4. **Kiểm thử `revalidate`** của từng lời gọi `fetch` (1800 / 120 / 300).
- **Vì sao:**
  - Mỗi lời gọi `fetch` mang đúng TTL của dữ liệu đó (quy tắc vàng "`revalidate` ≤ TTL"; từ MT-67 trang không còn ISR, cache nằm ở Data Cache).
  - D-7 xảy ra vì con số được viết tay không ai đối chiếu → nay có kiểm thử.
  - "Nổi bật" = mới xuất bản nhất (MT-23).
- **Xong khi:**
  - `grep "revalidate: 3600|600"` rỗng;
  - trang chủ hiện 8 món mới nhất;
  - bố cục đúng ở 3 kích thước.
- **Commit:** `feat(home,categories): homepage with latest recipes and category sidebar, category list and detail pages with fetch revalidate aligned to api ttl (D-7)`

### Dev 4 – Nguyễn Thăng Thiêng · Trang tìm kiếm + hạ tầng production (Nginx SSL, `/media/` D-14, scale, D-17)
- **Hướng đi:**
  1. **`/search`** — nâng cấp trang của Tiến:
     - lần đầu SSR + `HydrationBoundary`;
     - đổi bộ lọc bằng `window.history.pushState` + `useQuery` (`keepPreviousData`, `staleTime` 60 giây) → không tải lại trang, không gọi server hai lần.
  2. **`FilterPanel` đa tiêu chí:**
     - danh mục, độ khó (đủ 4 mức);
     - **thời gian nấu, thời gian chuẩn bị, khẩu phần (MT-66)**;
     - nhãn bộ lọc đang bật + gỡ;
     - `Sheet` trên điện thoại.
  3. **Phân trang offset:** "Hiển thị 13–24 trên 57 kết quả"; đổi bộ lọc → về trang 1.
  4. **Nginx:**
     - **`/media/` → MinIO (D-14)**, kèm script đổi URL ảnh cũ trong cơ sở dữ liệu;
     - HTTPS (TLS 1.2+), HSTS, gzip;
     - `_next/static` cache 1 năm;
     - giới hạn `/auth/*` 10/phút và upload 5/phút cho cả cụm (rate limit của API đếm riêng từng bản sao).
  5. **`docker-compose.prod.yml`:**
     - `api` bỏ `ports` + 3 bản sao; `hangfire` 1; không `seq`/`mailhog`; tắt Scalar;
     - service `migrate` chạy migration một lần trước khi `api` khởi động;
     - cập nhật `api` cuốn chiếu bằng `tools/deploy/rolling-update-api.sh`.
  6. **D-17:** `ProtectKeysWithCertificate` (chứng chỉ riêng, không dùng lại chứng chỉ TLS).
- **Vì sao:**
  - SSR cho crawler và link chia sẻ, client cho các lần lọc sau.
  - Một địa chỉ ảnh duy nhất cho cả trình duyệt lẫn container → mới bật lại được tối ưu ảnh.
  - Bỏ `ports` là điều kiện để `--scale api=3`.
  - `--force-recreate` dừng cả ba bản sao cùng lúc (đo: 502 trong ~3 giây) — chỉ cập nhật cuốn chiếu mới không gián đoạn.
  - Khóa DataProtection ký cả token đặt lại mật khẩu.
- **Xong khi:**
  - đổi bộ lọc chỉ có request JSON;
  - lọc theo thời gian đúng;
  - `https://` + HSTS;
  - ảnh qua `/media/`;
  - `--scale api=3` chạy, cập nhật `api` khi đang có request liên tục không có 502.
- **Commit:** `feat(search-ui,devops): client-hydrated search with multi-criteria filters and offset pagination; production nginx with tls/hsts and /media proxy (D-14), scaled prod compose and certificate-protected dataprotection keys (D-17)`

**Kiểm chứng cuối Buổi 7:**
- `ApiSurfaceTests` 48/48;
- luồng quên/đặt lại mật khẩu chạy qua MailHog;
- bản nháp → 404;
- `revalidate` khớp TTL;
- `/search` lọc theo thời gian;
- production chạy HTTPS + 3 bản sao `api`.

---

## BUỔI 8 – Lab 8: quản lý người dùng, biểu mẫu công thức & máy trạng thái, SEO & WCAG, log có cấu trúc, load test & đóng gói

**Yêu cầu tối thiểu Lab 8:** hoàn thiện quản trị, tải ảnh lên, ghi nhật ký hệ thống và đóng gói dự án. Kết thúc buổi: **39/39 FR**, tag `v1.0.0`.

**Lab khác SRS:** *"`GET /users` cho Admin/Author"* → **chỉ Admin** (Author thấy email mọi người dùng là rò rỉ dữ liệu); "phân quyền" = trang chỉ Admin + hiển thị vai trò + khóa/mở khóa; đổi vai trò không nằm trong SRS (X-8).

**Ngoài yêu cầu Lab:** mỗi người mang theo E2E và kiểm thử của module mình — NFR-MAINT-002 bắt buộc 5 luồng E2E.

**Đầu buổi:** Thiêng dựng môi trường giống production để cả nhóm chạy E2E.

### Dev 1 – Hoàng Bình Quân · Trang quản lý người dùng (`GET /users` đã có từ Buổi 4) + E2E xác thực & kiểm thử bảo mật
- **Hướng đi:**
  1. **`GET /api/v1/users`** (Buổi 4) giữ chỉ Admin; đo `EXPLAIN ANALYZE` tìm kiếm trên 10.000 người dùng — chậm mới thêm index trigram `B8_Auth_UserSearchTrgm`.
  2. **`/dashboard/users`** (`RequireRole Admin`):
     - ô tìm kiếm debounce 300 ms;
     - lọc trạng thái; `Table` (ảnh, tên, email, vai trò, trạng thái, số công thức); phân trang;
     - **khóa/mở khóa** qua `AlertDialog` có **lý do bắt buộc**, optimistic + hoàn tác;
     - dòng của chính mình không có nút khóa (server vẫn chặn 403).
  3. **E2E `frontend/e2e/auth.spec.ts`:** đăng ký → dashboard; sai 5 lần → 423; tự refresh; bị khóa → đăng xuất; quên/đặt lại mật khẩu qua MailHog.
  4. **`docs/security-test-report.md`:**
     - giả mạo `X-Forwarded-For`;
     - 429; `/hangfire` 401;
     - phân quyền;
     - rò rỉ trường nhạy cảm;
     - `localStorage`; chuyển hướng mở; CSP;
     - **log không chứa token/mật khẩu**.
  5. Độ phủ Application module Auth ≥ 80 %.
- **Vì sao:**
  - Quyền tối thiểu: Author không có nghiệp vụ nào cần danh sách email.
  - Lý do khóa bắt buộc thì audit log không có bản ghi trống.
  - Đo rồi mới tạo index.
  - Giả mạo IP là cấu hình "trông có vẻ đúng" chỉ kiểm thử chủ động mới phát hiện.
- **Xong khi:**
  - Author → 403;
  - Admin tìm, lọc, khóa được, phiên người bị khóa bị thu hồi;
  - E2E xanh;
  - báo cáo bảo mật đủ mục.
- **Commit:** `feat(admin-users): admin user management page with debounced search, status filter and audited lock/unlock; e2e auth flows incl. password reset and security test report`

### Dev 2 – Nguyễn Hồng Phúc Thọ · Biểu mẫu thêm/sửa công thức (RHF + Zod + `useFieldArray`), tải ảnh MinIO, máy trạng thái khép kín
- **Hướng đi:**
  1. **Schema:**
     - `limits.ts` chép SRS §7.9;
     - `recipeFormSchema` khớp validator Backend (nguyên liệu phải có số **hoặc** chữ **hoặc** đơn vị; bước không có `stepNumber`);
     - đổi `id` → `serverId` trong form (vì `useFieldArray` ghi đè `id`).
  2. **`RecipeWizard` — một `useForm` cho 5 bước:** thông tin → nguyên liệu → các bước → ảnh → xem lại. Mỗi bước `trigger()` trước khi đi tiếp; `useFieldArray` cho nguyên liệu và các bước (`move` bằng nút ↑/↓); cảnh báo rời trang khi chưa lưu.
  3. **`/dashboard/recipes/new`:** **một** `POST /recipes` có mảng nguyên liệu + các bước → sang bước ảnh.
  4. **`/dashboard/recipes/[id]/edit`:** lưu theo phần thay đổi:
     - `PUT` (If-Match) trước — 409 thì dừng và hiện dialog so sánh;
     - rồi `POST`/`PUT`/`DELETE` nguyên liệu/bước;
     - rồi `reorder`.
  5. **Ảnh:**
     - `ImageUploader` → `POST /recipes/{id}/images`; báo lỗi 429/400/503 rõ ràng;
     - chọn ảnh chính; **`altText` bắt buộc trước khi xuất bản**.
  6. **Máy trạng thái** (`status-transitions.ts`, kiểm thử chống lệch với `RecipeStatusTransitions.cs`):
     - chỉ hiện nút hợp lệ: xuất bản / hủy xuất bản / lưu trữ / khôi phục về nháp / xóa;
     - xóa phải gõ đúng tên;
     - 409 khi người khác đổi trạng thái trước.
  7. **E2E** vòng đời + hai tab 409; integration test 10 `PUT` song song → 1 thành công, 9 × 409; hồi quy MT-34; đếm truy vấn N+1.
- **Vì sao:**
  - Một form cho cả biểu mẫu nhiều bước giữ một nguồn sự thật.
  - Lưu theo phần thay đổi dùng 8 endpoint có sẵn, không phải thêm endpoint ngoài SRS.
  - `PUT` trước để phát hiện xung đột trước khi sửa bất kỳ dòng con nào.
  - Bảng chuyển trạng thái tập trung thì thêm trạng thái mới chỉ là thêm dòng.
- **Xong khi:**
  - tạo công thức bằng một `POST`;
  - sửa ở hai tab → dialog 409;
  - chỉ hiện nút hợp lệ;
  - E2E và kiểm thử đồng thời xanh.
- **Commit:** `feat(recipes-ui): react-hook-form wizard with zod and field arrays for create/edit with diff sync, minio image step and closed state-machine actions (publish, unpublish, archive, unarchive, soft delete); e2e lifecycle and concurrency tests`

### Dev 3 – Đoàn Hồng Tiến · SEO (JSON-LD Recipe, Open Graph, sitemap) + responsive & WCAG 2.1 AA
- **Hướng đi:**
  1. **`buildRecipeJsonLd`:**
     - thời gian ISO 8601, `recipeIngredient`, `HowToStep`, `NutritionInformation`;
     - không có `aggregateRating`;
     - `BreadcrumbList`, `WebSite` + `SearchAction`;
     - `JsonLd` **thoát ký tự `<`** (chống XSS qua tiêu đề công thức).
  2. **Metadata:**
     - `<title>` cắt ≤ 60 ký tự khi render, mô tả 160 ký tự, canonical, Open Graph;
     - **ảnh chia sẻ 1200×630 bằng `opengraph-image.tsx`** (`next/og`);
     - `noindex` cho dashboard/auth/mockup/kết quả tìm kiếm.
  3. **`app/sitemap.ts`** (từ `GET /recipes/sitemap`, 1 giờ) + **`app/robots.ts`**; không ping Google.
  4. **D-14 phía Next.js:** gỡ `unoptimized`, `remotePatterns` cho `/media/`, `sizes`, `priority` ảnh LCP.
  5. **Responsive** 320/768/1200; **axe** 0 vi phạm serious/critical; **Lighthouse** đạt CWV; **E2E** tìm kiếm (kể cả lọc thời gian).
- **Vì sao:**
  - Giới hạn độ dài tiêu đề là quy tắc hiển thị, không phải ràng buộc dữ liệu.
  - `aggregateRating` giả vi phạm chính sách Google.
  - Sitemap phải nằm ở domain chính mà crawler tìm.
  - Phần lớn accessibility đã có từ shadcn/Radix — buổi này kiểm chứng và vá nốt.
- **Xong khi:**
  - Google Rich Results Test đạt;
  - sitemap không có bản nháp;
  - axe, Lighthouse, E2E đạt;
  - báo cáo SEO/a11y/Lighthouse.
- **Commit:** `feat(seo,a11y): recipe json-ld and open graph metadata with og image, nextjs sitemap and robots, optimized next/image via /media (D-14), responsive and wcag 2.1 aa fixes with lighthouse and e2e search tests`

### Dev 4 – Nguyễn Thăng Thiêng · Serilog (CorrelationId, thời gian request, audit), load test k6, đóng gói Docker Compose 8 service
- **Hướng đi:**
  1. **Serilog** cho `api` và `hangfire`: Console JSON, File (xoay vòng theo ngày), Seq; `UseSerilogRequestLogging` (thời gian xử lý, 5xx → Error, > 500 ms → Warning).
  2. **`CorrelationIdMiddleware`:**
     - nhận header ≤ 64 ký tự an toàn, không hợp lệ thì sinh mới (chống chèn dòng log giả);
     - trả lại trong response và trong Problem Details;
     - job Hangfire mang theo CorrelationId.
  3. **Audit trong `LoggingBehavior`** (giữ đúng 4 behavior của SRS); `SlowQueryInterceptor` > 100 ms; **che dữ liệu nhạy cảm** + `LogRedactionTests`.
  4. **OpenTelemetry** (mức C — cắt được nếu thiếu thời gian).
  5. **Audit cache** (`SCAN`, không `KEYS`) + đo hit rate → [`docs/cache-report.md`](docs/cache-report.md):
     - xóa theo tiền tố trước đây không có tác dụng — nay xóa bằng `SCAN`;
     - đo được hit rate 75,4 % do sửa bản nháp vẫn xóa cache công khai → **CR-2026-06 (SRS v1.2.4, MT-69)**: chỉ xóa khi thay đổi chạm tới tập Published → 97,3 %.
  6. **k6** smoke/load (100 người dùng)/stress, tỉ lệ 70/20/10 → [`docs/load-test-report.md`](docs/load-test-report.md):
     - môi trường đo **nâng hạn mức rate limit** (k6 chạy từ một IP) — `docker-compose.loadtest.yml`;
     - chịu lỗi khi đang tải (tắt Redis, tắt MinIO).
  7. **Đầu buổi — môi trường E2E giống production** cho cả nhóm: `docker-compose.e2e.yml` (MailHog, token 1 phút, dữ liệu mẫu, rate limit thật).
  8. **Đóng gói:**
     - 8 service healthy (thêm healthcheck `nginx`, `hangfire`, `seq`); ghim tag image; quét Trivy;
     - README < 5 phút (`tools/dev-setup.sh --up`), CHANGELOG;
     - **10 ADR** (6 cũ + phân vai state Frontend, `ky`, versioning & rate limit, CSP nonce ↔ render) — [`docs/adr/`](docs/adr/);
     - tag `v1.0.0` **chuẩn bị sẵn**, gắn trên `main` khi cả 4 nhánh Buổi 8 đã vào (quyết định 21/10/2026).
- **Vì sao:**
  - CorrelationId hiện trên màn hình lỗi giúp tra đúng chuỗi log trong Seq.
  - Lọc bí mật khỏi log là mắt xích cuối của chuỗi bảo vệ token.
  - Kịch bản tải phải giống thực tế; báo cáo hit rate trung thực, không nới TTL cho đẹp số — số đo ngược chỉ tiêu thì sửa SRS qua Change Request.
- **Xong khi:**
  - log có `CorrelationId`/`RequestPath`/`UserId`, không có bí mật;
  - k6 đạt (hoặc báo cáo trung thực);
  - 8 service healthy;
  - đủ tài liệu; tag `v1.0.0` gắn khi cả nhóm xong Buổi 8.
- **Commit:** `feat(obs,release): serilog structured logging with validated correlation id, request timing, audit and secret redaction; opentelemetry; cache audit, k6 load and resilience tests; docker compose packaging, adrs and v1.0.0`

**Danh sách kiểm tra nghiệm thu cuối dự án:**
- 39/39 FR;
- 48/48 endpoint trên Scalar `/scalar/v1` (khóa bởi `ApiSurfaceTests`);
- yêu cầu tối thiểu Lab 5 → Lab 8 có bằng chứng trong báo cáo;
- 17/17 nợ kỹ thuật đã trả;
- 30/30 mã lỗi có kiểm thử;
- 0 chỗ dùng 422;
- TTL cache và `revalidate` khớp SRS;
- 10 ADR, CHANGELOG, tag `v1.0.0`.

---

# Phụ lục – Cài đặt & chạy dự án

**Chỉ để chạy hệ thống:** Docker Desktop 4.x (Compose v2) + Git Bash (Windows) hoặc shell bất kỳ (Linux/macOS).
**Để phát triển:** thêm .NET SDK 10.0.x · Node.js 20+ (container dùng 22 LTS) · `dotnet tool install --global dotnet-ef`.

```bash
git clone https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao.git culinary-blog && cd culinary-blog
./tools/dev-setup.sh --up     # .env + bí mật ngẫu nhiên, nginx/.htpasswd, hook gitleaks, rồi dựng 8 service và chờ healthy
```

`tools/dev-setup.sh` không ghi đè `.env` đã có — chỉ điền các khóa bí mật còn trống — và in mật khẩu tài khoản mẫu một lần. Làm tay vẫn được:
`cp .env.example .env` (điền `JWT_SIGNING_KEY`, `AUTH_SECRET`, `HANGFIRE_*`, `SEED_*`), `./nginx/generate-htpasswd.sh`,
`git config core.hooksPath .githooks`, `docker compose --profile dev up -d --build`.

| Service | URL |
|---|---|
| Nginx (cửa vào chính) | http://localhost |
| Frontend | http://localhost:3000 |
| API + Scalar | http://localhost:5000/scalar/v1 |
| Hangfire Dashboard | http://localhost/hangfire (Basic Auth `HANGFIRE_DASHBOARD_USER`/`…PASSWORD`) |
| MinIO Console | http://localhost:9001 |
| Seq (log, trace, metric) | http://localhost:5341 |
| Mailhog | http://localhost:8025 |

Tài khoản mẫu: `admin@culinaryblog.local` (mật khẩu `SEED_ADMIN_PASSWORD`), `author1..5@culinaryblog.local` (`SEED_AUTHOR_PASSWORD`). Dữ liệu mẫu: 20 danh mục, 100 công thức (~85% đã xuất bản) — API tự seed/bù khi khởi động, không cần xóa volume. Máy đã có PostgreSQL chiếm cổng 5432 → đặt `POSTGRES_HOST_PORT=5434`.

**Môi trường khác (Buổi 7 – 8)** — cùng project `culinaryblog`, dừng bản dev trước (`docker compose --profile dev down`):

| Mục đích | Lệnh | Ghi chú |
|---|---|---|
| Production (HTTPS, 3 bản sao API) | `docker compose -f docker-compose.prod.yml --env-file .env.production up -d --build` | chuẩn bị một lần: `.env.production`, `./nginx/generate-ssl-cert.sh` |
| E2E giống production (Playwright) | `docker compose -f docker-compose.prod.yml -f docker-compose.e2e.yml --env-file .env.production up -d --build` | + MailHog, access token 1 phút, dữ liệu mẫu; rate limit thật |
| Đo tải (k6) | `docker compose -f docker-compose.prod.yml -f docker-compose.loadtest.yml --env-file .env.production up -d --build` | nâng hạn mức rate limit — [`tests/load/README.md`](tests/load/README.md) |

**Tra log theo mã lỗi:** màn hình lỗi hiện "Mã tra cứu" = header `X-Correlation-ID` → dán vào Seq: `CorrelationId = '<mã>'` ra mọi log của
request đó ở mọi tầng, kể cả job Hangfire nó sinh ra. Production không có Seq: `docker compose … logs api | grep '<mã>'` (log JSON).

**Definition of Done mỗi commit:**
```bash
cd backend  && dotnet build && dotnet test
cd frontend && npm run lint && npm run typecheck && npm test && npm run build
```
