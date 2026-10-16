# KẾ HOẠCH PHÁT TRIỂN CHI TIẾT 8 BUỔI LÀM VIỆC – CULINARY BLOG
## (FEATURE-DRIVEN & ARCHITECTURAL RATIONALE)

> **Nguồn chuẩn yêu cầu:** `SPEC/SRS_Culinary_Blog_v1.2.3.md` (v1.2.3, áp dụng 12/10/2026) = v1.1.0 (CR-2026, MT-01 → MT-41) + **CR-2026-02** (MT-42 → MT-57, v1.2.0) + **CR-2026-03** (yêu cầu dữ liệu mẫu, v1.2.1) + **CR-2026-04** (MT-59 → MT-63: 2 mã lỗi, 1 endpoint, 2 làm rõ, đính chính – errata E-9 — v1.2.2) + **CR-2026-05** (MT-64 → MT-68: 2 FR, 3 endpoint, 1 mã lỗi, mở rộng bộ lọc tìm kiếm cho yêu cầu Lab 7; errata CSP nonce ↔ render động và header CORS phát hiện khi thiết kế Lab 5–6 — v1.2.3). Các câu "SRS v1.1.0 (MT-xx) chốt…" hay "SRS v1.2.0…" trong tài liệu chỉ thời điểm quyết định được đưa ra; mọi quyết định đó vẫn còn nguyên hiệu lực trong v1.2.3.
> **Yêu cầu của giảng viên:** Buổi 1 → 4 theo yêu cầu từng buổi (§0.1, §4.4, §4.5); **Buổi 5 → 8 theo yêu cầu tối thiểu Lab 5 → Lab 8 (12/10/2026)** — xem §0.3.
> **Hồ sơ mâu thuẫn:** `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` — 68 mục MT (MT-01 → MT-68), cùng các bảng xung đột code ↔ SRS (§4), lộ trình và yêu cầu Lab ↔ SRS (§5) và đánh đổi còn mở (§6).
> **Nguồn chuẩn hiện trạng:** code trên nhánh `main` (commit `f313e95`) + `SPEC/BAO_CAO_BUOI_2.md` — mọi hạng mục nợ kỹ thuật ở §4 đều đã được **kiểm chứng trực tiếp trong code**, kèm đường dẫn file. Từ Buổi 5: các nhánh `{mssv}_{HoTen}_buoiso5` của 4 dev, đối chiếu ngày 12/10/2026.
> **File:** `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`
> **Thay thế:** `SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md` (cùng nội dung, trước khi đánh số lại 8 buổi — xem §0.1) và `SPEC/KE_HOACH_PHAT_TRIEN_6_BUOI.md` (lập theo SRS v1.0.0 — đã lỗi thời); cả hai bản gốc lưu trong lịch sử git.

---

## 0. Bối cảnh, lý do mở rộng 6 → 7 → 8 buổi và yêu cầu Lab 5 → Lab 8

Repo `d:\ptudwnc_nhom20` đã hoàn thành **Buổi 1** (đọc đặc tả) và **Buổi 2** (nền móng & lát cắt đầu tiên, đã merge vào `main`). Nhóm có **4 dev full-stack**.

### 0.1 Đánh số lại 7 → 8 buổi (21/09/2026)

Giảng viên quy định lại **Buổi 1** là buổi **đọc đặc tả yêu cầu hệ thống và tìm hiểu các tính năng cần xây dựng**. Vì vậy toàn bộ các buổi của kế hoạch 7 buổi được **đôn lên một số**: Buổi 1 cũ → **Buổi 2**, …, Buổi 7 cũ → **Buổi 8**. **Nội dung công việc của từng buổi giữ nguyên**, chỉ đổi số thứ tự; riêng Buổi 2 được bổ sung **yêu cầu thứ 5 — dữ liệu mẫu** (xem mục Buổi 2 và **CR-2026-03** ở §4.4).

| Hạng mục | Quy ước sau khi đánh số lại |
|---|---|
| Số buổi trong tài liệu | "Buổi n" / "B{n}" luôn theo cách đánh số **8 buổi**. Các câu nhắc tới *kế hoạch 6 buổi cũ* ghi rõ là số của kế hoạch đó |
| Tài liệu đổi tên | `KE_HOACH_PHAT_TRIEN_7_BUOI.md` → `KE_HOACH_PHAT_TRIEN_8_BUOI.md`; `BAO_CAO_BUOI_1.md` → `BAO_CAO_BUOI_2.md`; `SRS_Culinary_Blog_v1.2.0.md` → `SRS_Culinary_Blog_v1.2.1.md` → `SRS_Culinary_Blog_v1.2.2.md` (CR-2026-04, 29/09/2026) → `SRS_Culinary_Blog_v1.2.3.md` (CR-2026-05, 12/10/2026) |
| Migration đã tạo | `B1_InitialSchema` **giữ nguyên tên** — đã áp dụng vào mọi cơ sở dữ liệu, đổi tên sẽ làm EF chạy lại migration. Migration **mới** đặt tên theo số buổi mới (`B3_…` → `B8_…`) |
| Nhánh Git đã có | Nhánh lịch sử `feature/b1-*` của 3 dev đã merge và xóa trên GitHub, giữ nguyên trong lịch sử commit. Nhánh của Dev 4 (`feature/b1-dev4-infra-file` + `fix/b1-runtime-hardening`) gộp thành **`2312755_NguyenThangThieng_buoiso2`** theo quy ước mới (§2.4) |
| Ngày tháng | Không đổi — Buổi 2 vẫn là 11/09 – 14/09/2026 (rà soát 20/09, bổ sung dữ liệu mẫu 21/09) |

### 0.2 Lý do mở rộng 6 → 7 buổi (giữ nguyên, đã đánh số theo 8 buổi)

Kế hoạch cũ được lập trên SRS **v1.0.0** — tài liệu chứa **41 điểm mâu thuẫn** đã được xử lý trong CR-2026. Vì vậy kế hoạch cũ không còn dùng được nguyên trạng: mục §1.3 của nó ("Giải quyết điểm mâu thuẫn — quyết định Tech Lead") đưa ra **7 quyết định mà 5 trong số đó nay đã bị SRS v1.1.0 đảo ngược** (422 cho kiểm tra hợp lệ, xóa cứng – hard delete recipe, 512-bit, lọc Draft theo danh tính, Output Cache).

**Vì sao cần thêm một buổi kỹ thuật (nay là Buổi 8):**

| Lý do | Chi tiết |
|---|---|
| SRS phát sinh thêm **3 FR mới** | `FR-AUTH-008` (Quản lý tài khoản [Admin]) và `FR-RCP-011` (`GET /recipes/mine`) ở v1.1.0; `FR-AUTH-009` (Quản lý phiên đăng nhập) ở v1.2.0 — tổng FR tăng từ 34 → **37** |
| Phát sinh thêm **11 endpoint mới** | v1.1.0: `/recipes/mine`, `/recipes/{id}/unarchive`, `/recipes/{id}/steps/reorder`, `/users/{id}/status`; v1.2.0: `GET /users`, 3 endpoint `/auth/sessions`, `GET /recipes/sitemap`, `POST /files/upload`, `DELETE /files/{**key}` — Chương 8 nay có **44 endpoint** |
| Phát sinh **nợ kỹ thuật phải hoàn trả** | Buổi 2 được code theo v1.0.0 nên có **18 hạng mục** lệch chuẩn v1.1.0 (17 cần sửa code, 1 xử lý qua Change Request), đã kiểm chứng trong code (xem §4.1) — trong đó 2 hạng mục là **lỗi bảo mật MT-34** và 1 hạng mục (**422 → 400**) ảnh hưởng mọi endpoint |
| Buổi 6 cũ **quá tải** | Buổi 6 cũ gộp cả E2E testing + WCAG + Nginx + Docker multi-stage + k6 load test vào một buổi — không thể hoàn thành với chất lượng nghiệm thu được |
| Nguyên tắc "mỗi buổi một sản phẩm chạy được" | Tách buổi cuối thành **Buổi 7 (hoàn thiện & tối ưu)** + **Buổi 8 (kiểm thử toàn diện & đóng gói)** để mỗi buổi đều có `git commit` trọn vẹn |

**Buổi 2 thực tế đã làm nhiều hơn kế hoạch** (theo `BAO_CAO_BUOI_2.md` §4.6 và code): `FR-JOB-001` Welcome Email đã chạy thật qua Hangfire + MailKit (thử lại 1′/5′/30′), Hangfire chạy ở **container worker riêng**, healthcheck Postgres/Redis/MinIO đã có, Nginx đã chuyển tiếp `X-Forwarded-For` và dùng **resolver DNS động**. Kế hoạch này **không giao làm lại** những việc đã xong; phần việc trống ra ở Buổi 3 của Dev 4 được dùng để dựng **bộ integration test (integration test harness)** — thứ mà mọi dev cần từ Buổi 3 trở đi — và (từ yêu cầu bổ sung 29/09/2026) **phần nền exception / repository & Unit of Work / middleware** cùng ba endpoint health check.

**Nguyên tắc xuyên suốt:** *mỗi buổi – mỗi dev – một lát cắt dọc hoàn chỉnh (DB → API → UI) – một commit*. Cuối mỗi buổi hệ thống phải chạy được, không gãy build. **Một FR không bị cắt đôi qua hai buổi** trừ khi có lý do kiến trúc được ghi rõ.

### 0.3 Yêu cầu Lab 5 → Lab 8 (12/10/2026) — viết lại Buổi 5 → 8

Ngày 12/10/2026 giảng viên ban hành **yêu cầu tối thiểu cho Lab 5 → Lab 8** (tương ứng Buổi 5 → 8), đặt bốn buổi cuối theo trục Frontend:

| Buổi | Trọng tâm của Lab |
|---|---|
| 5 | Bảo mật API (CORS, rate limit, versioning), tài liệu API (Scalar/OpenAPI), nền TypeScript Frontend |
| 6 | Tiện ích gọi API, shadcn/ui, Zustand store, giao diện tĩnh, bố cục dùng chung |
| 7 | Xác thực & hồ sơ, trang chủ, danh mục, chi tiết công thức, tìm kiếm — chạy end-to-end |
| 8 | Quản lý người dùng, biểu mẫu công thức + tải ảnh + máy trạng thái, SEO & WCAG, structured logging, load test, đóng gói |

Kế hoạch Buổi 5 → 8 lập trước ngày 12/10 **không đáp ứng** các yêu cầu này:
- CORS và rate limit nằm ở Buổi 6;
- không có Scalar, Zustand, shadcn/ui;
- không có đổi mật khẩu, quên mật khẩu.

Buổi 5 → 8 được **viết lại** theo năm nguyên tắc:

1. **Giữ nguyên Buổi 1 → 4** — cấu trúc, kết quả, giải trình. Chỉ cập nhật các câu ở Buổi 3 – 4 có nhắc tới buổi sau, để trỏ đúng buổi mới.
2. **Giữ phần Buổi 5 đã làm.** Ngày 07/10 – 12/10, cả 4 dev đã làm Buổi 5 theo kế hoạch cũ trên nhánh `buoiso5`. Trưởng nhóm quyết định (12/10/2026) giữ nguyên phần đó và **bổ sung** yêu cầu Lab 5. Buổi 5 của mỗi dev vì vậy có hai phần: **A** (yêu cầu tối thiểu Lab 5) và **B** (đã hoàn thành theo kế hoạch trước 12/10).
3. **"Nâng cấp", không "làm lại":** Lab 7/8 giao lại một số giao diện đã chạy được (trang chi tiết, danh mục, tìm kiếm, biểu mẫu nhiều bước, đăng nhập). Buổi đó dựng chúng lên nền chung của Buổi 5 – 6 (kiểu TypeScript, `ky`, shadcn/ui, Zustand) và làm phần còn thiếu.
4. **Lab khác SRS về cách làm → theo SRS, ghi rõ lý do** (nguyên tắc §3) — sáu điểm X-6 → X-11. **Lab đòi chức năng SRS chưa có → Change Request** — hai điểm X-12, X-13, gom thành CR-2026-05 (§4.6): đổi mật khẩu, quên/đặt lại mật khẩu, bộ lọc thời gian trên `/search`. Hai lỗi của chính SRS lộ ra khi thiết kế Lab 5–6 cũng được sửa trong CR này, **sau khi kiểm chứng thực nghiệm**: CSP có nonce buộc mọi trang render động (MT-67) và danh sách header CORS thiếu (MT-68).
5. **Không bỏ sót việc của kế hoạch cũ.** Những việc kế hoạch cũ đặt ở Buổi 5 → 8 mà Lab không nhắc được giao cho đúng chủ module ở buổi phù hợp nhất — 39/39 FR vẫn xong ở Buổi 8:
   - CSP, giao diện phiên đăng nhập;
   - xóa mềm, lưu trữ/khôi phục (giao diện);
   - sitemap, Nginx production, OpenTelemetry;
   - E2E, ADR.

**Bảng dời việc — kế hoạch trước 12/10 → hiện tại:**

| Việc | Trước 12/10 | Từ 12/10 | Lý do |
|---|---|---|---|
| Rate limit, CORS | B6 Dev 1 | **B5 Dev 1** | Lab 5 |
| API Versioning, tài liệu Scalar/OpenAPI, kiểu TypeScript theo miền | — | **B5** (Dev 1, Dev 2, cả nhóm) | Lab 5 (mới) |
| Lớp gọi API `ky` + bộ xử lý lỗi + route group | — | **B6 Dev 4** | Lab 6 (mới) |
| Zustand store + chặn route phía client + CSP | B6 Dev 1 (CSP, route guard) | **B6 Dev 1** | Lab 6; CSP đi cùng nơi lưu refresh token |
| shadcn/ui + trang tổng quan `/dashboard` | B6 Dev 2 (trang tổng quan) | **B6 Dev 2** | Lab 6; trang tổng quan là màn hình dùng thật đầu tiên của thư viện |
| Giao diện tĩnh + component khung | — | **B6 Dev 3** | Lab 6 (mới) |
| Giao diện hồ sơ (FR-AUTH-006/007) | B5 Dev 1 | **B7 Dev 1** | Lab 7 |
| Giao diện phiên (FR-AUTH-009) | B7 Dev 1 | **B7 Dev 1** (tab trong `/profile`) | không đổi |
| Đổi mật khẩu, quên/đặt lại mật khẩu (FR-AUTH-010/011) | — | **B7 Dev 1** | Lab 7 + CR-2026-05 |
| Giao diện quản lý tài khoản (FR-AUTH-008) | B7 Dev 1 | **B8 Dev 1** | Lab 8 |
| Trang chi tiết công thức (nâng cấp) | — | **B7 Dev 2** | Lab 7 |
| Danh sách "Công thức của tôi" | B6 Dev 2 | **B7 Dev 2** | đi cùng trang chi tiết |
| Xóa mềm / lưu trữ / khôi phục (giao diện) | B6, B7 Dev 2 | **B8 Dev 2** | Lab 8 "máy trạng thái khép kín" |
| Trang chủ, D-7 | B6 Dev 3 | **B7 Dev 3** | Lab 7 |
| JSON-LD, Open Graph | B6 Dev 3 | **B8 Dev 3** | Lab 8 |
| Sitemap, robots, D-14 phía Next.js | B7 Dev 3 | **B8 Dev 3** | gom cả SEO vào một buổi |
| Bộ lọc thời gian cho `/recipes/search` | — | API **B6 Dev 3** · giao diện **B7 Dev 4** | Lab 7 + CR-2026-05 |
| Serilog + OpenTelemetry | B6 Dev 4 | **B8 Dev 4** | Lab 8 |
| Nginx SSL, `/media/`, compose scale, D-17 | B7 Dev 4 | **B7 Dev 4** | không đổi; đi kèm trang tìm kiếm của Lab 7 |
| Audit cache + hit rate | B7 Dev 4 | **B8 Dev 4** | đo hit rate trong lúc load test |
| E2E, kiểm thử bảo mật, WCAG, Lighthouse, load test, ADR, `v1.0.0` | B8 cả nhóm | **B8 cả nhóm** | không đổi |

---

## 1. Phân công Full-Stack cố định (không đổi suốt 8 buổi)

| Dev | Module phụ trách | Phạm vi FR | Trọng tâm kiến trúc |
|---|---|---|---|
| **DEV 1** | Module 1 — Xác thực, Phân quyền & Quản lý Người dùng | `FR-AUTH-001` → `FR-AUTH-009` | JWT, Refresh Token Rotation, Security Hardening, `UseForwardedHeaders` |
| **DEV 2** | Module 3 — Quản lý Công thức Nấu ăn Lõi | `FR-RCP-001` → `FR-RCP-011` | Optimistic Concurrency, Máy trạng thái RecipeStatus, Soft Delete, Cách ly Public/Private |
| **DEV 3** | Module 2 & 4 — Danh mục, Tìm kiếm FTS & SEO | `FR-CAT-001`→`005`, `FR-SRCH-001`→`004` | PostgreSQL FTS (generated column), SSR + Data Cache (trước MT-67 là ISR), JSON-LD, Sitemap Next.js |
| **DEV 4** | Module 5, 6, 7 & Hạ tầng | `FR-FILE`, `FR-JOB`, `FR-OBS`, DevOps | MinIO/S3, Hangfire, Serilog/OTEL, Docker, Nginx, Redis |

> **Ghi chú phạm vi:** Đề bài giao DEV 1 phạm vi "FR-AUTH-001 đến 007" và DEV 2 "FR-RCP-001 đến 010". SRS bổ sung `FR-AUTH-008`, `FR-AUTH-009` và `FR-RCP-011`; các FR này thuộc đúng module của DEV 1 và DEV 2 nên được gán vào đó — ranh giới module giữ nguyên, chỉ mở rộng đuôi số.
>
> **Ngoại lệ duy nhất — Buổi 4 "hoàn thành tất cả API endpoints" (29/09/2026):**
> - Để cân bằng tải khi 25 endpoint dồn vào một buổi, **API** của FR-RCP-005/006/007 (vòng đời công thức) do **Dev 4** làm.
> - **API** của FR-RCP-011 (`/recipes/mine`, `/recipes/mine/{id}`) và `GET /recipes/sitemap` do **Dev 3** làm.
> - **Giao diện** của các FR này vẫn do Dev 2 làm (Dev 3 với sitemap), đúng module ở bảng trên.
>
> Chi tiết và lý do ở mục Buổi 4.
>
> **Ngoại lệ của Buổi 5 → 8 (theo đúng phân công của yêu cầu Lab, 12/10/2026):**
> - **Giao diện `/search` ở Buổi 7 do Dev 4 làm** (Lab 7). API tìm kiếm vẫn của Dev 3 (Buổi 4; mở rộng MT-66 ở Buổi 6). Hợp đồng giữa hai bên là SRS §8.3 + integration test.
> - **Kiểu TypeScript chia theo Lab 5:** `types/recipe.ts` — Dev 2; `types/category.ts`, `types/search.ts` — Dev 3; `types/auth.ts`, `types/api.ts` — Dev 4 (dù Auth thuộc module của Dev 1).
> - **Tài liệu OpenAPI cho mọi endpoint** do Dev 2 gắn ở Buổi 5 (Lab 5); chủ module duyệt phần endpoint của mình.

---

## 2. Quyết định kiến trúc chung (cả nhóm tuân thủ — theo SRS v1.2.3)

### 2.1 Cấu trúc repository (monorepo — giữ nguyên từ Buổi 2)

```
ptudwnc_nhom20/
├─ SPEC/                          # SRS v1.2.3 (+ bản gốc v1.0.0 PDF để đối chiếu), hồ sơ mâu thuẫn, kế hoạch này
├─ backend/
│  ├─ CulinaryBlog.sln
│  ├─ src/CulinaryBlog.Domain/          # Entities, VO (Slug, EmailAddress), Enums, Exceptions/{Auth,Recipes,Categories} + ErrorCodes – KHÔNG NuGet
│  ├─ src/CulinaryBlog.Application/     # Commands/Queries/Handlers (MediatR), Validators, DTOs, Behaviors, Interfaces (IRepository<T>, IUnitOfWork, repository theo module – B3)
│  ├─ src/CulinaryBlog.Infrastructure/  # EF Core DbContext, Configurations, Migrations, UnitOfWork + Repos + PersistenceExceptionTranslators, JwtService, MinIO, Redis, MailKit, Hangfire jobs, HealthChecks
│  ├─ src/CulinaryBlog.API/             # Minimal API Endpoint groups, Middleware (+ ExceptionMapping theo module), Program.cs; từ B5: RateLimiting/, OpenApi/ (Scalar /scalar/v1), Asp.Versioning
│  └─ tests/ (Domain.UnitTests, Application.UnitTests, API.IntegrationTests, ArchitectureTests)
├─ frontend/                      # Next.js 15 App Router + TS + Tailwind v4 + TanStack Query + RHF + Zod (+ từ B5–B6: Zustand, ky, shadcn/ui)
│  └─ src/{app/{(public),(auth),(dashboard)}, components/{ui,layout,shared}, features/{auth,recipes,categories,search,users,system}, hooks, lib/{api-client,error-handler,query-client,seo}, stores, types}
├─ nginx/nginx.conf               # B3: chuyển sang templates/ (envsubst secret) + htpasswd (gitignore)
├─ docker/postgres/init.sql       # chỉ CREATE EXTENSION; mọi DDL mà code phụ thuộc nằm trong migration (D-18)
├─ docker-compose.yml             # dev: 8 service + mailhog; docker-compose.prod.yml tạo ở B7; .env.example
└─ e2e/ (Playwright)
```

### 2.2 Quy ước kỹ thuật dùng chung

| Hạng mục | Quy ước (theo SRS v1.2.0) |
|---|---|
| BaseEntity | `Id (uuid, gen_random_uuid())`, `CreatedAt`, `UpdatedAt`, `IsDeleted` (Global Query Filter), `RowVersion bytea` (concurrency token) |
| RowVersion trên PostgreSQL | Cột `bytea` cấu hình `.IsConcurrencyToken()`; `AuditInterceptor` gán `Guid.NewGuid().ToByteArray()` mỗi lần Added/Modified (Postgres không tự sinh rowversion như SQL Server) |
| Pipeline MediatR | **Đúng 4 behavior** (SRS §6.3): `LoggingBehavior` (cảnh báo > 500ms) → `ValidationBehavior` → `CachingBehavior` (`ICacheable`) → Handler → `CacheInvalidationBehavior` (`ICacheInvalidator`). **Không có `PerformanceBehavior`.** |
| Ngưỡng cảnh báo | **> 500ms** cho toàn request = `LoggingBehavior`; **> 100ms** cho một query DB = **EF Core command interceptor** — hai chủ thể khác nhau, không nhầm lẫn |
| Mã lỗi HTTP | **400** = mọi lỗi kiểm tra hợp lệ/đầu vào (validation) · **409** = mọi xung đột trạng thái (unique, đồng thời, invalid state transition) · **423** = khóa tài khoản tạm thời (lockout) · **502** = Google JWKS lỗi. **Mã 422 bị loại bỏ hoàn toàn.** |
| Lỗi | RFC 7807 qua `GlobalExceptionMiddleware`; `type` = Application Error Code (SRS Phụ lục B, **30 mã** ở v1.2.3 — 27 mã của v1.2.0 + 2 mã của CR-2026-04 (§4.5) + 1 mã của CR-2026-05 (§4.6)) |
| Exception (từ B3) | Bất biến/quy tắc nghiệp vụ → lớp con của `XxxDomainException` trong `Domain/Exceptions/{Module}` (mang `Code`, **không mang mã HTTP**); mã HTTP đăng ký ở `API/Middleware/ExceptionMapping/{Module}ExceptionMappings.cs`; lỗi đầu vào → FluentValidation; phân quyền tài nguyên / dịch vụ ngoài → `AppException`. Bảng chọn loại lỗi: Buổi 3, mục "Hợp đồng kiến trúc" E |
| Truy cập dữ liệu (từ B3) | Phía **Command**: `IUnitOfWork.{Users, Categories, Recipes}` + `SaveChangesAsync()` một lần; nhiều lần lưu trong một transaction → `ExecuteInTransactionAsync` (đi qua execution strategy). Phía **Query**: read repository trả DTO `AsNoTracking`. Handler **không** tham chiếu `DbContext`, repository **không** trả `IQueryable`, không có xóa cứng. Lỗi ghi DB (`RowVersion`, `23505`) do `UnitOfWork` dịch sang domain exception qua `IPersistenceExceptionTranslator` của module |
| Response danh sách | `PagedResult<T> { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }` — khớp hình dạng Buổi 2 đã hiện thực; SRS v1.2.0 đã chốt (§5.2, **MT-42**), bỏ hẳn dạng `{ data, meta }`. Đối tượng đơn trả thẳng DTO |
| Phân trang | `pageSize` mặc định **12**, max 50; `sortBy ∈ {createdAt, publishedAt, title, cookTime, prepTime}`, `sortOrder ∈ {asc, desc}` — ngoài danh sách cho phép → **400** |
| Cache | **Redis cache-aside là cơ chế duy nhất.** Output Cache của .NET **không được dùng**. TTL theo bảng chuẩn NFR-PERF-003 |
| Endpoint | `/api/v1/...` (từ B5 đăng ký bằng `Asp.Versioning`: route `/api/v{version:apiVersion}`, URL thực tế không đổi), mỗi module một route group (`MapGroup`) – không Controller. Module có nhiều người cùng sửa thì tách file theo trách nhiệm, cùng gắn vào một group (từ B4: `RecipesEndpoints` ghi nội dung — Dev 2, `RecipeQueryEndpoints` mọi `GET` — Dev 3, `RecipeLifecycleEndpoints` vòng đời — Dev 4). Bề mặt API được khóa bởi `ApiSurfaceTests` (B4): route thật phải khớp đúng danh sách SRS Chương 8 |
| Định danh | **Đọc theo `slug`**, **ghi theo `id`**; DTO luôn mang cả `id` lẫn `slug` |
| Policy | `"AuthorPolicy"` = `RequireRole("Author","Admin")` (role Identity **không phân cấp**), `"AdminPolicy"`, `RecipeAuthorizationHandler` (Owner/Admin). **Không có policy `VerifiedAuthor`.** |
| Token phía Frontend | Access token **chỉ trong bộ nhớ**; refresh token là dữ liệu xác thực duy nhất lưu bền; access token mang claim **`sid`** (Id bản ghi refresh token) để nhận diện phiên hiện tại — SRS v1.2.0 NFR-SEC-002 (MT-55, MT-45) |
| FE API client | `lib/api-client.ts` — B2: fetch wrapper; **từ B6: instance `ky` bọc `fetch`** (giữ Data Cache của Next.js), hook gắn Bearer + `X-Correlation-ID`, refresh 401 single-flight, đổi lỗi sang `ApiError`; chỉ tự thử lại `GET`. API công khai (`apiFetch`, `ApiError`) không đổi. Lỗi xử lý tập trung ở `lib/error-handler.ts` (thông báo theo `ErrorCode`). TanStack Query keys theo module |
| FE form | React Hook Form + Zod schema mirror validator backend, lỗi inline theo `errors{}` |
| FE state (từ B6) | Dữ liệu server → **TanStack Query**; phiên đăng nhập phía client → **Zustand** `stores/auth-store.ts` (chỉ refresh token + `user` lưu bền); biểu mẫu → React Hook Form; bộ lọc/trang → `searchParams`. Không đưa dữ liệu server vào Zustand |
| FE UI (từ B6) | **shadcn/ui** (Radix UI + Tailwind) trong `components/ui/` — code chép vào repo, sửa trực tiếp; màu qua token CSS đạt tương phản WCAG AA |
| Bảo mật API (từ B5) | CORS named policy (kiểm cấu hình lúc khởi động, expose `ETag`/`Retry-After`/`X-RateLimit-*`); rate limit theo IP thật: sliding 10/phút `/auth/*`, fixed 100/phút toàn cục, fixed 5/phút upload (NFR-SEC-003); hạn mức cấu hình được |
| Render & CSP (từ B6 — MT-67) | CSP có nonce sinh mỗi request ở `src/middleware.ts` + `'strict-dynamic'` (NFR-SEC-005) → **mọi trang HTML render động**; dữ liệu công khai cache ở **Data Cache** của `fetch` với `revalidate` = TTL (§2.3). Không dùng ISR toàn trang (trừ `sitemap.xml`, `robots.txt` — không chứa script). Server Component **không bao giờ** gọi endpoint riêng tư |
| Tài liệu API (từ B5) | OpenAPI 3.1 sinh từ code (.NET 10) + Scalar `/scalar/v1`; mọi endpoint có summary/tag/operationId/response; ảnh chụp `docs/api/openapi.v1.json` chặn thay đổi hợp đồng không chủ đích |

### 2.3 Bảng TTL cache chuẩn (SRS NFR-PERF-003 — nguồn sự thật duy nhất)

| Khóa cache | TTL | Invalidate khi | `revalidate` tương ứng ở FE (Data Cache của `fetch`; trước MT-67 là ISR) |
|---|---|---|---|
| `categories:all` | 30 phút | Create/Update/Delete category | `/categories` → data `revalidate: 1800` |
| `categories:detail:{slug}:{queryHash}` | 2 phút | Thay đổi category đó hoặc recipe thuộc nó | `/categories/[slug]` → data `revalidate: 120` *(trang render động vì đọc `?page=` — xem `BAO_CAO_BUOI_2.md` §4.5)* |
| `recipes:list:{queryHash}` | 2 phút | Bất kỳ thay đổi recipe nào (xóa theo tiền tố – prefix) | `/` → `revalidate=120` |
| `recipe:{slug}` | 5 phút | Update/Publish/Unpublish/Archive/Unarchive/Delete recipe đó | `/recipes/[slug]` → `revalidate=300` |
| `search:{queryHash}` | 1 phút | Không xóa cache (invalidate) — hết hạn tự nhiên. Từ B6 khóa băm cả `maxCookTime`/`maxPrepTime`/`minServings` (MT-66) | — (SSR lần đầu; từ B7 client `staleTime` 60 giây) |
| **`GET /recipes/mine`** | **CẤM CACHE** | — | — (CSR, `Cache-Control: no-store`) |

> **Quy tắc vàng:** `revalidate` ở tầng Next.js phải **≤ TTL cache API** tương ứng. Nếu tầng Next.js giữ dữ liệu lâu hơn, nó trở thành tầng cache cũ nhất và mọi nỗ lực xóa cache ở Backend đều vô nghĩa. Từ Buổi 6 (MT-67), giá trị này nằm trên **từng lời gọi `fetch`** (Data Cache) chứ không ở `export const revalidate` của trang — vì CSP có nonce buộc trang render theo request; thực nghiệm xác nhận Data Cache vẫn giữ dữ liệu, không gọi lại API trong khoảng `revalidate`.

### 2.4 Quy trình Git

- Nhánh: `main` (luôn xanh) ← `develop` ← `{mssv}_{HoTenKhongDau}_buoiso{n}` — MSSV, họ tên đầy đủ viết liền không dấu (viết hoa chữ cái đầu mỗi từ), số buổi theo cách đánh số 8 buổi. VD `2314236_HoangBinhQuan_buoiso3`, `2312755_NguyenThangThieng_buoiso2`. Bảng MSSV–tên nhánh: xem `BAO_CAO_BUOI_2.md` §7.1.
- Conventional Commits; **1 commit squash / dev / buổi**, PR có ≥1 reviewer (NFR-MAINT-001).
- **Pull Request luôn nhắm vào `develop`, không bao giờ nhắm vào `main`.** `main` chỉ nhận merge từ `develop` cuối buổi, sau khi 4 nhánh đã vào `develop` theo đúng thứ tự và kiểm chứng cuối buổi đã xanh. *(Bổ sung 29/09/2026 — PR #1 đầu tiên của nhóm nhắm thẳng vào `main`.)*
- **Danh tính commit phải là của chính thành viên:**
  - `git config user.name` = họ tên thật;
  - `git config user.email` = email đã gắn với tài khoản GitHub của mình (email trường `{mssv}@dlu.edu.vn` hoặc email cá nhân đã thêm vào GitHub).

  Commit mang tên/email khác không được GitHub gắn vào tài khoản của thành viên, nên **không chứng minh được ai đã làm phần việc đó** khi chấm điểm. *(Bổ sung 29/09/2026 — đã có commit mang tên "Nguyen Van Teo".)*
- **Không push code chưa build được.** Trước mỗi lần push phải chạy `dotnet build` (0 cảnh báo — `TreatWarningsAsErrors`) và `dotnet test`; frontend chạy `npm run lint && npm run build`. Nhánh không build được thì không rà soát được và chặn cả nhóm khi merge.
- **Báo cáo cá nhân mỗi buổi:**
  - tên file `SPEC/BAO_CAO_LAB_0{n}_{MSSV}_{HoTenKhongDau}.md` (kèm bản `.docx` theo mẫu `SPEC/Mau_Nop_Bao_Cao_Lab_Ca_Nhan_2026.pdf`), với **n = số buổi** theo cách đánh số 8 buổi (Buổi 3 → `LAB_03`);
  - nằm trong **cùng nhánh** với code của buổi đó;
  - chỉ ghi "hoàn thành" cho hạng mục đã kiểm chứng được (test/ảnh chụp/log).
- **Migration:** mỗi dev tạo migration riêng tên `B{n}_{Module}_{FR}`; merge theo thứ tự **Dev4 → Dev1 → Dev3 → Dev2**, dev sau `rebase` + `dotnet ef migrations add` lại nếu xung đột snapshot.
- **Definition of Done** mỗi commit: `dotnet build` 0 cảnh báo (repo bật `TreatWarningsAsErrors` — **dùng API lỗi thời là gãy build**) · `dotnet test` xanh **kể cả integration test** (bộ integration test có từ Buổi 3) · `npm run lint && npm run build` xanh · `docker compose up` chạy · kiểm thử tay qua Scalar + UI.
- **Commit nền đầu buổi:** thay đổi xuyên suốt nhiều module (ví dụ D-11) được làm thành **một commit riêng trong 30 phút đầu buổi** trước khi 4 dev tách nhánh — giống commit bootstrap của Buổi 2 — để không ai phải merge chéo vào file của người khác.

---

## 3. ⚠️ Xung đột giữa lộ trình / yêu cầu Lab được giao và SRS — đã giải quyết theo SRS hoặc qua Change Request

Lộ trình dự kiến trong đề bài được viết trước khi SRS v1.1.0/v1.2.0 hoàn tất, nên có **5 điểm mâu thuẫn** (cũng được ghi tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §5). Theo nguyên tắc bắt buộc *"Bám sát 100% SRS v1.1.0"*, kế hoạch này giải quyết theo SRS và ghi rõ tại đây để nhóm không hiểu nhầm:

| # | Lộ trình đề bài ghi | SRS quy định | Quyết định áp dụng | Căn cứ |
|---|---|---|---|---|
| **X-1** | Buổi 3 Dev 1: *"FR-AUTH-003 Google OAuth **PKCE**"* | **ID Token flow** — FE gửi `{ idToken }`, BE xác minh bằng `Google.Apis.Auth`. **Bỏ Authorization Code + PKCE**, BE không có redirect URI | Làm theo **ID Token flow** | SRS §5.3, FR-AUTH-003, **MT-11** |
| **X-2** | Buổi 4 Dev 4: *"FR-JOB-003 **Recurring Sitemap Generator** 02:00 AM UTC"* | `FR-JOB-003` = **Dọn dữ liệu vĩnh viễn Job** (dọn dữ liệu xóa mềm – soft delete > 30 ngày), chạy **03:30 UTC**. Sitemap chuyển sang **Next.js `app/sitemap.ts`** | Buổi 4 Dev 4 làm **Purge Job**; sitemap giao Dev 3 làm ở **Buổi 8** (cập nhật 12/10/2026 — gom vào buổi SEO của Lab 8; kế hoạch trước đặt Buổi 7, trùng với lộ trình đề bài) | SRS §3.6 FR-JOB-003, NFR-SEO-003, **MT-05 + MT-28** |
| **X-3** | Buổi 6 Dev 3: *"Trang Danh mục (ISR **revalidate 3600s**)"* | `/categories` = **1800s**, `/categories/[slug]` = **120s**, `/` = **120s** | Dùng số của SRS (xem bảng §2.3) | SRS §5.1, **MT-33.3** |
| **X-4** | Phân công ghi *"FR-AUTH-001 đến 007"* và *"FR-RCP-001 đến 010"* | Tồn tại thêm **`FR-AUTH-008`**, **`FR-AUTH-009`** và **`FR-RCP-011`** | Gán `FR-AUTH-008` + `FR-AUTH-009` → Dev 1 (giao diện: FR-AUTH-009 Buổi 7, FR-AUTH-008 Buổi 8), `FR-RCP-011` → Dev 2 (giao diện Buổi 5 – 7) — đúng module, không đổi ranh giới phân công | SRS §2.2 (37 FR ở v1.2.2; 39 FR ở v1.2.3), **MT-22, MT-34, MT-45** |
| **X-5** | Buổi 7 Dev 1: *"Quản lý phiên làm việc nâng cao (Force revoke, Session Management UI)"* | v1.1.0 **không có FR nào**; "force revoke" đã nằm trong FR-AUTH-008 | Giữ tính năng vì đề bài yêu cầu; SRS v1.2.0 đã bổ sung **FR-AUTH-009** (mức C) + 3 endpoint `/auth/sessions` để tính năng có yêu cầu truy vết được | SRS FR-AUTH-009, **MT-45** |

**Lý do không chọn cách "làm theo đề bài cho nhanh":** các điểm X-1 → X-4 đều thuộc loại *nếu code theo bản cũ thì sau này phải viết lại*, không phải khác biệt bề mặt. Cụ thể: PKCE đòi BE quản lý state/verifier + chuyển hướng callback (xóa đi là bỏ cả một luồng code); sitemap sinh ở BE nằm sai domain nên crawler không bao giờ đọc được; ISR dài hơn TTL làm vô hiệu toàn bộ cơ chế xóa cache. Riêng X-5 thì ngược lại: tính năng hợp lý nhưng **SRS v1.1.0 chưa có** — âm thầm thêm endpoint ngoài SRS sẽ phá vỡ chính nguyên tắc "SRS là nguồn sự thật", nên nó đã được đưa qua quy trình Change Request (CR-2026-02) và trở thành FR-AUTH-009 trong SRS v1.2.0.

**Xung đột giữa yêu cầu tối thiểu Lab 5 → Lab 8 (12/10/2026) và SRS / hiện trạng code** — ghi cả ở `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §5:

| # | Lab ghi | SRS / hiện trạng | Quyết định áp dụng | Căn cứ |
|---|---|---|---|---|
| **X-6** | Lab 5 Dev 1: *"`FixedWindowLimiter` cho Auth, `SlidingWindow` cho API"* | NFR-SEC-003: *"Fixed Window, sliding window cho auth"* — ngược lại | Theo **SRS**: sliding 10/phút cho `/auth/*`, fixed 100/phút toàn cục, fixed 5/phút upload. Vẫn dùng đủ hai loại limiter Lab yêu cầu (quyết định của trưởng nhóm 12/10/2026) | NFR-SEC-003; Buổi 5 — Dev 1 |
| **X-7** | Lab 6 Dev 1: *"persist token vào HTTP-only cookie / localStorage"* | NFR-SEC-002: access token chỉ trong bộ nhớ, refresh token là dữ liệu duy nhất lưu bền. §5.2, NFR-SEC-005: không dùng cookie cho xác thực; chuyển sang cookie phải qua CR | Zustand `persist` + `partialize` **chỉ lưu refresh token + `user`** vào `localStorage`; không dùng cookie | NFR-SEC-002/005, MT-55; Buổi 6 — Dev 1 |
| **X-8** | Lab 8 Dev 1: *"API lấy danh sách người dùng dành cho Admin/Author"*; *"trang quản lý người dùng & phân quyền"* | FR-AUTH-008, §8.1: `GET /users` chỉ **Admin**; SRS không có chức năng đổi vai trò | Giữ **chỉ Admin** — Author thấy email của mọi người dùng là rò rỉ dữ liệu. "Phân quyền" = trang chỉ Admin vào được + hiển thị vai trò + khóa/mở khóa. Đổi vai trò **không** đưa vào CR-2026-05 (quyết định 12/10/2026) | FR-AUTH-008, MT-44; Buổi 8 — Dev 1 |
| **X-9** | Lab 5 Dev 3: *"Khởi tạo dự án Next.js 15 App Router với TypeScript & Tailwind CSS"* | Frontend Next.js 15.5.25 + TS + Tailwind v4 đã chạy từ Buổi 2 | **Không khởi tạo lại** (sẽ xóa 4 buổi code); chuẩn hóa cấu trúc thư mục + quy tắc import kiểm bằng ESLint | Nguyên tắc liên tục §0.3; Buổi 5 — Dev 3 |
| **X-10** | Lab 5 Dev 4: *"`axios`/`ky`"* | SRS không chỉ định; Next.js gắn Data Cache vào `fetch` (§2.3, MT-33.3) | Chọn **`ky`** (bọc `fetch`). `axios` không đi qua `fetch` nên mọi `revalidate` mất tác dụng | §2.3; Buổi 5 — Dev 4 |
| **X-11** | Lab 6 Dev 2: component *"`Toast`"* của shadcn/ui | `toast` của shadcn đã ngừng phát triển, thay bằng `sonner`; dự án dùng thư viện `sonner` từ Buổi 2 | Dùng component `sonner` của shadcn | Buổi 6 — Dev 2 |
| **X-12** | Lab 7 Dev 1: *"Đổi mật khẩu (`POST /api/v1/auth/change-password`), Quên mật khẩu (`POST /api/v1/auth/forgot-password`)"* | SRS v1.2.2 không có FR/endpoint nào; `ApiSurfaceTests` khóa 45 endpoint; quên mật khẩu không dùng được nếu thiếu bước đặt lại | **CR-2026-05**: FR-AUTH-010, FR-AUTH-011, 3 endpoint (thêm `reset-password`), 1 mã lỗi → SRS v1.2.3 | MT-64, MT-65; §4.6 |
| **X-13** | Lab 7 Dev 4: trang `/search` *"kèm bộ lọc đa tiêu chí (danh mục, độ khó, thời gian)"* | §8.3: `GET /recipes/search` chỉ nhận `q`, `categoryId`, `difficulty` | **CR-2026-05**: thêm `maxCookTime`, `maxPrepTime`, `minServings` (tùy chọn, tương thích ngược) | MT-66; §4.6 |

**Vì sao vẫn theo SRS ở X-6 → X-11 dù đó là "yêu cầu tối thiểu" của Lab:** mỗi điểm có hai vế.
- **Vế mục tiêu** — có rate limit dùng cả hai thuật toán, có store lưu phiên, có trang quản lý người dùng, có dự án Next.js chuẩn, có thư viện gọi API, có toast — **được đáp ứng đầy đủ**.
- **Vế cách làm cụ thể** được chỉnh để không phá một quyết định SRS đã có lý do bảo mật hoặc lý do cơ chế cache.

Báo cáo Lab của từng dev trích lại dòng tương ứng của bảng này để giảng viên thấy yêu cầu đã được đáp ứng ra sao.

---

## 4. Nợ kỹ thuật, Change Request và Errata

### 4.1 Nợ kỹ thuật từ Buổi 2 — đã kiểm chứng trong code (Buổi 2 giữ nguyên, retrofit ở buổi sau)

Buổi 2 đã code và merge theo **SRS v1.0.0**. Nội dung Buổi 2 trong tài liệu này **giữ nguyên 100%** (đúng yêu cầu), nhưng **18 hạng mục** dưới đây lệch chuẩn SRS (v1.1.0/v1.2.0). Bảng này cũng được ghi tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §4. Mỗi hạng mục được **đối chiếu trực tiếp với code trên `main`** (cột "Bằng chứng"), không suy đoán từ kế hoạch cũ, và được gán chủ + buổi cụ thể.

| # | Hiện trạng trong code Buổi 2 | Bằng chứng | Chuẩn SRS v1.2.0 | Retrofit tại | MT |
|---|---|---|---|---|---|
| **D-1** | API đăng ký nhận `{ fullName, email, userName, password }` — **người dùng tự nhập `userName`**; `UserDto` trả `fullName`, `userName`, thiếu `bio`; có mã `AUTH_USERNAME_EXISTS`. *(Cột DB đã đúng là `DisplayName` — **không cần migration**)* | `AuthEndpoints.cs:52`, `AuthContracts.cs:8`, `RegisterUserCommand.cs:9`, `ErrorCodes.cs:7` | Body `{ email, password, displayName }`; **BE tự sinh `UserName`** từ tiền tố email; DTO `{ id, email, displayName, avatarUrl, bio, roles }` | **B3 – Dev 1** (Google sign-in cần chung bộ sinh `UserName`) | MT-12 |
| **D-2** | Refresh token **64 byte = 512-bit** | `JwtTokenService.cs:17` `RefreshTokenBytes = 64` | **32 byte = 256-bit** | **B4 – Dev 1** | MT-13 |
| **D-3** | `Recipe.Slug` là UNIQUE thường | `RecipeConfiguration.cs:60` | **Partial unique** `WHERE "IsDeleted" = false` | **B4 – Dev 4** *(kéo từ B6 — đi cùng `DELETE /recipes/{id}`)* | MT-05 |
| **D-4** 🔴 | `GetRecipesQuery` lọc theo danh tính (Guest 46 / Author 47 / Admin 50 bản ghi) **và** bị Output Cache theo khóa công khai; `GetRecipeBySlug` trả **403** cho Draft (lộ sự tồn tại của bản nháp) | `RecipeReadRepository.cs:87-97`, `GetRecipeBySlugQuery.cs:31-33` | **Chỉ `Published` cho mọi người gọi**; Draft/Archived → **404** như slug không tồn tại; dữ liệu riêng qua `/recipes/mine` | **B4 – Dev 3** *(kéo từ B6 — hợp đồng `GET /recipes`)* | **MT-34** |
| **D-5** 🔴 | `GetCategoryBySlugQuery` cũng lọc theo danh tính | `GetCategoryBySlugQuery.cs:36` | **Chỉ `Published`**, không đọc `ICurrentUser` | **B4 – Dev 3** *(kéo từ B6)* | **MT-34** |
| **D-6** | Output Cache (Redis-backed) `RecipeList` 15′ / `RecipeDetail` 60′ | `OutputCachePolicies.cs:16-24`, `RecipesEndpoints.cs:19,26` | **Bỏ Output Cache**, dùng `ICacheable` Redis cache-aside TTL 2′/5′ | **B4 – Dev 3** *(kéo từ B6)*; audit **B8 – Dev 4** | MT-16, MT-17, MT-34 |
| **D-7** | FE fetch `revalidate` 3600 (`/categories`) và 600 (`/categories/[slug]`) | `BAO_CAO_BUOI_2.md` §4.5 | **1800** và **120** | `/categories/[slug]`: **B5 – Dev 3** (đã làm) · `/categories`: **B7 – Dev 3** | MT-33.3 |
| **D-8** | `Recipe.Instructions` là `NOT NULL` | `RecipeConfiguration.cs:25` `.IsRequired()` | **NULL** (trường cũ) | **B3 – Dev 2** — *phải làm trước khi `POST /recipes` nhận `instructions?` tùy chọn, nếu không mọi request thiếu trường này sẽ lỗi ở DB* | MT-20.8 |
| **D-9** | Hangfire chạy ở **container worker riêng** (`Hangfire__WorkerOnly=true`), trong khi SRS v1.1.0 §3.6 ghi "in-process" | `docker-compose.yml` service `hangfire`, `Program.cs:17-18` | **SRS v1.2.0 đã chuẩn hóa theo code** — worker riêng (MT-47). Không còn lệch, không retrofit | — | MT-47 |
| **D-10** | Sắp xếp bằng **một tham số `sort=-field`**; enum thiếu `PrepTime`; FE gửi `sort=-createdAt` | `RecipeSortParser.cs`, `IRecipeReadRepository.cs:7-13`, `RecipesEndpoints.cs:42`, `frontend/src/app/recipes/(list)/page.tsx:37` | **`sortBy` + `sortOrder`**, danh sách cho phép 5 trường | **B4 – Dev 3** (API) · **B5 – Dev 3** (FE) | MT-01 |
| **D-11** | Mọi lỗi kiểm tra hợp lệ trả **422**; FE bắt lỗi trường bằng `status === 422` | `GlobalExceptionMiddleware.cs:65-83`, `AuthEndpoints.cs:17,26`, `RecipesEndpoints.cs:21`, `LoginForm.tsx:35`, `RegisterForm.tsx:44` | **400** cho mọi lỗi kiểm tra hợp lệ; **bỏ hẳn 422** | **B3 – commit nền đầu buổi** (Dev 4 dẫn) | MT-08 |
| **D-12** | Access token **và** refresh token lưu trong **`localStorage`** | `auth-context.tsx:39-56`; `BAO_CAO_BUOI_2.md` §4.12 ("cần xem lại Buổi 4") | Access token **chỉ trong bộ nhớ**; refresh token là thứ duy nhất được lưu bền (xem giải trình B4 Dev 1) | **B4 – Dev 1** *(làm sớm — kế hoạch đặt B5; commit `5239618`, `f473fca`)* | NFR-SEC-002 |
| **D-13** | TTL `categories:all` = **60 phút** | `GetCategoriesQuery.cs:11` | **30 phút** | **B3 – Dev 3** | MT-17 |
| **D-14** | `next/image` chạy `unoptimized`; URL ảnh là `localhost:9000` — **container frontend không truy cập được** | `BAO_CAO_BUOI_2.md` §4.13, `MinioOptions.PublicBaseUrl` | Ảnh phục vụ qua Nginx `/media/` → MinIO, bật lại tối ưu ảnh | **B7 – Dev 4** (Nginx) + **B8 – Dev 3** (Next.js) | NFR-PERF-005 |
| **D-15** | `Recipe.Publish()` **cho phép Archived → Published** và coi Published → xuất bản là no-op; ném `DomainException` chung không có mã lỗi | `Recipe.cs:114-128` | Chuyển trạng thái ngoài bảng → **409 `RECIPE_INVALID_STATE_TRANSITION`**; thiếu bước/nguyên liệu → **400 `RECIPE_PUBLISH_INCOMPLETE`** | **B4 – Dev 4** *(kéo từ B5 — đi cùng API xuất bản)* | MT-35, MT-06 |
| **D-16** | UNIQUE `(RecipeId, StepNumber)` khai báo bằng **unique index** — PostgreSQL **không cho phép index là `DEFERRABLE`** | `RecipeConfiguration.cs:87` `HasIndex(...).IsUnique()` | Đổi thành **unique constraint `DEFERRABLE INITIALLY DEFERRED`** để đánh số lại trong một transaction | **B4 – Dev 2** | MT-03 |
| **D-17** | Khóa DataProtection lưu **không mã hóa** trên volume `dpkeys` | `BAO_CAO_BUOI_2.md` §5.1 (cảnh báo `XmlKeyManager[35]`) | `ProtectKeysWithCertificate` ở production | **B7 – Dev 4** (cùng HTTPS) | NFR-SEC-007 |
| **D-18** | `init.sql` tạo text search config `vietnamese_unaccent` — **cơ chế FTS thứ hai**, lại chỉ chạy trong Docker (Testcontainers không chạy file này) | `docker/postgres/init.sql` | Một cơ chế duy nhất theo SRS: `simple` + `unaccent_immutable`, khai báo **trong migration** | **B4 – Dev 3** | MT-25 |

**Phân bổ retrofit theo buổi (cập nhật 12/10/2026):** B3 → D-1, D-8, D-11, D-13 · **B4 → D-2, D-3, D-4, D-5, D-6, D-10 (API), D-12 (làm sớm), D-15, D-16, D-18** · B5 → D-10 (FE), D-7 (một phần) · B7 → D-7 (phần còn lại), D-14 (Nginx), D-17 · B8 → D-14 (Next.js), audit D-6. Mọi retrofit làm **thay đổi hợp đồng của một endpoint** dồn về Buổi 4, vì yêu cầu "hoàn thành tất cả API endpoints" không đạt được khi endpoint còn trả sai mã hoặc rò dữ liệu. Hệ quả tốt: lỗ hổng MT-34 được vá sớm hai buổi.

> **Vì sao không dồn retrofit vào đầu Buổi 3:** dồn 17 hạng mục vào một buổi sẽ tạo một commit khổng lồ chạm vào code của cả 4 dev cùng lúc → xung đột merge và không ai rà soát nổi. Mỗi hạng mục được gắn vào **đúng buổi mà dev đó đang mở lại file liên quan** — sửa khi tay đã ở trong file là rẻ nhất và dễ rà soát nhất. **Ngoại lệ duy nhất là D-11** (422 → 400): nó chạm vào middleware dùng chung và mọi endpoint mới của Buổi 3 đều phụ thuộc vào nó, nên phải làm **trước tiên** dưới dạng commit nền.

### 4.2 CR-2026-02 — Bổ sung và làm rõ SRS (đã duyệt, áp dụng trong SRS v1.2.0)

Khi đối chiếu SRS v1.1.0 với code Buổi 2, kế hoạch phát hiện 8 điểm SRS còn thiếu hoặc tự mâu thuẫn. SRS quy định *mọi thay đổi sau khi duyệt phải qua Change Request*, nên kế hoạch **không tự thêm endpoint ngoài SRS**: cả 8 điểm được gom thành **CR-2026-02**, đã được duyệt và áp dụng vào **SRS v1.2.0** (19/09/2026). Phân tích đầy đủ từng điểm (hiện trạng, phương án, lý do chọn) nằm trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md` theo số MT tương ứng. Nhờ vậy, **mọi việc trong kế hoạch này đều truy vết được về một yêu cầu trong SRS**. *(Cập nhật 29/09/2026: yêu cầu bổ sung của Buổi 3 phát sinh 2 mã lỗi chưa có trong SRS — đang chờ duyệt dưới dạng **CR-2026-04**, xem §4.5.)*

| # | Vấn đề ở SRS v1.1.0 | Quyết định trong SRS v1.2.0 | MT | Buổi thực hiện |
|---|---|---|---|---|
| **C-1** | Hình dạng response danh sách: §5.2/§8 ghi `{ data, meta }` (kèm `pageSize: 10`), FR-SRCH-004 và code dùng `PagedResult` | Chốt **`PagedResult<T>`**; đối tượng đơn trả thẳng DTO | MT-42 | Đã đúng từ B2 — không phải sửa code |
| **C-2** | `POST /files/upload`, `DELETE /files/{**key}` có từ Buổi 2 nhưng ngoài SRS — trong khi `avatarUrl`, `imageUrl` danh mục và ảnh bước nấu **chỉ nhận URL** | **Chính thức hóa** vào FR-FILE-001/002, thêm Chương 8.8 | MT-43 | Đã có từ B2; dùng ở B3 (Dev 3), B4 (Dev 2), B7 (Dev 1 — ảnh đại diện) |
| **C-3** | `/dashboard/users` (§5.1) không có API danh sách người dùng | Thêm `GET /users` vào **FR-AUTH-008** | MT-44 | API B4 – Dev 1 · UI B8 – Dev 1 |
| **C-4** | Quản lý phiên có trong lộ trình (X-5) nhưng không có FR | Thêm **FR-AUTH-009** (mức C) + 3 endpoint `/auth/sessions` + claim `sid` | MT-45 | API B4 – Dev 1 · UI B7 – Dev 1 |
| **C-5** | 3 mã lỗi code đã dùng nhưng thiếu trong Phụ lục B | Thêm `FILE_FORBIDDEN` (403), `FILE_STORAGE_UNAVAILABLE` (503), `INTERNAL_ERROR` (500) — 24 → **27 mã**. **Không** thêm `AUTH_USERNAME_EXISTS` (bị xóa khi làm D-1) | MT-46 | Đã có từ B2; xóa `AUTH_USERNAME_EXISTS` ở B3 |
| **C-6** | Hangfire "in-process" (SRS) vs worker riêng (code) — xem **D-9** | Chuẩn hóa **worker riêng** | MT-47 | Không phải sửa code |
| **C-7** | Sitemap cần toàn bộ slug, `GET /recipes` giới hạn 50 | Thêm **`GET /recipes/sitemap`** gọn nhẹ, cache 1 giờ | MT-48 | B4 – Dev 3 |
| **C-8** | Hứa "có thể khôi phục" nhưng không có chức năng khôi phục | Làm rõ: khôi phục do **quản trị viên vận hành** trên DB trong 30 ngày; tự phục vụ ngoài phạm vi (§1.2.3) | MT-49 | Không triển khai trong 7 buổi |

### 4.3 Lỗi kỹ thuật trong SRS v1.1.0 — đã sửa trong SRS v1.2.0

Cùng lượt đối chiếu còn phát hiện **8 lỗi kỹ thuật nằm trong chính SRS v1.1.0**: đặc tả nghe hợp lý trên giấy nhưng **làm đúng từng chữ thì hệ thống hỏng**. Tất cả đã được sửa trong SRS v1.2.0; kế hoạch này làm theo bản đã sửa. Ba điểm đầu (E-1 → E-3) là những điểm đã được nêu ở bản kế hoạch trước; năm điểm sau được tìm thấy khi rà lại toàn bộ SRS để áp dụng CR-2026-02.

| # | SRS v1.1.0 ghi | Vì sao sai | SRS v1.2.0 / kế hoạch áp dụng | MT |
|---|---|---|---|---|
| **E-1** | §6.5 cấu hình Nginx `upstream api_pool { server api:8080; }` (lặp lại ở §6.1, NFR-SCALE-003) | **Tái tạo đúng lỗi 502 đã sửa ở Buổi 2** (commit `064f582`): khối `upstream` chỉ phân giải DNS **một lần lúc Nginx khởi động** → container `api` được tạo lại đổi IP là Nginx trả 502 | `resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến. Khi `--scale api=3`, Docker DNS trả nhiều bản ghi và Nginx tự luân phiên — còn **tự nhận replica mới trong 10 giây**, tốt hơn `upstream` tĩnh | MT-50 🔴 |
| **E-2** | §6.5 healthcheck dùng `curl` cho `api`, `frontend`, `minio`; `pg_isready` không có `-h` | `aspnet:10.0` và `minio/minio` **không có `curl`**; `node:22-alpine` chỉ có `wget`; `pg_isready` không `-h` báo "ready" khi Postgres **chưa mở TCP** (lỗi #1 của Buổi 2) | `pg_isready -h 127.0.0.1`, `redis-cli ping`, `mc ready local`; `api` cài `curl` ở stage runtime; `frontend` dùng `wget` | MT-51 |
| **E-3** | §8.1: Admin tự khóa mình → **409** (`VALIDATION_ERROR`) | Mâu thuẫn với FR-AUTH-008 A2 (**403**) và với Phụ lục B (`VALIDATION_ERROR` luôn là 400) | **403 Forbidden** | MT-52 |
| **E-4** | "`/recipes/search` phải được **đăng ký trước** `/recipes/{slug}`"; danh sách slug dành riêng mỗi nơi một kiểu | ASP.NET Core ưu tiên segment literal **bất kể thứ tự khai báo** — vấn đề thật là slug trùng từ khóa sẽ không truy cập được | **Một** danh sách slug dành riêng tại NFR-SEO-004: `search, mine, sitemap, new, edit` | MT-53 |
| **E-5** | "Khai báo `KnownProxies` giới hạn ở dải mạng Docker" | `KnownProxies` chỉ nhận **từng IP**; `KnownNetworks` đã **lỗi thời trên .NET 10** → gãy build vì `TreatWarningsAsErrors` | `KnownIPNetworks` + `System.Net.IPNetwork` | MT-54 |
| **E-6** | Không quy định Frontend lưu token ở đâu | Khoảng trống này dẫn thẳng tới việc Buổi 2 lưu cả hai token vào `localStorage` (D-12) | Access token **chỉ trong bộ nhớ**; CSP thành yêu cầu tường minh | MT-55 |
| **E-7** | `UNIQUE (RecipeId, StepNumber)` "khai báo `DEFERRABLE`" mà không nói dạng khai báo | PostgreSQL **không cho index là deferrable** — làm bằng `HasIndex().IsUnique()` thì kéo-thả sắp xếp bước luôn lỗi `23505` (D-16) | Ghi rõ **UNIQUE CONSTRAINT**; ảnh chính (partial index) đổi hai bước trong một transaction | MT-56 |
| **E-8** | `/categories/[slug]` là "ISR" | Trang đọc `?page=` nên Next.js **bắt buộc render động** — không thể ISR | **SSR + Data Cache** `revalidate: 120` | MT-57 |

> **Bài học ghi lại:** cả 8 lỗi trên đều **không thể phát hiện bằng cách đọc chéo các chương với nhau** — chỉ lộ ra khi đối chiếu tài liệu với hệ thống đang chạy. Đây là lý do mọi quyết định trong kế hoạch này được kiểm chứng trên code thật (xem cột "Bằng chứng" ở §4.1), và là quy tắc mới cho mọi Change Request sau: thay đổi chạm tới cấu hình hạ tầng, mã lỗi, ràng buộc cơ sở dữ liệu hoặc API của framework phải được kiểm chứng trên môi trường thật trước khi duyệt.

### 4.4 CR-2026-03 — Yêu cầu dữ liệu mẫu của giảng viên (đã duyệt, áp dụng trong SRS v1.2.1)

Khi đánh số lại 8 buổi, giảng viên giao cho Buổi 2 năm yêu cầu nền tảng. Bốn yêu cầu đầu đã được Buổi 2 làm xong; **yêu cầu thứ 5 là yêu cầu mới, khác với SRS v1.2.0** (§2.6.1 chỉ ghi *"Bogus với 50 recipe mẫu và 5 tác giả mẫu"*). Theo nguyên tắc *"SRS là nguồn sự thật"*, yêu cầu này được đưa qua Change Request **CR-2026-03** và ghi vào **SRS v1.2.1** trước khi code, không âm thầm làm khác SRS.

| # | Yêu cầu của giảng viên cho Buổi 2 | Trạng thái | Nơi thực hiện |
|---|---|---|---|
| 1 | Tạo cấu trúc dự án backend theo Clean Architecture | Đã hoàn thành (11/09) | Commit bootstrap `b058654` — 4 tầng + 4 project kiểm thử, kiểm bằng `LayerDependencyTests` |
| 2 | Cài đặt các gói thư viện cần thiết | Đã hoàn thành (11/09) | `*.csproj`, `Directory.Build.props`; danh sách và phiên bản ở `CONG_NGHE_VA_PHIEN_BAN.md` |
| 3 | Cài đặt các lớp entities, configuration, DbContext | Đã hoàn thành (11/09) | `Domain/Entities`, `Infrastructure/Persistence/Configurations`, `CulinaryBlogDbContext` |
| 4 | Tạo migration, cài đặt các lớp để tạo dữ liệu ngẫu nhiên | Đã hoàn thành (11/09) | Migration `B1_InitialSchema`, `DatabaseSeeder` (Bogus) |
| **5** | **CSDL có dữ liệu ngẫu nhiên cho ít nhất 20 categories, 100 recipes; mỗi recipe có ít nhất 10 nguyên liệu và ít nhất 5 bước chế biến** | **Yêu cầu bổ sung — đã hoàn thành (21/09)** | CR-2026-03, SRS v1.2.1 §2.6.1; Buổi 2 — mục "Dev 4 – Yêu cầu bổ sung" |

| Hạng mục | SRS v1.2.0 (cũ) | SRS v1.2.1 (CR-2026-03) |
|---|---|---|
| Danh mục mẫu | ~8 (kế hoạch Buổi 2) | **≥ 20** |
| Công thức mẫu | 50 | **≥ 100** |
| Nguyên liệu / công thức | không quy định (seed cũ: 4–9, chọn ngẫu nhiên từ danh sách chung) | **≥ 10**, đúng với món ăn |
| Bước chế biến / công thức | không quy định (seed cũ: 3–6, mô tả Lorem ipsum) | **≥ 5**, mô tả thật từng bước |
| Tác giả mẫu | 5 | 5 (không đổi) |

- **Tại sao không "sinh cho đủ số" bằng Bogus:** Bogus chọn ngẫu nhiên từ một danh sách nguyên liệu chung sẽ cho ra "Sinh tố bơ" có nước mắm và "Phở bò" có 0 phút nấu. Dữ liệu mẫu là thứ FE, kiểm thử tìm kiếm (Buổi 4), đo hiệu năng (Buổi 5) và buổi bảo vệ đều nhìn vào, nên dữ liệu sai sẽ làm các buổi sau kiểm chứng trên dữ liệu vô nghĩa. Vì vậy **nội dung món ăn** (tên, mô tả, nguyên liệu, định lượng, các bước, thời gian) là **công thức thật viết tay** trong `Seed/Data/*.json`; **Bogus chỉ sinh phần thật sự ngẫu nhiên** (tác giả, trạng thái ~85% Published, ngày xuất bản, dinh dưỡng theo khoảng hợp lý từng nhóm món), với seed cố định để dữ liệu giống nhau trên mọi máy.
- **Tại sao bộ sinh dữ liệu mẫu (seeder) tự bù thay vì bắt cả nhóm xóa volume:** cơ sở dữ liệu của mọi thành viên đang có 50 công thức cũ, và bộ sinh dữ liệu mẫu cũ bỏ qua bước tạo công thức khi bảng đã có dữ liệu. Bộ sinh dữ liệu mẫu mới **idempotent và tự bù**: thêm danh mục/công thức còn thiếu; công thức mẫu cũ — do tác giả mẫu tạo, **chưa từng bị ai sửa** (`UpdatedAt` null) và chưa đạt ngưỡng — được thay nội dung bằng bản đúng trong catalog. Dữ liệu người dùng tạo hoặc đã chỉnh sửa không bị động tới.
- **Ảnh hưởng tới kế hoạch tổng thể:** không đổi schema, không đổi API, không đổi phân công. Các con số "50 công thức" ở Buổi 4 (backfill `SearchVector`) và Buổi 5 (`EXPLAIN ANALYZE`) được cập nhật thành 100; kết luận của Buổi 5 giữ nguyên vì 100 bản ghi vẫn quá nhỏ để PostgreSQL chọn Index Scan (`PerformanceSeeder` ≥ 10.000 bản ghi vẫn cần).

### 4.5 Yêu cầu bổ sung Buổi 3 & Buổi 4 (29/09/2026) — đính chính kiến trúc E-9 và CR-2026-04 (đã áp dụng trong SRS v1.2.2)

Giảng viên giao cho Buổi 3 bốn yêu cầu:
1. cài đặt các lớp domain exceptions;
2. cài đặt các lớp repository & Unit of Work;
3. middleware bắt lỗi toàn cục trả Problem Details;
4. ít nhất 2 API/thành viên.

Kế hoạch trước ngày 29/09 chỉ đáp ứng yêu cầu 3 (middleware có từ Buổi 2) và đáp ứng yêu cầu 4 ở 3/4 dev (Dev 4 không có API). Buổi 3 được viết lại để chia cả bốn yêu cầu cho 4 dev theo module — chi tiết và giải trình ở **mục Buổi 3**.

Cùng ngày, giảng viên giao cho Buổi 4 yêu cầu **"hoàn thành việc cài đặt tất cả API endpoints"**. Kế hoạch cũ không đạt: hết Buổi 4 chỉ có 29/44 endpoint, và 3 endpoint cũ còn sai hợp đồng. Buổi 4 được viết lại thành buổi "API-first"; giao diện của các FR đó dời sang Buổi 5 → 7 — xem **mục Buổi 4**.

Hai việc viết lại này chạm tới SRS ở các điểm dưới đây. Trưởng nhóm đã quyết định áp dụng ngày 29/09/2026 → **SRS v1.2.2**.

| # | Nội dung | Trạng thái | Ảnh hưởng |
|---|---|---|---|
| **E-9** | SRS §6.2 đặt `IRepository<T>`, `IRecipeRepository`, `ICategoryRepository` ở **Domain**; nhóm quyết định đặt **mọi interface truy cập dữ liệu ở Application** (cùng chỗ với `IUnitOfWork` và các read repository đã có từ Buổi 2). Domain chỉ còn entity, value object, enum, exception. | **Đã áp dụng** — SRS v1.2.2 §6.2 (**MT-63**); §2.1 của kế hoạch đã cập nhật | Không đổi API, không đổi schema. Giải trình: Buổi 3 — Dev 4 Phần 4 |
| **CR-2026-04 (a)** | Thêm mã **`AUTH_USER_NOT_FOUND` (404, Auth)** vào Phụ lục B — FR-AUTH-008 A3 và Chương 8.1 (`PATCH /users/{id}/status`) có trả 404 nhưng **không có mã** cho trường `type` | **Đã áp dụng** — SRS v1.2.2 Phụ lục B (**MT-59**) | `UserNotFoundException` (Dev 1, B3) — ném từ B4 (`PATCH /users/{id}/status`) |
| **CR-2026-04 (b)** | Thêm mã **`CONCURRENCY_CONFLICT` (409, Common)** vào Phụ lục B — mọi `BaseEntity` đều có `RowVersion` là concurrency token (§2.2), nhưng Phụ lục B chỉ có `RECIPE_CONCURRENCY_CONFLICT`; xung đột trên `Category` hiện không có mã nào để trả | **Đã áp dụng** — SRS v1.2.2 Phụ lục B (**MT-60**) | `ConcurrencyConflictException` (Dev 4, B3) |
| **CR-2026-04 (c)** | Làm rõ hai chỗ SRS tự mâu thuẫn mà code Buổi 3 phải chọn một: `AUTH_GOOGLE_TOKEN_INVALID` là 400 (Phụ lục B) hay 401 (FR-AUTH-003 A1) → **cả hai, theo tình huống**; `categoryId` không tồn tại là 404 (Chương 8.3) hay 400 (FR-RCP-003 A2) → **400** | **Đã áp dụng** — SRS v1.2.2 FR-AUTH-003, Phụ lục B, §8.3 (**MT-61**) | Không đổi code |
| **CR-2026-04 (d)** | Thêm endpoint **`GET /recipes/mine/{id}`** (Owner/Admin, mọi trạng thái, `no-store`, trả `rowVersion` + `ETag`). Trang `/dashboard/recipes/[id]/edit` (§5.1) cần đọc trọn một công thức **Draft**, nhưng `GET /recipes/{slug}` chỉ trả Published (MT-34), còn `/recipes/mine` chỉ trả tóm tắt → Author không sửa được bản nháp của chính mình. Chương 8 tăng **44 → 45** endpoint | **Đã áp dụng** — SRS v1.2.2 FR-RCP-011, §8.3 (**MT-62**) | Dev 3, Buổi 4 |

- **Tại sao đi qua CR thay vì tự thêm mã:** Phụ lục B tồn tại để Frontend xử lý lỗi theo mã mà không phụ thuộc câu chữ. Một mã chỉ có trong code mà không có trong SRS là đúng loại lệch mà MT-46 vừa phải sửa ở v1.2.0 — không lặp lại.
- **Vì sao áp dụng ngay thay vì để chờ:**
  - Cả bốn điểm đều là **bổ sung hoặc làm rõ**, không đảo ngược quyết định nào đã có; không đổi schema, không đổi hành vi của endpoint đang chạy.
  - Code Buổi 3–4 cần chúng ngay: hai lớp exception, và trang sửa bản nháp.
  - Để chờ thì hai dev phải viết code theo một mã lỗi "chưa có trong SRS" — đúng loại lệch mà MT-46 đã phải sửa.

### 4.6 Yêu cầu Lab 5 → Lab 8 (12/10/2026) — CR-2026-05 (đã áp dụng trong SRS v1.2.3)

Khi lập kế hoạch cho yêu cầu Lab 5 → Lab 8 (§0.3), nhóm đối chiếu từng yêu cầu với SRS v1.2.2. Kết quả:
- **6 điểm** Lab ghi khác SRS về cách làm (X-6 → X-11, §3) — giải quyết theo SRS;
- **3 chức năng** Lab đòi mà SRS chưa có — đưa qua Change Request **CR-2026-05**, trưởng nhóm duyệt ngày 12/10/2026 → **SRS v1.2.3**;
- **2 lỗi kỹ thuật của chính SRS** lộ ra khi thiết kế Lab 5 (CORS) và Lab 6 (CSP) — kiểm chứng thực nghiệm rồi sửa trong cùng CR (MT-67, MT-68).

| # | Nội dung | Thay đổi trong SRS v1.2.3 | Hiện thực |
|---|---|---|---|
| **MT-64** | Lab 7 Dev 1 yêu cầu **đổi mật khẩu**; SRS không có FR/endpoint | Thêm **FR-AUTH-010** (M) + `POST /auth/change-password`: Bearer, `{ currentPassword, newPassword }` → 204. Sai mật khẩu hiện tại → 400 `VALIDATION_ERROR` ở ô `currentPassword`; tài khoản chưa có mật khẩu (chỉ đăng nhập Google) → 400. Thành công thu hồi mọi phiên **khác** phiên hiện tại (theo claim `sid`) | Buổi 7 — Dev 1 |
| **MT-65** | Lab 7 Dev 1 yêu cầu **quên mật khẩu** (`forgot-password`); thiếu bước đặt lại thì link trong email không làm gì được | Thêm **FR-AUTH-011** (M) + `POST /auth/forgot-password` (luôn **202**) + `POST /auth/reset-password` (204). Mã lỗi mới **`AUTH_PASSWORD_RESET_TOKEN_INVALID`** (400). Token sinh trong worker Hangfire, hiệu lực 30 phút, dùng một lần, không vào log hay kho job (bổ sung NFR-SEC-007). Thêm mã HTTP 202 vào Phụ lục A; thêm route `/auth/forgot-password`, `/auth/reset-password` vào §5.1 | Buổi 7 — Dev 1 |
| **MT-66** | Lab 7 Dev 4 yêu cầu trang `/search` lọc theo **thời gian**; `GET /recipes/search` chỉ nhận `q`, `categoryId`, `difficulty` | Thêm tham số tùy chọn `maxCookTime`, `maxPrepTime`, `minServings` vào `GET /recipes/search` (FR-SRCH-001, §8.3) — cùng `RecipeFilterSpec` với `GET /recipes`. Không thêm endpoint, không đổi sắp xếp (vẫn theo `ts_rank`) | API: Buổi 6 — Dev 3 · Giao diện: Buổi 7 — Dev 4 |
| **MT-67** | NFR-SEC-005 đòi CSP có **nonce**; §5.1 đòi **ISR** cho `/`, `/recipes/[slug]`, `/categories`. Nonce phải mới mỗi lượt xem nên chỉ gắn được khi trang render theo request | Giữ CSP nonce + `'strict-dynamic'` (sinh ở `middleware.ts`); ba trang chuyển sang **SSR + Data Cache** với đúng `revalidate` cũ; cấm `'unsafe-inline'` cho script. Bằng chứng ở dưới bảng | CSP: Buổi 6 — Dev 1 · trang: Buổi 7 — Dev 2, Dev 3 |
| **MT-68** | §5.2 `Access-Control-Allow-Headers` thiếu `If-Match` mà §8.3 dùng; không có `Access-Control-Expose-Headers` nên `ETag`, `Retry-After`, `X-RateLimit-*` bị trình duyệt ẩn khỏi JavaScript khi gọi khác origin | Bổ sung `If-Match`, danh sách header expose, `Access-Control-Max-Age: 600`, không `Allow-Credentials` | Buổi 5 — Dev 1 |

**Tổng sau CR-2026-05:** **39 FR** (FR-AUTH 9 → 11) · **48 endpoint** (45 → 48) · **30 mã lỗi** (29 → 30) · **68 mục MT**.

**Bằng chứng cho MT-67 (12/10/2026)** — app Next.js 15.5.25 tối giản (một trang `revalidate = 120` có Client Component đổi chữ sau khi hydrate), chạy `next build && next start`, đo bằng trình duyệt Edge headless qua Playwright:

| Phương án | Kiểu route sau build | Script inline (dữ liệu RSC) | Vi phạm CSP | Hydrate | Data Cache của `fetch` |
|---|---|---|---|---|---|
| **A.** `experimental.sri` + `script-src 'self'` (cách tài liệu Next.js giới thiệu để giữ trang tĩnh) | ○ tĩnh / ISR | 6 thẻ, **không** có `integrity` (SRI chỉ gắn cho file script ngoài) | **6** — trình duyệt chặn mọi `self.__next_f.push(...)` | **Không** — trang không nhận được dữ liệu RSC | — |
| **B.** nonce + `'strict-dynamic'` sinh ở `middleware.ts` | ƒ động | 4 thẻ, **cả 4** mang nonce | **0** | **Có** | **Giữ** — tải trang 3 lần, số đếm của API đứng yên; gọi thẳng API thì số đếm tăng |
| C. `'unsafe-inline'` cho script | ○ tĩnh / ISR | — | 0 | Có | — |

Chọn **B**: đúng chữ NFR-SEC-005, và yêu cầu thật sự của ISR (dữ liệu không cũ hơn TTL, không tạo thêm tải cho API) vẫn đạt nhờ Data Cache — cùng lý lẽ MT-57. **A bị loại** vì không chạy. **C bị loại** vì CSP cho mọi script inline chạy thì không chặn được XSS — trong khi lý do tồn tại của CSP trong hệ thống này là bảo vệ refresh token nằm ở nơi JavaScript đọc được. Cái giá chấp nhận: máy chủ Next.js render HTML ở mỗi request (không thêm lượt gọi API); HTML trả `Cache-Control: private, no-store` nên Nginx không cache HTML (ảnh và `_next/static` vẫn cache như Buổi 7).

- **Tại sao thêm `reset-password` dù Lab chỉ ghi `forgot-password`:** quên mật khẩu là luồng hai bước — gửi email chứa link, rồi trang đích đặt mật khẩu mới. Chỉ có bước 1 thì người dùng nhận email nhưng không có gì để bấm vào. Làm đúng chữ của Lab là làm một nửa tính năng.
- **Tại sao không đưa "đổi vai trò người dùng" vào CR:** Lab 8 ghi "phân quyền" cho trang quản lý người dùng. Trưởng nhóm quyết định (12/10/2026) hiểu là trang chỉ Admin vào được + hiển thị vai trò + khóa/mở khóa — đúng FR-AUTH-008 hiện có. Đổi vai trò có rủi ro riêng (Admin tự hạ quyền, hệ thống mất Admin cuối cùng) và cần một FR riêng.
- **Tại sao mở rộng `/recipes/search` thay vì cho trang `/search` gọi `GET /recipes`:** `GET /recipes` không có tìm kiếm toàn văn. Ghép kết quả hai endpoint ở client thì không phân trang đúng được. Thêm tham số tùy chọn vào endpoint có sẵn là thay đổi tương thích ngược nhỏ nhất.
- **Vì sao sửa MT-67, MT-68 ngay trong CR này thay vì để "thử ở Buổi 6":** cả hai quyết định ảnh hưởng tới cách ba dev dựng trang ở Buổi 6 – 7 (trang có `export const revalidate` hay không, header nào Frontend đọc được). Để mở tới Buổi 6 nghĩa là Buổi 7 có thể phải làm lại. Thực nghiệm chỉ tốn một app tối giản, nên làm ngay và ghi kết quả là đúng quy tắc đã rút ra ở §4.3: *thay đổi chạm tới cấu hình hạ tầng hoặc API của framework phải được kiểm chứng trên môi trường thật trước khi duyệt*.

---

# BUỔI 1 – ĐỌC ĐẶC TẢ YÊU CẦU HỆ THỐNG & TÌM HIỂU CÁC TÍNH NĂNG CẦN XÂY DỰNG

> **Yêu cầu của giảng viên:** đọc đặc tả yêu cầu hệ thống và tìm hiểu các tính năng cần xây dựng. Buổi này **không viết code**; sản phẩm là hiểu biết chung của cả nhóm về hệ thống và bảng đầu việc làm đầu vào cho Buổi 2 → Buổi 8.

**Mục tiêu:** cả 4 thành viên nắm được phạm vi sản phẩm, các lớp người dùng, kiến trúc và toàn bộ yêu cầu; mỗi người hiểu sâu phần mình sẽ phụ trách (phân công §1) để từ Buổi 2 có thể làm trọn lát cắt dọc DB → API → UI mà không phải hỏi lại yêu cầu.

| Thành viên | Phần SRS đọc kỹ | Cần trả lời được sau buổi |
|---|---|---|
| **Dev 1 – Hoàng Bình Quân** | Chương 3.1 `FR-AUTH-001 → 009`, §2.3 Các lớp người dùng, `NFR-SEC-001 → 007`, §8.1 API Auth | Luồng đăng ký/đăng nhập/Google/refresh/đăng xuất; vòng đời refresh token; role Author/Admin; các mã lỗi `AUTH_*` |
| **Dev 2 – Nguyễn Hồng Phúc Thọ** | Chương 3.3 `FR-RCP-001 → 011`, Chương 7 Data Model (§7.2 – §7.5), §8.3 – §8.6 API Recipe | Máy trạng thái Draft/Published/Archived; quy tắc nguyên liệu, các bước, ảnh; xóa mềm; `RowVersion` |
| **Dev 3 – Đoàn Hồng Tiến** | Chương 3.2 `FR-CAT-001 → 005`, Chương 3.4 `FR-SRCH-001 → 004`, `NFR-SEO-001 → 004`, §5.1 Route Frontend | Tìm kiếm tiếng Việt không dấu; lọc/sắp xếp/phân trang; ISR/SSR từng trang; SEO |
| **Dev 4 – Nguyễn Thăng Thiêng** | `FR-FILE`, `FR-JOB`, `FR-OBS`, Chương 6 Kiến trúc, `CONS-001 → 010`, `NFR-PERF`, `NFR-REL`, `NFR-SCALE`, §2.6 Giả định (dữ liệu mẫu) | Kiến trúc Clean Architecture + CQRS; 8 service Docker; cache Redis; job nền; health check, log, tracing |

**Các bước:**
1. **Đọc tổng quan (cả nhóm):** Chương 1 – 2 của SRS: mục đích, phạm vi, lớp người dùng, môi trường vận hành, ràng buộc thiết kế, giả định.
2. **Đọc sâu theo phân công:** mỗi người đọc phần của mình ở bảng trên, liệt kê chức năng, endpoint, bảng dữ liệu, mã lỗi liên quan.
3. **Lập bảng đầu việc chung:** gom thành danh sách **37 FR, 30 NFR, 10 CONS, 44 endpoint, 27 mã lỗi** kèm mức ưu tiên MoSCoW — kết quả nằm ở **README Phần 1**.
4. **Ghi lại điểm chưa rõ / mâu thuẫn:** mỗi chỗ SRS tự mâu thuẫn hoặc thiếu được ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` để xử lý qua Change Request, không tự suy diễn khi code.
5. **Chốt công nghệ và phân công:** thống nhất danh sách công nghệ – phiên bản (`CONG_NGHE_VA_PHIEN_BAN.md`), phân công module cố định (§1) và quy trình Git (§2.4).

**Tại sao cần một buổi riêng:** mọi quyết định kiến trúc ở Buổi 2 (schema, pipeline MediatR, cache, topology Docker) đều khó sửa sau khi có dữ liệu. Đọc kỹ SRS trước giúp dựng đúng schema một lần (Buổi 2 dựng trọn schema Recipe dù chưa có API cho Step/Ingredient/Image) và phát hiện mâu thuẫn trên giấy thay vì phát hiện khi đã code.

**Xong khi:** mỗi thành viên trình bày được các chức năng của module mình và các module liên quan; bảng đầu việc (README Phần 1) và danh sách câu hỏi/mâu thuẫn đã được cả nhóm thống nhất.

**Sản phẩm:** README Phần 1 (toàn bộ đầu việc), `SRS_MAU_THUAN_VA_GIAI_PHAP.md`, `CONG_NGHE_VA_PHIEN_BAN.md`, bảng phân công §1.

---

# BUỔI 2 – NỀN MÓNG & LÁT CẮT ĐẦU TIÊN

> **ĐÃ HOÀN THÀNH & MERGE VÀO `main`.** Toàn bộ nội dung Phần 1, Phần 2 và Commit dưới đây **giữ nguyên 100%** so với kế hoạch đã chốt. Phần 3 (Định hướng) và Phần 4 (Ghi chú mâu thuẫn) là **phần giải trình bổ sung**, viết dựa trên **code thực tế** và `BAO_CAO_BUOI_2.md` — không làm thay đổi bất kỳ công việc nào đã thực hiện.
>
> **Khác biệt giữa kế hoạch và thực tế Buổi 2** (chi tiết ở `BAO_CAO_BUOI_2.md` §4): làm sớm `FR-JOB-001` Welcome Email; một migration chung `B1_InitialSchema` thay vì 4 migration riêng; endpoint file thực tế là `POST /api/v1/files/upload` và `DELETE /api/v1/files/{**fileId}`; JWT lưu `localStorage`; ảnh chạy `next/image unoptimized`; trang danh mục render động (không phải ISR thuần). Các điểm lệch chuẩn đã vào bảng nợ §4.1.

> **Năm yêu cầu của giảng viên cho Buổi 2** (bảng đầy đủ ở §4.4): (1) cấu trúc Clean Architecture, (2) cài gói thư viện, (3) entities – configuration – DbContext, (4) migration + lớp sinh dữ liệu ngẫu nhiên đều nằm trong commit bootstrap và 4 lát cắt dưới đây; **(5) dữ liệu mẫu ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước là yêu cầu bổ sung, khác SRS v1.2.0** — thực hiện ở mục "Dev 4 – Yêu cầu bổ sung" cuối buổi.

> **30 phút đầu (Dev 4 dẫn):** push commit `chore: bootstrap clean architecture solution & nextjs app` gồm solution 4 tầng, `BaseEntity`, `CulinaryBlogDbContext`, `AuditInterceptor`, 4 Pipeline Behaviors, `GlobalExceptionMiddleware`, `PagedResult<T>`, Next.js skeleton + `api-client`. 3 dev còn lại branch từ commit này.

## DEV 1 – Auth

**Phần 1 – Chức năng hoàn thành:** `FR-AUTH-001` Đăng ký tài khoản + `FR-AUTH-002` Đăng nhập Email/Mật khẩu.

**Phần 2 – Các bước:**
1. **DB/Entity:** `ApplicationUser : IdentityUser` (DisplayName/FullName varchar(100), AvatarUrl, Bio, IsActive, CreatedAt), entity `RefreshToken` (TokenHash varchar(64) UNIQUE, ExpiresAt, RevokedAt, ReplacedByTokenHash, CreatedByIp); cấu hình Identity: PBKDF2 (Identity mặc định, ≥100k iterations), password ≥8 (hoa+thường+số+đặc biệt), Lockout 5 lần/15 phút; seed role `Author`, `Admin` + 1 admin → migration `B2_Auth_Identity`.
2. **Service:** `IJwtService`/`JwtService` – access token HS256 15 phút (claims userId, email, roles, jti), refresh token 512-bit random + SHA-256 hash, TTL 7 ngày; `IRefreshTokenRepository`.
3. **CQRS:** `RegisterCommand` + `RegisterCommandValidator` + Handler (FindByEmail → 409 `AUTH_EMAIL_EXISTS`, CreateAsync, AddToRole "Author", phát token, lưu RefreshToken, gọi `IWelcomeEmailScheduler` – stub no-op, Dev 4 cắm Hangfire ở Buổi 3); `LoginCommand` + Handler (CheckPassword, IsLockedOut → 423, AccessFailed, 401 generic `AUTH_INVALID_CREDENTIALS`).
4. **API:** `AuthEndpoints.cs` → `POST /api/v1/auth/register` (201 AuthResponseDto), `POST /api/v1/auth/login` (200); cấu hình `AddAuthentication().AddJwtBearer()`.
5. **UI:** `/auth/register`, `/auth/login` (CSR) – RHF + Zod, lỗi inline map từ ProblemDetails; `AuthProvider` lưu access token trong bộ nhớ (in-memory) + refresh token; header hiển thị người dùng đã đăng nhập. Kiểm thử: xUnit cho validator + integration test register/login (happy + 409/401).

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **ASP.NET Core Identity** thay vì tự viết bảng người dùng: Identity đã đóng gói sẵn PBKDF2 với iteration count đạt chuẩn, `SecurityStamp` (để thu hồi quyền tức thời), khóa tài khoản tạm thời chống brute-force và toàn bộ hạ tầng role/claim — tự viết lại chỉ để "gọn hơn" là đánh đổi bảo mật lấy vài trăm dòng code, không đáng.

Quyết định **lưu SHA-256 hash của refresh token** (không lưu raw) ngay từ buổi đầu là có chủ đích: nếu để sau mới sửa thì phải viết migration vá và vô hiệu hóa toàn bộ token đang lưu hành. Hệ quả thực tế của việc lưu raw là kẻ đọc được DB (SQL injection, backup rò rỉ) mạo danh được **mọi** người dùng cho tới khi token hết hạn.

Refresh token đi trong **body request** (không cookie) để tránh CSRF mà không cần thêm cơ chế anti-forgery (SRS §5.2). Ở Buổi 2, cả access token lẫn refresh token được lưu `localStorage` — một lựa chọn **có ý thức và tạm thời**: khi chưa có luồng refresh (FR-AUTH-004), đây là cách duy nhất giữ phiên qua lần tải lại trang. Nó được ghi thành nợ **D-12** và trả ở Buổi 4, khi luồng refresh cho phép đưa access token về bộ nhớ.

`IWelcomeEmailScheduler` được để **stub no-op** thay vì chờ Dev 4 dựng xong Hangfire: đây là kỹ thuật *seam* — Dev 1 hoàn thành lát cắt dọc của mình ngay trong buổi, Dev 4 chỉ cần thay implementation ở Buổi 3 mà không đụng vào handler đăng ký. Không có seam này, hai dev sẽ chặn nhau.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 ghi độ dài refresh token **512-bit** ở FR-AUTH-001 nhưng **128-bit** ở NFR-SEC-002 — lệch nhau 4 lần. FR-AUTH dùng trường `fullName` + `userName` (người dùng tự nhập), trong khi Chương 7 chỉ có cột `DisplayName` và Chương 8 chỉ có `displayName`.
- **Hướng đi chọn lựa:** Buổi 2 đã code theo **512-bit** (`RefreshTokenBytes = 64`); cột DB đúng là `DisplayName` nhưng **API contract** vẫn nhận `fullName` + `userName`. SRS v1.1.0 (MT-13, MT-12) chốt: **256-bit**; body đăng ký `{ email, password, displayName }`; **BE tự sinh `UserName`** từ tiền tố email.
- **Tại sao chọn:** 256-bit khớp đúng độ dài output SHA-256 và `TokenHash varchar(64)`; entropy vượt 256-bit không tăng thực chất vì đằng nào cũng bị hash, chỉ tốn băng thông mỗi lần refresh. Bắt người dùng tự nghĩ `userName` vừa thêm một ô nhập vô nghĩa (hệ thống không hiển thị nó ở đâu), vừa sinh thêm một loại lỗi trùng (`AUTH_USERNAME_EXISTS`) mà người dùng không hiểu vì sao. **Retrofit D-1 (Buổi 3), D-2 và D-12 (Buổi 4).**

**Phần 5 – Commit:** `feat(auth): complete FR-AUTH-001 & FR-AUTH-002 register and local login flow`

## DEV 2 – Recipe lõi

**Phần 1 – Chức năng hoàn thành:** `FR-RCP-001` Xem danh sách công thức (phân trang) + `FR-RCP-002` Xem chi tiết công thức.

**Phần 2 – Các bước:**
1. **DB/Entity:** `Recipe` (Title 200, Slug 220 UNIQUE, Description, Instructions, PrepTime, CookTime, Servings, `RecipeDifficulty` 1–4, `RecipeStatus` 0–2, CategoryId FK RESTRICT, AuthorId FK, PublishedAt), owned `RecipeNutrition` (tiền tố `Nutrition_`, decimal(8,2)?), `RecipeStep`, `RecipeIngredient`, `RecipeImage` (cascade); index Slug/Status/CategoryId/AuthorId/PublishedAt/Difficulty → migration `B2_Recipe_Schema`.
2. **Seed:** `RecipeSeeder` dùng **Bogus** – 50 recipe mẫu, 5 tác giả (theo SRS §2.6.1), đủ bước/nguyên liệu/dinh dưỡng để FE có dữ liệu thật. *(Sau này nâng lên 20 danh mục / 100 công thức theo CR-2026-03 — mục "Dev 4 – Yêu cầu bổ sung".)*
3. **CQRS:** `GetRecipesQuery` (page ≥1, pageSize 1–50, lọc quyền: Guest→Published, Author→Published + Draft của mình, Admin→tất cả; `.AsNoTracking()` + projection sang `RecipeSummaryDto`, không N+1); `GetRecipeBySlugQuery` (Include Steps/Ingredients/Images/Category/Author, 404 `RECIPE_NOT_FOUND`, Draft/Archived → 403 nếu không phải owner/Admin).
4. **API:** `RecipesEndpoints.cs` → `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}`; Output Cache policy `RecipeList` (15 phút, vary query) và `RecipeDetail` (60 phút, tag `recipes`, `recipe:{slug}`).
5. **UI:** `/recipes` (SSR) lưới `RecipeCard` + phân trang + loading skeleton; `/recipes/[slug]` (ISR revalidate=300) hiển thị ảnh, nguyên liệu, các bước, bảng dinh dưỡng, tác giả.

**Phần 3 – Định hướng & Lý do thiết kế:**
Dựng **toàn bộ schema Recipe ngay Buổi 2** (kể cả Step/Ingredient/Image/Nutrition dù chưa có API cho chúng) là quyết định có tính toán: schema là thứ đắt nhất để sửa sau khi đã có dữ liệu. Có đủ bảng từ đầu thì các buổi sau chỉ thêm API/UI, không phải viết migration vá.

`RecipeNutrition` là **Owned Entity** (cột `Nutrition_*` nhúng thẳng vào bảng `Recipes`) chứ không phải bảng riêng: dinh dưỡng không có vòng đời độc lập với công thức — không ai tra cứu "bảng dinh dưỡng" tách rời khỏi món ăn. Mô hình hóa đúng bản chất này giúp tránh một JOIN ở mọi truy vấn chi tiết và loại bỏ nhu cầu tạo repository/DTO/endpoint thừa.

Dữ liệu mẫu **50 recipe bằng Bogus** ngay buổi đầu để FE có dữ liệu thật mà dựng lưới, phân trang, skeleton — không phải chờ Dev khác tạo nội dung thủ công. Đây cũng là bộ dữ liệu để Dev 3 kiểm thử Full-Text Search ở Buổi 4.

`.AsNoTracking()` + **projection thẳng sang DTO** trong query đọc: EF Core không cần dựng change tracker cho dữ liệu chỉ để hiển thị, và projection khiến SQL chỉ `SELECT` đúng cột cần — chặn N+1 từ gốc thay vì đi vá bằng `.Include()` sau khi phát hiện chậm.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** FR-RCP-001 bước 4 quy định lọc kết quả **theo danh tính** người gọi (Guest thấy Published, Author thấy thêm Draft của mình, Admin thấy tất cả), nhưng bước 9 — cách đó 5 dòng trong **cùng một FR** — lại quy định Output Cache lưu response theo khóa chỉ gồm `{path}?{queryString}`.
- **Hướng đi chọn lựa:** Buổi 2 đã code theo đúng v1.0.0 — báo cáo Buổi 2 ghi nhận Guest/Author/Admin thấy lần lượt 46/47/50 bản ghi **từ cùng một URL**, và Draft trả **403** cho người lạ. Output Cache được đặt trên Redis (`AddStackExchangeRedisOutputCache`) nên không bị lệch giữa các instance, nhưng **vẫn không phân biệt danh tính** — tức lỗ hổng vẫn còn nguyên. SRS v1.1.0 (**MT-34**) đảo ngược: endpoint công khai **chỉ trả `Published` cho mọi người gọi, không ngoại lệ kể cả Admin**; Draft/Archived trả **404** (không tiết lộ bản nháp tồn tại) và chỉ xem được qua `GET /recipes/mine` (`FR-RCP-011`), **cấm cache**.
- **Tại sao chọn:** đây là **lỗi bảo mật nghiêm trọng nhất toàn tài liệu**, không phải khác biệt phong cách. Kịch bản cụ thể: Admin mở `GET /api/v1/recipes?page=1` → response chứa **toàn bộ Draft của mọi Author** được nạp vào cache dưới khóa công khai → trong 15 phút tiếp theo **mọi Guest** gọi đúng URL đó đều đọc được. Với `/recipes/{slug}` còn tệ hơn: Draft bị cache 60 phút và kiểm tra quyền ở bước 5 trở nên vô nghĩa vì request thứ hai **không bao giờ chạy tới handler**. Phương án thêm `userId` vào khóa cache bị loại vì hit rate sụp đổ và chỉ cần quên `VaryBy` ở một endpoint là lỗ hổng quay lại — tách bạch về mặt kiến trúc mới là giải pháp dài hạn. **Retrofit D-4, D-6 (Buổi 6).**

**Phần 5 – Commit:** `feat(recipes): complete FR-RCP-001 & FR-RCP-002 recipe listing and detail page`

## DEV 3 – Category

**Phần 1 – Chức năng hoàn thành:** `FR-CAT-001` Xem danh sách danh mục (cache) + `FR-CAT-002` Xem chi tiết danh mục kèm công thức.

**Phần 2 – Các bước:**
1. **DB/Entity:** `Category` (Name varchar(100) UNIQUE, Slug varchar(120) UNIQUE, Description, ImageUrl, OrderIndex) + Value Object `Slug` và `SlugHelper.Generate()` (lowercase, bỏ dấu tiếng Việt, đ→d, space→"-") + unit test slug; seed ~8 danh mục → migration `B2_Category_Schema`.
2. **Cache:** `ICacheService`/`RedisCacheService` (StackExchange.Redis, try/catch phương án dự phòng – fallback DB khi Redis down); `CachingBehavior` dùng `ICacheable { CacheKey, Expiration }`.
3. **CQRS:** `GetCategoriesQuery : ICacheable` (key `categories:all`, TTL 30 phút, sắp xếp Name ASC, `recipeCount` chỉ đếm Published); `GetCategoryBySlugQuery` (404 `CATEGORY_NOT_FOUND`, recipes Published + Draft của currentUser, OFFSET pagination → `PagedResult<RecipeSummaryDto>`).
4. **API:** `CategoriesEndpoints.cs` → `GET /api/v1/categories`, `GET /api/v1/categories/{slug}?page&pageSize`.
5. **UI:** `/categories` (ISR 3600) lưới `CategoryCard`; `/categories/[slug]` (ISR 600) header danh mục + list recipe phân trang; component `CategoryNav` cho header site.

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **Redis `IDistributedCache` ngay từ đầu** thay vì `IMemoryCache` (dù v1.0.0 Chương 3 ghi `IMemoryCache`): với `IMemoryCache`, khi chạy nhiều API instance sau Nginx thì Admin sửa danh mục trên instance 1 mà instance 2 vẫn phục vụ dữ liệu cũ tới 60 phút, **và không có cách nào xóa cache**. Danh mục chỉ ≤ 50 bản ghi nên chi phí lượt gọi qua lại (round-trip) Redis (~1ms) không đáng kể so với rủi ro đó.

`RedisCacheService` bọc **try/catch phương án dự phòng về DB**: Redis là tầng tăng tốc, không phải tầng bắt buộc. Redis chết thì hệ thống chậm đi chứ không được sập (NFR-REL-002).

`SlugHelper` đặt ở Buổi 2 dù Category chưa cần sinh slug động, vì **Dev 2 sẽ dùng lại nó ở Buổi 3** để sinh slug cho Recipe. Viết một lần, có unit test riêng cho tiếng Việt (`Phở bò` → `pho-bo`, `Bánh mì` → `banh-mi`, `đ` → `d`) là rẻ hơn nhiều so với hai dev tự viết hai hàm bỏ dấu khác nhau.

`recipeCount` **chỉ đếm Published** vì đây là con số hiển thị cho khách: đếm cả Draft sẽ khiến người dùng bấm vào danh mục "12 công thức" rồi chỉ thấy 3 món.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** mục 3.2 và FR-CAT-001/003 của v1.0.0 quy định cache danh mục bằng **`IMemoryCache`**, trong khi NFR-SCALE-001 ghi rõ *"Distributed cache (Redis, **không** in-memory `IMemoryCache`) cho mọi shared state"* và Chương 6 vẽ Cache Layer = Redis. Ngoài ra `GetCategoryBySlugQuery` trả thêm Draft của người gọi — cùng lỗi gốc với MT-34.
- **Hướng đi chọn lựa:** Buổi 2 đã chọn **Redis** (đón đầu đúng hướng, khớp MT-16), nhưng đặt TTL **60 phút** theo Chương 3 cũ thay vì 30 phút của NFR-PERF-003. Riêng phần trả Draft sẽ được gỡ bỏ theo MT-34.
- **Tại sao chọn:** Redis là một cơ chế duy nhất cho toàn hệ thống — hai cơ chế cache song song (`IMemoryCache` cho danh mục, Redis cho phần còn lại) nghĩa là hai chỗ phải nhớ xóa cache, và chỗ bị quên sẽ là chỗ sinh lỗi khó tái hiện nhất. **Retrofit D-13 (Buổi 3), D-5 và D-7 (Buổi 6).**

**Phần 5 – Commit:** `feat(categories): complete FR-CAT-001 & FR-CAT-002 public categories with redis cache`

## DEV 4 – Hạ tầng & File

**Phần 1 – Chức năng hoàn thành:** Docker Compose 8 services + `FR-FILE-001` Tải lên file lên MinIO + `FR-FILE-002` Xóa file khỏi MinIO (kèm Reusable Image Tải lên Component).

**Phần 2 – Các bước:**
1. **DevOps:** `docker-compose.yml` gồm `nginx, api, frontend, postgres:16-alpine, redis:7-alpine (--appendonly yes), minio (console :9001), hangfire (worker), seq` + `mailhog` (profile dev); volumes `pgdata, redisdata, miniodata, seqdata`; `init.sql` bật extension `unaccent`, `pg_trgm`; `.env.example`, User Secrets cho dev (NFR-SEC-007); README "setup < 5 phút".
2. **Application/Infra:** `IFileStorageService { UploadAsync(stream, fileName, contentType, folder, ct), DeleteAsync(url, ct) }`; `MinioFileStorageService` (AWSSDK.S3 + ServiceURL ghi đè, `ForcePathStyle`), bucket `culinary-blog` tự tạo + policy public-read khi khởi động; tên file `{folder}/{Guid}{ext}` chống path traversal; Delete idempotent (object không tồn tại không throw).
3. **Kiểm tra hợp lệ:** `FileValidator` – ≤5MB kiểm tra trước khi đọc stream, danh sách MIME cho phép (whitelist) jpeg/png/webp/avif, **magic bytes** (FF D8 FF, 89 50 4E 47, RIFF…WEBP, ftypavif) → 400 `FILE_SIZE_EXCEEDED` / `FILE_MIME_INVALID`.
4. **API tạm để kiểm thử độc lập:** `POST /api/v1/files` + `DELETE /api/v1/files` (Bearer, dùng lại ở ảnh đại diện Buổi 5); unit test magic bytes.
5. **UI:** `components/ImageUploader.tsx` tái sử dụng – drag & drop, preview, kiểm tra hợp lệ (validate) client (size/MIME), **progress bar %** qua XHR `upload.onprogress` (NFR-USE-004), `onUploaded(url)` callback.

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **`AWSSDK.S3`** thay vì MinIO .NET SDK chính chủ: `IFileStorageService` được thiết kế để swap implementation, và với `AWSSDK.S3` thì việc chuyển sang AWS S3 thật chỉ là bỏ dòng ghi đè `ServiceURL` — trong khi SDK chính chủ khóa chặt hệ thống vào MinIO và phải viết lại toàn bộ implementation khi đổi nhà cung cấp.

**Magic bytes kiểm tra hợp lệ** thay vì tin `Content-Type` header: header do client gửi nên đổi được tùy ý — đổi tên `payload.php` thành `.jpg` và khai `image/jpeg` là qua được mọi kiểm tra dựa trên header. Đọc vài byte đầu file mới biết được định dạng thật.

Tên file **`{folder}/{Guid}{ext}`** (không dùng tên gốc của người dùng) chặn path traversal (`../../etc/passwd`) từ gốc, đồng thời loại luôn vấn đề trùng tên và ký tự lạ trong tên file tiếng Việt.

`ImageUploader` được viết **tái sử dụng ngay từ đầu** vì nó sẽ được dùng ở ít nhất 4 chỗ: ảnh công thức (Dev 2, B3), ảnh bước nấu (Dev 2, B4), ảnh đại diện (Dev 1, B5), ảnh danh mục (Dev 3, B3). Progress bar thật (qua `XHR.upload.onprogress`, không phải spinner giả) là yêu cầu định lượng của NFR-USE-004.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** mục 2.1.2 của v1.0.0 ghi *"HTTP/S3 API + **MinIO .NET SDK**"*, trong khi Chương 5.3, 6.1 và 6.2 đều ghi **`AWSSDK.S3`** (3/4 tài liệu). Ngoài ra "8 services" của compose bao gồm một service `hangfire` riêng, còn SRS §6.5 liệt kê 8 services khác (`nginx, api, frontend, postgres, redis, minio, seq, mailhog`) với Hangfire chạy **in-process trong api**.
- **Hướng đi chọn lựa:** Buổi 2 đã chọn **`AWSSDK.S3`** (khớp MT-18 — đúng hướng) và chạy Hangfire ở **container worker riêng** dùng chung image API (`Hangfire__WorkerOnly=true`). Kế hoạch **giữ topology worker riêng**, và SRS v1.2.0 đã được sửa theo nó (§3.6, §6.5 — **MT-47**) thay vì ép code gộp ngược vào `api`.
- **Tại sao chọn:** worker riêng là thiết kế tốt hơn về dài hạn, không chỉ "hợp lý về vận hành": FR-JOB-002 đổi kích thước (resize) ảnh bằng ImageSharp là tác vụ **nặng CPU**; nếu chạy in-process, mỗi lượt tải ảnh lên sẽ tranh CPU với chính các request đang phục vụ người đọc, kéo p95 vượt 500ms (NFR-PERF-001). Tách worker ra còn cho phép scale hai loại tải độc lập (`--scale api=3` mà không nhân ba số job server). Chi phí đã được trả xong ở Buổi 2: khóa DataProtection dùng chung qua volume `dpkeys`, và worker chờ DB sẵn sàng qua `DatabaseReadiness`. Danh sách service thực tế: `nginx, api, hangfire, frontend, postgres, redis, minio, seq` + `mailhog` (dev) — khớp con số "8 services" của Buổi 2. **D-9 được xử lý bằng cách sửa SRS (MT-47) — không có việc retrofit code.**

**Phần 5 – Commit:** `feat(infra,files): docker compose 8 services & complete FR-FILE-001/002 minio storage with image uploader`

## DEV 4 – Yêu cầu bổ sung: Dữ liệu mẫu theo CR-2026-03 (21/09/2026)

> **Yêu cầu mới, khác SRS v1.2.0:** SRS v1.2.0 §2.6.1 chỉ yêu cầu 50 công thức và 5 tác giả mẫu. Giảng viên yêu cầu CSDL có **ít nhất 20 danh mục, 100 công thức; mỗi công thức ít nhất 10 nguyên liệu và 5 bước chế biến**. Yêu cầu đã được ghi vào SRS v1.2.1 (CR-2026-03, §4.4) trước khi code.

**Phần 1 – Chức năng hoàn thành:** bộ dữ liệu mẫu đạt ngưỡng CR-2026-03 trên mọi cơ sở dữ liệu — cài mới cũng như cơ sở dữ liệu đang có 50 công thức cũ.

**Phần 2 – Các bước:**
1. **Catalog dữ liệu thật:** `Infrastructure/Persistence/Seed/Data/categories.json` (20 danh mục: 8 cũ + Cơm & Xôi, Lẩu, Hải sản, Gỏi & Salad, Món cuốn, Món hấp, Món chiên, Ăn vặt, Món Hàn Quốc, Món Nhật Bản, Món Thái, Món Âu) và 5 file `recipes-*.json` (100 món: 50 món cũ giữ nguyên tên, slug và danh mục + 50 món mới; mỗi món 10 – 14 nguyên liệu có định lượng, đơn vị, ghi chú và 5 – 6 bước có thời gian). Nhúng vào assembly bằng `EmbeddedResource`.
2. **`RecipeSeedCatalog`:** đọc catalog, khai báo ngưỡng `MinCategories = 20`, `MinRecipes = 100`, `MinIngredientsPerRecipe = 10`, `MinStepsPerRecipe = 5`.
3. **`DatabaseSeeder` tự bù, idempotent, trong một transaction (qua execution strategy):** thêm danh mục thiếu (so theo tên và slug, kể cả bản ghi đã xóa mềm để không vi phạm UNIQUE); thêm công thức thiếu theo slug; công thức mẫu cũ của tác giả mẫu, `UpdatedAt` null và dưới ngưỡng → xóa nguyên liệu/bước sinh ngẫu nhiên, nạp lại từ catalog (`StepNumber` đánh lại từ 1). Bogus (seed cố định) chỉ sinh tác giả, trạng thái, ngày xuất bản, dinh dưỡng theo khoảng hợp lý từng nhóm món.
4. **Cache:** khi có danh mục mới, bộ sinh dữ liệu mẫu xóa khóa `categories:all`; khi có dữ liệu thay đổi, `Program.cs` xóa Output Cache tag `recipes` — máy nào đang có cache cũ cũng thấy dữ liệu mới ngay.
5. **Test + kiểm chứng:** `RecipeSeedCatalogTests` (5 kiểm thử: ngưỡng, slug duy nhất, danh mục hợp lệ, không danh mục rỗng, khớp độ dài cột DB). Chạy thật trên Docker cả hai trường hợp: cơ sở dữ liệu cũ 8 danh mục/50 công thức → *"50 recipes created, 50 recipes repaired"*; cơ sở dữ liệu trống → *"100 recipes created"*; khởi động lại → *"0 created, 0 repaired"*. Truy vấn SQL xác nhận 20 danh mục, 100 công thức, tối thiểu 10 nguyên liệu và 5 bước, số bước liên tục, không còn mô tả Lorem ipsum, danh mục nào cũng có công thức.

**Phần 3 – Định hướng & Lý do thiết kế:** xem §4.4 — nội dung món ăn viết tay vì dữ liệu sinh ngẫu nhiên từ danh sách chung không đúng với món; bộ sinh dữ liệu mẫu tự bù để không ai phải xóa volume; chỉ sửa bản ghi mẫu chưa từng bị chỉnh sửa để không đè lên dữ liệu người dùng.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn:** yêu cầu của giảng viên (≥ 100 công thức, ≥ 20 danh mục) khác SRS v1.2.0 §2.6.1 (50 công thức).
- **Hướng đi chọn lựa:** không làm lệch SRS mà cập nhật SRS lên v1.2.1 qua CR-2026-03, rồi mới code.
- **Tại sao chọn:** giữ đúng nguyên tắc mọi việc truy vết được về SRS (§4.2); ai đọc SRS cũng thấy đúng con số đang chạy trong cơ sở dữ liệu.

**Phần 5 – Commit:** `feat(seed): seed 20 categories and 100 real recipes per CR-2026-03`

---

# BUỔI 3 – NỀN TẢNG XỬ LÝ LỖI & TRUY CẬP DỮ LIỆU, ĐĂNG NHẬP GOOGLE / LOGOUT, TẠO DRAFT RECIPE, CRUD CATEGORIES ADMIN, HEALTH CHECKS & HẠ TẦNG KIỂM THỬ

> **Mục tiêu buổi:** hệ thống có đủ đường vào (email + Google + thoát), Author tạo được nội dung nháp kèm ảnh, Admin quản trị được danh mục, trạng thái hệ thống quan sát được qua health check và dashboard job an toàn, **cả nhóm có hạ tầng integration test** để mọi buổi sau viết kiểm thử thật. **Bổ sung 29/09/2026:** cả nhóm dựng xong **ba nền tảng kiến trúc** mà mọi thao tác ghi dữ liệu từ Buổi 3 đến Buổi 8 đều đi qua — **Domain Exceptions**, **Repository & Unit of Work**, **Global Exception Middleware** — và **mỗi dev giao ít nhất 2 API chạy thật**.

## Yêu cầu của giảng viên cho Buổi 3 (bổ sung 29/09/2026) và cách chia cho 4 dev

Giảng viên giao bốn yêu cầu bắt buộc cho Buổi 3. Bản kế hoạch trước (lập trước ngày 29/09) **không đáp ứng được ba trong bốn yêu cầu**: không dev nào có bước dựng domain exceptions (việc thêm mã lỗi cho `DomainException` nằm ở Buổi 4), không dev nào có bước dựng repository/Unit of Work, và Dev 4 không có API nào (dashboard `/hangfire` là trang giao diện, bộ kiểm thử là hạ tầng). Bảng dưới là phân công mới — **mỗi yêu cầu được chia cho cả 4 dev theo module mà dev đó sở hữu suốt 8 buổi (§1)**:

| Yêu cầu | Dev 1 – Auth | Dev 2 – Recipe | Dev 3 – Category | Dev 4 – Hạ tầng |
|---|---|---|---|---|
| **1. Domain Exceptions** (`CulinaryBlog.Domain/Exceptions`) | `AuthDomainException` + 6 lớp cụ thể | `RecipeDomainException` + 5 lớp cụ thể | `CategoryDomainException` + 3 lớp cụ thể | Lớp gốc trừu tượng `DomainException`, `BusinessRuleViolationException`, `ConcurrencyConflictException`; chuyển danh mục mã lỗi `ErrorCodes` về Domain |
| **2. Repository & Unit of Work** (interface ở Application, cài đặt ở Infrastructure) | `IUserRepository` / `UserRepository` | `IRecipeRepository` / `RecipeRepository` + bộ dịch lỗi ghi DB của Recipe | `ICategoryRepository` / `CategoryRepository` + bộ dịch lỗi ghi DB của Category | `IRepository<T>`, `IUnitOfWork`, `EfRepository<T>`, `UnitOfWork` (transaction commit/rollback qua execution strategy, dịch lỗi ghi DB) |
| **3. Global Exception Middleware** (RFC 7807) | `AuthExceptionMappings` | `RecipeExceptionMappings` | `CategoryExceptionMappings` | Nâng cấp `GlobalExceptionMiddleware` + `ExceptionStatusMap` + bộ kiểm thử hợp đồng Problem Details |
| **4. ≥ 2 API chạy thật / dev** | **2** | **4** | **3** | **3** |

**API mới của Buổi 3 — 12 endpoint (yêu cầu tối thiểu 8):**

| Dev | Endpoint | FR | Luồng xử lý (đều chạy trên dữ liệu thật, có integration test trên Testcontainers) |
|---|---|---|---|
| Dev 1 | `POST /api/v1/auth/google` · `POST /api/v1/auth/logout` | FR-AUTH-003, FR-AUTH-005 | Endpoint → MediatR Command → `IGoogleIdTokenValidator` / `IIdentityService` / `IUnitOfWork.Users` → PostgreSQL |
| Dev 2 | `POST /api/v1/recipes` · `POST /api/v1/recipes/{id}/images` · `PATCH` và `DELETE /api/v1/recipes/{id}/images/{imageId}` | FR-RCP-003, FR-RCP-008 | Endpoint → MediatR Command → `IUnitOfWork.Recipes` + `IFileStorageService` → PostgreSQL + MinIO |
| Dev 3 | `POST /api/v1/categories` · `PUT /api/v1/categories/{id}` · `DELETE /api/v1/categories/{id}` | FR-CAT-003, 004, 005 | Endpoint → MediatR Command → `IUnitOfWork.Categories` → PostgreSQL (+ xóa cache Redis qua `CacheInvalidationBehavior`) |
| Dev 4 | `GET /health` · `GET /health/live` · `GET /health/ready` | FR-OBS-001 *(phần endpoint — kéo từ Buổi 5 lên)* | Endpoint → `HealthCheckService` → `IHealthCheck` → PostgreSQL + Redis + MinIO |

> **Ngoại lệ có chủ đích — health check không đi qua MediatR:** SRS FR-OBS-001 quy định kỹ thuật là `IHealthCheck` + `AspNetCore.HealthChecks.*`, không phải Command/Query. Health check là thứ Docker gọi mỗi 10 giây để quyết định có chuyển traffic vào instance hay không, nên nó phải **phụ thuộc càng ít thành phần càng tốt**: bọc qua MediatR thì `LoggingBehavior` sẽ ghi một dòng log mỗi 10 giây cho mỗi container, và một lỗi trong pipeline (ví dụ `CachingBehavior` khi Redis chết) có thể làm chính endpoint báo "Redis chết" không trả lời được. Ba endpoint này vẫn là API thật: truy vấn thật tới PostgreSQL, Redis, MinIO và trả JSON theo SRS §8.7.
>
> **`POST /files/upload` không được tính là API của Buổi 3:** endpoint này đã làm xong và merge ở Buổi 2 (FR-FILE-001). Tính lại nó để đủ chỉ tiêu là báo cáo sai khối lượng công việc.

**Điều chỉnh so với bản đề xuất phân công ngày 29/09 để bám SRS v1.2.2** — kế hoạch giữ nguyên nguyên tắc ở §3: *mâu thuẫn thì giải quyết theo SRS và ghi rõ lý do*:

| Đề xuất ghi | SRS v1.2.2 / code thực tế | Áp dụng |
|---|---|---|
| `AccountLockedException` → **403** | Phụ lục B: `AUTH_ACCOUNT_LOCKED` = **423**; 403 là `AUTH_ACCOUNT_DISABLED`. Test Buổi 2 đang kiểm tra 423 | Locked → **423** (kèm `lockoutEnd`); thêm `AccountDisabledException` → **403** (luồng Google cần) |
| Google "OAuth PKCE/ID-Token" | MT-11, xung đột X-1 (§3): **chỉ ID Token flow**, PKCE bị loại | Chỉ ID Token flow |
| `GET /health` & `GET /ready` | SRS §8.7: `/health`, `/health/live`, `/health/ready` | Đủ 3 endpoint, đúng đường dẫn SRS |
| `CategoryRepository` tích hợp `IDistributedCache` + FTS helper | Cache chỉ có một cơ chế: `CachingBehavior` + `ICacheable` (§2.2, NFR-PERF-003). FTS thuộc **Recipe** (FR-SRCH-001, Buổi 4); danh mục không có tìm kiếm toàn văn | Repository chỉ làm việc với EF Core; cache giữ ở pipeline MediatR (xem giải trình Dev 3) |
| Dev 2 chỉ `POST /recipes` + `POST /images`; Dev 3 chỉ `POST` + `PUT` | FR-RCP-008 gồm cả PATCH/DELETE ảnh; FR-CAT-005 là mức **M** và là nơi duy nhất sinh `CategoryHasRecipesException` | Giữ đủ endpoint — "≥ 2" là mức sàn, không phải trần. Cắt đi thì FR bị tách đôi qua hai buổi (trái nguyên tắc §0.2) |
| `AppDbContext` | Code đang dùng `CulinaryBlogDbContext` (migration, bộ integration test, bộ sinh dữ liệu mẫu tham chiếu) | Giữ tên cũ — đổi tên chỉ tạo diff lớn, không thêm giá trị |
| "Middleware hoặc .NET 10 `IExceptionHandler`" | SRS §6.2 gọi tên `GlobalExceptionMiddleware`; lớp này đã chạy từ Buổi 2 và đang có kiểm thử | Giữ middleware, nâng cấp bên trong (xem giải trình Dev 4) |
| `IUserRepository` thao tác `AspNetUsers` | `ApplicationUser` là kiểu của ASP.NET Identity nằm ở Infrastructure; mọi thao tác **ghi** người dùng phải qua `UserManager` | `IUserRepository` chỉ **đọc** người dùng + quản lý `RefreshTokens`; ghi người dùng vẫn qua `IIdentityService` (xem giải trình Dev 1) |

## Tiến trình trong buổi

1. **CR-2026-04 (§4.5) — ✅ đã áp dụng trong SRS v1.2.2:** 2 mã lỗi `AUTH_USER_NOT_FOUND` (404) và `CONCURRENCY_CONFLICT` (409) đã có trong Phụ lục B.
2. **Commit nền D-11 (422 → 400)** — ✅ **đã hoàn thành** trong commit `67d29c0` của Dev 4 (`BAO_CAO_KET_QUA_BUOI_3.md` §2). Mọi endpoint mới của buổi này trả lỗi kiểm tra hợp lệ **400**; Frontend dùng `mapProblemDetailsToForm()` trong `lib/api-client.ts`.
3. **Mốc M1 — commit nền kiến trúc của Dev 4 vào `develop` (chậm nhất giữa buổi):** lớp gốc exception, `IRepository<T>`/`IUnitOfWork`/`UnitOfWork`, middleware + `ExceptionStatusMap`. Trong lúc chờ M1, ba dev làm những phần **không phụ thuộc** nền: Dev 1 retrofit D-1 + nút Google phía FE; Dev 2 migration D-8 + validator + biểu mẫu nhiều bước (wizard) FE; Dev 3 validator + trang quản trị FE + D-13.
4. **Sau M1:** Dev 1–3 rebase lên `develop`, viết exception + repository + file ánh xạ của module mình, rồi tới handler và integration test.
   > **Với nhánh đã push code Buổi 3 trước ngày 29/09** (Dev 1, Dev 2, Dev 3 đều đã push — xem nhận xét rà soát trên GitHub): **không làm lại từ đầu**. Rebase lên `develop` sau M1 rồi *chuyển* code đã có sang nền mới:
   > - `ErrorCodes` đổi namespace sang `CulinaryBlog.Domain.Exceptions`, và **không tự thêm mã** — đủ 29 mã đã có;
   > - các chỗ ném `NotFoundException`/`ConflictException`/`LockedException` của module chuyển sang domain exception của module;
   > - repository ghi của module (`IRecipeWriteRepository`, `ICategoryRepository`, `IRefreshTokenRepository`…) chuyển thành interface kế thừa `IRepository<T>` trong `Application/Common/Interfaces/Persistence` và gắn vào `IUnitOfWork`;
   > - xử lý lỗi PostgreSQL trong middleware chuyển sang `IPersistenceExceptionTranslator`;
   > - xóa phần D-11 tự làm nếu trùng commit nền của Dev 4 — lấy bản của `develop` khi giải xung đột.
5. **Merge cuối buổi theo thứ tự chuẩn Dev 4 → Dev 1 → Dev 3 → Dev 2** (§2.4). Hai file dùng chung bị nhiều dev sửa — `IUnitOfWork.cs`, `UnitOfWork.cs` — chỉ được **thêm dòng vào đúng khu vực của module mình** (append-only), nên rebase không có xung đột ngữ nghĩa. `ErrorCodes.cs` được Dev 4 đưa **đủ 29 mã** Phụ lục B vào ngay ở commit nền, nên dev module không phải sửa file này.

## Hợp đồng kiến trúc dùng chung (Dev 4 dựng — cả nhóm tuân thủ từ Buổi 3 đến Buổi 8)

### A. Vị trí từng thành phần theo tầng

```
CulinaryBlog.Domain/                         # chỉ .NET BCL (CONS-001)
  Exceptions/DomainException.cs              ← Dev 4  lớp gốc trừu tượng
  Exceptions/BusinessRuleViolationException.cs, ConcurrencyConflictException.cs ← Dev 4
  Exceptions/ErrorCodes.cs                   ← Dev 4  (chuyển từ Application — danh mục mã Phụ lục B)
  Exceptions/Auth/…        ← Dev 1    Exceptions/Recipes/…  ← Dev 2    Exceptions/Categories/… ← Dev 3
CulinaryBlog.Application/
  Common/Interfaces/Persistence/IRepository.cs, IUnitOfWork.cs      ← Dev 4
  Common/Interfaces/Persistence/IUserRepository.cs                  ← Dev 1
  Common/Interfaces/Persistence/IRecipeRepository.cs                ← Dev 2
  Common/Interfaces/Persistence/ICategoryRepository.cs              ← Dev 3
  Common/Exceptions/AppException.cs (+ BadGatewayException 502)     ← Dev 4
CulinaryBlog.Infrastructure/Persistence/
  UnitOfWork.cs, Repositories/EfRepository.cs, IPersistenceExceptionTranslator.cs ← Dev 4
  Repositories/UserRepository.cs                                    ← Dev 1
  Repositories/RecipeRepository.cs, Translators/RecipePersistenceExceptionTranslator.cs     ← Dev 2
  Repositories/CategoryRepository.cs, Translators/CategoryPersistenceExceptionTranslator.cs ← Dev 3
CulinaryBlog.API/Middleware/
  GlobalExceptionMiddleware.cs, ExceptionMapping/{ExceptionStatusMap, IExceptionStatusMapping, CommonExceptionMappings}.cs ← Dev 4
  ExceptionMapping/AuthExceptionMappings.cs ← Dev 1 · RecipeExceptionMappings.cs ← Dev 2 · CategoryExceptionMappings.cs ← Dev 3
```

Các repository **đọc** đã có từ Buổi 2 (`IRecipeReadRepository`, `ICategoryReadRepository` — trả DTO, `AsNoTracking`) **giữ nguyên** cho phía Query của CQRS. Repository mới của Buổi 3 phục vụ phía **Command**: trả entity được EF theo dõi để Domain thay đổi rồi `SaveChangesAsync()` một lần.

### B. Domain Exceptions

```csharp
namespace CulinaryBlog.Domain.Exceptions;

public abstract class DomainException : Exception
{
    private readonly Dictionary<string, object?> _extensions = [];

    protected DomainException(string code, string message, Exception? innerException = null)
        : base(message, innerException) => Code = code;

    /// <summary>Application Error Code (SRS Phụ lục B) — thành trường "type" của RFC 7807.</summary>
    public string Code { get; }

    /// <summary>Dữ liệu bổ sung cho "extensions" của Problem Details (ví dụ recipeCount, lockoutEnd).</summary>
    public IReadOnlyDictionary<string, object?> Extensions => _extensions;

    protected void AddExtension(string key, object? value) => _extensions[key] = value;
}

// Quy tắc nghiệp vụ chung không đáng một lớp riêng (mặc định 400).
public sealed class BusinessRuleViolationException(string code, string message)
    : DomainException(code, message);

// Xung đột RowVersion của entity không thuộc module nào đăng ký bộ dịch riêng (409 CONCURRENCY_CONFLICT).
public sealed class ConcurrencyConflictException(string entityName, Guid? entityId, Exception innerException)
    : DomainException(ErrorCodes.ConcurrencyConflict, $"{entityName} đã được cập nhật bởi một yêu cầu khác. Hãy tải lại dữ liệu.", innerException) { … }
```

- **Domain không biết HTTP:** exception chỉ mang `Code` (chuỗi Phụ lục B) và dữ liệu bổ sung; mã HTTP do tầng API quyết định (mục D).
- **Mỗi module có một lớp gốc trừu tượng** (`AuthDomainException`, `RecipeDomainException`, `CategoryDomainException`) để `catch` được theo module khi cần, và để kiểm thử kiến trúc kiểm được "exception của module nào nằm đúng thư mục module đó".
- **Ba chỗ đang `throw new DomainException(...)` từ Buổi 2** (`Recipe.cs:69`, `Recipe.cs:123`, `RecipeStep.cs:28`) sẽ gãy biên dịch khi lớp gốc thành `abstract` — Dev 4 đổi chúng sang `BusinessRuleViolationException` **trong cùng commit nền** (mã `VALIDATION_ERROR`, riêng `Recipe.Publish` dùng `RECIPE_PUBLISH_INCOMPLETE`) để build luôn xanh; `RecipeTests` đổi `Assert.Throws<DomainException>` sang lớp cụ thể (xUnit so khớp đúng kiểu, không nhận lớp con).

### C. Repository & Unit of Work

```csharp
namespace CulinaryBlog.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    // Cố ý KHÔNG có Remove/Delete và KHÔNG trả IQueryable — xem giải trình Dev 4.
}

public interface IUnitOfWork
{
    IUserRepository Users { get; }            // Dev 1
    ICategoryRepository Categories { get; }   // Dev 3
    IRecipeRepository Recipes { get; }        // Dev 2

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Chạy nhiều lần SaveChanges trong MỘT transaction. operation có thể bị chạy lại (retry)
    /// nên phải tự đọc dữ liệu bên trong nó, không dùng entity đã load từ trước.</summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
```

```csharp
// CulinaryBlog.Infrastructure/Persistence/UnitOfWork.cs
internal sealed class UnitOfWork(CulinaryBlogDbContext db, IEnumerable<IPersistenceExceptionTranslator> translators, …) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            foreach (var translator in translators)
            {
                if (translator.TryTranslate(ex) is { } domainError) throw domainError;   // lỗi nghiệp vụ của module
            }

            if (ex is DbUpdateConcurrencyException concurrency) throw ConcurrencyConflictFrom(concurrency); // 409 chung
            throw;                                                                        // không lường trước → 500
        }
    }

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();   // bắt buộc: DbContext bật EnableRetryOnFailure
        return strategy.ExecuteAsync(async ct =>
        {
            db.ChangeTracker.Clear();                            // mỗi lần thử lại bắt đầu từ trạng thái sạch
            await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
            await operation(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);   // lỗi trước dòng này → DisposeAsync tự rollback
        }, cancellationToken);
    }
}
```

- **Đăng ký DI (Dev 4):** `IUnitOfWork → UnitOfWork` (thay cho việc `CulinaryBlogDbContext` tự đóng vai `IUnitOfWork` như Buổi 2); `IRepository<> → EfRepository<>` dạng open generic; mọi `IPersistenceExceptionTranslator` trong assembly Infrastructure được **quét tự động** — dev module không phải sửa file DI.
- **`IPersistenceExceptionTranslator`** (`DomainException? TryTranslate(DbUpdateException ex)`) là chỗ duy nhất trong hệ thống được đọc chi tiết của PostgreSQL (`PostgresException.SqlState`, `ConstraintName`). Nhờ vậy tầng API không tham chiếu Npgsql.

### D. Global Exception Middleware và ánh xạ mã HTTP

```csharp
// CulinaryBlog.API/Middleware/ExceptionMapping — mỗi module một file, được quét tự động lúc khởi động.
public interface IExceptionStatusMapping { void Configure(ExceptionStatusMap map); }

internal sealed class CategoryExceptionMappings : IExceptionStatusMapping      // ví dụ — file của Dev 3
{
    public void Configure(ExceptionStatusMap map) => map
        .Map<CategoryNotFoundException>(StatusCodes.Status404NotFound)
        .Map<CategoryNameAlreadyExistsException>(StatusCodes.Status409Conflict)
        .Map<CategoryHasRecipesException>(StatusCodes.Status409Conflict);
}
```

`GlobalExceptionMiddleware` đứng **đầu pipeline** (đã có từ Buổi 2) và xử lý theo thứ tự:

| Exception | HTTP | `type` | Ghi chú |
|---|---|---|---|
| `DomainException` (mọi lớp con) | theo `ExceptionStatusMap` (tra theo kiểu, đi ngược lên lớp cha) | `Code` của exception | `Extensions` của exception chép sang `extensions` của Problem Details |
| `ValidationException` (FluentValidation) | 400 | `VALIDATION_ERROR` | `ValidationProblemDetails` với `errors{}` theo tên trường camelCase (D-11) |
| `AppException` (Application) | `StatusCode` của exception | `ErrorCode` | Lỗi phân quyền theo tài nguyên, lỗi tệp, lỗi dịch vụ ngoài (502/503) |
| `BadHttpRequestException` | theo framework | `VALIDATION_ERROR` | Body/tham số sai định dạng |
| Còn lại (kể cả `DbUpdateException` không dịch được) | 500 | `INTERNAL_ERROR` | Log đầy đủ; `detail` chỉ có câu chung + `traceId`; **không lộ stack trace** (NFR-REL-002) |

401 do thiếu/sai token và 403 do policy **không đi qua exception**: 401 do `JwtBearerEvents.OnChallenge` ghi Problem Details (`AUTH_TOKEN_EXPIRED` / `AUTH_TOKEN_INVALID`, có từ Buổi 2); 403 do policy và 404 do sai route do `UseStatusCodePages` + `AddProblemDetails` ghi. Tất cả đều là `application/problem+json`. Hai trường hợp sau giữ `type` là URI chuẩn của RFC vì Phụ lục B không có mã chung cho chúng — Frontend phân biệt bằng `status`.

Ví dụ phản hồi:

```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{ "type": "CATEGORY_DELETE_HAS_RECIPES", "title": "Conflict", "status": 409,
  "detail": "Danh mục còn chứa 7 công thức. Hãy chuyển chúng sang danh mục khác trước khi xóa.",
  "instance": "/api/v1/categories/0b6c…", "recipeCount": 7, "traceId": "00-4bf9…" }
```

### E. Chọn loại lỗi nào — một bảng cho cả nhóm

| Tình huống | Ném gì | Ví dụ |
|---|---|---|
| Dữ liệu đầu vào sai định dạng, độ dài, thiếu trường | Validator FluentValidation → `ValidationException` (400) | `title` < 5 ký tự; `categoryId` không tồn tại (FR-RCP-003 A2) |
| Vi phạm quy tắc nghiệp vụ có mã riêng trong Phụ lục B | Lớp con của `XxxDomainException` của module | `CategoryHasRecipesException`, `AccountLockedException` |
| Quy tắc nghiệp vụ không có mã riêng, không đáng một lớp | `BusinessRuleViolationException(code, message)` | `reorder` thiếu id bước (Buổi 4) |
| Xung đột khi ghi DB (RowVersion, unique) | **Không ném tay** — `UnitOfWork` dịch tự động qua bộ dịch lỗi (translator) | Hai Admin tạo trùng tên cùng lúc |
| Không có quyền trên một tài nguyên cụ thể | `ForbiddenException` (AppException) | `RECIPE_FORBIDDEN`, `FILE_FORBIDDEN` |
| Dịch vụ ngoài / hạ tầng không trả lời | `BadGatewayException` (502), `ServiceUnavailableException` (503) | Google JWKS, MinIO |

Sau khi Dev 1, 2, 3 chuyển các chỗ ném cũ sang domain exception, ba lớp `NotFoundException`, `ConflictException`, `LockedException` trong `AppException.cs` **không còn ai dùng** → dev merge cuối (Dev 2) xóa chúng, để mỗi loại lỗi chỉ có **một** cách biểu diễn.

## DEV 1 — Google Sign-In & Đăng xuất (+ Auth Domain Exceptions & User Repository)

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-AUTH-003` Đăng nhập / Đăng ký bằng Google OAuth 2.0 (**ID Token flow**) + `FR-AUTH-005` Đăng xuất & Thu hồi Refresh Token. **Nền tảng module Auth:** cụm Auth Domain Exceptions, `IUserRepository`/`UserRepository`, ánh xạ lỗi Auth trong middleware. **Kèm retrofit D-1:** chuẩn hóa hợp đồng đăng ký theo `displayName` và BE tự sinh `UserName`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain Exceptions — `Domain/Exceptions/Auth/`:** lớp gốc trừu tượng `AuthDomainException : DomainException` và các lớp cụ thể (mã lỗi lấy từ `ErrorCodes` — đã có đủ từ commit nền của Dev 4):

   | Lớp | Mã (Phụ lục B) | HTTP | Dùng ở |
   |---|---|---|---|
   | `InvalidCredentialsException` | `AUTH_INVALID_CREDENTIALS` | 401 | Đăng nhập (chuyển từ Buổi 2) |
   | `InvalidTokenException` | `AUTH_TOKEN_INVALID`, `AUTH_GOOGLE_TOKEN_INVALID` (factory `GoogleTokenRejected()`); Buổi 4 thêm `RefreshTokenExpired()`, `RefreshTokenRevoked()` | 401 | Google (B3), refresh (B4) |
   | `AccountLockedException(DateTimeOffset? lockoutEnd)` | `AUTH_ACCOUNT_LOCKED` + extension `lockoutEnd` | **423** | Đăng nhập (chuyển từ Buổi 2) |
   | `AccountDisabledException` | `AUTH_ACCOUNT_DISABLED` | 403 | Đăng nhập, Google (B3), refresh (B4) |
   | `EmailAlreadyExistsException` | `AUTH_EMAIL_EXISTS` | 409 | Đăng ký (chuyển từ Buổi 2) |
   | `UserNotFoundException(string userId)` | `AUTH_USER_NOT_FOUND` *(CR-2026-04, MT-59)* | 404 | FR-AUTH-008 (API ở Buổi 4) |

   Chuyển các chỗ ném `UnauthorizedException`/`ForbiddenException`/`LockedException`/`ConflictException` trong `LoginUserCommand`, `RegisterUserCommand` sang các lớp trên; xóa mã `AUTH_USERNAME_EXISTS` (D-1, MT-46).
2. **Ánh xạ HTTP — `API/Middleware/ExceptionMapping/AuthExceptionMappings.cs`:** đăng ký đủ 6 lớp theo cột HTTP ở bảng trên. Kiểm thử kiến trúc của Dev 4 sẽ đỏ nếu thiếu lớp nào.
3. **Repository — `IUserRepository` (Application) / `UserRepository` (Infrastructure):** thay `IRefreshTokenRepository` của Buổi 2 (xóa interface cũ; `AuthResponseFactory` chuyển sang `unitOfWork.Users`). Phạm vi Buổi 3:
   - `AddRefreshTokenAsync(RefreshToken token, …)` — dời từ `RefreshTokenRepository`.
   - `GetRefreshTokenAsync(string userId, string tokenHash, …)` — token **đang được theo dõi** để thu hồi (logout).
   - `GetUserNamesStartingWithAsync(string prefix, …)` — cho `IUserNameGenerator` chọn hậu tố `2, 3…` bằng **một** truy vấn thay vì thử từng tên.
   - Thêm thuộc tính `Users` vào `IUnitOfWork`/`UnitOfWork`.
   - **Ghi người dùng (tạo, liên kết Google, khóa) vẫn qua `IIdentityService` → `UserManager`** — không ghi thẳng `AspNetUsers`.
4. **Domain — `RefreshToken.Revoke(DateTime utcNow, string? replacedByTokenHash = null)`** *(kéo từ Buổi 4 lên vì đăng xuất cần)*: gán `RevokedAt` nếu chưa thu hồi — **gọi lại không đổi gì** (idempotent). Kiểm tra hiệu lực dùng `IsActive(utcNow)` **đã có sẵn** từ Buổi 2.
5. **Retrofit D-1 — `UserNameGenerator` + hợp đồng mới:** `IUserNameGenerator` sinh `UserName` từ phần trước `@` của email (chữ thường, loại ký tự ngoài `IdentityOptions.User.AllowedUserNameCharacters`, thêm hậu tố `2, 3…` nếu trùng — tra bằng `GetUserNamesStartingWithAsync`). Đổi `RegisterRequest`/`RegisterUserCommand` thành `{ email, password, displayName }`; `UserDto` thành `{ id, email, displayName, avatarUrl, bio, roles }`. FE: `RegisterForm` bỏ ô "Tên đăng nhập", đổi "Họ tên" thành "Tên hiển thị"; `AuthProvider` đọc `user.displayName`. **Không cần migration** — cột DB đã là `DisplayName`.
6. **FE — Google Identity Services:** cài `@react-oauth/google`, bọc `<GoogleOAuthProvider>` ở layout; dùng **component `<GoogleLogin onSuccess={({ credential }) => ...}>`** để lấy **ID Token**, POST lên `/api/v1/auth/google`. ⚠️ **Không dùng hook `useGoogleLogin`** — hook đó trả *access token/authorization code*, **không phải ID Token**, và backend sẽ không xác minh được. Không cấu hình redirect URI ở Backend, không dùng PKCE.
7. **CQRS — `GoogleLoginCommand { IdToken }`:** xác minh qua **`IGoogleIdTokenValidator`** (interface ở Application, cài đặt ở Infrastructure gọi `GoogleJsonWebSignature.ValidateAsync(idToken, new ValidationSettings { Audience = [_cfg.GoogleClientId] })` — thư viện tự kiểm **chữ ký, `iss`, `aud`, `exp`**). Bảng lỗi theo FR-AUTH-003:
   - `idToken` rỗng/sai định dạng → validator → **400 `VALIDATION_ERROR`**; token thiếu claim `email` → `BusinessRuleViolationException(AUTH_GOOGLE_TOKEN_INVALID)` → **400**.
   - Chữ ký/`aud`/`exp` sai → `InvalidTokenException.GoogleTokenRejected()` → **401 `AUTH_GOOGLE_TOKEN_INVALID`**.
   - Không lấy được JWKS → `BadGatewayException` → **502 `AUTH_GOOGLE_UNAVAILABLE`**.
   - Liên kết tài khoản: `FindByLoginAsync("Google", payload.Subject)` → có thì đăng nhập; chưa có mà email đã tồn tại thì **chỉ `AddLoginAsync` khi `payload.EmailVerified == true`** (ngược lại → `BusinessRuleViolationException` **400**); hoàn toàn mới thì tạo người dùng với `DisplayName = payload.Name`, `AvatarUrl = payload.Picture`, `UserName` từ `IUserNameGenerator`, role `Author`. `IsActive == false` → `AccountDisabledException` → **403**.
8. **CQRS — `LogoutCommand { RefreshToken }`:** hash SHA-256 chuỗi nhận được → `unitOfWork.Users.GetRefreshTokenAsync(currentUser.UserId, hash)` → `token.Revoke(utcNow)` → `SaveChangesAsync()`. **Idempotent:** không tìm thấy vẫn trả 204 (không tiết lộ token có tồn tại hay không).
9. **API + UI + Kiểm thử:** `POST /api/v1/auth/google` (200 `AuthResponseDto`), `POST /api/v1/auth/logout` (`RequireAuthorization()`, 204). UI: nút Google trên `/auth/login` kèm thông báo dự phòng khi nhận 502 (SRS §2.6.2); menu người dùng → "Đăng xuất" gọi API rồi mới xóa phiên phía client (trả nợ `BAO_CAO_BUOI_2.md` §4.10 — trước đây nút này chỉ xóa token phía client, refresh token vẫn sống 7 ngày). **Integration test** (bộ integration test của Dev 4, `IGoogleIdTokenValidator` thay bằng bản giả trong kiểm thử — không gọi Google thật): Google hợp lệ → 200; token bị từ chối → 401 `AUTH_GOOGLE_TOKEN_INVALID`; JWKS lỗi → 502; email đã có nhưng chưa xác minh → 400; tài khoản bị vô hiệu → 403; đăng ký `an.nguyen@x.com` rồi `an.nguyen@y.com` → `UserName` là `an.nguyen` và `an.nguyen2`; đăng xuất hai lần liên tiếp đều 204 và DB ghi `RevokedAt`; **hồi quy:** đăng nhập sai quá số lần vẫn trả **423** kèm `lockoutEnd` sau khi chuyển sang `AccountLockedException`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**ID Token flow** đẩy toàn bộ vòng lặp chuyển hướng về phía trình duyệt và Google, Backend chỉ còn một việc duy nhất: nhận một chuỗi JWT rồi xác minh chữ ký. Hệ quả kiến trúc là Backend **giữ nguyên tính stateless** — không phải lưu `state`, không phải lưu PKCE `code_verifier`, không phải mở thêm route callback, không cần `GoogleClientSecret`. Với một hệ thống đặt mục tiêu scale ngang (NFR-SCALE-001), mọi thứ phải lưu giữa hai request đều là gánh nặng: nó buộc ta phải chọn giữa sticky session hoặc một kho state dùng chung.

Điểm **bắt buộc không được cắt**: Backend phải **tự xác minh chữ ký** ID Token bằng `Google.Apis.Auth`. Nếu tin dữ liệu Frontend gửi lên (kiểu "FE đã đăng nhập Google rồi, BE cứ tạo user theo email FE báo") thì bất kỳ ai cũng có thể `curl` thẳng vào endpoint với email của người khác và chiếm tài khoản. Đây là lỗ hổng nghiêm trọng nhất có thể mắc trong luồng OAuth.

**Vì sao bọc Google sau `IGoogleIdTokenValidator`:** `GoogleJsonWebSignature.ValidateAsync` là hàm tĩnh gọi mạng tới Google. Gọi thẳng trong handler thì (1) tầng Application phải tham chiếu gói thư viện `Google.Apis.Auth` — trái nguyên tắc Application chỉ phụ thuộc Domain; (2) không có cách nào viết integration test cho nhánh 401/502 mà không gọi Google thật. Interface một phương thức giải quyết cả hai, và chính lớp cài đặt ở Infrastructure là nơi dịch `InvalidJwtException` → `InvalidTokenException`, `HttpRequestException` → `BadGatewayException` — handler chỉ thấy lỗi nghiệp vụ.

**Logout idempotent** là lựa chọn có chủ đích: trả 404 khi token không tồn tại sẽ biến endpoint thành một *oracle* cho phép kẻ tấn công dò xem chuỗi token nào đang hợp lệ. Ngoài ra người dùng bấm Đăng xuất hai lần (hoặc mạng chập chờn gây thử lại) không có lý do gì phải nhận lỗi.

Việc **link Google vào email đã tồn tại** thay vì tạo tài khoản trùng email giải quyết một tình huống rất thực tế: người dùng đăng ký bằng email/mật khẩu hôm trước, hôm sau bấm "Đăng nhập bằng Google" với cùng email đó. Nếu tạo tài khoản mới, họ sẽ mất toàn bộ công thức đã viết mà không hiểu vì sao. Nhưng tự động liên kết **chỉ an toàn khi Google xác nhận email đã được kiểm chứng** (`email_verified`): nếu liên kết theo một email chưa kiểm chứng, kẻ tấn công chỉ cần tạo tài khoản Google gắn email của nạn nhân là **chiếm được tài khoản Culinary Blog của nạn nhân**. Một dòng kiểm tra `payload.EmailVerified` chặn đứng kịch bản đó.

**Vì sao `IUserRepository` không ghi thẳng `AspNetUsers` và không kế thừa `IRepository<T>`:** `UserManager` của Identity làm nhiều việc mà một repository tự viết sẽ bỏ sót — chuẩn hóa `NormalizedEmail`/`NormalizedUserName` (thiếu là tìm theo email không ra), đổi `SecurityStamp`, đếm lần đăng nhập sai để khóa tài khoản tạm thời. Ghi thẳng bảng là tạo ra người dùng "đúng dữ liệu nhưng sai trạng thái bảo mật" mà không có lỗi nào báo. Vì vậy phân vai rõ: **ghi người dùng → `IIdentityService`/`UserManager`**, **đọc người dùng phục vụ nghiệp vụ + vòng đời refresh token → `IUserRepository`**. `IRepository<T>` ràng buộc `T : BaseEntity` (có `Id` Guid, xóa mềm, `RowVersion`), trong khi `ApplicationUser` là kiểu Identity ở Infrastructure và `RefreshToken` không kế thừa `BaseEntity` (SRS §7.8) — ép chúng vào `IRepository<T>` là nới lỏng ràng buộc cho mọi module chỉ vì một ngoại lệ.

**Vì sao gộp refresh token vào `IUserRepository` thay vì giữ `IRefreshTokenRepository`:** refresh token không có vòng đời độc lập — nó sinh ra khi người dùng đăng nhập và mọi truy vấn về nó đều bắt đầu từ `UserId` (đăng xuất B3, thu hồi cả họ token B4, khóa tài khoản B7, danh sách phiên B7). Một repository cho cả nhóm thao tác "phiên của người dùng" giúp Buổi 4 (refresh, khóa tài khoản, danh sách phiên — cùng một buổi) chỉ thêm phương thức vào **một** chỗ.

**Vì sao làm D-1 ngay buổi này, không đợi Buổi 5 (Profile):** luồng Google tạo tài khoản mới mà **không có ai nhập `userName`** — nên bắt buộc phải có bộ sinh `UserName` tự động ngay bây giờ. Khi đã có nó, giữ ô "Tên đăng nhập" ở form đăng ký thường là giữ hai cách tạo `UserName` song song cho cùng một hệ thống. Ngoài ra `UserDto` là hợp đồng mà **mọi màn hình** FE đọc để hiển thị người dùng; đổi nó ở Buổi 3 khi mới có 2 màn hình dùng là rẻ nhất — càng để muộn, càng nhiều component phải sửa.

**Vì sao tạo `UserNotFoundException` từ bây giờ dù Buổi 4 mới dùng:** yêu cầu của buổi này là dựng **trọn** cụm exception của module. Tạo lớp cùng ánh xạ và kiểm thử ngay bây giờ giúp API FR-AUTH-008 ở Buổi 4 chỉ việc `throw`. Mã `AUTH_USER_NOT_FOUND` chưa có trong Phụ lục B nên đi qua CR-2026-04 (§4.5) — không tự đặt mã ngoài SRS.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 mô tả **ba kiến trúc Google OAuth khác nhau ở ba chỗ**: FR-AUTH-003 ghi *"Authorization Code Flow + PKCE, Auth.js v5 ở Next.js xử lý callback, FE gửi `ExternalLoginInfo` lên BE"*; mục 5.3 khai báo redirect URI là `/api/v1/auth/google/callback` (tức **BE** nhận callback); Chương 8 lại nhận body `{ idToken }` từ Google Sign-In JS SDK (ID-token flow, không phải Authorization Code). Nghiêm trọng hơn, `ExternalLoginInfo` là **kiểu nội bộ của ASP.NET Core Identity — không serialize qua HTTP được**, nên đặc tả đó không khả thi về mặt kỹ thuật.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-11**) chốt **ID Token flow**: body `{ idToken }`, BE xác minh bằng `GoogleJsonWebSignature.ValidateAsync`, **bỏ `ExternalLoginInfo`, bỏ Authorization Code + PKCE, bỏ redirect URI phía BE**. *(Đây là điểm xung đột **X-1** với lộ trình đề bài — xem §3.)*
- **Tại sao chọn:** So sánh ba phương án theo tầm nhìn dài hạn. **(A) ID Token** — đơn giản nhất, BE stateless, chỉ cần `GoogleClientId`, khớp Chương 8; nhược điểm là không lấy được refresh token của Google, nhưng ta **không cần** vì hệ thống tự phát JWT riêng. **(B) Tin Auth.js ở FE** — tận dụng thư viện có sẵn nhưng BE phải tin dữ liệu FE gửi lên → **lỗ hổng bảo mật nghiêm trọng**, loại ngay. **(C) BE tự chạy Authorization Code + PKCE** — chuẩn mực nhất về lý thuyết, nhưng BE phải quản lý state/verifier qua nhiều request (phá vỡ stateless), phải xử lý chuyển hướng qua lại giữa BE và FE, và tăng đáng kể số đường code cần kiểm thử. Với hệ thống mà JWT do chính ta phát hành và Google chỉ đóng vai trò *"chứng minh người này sở hữu email X"*, phương án A đạt đúng mục tiêu bảo mật với chi phí kiến trúc thấp nhất.
- **Mâu thuẫn trong SRS v1.2.1 — cùng một mã, hai mã HTTP (đã làm rõ ở v1.2.2, MT-61):** Phụ lục B ghi `AUTH_GOOGLE_TOKEN_INVALID` = **400**, còn FR-AUTH-003 A1 trả **401** với chính mã đó khi chữ ký/`aud`/`exp` sai. Kế hoạch theo **FR** (401 cho token không qua kiểm tra chữ ký, 400 cho token thiếu dữ liệu) vì FR là đặc tả hành vi chi tiết hơn; ánh xạ theo **kiểu exception** (không theo mã) cho phép làm đúng cả hai. SRS v1.2.2 (CR-2026-04 c) ghi rõ hai trường hợp này ở cả FR-AUTH-003 lẫn Phụ lục B.
- **Đề xuất phân công ghi `AccountLockedException` → 403:** trái Phụ lục B (**423** — MT-08 chỉ loại 422, không đổi 423) và trái kiểm thử Buổi 2 đang chạy. Giữ 423; mã 403 thuộc `AccountDisabledException`.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314236_HoangBinhQuan_buoiso3`.
```
feat(auth): add auth domain exceptions, user repository and complete google sign-in & logout APIs
```

## DEV 2 — Tạo công thức nháp & Quản lý ảnh (+ Recipe Domain Exceptions & Recipe Repository)

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-003` Tạo Công thức Nấu ăn Mới (trạng thái Draft) + `FR-RCP-008` Quản lý Ảnh Công thức (Upload / Metadata / Delete). **Nền tảng module Recipe:** cụm Recipe Domain Exceptions, `IRecipeRepository`/`RecipeRepository` (eager loading aggregate), bộ dịch lỗi ghi DB của Recipe, ánh xạ lỗi Recipe trong middleware. **Kèm retrofit D-8:** `Instructions` chuyển sang `NULL`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-8 — migration `B3_Recipe_InstructionsNullable`:** `Instructions` bỏ `.IsRequired()` → cột **NULL**. Phải làm **trước** bước 5, vì body mới coi `instructions?` là tùy chọn — nếu cột còn `NOT NULL` thì mọi request không gửi trường này sẽ nổ ở `SaveChangesAsync()`.
2. **Domain Exceptions — `Domain/Exceptions/Recipes/`:** lớp gốc trừu tượng `RecipeDomainException : DomainException` và:

   | Lớp | Mã (Phụ lục B) | HTTP | Dùng ở |
   |---|---|---|---|
   | `RecipeNotFoundException` (theo `id` hoặc `slug`) | `RECIPE_NOT_FOUND` | 404 | Mọi command theo `id` (B3), `GetRecipeBySlugQuery` (chuyển từ Buổi 2) |
   | `RecipeImageNotFoundException(recipeId, imageId)` | `RECIPE_NOT_FOUND` | 404 | PATCH/DELETE ảnh (B3) |
   | `RecipeConcurrencyException(recipeId, inner)` | `RECIPE_CONCURRENCY_CONFLICT` | 409 | Sinh tự động bởi bộ dịch ở bước 4 (B3 → `PUT` B4, kiểm thử đồng thời B8) |
   | `InvalidRecipeStatusException(RecipeStatus from, string action)` | `RECIPE_INVALID_STATE_TRANSITION` | 409 | Máy trạng thái + D-15 (API B4 — Dev 4) |
   | `RecipeSlugConflictException(slug)` | `RECIPE_SLUG_EXISTS` | 409 | Sinh tự động khi hai request tranh cùng slug (bước 4) |

   Chuyển `NotFoundException(ErrorCodes.RecipeNotFound, …)` trong `GetRecipeBySlugQuery` sang `RecipeNotFoundException`. `RECIPE_FORBIDDEN` **giữ** `ForbiddenException` (AppException) — đó là lỗi phân quyền do `RecipeAuthorizationHandler` quyết định, không phải bất biến của entity.
3. **Repository — `IRecipeRepository : IRepository<Recipe>` (Application) / `RecipeRepository` (Infrastructure):**
   - `GetByIdWithDetailsAsync(Guid id, …)` — `Include(Steps)`, `Include(Ingredients)`, `Include(Images)`, `.AsSplitQuery()` (tránh tích Descartes 3 collection). **`Nutrition` là Owned Entity nên EF tự nạp cùng dòng `Recipes`, không cần `Include`.** Dùng cho Buổi 4: bước/nguyên liệu/cập nhật (Dev 2), xuất bản/lưu trữ — D-15 bắt buộc có đủ Steps + Ingredients (Dev 4), `/recipes/mine/{id}` (Dev 3).
   - `GetByIdWithImagesAsync(Guid id, …)` — chỉ nạp `Images`, cho 3 command ảnh của buổi này (không kéo bước/nguyên liệu không dùng tới).
   - `SlugExistsAsync(string slug, …)` — dùng **`IgnoreQueryFilters()`** vì `IDX_Recipe_Slug` hiện là unique **thường** (tính cả bản ghi đã xóa mềm) — thiếu nó thì trùng slug với recipe đã xóa sẽ nổ `23505`. Buổi 4 (D-3 — Dev 4) đổi index sang partial thì bỏ `IgnoreQueryFilters()` tại đúng phương thức này.
   - Thêm thuộc tính `Recipes` vào `IUnitOfWork`/`UnitOfWork`. `IRecipeReadRepository` (Buổi 2) giữ nguyên cho phía Query.
4. **Bộ dịch lỗi ghi DB — `RecipePersistenceExceptionTranslator : IPersistenceExceptionTranslator`:** `DbUpdateConcurrencyException` có entry thuộc aggregate Recipe (`Recipe`, `RecipeStep`, `RecipeIngredient`, `RecipeImage` — lấy `RecipeId` của entry con) → `RecipeConcurrencyException`; `23505` trên `IDX_Recipe_Slug` → `RecipeSlugConflictException`. Đây là cơ chế mà FR-RCP-004 (Buổi 5) và kiểm thử đồng thời (Buổi 8) dựa vào — handler không bao giờ phải bắt exception của EF.
5. **Ánh xạ HTTP — `RecipeExceptionMappings.cs`:** đăng ký 5 lớp theo cột HTTP ở bước 2.
6. **CQRS — `CreateRecipeCommand` + Validator:** body `{ title, description, categoryId, prepTime, cookTime, servings, difficulty, instructions?, nutrition? }` — `title` 5–200, `description` **20–2000** (Bảng Giới hạn Chuẩn §7.9), `prepTime > 0`, `cookTime >= 0`, `servings > 0`. **`categoryId` phải tồn tại:** quy tắc bất đồng bộ trong validator gọi `IRepository<Category>.ExistsAsync` (repository generic của Dev 4 — **không phải chờ** `ICategoryRepository` của Dev 3) → sai thì **400 `VALIDATION_ERROR`** với lỗi ở trường `categoryId` (FR-RCP-003 A2). Dùng factory **`Recipe.Create(...)` đã có sẵn từ Buổi 2** (`Status = Draft`, `PublishedAt = null`); slug qua `SlugHelper.Generate(title)` (đã có ở `Domain/Common`), trùng (`SlugExistsAsync`) thì hậu tố `-2`, `-3`; **cấm trùng từ khóa dành riêng** `search`, `mine`, `sitemap`, `new`, `edit`. Nutrition gán qua `recipe.SetNutrition(...)` — **không có endpoint riêng**. Ghi: `unitOfWork.Recipes.AddAsync(recipe)` → `SaveChangesAsync()`. Hai mảng tùy chọn `steps?`/`ingredients?` trong cùng body được kích hoạt ở **Buổi 4** (xem giải trình).
7. **Authorization dùng chung:** `RecipeAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Recipe>` (đạt khi `recipe.AuthorId == currentUser.Id` **hoặc** người dùng có role `Admin`); `AuthorPolicy = RequireRole("Author","Admin")`. Đây là handler dùng lại cho **toàn bộ** FR-RCP còn lại.
8. **CQRS ảnh:** `UploadRecipeImageCommand` nạp recipe bằng `GetByIdWithImagesAsync` (không có → `RecipeNotFoundException`), gọi `IFileStorageService` + `ImageFileInspector` (magic bytes, có sẵn từ Buổi 2), folder `recipes/{recipeId}/`, rồi `recipe.AddImage(...)` (**đã có sẵn** — ảnh đầu tiên tự `IsPrimary = true`); `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`; `DeleteRecipeImageCommand` xóa bản ghi + `BackgroundJob.Enqueue` xóa file MinIO, xóa đúng ảnh chính thì ảnh có **`OrderIndex` nhỏ nhất (hòa thì `CreatedAt` sớm nhất)** lên thay. Ảnh không thuộc recipe → `RecipeImageNotFoundException` → 404. ⚠️ **Đổi ảnh chính phải làm hai bước trong một transaction tường minh — dùng `unitOfWork.ExecuteInTransactionAsync(...)`**: bên trong, nạp lại recipe → hạ ảnh chính cũ → `SaveChangesAsync` → nâng ảnh chính mới → `SaveChangesAsync`. Vì index `IDX_RecipeImage_Primary` (partial unique, có từ Buổi 2) **không thể deferrable**, và EF Core không đảm bảo thứ tự hai lệnh `UPDATE` trong một batch. Tất cả cài đặt `ICacheInvalidator` → xóa tiền tố `recipes:list:` + khóa `recipe:{slug}`.
9. **API + UI + Kiểm thử:** `POST /api/v1/recipes` (201); `POST /api/v1/recipes/{id}/images` (multipart, 201 trả `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }`, **503 `FILE_STORAGE_UNAVAILABLE`** nếu MinIO lỗi); `PATCH` và `DELETE /api/v1/recipes/{id}/images/{imageId}`. UI `/dashboard/recipes/new` — biểu mẫu nhiều bước bước 1 (thông tin cơ bản + dinh dưỡng), bước 2 (gallery ảnh dùng `ImageUploader`, chọn ảnh chính bằng radio → gọi PATCH), toast xác nhận. **Integration test:** tạo nháp 201; body sai → 400 kèm `errors`; `categoryId` không tồn tại → 400 ở trường `categoryId`; Guest → 401; recipe của người khác → 403 `RECIPE_FORBIDDEN`; ảnh không tồn tại → 404 `RECIPE_NOT_FOUND`; MinIO lỗi → 503; **đổi ảnh chính giữa hai ảnh 10 lần liên tiếp không lần nào vỡ unique index**; hai `DbContext` cùng sửa một recipe → `SaveChangesAsync` lần sau ném `RecipeConcurrencyException` (kiểm chứng bộ dịch trên PostgreSQL thật, trước khi có endpoint update ở Buổi 5).

**Phần 3 – Định hướng & Lý do thiết kế:**
Công thức **luôn sinh ra ở trạng thái `Draft`** chứ không cho tạo thẳng Published: người viết cần không gian nháp để hoàn thiện dần, và hệ thống cần một điểm chặn để kiểm tra điều kiện xuất bản (đủ bước + nguyên liệu) ở FR-RCP-005. Cho phép tạo thẳng Published sẽ mở đường cho công thức rỗng lọt ra ngoài và làm hỏng structured data SEO.

**Nutrition đi kèm trong body `POST /recipes`** chứ không có endpoint riêng — hệ quả trực tiếp của việc nó là Owned Entity. Nếu làm `PUT /recipes/{id}/nutrition` riêng, ta phải tạo thêm command, validator, DTO và repository cho một thứ vốn chỉ là nhóm cột trong bảng `Recipes`, đồng thời mở ra khả năng **hai đường code cùng ghi một nhóm cột** — nguồn lỗi kinh điển. Frontend dùng biểu mẫu nhiều bước nhiều bước thì gom state ở client rồi submit một lần.

**Repository theo aggregate, hai mức nạp dữ liệu:** Recipe là aggregate root (SRS §3.3) — steps, ingredients, images chỉ được sửa qua `Recipe`, nên repository chỉ có cho `Recipe`, không có `IRecipeImageRepository` hay `IRecipeStepRepository`. Hai phương thức nạp (`WithImages` / `WithDetails`) thay vì một phương thức nạp tất cả vì command ảnh chạy rất thường xuyên (mỗi lần kéo-thả, mỗi lần chọn ảnh chính) — kéo theo 10+ nguyên liệu và 5+ bước mỗi lần là tốn vô ích. Ngược lại, **không** mở `IQueryable` cho handler tự `Include`: khi đó mỗi handler tự quyết định nạp gì, và lỗi kinh điển "gọi `Publish()` khi chưa `Include(Steps)` nên recipe đủ điều kiện vẫn bị từ chối" (Buổi 5) sẽ lặp lại ở mọi nơi.

**Dịch lỗi xung đột ở Infrastructure, không bắt trong handler:** `DbUpdateConcurrencyException` là kiểu của EF Core; tầng Application không tham chiếu EF nên handler **không thể** bắt nó mà không phá Clean Architecture. Dịch ngay tại `UnitOfWork` nghĩa là mọi command ghi Recipe — hiện tại và tương lai — tự động trả 409 `RECIPE_CONCURRENCY_CONFLICT` thay vì 500, không ai phải nhớ viết `try/catch`. Kiểm chứng bộ dịch ngay Buổi 3 (chưa có endpoint update) vì PATCH ảnh đã có thể xung đột khi hai tab cùng đổi ảnh chính.

**`categoryId` kiểm tra bằng `IRepository<Category>` generic**, không đợi `ICategoryRepository` của Dev 3: câu hỏi "Id này có tồn tại không" là thao tác chung mà repository generic đã trả lời được. Nhờ vậy Dev 2 không phụ thuộc thứ tự làm việc của Dev 3 trong buổi, và hai module không tham chiếu repository chuyên biệt của nhau.

`RecipeAuthorizationHandler` được dựng **ngay ở FR đầu tiên có ghi dữ liệu**, không đợi tới lúc có nhiều endpoint mới gom lại. Lý do: NFR-SEC-006 yêu cầu kiểm tra phân quyền **tại Application Layer**, không chỉ ở Presentation. Nếu mỗi handler tự viết `if (recipe.AuthorId != userId) throw` thì chỉ cần một handler quên là có lỗ hổng, và không có cách nào kiểm chứng tập trung.

Quy tắc **"xóa ảnh primary thì ảnh nào lên thay"** được ghi thành tiêu chí xác định (`OrderIndex` nhỏ nhất, hòa thì `CreatedAt` sớm nhất) thay vì "ảnh đầu tiên còn lại": "đầu tiên" theo thứ tự nào là câu hỏi không có đáp án nếu không nói rõ, và hai dev sẽ hiểu hai kiểu — một người lấy theo thứ tự trả về của DB (vốn không đảm bảo), một người lấy theo thời gian tạo.

**Vì sao hai mảng inline `steps?`/`ingredients?` của `POST /recipes` để sang Buổi 4:** chúng cần đúng những bất biến mà FR-RCP-009/010 xây ở Buổi 4 — cột `QuantityText`, ràng buộc "không rỗng cả ba", `StepNumber` do server gán. Làm chúng ngay bây giờ nghĩa là viết validator nguyên liệu/bước **hai lần** (một lần cho body inline, một lần cho endpoint riêng) rồi phải giữ đồng bộ mãi mãi — đúng loại trùng lặp logic sinh nợ kỹ thuật. Ở Buổi 4, body inline chỉ việc gọi lại `recipe.AddIngredient(...)`/`recipe.AddStep(...)`, nên mỗi quy tắc chỉ tồn tại ở một chỗ. Giao diện của Buổi 3 cũng không cần chúng (wizard bước 3–4 thuộc Buổi 4), nên lát cắt DB → API → UI của buổi này vẫn trọn vẹn.

**Vì sao tạo `InvalidRecipeStatusException` từ Buổi 3 dù Buổi 4 mới ném:** máy trạng thái Recipe do một dev khác (Dev 4, Buổi 4) hiện thực. Có sẵn lớp exception + ánh xạ 409 + kiểm thử ánh xạ trong module Recipe từ bây giờ nghĩa là Dev 4 dùng **đúng** lớp của module, không tự nghĩ ra lớp thứ hai.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-008 của v1.0.0 thiết kế endpoint hành động riêng `PATCH /recipes/{id}/images/{imgId}/primary` với response tải lên `{ url, isPrimary }`, trong khi Chương 8.4 thiết kế `PATCH /recipes/{id}/images/{imageId}` với body gộp `{ altText?, isPrimary?, orderIndex? }` và response 4 trường — hai thiết kế API khác hẳn nhau. (b) FR-RCP-008 bước 1 nói form-data chỉ chứa trường `"file"` nhưng bước 6 lại dùng `altText`. (c) Nhánh A4 trả **503** khi MinIO lỗi nhưng ô "HTTP Status Code trả về" của chính FR đó **không liệt kê 503**. (d) FR-RCP-003 nói Steps/Ingredients "có thể tạo cùng lúc hoặc thêm riêng lẻ sau", nhưng dinh dưỡng bị bỏ lửng — không FR nào đặc tả endpoint riêng cho nó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-19, MT-41.4, MT-41.5, MT-41.6, MT-02**) chốt **PATCH metadata gộp**, response tải lên đủ 7 trường (thêm `mediumUrl`, `thumbnailUrl` vì FR-JOB-002 sinh ra chúng), form-data có `altText?`/`isPrimary?`, bổ sung **503** vào danh sách mã trạng thái HTTP, và ghi rõ **dinh dưỡng chỉ tạo/sửa cùng recipe**.
- **Tại sao chọn:** PATCH gộp **RESTful hơn** — một tài nguyên, một endpoint, sửa được nhiều trường trong một request; endpoint hành động riêng `/primary` sẽ kéo theo `/alt-text`, `/order` khi cần sửa các trường khác, làm số endpoint phình theo số thuộc tính. Nhược điểm của PATCH gộp là logic "chỉ 1 ảnh primary" nằm lẫn trong handler chung nên phải xử lý cẩn thận — chấp nhận được, và đã được ghi thành hai quy tắc nghiệp vụ tường minh trong SRS Chương 8.4 để không ai quên. Về `mediumUrl`/`thumbnailUrl`: trả về ngay cả khi còn `null` (job đổi kích thước chưa chạy) giúp **hợp đồng API ổn định** — FE không phải xử lý hai hình dạng response khác nhau tùy thời điểm.
- **`categoryId` không tồn tại — 400 hay 404?** Chương 8.3 ghi *"404 (categoryId không tồn tại)"*, còn FR-RCP-003 A2 ghi **400** với lỗi field-level. Kế hoạch theo **FR (400)**: `categoryId` là một trường của body, sai giá trị là lỗi đầu vào; trả 404 sẽ khiến FE hiểu nhầm là **URL** `/recipes` không tồn tại, và form không gắn được lỗi vào ô "Danh mục". SRS v1.2.2 (CR-2026-04 c, MT-61) đã sửa Chương 8.3 cho khớp.
- **Phụ lục B không có mã riêng cho ảnh không tồn tại:** SRS §8.4 chỉ ghi 404. Dùng lại `RECIPE_NOT_FOUND` vì ảnh là thành phần của aggregate Recipe và FE xử lý hai trường hợp như nhau (tải lại trang công thức); không tự đặt mã mới ngoài SRS.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2312758_NguyenHongPhucTho_buoiso3`.
```
feat(recipe): add recipe domain exceptions, recipe repository and complete create draft & recipe image APIs
```

## DEV 3 — CRUD Danh mục [Admin] (+ Category Domain Exceptions & Category Repository)

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-CAT-003` Tạo Danh mục + `FR-CAT-004` Cập nhật Danh mục + `FR-CAT-005` Xóa Danh mục (Admin, Auto Slug, Conflict 409). **Nền tảng module Category:** cụm Category Domain Exceptions, `ICategoryRepository`/`CategoryRepository`, bộ dịch lỗi ghi DB của Category, ánh xạ lỗi Category trong middleware. **Kèm retrofit D-13:** TTL `categories:all` 60 → **30 phút**.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain Exceptions — `Domain/Exceptions/Categories/`:** lớp gốc trừu tượng `CategoryDomainException : DomainException` và:

   | Lớp | Mã (Phụ lục B) | HTTP | Dùng ở |
   |---|---|---|---|
   | `CategoryNotFoundException` (theo `id` hoặc `slug`) | `CATEGORY_NOT_FOUND` | 404 | PUT/DELETE (B3), `GetCategoryBySlugQuery` (chuyển từ Buổi 2) |
   | `CategoryNameAlreadyExistsException(name)` | `CATEGORY_NAME_EXISTS` | 409 | POST/PUT — kiểm tra chủ động + bộ dịch `23505` |
   | `CategoryHasRecipesException(int recipeCount)` | `CATEGORY_DELETE_HAS_RECIPES` + extension `recipeCount` | 409 | DELETE |

   Chuyển `NotFoundException(ErrorCodes.CategoryNotFound, …)` trong `GetCategoryBySlugQuery` sang `CategoryNotFoundException`.
2. **Domain — entity `Category`:** thêm `Update(name, description, imageUrl, orderIndex)` (**không** đụng `Slug`) và `SoftDelete()` (gán `IsDeleted = true`). Handler không gán thuộc tính trực tiếp.
3. **Repository — `ICategoryRepository : IRepository<Category>` (Application) / `CategoryRepository` (Infrastructure):**
   - `ExistsByNameAsync(string name, Guid? excludeId, …)` — `excludeId` để PUT không tự báo trùng với chính nó.
   - `SlugExistsAsync(string slug, …)`.
   - `CountActiveRecipesAsync(Guid categoryId, …)` — đếm recipe `IsDeleted = false` ở **mọi** trạng thái (Draft cũng chặn xóa).
   - Hai phương thức kiểm tra trùng dùng **`IgnoreQueryFilters()`**: `IDX_Category_Name` và `IDX_Category_Slug` là unique **thường**, tính cả danh mục đã xóa mềm — bỏ qua bản ghi đã xóa thì kiểm tra chủ động nói "không trùng" nhưng DB vẫn ném `23505`.
   - Thêm thuộc tính `Categories` vào `IUnitOfWork`/`UnitOfWork`. `ICategoryReadRepository` (Buổi 2) giữ nguyên cho phía Query.
4. **Lớp phòng vệ thứ hai — `CategoryPersistenceExceptionTranslator : IPersistenceExceptionTranslator`:** `DbUpdateException` có `PostgresException { SqlState: "23505", ConstraintName: "IDX_Category_Name" }` → `CategoryNameAlreadyExistsException` → 409 thay vì 500 — phòng tình huống hai Admin tạo trùng tên cùng lúc (race condition) lọt qua bước kiểm tra chủ động. *(Bản kế hoạch trước đặt việc này trong `GlobalExceptionMiddleware`; nay chuyển về Infrastructure — xem giải trình.)*
5. **Ánh xạ HTTP — `CategoryExceptionMappings.cs`:** đăng ký 3 lớp theo cột HTTP ở bước 1.
6. **CQRS — `CreateCategoryCommand`:** Validator `name` **2–100 ký tự** (theo §7.9, **không phải 2–50** như v1.0.0), `description?`, `imageUrl?` URL hợp lệ, `orderIndex?` ≥ 0. Handler **kiểm tra `Name` trùng chủ động** (`unitOfWork.Categories.ExistsByNameAsync`) trước khi ghi → `CategoryNameAlreadyExistsException` → **409 `CATEGORY_NAME_EXISTS`**; sinh slug tự động + hậu tố `-2`,`-3` nếu trùng (`SlugExistsAsync`); `AddAsync` → `SaveChangesAsync()`; trả 201 kèm header `Location: /api/v1/categories/{slug}`.
7. **CQRS — `UpdateCategoryCommand`:** cùng bộ validator; không tìm thấy → `CategoryNotFoundException` → 404; **cũng phải kiểm tra `Name` trùng** (`excludeId = id`, đổi sang tên đã có → 409); **slug KHÔNG đổi** khi đổi tên (tránh chết link đã chia sẻ).
8. **CQRS — `DeleteCategoryCommand`:** `CountActiveRecipesAsync`; nếu > 0 → `CategoryHasRecipesException(count)` → **409 `CATEGORY_DELETE_HAS_RECIPES`**, số lượng vừa nằm trong `detail` vừa nằm trong extension `recipeCount` (FE hiển thị không cần tách chuỗi); nếu = 0 → `category.SoftDelete()`, **không xóa vật lý**.
9. **Cache + API + UI + Kiểm thử:** **retrofit D-13** — `GetCategoriesQuery.Expiration` đổi `TimeSpan.FromMinutes(60)` → **`30`** theo bảng TTL chuẩn §2.3 (buổi này mới có đường ghi danh mục, nên đây là lúc TTL bắt đầu có ý nghĩa thật); cả 3 command cài đặt `ICacheInvalidator` → xóa `categories:all` và tiền tố `categories:detail:`. API: `POST /categories`, `PUT /categories/{id:guid}`, `DELETE /categories/{id:guid}` — tất cả `RequireAuthorization("AdminPolicy")` → 403 cho Author. UI `/dashboard/categories` (Admin only): bảng danh sách, modal form RHF+Zod (name, description, `ImageUploader` cho imageUrl, orderIndex), confirm dialog khi xóa, **hiển thị lỗi 409 thân thiện** đọc `recipeCount` ("Danh mục này đang có 7 công thức, hãy chuyển chúng sang danh mục khác trước"). **Integration test:** 201 + `Location`; Author → 403; 409 trùng tên (kiểm tra chủ động); **hai request tạo cùng tên song song bằng `Task.WhenAll` → đúng một 201 và một 409, không có 500** (kiểm chứng bộ dịch `23505` trên PostgreSQL thật); PUT đổi tên → slug giữ nguyên; PUT sang tên đã có → 409; DELETE còn recipe → 409 kèm `recipeCount`; DELETE danh mục rỗng → 204, sau đó `GET /categories/{slug}` → 404 `CATEGORY_NOT_FOUND`; `GET /categories` sau mỗi thao tác ghi thấy dữ liệu mới (cache đã bị xóa).

**Phần 3 – Định hướng & Lý do thiết kế:**
**Kiểm tra `Name` trùng chủ động ở tầng Application** thay vì để ràng buộc UNIQUE của cơ sở dữ liệu bắt, vì hai lý do. Thứ nhất về trải nghiệm: dựa vào exception của DB thì người dùng nhận **HTTP 500 "Lỗi hệ thống"** thay vì một thông báo nghiệp vụ rõ ràng, và log bị nhiễu bởi exception không đáng có. Thứ hai về khả năng bảo trì: dò mã lỗi PostgreSQL `23505` là code khó đọc và phụ thuộc vào chi tiết của một DBMS cụ thể. Tuy nhiên **vẫn giữ lớp bắt `23505`** làm phòng vệ thứ hai — kiểm tra chủ động không loại bỏ được hoàn toàn khe hở race condition giữa lúc `SELECT` và lúc `INSERT`.

**Vì sao bắt `23505` ở Infrastructure (bộ dịch lỗi) chứ không ở middleware như bản kế hoạch trước:** chính lý do ở đoạn trên — *"phụ thuộc chi tiết của một DBMS"* — là lý do nó không được nằm ở tầng API. Middleware muốn đọc `PostgresException` thì project API phải phụ thuộc Npgsql, và mỗi ràng buộc unique mới của mỗi module lại phải sửa **một** file middleware dùng chung (xung đột merge mỗi buổi). Đặt ở bộ dịch lỗi của module: kiến thức "ràng buộc `IDX_Category_Name` nghĩa là tên danh mục trùng" nằm cạnh repository của chính module đó, middleware chỉ còn thấy một `CategoryNameAlreadyExistsException` bình thường.

**Vì sao repository KHÔNG tích hợp cache Redis:** hệ thống có **một** cơ chế cache duy nhất (NFR-PERF-003, §2.2): Query khai báo `ICacheable` → `CachingBehavior` đọc/ghi Redis; Command khai báo `ICacheInvalidator` → `CacheInvalidationBehavior` xóa khóa **sau khi** handler thành công. Nếu repository cũng tự cache thì có hai tầng cache cho cùng dữ liệu với hai TTL và hai chỗ phải nhớ xóa cache — đúng loại lỗi "sửa danh mục xong vẫn thấy tên cũ" mà không kiểm thử nào bắt được vì nó phụ thuộc thời điểm. Ngoài ra repository phía Command trả entity được EF theo dõi để **sửa**; cache một entity đang được sửa là sai về bản chất (dữ liệu trong cache sẽ khác dữ liệu sắp được lưu).

**Vì sao repository KHÔNG có "PostgreSQL Full-Text Search helper":** FR-SRCH-001 tìm kiếm trên **recipe** (cột generated `SearchVector` của bảng `Recipes`), danh mục không có yêu cầu tìm kiếm toàn văn nào trong SRS. Dev 3 dựng FTS ở **Buổi 4** như kế hoạch — khi đó phần truy vấn tìm kiếm thuộc phía Query (`IRecipeSearchReadRepository` hoặc mở rộng `IRecipeReadRepository`), không thuộc repository ghi của Category.

**Slug không đổi khi đổi tên** là quyết định bảo vệ SEO và người dùng: URL đã được chia sẻ, đã được Google lập chỉ mục, đã nằm trong bookmark. Đổi slug mà không có bảng lịch sử thì mọi link cũ chết ngay lập tức. Hệ thống cũng **không xây bảng lịch sử slug** — xem phân tích ở FR-RCP-004 (Buổi 5) và NFR-SEO-004.

**Xóa mềm cho Category** (không xóa cứng) giữ đúng thiết kế `BaseEntity` áp dụng cho toàn hệ thống. Ràng buộc FK `Recipes.CategoryId → Categories.Id` vẫn giữ `ON DELETE RESTRICT` — không phải để chặn thao tác xóa mềm (xóa mềm không đụng tới FK), mà là **lớp bảo vệ cuối cùng** cho mọi thao tác xóa **vật lý** danh mục: hiện FR-JOB-003 chỉ dọn Recipe, nhưng nếu sau này mở rộng sang Category (hoặc có người xóa tay trong DB), cơ sở dữ liệu sẽ từ chối xóa một danh mục còn công thức tham chiếu thay vì để lại dữ liệu mồ côi.

**Hệ quả cần biết của unique thường trên `Name`:** danh mục đã xóa mềm **vẫn giữ tên của nó** — tạo lại "Món chính" sau khi đã xóa "Món chính" sẽ nhận 409. Đây là hành vi đúng với ràng buộc DB hiện tại (SRS §7 không yêu cầu partial unique cho `Categories`), và an toàn hơn chiều ngược lại (khôi phục danh mục đã xóa theo quy trình MT-49 sẽ không bao giờ đụng tên với danh mục mới). Nếu nhóm muốn giải phóng tên sau khi xóa thì phải qua CR để đổi sang partial unique như D-3 của Recipe.

Thông báo lỗi 409 hiển thị **số lượng công thức cụ thể** thay vì câu chung chung: Admin cần biết quy mô việc phải làm trước khi quyết định, và con số đó server đã đếm sẵn rồi — không trả về là lãng phí. Trả thêm trong extension `recipeCount` để FE không phải tách số từ một chuỗi tiếng Việt có thể đổi câu chữ.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** bốn ô trong **cùng một bảng FR-CAT-003** không khớp nhau: ô "Điều kiện tiên quyết" ghi *"Name chưa tồn tại trong database"*; ô "HTTP Status Code" ghi *"409 Conflict – Name đã tồn tại"*; nhưng **Luồng chính bước 6 chỉ kiểm tra slug** (trùng thì tự thêm `-2`, `-3`), **không kiểm tra Name**; và Luồng thay thế chỉ có A1 (403) và A2 (422) — **không có nhánh nào dẫn tới 409**. Thêm nữa, validator ghi `Name` 2–50 ký tự trong khi cột DB là `varchar(100)` — lệch 2 lần. FR-CAT-005 ghi "Xóa entity" còn Chương 8 ghi "soft delete".
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-36, MT-37, MT-20.11**) bổ sung **bước kiểm tra Name trùng** vào cả FR-CAT-003 lẫn FR-CAT-004, chốt `Name` **2–100** theo Bảng Giới hạn Chuẩn §7.9, và chốt **xóa mềm**.
- **Tại sao chọn:** mã 409 ở v1.0.0 là **mã không bao giờ được sinh ra** — tạo danh mục trùng tên "Món chính" lần thứ hai sẽ đi hết luồng (slug `mon-chinh` trùng → tự đổi thành `mon-chinh-2` → `AddAsync` → `SaveChangesAsync`), rồi mới bị PostgreSQL chặn vì `Name` là UNIQUE → HTTP 500. Một mã lỗi được đặc tả mà không có đường nào sinh ra nó là dấu hiệu đặc tả chưa được đọc ngang qua bốn ô. Về giới hạn độ dài: nguyên tắc dài hạn chốt trong SRS §7.9 là **validator và cột DB phải BẰNG NHAU**, không phải "validator chặt hơn cho an toàn" — khi hai con số lệch nhau, không ai biết con số nào là yêu cầu thật, QA không biết lấy đâu làm chuẩn viết kiểm thử, và dữ liệu nhập qua seeding sẽ lọt qua validator rồi vẫn nằm được trong DB.
- **SRS FR-CAT-003 A4 ghi "`GlobalExceptionMiddleware` dịch mã `23505`":** kế hoạch giữ đúng **hành vi** SRS yêu cầu (409 `CATEGORY_NAME_EXISTS`, không bao giờ 500) nhưng đặt việc dịch ở bộ dịch lỗi của Infrastructure, middleware nhận exception đã dịch. Đây là chi tiết hiện thực, không đổi hợp đồng API.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314291_DoanHongTien_buoiso3`.
```
feat(category): add category domain exceptions, category repository and complete create & update admin category APIs
```
*(Commit gồm cả `DELETE /categories/{id}` — FR-CAT-005 — dù message nêu create & update theo phân công ngày 29/09.)*

## DEV 4 — Nền tảng Exception / Repository / Middleware, Health Checks, Hangfire Dashboard & Integration Test Harness

**Phần 1 – Chức năng hoàn thành trong buổi:**
**Commit nền kiến trúc** (lớp gốc `DomainException`, `IRepository<T>`/`IUnitOfWork`/`UnitOfWork`, nâng cấp `GlobalExceptionMiddleware` + `ExceptionStatusMap`) + **`FR-OBS-001` phần endpoint** (`/health`, `/health/live`, `/health/ready` — kéo từ Buổi 5 lên) + hoàn tất `FR-JOB-001` — **Hangfire Dashboard `/hangfire` được bảo vệ** (bản thân job email đã chạy thật từ Buổi 2, `BAO_CAO_BUOI_2.md` §4.6) + **Integration Test Harness** dùng chung cho cả nhóm (NFR-MAINT-002) + dẫn **commit nền D-11**.

**Trạng thái (29/09/2026):** D-11, Hangfire Dashboard, Integration Test Harness và trả nợ kiểm thử Buổi 2 — ✅ **đã xong** trong commit `67d29c0` (bước 5 → 8, `BAO_CAO_KET_QUA_BUOI_3.md`). Bước 1 → 4 là phần bổ sung theo yêu cầu ngày 29/09 — ✅ **đã code xong và kiểm chứng ngày 29/09/2026** (117 kiểm thử xanh, 1 bỏ qua có chủ đích; `/health` kiểm chứng trên `docker compose` với MinIO/Redis sập — `BAO_CAO_KET_QUA_BUOI_3.md` §11), chờ commit trên nhánh `2312755_NguyenThangThieng_buoiso3` rồi vào `develop` ở mốc M1.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain Exceptions nền — `Domain/Exceptions/`** (mã nguồn ở mục B của Hợp đồng kiến trúc):
   - `DomainException` thành lớp **trừu tượng**, chuyển từ `Domain/Common` sang `Domain/Exceptions`, thêm `Code` và `Extensions`.
   - `BusinessRuleViolationException` (400 mặc định) và `ConcurrencyConflictException` (409 `CONCURRENCY_CONFLICT` — CR-2026-04).
   - Chuyển `ErrorCodes` từ `Application/Common/Exceptions` sang `Domain/Exceptions` (chuỗi thuần, không phụ thuộc gì), chia khu vực theo module, và đưa **đủ 29 mã** của Phụ lục B v1.2.2 vào một lần — dev module không phải sửa file này. `AUTH_USERNAME_EXISTS` (ngoài SRS) giữ tạm cho tới khi Dev 1 làm xong D-1 ở cùng buổi.
   - Đổi ba chỗ `throw new DomainException(...)` của Buổi 2 sang `BusinessRuleViolationException`, cập nhật `RecipeTests`.
   - Thêm `BadGatewayException` (502) vào `AppException.cs` cho các lỗi dịch vụ ngoài (Google JWKS — Dev 1 dùng).
2. **Repository & Unit of Work nền** (mục C):
   - `IRepository<T>` và `IUnitOfWork` ở `Application/Common/Interfaces/Persistence` — `IUnitOfWork` hiện có (chỉ `SaveChangesAsync`) chuyển vào thư mục này và thêm `ExecuteInTransactionAsync`.
   - `EfRepository<T>` và `UnitOfWork` ở `Infrastructure/Persistence`; `CulinaryBlogDbContext` **không còn** tự đóng vai `IUnitOfWork`.
   - `IPersistenceExceptionTranslator` + quét tự động mọi cài đặt trong assembly Infrastructure.
   - Transaction đi qua `CreateExecutionStrategy()` theo đúng mẫu `DatabaseSeeder` đã chạy ổn định từ Buổi 2 (DbContext bật `EnableRetryOnFailure`).
3. **Global Exception Middleware** (mục D):
   - `ExceptionStatusMap` + `IExceptionStatusMapping` + quét tự động mọi mapping trong assembly API. Bảng tra theo kiểu và đi ngược lên lớp cha; đăng ký trùng một kiểu ở hai module → lỗi ngay lúc khởi động; exception chưa đăng ký rơi về 400.
   - `CommonExceptionMappings`: `BusinessRuleViolationException` → 400, `ConcurrencyConflictException` → 409.
   - `GlobalExceptionMiddleware` thêm nhánh `DomainException` tra `ExceptionStatusMap` (thay cho nhánh "mọi `DomainException` → 400 `VALIDATION_ERROR`" hiện tại) và chép `Extensions`.
   - Mọi response lỗi có `traceId` (do `IProblemDetailsService` tự thêm — không phải code riêng); Buổi 8 bổ sung `CorrelationId` khi có `CorrelationIdMiddleware`.
   - **Kiểm thử kiến trúc** (`ArchitectureTests`):
     - (a) mọi lớp `DomainException` **không trừu tượng** trong assembly Domain phải có ánh xạ tường minh trong `ExceptionStatusMap` — quên đăng ký là kiểm thử đỏ, không âm thầm rơi về 400;
     - (b) exception của module nằm đúng thư mục `Exceptions/{Module}`;
     - (c) interface repository không trả `IQueryable`;
     - (d) `ErrorCodes` khớp **đúng** Phụ lục B — kiểm thử đọc thẳng bảng Phụ lục B trong file SRS mới nhất ở `SPEC/` (không dùng danh sách chép tay), nên SRS đổi mà code không đổi (hoặc ngược lại) là kiểm thử đỏ; mã ngoài SRS duy nhất được miễn là `AUTH_USERNAME_EXISTS`, ghi rõ là tạm thời chờ D-1;
     - (e) `IRepository<T>` không có phương thức xóa cứng.
   - **Kiểm thử hợp đồng Problem Details** (integration, `ProblemDetailsContractTests`): bộ integration test đăng ký endpoint **chỉ tồn tại trong kiểm thử** (qua `IStartupFilter` của `CulinaryBlogApiFactory`) để ném từng loại lỗi. Kiểm tra với mỗi loại:
     - `Content-Type: application/problem+json`;
     - đúng `status` / `type`;
     - có `errors{}` với lỗi kiểm tra hợp lệ;
     - lỗi 500 không chứa stack trace.

     Thêm hai kịch bản thật:
     - hai `DbContext` cùng sửa một `Category` với `RowVersion` cũ → `UnitOfWork` ném `ConcurrencyConflictException`;
     - `ExecuteInTransactionAsync` ném lỗi sau lần `SaveChanges` đầu tiên → **không có dòng nào được lưu** (rollback thật trên PostgreSQL).
4. **Health Checks — FR-OBS-001 (phần endpoint):**
   - **Đăng ký check** trong `AddInfrastructure`, dùng hai gói thư viện đã có trong `CulinaryBlog.Infrastructure.csproj` từ Buổi 2:
     - `AddNpgSql(…, name: "database", tags: ["ready"])`;
     - `AddRedis(…, name: "redis", tags: ["ready"])` — dùng cùng chuỗi kết nối Redis của cache, thêm `connectTimeout=2000` để Redis chết thì check báo lỗi trong ≤ 3 giây;
     - `AddCheck<MinioHealthCheck>("minio", failureStatus: HealthStatus.Degraded, timeout: 3s)` — `MinioHealthCheck : IHealthCheck` tự viết bằng `AWSSDK.S3`, kiểm tra **đúng bucket ứng dụng dùng** có tồn tại (không chỉ "MinIO có trả lời"), vì gói thư viện `AspNetCore.HealthChecks.Minio` mà SRS nêu **không tồn tại** (`CONG_NGHE_VA_PHIEN_BAN.md` §6.4).
   - **Map endpoint** trong `Program.cs` — cả ba đều `AllowAnonymous`, không qua Output Cache:
     - `/health` → mọi check, JSON `{ status, entries }` qua `UIResponseWriter.WriteHealthCheckUIResponse` (gói thư viện `AspNetCore.HealthChecks.UI.Client` đã có), **503 khi Unhealthy**;
     - `/health/live` → `Predicate = _ => false` (không chạy check nào, **luôn 200** trừ khi process chết);
     - `/health/ready` → `Predicate = c => c.Tags.Contains("ready")`.
   - Nginx đã chuyển tiếp `~ ^/(health|scalar|openapi)` từ Buổi 2 — không phải sửa.
   - **Integration test:**
     - `/health/live` 200;
     - `/health/ready` 200 và `/health` có đủ `entries.database`, `entries.redis`, `entries.minio` trên Testcontainers;
     - factory thứ hai trỏ Redis sai địa chỉ → `/health/ready` **503** nhưng `/health/live` **vẫn 200**.
   - Phần còn lại của FR-OBS-001 (khối `healthcheck:` Docker cho `api`/`frontend`, `proxy_next_upstream`, `HealthIndicator` UI) **giữ ở Buổi 5**.
5. ✅ **Map dashboard với filter "cổng Nginx":** trong `Program.cs` (chỉ ở container `api`, không ở worker) gọi `app.MapHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [new NginxGateDashboardFilter(gateSecret)] })`. `NginxGateDashboardFilter : IDashboardAuthorizationFilter` chỉ cho qua khi header `X-Hangfire-Gate` **khớp secret** trong cấu hình (`Hangfire__DashboardGateSecret`, so sánh bằng `CryptographicOperations.FixedTimeEquals`). ⚠️ Bắt buộc thay filter mặc định: `LocalRequestsOnlyAuthorizationFilter` của Hangfire **từ chối mọi request đi qua proxy** (request đến từ IP container Nginx, không phải localhost) → dashboard sẽ luôn trả 401.
6. ✅ **Nginx Basic Auth (giữ nguyên cơ chế resolver động của Buổi 2):** thêm block riêng trước block `location ~ ^/(health|hangfire|scalar|openapi)`:
   ```nginx
   location /hangfire {
       auth_basic           "Hangfire Dashboard";
       auth_basic_user_file /etc/nginx/htpasswd;          # sinh bằng: htpasswd -B, KHÔNG commit (gitignore + gitleaks)
       proxy_set_header     X-Hangfire-Gate ${HANGFIRE_GATE_SECRET};  # nạp qua /etc/nginx/templates (envsubst)
       proxy_set_header     X-Forwarded-For $proxy_add_x_forwarded_for;
       proxy_pass           $api_upstream;
   }
   ```
   Chuyển `nginx.conf` sang thư mục `templates/` để image `nginx:alpine` tự `envsubst` secret lúc khởi động; bổ sung `HANGFIRE_GATE_SECRET`, `HANGFIRE_DASHBOARD_USER` vào `.env.example` (giá trị giả).
7. ✅ **Integration Test Harness — `tests/CulinaryBlog.API.IntegrationTests`:** project này **đã được tạo khung ở Buổi 2** (đã tham chiếu `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `Microsoft.AspNetCore.Mvc.Testing`, `coverlet`) nhưng **chưa có file kiểm thử nào** — buổi này lấp đầy nó, bổ sung gói thư viện `Respawn` và `public partial class Program;` ở API để `WebApplicationFactory` truy cập được. `CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, ghi đè connection string, chạy migration, tắt Hangfire server (`Hangfire:ServerEnabled=false`) và thay `IBackgroundJobClient`/`IFileStorageService` bằng bản giả ghi lại lời gọi. Kèm `Respawn` để dọn DB giữa các kiểm thử và helper `CreateClientAs(role)` phát JWT thật qua `ITokenService`.  Testcontainers **không chạy `docker/postgres/init.sql`** → mọi extension/DDL mà code cần phải nằm **trong migration** (điều kiện tiên quyết của D-18, Buổi 4).
8. ✅ **Trả nợ kiểm thử Buổi 2:** viết integration test cho toàn bộ endpoint Buổi 2 (`BAO_CAO_BUOI_2.md` §6 ghi tồn đọng): register/login (201/400/401/409/423), recipes list/detail, categories list/detail, files tải lên/xóa (201/400/401/403/204). Thêm sẵn **kiểm thử tái hiện lỗ hổng MT-34** (Admin gọi `GET /recipes` rồi Guest gọi lại cùng URL) đánh dấu `[Fact(Skip = "Lỗ hổng đã biết — retrofit D-4 ở Buổi 4")]` *(ban đầu ghi Buổi 6; D-4 kéo lên Buổi 4 ngày 29/09/2026)* để lỗ hổng được **ghi thành kiểm thử ngay hôm nay**, không phụ thuộc trí nhớ.
9. **Kiểm chứng:**
   - `http://localhost/hangfire` qua Nginx → hỏi Basic Auth → vào được dashboard, thấy job `WelcomeEmailJob` Succeeded; gọi thẳng `http://localhost:5000/hangfire` (bỏ qua Nginx) → **401**. Giảm `QueuePollInterval` xuống 1 giây ở môi trường dev (tồn đọng §6 của báo cáo Buổi 2).
   - `curl http://localhost/health` → `Healthy` kèm 3 entry; `docker compose stop minio` → `/health` báo `Degraded` còn `/health/ready` **vẫn 200**; `docker compose stop redis` → `/health/ready` **503**.
   - `dotnet test` chạy toàn bộ unit + architecture + integration test xanh trên máy có Docker.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Vì sao Dev 4 làm phần nền, còn phần theo module do chính chủ module làm:** lớp gốc exception, `IRepository<T>`, `UnitOfWork`, middleware là thứ **không thuộc module nào** — nếu ba dev cùng viết thì sẽ có ba kiểu. Ngược lại, "tên danh mục trùng là lỗi gì, trả mã gì" là kiến thức của Dev 3, và chính Dev 3 sẽ dùng lại nó ở Buổi 4–7. Chia như vậy giữ đúng mô hình **Feature Ownership** của §1: mỗi dòng code của module có một chủ duy nhất từ Buổi 2 đến Buổi 8, và phần việc của bốn người tương đương nhau (mỗi dev module: 3–6 exception + 1 repository + 1 file ánh xạ + ≥ 2 API; Dev 4: phần nền + middleware + 3 API health, cộng với dashboard và bộ integration test đã xong).

**Domain Exception không mang mã HTTP:** Domain là tầng lõi, dùng lại được cho một job Hangfire, một CLI, hay một bài kiểm thử đơn vị — những nơi không có khái niệm HTTP. Nếu `CategoryHasRecipesException` tự khai báo `StatusCode = 409` thì Domain đã biết mình đang chạy sau một web API. Để exception chỉ mang **mã nghiệp vụ** (`CATEGORY_DELETE_HAS_RECIPES`) và để tầng API quyết định mã HTTP, đúng hướng phụ thuộc của Clean Architecture (NFR-MAINT-004).

**Ánh xạ theo kiểu exception, mỗi module một file, quét tự động:** ba phương án được cân nhắc. **(A) Một `switch` lớn trong middleware** — đơn giản nhưng cả 3 dev cùng sửa một file mỗi buổi → xung đột merge liên tục, và quên thêm một nhánh là lỗi âm thầm rơi về 400. **(B) Ánh xạ theo chuỗi `Code`** — ngắn gọn, nhưng SRS có chỗ dùng **cùng một mã với hai mã HTTP** (`AUTH_GOOGLE_TOKEN_INVALID`: 400 và 401 — xem Dev 1 Phần 4), và đổi tên chuỗi không được trình biên dịch kiểm tra. **(C) Ánh xạ theo kiểu, một file/module, quét tự động (đã chọn)** — trình biên dịch kiểm tra tên lớp, mỗi dev chỉ sửa file của mình, và kiểm thử kiến trúc bảo đảm không lớp nào bị quên.

**Giữ `GlobalExceptionMiddleware` thay vì chuyển sang `IExceptionHandler`:** hai cơ chế cho kết quả như nhau (cùng bắt exception và ghi Problem Details qua `IProblemDetailsService`). Middleware đã chạy và có kiểm thử từ Buổi 2, và SRS §6.2 gọi đích danh tên `GlobalExceptionMiddleware`. Chuyển cơ chế lúc này chỉ đổi nơi đăng ký và phải viết lại kiểm thử, không thêm khả năng nào.

**`IRepository<T>` cố ý nhỏ — không có `Remove`, không trả `IQueryable`, không có `Update`:**
- **Không `Remove`:** mọi xóa trong hệ thống là **xóa mềm** (NFR-REL-003); xóa vật lý chỉ có một nơi hợp lệ là `PermanentPurgeJob` (Buổi 4, Infrastructure). Có `Remove` trong interface chung thì sớm muộn sẽ có handler gọi nhầm và mất dữ liệu **không khôi phục được**. Xóa mềm đi qua phương thức Domain (`category.SoftDelete()`).
- **Không `IQueryable`:** mở `IQueryable` cho tầng Application là để lọt chi tiết EF lên trên (`Include`, `AsSplitQuery`, `IgnoreQueryFilters`) — mỗi handler tự quyết định nạp gì, và repository chỉ còn là một lớp bọc rỗng. Truy vấn đặc thù là phương thức có tên trên repository của module (`GetByIdWithDetailsAsync`, `ExistsByNameAsync`).
- **Không `Update`:** entity lấy ra đã được EF theo dõi; đổi thuộc tính qua phương thức Domain rồi `SaveChangesAsync()` là đủ. Một `Update(entity)` sẽ đánh dấu **mọi cột** là đã sửa, kể cả cột người khác vừa đổi.

**Repository nằm trên Unit of Work (`unitOfWork.Categories`) thay vì tiêm từng repository riêng:** đây là đúng cách SRS mô tả luồng xử lý (`_unitOfWork.Categories.ExistsByNameAsync(...)` ở FR-CAT-003, `_unitOfWork.Recipes.AddAsync(...)` ở FR-RCP-003), và nó làm rõ ranh giới transaction: mọi repository lấy từ cùng một `IUnitOfWork` dùng chung một `DbContext`, nên một lần `SaveChangesAsync()` lưu tất cả hoặc không lưu gì. Chi phí là mỗi dev thêm một dòng vào `IUnitOfWork` — chấp nhận được vì chỉ là thêm dòng.

**Transaction phải đi qua execution strategy:** `CulinaryBlogDbContext` bật `EnableRetryOnFailure` từ Buổi 2 (Postgres khởi động chậm hơn API). Với cấu hình đó, EF Core **ném `InvalidOperationException`** nếu code tự `BeginTransaction` bên ngoài `IExecutionStrategy` — vì khi mất kết nối giữa chừng, EF không biết phải thử lại từ đâu. `ExecuteInTransactionAsync` gói đúng mẫu mà `DatabaseSeeder` đã chạy ổn định: `ChangeTracker.Clear()` ở đầu mỗi lần thử, transaction bên trong, commit ở cuối. Hệ quả mà mọi dev phải biết (ghi ngay trên chú thích của phương thức): **thao tác truyền vào có thể bị chạy lại**, nên phải tự đọc dữ liệu bên trong nó — Dev 2 (đổi ảnh chính, B3), Dev 1 (xoay vòng – rotation, B4), Dev 4 (purge job, B4) đều dùng đúng một cách này.

**Health endpoints kéo lên Buổi 3 thay vì giữ ở Buổi 5:** (1) FR-OBS-001 là mức **M** và vốn giao cho Dev 4 — chuyển buổi không đổi ranh giới phân công, không thêm yêu cầu ngoài SRS; (2) mục Verification §7 bắt `curl /health` trả `Healthy` ở cuối **mọi** buổi, nhưng tới trước Buổi 3 code **chưa có endpoint health nào** — kế hoạch đang tự mâu thuẫn, kéo lên là sửa luôn chỗ đó; (3) hai gói thư viện NpgSql/Redis health check đã được cài từ Buổi 2 mà chưa dùng. Phần gắn Docker `healthcheck:` + `proxy_next_upstream` + giao diện vẫn ở Buổi 5 vì nó cần sửa `Dockerfile` (cài `curl`) và `docker-compose.yml`, và nhóm đang dùng `/health/ready` ở Buổi 3 chỉ để kiểm chứng. FR-OBS-001 trải qua hai buổi (xem §5) — phần giao ở mỗi buổi đều dùng được ngay.

**MinIO chỉ làm `/health` thành `Degraded`, không làm `/health/ready` thất bại:** mất MinIO thì ảnh không tải lên được nhưng người dùng vẫn đọc và viết công thức được — đó là **suy giảm**, không phải **ngừng phục vụ**. Loại instance khỏi pool vì lý do này là phản ứng thái quá (giải trình đầy đủ ở Buổi 5 Dev 4).

**Vì sao dựng bộ integration test ngay Buổi 3 — đây là quyết định quan trọng nhất của Dev 4 trong buổi này.** Definition of Done của nhóm (§2.4) đòi `dotnet test` xanh ở **mọi** commit, NFR-MAINT-002 đòi *"mọi endpoint có ít nhất 1 happy path + 1 error case"*, và các buổi sau liên tục yêu cầu kiểm thử kiểu *"10 request `PUT` đồng thời chỉ 1 thành công"*. Kế hoạch 6 buổi cũ đặt bộ integration test ở Buổi 6–7 (theo cách đánh số của kế hoạch đó) — nghĩa là 5 buổi liền **không ai viết được integration test**, lỗi tích tụ rồi dồn cục vào cuối dự án. Buổi 2 đã làm sớm FR-JOB-001, nên phần thời gian trống ra của Dev 4 được dùng đúng vào chỗ có đòn bẩy lớn nhất: một hạ tầng mà cả 4 dev dùng mỗi ngày. Buổi này bộ integration test còn là nơi **kiểm chứng chính phần nền kiến trúc**: rollback transaction và dịch lỗi xung đột chỉ chứng minh được trên PostgreSQL thật.

**Testcontainers thay vì EF Core In-Memory:** provider In-Memory **không có** ràng buộc UNIQUE, không có FK cascade, không có `tsvector`, không có partial index, không có concurrency token thật, **không có transaction** — tức là đúng những thứ cần kiểm chứng nhất (MT-03, MT-05, MT-25, MT-09, và `ExecuteInTransactionAsync` của buổi này) đều biến mất. Test xanh trên In-Memory rồi vỡ trên PostgreSQL thật là kịch bản tệ nhất vì nó tạo ra **niềm tin sai**. Testcontainers chạy đúng `postgres:16-alpine` như production.

**Hai lớp bảo vệ cho dashboard, không phải một:** Basic Auth ở Nginx là lớp chính; header `X-Hangfire-Gate` là lớp thứ hai cho tình huống **có đường vào API không đi qua Nginx** — ở môi trường dev, cổng `5000:8080` của `api` được mở ra ngoài (§6.5), nên ai gõ thẳng `:5000/hangfire` sẽ vượt qua Basic Auth nếu chỉ có một lớp. Secret chỉ Nginx biết, được Nginx gắn vào **sau khi** người dùng qua Basic Auth. Chi phí: khoảng 15 dòng code; lợi ích: dashboard (vốn có quyền xóa/chạy lại mọi job) không bao giờ lộ ra dù cấu hình cổng thay đổi thế nào.

**Không làm lại những gì Buổi 2 đã xong:** PostgreSQL storage, `IEmailService`/`MailKitEmailService`, `WelcomeEmailJob` với backoff 1′/5′/30′, seam `IWelcomeEmailScheduler` → `HangfireWelcomeEmailScheduler` đều đã chạy thật (báo cáo Buổi 2 §2.1: *"Container `hangfire` gửi thành công → Mailhog"*). Giao lại việc đã hoàn thành chỉ vì kế hoạch cũ ghi vậy là lãng phí nguồn lực và dễ làm hỏng thứ đang chạy. Cũng vì vậy, `POST /files/upload` (Buổi 2) **không** được tính là API của Buổi 3.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) v1.0.0 quy định Hangfire Dashboard *"chỉ Admin, policy-protected"* trong khi hệ thống xác thực bằng **JWT Bearer stateless** và mục 5.2 ghi rõ *"không dùng cookie để tránh CSRF"*. (b) FR-JOB-001 mô tả email chào mừng chứa *"link kích hoạt email (nếu cần)"* trong khi **không có FR nào** đặc tả luồng xác nhận email — cần rà lại template của `WelcomeEmailJob` Buổi 2 để chắc chắn không còn link này. (c) Kế hoạch 6 buổi cũ tự mâu thuẫn: Definition of Done đòi kiểm thử ở mỗi commit, nhưng bộ integration test lại xếp ở Buổi 6 của kế hoạch đó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-39, MT-21**) chốt: Dashboard bảo vệ bằng **HTTP Basic Auth tại Nginx**; **bỏ link kích hoạt email**. Kế hoạch bổ sung lớp thứ hai (header gate) và đưa bộ integration test lên Buổi 3.
- **Tại sao chọn:** Hangfire Dashboard là **trang HTML mở trực tiếp trong trình duyệt** — khi Admin gõ `https://domain.com/hangfire`, trình duyệt gửi một request điều hướng thông thường, **không có cách nào đính kèm header `Authorization: Bearer`**. Mà SRS lại loại bỏ cookie. Kết quả ở v1.0.0: `IDashboardAuthorizationFilter` luôn thấy người dùng ẩn danh → **Admin không bao giờ vào được dashboard**, dù đây là quyền đã liệt kê ở mục 2.3. Ba phương án được cân nhắc: **(A)** chặn `/hangfire` khỏi internet, chỉ vào qua SSH tunnel — an toàn nhất nhưng bất tiện khi demo; **(B)** Basic Auth ở Nginx — ba dòng cấu hình, mở được từ trình duyệt, **không đụng gì tới JWT của ứng dụng**, đánh đổi là phải quản lý một bộ thông tin đăng nhập riêng; **(C)** cấp cookie riêng cho dashboard — mở lại cánh cửa cookie mà mục 5.2 vừa đóng, phải xử lý CSRF, thêm code chỉ để phục vụ một trang quản trị. Chọn **B cho môi trường demo/nộp bài**, khuyến nghị **A cho production thật**. Về link kích hoạt email: giữ nó sẽ tạo lời hứa suông với người dùng (bấm vào không có gì xảy ra) — tệ hơn là không có gì.
- **Vị trí interface repository — SRS §6.2 đặt ở Domain, kế hoạch đặt ở Application (quyết định 29/09/2026, đính chính E-9 ở §4.5):**
  - **Lý do đặt ở Application:** repository là **cổng ra hạ tầng** mà tầng use case cần, không phải khái niệm nghiệp vụ; giữ Domain chỉ gồm entity + exception + value object làm lõi dễ kiểm thử nhất. Code Buổi 2 cũng đã đặt `IUnitOfWork` và các read repository ở Application — đặt interface ghi ở Domain sẽ chia đôi các cổng truy cập dữ liệu sang hai tầng.
  - **Đánh đổi:** entity Domain không gọi được repository — điều vốn không nên làm.
  - **Việc cần làm:** SRS §6.2 và §2.1 của kế hoạch này được cập nhật theo quyết định, không để hai tài liệu nói hai điều.
- **Phụ lục B thiếu mã cho hai tình huống mà middleware phải trả:** 404 người dùng không tồn tại (FR-AUTH-008 A3) và 409 xung đột `RowVersion` của entity ngoài Recipe (mọi `BaseEntity` đều có concurrency token — §2.2). Theo đúng tiền lệ MT-46, bổ sung qua **CR-2026-04** (§4.5), không tự đặt mã ngoài SRS.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2312755_NguyenThangThieng_buoiso3` — ba commit, theo thứ tự:
```
fix(infra): pin minio image to quay.io registry after docker hub repo removal                                        # đã có — 6833412
feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness       # đã có — 67d29c0 (gồm D-11)
feat(infra): add base domain exception, unit of work, global exception middleware and health check APIs             # bước 1 → 4, merge ở mốc M1
```

## Kiểm chứng cuối Buổi 3

1. **Yêu cầu 1:**
   - `CulinaryBlog.Domain/Exceptions` có lớp gốc + 3 thư mục module;
   - kiểm thử kiến trúc xanh: mọi exception cụ thể có ánh xạ, Domain không tham chiếu assembly ngoài.
2. **Yêu cầu 2:**
   - handler ghi dữ liệu chỉ phụ thuộc `IUnitOfWork` / repository, không handler nào tham chiếu `CulinaryBlogDbContext`;
   - kiểm thử rollback transaction và kiểm thử dịch xung đột xanh trên PostgreSQL thật.
3. **Yêu cầu 3:**
   - `ProblemDetailsContractTests` xanh cho 400 / 401 / 403 / 404 / 409 / 423 / 500 / 502 / 503 — đều `application/problem+json`, đúng `type`;
   - lỗi 500 không lộ stack trace.
4. **Yêu cầu 4:** 12 endpoint mới (bảng đầu buổi), mỗi endpoint có ≥ 1 luồng thành công (happy path) + ≥ 1 trường hợp lỗi trong integration test; kiểm thử tay qua Scalar `/scalar` và UI.
5. **Chức năng:**
   - không còn chuỗi `Status422` / `status === 422` trong code;
   - form đăng ký hiện lỗi từng ô với 400;
   - 2 email cùng tiền tố sinh `UserName` có hậu tố số;
   - `/hangfire` được bảo vệ;
   - `curl http://localhost/health` trả `Healthy`.

---

# BUỔI 4 – HOÀN THÀNH TOÀN BỘ API: PHIÊN & TÀI KHOẢN, NỘI DUNG CÔNG THỨC, TRUY VẤN & TÌM KIẾM, VÒNG ĐỜI CÔNG THỨC & JOB NỀN

> **Mục tiêu buổi (yêu cầu của giảng viên, 29/09/2026): "Hoàn thành việc cài đặt tất cả API endpoints".** Cuối buổi, **45/45 endpoint** của SRS v1.2.2 Chương 8 được cài đặt **đúng hợp đồng** (mã HTTP, mã lỗi Phụ lục B, DTO, quy tắc cache), có integration test trên Testcontainers và hiện đủ trên Scalar. Kèm theo là hai job nền còn lại. **Giao diện** của các FR có API làm ở buổi này được dựng ở Buổi 5 → 7 trên hợp đồng API đã chốt.

## Yêu cầu của giảng viên cho Buổi 4 và kết quả kiểm tra kế hoạch cũ

**Kết quả kiểm tra bản kế hoạch trước ngày 29/09 — KHÔNG đạt**, vì bốn lý do:

1. **Thiếu 15 endpoint.** Buổi 4 cũ chỉ thêm 9 endpoint mới (refresh, 4 endpoint bước nấu, 3 endpoint nguyên liệu, search), nên hết Buổi 4 hệ thống mới có **29/44 endpoint**:

   | Endpoint còn thiếu sau Buổi 4 cũ | Buổi cũ | Dev cũ |
   |---|---|---|
   | `GET /auth/me`, `PATCH /auth/me` | 5 | Dev 1 |
   | `PUT /recipes/{id}`, `PATCH /recipes/{id}/publish`, `PATCH /recipes/{id}/unpublish` | 5 | Dev 2 |
   | `DELETE /recipes/{id}`, `GET /recipes/mine` | 6 | Dev 2 |
   | `GET /users`, `PATCH /users/{id}/status`, `GET /auth/sessions`, `DELETE /auth/sessions/{id}`, `POST /auth/sessions/revoke-all` | 7 | Dev 1 |
   | `PATCH /recipes/{id}/archive`, `PATCH /recipes/{id}/unarchive`, `GET /recipes/sitemap` | 7 | Dev 2 |

2. **Ba endpoint đã có nhưng vẫn sai hợp đồng SRS cho tới Buổi 5–6:**
   - `GET /recipes`: còn lọc theo danh tính và còn Output Cache (**D-4, D-6 — lỗ hổng MT-34**); còn tham số `sort=-field` thay vì `sortBy`/`sortOrder`; thiếu `maxPrepTime`, `minServings` (**D-10**, FR-SRCH-002/003).
   - `GET /recipes/{slug}`: trả 403 cho bản nháp — lộ sự tồn tại của nội dung chưa công bố (**D-4**).
   - `GET /categories/{slug}`: lọc theo danh tính (**D-5**), thiếu `sortBy`/`sortOrder`.

   Một endpoint trả sai mã, sai dữ liệu hoặc rò rỉ dữ liệu thì chưa thể gọi là "đã cài đặt xong".
3. **SRS thiếu một endpoint mà giao diện bắt buộc cần:** trang `/dashboard/recipes/[id]/edit` (SRS §5.1) phải nạp **toàn bộ** một công thức **Draft** (bước, nguyên liệu, ảnh, `rowVersion`). Nhưng `GET /recipes/{slug}` chỉ trả Published (MT-34), còn `GET /recipes/mine` chỉ trả bản tóm tắt. Cài xong đủ 44 endpoint thì Author vẫn **không sửa được bản nháp của chính mình**. Lỗ hổng này được bổ sung qua **CR-2026-04 (d)**: `GET /recipes/mine/{id}` — SRS v1.2.2 có **45 endpoint**.
4. **Chia việc lệch nặng** nếu chỉ dồn endpoint về đúng module cũ: Dev 2 sẽ phải làm 15 endpoint trong một buổi, Dev 4 làm 0.

**Cách làm mới:**
- **Buổi 4 thành buổi "API-first":** mọi endpoint còn lại, cùng mọi retrofit làm thay đổi hợp đồng của endpoint cũ (D-3, D-4, D-5, D-6, D-10, D-15), đều làm trong buổi này.
- **Giao diện dời sang Buổi 5 → 7:** các bước giao diện của Buổi 4 cũ (wizard bước 3–4, trang tìm kiếm, interceptor refresh, D-12) chuyển sang Buổi 5. *(Cập nhật 12/10/2026: giao diện Buổi 5 → 8 được phân bổ lại theo yêu cầu Lab 5 → Lab 8 — §0.3; cột "Buổi làm giao diện" dưới đây đã cập nhật.)*
- **Chia endpoint theo năng lực** để cân bằng:

| Dev | Nhóm việc Buổi 4 | Endpoint **mới** | Endpoint **sửa hợp đồng** | Việc nền kèm theo |
|---|---|---|---|---|
| **Dev 1** | Phiên & tài khoản — FR-AUTH-004, 006, 007, 008, 009 | 8 | — | D-2, `UseForwardedHeaders`, claim `sid` |
| **Dev 2** | Nội dung công thức — FR-RCP-009, 010, 004 | 8 | `POST /recipes` (mảng `steps?`/`ingredients?`) | D-16 |
| **Dev 3** | Truy vấn công khai & riêng tư — FR-SRCH-001 → 004, FR-RCP-011, MT-48, CR-2026-04 (d) | 4 | `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` | D-18, D-4, D-5, D-6, D-10 (phía API) |
| **Dev 4** | Vòng đời công thức & job nền — FR-RCP-005, 006, 007, FR-JOB-002, 003 | 5 | — | D-3, D-15, kiểm thử bề mặt API 45/45 |

**Kết quả sau buổi: 20 (B2 + B3) + 25 = 45/45 endpoint.**

**Phân vai "chủ API" và "chủ giao diện" từ Buổi 4.** Vai trò chủ giao diện theo module của §1 giữ nguyên. Hợp đồng giữa hai bên là SRS Chương 8 + integration test của chủ API. Chủ giao diện không sửa handler; thấy API sai thì báo chủ API.

| FR | Chủ API (Buổi 4) | Chủ giao diện | Buổi làm giao diện |
|---|---|---|---|
| FR-RCP-005 xuất bản/hủy xuất bản | Dev 4 | Dev 2 | 5 + 8 |
| FR-RCP-007 xóa mềm | Dev 4 | Dev 2 | 8 |
| FR-RCP-006 lưu trữ/khôi phục | Dev 4 | Dev 2 | 8 |
| FR-RCP-011 `/recipes/mine` + `/recipes/mine/{id}` | Dev 3 | Dev 2 | 5 (trang sửa) · 6 (tổng quan) · 7 (danh sách) |
| `GET /recipes/sitemap` (MT-48) | Dev 3 | Dev 3 | 8 |

**Vì sao chia theo năng lực chứ không ép theo module ở buổi này:**
- **Tải cân bằng:** mỗi dev 4–8 endpoint cộng một phần việc nền tương đương.
- **Mỗi nhóm việc là một khối gắn kết, không chia vụn:**
  - Dev 3 đã là chủ phần **đọc** công thức — bộ lọc, sắp xếp, phân trang của FR-SRCH vốn áp lên `GET /recipes` — nên nhận luôn các truy vấn riêng tư (`/mine`, `/mine/{id}`) và sitemap: cùng `RecipeFilterSpec`, cùng quy tắc cache.
  - Dev 4 đã là chủ **job dọn dữ liệu** FR-JOB-003 (xóa vĩnh viễn công thức đã xóa mềm > 30 ngày), nên nhận trọn **vòng đời** công thức: xuất bản → lưu trữ → xóa mềm → dọn. Máy trạng thái và xóa mềm nằm chung một người, **không bị cắt đôi** giữa hai dev.
- **Giảm phụ thuộc chéo:**
  - Dev 3 tự làm endpoint sitemap cho sitemap Next.js của chính mình (Buổi 8), không còn phải chờ Dev 2.
  - Dev 3 tự sửa `GetRecipesQuery` (trước đây là điểm "sửa code của Dev 2" ở Buổi 5 — §6).

**Phạm vi của "hoàn thành tất cả API endpoints" — cái gì KHÔNG thuộc Buổi 4:** rate limiting (NFR-SEC-003, mã 429 `RATE_LIMIT_EXCEEDED`) ở **Buổi 5 — Dev 1** (Lab 5; kế hoạch trước 12/10 đặt Buổi 6).
- Rate limiting là *chính sách middleware áp lên mọi endpoint*, không phải một endpoint.
- Nó phụ thuộc IP thật từ `UseForwardedHeaders` (bật ở Buổi 4), và phải kiểm chứng cùng CSP, route guard trong một lượt gia cố bảo mật.
- Mọi endpoint của Buổi 4 vẫn trả đủ các mã khác trong hợp đồng Chương 8; riêng dòng 429 của hợp đồng được đáp ứng ở Buổi 5 mà không cần sửa endpoint nào.

Tương tự, composite index + `EXPLAIN ANALYZE` (NFR-PERF-004) ở Buổi 5 là tối ưu hiệu năng, không đổi hợp đồng API.

**Tổ chức code để 4 người cùng sửa module Recipe mà không xung đột:**
- **`Recipe` thành `partial class`:** `Recipe.cs` (Dev 2 — nội dung) và `Recipe.Lifecycle.cs` (Dev 4 — trạng thái, xóa mềm).
- **Endpoint của `/recipes` tách ba file cùng gắn vào một `MapGroup("/recipes")`:**
  - `RecipesEndpoints.cs` — Dev 2: ghi nội dung;
  - `RecipeQueryEndpoints.cs` — Dev 3: mọi `GET`;
  - `RecipeLifecycleEndpoints.cs` — Dev 4: xuất bản/hủy xuất bản/lưu trữ/khôi phục/xóa.
- **Thứ tự merge chuẩn** Dev 4 → Dev 1 → Dev 3 → Dev 2. Ba migration của buổi (`B4_Recipe_PartialUniqueSlug` của Dev 4, `B4_Search_FTS` của Dev 3, `B4_Recipe_IngredientStep` của Dev 2) vào theo đúng thứ tự đó.

## DEV 1 — Phiên & Tài khoản: Refresh Rotation, Hồ sơ, Quản lý Tài khoản [Admin], Quản lý Phiên (API)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Phần API của năm FR:
- `FR-AUTH-004` Làm mới Access Token — Token Rotation & Reuse Detection;
- `FR-AUTH-006` Xem hồ sơ và `FR-AUTH-007` Cập nhật hồ sơ *(API kéo từ Buổi 5)*;
- `FR-AUTH-008` Quản lý tài khoản [Admin] và `FR-AUTH-009` Quản lý phiên đăng nhập *(API kéo từ Buổi 7)*.

Tổng cộng **8 endpoint mới**. **Kèm retrofit D-2** (512 → **256-bit**), claim **`sid`**, và bật **`UseForwardedHeaders`** để `CreatedByIp` ghi đúng IP thật. *(Phần Frontend — D-12 access token rời `localStorage` và interceptor single-flight — chuyển sang Buổi 5.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-2 + Domain:** trong `JwtTokenService` đổi `RefreshTokenBytes = 64` → **`32`** (**256-bit**), vẫn encode Base64URL.
   - Entity `RefreshToken` **đã có** `IsActive(DateTime utcNow)` (từ Buổi 2) và `Revoke(DateTime utcNow, string? replacedByTokenHash)` (từ Buổi 3 — đăng xuất); buổi này dùng lại, không thêm phương thức kiểm tra thứ hai.
   - `IUserRepository` (Buổi 3) thêm `GetRefreshTokenByHashAsync(tokenHash)` (tra **không** theo `UserId` — lúc refresh chưa có access token hợp lệ), `RevokeAllRefreshTokensAsync(userId, now)`, `GetActiveRefreshTokensAsync(userId, now)` và `GetRefreshTokenByIdAsync(userId, id)`.
   - `InvalidTokenException` (Buổi 3) thêm hai factory `RefreshTokenExpired()`, `RefreshTokenRevoked()` — cùng lớp, cùng ánh xạ 401, không cần sửa middleware.
2. **CQRS — `RefreshTokenCommand` (cây quyết định đầy đủ):**
   - Hash chuỗi nhận được → tra `TokenHash`. Không thấy → `InvalidTokenException` → **401 `AUTH_TOKEN_INVALID`**.
   - `ExpiresAt <= UtcNow` → `InvalidTokenException.RefreshTokenExpired()` → **401 `AUTH_REFRESH_TOKEN_EXPIRED`**.
   - **`RevokedAt != null` → REUSE DETECTED:**
     - gọi `RevokeAllRefreshTokensAsync(token.UserId)` (**thu hồi – revoke toàn bộ họ token – token family**) và `SaveChangesAsync()` **trước khi** ném lỗi;
     - ghi `Log.Warning("SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {Ip}")`;
     - rồi `InvalidTokenException.RefreshTokenRevoked()` → **401 `AUTH_REFRESH_TOKEN_REVOKED`**.
   - `user.IsActive == false` → `AccountDisabledException` → **403 `AUTH_ACCOUNT_DISABLED`** *(bước này khiến mã lỗi ở Phụ lục B thực sự được sinh ra — xem MT-22)*.
   - Hợp lệ → trong **một transaction** (`unitOfWork.ExecuteInTransactionAsync` — nạp lại token bên trong thao tác vì execution strategy có thể chạy lại): thu hồi token cũ với `ReplacedByTokenHash = SHA256(newToken)`, tạo bản ghi token mới, phát `AuthResponseDto`.
3. **Claim `sid` cho mọi access token** *(kéo từ Buổi 7 — điều kiện của FR-AUTH-009)*: trong `AuthResponseFactory` (dùng chung cho register/login/Google/refresh), gắn claim **`sid` = `Id` của bản ghi `RefreshToken`** được phát cùng cặp. Token phát trước khi triển khai không có `sid` — chúng tự hết hạn sau tối đa 15 phút, nên không cần di trú gì.
4. **Lấy IP thật — `UseForwardedHeaders` (API đúng của .NET 10):** Nginx đã gửi `X-Forwarded-For` từ Buổi 2; phía API bật:
   ```csharp
   builder.Services.Configure<ForwardedHeadersOptions>(o =>
   {
       o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
       o.KnownIPNetworks.Clear();
       o.KnownProxies.Clear();
       foreach (var cidr in builder.Configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? ["172.16.0.0/12"])
           o.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(cidr));   // dải mạng Docker nội bộ, cấu hình được
   });
   app.UseForwardedHeaders();   // đặt ĐẦU pipeline — trước Authentication và (Buổi 5) RateLimiter
   ```
   ⚠️ Dùng **`KnownIPNetworks`** + `System.Net.IPNetwork`, **không dùng `KnownNetworks`** + `Microsoft.AspNetCore.HttpOverrides.IPNetwork` như nhiều tài liệu cũ — hai API này đã bị đánh dấu **lỗi thời trên .NET 10**, và repo bật `TreatWarningsAsErrors` nên dùng chúng là **gãy build**. `CreatedByIp` của mọi refresh token (kể cả token phát lúc đăng ký/đăng nhập từ Buổi 2) từ đây ghi đúng IP người dùng.
5. **FR-AUTH-006/007 — `GetCurrentUserQuery`, `UpdateProfileCommand`** *(kéo từ Buổi 5)*:
   - **Xem hồ sơ:** dùng `ICurrentUser` (Buổi 2) lấy `UserId`; trả `UserDto { id, email, displayName, avatarUrl, bio, roles }`.
     - **Không bao gồm** `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `UserName`, `emailConfirmed`.
     - Token hợp lệ nhưng người dùng không còn tồn tại → **401** (không phải 404 — MT-33.9).
   - **Cập nhật hồ sơ:** PATCH partial `{ displayName?, avatarUrl?, bio? }`.
     - Validator: `displayName` **2–100**, `bio` ≤ **1000**, `avatarUrl` là URL ≤ 500 ký tự **và phải trỏ về đúng bucket MinIO của hệ thống** (`MinioOptions.PublicBaseUrl`).
     - **`Email` và `UserName` không cho đổi** ở endpoint này.
     - Ghi qua `IIdentityService` (Buổi 3 — ghi người dùng luôn qua `UserManager`).
6. **FR-AUTH-008 — `GetUsersQuery`, `SetUserStatusCommand { UserId, IsActive, Reason? }`** *(kéo từ Buổi 7)*, chỉ Admin (`AdminPolicy`):
   - **Danh sách:** phân trang `page`, `pageSize`, `search?` (khớp một phần email/displayName), `isActive?` → `PagedResult<UserAdminDto { id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }>`, **không cache**. Truy vấn đọc qua `IUserRepository`.
   - **Khóa/mở khóa:**
     - Admin tự khóa chính mình → **403** (MT-52);
     - người dùng không tồn tại → `UserNotFoundException` (Buổi 3) → **404 `AUTH_USER_NOT_FOUND`** (CR-2026-04);
     - khi `isActive = false`: gán cờ qua `IIdentityService` **và** gọi `unitOfWork.Users.RevokeAllRefreshTokensAsync(userId, now)` trong cùng một transaction — đây chính là "force revoke";
     - ghi audit log `Log.Warning("ADMIN ACTION: user {TargetUserId} deactivated by {AdminId}, reason: {Reason}")`.
7. **FR-AUTH-009 — `GetMySessionsQuery`, `RevokeSessionCommand`, `RevokeAllSessionsCommand`** *(kéo từ Buổi 7)*:
   - **Liệt kê** refresh token còn hiệu lực của người gọi → `SessionDto { id, createdAt, createdByIp, expiresAt, isCurrent }`, với `isCurrent = (id == claim sid)`; **không bao giờ trả `TokenHash`**.
   - **Thu hồi một phiên:** tìm theo `id` **và** `UserId == currentUserId`. Phiên của người khác trả **404** (không phải 403) để không lộ id; phiên đã thu hồi rồi vẫn trả 204.
   - **Thu hồi tất cả:** thu hồi mọi phiên, kể cả phiên hiện tại.
8. **API — 8 endpoint mới:**

   | Endpoint | Yêu cầu | Ghi chú |
   |---|---|---|
   | `POST /api/v1/auth/refresh` | **Không cần Bearer** | Access token đã hết hạn thì không dùng để xác thực được |
   | `GET /api/v1/auth/me` | `RequireAuthorization()` | |
   | `PATCH /api/v1/auth/me` | `RequireAuthorization()` | 200 trả `UserDto` mới |
   | `GET /api/v1/users` | `AdminPolicy` | `Cache-Control: no-store` |
   | `PATCH /api/v1/users/{id}/status` | `AdminPolicy` | |
   | `GET /api/v1/auth/sessions` | Bearer | `Cache-Control: no-store` |
   | `DELETE /api/v1/auth/sessions/{id}` | Bearer | |
   | `POST /api/v1/auth/sessions/revoke-all` | Bearer | |

9. **Integration test** (bộ integration test Buổi 3):
   - **Refresh:** refresh hợp lệ → cặp token mới, token cũ bị thu hồi; dùng lại token cũ sau khi xoay vòng → cả họ token bị thu hồi + 401 `AUTH_REFRESH_TOKEN_REVOKED`; token hết hạn → 401 `AUTH_REFRESH_TOKEN_EXPIRED`; request đi qua `X-Forwarded-For` từ mạng tin cậy → `CreatedByIp` ghi đúng IP đó.
   - **Hồ sơ:** `GET /auth/me` không có trường nhạy cảm nào trong JSON; PATCH với `avatarUrl` trỏ ra domain ngoài → 400.
   - **Quản lý tài khoản:** Author gọi `/users` → 403; khóa người dùng → refresh token của họ trả **403 `AUTH_ACCOUNT_DISABLED`**; Admin tự khóa mình → 403; người dùng không tồn tại → 404 `AUTH_USER_NOT_FOUND`.
   - **Phiên:** người dùng A thu hồi phiên của người dùng B → **404**; JSON của `/auth/sessions` không chứa `tokenHash`; `isCurrent` đúng với claim `sid`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Token Rotation** (mỗi lần refresh là cấp cặp mới và thu hồi cặp cũ) biến refresh token từ một *bí mật dài hạn 7 ngày* thành một *bí mật dùng một lần*. Nếu không xoay vòng, kẻ trộm được token sẽ dùng thoải mái suốt 7 ngày mà hệ thống không có cách nào biết.

**Reuse Detection** là phần khiến xoay vòng trở nên có giá trị thật. Sau khi xoay vòng, token cũ **không còn ai có lý do chính đáng để dùng nữa**. Nếu một token đã thu hồi bất ngờ được sử dụng, chỉ có hai khả năng:
- token đã bị đánh cắp và kẻ trộm đang dùng;
- chính người dùng đang dùng token mà kẻ trộm đã xoay vòng mất.

**Cả hai trường hợp đều có nghĩa là tài khoản đã bị xâm phạm**, nên phản ứng đúng là thu hồi toàn bộ họ token, buộc đăng nhập lại. Đây là cơ chế duy nhất trong thiết kế JWT stateless cho phép **phát hiện** việc token bị đánh cắp, thay vì chỉ chờ nó hết hạn. Thu hồi cả họ token phải được **lưu trước khi ném lỗi** — ném trước thì request kết thúc mà lệnh thu hồi chưa bao giờ được ghi xuống DB.

**Vì sao chỉ lưu hash:** token gốc (raw token) **không bao giờ** được ghi vào cơ sở dữ liệu; DB chỉ chứa `TokenHash = SHA-256(raw)`. Nếu cơ sở dữ liệu bị lộ, kẻ tấn công chỉ có trong tay các chuỗi hash — **không đảo ngược được thành token dùng được**. Không dùng bcrypt/Argon2 vì token là chuỗi ngẫu nhiên 256-bit, không có nguy cơ dictionary attack; SHA-256 là đủ và nhanh hơn nhiều.

**Vì sao `UseForwardedHeaders` bật ở buổi này:** chính buổi này là lúc mỗi lần refresh ghi thêm một bản ghi token kèm `CreatedByIp`, và danh sách phiên (FR-AUTH-009, cũng làm buổi này) hiển thị IP đó cho người dùng. Để muộn hơn nghĩa là ghi **IP của container Nginx** vào cột audit — dữ liệu sai được ghi vĩnh viễn.

**Vì sao gom FR-AUTH-006/007/008/009 vào cùng buổi với refresh:** cả bốn FR đều xoay quanh **một** cặp đối tượng — `ApplicationUser` và tập `RefreshToken` của họ — và dùng chung `IUserRepository` vừa mở rộng ở bước 1. Làm cùng lúc thì các phương thức `RevokeAll…`, `GetActive…` được thiết kế một lần cho cả bốn nhu cầu (khóa tài khoản, danh sách phiên, xoay vòng) thay vì vá dần qua ba buổi. Claim `sid` phải có trước khi danh sách phiên đánh dấu được "thiết bị này", nên nó theo luôn.

**Không cache danh sách người dùng và danh sách phiên** (`Cache-Control: no-store`): cả hai đều là dữ liệu riêng tư phụ thuộc danh tính người gọi — đúng nguyên tắc cách ly Public/Private của NFR-SEC-006 (giải trình đầy đủ ở Buổi 6 — Dev 2).

Giải trình thiết kế của hồ sơ (PATCH partial, chặn đổi email, `/auth/me` là nguồn sự thật về quyền) và của quản lý phiên (`sid`, 404 cho phiên của người khác) nằm ở **Buổi 7 — Dev 1**. Của quản lý tài khoản (revoke khi khóa, chặn tự khóa, đánh đổi 15 phút của JWT stateless) nằm ở **Buổi 8 — Dev 1** — nơi các FR đó hoàn tất giao diện (cập nhật 12/10/2026).

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:**
  - (a) FR-AUTH-004/005 dùng cờ **`IsRevoked = true`** và kiểm tra `IsRevoked == false`, nhưng bảng `RefreshToken` ở Chương 7.8 **chỉ có `RevokedAt timestamptz NULL`** — *"NULL = còn hiệu lực"* — **không có cột `IsRevoked`**.
  - (b) FR-AUTH-004 bước 5 gán `ReplacedByToken = newToken` (**token gốc**) trong khi Chương 7.8 và NFR-SEC-002 quy định chỉ lưu hash.
  - (c) Độ dài token 512-bit (FR) vs 128-bit (NFR).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-14, MT-15, MT-13**) chốt:
  - **bỏ hẳn `IsRevoked`**, token hợp lệ ⇔ `RevokedAt IS NULL AND ExpiresAt > NOW()`;
  - **`ReplacedByTokenHash = SHA256(newToken)`**;
  - **256-bit**.
- **Tại sao chọn:** hai cột cùng biểu diễn một trạng thái là **nguồn lỗi kinh điển**. Một nguồn sự thật duy nhất (`RevokedAt`) vừa gọn hơn, vừa mang thêm thông tin *khi nào* bị thu hồi phục vụ audit. 256-bit khớp đúng độ dài output SHA-256 và cột `TokenHash varchar(64)`.
- **Kéo API của FR-AUTH-006 → 009 lên Buổi 4 (29/09/2026):**
  - **Lý do:** yêu cầu "hoàn thành tất cả API endpoints" của giảng viên.
  - **Không đổi yêu cầu nào trong SRS**, chỉ đổi thời điểm cài đặt; giao diện ở Buổi 7 và Buổi 8 (cập nhật 12/10/2026).

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314236_HoangBinhQuan_buoiso4`.
```
feat(auth): complete FR-AUTH-004 refresh rotation and FR-AUTH-006/007/008/009 profile, admin user and session APIs
```

## DEV 2 — Nội dung Công thức: Nguyên liệu, Các bước & Cập nhật có Concurrency (API)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Phần API của:
- `FR-RCP-009` CRUD nguyên liệu (chiến lược 2 cột Quantity);
- `FR-RCP-010` CRUD các bước nấu (server tự đánh số + endpoint sắp xếp lại);
- `FR-RCP-004` Cập nhật công thức có Optimistic Concurrency `RowVersion` *(API kéo từ Buổi 5)*.

Tổng cộng **8 endpoint mới**. Kèm **kích hoạt hai mảng inline `steps?`/`ingredients?` của `POST /recipes`** (phần còn lại của FR-RCP-003) và **retrofit D-16**. *(Wizard bước 3–4 và trang sửa công thức chuyển sang Buổi 5.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **DB migration `B4_Recipe_IngredientStep` (+ retrofit D-16):** Buổi 2 đã có sẵn `Quantity decimal(10,3) NULL`, `Unit` cho phép NULL và `OrderIndex` — **không đổi các cột này**. Chỉ bổ sung:
   - cột **`QuantityText varchar(50) NULL`**;
   - `CHECK ("Quantity" IS NULL OR "Quantity" > 0)`.

   Riêng ràng buộc thứ tự bước phải **đổi từ unique index sang unique constraint**, vì PostgreSQL **chỉ cho constraint là `DEFERRABLE`, không cho index**:
   ```sql
   DROP INDEX "IDX_RecipeStep_Recipe_StepNumber";
   ALTER TABLE "RecipeSteps" ADD CONSTRAINT "UQ_RecipeStep_Recipe_StepNumber"
       UNIQUE ("RecipeId", "StepNumber") DEFERRABLE INITIALLY DEFERRED;
   ```
   Viết bằng `migrationBuilder.Sql(...)` và **bỏ `.IsUnique()`** khỏi `HasIndex` trong `RecipeConfiguration` (giữ index thường cho truy vấn), để model snapshot của EF không cố tạo lại unique index ở migration sau.
2. **Domain — nguyên liệu (dùng nền exception của Buổi 3):**
   - Thêm `IngredientQuantityRequiredException : RecipeDomainException` (`INGREDIENT_QUANTITY_REQUIRED`) và một dòng `.Map<IngredientQuantityRequiredException>(400)` trong `RecipeExceptionMappings`.
   - Mở rộng `Recipe.AddIngredient(...)` đã có thêm tham số `quantityText`; thêm `UpdateIngredient`, `RemoveIngredient`.
   - Invariant: **không được rỗng cả `Quantity`, `QuantityText` lẫn `Unit`** → `IngredientQuantityRequiredException` → **400**.
3. **Domain — các bước:**
   - `Recipe.AddStep(...)` **đã tự gán `StepNumber = Max + 1` từ Buổi 2** — giữ nguyên.
   - Thêm `Recipe.RemoveStep(stepId)` (xóa rồi **đánh số lại 1..N**).
   - Thêm `Recipe.ReorderSteps(IReadOnlyList<Guid> stepIds)`: tập id phải **khớp chính xác** tập bước hiện có — thiếu/thừa/trùng → `BusinessRuleViolationException(VALIDATION_ERROR)` → 400 — rồi gán `StepNumber = index + 1`.
   - **Mọi thao tác đánh số lại nằm trong một `SaveChangesAsync()` duy nhất**; nhờ constraint deferred ở bước 1, trạng thái trung gian không làm vỡ ràng buộc.
   - `CreateRecipeCommand` (Buổi 3) nhận thêm `steps?`/`ingredients?` và **chỉ gọi lại** `AddStep`/`AddIngredient` — không có validator thứ hai.
4. **CQRS + Validator nguyên liệu / bước:**
   - `Add/Update/DeleteIngredientCommand`: `name` **1–200**, `quantityText` ≤ 50, `unit` ≤ 50, `notes` ≤ 500 — theo §7.9.
   - `Add/Update/DeleteStepCommand`: `title` **1–200 bắt buộc**, `description` ≤ 2000, `timerMinutes` ≥ 0.
   - **`ReorderStepsCommand { RecipeId, StepIds[] }`**.
   - Handler nạp recipe bằng `unitOfWork.Recipes.GetByIdWithDetailsAsync` (Buổi 3); tất cả đi qua `RecipeAuthorizationHandler` và cài đặt `ICacheInvalidator` → xóa `recipe:{slug}` + tiền tố `recipes:list:`.
5. **FR-RCP-004 — `UpdateRecipeCommand`** *(kéo từ Buổi 5)*:
   - Body `{ title?, description?, categoryId?, prepTime?, cookTime?, servings?, difficulty?, instructions?, nutrition?, rowVersion }` — `rowVersion` nhận từ body **hoặc** header `If-Match`.
   - Validator dùng cùng bộ giới hạn như FR-RCP-003.
   - Handler gọi **`unitOfWork.Recipes.SetOriginalRowVersion(recipe, rowVersion)`** — phương thức mới trên `IRecipeRepository`, cài ở Infrastructure bằng `Entry(recipe).Property(r => r.RowVersion).OriginalValue = …` (handler không chạm `DbContext`).
   - Đổi dữ liệu qua domain method `recipe.Update(...)`, rồi `SaveChangesAsync()`.
   - **Không `try/catch`:** `DbUpdateConcurrencyException` đã được `RecipePersistenceExceptionTranslator` (Buổi 3) dịch sang `RecipeConcurrencyException` → **409 `RECIPE_CONCURRENCY_CONFLICT`**.
   - **Quy tắc slug:** chỉ sinh lại slug khi `recipe.Status == RecipeStatus.Draft`; recipe đã từng Published thì **slug khóa vĩnh viễn** dù đổi Title.
   - Trả `RecipeDetailDto` kèm **`rowVersion`** mới (base64) và header **`ETag`**.
   - Invalidate `recipe:{slug}` (cả slug cũ lẫn mới) + tiền tố `recipes:list:`.
6. **API — 8 endpoint mới + 1 endpoint mở rộng hợp đồng:**

   | Endpoint | Ghi chú |
   |---|---|
   | `POST /api/v1/recipes/{id}/ingredients` | |
   | `PUT /api/v1/recipes/{id}/ingredients/{ingId}` | |
   | `DELETE /api/v1/recipes/{id}/ingredients/{ingId}` | |
   | `POST /api/v1/recipes/{id}/steps` | body **KHÔNG chứa `stepNumber`** |
   | `PUT /api/v1/recipes/{id}/steps/{stepId}` | body **KHÔNG chứa `stepNumber`** |
   | `DELETE /api/v1/recipes/{id}/steps/{stepId}` | |
   | **`PATCH /api/v1/recipes/{id}/steps/reorder`** | nhận `{ stepIds: [...] }` |
   | **`PUT /api/v1/recipes/{id}`** | cập nhật có đồng thời |
   | `POST /api/v1/recipes` | nay nhận thêm hai mảng inline `steps?`/`ingredients?` |

   Tất cả nằm trong `RecipesEndpoints.cs` (file ghi nội dung của Dev 2).
7. **Kiểm thử:**
   - **Unit test:** đánh số lại sau khi xóa bước giữa; sắp xếp lại với mảng thiếu id → 400.
   - **Integration test:**
     - hoán đổi bước 2 và 3 bằng `reorder` → 200, không vỡ ràng buộc (kiểm chứng D-16 trên PostgreSQL thật);
     - nguyên liệu rỗng cả ba cột → 400 `INGREDIENT_QUANTITY_REQUIRED`;
     - `POST /recipes` kèm 10 nguyên liệu + 5 bước → 201, `StepNumber` 1..5;
     - `PUT` với `rowVersion` cũ → **409 `RECIPE_CONCURRENCY_CONFLICT`**;
     - `PUT` đổi tên recipe Draft → slug đổi; đổi tên recipe đã từng Published → slug giữ nguyên;
     - người không phải chủ → 403 `RECIPE_FORBIDDEN`.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình chiến lược 2 cột `Quantity` / `QuantityText`:** nấu ăn tiếng Việt đầy những định lượng không quy ra số được ("1/2 muỗng", "nửa củ", "vừa đủ", "1–2 quả"). Ba phương án:
- **Chỉ `decimal`** — tính toán được nhưng **mất nguyên văn**.
- **Chỉ `varchar`** — nhập tự do nhưng **giết chết mọi khả năng tính toán** và làm `recipeIngredient` trong JSON-LD thành chuỗi vô nghĩa.
- **Hai cột song song (đã chọn)** — giữ được cả hai.

Quy tắc: hiển thị ưu tiên `QuantityText`, không có thì format từ `Quantity` + `Unit`; scale khẩu phần chỉ áp dụng cho nguyên liệu có `Quantity`.

**Giải trình Server tự gán `StepNumber`:** Client **không bao giờ** gửi `stepNumber`, kể cả ở POST lẫn PUT — ràng buộc `UNIQUE (RecipeId, StepNumber)` sẽ bị vi phạm ngay khi hai request thêm bước chạy gần như đồng thời, và kết quả là **HTTP 500**. Muốn đổi thứ tự thì dùng `reorder` — Server nhận **ý định** ("thứ tự mới là mảng id này") chứ không nhận **giá trị cột**: API phơi bày *hành động nghiệp vụ*, không phơi bày *chi tiết lưu trữ*.

**Vì sao D-16 là điều kiện tiên quyết, không phải tối ưu:** unique **index** được PostgreSQL kiểm tra **ngay sau từng câu `UPDATE`**; `DEFERRABLE` chỉ tồn tại cho **constraint**. Bỏ qua chi tiết này thì sắp xếp lại **không bao giờ chạy được** (`23505` ở câu `UPDATE` đầu tiên → 500), dù code Domain hoàn toàn đúng.

**Vì sao `PUT /recipes/{id}` về tay Dev 2 ở buổi này (không đợi Buổi 5):** cập nhật công thức và CRUD bước/nguyên liệu cùng thao tác trên **một** aggregate, cùng `GetByIdWithDetailsAsync`, cùng quy tắc phân quyền và cùng chiến lược cache. Làm chung một buổi thì `recipe.Update(...)`, `SetOriginalRowVersion` và bộ kiểm thử đồng thời được viết một lần. Giải trình Optimistic Concurrency, `ETag`, khóa slug sau xuất bản nằm ở **Buổi 5 — Dev 2** (nơi trang sửa công thức dùng chúng).

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:**
  - (a) Chương 7.4 ghi `Quantity` cho phép NULL nhưng FR-RCP-009 đòi **bắt buộc**, còn Chương 8 ghi `quantity?`, `unit?` — ba chỗ ba kiểu.
  - (b) FR-RCP-010 quy định Server tự gán `StepNumber` nhưng Chương 8 cho client gửi `stepNumber`.
  - (c) Tên trường lệch: `SortOrder` vs `OrderIndex`; `DurationMinutes` vs `TimerMinutes`.
  - (d) `RecipeStep.Title` là `NOT NULL` nhưng body POST bước không có `title`.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-04, MT-03, MT-20.4, MT-20.5, MT-20.7, MT-41.7**) chốt:
  - hai cột `Quantity` + `QuantityText`, ràng buộc mềm "không rỗng cả ba";
  - Server toàn quyền gán `StepNumber` + endpoint `reorder`;
  - thống nhất `orderIndex`, `timerMinutes`;
  - `title` bắt buộc.
- **Tại sao chọn:** điểm (d) làm **mọi request POST bước thất bại ở tầng DB** — loại lỗi chỉ lộ ra khi chạy thật. Tên trường lệch khiến request từ FE bị bind thành `null` **âm thầm**. Chốt theo tên trong schema vì schema là thứ khó đổi nhất.
- **Kéo API FR-RCP-004 lên Buổi 4 (29/09/2026)** theo yêu cầu "hoàn thành tất cả API endpoints"; giao diện trang sửa ở Buổi 5.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2312758_NguyenHongPhucTho_buoiso4`.
```
feat(recipes): complete FR-RCP-009/010 ingredients & server-assigned steps with deferrable reorder and FR-RCP-004 rowversion update API
```

## DEV 3 — Truy vấn Công thức: Tìm kiếm Toàn văn, Lọc/Sắp xếp/Phân trang, Cách ly Public/Private & Sitemap (API)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Phần API của:
- `FR-SRCH-001` Tìm kiếm toàn văn tiếng Việt;
- `FR-SRCH-002/003/004` Lọc, sắp xếp, phân trang *(API kéo từ Buổi 5)*;
- `FR-RCP-011` `GET /recipes/mine` *(kéo từ Buổi 6)*;
- **`GET /recipes/mine/{id}`** *(mới — CR-2026-04 d)*;
- `GET /recipes/sitemap` *(MT-48, kéo từ Buổi 7)*.

Tổng cộng **4 endpoint mới** và **3 endpoint sửa hợp đồng**: `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}`. **Kèm retrofit D-18, D-4, D-5, D-6 và D-10 (phía API)** — trong đó **D-4, D-5 là lỗ hổng bảo mật MT-34**, nay được vá **sớm hai buổi** so với kế hoạch cũ. *(Trang `/search`, `FilterPanel` và D-10 phía FE chuyển sang Buổi 5.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Làm bước 4 trước tiên (vá MT-34),** rồi mới tới tính năng mới — lỗ hổng rò rỉ bản nháp đang tồn tại trên `develop` từ Buổi 2.
2. **DB migration `B4_Search_FTS` — DDL đặc thù PostgreSQL, thứ tự bắt buộc:** trong `Up()`, các lệnh `migrationBuilder.Sql(...)` sau phải đứng **trước** lệnh `AddColumn` do EF sinh ra (hàm phải tồn tại trước khi cột generated tham chiếu tới nó):
   ```sql
   CREATE EXTENSION IF NOT EXISTS unaccent;   -- idempotent: init.sql đã tạo trong Docker,
   CREATE EXTENSION IF NOT EXISTS pg_trgm;    -- nhưng Testcontainers/môi trường khác KHÔNG chạy init.sql

   -- unaccent() mặc định KHÔNG immutable → không dùng trực tiếp trong generated column
   CREATE OR REPLACE FUNCTION unaccent_immutable(text)
   RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
   LANGUAGE sql IMMUTABLE STRICT;
   ```
   - Khai báo cột qua EF Core: `.HasComputedColumnSql("to_tsvector('simple', unaccent_immutable(coalesce(\"Title\",'') || ' ' || coalesce(\"Description\",'')))", stored: true)` → **generated column `STORED`, không dùng trigger**.
   - Tạo **GIN index** `IDX_Recipe_Search`; `Down()` xóa theo thứ tự ngược lại.
   - **Retrofit D-18:** xóa khối tạo `vietnamese_unaccent` khỏi `docker/postgres/init.sql` (chỉ giữ hai dòng `CREATE EXTENSION`) để hệ thống chỉ có **một** cơ chế FTS.
   - Generated column tự tính cho toàn bộ 100 recipe seed (CR-2026-03) ngay khi migration chạy — không cần script backfill.
3. **Application — `RecipeFilterSpec`, `SortMapper`, phân trang** *(kéo từ Buổi 5)*:
   - **`RecipeFilterSpec`:** **một** specification dùng chung cho `GetRecipesQuery`, `SearchRecipesQuery`, `GetCategoryBySlugQuery` và `GetMyRecipesQuery`, gồm `categoryId?`, `difficulty?`, `maxCookTime?`, `maxPrepTime?`, `minServings?` — kết hợp bằng **AND**. `difficulty` phải thuộc enum **4 giá trị** `{Easy, Medium, Hard, Expert}` → ngoài enum thì **400**.
   - **`SortMapper`:** danh sách cho phép dictionary 5 trường `createdAt`, `publishedAt`, `title`, `cookTime`, `prepTime` (mã nguồn ở Buổi 5 — Dev 3).
     - `sortBy` ngoài danh sách cho phép → **400 `VALIDATION_ERROR`**; `sortOrder ∈ {asc, desc}`; mặc định `sortBy=createdAt&sortOrder=desc`.
     - **Tuyệt đối không ghép chuỗi SQL từ `sortBy`.**
     - **Retrofit D-10 (API):** xóa `RecipeSortParser` và enum `RecipeSortField`; `GET /recipes` đổi `string? sort` thành `sortBy`/`sortOrder` và nhận thêm `maxPrepTime`, `minServings`; `GET /categories/{slug}` nhận thêm `sortBy`/`sortOrder`.
   - **Phân trang:** `page` ≥ 1 (mặc định 1), **`pageSize` mặc định 12**, max 50 → sai thì 400; `PagedResult<T>` đủ `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`.
4. **Retrofit D-4 + D-5 + D-6 — đóng lỗ hổng MT-34** *(kéo từ Buổi 6)*:
   - **`RecipeReadRepository`:** xóa cơ chế `RecipeVisibility` (Guest/Author/Admin) → **chỉ `Status == Published` cho mọi người gọi**. `GetRecipesQuery`, `GetRecipeBySlugQuery`, `GetCategoryBySlugQuery` **không inject `ICurrentUser` nữa**.
   - **`GetRecipeBySlugQuery`:** Draft/Archived đổi từ **403 `RECIPE_FORBIDDEN` → 404** — ném `RecipeNotFoundException` (Buổi 3), cùng thông điệp như slug không tồn tại.
   - **D-6:** xóa `OutputCachePolicies.cs`, `AddCulinaryOutputCache` và hai lời gọi `.CacheOutput(...)`; chuyển sang `ICacheable`:

     | Khóa cache | TTL |
     |---|---|
     | `recipes:list:{queryHash}` | 2 phút |
     | `recipe:{slug}` | 5 phút |
     | `categories:detail:{slug}:{queryHash}` | 2 phút |

   - **Gỡ `Skip`** khỏi kiểm thử tái hiện MT-34 mà Dev 4 viết sẵn ở Buổi 3 và cập nhật `GetRecipeBySlug_DraftAsGuest_Returns403ForNow` thành 404 — hai kiểm thử phải chuyển sang xanh.
5. **CQRS — `SearchRecipesQuery : ICacheable`:**
   - Validate `q` ≥ 2 ký tự → sai thì **400 `VALIDATION_ERROR`**.
   - Sanitize term (loại `& | ! ( ) : *`) và build chuỗi tiền tố `pho:* & bo:*`.
   - `.Where(r => r.SearchVector.Matches(EF.Functions.ToTsQuery("simple", query)))` + `RecipeFilterSpec`; `ORDER BY ts_rank(...) DESC`; **chỉ `Status == Published`**; trả `relevanceScore`.
   - `CacheKey = search:{queryHash}`, `Expiration = 1 phút`.
6. **FR-RCP-011 — `GetMyRecipesQuery`** *(kéo từ Buổi 6)*:
   - **Không** cài đặt `ICacheable`; lọc `r.AuthorId == targetAuthorId` (`authorId` khi người gọi là Admin, ngược lại `ICurrentUser.UserId`).
   - Author truyền `authorId` của người khác → **403 `RECIPE_FORBIDDEN`**.
   - `status` ∈ `{Draft, Published, Archived}` (ngoài → 400); dùng chung `SortMapper` + phân trang.
   - Endpoint gắn **`Cache-Control: no-store`**.
7. **CR-2026-04 (d) — `GetMyRecipeByIdQuery`:**
   - Nạp recipe theo `id` ở **mọi trạng thái** qua `unitOfWork.Recipes.GetByIdWithDetailsAsync` (Buổi 3), rồi kiểm quyền bằng `RecipeAuthorizationHandler` (chủ sở hữu hoặc Admin).
   - Recipe không tồn tại **hoặc thuộc người khác** → **404 `RECIPE_NOT_FOUND`** (không phải 403, để không dò được id).
   - Trả `RecipeDetailDto` **kèm `rowVersion`** và header **`ETag`** + **`Cache-Control: no-store`**. Đây là nguồn dữ liệu của trang sửa công thức (`PUT` của Dev 2 dùng lại `rowVersion` này).
8. **Endpoint sitemap (MT-48)** *(kéo từ Buổi 7)*: `GetRecipeSitemapQuery : ICacheable` trả `{ slug, updatedAt }[]` của mọi recipe Published, không phân trang, TTL 1 giờ. Invalidate khi xuất bản/hủy xuất bản/lưu trữ/xóa: Dev 4 thêm khóa `recipes:sitemap` vào `ICacheInvalidator` của các command vòng đời.
9. **API** — tất cả `GET` của `/recipes` gom vào `RecipeQueryEndpoints.cs`, chuyển hai `GET` cũ ra khỏi `RecipesEndpoints.cs`:

   | Endpoint | Loại thay đổi |
   |---|---|
   | `GET /api/v1/recipes/search?q&page&pageSize&categoryId&difficulty` | mới |
   | `GET /api/v1/recipes/mine?status&page&pageSize&sortBy&sortOrder&authorId` | mới |
   | `GET /api/v1/recipes/mine/{id:guid}` | mới |
   | `GET /api/v1/recipes/sitemap` | mới |
   | `GET /api/v1/recipes` | sửa hợp đồng |
   | `GET /api/v1/recipes/{slug}` | sửa hợp đồng |
   | `GET /api/v1/categories/{slug}` | sửa hợp đồng |

   Segment literal (`search`, `mine`, `sitemap`) luôn thắng `{slug}` bất kể thứ tự khai báo; cả ba đã nằm trong danh sách slug dành riêng (Buổi 3).
10. **Integration test:**
    - **Tìm kiếm:** gõ **"pho"** (không dấu) tìm được **"Phở bò"**; `q` = 1 ký tự → 400.
    - **Lọc / sắp xếp:** `?sort=-title` → 400; `?sortBy=title&sortOrder=desc` → 200 đúng thứ tự; `difficulty=Expert` → 200; `pageSize=51` → 400.
    - **MT-34:** Admin gọi `GET /recipes` rồi Guest gọi lại cùng URL → Guest **không** thấy Draft; Draft qua `/recipes/{slug}` → 404.
    - **Công thức của tôi:** `/recipes/mine` không có token → 401; Author truyền `authorId` người khác → 403; response có `Cache-Control: no-store`.
    - **Chi tiết riêng tư:** `/recipes/mine/{id}` của người khác → 404, của chính mình ở trạng thái Draft → 200 kèm `ETag`.
    - **Sitemap:** `/recipes/sitemap` chỉ chứa slug Published.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Generated column `STORED` thay vì trigger** là quyết định về tính đúng đắn: PostgreSQL **tự bảo đảm** `SearchVector` luôn khớp với `Title`/`Description` — không tồn tại đường code nào có thể cập nhật tiêu đề mà quên cập nhật vector. Generated column cũng khai báo được trọn vẹn trong `OnModelCreating`.

**Chọn `simple` + `unaccent` thay vì dictionary tiếng Việt tự build:**
- PostgreSQL 16 **không có sẵn** configuration `vietnamese`; tự build đòi image PostgreSQL riêng.
- `simple` + `unaccent` chạy ngay trên image gốc và đạt đúng yêu cầu bỏ dấu.
- Stemming gần như vô giá trị với tiếng Việt vốn không biến cách.
- Phương án `pg_trgm` + `ILIKE` bị loại vì mất `ts_rank` mà FR-SRCH-001 **bắt buộc** trả `relevanceScore`.

**Vì sao gỡ `vietnamese_unaccent` khỏi `init.sql` (D-18):** `init.sql` **không chạy** trong Testcontainers hay môi trường mới. Code tham chiếu tới nó là lỗi `text search configuration does not exist` — đúng loại lỗi MT-25 vừa sửa. Nguyên tắc: **mọi đối tượng schema mà code phụ thuộc phải sinh ra từ migration**.

**Vì sao vá MT-34 ở buổi này (sớm hơn kế hoạch cũ hai buổi) và làm trước tiên:**
- Đây là **lỗ hổng rò rỉ dữ liệu** đang chạy trên `develop`: một lượt truy cập của Admin nạp Draft của mọi Author vào cache, rồi mọi Guest gọi cùng URL đều đọc được.
- "Cài đặt xong tất cả API endpoints" mà `GET /recipes` vẫn rò dữ liệu thì không thể nghiệm thu.
- Và vì `/recipes/mine` — phần đối xứng riêng tư — cũng làm buổi này, việc gỡ lọc theo danh tính khỏi endpoint công khai **không làm mất đường nào** để Author xem bản nháp của mình.

Giải trình đầy đủ nguyên tắc *"cache nằm trước handler nên dữ liệu riêng tư không bao giờ được cache"* nằm ở **Buổi 7 — Dev 2**.

**Một `RecipeFilterSpec` cho bốn truy vấn:** bốn nơi tự viết bốn bộ lọc hơi khác nhau là điều gần như chắc chắn xảy ra khi bốn màn hình được làm ở bốn thời điểm. Dev 3 sở hữu **toàn bộ phía đọc** công thức từ buổi này nên không còn điểm phối hợp "sửa `GetRecipesQuery` của người khác" như kế hoạch cũ (§6). Giải trình danh sách cho phép, `sortBy`/`sortOrder`, `pageSize = 12` nằm ở **Buổi 5 — Dev 3**.

**Vì sao `GET /recipes/mine/{id}` trả 404 (không phải 403) cho công thức của người khác và nằm dưới `/mine`:**
- Nằm dưới tiền tố `/mine` thì mọi endpoint riêng tư của module Recipe có chung một dấu hiệu nhận biết: bắt buộc Bearer, `no-store`, không `ICacheable`. Quy tắc cách ly Public/Private vẫn đọc được từ chính URL.
- Trả 404 cho id của người khác theo đúng tiền lệ FR-AUTH-009 (phiên của người khác → 404): endpoint không trở thành công cụ dò id nào đang tồn tại.
- **Phương án bị loại — cho `GET /recipes/{slug}` trả Draft khi người gọi là chủ:** mở lại đúng lỗ hổng MT-34 (endpoint công khai lọc theo danh tính trong khi bị cache theo khóa công khai).

**Cache sitemap 1 giờ, xóa cache khi vòng đời thay đổi:** crawler gọi sitemap vài lần mỗi ngày, nhưng một truy vấn toàn bảng mỗi lần thì phí; 1 giờ đúng bằng chu kỳ tái sinh `app/sitemap.ts` (Buổi 8). Invalidate chủ động ở các command xuất bản/hủy xuất bản/lưu trữ/xóa giúp slug mới xuất hiện ngay ở lượt tái sinh kế tiếp.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ — ba lỗi chồng nhau ở FR-SRCH-001:**
  - (a) *"SearchVector (**computed column**) được tự động cập nhật bởi PostgreSQL **trigger**"* — hai cơ chế loại trừ nhau trong cùng một câu.
  - (b) `ToTsQuery("vietnamese", …)` trong khi PostgreSQL 16 không có configuration đó.
  - (c) Trigger cần raw SQL trong migration, trái CONS-006 đọc theo nghĩa đen.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-25**) chốt generated column `STORED` + `simple` + `unaccent_immutable`, và **diễn giải lại CONS-006** — DDL đặc thù PostgreSQL được phép trong migration.
- **Tại sao chọn:** lỗi (b) không sai lúc biên dịch, chỉ nổ **khi chạy thật** → tính năng tìm kiếm chết hẳn. `unaccent()` mặc định không `IMMUTABLE` nên hàm wrapper là **bắt buộc**.
- **SRS thiếu endpoint đọc bản nháp để sửa (CR-2026-04 d, §4.5):** phát hiện khi đối chiếu 44 endpoint của Chương 8 với 15 route giao diện của §5.1 — mọi route đều có API phục vụ, trừ `/dashboard/recipes/[id]/edit` với recipe chưa Published. Bổ sung **một** endpoint riêng tư thay vì nới quy tắc Public/Private của `GET /recipes/{slug}`.
- **Kéo API FR-SRCH-002/003/004, FR-RCP-011, MT-48 và retrofit D-4/D-5/D-6/D-10 lên Buổi 4 (29/09/2026)** theo yêu cầu "hoàn thành tất cả API endpoints". Composite index + `EXPLAIN ANALYZE` (NFR-PERF-004) vẫn ở Buổi 5 vì đó là tối ưu hiệu năng, không phải hợp đồng API.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314291_DoanHongTien_buoiso4`.
```
fix(search)!: close MT-34 draft leak; complete FR-SRCH-001..004 query APIs, FR-RCP-011 private my-recipes and sitemap source
```
> Dùng `fix!` (breaking) vì hành vi công khai thay đổi:
> - Admin/Author không còn thấy Draft ở `GET /recipes`;
> - Draft trả 404 thay vì 403;
> - `sort=-field` bị thay bằng `sortBy`/`sortOrder`.

## DEV 4 — Vòng đời Công thức (API) & Job Nền: Xuất bản/Hủy xuất bản, Lưu trữ/Khôi phục, Xóa mềm, Đổi kích thước ảnh, Dọn dữ liệu vĩnh viễn

**Phần 1 – Chức năng hoàn thành trong buổi:**
- Phần API của `FR-RCP-005` Xuất bản / Hủy xuất bản *(kéo từ Buổi 5)*, `FR-RCP-006` Lưu trữ / Khôi phục *(kéo từ Buổi 7)* và `FR-RCP-007` Xóa mềm *(kéo từ Buổi 6)* — **5 endpoint mới**, cùng máy trạng thái khép kín.
- `FR-JOB-002` Đổi kích thước ảnh Job + `FR-JOB-003` Dọn dữ liệu vĩnh viễn Job.
- **Kèm retrofit D-15** (máy trạng thái) và **D-3** (partial unique slug).
- **Test bề mặt API 45/45** — bằng chứng tự động cho yêu cầu của buổi.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain — `Recipe` thành `partial class`; vòng đời nằm ở `Recipe.Lifecycle.cs`:**
   - Hiện thực `Publish()`, `Unpublish()`, `Archive()`, `Unarchive()`, `SoftDelete()`.
   - Gom toàn bộ luật chuyển trạng thái vào **một** nơi duy nhất — `RecipeStatusTransitions`, dictionary tra cứu `(from, action) → to`:

     | Từ trạng thái | Hành động | Endpoint | Sang trạng thái |
     |---|---|---|---|
     | Draft | xuất bản | `PATCH /recipes/{id}/publish` | Published |
     | Published | hủy xuất bản | `PATCH /recipes/{id}/unpublish` | Draft |
     | Draft **hoặc** Published | lưu trữ | `PATCH /recipes/{id}/archive` | Archived |
     | Archived | **khôi phục** | `PATCH /recipes/{id}/unarchive` | **Draft** |

   - Mọi chuyển đổi **ngoài bảng** → `InvalidRecipeStatusException` (Buổi 3) → **409 `RECIPE_INVALID_STATE_TRANSITION`**.
   - **Retrofit D-15 trên `Recipe.Publish()` có sẵn:**
     - **giữ** hai điểm Buổi 2 đã làm đúng: kiểm tra `_steps.Count == 0 || _ingredients.Count == 0`, và `PublishedAt ??= utcNow` (chỉ gán lần đầu);
     - **sửa** (a): bỏ nhánh `if (Status == Published) return;` và nhánh cho Archived lọt qua — nay đi qua bảng chuyển trạng thái;
     - **sửa** (b): thiếu bước/nguyên liệu ném **`RecipePublishIncompleteException : RecipeDomainException`** (mới, `RECIPE_PUBLISH_INCOMPLETE` → **400**, thêm một dòng vào `RecipeExceptionMappings`), thay cho `BusinessRuleViolationException` tạm dùng từ Buổi 3.
2. **CQRS — `PublishRecipeCommand`, `UnpublishRecipeCommand`, `ArchiveRecipeCommand`, `UnarchiveRecipeCommand`, `DeleteRecipeCommand`:**
   - Tất cả nạp recipe bằng `unitOfWork.Recipes.GetByIdWithDetailsAsync`. ⚠️ Dùng `GetByIdAsync` thì hai collection rỗng và recipe đủ điều kiện cũng bị từ chối xuất bản.
   - Tất cả đi qua `RecipeAuthorizationHandler` (403 `RECIPE_FORBIDDEN`), không tìm thấy → 404 `RECIPE_NOT_FOUND`.
   - Tất cả cài đặt `ICacheInvalidator` → xóa `recipe:{slug}` + tiền tố `recipes:list:` + tiền tố `categories:detail:` + khóa `recipes:sitemap`.
   - `DeleteRecipeCommand` gọi `recipe.SoftDelete()`: **KHÔNG** xóa child entity (vô hình cùng recipe qua Global Query Filter) và **KHÔNG** đưa vào hàng đợi (enqueue) xóa file MinIO — việc đó thuộc `PermanentPurgeJob` ở bước 6.
3. **Retrofit D-3 — migration `B4_Recipe_PartialUniqueSlug`:**
   ```sql
   DROP INDEX "IDX_Recipe_Slug";
   CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;
   ```
   Đồng thời **bỏ `IgnoreQueryFilters()`** trong `RecipeRepository.SlugExistsAsync` (Buổi 3) — slug của recipe đã xóa mềm từ nay được dùng lại.
4. **API — 5 endpoint mới trong `RecipeLifecycleEndpoints.cs`:**

   | Endpoint | Thành công | Lỗi |
   |---|---|---|
   | `PATCH /api/v1/recipes/{id:guid}/publish` | 200 | 400 `RECIPE_PUBLISH_INCOMPLETE`; 403; 404; 409 |
   | `PATCH /api/v1/recipes/{id:guid}/unpublish` | 200 | 403; 404; 409 |
   | `PATCH /api/v1/recipes/{id:guid}/archive` | 200 | 403; 404; 409 |
   | `PATCH /api/v1/recipes/{id:guid}/unarchive` | 200 | 403; 404; 409 |
   | `DELETE /api/v1/recipes/{id:guid}` | 204 | 403; 404 |

5. **FR-JOB-002 — `ImageResizeJob(Guid imageId)`:**
   - Dùng **SixLabors.ImageSharp** tạo medium **800×600** và ảnh thu nhỏ (thumbnail) **300×300** (giữ tỉ lệ, crop giữa).
   - Tải lên cả hai lên MinIO cùng folder với ảnh gốc, cập nhật `MediumUrl`/`ThumbnailUrl` vào DB.
   - `[AutomaticRetry(Attempts = 3)]`; thất bại hết thử lại → giữ nguyên `null`, **ảnh gốc vẫn hiển thị bình thường**.
   - Móc vào `UploadRecipeImageCommand` của Dev 2 (Buổi 3) bằng `BackgroundJob.Enqueue`.
6. **FR-JOB-003 — `PermanentPurgeJob`:** recurring **`"30 3 * * *"` (03:30 UTC)**. Thân job:
   - Query `_db.Recipes.IgnoreQueryFilters().Where(r => r.IsDeleted && r.UpdatedAt < DateTime.UtcNow.AddDays(-30))` — **bắt buộc `IgnoreQueryFilters()`** vì Global Query Filter đã ẩn chính những bản ghi ta cần tìm.
   - Với mỗi recipe:
     - **(1)** thu thập toàn bộ URL ảnh (`RecipeImages`: original/medium/thumbnail, và `RecipeStep.ImageUrl`) **trước khi xóa**;
     - **(2)** xóa cứng recipe trong một transaction — FK `ON DELETE CASCADE` tự dọn Steps/Ingredients/Images. Job nằm ở Infrastructure nên thao tác thẳng trên `CulinaryBlogDbContext`: đây là **nơi duy nhất** được xóa vật lý (`IRepository<T>` cố ý không có xóa cứng — xem Buổi 3). Transaction mở **qua execution strategy** theo đúng mẫu `UnitOfWork.ExecuteInTransactionAsync`;
     - **(3)** **chỉ sau khi transaction commit thành công** mới gọi `IFileStorageService.DeleteAsync()` cho từng URL.
7. **Distributed lock + đăng ký:**
   - Job chạy trong container **`hangfire` (worker)**. Bọc thân `PermanentPurgeJob` bằng RedLock (`RedLock.net`) khóa `lock:purge-job`, TTL 10 phút, kèm `[DisableConcurrentExecution(600)]`. Không lấy được lock thì bỏ qua lượt và log Information.
   - Đăng ký: `RecurringJob.AddOrUpdate<PermanentPurgeJob>("permanent-purge", j => j.ExecuteAsync(), "30 3 * * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc })`; `[AutomaticRetry(Attempts = 2)]`; log số recipe đã dọn và số file đã xóa.
8. **Kiểm thử:**
   - **Unit test ma trận trạng thái:** **12 tổ hợp** `(3 trạng thái × 4 hành động)` — 5 hợp lệ cho đúng trạng thái đích, **7 còn lại ném `InvalidRecipeStatusException`** (assert đúng kiểu); kịch bản `publish → unpublish → publish lại` giữ nguyên `PublishedAt` lần đầu.
   - **Integration test vòng đời:**
     - xuất bản recipe thiếu nguyên liệu → 400 `RECIPE_PUBLISH_INCOMPLETE`;
     - xuất bản recipe Archived → 409;
     - khôi phục → Draft;
     - xóa recipe slug `pho-bo` → tạo recipe mới cùng tên → slug mới lại là `pho-bo` (D-3);
     - recipe đã xóa → `GET /recipes/{slug}` 404, và **file ảnh vẫn còn** trên MinIO giả.
   - **Integration test job:** seed recipe `IsDeleted = true, UpdatedAt = 40 ngày trước` → chạy `PermanentPurgeJob` trực tiếp → bản ghi biến mất **và** `IFileStorageService.DeleteAsync` được gọi đủ URL; `ImageResizeJob` với ảnh mẫu → `MediumUrl`/`ThumbnailUrl` được điền.
9. **Test bề mặt API — `ApiSurfaceTests` (bằng chứng cho yêu cầu của buổi):**
   - Test đọc toàn bộ route thật của ứng dụng từ `EndpointDataSource` và so với **danh sách 45 cặp (method, route) của SRS v1.2.2 Chương 8**, lưu thành hằng số trong kiểm thử kèm số mục SRS.
   - Test đỏ nếu **thiếu** endpoint nào của SRS **hoặc thừa** endpoint `/api/v1` nào ngoài SRS.
   - Ba endpoint health đi ngoài tiền tố `/api/v1` và được liệt kê riêng.
10. **Kiểm chứng tay:** trên `/hangfire` trigger `permanent-purge`, xác nhận bản ghi biến mất khỏi DB **và** file biến mất khỏi MinIO console; tải ảnh lên qua Scalar → thấy đủ 3 biến thể ảnh trong MinIO. *(Hiển thị `ThumbnailUrl` trong `RecipeCard` chuyển sang Buổi 5.)*

**Phần 3 – Định hướng & Lý do thiết kế:**
**Vì sao máy trạng thái và xóa mềm về tay Dev 4, cùng buổi với purge job:** bốn hành động trạng thái + xóa mềm + dọn vĩnh viễn là **một chuỗi vòng đời liền mạch** của cùng một bản ghi: Draft ⇄ Published → Archived → (xóa mềm) → sau 30 ngày bị job xóa hẳn. Tách chuỗi này cho hai người sẽ tạo ra đúng loại khe hở mà kế hoạch đã phải sửa hai lần:
- Archived "vào được không ra được" (MT-35);
- xóa mềm nhưng không ai dọn (MT-05).

Gom vào một người, cùng một buổi, giúp bảng `RecipeStatusTransitions`, `SoftDelete()`, partial unique index (D-3) và truy vấn `IgnoreQueryFilters()` của purge job được thiết kế **nhất quán một lần**. Đồng thời cân bằng tải: Dev 2 giữ phần nội dung (8 endpoint), không phải gánh 15 endpoint.

**`partial class` và ba file endpoint thay vì một file mỗi module:**
- Buổi này có ba dev cùng sửa module Recipe.
- Một file `Recipe.cs` và một file `RecipesEndpoints.cs` cho ba người là xung đột merge chắc chắn xảy ra, vì cùng vùng code và cùng ngày.
- Chia file theo **trách nhiệm** (nội dung / truy vấn / vòng đời) giữ nguyên một aggregate, một route group `/recipes`, nhưng mỗi người chỉ sửa file của mình.

**Gom luật chuyển trạng thái vào một dictionary** thay vì rải `if` trong từng method: thêm một trạng thái mới (ví dụ `PendingReview`) chỉ là thêm dòng, và bảng đó là tài liệu sống của máy trạng thái. FE (Buổi 8) xuất chính bảng này thành hằng số để chỉ hiện nút hợp lệ.

**Test đủ 12 tổ hợp thay vì chỉ đường hạnh phúc:** lỗi ở v1.0.0 không phải "code sai" mà là "có một chuyển đổi không tồn tại và không ai nhận ra" — chỉ lộ ra khi liệt kê **toàn bộ** ma trận.

**`ApiSurfaceTests` biến "đã cài đặt tất cả API endpoints" thành điều kiện kiểm được:**
- "Đủ 45 endpoint" không còn là việc đếm tay trên Scalar, mà là một kiểm thử chạy ở mọi commit từ nay tới Buổi 8.
- Chiều "thừa" quan trọng không kém chiều "thiếu": một endpoint không có trong SRS là yêu cầu không truy vết được (nguyên tắc §4.2).
- Người thêm endpoint mới buộc phải đi qua Change Request trước.

**Resize bất đồng bộ:** đổi kích thước một ảnh 5MB tốn 1–3 giây CPU; làm đồng bộ thì 5 ảnh là 15 giây chờ. Quan trọng hơn, **đổi kích thước thất bại không làm hỏng việc tải lên** — tách việc bắt buộc khỏi việc nên có.

**"Commit DB trước, xóa file sau" trong purge job là bắt buộc:**
- **Làm ngược lại (xóa file trước):** nếu transaction DB rollback thì recipe trỏ tới file không còn tồn tại — hỏng vĩnh viễn.
- **Làm đúng thứ tự:** trường hợp xấu nhất chỉ là **file mồ côi**, vô hại.
- **Nguyên tắc chung:** khi phối hợp hai hệ thống không có transaction chung, sắp xếp sao cho lỗi rơi vào phía **rác thừa**, không phải **mất dữ liệu**.

**Distributed lock dù Hangfire đã điều phối:** Hangfire bảo đảm mỗi *job instance* chỉ một server lấy, nhưng không bảo đảm hai *lượt* của cùng recurring job không chồng lên nhau (thử lại, trigger tay, worker thứ hai khi scale).

**Soft Delete và ba hệ quả bắt buộc:**
- **(1) Partial unique index** — nếu không, slug của recipe đã xóa chiếm chỗ vĩnh viễn.
- **(2) Child entity không xóa theo.**
- **(3) Không xóa file khi xóa mềm** — nếu xóa thì "khôi phục được trong 30 ngày" (NFR-REL-003, MT-49) thành lời hứa suông.

Giải trình đầy đủ về xóa mềm và Lưu trữ/Khôi phục ở **Buổi 8 — Dev 2**; về `PublishedAt` và danh sách kiểm tra xuất bản ở **Buổi 5 — Dev 2** (phần B) — nơi các FR này hoàn tất giao diện.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **FR-JOB-003 ở v1.0.0 là Sitemap Generation Job:** tạo `sitemap.xml` lên MinIO/`wwwroot` rồi ping Google. Đặc tả này sai vị trí phục vụ (crawler đọc sitemap ở domain Next.js). Endpoint ping cũng đã bị Google khai tử từ 6/2023.
  - **Hướng đi:** SRS v1.1.0 (**MT-28 + MT-05**) chuyển sitemap sang Next.js và dùng khe FR-JOB-003 cho **Dọn dữ liệu vĩnh viễn Job** (xung đột **X-2** — §3).
- **Máy trạng thái ở v1.0.0 có Archived là hố đen:** vào được, không ra được.
  - **Hướng đi:** SRS v1.1.0 (**MT-35**) bổ sung **khôi phục → Draft** và mã 409 cho mọi chuyển đổi ngoài bảng.
- **FR-RCP-007 ở v1.0.0 ghi xóa cứng, trong khi 4 chỗ khác ghi xóa mềm.**
  - **Hướng đi:** SRS v1.1.0 (**MT-05**) chốt **xóa mềm** + job dọn sau 30 ngày.
- **Chuyển API FR-RCP-005/006/007 từ Dev 2 sang Dev 4 (29/09/2026):**
  - **Lý do:** cân bằng tải khi yêu cầu "hoàn thành tất cả API endpoints" dồn 25 endpoint vào một buổi.
  - **Không đổi SRS**; Dev 2 vẫn là chủ giao diện của ba FR này (Buổi 5 và Buổi 8) và dùng API qua hợp đồng Chương 8.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2312755_NguyenThangThieng_buoiso4`.
```
feat(recipes,jobs): complete FR-RCP-005/006/007 lifecycle APIs with closed state machine, FR-JOB-002 image resize & FR-JOB-003 purge job, api surface test 45/45
```

## Kiểm chứng cuối Buổi 4

1. **`ApiSurfaceTests` xanh:** route thật của ứng dụng khớp **đúng 45** endpoint của SRS v1.2.2 Chương 8 — không thiếu, không thừa.
2. **Mọi endpoint có ≥ 1 luồng thành công + ≥ 1 trường hợp lỗi** trong integration test (NFR-MAINT-002); mã lỗi đúng Phụ lục B; mọi lỗi là `application/problem+json`.
3. **Không còn lỗ hổng rò rỉ bản nháp:** kiểm thử MT-34 đã gỡ `Skip` và xanh; không còn `CacheOutput` trong code.
4. **Tìm kiếm:** tìm "pho" ra "Phở bò".
5. **Vòng đời:** ma trận 12 tổ hợp trạng thái xanh; xóa rồi tạo lại cùng tên được slug cũ.
6. **Phiên đăng nhập:** dùng lại refresh token cũ → cả họ token bị thu hồi.
7. **Scalar `/scalar`** hiển thị đủ 45 endpoint, kiểm thử tay từng nhóm.
8. **Job:** purge job xóa đúng bản ghi > 30 ngày và file của nó; đổi kích thước job sinh đủ 3 biến thể.

---

# BUỔI 5 – BẢO MẬT API, TÀI LIỆU API, NỀN TẢNG TYPESCRIPT FRONTEND (LAB 5) + GIAO DIỆN ĐÃ LÀM SỚM: SOẠN/SỬA/XUẤT BẢN, TÌM KIẾM & BỘ LỌC, HEALTH CHECKS

> **Yêu cầu tối thiểu Lab 5 của giảng viên (12/10/2026):** hoàn thiện lớp bảo mật API Backend, tài liệu hóa API và dựng nền móng TypeScript cho Frontend.
>
> | Dev | Yêu cầu Lab 5 | Hiện trạng trong code trước buổi | Việc thật sự phải làm |
> |---|---|---|---|
> | Dev 1 | CORS whitelist, Rate Limiting (`FixedWindowLimiter` + `SlidingWindowLimiter`), API Versioning `/api/v1/...` | CORS default policy có từ Buổi 2 (thiếu `If-Match`, không expose header); **chưa có rate limit**; `/api/v1` là chuỗi gắn cứng trong `MapGroup` | Named policy + kiểm cấu hình lúc khởi động; 3 chính sách rate limit theo SRS NFR-SEC-003; đăng ký `Asp.Versioning` |
> | Dev 2 | Scalar UI / OpenAPI tại `/scalar/v1`; `types/recipe.ts` | `MapScalarApiReference` có từ Buổi 2 nhưng chỉ bật ở Development, không có summary/tag/security scheme/response; kiểu Recipe nằm chung `types/api.ts` | Tài liệu đủ 45 endpoint + kiểm thử hợp đồng; tách và hoàn thiện kiểu Recipe |
> | Dev 3 | Khởi tạo Next.js 15 + TS + Tailwind; thư mục `/app, /components, /lib, /stores, /types, /hooks`; `types/category.ts`, `types/search.ts` | Next.js **15.5.25** App Router + TS + Tailwind v4 **đã chạy từ Buổi 2** (`src/{app, components, features, lib, types}`) | **Không khởi tạo lại** (xung đột X-9, §3); chuẩn hóa cấu trúc, thêm `stores/`, `hooks/`, quy tắc phụ thuộc giữa thư mục; kiểu Category & Search |
> | Dev 4 | Cài `@tanstack/react-query`, `lucide-react`, `clsx`, `tailwind-merge`, `axios`/`ky`, `zustand`, `zod`; `types/auth.ts`, `types/api.ts` | 5/7 gói **đã có từ Buổi 2**; thiếu `zustand`, `ky`; còn gói thừa `next-auth` | Thêm `zustand` + `ky` (chọn `ky`, không chọn `axios` — X-10, §3), gỡ `next-auth`; kiểu Auth & API kèm kiểm thử chống lệch mã lỗi với Backend |
>
> **Buổi 5 đã được làm một phần trước khi có yêu cầu Lab 5.** Ngày 07/10 – 12/10/2026, cả 4 dev đã làm Buổi 5 theo kế hoạch cũ trên nhánh `buoiso5` (Dev 3 đã viết báo cáo Lab 05). Trưởng nhóm quyết định ngày 12/10/2026: **giữ nguyên phần đã làm và bổ sung yêu cầu Lab 5** — mỗi dev có hai phần:
> - **A. Yêu cầu tối thiểu Lab 5** — phần mới, trình bày đủ 4 mục theo mẫu;
> - **B. Phần đã hoàn thành theo kế hoạch trước 12/10/2026** — giữ nguyên giải trình, ghi rõ commit.
>
> Khi Lab 7/8 giao lại một tính năng đã có ở phần B (trang tìm kiếm, biểu mẫu nhiều bước – wizard), buổi đó **nâng cấp lên nền mới** của Buổi 5–6 (kiểu TypeScript, `ky`, shadcn/ui, Zustand) và làm phần còn thiếu — **không làm lại từ đầu**.
>
> **Thứ tự merge chuẩn Dev 4 → Dev 1 → Dev 3 → Dev 2** (§2.4) được tận dụng có chủ đích: Dev 2 merge cuối nên rebase lên code Buổi 5 của cả ba người còn lại rồi mới gắn tài liệu OpenAPI cho mọi endpoint — một lượt, một giọng văn, không ai phải sửa lại.
>
> **Commit nền Buổi 5 (Dev 4, 30 phút đầu):** tách `frontend/src/types/api.ts` thành 5 file theo miền (`api.ts`, `auth.ts`, `recipe.ts`, `category.ts`, `search.ts`) bằng cách **chuyển nguyên khai báo, không sửa nội dung**, rồi cập nhật import. Sau commit này mỗi file có đúng một chủ (bảng trên) và bốn dev làm song song không đụng file của nhau.

## DEV 1 — CORS, Rate Limiting & API Versioning (Lab 5)

### A. Yêu cầu tối thiểu Lab 5

**1. Tính năng & mã yêu cầu:** Lab 5 – Dev 1. `NFR-SEC-003` Rate Limiting · `NFR-SEC-005` (phần CORS) · SRS §5.2 (Versioning, CORS Headers, Rate Limit Headers) · mã lỗi `RATE_LIMIT_EXCEEDED` (429) của Phụ lục B — tính tới hết Buổi 4 mã này **chưa có đường sinh ra** trong code.

**2. Các bước triển khai:**

*Luồng request sau buổi này:* Trình duyệt → Nginx (`limit_req` 100r/m — lớp chặn thô; gắn `X-Forwarded-For`, `X-Correlation-ID`) → API: `UseForwardedHeaders` (IP thật, Buổi 4) → `GlobalExceptionMiddleware` → `UseStatusCodePages` → **`UseCors`** (preflight `OPTIONS` kết thúc tại đây) → **`UseRateLimiter`** (phân vùng theo IP thật, chọn chính sách theo metadata của endpoint) → `UseAuthentication` → `UseAuthorization` → route `/api/v{version:apiVersion}/…` (**`Asp.Versioning`** đọc phiên bản từ URL) → MediatR.

1. **CORS — `API/Extensions/CorsExtensions.cs`** (`AddFrontendCors(IConfiguration)`), thay khối `AddCors` ẩn danh trong `Program.cs`:
   - Named policy `CorsPolicies.Frontend = "Frontend"`; `app.UseCors(CorsPolicies.Frontend)`.
   - Danh sách origin đọc từ `Cors:AllowedOrigins` (`appsettings.Development.json`: `http://localhost:3000`; production: biến môi trường `Cors__AllowedOrigins__0=https://<domain>`).
   - **Kiểm tra lúc khởi động** (`IValidateOptions<CorsSettings>` + `ValidateOnStart()`): danh sách rỗng ở Production, có `*`, hoặc không phải URI tuyệt đối `http/https` → **ứng dụng không khởi động**. Sai cấu hình CORS bị phát hiện lúc deploy, không phải lúc người dùng báo lỗi.
   - `WithMethods` đúng danh sách SRS §5.2. `WithHeaders("Content-Type", "Authorization", "X-Correlation-ID", "If-Match")` — `If-Match` cần cho `PUT /recipes/{id}` (SRS §8.3, FR-RCP-004; ghi vào §5.2 ở v1.2.3 — MT-68); thiếu nó thì preflight của trang sửa công thức bị trình duyệt chặn khi Frontend gọi khác origin (`npm run dev` ở cổng 3000 gọi thẳng API ở cổng 5000).
   - `WithExposedHeaders("ETag", "Retry-After", "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "X-Correlation-ID", "api-supported-versions")` — không expose thì JavaScript **không đọc được** các header này khi gọi khác origin, dù server có gửi.
   - `SetPreflightMaxAge(TimeSpan.FromMinutes(10))`; **không** `AllowCredentials()` — hệ thống không dùng cookie (SRS §5.2, NFR-SEC-005).
   - `UseCors` đứng **trước** `UseRateLimiter`: preflight kết thúc ở CORS nên không trừ vào hạn mức 10 request/phút của `/auth/*`.
2. **Rate limiting — `API/RateLimiting/`**:
   - `RateLimitPolicies.cs`: hằng số `Auth = "auth"`, `Upload = "upload"`.
   - `RateLimitingOptions.cs`: đọc `RateLimiting:{Global,Auth,Upload}:{PermitLimit,WindowSeconds}` và `RateLimiting:Enabled`. Mặc định đúng SRS: 100 / 10 / 5 request mỗi 60 giây.
   - `RateLimitingExtensions.AddCulinaryRateLimiting()` — **thuật toán theo SRS NFR-SEC-003** (quyết định của trưởng nhóm 12/10/2026, xung đột X-6 §3):

     | Chính sách | Thuật toán | Hạn mức | Áp lên | Phân vùng |
     |---|---|---|---|---|
     | Toàn cục (`GlobalLimiter`) | `FixedWindowRateLimiter` | 100 req/phút | mọi request, **trừ** `/health*` và `/hangfire*` | IP thật |
     | `"auth"` | `SlidingWindowRateLimiter`, 6 phân đoạn × 10 giây | 10 req/phút | cả group `/auth/*` (`MapGroup("/auth").RequireRateLimiting(RateLimitPolicies.Auth)`) | IP thật |
     | `"upload"` | `FixedWindowRateLimiter` | 5 req/phút | `POST /files/upload`, `POST /recipes/{id}/images` | IP thật |

     Chính sách endpoint chạy **cộng thêm** chính sách toàn cục (ASP.NET Core áp cả hai). `QueueLimit = 0`: vượt hạn mức là từ chối ngay, không xếp hàng giữ kết nối.
   - Khóa phân vùng: `context.Connection.RemoteIpAddress` — đã là IP thật nhờ `UseForwardedHeaders` + `KnownIPNetworks` (Buổi 4). Không có IP (chạy kiểm thử) → khóa `"unknown"`.
   - `OnRejected`: trả **429** qua `IProblemDetailsService` (cùng định dạng với `GlobalExceptionMiddleware`) với `type = RATE_LIMIT_EXCEEDED`, header `Retry-After` lấy từ `lease.TryGetMetadata(MetadataName.RetryAfter, …)`; ghi `Log.Warning("RATE LIMIT: {Policy} rejected {Ip} on {Path}")`.
   - **Header `X-RateLimit-Limit/Remaining/Reset` (SRS §5.2):** ba chính sách được viết thành lớp `IRateLimiterPolicy<string>` (`GlobalRateLimiterPolicy`, `AuthRateLimiterPolicy`, `UploadRateLimiterPolicy`), mỗi lớp giữ một `PartitionedRateLimiter` riêng. `RateLimitHeadersMiddleware` (đứng ngay sau `UseRateLimiter`) gọi `GetStatistics(ip)` để lấy số lượt còn lại và ghi header qua `Response.OnStarting`. Không dùng lambda `AddPolicy(...)` vì lambda không cho đọc lại thống kê của limiter.
   - **Nginx** (`nginx/templates/nginx.conf.template`) giữ `limit_req_zone … rate=100r/m` của Buổi 2, thêm chú thích: ngưỡng Nginx (kể cả `burst`) phải **≥** ngưỡng ứng dụng, để người dùng luôn nhận 429 có cấu trúc từ API thay vì trang lỗi trống của Nginx.
3. **API Versioning — `API/Extensions/ApiVersioningExtensions.cs`**:
   - Gói thư viện `Asp.Versioning.Http` 10.2.x (Minimal APIs) + `Asp.Versioning.OpenApi` 10.2.x (để Dev 2 sinh tài liệu theo phiên bản cho `/scalar/v1`) — bổ sung, SRS không chỉ định công cụ (➕ trong `CONG_NGHE_VA_PHIEN_BAN.md`).
   - `AddApiVersioning(o => { o.DefaultApiVersion = new ApiVersion(1, 0); o.AssumeDefaultVersionWhenUnspecified = false; o.ReportApiVersions = true; o.ApiVersionReader = new UrlSegmentApiVersionReader(); })`.
   - `Program.cs`: `var versions = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).ReportApiVersions().Build();` → `app.MapGroup("/api/v{version:apiVersion}").WithApiVersionSet(versions)`; các `Map*Endpoints()` giữ nguyên.
   - URL thực tế **không đổi** (`/api/v1/...`) — Frontend, Nginx, kiểm thử không phải sửa. Response mang `api-supported-versions: 1.0`.
   - `/api/v2/...` → Asp.Versioning trả 400; `CustomizeProblemDetails` (cùng chỗ đang đổi 413 → `FILE_SIZE_EXCEEDED`) đổi `type` sang **`VALIDATION_ERROR`** kèm `errors.version` — Phụ lục B quy định mọi lỗi đầu vào là 400 `VALIDATION_ERROR`, không để lọt `type` dạng URI của thư viện.
   - `/health*` và `/hangfire` **không** gắn phiên bản: đó là đường dẫn hạ tầng mà Docker/Nginx gọi, SRS §8.7 đặt ngoài `/api/v1`.
   - **`ApiSurfaceTests` (file của Dev 4, Buổi 4):** route thật giờ có dạng `/api/v{version:apiVersion}/recipes`. Thêm bước chuẩn hóa thay `v{version:apiVersion}` bằng `v1` trước khi so với danh sách SRS; Dev 4 rà soát PR. Kiểm thử vẫn khóa đúng 45 endpoint.
4. **Kiểm thử:**
   - **Cấu hình kiểm thử:** `CulinaryBlogApiFactory` (Buổi 3) đặt `RateLimiting:Enabled=false` cho toàn bộ bộ integration test. Hàng trăm kiểm thử cùng gọi `/auth/login` từ một "IP" sẽ chạm hạn mức 10 request/phút và đỏ ngẫu nhiên. Riêng `RateLimitingTests` dùng factory con với hạn mức nhỏ (`Auth:PermitLimit=3`).
   - **`RateLimitingTests`** (dùng lại cơ chế giả lập IP nguồn của kiểm thử `CreatedByIp` — Buổi 4 Dev 1):
     - request đăng nhập thứ 4 từ cùng IP → 429 + `Retry-After` + `type = RATE_LIMIT_EXCEEDED`;
     - cùng lúc đó IP khác vẫn 200 — chứng minh phân vùng theo IP thật, không phải giới hạn toàn hệ thống;
     - request upload thứ 6 → 429;
     - `/health/ready` gọi 200 lần liên tiếp không bao giờ bị 429;
     - response thành công có đủ `X-RateLimit-*`.
   - **`CorsTests`:**
     - preflight từ origin cho phép → có `Access-Control-Allow-Origin` đúng origin (không phải `*`), `Access-Control-Expose-Headers` chứa `ETag`, cho phép `If-Match`;
     - origin lạ → không có header CORS;
     - khởi động với `Cors:AllowedOrigins = ["*"]` → `OptionsValidationException`.
   - **`ApiVersioningTests`:** `/api/v1/categories` → 200 kèm `api-supported-versions`; `/api/v2/categories` → 400 `VALIDATION_ERROR`.
   - `ApiSurfaceTests` vẫn **45/45**.

**3. Định hướng & lý do kiến trúc:**

**Vì sao giữ thuật toán của SRS (sliding window cho `/auth/*`) dù Lab 5 ghi ngược lại:** với fixed window, cửa sổ đếm reset cứng ở đầu mỗi phút. Kẻ dò mật khẩu bắn 10 request ở giây thứ 59 và 10 request ở giây thứ 61 — **20 lần thử trong 2 giây** mà không vi phạm hạn mức nào. Sliding window chia phút thành 6 phân đoạn và trượt theo thời gian, nên trong **bất kỳ** khoảng 60 giây nào cũng không quá 10 lần thử. Đây đúng là thứ endpoint đăng nhập cần. API đọc chung thì không có động cơ tấn công dồn ở ranh giới cửa sổ, nên fixed window (rẻ hơn về bộ nhớ: một bộ đếm thay vì 6) là đủ. Yêu cầu Lab 5 *"có cả `FixedWindowLimiter` lẫn `SlidingWindowLimiter`"* vẫn được đáp ứng — chỉ đặt mỗi thuật toán vào đúng chỗ nó phát huy tác dụng (X-6, §3).

**Rate limiting chỉ đúng khi đứng sau `UseForwardedHeaders`:** khi API đứng sau Nginx mà không xử lý header chuyển tiếp, `RemoteIpAddress` là **IP của container Nginx** — giống nhau cho mọi người dùng. Khi đó rate limit **biến thành giới hạn toàn hệ thống**: 10 request/phút tới `/auth/*` cho **tất cả** người dùng cộng lại — một biện pháp bảo mật tự gây ra từ chối dịch vụ. Ngược lại, bật forwarded headers mà không giới hạn mạng tin cậy (`KnownIPNetworks`) thì kẻ tấn công chỉ cần đổi `X-Forwarded-For` mỗi request là né hoàn toàn rate limit. Cả hai điều kiện đã có từ Buổi 4; buổi này dựng rate limit lên đúng nền đó và có kiểm thử chứng minh phân vùng theo IP.

**Hai lớp giới hạn, hai vai trò:** Nginx là lớp chặn thô ở vòng ngoài — rẻ, chặn trước khi request tốn tài nguyên .NET. ASP.NET Core là lớp nghiệp vụ — phân biệt `/auth`, upload, trả Problem Details và `X-RateLimit-*` đúng SRS. Nếu ngưỡng Nginx thấp hơn, người dùng nhận trang 503 trống không có `Retry-After`, Frontend không biết phải chờ bao lâu.

**Hạn mức cấu hình được, không gắn cứng:** (1) kiểm thử cần tắt hoặc thu nhỏ hạn mức — gắn cứng thì bộ integration test đỏ ngẫu nhiên; (2) load test Buổi 8 bắn từ một máy nên phải nâng hạn mức trong môi trường đo (xem Buổi 8 — Dev 4); (3) lúc bảo vệ đồ án, cả lớp dùng chung một Wi-Fi (một IP NAT) — chỉnh `RateLimiting__Auth__PermitLimit` bằng biến môi trường thay vì sửa code và build lại. Giá trị mặc định luôn đúng SRS.

**Vì sao đăng ký `Asp.Versioning` thay vì giữ chuỗi `/api/v1` gắn cứng:** SRS §5.2 cam kết *"breaking changes → `/api/v2/`, v1 được duy trì tối thiểu 6 tháng"*. Với chuỗi gắn cứng, ngày có v2 nhóm phải nhân đôi mọi `MapGroup` và tự viết header báo phiên bản. Với `Asp.Versioning`:
- hai phiên bản dùng chung một route group, mỗi endpoint khai báo phiên bản mình phục vụ;
- header `api-supported-versions` được sinh tự động;
- chính sách ngừng hỗ trợ (header `Sunset`/`Deprecation` theo RFC 8594) là cấu hình có sẵn — đúng thứ cần để thực thi cam kết "6 tháng".

Đổi lại: thêm 2 gói thư viện, và `ApiSurfaceTests` cần một bước chuẩn hóa route.

**Expose header là một nửa còn thiếu của CORS:** SRS §8.3 trả `ETag` để Frontend gửi lại trong `If-Match`, §5.2 trả `Retry-After`, `X-RateLimit-*`. Trình duyệt **ẩn mọi header không nằm trong danh sách an toàn** khỏi JavaScript khi gọi khác origin — server gửi đủ mà code vẫn đọc ra `null`. Lỗi này không làm hỏng request nào, chỉ làm hỏng âm thầm cơ chế concurrency của trang sửa. SRS v1.2.2 §5.2 chưa có `If-Match` lẫn danh sách header expose dù §8.3 và chính §5.2 dùng các header đó — đã sửa trong SRS v1.2.3 (CR-2026-05 / MT-68).

**4. Commit gợi ý:**
```
feat(security): cors allowlist with exposed headers, ip-based rate limiting (sliding auth, fixed global/upload) and url-segment api versioning
```

### B. Phần đã hoàn thành theo kế hoạch trước 12/10/2026

Nhánh `2314236_HoangBinhQuan_buoiso5` có hai commit **gia cố API phiên & tài khoản của Buổi 4** (không có phần giao diện):
- `7e24c92 feat(auth): complete Dev1 session 4 account API validation and tests`;
- `a8ec784 feat(auth): harden session revocation concurrency and auth contracts` — `UserTokenTransaction`, `TokenRevocationConcurrencyTests` (thu hồi phiên song song), `AuthRequestContractTests`.

**Retrofit D-12 đã xong sớm ở Buổi 4** (commit `5239618`, `f473fca`): `AuthProvider` giữ access token chỉ trong bộ nhớ, `localStorage` chỉ còn `{ refreshToken, user }`, refresh single-flight, khóa liên tab bằng Web Locks (`refresh-coordinator.ts`). Giao diện hồ sơ (FR-AUTH-006/007) của kế hoạch Buổi 5 cũ **chưa làm** → chuyển sang **Buổi 7 — Dev 1** (Lab 7 giao trang `/profile`).

---

## DEV 2 — Tài liệu API Scalar/OpenAPI & Kiểu TypeScript miền Recipe (Lab 5)

### A. Yêu cầu tối thiểu Lab 5

**1. Tính năng & mã yêu cầu:** Lab 5 – Dev 2. `NFR-MAINT-003` (*"API documentation: tự động sinh từ XML comments + Scalar/Swagger UI tại `/scalar`"*) · SRS §6.2 (OpenAPI: Scalar UI, XML documentation comments) · hợp đồng SRS §8.3 – §8.6 (miền Recipe) cho phần kiểu TypeScript.

**2. Các bước triển khai:**

*Luồng dữ liệu:* code endpoint (`.WithSummary/.WithTags/.Produces…`) + XML doc → `Microsoft.AspNetCore.OpenApi` (.NET 10 — OpenAPI **3.1**) qua các transformer → `/openapi/v1.json` → Scalar `/scalar/v1`. Cùng file JSON được chụp thành `docs/api/openapi.v1.json` → kiểm thử Backend chặn thay đổi hợp đồng không chủ đích, kiểm thử Frontend so khóa của kiểu TypeScript với schema.

1. **`API/OpenApi/OpenApiExtensions.cs` — `AddCulinaryOpenApi()`:** đăng ký tài liệu `"v1"` (gắn với API version 1.0 của Dev 1 qua `Asp.Versioning.OpenApi`) cùng bốn transformer:
   - `ApiInfoDocumentTransformer` — `info` (title *Culinary Blog API*, version `v1`, mô tả dẫn về SRS v1.2.3); `servers` dùng đường dẫn tương đối `/` để tài liệu đúng cả khi đi qua Nginx lẫn gọi thẳng cổng 5000.
   - `BearerSecuritySchemeTransformer` — thêm security scheme `Bearer` (HTTP, JWT) vào `components`.
   - `AuthorizationOperationTransformer` — endpoint có metadata `IAuthorizeData` (`RequireAuthorization`, `AuthorPolicy`, `AdminPolicy`) được gắn security requirement + response 401/403; endpoint ẩn danh không gắn.
   - `ProblemDetailsOperationTransformer` — mọi response 4xx/5xx dùng `application/problem+json` với schema `ProblemDetails`; thêm schema enum `ErrorCode` liệt kê mã Phụ lục B, sinh bằng reflection trên `ErrorCodes` — Frontend đọc trực tiếp danh sách mã từ tài liệu.

   *Lưu ý .NET 10:* `Microsoft.AspNetCore.OpenApi` 10 dùng `Microsoft.OpenApi` 2.x — kiểu dữ liệu khác bản 1.x trong phần lớn bài hướng dẫn trên mạng (ví dụ `components.SecuritySchemes` chứa `IOpenApiSecurityScheme`). Repo bật `TreatWarningsAsErrors` nên phải dùng đúng API của bản đang cài.
2. **Scalar — `app.MapScalarApiReference(...)`:**
   - Tiêu đề, scheme `Bearer` là mặc định để nút *Authorize* điền token một lần dùng cho mọi request thử, client mẫu `JavaScript/fetch`.
   - Đường dẫn theo tài liệu: **`/scalar/v1`**; `/scalar` (SRS) vẫn vào được.
   - Bật/tắt bằng cấu hình `OpenApi:Enabled` thay cho `IsDevelopment()` gắn cứng: compose dev và môi trường demo bật; `docker-compose.prod.yml` (Buổi 7) tắt.
   - Thêm `location ^~ /scalar/` và `location ^~ /openapi/` vào `nginx.conf.template` (file của Dev 4 — Dev 4 rà soát), để `http://localhost/scalar/v1` chạy qua Nginx chứ không rơi vào Next.js (404).
3. **Gắn metadata cho đủ 45 endpoint** (Dev 2 merge cuối, rebase lên code của cả nhóm):
   - mỗi endpoint: `.WithName("Recipes_Create")` (operationId ổn định — sinh client không đổi tên hàm giữa các lần build), `.WithSummary(...)`, `.WithDescription("FR-RCP-003 — SRS §8.3 …")`, `.WithTags(...)`;
   - kiểu response: `.Produces<TDto>(200|201)`, `.ProducesProblem(404|409|…)`, `.ProducesValidationProblem()`;
   - 9 tag: `Auth`, `Users`, `Categories`, `Recipes`, `Recipe Images`, `Recipe Steps`, `Recipe Ingredients`, `Files`, `Health`.

   Handler viết thành method có tên → XML doc (`<summary>`, `<param>`, `<response code="409">`) được .NET 10 tự đưa vào tài liệu (repo đã bật `GenerateDocumentationFile`). Handler lambda → dùng `.WithSummary`. Record DTO có XML doc → mô tả schema. Mỗi chủ module rà soát phần endpoint của mình trong PR.
4. **`OpenApiDocumentTests`** (integration, `API.IntegrationTests/Platform/`):
   - số operation trong `/openapi/v1.json` = số endpoint `/api/v1` trong danh sách của `ApiSurfaceTests` — dùng chung một nguồn, không đếm hai lần;
   - mọi operation có `summary`, `tags`, `operationId` duy nhất;
   - operation có security requirement ⇔ endpoint có `RequireAuthorization`;
   - mọi response 4xx/5xx là `application/problem+json`.

   **Ảnh chụp hợp đồng:** kiểm thử ghi `docs/api/openapi.v1.json` khi chạy với `OPENAPI_SNAPSHOT_UPDATE=1`, các lần khác so khớp và **đỏ khi hợp đồng đổi mà file chưa cập nhật** — mọi thay đổi API hiện thành diff trong PR để người duyệt thấy.
5. **Frontend — `types/recipe.ts`** (Dev 2 sở hữu sau commit nền của Dev 4):
   - Hằng số làm nguồn duy nhất: `RECIPE_DIFFICULTIES = ['Easy','Medium','Hard','Expert'] as const`, `RECIPE_STATUSES = ['Draft','Published','Archived'] as const` → `type RecipeDifficulty = (typeof RECIPE_DIFFICULTIES)[number]`. Zod (`z.enum(RECIPE_DIFFICULTIES)`) và ô chọn độ khó dùng lại đúng mảng này.
   - Kiểu response (khớp DTO C#, JSON camelCase):
     - `RecipeSummary`;
     - **`Recipe`** — đổi tên từ `RecipeDetail` để khớp tên Lab 5; `RecipeDetail` giữ làm alias `@deprecated` tới Buổi 8 để không gãy code đang dùng;
     - **`RecipeIngredient`** (`quantity` số + `quantityText` nguyên văn — MT-04), **`RecipeStep`**;
     - **`Nutrition`** — đổi tên từ `RecipeNutrition`, alias `@deprecated`;
     - `RecipeImage`, `RecipeImageResult`.
   - Kiểu request theo SRS §8.3 – §8.6:
     - `CreateRecipeRequest` (kèm mảng tùy chọn `ingredients?`, `steps?` — Buổi 4);
     - `UpdateRecipeRequest` (kèm `rowVersion`);
     - `IngredientInput`, `StepInput` — **không có `stepNumber`**: server tự đánh số (FR-RCP-010);
     - `ReorderStepsRequest { stepIds: string[] }`, `UpdateRecipeImageRequest { altText?, isPrimary? }`;
     - `MyRecipesParams { status?, page?, pageSize?, sortBy?, sortOrder? }`;
     - `RecipeStatusAction = 'publish' | 'unpublish' | 'archive' | 'unarchive'`.
   - Mỗi kiểu có JSDoc trỏ về mục SRS tương ứng.
6. **Kiểm thử hợp đồng phía Frontend — `types/__tests__/recipe-contract.test.ts`:**
   - Khai báo `const recipeKeys = { id: true, title: true, … } satisfies Record<keyof Recipe, true>`. TypeScript **báo lỗi biên dịch** khi thiếu hoặc thừa khóa so với kiểu.
   - Jest đọc `docs/api/openapi.v1.json` và so `Object.keys(recipeKeys)` với `properties` của schema `RecipeDetailDto`; làm tương tự cho `RecipeSummaryDto`, `RecipeIngredientDto`, `RecipeStepDto`.

   Backend thêm trường mà kiểu TypeScript chưa có (hoặc ngược lại) → kiểm thử đỏ ngay trong PR, không đợi màn hình hiển thị `undefined`.

**3. Định hướng & lý do kiến trúc:**

**Tài liệu sinh từ code, không viết tay:** tài liệu viết tay (Word, Postman collection) lệch khỏi code ngay từ lần sửa đầu tiên, và không ai biết nó đã lệch. Sinh từ metadata của chính endpoint thì tài liệu **không thể** sai khác code; kiểm thử ảnh chụp biến mọi thay đổi hợp đồng thành một diff phải được duyệt. Đây là cùng triết lý với `ApiSurfaceTests` (Buổi 4): yêu cầu "đủ và đúng" phải kiểm được bằng máy, không bằng mắt.

**Vì sao một người gắn metadata cho cả 45 endpoint:** tài liệu API là một sản phẩm thống nhất — cùng cách đặt tag, cùng giọng văn summary, cùng quy tắc operationId. Bốn người tự viết cho module mình sẽ ra bốn phong cách. Dev 2 merge cuối theo thứ tự chuẩn nên là người duy nhất làm được việc này mà không gây xung đột: code của ba người kia đã nằm trên `develop`. Chủ module vẫn duyệt phần của mình, nên nội dung đúng nghiệp vụ.

**operationId ổn định là hợp đồng cho tương lai:** nếu sau này sinh client TypeScript từ OpenAPI, tên hàm lấy từ operationId. Để framework tự đặt thì tên đổi theo thứ tự đăng ký route — thêm một endpoint là đổi tên hàng loạt.

**Kiểu TypeScript viết tay + kiểm thử hợp đồng, không sinh tự động:** sinh bằng công cụ (`openapi-typescript`) cho kiểu đúng 100% nhưng thêm một gói thư viện, một bước build, và kiểu sinh ra không mang JSDoc về SRS. Viết tay đọc được và có tài liệu, nhưng dễ lệch. Cách chọn giữ ưu điểm của cả hai: viết tay để đọc, kiểm thử `satisfies Record<keyof T, true>` + so với `openapi.v1.json` để không lệch — đổi một trường ở Backend mà quên Frontend thì PR đỏ.

**Hằng số `as const` là nguồn duy nhất cho enum:** độ khó xuất hiện ở ít nhất 4 nơi phía Frontend — kiểu dữ liệu, Zod schema, ô chọn của bộ lọc, nhãn hiển thị. Bốn danh sách viết tay là bốn chỗ có thể quên `Expert` — đúng lỗi mà SRS v1.1.0 phải sửa ở MT-20.15.

**4. Commit gợi ý:**
```
feat(api-docs,types): scalar /scalar/v1 openapi documentation for all endpoints with contract snapshot and recipe domain typescript types
```

### B. Phần đã hoàn thành theo kế hoạch trước 12/10/2026

Nhánh `2312758_NguyenHongPhucTho_buoiso5` — commit `3a0750a` (tích hợp Buổi 4) và **`79d0132 feat(recipes): complete Buoi 5 recipe editor and publishing flow`**:
- `RecipeContentEditor.tsx` — nguyên liệu và các bước: thêm/sửa/xóa từng dòng gọi API ngay, đổi thứ tự bước bằng nút ↑/↓ → `PATCH /recipes/{id}/steps/reorder`;
- `/dashboard/recipes/[id]/edit` nạp bằng `GET /recipes/mine/{id}` (CR-2026-04);
- `RecipePublishActions.tsx` — xuất bản/hủy xuất bản kèm danh sách kiểm tra điều kiện;
- `write-api.ts`.

Hợp đồng API dùng trong phần này (đã làm ở Buổi 4):

| Nhóm | Endpoint | Chủ API |
|---|---|---|
| Nguyên liệu | `POST/PUT/DELETE /recipes/{id}/ingredients/{ingId?}` | Dev 2 |
| Các bước | `POST/PUT/DELETE /recipes/{id}/steps/{stepId?}` + `PATCH /recipes/{id}/steps/reorder` | Dev 2 |
| Sửa | `PUT /recipes/{id}` (409 `RECIPE_CONCURRENCY_CONFLICT`) | Dev 2 |
| Đọc bản nháp | `GET /recipes/mine/{id}` (trả `rowVersion` + `ETag`) | Dev 3 |
| Xuất bản | `PATCH /recipes/{id}/publish` (400 `RECIPE_PUBLISH_INCOMPLETE`, 409 `RECIPE_INVALID_STATE_TRANSITION`) và `/unpublish` | Dev 4 |

**Khác kế hoạch cũ:** đổi thứ tự bằng nút ↑/↓ thay cho kéo-thả `dnd-kit` — không thêm gói thư viện, và thao tác được bằng bàn phím (WCAG 2.1 AA yêu cầu mọi thao tác kéo-thả phải có cách làm thay thế). Mỗi dòng đang tự gọi API riêng (`useState` cục bộ), chưa dùng React Hook Form + `useFieldArray` → **Buổi 8 — Dev 2** chuyển sang một biểu mẫu thống nhất (Lab 8).

**Giải trình thiết kế (giữ nguyên):**

**Optimistic Concurrency thay vì khóa bi quan (pessimistic lock):** khóa bi quan giữ transaction mở suốt thời gian người dùng điền form — có thể vài phút — làm nghẽn connection pool và tạo nguy cơ deadlock. Optimistic không khóa gì, chỉ phát hiện xung đột **lúc lưu**: `RowVersion` trong DB khác giá trị client đọc lúc đầu nghĩa là có người khác đã sửa. Với một blog nơi xung đột hiếm khi xảy ra, đây là đánh đổi đúng. Trả `rowVersion` trong DTO và gắn `ETag` là điều kiện để cơ chế này chạy được; mã đúng là **409** — khớp ngữ nghĩa HTTP *"yêu cầu xung đột với trạng thái hiện tại của tài nguyên"* của mô hình ETag/If-Match.

**Danh sách kiểm tra điều kiện xuất bản hiển thị trước khi bấm:** người dùng viết xong một công thức dài, bấm Xuất bản rồi mới nhận lỗi là trải nghiệm bực bội. Server vẫn kiểm tra (không tin client), nhưng UI nói trước.

**`PublishedAt` chỉ gán một lần:** ghi đè mỗi lần xuất bản thì thao tác *hủy xuất bản rồi xuất bản lại* (sửa lỗi chính tả) làm bài cũ **nhảy lên đầu trang chủ** (trang chủ sắp theo `publishedAt` giảm dần). Giữ ngữ nghĩa *"ngày xuất bản lần đầu"* cũng là điều Google mong đợi ở `datePublished` trong JSON-LD. Nếu làm theo SRS v1.0.0 (không gán `PublishedAt`), cột này **mãi mãi `NULL`** — index `IDX_Recipe_PublishedAt` vô dụng, JSON-LD hỏng, trang chủ không có gì để sắp — một lỗi im lặng không ném exception nào (MT-35).

**Khóa slug sau lần xuất bản đầu (MT-27):** cho đổi slug tự do làm chết mọi link đã chia sẻ và URL Google đã lập chỉ mục, trong khi hệ thống không có bảng lưu slug cũ để chuyển hướng 301. Cho đổi khi còn Draft là đủ để sửa lỗi chính tả trước khi công khai. Phương án thêm bảng `RecipeSlugHistory` đúng chuẩn SEO nhất nhưng thêm một bảng và một nhánh tra cứu cho tình huống hiếm.

---

## DEV 3 — Chuẩn hóa cấu trúc Frontend & Kiểu TypeScript Category/Search (Lab 5)

### A. Yêu cầu tối thiểu Lab 5

**1. Tính năng & mã yêu cầu:** Lab 5 – Dev 3. CONS-003 (*"Frontend: Next.js App Router, không dùng Pages Router"*) · SRS §2.4.2, §6.1 (Next.js 15 + TypeScript + Tailwind CSS) · hợp đồng SRS §8.2 (Category) và FR-SRCH-001 → 004 (Search) cho phần kiểu.

**2. Các bước triển khai:**

1. **Không khởi tạo lại dự án (xung đột X-9, §3).** `create-next-app` sẽ ghi đè `package.json`, `tsconfig.json`, `next.config.ts`, cấu hình ESLint Airbnb và xóa toàn bộ code Buổi 2 – 5. Thay vào đó, kiểm chứng và ghi vào `frontend/README.md` (mục *Kiến trúc Frontend*) những gì đã có:
   - `next@15.5.25` App Router, `react@19.1.0`, TypeScript `strict`, alias `@/*` → `src/*`;
   - Tailwind CSS v4 qua `@tailwindcss/postcss`;
   - ESLint Airbnb + Prettier (`npm run lint`), Jest + Testing Library, Playwright;
   - `output: 'standalone'` cho Docker.
2. **Cấu trúc thư mục chuẩn** (đúng 6 thư mục Lab 5 yêu cầu, giữ `features/` của Buổi 2 – 5):
   ```
   frontend/src/
   ├─ app/         # chỉ route: page/layout/loading/error/not-found/route.ts — không chứa nghiệp vụ
   ├─ components/
   │  ├─ ui/       # DÀNH RIÊNG cho shadcn/ui (Buổi 6 — Dev 2), tên file kebab-case
   │  ├─ layout/   # Header, Footer, DashboardSidebar… (Buổi 6)
   │  └─ shared/   # component dùng chung nhiều module: ImageUploader, Pagination, RecipeGridSkeleton, FormField
   ├─ features/{auth,recipes,categories,system}/  # code theo module: api.ts, components/, schemas.ts, query-keys.ts (Buổi 7 thêm search/, Buổi 8 thêm users/)
   ├─ hooks/       # hook dùng chung nhiều module
   ├─ lib/         # tiện ích không phụ thuộc React/UI: api-client, query-client, config, utils
   ├─ stores/      # Zustand — CHỈ state phía client (store đầu tiên: auth-store, Buổi 6 — Dev 1)
   └─ types/       # kiểu hợp đồng API: api, auth, recipe, category, search
   ```
   - **`git mv components/ui/{FormField,ImageUploader,Pagination,RecipeGridSkeleton}.tsx components/shared/`** và sửa import. Lý do: Buổi 6, `npx shadcn add pagination` sẽ ghi `components/ui/pagination.tsx`. Windows/macOS không phân biệt hoa thường → `Pagination.tsx` và `pagination.tsx` là **cùng một file**, git rối loạn. Giải phóng `components/ui/` ngay từ bây giờ.
   - **`hooks/`** có hai hook thật, tách ra từ code đang có chứ không tạo file rỗng:
     - `use-debounced-value.ts` — logic debounce 300 ms đang nằm trong `SearchBar` (Buổi 5); Buổi 7–8 dùng lại cho ô tìm người dùng;
     - `use-media-query.ts` — điều kiện mở drawer bộ lọc trên điện thoại.
   - **`stores/`** có file đầu tiên ở Buổi 6. Buổi này ghi quy tắc *"store chỉ giữ state phía client; dữ liệu từ server thuộc TanStack Query"* vào `frontend/README.md`.
3. **Quy tắc phụ thuộc giữa thư mục — ESLint `import/no-restricted-paths`** (gói `eslint-plugin-import` đã có trong cấu hình Airbnb):

   | Thư mục | Không được import từ |
   |---|---|
   | `types/` | mọi thư mục khác (chỉ chứa kiểu) |
   | `lib/` | `app`, `components`, `features`, `stores`, `hooks` |
   | `components/ui/` | `app`, `features`, `stores` |
   | `features/*`, `components/*`, `hooks/`, `stores/` | `app/` |

   Đây là bản Frontend của `LayerDependencyTests` phía Backend: chiều phụ thuộc được máy kiểm ở mỗi lần `npm run lint`, không dựa vào trí nhớ.
4. **`types/category.ts`** (khớp SRS §8.2):
   - `Category { id, name, slug, description, imageUrl, orderIndex, recipeCount }`;
   - `CategoryRef { id, name, slug }`;
   - `CategoryDetail { category, recipes: PagedResult<RecipeSummary> }`;
   - `CreateCategoryRequest` / `UpdateCategoryRequest` (khớp validator FR-CAT-003/004 — tên ≤ 100 ký tự);
   - `CategoryDetailParams { page?, pageSize?, sortBy?, sortOrder? }` — §8.2 chỉ cho sắp xếp và phân trang, không có bộ lọc.
5. **`types/search.ts`** — chuyển kiểu và hằng số đang nằm trong `features/recipes/search-params.ts` (Buổi 5) về đây; logic phân tích URL (`parseSearchState`, `buildHref`) ở lại `features/`:
   - `SORT_FIELDS = ['createdAt','publishedAt','title','cookTime','prepTime'] as const` → `SortBy`; `SORT_ORDERS = ['asc','desc'] as const` → `SortOrder`;
   - `DEFAULT_PAGE_SIZE = 12`, `MAX_PAGE_SIZE = 50`, `MIN_SEARCH_LENGTH = 2` — cùng giá trị validator Backend;
   - `RecipeListParams` (đủ 5 bộ lọc + sắp xếp + phân trang của `GET /recipes`);
   - `RecipeSearchParams` — `q`, `categoryId`, `difficulty`, phân trang; Buổi 6 thêm `maxCookTime`, `maxPrepTime`, `minServings` theo CR-2026-05 / MT-66;
   - `RecipeSearchResult = PagedResult<RecipeSummary>`.
6. **Kiểm thử:**
   - `category-contract.test.ts`, `search-contract.test.ts` — cùng kỹ thuật `satisfies Record<keyof T, true>` + so với `docs/api/openapi.v1.json` của Dev 2;
   - `npm run lint` bắt được một import cố ý sai chiều (`lib/` import `features/`) — kiểm chứng rồi xóa;
   - `npm run build` xanh sau khi chuyển file.

**3. Định hướng & lý do kiến trúc:**

**Vì sao không làm đúng chữ "khởi tạo dự án":** yêu cầu này đúng với một nhóm bắt đầu Frontend từ con số 0. Nhóm này đã có Frontend chạy thật từ Buổi 2 — lát cắt dọc DB → API → UI là nguyên tắc từ ngày đầu (§0.2). Khởi tạo lại là xóa 4 buổi làm việc của cả nhóm. Mục tiêu thật của yêu cầu — *một dự án Next.js 15 + TS + Tailwind có cấu trúc rõ ràng* — được đáp ứng bằng chuẩn hóa và **chứng minh** cấu trúc (quy tắc lint), không phải bằng tạo mới.

**Giữ `features/` thay vì dồn hết vào `components/`:** cấu trúc theo loại file (`components/`, `hooks/`…) dễ hiểu khi dự án nhỏ, nhưng tới 40 component thì một tính năng phải sửa ở 5 thư mục khác nhau. `features/{module}` gom mọi thứ của một module (API, component, schema, query key) — trùng đúng ranh giới phân công 4 dev (§1), nên ít ai phải sửa thư mục của người khác. Sáu thư mục Lab 5 chứa phần **dùng chung**: `components/ui` (thư viện), `components/layout` (khung trang), `hooks`, `lib`, `stores`, `types`.

**`components/ui` dành riêng cho shadcn:** shadcn/ui không phải gói npm mà là code được chép vào dự án — chủ đích là để nhóm sửa trực tiếp. Trộn component tự viết vào cùng thư mục thì không còn biết file nào là bản gốc của thư viện (khi cần cập nhật) và file nào là của mình.

**4. Commit gợi ý:**
```
refactor(frontend): standardize app/components/lib/stores/types/hooks structure with lint-enforced import boundaries and category & search types
```

### B. Phần đã hoàn thành theo kế hoạch trước 12/10/2026

Nhánh `2314291_DoanHongTien_buoiso5` — `bd9c0f7 feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze`, `23bf8c7` (đo `EXPLAIN ANALYZE` trên SQL thật của EF), `99798b8` (báo cáo Lab 05, 319/319 kiểm thử xanh trên Docker):
- Migration **`B5_Search_CompositeIndexes`**: `IDX_Recipe_List (IsDeleted, Status, PublishedAt DESC)`, `IDX_Recipe_ByCategory (Status, CategoryId, PublishedAt DESC)`, `IDX_Recipe_CookTime (Status, CookTimeMinutes)`.
- `PerformanceSeeder` (cờ `PerformanceSeed:Enabled`, mặc định tắt) sinh 10.000 công thức. Ba truy vấn còn Seq Scan trước Buổi 5 đều chuyển sang Index Scan — `docs/explain-analyze-b5.md`. Không tạo index `(Status, CreatedAt DESC)` dự phòng vì `createdAt desc` đã Index Scan nhờ `IDX_Recipe_CreatedAt`.
- `SearchBar` (debounce 300 ms), trang `/search` (SSR, highlight không dấu, empty state), `FilterPanel` (drawer trên điện thoại), `SortSelect`, phân trang đồng bộ URL.
- **Retrofit D-10 phía FE.**
- **Một phần D-7:** `getCategoryBySlug` đặt `revalidate: 120`.

Còn lại cho buổi sau:
- `getCategories` vẫn `revalidate: 3600` → **Buổi 7 — Dev 3**;
- trang `/search` chỉ lọc được danh mục + độ khó vì SRS §8.3 chỉ cho `GET /recipes/search` nhận hai bộ lọc này → CR-2026-05 / MT-66 (API: **Buổi 6 — Dev 3**, giao diện: **Buổi 7 — Dev 4**).

**Giải trình thiết kế (giữ nguyên):**

**`sortBy` + `sortOrder` thắng `sort=-field`:** hai tham số độc lập cho phép hai quy tắc FluentValidation tách biệt (`sortBy ∈ danh sách cho phép`, `sortOrder ∈ {asc,desc}`). Quan trọng hơn: dấu `-` **rất dễ bị nuốt khi quên URL-encode** — `sort=-createdAt` thành `sort= createdAt`, hệ thống sắp tăng dần thay vì giảm dần, **không có lỗi nào được ném ra**. Phía Frontend, cặp `sortBy`/`sortOrder` ánh xạ 1-1 với cặp dropdown "cột"/"chiều".

**Danh sách cho phép là rào chắn bảo mật:** `sortBy` là tên cột, và tên cột **không tham số hóa được** trong SQL. Ánh xạ qua dictionary ở tầng Application đảm bảo giá trị đi vào truy vấn chỉ có thể là một trong 5 biểu thức viết sẵn. Trả **400** cho giá trị ngoài danh sách thay vì im lặng dùng mặc định: im lặng thì Frontend gửi sai tên trường sẽ không bao giờ biết mình sai.

**Toàn bộ state bộ lọc nằm trong `searchParams`:** link chia sẻ được, back/forward đúng, SSR đọc được ngay. **`pageSize = 12`** chia hết cho lưới 2, 3 và 4 cột.

**Composite index chọn theo hình dạng truy vấn thật** (MT-26): yêu cầu cũ *"mọi cột WHERE/ORDER BY đều có B-tree index"* dẫn tới hàng loạt index đơn cột làm chậm mọi lệnh ghi, trong khi PostgreSQL thường chỉ chọn một index cho một truy vấn. Tiêu chí nghiệm thu là *"`EXPLAIN ANALYZE` cho Index Scan"* trên ≥ 10.000 bản ghi — với 100 bản ghi seed, PostgreSQL luôn chọn Seq Scan vì quét cả bảng rẻ hơn, nên đo trên dữ liệu nhỏ không chứng minh được gì.

---

## DEV 4 — Gói thư viện Frontend & Kiểu TypeScript Auth/API (Lab 5)

### A. Yêu cầu tối thiểu Lab 5

**1. Tính năng & mã yêu cầu:** Lab 5 – Dev 4. SRS §6.1 (Frontend: TanStack Query, React Hook Form + Zod) · NFR-USE-003 (*"error messages sử dụng error code"*) · Phụ lục B (mã lỗi) · SRS §8.1 (hợp đồng Auth) cho phần kiểu.

**2. Các bước triển khai:**

1. **Rà soát gói thư viện theo danh sách Lab 5** (ghi vào `CONG_NGHE_VA_PHIEN_BAN.md` mục Frontend):

   | Gói | Hiện trạng | Hành động |
   |---|---|---|
   | `@tanstack/react-query` (+ devtools) | có từ Buổi 2 (`^5.102.8`) | giữ |
   | `lucide-react`, `clsx`, `tailwind-merge`, `zod` | có từ Buổi 2 | giữ |
   | `zustand` | chưa có | **thêm** — bản mới nhất ngày 12/10/2026: 5.0.15 |
   | `ky` *(Lab cho chọn `axios` hoặc `ky`)* | chưa có | **thêm `ky`** — bản mới nhất 2.2.0; **không** thêm `axios` (X-10, §3) |
   | `next-auth` (`^5.0.0-beta.32`) | có nhưng **0 chỗ dùng** (`git grep next-auth frontend/src` rỗng) — sót lại từ trước MT-11 | **gỡ** |

   - Cài bằng npm 10 (`npx npm@10 install zustand ky`) để `package-lock.json` giữ định dạng mà container Node 22 đọc được (`CONG_NGHE_VA_PHIEN_BAN.md` mục 6.5).
   - Ghi phiên bản thật từ `package-lock.json` vào tài liệu công nghệ.
2. **Hai điểm kiểm chứng bắt buộc ngay khi cài `ky`** — không đạt thì dừng, ghi lại, chưa thay lớp gọi API ở Buổi 6:
   - **Option `next` phải tới được `fetch` của Next.js.** Unit test với `fetch` giả: `ky(url, { next: { revalidate: 300 } })` → `fetch` nhận `init.next.revalidate === 300`. Nếu bị rơi mất, mọi trang ISR/Data Cache (§2.3) mất `revalidate` mà không có lỗi nào.
   - **Jest chạy được `ky`.** `ky` là gói ESM thuần; `next/jest` mặc định không biên dịch `node_modules` → thêm `transformIgnorePatterns: ['/node_modules/(?!ky/)']` vào `jest.config.mjs`.
3. **Commit nền Buổi 5 — tách `types/api.ts`** (30 phút đầu, trước khi ba dev khác bắt đầu): chuyển nguyên khai báo sang `types/{api,auth,recipe,category,search}.ts`, sửa import, `npm run build` xanh, merge vào `develop`.
4. **`types/api.ts`** (Dev 4 sở hữu):
   - `PagedResult<T>` (MT-42);
   - `ProblemDetails` và `ValidationProblemDetails` — chuyển từ `lib/api-client.ts` sang đây; `api-client` import lại;
   - **`ERROR_CODES`** — mảng `as const` đủ 29 mã đang có trong `ErrorCodes.cs` → `type ErrorCode`; mã thứ 30 của SRS v1.2.3 (`AUTH_PASSWORD_RESET_TOKEN_INVALID`) được thêm cùng lúc ở hai phía khi có API (Buổi 7 — Dev 1). Trong lúc chờ, test kiến trúc `ErrorCodes_MatchSrsAppendixB` ghi mã này trong `PendingSrsCodes` kèm tên người gỡ;
   - `UploadedFile { key, url, contentType, size }` (FR-FILE-001);
   - `ApiVersion = 'v1'`.
5. **`types/auth.ts`** (khớp SRS §8.1):
   - `ROLES = ['Author','Admin'] as const` → `Role`;
   - `User` (`UserDto { id, email, displayName, avatarUrl, bio, roles }`), `AuthResponse`;
   - request: `LoginRequest`, `RegisterRequest { email, password, displayName }`, `GoogleLoginRequest { idToken }`, `RefreshTokenRequest`, `LogoutRequest`, `UpdateProfileRequest { displayName?, avatarUrl?, bio? }`;
   - quản lý phiên và tài khoản: `Session` (`SessionDto` — không có `tokenHash`), `UserAdmin` (`UserAdminDto`), `UserListParams { page?, pageSize?, search?, isActive? }`, `SetUserStatusRequest { isActive, reason? }`;
   - kiểu của **CR-2026-05** (SRS v1.2.3, API làm ở Buổi 7): `ChangePasswordRequest { currentPassword, newPassword }`, `ForgotPasswordRequest { email }`, `ResetPasswordRequest { email, token, newPassword }`.
6. **Kiểm thử chống lệch mã lỗi — `types/__tests__/error-codes.test.ts`:**
   - đọc `backend/src/CulinaryBlog.Domain/Exceptions/ErrorCodes.cs` (cùng monorepo), lấy mọi hằng chuỗi bằng regex;
   - so **bằng nhau theo tập hợp** với `ERROR_CODES`.

   Backend thêm mã mà Frontend chưa biết (hoặc ngược lại) → đỏ. Buổi 6, `lib/error-messages.ts` khai báo `satisfies Record<ErrorCode, string>`, nên thiếu thông báo tiếng Việt cho một mã là **lỗi biên dịch**.
7. **Kiểm thử hợp đồng** `auth-contract.test.ts` cho `User`, `Session`, `UserAdmin` — cùng kỹ thuật với Dev 2/Dev 3.

**3. Định hướng & lý do kiến trúc:**

**Vì sao chọn `ky`, không chọn `axios`:** Next.js App Router gắn cơ chế cache của nó (Data Cache với `revalidate`, gom request trùng trong một lần render) vào **hàm `fetch`**. Từ MT-67, Data Cache là **tầng cache duy nhất** của trang công khai (trang render theo request vì CSP nonce), nên điều này càng quan trọng. `axios` không đi qua `fetch` — trên trình duyệt nó dùng `XMLHttpRequest`, trên server nó dùng module `http` của Node. Mọi trang Server Component gọi qua `axios` sẽ:
- **mất `revalidate`** — quy tắc vàng "`revalidate` ≤ TTL cache API" (§2.3) không còn cơ chế nào thực thi, và mỗi lượt xem trang sẽ gọi thẳng API;
- **mất gom request trùng** — `Header`, `SidebarCategory` và trang cùng gọi `getCategories()` thành 3 request.

`ky` là lớp mỏng **bọc trên `fetch`**, giữ nguyên các option của Next.js, đồng thời cho đúng những gì Lab 6 cần: hook `beforeRequest` (gắn token), `afterResponse` (thử lại sau khi refresh), `beforeError` (đổi lỗi sang `ApiError`), retry tôn trọng `Retry-After`. Chọn sai ở đây là lỗi im lặng: trang vẫn hiển thị, chỉ có cache không hoạt động.

**Gỡ gói không dùng là một phần của "cài đặt gói cần thiết":** `next-auth` bản beta không được import ở đâu nhưng vẫn được cài vào image, kéo theo cây phụ thuộc riêng, và gây hiểu nhầm cho người đọc `package.json` rằng hệ thống dùng Auth.js — điều SRS đã loại bỏ ở MT-11 (ID Token flow, không có callback phía Frontend).

**Kiểm thử đọc chéo `ErrorCodes.cs` từ Jest:** Frontend xử lý lỗi theo `type` (Phụ lục B), không theo câu chữ. Danh sách mã có ba bản sao — SRS, `ErrorCodes.cs`, `ERROR_CODES` — và bản nào lệch thì Frontend nhận về một mã nó không biết xử lý. Repo là monorepo nên kiểm thử đọc thẳng file C# — không cần sinh code hay chia sẻ gói thư viện, mà lệch là đỏ.

**4. Commit gợi ý:**
```
chore(frontend): add zustand and ky, drop unused next-auth, split shared api/auth typescript types with backend error-code drift test
```

### B. Phần đã hoàn thành theo kế hoạch trước 12/10/2026

Nhánh `2312755_NguyenThangThieng_buoiso5`:
- merge 4 nhánh Buổi 4;
- `62aaf61` — `ApiSurfaceTests` không còn endpoint chờ: **45/45**;
- **`b3cbe28 feat(obs): complete FR-OBS-001 docker healthcheck wiring, nginx failover and health ui indicator`**.

Chi tiết `b3cbe28`:
- healthcheck `api` (`curl -fsS http://127.0.0.1:8080/health/ready`, cài `curl` ở stage runtime vì `aspnet:10.0` không có sẵn);
- healthcheck `frontend` (`wget` của busybox);
- `nginx` đổi `depends_on` sang `api: { condition: service_healthy }`;
- `proxy_next_upstream error timeout http_502 http_503 http_504` trong `nginx.conf.template`;
- `app/api/health/route.ts` (proxy), `HealthIndicator` (poll 30 giây), trang `/dashboard/system`;
- `RecipeCard` dùng `ThumbnailUrl`, dự phòng `OriginalUrl` khi job đổi kích thước (FR-JOB-002) chưa chạy xong.

**Giải trình thiết kế (giữ nguyên):**

**Ba endpoint cho ba mục đích:** `/health/live` trả lời *"process còn sống không?"* — không thì **khởi động lại container**. `/health/ready` trả lời *"instance có sẵn sàng nhận traffic không?"* — không thì **ngừng chuyển traffic**, nhưng **không** khởi động lại. Gộp làm một thì Redis chết sẽ khiến mọi container API bị khởi động lại liên tục — trong khi vấn đề nằm ở Redis, khởi động lại API không giải quyết được gì. **MinIO không gắn tag `ready`**: mất MinIO là suy giảm (không tải ảnh lên được), không phải ngừng phục vụ.

**Khối `healthcheck:` trong Docker Compose là phần bắt buộc:** viết xong endpoint mà không khai báo `healthcheck:` thì **không có ai gọi chúng**. Docker Compose, khác Kubernetes, không tự tìm `/health/ready`; phải khai báo tường minh mới có hành vi "ngừng chuyển traffic khi DB ngừng" mà NFR-REL-001 yêu cầu (MT-29 — gỡ mọi nhắc tới Kubernetes khỏi SRS và bổ sung `healthcheck:` thay vì chỉ xóa chữ).

---

# BUỔI 6 – TIỆN ÍCH GỌI API, SHADCN/UI, ZUSTAND STORE, GIAO DIỆN TĨNH & BỐ CỤC DÙNG CHUNG (LAB 6)

> **Yêu cầu tối thiểu Lab 6 của giảng viên:** dựng khung giao diện dùng chung, hệ thống quản lý state phía client và thư viện UI component.
>
> **Mục tiêu kỹ thuật:** sau buổi này, mọi màn hình của Buổi 7 – 8 được dựng trên **một** nền chung:
> - **một** lớp gọi API (`lib/api-client.ts` trên `ky`);
> - **một** cách xử lý lỗi (`lib/error-handler.ts`);
> - **một** nơi giữ phiên đăng nhập (`stores/auth-store.ts`);
> - **một** bộ component (`components/ui` — shadcn/ui);
> - **ba** bố cục theo route group: `(public)`, `(auth)`, `(dashboard)`.
>
> **Phân vai state — quy tắc cả nhóm tuân thủ từ buổi này:**
>
> | Loại state | Ví dụ | Nơi giữ | Lý do |
> |---|---|---|---|
> | Dữ liệu từ server | công thức, danh mục, hồ sơ, danh sách phiên | **TanStack Query** (cache theo query key, refetch, invalidate) | Server là nguồn sự thật; cần cache, đồng bộ lại, đánh dấu cũ |
> | Phiên đăng nhập phía client | access token, refresh token, `user` rút gọn | **Zustand** `stores/auth-store.ts` | Phải đọc được ngoài React (lớp gọi API), lưu bền một phần, đồng bộ giữa các tab |
> | State của biểu mẫu | giá trị ô nhập, lỗi từng ô | **React Hook Form** + Zod | Không re-render cả cây khi gõ |
> | State chia sẻ qua URL | bộ lọc, trang, tab đang chọn | **`searchParams`** | Chia sẻ link, back/forward, SSR đọc được |
>
> **Không đưa dữ liệu server vào Zustand:** làm vậy là tự viết lại cơ chế cache, refetch và invalidate mà TanStack Query đã có — và sinh ra hai nguồn sự thật lệch nhau.
>
> **Render từ buổi này (MT-67, đã kiểm chứng):** CSP có nonce (Dev 1) làm mọi trang HTML render theo request. Không trang nào dùng `export const revalidate` nữa; độ tươi của dữ liệu công khai đặt trên **từng lời gọi `fetch`** (`next: { revalidate }` = TTL ở §2.3).
>
> **Hai commit nền đầu buổi (30 – 45 phút đầu, merge vào `develop` trước khi 4 dev tách việc):**
> 1. **Dev 4 — `refactor(app): move routes into (public), (auth) and (dashboard) route groups`**: chỉ `git mv` file route vào ba route group. URL **không đổi** vì tên trong ngoặc tròn không xuất hiện trên URL. Không sửa nội dung file.
> 2. **Dev 2 — `chore(ui): initialize shadcn/ui with tailwind v4 theme tokens`**: `components.json`, `lib/utils.ts` (`cn`), token màu trong `globals.css`. Dev 3 cần `Button`, `Card`, `Badge` cho component dùng chung nên phần khởi tạo phải vào trước.

## DEV 1 — Zustand Auth Store, chặn route phía client & Content-Security-Policy (Lab 6)

**1. Tính năng & mã yêu cầu:** Lab 6 – Dev 1. `NFR-SEC-002` (access token chỉ trong bộ nhớ, refresh token là dữ liệu xác thực duy nhất lưu bền) · `NFR-SEC-005` (CSP) · phần Frontend của `FR-AUTH-004` (giữ phiên qua lần tải lại trang).

**2. Các bước triển khai:**

*Luồng dữ liệu:*
- **Đăng nhập:** `LoginForm` → `useAuth().login()` → `apiFetch('/auth/login')` → `AuthResponse` → `authStore.setSession()`.
  - Access token + `expiresAt` → **bộ nhớ**.
  - `{ refreshToken, user }` → `localStorage` (qua `persist` + `partialize`).
- **Gọi API:** `api-client` đọc token qua callback `getAccessToken` mà `AuthStoreProvider` đăng ký → gặp 401 `AUTH_TOKEN_EXPIRED` → `refresh()` single-flight (khóa liên tab Web Locks) → `setSession()` → thử lại request.
- **Tải lại trang:** `AuthBootstrap` → `persist.rehydrate()` → có refresh token thì gọi `/auth/refresh` → `status = 'authenticated'`.

1. **`stores/auth-store.ts` — store tạo theo từng cây React, không phải biến toàn cục:**
   ```ts
   export type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';
   export interface AuthState {
     status: AuthStatus; user: User | null; accessToken: string | null; expiresAt: string | null; refreshToken: string | null;
     setSession: (r: AuthResponse) => void; setUser: (u: User) => void; clear: () => void;
   }
   export const createAuthStore = () => createStore<AuthState>()(persist((set) => ({ … }), {
     name: 'culinaryblog.auth', version: 1, storage: createJSONStorage(() => legacyAwareLocalStorage),
     partialize: (s) => ({ refreshToken: s.refreshToken, user: s.user }),   // KHÔNG BAO GIỜ có accessToken
     skipHydration: true,                                                     // hydrate trong effect → không lệch HTML SSR
   }));
   ```
   - **`AuthStoreProvider`** (`stores/auth-store-provider.tsx`) tạo store **một lần** bằng `useRef` và cấp qua React Context. Hook `useAuthStore(selector)` dùng selector, kèm `useShallow` khi lấy nhiều trường — component chỉ re-render khi đúng phần nó đọc thay đổi.
   - **`legacyAwareLocalStorage`** — adapter đọc khóa `culinaryblog.auth` ở **định dạng của Buổi 4** (`{ refreshToken, user }`, không có lớp vỏ `{ state, version }` của Zustand) và chuyển sang định dạng mới. Người dùng đang đăng nhập từ trước khi cập nhật **không bị đăng xuất**.
   - Đồng bộ giữa các tab: lắng nghe sự kiện `storage` → `persist.rehydrate()`; refresh token bị tab khác xóa (đăng xuất) → `clear()` + `queryClient.clear()`. Giữ nguyên `coordinateRefresh` (Web Locks, Buổi 4) — chuyển sang `features/auth/session-sync.ts`.
2. **Mặt tiền `useAuth()` — giữ nguyên chữ ký để không ai phải sửa code đang dùng:**
   - `features/auth/use-auth.ts` trả đúng `{ user, accessToken, isAuthenticated, isReady, login, loginWithGoogle, register, logout }` như `AuthContext` của Buổi 2 – 5, nhưng đọc/ghi qua store.
   - `features/auth/auth-context.tsx` còn lại một dòng re-export `useAuth`, đánh dấu `@deprecated`; xóa ở Buổi 8.
   - Trang soạn công thức (Dev 2), trang quản trị danh mục (Dev 3), `HealthIndicator` (Dev 4) **không sửa một dòng nào**.
   - `app/providers.tsx`: thay `<AuthProvider>` bằng `<AuthStoreProvider>` + `<AuthBootstrap />`.
3. **Chặn route phía client — `features/auth/components/`:**
   - `AuthGuard.tsx`:
     - `status === 'unknown'` → `LoadingSkeleton` (đang khôi phục phiên);
     - `'anonymous'` → `router.replace('/auth/login?callbackUrl=' + encodeURIComponent(pathname + search))`;
     - `'authenticated'` → render con.

     Dev 4 bọc nó trong `(dashboard)/layout.tsx`.
   - `RequireRole.tsx` (`roles: Role[]`, `fallback` mặc định là màn hình 403) — dùng cho `/dashboard/categories`, `/dashboard/users`, `/dashboard/system`.
   - `GuestOnly.tsx` cho `(auth)` — đã đăng nhập thì chuyển hướng về `callbackUrl` hoặc `/dashboard`.
   - **`lib/safe-redirect.ts` — `toSafeInternalPath(callbackUrl)`:** chỉ chấp nhận đường dẫn bắt đầu bằng một dấu `/`, không phải `//` hoặc `/\`, không chứa `:`. Ngược lại trả `/dashboard`. Chặn lỗ hổng **chuyển hướng mở (open redirect)**: không có bước này, link `…/auth/login?callbackUrl=//evil.com` gửi người dùng tới trang giả mạo **ngay sau khi họ đăng nhập thật**.
   - **Đính chính kế hoạch cũ:** kế hoạch Buổi 6 cũ dùng `middleware.ts` của Next.js để chặn `/dashboard/**`. Cách đó **không chạy được** với thiết kế token của SRS: middleware chạy trên server và chỉ thấy cookie, trong khi NFR-SEC-002/005 cấm cookie xác thực và token nằm trong bộ nhớ/`localStorage` của trình duyệt. Server không có cách nào biết người dùng đã đăng nhập. Chặn route vì vậy làm ở client — và như kế hoạch cũ đã ghi, đó là **trải nghiệm**, không phải bảo mật: quyền thật luôn kiểm ở Backend (NFR-SEC-006). `middleware.ts` của buổi này chỉ làm một việc: sinh nonce CSP (bước 4).
4. **Content-Security-Policy có nonce (NFR-SEC-005 — đã chốt ở CR-2026-05 / MT-67, bằng chứng ở §4.6):**
   - **`src/middleware.ts`** (tên file của Next.js 15) sinh nonce mới cho **mỗi request**: `Buffer.from(crypto.randomUUID()).toString('base64')`, đặt vào header request `x-nonce` và header `Content-Security-Policy` của cả request lẫn response. `matcher` bỏ qua `api`, `_next/static`, `_next/image`, `favicon.ico`, `sitemap.xml`, `robots.txt` và request prefetch (`next-router-prefetch`).
   - **Chính sách** (dựng trong `lib/security/csp.ts`, có unit test):
     - `default-src 'self'`;
     - `script-src 'self' 'nonce-{nonce}' 'strict-dynamic' https://accounts.google.com/gsi/client` — `'strict-dynamic'` cho phép script do script có nonce nạp tiếp (thư viện Google Identity Services tự chèn thẻ script của nó); **không** `'unsafe-inline'`;
     - `style-src 'self' 'unsafe-inline' https://accounts.google.com/gsi/style` (CSS không chạy được mã; Google chèn style động);
     - `img-src 'self' data: blob: https://lh3.googleusercontent.com`; `font-src 'self'`;
     - `connect-src 'self' https://accounts.google.com/gsi/`; `frame-src https://accounts.google.com/gsi/`;
     - `object-src 'none'`; `base-uri 'self'`; `form-action 'self'`; `frame-ancestors 'none'`; `upgrade-insecure-requests` (chỉ production).
     - Môi trường dev thêm `'unsafe-eval'` vào `script-src` (React dev cần), production không có.
   - **Root layout** (`app/layout.tsx` của Dev 4) thành async Server Component, gọi `await headers()` và đọc `x-nonce` → Next.js tự gắn nonce cho mọi script của framework; truyền `nonce` cho `GoogleOAuthProvider` nếu cần. Dòng này làm **mọi trang render theo request** — đúng kết luận của MT-67.
   - **Triển khai an toàn:** 1 ngày đầu gửi `Content-Security-Policy-Report-Only` (cùng chính sách) để bắt vi phạm trên mọi route mà không làm hỏng trang; hết vi phạm thì đổi sang header chặn thật.
   - **Kiểm thử** (`frontend/e2e/csp.spec.ts`, Playwright): mỗi route công khai, dashboard, đăng nhập (kể cả nút Google) — 0 vi phạm CSP trong console, trang hydrate (một phần tử chỉ xuất hiện sau `useEffect`), hai lần tải liên tiếp có **nonce khác nhau**. Thêm unit test `csp.test.ts`: production không có `'unsafe-inline'` lẫn `'unsafe-eval'` trong `script-src`.
   - Các header khác đặt cùng `middleware.ts`: `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy: camera=(), microphone=(), geolocation=()`. HSTS đặt ở Nginx (Buổi 7 — Dev 4).
5. **Kiểm thử (Jest + Testing Library):**
   - `auth-store.test.ts`:
     - sau `setSession`, chuỗi JSON trong `localStorage` **không chứa** `accessToken`;
     - dữ liệu định dạng Buổi 4 được chuyển đổi, phiên giữ nguyên;
     - `clear()` xóa sạch;
     - sự kiện `storage` (tab khác đăng xuất) → store về `anonymous`.
   - `AuthGuard.test.tsx` — chuyển hướng kèm `callbackUrl` đã mã hóa; `RequireRole` hiện 403 cho Author.
   - `safe-redirect.test.ts` — `//evil.com`, `/\evil.com`, `javascript:alert(1)`, `https://evil.com` đều thành `/dashboard`.
   - Kiểm thử hồi quy `api-client.test.ts` (refresh single-flight) xanh nguyên vẹn.

**3. Định hướng & lý do kiến trúc:**

**Vì sao Zustand thay cho React Context của Buổi 2 – 5:**
- **Context re-render mọi component tiêu thụ nó** mỗi khi bất kỳ trường nào đổi. Refresh token xoay vòng 15 phút một lần làm cả header, menu, mọi trang dashboard re-render dù chỉ cần biết "đã đăng nhập chưa". Zustand cho mỗi component đăng ký đúng **lát** state nó đọc.
- **`persist` + `partialize` khai báo đúng một lần** điều NFR-SEC-002 đòi hỏi — *trường nào được lưu bền* — thay cho các hàm `readSession`/`writeSession` tự viết. Kiểm thử chỉ cần kiểm một chỗ.

**Vì sao store được tạo theo cây React, không phải `create()` ở cấp module:** trong App Router, Client Component **vẫn được render trước trên server**. Store khai báo ở cấp module là một biến dùng chung cho **mọi request** trong process Node của container `frontend`. Store thường (giỏ hàng, theme) thì vô hại. Store phiên đăng nhập thì là nguy cơ lộ dữ liệu giữa người dùng nếu sau này có code ghi vào store trong lúc render. Tạo store trong `useRef` của Provider (cách tài liệu Zustand khuyến nghị cho Next.js) loại bỏ hẳn khả năng đó.

**Vì sao không lưu token vào HTTP-only cookie dù Lab 6 cho chọn:**
- SRS §5.2 chốt *"refresh token trong request body, không dùng cookie để tránh CSRF"*; NFR-SEC-005 ghi rõ *"chuyển refresh token sang cookie phải qua Change Request"*.
- JavaScript **không ghi được** cookie HTTP-only — muốn có thì Backend phải đổi `/auth/login`, `/auth/refresh`, `/auth/logout` sang `Set-Cookie`, thêm chống CSRF, đổi CORS sang `AllowCredentials`. Đó là viết lại luồng đã có integration test từ Buổi 2 – 4.

Lựa chọn `localStorage` của Lab được áp dụng đúng phạm vi SRS cho phép: chỉ refresh token; access token không bao giờ rời bộ nhớ (X-7, §3).

**Vì sao chọn CSP có nonce, chấp nhận render theo request (MT-67):** thực nghiệm ở §4.6 cho thấy CSP dựa trên hash (`experimental.sri`) **không chạy** với App Router — SRI chỉ gắn `integrity` cho file script ngoài, còn dữ liệu RSC luôn đi trong script inline nên bị chặn và trang không hydrate. Hai lối ra còn lại là nonce (mọi trang render theo request) hoặc `'unsafe-inline'` (giữ trang tĩnh). `'unsafe-inline'` cho phép **mọi** script inline chạy — kể cả script kẻ tấn công chèn vào — nên CSP mất đúng tác dụng nó được đặt ra. Render theo request thì tốn CPU của container `frontend`, nhưng **không tốn thêm lượt gọi API** vì Data Cache của `fetch` vẫn giữ dữ liệu đúng TTL (đã đo). Với một blog mà tài sản cần bảo vệ là refresh token của người viết, đánh đổi đúng là giữ bảo mật.

**CSP là lớp phòng vệ cuối cho refresh token:** refresh token nằm ở nơi JavaScript đọc được — đó là đánh đổi SRS chấp nhận để tránh CSRF. Nghĩa là **bất kỳ lỗ hổng XSS nào** cũng đọc được nó. Rotation + Reuse Detection (Buổi 4) giới hạn thiệt hại *sau khi* token bị lấy. CSP ngăn script lạ chạy *ngay từ đầu*. Store lưu token và CSP bảo vệ token làm cùng một buổi, cùng một người, để không có khoảng thời gian token được lưu mà chưa có lớp bảo vệ.

**Giữ chữ ký `useAuth()` (mặt tiền – facade):** thay kho chứa phiên là thay đổi xuyên suốt. Nếu đổi luôn API thì mọi trang của ba dev còn lại phải sửa cùng lúc trong một buổi → xung đột merge ở file của người khác. Giữ chữ ký cũ, chỉ thay phần bên trong, thì thay đổi nằm gọn trong module Auth và có thể merge độc lập.

**4. Commit gợi ý:**
```
feat(auth-state): zustand auth store with refresh-token-only persistence, legacy session migration, client route guards and nonce-based content security policy
```

---

## DEV 2 — Thư viện UI shadcn/ui & Trang Tổng quan Dashboard (Lab 6)

**1. Tính năng & mã yêu cầu:** Lab 6 – Dev 2. `NFR-USE-001` (responsive, Tailwind utility-first) · `NFR-USE-002` (WCAG 2.1 AA) · `NFR-USE-003` (lỗi hiển thị bên cạnh ô nhập) · `NFR-USE-004` (trạng thái tải). Màn hình dùng thật đầu tiên: `/dashboard` — tổng quan công thức theo trạng thái (SRS §5.1, dựa trên `FR-RCP-011`).

**2. Các bước triển khai:**

1. **Commit nền — khởi tạo shadcn/ui** (ghim phiên bản CLI: `npx shadcn@4.21.4 init`):
   - `components.json`: `rsc: true`, `tsx: true`, alias `components: @/components`, `ui: @/components/ui`, `utils: @/lib/utils`, `hooks: @/hooks`.
   - `lib/utils.ts`: `export const cn = (...inputs: ClassValue[]) => twMerge(clsx(inputs));` (dùng `clsx` + `tailwind-merge` có từ Buổi 2).
   - `globals.css`: token màu theo cách Tailwind v4 (`@theme inline` + biến CSS `--background`, `--foreground`, `--primary`, `--destructive`, `--ring`, `--radius`…).
   - **Màu thương hiệu kiểm tra độ tương phản trước khi chọn:** code hiện dùng `bg-orange-600 text-white` cho nút. Chữ trắng trên `#ea580c` có tỉ lệ tương phản **3,56 : 1** — **không đạt** WCAG AA cho chữ thường (cần ≥ 4,5 : 1). Token `--primary` đặt bằng tông `orange-700` (`#c2410c`, tỉ lệ **5,18 : 1**). Màu cam vẫn là màu thương hiệu, chỉ đậm hơn một bậc.
   - Gói kéo theo: `class-variance-authority`, `tw-animate-css`, các gói Radix UI — ghi ➕ vào `CONG_NGHE_VA_PHIEN_BAN.md`.
2. **Thêm component** — `npx shadcn@4.21.4 add …`, chép vào `components/ui/`:

   | Nhóm | Component | Dùng ở |
   |---|---|---|
   | **8 component Lab 6 yêu cầu** | `button`, `input`, `dialog`, `dropdown-menu`, `sonner` *(thay cho `toast`)*, `form`, `card`, `badge` | Mọi màn hình |
   | Phụ trợ — component Lab 6 cần hoặc Buổi 7 – 8 dùng ngay | `label` (`form` cần), `textarea`, `select`, `alert-dialog` (xác nhận xóa/khóa), `sheet` (menu điện thoại, drawer bộ lọc), `tabs` (tab trạng thái, tab hồ sơ), `skeleton`, `table` (`/dashboard/users`, `/dashboard/recipes`) | Buổi 6 – 8 |

   - **`Toast` → `sonner`:** component `toast` của shadcn đã ngừng phát triển, chuyển sang `sonner`. Dự án dùng thư viện `sonner` từ Buổi 2, nên `components/ui/sonner.tsx` chỉ bọc `Toaster` với token màu. Mọi lời gọi `toast(...)` đang có giữ nguyên (X-11, §3).
   - **`Form`:** `components/ui/form.tsx` (`Form`, `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormDescription`, `FormMessage`) xây trên `Controller` + `useFormContext` của React Hook Form.
     - `FormControl` tự gắn `aria-invalid`, `aria-describedby` trỏ tới `FormMessage` — đúng thuộc tính trình đọc màn hình cần (WCAG 3.3.1).
     - `mapProblemDetailsToForm()` (Buổi 3, D-11) gọi `setError` → lỗi 400 từ Backend hiện ngay dưới đúng ô.
     - `components/shared/FormField.tsx` (Buổi 2) đánh dấu `@deprecated`; chủ từng biểu mẫu chuyển sang khi mở lại file ở Buổi 7 – 8; xóa ở Buổi 8 khi không còn chỗ dùng.
   - **Chỉnh trực tiếp trong code đã chép** (shadcn chép code vào dự án chính là để nhóm sửa): `Button` thêm prop `isLoading` → `disabled` + `aria-busy="true"` + biểu tượng `Loader2` quay. Mọi nút gửi biểu mẫu dùng chung, không ai tự viết spinner riêng.
3. **Component miền Recipe dùng thư viện mới — `features/recipes/components/RecipeStatusBadge.tsx`:** ánh xạ `Draft → secondary`, `Published → default`, `Archived → outline` kèm nhãn tiếng Việt. Dùng ở dashboard (buổi này), danh sách "Công thức của tôi" (Buổi 7), trang sửa (Buổi 8).
4. **Trang `(dashboard)/dashboard/page.tsx` (CSR) — màn hình dùng thật đầu tiên của thư viện:**
   - `useQueries` gồm 3 truy vấn `getMyRecipes({ status, pageSize: 1 })` với `status` ∈ Draft/Published/Archived. Mỗi truy vấn chỉ lấy `totalCount` từ `PagedResult` — **không cần endpoint thống kê riêng**.
   - Query key `recipeKeys.mine({ status, pageSize: 1 })` trong `features/recipes/query-keys.ts` (khai báo tại đây, Buổi 7 – 8 dùng tiếp).
   - 3 `Card` số liệu + `Card` "Viết công thức mới" (link `/dashboard/recipes/new`); đang tải hiện `Skeleton`; lỗi dùng `ErrorBoundary` của Dev 3.
   - Endpoint `GET /recipes/mine` mang `Cache-Control: no-store` (Buổi 4) → `staleTime: 0`, không giữ dữ liệu riêng tư lâu trong cache phía client.
5. **Kiểm thử:**
   - `button.test.tsx` — `isLoading` → `disabled` + `aria-busy`;
   - `dialog.test.tsx` — mở bằng bàn phím, `Escape` đóng, focus quay về nút mở (Radix);
   - `form.test.tsx` — lỗi Zod hiện ở `FormMessage`, ô nhập có `aria-invalid="true"` và `aria-describedby` trỏ đúng;
   - `RecipeStatusBadge.test.tsx` — đủ 3 trạng thái (lặp trên `RECIPE_STATUSES`).

**3. Định hướng & lý do kiến trúc:**

**Vì sao shadcn/ui hợp với dự án này:**
- **Không thay công nghệ của SRS:** NFR-USE-001 chỉ định Tailwind CSS utility-first và *"không sử dụng CSS framework override"*. shadcn/ui là component viết bằng chính class Tailwind, không mang theo CSS framework riêng như Bootstrap hay MUI.
- **Accessibility có sẵn:** phần hành vi do Radix UI đảm nhận — focus trap trong dialog, `Escape` đóng, điều hướng menu bằng phím mũi tên, `aria-*` đúng chuẩn. Đây chính là những mục `@axe-core/playwright` sẽ kiểm ở Buổi 8. Tự viết `Dialog` đúng WCAG mất cả ngày; dùng Radix thì có ngay.
- **Code nằm trong repo:** sửa được (`isLoading`), không bị khóa vào phiên bản của một gói thư viện, và cập nhật có chủ đích qua CLI.

**Token màu thay cho class màu gắn cứng:** khi 40 component viết thẳng `bg-orange-600`, đổi màu thương hiệu (hoặc sửa lỗi tương phản như trên) là sửa 40 file. Với token, sửa **một** biến CSS. Lỗi tương phản 3,56 : 1 là ví dụ cụ thể: phát hiện bây giờ thì sửa một dòng; để Buổi 8 axe phát hiện thì phải tìm khắp code.

**Màn hình dùng thật thay cho trang "demo component":** một thư viện component chỉ chứng minh được giá trị khi nó dựng nên màn hình thật. Trang tổng quan dashboard là màn hình SRS §5.1 yêu cầu, dùng đủ `Card`, `Badge`, `Skeleton`, `Button` — và là chỗ kiểm chứng sớm luồng `/recipes/mine` (no-store) với TanStack Query.

**4. Commit gợi ý:**
```
feat(ui): shadcn/ui primitives (button, input, dialog, dropdown-menu, sonner, form, card, badge) on wcag-checked theme tokens and dashboard overview
```

---

## DEV 3 — Giao diện tĩnh & Component dùng chung (Lab 6) + bộ lọc thời gian cho `/recipes/search` (CR-2026-05 / MT-66)

**1. Tính năng & mã yêu cầu:** Lab 6 – Dev 3. `NFR-USE-001` (responsive 320/768/1200 px) · `NFR-USE-002` (landmark, ARIA) · `NFR-USE-004` (trạng thái tải, CLS) · `NFR-PERF-005` (CLS ≤ 0,1). Kèm phần API của **MT-66** (`FR-SRCH-001` mở rộng bộ lọc) — Dev 3 là chủ API tìm kiếm từ Buổi 4.

**2. Các bước triển khai:**

1. **Component khung trang — `components/layout/`:**
   - **`Header.tsx`** (Server Component) — thay `SiteHeader.tsx` của Buổi 2. Gồm:
     - logo;
     - `CategoryNav` (Buổi 3): lấy `getCategories()` với `revalidate: 1800`;
     - `SearchBar` (Buổi 5);
     - `UserMenu` (Client): đọc `useAuth()`; `DropdownMenu` gồm Hồ sơ, Dashboard, Quản lý danh mục và Người dùng (chỉ Admin), Đăng xuất.
     - Trên điện thoại: nút menu mở `Sheet`.
   - **`Footer.tsx`** (Server) — landmark `<footer>`, liên kết danh mục, bản quyền.
   - **`SidebarCategory.tsx`** (Server, nhận `categories: Category[]`, `activeSlug?`) — `<nav aria-label="Danh mục">`, số công thức hiển thị bằng `Badge`, mục đang xem có `aria-current="page"`. Không gửi JavaScript xuống trình duyệt. Dùng ở trang chủ và trang danh mục (Buổi 7).
2. **Component dùng chung — `components/shared/`:**
   - **`Pagination.tsx`** (đã chuyển thư mục ở Buổi 5) — nâng cấp thành `<nav aria-label="Phân trang">`:
     - số trang có dấu `…`, trang hiện tại có `aria-current="page"`;
     - nhận `buildHref(page)` → link thật (`<Link>`), **chạy được không cần JavaScript** và giữ nguyên các tham số lọc khác trên URL;
     - dựa trên `PagedResult` (`totalPages`, `hasNextPage`, `hasPreviousPage`).
   - **`LoadingSkeleton.tsx`** — biến thể `recipe-grid` (thay `RecipeGridSkeleton`), `recipe-detail`, `table`, `form`. Kích thước khung xương **bằng đúng** kích thước thật (cùng `aspect-ratio` ảnh 4 : 3, cùng chiều cao thẻ), nên khi dữ liệu về trang không nhảy bố cục (CLS).
   - **`EmptyState.tsx`** — biểu tượng `lucide-react`, tiêu đề, mô tả, khe `action`. Dùng cho: không có kết quả tìm kiếm, danh mục trống, chưa có công thức nào.
   - **`ErrorBoundary.tsx`** (Client, **lớp – class component**, vì React chỉ cho class bắt lỗi render) — nhận `fallback`, kết hợp `QueryErrorResetBoundary` của TanStack Query để nút "Thử lại" vừa reset boundary vừa refetch truy vấn lỗi.
   - **`ErrorFallback.tsx`** — giao diện lỗi dùng chung: thông báo thân thiện, **mã tra cứu** (`correlationId` mà `ApiError` mang theo — Dev 4), nút thử lại.
   - Các file `error.tsx` / `global-error.tsx` / `not-found.tsx` của ba route group dùng `ErrorFallback` và `EmptyState`.
3. **Giao diện tĩnh — dựng bằng chính component thật và dữ liệu mẫu thật:**
   - `src/mocks/fixtures/recipes.ts`, `categories.ts` — **4 công thức và 6 danh mục lấy nguyên từ dữ liệu mẫu** `Infrastructure/Persistence/Seed/Data/*.json` (ví dụ "Phở bò": đủ nguyên liệu có định lượng, đủ bước). Khai báo `satisfies Recipe[]` / `satisfies Category[]` → đổi kiểu TypeScript mà quên dữ liệu mẫu là lỗi biên dịch.
   - Route xem bố cục: `app/(dev)/mockups/{home,recipe,category,search,dashboard}/page.tsx` — ráp `Header`, `Footer`, `SidebarCategory`, lưới `RecipeCard`, `Pagination`, `EmptyState`, `LoadingSkeleton` với dữ liệu trên.
   - `app/(dev)/layout.tsx` gọi `notFound()` khi `NODE_ENV === 'production'` → bản build production trả 404 cho mọi trang mockup; `robots.ts` (Buổi 8) cũng chặn `/mockups`.
   - Rà bố cục ở 320 / 768 / 1200 px trước khi Buổi 7 nối API thật. Ảnh chụp ba kích thước đưa vào báo cáo Lab 06.
4. **CR-2026-05 / MT-66 — bộ lọc thời gian cho `GET /recipes/search`** (phần API):
   - `SearchRecipesQuery` thêm `MaxCookTime`, `MaxPrepTime`, `MinServings` → áp qua **`RecipeFilterSpec` dùng chung** (Buổi 4) — cùng điều kiện với `GET /recipes`, không viết bộ lọc thứ hai.
   - `SearchRecipesQueryValidator`: dùng lại đúng quy tắc khoảng giá trị của `GetRecipesQueryValidator` (tách thành `RecipeFilterRules` dùng chung cho hai validator).
   - Endpoint (`RecipeQueryEndpoints.cs`) nhận thêm ba tham số qua `[AsParameters]`. Sắp xếp **không** thêm — kết quả tìm kiếm luôn xếp theo độ liên quan `ts_rank` (FR-SRCH-001).
   - Khóa cache `search:{queryHash}`: kiểm thử khẳng định hai request khác nhau chỉ ở `maxCookTime` sinh **hai khóa khác nhau** — không trả nhầm kết quả của bộ lọc khác.
   - Integration test:
     - `q=pho&maxCookTime=30` chỉ ra món nấu ≤ 30 phút;
     - `maxCookTime=-1` → 400 `VALIDATION_ERROR`;
     - không gửi tham số mới → kết quả như cũ (tương thích ngược).
   - Frontend: thêm ba trường vào `RecipeSearchParams` (`types/search.ts`) và `searchRecipes()`. Giao diện bộ lọc là việc của Dev 4 ở Buổi 7.
5. **Kiểm thử:**
   - `Pagination.test.tsx` — dấu `…` đúng ở trang đầu/giữa/cuối, `aria-current`, `buildHref` giữ tham số lọc;
   - `ErrorBoundary.test.tsx` — component con ném lỗi → hiện fallback; bấm "Thử lại" → render lại;
   - `SidebarCategory.test.tsx` — `aria-current` đúng mục.

**3. Định hướng & lý do kiến trúc:**

**Giao diện tĩnh làm bằng route trong app, không dùng Storybook:**
- Storybook thêm hàng trăm MB gói thư viện và một bộ cấu hình riêng phải giữ khớp với Tailwind v4, Next.js 15, font, provider.
- Route mockup dùng **chính** `layout`, CSS, provider của ứng dụng: thứ nhìn thấy trên mockup là thứ người dùng sẽ thấy.
- Dữ liệu mẫu ràng bằng `satisfies Recipe[]` nên mockup **gãy biên dịch khi hợp đồng đổi** — mockup trở thành kiểm thử bố cục sống, không phải ảnh chết.
- Dùng công thức thật từ seed (không Lorem ipsum) để thấy đúng các ca khó: tên món dài, 14 nguyên liệu, mô tả nhiều dòng.

**Component khung là Server Component:** `Header`, `Footer`, `SidebarCategory` không có tương tác (trừ `UserMenu`, `SearchBar` được tách thành Client Component nhỏ bên trong). Để chúng ở server thì phần lớn khung trang không tốn byte JavaScript nào ở trình duyệt — trực tiếp giúp chỉ tiêu First Load JS ≤ 200 KB (NFR-PERF-005).

**Phân trang bằng link thật:** `<Link href="?page=3&difficulty=Easy">` hoạt động cả khi JavaScript chưa tải xong, crawler theo được, và giữ nguyên bộ lọc. Phân trang bằng `onClick` + state là phân trang crawler không đi theo được và mất bộ lọc khi F5.

**`ErrorBoundary` phải là class component:** React chưa có hook để bắt lỗi trong lúc render (`componentDidCatch` / `getDerivedStateFromError` chỉ có ở class). App Router có `error.tsx` cho cấp route, nhưng muốn một widget lỗi (ví dụ khối "công thức mới") không kéo sập cả trang thì phải có boundary cấp component.

**Vì sao mở rộng `/recipes/search` qua Change Request thay vì lọc ở trình duyệt:** Lab 7 đòi trang `/search` lọc theo thời gian, nhưng SRS v1.2.2 §8.3 không cho endpoint tìm kiếm nhận tham số đó. Lọc ở client trên trang kết quả đã phân trang là **sai về dữ liệu**: trang 1 có 12 kết quả, lọc ≤ 30 phút còn 3 kết quả, trong khi trang 2 – 5 có thể còn chục món khớp mà người dùng không bao giờ thấy, và `totalCount` sai. Bộ lọc phải chạy ở cơ sở dữ liệu. CR-2026-05 chỉ thêm tham số tùy chọn vào endpoint có sẵn (không thêm endpoint, tương thích ngược), và `RecipeFilterSpec` dùng chung khiến phần code mới gần như bằng không.

**4. Commit gợi ý:**
```
feat(ui-shell,search): shared layout and accessible pagination, skeleton, empty state and error boundary with typed static mockups; search time filters (CR-2026-05 MT-66)
```

---

## DEV 4 — Lớp gọi API `ky`, Bộ xử lý lỗi toàn cục & Bố cục theo Route Group (Lab 6)

**1. Tính năng & mã yêu cầu:** Lab 6 – Dev 4. Phần Frontend của `FR-AUTH-004` (gắn Bearer, refresh khi gặp 401) · `NFR-USE-003` (thông báo lỗi theo mã, không lộ chi tiết kỹ thuật) · `NFR-SEO-002` (`noindex` cho trang riêng tư) · phần Frontend của `FR-OBS-002` (gửi `X-Correlation-ID`).

**2. Các bước triển khai:**

*Luồng dữ liệu:* component → TanStack `useQuery`/`useMutation` → hàm API của module (`features/*/api.ts`) → **`apiFetch`** (instance `ky`: gắn `X-Correlation-ID` + Bearer → gửi → 401 thì refresh rồi thử lại → lỗi thì đổi sang `ApiError`) → Nginx → API → `ProblemDetails` → `QueryCache.onError` / `MutationCache.onError` → **`handleApiError`** → lỗi từng ô của biểu mẫu hoặc toast.

1. **Commit nền — route group** (`git mv`, không sửa nội dung):
   ```
   app/(public)/  page.tsx, recipes/**, categories/**, search/**
   app/(auth)/    auth/login, auth/register
   app/(dashboard)/ dashboard/**, profile/**
   app/api/health/route.ts   (giữ nguyên — route handler, không thuộc group)
   ```
   - `/dashboard/media` (trang thử `ImageUploader` của Buổi 2) chuyển vào `(dashboard)` và bọc `RequireRole Admin`; xóa ở Buổi 8 khi uploader đã nằm trong biểu mẫu thật.
2. **`lib/api-client.ts` — viết lại phần bên trong bằng `ky`, giữ nguyên API công khai.** `apiFetch<T>(path, options)`, `ApiError`, `withAuthRefresh`, `configureAuthCallbacks`, `mapProblemDetailsToForm` giữ tên và chữ ký → hơn 40 chỗ gọi trong `features/*/api.ts` **không phải sửa**.
   ```ts
   const http = ky.create({
     timeout: 15_000,                             // `prefix: getApiBaseUrl()` truyền theo từng lời gọi (server/trình duyệt khác URL)
     retry: { limit: 2, methods: ['get'], statusCodes: [408, 429, 502, 503, 504], maxRetryAfter: 5_000,
              shouldRetry },                      // Retry-After > 5 giây → dừng hẳn (ky chỉ kẹp về 5 giây rồi vẫn gửi lại)
     hooks: {
       beforeRequest: [attachCorrelationId, attachBearerToken],
       afterResponse: [refreshOnExpiredToken],   // 401 AUTH_TOKEN_EXPIRED → refresh single-flight → thử lại đúng 1 lần
       beforeError:   [toApiError],              // Response → ApiError(status, ProblemDetails, correlationId)
     },
   });
   ```
   - **`ky` 2.x đổi tên `prefixUrl` → `prefix`** (bản 2.2.0 đã ghim ở Buổi 5); đoạn code trên dùng tên mới.
   - **Gắn token:** `attachBearerToken` đọc token qua `authCallbacks.getAccessToken()` — callback do `AuthStoreProvider` của Dev 1 đăng ký. `lib/` không import `stores/`, đúng quy tắc phụ thuộc Buổi 5.
   - **Refresh:** `refreshOnExpiredToken` dùng lại **đúng** logic single-flight đã có kiểm thử (biến `refreshPromise` dùng chung; token đã được request khác làm mới thì không refresh lần hai). Refresh thất bại → `onRefreshFailure()` → xóa phiên, về `/auth/login?callbackUrl=…`. Lỗi `AUTH_TOKEN_INVALID` (token hỏng, không phải hết hạn) → đăng xuất ngay, không thử refresh.
   - **Thử lại:** chỉ `GET`, chỉ lỗi tạm thời. `429` chờ đúng `Retry-After` nếu ≤ 5 giây, lâu hơn thì trả lỗi để giao diện báo người dùng. **`POST`/`PUT`/`PATCH`/`DELETE` không bao giờ tự thử lại** — thử lại một lệnh ghi có thể tạo hai công thức hoặc xuất bản hai lần.
   - **`X-Correlation-ID`:** `crypto.randomUUID()` cho mỗi request (trình duyệt và Node 22 đều có sẵn). `ApiError.correlationId` lấy từ header phản hồi (Buổi 8 Backend trả lại) hoặc `traceId` trong Problem Details (đã có từ Buổi 3).
   - **Server Component chỉ gọi endpoint công khai:** khi chạy trên server, `apiFetch` dùng `API_INTERNAL_BASE_URL` và **không bao giờ gắn token** — server không có token (token nằm ở trình duyệt). Đây là chỗ thực thi nguyên tắc tách bạch công khai / riêng tư (MT-34) ở Frontend: dữ liệu riêng tư chỉ đi qua Client Component.
   - **Tải ảnh lên có tiến trình** (`ImageUploader`, `XMLHttpRequest.upload.onprogress` — NFR-USE-004) giữ đường XHR riêng nhưng dùng chung `withAuthRefresh`. Tiến trình tải lên qua `fetch` cần request stream — Firefox và Safari chưa hỗ trợ.
3. **`lib/error-handler.ts` + `lib/error-messages.ts`:**
   - `normalizeError(error: unknown): AppError` với
     - `kind ∈ { validation, unauthenticated, forbidden, not-found, conflict, rate-limited, server, network, unknown }`;
     - `code: ErrorCode | 'NETWORK_ERROR' | 'UNKNOWN_ERROR'`;
     - `status`, `message`, `fieldErrors`, `retryAfterSeconds?`, `correlationId?`.
   - **`ERROR_MESSAGES` — `satisfies Record<ErrorCode, string>`**: thông báo tiếng Việt cho **đủ** mã của Phụ lục B. Thiếu một mã là lỗi biên dịch. `detail` của Backend chỉ là phương án dự phòng. Đây đúng là yêu cầu *"I18n-ready: error messages sử dụng error code"* của NFR-USE-003.
   - `handleApiError(error, { form?: { fields, setError }, silent? })`:

     | `kind` | Xử lý |
     |---|---|
     | `validation` + có `form` | `mapProblemDetailsToForm` → lỗi từng ô; không có ô nào khớp thì toast |
     | `rate-limited` | toast *"Bạn thao tác quá nhanh, thử lại sau N giây"* (N từ `Retry-After`) |
     | `conflict` | toast câu theo mã (ví dụ *"Tên danh mục đã tồn tại"*) và trả `AppError` cho nơi gọi. Màn hình cần dialog riêng (*"dữ liệu đã thay đổi"* của trang sửa công thức) khai báo `meta: { suppressGlobalError: true }` rồi tự xử lý — không để lỗi 409 của một mutation không có `onError` trôi qua im lặng |
     | `server`, `network` | thông báo thân thiện + *"Mã tra cứu: {correlationId}"* (không lộ stack trace — NFR-USE-003) |
     | `forbidden` | toast *"Bạn không có quyền thực hiện thao tác này"* |
   - `lib/query-client.ts`:
     - `QueryCache({ onError })` và `MutationCache({ onError })` gọi `handleApiError`, trừ khi truy vấn khai báo `meta: { suppressGlobalError: true }`; mutation **tự khai báo `onError`** cũng được bỏ qua (nó đã tự xử lý — không toast lần hai);
     - `retry: (count, err) => ['network','server'].includes(normalizeError(err).kind) && count < 2` — **không thử lại lỗi 4xx**.
   - `getErrorMessage()` (Buổi 2) giữ lại như lớp vỏ mỏng gọi `normalizeError(...).message`.
4. **Bố cục — `app/`:**
   - **`app/layout.tsx` (Root):** `<html lang="vi">`, font Inter có subset `vietnamese`, `metadata` (`title.template = '%s | Culinary Blog'`), `<Providers>` (`QueryClientProvider` + `AuthStoreProvider` + `GoogleOAuthProvider` + `Toaster`), link "Bỏ qua điều hướng". **Không** chứa header/footer — mỗi nhóm trang có khung riêng. Dev 1 (merge ngay sau) biến layout này thành async và đọc `x-nonce` từ `headers()` cho CSP (MT-67) — từ đó mọi trang render theo request, nên **không file trang nào khai báo `export const revalidate`**.
   - **`app/(public)/layout.tsx`:** `Header` + `<main id="main-content">` + `Footer` (component của Dev 3; trước khi Dev 3 merge thì dùng tạm `SiteHeader` cũ — Dev 3 thay một dòng import).
   - **`app/(auth)/layout.tsx`:** khung căn giữa có logo, bọc `GuestOnly` (Dev 1).
   - **`app/(dashboard)/layout.tsx` — Server Component** để khai báo được `export const metadata = { robots: { index: false, follow: false } }` (NFR-SEO-002 — Client Component không được export `metadata`). Bên trong là `<DashboardShell>` (Client):
     - bọc `AuthGuard`;
     - `components/layout/DashboardSidebar.tsx`: Tổng quan, Công thức của tôi, Viết công thức, Hồ sơ; Danh mục, Người dùng, Hệ thống (chỉ Admin); mục đang mở theo `usePathname()` có `aria-current="page"`;
     - thanh trên cùng có `UserMenu`;
     - điện thoại: sidebar thành `Sheet`.
   - Mỗi group có `error.tsx` (dùng `ErrorFallback`). `loading.tsx` (dùng `LoadingSkeleton`) đặt ở gốc `(auth)` và `(dashboard)`; **`(public)` không có `loading.tsx` ở gốc group** — nó bao luôn `/recipes/[slug]` và `/categories/[slug]`, bật streaming và làm trang không tồn tại trả 200 thay vì 404 (Buổi 7 — Dev 2). Khung chờ của trang công khai nằm ở `recipes/(list)/loading.tsx`, `categories/(list)/loading.tsx` (có từ Buổi 2).
   - `app/not-found.tsx` nằm ngoài mọi group nên tự bọc `PublicShell` (header + `main` + footer dùng chung với `(public)/layout.tsx`) — không có bước này, trang 404 mất header/footer khi root layout không còn chứa chúng.
5. **Kiểm thử:**
   - Hồi quy: toàn bộ `api-client.test.ts` của Buổi 2 – 4 xanh nguyên vẹn.
   - Mới:
     - mọi request có `X-Correlation-ID` dạng UUID;
     - 5 request song song gặp 401 `AUTH_TOKEN_EXPIRED` → đúng **1** lần gọi `/auth/refresh`, cả 5 được thử lại với token mới;
     - `GET` gặp 429 `Retry-After: 1` → thử lại 1 lần; `POST` gặp 503 → **không** thử lại;
     - option `next.revalidate` tới được `fetch`;
     - chạy phía server không có header `Authorization`.
   - `error-handler.test.ts` — lặp qua `ERROR_CODES`: mã nào cũng có `kind` và thông báo; 429 lấy đúng `retryAfterSeconds`.
   - `DashboardSidebar.test.tsx` — Author không thấy mục Admin.

**Ghi chú khi thực hiện (14/10/2026 — nhánh `2312755_NguyenThangThieng_buoiso6`, tách từ `…_buoiso5`):**

- **Chưa merge nhánh `buoiso5` của Dev 1/2/3** (trưởng nhóm chọn ngày 14/10/2026): merge thử thì xung đột — Dev 1: 15 file Auth backend; Dev 3: 5 file Frontend (`types/api.ts`, `SiteHeader`, `features/*/api.ts`, `RecipeCard`); Dev 2: 3 file (`RecipesEndpoints.cs`, `GetMyRecipeByIdQuery.cs`, `write-api.ts`). Commit route group vì vậy chỉ chuyển các route đang có trên nhánh. Khi merge, chủ trang chuyển nốt trang của mình vào group (file mới thêm ở đường dẫn cũ Git không tự chuyển):
  ```
  git mv frontend/src/app/search "frontend/src/app/(public)/search"                                     # Dev 3
  git mv "frontend/src/app/dashboard/recipes/[id]" "frontend/src/app/(dashboard)/dashboard/recipes/[id]"  # Dev 2
  ```
  Phần Dev 3 thêm vào `types/api.ts` chuyển sang `types/search.ts` (file đã tách ở Buổi 5).
  **Cập nhật 16/10/2026:** khi đưa nền Buổi 6 vào `develop` (sau khi Dev 3 Buổi 5 đã vào), trưởng nhóm chuyển `app/search` → `app/(public)/search` ngay trong merge commit. Để ngoài group thì `/search` mất header/footer vì root layout không còn chứa chúng. Dòng của Dev 2 vẫn do Dev 2 làm khi merge `develop` vào nhánh của mình.
- **Bản tạm cho component của ba dev khác làm cùng buổi** — gom trong **một** file `components/layout/interim-shell.tsx`: `InterimAuthGuard` → `AuthGuard`, `InterimRequireRole` → `RequireRole` (Dev 1); `InterimUserMenu` → `UserMenu`, `InterimErrorFallback` → `ErrorFallback`, `InterimPageLoading` → `LoadingSkeleton` (Dev 3); nút mở/đóng menu điện thoại → `Sheet` (Dev 2). Bản tạm là lớp phủ cố định dưới thanh trên cùng: bản đầu chèn menu vào luồng trang và trình duyệt cuộn xuống đúng chiều cao menu (scroll anchoring — đo 363 px) nên bấm mở mà không thấy menu; đã sửa ở commit `c4807d3`. Chủ component đổi một dòng import ở nơi dùng rồi xóa phần tương ứng. Header/footer công khai nằm ở `components/layout/PublicShell.tsx` — Dev 3 thay `SiteHeader`/`<footer>` tại đó.
- **`(public)` không có `loading.tsx` ở gốc group — đã đo:** thêm tạm `app/(public)/loading.tsx`, build production, gọi trang không tồn tại → `/recipes/khong-ton-tai-123` và `/categories/khong-ton-tai` đều trả **200**; bỏ file đó → cả hai trả **404**. Mục 4 ở trên đã sửa theo kết quả này.
- **Trang `/recipes/[slug]` (ISR, `generateStaticParams`) trả 404 với khung `<html id="__next_error__">`** rồi mới hiện trang "Không tìm thấy" sau khi hydrate. Đối chứng trên bản build `buoiso5` cho kết quả giống hệt → hành vi sẵn có của trang ISR, không do route group; hết khi Dev 2 chuyển trang sang SSR + Data Cache ở Buổi 7 (MT-67).
- **Sửa lỗi sẵn có làm mất `callbackUrl`:** `AuthCard` (Buổi 3) thấy đã đăng nhập là `router.replace('/recipes')`, đè lên `callbackUrl` mà `LoginForm` vừa chuyển tới — người bị chặn ở dashboard đăng nhập xong không quay lại trang đang mở. `AuthCard` nay dùng chung `safeCallbackUrl` của `LoginForm` (kiểm thử `AuthCard.test.tsx`). Dev 1 thay cả hai bằng `GuestOnly` + `lib/safe-redirect.ts` trong buổi này.
- **Bốn chỗ tự hiển thị lỗi khai báo `meta: { suppressGlobalError: true }`** để không toast trùng khi bật bộ xử lý toàn cục: `HealthIndicator` (chấm đỏ), `CategoryNav` (menu thu gọn khi lỗi), truy vấn và mutation lưu của `/dashboard/categories` (khối `role="alert"` trên trang, lỗi dưới từng ô của `CategoryFormDialog`). Mutation tự khai báo `onError` (xóa danh mục) được bỏ qua tự động.
- **`ImageUploader`** (đường XHR) cũng gửi `X-Correlation-ID`; lỗi mạng của XHR ném `TypeError` như `fetch` để `normalizeError` xếp vào nhóm `network`. `apiFetch` nhận thêm `FormData`/`Blob` (gửi nguyên) — tương thích ngược.
- **Kiểm thử:** jsdom không có Fetch API mà `ky` cần → `frontend/test-utils/jsdom-fetch-environment.js` (jsdom + `Request`/`Response`/`Headers`/`AbortSignal`/`FormData` của Node; không thêm gói). 15 ca của `api-client.test.ts` Buổi 2 – 4 giữ nguyên tên và kỳ vọng, chỉ đổi lớp giả lập `fetch` (`ky` gửi `Request` thật). Tổng **180/180** Jest xanh.
- **Kiểm chứng trên stack Docker thật** (Playwright + Edge, `http://localhost` qua Nginx): **24/24** — khách vào `/dashboard/system?tab=jobs` → `/auth/login?callbackUrl=%2Fdashboard%2Fsystem%3Ftab%3Djobs` → đăng nhập xong quay lại đúng trang; Author không thấy mục Admin, `/dashboard/media` hiện 403; dashboard có `noindex, nofollow`; mọi request `/api/v1` có `X-Correlation-ID` UUID khác nhau; 401 `AUTH_TOKEN_EXPIRED` → đúng 1 lần `/auth/refresh` thật rồi gửi lại với token mới; 429 `Retry-After: 30` → 1 lần gọi; 503 → 9 lần gọi (ky 3 × TanStack Query 3) trong 6,0 giây; `POST` 503 → 1 lần gọi; menu điện thoại 375 px nằm ngay dưới thanh trên cùng (y = 56), mở menu không làm trang cuộn, Escape đóng; 404 có header/footer.
- **Hook gitleaks không quét được trong git worktree** (Buổi 2): file `.git` của worktree trỏ tới `<repo chính>/.git/worktrees/<tên>`, đường dẫn đó không có trong container → gitleaks quét 0 byte nhưng vẫn trả 0 (đã thử: khóa giả đã stage vẫn lọt). `.githooks/pre-commit` nay gắn thêm thư mục `.git` chung và đặt `GIT_DIR`/`GIT_WORK_TREE`; thử lại: worktree có khóa giả → chặn (mã 1), worktree sạch → quét 740 byte, repo chính → chặn như cũ.
- **First Load JS** (bản build production): lớn nhất `/dashboard/categories` **187 kB** ≤ 200 kB (NFR-PERF-005); trang công khai không đổi (`/` 106 kB, `/recipes` 113 kB).

**3. Định hướng & lý do kiến trúc:**

**Giữ API công khai của `apiFetch`, chỉ thay phần bên trong (mặt tiền):** lớp gọi API là thứ **mọi** module phụ thuộc. Thay luôn chữ ký (ví dụ để mọi nơi gọi thẳng `ky`) thì buổi này phải sửa file của cả 4 dev, và mọi kiểm thử đang có phải viết lại. Giữ chữ ký thì toàn bộ kiểm thử Buổi 2 – 4 trở thành **kiểm thử hồi quy miễn phí** cho lần viết lại: chúng xanh nghĩa là hành vi không đổi.

**Hook của `ky` thay cho tự gói `fetch`:** bản Buổi 2 trộn bốn việc trong một hàm — gắn header, gửi, đọc lỗi, refresh — nên thêm việc thứ năm (correlation id, retry) là sửa giữa hàm. Hook tách mỗi việc thành một hàm nhỏ kiểm thử riêng được, thứ tự thực thi rõ ràng (`beforeRequest` → gửi → `afterResponse` → `beforeError`).

**Chỉ tự thử lại `GET`:** `GET` là idempotent theo định nghĩa HTTP — gửi hai lần không đổi gì. `POST /recipes` không idempotent: lần đầu đã tạo công thức nhưng phản hồi bị mất (mạng chập chờn), thử lại sẽ tạo **công thức thứ hai**. Tự động thử lại lệnh ghi là cách tạo dữ liệu trùng khó truy ra nhất.

**Bộ xử lý lỗi toàn cục qua `QueryCache`/`MutationCache`:** không có nó, mỗi màn hình tự viết `try/catch` + toast, và chắc chắn sẽ có màn hình quên xử lý 429 hoặc hiện nguyên câu lỗi tiếng Anh của framework. Đặt một lần ở cấp `QueryClient` thì mọi truy vấn mới tự có xử lý đúng; màn hình nào cần xử lý riêng (409 của trang sửa) chỉ việc khai báo `suppressGlobalError`.

**Route group thay cho `if (pathname.startsWith('/dashboard'))` trong một layout:** ba nhóm trang có ba nhu cầu khác nhau:
- trang công khai cần header/footer đầy đủ cho SEO;
- trang đăng nhập cần khung tối giản;
- dashboard cần sidebar, `AuthGuard` và `noindex`.

Một layout chung rẽ nhánh theo đường dẫn sẽ gửi code của cả ba xuống mọi trang. Route group cho mỗi nhóm một layout riêng; Next.js chỉ gửi code của layout đang dùng, và URL không đổi.

**4. Commit gợi ý:**
```
feat(frontend-core): ky-based api client with bearer, single-flight refresh, correlation id and safe retry, central error handler and public/auth/dashboard route-group layouts
```

---

# BUỔI 7 – HOÀN THIỆN XÁC THỰC & HỒ SƠ, TRANG CHỦ, DANH MỤC, CHI TIẾT CÔNG THỨC & TÌM KIẾM (LAB 7) + HẠ TẦNG PRODUCTION

> **Yêu cầu tối thiểu Lab 7 của giảng viên:** đưa các trang công khai và nghiệp vụ người dùng vào hoạt động end-to-end với Backend.
>
> **Ba endpoint mới theo CR-2026-05** (SRS v1.2.3 — §4.6): `POST /auth/change-password` (FR-AUTH-010), `POST /auth/forgot-password`, `POST /auth/reset-password` (FR-AUTH-011). Chương 8 tăng **45 → 48 endpoint**; Dev 1 thêm 3 dòng vào `ApiSurfaceTests` trong cùng PR, Phụ lục B tăng **29 → 30 mã** (`AUTH_PASSWORD_RESET_TOKEN_INVALID`).
>
> **Nhiều trang Lab 7 yêu cầu đã có từ Buổi 2 – 5** — buổi này **nâng cấp lên nền Buổi 5 – 6** (kiểu TypeScript, `apiFetch` trên `ky`, shadcn/ui, `useAuth()` trên Zustand, layout route group) và làm phần còn thiếu, không làm lại:
>
> | Trang Lab 7 | Đã có | Buổi này thêm |
> |---|---|---|
> | Đăng ký / Đăng nhập / Đăng xuất | Buổi 2 – 3 | shadcn `Form`, `GuestOnly`, `callbackUrl` an toàn; đổi mật khẩu, quên/đặt lại mật khẩu (mới hoàn toàn) |
> | `/profile` | — | toàn bộ (FR-AUTH-006/007) + tab Phiên đăng nhập (FR-AUTH-009) |
> | `/recipes/[slug]` | Buổi 2 (ISR 300) | điều chỉnh khẩu phần, in, chia sẻ, thư viện ảnh; trả đúng 404 |
> | `/`, `/categories`, `/categories/[slug]` | Buổi 2 (trang chủ còn là trang khung) | trang chủ đầy đủ, sidebar danh mục, phần còn lại của D-7 |
> | `/search` | Buổi 5 (Dev 3) | bộ lọc thời gian (MT-66), cập nhật kết quả phía client không tải lại trang |
>
> **Ngoại lệ phân công (theo đúng Lab 7):** giao diện `/search` do **Dev 4** làm, dù API tìm kiếm do Dev 3 sở hữu từ Buổi 4. Hợp đồng giữa hai bên là SRS §8.3 + integration test của Dev 3 — cùng cơ chế "chủ API / chủ giao diện" của Buổi 4.

## DEV 1 — Đăng ký, Đăng nhập, Đăng xuất, Đổi mật khẩu, Quên mật khẩu & Trang Hồ sơ (Lab 7)

**1. Tính năng & mã yêu cầu:** Lab 7 – Dev 1.
- Giao diện `FR-AUTH-001/002/003/005` (nâng cấp);
- **`FR-AUTH-010` Đổi mật khẩu** và **`FR-AUTH-011` Quên & đặt lại mật khẩu** — API + giao diện, mới theo CR-2026-05 (MT-64, MT-65);
- giao diện `FR-AUTH-006/007` (`/profile`, ảnh đại diện) và `FR-AUTH-009` (tab Phiên đăng nhập).

API của FR-AUTH-006/007/009 đã có từ Buổi 4.

**2. Các bước triển khai:**

*Backend — 3 endpoint của CR-2026-05:*

1. **Mã lỗi + exception:**
   - `ErrorCodes.PasswordResetTokenInvalid = "AUTH_PASSWORD_RESET_TOKEN_INVALID"` — thêm một dòng vào file của Dev 4, theo quy tắc append-only; Dev 4 rà soát PR. Cùng PR gỡ dòng của mã này khỏi `PendingSrsCodes` trong `ArchitectureTests/DomainExceptionRulesTests.cs` (đặt ở Buổi 5 — quên gỡ thì test kiến trúc đỏ).
   - `Domain/Exceptions/Auth/PasswordResetTokenInvalidException : AuthDomainException`, ánh xạ **400** trong `AuthExceptionMappings`.
   - Phía Frontend: thêm mã vào `ERROR_CODES` và `ERROR_MESSAGES` trong cùng PR. Kiểm thử chống lệch của Buổi 5 bắt buộc làm vậy.
2. **`IIdentityService` mở rộng** (ghi người dùng luôn qua `UserManager` — quy tắc Buổi 3):
   - `ChangePasswordAsync(userId, current, next)` — trước tiên `HasPasswordAsync`, rồi `UserManager.ChangePasswordAsync`;
   - `GeneratePasswordResetTokenAsync(userId)`;
   - `ResetPasswordAsync(email, token, next)` — `UserManager.ResetPasswordAsync`, thành công thì `ResetAccessFailedCountAsync` + `SetLockoutEndDateAsync(null)`.

   Lớp cài đặt dịch `IdentityError` sang lỗi của hệ thống:
   - `PasswordMismatch` → `ValidationException` ở ô `currentPassword`;
   - `InvalidToken` → `PasswordResetTokenInvalidException`;
   - lỗi chính sách mật khẩu → `ValidationException` ở ô `newPassword`.

   `UserManager` tự đổi `SecurityStamp` khi đổi/đặt lại mật khẩu → mọi token đặt lại mật khẩu phát trước đó **mất hiệu lực** (dùng một lần).
3. **`ChangePasswordCommand { CurrentPassword, NewPassword }`** — `POST /api/v1/auth/change-password`, `RequireAuthorization()`, chính sách rate limit `auth`:
   - **Validator:** `NewPassword` theo đúng `IdentityOptions.Password` (NFR-SEC-001: ≥ 8 ký tự, đủ hoa/thường/số/ký tự đặc biệt); khác `CurrentPassword`.
   - **Tài khoản chỉ đăng nhập bằng Google** (chưa có mật khẩu) → **400** `VALIDATION_ERROR`, `detail` hướng dẫn dùng "Quên mật khẩu" để đặt mật khẩu lần đầu.
   - **Thành công** — trong **một** transaction (`unitOfWork.ExecuteInTransactionAsync`):
     - đổi mật khẩu;
     - `unitOfWork.Users.RevokeAllRefreshTokensExceptAsync(userId, currentSessionId, now)` — phương thức mới; `currentSessionId` lấy từ claim **`sid`** (Buổi 4). Mọi phiên **khác** bị thu hồi, phiên đang dùng giữ nguyên.
   - **204**; audit log `Log.Information("SECURITY: password changed for user {UserId}, {RevokedCount} other sessions revoked")`.
4. **`ForgotPasswordCommand { Email }`** — `POST /api/v1/auth/forgot-password`, ẩn danh, rate limit `auth`:
   - **Luôn trả 202 Accepted**, dù email có tồn tại hay không.
   - Người dùng tồn tại **và** `IsActive` → `IBackgroundJobClient.Enqueue<PasswordResetEmailJob>(j => j.SendAsync(user.Id, correlationId, CancellationToken.None))`. **Tham số job chỉ có `userId`** — không có token, không có link.
5. **`Infrastructure/Jobs/PasswordResetEmailJob`** (chạy ở container `hangfire`):
   - nạp người dùng → `GeneratePasswordResetTokenAsync` **ngay trong worker**. Khóa DataProtection dùng chung qua volume `dpkeys` từ Buổi 2, nên token do worker sinh được container `api` xác minh;
   - Base64Url → link `{App:FrontendBaseUrl}/auth/reset-password?email=…&token=…`;
   - `IEmailService.SendAsync` (MailKit, MailHog ở dev) với template `PasswordResetEmail`;
   - `[AutomaticRetry]` 1′/5′/30′ như `WelcomeEmailJob`;
   - log chỉ ghi `userId` + `correlationId`, **không bao giờ ghi token hay link**.
   - Thời hạn token: `DataProtectionTokenProviderOptions.TokenLifespan` = **30 phút** (cấu hình `Identity:PasswordResetTokenLifespanMinutes`).
6. **`ResetPasswordCommand { Email, Token, NewPassword }`** — `POST /api/v1/auth/reset-password`, ẩn danh, rate limit `auth`:
   - **Cùng một lỗi 400 `AUTH_PASSWORD_RESET_TOKEN_INVALID`** cho mọi trường hợp: token sai định dạng Base64Url, email không tồn tại, token sai/hết hạn/đã dùng — không cho biết trường hợp nào.
   - Tài khoản đã bị Admin khóa → **403** `AUTH_ACCOUNT_DISABLED`.
   - **Thành công** — trong một transaction: đặt mật khẩu + `RevokeAllRefreshTokensAsync(userId)` (thu hồi **mọi** phiên) + xóa khóa đăng nhập sai. **204**, audit log.
7. **Endpoint** trong `AuthEndpoints.cs` kèm metadata OpenAPI (kiểm thử quy ước của Buổi 5 bắt buộc); `ApiSurfaceTests` thêm 3 dòng (**48**); ảnh chụp `docs/api/openapi.v1.json` cập nhật.
8. **Integration test:**
   - **Đổi mật khẩu:**
     - sai mật khẩu hiện tại → 400, `errors.currentPassword`;
     - thành công → đăng nhập bằng mật khẩu cũ trả 401; refresh token của **phiên khác** trả 401 `AUTH_REFRESH_TOKEN_REVOKED`; phiên hiện tại vẫn refresh được;
     - tài khoản Google → 400.
   - **Quên mật khẩu:**
     - email không tồn tại → 202, **không** có job;
     - email tồn tại → 202, có đúng 1 job, tham số job **không chứa** chuỗi token.
   - **Đặt lại mật khẩu** — chạy job với `IEmailService` giả, lấy link từ email:
     - đặt lại → 204, đăng nhập bằng mật khẩu mới 200, mọi refresh token cũ 401, khóa đăng nhập sai được xóa;
     - dùng lại cùng token → 400 `AUTH_PASSWORD_RESET_TOKEN_INVALID`;
     - token hết hạn (factory đặt thời hạn 1 giây) → 400;
     - tài khoản bị khóa → 403.
   - **Rate limit:** request thứ 4 tới `forgot-password` (factory hạn mức nhỏ của Buổi 5) → 429.

*Frontend:*

9. **Nâng cấp `(auth)/auth/login` và `(auth)/auth/register`:**
   - shadcn `Form` + Zod (`features/auth/schemas.ts` khớp validator Backend), `Button isLoading`;
   - `useAuth()` (Zustand); bọc `GuestOnly`;
   - sau đăng nhập về `toSafeInternalPath(callbackUrl)`; đăng ký tự đăng nhập (FR-AUTH-001) → `/dashboard`;
   - nút Google (Buổi 3) giữ nguyên, kèm thông báo dự phòng khi 502;
   - link "Quên mật khẩu?" dưới ô mật khẩu.
10. **Đăng xuất:** `UserMenu` → `useAuth().logout()` → `POST /auth/logout` (thu hồi refresh token phía server) → `authStore.clear()` → `queryClient.clear()` (xóa mọi dữ liệu riêng tư còn trong cache) → `router.replace('/')`.
11. **`(auth)/auth/forgot-password/page.tsx`:** ô email → 202 → luôn hiện cùng một câu *"Nếu email này đã đăng ký, bạn sẽ nhận được hướng dẫn trong vài phút"*.
12. **`(auth)/auth/reset-password/page.tsx`:**
    - đọc `email`, `token` từ `searchParams` vào state, rồi **`history.replaceState` xóa token khỏi thanh địa chỉ** — tránh token nằm trong lịch sử trình duyệt hay ảnh chụp màn hình;
    - `metadata`: `robots: noindex` và `referrer: 'no-referrer'` — trang không gửi `Referer` chứa token cho bất kỳ tài nguyên bên ngoài nào;
    - ô mật khẩu mới + nhập lại (Zod `refine`);
    - 400 `AUTH_PASSWORD_RESET_TOKEN_INVALID` → thông báo *"Liên kết đã hết hạn hoặc đã được dùng"* + link về trang quên mật khẩu;
    - thành công → toast → `/auth/login`.
13. **`(dashboard)/profile/page.tsx` (CSR) — FR-AUTH-006/007/009, shadcn `Tabs`:**
    - **Tab "Thông tin":**
      - `useQuery({ queryKey: authKeys.me(), queryFn: getMe })` — `GET /auth/me` là nguồn sự thật về hồ sơ và vai trò;
      - form `displayName` (2 – 100), `bio` (≤ 1000);
      - ảnh đại diện: `ImageUploader` → `POST /files/upload` → `PATCH /auth/me { avatarUrl }`;
      - optimistic update cho cả `authKeys.me()` lẫn `authStore.user` → ảnh ở header đổi ngay; API lỗi thì rollback;
      - ảnh cũ xóa qua `DELETE /files/{**key}` **sau khi** PATCH thành công.
    - **Tab "Bảo mật":** form đổi mật khẩu (mật khẩu hiện tại, mới, nhập lại). Thành công → toast *"Đã đổi mật khẩu. Các thiết bị khác đã bị đăng xuất."*
    - **Tab "Phiên đăng nhập" (FR-AUTH-009):**
      - `useQuery(authKeys.sessions())` → danh sách IP + thời điểm tạo + hết hạn; nhãn **"Thiết bị này"** theo `isCurrent` (claim `sid`);
      - "Đăng xuất thiết bị này" (`AlertDialog`) → `DELETE /auth/sessions/{id}`;
      - "Đăng xuất tất cả" → `POST /auth/sessions/revoke-all` → xóa phiên cục bộ → `/auth/login`.
    - `features/auth/query-keys.ts`: `authKeys = { me: () => ['auth','me'], sessions: () => ['auth','sessions'] }`.
14. **Kiểm thử Frontend:**
    - `reset-password` xóa token khỏi URL sau khi đọc;
    - `forgot-password` hiện cùng một thông báo cho mọi phản hồi 202;
    - `profile` rollback ảnh đại diện khi PATCH lỗi;
    - kiểm tay qua MailHog (`http://localhost:8025`): bấm link trong email → đặt mật khẩu → đăng nhập.

**3. Định hướng & lý do kiến trúc:**

**Không cho biết email có tồn tại (luôn 202):** nếu `forgot-password` trả 404 cho email lạ, endpoint thành công cụ dò xem ai có tài khoản (chống dò tài khoản – user enumeration). Cùng lý do với logout idempotent (Buổi 3) và 404 cho phiên của người khác (Buổi 4). `reset-password` trả **một** mã lỗi cho mọi kiểu token hỏng vì cùng lý do.

**Token đặt lại mật khẩu không bao giờ nằm trong kho job:** Hangfire lưu tham số job **dạng rõ** trong schema `hangfire` của PostgreSQL và hiển thị trên dashboard `/hangfire`. Truyền link vào job là ghi một "chìa khóa tài khoản" còn hiệu lực 30 phút vào cơ sở dữ liệu và lên màn hình quản trị. Chỉ truyền `userId`, để worker tự sinh token lúc gửi. Thiết kế này dựa trên hai thứ đã dựng từ Buổi 2:
- worker riêng (MT-47);
- khóa DataProtection dùng chung qua volume `dpkeys` — thiếu nó thì token do worker sinh, API sẽ không xác minh được.

NFR-SEC-007 được mở rộng tương ứng: token đặt lại mật khẩu thuộc danh sách cấm ghi log.

**Thu hồi phiên sau khi đổi/đặt lại mật khẩu:** người dùng đổi mật khẩu thường vì nghi ngờ bị lộ. Nếu refresh token cũ vẫn sống 7 ngày thì kẻ đã đăng nhập trước đó **vẫn ở trong tài khoản** — đổi mật khẩu thành vô nghĩa.
- **Đổi mật khẩu** (người dùng đang đăng nhập, chứng minh được mật khẩu cũ) giữ phiên hiện tại, nhờ claim `sid` có từ Buổi 4.
- **Đặt lại** (người dùng chứng minh quyền với hộp thư, không chứng minh được đang cầm thiết bị nào) thu hồi tất cả.

**Sai mật khẩu hiện tại là 400 ở ô `currentPassword`, không phải 401:** 401 nghĩa là *"bạn chưa được xác thực"*. Người dùng ở đây **đã** xác thực bằng access token hợp lệ, chỉ có một trường dữ liệu sai. Trả 401 còn làm lớp gọi API hiểu nhầm là phiên có vấn đề. Trả 400 kèm `errors.currentPassword` thì `mapProblemDetailsToForm` hiện lỗi ngay dưới đúng ô.

**Xóa token khỏi URL và tắt `Referer`:** URL có token xuất hiện ở lịch sử trình duyệt, ở thanh địa chỉ khi chia sẻ màn hình, và trong header `Referer` khi trang tải tài nguyên bên ngoài (font, ảnh Google). Hai dòng code chặn cả ba đường lộ.

**`GET /auth/me` là nguồn sự thật về vai trò, không tự giải mã JWT** (giải trình giữ từ kế hoạch cũ): giải mã payload ở Frontend biến token thành nguồn sự thật về quyền — quyền bị thu hồi vẫn hiệu lực tới khi access token hết hạn (≤ 15 phút) — và tạo thói quen tin dữ liệu client tự giải mã.

**PATCH một phần cho hồ sơ; email không đổi ở endpoint này:** đổi ảnh đại diện không có lý do gì phải gửi lại cả `bio` và `displayName` (PUT toàn phần dễ ghi đè mất trường người dùng không định sửa). Đổi email là luồng nghiệp vụ khác hẳn — cần xác minh email mới, cân nhắc liên kết Google — không thể là một ô bình thường trong form (MT-12, MT-21).

**Vì sao nhận diện "Thiết bị này" bằng claim `sid`:** server so `sid` với `id` của từng phiên mà client không phải gửi thêm gì (cách OpenID Connect dùng). Bắt client gửi kèm refresh token để so là đưa dữ liệu nhạy cảm vào request không cần thiết.

**4. Commit gợi ý:**
```
feat(auth): change, forgot and reset password (CR-2026-05) with session revocation, hardened login/register forms and profile page with avatar upload and sessions tab
```

---

## DEV 2 — Trang Chi tiết Công thức & Danh sách "Công thức của tôi" (Lab 7)

**1. Tính năng & mã yêu cầu:** Lab 7 – Dev 2.
- Giao diện `FR-RCP-002` Chi tiết công thức (`GET /recipes/{slug}` — nâng cấp trang Buổi 2);
- giao diện `FR-RCP-011` danh sách "Công thức của tôi" `/dashboard/recipes` (`GET /recipes/mine`) — phần kế hoạch cũ đặt ở Buổi 6;
- `NFR-PERF-003` (`revalidate` ≤ TTL), `NFR-SEC-006` (cách ly dữ liệu công khai/riêng tư ở cache phía client).

**2. Các bước triển khai:**

*Luồng dữ liệu trang chi tiết:*
- `(public)/recipes/[slug]/page.tsx` (Server Component, render theo request vì CSP nonce — MT-67; **Data Cache `revalidate: 300`** = TTL `recipe:{slug}` 5 phút) → `getRecipeBySlug(slug)` → `apiFetch` với `next: { revalidate: 300, tags: ['recipe:' + slug] }` → `GET /api/v1/recipes/{slug}`.
- 404 (không tồn tại, Draft, Archived — MT-34) → `notFound()` → **HTTP 404**.
- 200 → render; phần tương tác (khẩu phần, in, chia sẻ) là Client Component nhỏ nhận dữ liệu qua props.

1. **Trang chi tiết:**
   - **Không `generateStaticParams`, không `export const revalidate`:** trang render theo request (MT-67); dữ liệu được Data Cache giữ 300 giây nên mỗi lượt xem **không** gọi lại API. Lúc `docker build` cũng không cần API chạy.
   - **Không có `loading.tsx` ở route này:** `loading.tsx` bật streaming — Next.js gửi **status 200** ngay khi bắt đầu stream, trước khi biết dữ liệu có hay không. Bản nháp vì thế trả 200 với nội dung "không tìm thấy" thay vì 404 thật (lỗi `BAO_CAO_BUOI_2.md` §5 từng gặp). Dữ liệu đã nằm trong Data Cache nên trang trả về nhanh, không cần màn hình chờ.
   - `generateMetadata` — `title`, `description` cơ bản; Open Graph và JSON-LD đầy đủ do Dev 3 làm ở Buổi 8 (`lib/seo/*`).
2. **Component — `features/recipes/components/`:**
   - **`RecipeHero`:** tiêu đề `<h1>`, mô tả, tác giả (ảnh đại diện + tên), `publishedAt` (`<time dateTime>`), `Badge` danh mục. Thông số trình bày bằng `<dl>`: chuẩn bị, nấu, tổng thời gian, khẩu phần, độ khó.
   - **`RecipeGallery`:** ảnh chính `next/image` dùng `MediumUrl` (800 × 600, FR-JOB-002) với `priority` — ảnh này là LCP. Các ảnh phụ dạng thumbnail, bấm mở `Dialog` xem lớn. `alt` lấy từ `AltText` (FR-RCP-008).
   - **`IngredientList` — điều chỉnh khẩu phần:**
     - ô số khẩu phần (mặc định `recipe.servings`);
     - nguyên liệu có `quantity` (số) được nhân theo tỉ lệ `mục tiêu / gốc`, làm tròn qua `formatQuantity` (Buổi 2);
     - nguyên liệu chỉ có `quantityText` ("vừa đủ", "1 nhúm") **giữ nguyên văn**;
     - mỗi dòng có ô đánh dấu "đã chuẩn bị" (state cục bộ).
   - **`StepChecklist`** (Buổi 2): thêm ảnh minh họa từng bước và nút hẹn giờ cho bước có `timerMinutes` (đếm ngược, thông báo bằng `toast` khi hết giờ).
   - **`NutritionTable`** (Buổi 2): nhúng trong trang, có dòng "mỗi khẩu phần"; không hiển thị khi `nutrition` null.
   - **`RecipeActions`** (Client):
     - **In** — `window.print()` + khối `@media print` trong `globals.css`: ẩn header/footer/nav/nút, hiện URL công thức, `break-inside: avoid` cho từng bước;
     - **Chia sẻ** — `navigator.share({ title, text, url })` khi trình duyệt hỗ trợ (điện thoại), ngược lại `navigator.clipboard.writeText(url)` + toast "Đã sao chép liên kết";
     - nút có `aria-label` rõ ràng.
3. **`(dashboard)/dashboard/recipes/page.tsx` (CSR) — FR-RCP-011:**
   - `Tabs` Tất cả / Draft / Published / Archived; tab đang chọn nằm trong `searchParams.status`.
   - `Table`: tiêu đề (link tới trang sửa), `RecipeStatusBadge`, danh mục, `updatedAt`, menu thao tác (Sửa; các thao tác đổi trạng thái thêm ở Buổi 8).
   - `Pagination` dùng chung; `EmptyState` *"Bạn chưa có công thức nào"* kèm nút "Viết công thức đầu tiên".
   - `useQuery({ queryKey: recipeKeys.mine(params), queryFn: () => getMyRecipes(params), placeholderData: keepPreviousData, staleTime: 0 })`.
   - **Chỉ gọi `GET /recipes/mine`**, không bao giờ gọi `GET /recipes` (SRS §5.1).
4. **Query key tách bạch công khai/riêng tư — `features/recipes/query-keys.ts`:**
   ```ts
   export const recipeKeys = {
     all: ['recipes'] as const,
     list:   (p: RecipeListParams)   => ['recipes', 'public', 'list', p] as const,
     detail: (slug: string)          => ['recipes', 'public', 'detail', slug] as const,
     mine:   (p: MyRecipesParams)    => ['recipes', 'private', 'mine', p] as const,
     mineById: (id: string)          => ['recipes', 'private', 'mine', id] as const,
   };
   ```
   Đăng xuất gọi `queryClient.clear()` (Dev 1), còn nhánh `'private'` có thể `removeQueries` riêng khi cần.
5. **Kiểm thử:**
   - `IngredientList.test.tsx` — 200 g × (4/2) = 400 g; `quantityText` "vừa đủ" không đổi; khẩu phần 0 hoặc âm bị chặn;
   - `RecipeActions.test.tsx` — không có `navigator.share` thì dùng clipboard;
   - kiểm tay `curl -I http://localhost/recipes/<slug-của-một-Draft>` → `HTTP/1.1 404`; `curl -I` một slug Published → 200 và header `Content-Security-Policy` có nonce.

**3. Định hướng & lý do kiến trúc:**

**Data Cache 300 giây = TTL cache API 5 phút (quy tắc vàng §2.3):** nếu tầng Next.js giữ dữ liệu lâu hơn TTL, Next.js thành tầng cache cũ nhất — tác giả sửa công thức, Redis đã xóa khóa, nhưng trang vẫn phục vụ bản cũ. Bằng nhau thì độ trễ tối đa người đọc thấy là 5 phút, đúng như SRS cam kết.

**Bản nháp trả 404, không phải 403** (MT-34): 403 nói với người lạ *"có một bản nháp ở đúng slug này"* — tiết lộ sự tồn tại của nội dung chưa công bố và cho phép dò slug hàng loạt. Với trang công khai, bản nháp đơn giản là **không tồn tại**. Bỏ `loading.tsx` ở route này là để con số 404 đó **thật sự** tới trình duyệt và crawler.

**Điều chỉnh khẩu phần cho thấy giá trị của chiến lược định lượng hai cột (MT-04):** cột `Quantity` là số nên **tính toán được**; cột `QuantityText` giữ những cách viết không quy ra số được ("vừa đủ", "1 nhúm"). Nếu chỉ có một cột chữ như SRS v1.0.0, tính năng nhân khẩu phần không làm được — hoặc phải tự phân tích chuỗi tiếng Việt, một nguồn lỗi vô tận.

**In bằng CSS `@media print`, không sinh PDF:** thư viện sinh PDF thêm hàng trăm KB JavaScript cho một nút ít người bấm, và PDF sinh ra không theo cỡ giấy của người dùng. Trình duyệt nào cũng in và "lưu thành PDF" được; chỉ cần một khối CSS quyết định phần nào xuất hiện trên giấy.

**Chia sẻ theo kiểu nâng cấp dần:** `navigator.share` mở bảng chia sẻ của hệ điều hành trên điện thoại (Zalo, Messenger…) — trải nghiệm tốt nhất ở nơi nó có. Máy tính không có thì quay về sao chép link. Không cần SDK của mạng xã hội nào — không thêm script bên ngoài, không phải nới CSP.

**Không cache dữ liệu riêng tư lẫn với công khai — kể cả ở phía client** (giải trình giữ từ kế hoạch cũ): NFR-SEC-006 cấm đặt dữ liệu phụ thuộc danh tính vào cache dùng chung dưới khóa công khai, vì **cache nằm trước code kiểm tra quyền**. Phía server, `GET /recipes/mine` mang `no-store`. Phía client, TanStack Query cũng là một cache: nếu danh sách "của tôi" và danh sách công khai dùng chung một khóa, bản nháp có thể hiện trong lưới công khai của chính trình duyệt đó, hoặc còn lại sau khi đổi tài khoản. Nhánh khóa `'private'` tách riêng + `staleTime: 0` + `clear()` khi đăng xuất là bản client của cùng nguyên tắc.

**4. Commit gợi ý:**
```
feat(recipes-ui): recipe detail page with servings scaler, gallery, nutrition, print and share returning real 404 for drafts, and my-recipes dashboard over /recipes/mine
```

---

## DEV 3 — Trang chủ, Danh sách Danh mục & Trang Danh mục (Lab 7) + hoàn tất D-7

**1. Tính năng & mã yêu cầu:** Lab 7 – Dev 3.
- Giao diện SRS §5.1 `/` (danh sách công thức mới xuất bản + danh mục — MT-23);
- giao diện `FR-CAT-001` `/categories` và `FR-CAT-002` `/categories/[slug]` (nâng cấp trang Buổi 2);
- **phần còn lại của retrofit D-7** (MT-33.3); `NFR-PERF-003` (`revalidate` ≤ TTL).

**2. Các bước triển khai:**

1. **`(public)/page.tsx` — trang chủ (SRS v1.2.3 §5.1: SSR + Data Cache — mỗi `fetch` mang `revalidate` của chính dữ liệu đó):**
   - Gọi song song (`Promise.all`):
     - `getRecipes({ sortBy: 'publishedAt', sortOrder: 'desc', pageSize: 8 })` với `next: { revalidate: 120 }` (TTL `recipes:list` 2 phút);
     - `getCategories()` với `next: { revalidate: 1800 }` (TTL `categories:all` 30 phút).
   - Bố cục:
     - khối giới thiệu đầu trang có ô tìm kiếm (form GET tới `/search` — chạy cả khi chưa có JavaScript);
     - **cột chính** — lưới 8 `RecipeCard` "Công thức mới xuất bản" (ảnh `ThumbnailUrl`, Buổi 5), nút "Xem tất cả" → `/recipes`;
     - **cột phụ** — `SidebarCategory` (Buổi 6) trên màn hình ≥ 1200 px; dưới 1200 px thành hàng nhãn danh mục cuộn ngang.
   - Ảnh của thẻ đầu tiên `priority` (LCP); các thẻ khác `loading="lazy"` (mặc định của `next/image`).
2. **`(public)/categories/(list)/page.tsx` — danh sách danh mục (SSR + Data Cache `revalidate: 1800`):**
   - **Retrofit D-7 phần còn lại:** `getCategories()` đổi `revalidate: 3600` → **`1800`**.
   - Lưới `CategoryCard` (Buổi 2) chuyển sang shadcn `Card`: ảnh, tên, mô tả hai dòng, `Badge` số công thức (chỉ đếm Published — FR-CAT-001); `EmptyState` khi chưa có danh mục.
3. **`(public)/categories/[slug]/page.tsx` — trang danh mục (SSR + Data Cache `revalidate: 120` — MT-57, đã đặt ở Buổi 5):**
   - Phần đầu: tên, mô tả, số công thức.
   - Bố cục hai cột: `SidebarCategory activeSlug={slug}` + lưới công thức.
   - `SortSelect` (Buổi 5) cho `sortBy`/`sortOrder` — SRS §8.2 chỉ cho danh mục sắp xếp và phân trang, **không** có bộ lọc (báo cáo Lab 05 của Dev 3 đã ghi nhận).
   - `Pagination` dùng chung với `buildHref` giữ tham số sắp xếp.
   - 404 → `notFound()`; danh mục không có công thức → `EmptyState` kèm link về `/recipes`.
4. **Kiểm chứng D-7 tự động — `features/categories/api.test.ts`:** giả lập `apiFetch` và khẳng định:
   - `getCategories` gửi `next.revalidate === 1800`;
   - `getCategoryBySlug` gửi `next.revalidate === 120`.

   `features/recipes/api.test.ts` làm tương tự cho trang chủ (120) và trang chi tiết (300, Dev 2). Kèm lệnh kiểm tay `grep -rn "revalidate: \(3600\|600\)" frontend/src` → rỗng.
5. **Kiểm tra bố cục** ở 320 / 768 / 1200 px trên dữ liệu thật — so với mockup Buổi 6, ghi ảnh chụp vào báo cáo Lab 07.

**3. Định hướng & lý do kiến trúc:**

**Mỗi lời gọi `fetch` mang `revalidate` bằng TTL của chính dữ liệu đó:** trang chủ ghép hai loại dữ liệu có TTL khác nhau — công thức 2 phút, danh mục 30 phút. Next.js App Router cache theo **từng lời gọi `fetch`**. Đặt đúng giá trị cho từng lời gọi thì danh mục không bị tải lại mỗi 2 phút một cách vô ích, mà công thức mới vẫn hiện sau tối đa 2 phút. Đây là áp dụng chi tiết của quy tắc vàng §2.3 (MT-33.3).

**Kiểm thử thay cho trí nhớ cho con số `revalidate`:** D-7 tồn tại chính vì con số 3600 được viết ở Buổi 2 và không ai nhớ đối chiếu với SRS. Một kiểm thử nhỏ khẳng định giá trị `revalidate` biến quy tắc vàng thành thứ máy kiểm được — lần sau ai đổi TTL ở Backend mà quên Frontend sẽ thấy kiểm thử đỏ.

**"Nổi bật" = mới xuất bản nhất** (MT-23, giải trình giữ từ kế hoạch cũ): bảng `Recipes` không có cột `IsFeatured`/`ViewCount`/`Rating`. Thêm `ViewCount` nghĩa là **ghi cơ sở dữ liệu mỗi lượt xem** — tạo điểm ghi nóng ngay trên đường đọc, phá cả cache lẫn mục tiêu p95 ≤ 500 ms. `PublishedAt` dùng ngay index có sẵn, không thêm cột, không thêm endpoint.

**Gọi `getCategories()` ở nhiều chỗ mà không tốn nhiều request:** `Header`, `SidebarCategory` và trang cùng cần danh sách danh mục. Vì lớp gọi API vẫn đi qua `fetch` (lý do chọn `ky` ở Buổi 5), Next.js tự gom các lời gọi trùng trong cùng một lần render và dùng chung Data Cache — mỗi component tự lấy dữ liệu nó cần mà không phải truyền props qua nhiều tầng.

**4. Commit gợi ý:**
```
feat(home,categories): homepage with latest recipes and category sidebar, category list and detail pages with fetch revalidate aligned to api ttl (D-7)
```

---

## DEV 4 — Trang Tìm kiếm (Lab 7) + Hạ tầng Production: Nginx SSL, `/media/` (D-14), Compose scale & DataProtection (D-17)

**1. Tính năng & mã yêu cầu:** Lab 7 – Dev 4.
- Giao diện `FR-SRCH-001` (`GET /api/v1/recipes/search`, PostgreSQL FTS) với bộ lọc `FR-SRCH-002` (danh mục, độ khó, thời gian — MT-66) và phân trang offset `FR-SRCH-004`;
- **hạ tầng production** (module của Dev 4, chuyển từ Buổi 7 cũ): `NFR-SEC-005` (HTTPS, HSTS), `NFR-SCALE-003`, `NFR-PERF-005`;
- **retrofit D-14 (phía Nginx), D-17**.

**2. Các bước triển khai:**

*Luồng dữ liệu trang tìm kiếm:*
- **Lần tải đầu (SSR):** `(public)/search/page.tsx` đọc `searchParams` → `parseSearchState` → `queryClient.prefetchQuery(searchKeys.results(state))` → `<HydrationBoundary state={dehydrate(queryClient)}>` → HTML có sẵn kết quả (SEO, chạy cả khi không có JavaScript).
- **Đổi bộ lọc/trang (client):** `SearchResults` đổi URL bằng `window.history.pushState` → `useSearchParams()` cập nhật → `useQuery({ queryKey: searchKeys.results(state), queryFn: () => searchRecipes(state), placeholderData: keepPreviousData, staleTime: 60_000 })` → chỉ gọi `GET /api/v1/recipes/search?...`. **Không** render lại Server Component.

1. **Trang `/search` — nâng cấp trang của Dev 3 (Buổi 5):**
   - Giữ: SSR (`dynamic = 'force-dynamic'` — SRS §5.1), `HighlightText` không dấu, `q` < 2 ký tự không gọi API (sẽ là 400) mà hiện gợi ý, form GET cho trường hợp không có JavaScript.
   - **Cập nhật phía client:** Next.js đồng bộ `window.history.pushState/replaceState` với `useSearchParams` mà không gọi lại server. Đổi bộ lọc vì vậy chỉ tốn đúng một request JSON tới API, không tải lại RSC payload của cả trang. Kết quả cũ hiện mờ (`isPlaceholderData`) trong lúc chờ — không nháy trang trắng.
   - Thư mục mới `features/search/` chứa `query-keys.ts` (`searchKeys`) và `components/SearchResults.tsx`. Phần phân tích URL `search-params.ts` dùng chung với `/recipes` nên ở lại `features/recipes/`.
   - `staleTime` 60 giây = TTL `search:` 1 phút. Back/forward lấy lại từ cache TanStack ngay lập tức.
2. **`FilterPanel` đa tiêu chí** (dựa trên `types/search.ts`):

   | Tiêu chí | Tham số | Điều khiển |
   |---|---|---|
   | Danh mục | `categoryId` | `Select`, dữ liệu từ `getCategories()` |
   | Độ khó | `difficulty` | nhóm nút từ `RECIPE_DIFFICULTIES` (đủ 4 mức, có `Expert`) |
   | Thời gian nấu tối đa | `maxCookTime` | 15 / 30 / 60 / 120 phút |
   | Thời gian chuẩn bị tối đa | `maxPrepTime` | 15 / 30 / 60 phút |
   | Khẩu phần tối thiểu | `minServings` | 1 / 2 / 4 / 6 |

   - Ba tiêu chí cuối dùng tham số mới của MT-66 — API do Dev 3 làm ở Buổi 6.
   - Nhãn bộ lọc đang bật, mỗi nhãn có nút gỡ; nút "Xóa bộ lọc".
   - Đổi bất kỳ bộ lọc nào → về trang 1.
   - Màn hình < 1024 px: bộ lọc nằm trong `Sheet` (shadcn), mở bằng nút có badge số bộ lọc đang bật.
3. **Phân trang offset:** `Pagination` (Buổi 6) nhận `page`, `totalPages`, `hasNextPage`, `hasPreviousPage` từ `PagedResult`; dòng *"Hiển thị 13–24 trên 57 kết quả"* tính từ `page`, `pageSize`, `totalCount`; `pageSize` 12.
4. **Kết quả trống:** `EmptyState` gợi ý gỡ bớt bộ lọc (liệt kê bộ lọc đang bật) và vài danh mục phổ biến.
5. **Kiểm thử:**
   - `search-params.test.ts` (Buổi 5) mở rộng cho 3 tham số mới: phân tích ↔ dựng URL khứ hồi không mất dữ liệu; giá trị hỏng (`maxCookTime=abc`) về mặc định;
   - `SearchResults.test.tsx` — đổi bộ lọc gọi `history.pushState` và về trang 1.

*Hạ tầng production (Dev 4):*

6. **Nginx — `nginx/templates/nginx.conf.template`** (giữ resolver động `127.0.0.11 valid=10s` + `proxy_pass` qua biến — MT-50; giữ `proxy_next_upstream` (Buổi 5), `X-Forwarded-*`, `X-Correlation-ID` (Buổi 2), `/hangfire` + gate (Buổi 3), `/scalar/` (Buổi 5)):
   - **Retrofit D-14 — ảnh qua `/media/`:** `location /media/ { set $minio_upstream http://minio:9000; proxy_pass $minio_upstream/culinary-blog/; proxy_cache media; expires 30d; }`.
     - `MinIO__PublicBaseUrl` → `https://<domain>/media` (dev: `http://localhost/media`).
     - Từ đây trình duyệt **và** container `frontend` cùng truy cập ảnh qua một địa chỉ → Dev 3 gỡ `unoptimized` của `next/image` ở Buổi 8.
     - Script một lần đổi URL ảnh đã lưu trong cơ sở dữ liệu từ `http://localhost:9000/culinary-blog/` sang `/media/` (bảng `RecipeImages`, `RecipeSteps.ImageUrl`, `Categories.ImageUrl`, `AspNetUsers.AvatarUrl`), chạy trong một transaction, có bản sao lưu trước khi chạy.
   - **HTTPS:**
     - `server` cổng 80 chỉ trả `301` sang HTTPS;
     - cổng 443 với chứng chỉ trong `./ssl/` (không commit), `ssl_protocols TLSv1.2 TLSv1.3`;
     - `add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always`.
   - Nén `gzip` cho text/JSON/JS/CSS; `location /_next/static/ { expires 1y; add_header Cache-Control "public, immutable"; }`.
7. **`docker-compose.prod.yml`:**
   - `api`: **bỏ khối `ports`**, `deploy: { replicas: 3 }`;
   - `hangfire`: 1 bản sao (scale riêng khi cần — MT-47);
   - không có `seq`, `mailhog`; `env_file: .env.production` (quyền đọc giới hạn — NFR-SEC-007);
   - `OpenApi__Enabled=false`.

   Kiểm chứng: `docker compose -f docker-compose.prod.yml up -d --scale api=3` chạy được; `docker compose up -d --force-recreate --no-deps api` giữa lúc gửi request liên tục → **không có 502** (MT-50).
8. **Retrofit D-17:** production gọi `.ProtectKeysWithCertificate(cert)` cho DataProtection — chứng chỉ nạp qua biến môi trường/secret, dùng chung nguồn với bước HTTPS. Khóa trên volume `dpkeys` không còn nằm dạng rõ. Khóa này cũng ký token đặt lại mật khẩu của FR-AUTH-011.

**3. Định hướng & lý do kiến trúc:**

**SSR cho lần tải đầu, client cho các lần lọc sau:** trang tìm kiếm phục vụ hai đối tượng:
- crawler và người mở link chia sẻ cần HTML có sẵn kết quả → SSR;
- người đang lọc cần phản hồi tức thì → không đáng tải lại cả trang cho mỗi lần bấm.

Nếu đổi bộ lọc bằng `router.replace`, Next.js chạy lại Server Component **và** client cũng gọi API — mỗi lần bấm tốn hai lượt. Đồng bộ `history.pushState` với `useSearchParams` cho đúng một lượt, và URL vẫn là nguồn sự thật duy nhất (chia sẻ link, F5, back/forward đều đúng).

**Bộ lọc thời gian chạy ở cơ sở dữ liệu (MT-66), không lọc ở trình duyệt:** xem giải trình ở Buổi 6 — Dev 3. Lọc trên một trang kết quả đã phân trang làm sai `totalCount` và giấu kết quả ở các trang sau.

**Ngoại lệ phân công không làm hỏng ranh giới module:** Dev 4 không sửa handler tìm kiếm. Mọi thay đổi hợp đồng đã được Dev 3 làm ở Buổi 6 kèm integration test. Dev 4 làm việc trên hợp đồng đó như mọi chủ giao diện khác từ Buổi 5.

**Ảnh qua `/media/` thay vì sửa cấu hình Next.js** (giải trình giữ từ kế hoạch cũ): gốc rễ của D-14 là hệ thống có **hai "sự thật" về địa chỉ một tấm ảnh**:
- `localhost:9000` đúng với trình duyệt nhưng sai với container;
- `minio:9000` đúng với container nhưng sai với trình duyệt.

Mọi cách vá phía Next.js đều là duy trì hai địa chỉ song song. Một đường dẫn công khai qua Nginx xóa bỏ sự phân đôi, cho phép cache ảnh ở Nginx và **đóng cổng 9000** của MinIO ở production.

**Bỏ `ports` của `api` ở production là điều kiện cần để scale ngang:** ba container không thể cùng bind một cổng host. Tách hai file (dev giữ cổng để gọi thẳng Scalar, prod bỏ cổng + `replicas: 3`) đáp ứng cả hai nhu cầu (MT-32). Giữ resolver động thay cho khối `upstream` của SRS v1.1.0 (MT-50): `upstream` chỉ phân giải DNS lúc Nginx khởi động — container `api` được tạo lại đổi IP là 502. Buổi 2 đã gặp đúng lỗi này. Resolver động còn **tự nhận replica mới trong 10 giây** khi scale từ 3 lên 5 mà không cần reload.

**D-17 làm cùng HTTPS vì cùng cần chứng chỉ;** khóa DataProtection ký cả cookie nội bộ của ASP.NET Core lẫn token đặt lại mật khẩu — để dạng rõ trên volume nghĩa là ai đọc được volume sẽ giả mạo được token. **Cache `_next/static` `immutable` 1 năm** an toàn vì Next.js đưa hash nội dung vào tên file.

**4. Commit gợi ý:**
```
feat(search-ui,devops): client-hydrated search with multi-criteria filters and offset pagination; production nginx with tls/hsts and /media proxy (D-14), scaled prod compose and certificate-protected dataprotection keys (D-17)
```

---

# BUỔI 8 – QUẢN LÝ NGƯỜI DÙNG, BIỂU MẪU CÔNG THỨC & MÁY TRẠNG THÁI, SEO & WCAG, STRUCTURED LOGGING, LOAD TEST & ĐÓNG GÓI (LAB 8)

> **Yêu cầu tối thiểu Lab 8 của giảng viên:** đóng gói toàn bộ tính năng quản trị, tải ảnh lên, ghi nhật ký hệ thống và hoàn thiện dự án.
>
> **Điều kiện đầu vào:**
> - 48/48 endpoint chạy (Buổi 4 + CR-2026-05 ở Buổi 7);
> - nền Frontend của Buổi 5 – 6 đã merge;
> - `/media/` + HTTPS + compose production (Buổi 7) đã sẵn sàng;
> - bộ integration test chạy từ Buổi 3.
>
> **Kết thúc buổi:** **39/39 FR** hoàn thành, 17/17 hạng mục nợ kỹ thuật cần sửa code đã hoàn trả, tag `v1.0.0`.
>
> **Ngoài yêu cầu tối thiểu, mỗi dev mang theo phần kiểm thử của module mình** — E2E Playwright và danh sách kiểm tra bảo mật/hiệu năng của Buổi 8 cũ. NFR-MAINT-002 bắt buộc *"E2E: 5 luồng nghiệp vụ chính"*, nên phần này không cắt được.
>
> **Đầu buổi (Dev 4):** dựng môi trường giống production trên máy demo (`docker-compose.prod.yml`, HTTPS tự ký, dữ liệu mẫu CR-2026-03). Ba dev còn lại chạy E2E trên cùng môi trường đó.

## DEV 1 — API danh sách người dùng & Trang Quản lý Người dùng (Lab 8) + E2E xác thực & kiểm thử bảo mật

**1. Tính năng & mã yêu cầu:** Lab 8 – Dev 1.
- `FR-AUTH-008` Quản lý tài khoản [Admin] — API `GET /api/v1/users` (đã làm ở Buổi 4) + giao diện `/dashboard/users`;
- kiểm thử `NFR-SEC-001 → 007`, `NFR-MAINT-002` (E2E module Auth).

**2. Các bước triển khai:**

*Luồng dữ liệu:* `(dashboard)/dashboard/users/page.tsx` (CSR, bọc `RequireRole roles={['Admin']}`) → `useQuery({ queryKey: userKeys.list(params), queryFn: () => getUsers(params), placeholderData: keepPreviousData, staleTime: 0 })` → `GET /api/v1/users?page&pageSize&search&isActive` (`AdminPolicy`, `Cache-Control: no-store`) → `PagedResult<UserAdmin>`. Khóa/mở khóa: `useMutation` → `PATCH /api/v1/users/{id}/status { isActive, reason }` → optimistic update → lỗi thì rollback → `invalidateQueries(userKeys.all)`.

1. **API `GET /api/v1/users` — đã có từ Buổi 4 (Dev 1); buổi này rà soát và đo:**
   - Chỉ **Admin** gọi được (`AdminPolicy` — SRS FR-AUTH-008, §8.1). Lab 8 ghi *"dành cho Admin/Author"* → giữ chỉ Admin (X-8, §3); integration test Author → **403** đã có, giữ làm kiểm thử hồi quy.
   - `search` khớp một phần `email`/`displayName`: chạy `EXPLAIN ANALYZE` với 10.000 người dùng sinh bằng `PerformanceSeeder` (mở rộng của Buổi 5). Nếu `ILIKE '%…%'` cho Seq Scan quá 100 ms (ngưỡng của `DbCommandInterceptor`) → migration **`B8_Auth_UserSearchTrgm`** tạo index GIN `gin_trgm_ops` trên `NormalizedEmail` và `DisplayName` — extension `pg_trgm` đã bật từ Buổi 2. Không chậm thì **không** tạo index (NFR-PERF-004 — đo rồi mới tạo).
   - DTO `UserAdminDto` không chứa `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp` — kiểm thử JSON đã có từ Buổi 4.
2. **Giao diện `/dashboard/users`:**
   - **Thanh công cụ:**
     - ô tìm kiếm (`useDebouncedValue` 300 ms — hook Buổi 5) → `searchParams.search`;
     - `Select` trạng thái Tất cả / Đang hoạt động / Đã khóa → `searchParams.isActive`;
     - đổi điều kiện → về trang 1.
   - **`Table`** (shadcn): ảnh đại diện + tên hiển thị, email, vai trò (mỗi vai trò một `Badge`), trạng thái, ngày tạo, số công thức (`recipeCount`).
   - **`Pagination`** dùng chung.
   - **Khóa / Mở khóa:**
     - `AlertDialog` có ô **lý do bắt buộc** (Zod: 5 – 500 ký tự) — lý do đi vào audit log `ADMIN ACTION` (Buổi 4);
     - optimistic update dòng đó, lỗi thì rollback;
     - dòng của chính Admin ẩn nút khóa (server vẫn chặn **403** — MT-52 — làm lớp chặn thứ hai);
     - 404 `AUTH_USER_NOT_FOUND` → toast + tải lại danh sách.
   - **"Phân quyền" trong Lab 8** = trang chỉ Admin vào được (`RequireRole` + `AdminPolicy`), hiển thị vai trò từng người, khóa/mở khóa tài khoản. **Đổi vai trò** không có trong SRS và không nằm trong CR-2026-05 (quyết định ngày 12/10/2026) → ngoài phạm vi.
   - `features/users/` (module mới của Dev 1): `api.ts`, `query-keys.ts` (`userKeys.list(params)`), `components/UserTable.tsx`, `components/UserStatusDialog.tsx`.
3. **Playwright E2E — `frontend/e2e/auth.spec.ts`** (5 luồng, chạy trên môi trường giống production):
   - đăng ký → tự đăng nhập → vào thẳng `/dashboard` (chứng minh `AuthResponseDto` trả token);
   - đăng nhập sai 5 lần → **423**, giao diện hiện thời gian mở khóa;
   - access token hết hạn (môi trường E2E đặt `Jwt__AccessTokenMinutes=1`) → request kế tiếp tự refresh, người dùng không nhận ra;
   - Admin khóa một Author → phiên của Author bị đăng xuất ở lần refresh kế tiếp;
   - **quên mật khẩu → đọc email qua API MailHog (`GET /api/v2/messages`) → đặt lại → đăng nhập bằng mật khẩu mới** (CR-2026-05).
4. **Kiểm thử bảo mật — `docs/security-test-report.md`** (mỗi mục: cách làm, kết quả, ảnh chụp):
   - **Giả mạo IP:** gửi `X-Forwarded-For: 1.2.3.4` thẳng vào cổng 5000 (không qua Nginx) → bị bỏ qua (`KnownIPNetworks`), rate limit vẫn tính theo IP thật.
   - **Rate limit:** 11 request đăng nhập/phút → request 11 nhận **429** + `Retry-After`; giao diện hiện đếm ngược.
   - **Dashboard Hangfire:** `:5000/hangfire` không qua Nginx → 401; qua Nginx không Basic Auth → 401.
   - **Phân quyền:**
     - Author gọi `GET /users`, `PATCH /users/{id}/status` → 403;
     - Author sửa công thức của người khác → 403;
     - Author mở `/dashboard/users` → màn hình 403 phía client, API vẫn 403.
   - **Rò rỉ dữ liệu:** `/auth/me` không có `PasswordHash`/`SecurityStamp`; `/auth/sessions` không có `tokenHash`; `forgot-password` trả cùng một phản hồi cho email có và không có.
   - **Token trong trình duyệt:** `localStorage` chỉ có `refreshToken` + `user`, không có access token; trang `reset-password` không còn token trên thanh địa chỉ.
   - **Chuyển hướng mở:** `/auth/login?callbackUrl=//evil.com` → sau đăng nhập ở lại trong site.
   - **CSP:** 0 vi phạm trên mọi route theo phương án đã chốt ở Buổi 6.
   - **Log:** `grep` toàn bộ file log và Seq — không có refresh token, mật khẩu, header `Authorization`, Google `idToken`, token đặt lại mật khẩu (sau khi Dev 4 bật Serilog trong buổi này).
5. **Độ phủ:** unit test tầng Application module Auth **≥ 80 %** dòng (báo cáo coverlet); mọi luồng phát token (đăng ký/đăng nhập/Google/refresh/đặt lại mật khẩu) đi qua `AuthResponseFactory` hoặc `RevokeAll…` có sẵn — không luồng nào tự dựng.

**3. Định hướng & lý do kiến trúc:**

**Danh sách người dùng chỉ dành cho Admin, dù Lab 8 ghi "Admin/Author":** `UserAdminDto` chứa email và trạng thái khóa của **mọi** người dùng. Mở cho Author nghĩa là bất kỳ ai tự đăng ký (Author là vai trò mặc định — FR-AUTH-001) đều tải được toàn bộ danh sách email của hệ thống — rò rỉ dữ liệu cá nhân, trái nguyên tắc quyền tối thiểu và trái SRS FR-AUTH-008. Author không có nghiệp vụ nào cần danh sách này.

**Khóa tài khoản phải thu hồi toàn bộ refresh token, không chỉ gán cờ** (giải trình giữ từ kế hoạch cũ, đã cài ở Buổi 4): chỉ gán `IsActive = false` thì người bị khóa vẫn gia hạn phiên bình thường. **Access token cũ vẫn sống tối đa 15 phút** sau khi khóa — đánh đổi cố hữu của JWT stateless, SRS ghi là chấp nhận được (O-1 trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §6). Thu hồi tức thời đòi tra cơ sở dữ liệu ở **mọi** request — vứt bỏ lợi ích stateless trên đường nóng nhất của hệ thống.

**Chặn Admin tự khóa chính mình (403):** hệ thống chỉ có một Admin mà người đó bấm nhầm thì **không còn ai mở khóa được**. Mã 403 đúng ngữ nghĩa: yêu cầu hợp lệ, không xung đột trạng thái — chỉ là người gọi không được làm điều này với chính mình (MT-52). Ẩn nút trên giao diện là trải nghiệm; 403 ở server mới là bảo vệ.

**Lý do khóa bắt buộc:** audit log `ADMIN ACTION: user … deactivated by …, reason: …` chỉ có giá trị khi có lý do. Bắt buộc ngay ở biểu mẫu thì không có bản ghi audit nào trống.

**Đo trước, tạo index sau:** tìm kiếm `ILIKE '%an%'` không dùng được B-tree. Với vài trăm người dùng thì Seq Scan nhanh hơn index; với 10.000 người dùng thì có thể không. Index trigram có chi phí ghi và dung lượng, nên chỉ tạo khi `EXPLAIN ANALYZE` chứng minh cần — cùng tiêu chí NFR-PERF-004 mà Dev 3 áp dụng ở Buổi 5.

**Kiểm thử giả mạo `X-Forwarded-For` là mục quan trọng nhất của danh sách bảo mật:** bật forwarded headers **mà không giới hạn mạng tin cậy** khiến hệ thống kém an toàn hơn cả khi không bật — kẻ tấn công đổi header mỗi request là né hoàn toàn rate limit. Đây là loại cấu hình "trông có vẻ đúng" mà chỉ kiểm thử chủ động mới phát hiện.

**4. Commit gợi ý:**
```
feat(admin-users): admin user management page with debounced search, status filter and audited lock/unlock; e2e auth flows incl. password reset and security test report
```

---

## DEV 2 — Biểu mẫu Thêm/Sửa Công thức (RHF + Zod + `useFieldArray`), Tải ảnh MinIO & Máy trạng thái khép kín (Lab 8)

**1. Tính năng & mã yêu cầu:** Lab 8 – Dev 2. Giao diện hoàn chỉnh của:
- `FR-RCP-003` Tạo, `FR-RCP-004` Sửa (concurrency), `FR-RCP-009/010` Nguyên liệu/Các bước;
- `FR-RCP-008` Ảnh công thức (MinIO) + `FR-FILE-001` (ảnh bước nấu);
- `FR-RCP-005/006/007` — xuất bản / hủy xuất bản / lưu trữ / khôi phục / xóa mềm.

Kèm E2E vòng đời công thức và kiểm thử đồng thời (`NFR-MAINT-002`).

**2. Các bước triển khai:**

1. **Schema — `features/recipes/schemas.ts` + `features/recipes/limits.ts`:**
   - `limits.ts` chép đúng **SRS §7.9 Bảng Giới hạn Dữ liệu Chuẩn** (tiêu đề 5 – 200, mô tả ≤ 2000, khẩu phần, thời gian, tên nguyên liệu, `quantityText` ≤ 50…). Chỉ có một nguồn cho mọi `z.string().max(...)`, khớp validator FluentValidation phía Backend.
   - `recipeFormSchema`:
     ```ts
     z.object({
       title, description, categoryId: z.uuid(), prepTimeMinutes, cookTimeMinutes, servings,
       difficulty: z.enum(RECIPE_DIFFICULTIES), nutrition: nutritionSchema.partial().nullable(),
       ingredients: z.array(ingredientSchema), steps: z.array(stepSchema),
     })
     ```
   - `ingredientSchema` có `.refine(i => i.quantity != null || !!i.quantityText || !!i.unit, { path: ['quantity'], message: 'INGREDIENT_QUANTITY_REQUIRED' })` — cùng quy tắc với Backend. Thông báo tiếng Việt lấy từ `ERROR_MESSAGES` theo mã.
   - `stepSchema` **không có `stepNumber`** (server tự đánh số — FR-RCP-010).
   - **`serverId` thay cho `id` trong model của biểu mẫu:** `useFieldArray` tự sinh khóa `id` cho từng dòng và **ghi đè** trường `id` của dữ liệu. Hàm `toFormValues(recipe)` đổi `id` → `serverId`; `toRequests(values)` đổi ngược lại.
2. **`features/recipes/components/RecipeWizard.tsx` — một `useForm` cho cả biểu mẫu nhiều bước:**
   - `useForm<RecipeFormValues>({ resolver: zodResolver(recipeFormSchema), mode: 'onBlur', defaultValues })` + `FormProvider`; mỗi bước là một component đọc `useFormContext()`.
   - **5 bước:** 1 Thông tin & dinh dưỡng → 2 Nguyên liệu → 3 Các bước → 4 Ảnh → 5 Xem lại & xuất bản.
   - Bấm "Tiếp" gọi `trigger([...trường của bước])`; lỗi thì ở lại và focus ô lỗi đầu tiên.
   - Thanh tiến trình bước dạng `<ol>` có `aria-current="step"`.
   - **Bước Nguyên liệu:**
     - `useFieldArray({ name: 'ingredients' })` — `append`, `remove`, `move`; mỗi dòng: tên, ô số `quantity`, ô chữ `quantityText`, đơn vị, ghi chú;
     - gợi ý *"nhập số **hoặc** cách viết như 'vừa đủ'"*;
     - lỗi `INGREDIENT_QUANTITY_REQUIRED` hiện đúng dòng.
   - **Bước Các bước:** `useFieldArray({ name: 'steps' })` — tiêu đề, mô tả, `timerMinutes`, ảnh minh họa (`ImageUploader` → `POST /files/upload` → `imageUrl`); đổi thứ tự bằng nút ↑/↓ gọi `move()` (giữ cách làm của Buổi 5 — dùng được bằng bàn phím); số thứ tự hiển thị tính từ vị trí, không lưu.
   - Dữ liệu chưa lưu: `formState.isDirty` → cảnh báo `beforeunload` khi rời trang.
3. **Trang `(dashboard)/dashboard/recipes/new/page.tsx` — tạo mới, chỉ một request ghi nội dung:**
   - Bước 1 – 3 điền xong → **`POST /api/v1/recipes`** với đủ thông tin + mảng `ingredients` + mảng `steps` (Buổi 4 đã bật mảng nội tuyến).
   - Nhận `id` của bản nháp → `router.replace('/dashboard/recipes/{id}/edit?step=images')` → tiếp bước 4 Ảnh trên bản nháp đã có `id`.
4. **Trang `(dashboard)/dashboard/recipes/[id]/edit/page.tsx` — sửa, nâng cấp trang Buổi 5:**
   - **Nạp:** `useQuery(recipeKeys.mineById(id), () => getMyRecipe(id))` → `GET /recipes/mine/{id}` (mọi trạng thái, có `rowVersion` + `ETag`) → `reset(toFormValues(recipe))`.
   - **Lưu — đồng bộ theo phần thay đổi** (`features/recipes/sync-recipe.ts`), chạy tuần tự:
     1. trường chính đổi → `PUT /recipes/{id}` kèm `If-Match: "{rowVersion}"`. Gặp **409 `RECIPE_CONCURRENCY_CONFLICT`** → **dừng**, hiện dialog *"Công thức đã được cập nhật ở nơi khác — Tải lại / Xem khác biệt"* (bảng so sánh giá trị đang sửa với giá trị mới nhất);
     2. so mảng ban đầu với mảng hiện tại theo `serverId`: dòng mới → `POST`, dòng đổi → `PUT`, dòng bị xóa → `DELETE` (nguyên liệu và các bước);
     3. thứ tự bước đổi → `PATCH /recipes/{id}/steps/reorder { stepIds }` theo thứ tự cuối;
     4. xong → `invalidateQueries(recipeKeys.mineById(id))` để lấy `rowVersion` mới.
   - Một lần bấm "Lưu" thay cho từng dòng tự gọi API như `RecipeContentEditor` của Buổi 5 (component đó được thay thế).
   - Công thức đã từng Published: ô tiêu đề hiện ghi chú *"Đổi tiêu đề không đổi đường dẫn"* (khóa slug — MT-27).
5. **Bước Ảnh — tải lên MinIO tích hợp vào biểu mẫu (FR-RCP-008):**
   - `ImageUploader` (Buổi 2; thanh tiến trình `XMLHttpRequest.upload.onprogress` — NFR-USE-004) → `POST /recipes/{id}/images` (multipart).
   - Lỗi:
     - **429** (rate limit upload 5/phút — Buổi 5) → *"Thử lại sau N giây"*;
     - **400** `FILE_SIZE_EXCEEDED` / `FILE_MIME_INVALID` → lỗi ngay trên ô tải;
     - **503** `FILE_STORAGE_UNAVAILABLE` → thông báo hệ thống lưu ảnh tạm ngừng.
   - Thư viện ảnh: chọn ảnh chính (`PATCH …/images/{imageId} { isPrimary: true }`), sửa `altText`, xóa (`DELETE`).
   - **`altText` bắt buộc trước khi xuất bản** — danh sách kiểm tra xuất bản chặn (WCAG 1.1.1).
   - Hiển thị `thumbnailUrl ?? originalUrl` — job đổi kích thước (FR-JOB-002) có thể chưa chạy xong.
6. **Máy trạng thái khép kín — `features/recipes/status-transitions.ts`** (bản Frontend của bảng `RecipeStatusTransitions` — Buổi 4, Dev 4):

   | Từ trạng thái | Hành động hợp lệ | Endpoint | Sang trạng thái |
   |---|---|---|---|
   | Draft | Xuất bản | `PATCH /recipes/{id}/publish` | Published |
   | Draft | Lưu trữ | `PATCH /recipes/{id}/archive` | Archived |
   | Published | Hủy xuất bản | `PATCH /recipes/{id}/unpublish` | Draft |
   | Published | Lưu trữ | `PATCH /recipes/{id}/archive` | Archived |
   | Archived | Khôi phục về nháp | `PATCH /recipes/{id}/unarchive` | **Draft** |
   | Mọi trạng thái | Xóa (xóa mềm) | `DELETE /recipes/{id}` | — (ẩn khỏi mọi endpoint) |

   - **`RecipeStatusActions.tsx`** chỉ hiện các hành động hợp lệ với trạng thái hiện tại. Dùng ở menu từng dòng của `/dashboard/recipes` (Buổi 7) và thanh thao tác trang sửa.
   - **Xuất bản** hiện danh sách kiểm tra (Buổi 5): ≥ 1 nguyên liệu, ≥ 1 bước, ảnh chính có `altText`; thiếu thì nút bị khóa kèm lý do.
   - **Xóa:** `AlertDialog` bắt **gõ đúng tên công thức** mới bật nút xóa.
   - Mỗi hành động là một `useMutation`; thành công → `invalidateQueries({ queryKey: ['recipes','private'] })` + toast.
   - **409 `RECIPE_INVALID_STATE_TRANSITION`** (người khác đổi trạng thái trước) → *"Trạng thái công thức vừa thay đổi ở nơi khác"* + tải lại.
   - **400 `RECIPE_PUBLISH_INCOMPLETE`** → mở bước còn thiếu.
   - **Kiểm thử chống lệch:** `status-transitions.test.ts` đọc `RecipeStatusTransitions.cs` (regex trên các cặp `RecipeStatus.X → RecipeStatus.Y`) và so với bảng Frontend — đổi luật ở Backend mà quên Frontend là đỏ. Cùng kỹ thuật kiểm thử mã lỗi của Buổi 5.
7. **E2E & kiểm thử đồng thời** (phần Buổi 8 cũ của Dev 2):
   - **`frontend/e2e/recipe-lifecycle.spec.ts` — vòng đời đầy đủ:** tạo qua biểu mẫu (thông tin → nguyên liệu → các bước → ảnh) → xuất bản → xuất hiện ở trang công khai → lưu trữ → khôi phục về Draft → xóa → biến mất khỏi `/recipes/mine` và trang công khai trả 404. Thêm luồng đổi thứ tự bước → F5 vẫn đúng thứ tự, `StepNumber` liên tục 1..N.
   - **`frontend/e2e/recipe-concurrency.spec.ts`:** hai tab mở cùng công thức, lưu tab 1 rồi lưu tab 2 → tab 2 hiện dialog 409.
   - **Integration test** (`Task.WhenAll`): 10 lệnh `PUT` đồng thời cùng `rowVersion` → đúng **1** thành công, **9** nhận 409, **0** lỗi 500.
   - **Hồi quy MT-34 (bắt buộc):** Author tạo Draft → Admin gọi `GET /recipes` → Guest gọi lại **đúng URL đó** → Guest không thấy Draft; `GET /recipes/mine` có `Cache-Control: no-store`.
   - **Đếm truy vấn chống N+1:** bật log EF Core trong kiểm thử, `GetRecipeBySlug` ≤ 2 truy vấn.

**3. Định hướng & lý do kiến trúc:**

**Một `useForm` cho cả biểu mẫu nhiều bước:** mỗi bước một form riêng thì dữ liệu phải truyền qua lại giữa các form, "Quay lại" dễ mất dữ liệu, và không có chỗ nào biết toàn bộ công thức hợp lệ hay chưa. Một form + `trigger()` theo từng bước giữ một nguồn sự thật. React Hook Form dùng ô nhập không điều khiển (uncontrolled), nên biểu mẫu 14 nguyên liệu × 5 trường không re-render cả cây mỗi lần gõ.

**Zod khớp validator Backend qua cùng một bảng giới hạn:** người dùng nhận lỗi ngay khi rời ô (không chờ round-trip), nhưng **server vẫn kiểm tra** — Zod là trải nghiệm, FluentValidation là bảo vệ. Hai bên cùng đọc SRS §7.9 thì không có chuyện client cho 250 ký tự còn server chặn ở 200.

**Đồng bộ theo phần thay đổi trên 8 endpoint có sẵn, không thêm endpoint "lưu cả gói":** SRS Chương 8 thiết kế nguyên liệu và các bước thành tài nguyên con có endpoint riêng (FR-RCP-009/010), và bề mặt API bị `ApiSurfaceTests` khóa. Thêm endpoint "PUT toàn bộ" phải qua Change Request và lặp lại mọi quy tắc nghiệp vụ trong một handler mới. Tính phần khác biệt ở client rồi gọi đúng endpoint có sẵn cho cùng kết quả mà không đổi hợp đồng. `PUT` trường chính chạy **trước** để xung đột concurrency được phát hiện trước khi bất kỳ tài nguyên con nào bị sửa.

**Tạo mới bằng một `POST` có mảng nội tuyến:** cách Buổi 3 (tạo bản nháp rỗng ở bước 1 rồi thêm dần) để lại bản nháp rỗng khi người dùng bỏ ngang, và tốn N request cho N nguyên liệu. Gửi một lần sau khi đã có đủ nội dung → bản nháp sinh ra luôn đầy đủ. Ảnh là ngoại lệ duy nhất: endpoint ảnh cần `id` công thức, nên ảnh là bước sau khi đã có bản nháp.

**Bảng chuyển trạng thái phía Frontend sinh ra nút, không rải `if`** (giải trình giữ từ kế hoạch cũ): luật nằm rải rác thì thêm một trạng thái mới (ví dụ `PendingReview`) phải sửa 4 – 5 chỗ và chắc chắn sót. Một bảng tra cứu tập trung khiến mở rộng chỉ là thêm dòng, và bảng là tài liệu sống của máy trạng thái. **Khôi phục về Draft, không về trạng thái trước khi lưu trữ:** phương án "khôi phục như cũ" đòi thêm cột `StatusBeforeArchive` chỉ cho một thao tác hiếm. Về Draft **buộc tác giả rà soát lại** trước khi công khai lần nữa — công thức bị lưu trữ thường vì nội dung đã cũ (MT-35).

**Xóa mềm và ba hệ quả bắt buộc** (MT-05):
- partial unique index — tạo lại công thức cùng tên dùng lại được slug;
- tài nguyên con không xóa theo (vô hình qua Global Query Filter);
- **không xóa file MinIO khi xóa mềm** — nếu xóa, khôi phục sẽ mất toàn bộ ảnh. Việc dọn file thuộc FR-JOB-003 sau 30 ngày.

Hộp xác nhận bắt gõ tên công thức vì với người dùng, xóa mềm vẫn là "biến mất" — khôi phục chỉ làm được qua quản trị viên vận hành (MT-49).

**Mọi lỗi bảo mật đã sửa phải có kiểm thử tái hiện kịch bản gốc:** MT-34 không biểu hiện gì trong sử dụng thông thường — chỉ lộ khi đúng trình tự (Admin truy cập trước, Guest truy cập sau trong cửa sổ TTL). Không có kiểm thử này, lần "tối ưu cache" sau có thể bật lại lỗ hổng mà code trông vẫn hợp lý.

**4. Commit gợi ý:**
```
feat(recipes-ui): react-hook-form wizard with zod and field arrays for create/edit with diff sync, minio image step and closed state-machine actions (publish, unpublish, archive, unarchive, soft delete); e2e lifecycle and concurrency tests
```

---

## DEV 3 — Tối ưu SEO (JSON-LD Recipe, Open Graph, Sitemap) & Responsive / WCAG 2.1 AA (Lab 8)

**1. Tính năng & mã yêu cầu:** Lab 8 – Dev 3.
- `NFR-SEO-001` JSON-LD Schema.org `Recipe`;
- `NFR-SEO-002` meta/Open Graph, `noindex`;
- `NFR-SEO-003` sitemap + robots (chuyển từ Buổi 7 cũ);
- `NFR-USE-001` responsive, `NFR-USE-002` WCAG 2.1 AA, `NFR-PERF-005` Core Web Vitals;
- **retrofit D-14 phía Next.js**.

Kèm E2E tìm kiếm/bộ lọc.

**2. Các bước triển khai:**

1. **JSON-LD — `lib/seo/recipe-json-ld.ts` → `buildRecipeJsonLd(recipe: Recipe, siteUrl: string)`:**
   - `@type: "Recipe"`, `name`, `description`;
   - `image` — mảng URL tuyệt đối `MediumUrl`/`OriginalUrl` qua `/media/`;
   - `author: { @type: "Person", name }`, **`datePublished`** từ `PublishedAt` (gán một lần — Buổi 4);
   - `prepTime`/`cookTime`/`totalTime` dạng **ISO 8601** (`PT15M`), `recipeYield`, `recipeCategory`;
   - **`recipeIngredient[]`** — ưu tiên `quantityText`, không có thì ghép `quantity + unit + name`;
   - `recipeInstructions[]` dạng `HowToStep` (`name`, `text`, `image`, `url: …#buoc-{n}`);
   - `nutrition: { @type: "NutritionInformation", calories: "350 calories", … }`.
   - **Không nhúng `aggregateRating`** — không có hệ thống đánh giá (MT-24).
   - Thêm `BreadcrumbList` cho trang công thức và trang danh mục; `WebSite` + `SearchAction` (ô tìm kiếm trên kết quả Google) cho trang chủ.
   - **`components/seo/JsonLd.tsx`:** `<script type="application/ld+json">` với `JSON.stringify(data).replace(/</g, '\\u003c')`. Không thoát `<` thì một công thức có tiêu đề chứa `</script><script>…` sẽ **chèn được script vào trang** (XSS qua dữ liệu người dùng).
   - Gắn vào trang của Dev 2: `<JsonLd data={buildRecipeJsonLd(recipe, SITE_URL)} />` — một dòng trong `(public)/recipes/[slug]/page.tsx`, Dev 2 rà soát.
2. **Metadata — `lib/seo/metadata.ts`:**
   - `buildRecipeMetadata(recipe)`:
     - `title` = `"{Tên} | Culinary Blog"` **cắt ở 60 ký tự khi render** (theo ranh giới từ, thêm `…` — MT-37);
     - `description` = 160 ký tự đầu của `Description`;
     - `alternates.canonical`;
     - `openGraph` (`type: 'article'`, `publishedTime`, `authors`), `twitter: { card: 'summary_large_image' }`.
   - **Ảnh Open Graph 1200 × 630:** `(public)/recipes/[slug]/opengraph-image.tsx` dùng `ImageResponse` của `next/og` (có sẵn trong Next.js) — ảnh món + tên món + logo. Ảnh gốc 800 × 600 không đúng tỉ lệ thẻ chia sẻ.
   - `buildCategoryMetadata`, metadata trang chủ, `/search` (`noindex` khi có `q` — trang kết quả tìm kiếm không nên vào chỉ mục).
   - `(dashboard)`, `(auth)`, `(dev)/mockups` mang `robots: { index: false }` (Buổi 6 – 7).
3. **Sitemap & robots** (chuyển từ Buổi 7 cũ):
   - **`app/sitemap.ts`** (`revalidate = 3600`):
     - nguồn: `GET /api/v1/recipes/sitemap` — một request trả mọi `{ slug, updatedAt }` Published (MT-48, Buổi 4), cache 1 giờ đúng bằng chu kỳ sinh lại;
     - cộng danh mục + trang tĩnh (`/`, `/recipes`, `/categories`, `/search`);
     - lời gọi API tách ra `lib/seo/sitemap-source.ts` để phần sinh XML kiểm thử được độc lập.
   - **`app/robots.ts`:** `allow: '/'`, `disallow: ['/dashboard/', '/profile', '/auth/', '/api/', '/hangfire', '/scalar', '/mockups']`, `sitemap: 'https://<domain>/sitemap.xml'`. **Không** gọi ping Google.
4. **Retrofit D-14 phía Next.js** (sau khi Dev 4 dựng `/media/` ở Buổi 7):
   - gỡ `unoptimized`;
   - `images.remotePatterns` cho `/media/**` và ảnh đại diện Google (`lh3.googleusercontent.com`);
   - rà mọi `<Image>`: `sizes` đúng breakpoint (`(min-width:1200px) 25vw, (min-width:768px) 50vw, 100vw` cho lưới), `priority` cho ảnh LCP (trang chủ, trang chi tiết);
   - lưới dùng `ThumbnailUrl`.
5. **Responsive (NFR-USE-001):** kiểm 320 / 768 / 1200 px trên mọi route — điện thoại một cột, máy tính bảng hai cột, máy tính bố cục đầy đủ; bộ lọc/menu dạng `Sheet` dùng được bằng cả chạm lẫn bàn phím.
6. **WCAG 2.1 AA (NFR-USE-002):**
   - `@axe-core/playwright` (đã có trong devDependencies) quét **mọi route** công khai và dashboard → **0 vi phạm mức serious/critical**.
   - Rà thủ công:
     - landmark `header/nav/main/aside/footer`, thứ tự heading không nhảy cấp;
     - điều hướng đầy đủ bằng Tab/Enter/Escape, vòng focus nhìn thấy được (`--ring`);
     - focus trap trong `Dialog`/`Sheet` (Radix);
     - `alt` từ `AltText`, ô nhập có nhãn và `aria-describedby` (shadcn `Form`);
     - tương phản ≥ 4,5 : 1 (đã sửa token ở Buổi 6);
     - `prefers-reduced-motion` tắt hiệu ứng chuyển động; `lang="vi"`.
   - Báo cáo `docs/a11y-report.md`.
7. **Core Web Vitals (NFR-PERF-005) — Lighthouse CI** (`@lhci/cli` đã có): LCP ≤ 2,5 s, CLS ≤ 0,1, INP ≤ 200 ms, First Load JS ≤ 200 KB (gzip) trên trang chủ, `/recipes`, trang chi tiết → `docs/lighthouse-report.md`.
8. **E2E — `frontend/e2e/search.spec.ts`:**
   - gõ "pho" (không dấu) → thấy "Phở bò" (`unaccent`);
   - áp bộ lọc **kể cả thời gian (MT-66)** + phân trang → URL giữ trạng thái, F5 và back/forward đúng;
   - `GET /recipes?sortBy=maliciousColumn` → **400**;
   - Admin xóa danh mục còn công thức → **409**.
9. **Kiểm chứng SEO — `docs/seo-report.md`:**
   - Google Rich Results Test đạt cho loại Recipe;
   - `<title>` thực tế ≤ 60 ký tự sau cắt;
   - `/recipes/<slug-Draft>` → HTTP **404**;
   - `/sitemap.xml` không chứa slug Draft/Archived;
   - `/robots.txt`, `/sitemap.xml` trả ở **domain chính**.

**3. Định hướng & lý do kiến trúc:**

**Giới hạn SEO là quy tắc render, không phải ràng buộc dữ liệu** (MT-37): `<title>` ≤ 60 ký tự mà hậu tố `" | Culinary Blog"` đã chiếm 16 → tên món chỉ còn 44 ký tự, trong khi "Bún bò Huế chuẩn vị cố đô nấu bằng nồi áp suất" đã 47. Ép người dùng đặt tên ngắn để chiều Google là vô lý; cắt chuỗi lúc render mới đúng chỗ.

**Không nhúng `aggregateRating`** (MT-24): không có hệ thống đánh giá nên không có dữ liệu thật; nhúng dữ liệu giả vi phạm chính sách structured data của Google — nguy cơ bị phạt toàn site.

**Thoát `<` trong JSON-LD:** `JSON.stringify` không thoát `</script>`. Đây là đường XSS hay bị bỏ sót nhất của structured data, vì dữ liệu (tiêu đề, mô tả) do người dùng nhập và được nhúng nguyên vào HTML. Một `replace` chặn hẳn.

**Sitemap ở Next.js, nơi crawler đi tìm** (MT-28, giải trình giữ từ kế hoạch cũ): crawler tìm `https://domain/sitemap.xml` — domain do Next.js phục vụ. Sinh ở Backend rồi để trên MinIO hay `wwwroot` của API là đặt file ở địa chỉ không khớp `robots.txt`. Sinh động với ISR 1 giờ thì không có file nào mất khi deploy, không có bản nào lệch giữa các instance. **Bỏ ping Google** vì Google đã ngừng hỗ trợ endpoint ping từ tháng 6/2023.

**Ngưỡng 0 vi phạm mức serious/critical:** axe báo cả mục cần đánh giá thủ công và cảnh báo minor không áp dụng cho ngữ cảnh. Ngưỡng này đạt được và có ý nghĩa, thay vì một con số lý tưởng khiến cả nhóm bỏ cuộc. Phần lớn công việc (Radix, nhãn form, tương phản) đã làm đúng từ Buổi 6 — buổi này là **kiểm chứng và vá nốt**.

**4. Commit gợi ý:**
```
feat(seo,a11y): recipe json-ld and open graph metadata with og image, nextjs sitemap and robots, optimized next/image via /media (D-14), responsive and wcag 2.1 aa fixes with lighthouse and e2e search tests
```

---

## DEV 4 — Serilog Structured Logging, Load Test & Đóng gói Docker Compose 8 service (Lab 8)

**1. Tính năng & mã yêu cầu:** Lab 8 – Dev 4.
- `FR-OBS-002` Structured Logging (CorrelationId, thời gian request/response, audit log) + `FR-OBS-003` Tracing & Metrics (mức **C**);
- `CONS-010` (mọi log có `CorrelationId`, `RequestPath`, `UserId`), `NFR-SEC-007` (cấm log bí mật);
- `NFR-PERF-001/002/003` (load test, hit rate), `NFR-REL-002` (chịu lỗi khi có tải), `NFR-MAINT-003` (README, ADR, CHANGELOG), `NFR-SCALE-003`, `CONS-009` (Docker Compose);
- audit D-6.

**2. Các bước triển khai:**

*Luồng log:* Nginx sinh `X-Correlation-ID` (`$request_id`, Buổi 2) hoặc nhận từ Frontend (`crypto.randomUUID()`, Buổi 6) → API `CorrelationIdMiddleware` (đầu pipeline, ngay sau `UseForwardedHeaders`) kiểm định dạng → `LogContext.PushProperty(CorrelationId, RequestPath, UserId)` → mọi log của request (MediatR `LoggingBehavior`, EF Core interceptor, `GlobalExceptionMiddleware`) mang 3 trường → sinks Console (JSON compact), File (xoay vòng theo ngày), Seq → khi enqueue job Hangfire, CorrelationId đi theo vào worker.

1. **Serilog** — các gói `Serilog.AspNetCore`, `Serilog.Formatting.Compact`, `Serilog.Sinks.Seq` đã tham chiếu từ Buổi 2 nhưng chưa nối dây:
   - `builder.Host.UseSerilog((ctx, services, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().Enrich.WithProperty("Application", workerOnly ? "CulinaryBlog.Hangfire" : "CulinaryBlog.API"))` — áp cho **cả** container `api` và `hangfire` (cùng image).
   - `appsettings.json`: Console (`CompactJsonFormatter`), File (`logs/culinaryblog-.json`, xoay vòng theo ngày, giữ 14 file), Seq (`Seq__ServerUrl=http://seq:5341`); mức log Debug (dev) / Information (prod); `Microsoft.EntityFrameworkCore.Database.Command` ở Warning.
2. **Thời gian request/response — `app.UseSerilogRequestLogging(o => …)`:**
   - mẫu `"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms"`;
   - `EnrichDiagnosticContext` thêm `UserId`, `ClientIp` (IP thật), `CorrelationId`, `ApiVersion`;
   - `GetLevel`: 5xx → Error, > 500 ms → Warning, còn lại → Information;
   - `/health*` → Verbose (Docker gọi mỗi 10 giây — không làm ngập log).
3. **`API/Middleware/CorrelationIdMiddleware.cs`** (SRS §6.2 gọi tên):
   - đọc `X-Correlation-ID`; **chỉ chấp nhận ≤ 64 ký tự `[A-Za-z0-9-]`**, ngược lại sinh mới;
   - ghi lại vào response header (CORS đã expose từ Buổi 5);
   - `GlobalExceptionMiddleware` thêm `correlationId` vào `extensions` của Problem Details → Frontend hiện "Mã tra cứu" (Buổi 6).
   - **Hangfire:** `CorrelationIdJobFilter : IClientFilter, IServerFilter` lưu CorrelationId vào tham số job lúc enqueue và `LogContext.PushProperty` lúc worker chạy → log của `WelcomeEmailJob`, `ImageResizeJob`, `PasswordResetEmailJob`, `PermanentPurgeJob` nối được với request đã sinh ra chúng.
4. **Audit log — trong `LoggingBehavior`, không thêm behavior thứ năm:**
   - command (không phải query) thành công → `Log.Information("AUDIT {Command} by {UserId} succeeded in {Elapsed} ms")`; thất bại → Warning kèm mã lỗi;
   - **không bao giờ ghi payload** của command;
   - cảnh báo request > 500 ms giữ nguyên.
   - Ngưỡng **> 100 ms cho một truy vấn** cài bằng `Infrastructure/Persistence/Interceptors/SlowQueryInterceptor : DbCommandInterceptor` (ghi câu SQL, **không** ghi giá trị tham số; `EnableSensitiveDataLogging` tắt).
5. **Lọc dữ liệu nhạy cảm (NFR-SEC-007):**
   - `SensitiveDataDestructuringPolicy` che các thuộc tính tên `password`, `currentPassword`, `newPassword`, `refreshToken`, `accessToken`, `idToken`, `token`, `authorization`;
   - request logging không ghi body/header `Authorization`.
   - **`LogRedactionTests`** (sink trong bộ nhớ) gọi `login`, `refresh`, `google` (validator giả), `change-password`, `reset-password` rồi khẳng định không log nào chứa các chuỗi bí mật đã gửi.
6. **OpenTelemetry (FR-OBS-003 — mức C):** instrumentation AspNetCore, HttpClient, EF Core/Npgsql; metric `recipes_created_total`, `recipes_published_total`, histogram thời gian request; OTLP → Seq (dev); log mang `TraceId`/`SpanId`. **Thiếu thời gian thì cắt mục này trước** — không FR mức M/S nào phụ thuộc (MT-40).
7. **Audit cache + hit rate** (chuyển từ Buổi 7 cũ):
   - bảng đối chiếu **mọi** command với khóa phải xóa, so với cột "Invalidate khi" ở §2.3;
   - `RedisCacheService.RemoveByPrefixAsync` dùng **`SCAN`** (không dùng `KEYS` — block Redis), khớp tiền tố instance `culinaryblog:`;
   - xác nhận không còn dấu vết Output Cache (D-6) và không khóa nào thuộc endpoint riêng tư;
   - hit rate đo trong lúc load test bằng `redis-cli INFO stats` → `docs/cache-report.md`.
8. **Load test k6 — `tests/load/{smoke,load,stress}.js`:**
   - *smoke* (1 VU, 1 phút) → *load* (**100 người dùng đồng thời**, 5 phút) → *stress* (tăng dần tới ngưỡng gãy);
   - tỉ lệ: 70 % đọc danh sách/chi tiết (Guest), 20 % tìm kiếm, 10 % ghi (Author đăng nhập sẵn);
   - ngưỡng: **p50 ≤ 150 ms, p95 ≤ 500 ms, p99 ≤ 1000 ms**, Redis hit rate ≥ 80 %.
   - **Môi trường đo nâng hạn mức rate limit** (`RateLimiting__Global__PermitLimit=1000000`, tương tự cho `Auth`, `Upload` — cấu hình Buổi 5): k6 chạy từ **một máy** nên mọi request cùng một IP. Giữ hạn mức thật thì từ giây thứ 2 hầu hết request là 429, con số đo vô nghĩa. Báo cáo ghi rõ việc nâng hạn mức; rate limit đã có kiểm thử riêng (Buổi 5, Buổi 8 — Dev 1).
   - **Chịu lỗi khi đang có tải (NFR-REL-002):**
     - `docker compose stop redis` giữa lúc chạy → hệ thống dùng phương án dự phòng là cơ sở dữ liệu, không ném exception, chỉ chậm hơn;
     - `stop minio` → API vẫn phục vụ công thức, chỉ tải ảnh lên trả 503.
   - Báo cáo `docs/load-test-report.md`: biểu đồ p50/p95/p99, throughput, hit rate, ghi trung thực nếu chưa đạt kèm phân tích.
9. **Đóng gói Docker Compose 8 service (CONS-009):**
   - **Dev:** `docker compose up -d --build` → 8 service `nginx, api, hangfire, frontend, postgres, redis, minio, seq` đều `healthy` + `mailhog` (dev).
   - **Prod (Buổi 7):** `--scale api=3`, `CorrelationId` rơi vào các container khác nhau trong log (chứng minh Nginx luân phiên).
   - **Gia cố image** (multi-stage + người dùng non-root đã có từ Buổi 2):
     - tách layer `dotnet restore`/`npm ci` để tận dụng cache build; rà `.dockerignore`;
     - **ghim tag** image nền thay cho `latest` (`seq`, …);
     - quét Trivy — không chấp nhận lỗ hổng Critical.
   - Tài liệu:
     - `README.md` cài đặt **< 5 phút**;
     - `CHANGELOG.md` (Keep a Changelog);
     - **ADR** trong `docs/adr/`:
       - 6 quyết định của kế hoạch cũ: MT-05 xóa mềm, MT-08/09 mã lỗi, MT-16 Redis, MT-34 cách ly cache, MT-47 Hangfire worker riêng, MT-50 Nginx resolver động;
       - **4 quyết định của Buổi 5 – 8:** chia state (Zustand cho phiên phía client, TanStack Query cho dữ liệu server, refresh token là thứ duy nhất lưu bền); `ky` thay vì `axios` để giữ Data Cache; `Asp.Versioning` + thuật toán rate limit theo SRS; **CSP có nonce ↔ mọi trang render theo request, độ tươi đặt ở Data Cache** (MT-67, kèm bảng kết quả thực nghiệm).
   - Gắn tag **`v1.0.0`** — phiên bản **phần mềm** phát hành lần đầu, độc lập với số phiên bản SRS (v1.2.3).

**3. Định hướng & lý do kiến trúc:**

**CorrelationId biến log từ "đống text" thành công cụ điều tra** (giải trình giữ từ kế hoạch cũ): người dùng báo *"bấm Xuất bản lúc 3 giờ chiều thì lỗi"* — không có correlation id thì phải mò hàng nghìn dòng log. Có nó, hiện trên giao diện lỗi ở Frontend, người dùng đọc mã, dev dán vào Seq, ra đúng chuỗi log của request đó qua mọi tầng **và cả job nền** nó sinh ra.

**Kiểm định dạng CorrelationId nhận từ client:** header do client gửi lên. Nhận nguyên một chuỗi 10 KB hoặc chứa ký tự xuống dòng là cho phép kẻ tấn công làm ngập log hoặc chèn dòng log giả (log injection). Giới hạn 64 ký tự chữ-số-gạch rồi sinh mới khi không hợp lệ là đủ an toàn mà không mất khả năng truy vết.

**Audit nằm trong `LoggingBehavior` vì SRS §6.3 chốt đúng 4 behavior:** thêm `AuditBehavior` là đi ngược quyết định MT-31 (đã bỏ `PerformanceBehavior` vì cùng lý do — đo thời gian và ghi log là việc tự nhiên của logging). Hai ngưỡng thuộc hai chủ thể: `LoggingBehavior` đo **cả request** (> 500 ms), `DbCommandInterceptor` đo **một câu SQL** (> 100 ms).

**Lọc bí mật khỏi log là mắt xích cuối của chuỗi bảo vệ token:** token đã được hash trước khi lưu DB (Buổi 2), chỉ lưu hash khi xoay vòng (Buổi 4), token đặt lại mật khẩu không đi vào kho job (Buổi 7). Chỉ cần một câu log ghi nguyên payload của `RefreshTokenCommand` là toàn bộ công sức đó vô ích — mà log thường được giữ lâu hơn và nhiều người đọc được hơn cơ sở dữ liệu. Kiểm thử với sink trong bộ nhớ biến lời hứa thành điều kiện build.

**Kịch bản load test phản ánh tỉ lệ truy cập thật:** bắn 100 % vào endpoint ghi cho con số vô nghĩa; bắn 100 % vào một URL thì mọi request đều trúng cache và p50 đẹp giả tạo. Tỉ lệ 70/20/10 mô phỏng đúng một blog — phần lớn là Guest đọc nội dung công khai, chính là đặc điểm khiến cache công khai (MT-34) mang lại giá trị. **TTL chuẩn ngắn hơn TTL cũ** (chi tiết 5 phút thay vì 60) nên hit rate khó đạt 80 % hơn; chưa đạt thì **báo cáo trung thực kèm phân tích**, không nới TTL cho đạt chỉ tiêu.

**ADR ghi lại "vì sao", code và kiểm thử chỉ ghi lại "cái gì":** người bảo trì sau này sẽ thấy `GET /recipes` không trả Draft, thấy access token không nằm trong `localStorage`, thấy lớp gọi API dùng `ky` chứ không phải `axios` — và có thể "sửa" chúng. ADR là thứ ngăn những lần sửa đó làm sống lại lỗ hổng hay phá cơ chế cache.

**4. Commit gợi ý:**
```
feat(obs,release): serilog structured logging with validated correlation id, request timing, audit and secret redaction; opentelemetry; cache audit, k6 load and resilience tests; docker compose packaging, adrs and v1.0.0
```

---

## 5. Ma trận truy vết FR → Buổi → Dev (39/39 FR — SRS v1.2.3)

> Buổi 1 (đọc đặc tả) không hiện thực FR nào; số buổi dưới đây theo cách đánh số 8 buổi.
>
> **Cách đọc cột "Buổi":**
> - Từ 29/09/2026 ghi dạng **API B{n} · UI B{m}** khi API và giao diện của một FR làm ở hai buổi khác nhau (yêu cầu "hoàn thành tất cả API endpoints" ở Buổi 4); cột "Dev" ghi chủ API / chủ giao diện khi hai người khác nhau.
> - Từ 12/10/2026 (yêu cầu Lab 5 → Lab 8, §0.3) ghi thêm **"nâng cấp B{k}"** khi một giao diện đã chạy được Lab sau giao lại để dựng lên nền Buổi 5 – 6.

| Mã FR | Tên | Buổi | Dev | Ưu tiên |
|---|---|---|---|---|
| FR-AUTH-001 / 002 | Đăng ký / Đăng nhập bằng email | 2 · nâng cấp UI **7** | 1 | M / M |
| FR-AUTH-003 / 005 | Google ID Token / Logout | 3 · nâng cấp UI **7** | 1 | **M** / M |
| FR-AUTH-004 | Refresh Rotation + Reuse Detection | **API 4 · FE 4** (D-12 + single-flight — Dev 1 làm sớm) · **Zustand store 6** | 1 | M |
| FR-AUTH-006 / 007 | Xem / Cập nhật Profile | **API 4 · UI 7** | 1 | **M** / M |
| **FR-AUTH-008** | **Quản lý tài khoản [Admin]: danh sách + khóa / mở khóa** | **API 4 · UI 8** | **1** | **S** |
| **FR-AUTH-009** | **Quản lý phiên đăng nhập của chính mình** (mới ở v1.2.0 — MT-45) | **API 4 · UI 7** | **1** | **C** |
| **FR-AUTH-010** | **Đổi mật khẩu** (mới ở v1.2.3 — CR-2026-05 / MT-64) | **API + UI 7** | **1** | **M** |
| **FR-AUTH-011** | **Quên & đặt lại mật khẩu qua email** (mới ở v1.2.3 — CR-2026-05 / MT-65) | **API + UI 7** | **1** | **M** |
| FR-CAT-001 / 002 | Danh mục công khai + Redis cache | 2 · nâng cấp UI **7** (+ D-7) | 3 | M / M |
| FR-CAT-003 / 004 / 005 | CRUD danh mục Admin | 3 | 3 | M / M / M |
| FR-RCP-001 / 002 | Danh sách / Chi tiết công thức | 2 · nâng cấp trang chi tiết **7** | 2 | M / M |
| FR-RCP-003 / 008 | Tạo Draft / Quản lý ảnh | 3 *(mảng inline `steps?`/`ingredients?` của FR-RCP-003: 4)* · **biểu mẫu RHF + tải ảnh 8** | 2 | M / M |
| FR-RCP-009 / 010 | Nguyên liệu / Các bước + sắp xếp lại | **API 4 · UI 5** · **`useFieldArray` 8** | 2 | M / M |
| FR-RCP-004 | Update RowVersion | **API 4 · UI 5** · **đồng bộ theo phần thay đổi 8** | 2 | M |
| FR-RCP-005 | Xuất bản / Hủy xuất bản (+ D-15) | **API 4 · UI 5 + 8** | **API: 4 · UI: 2** | M |
| FR-RCP-007 | Soft Delete (+ D-3) | **API 4 · UI 8** | **API: 4 · UI: 2** | M |
| **FR-RCP-011** | **`GET /recipes/mine` + `GET /recipes/mine/{id}` (cấm cache — CR-2026-04 d)** | **API 4 · UI 5 (trang sửa), 6 (tổng quan `/dashboard`), 7 (danh sách `/dashboard/recipes`)** | **API: 3 · UI: 2** | **M** |
| FR-RCP-006 | **Archive + Unarchive trọn gói**, đóng kín máy trạng thái | **API 4 · UI 8** | **API: 4 · UI: 2** | **M** |
| FR-SRCH-001 | Full-Text Search (generated column) | **API 4 · UI 5** · **bộ lọc thời gian: API 6 (MT-66) · UI 7** | **API: 3 · UI: 3 (B5), 4 (B7)** | M |
| FR-SRCH-002 / 003 / 004 | Lọc / Sắp xếp / Phân trang (+ D-10) | **API 4 · UI + index 5** · nâng cấp UI **7** | **3 · UI B7: 4** | **M / M / M** |
| FR-FILE-001 / 002 | MinIO Tải lên / Delete (service + endpoint `/files/*` — endpoint chính thức hóa ở v1.2.0, MT-43) | 2 | 4 | **M / M** |
| FR-JOB-001 | Welcome Email (job: **đã làm ở B2**; dashboard bảo vệ: B3) | **2 + 3** | 4 | S |
| FR-JOB-002 | Đổi kích thước ảnh | 4 | 4 | S |
| **FR-JOB-003** | **Dọn dữ liệu vĩnh viễn Job** (không phải Sitemap) | **4** | **4** | **S** |
| FR-OBS-001 | Health Checks (3 endpoint: **B3**; Docker `healthcheck:` + failover Nginx + UI: B5) | **3 + 5** | 4 | **M** |
| FR-OBS-002 | Structured Logging | **8** | 4 | **M** |
| FR-OBS-003 | Tracing & Metrics | **8** | 4 | **C** |

> **In đậm** = thay đổi so với kế hoạch 6 buổi (FR mới, mức ưu tiên được nâng theo MT-40, nội dung được định nghĩa lại, hoặc buổi thực hiện thay đổi).
>
> **Từ 29/09/2026, mọi FR có giao diện mà API chưa xong trước Buổi 4 đều được tách "API ở Buổi 4 · giao diện ở buổi sau"** — ngoại lệ có chủ đích với nguyên tắc "một FR không cắt đôi qua hai buổi" (§0.2), vì giảng viên yêu cầu hoàn thành toàn bộ API ở Buổi 4. Đổi lại:
> - Frontend từ Buổi 5 làm việc trên hợp đồng API đã có integration test (không còn cảnh chờ API);
> - lỗ hổng MT-34 được vá sớm hai buổi;
> - `ApiSurfaceTests` giữ bề mặt API ổn định tới Buổi 8.
>
> Mỗi phần giao ở mỗi buổi vẫn **chạy được và kiểm chứng được** (API qua Scalar + kiểm thử).
>
> **Từ 12/10/2026 (Lab 5 → Lab 8, §0.3), giao diện được phân bổ lại theo yêu cầu tối thiểu của từng Lab.** Một số giao diện đã chạy được ở Buổi 2 – 5 được Lab 7/8 giao lại. Kế hoạch ghi là "nâng cấp" — dựng lại trên nền chung của Buổi 5 – 6 (kiểu TypeScript, `ky`, shadcn/ui, Zustand) và làm phần còn thiếu — **không phải làm lại từ đầu**, và không FR nào bị bỏ trống giữa chừng: ở mọi thời điểm, phần đã merge vẫn chạy.
>
> **Ba FR trải qua hai buổi vì lý do riêng (trước 29/09):**
> - FR-JOB-001 — Buổi 2 đã làm sớm phần job (theo quy tắc "không code giả" của Buổi 2), chỉ còn lại dashboard;
> - FR-RCP-003 — hai mảng inline dùng lại đúng bất biến của FR-RCP-009/010; làm sớm sẽ phải viết validator nguyên liệu/bước hai lần;
> - FR-OBS-001 — yêu cầu "≥ 2 API/thành viên" của Buổi 3 (29/09/2026): phần endpoint kéo lên B3 và dùng được ngay (Verification §7 gọi `/health` từ B3), phần nối vào Docker/Nginx/UI giữ ở B5.
>
> **FR-RCP-006 không còn bị cắt đôi** như lộ trình ban đầu.

**Hạ tầng chung không gắn với một FR cụ thể:**
- **Buổi 3:** commit nền D-11 (Dev 4 dẫn) · Integration Test Harness (Dev 4) · **nền Domain Exceptions / Repository & Unit of Work / Global Exception Middleware** (Dev 4 phần nền; Dev 1/2/3 phần module của mình);
- **Buổi 4:** `UseForwardedHeaders` (Dev 1) · **`ApiSurfaceTests`** 45/45 → **48/48** từ B7 (Dev 4);
- **Buổi 5:** composite indexes (Dev 3) · **CORS / rate limit / `Asp.Versioning`** (Dev 1) · **OpenAPI + Scalar `/scalar/v1` + ảnh chụp hợp đồng** (Dev 2) · **cấu trúc thư mục Frontend + quy tắc import** (Dev 3) · **kiểu TypeScript theo miền + kiểm thử chống lệch mã lỗi** (Dev 4 + cả nhóm);
- **Buổi 6:** **lớp gọi API `ky` + bộ xử lý lỗi + route group** (Dev 4) · **Zustand auth store + CSP** (Dev 1) · **shadcn/ui** (Dev 2) · **component khung + mockup** (Dev 3);
- **Buổi 7:** Nginx production + `/media/` + HTTPS + compose scale (Dev 4);
- **Buổi 8:** Serilog + CorrelationId + k6 + đóng gói (Dev 4).

**Truy vết NFR:**

| Nhóm NFR | Buổi – Dev |
|---|---|
| SEC-001 / 002 (mật khẩu, JWT) | B2 Dev1 · B4 Dev1 (256-bit, hash, xoay vòng — D-2; access token trong bộ nhớ — D-12 làm sớm) · **B6 Dev1** (Zustand: chỉ refresh token lưu bền) · **B7 Dev1** (đổi/đặt lại mật khẩu thu hồi phiên — MT-64/65) |
| SEC-003 (rate limit + ForwardedHeaders) | **B4 Dev1** (`UseForwardedHeaders`) · **B5 Dev1** (rate limit — Lab 5) · kiểm chứng B8 Dev1 |
| SEC-004 (kiểm tra hợp lệ đầu vào, file tải lên) | B2 Dev4 (magic bytes) · B3 commit nền (400 thống nhất — D-11) · xuyên suốt |
| SEC-005 (HTTPS, CORS, HSTS, CSP) | B2 (CORS) · **B5 Dev1** (CORS named policy + expose header — MT-68) · **B6 Dev1** (CSP nonce — MT-67) · **B7 Dev4** (SSL, HSTS) |
| SEC-006 (phân quyền + cách ly cache) | B3 Dev2 (`RecipeAuthorizationHandler`) · **B4 Dev3** (MT-34 — D-4, D-5; `/recipes/mine`, `/recipes/mine/{id}` cấm cache) · **B7 Dev2** (cách ly cache phía client) · B8 Dev1 (kiểm thử phân quyền) |
| SEC-007 (secrets, không log bí mật) | B2 Dev4 (gitleaks, User Secrets) · B3 Dev4 (htpasswd/gate secret) · **B7 Dev1** (token đặt lại mật khẩu không vào kho job) · B7 Dev4 (DataProtection — D-17) · **B8 Dev4** (redaction + kiểm thử) |
| PERF-001 / 002 (response time, throughput) | **B8 Dev4** (k6) |
| PERF-003 (cache TTL + hit rate) | B3 Dev3 (D-13) · B4 Dev3 (D-6) · B5 Dev3 + **B7 Dev3** (D-7) · **B8 Dev4** (audit + hit rate) |
| PERF-004 (index, EXPLAIN ANALYZE) | **B5 Dev3** (composite index + đo trên ≥ 10.000 bản ghi) · B8 Dev1 (đo tìm kiếm người dùng) |
| PERF-005 (Core Web Vitals) | B7 Dev4 + **B8 Dev3** (bật lại tối ưu ảnh — D-14) · **B8 Dev3** (Lighthouse) |
| USE-001 → 004 (responsive, a11y, lỗi, loading) | B2 Dev4 (progress bar) · B3 (lỗi từng ô sau D-11) · **B6** (shadcn/Radix, token tương phản, skeleton, error handler) · **B8 Dev3** (WCAG) |
| REL-001 (uptime, healthcheck) | B2 (healthcheck postgres/redis/minio) · **B3 Dev4** (health endpoints) · **B5 Dev4** (healthcheck api/frontend + `proxy_next_upstream`) |
| REL-002 (khả năng chịu lỗi, phương án dự phòng) | B2 Dev3 (Redis phương án dự phòng) · **B3 Dev4** (Global Exception Middleware — 500 không lộ stack trace; dịch lỗi ghi DB) · **B8 Dev4** (kiểm chứng có tải) |
| REL-003 (độ bền dữ liệu, xóa mềm 30 ngày) | **B4 Dev4** (xóa mềm + partial unique D-3 + purge job — cùng một người, cùng một buổi) · B8 Dev2 (giao diện xóa) |
| MAINT-001 → 004 (chất lượng, kiểm thử, docs, Clean Arch) | xuyên suốt · **B3 Dev4** (bộ integration test; kiểm thử kiến trúc exception/repository) · **B4 Dev4** (`ApiSurfaceTests`) · **B5 Dev2** (tài liệu OpenAPI sinh từ code + kiểm thử hợp đồng) · **B5 Dev3** (quy tắc import Frontend) · **B8** (E2E, coverage, 10 ADR, CHANGELOG) |
| SCALE-001 → 003 (stateless, DB, hạ tầng) | B2 (worker Hangfire riêng — SRS v1.2.0 MT-47) · B4 Dev4 (khóa phân tán) · **B7 Dev4** (replicas, resolver động) |
| SEO-001 → 004 (JSON-LD, meta, sitemap, URL) | B3 Dev2 (slug dành riêng) · **B8 Dev3** (JSON-LD, OG, sitemap, robots) |

---

## 6. Phụ thuộc liên dev (điểm bắt buộc phối hợp)

| Buổi | Phụ thuộc | Cách xử lý |
|---|---|---|
| **B3** | ⚠️ **Mọi dev** phụ thuộc commit nền D-11 (422 → 400) | ✅ Đã xong trong commit `67d29c0` của Dev 4 |
| **B3** | ⚠️ **Dev 1, 2, 3** phụ thuộc **commit nền kiến trúc** của Dev 4 (lớp gốc `DomainException`, `IRepository<T>`/`IUnitOfWork`, `ExceptionStatusMap`) | Dev 4 merge vào `develop` ở **mốc M1 (chậm nhất giữa buổi)**; trước M1 ba dev làm phần không phụ thuộc (D-1 + FE Google; D-8 + validator + biểu mẫu nhiều bước; validator + trang quản trị + D-13), sau M1 rebase rồi viết exception/repository/mapping của module |
| **B3** | Hai file dùng chung bị nhiều dev sửa: `IUnitOfWork.cs`, `UnitOfWork.cs` (`ErrorCodes.cs` đã có đủ 29 mã từ commit nền) | **Chỉ thêm dòng** vào khu vực của module mình (property `Users`/`Categories`/`Recipes`). Mapping HTTP và bộ dịch lỗi mỗi module một file, quét tự động — không ai sửa file DI hay middleware của người khác |
| **B3** | Dev 2 kiểm tra `categoryId` tồn tại — dữ liệu thuộc module của Dev 3 | Dùng `IRepository<Category>.ExistsAsync` (generic, của Dev 4) — **không** chờ `ICategoryRepository` của Dev 3 |
| **B3** | Hai mã lỗi mới (`AUTH_USER_NOT_FOUND`, `CONCURRENCY_CONFLICT`) | ✅ Đã áp dụng trong SRS v1.2.2 (CR-2026-04, §4.5) |
| **B3** | Mọi dev viết integration test trên bộ integration test của **Dev 4** | Dev 4 push bộ integration test (`CulinaryBlogApiFactory`, `CreateClientAs`) **trước giữa buổi**; trước đó các dev viết kiểm thử theo mẫu đã thống nhất rồi chạy khi bộ integration test vào |
| **B3** | Dev 1 đổi `UserDto` (`fullName` → `displayName`, bỏ `userName` — D-1) | Mọi component FE đọc `user.fullName` phải đổi; Dev 1 merge trước Dev 2/Dev 3 theo thứ tự chuẩn, các dev sau rebase |
| **B3** | Dev 2 cần `SlugHelper`, `IFileStorageService`, `ImageFileInspector` | Đã có sẵn từ B2 (`Domain/Common`, `Application/Common`) — chỉ inject |
| **B4** | Dev 4 móc `ImageResizeJob` vào `UploadRecipeImageCommand` của Dev 2 (B3) | Dev 2 để sẵn điểm `BackgroundJob.Enqueue`; Dev 4 chỉ cắm job vào |
| **B4** | Dev 4 (`B4_Recipe_PartialUniqueSlug`), Dev 3 (`B4_Search_FTS`) và Dev 2 (`B4_Recipe_IngredientStep`) **cùng tạo migration trên bảng `Recipes`** | Theo thứ tự merge chuẩn: Dev 4 → Dev 3 → Dev 2; dev sau rebase rồi chạy lại `dotnet ef migrations add` |
| **B4** | ⚠️ Ba dev cùng sửa module Recipe (Dev 2 nội dung, Dev 3 truy vấn, Dev 4 vòng đời) | Dev 4 (merge đầu) đổi `Recipe` thành `partial class` và tách endpoint thành 3 file (`RecipesEndpoints`, `RecipeQueryEndpoints`, `RecipeLifecycleEndpoints`) **ngay đầu buổi**, đẩy lên `develop` trước; Dev 2, Dev 3 rebase rồi mỗi người chỉ sửa file của mình |
| **B4** | Dev 3 (`GET /recipes/mine/{id}`) và Dev 2 (`PUT /recipes/{id}`) dùng chung `rowVersion`/`ETag` | Hợp đồng đã chốt trong SRS v1.2.2 §8.3: `rowVersion` là base64 của cột `RowVersion`, `ETag` = `"{rowVersion}"`; `PUT` nhận cả body lẫn `If-Match` |
| **B4** | Dev 4 xóa cache `recipes:sitemap` của Dev 3 | Khóa cache đặt tên trong `CacheKeys` dùng chung (Dev 3 tạo), Dev 4 chỉ tham chiếu |
| **B4** | Dev 1 bật `UseForwardedHeaders` — cần Nginx gửi `X-Forwarded-For` | **Đã có từ B2** (`nginx.conf` dòng 52) — không phụ thuộc ai |
| **B4** | Dev 1 thêm claim `sid` vào `AuthResponseFactory` — mọi luồng phát token (đăng ký, đăng nhập, Google, refresh) đều đi qua đây | Chỉ Dev 1 sửa file này trong B4; các dev khác không phụ thuộc vào `sid` |
| **B5** | *(Phần đã làm trước 12/10)* Giao diện của Dev 2 (biểu mẫu nhiều bước, trang sửa, nút xuất bản) dùng API của Dev 2, Dev 3, Dev 4 (Buổi 4) | Hợp đồng là SRS Chương 8 + integration test của chủ API; lệch hợp đồng thì chủ API sửa, Dev 2 không sửa handler của người khác |
| **B5** | Commit nền: tách `types/api.ts` thành 5 file theo miền | **Dev 4** merge trong 30 phút đầu; Dev 1/2/3 rebase rồi mới sửa file kiểu của mình — mỗi file một chủ |
| **B5** | Dev 3 chuyển `components/ui/{FormField,ImageUploader,Pagination,RecipeGridSkeleton}` sang `components/shared/` — đổi import trong code của Dev 1, 2, 4 | Theo thứ tự merge Dev 4 → Dev 1 → Dev 3 → Dev 2: Dev 3 sửa import của những nhánh đã merge trước; **Dev 2** (merge cuối) sửa import trong `RecipeImageGallery`/trang soạn công thức sau khi rebase |
| **B5** | Dev 1 đổi route sang `/api/v{version:apiVersion}` → `ApiSurfaceTests` (file của Dev 4) cần bước chuẩn hóa | Dev 1 thêm bước chuẩn hóa trong PR của mình, **Dev 4 rà soát**; kiểm thử vẫn khóa 45/45 |
| **B5** | Rate limit làm bộ integration test đỏ ngẫu nhiên (hàng trăm lần gọi `/auth/login` từ cùng "IP") | `CulinaryBlogApiFactory` (Dev 4) đặt `RateLimiting:Enabled=false`; riêng `RateLimitingTests` của Dev 1 dùng factory con hạn mức nhỏ |
| **B5** | Dev 2 gắn metadata OpenAPI cho **mọi** endpoint (file endpoint của cả 4 dev) + thêm `location /scalar/`, `/openapi/` vào Nginx (file của Dev 4) | Dev 2 merge **cuối**, rebase lên code Buổi 5 của cả nhóm; mỗi chủ module duyệt phần endpoint của mình trong PR |
| **B5** | Kiểm thử hợp đồng kiểu TypeScript của Dev 2/3/4 đọc `docs/api/openapi.v1.json` mà Dev 2 sinh | Dev 2 đẩy ảnh chụp lên `develop` sớm (một commit riêng chỉ chứa file JSON); trước đó kiểm thử hợp đồng đánh dấu `skip` khi chưa có file |
| **B6** | ⚠️ Hai commit nền: route group (`git mv` — Dev 4) và khởi tạo shadcn/ui (`components.json`, `cn`, token màu — Dev 2) | Cả hai merge trong 30 – 45 phút đầu, **trước** khi Dev 3 dựng component khung (cần `Button`, `Card`, `Badge`) |
| **B6** | `(dashboard)/layout.tsx` (Dev 4) cần `AuthGuard`; `providers.tsx` cần `AuthStoreProvider` (Dev 1) | Dev 4 viết layout dựa trên `useAuth()` — chữ ký giữ nguyên trước và sau khi Dev 1 đổi sang Zustand; Dev 1 (merge ngay sau Dev 4) chỉ thay phần bên trong `useAuth()` và bọc `AuthGuard` (một dòng trong file của Dev 4) |
| **B6** | CSP nonce (Dev 1) cần root layout (Dev 4) đọc `headers()` — làm **mọi** trang render theo request (MT-67) | Dev 1 sửa một dòng trong `app/layout.tsx` của Dev 4 khi merge, Dev 4 rà soát; Dev 2/3/4 không khai báo `export const revalidate` ở trang nào, chỉ đặt `revalidate` trên `fetch` |
| **B6** | `(public)/layout.tsx` (Dev 4) dùng `Header`/`Footer` của Dev 3 | Dev 4 dùng tạm `SiteHeader` cũ; Dev 3 thay một dòng import khi merge |
| **B6** | `lib/error-messages.ts` (Dev 4) phải có thông báo cho **mọi** `ErrorCode` | `satisfies Record<ErrorCode, string>` → thiếu là lỗi biên dịch; Dev 1 thêm thông báo cho mã mới của CR-2026-05 ở Buổi 7 |
| **B6** | Dev 3 mở rộng `GET /recipes/search` (MT-66) — cache `search:{queryHash}` thuộc cơ chế chung | Kiểm thử của Dev 3 khẳng định khóa cache khác nhau khi bộ lọc khác nhau; Dev 4 rà soát lại trong lượt audit cache Buổi 8 |
| **B7** | Dev 1 thêm mã `AUTH_PASSWORD_RESET_TOKEN_INVALID` vào `ErrorCodes.cs` (file của Dev 4), gỡ nó khỏi `PendingSrsCodes` của `DomainExceptionRulesTests` và thêm 3 dòng vào `ApiSurfaceTests` (45 → 48) | Append-only, Dev 4 rà soát; cùng PR cập nhật `ERROR_CODES`/`ERROR_MESSAGES` phía Frontend và ảnh chụp `openapi.v1.json` — kiểm thử chống lệch bắt buộc |
| **B7** | `PasswordResetEmailJob` (Dev 1) chạy ở container `hangfire` cần cùng khóa DataProtection với `api` | Đã có volume `dpkeys` dùng chung từ Buổi 2; D-17 (Dev 4, cùng buổi) bảo vệ khóa bằng chứng chỉ — Dev 1 kiểm lại luồng đặt lại mật khẩu sau khi Dev 4 merge |
| **B7** | Giao diện `/search` (Dev 4) dựa trên API của Dev 3 (Buổi 4 + MT-66 ở Buổi 6) | Hợp đồng là SRS v1.2.3 §8.3 + integration test của Dev 3; Dev 4 không sửa handler |
| **B7** | Trang chủ và trang danh mục (Dev 3) dùng `RecipeCard`, `SidebarCategory`; trang chi tiết (Dev 2) dùng `ImageUploader`/ảnh qua `/media/` | Ảnh vẫn chạy `unoptimized` tới khi Dev 4 dựng `/media/` (cùng buổi); gỡ `unoptimized` để sang Buổi 8 — Dev 3 |
| **B8** | ⚠️ Cả 4 dev chạy E2E trên cùng một môi trường giống production | Dev 4 dựng **đầu Buổi 8** (`docker-compose.prod.yml`, HTTPS tự ký, dữ liệu mẫu CR-2026-03); E2E chạy với `Jwt__AccessTokenMinutes=1` để kiểm refresh |
| **B8** | Dev 3 thêm `<JsonLd …/>` vào `(public)/recipes/[slug]/page.tsx` (file của Dev 2) | Một dòng, Dev 2 rà soát |
| **B8** | Dev 2 đọc `RecipeStatusTransitions.cs` (Dev 4, Buổi 4) để kiểm thử chống lệch bảng chuyển trạng thái | Chỉ đọc; đổi luật ở Backend phải sửa cả bảng Frontend trong cùng PR |
| **B8** | Load test (Dev 4) cần nâng hạn mức rate limit; kiểm thử bảo mật (Dev 1) cần hạn mức thật | Hai cấu hình môi trường riêng: profile đo tải đặt `RateLimiting__*__PermitLimit` cao; môi trường E2E/bảo mật dùng giá trị mặc định của SRS |
| **B8** | Kiểm tra "log không chứa bí mật" (Dev 1) cần Serilog đã bật (Dev 4) | Dev 4 merge trước theo thứ tự chuẩn; Dev 1 chạy `grep` log sau khi rebase |

**Thứ tự merge migration (giữ nguyên quy ước B2):** Dev4 → Dev1 → Dev3 → Dev2. Dev sau `rebase` + chạy lại `dotnet ef migrations add` nếu xung đột snapshot.

---

## 7. Verification (cuối mỗi buổi & cuối dự án)

**Cuối mỗi buổi — Definition of Done:**
1. `docker compose up -d --build` → tất cả container `healthy`; `curl http://localhost/health` (qua Nginx) trả `Healthy` (endpoint có từ B3).
2. `dotnet build && dotnet test` xanh — build đã bật `TreatWarningsAsErrors`; kiểm thử gồm unit + **integration (Testcontainers, từ B3)** + **ArchitectureTests** (Domain không reference assembly ngoài nào, Application không reference Infrastructure; từ B3: mọi `DomainException` cụ thể có ánh xạ HTTP, interface repository không trả `IQueryable`, `ErrorCodes` khớp Phụ lục B).
3. `cd frontend && npm run lint && npm run build && npm test` xanh.
4. Test tay các endpoint của buổi qua Scalar (`/scalar`, từ B5 là `/scalar/v1`) + UI tương ứng. **Từ B4:** `ApiSurfaceTests` xanh — route thật khớp đúng danh sách SRS Chương 8 (45 endpoint ở v1.2.2; **48** từ B7 theo v1.2.3). **Từ B5:** `OpenApiDocumentTests` và ảnh chụp `docs/api/openapi.v1.json` xanh.
5. **Từ B5 (Frontend):** `npm run lint` không có import sai chiều thư mục; kiểm thử hợp đồng kiểu TypeScript và `error-codes.test.ts` xanh. **Từ B8:** log trên Seq có đủ `CorrelationId`, `RequestPath`, `UserId` và **không có** bí mật nào (`LogRedactionTests`).
6. Retrofit của buổi (§4.1) đã hoàn trả **và** có kiểm thử chứng minh; hạng mục phụ thuộc CR chỉ được bật khi CR đã duyệt.

**Kiểm tra riêng theo buổi:**

| Buổi | Kiểm chứng đặc thù |
|---|---|
| B1 | Bảng đầu việc (README Phần 1) đủ 37 FR / 30 NFR / 10 CONS / 44 endpoint; mỗi thành viên trình bày được module của mình; danh sách mâu thuẫn đã ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` |
| B2 | Đăng ký/đăng nhập, xem công thức, xem danh mục, tải lên/xóa ảnh chạy qua Nginx; **CR-2026-03:** `SELECT` trên DB cho ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước; `RecipeSeedCatalogTests` xanh; khởi động lại API log *"0 recipes created, 0 recipes repaired"* |
| B3 | `grep -rE "Status422|status === 422" backend/src frontend/src` không còn kết quả (D-11); form đăng ký hiện lỗi từng ô với 400; đăng ký 2 email cùng tiền tố → `UserName` có hậu tố số; `/hangfire` qua Nginx hỏi Basic Auth, gọi thẳng `:5000/hangfire` → 401; `dotnet test` chạy integration test trên Testcontainers |
| B4 | Xóa bước giữa và kéo-thả đổi chỗ → `StepNumber` liên tục 1..N, **không có lỗi 23505** (D-16); `to_tsquery('simple','pho:*')` tìm được "Phở bò" **trên Testcontainers** (DDL nằm đủ trong migration — D-18); `localStorage` không còn chứa access token (D-12); `CreatedByIp` là IP thật |
| B5 | **Lab 5:** request đăng nhập thứ 11/phút từ cùng IP → **429** + `Retry-After` + `type = RATE_LIMIT_EXCEEDED`, IP khác vẫn 200; preflight từ `http://localhost:3000` có `Access-Control-Expose-Headers: ETag…` và cho phép `If-Match`; `/api/v1/categories` trả `api-supported-versions: 1.0`, `/api/v2/categories` → 400 `VALIDATION_ERROR`; `http://localhost/scalar/v1` hiển thị đủ 45 endpoint có summary/tag, nút *Authorize* dùng được; `OpenApiDocumentTests` + ảnh chụp `docs/api/openapi.v1.json` xanh; `npm run lint` chặn import sai chiều thư mục; `error-codes.test.ts` (Frontend ↔ `ErrorCodes.cs`) xanh; `package.json` có `zustand`, `ky`, không còn `next-auth`. **Phần đã làm trước 12/10:** `EXPLAIN ANALYZE` trên ≥ 10.000 bản ghi → Index Scan; `?sort=-title` → 400, `?sortBy=title&sortOrder=desc` → 200 (D-10); xuất bản một recipe Archived → 409 (D-15); `docker compose stop redis` → `/health/ready` 503, `/health/live` 200 |
| B6 | **Lab 6:** `localStorage` chỉ chứa `refreshToken` + `user` (không có access token), người dùng đăng nhập từ trước khi cập nhật **không bị đăng xuất**; 5 request song song gặp 401 → đúng 1 lần `/auth/refresh`; mọi request có `X-Correlation-ID`; `POST` không bao giờ tự thử lại; mở `/dashboard` khi chưa đăng nhập → về `/auth/login?callbackUrl=%2Fdashboard`, `callbackUrl=//evil.com` không chuyển ra ngoài; `frontend/e2e/csp.spec.ts`: 0 vi phạm CSP và trang hydrate trên mọi route (kể cả nút Google), hai lần tải có nonce khác nhau; production không có `'unsafe-inline'`/`'unsafe-eval'` trong `script-src`; tải trang chủ nhiều lần trong 2 phút không sinh request mới tới API (log API) — Data Cache giữ đúng TTL (MT-67); 8 component shadcn Lab 6 có trong `components/ui/`, `Dialog` đóng bằng `Escape` và trả focus; token `--primary` đạt tương phản ≥ 4,5 : 1; mockup `/mockups/*` chạy ở dev và trả 404 ở bản build production; `q=pho&maxCookTime=30` chỉ ra món nấu ≤ 30 phút (MT-66) |
| B7 | **Lab 7:** `ApiSurfaceTests` **48/48**; đổi mật khẩu → phiên khác bị thu hồi, phiên hiện tại vẫn refresh được; quên mật khẩu → email trong MailHog → đặt lại → đăng nhập được, dùng lại link → 400 `AUTH_PASSWORD_RESET_TOKEN_INVALID`; tham số job Hangfire **không chứa** token; trang `reset-password` không còn token trên thanh địa chỉ; `/profile` đổi ảnh đại diện → header đổi ngay, F5 vẫn giữ; tab phiên đánh dấu đúng "Thiết bị này"; `curl -I /recipes/<slug-Draft>` → **404**; điều chỉnh khẩu phần nhân đúng `quantity`, giữ nguyên `quantityText`; `grep -rn "revalidate: \(3600\|600\)" frontend/src` rỗng (D-7); `/search` đổi bộ lọc không tải lại trang (tab Network chỉ có request JSON); `http://` chuyển sang `https://` kèm HSTS; ảnh tải qua `/media/` (D-14); `docker compose -f docker-compose.prod.yml up --scale api=3` chạy được và tạo lại container `api` **không gây 502**; khóa DataProtection được bảo vệ bằng chứng chỉ (D-17) |
| B8 | **Lab 8:** Author gọi `GET /users` → 403; Admin tìm, lọc và khóa người dùng (lý do bắt buộc) → phiên của họ bị thu hồi; tạo công thức qua biểu mẫu nhiều bước bằng **một** `POST` có mảng nội tuyến; hai tab sửa cùng lúc → dialog 409; chỉ hiện nút chuyển trạng thái hợp lệ, khôi phục về Draft; Google Rich Results Test đạt; `/sitemap.xml` không có slug Draft/Archived; axe 0 vi phạm serious/critical; Lighthouse đạt CWV; log Seq có đủ `CorrelationId`, `RequestPath`, `UserId`, log của job nối được với request; `LogRedactionTests` xanh; k6 đạt p50 ≤ 150 ms / p95 ≤ 500 ms / p99 ≤ 1000 ms (hoặc báo cáo trung thực nếu chưa đạt); `npx playwright test` xanh; 8 service `healthy`; tag `v1.0.0` |

**Danh sách kiểm tra nghiệm thu cuối dự án:**
- [ ] **39/39 FR** (SRS v1.2.3) hoàn thành và truy vết được theo ma trận §5.
- [ ] **48/48 endpoint** của SRS v1.2.3 Chương 8 hoạt động, có trong Scalar `/scalar/v1` và được `ApiSurfaceTests` khóa.
- [ ] **Yêu cầu tối thiểu Lab 5 → Lab 8** của 4 dev đều có bằng chứng trong báo cáo Lab tương ứng (kiểm thử, ảnh chụp, log).
- [ ] **17/17 hạng mục nợ kỹ thuật cần sửa code** (§4.1) đã hoàn trả — đặc biệt **D-4, D-5 (lỗi bảo mật MT-34)** và **D-11 (422 → 400)**.
- [ ] **30/30 mã lỗi** ở SRS v1.2.3 Phụ lục B đều có đường sinh ra và có kiểm thử tương ứng; `ERROR_CODES` (Frontend) khớp `ErrorCodes.cs`; mã `AUTH_USERNAME_EXISTS` đã bị xóa khỏi code.
- [ ] Dữ liệu mẫu đạt SRS v1.2.1 §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
- [ ] **0 vị trí** dùng mã **422** trong toàn bộ code và kiểm thử.
- [ ] Bảng TTL cache trong code khớp **100%** với SRS NFR-PERF-003, và `revalidate` của mọi `fetch` phía Frontend ≤ TTL tương ứng (kiểm thử Buổi 7).
- [ ] Cấu hình Nginx và lệnh healthcheck trong repo khớp SRS §6.5 (resolver động, không khối `upstream`; không lệnh healthcheck nào gọi công cụ không có trong image).
- [ ] **10 ADR** đã viết (MT-05, MT-08/09, MT-16, MT-34, MT-47, MT-50 + phân vai state Frontend, `ky`, versioning & rate limit, CSP nonce ↔ render — MT-67).
- [ ] Tag `v1.0.0` (phiên bản phần mềm), `CHANGELOG.md` cập nhật, `README.md` setup được trong < 5 phút.
- [ ] Nếu trong quá trình làm phát hiện thêm điểm SRS sai hoặc thiếu: ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` (**MT-69** trở đi — MT-64 → MT-68 đã dùng cho CR-2026-05) và đi qua Change Request — **không** tự làm khác SRS trong code.

---

*— Hết kế hoạch —*

> **Nguồn chuẩn:** mọi yêu cầu trong tài liệu này bám theo `SPEC/SRS_Culinary_Blog_v1.2.3.md` (12/10/2026) — gồm CR-2026 (41 quyết định, SRS Phụ lục D), CR-2026-02 (16 quyết định, SRS Phụ lục E), CR-2026-03 (yêu cầu dữ liệu mẫu, SRS §2.6.1), CR-2026-04 (5 quyết định, SRS Phụ lục G) và CR-2026-05 (5 quyết định: 3 chức năng cho yêu cầu Lab 7 + 2 errata CSP/CORS, SRS Phụ lục H). Hiện trạng code được đối chiếu trực tiếp với các nhánh `buoiso5` của 4 dev (12/10/2026), nhánh `main` và các báo cáo buổi. Mọi xung đột và mâu thuẫn — trong SRS, giữa SRS và code, giữa SRS và lộ trình/yêu cầu Lab được giao — đều được ghi đầy đủ tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md`. Kế hoạch **không có điểm nào làm khác SRS v1.2.3**; khi phát hiện khác biệt, **SRS là nguồn đúng** và mọi thay đổi SRS phải qua Change Request.
