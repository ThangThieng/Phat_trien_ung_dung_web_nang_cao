# KẾ HOẠCH PHÁT TRIỂN CHI TIẾT 8 BUỔI LÀM VIỆC – CULINARY BLOG
## (FEATURE-DRIVEN & ARCHITECTURAL RATIONALE)

> **Nguồn chuẩn yêu cầu:** `SPEC/SRS_Culinary_Blog_v1.2.2.md` (v1.2.2, áp dụng 29/09/2026) = v1.1.0 (CR-2026, MT-01 → MT-41) + **CR-2026-02** (MT-42 → MT-57, v1.2.0) + **CR-2026-03** (yêu cầu dữ liệu mẫu, v1.2.1) + **CR-2026-04** (MT-59 → MT-63: 2 mã lỗi, 1 endpoint, 2 làm rõ, errata E-9 — v1.2.2). Các câu "SRS v1.1.0 (MT-xx) chốt…" hay "SRS v1.2.0…" trong tài liệu chỉ thời điểm quyết định được đưa ra; mọi quyết định đó vẫn còn nguyên hiệu lực trong v1.2.2.
> **Hồ sơ mâu thuẫn:** `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` — 57 mục MT, cùng các bảng xung đột code ↔ SRS (§4), lộ trình ↔ SRS (§5) và đánh đổi còn mở (§6).
> **Nguồn chuẩn hiện trạng:** code trên nhánh `main` (commit `f313e95`) + `SPEC/BAO_CAO_BUOI_2.md` — mọi hạng mục nợ kỹ thuật ở §4 đều đã được **kiểm chứng trực tiếp trong code**, kèm đường dẫn file.
> **File:** `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`
> **Thay thế:** `SPEC/KE_HOACH_PHAT_TRIEN_7_BUOI.md` (cùng nội dung, trước khi đánh số lại 8 buổi — xem §0.1) và `SPEC/KE_HOACH_PHAT_TRIEN_6_BUOI.md` (lập theo SRS v1.0.0 — đã lỗi thời); cả hai bản gốc lưu trong lịch sử git.

---

## 0. Bối cảnh và lý do mở rộng 6 → 7 → 8 buổi

Repo `d:\ptudwnc_nhom20` đã hoàn thành **Buổi 1** (đọc đặc tả) và **Buổi 2** (nền móng & lát cắt đầu tiên, đã merge vào `main`). Nhóm có **4 dev full-stack**.

### 0.1 Đánh số lại 7 → 8 buổi (21/09/2026)

Giảng viên quy định lại **Buổi 1** là buổi **đọc đặc tả yêu cầu hệ thống và tìm hiểu các tính năng cần xây dựng**. Vì vậy toàn bộ các buổi của kế hoạch 7 buổi được **đôn lên một số**: Buổi 1 cũ → **Buổi 2**, …, Buổi 7 cũ → **Buổi 8**. **Nội dung công việc của từng buổi giữ nguyên**, chỉ đổi số thứ tự; riêng Buổi 2 được bổ sung **yêu cầu thứ 5 — dữ liệu mẫu** (xem mục Buổi 2 và **CR-2026-03** ở §4.4).

| Hạng mục | Quy ước sau khi đánh số lại |
|---|---|
| Số buổi trong tài liệu | "Buổi n" / "B{n}" luôn theo cách đánh số **8 buổi**. Các câu nhắc tới *kế hoạch 6 buổi cũ* ghi rõ là số của kế hoạch đó |
| Tài liệu đổi tên | `KE_HOACH_PHAT_TRIEN_7_BUOI.md` → `KE_HOACH_PHAT_TRIEN_8_BUOI.md`; `BAO_CAO_BUOI_1.md` → `BAO_CAO_BUOI_2.md`; `SRS_Culinary_Blog_v1.2.0.md` → `SRS_Culinary_Blog_v1.2.1.md` → `SRS_Culinary_Blog_v1.2.2.md` (CR-2026-04, 29/09/2026) |
| Migration đã tạo | `B1_InitialSchema` **giữ nguyên tên** — đã áp dụng vào mọi database, đổi tên sẽ làm EF chạy lại migration. Migration **mới** đặt tên theo số buổi mới (`B3_…` → `B8_…`) |
| Nhánh Git đã có | Nhánh lịch sử `feature/b1-*` của 3 dev đã merge và xóa trên GitHub, giữ nguyên trong lịch sử commit. Nhánh của Dev 4 (`feature/b1-dev4-infra-file` + `fix/b1-runtime-hardening`) gộp thành **`2312755_NguyenThangThieng_buoiso2`** theo quy ước mới (§2.4) |
| Ngày tháng | Không đổi — Buổi 2 vẫn là 11/09 – 14/09/2026 (rà soát 20/09, bổ sung dữ liệu mẫu 21/09) |

### 0.2 Lý do mở rộng 6 → 7 buổi (giữ nguyên, đã đánh số theo 8 buổi)

Kế hoạch cũ được lập trên SRS **v1.0.0** — tài liệu chứa **41 điểm mâu thuẫn** đã được xử lý trong CR-2026. Vì vậy kế hoạch cũ không còn dùng được nguyên trạng: mục §1.3 của nó ("Giải quyết điểm mâu thuẫn — quyết định Tech Lead") đưa ra **7 quyết định mà 5 trong số đó nay đã bị SRS v1.1.0 đảo ngược** (422 cho validation, hard delete recipe, 512-bit, lọc Draft theo danh tính, Output Cache).

**Vì sao cần thêm một buổi kỹ thuật (nay là Buổi 8):**

| Lý do | Chi tiết |
|---|---|
| SRS phát sinh thêm **3 FR mới** | `FR-AUTH-008` (Quản lý tài khoản [Admin]) và `FR-RCP-011` (`GET /recipes/mine`) ở v1.1.0; `FR-AUTH-009` (Quản lý phiên đăng nhập) ở v1.2.0 — tổng FR tăng từ 34 → **37** |
| Phát sinh thêm **11 endpoint mới** | v1.1.0: `/recipes/mine`, `/recipes/{id}/unarchive`, `/recipes/{id}/steps/reorder`, `/users/{id}/status`; v1.2.0: `GET /users`, 3 endpoint `/auth/sessions`, `GET /recipes/sitemap`, `POST /files/upload`, `DELETE /files/{**key}` — Chương 8 nay có **44 endpoint** |
| Phát sinh **nợ kỹ thuật phải hoàn trả** | Buổi 2 được code theo v1.0.0 nên có **18 hạng mục** lệch chuẩn v1.1.0 (17 cần sửa code, 1 xử lý qua Change Request), đã kiểm chứng trong code (xem §4.1) — trong đó 2 hạng mục là **lỗi bảo mật MT-34** và 1 hạng mục (**422 → 400**) ảnh hưởng mọi endpoint |
| Buổi 6 cũ **quá tải** | Buổi 6 cũ gộp cả E2E testing + WCAG + Nginx + Docker multi-stage + k6 load test vào một buổi — không thể hoàn thành với chất lượng nghiệm thu được |
| Nguyên tắc "mỗi buổi một sản phẩm chạy được" | Tách buổi cuối thành **Buổi 7 (hoàn thiện & tối ưu)** + **Buổi 8 (kiểm thử toàn diện & đóng gói)** để mỗi buổi đều có `git commit` trọn vẹn |

**Buổi 2 thực tế đã làm nhiều hơn kế hoạch** (theo `BAO_CAO_BUOI_2.md` §4.6 và code): `FR-JOB-001` Welcome Email đã chạy thật qua Hangfire + MailKit (retry 1′/5′/30′), Hangfire chạy ở **container worker riêng**, healthcheck Postgres/Redis/MinIO đã có, Nginx đã chuyển tiếp `X-Forwarded-For` và dùng **resolver DNS động**. Kế hoạch này **không giao làm lại** những việc đã xong; phần việc trống ra ở Buổi 3 của Dev 4 được dùng để dựng **integration test harness** — thứ mà mọi dev cần từ Buổi 3 trở đi — và (từ yêu cầu bổ sung 29/09/2026) **phần nền exception / repository & Unit of Work / middleware** cùng ba endpoint health check.

**Nguyên tắc xuyên suốt:** *mỗi buổi – mỗi dev – một lát cắt dọc hoàn chỉnh (DB → API → UI) – một commit*. Cuối mỗi buổi hệ thống phải chạy được, không gãy build. **Một FR không bị cắt đôi qua hai buổi** trừ khi có lý do kiến trúc được ghi rõ.

---

## 1. Phân công Full-Stack cố định (không đổi suốt 8 buổi)

| Dev | Module phụ trách | Phạm vi FR | Trọng tâm kiến trúc |
|---|---|---|---|
| **DEV 1** | Module 1 — Xác thực, Phân quyền & Quản lý Người dùng | `FR-AUTH-001` → `FR-AUTH-009` | JWT, Refresh Token Rotation, Security Hardening, `UseForwardedHeaders` |
| **DEV 2** | Module 3 — Quản lý Công thức Nấu ăn Lõi | `FR-RCP-001` → `FR-RCP-011` | Optimistic Concurrency, Máy trạng thái RecipeStatus, Soft Delete, Cách ly Public/Private |
| **DEV 3** | Module 2 & 4 — Danh mục, Tìm kiếm FTS & SEO | `FR-CAT-001`→`005`, `FR-SRCH-001`→`004` | PostgreSQL FTS (generated column), ISR, JSON-LD, Sitemap Next.js |
| **DEV 4** | Module 5, 6, 7 & Hạ tầng | `FR-FILE`, `FR-JOB`, `FR-OBS`, DevOps | MinIO/S3, Hangfire, Serilog/OTEL, Docker, Nginx, Redis |

> **Ghi chú phạm vi:** Đề bài giao DEV 1 phạm vi "FR-AUTH-001 đến 007" và DEV 2 "FR-RCP-001 đến 010". SRS bổ sung `FR-AUTH-008`, `FR-AUTH-009` và `FR-RCP-011`; các FR này thuộc đúng module của DEV 1 và DEV 2 nên được gán vào đó — ranh giới module giữ nguyên, chỉ mở rộng đuôi số.
>
> **Ngoại lệ duy nhất — Buổi 4 "hoàn thành tất cả API endpoints" (29/09/2026):**
> - Để cân bằng tải khi 25 endpoint dồn vào một buổi, **API** của FR-RCP-005/006/007 (vòng đời công thức) do **Dev 4** làm.
> - **API** của FR-RCP-011 (`/recipes/mine`, `/recipes/mine/{id}`) và `GET /recipes/sitemap` do **Dev 3** làm.
> - **Giao diện** của các FR này vẫn do Dev 2 làm (Dev 3 với sitemap), đúng module ở bảng trên.
>
> Chi tiết và lý do ở mục Buổi 4.

---

## 2. Quyết định kiến trúc chung (cả nhóm tuân thủ — theo SRS v1.2.2)

### 2.1 Cấu trúc repository (monorepo — giữ nguyên từ Buổi 2)

```
ptudwnc_nhom20/
├─ SPEC/                          # SRS v1.2.0 (+ bản gốc v1.0.0 PDF để đối chiếu), hồ sơ mâu thuẫn, kế hoạch này
├─ backend/
│  ├─ CulinaryBlog.sln
│  ├─ src/CulinaryBlog.Domain/          # Entities, VO (Slug, EmailAddress), Enums, Exceptions/{Auth,Recipes,Categories} + ErrorCodes – KHÔNG NuGet
│  ├─ src/CulinaryBlog.Application/     # Commands/Queries/Handlers (MediatR), Validators, DTOs, Behaviors, Interfaces (IRepository<T>, IUnitOfWork, repository theo module – B3)
│  ├─ src/CulinaryBlog.Infrastructure/  # EF Core DbContext, Configurations, Migrations, UnitOfWork + Repos + PersistenceExceptionTranslators, JwtService, MinIO, Redis, MailKit, Hangfire jobs, HealthChecks
│  ├─ src/CulinaryBlog.API/             # Minimal API Endpoint groups, Middleware (+ ExceptionMapping theo module), Program.cs, Scalar /scalar
│  └─ tests/ (Domain.UnitTests, Application.UnitTests, API.IntegrationTests, ArchitectureTests)
├─ frontend/                      # Next.js 15 App Router + TS + Tailwind + TanStack Query + RHF + Zod
│  └─ src/{app, components, features/{auth,recipes,categories,search,files}, lib/{api-client,query-client,seo}}
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
| Mã lỗi HTTP | **400** = mọi lỗi validation/input · **409** = mọi xung đột trạng thái (unique, concurrency, invalid state transition) · **423** = lockout · **502** = Google JWKS lỗi. **Mã 422 bị loại bỏ hoàn toàn.** |
| Lỗi | RFC 7807 qua `GlobalExceptionMiddleware`; `type` = Application Error Code (SRS Phụ lục B, **29 mã** ở v1.2.2 — 27 mã của v1.2.0 + 2 mã của CR-2026-04, §4.5) |
| Exception (từ B3) | Bất biến/quy tắc nghiệp vụ → lớp con của `XxxDomainException` trong `Domain/Exceptions/{Module}` (mang `Code`, **không mang mã HTTP**); mã HTTP đăng ký ở `API/Middleware/ExceptionMapping/{Module}ExceptionMappings.cs`; lỗi đầu vào → FluentValidation; phân quyền tài nguyên / dịch vụ ngoài → `AppException`. Bảng chọn loại lỗi: Buổi 3, mục "Hợp đồng kiến trúc" E |
| Truy cập dữ liệu (từ B3) | Phía **Command**: `IUnitOfWork.{Users, Categories, Recipes}` + `SaveChangesAsync()` một lần; nhiều lần lưu trong một transaction → `ExecuteInTransactionAsync` (đi qua execution strategy). Phía **Query**: read repository trả DTO `AsNoTracking`. Handler **không** tham chiếu `DbContext`, repository **không** trả `IQueryable`, không có hard delete. Lỗi ghi DB (`RowVersion`, `23505`) do `UnitOfWork` dịch sang domain exception qua `IPersistenceExceptionTranslator` của module |
| Response danh sách | `PagedResult<T> { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }` — khớp hình dạng Buổi 2 đã hiện thực; SRS v1.2.0 đã chốt (§5.2, **MT-42**), bỏ hẳn dạng `{ data, meta }`. Đối tượng đơn trả thẳng DTO |
| Phân trang | `pageSize` mặc định **12**, max 50; `sortBy ∈ {createdAt, publishedAt, title, cookTime, prepTime}`, `sortOrder ∈ {asc, desc}` — ngoài whitelist → **400** |
| Cache | **Redis cache-aside là cơ chế duy nhất.** Output Cache của .NET **không được dùng**. TTL theo bảng chuẩn NFR-PERF-003 |
| Endpoint | `/api/v1/...`, mỗi module một route group (`MapGroup`) – không Controller. Module có nhiều người cùng sửa thì tách file theo trách nhiệm, cùng gắn vào một group (từ B4: `RecipesEndpoints` ghi nội dung — Dev 2, `RecipeQueryEndpoints` mọi `GET` — Dev 3, `RecipeLifecycleEndpoints` vòng đời — Dev 4). Bề mặt API được khóa bởi `ApiSurfaceTests` (B4): route thật phải khớp đúng danh sách SRS Chương 8 |
| Định danh | **Đọc theo `slug`**, **ghi theo `id`**; DTO luôn mang cả `id` lẫn `slug` |
| Policy | `"AuthorPolicy"` = `RequireRole("Author","Admin")` (role Identity **không phân cấp**), `"AdminPolicy"`, `RecipeAuthorizationHandler` (Owner/Admin). **Không có policy `VerifiedAuthor`.** |
| Token phía Frontend | Access token **chỉ trong bộ nhớ**; refresh token là dữ liệu xác thực duy nhất lưu bền; access token mang claim **`sid`** (Id bản ghi refresh token) để nhận diện phiên hiện tại — SRS v1.2.0 NFR-SEC-002 (MT-55, MT-45) |
| FE API client | `lib/api-client.ts` (fetch wrapper gắn Bearer, auto-refresh 401 single-flight, parse ProblemDetails), TanStack Query keys theo module |
| FE form | React Hook Form + Zod schema mirror validator backend, lỗi inline theo `errors{}` |

### 2.3 Bảng TTL cache chuẩn (SRS NFR-PERF-003 — nguồn sự thật duy nhất)

| Khóa cache | TTL | Invalidate khi | ISR tương ứng ở FE |
|---|---|---|---|
| `categories:all` | 30 phút | Create/Update/Delete category | `/categories` → data `revalidate: 1800` |
| `categories:detail:{slug}:{queryHash}` | 2 phút | Thay đổi category đó hoặc recipe thuộc nó | `/categories/[slug]` → data `revalidate: 120` *(trang render động vì đọc `?page=` — xem `BAO_CAO_BUOI_2.md` §4.5)* |
| `recipes:list:{queryHash}` | 2 phút | Bất kỳ thay đổi recipe nào (xóa theo prefix) | `/` → `revalidate=120` |
| `recipe:{slug}` | 5 phút | Update/Publish/Unpublish/Archive/Unarchive/Delete recipe đó | `/recipes/[slug]` → `revalidate=300` |
| `search:{queryHash}` | 1 phút | Không invalidate — hết hạn tự nhiên | — (SSR) |
| **`GET /recipes/mine`** | **CẤM CACHE** | — | — (CSR, `Cache-Control: no-store`) |

> **Quy tắc vàng:** ISR `revalidate` phải **≤ TTL cache API** tương ứng. Nếu ISR dài hơn, tầng Next.js trở thành tầng cache cũ nhất và mọi nỗ lực invalidate ở Backend đều vô nghĩa.

### 2.4 Quy trình Git

- Nhánh: `main` (luôn xanh) ← `develop` ← `{mssv}_{HoTenKhongDau}_buoiso{n}` — MSSV, họ tên đầy đủ viết liền không dấu (viết hoa chữ cái đầu mỗi từ), số buổi theo cách đánh số 8 buổi. VD `2314236_HoangBinhQuan_buoiso3`, `2312755_NguyenThangThieng_buoiso2`. Bảng MSSV–tên nhánh: xem `BAO_CAO_BUOI_2.md` §7.1.
- Conventional Commits; **1 commit squash / dev / buổi**, PR có ≥1 reviewer (NFR-MAINT-001).
- **Pull Request luôn nhắm vào `develop`, không bao giờ nhắm vào `main`.** `main` chỉ nhận merge từ `develop` cuối buổi, sau khi 4 nhánh đã vào `develop` theo đúng thứ tự và kiểm chứng cuối buổi đã xanh. *(Bổ sung 29/09/2026 — PR #1 đầu tiên của nhóm nhắm thẳng vào `main`.)*
- **Danh tính commit phải là của chính thành viên:**
  - `git config user.name` = họ tên thật;
  - `git config user.email` = email đã gắn với tài khoản GitHub của mình (email trường `{mssv}@dlu.edu.vn` hoặc email cá nhân đã thêm vào GitHub).

  Commit mang tên/email khác không được GitHub gắn vào tài khoản của thành viên, nên **không chứng minh được ai đã làm phần việc đó** khi chấm điểm. *(Bổ sung 29/09/2026 — đã có commit mang tên "Nguyen Van Teo".)*
- **Không push code chưa build được.** Trước mỗi lần push phải chạy `dotnet build` (0 warning — `TreatWarningsAsErrors`) và `dotnet test`; frontend chạy `npm run lint && npm run build`. Nhánh không build được thì không review được và chặn cả nhóm khi merge.
- **Báo cáo cá nhân mỗi buổi:**
  - tên file `SPEC/BAO_CAO_LAB_0{n}_{MSSV}_{HoTenKhongDau}.md` (kèm bản `.docx` theo mẫu `SPEC/Mau_Nop_Bao_Cao_Lab_Ca_Nhan_2026.pdf`), với **n = số buổi** theo cách đánh số 8 buổi (Buổi 3 → `LAB_03`);
  - nằm trong **cùng nhánh** với code của buổi đó;
  - chỉ ghi "hoàn thành" cho hạng mục đã kiểm chứng được (test/ảnh chụp/log).
- **Migration:** mỗi dev tạo migration riêng tên `B{n}_{Module}_{FR}`; merge theo thứ tự **Dev4 → Dev1 → Dev3 → Dev2**, dev sau `rebase` + `dotnet ef migrations add` lại nếu snapshot conflict.
- **Definition of Done** mỗi commit: `dotnet build` 0 warning (repo bật `TreatWarningsAsErrors` — **dùng API obsolete là gãy build**) · `dotnet test` xanh **kể cả integration test** (harness có từ Buổi 3) · `npm run lint && npm run build` xanh · `docker compose up` chạy · test tay qua Scalar + UI.
- **Commit nền đầu buổi:** thay đổi xuyên suốt nhiều module (ví dụ D-11) được làm thành **một commit riêng trong 30 phút đầu buổi** trước khi 4 dev tách nhánh — giống commit bootstrap của Buổi 2 — để không ai phải merge chéo vào file của người khác.

---

## 3. ⚠️ Xung đột giữa lộ trình được giao và SRS — đã giải quyết theo SRS

Lộ trình dự kiến trong đề bài được viết trước khi SRS v1.1.0/v1.2.0 hoàn tất, nên có **5 điểm mâu thuẫn** (cũng được ghi tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §5). Theo nguyên tắc bắt buộc *"Bám sát 100% SRS v1.1.0"*, kế hoạch này giải quyết theo SRS và ghi rõ tại đây để nhóm không hiểu nhầm:

| # | Lộ trình đề bài ghi | SRS quy định | Quyết định áp dụng | Căn cứ |
|---|---|---|---|---|
| **X-1** | Buổi 3 Dev 1: *"FR-AUTH-003 Google OAuth **PKCE**"* | **ID Token flow** — FE gửi `{ idToken }`, BE verify bằng `Google.Apis.Auth`. **Bỏ Authorization Code + PKCE**, BE không có redirect URI | Làm theo **ID Token flow** | SRS §5.3, FR-AUTH-003, **MT-11** |
| **X-2** | Buổi 4 Dev 4: *"FR-JOB-003 **Recurring Sitemap Generator** 02:00 AM UTC"* | `FR-JOB-003` = **Permanent Purge Job** (dọn dữ liệu soft-deleted > 30 ngày), chạy **03:30 UTC**. Sitemap chuyển sang **Next.js `app/sitemap.ts`** | Buổi 4 Dev 4 làm **Purge Job**; sitemap giao Dev 3 làm ở **Buổi 7** (đề bài cũng đã xếp Dev 3 làm sitemap Next.js ở Buổi 7 — nay hết chồng chéo) | SRS §3.6 FR-JOB-003, NFR-SEO-003, **MT-05 + MT-28** |
| **X-3** | Buổi 6 Dev 3: *"Trang Danh mục (ISR **revalidate 3600s**)"* | `/categories` = **1800s**, `/categories/[slug]` = **120s**, `/` = **120s** | Dùng số của SRS (xem bảng §2.3) | SRS §5.1, **MT-33.3** |
| **X-4** | Phân công ghi *"FR-AUTH-001 đến 007"* và *"FR-RCP-001 đến 010"* | Tồn tại thêm **`FR-AUTH-008`**, **`FR-AUTH-009`** và **`FR-RCP-011`** | Gán `FR-AUTH-008` + `FR-AUTH-009` → Dev 1 (Buổi 7), `FR-RCP-011` → Dev 2 (Buổi 6) — đúng module, không đổi ranh giới phân công | SRS §2.2 (37 FR), **MT-22, MT-34, MT-45** |
| **X-5** | Buổi 7 Dev 1: *"Quản lý phiên làm việc nâng cao (Force revoke, Session Management UI)"* | v1.1.0 **không có FR nào**; "force revoke" đã nằm trong FR-AUTH-008 | Giữ tính năng vì đề bài yêu cầu; SRS v1.2.0 đã bổ sung **FR-AUTH-009** (mức C) + 3 endpoint `/auth/sessions` để tính năng có yêu cầu truy vết được | SRS FR-AUTH-009, **MT-45** |

**Lý do không chọn cách "làm theo đề bài cho nhanh":** các điểm X-1 → X-4 đều thuộc loại *nếu code theo bản cũ thì sau này phải viết lại*, không phải khác biệt bề mặt. Cụ thể: PKCE đòi BE quản lý state/verifier + redirect callback (xóa đi là bỏ cả một luồng code); sitemap sinh ở BE nằm sai domain nên crawler không bao giờ đọc được; ISR dài hơn TTL làm vô hiệu toàn bộ cơ chế invalidate. Riêng X-5 thì ngược lại: tính năng hợp lý nhưng **SRS v1.1.0 chưa có** — âm thầm thêm endpoint ngoài SRS sẽ phá vỡ chính nguyên tắc "SRS là nguồn sự thật", nên nó đã được đưa qua quy trình Change Request (CR-2026-02) và trở thành FR-AUTH-009 trong SRS v1.2.0.

---

## 4. Nợ kỹ thuật, Change Request và Errata

### 4.1 Nợ kỹ thuật từ Buổi 2 — đã kiểm chứng trong code (Buổi 2 giữ nguyên, retrofit ở buổi sau)

Buổi 2 đã code và merge theo **SRS v1.0.0**. Nội dung Buổi 2 trong tài liệu này **giữ nguyên 100%** (đúng yêu cầu), nhưng **18 hạng mục** dưới đây lệch chuẩn SRS (v1.1.0/v1.2.0). Bảng này cũng được ghi tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §4. Mỗi hạng mục được **đối chiếu trực tiếp với code trên `main`** (cột "Bằng chứng"), không suy đoán từ kế hoạch cũ, và được gán chủ + buổi cụ thể.

| # | Hiện trạng trong code Buổi 2 | Bằng chứng | Chuẩn SRS v1.2.0 | Retrofit tại | MT |
|---|---|---|---|---|---|
| **D-1** | API đăng ký nhận `{ fullName, email, userName, password }` — **người dùng tự nhập `userName`**; `UserDto` trả `fullName`, `userName`, thiếu `bio`; có mã `AUTH_USERNAME_EXISTS`. *(Cột DB đã đúng là `DisplayName` — **không cần migration**)* | `AuthEndpoints.cs:52`, `AuthContracts.cs:8`, `RegisterUserCommand.cs:9`, `ErrorCodes.cs:7` | Body `{ email, password, displayName }`; **BE tự sinh `UserName`** từ prefix email; DTO `{ id, email, displayName, avatarUrl, bio, roles }` | **B3 – Dev 1** (Google sign-in cần chung bộ sinh `UserName`) | MT-12 |
| **D-2** | Refresh token **64 byte = 512-bit** | `JwtTokenService.cs:17` `RefreshTokenBytes = 64` | **32 byte = 256-bit** | **B4 – Dev 1** | MT-13 |
| **D-3** | `Recipe.Slug` là UNIQUE thường | `RecipeConfiguration.cs:60` | **Partial unique** `WHERE "IsDeleted" = false` | **B4 – Dev 4** *(kéo từ B6 — đi cùng `DELETE /recipes/{id}`)* | MT-05 |
| **D-4** 🔴 | `GetRecipesQuery` lọc theo danh tính (Guest 46 / Author 47 / Admin 50 bản ghi) **và** bị Output Cache theo khóa công khai; `GetRecipeBySlug` trả **403** cho Draft (lộ sự tồn tại của bản nháp) | `RecipeReadRepository.cs:87-97`, `GetRecipeBySlugQuery.cs:31-33` | **Chỉ `Published` cho mọi người gọi**; Draft/Archived → **404** như slug không tồn tại; dữ liệu riêng qua `/recipes/mine` | **B4 – Dev 3** *(kéo từ B6 — hợp đồng `GET /recipes`)* | **MT-34** |
| **D-5** 🔴 | `GetCategoryBySlugQuery` cũng lọc theo danh tính | `GetCategoryBySlugQuery.cs:36` | **Chỉ `Published`**, không đọc `ICurrentUser` | **B4 – Dev 3** *(kéo từ B6)* | **MT-34** |
| **D-6** | Output Cache (Redis-backed) `RecipeList` 15′ / `RecipeDetail` 60′ | `OutputCachePolicies.cs:16-24`, `RecipesEndpoints.cs:19,26` | **Bỏ Output Cache**, dùng `ICacheable` Redis cache-aside TTL 2′/5′ | **B4 – Dev 3** *(kéo từ B6)*; audit **B7 – Dev 4** | MT-16, MT-17, MT-34 |
| **D-7** | FE fetch `revalidate` 3600 (`/categories`) và 600 (`/categories/[slug]`) | `BAO_CAO_BUOI_2.md` §4.5 | **1800** và **120** | **B6 – Dev 3** | MT-33.3 |
| **D-8** | `Recipe.Instructions` là `NOT NULL` | `RecipeConfiguration.cs:25` `.IsRequired()` | **NULL** (legacy field) | **B3 – Dev 2** — *phải làm trước khi `POST /recipes` nhận `instructions?` tùy chọn, nếu không mọi request thiếu trường này sẽ lỗi ở DB* | MT-20.8 |
| **D-9** | Hangfire chạy ở **container worker riêng** (`Hangfire__WorkerOnly=true`), trong khi SRS v1.1.0 §3.6 ghi "in-process" | `docker-compose.yml` service `hangfire`, `Program.cs:17-18` | **SRS v1.2.0 đã chuẩn hóa theo code** — worker riêng (MT-47). Không còn lệch, không retrofit | — | MT-47 |
| **D-10** | Sắp xếp bằng **một tham số `sort=-field`**; enum thiếu `PrepTime`; FE gửi `sort=-createdAt` | `RecipeSortParser.cs`, `IRecipeReadRepository.cs:7-13`, `RecipesEndpoints.cs:42`, `frontend/src/app/recipes/(list)/page.tsx:37` | **`sortBy` + `sortOrder`**, whitelist 5 trường | **B4 – Dev 3** (API) · **B5 – Dev 3** (FE) | MT-01 |
| **D-11** | Mọi lỗi validation trả **422**; FE bắt lỗi field bằng `status === 422` | `GlobalExceptionMiddleware.cs:65-83`, `AuthEndpoints.cs:17,26`, `RecipesEndpoints.cs:21`, `LoginForm.tsx:35`, `RegisterForm.tsx:44` | **400** cho mọi lỗi validation; **bỏ hẳn 422** | **B3 – commit nền đầu buổi** (Dev 4 dẫn) | MT-08 |
| **D-12** | Access token **và** refresh token lưu trong **`localStorage`** | `auth-context.tsx:39-56`; `BAO_CAO_BUOI_2.md` §4.12 ("cần xem lại Buổi 4") | Access token **chỉ trong bộ nhớ**; refresh token là thứ duy nhất được lưu bền (xem giải trình B4 Dev 1) | **B5 – Dev 1** *(phần FE, tách khỏi API refresh ở B4)* | NFR-SEC-002 |
| **D-13** | TTL `categories:all` = **60 phút** | `GetCategoriesQuery.cs:11` | **30 phút** | **B3 – Dev 3** | MT-17 |
| **D-14** | `next/image` chạy `unoptimized`; URL ảnh là `localhost:9000` — **container frontend không truy cập được** | `BAO_CAO_BUOI_2.md` §4.13, `MinioOptions.PublicBaseUrl` | Ảnh phục vụ qua Nginx `/media/` → MinIO, bật lại tối ưu ảnh | **B7 – Dev 4** (Nginx) + **B7 – Dev 3** (Next.js) | NFR-PERF-005 |
| **D-15** | `Recipe.Publish()` **cho phép Archived → Published** và coi Published → publish là no-op; ném `DomainException` chung không có mã lỗi | `Recipe.cs:114-128` | Chuyển trạng thái ngoài bảng → **409 `RECIPE_INVALID_STATE_TRANSITION`**; thiếu step/ingredient → **400 `RECIPE_PUBLISH_INCOMPLETE`** | **B4 – Dev 4** *(kéo từ B5 — đi cùng API publish)* | MT-35, MT-06 |
| **D-16** | UNIQUE `(RecipeId, StepNumber)` khai báo bằng **unique index** — PostgreSQL **không cho phép index là `DEFERRABLE`** | `RecipeConfiguration.cs:87` `HasIndex(...).IsUnique()` | Đổi thành **unique constraint `DEFERRABLE INITIALLY DEFERRED`** để renumber trong một transaction | **B4 – Dev 2** | MT-03 |
| **D-17** | Khóa DataProtection lưu **không mã hóa** trên volume `dpkeys` | `BAO_CAO_BUOI_2.md` §5.1 (cảnh báo `XmlKeyManager[35]`) | `ProtectKeysWithCertificate` ở production | **B7 – Dev 4** (cùng HTTPS) | NFR-SEC-007 |
| **D-18** | `init.sql` tạo text search config `vietnamese_unaccent` — **cơ chế FTS thứ hai**, lại chỉ chạy trong Docker (Testcontainers không chạy file này) | `docker/postgres/init.sql` | Một cơ chế duy nhất theo SRS: `simple` + `unaccent_immutable`, khai báo **trong migration** | **B4 – Dev 3** | MT-25 |

**Phân bổ retrofit theo buổi (cập nhật 29/09/2026):** B3 → D-1, D-8, D-11, D-13 · **B4 → D-2, D-3, D-4, D-5, D-6, D-10 (API), D-15, D-16, D-18** · B5 → D-10 (FE), D-12 · B6 → D-7 · B7 → D-14, D-17 (+ audit D-6). Mọi retrofit làm **thay đổi hợp đồng của một endpoint** dồn về Buổi 4, vì yêu cầu "hoàn thành tất cả API endpoints" không đạt được khi endpoint còn trả sai mã hoặc rò dữ liệu. Hệ quả tốt: lỗ hổng MT-34 được vá sớm hai buổi.

> **Vì sao không dồn retrofit vào đầu Buổi 3:** dồn 17 hạng mục vào một buổi sẽ tạo một commit khổng lồ chạm vào code của cả 4 dev cùng lúc → xung đột merge và không ai review nổi. Mỗi hạng mục được gắn vào **đúng buổi mà dev đó đang mở lại file liên quan** — sửa khi tay đã ở trong file là rẻ nhất và dễ review nhất. **Ngoại lệ duy nhất là D-11** (422 → 400): nó chạm vào middleware dùng chung và mọi endpoint mới của Buổi 3 đều phụ thuộc vào nó, nên phải làm **trước tiên** dưới dạng commit nền.

### 4.2 CR-2026-02 — Bổ sung và làm rõ SRS (đã duyệt, áp dụng trong SRS v1.2.0)

Khi đối chiếu SRS v1.1.0 với code Buổi 2, kế hoạch phát hiện 8 điểm SRS còn thiếu hoặc tự mâu thuẫn. SRS quy định *mọi thay đổi sau khi duyệt phải qua Change Request*, nên kế hoạch **không tự thêm endpoint ngoài SRS**: cả 8 điểm được gom thành **CR-2026-02**, đã được duyệt và áp dụng vào **SRS v1.2.0** (19/09/2026). Phân tích đầy đủ từng điểm (hiện trạng, phương án, lý do chọn) nằm trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md` theo số MT tương ứng. Nhờ vậy, **mọi việc trong kế hoạch này đều truy vết được về một yêu cầu trong SRS**. *(Cập nhật 29/09/2026: yêu cầu bổ sung của Buổi 3 phát sinh 2 mã lỗi chưa có trong SRS — đang chờ duyệt dưới dạng **CR-2026-04**, xem §4.5.)*

| # | Vấn đề ở SRS v1.1.0 | Quyết định trong SRS v1.2.0 | MT | Buổi thực hiện |
|---|---|---|---|---|
| **C-1** | Hình dạng response danh sách: §5.2/§8 ghi `{ data, meta }` (kèm `pageSize: 10`), FR-SRCH-004 và code dùng `PagedResult` | Chốt **`PagedResult<T>`**; đối tượng đơn trả thẳng DTO | MT-42 | Đã đúng từ B2 — không phải sửa code |
| **C-2** | `POST /files/upload`, `DELETE /files/{**key}` có từ Buổi 2 nhưng ngoài SRS — trong khi `avatarUrl`, `imageUrl` danh mục và ảnh bước nấu **chỉ nhận URL** | **Chính thức hóa** vào FR-FILE-001/002, thêm Chương 8.8 | MT-43 | Đã có từ B2; dùng ở B3 (Dev 3), B4 (Dev 2), B5 (Dev 1) |
| **C-3** | `/dashboard/users` (§5.1) không có API danh sách người dùng | Thêm `GET /users` vào **FR-AUTH-008** | MT-44 | API B4 – Dev 1 · UI B7 – Dev 1 |
| **C-4** | Quản lý phiên có trong lộ trình (X-5) nhưng không có FR | Thêm **FR-AUTH-009** (mức C) + 3 endpoint `/auth/sessions` + claim `sid` | MT-45 | API B4 – Dev 1 · UI B7 – Dev 1 |
| **C-5** | 3 mã lỗi code đã dùng nhưng thiếu trong Phụ lục B | Thêm `FILE_FORBIDDEN` (403), `FILE_STORAGE_UNAVAILABLE` (503), `INTERNAL_ERROR` (500) — 24 → **27 mã**. **Không** thêm `AUTH_USERNAME_EXISTS` (bị xóa khi làm D-1) | MT-46 | Đã có từ B2; xóa `AUTH_USERNAME_EXISTS` ở B3 |
| **C-6** | Hangfire "in-process" (SRS) vs worker riêng (code) — xem **D-9** | Chuẩn hóa **worker riêng** | MT-47 | Không phải sửa code |
| **C-7** | Sitemap cần toàn bộ slug, `GET /recipes` giới hạn 50 | Thêm **`GET /recipes/sitemap`** gọn nhẹ, cache 1 giờ | MT-48 | B4 – Dev 3 |
| **C-8** | Hứa "có thể khôi phục" nhưng không có chức năng khôi phục | Làm rõ: khôi phục do **quản trị viên vận hành** trên DB trong 30 ngày; tự phục vụ ngoài phạm vi (§1.2.3) | MT-49 | Không triển khai trong 7 buổi |

### 4.3 Lỗi kỹ thuật trong SRS v1.1.0 — đã sửa trong SRS v1.2.0

Cùng lượt đối chiếu còn phát hiện **8 lỗi kỹ thuật nằm trong chính SRS v1.1.0**: đặc tả nghe hợp lý trên giấy nhưng **làm đúng từng chữ thì hệ thống hỏng**. Tất cả đã được sửa trong SRS v1.2.0; kế hoạch này làm theo bản đã sửa. Ba điểm đầu (E-1 → E-3) là những điểm đã được nêu ở bản kế hoạch trước; năm điểm sau được tìm thấy khi rà lại toàn bộ SRS để áp dụng CR-2026-02.

| # | SRS v1.1.0 ghi | Vì sao sai | SRS v1.2.0 / kế hoạch áp dụng | MT |
|---|---|---|---|---|
| **E-1** | §6.5 cấu hình Nginx `upstream api_pool { server api:8080; }` (lặp lại ở §6.1, NFR-SCALE-003) | **Tái tạo đúng bug 502 đã sửa ở Buổi 2** (commit `064f582`): khối `upstream` chỉ phân giải DNS **một lần lúc Nginx khởi động** → container `api` được tạo lại đổi IP là Nginx trả 502 | `resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến. Khi `--scale api=3`, Docker DNS trả nhiều bản ghi và Nginx tự luân phiên — còn **tự nhận replica mới trong 10 giây**, tốt hơn `upstream` tĩnh | MT-50 🔴 |
| **E-2** | §6.5 healthcheck dùng `curl` cho `api`, `frontend`, `minio`; `pg_isready` không có `-h` | `aspnet:10.0` và `minio/minio` **không có `curl`**; `node:22-alpine` chỉ có `wget`; `pg_isready` không `-h` báo "ready" khi Postgres **chưa mở TCP** (lỗi #1 của Buổi 2) | `pg_isready -h 127.0.0.1`, `redis-cli ping`, `mc ready local`; `api` cài `curl` ở stage runtime; `frontend` dùng `wget` | MT-51 |
| **E-3** | §8.1: Admin tự khóa mình → **409** (`VALIDATION_ERROR`) | Mâu thuẫn với FR-AUTH-008 A2 (**403**) và với Phụ lục B (`VALIDATION_ERROR` luôn là 400) | **403 Forbidden** | MT-52 |
| **E-4** | "`/recipes/search` phải được **đăng ký trước** `/recipes/{slug}`"; danh sách slug dành riêng mỗi nơi một kiểu | ASP.NET Core ưu tiên segment literal **bất kể thứ tự khai báo** — vấn đề thật là slug trùng từ khóa sẽ không truy cập được | **Một** danh sách slug dành riêng tại NFR-SEO-004: `search, mine, sitemap, new, edit` | MT-53 |
| **E-5** | "Khai báo `KnownProxies` giới hạn ở dải mạng Docker" | `KnownProxies` chỉ nhận **từng IP**; `KnownNetworks` đã **obsolete trên .NET 10** → gãy build vì `TreatWarningsAsErrors` | `KnownIPNetworks` + `System.Net.IPNetwork` | MT-54 |
| **E-6** | Không quy định Frontend lưu token ở đâu | Khoảng trống này dẫn thẳng tới việc Buổi 2 lưu cả hai token vào `localStorage` (D-12) | Access token **chỉ trong bộ nhớ**; CSP thành yêu cầu tường minh | MT-55 |
| **E-7** | `UNIQUE (RecipeId, StepNumber)` "khai báo `DEFERRABLE`" mà không nói dạng khai báo | PostgreSQL **không cho index là deferrable** — làm bằng `HasIndex().IsUnique()` thì kéo-thả sắp xếp bước luôn lỗi `23505` (D-16) | Ghi rõ **UNIQUE CONSTRAINT**; ảnh primary (partial index) đổi hai bước trong một transaction | MT-56 |
| **E-8** | `/categories/[slug]` là "ISR" | Trang đọc `?page=` nên Next.js **bắt buộc render động** — không thể ISR | **SSR + Data Cache** `revalidate: 120` | MT-57 |

> **Bài học ghi lại:** cả 8 lỗi trên đều **không thể phát hiện bằng cách đọc chéo các chương với nhau** — chỉ lộ ra khi đối chiếu tài liệu với hệ thống đang chạy. Đây là lý do mọi quyết định trong kế hoạch này được kiểm chứng trên code thật (xem cột "Bằng chứng" ở §4.1), và là quy tắc mới cho mọi Change Request sau: thay đổi chạm tới cấu hình hạ tầng, mã lỗi, ràng buộc cơ sở dữ liệu hoặc API của framework phải được kiểm chứng trên môi trường thật trước khi duyệt.

### 4.4 CR-2026-03 — Yêu cầu dữ liệu mẫu của giảng viên (đã duyệt, áp dụng trong SRS v1.2.1)

Khi đánh số lại 8 buổi, giảng viên giao cho Buổi 2 năm yêu cầu nền tảng. Bốn yêu cầu đầu đã được Buổi 2 làm xong; **yêu cầu thứ 5 là yêu cầu mới, khác với SRS v1.2.0** (§2.6.1 chỉ ghi *"Bogus với 50 recipe mẫu và 5 tác giả mẫu"*). Theo nguyên tắc *"SRS là nguồn sự thật"*, yêu cầu này được đưa qua Change Request **CR-2026-03** và ghi vào **SRS v1.2.1** trước khi code, không âm thầm làm khác SRS.

| # | Yêu cầu của giảng viên cho Buổi 2 | Trạng thái | Nơi thực hiện |
|---|---|---|---|
| 1 | Tạo cấu trúc dự án backend theo Clean Architecture | Đã hoàn thành (11/09) | Commit bootstrap `b058654` — 4 tầng + 4 project test, kiểm bằng `LayerDependencyTests` |
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
- **Tại sao seeder tự bù thay vì bắt cả nhóm xóa volume:** database của mọi thành viên đang có 50 công thức cũ, và seeder cũ bỏ qua bước tạo công thức khi bảng đã có dữ liệu. Seeder mới **idempotent và tự bù**: thêm danh mục/công thức còn thiếu; công thức mẫu cũ — do tác giả mẫu tạo, **chưa từng bị ai sửa** (`UpdatedAt` null) và chưa đạt ngưỡng — được thay nội dung bằng bản đúng trong catalog. Dữ liệu người dùng tạo hoặc đã chỉnh sửa không bị động tới.
- **Ảnh hưởng tới kế hoạch tổng thể:** không đổi schema, không đổi API, không đổi phân công. Các con số "50 công thức" ở Buổi 4 (backfill `SearchVector`) và Buổi 5 (`EXPLAIN ANALYZE`) được cập nhật thành 100; kết luận của Buổi 5 giữ nguyên vì 100 bản ghi vẫn quá nhỏ để PostgreSQL chọn Index Scan (`PerformanceSeeder` ≥ 10.000 bản ghi vẫn cần).

### 4.5 Yêu cầu bổ sung Buổi 3 & Buổi 4 (29/09/2026) — errata kiến trúc E-9 và CR-2026-04 (đã áp dụng trong SRS v1.2.2)

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
5. **UI:** `/auth/register`, `/auth/login` (CSR) – RHF + Zod, lỗi inline map từ ProblemDetails; `AuthProvider` lưu access token in-memory + refresh token; header hiển thị user đã đăng nhập. Test: xUnit cho validator + integration test register/login (happy + 409/401).

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **ASP.NET Core Identity** thay vì tự viết bảng user: Identity đã đóng gói sẵn PBKDF2 với iteration count đạt chuẩn, `SecurityStamp` (để thu hồi quyền tức thời), lockout chống brute-force và toàn bộ hạ tầng role/claim — tự viết lại chỉ để "gọn hơn" là đánh đổi bảo mật lấy vài trăm dòng code, không đáng.

Quyết định **lưu SHA-256 hash của refresh token** (không lưu raw) ngay từ buổi đầu là có chủ đích: nếu để sau mới sửa thì phải viết migration vá và vô hiệu hóa toàn bộ token đang lưu hành. Hệ quả thực tế của việc lưu raw là kẻ đọc được DB (SQL injection, backup rò rỉ) mạo danh được **mọi** người dùng cho tới khi token hết hạn.

Refresh token đi trong **body request** (không cookie) để tránh CSRF mà không cần thêm cơ chế anti-forgery (SRS §5.2). Ở Buổi 2, cả access token lẫn refresh token được lưu `localStorage` — một lựa chọn **có ý thức và tạm thời**: khi chưa có luồng refresh (FR-AUTH-004), đây là cách duy nhất giữ phiên qua lần tải lại trang. Nó được ghi thành nợ **D-12** và trả ở Buổi 4, khi luồng refresh cho phép đưa access token về bộ nhớ.

`IWelcomeEmailScheduler` được để **stub no-op** thay vì chờ Dev 4 dựng xong Hangfire: đây là kỹ thuật *seam* — Dev 1 hoàn thành lát cắt dọc của mình ngay trong buổi, Dev 4 chỉ cần thay implementation ở Buổi 3 mà không đụng vào handler đăng ký. Không có seam này, hai dev sẽ chặn nhau.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 ghi độ dài refresh token **512-bit** ở FR-AUTH-001 nhưng **128-bit** ở NFR-SEC-002 — lệch nhau 4 lần. FR-AUTH dùng trường `fullName` + `userName` (người dùng tự nhập), trong khi Chương 7 chỉ có cột `DisplayName` và Chương 8 chỉ có `displayName`.
- **Hướng đi chọn lựa:** Buổi 2 đã code theo **512-bit** (`RefreshTokenBytes = 64`); cột DB đúng là `DisplayName` nhưng **API contract** vẫn nhận `fullName` + `userName`. SRS v1.1.0 (MT-13, MT-12) chốt: **256-bit**; body đăng ký `{ email, password, displayName }`; **BE tự sinh `UserName`** từ prefix email.
- **Tại sao chọn:** 256-bit khớp đúng độ dài output SHA-256 và `TokenHash varchar(64)`; entropy vượt 256-bit không tăng thực chất vì đằng nào cũng bị hash, chỉ tốn băng thông mỗi lần refresh. Bắt người dùng tự nghĩ `userName` vừa thêm một ô nhập vô nghĩa (hệ thống không hiển thị nó ở đâu), vừa sinh thêm một loại lỗi trùng (`AUTH_USERNAME_EXISTS`) mà người dùng không hiểu vì sao. **Retrofit D-1 (Buổi 3), D-2 và D-12 (Buổi 4).**

**Phần 5 – Commit:** `feat(auth): complete FR-AUTH-001 & FR-AUTH-002 register and local login flow`

## DEV 2 – Recipe lõi

**Phần 1 – Chức năng hoàn thành:** `FR-RCP-001` Xem danh sách công thức (phân trang) + `FR-RCP-002` Xem chi tiết công thức.

**Phần 2 – Các bước:**
1. **DB/Entity:** `Recipe` (Title 200, Slug 220 UNIQUE, Description, Instructions, PrepTime, CookTime, Servings, `RecipeDifficulty` 1–4, `RecipeStatus` 0–2, CategoryId FK RESTRICT, AuthorId FK, PublishedAt), owned `RecipeNutrition` (prefix `Nutrition_`, decimal(8,2)?), `RecipeStep`, `RecipeIngredient`, `RecipeImage` (cascade); index Slug/Status/CategoryId/AuthorId/PublishedAt/Difficulty → migration `B2_Recipe_Schema`.
2. **Seed:** `RecipeSeeder` dùng **Bogus** – 50 recipe mẫu, 5 tác giả (theo SRS §2.6.1), đủ step/ingredient/nutrition để FE có dữ liệu thật. *(Sau này nâng lên 20 danh mục / 100 công thức theo CR-2026-03 — mục "Dev 4 – Yêu cầu bổ sung".)*
3. **CQRS:** `GetRecipesQuery` (page ≥1, pageSize 1–50, lọc quyền: Guest→Published, Author→Published + Draft của mình, Admin→tất cả; `.AsNoTracking()` + projection sang `RecipeSummaryDto`, không N+1); `GetRecipeBySlugQuery` (Include Steps/Ingredients/Images/Category/Author, 404 `RECIPE_NOT_FOUND`, Draft/Archived → 403 nếu không phải owner/Admin).
4. **API:** `RecipesEndpoints.cs` → `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}`; Output Cache policy `RecipeList` (15 phút, vary query) và `RecipeDetail` (60 phút, tag `recipes`, `recipe:{slug}`).
5. **UI:** `/recipes` (SSR) lưới `RecipeCard` + phân trang + loading skeleton; `/recipes/[slug]` (ISR revalidate=300) hiển thị ảnh, nguyên liệu, các bước, bảng dinh dưỡng, tác giả.

**Phần 3 – Định hướng & Lý do thiết kế:**
Dựng **toàn bộ schema Recipe ngay Buổi 2** (kể cả Step/Ingredient/Image/Nutrition dù chưa có API cho chúng) là quyết định có tính toán: schema là thứ đắt nhất để sửa sau khi đã có dữ liệu. Có đủ bảng từ đầu thì các buổi sau chỉ thêm API/UI, không phải viết migration vá.

`RecipeNutrition` là **Owned Entity** (cột `Nutrition_*` nhúng thẳng vào bảng `Recipes`) chứ không phải bảng riêng: dinh dưỡng không có vòng đời độc lập với công thức — không ai tra cứu "bảng dinh dưỡng" tách rời khỏi món ăn. Mô hình hóa đúng bản chất này giúp tránh một JOIN ở mọi truy vấn chi tiết và loại bỏ nhu cầu tạo repository/DTO/endpoint thừa.

Seed **50 recipe bằng Bogus** ngay buổi đầu để FE có dữ liệu thật mà dựng lưới, phân trang, skeleton — không phải chờ Dev khác tạo nội dung thủ công. Đây cũng là bộ dữ liệu để Dev 3 kiểm thử Full-Text Search ở Buổi 4.

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
2. **Cache:** `ICacheService`/`RedisCacheService` (StackExchange.Redis, try/catch fallback DB khi Redis down); `CachingBehavior` dùng `ICacheable { CacheKey, Expiration }`.
3. **CQRS:** `GetCategoriesQuery : ICacheable` (key `categories:all`, TTL 30 phút, sort Name ASC, `recipeCount` chỉ đếm Published); `GetCategoryBySlugQuery` (404 `CATEGORY_NOT_FOUND`, recipes Published + Draft của currentUser, OFFSET pagination → `PagedResult<RecipeSummaryDto>`).
4. **API:** `CategoriesEndpoints.cs` → `GET /api/v1/categories`, `GET /api/v1/categories/{slug}?page&pageSize`.
5. **UI:** `/categories` (ISR 3600) lưới `CategoryCard`; `/categories/[slug]` (ISR 600) header danh mục + list recipe phân trang; component `CategoryNav` cho header site.

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **Redis `IDistributedCache` ngay từ đầu** thay vì `IMemoryCache` (dù v1.0.0 Chương 3 ghi `IMemoryCache`): với `IMemoryCache`, khi chạy nhiều API instance sau Nginx thì Admin sửa danh mục trên instance 1 mà instance 2 vẫn phục vụ dữ liệu cũ tới 60 phút, **và không có cách nào invalidate**. Danh mục chỉ ≤ 50 bản ghi nên chi phí round-trip Redis (~1ms) không đáng kể so với rủi ro đó.

`RedisCacheService` bọc **try/catch fallback về DB**: Redis là tầng tăng tốc, không phải tầng bắt buộc. Redis chết thì hệ thống chậm đi chứ không được sập (NFR-REL-002).

`SlugHelper` đặt ở Buổi 2 dù Category chưa cần sinh slug động, vì **Dev 2 sẽ dùng lại nó ở Buổi 3** để sinh slug cho Recipe. Viết một lần, có unit test riêng cho tiếng Việt (`Phở bò` → `pho-bo`, `Bánh mì` → `banh-mi`, `đ` → `d`) là rẻ hơn nhiều so với hai dev tự viết hai hàm bỏ dấu khác nhau.

`recipeCount` **chỉ đếm Published** vì đây là con số hiển thị cho khách: đếm cả Draft sẽ khiến người dùng bấm vào danh mục "12 công thức" rồi chỉ thấy 3 món.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** mục 3.2 và FR-CAT-001/003 của v1.0.0 quy định cache danh mục bằng **`IMemoryCache`**, trong khi NFR-SCALE-001 ghi rõ *"Distributed cache (Redis, **không** in-memory `IMemoryCache`) cho mọi shared state"* và Chương 6 vẽ Cache Layer = Redis. Ngoài ra `GetCategoryBySlugQuery` trả thêm Draft của người gọi — cùng lỗi gốc với MT-34.
- **Hướng đi chọn lựa:** Buổi 2 đã chọn **Redis** (đón đầu đúng hướng, khớp MT-16), nhưng đặt TTL **60 phút** theo Chương 3 cũ thay vì 30 phút của NFR-PERF-003. Riêng phần trả Draft sẽ được gỡ bỏ theo MT-34.
- **Tại sao chọn:** Redis là một cơ chế duy nhất cho toàn hệ thống — hai cơ chế cache song song (`IMemoryCache` cho danh mục, Redis cho phần còn lại) nghĩa là hai chỗ phải nhớ invalidate, và chỗ bị quên sẽ là chỗ sinh bug khó tái hiện nhất. **Retrofit D-13 (Buổi 3), D-5 và D-7 (Buổi 6).**

**Phần 5 – Commit:** `feat(categories): complete FR-CAT-001 & FR-CAT-002 public categories with redis cache`

## DEV 4 – Hạ tầng & File

**Phần 1 – Chức năng hoàn thành:** Docker Compose 8 services + `FR-FILE-001` Upload file lên MinIO + `FR-FILE-002` Xóa file khỏi MinIO (kèm Reusable Image Upload Component).

**Phần 2 – Các bước:**
1. **DevOps:** `docker-compose.yml` gồm `nginx, api, frontend, postgres:16-alpine, redis:7-alpine (--appendonly yes), minio (console :9001), hangfire (worker), seq` + `mailhog` (profile dev); volumes `pgdata, redisdata, miniodata, seqdata`; `init.sql` bật extension `unaccent`, `pg_trgm`; `.env.example`, User Secrets cho dev (NFR-SEC-007); README "setup < 5 phút".
2. **Application/Infra:** `IFileStorageService { UploadAsync(stream, fileName, contentType, folder, ct), DeleteAsync(url, ct) }`; `MinioFileStorageService` (AWSSDK.S3 + ServiceURL override, `ForcePathStyle`), bucket `culinary-blog` tự tạo + policy public-read khi khởi động; tên file `{folder}/{Guid}{ext}` chống path traversal; Delete idempotent (object không tồn tại không throw).
3. **Validation:** `FileValidator` – ≤5MB kiểm tra trước khi đọc stream, MIME whitelist jpeg/png/webp/avif, **magic bytes** (FF D8 FF, 89 50 4E 47, RIFF…WEBP, ftypavif) → 400 `FILE_SIZE_EXCEEDED` / `FILE_MIME_INVALID`.
4. **API tạm để test độc lập:** `POST /api/v1/files` + `DELETE /api/v1/files` (Bearer, dùng lại ở avatar Buổi 5); unit test magic bytes.
5. **UI:** `components/ImageUploader.tsx` tái sử dụng – drag & drop, preview, validate client (size/MIME), **progress bar %** qua XHR `upload.onprogress` (NFR-USE-004), `onUploaded(url)` callback.

**Phần 3 – Định hướng & Lý do thiết kế:**
Chọn **`AWSSDK.S3`** thay vì MinIO .NET SDK chính chủ: `IFileStorageService` được thiết kế để swap implementation, và với `AWSSDK.S3` thì việc chuyển sang AWS S3 thật chỉ là bỏ dòng override `ServiceURL` — trong khi SDK chính chủ khóa chặt hệ thống vào MinIO và phải viết lại toàn bộ implementation khi đổi nhà cung cấp.

**Magic bytes validation** thay vì tin `Content-Type` header: header do client gửi nên đổi được tùy ý — đổi tên `payload.php` thành `.jpg` và khai `image/jpeg` là qua được mọi kiểm tra dựa trên header. Đọc vài byte đầu file mới biết được định dạng thật.

Tên file **`{folder}/{Guid}{ext}`** (không dùng tên gốc của người dùng) chặn path traversal (`../../etc/passwd`) từ gốc, đồng thời loại luôn vấn đề trùng tên và ký tự lạ trong tên file tiếng Việt.

`ImageUploader` được viết **tái sử dụng ngay từ đầu** vì nó sẽ được dùng ở ít nhất 4 chỗ: ảnh công thức (Dev 2, B3), ảnh bước nấu (Dev 2, B4), avatar (Dev 1, B5), ảnh danh mục (Dev 3, B3). Progress bar thật (qua `XHR.upload.onprogress`, không phải spinner giả) là yêu cầu định lượng của NFR-USE-004.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** mục 2.1.2 của v1.0.0 ghi *"HTTP/S3 API + **MinIO .NET SDK**"*, trong khi Chương 5.3, 6.1 và 6.2 đều ghi **`AWSSDK.S3`** (3/4 tài liệu). Ngoài ra "8 services" của compose bao gồm một service `hangfire` riêng, còn SRS §6.5 liệt kê 8 services khác (`nginx, api, frontend, postgres, redis, minio, seq, mailhog`) với Hangfire chạy **in-process trong api**.
- **Hướng đi chọn lựa:** Buổi 2 đã chọn **`AWSSDK.S3`** (khớp MT-18 — đúng hướng) và chạy Hangfire ở **container worker riêng** dùng chung image API (`Hangfire__WorkerOnly=true`). Kế hoạch **giữ topology worker riêng**, và SRS v1.2.0 đã được sửa theo nó (§3.6, §6.5 — **MT-47**) thay vì ép code gộp ngược vào `api`.
- **Tại sao chọn:** worker riêng là thiết kế tốt hơn về dài hạn, không chỉ "hợp lý về vận hành": FR-JOB-002 resize ảnh bằng ImageSharp là tác vụ **nặng CPU**; nếu chạy in-process, mỗi lượt upload ảnh sẽ tranh CPU với chính các request đang phục vụ người đọc, kéo p95 vượt 500ms (NFR-PERF-001). Tách worker ra còn cho phép scale hai loại tải độc lập (`--scale api=3` mà không nhân ba số job server). Chi phí đã được trả xong ở Buổi 2: khóa DataProtection dùng chung qua volume `dpkeys`, và worker chờ DB sẵn sàng qua `DatabaseReadiness`. Danh sách service thực tế: `nginx, api, hangfire, frontend, postgres, redis, minio, seq` + `mailhog` (dev) — khớp con số "8 services" của Buổi 2. **D-9 được xử lý bằng cách sửa SRS (MT-47) — không có việc retrofit code.**

**Phần 5 – Commit:** `feat(infra,files): docker compose 8 services & complete FR-FILE-001/002 minio storage with image uploader`

## DEV 4 – Yêu cầu bổ sung: Dữ liệu mẫu theo CR-2026-03 (21/09/2026)

> **Yêu cầu mới, khác SRS v1.2.0:** SRS v1.2.0 §2.6.1 chỉ yêu cầu 50 công thức và 5 tác giả mẫu. Giảng viên yêu cầu CSDL có **ít nhất 20 danh mục, 100 công thức; mỗi công thức ít nhất 10 nguyên liệu và 5 bước chế biến**. Yêu cầu đã được ghi vào SRS v1.2.1 (CR-2026-03, §4.4) trước khi code.

**Phần 1 – Chức năng hoàn thành:** bộ dữ liệu mẫu đạt ngưỡng CR-2026-03 trên mọi database — cài mới cũng như database đang có 50 công thức cũ.

**Phần 2 – Các bước:**
1. **Catalog dữ liệu thật:** `Infrastructure/Persistence/Seed/Data/categories.json` (20 danh mục: 8 cũ + Cơm & Xôi, Lẩu, Hải sản, Gỏi & Salad, Món cuốn, Món hấp, Món chiên, Ăn vặt, Món Hàn Quốc, Món Nhật Bản, Món Thái, Món Âu) và 5 file `recipes-*.json` (100 món: 50 món cũ giữ nguyên tên, slug và danh mục + 50 món mới; mỗi món 10 – 14 nguyên liệu có định lượng, đơn vị, ghi chú và 5 – 6 bước có thời gian). Nhúng vào assembly bằng `EmbeddedResource`.
2. **`RecipeSeedCatalog`:** đọc catalog, khai báo ngưỡng `MinCategories = 20`, `MinRecipes = 100`, `MinIngredientsPerRecipe = 10`, `MinStepsPerRecipe = 5`.
3. **`DatabaseSeeder` tự bù, idempotent, trong một transaction (qua execution strategy):** thêm danh mục thiếu (so theo tên và slug, kể cả bản ghi đã xóa mềm để không vi phạm UNIQUE); thêm công thức thiếu theo slug; công thức mẫu cũ của tác giả mẫu, `UpdatedAt` null và dưới ngưỡng → xóa nguyên liệu/bước sinh ngẫu nhiên, nạp lại từ catalog (`StepNumber` đánh lại từ 1). Bogus (seed cố định) chỉ sinh tác giả, trạng thái, ngày xuất bản, dinh dưỡng theo khoảng hợp lý từng nhóm món.
4. **Cache:** khi có danh mục mới, seeder xóa khóa `categories:all`; khi có dữ liệu thay đổi, `Program.cs` xóa Output Cache tag `recipes` — máy nào đang có cache cũ cũng thấy dữ liệu mới ngay.
5. **Test + kiểm chứng:** `RecipeSeedCatalogTests` (5 test: ngưỡng, slug duy nhất, danh mục hợp lệ, không danh mục rỗng, khớp độ dài cột DB). Chạy thật trên Docker cả hai trường hợp: database cũ 8 danh mục/50 công thức → *"50 recipes created, 50 recipes repaired"*; database trống → *"100 recipes created"*; khởi động lại → *"0 created, 0 repaired"*. Truy vấn SQL xác nhận 20 danh mục, 100 công thức, tối thiểu 10 nguyên liệu và 5 bước, số bước liên tục, không còn mô tả Lorem ipsum, danh mục nào cũng có công thức.

**Phần 3 – Định hướng & Lý do thiết kế:** xem §4.4 — nội dung món ăn viết tay vì dữ liệu sinh ngẫu nhiên từ danh sách chung không đúng với món; seeder tự bù để không ai phải xóa volume; chỉ sửa bản ghi mẫu chưa từng bị chỉnh sửa để không đè lên dữ liệu người dùng.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn:** yêu cầu của giảng viên (≥ 100 công thức, ≥ 20 danh mục) khác SRS v1.2.0 §2.6.1 (50 công thức).
- **Hướng đi chọn lựa:** không làm lệch SRS mà cập nhật SRS lên v1.2.1 qua CR-2026-03, rồi mới code.
- **Tại sao chọn:** giữ đúng nguyên tắc mọi việc truy vết được về SRS (§4.2); ai đọc SRS cũng thấy đúng con số đang chạy trong database.

**Phần 5 – Commit:** `feat(seed): seed 20 categories and 100 real recipes per CR-2026-03`

---

# BUỔI 3 – NỀN TẢNG XỬ LÝ LỖI & TRUY CẬP DỮ LIỆU, ĐĂNG NHẬP GOOGLE / LOGOUT, TẠO DRAFT RECIPE, CRUD CATEGORIES ADMIN, HEALTH CHECKS & HẠ TẦNG KIỂM THỬ

> **Mục tiêu buổi:** hệ thống có đủ đường vào (local + Google + thoát), Author tạo được nội dung nháp kèm ảnh, Admin quản trị được danh mục, trạng thái hệ thống quan sát được qua health check và dashboard job an toàn, **cả nhóm có hạ tầng integration test** để mọi buổi sau viết test thật. **Bổ sung 29/09/2026:** cả nhóm dựng xong **ba nền tảng kiến trúc** mà mọi thao tác ghi dữ liệu từ Buổi 3 đến Buổi 8 đều đi qua — **Domain Exceptions**, **Repository & Unit of Work**, **Global Exception Middleware** — và **mỗi dev giao ít nhất 2 API chạy thật**.

## Yêu cầu của giảng viên cho Buổi 3 (bổ sung 29/09/2026) và cách chia cho 4 dev

Giảng viên giao bốn yêu cầu bắt buộc cho Buổi 3. Bản kế hoạch trước (lập trước ngày 29/09) **không đáp ứng được ba trong bốn yêu cầu**: không dev nào có bước dựng domain exceptions (việc thêm mã lỗi cho `DomainException` nằm ở Buổi 4), không dev nào có bước dựng repository/Unit of Work, và Dev 4 không có API nào (dashboard `/hangfire` là trang giao diện, bộ test là hạ tầng). Bảng dưới là phân công mới — **mỗi yêu cầu được chia cho cả 4 dev theo module mà dev đó sở hữu suốt 8 buổi (§1)**:

| Yêu cầu | Dev 1 – Auth | Dev 2 – Recipe | Dev 3 – Category | Dev 4 – Hạ tầng |
|---|---|---|---|---|
| **1. Domain Exceptions** (`CulinaryBlog.Domain/Exceptions`) | `AuthDomainException` + 6 lớp cụ thể | `RecipeDomainException` + 5 lớp cụ thể | `CategoryDomainException` + 3 lớp cụ thể | Lớp gốc trừu tượng `DomainException`, `BusinessRuleViolationException`, `ConcurrencyConflictException`; chuyển danh mục mã lỗi `ErrorCodes` về Domain |
| **2. Repository & Unit of Work** (interface ở Application, cài đặt ở Infrastructure) | `IUserRepository` / `UserRepository` | `IRecipeRepository` / `RecipeRepository` + bộ dịch lỗi ghi DB của Recipe | `ICategoryRepository` / `CategoryRepository` + bộ dịch lỗi ghi DB của Category | `IRepository<T>`, `IUnitOfWork`, `EfRepository<T>`, `UnitOfWork` (transaction commit/rollback qua execution strategy, dịch lỗi ghi DB) |
| **3. Global Exception Middleware** (RFC 7807) | `AuthExceptionMappings` | `RecipeExceptionMappings` | `CategoryExceptionMappings` | Nâng cấp `GlobalExceptionMiddleware` + `ExceptionStatusMap` + bộ test hợp đồng Problem Details |
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
| `AppDbContext` | Code đang dùng `CulinaryBlogDbContext` (migration, harness, seeder tham chiếu) | Giữ tên cũ — đổi tên chỉ tạo diff lớn, không thêm giá trị |
| "Middleware hoặc .NET 10 `IExceptionHandler`" | SRS §6.2 gọi tên `GlobalExceptionMiddleware`; lớp này đã chạy từ Buổi 2 và đang có test | Giữ middleware, nâng cấp bên trong (xem giải trình Dev 4) |
| `IUserRepository` thao tác `AspNetUsers` | `ApplicationUser` là kiểu của ASP.NET Identity nằm ở Infrastructure; mọi thao tác **ghi** user phải qua `UserManager` | `IUserRepository` chỉ **đọc** user + quản lý `RefreshTokens`; ghi user vẫn qua `IIdentityService` (xem giải trình Dev 1) |

## Tiến trình trong buổi

1. **CR-2026-04 (§4.5) — ✅ đã áp dụng trong SRS v1.2.2:** 2 mã lỗi `AUTH_USER_NOT_FOUND` (404) và `CONCURRENCY_CONFLICT` (409) đã có trong Phụ lục B.
2. **Commit nền D-11 (422 → 400)** — ✅ **đã hoàn thành** trong commit `67d29c0` của Dev 4 (`BAO_CAO_KET_QUA_BUOI_3.md` §2). Mọi endpoint mới của buổi này trả lỗi validation **400**; Frontend dùng `mapProblemDetailsToForm()` trong `lib/api-client.ts`.
3. **Mốc M1 — commit nền kiến trúc của Dev 4 vào `develop` (chậm nhất giữa buổi):** lớp gốc exception, `IRepository<T>`/`IUnitOfWork`/`UnitOfWork`, middleware + `ExceptionStatusMap`. Trong lúc chờ M1, ba dev làm những phần **không phụ thuộc** nền: Dev 1 retrofit D-1 + nút Google phía FE; Dev 2 migration D-8 + validator + wizard FE; Dev 3 validator + trang quản trị FE + D-13.
4. **Sau M1:** Dev 1–3 rebase lên `develop`, viết exception + repository + file ánh xạ của module mình, rồi tới handler và integration test.
   > **Với nhánh đã push code Buổi 3 trước ngày 29/09** (Dev 1, Dev 2, Dev 3 đều đã push — xem nhận xét review trên GitHub): **không làm lại từ đầu**. Rebase lên `develop` sau M1 rồi *chuyển* code đã có sang nền mới:
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
- **Mỗi module có một lớp gốc trừu tượng** (`AuthDomainException`, `RecipeDomainException`, `CategoryDomainException`) để `catch` được theo module khi cần, và để test kiến trúc kiểm được "exception của module nào nằm đúng thư mục module đó".
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
| `ValidationException` (FluentValidation) | 400 | `VALIDATION_ERROR` | `ValidationProblemDetails` với `errors{}` theo tên field camelCase (D-11) |
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
| Xung đột khi ghi DB (RowVersion, unique) | **Không ném tay** — `UnitOfWork` dịch tự động qua translator | Hai Admin tạo trùng tên cùng lúc |
| Không có quyền trên một tài nguyên cụ thể | `ForbiddenException` (AppException) | `RECIPE_FORBIDDEN`, `FILE_FORBIDDEN` |
| Dịch vụ ngoài / hạ tầng không trả lời | `BadGatewayException` (502), `ServiceUnavailableException` (503) | Google JWKS, MinIO |

Sau khi Dev 1, 2, 3 chuyển các chỗ ném cũ sang domain exception, ba lớp `NotFoundException`, `ConflictException`, `LockedException` trong `AppException.cs` **không còn ai dùng** → dev merge cuối (Dev 2) xóa chúng, để mỗi loại lỗi chỉ có **một** cách biểu diễn.

## DEV 1 — Google Sign-In & Đăng xuất (+ Auth Domain Exceptions & User Repository)

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-AUTH-003` Đăng nhập / Đăng ký bằng Google OAuth 2.0 (**ID Token flow**) + `FR-AUTH-005` Đăng xuất & Revoke Refresh Token. **Nền tảng module Auth:** cụm Auth Domain Exceptions, `IUserRepository`/`UserRepository`, ánh xạ lỗi Auth trong middleware. **Kèm retrofit D-1:** chuẩn hóa hợp đồng đăng ký theo `displayName` và BE tự sinh `UserName`.

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
2. **Ánh xạ HTTP — `API/Middleware/ExceptionMapping/AuthExceptionMappings.cs`:** đăng ký đủ 6 lớp theo cột HTTP ở bảng trên. Test kiến trúc của Dev 4 sẽ đỏ nếu thiếu lớp nào.
3. **Repository — `IUserRepository` (Application) / `UserRepository` (Infrastructure):** thay `IRefreshTokenRepository` của Buổi 2 (xóa interface cũ; `AuthResponseFactory` chuyển sang `unitOfWork.Users`). Phạm vi Buổi 3:
   - `AddRefreshTokenAsync(RefreshToken token, …)` — dời từ `RefreshTokenRepository`.
   - `GetRefreshTokenAsync(string userId, string tokenHash, …)` — token **đang được theo dõi** để thu hồi (logout).
   - `GetUserNamesStartingWithAsync(string prefix, …)` — cho `IUserNameGenerator` chọn hậu tố `2, 3…` bằng **một** truy vấn thay vì thử từng tên.
   - Thêm property `Users` vào `IUnitOfWork`/`UnitOfWork`.
   - **Ghi user (tạo, liên kết Google, khóa) vẫn qua `IIdentityService` → `UserManager`** — không ghi thẳng `AspNetUsers`.
4. **Domain — `RefreshToken.Revoke(DateTime utcNow, string? replacedByTokenHash = null)`** *(kéo từ Buổi 4 lên vì logout cần)*: gán `RevokedAt` nếu chưa thu hồi — **gọi lại không đổi gì** (idempotent). Kiểm tra hiệu lực dùng `IsActive(utcNow)` **đã có sẵn** từ Buổi 2.
5. **Retrofit D-1 — `UserNameGenerator` + hợp đồng mới:** `IUserNameGenerator` sinh `UserName` từ phần trước `@` của email (chữ thường, loại ký tự ngoài `IdentityOptions.User.AllowedUserNameCharacters`, thêm hậu tố `2, 3…` nếu trùng — tra bằng `GetUserNamesStartingWithAsync`). Đổi `RegisterRequest`/`RegisterUserCommand` thành `{ email, password, displayName }`; `UserDto` thành `{ id, email, displayName, avatarUrl, bio, roles }`. FE: `RegisterForm` bỏ ô "Tên đăng nhập", đổi "Họ tên" thành "Tên hiển thị"; `AuthProvider` đọc `user.displayName`. **Không cần migration** — cột DB đã là `DisplayName`.
6. **FE — Google Identity Services:** cài `@react-oauth/google`, bọc `<GoogleOAuthProvider>` ở layout; dùng **component `<GoogleLogin onSuccess={({ credential }) => ...}>`** để lấy **ID Token**, POST lên `/api/v1/auth/google`. ⚠️ **Không dùng hook `useGoogleLogin`** — hook đó trả *access token/authorization code*, **không phải ID Token**, và backend sẽ không verify được. Không cấu hình redirect URI ở Backend, không dùng PKCE.
7. **CQRS — `GoogleLoginCommand { IdToken }`:** xác minh qua **`IGoogleIdTokenValidator`** (interface ở Application, cài đặt ở Infrastructure gọi `GoogleJsonWebSignature.ValidateAsync(idToken, new ValidationSettings { Audience = [_cfg.GoogleClientId] })` — thư viện tự kiểm **chữ ký, `iss`, `aud`, `exp`**). Bảng lỗi theo FR-AUTH-003:
   - `idToken` rỗng/sai định dạng → validator → **400 `VALIDATION_ERROR`**; token thiếu claim `email` → `BusinessRuleViolationException(AUTH_GOOGLE_TOKEN_INVALID)` → **400**.
   - Chữ ký/`aud`/`exp` sai → `InvalidTokenException.GoogleTokenRejected()` → **401 `AUTH_GOOGLE_TOKEN_INVALID`**.
   - Không lấy được JWKS → `BadGatewayException` → **502 `AUTH_GOOGLE_UNAVAILABLE`**.
   - Liên kết tài khoản: `FindByLoginAsync("Google", payload.Subject)` → có thì đăng nhập; chưa có mà email đã tồn tại thì **chỉ `AddLoginAsync` khi `payload.EmailVerified == true`** (ngược lại → `BusinessRuleViolationException` **400**); hoàn toàn mới thì tạo user với `DisplayName = payload.Name`, `AvatarUrl = payload.Picture`, `UserName` từ `IUserNameGenerator`, role `Author`. `IsActive == false` → `AccountDisabledException` → **403**.
8. **CQRS — `LogoutCommand { RefreshToken }`:** hash SHA-256 chuỗi nhận được → `unitOfWork.Users.GetRefreshTokenAsync(currentUser.UserId, hash)` → `token.Revoke(utcNow)` → `SaveChangesAsync()`. **Idempotent:** không tìm thấy vẫn trả 204 (không tiết lộ token có tồn tại hay không).
9. **API + UI + Test:** `POST /api/v1/auth/google` (200 `AuthResponseDto`), `POST /api/v1/auth/logout` (`RequireAuthorization()`, 204). UI: nút Google trên `/auth/login` kèm thông báo dự phòng khi nhận 502 (SRS §2.6.2); menu user → "Đăng xuất" gọi API rồi mới xóa phiên phía client (trả nợ `BAO_CAO_BUOI_2.md` §4.10 — trước đây nút này chỉ xóa token phía client, refresh token vẫn sống 7 ngày). **Integration test** (harness của Dev 4, `IGoogleIdTokenValidator` thay bằng bản giả trong test — không gọi Google thật): Google hợp lệ → 200; token bị từ chối → 401 `AUTH_GOOGLE_TOKEN_INVALID`; JWKS lỗi → 502; email đã có nhưng chưa xác minh → 400; tài khoản bị vô hiệu → 403; đăng ký `an.nguyen@x.com` rồi `an.nguyen@y.com` → `UserName` là `an.nguyen` và `an.nguyen2`; logout hai lần liên tiếp đều 204 và DB ghi `RevokedAt`; **hồi quy:** đăng nhập sai quá số lần vẫn trả **423** kèm `lockoutEnd` sau khi chuyển sang `AccountLockedException`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**ID Token flow** đẩy toàn bộ vòng lặp redirect về phía trình duyệt và Google, Backend chỉ còn một việc duy nhất: nhận một chuỗi JWT rồi xác minh chữ ký. Hệ quả kiến trúc là Backend **giữ nguyên tính stateless** — không phải lưu `state`, không phải lưu PKCE `code_verifier`, không phải mở thêm route callback, không cần `GoogleClientSecret`. Với một hệ thống đặt mục tiêu scale ngang (NFR-SCALE-001), mọi thứ phải lưu giữa hai request đều là gánh nặng: nó buộc ta phải chọn giữa sticky session hoặc một kho state dùng chung.

Điểm **bắt buộc không được cắt**: Backend phải **tự verify chữ ký** ID Token bằng `Google.Apis.Auth`. Nếu tin dữ liệu Frontend gửi lên (kiểu "FE đã đăng nhập Google rồi, BE cứ tạo user theo email FE báo") thì bất kỳ ai cũng có thể `curl` thẳng vào endpoint với email của người khác và chiếm tài khoản. Đây là lỗ hổng nghiêm trọng nhất có thể mắc trong luồng OAuth.

**Vì sao bọc Google sau `IGoogleIdTokenValidator`:** `GoogleJsonWebSignature.ValidateAsync` là hàm tĩnh gọi mạng tới Google. Gọi thẳng trong handler thì (1) tầng Application phải tham chiếu package `Google.Apis.Auth` — trái nguyên tắc Application chỉ phụ thuộc Domain; (2) không có cách nào viết integration test cho nhánh 401/502 mà không gọi Google thật. Interface một phương thức giải quyết cả hai, và chính lớp cài đặt ở Infrastructure là nơi dịch `InvalidJwtException` → `InvalidTokenException`, `HttpRequestException` → `BadGatewayException` — handler chỉ thấy lỗi nghiệp vụ.

**Logout idempotent** là lựa chọn có chủ đích: trả 404 khi token không tồn tại sẽ biến endpoint thành một *oracle* cho phép kẻ tấn công dò xem chuỗi token nào đang hợp lệ. Ngoài ra người dùng bấm Đăng xuất hai lần (hoặc mạng chập chờn gây retry) không có lý do gì phải nhận lỗi.

Việc **link Google vào email đã tồn tại** thay vì tạo tài khoản trùng email giải quyết một tình huống rất thực tế: người dùng đăng ký bằng email/mật khẩu hôm trước, hôm sau bấm "Đăng nhập bằng Google" với cùng email đó. Nếu tạo tài khoản mới, họ sẽ mất toàn bộ công thức đã viết mà không hiểu vì sao. Nhưng tự động liên kết **chỉ an toàn khi Google xác nhận email đã được kiểm chứng** (`email_verified`): nếu liên kết theo một email chưa kiểm chứng, kẻ tấn công chỉ cần tạo tài khoản Google gắn email của nạn nhân là **chiếm được tài khoản Culinary Blog của nạn nhân**. Một dòng kiểm tra `payload.EmailVerified` chặn đứng kịch bản đó.

**Vì sao `IUserRepository` không ghi thẳng `AspNetUsers` và không kế thừa `IRepository<T>`:** `UserManager` của Identity làm nhiều việc mà một repository tự viết sẽ bỏ sót — chuẩn hóa `NormalizedEmail`/`NormalizedUserName` (thiếu là tìm theo email không ra), đổi `SecurityStamp`, đếm lần đăng nhập sai để lockout. Ghi thẳng bảng là tạo ra user "đúng dữ liệu nhưng sai trạng thái bảo mật" mà không có lỗi nào báo. Vì vậy phân vai rõ: **ghi user → `IIdentityService`/`UserManager`**, **đọc user phục vụ nghiệp vụ + vòng đời refresh token → `IUserRepository`**. `IRepository<T>` ràng buộc `T : BaseEntity` (có `Id` Guid, xóa mềm, `RowVersion`), trong khi `ApplicationUser` là kiểu Identity ở Infrastructure và `RefreshToken` không kế thừa `BaseEntity` (SRS §7.8) — ép chúng vào `IRepository<T>` là nới lỏng ràng buộc cho mọi module chỉ vì một ngoại lệ.

**Vì sao gộp refresh token vào `IUserRepository` thay vì giữ `IRefreshTokenRepository`:** refresh token không có vòng đời độc lập — nó sinh ra khi user đăng nhập và mọi truy vấn về nó đều bắt đầu từ `UserId` (logout B3, thu hồi cả họ token B4, khóa tài khoản B7, danh sách phiên B7). Một repository cho cả nhóm thao tác "phiên của người dùng" giúp Buổi 4 (refresh, khóa tài khoản, danh sách phiên — cùng một buổi) chỉ thêm phương thức vào **một** chỗ.

**Vì sao làm D-1 ngay buổi này, không đợi Buổi 5 (Profile):** luồng Google tạo tài khoản mới mà **không có ai nhập `userName`** — nên bắt buộc phải có bộ sinh `UserName` tự động ngay bây giờ. Khi đã có nó, giữ ô "Tên đăng nhập" ở form đăng ký thường là giữ hai cách tạo `UserName` song song cho cùng một hệ thống. Ngoài ra `UserDto` là hợp đồng mà **mọi màn hình** FE đọc để hiển thị người dùng; đổi nó ở Buổi 3 khi mới có 2 màn hình dùng là rẻ nhất — càng để muộn, càng nhiều component phải sửa.

**Vì sao tạo `UserNotFoundException` từ bây giờ dù Buổi 4 mới dùng:** yêu cầu của buổi này là dựng **trọn** cụm exception của module. Tạo lớp cùng ánh xạ và test ngay bây giờ giúp API FR-AUTH-008 ở Buổi 4 chỉ việc `throw`. Mã `AUTH_USER_NOT_FOUND` chưa có trong Phụ lục B nên đi qua CR-2026-04 (§4.5) — không tự đặt mã ngoài SRS.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 mô tả **ba kiến trúc Google OAuth khác nhau ở ba chỗ**: FR-AUTH-003 ghi *"Authorization Code Flow + PKCE, Auth.js v5 ở Next.js xử lý callback, FE gửi `ExternalLoginInfo` lên BE"*; mục 5.3 khai báo redirect URI là `/api/v1/auth/google/callback` (tức **BE** nhận callback); Chương 8 lại nhận body `{ idToken }` từ Google Sign-In JS SDK (ID-token flow, không phải Authorization Code). Nghiêm trọng hơn, `ExternalLoginInfo` là **kiểu nội bộ của ASP.NET Core Identity — không serialize qua HTTP được**, nên đặc tả đó không khả thi về mặt kỹ thuật.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-11**) chốt **ID Token flow**: body `{ idToken }`, BE verify bằng `GoogleJsonWebSignature.ValidateAsync`, **bỏ `ExternalLoginInfo`, bỏ Authorization Code + PKCE, bỏ redirect URI phía BE**. *(Đây là điểm xung đột **X-1** với lộ trình đề bài — xem §3.)*
- **Tại sao chọn:** So sánh ba phương án theo tầm nhìn dài hạn. **(A) ID Token** — đơn giản nhất, BE stateless, chỉ cần `GoogleClientId`, khớp Chương 8; nhược điểm là không lấy được refresh token của Google, nhưng ta **không cần** vì hệ thống tự phát JWT riêng. **(B) Tin Auth.js ở FE** — tận dụng thư viện có sẵn nhưng BE phải tin dữ liệu FE gửi lên → **lỗ hổng bảo mật nghiêm trọng**, loại ngay. **(C) BE tự chạy Authorization Code + PKCE** — chuẩn mực nhất về lý thuyết, nhưng BE phải quản lý state/verifier qua nhiều request (phá vỡ stateless), phải xử lý redirect qua lại giữa BE và FE, và tăng đáng kể số đường code cần test. Với hệ thống mà JWT do chính ta phát hành và Google chỉ đóng vai trò *"chứng minh người này sở hữu email X"*, phương án A đạt đúng mục tiêu bảo mật với chi phí kiến trúc thấp nhất.
- **Mâu thuẫn trong SRS v1.2.1 — cùng một mã, hai mã HTTP (đã làm rõ ở v1.2.2, MT-61):** Phụ lục B ghi `AUTH_GOOGLE_TOKEN_INVALID` = **400**, còn FR-AUTH-003 A1 trả **401** với chính mã đó khi chữ ký/`aud`/`exp` sai. Kế hoạch theo **FR** (401 cho token không qua kiểm tra chữ ký, 400 cho token thiếu dữ liệu) vì FR là đặc tả hành vi chi tiết hơn; ánh xạ theo **kiểu exception** (không theo mã) cho phép làm đúng cả hai. SRS v1.2.2 (CR-2026-04 c) ghi rõ hai trường hợp này ở cả FR-AUTH-003 lẫn Phụ lục B.
- **Đề xuất phân công ghi `AccountLockedException` → 403:** trái Phụ lục B (**423** — MT-08 chỉ loại 422, không đổi 423) và trái test Buổi 2 đang chạy. Giữ 423; mã 403 thuộc `AccountDisabledException`.

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
   | `RecipeConcurrencyException(recipeId, inner)` | `RECIPE_CONCURRENCY_CONFLICT` | 409 | Sinh tự động bởi bộ dịch ở bước 4 (B3 → `PUT` B4, test đồng thời B8) |
   | `InvalidRecipeStatusException(RecipeStatus from, string action)` | `RECIPE_INVALID_STATE_TRANSITION` | 409 | Máy trạng thái + D-15 (API B4 — Dev 4) |
   | `RecipeSlugConflictException(slug)` | `RECIPE_SLUG_EXISTS` | 409 | Sinh tự động khi hai request tranh cùng slug (bước 4) |

   Chuyển `NotFoundException(ErrorCodes.RecipeNotFound, …)` trong `GetRecipeBySlugQuery` sang `RecipeNotFoundException`. `RECIPE_FORBIDDEN` **giữ** `ForbiddenException` (AppException) — đó là lỗi phân quyền do `RecipeAuthorizationHandler` quyết định, không phải bất biến của entity.
3. **Repository — `IRecipeRepository : IRepository<Recipe>` (Application) / `RecipeRepository` (Infrastructure):**
   - `GetByIdWithDetailsAsync(Guid id, …)` — `Include(Steps)`, `Include(Ingredients)`, `Include(Images)`, `.AsSplitQuery()` (tránh tích Descartes 3 collection). **`Nutrition` là Owned Entity nên EF tự nạp cùng dòng `Recipes`, không cần `Include`.** Dùng cho Buổi 4: steps/ingredients/update (Dev 2), publish/archive — D-15 bắt buộc có đủ Steps + Ingredients (Dev 4), `/recipes/mine/{id}` (Dev 3).
   - `GetByIdWithImagesAsync(Guid id, …)` — chỉ nạp `Images`, cho 3 command ảnh của buổi này (không kéo steps/ingredients không dùng tới).
   - `SlugExistsAsync(string slug, …)` — dùng **`IgnoreQueryFilters()`** vì `IDX_Recipe_Slug` hiện là unique **thường** (tính cả bản ghi đã xóa mềm) — thiếu nó thì trùng slug với recipe đã xóa sẽ nổ `23505`. Buổi 4 (D-3 — Dev 4) đổi index sang partial thì bỏ `IgnoreQueryFilters()` tại đúng phương thức này.
   - Thêm property `Recipes` vào `IUnitOfWork`/`UnitOfWork`. `IRecipeReadRepository` (Buổi 2) giữ nguyên cho phía Query.
4. **Bộ dịch lỗi ghi DB — `RecipePersistenceExceptionTranslator : IPersistenceExceptionTranslator`:** `DbUpdateConcurrencyException` có entry thuộc aggregate Recipe (`Recipe`, `RecipeStep`, `RecipeIngredient`, `RecipeImage` — lấy `RecipeId` của entry con) → `RecipeConcurrencyException`; `23505` trên `IDX_Recipe_Slug` → `RecipeSlugConflictException`. Đây là cơ chế mà FR-RCP-004 (Buổi 5) và test đồng thời (Buổi 8) dựa vào — handler không bao giờ phải bắt exception của EF.
5. **Ánh xạ HTTP — `RecipeExceptionMappings.cs`:** đăng ký 5 lớp theo cột HTTP ở bước 2.
6. **CQRS — `CreateRecipeCommand` + Validator:** body `{ title, description, categoryId, prepTime, cookTime, servings, difficulty, instructions?, nutrition? }` — `title` 5–200, `description` **20–2000** (Bảng Giới hạn Chuẩn §7.9), `prepTime > 0`, `cookTime >= 0`, `servings > 0`. **`categoryId` phải tồn tại:** rule bất đồng bộ trong validator gọi `IRepository<Category>.ExistsAsync` (repository generic của Dev 4 — **không phải chờ** `ICategoryRepository` của Dev 3) → sai thì **400 `VALIDATION_ERROR`** với lỗi ở field `categoryId` (FR-RCP-003 A2). Dùng factory **`Recipe.Create(...)` đã có sẵn từ Buổi 2** (`Status = Draft`, `PublishedAt = null`); slug qua `SlugHelper.Generate(title)` (đã có ở `Domain/Common`), trùng (`SlugExistsAsync`) thì hậu tố `-2`, `-3`; **cấm trùng từ khóa dành riêng** `search`, `mine`, `sitemap`, `new`, `edit`. Nutrition gán qua `recipe.SetNutrition(...)` — **không có endpoint riêng**. Ghi: `unitOfWork.Recipes.AddAsync(recipe)` → `SaveChangesAsync()`. Hai mảng tùy chọn `steps?`/`ingredients?` trong cùng body được kích hoạt ở **Buổi 4** (xem giải trình).
7. **Authorization dùng chung:** `RecipeAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Recipe>` (pass khi `recipe.AuthorId == currentUser.Id` **hoặc** user có role `Admin`); `AuthorPolicy = RequireRole("Author","Admin")`. Đây là handler dùng lại cho **toàn bộ** FR-RCP còn lại.
8. **CQRS ảnh:** `UploadRecipeImageCommand` nạp recipe bằng `GetByIdWithImagesAsync` (không có → `RecipeNotFoundException`), gọi `IFileStorageService` + `ImageFileInspector` (magic bytes, có sẵn từ Buổi 2), folder `recipes/{recipeId}/`, rồi `recipe.AddImage(...)` (**đã có sẵn** — ảnh đầu tiên tự `IsPrimary = true`); `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`; `DeleteRecipeImageCommand` xóa bản ghi + `BackgroundJob.Enqueue` xóa file MinIO, xóa đúng ảnh primary thì ảnh có **`OrderIndex` nhỏ nhất (hòa thì `CreatedAt` sớm nhất)** lên thay. Ảnh không thuộc recipe → `RecipeImageNotFoundException` → 404. ⚠️ **Đổi ảnh primary phải làm hai bước trong một transaction tường minh — dùng `unitOfWork.ExecuteInTransactionAsync(...)`**: bên trong, nạp lại recipe → hạ primary cũ → `SaveChangesAsync` → nâng primary mới → `SaveChangesAsync`. Vì index `IDX_RecipeImage_Primary` (partial unique, có từ Buổi 2) **không thể deferrable**, và EF Core không đảm bảo thứ tự hai lệnh `UPDATE` trong một batch. Tất cả implement `ICacheInvalidator` → xóa prefix `recipes:list:` + khóa `recipe:{slug}`.
9. **API + UI + Test:** `POST /api/v1/recipes` (201); `POST /api/v1/recipes/{id}/images` (multipart, 201 trả `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }`, **503 `FILE_STORAGE_UNAVAILABLE`** nếu MinIO lỗi); `PATCH` và `DELETE /api/v1/recipes/{id}/images/{imageId}`. UI `/dashboard/recipes/new` — wizard bước 1 (thông tin cơ bản + nutrition), bước 2 (gallery ảnh dùng `ImageUploader`, chọn ảnh chính bằng radio → gọi PATCH), toast xác nhận. **Integration test:** tạo nháp 201; body sai → 400 kèm `errors`; `categoryId` không tồn tại → 400 ở field `categoryId`; Guest → 401; recipe của người khác → 403 `RECIPE_FORBIDDEN`; ảnh không tồn tại → 404 `RECIPE_NOT_FOUND`; MinIO lỗi → 503; **đổi primary giữa hai ảnh 10 lần liên tiếp không lần nào vỡ unique index**; hai `DbContext` cùng sửa một recipe → `SaveChangesAsync` lần sau ném `RecipeConcurrencyException` (kiểm chứng bộ dịch trên PostgreSQL thật, trước khi có endpoint update ở Buổi 5).

**Phần 3 – Định hướng & Lý do thiết kế:**
Công thức **luôn sinh ra ở trạng thái `Draft`** chứ không cho tạo thẳng Published: người viết cần không gian nháp để hoàn thiện dần, và hệ thống cần một điểm chặn để kiểm tra điều kiện xuất bản (đủ step + ingredient) ở FR-RCP-005. Cho phép tạo thẳng Published sẽ mở đường cho công thức rỗng lọt ra ngoài và làm hỏng structured data SEO.

**Nutrition đi kèm trong body `POST /recipes`** chứ không có endpoint riêng — hệ quả trực tiếp của việc nó là Owned Entity. Nếu làm `PUT /recipes/{id}/nutrition` riêng, ta phải tạo thêm command, validator, DTO và repository cho một thứ vốn chỉ là nhóm cột trong bảng `Recipes`, đồng thời mở ra khả năng **hai đường code cùng ghi một nhóm cột** — nguồn bug kinh điển. Frontend dùng wizard nhiều bước thì gom state ở client rồi submit một lần.

**Repository theo aggregate, hai mức nạp dữ liệu:** Recipe là aggregate root (SRS §3.3) — steps, ingredients, images chỉ được sửa qua `Recipe`, nên repository chỉ có cho `Recipe`, không có `IRecipeImageRepository` hay `IRecipeStepRepository`. Hai phương thức nạp (`WithImages` / `WithDetails`) thay vì một phương thức nạp tất cả vì command ảnh chạy rất thường xuyên (mỗi lần kéo-thả, mỗi lần chọn ảnh chính) — kéo theo 10+ nguyên liệu và 5+ bước mỗi lần là tốn vô ích. Ngược lại, **không** mở `IQueryable` cho handler tự `Include`: khi đó mỗi handler tự quyết định nạp gì, và lỗi kinh điển "gọi `Publish()` khi chưa `Include(Steps)` nên recipe đủ điều kiện vẫn bị từ chối" (Buổi 5) sẽ lặp lại ở mọi nơi.

**Dịch lỗi xung đột ở Infrastructure, không bắt trong handler:** `DbUpdateConcurrencyException` là kiểu của EF Core; tầng Application không tham chiếu EF nên handler **không thể** bắt nó mà không phá Clean Architecture. Dịch ngay tại `UnitOfWork` nghĩa là mọi command ghi Recipe — hiện tại và tương lai — tự động trả 409 `RECIPE_CONCURRENCY_CONFLICT` thay vì 500, không ai phải nhớ viết `try/catch`. Kiểm chứng bộ dịch ngay Buổi 3 (chưa có endpoint update) vì PATCH ảnh đã có thể xung đột khi hai tab cùng đổi ảnh chính.

**`categoryId` kiểm tra bằng `IRepository<Category>` generic**, không đợi `ICategoryRepository` của Dev 3: câu hỏi "Id này có tồn tại không" là thao tác chung mà repository generic đã trả lời được. Nhờ vậy Dev 2 không phụ thuộc thứ tự làm việc của Dev 3 trong buổi, và hai module không tham chiếu repository chuyên biệt của nhau.

`RecipeAuthorizationHandler` được dựng **ngay ở FR đầu tiên có ghi dữ liệu**, không đợi tới lúc có nhiều endpoint mới gom lại. Lý do: NFR-SEC-006 yêu cầu kiểm tra phân quyền **tại Application Layer**, không chỉ ở Presentation. Nếu mỗi handler tự viết `if (recipe.AuthorId != userId) throw` thì chỉ cần một handler quên là có lỗ hổng, và không có cách nào kiểm chứng tập trung.

Quy tắc **"xóa ảnh primary thì ảnh nào lên thay"** được ghi thành tiêu chí xác định (`OrderIndex` nhỏ nhất, hòa thì `CreatedAt` sớm nhất) thay vì "ảnh đầu tiên còn lại": "đầu tiên" theo thứ tự nào là câu hỏi không có đáp án nếu không nói rõ, và hai dev sẽ hiểu hai kiểu — một người lấy theo thứ tự trả về của DB (vốn không đảm bảo), một người lấy theo thời gian tạo.

**Vì sao hai mảng inline `steps?`/`ingredients?` của `POST /recipes` để sang Buổi 4:** chúng cần đúng những bất biến mà FR-RCP-009/010 xây ở Buổi 4 — cột `QuantityText`, ràng buộc "không rỗng cả ba", `StepNumber` do server gán. Làm chúng ngay bây giờ nghĩa là viết validator nguyên liệu/bước **hai lần** (một lần cho body inline, một lần cho endpoint riêng) rồi phải giữ đồng bộ mãi mãi — đúng loại trùng lặp logic sinh nợ kỹ thuật. Ở Buổi 4, body inline chỉ việc gọi lại `recipe.AddIngredient(...)`/`recipe.AddStep(...)`, nên mỗi quy tắc chỉ tồn tại ở một chỗ. Giao diện của Buổi 3 cũng không cần chúng (wizard bước 3–4 thuộc Buổi 4), nên lát cắt DB → API → UI của buổi này vẫn trọn vẹn.

**Vì sao tạo `InvalidRecipeStatusException` từ Buổi 3 dù Buổi 4 mới ném:** máy trạng thái Recipe do một dev khác (Dev 4, Buổi 4) hiện thực. Có sẵn lớp exception + ánh xạ 409 + test ánh xạ trong module Recipe từ bây giờ nghĩa là Dev 4 dùng **đúng** lớp của module, không tự nghĩ ra lớp thứ hai.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-008 của v1.0.0 thiết kế endpoint hành động riêng `PATCH /recipes/{id}/images/{imgId}/primary` với response upload `{ url, isPrimary }`, trong khi Chương 8.4 thiết kế `PATCH /recipes/{id}/images/{imageId}` với body gộp `{ altText?, isPrimary?, orderIndex? }` và response 4 trường — hai thiết kế API khác hẳn nhau. (b) FR-RCP-008 bước 1 nói form-data chỉ chứa field `"file"` nhưng bước 6 lại dùng `altText`. (c) Nhánh A4 trả **503** khi MinIO lỗi nhưng ô "HTTP Status Code trả về" của chính FR đó **không liệt kê 503**. (d) FR-RCP-003 nói Steps/Ingredients "có thể tạo cùng lúc hoặc thêm riêng lẻ sau", nhưng nutrition bị bỏ lửng — không FR nào đặc tả endpoint riêng cho nó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-19, MT-41.4, MT-41.5, MT-41.6, MT-02**) chốt **PATCH metadata gộp**, response upload đủ 7 trường (thêm `mediumUrl`, `thumbnailUrl` vì FR-JOB-002 sinh ra chúng), form-data có `altText?`/`isPrimary?`, bổ sung **503** vào danh sách status code, và ghi rõ **nutrition chỉ tạo/sửa cùng recipe**.
- **Tại sao chọn:** PATCH gộp **RESTful hơn** — một tài nguyên, một endpoint, sửa được nhiều trường trong một request; endpoint hành động riêng `/primary` sẽ kéo theo `/alt-text`, `/order` khi cần sửa các trường khác, làm số endpoint phình theo số thuộc tính. Nhược điểm của PATCH gộp là logic "chỉ 1 ảnh primary" nằm lẫn trong handler chung nên phải xử lý cẩn thận — chấp nhận được, và đã được ghi thành hai quy tắc nghiệp vụ tường minh trong SRS Chương 8.4 để không ai quên. Về `mediumUrl`/`thumbnailUrl`: trả về ngay cả khi còn `null` (job resize chưa chạy) giúp **hợp đồng API ổn định** — FE không phải xử lý hai hình dạng response khác nhau tùy thời điểm.
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
   - Thêm property `Categories` vào `IUnitOfWork`/`UnitOfWork`. `ICategoryReadRepository` (Buổi 2) giữ nguyên cho phía Query.
4. **Lớp phòng vệ thứ hai — `CategoryPersistenceExceptionTranslator : IPersistenceExceptionTranslator`:** `DbUpdateException` có `PostgresException { SqlState: "23505", ConstraintName: "IDX_Category_Name" }` → `CategoryNameAlreadyExistsException` → 409 thay vì 500 — phòng tình huống hai Admin tạo trùng tên cùng lúc (race condition) lọt qua bước kiểm tra chủ động. *(Bản kế hoạch trước đặt việc này trong `GlobalExceptionMiddleware`; nay chuyển về Infrastructure — xem giải trình.)*
5. **Ánh xạ HTTP — `CategoryExceptionMappings.cs`:** đăng ký 3 lớp theo cột HTTP ở bước 1.
6. **CQRS — `CreateCategoryCommand`:** Validator `name` **2–100 ký tự** (theo §7.9, **không phải 2–50** như v1.0.0), `description?`, `imageUrl?` URL hợp lệ, `orderIndex?` ≥ 0. Handler **kiểm tra `Name` trùng chủ động** (`unitOfWork.Categories.ExistsByNameAsync`) trước khi ghi → `CategoryNameAlreadyExistsException` → **409 `CATEGORY_NAME_EXISTS`**; sinh slug tự động + hậu tố `-2`,`-3` nếu trùng (`SlugExistsAsync`); `AddAsync` → `SaveChangesAsync()`; trả 201 kèm header `Location: /api/v1/categories/{slug}`.
7. **CQRS — `UpdateCategoryCommand`:** cùng bộ validator; không tìm thấy → `CategoryNotFoundException` → 404; **cũng phải kiểm tra `Name` trùng** (`excludeId = id`, đổi sang tên đã có → 409); **slug KHÔNG đổi** khi đổi tên (tránh chết link đã chia sẻ).
8. **CQRS — `DeleteCategoryCommand`:** `CountActiveRecipesAsync`; nếu > 0 → `CategoryHasRecipesException(count)` → **409 `CATEGORY_DELETE_HAS_RECIPES`**, số lượng vừa nằm trong `detail` vừa nằm trong extension `recipeCount` (FE hiển thị không cần tách chuỗi); nếu = 0 → `category.SoftDelete()`, **không xóa vật lý**.
9. **Cache + API + UI + Test:** **retrofit D-13** — `GetCategoriesQuery.Expiration` đổi `TimeSpan.FromMinutes(60)` → **`30`** theo bảng TTL chuẩn §2.3 (buổi này mới có đường ghi danh mục, nên đây là lúc TTL bắt đầu có ý nghĩa thật); cả 3 command implement `ICacheInvalidator` → xóa `categories:all` và prefix `categories:detail:`. API: `POST /categories`, `PUT /categories/{id:guid}`, `DELETE /categories/{id:guid}` — tất cả `RequireAuthorization("AdminPolicy")` → 403 cho Author. UI `/dashboard/categories` (Admin only): bảng danh sách, modal form RHF+Zod (name, description, `ImageUploader` cho imageUrl, orderIndex), confirm dialog khi xóa, **hiển thị lỗi 409 thân thiện** đọc `recipeCount` ("Danh mục này đang có 7 công thức, hãy chuyển chúng sang danh mục khác trước"). **Integration test:** 201 + `Location`; Author → 403; 409 trùng tên (kiểm tra chủ động); **hai request tạo cùng tên song song bằng `Task.WhenAll` → đúng một 201 và một 409, không có 500** (kiểm chứng bộ dịch `23505` trên PostgreSQL thật); PUT đổi tên → slug giữ nguyên; PUT sang tên đã có → 409; DELETE còn recipe → 409 kèm `recipeCount`; DELETE danh mục rỗng → 204, sau đó `GET /categories/{slug}` → 404 `CATEGORY_NOT_FOUND`; `GET /categories` sau mỗi thao tác ghi thấy dữ liệu mới (cache đã bị xóa).

**Phần 3 – Định hướng & Lý do thiết kế:**
**Kiểm tra `Name` trùng chủ động ở tầng Application** thay vì để ràng buộc UNIQUE của database bắt, vì hai lý do. Thứ nhất về trải nghiệm: dựa vào exception của DB thì người dùng nhận **HTTP 500 "Lỗi hệ thống"** thay vì một thông báo nghiệp vụ rõ ràng, và log bị nhiễu bởi exception không đáng có. Thứ hai về khả năng bảo trì: dò mã lỗi PostgreSQL `23505` là code khó đọc và phụ thuộc vào chi tiết của một DBMS cụ thể. Tuy nhiên **vẫn giữ lớp bắt `23505`** làm phòng vệ thứ hai — kiểm tra chủ động không loại bỏ được hoàn toàn khe hở race condition giữa lúc `SELECT` và lúc `INSERT`.

**Vì sao bắt `23505` ở Infrastructure (translator) chứ không ở middleware như bản kế hoạch trước:** chính lý do ở đoạn trên — *"phụ thuộc chi tiết của một DBMS"* — là lý do nó không được nằm ở tầng API. Middleware muốn đọc `PostgresException` thì project API phải phụ thuộc Npgsql, và mỗi ràng buộc unique mới của mỗi module lại phải sửa **một** file middleware dùng chung (xung đột merge mỗi buổi). Đặt ở translator của module: kiến thức "ràng buộc `IDX_Category_Name` nghĩa là tên danh mục trùng" nằm cạnh repository của chính module đó, middleware chỉ còn thấy một `CategoryNameAlreadyExistsException` bình thường.

**Vì sao repository KHÔNG tích hợp cache Redis:** hệ thống có **một** cơ chế cache duy nhất (NFR-PERF-003, §2.2): Query khai báo `ICacheable` → `CachingBehavior` đọc/ghi Redis; Command khai báo `ICacheInvalidator` → `CacheInvalidationBehavior` xóa khóa **sau khi** handler thành công. Nếu repository cũng tự cache thì có hai tầng cache cho cùng dữ liệu với hai TTL và hai chỗ phải nhớ invalidate — đúng loại lỗi "sửa danh mục xong vẫn thấy tên cũ" mà không test nào bắt được vì nó phụ thuộc thời điểm. Ngoài ra repository phía Command trả entity được EF theo dõi để **sửa**; cache một entity đang được sửa là sai về bản chất (dữ liệu trong cache sẽ khác dữ liệu sắp được lưu).

**Vì sao repository KHÔNG có "PostgreSQL Full-Text Search helper":** FR-SRCH-001 tìm kiếm trên **recipe** (cột generated `SearchVector` của bảng `Recipes`), danh mục không có yêu cầu tìm kiếm toàn văn nào trong SRS. Dev 3 dựng FTS ở **Buổi 4** như kế hoạch — khi đó phần truy vấn tìm kiếm thuộc phía Query (`IRecipeSearchReadRepository` hoặc mở rộng `IRecipeReadRepository`), không thuộc repository ghi của Category.

**Slug không đổi khi đổi tên** là quyết định bảo vệ SEO và người dùng: URL đã được chia sẻ, đã được Google lập chỉ mục, đã nằm trong bookmark. Đổi slug mà không có bảng lịch sử thì mọi link cũ chết ngay lập tức. Hệ thống cũng **không xây bảng lịch sử slug** — xem phân tích ở FR-RCP-004 (Buổi 5) và NFR-SEO-004.

**Soft delete cho Category** (không hard delete) giữ đúng thiết kế `BaseEntity` áp dụng cho toàn hệ thống. Ràng buộc FK `Recipes.CategoryId → Categories.Id` vẫn giữ `ON DELETE RESTRICT` — không phải để chặn thao tác xóa mềm (xóa mềm không đụng tới FK), mà là **lớp bảo vệ cuối cùng** cho mọi thao tác xóa **vật lý** danh mục: hiện FR-JOB-003 chỉ dọn Recipe, nhưng nếu sau này mở rộng sang Category (hoặc có người xóa tay trong DB), database sẽ từ chối xóa một danh mục còn công thức tham chiếu thay vì để lại dữ liệu mồ côi.

**Hệ quả cần biết của unique thường trên `Name`:** danh mục đã xóa mềm **vẫn giữ tên của nó** — tạo lại "Món chính" sau khi đã xóa "Món chính" sẽ nhận 409. Đây là hành vi đúng với ràng buộc DB hiện tại (SRS §7 không yêu cầu partial unique cho `Categories`), và an toàn hơn chiều ngược lại (khôi phục danh mục đã xóa theo quy trình MT-49 sẽ không bao giờ đụng tên với danh mục mới). Nếu nhóm muốn giải phóng tên sau khi xóa thì phải qua CR để đổi sang partial unique như D-3 của Recipe.

Thông báo lỗi 409 hiển thị **số lượng công thức cụ thể** thay vì câu chung chung: Admin cần biết quy mô việc phải làm trước khi quyết định, và con số đó server đã đếm sẵn rồi — không trả về là lãng phí. Trả thêm trong extension `recipeCount` để FE không phải tách số từ một chuỗi tiếng Việt có thể đổi câu chữ.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** bốn ô trong **cùng một bảng FR-CAT-003** không khớp nhau: ô "Điều kiện tiên quyết" ghi *"Name chưa tồn tại trong database"*; ô "HTTP Status Code" ghi *"409 Conflict – Name đã tồn tại"*; nhưng **Luồng chính bước 6 chỉ kiểm tra slug** (trùng thì tự thêm `-2`, `-3`), **không kiểm tra Name**; và Luồng thay thế chỉ có A1 (403) và A2 (422) — **không có nhánh nào dẫn tới 409**. Thêm nữa, validator ghi `Name` 2–50 ký tự trong khi cột DB là `varchar(100)` — lệch 2 lần. FR-CAT-005 ghi "Xóa entity" còn Chương 8 ghi "soft delete".
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-36, MT-37, MT-20.11**) bổ sung **bước kiểm tra Name trùng** vào cả FR-CAT-003 lẫn FR-CAT-004, chốt `Name` **2–100** theo Bảng Giới hạn Chuẩn §7.9, và chốt **soft delete**.
- **Tại sao chọn:** mã 409 ở v1.0.0 là **mã không bao giờ được sinh ra** — tạo danh mục trùng tên "Món chính" lần thứ hai sẽ đi hết luồng (slug `mon-chinh` trùng → tự đổi thành `mon-chinh-2` → `AddAsync` → `SaveChangesAsync`), rồi mới bị PostgreSQL chặn vì `Name` là UNIQUE → HTTP 500. Một mã lỗi được đặc tả mà không có đường nào sinh ra nó là dấu hiệu đặc tả chưa được đọc ngang qua bốn ô. Về giới hạn độ dài: nguyên tắc dài hạn chốt trong SRS §7.9 là **validator và cột DB phải BẰNG NHAU**, không phải "validator chặt hơn cho an toàn" — khi hai con số lệch nhau, không ai biết con số nào là yêu cầu thật, QA không biết lấy đâu làm chuẩn viết test, và dữ liệu nhập qua seeding sẽ lọt qua validator rồi vẫn nằm được trong DB.
- **SRS FR-CAT-003 A4 ghi "`GlobalExceptionMiddleware` dịch mã `23505`":** kế hoạch giữ đúng **hành vi** SRS yêu cầu (409 `CATEGORY_NAME_EXISTS`, không bao giờ 500) nhưng đặt việc dịch ở translator của Infrastructure, middleware nhận exception đã dịch. Đây là chi tiết hiện thực, không đổi hợp đồng API.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314291_DoanHongTien_buoiso3`.
```
feat(category): add category domain exceptions, category repository and complete create & update admin category APIs
```
*(Commit gồm cả `DELETE /categories/{id}` — FR-CAT-005 — dù message nêu create & update theo phân công ngày 29/09.)*

## DEV 4 — Nền tảng Exception / Repository / Middleware, Health Checks, Hangfire Dashboard & Integration Test Harness

**Phần 1 – Chức năng hoàn thành trong buổi:**
**Commit nền kiến trúc** (lớp gốc `DomainException`, `IRepository<T>`/`IUnitOfWork`/`UnitOfWork`, nâng cấp `GlobalExceptionMiddleware` + `ExceptionStatusMap`) + **`FR-OBS-001` phần endpoint** (`/health`, `/health/live`, `/health/ready` — kéo từ Buổi 5 lên) + hoàn tất `FR-JOB-001` — **Hangfire Dashboard `/hangfire` được bảo vệ** (bản thân job email đã chạy thật từ Buổi 2, `BAO_CAO_BUOI_2.md` §4.6) + **Integration Test Harness** dùng chung cho cả nhóm (NFR-MAINT-002) + dẫn **commit nền D-11**.

**Trạng thái (29/09/2026):** D-11, Hangfire Dashboard, Integration Test Harness và trả nợ test Buổi 2 — ✅ **đã xong** trong commit `67d29c0` (bước 5 → 8, `BAO_CAO_KET_QUA_BUOI_3.md`). Bước 1 → 4 là phần bổ sung theo yêu cầu ngày 29/09 — ✅ **đã code xong và kiểm chứng ngày 29/09/2026** (117 test xanh, 1 skip có chủ đích; `/health` kiểm chứng trên `docker compose` với MinIO/Redis sập — `BAO_CAO_KET_QUA_BUOI_3.md` §11), chờ commit trên nhánh `2312755_NguyenThangThieng_buoiso3` rồi vào `develop` ở mốc M1.

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
   - Mọi response lỗi có `traceId` (do `IProblemDetailsService` tự thêm — không phải code riêng); Buổi 6 bổ sung `CorrelationId` khi có `CorrelationIdMiddleware`.
   - **Test kiến trúc** (`ArchitectureTests`):
     - (a) mọi lớp `DomainException` **không trừu tượng** trong assembly Domain phải có ánh xạ tường minh trong `ExceptionStatusMap` — quên đăng ký là test đỏ, không âm thầm rơi về 400;
     - (b) exception của module nằm đúng thư mục `Exceptions/{Module}`;
     - (c) interface repository không trả `IQueryable`;
     - (d) `ErrorCodes` khớp **đúng** Phụ lục B — test đọc thẳng bảng Phụ lục B trong file SRS mới nhất ở `SPEC/` (không dùng danh sách chép tay), nên SRS đổi mà code không đổi (hoặc ngược lại) là test đỏ; mã ngoài SRS duy nhất được miễn là `AUTH_USERNAME_EXISTS`, ghi rõ là tạm thời chờ D-1;
     - (e) `IRepository<T>` không có phương thức xóa cứng.
   - **Test hợp đồng Problem Details** (integration, `ProblemDetailsContractTests`): harness đăng ký endpoint **chỉ tồn tại trong test** (qua `IStartupFilter` của `CulinaryBlogApiFactory`) để ném từng loại lỗi. Kiểm tra với mỗi loại:
     - `Content-Type: application/problem+json`;
     - đúng `status` / `type`;
     - có `errors{}` với lỗi validation;
     - lỗi 500 không chứa stack trace.

     Thêm hai kịch bản thật:
     - hai `DbContext` cùng sửa một `Category` với `RowVersion` cũ → `UnitOfWork` ném `ConcurrencyConflictException`;
     - `ExecuteInTransactionAsync` ném lỗi sau lần `SaveChanges` đầu tiên → **không có dòng nào được lưu** (rollback thật trên PostgreSQL).
4. **Health Checks — FR-OBS-001 (phần endpoint):**
   - **Đăng ký check** trong `AddInfrastructure`, dùng hai package đã có trong `CulinaryBlog.Infrastructure.csproj` từ Buổi 2:
     - `AddNpgSql(…, name: "database", tags: ["ready"])`;
     - `AddRedis(…, name: "redis", tags: ["ready"])` — dùng cùng chuỗi kết nối Redis của cache, thêm `connectTimeout=2000` để Redis chết thì check báo lỗi trong ≤ 3 giây;
     - `AddCheck<MinioHealthCheck>("minio", failureStatus: HealthStatus.Degraded, timeout: 3s)` — `MinioHealthCheck : IHealthCheck` tự viết bằng `AWSSDK.S3`, kiểm tra **đúng bucket ứng dụng dùng** có tồn tại (không chỉ "MinIO có trả lời"), vì package `AspNetCore.HealthChecks.Minio` mà SRS nêu **không tồn tại** (`CONG_NGHE_VA_PHIEN_BAN.md` §6.4).
   - **Map endpoint** trong `Program.cs` — cả ba đều `AllowAnonymous`, không qua Output Cache:
     - `/health` → mọi check, JSON `{ status, entries }` qua `UIResponseWriter.WriteHealthCheckUIResponse` (package `AspNetCore.HealthChecks.UI.Client` đã có), **503 khi Unhealthy**;
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
7. ✅ **Integration Test Harness — `tests/CulinaryBlog.API.IntegrationTests`:** project này **đã được tạo khung ở Buổi 2** (đã tham chiếu `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `Microsoft.AspNetCore.Mvc.Testing`, `coverlet`) nhưng **chưa có file test nào** — buổi này lấp đầy nó, bổ sung package `Respawn` và `public partial class Program;` ở API để `WebApplicationFactory` truy cập được. `CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, ghi đè connection string, chạy migration, tắt Hangfire server (`Hangfire:ServerEnabled=false`) và thay `IBackgroundJobClient`/`IFileStorageService` bằng bản giả ghi lại lời gọi. Kèm `Respawn` để dọn DB giữa các test và helper `CreateClientAs(role)` phát JWT thật qua `ITokenService`.  Testcontainers **không chạy `docker/postgres/init.sql`** → mọi extension/DDL mà code cần phải nằm **trong migration** (điều kiện tiên quyết của D-18, Buổi 4).
8. ✅ **Trả nợ test Buổi 2:** viết integration test cho toàn bộ endpoint Buổi 2 (`BAO_CAO_BUOI_2.md` §6 ghi tồn đọng): register/login (201/400/401/409/423), recipes list/detail, categories list/detail, files upload/delete (201/400/401/403/204). Thêm sẵn **test tái hiện lỗ hổng MT-34** (Admin gọi `GET /recipes` rồi Guest gọi lại cùng URL) đánh dấu `[Fact(Skip = "Lỗ hổng đã biết — retrofit D-4 ở Buổi 4")]` *(ban đầu ghi Buổi 6; D-4 kéo lên Buổi 4 ngày 29/09/2026)* để lỗ hổng được **ghi thành test ngay hôm nay**, không phụ thuộc trí nhớ.
9. **Kiểm chứng:**
   - `http://localhost/hangfire` qua Nginx → hỏi Basic Auth → vào được dashboard, thấy job `WelcomeEmailJob` Succeeded; gọi thẳng `http://localhost:5000/hangfire` (bỏ qua Nginx) → **401**. Giảm `QueuePollInterval` xuống 1 giây ở môi trường dev (tồn đọng §6 của báo cáo Buổi 2).
   - `curl http://localhost/health` → `Healthy` kèm 3 entry; `docker compose stop minio` → `/health` báo `Degraded` còn `/health/ready` **vẫn 200**; `docker compose stop redis` → `/health/ready` **503**.
   - `dotnet test` chạy toàn bộ unit + architecture + integration test xanh trên máy có Docker.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Vì sao Dev 4 làm phần nền, còn phần theo module do chính chủ module làm:** lớp gốc exception, `IRepository<T>`, `UnitOfWork`, middleware là thứ **không thuộc module nào** — nếu ba dev cùng viết thì sẽ có ba kiểu. Ngược lại, "tên danh mục trùng là lỗi gì, trả mã gì" là kiến thức của Dev 3, và chính Dev 3 sẽ dùng lại nó ở Buổi 4–7. Chia như vậy giữ đúng mô hình **Feature Ownership** của §1: mỗi dòng code của module có một chủ duy nhất từ Buổi 2 đến Buổi 8, và phần việc của bốn người tương đương nhau (mỗi dev module: 3–6 exception + 1 repository + 1 file ánh xạ + ≥ 2 API; Dev 4: phần nền + middleware + 3 API health, cộng với dashboard và harness đã xong).

**Domain Exception không mang mã HTTP:** Domain là tầng lõi, dùng lại được cho một job Hangfire, một CLI, hay một bài test đơn vị — những nơi không có khái niệm HTTP. Nếu `CategoryHasRecipesException` tự khai báo `StatusCode = 409` thì Domain đã biết mình đang chạy sau một web API. Để exception chỉ mang **mã nghiệp vụ** (`CATEGORY_DELETE_HAS_RECIPES`) và để tầng API quyết định mã HTTP, đúng hướng phụ thuộc của Clean Architecture (NFR-MAINT-004).

**Ánh xạ theo kiểu exception, mỗi module một file, quét tự động:** ba phương án được cân nhắc. **(A) Một `switch` lớn trong middleware** — đơn giản nhưng cả 3 dev cùng sửa một file mỗi buổi → xung đột merge liên tục, và quên thêm một nhánh là lỗi âm thầm rơi về 400. **(B) Ánh xạ theo chuỗi `Code`** — ngắn gọn, nhưng SRS có chỗ dùng **cùng một mã với hai mã HTTP** (`AUTH_GOOGLE_TOKEN_INVALID`: 400 và 401 — xem Dev 1 Phần 4), và đổi tên chuỗi không được trình biên dịch kiểm tra. **(C) Ánh xạ theo kiểu, một file/module, quét tự động (đã chọn)** — trình biên dịch kiểm tra tên lớp, mỗi dev chỉ sửa file của mình, và test kiến trúc bảo đảm không lớp nào bị quên.

**Giữ `GlobalExceptionMiddleware` thay vì chuyển sang `IExceptionHandler`:** hai cơ chế cho kết quả như nhau (cùng bắt exception và ghi Problem Details qua `IProblemDetailsService`). Middleware đã chạy và có test từ Buổi 2, và SRS §6.2 gọi đích danh tên `GlobalExceptionMiddleware`. Chuyển cơ chế lúc này chỉ đổi nơi đăng ký và phải viết lại test, không thêm khả năng nào.

**`IRepository<T>` cố ý nhỏ — không có `Remove`, không trả `IQueryable`, không có `Update`:**
- **Không `Remove`:** mọi xóa trong hệ thống là **xóa mềm** (NFR-REL-003); xóa vật lý chỉ có một nơi hợp lệ là `PermanentPurgeJob` (Buổi 4, Infrastructure). Có `Remove` trong interface chung thì sớm muộn sẽ có handler gọi nhầm và mất dữ liệu **không khôi phục được**. Xóa mềm đi qua phương thức Domain (`category.SoftDelete()`).
- **Không `IQueryable`:** mở `IQueryable` cho tầng Application là để lọt chi tiết EF lên trên (`Include`, `AsSplitQuery`, `IgnoreQueryFilters`) — mỗi handler tự quyết định nạp gì, và repository chỉ còn là một lớp bọc rỗng. Truy vấn đặc thù là phương thức có tên trên repository của module (`GetByIdWithDetailsAsync`, `ExistsByNameAsync`).
- **Không `Update`:** entity lấy ra đã được EF theo dõi; đổi thuộc tính qua phương thức Domain rồi `SaveChangesAsync()` là đủ. Một `Update(entity)` sẽ đánh dấu **mọi cột** là đã sửa, kể cả cột người khác vừa đổi.

**Repository nằm trên Unit of Work (`unitOfWork.Categories`) thay vì tiêm từng repository riêng:** đây là đúng cách SRS mô tả luồng xử lý (`_unitOfWork.Categories.ExistsByNameAsync(...)` ở FR-CAT-003, `_unitOfWork.Recipes.AddAsync(...)` ở FR-RCP-003), và nó làm rõ ranh giới transaction: mọi repository lấy từ cùng một `IUnitOfWork` dùng chung một `DbContext`, nên một lần `SaveChangesAsync()` lưu tất cả hoặc không lưu gì. Chi phí là mỗi dev thêm một dòng vào `IUnitOfWork` — chấp nhận được vì chỉ là thêm dòng.

**Transaction phải đi qua execution strategy:** `CulinaryBlogDbContext` bật `EnableRetryOnFailure` từ Buổi 2 (Postgres khởi động chậm hơn API). Với cấu hình đó, EF Core **ném `InvalidOperationException`** nếu code tự `BeginTransaction` bên ngoài `IExecutionStrategy` — vì khi mất kết nối giữa chừng, EF không biết phải thử lại từ đâu. `ExecuteInTransactionAsync` gói đúng mẫu mà `DatabaseSeeder` đã chạy ổn định: `ChangeTracker.Clear()` ở đầu mỗi lần thử, transaction bên trong, commit ở cuối. Hệ quả mà mọi dev phải biết (ghi ngay trên chú thích của phương thức): **thao tác truyền vào có thể bị chạy lại**, nên phải tự đọc dữ liệu bên trong nó — Dev 2 (đổi ảnh chính, B3), Dev 1 (rotation, B4), Dev 4 (purge job, B4) đều dùng đúng một cách này.

**Health endpoints kéo lên Buổi 3 thay vì giữ ở Buổi 5:** (1) FR-OBS-001 là mức **M** và vốn giao cho Dev 4 — chuyển buổi không đổi ranh giới phân công, không thêm yêu cầu ngoài SRS; (2) mục Verification §7 bắt `curl /health` trả `Healthy` ở cuối **mọi** buổi, nhưng tới trước Buổi 3 code **chưa có endpoint health nào** — kế hoạch đang tự mâu thuẫn, kéo lên là sửa luôn chỗ đó; (3) hai package NpgSql/Redis health check đã được cài từ Buổi 2 mà chưa dùng. Phần gắn Docker `healthcheck:` + `proxy_next_upstream` + giao diện vẫn ở Buổi 5 vì nó cần sửa `Dockerfile` (cài `curl`) và `docker-compose.yml`, và nhóm đang dùng `/health/ready` ở Buổi 3 chỉ để kiểm chứng. FR-OBS-001 trải qua hai buổi (xem §5) — phần giao ở mỗi buổi đều dùng được ngay.

**MinIO chỉ làm `/health` thành `Degraded`, không làm `/health/ready` thất bại:** mất MinIO thì ảnh không tải lên được nhưng người dùng vẫn đọc và viết công thức được — đó là **suy giảm**, không phải **ngừng phục vụ**. Loại instance khỏi pool vì lý do này là phản ứng thái quá (giải trình đầy đủ ở Buổi 5 Dev 4).

**Vì sao dựng integration test harness ngay Buổi 3 — đây là quyết định quan trọng nhất của Dev 4 trong buổi này.** Definition of Done của nhóm (§2.4) đòi `dotnet test` xanh ở **mọi** commit, NFR-MAINT-002 đòi *"mọi endpoint có ít nhất 1 happy path + 1 error case"*, và các buổi sau liên tục yêu cầu test kiểu *"10 request `PUT` đồng thời chỉ 1 thành công"*. Kế hoạch 6 buổi cũ đặt harness ở Buổi 6–7 (theo cách đánh số của kế hoạch đó) — nghĩa là 5 buổi liền **không ai viết được integration test**, lỗi tích tụ rồi dồn cục vào cuối dự án. Buổi 2 đã làm sớm FR-JOB-001, nên phần thời gian trống ra của Dev 4 được dùng đúng vào chỗ có đòn bẩy lớn nhất: một hạ tầng mà cả 4 dev dùng mỗi ngày. Buổi này harness còn là nơi **kiểm chứng chính phần nền kiến trúc**: rollback transaction và dịch lỗi xung đột chỉ chứng minh được trên PostgreSQL thật.

**Testcontainers thay vì EF Core In-Memory:** provider In-Memory **không có** ràng buộc UNIQUE, không có FK cascade, không có `tsvector`, không có partial index, không có concurrency token thật, **không có transaction** — tức là đúng những thứ cần kiểm chứng nhất (MT-03, MT-05, MT-25, MT-09, và `ExecuteInTransactionAsync` của buổi này) đều biến mất. Test xanh trên In-Memory rồi vỡ trên PostgreSQL thật là kịch bản tệ nhất vì nó tạo ra **niềm tin sai**. Testcontainers chạy đúng `postgres:16-alpine` như production.

**Hai lớp bảo vệ cho dashboard, không phải một:** Basic Auth ở Nginx là lớp chính; header `X-Hangfire-Gate` là lớp thứ hai cho tình huống **có đường vào API không đi qua Nginx** — ở môi trường dev, cổng `5000:8080` của `api` được mở ra ngoài (§6.5), nên ai gõ thẳng `:5000/hangfire` sẽ vượt qua Basic Auth nếu chỉ có một lớp. Secret chỉ Nginx biết, được Nginx gắn vào **sau khi** người dùng qua Basic Auth. Chi phí: khoảng 15 dòng code; lợi ích: dashboard (vốn có quyền xóa/chạy lại mọi job) không bao giờ lộ ra dù cấu hình cổng thay đổi thế nào.

**Không làm lại những gì Buổi 2 đã xong:** PostgreSQL storage, `IEmailService`/`MailKitEmailService`, `WelcomeEmailJob` với backoff 1′/5′/30′, seam `IWelcomeEmailScheduler` → `HangfireWelcomeEmailScheduler` đều đã chạy thật (báo cáo Buổi 2 §2.1: *"Container `hangfire` gửi thành công → Mailhog"*). Giao lại việc đã hoàn thành chỉ vì kế hoạch cũ ghi vậy là lãng phí nguồn lực và dễ làm hỏng thứ đang chạy. Cũng vì vậy, `POST /files/upload` (Buổi 2) **không** được tính là API của Buổi 3.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) v1.0.0 quy định Hangfire Dashboard *"chỉ Admin, policy-protected"* trong khi hệ thống xác thực bằng **JWT Bearer stateless** và mục 5.2 ghi rõ *"không dùng cookie để tránh CSRF"*. (b) FR-JOB-001 mô tả email chào mừng chứa *"link kích hoạt email (nếu cần)"* trong khi **không có FR nào** đặc tả luồng xác nhận email — cần rà lại template của `WelcomeEmailJob` Buổi 2 để chắc chắn không còn link này. (c) Kế hoạch 6 buổi cũ tự mâu thuẫn: Definition of Done đòi test ở mỗi commit, nhưng integration test harness lại xếp ở Buổi 6 của kế hoạch đó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-39, MT-21**) chốt: Dashboard bảo vệ bằng **HTTP Basic Auth tại Nginx**; **bỏ link kích hoạt email**. Kế hoạch bổ sung lớp thứ hai (header gate) và đưa harness lên Buổi 3.
- **Tại sao chọn:** Hangfire Dashboard là **trang HTML mở trực tiếp trong trình duyệt** — khi Admin gõ `https://domain.com/hangfire`, trình duyệt gửi một request điều hướng thông thường, **không có cách nào đính kèm header `Authorization: Bearer`**. Mà SRS lại loại bỏ cookie. Kết quả ở v1.0.0: `IDashboardAuthorizationFilter` luôn thấy người dùng ẩn danh → **Admin không bao giờ vào được dashboard**, dù đây là quyền đã liệt kê ở mục 2.3. Ba phương án được cân nhắc: **(A)** chặn `/hangfire` khỏi internet, chỉ vào qua SSH tunnel — an toàn nhất nhưng bất tiện khi demo; **(B)** Basic Auth ở Nginx — ba dòng cấu hình, mở được từ trình duyệt, **không đụng gì tới JWT của ứng dụng**, đánh đổi là phải quản lý một bộ thông tin đăng nhập riêng; **(C)** cấp cookie riêng cho dashboard — mở lại cánh cửa cookie mà mục 5.2 vừa đóng, phải xử lý CSRF, thêm code chỉ để phục vụ một trang quản trị. Chọn **B cho môi trường demo/nộp bài**, khuyến nghị **A cho production thật**. Về link kích hoạt email: giữ nó sẽ tạo lời hứa suông với người dùng (bấm vào không có gì xảy ra) — tệ hơn là không có gì.
- **Vị trí interface repository — SRS §6.2 đặt ở Domain, kế hoạch đặt ở Application (quyết định 29/09/2026, errata E-9 ở §4.5):**
  - **Lý do đặt ở Application:** repository là **cổng ra hạ tầng** mà tầng use case cần, không phải khái niệm nghiệp vụ; giữ Domain chỉ gồm entity + exception + value object làm lõi dễ test nhất. Code Buổi 2 cũng đã đặt `IUnitOfWork` và các read repository ở Application — đặt interface ghi ở Domain sẽ chia đôi các cổng truy cập dữ liệu sang hai tầng.
  - **Đánh đổi:** entity Domain không gọi được repository — điều vốn không nên làm.
  - **Việc cần làm:** SRS §6.2 và §2.1 của kế hoạch này được cập nhật theo quyết định, không để hai tài liệu nói hai điều.
- **Phụ lục B thiếu mã cho hai tình huống mà middleware phải trả:** 404 user không tồn tại (FR-AUTH-008 A3) và 409 xung đột `RowVersion` của entity ngoài Recipe (mọi `BaseEntity` đều có concurrency token — §2.2). Theo đúng tiền lệ MT-46, bổ sung qua **CR-2026-04** (§4.5), không tự đặt mã ngoài SRS.

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
   - test kiến trúc xanh: mọi exception cụ thể có ánh xạ, Domain không tham chiếu assembly ngoài.
2. **Yêu cầu 2:**
   - handler ghi dữ liệu chỉ phụ thuộc `IUnitOfWork` / repository, không handler nào tham chiếu `CulinaryBlogDbContext`;
   - test rollback transaction và test dịch xung đột xanh trên PostgreSQL thật.
3. **Yêu cầu 3:**
   - `ProblemDetailsContractTests` xanh cho 400 / 401 / 403 / 404 / 409 / 423 / 500 / 502 / 503 — đều `application/problem+json`, đúng `type`;
   - lỗi 500 không lộ stack trace.
4. **Yêu cầu 4:** 12 endpoint mới (bảng đầu buổi), mỗi endpoint có ≥ 1 happy path + ≥ 1 error case trong integration test; test tay qua Scalar `/scalar` và UI.
5. **Chức năng:**
   - không còn chuỗi `Status422` / `status === 422` trong code;
   - form đăng ký hiện lỗi từng ô với 400;
   - 2 email cùng prefix sinh `UserName` có hậu tố số;
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
- **Giao diện dời sang Buổi 5 → 7:** các bước giao diện của Buổi 4 cũ (wizard bước 3–4, trang tìm kiếm, interceptor refresh, D-12) chuyển sang Buổi 5.
- **Chia endpoint theo năng lực** để cân bằng:

| Dev | Nhóm việc Buổi 4 | Endpoint **mới** | Endpoint **sửa hợp đồng** | Việc nền kèm theo |
|---|---|---|---|---|
| **Dev 1** | Phiên & tài khoản — FR-AUTH-004, 006, 007, 008, 009 | 8 | — | D-2, `UseForwardedHeaders`, claim `sid` |
| **Dev 2** | Nội dung công thức — FR-RCP-009, 010, 004 | 8 | `POST /recipes` (mảng `steps?`/`ingredients?`) | D-16 |
| **Dev 3** | Truy vấn công khai & riêng tư — FR-SRCH-001 → 004, FR-RCP-011, MT-48, CR-2026-04 (d) | 4 | `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` | D-18, D-4, D-5, D-6, D-10 (phía API) |
| **Dev 4** | Vòng đời công thức & job nền — FR-RCP-005, 006, 007, FR-JOB-002, 003 | 5 | — | D-3, D-15, test bề mặt API 45/45 |

**Kết quả sau buổi: 20 (B2 + B3) + 25 = 45/45 endpoint.**

**Phân vai "chủ API" và "chủ giao diện" từ Buổi 4.** Vai trò chủ giao diện theo module của §1 giữ nguyên. Hợp đồng giữa hai bên là SRS Chương 8 + integration test của chủ API. Chủ giao diện không sửa handler; thấy API sai thì báo chủ API.

| FR | Chủ API (Buổi 4) | Chủ giao diện | Buổi làm giao diện |
|---|---|---|---|
| FR-RCP-005 publish/unpublish | Dev 4 | Dev 2 | 5 |
| FR-RCP-007 xóa mềm | Dev 4 | Dev 2 | 6 |
| FR-RCP-006 archive/unarchive | Dev 4 | Dev 2 | 7 |
| FR-RCP-011 `/recipes/mine` + `/recipes/mine/{id}` | Dev 3 | Dev 2 | 5 (trang sửa) · 6 (dashboard) |
| `GET /recipes/sitemap` (MT-48) | Dev 3 | Dev 3 | 7 |

**Vì sao chia theo năng lực chứ không ép theo module ở buổi này:**
- **Tải cân bằng:** mỗi dev 4–8 endpoint cộng một phần việc nền tương đương.
- **Mỗi nhóm việc là một khối gắn kết, không chia vụn:**
  - Dev 3 đã là chủ phần **đọc** công thức — bộ lọc, sắp xếp, phân trang của FR-SRCH vốn áp lên `GET /recipes` — nên nhận luôn các truy vấn riêng tư (`/mine`, `/mine/{id}`) và sitemap: cùng `RecipeFilterSpec`, cùng quy tắc cache.
  - Dev 4 đã là chủ **job dọn dữ liệu** FR-JOB-003 (xóa vĩnh viễn công thức đã xóa mềm > 30 ngày), nên nhận trọn **vòng đời** công thức: xuất bản → lưu trữ → xóa mềm → dọn. Máy trạng thái và xóa mềm nằm chung một người, **không bị cắt đôi** giữa hai dev.
- **Giảm phụ thuộc chéo:**
  - Dev 3 tự làm endpoint sitemap cho sitemap Next.js của chính mình (Buổi 7), không còn phải chờ Dev 2.
  - Dev 3 tự sửa `GetRecipesQuery` (trước đây là điểm "sửa code của Dev 2" ở Buổi 5 — §6).

**Phạm vi của "hoàn thành tất cả API endpoints" — cái gì KHÔNG thuộc Buổi 4:** rate limiting (NFR-SEC-003, mã 429 `RATE_LIMIT_EXCEEDED`) vẫn ở **Buổi 6 — Dev 1**.
- Rate limiting là *chính sách middleware áp lên mọi endpoint*, không phải một endpoint.
- Nó phụ thuộc IP thật từ `UseForwardedHeaders` (bật ở Buổi 4), và phải kiểm chứng cùng CSP, route guard trong một lượt gia cố bảo mật.
- Mọi endpoint của Buổi 4 vẫn trả đủ các mã khác trong hợp đồng Chương 8; riêng dòng 429 của hợp đồng được đáp ứng ở Buổi 6 mà không cần sửa endpoint nào.

Tương tự, composite index + `EXPLAIN ANALYZE` (NFR-PERF-004) ở Buổi 5 là tối ưu hiệu năng, không đổi hợp đồng API.

**Tổ chức code để 4 người cùng sửa module Recipe mà không xung đột:**
- **`Recipe` thành `partial class`:** `Recipe.cs` (Dev 2 — nội dung) và `Recipe.Lifecycle.cs` (Dev 4 — trạng thái, xóa mềm).
- **Endpoint của `/recipes` tách ba file cùng gắn vào một `MapGroup("/recipes")`:**
  - `RecipesEndpoints.cs` — Dev 2: ghi nội dung;
  - `RecipeQueryEndpoints.cs` — Dev 3: mọi `GET`;
  - `RecipeLifecycleEndpoints.cs` — Dev 4: publish/unpublish/archive/unarchive/delete.
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
   - Entity `RefreshToken` **đã có** `IsActive(DateTime utcNow)` (từ Buổi 2) và `Revoke(DateTime utcNow, string? replacedByTokenHash)` (từ Buổi 3 — logout); buổi này dùng lại, không thêm phương thức kiểm tra thứ hai.
   - `IUserRepository` (Buổi 3) thêm `GetRefreshTokenByHashAsync(tokenHash)` (tra **không** theo `UserId` — lúc refresh chưa có access token hợp lệ), `RevokeAllRefreshTokensAsync(userId, now)`, `GetActiveRefreshTokensAsync(userId, now)` và `GetRefreshTokenByIdAsync(userId, id)`.
   - `InvalidTokenException` (Buổi 3) thêm hai factory `RefreshTokenExpired()`, `RefreshTokenRevoked()` — cùng lớp, cùng ánh xạ 401, không cần sửa middleware.
2. **CQRS — `RefreshTokenCommand` (cây quyết định đầy đủ):**
   - Hash chuỗi nhận được → tra `TokenHash`. Không thấy → `InvalidTokenException` → **401 `AUTH_TOKEN_INVALID`**.
   - `ExpiresAt <= UtcNow` → `InvalidTokenException.RefreshTokenExpired()` → **401 `AUTH_REFRESH_TOKEN_EXPIRED`**.
   - **`RevokedAt != null` → REUSE DETECTED:**
     - gọi `RevokeAllRefreshTokensAsync(token.UserId)` (**revoke toàn bộ token family**) và `SaveChangesAsync()` **trước khi** ném lỗi;
     - ghi `Log.Warning("SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {Ip}")`;
     - rồi `InvalidTokenException.RefreshTokenRevoked()` → **401 `AUTH_REFRESH_TOKEN_REVOKED`**.
   - `user.IsActive == false` → `AccountDisabledException` → **403 `AUTH_ACCOUNT_DISABLED`** *(bước này khiến mã lỗi ở Phụ lục B thực sự được sinh ra — xem MT-22)*.
   - Hợp lệ → trong **một transaction** (`unitOfWork.ExecuteInTransactionAsync` — nạp lại token bên trong thao tác vì execution strategy có thể chạy lại): revoke token cũ với `ReplacedByTokenHash = SHA256(newToken)`, tạo bản ghi token mới, phát `AuthResponseDto`.
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
   app.UseForwardedHeaders();   // đặt ĐẦU pipeline — trước Authentication và (Buổi 6) RateLimiter
   ```
   ⚠️ Dùng **`KnownIPNetworks`** + `System.Net.IPNetwork`, **không dùng `KnownNetworks`** + `Microsoft.AspNetCore.HttpOverrides.IPNetwork` như nhiều tài liệu cũ — hai API này đã bị đánh dấu **obsolete trên .NET 10**, và repo bật `TreatWarningsAsErrors` nên dùng chúng là **gãy build**. `CreatedByIp` của mọi refresh token (kể cả token phát lúc đăng ký/đăng nhập từ Buổi 2) từ đây ghi đúng IP người dùng.
5. **FR-AUTH-006/007 — `GetCurrentUserQuery`, `UpdateProfileCommand`** *(kéo từ Buổi 5)*:
   - **Xem hồ sơ:** dùng `ICurrentUser` (Buổi 2) lấy `UserId`; trả `UserDto { id, email, displayName, avatarUrl, bio, roles }`.
     - **Không bao gồm** `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `UserName`, `emailConfirmed`.
     - Token hợp lệ nhưng user không còn tồn tại → **401** (không phải 404 — MT-33.9).
   - **Cập nhật hồ sơ:** PATCH partial `{ displayName?, avatarUrl?, bio? }`.
     - Validator: `displayName` **2–100**, `bio` ≤ **1000**, `avatarUrl` là URL ≤ 500 ký tự **và phải trỏ về đúng bucket MinIO của hệ thống** (`MinioOptions.PublicBaseUrl`).
     - **`Email` và `UserName` không cho đổi** ở endpoint này.
     - Ghi qua `IIdentityService` (Buổi 3 — ghi user luôn qua `UserManager`).
6. **FR-AUTH-008 — `GetUsersQuery`, `SetUserStatusCommand { UserId, IsActive, Reason? }`** *(kéo từ Buổi 7)*, chỉ Admin (`AdminPolicy`):
   - **Danh sách:** phân trang `page`, `pageSize`, `search?` (khớp một phần email/displayName), `isActive?` → `PagedResult<UserAdminDto { id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }>`, **không cache**. Truy vấn đọc qua `IUserRepository`.
   - **Khóa/mở khóa:**
     - Admin tự khóa chính mình → **403** (MT-52);
     - user không tồn tại → `UserNotFoundException` (Buổi 3) → **404 `AUTH_USER_NOT_FOUND`** (CR-2026-04);
     - khi `isActive = false`: gán cờ qua `IIdentityService` **và** gọi `unitOfWork.Users.RevokeAllRefreshTokensAsync(userId, now)` trong cùng một transaction — đây chính là "force revoke";
     - ghi audit log `Log.Warning("ADMIN ACTION: user {TargetUserId} deactivated by {AdminId}, reason: {Reason}")`.
7. **FR-AUTH-009 — `GetMySessionsQuery`, `RevokeSessionCommand`, `RevokeAllSessionsCommand`** *(kéo từ Buổi 7)*:
   - **Liệt kê** refresh token còn hiệu lực của người gọi → `SessionDto { id, createdAt, createdByIp, expiresAt, isCurrent }`, với `isCurrent = (id == claim sid)`; **không bao giờ trả `TokenHash`**.
   - **Thu hồi một phiên:** tìm theo `id` **và** `UserId == currentUserId`. Phiên của người khác trả **404** (không phải 403) để không lộ id; phiên đã thu hồi rồi vẫn trả 204.
   - **Thu hồi tất cả:** revoke mọi phiên, kể cả phiên hiện tại.
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

9. **Integration test** (harness Buổi 3):
   - **Refresh:** refresh hợp lệ → cặp token mới, token cũ bị revoke; dùng lại token cũ sau khi rotate → cả family bị revoke + 401 `AUTH_REFRESH_TOKEN_REVOKED`; token hết hạn → 401 `AUTH_REFRESH_TOKEN_EXPIRED`; request đi qua `X-Forwarded-For` từ mạng tin cậy → `CreatedByIp` ghi đúng IP đó.
   - **Hồ sơ:** `GET /auth/me` không có trường nhạy cảm nào trong JSON; PATCH với `avatarUrl` trỏ ra domain ngoài → 400.
   - **Quản lý tài khoản:** Author gọi `/users` → 403; khóa user → refresh token của họ trả **403 `AUTH_ACCOUNT_DISABLED`**; Admin tự khóa mình → 403; user không tồn tại → 404 `AUTH_USER_NOT_FOUND`.
   - **Phiên:** user A thu hồi phiên của user B → **404**; JSON của `/auth/sessions` không chứa `tokenHash`; `isCurrent` đúng với claim `sid`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Token Rotation** (mỗi lần refresh là cấp cặp mới và thu hồi cặp cũ) biến refresh token từ một *bí mật dài hạn 7 ngày* thành một *bí mật dùng một lần*. Nếu không rotate, kẻ trộm được token sẽ dùng thoải mái suốt 7 ngày mà hệ thống không có cách nào biết.

**Reuse Detection** là phần khiến rotation trở nên có giá trị thật. Sau khi rotate, token cũ **không còn ai có lý do chính đáng để dùng nữa**. Nếu một token đã revoke bất ngờ được sử dụng, chỉ có hai khả năng:
- token đã bị đánh cắp và kẻ trộm đang dùng;
- chính người dùng đang dùng token mà kẻ trộm đã rotate mất.

**Cả hai trường hợp đều có nghĩa là tài khoản đã bị xâm phạm**, nên phản ứng đúng là revoke toàn bộ family, buộc đăng nhập lại. Đây là cơ chế duy nhất trong thiết kế JWT stateless cho phép **phát hiện** việc token bị đánh cắp, thay vì chỉ chờ nó hết hạn. Revoke cả family phải được **lưu trước khi ném lỗi** — ném trước thì request kết thúc mà lệnh revoke chưa bao giờ được ghi xuống DB.

**Vì sao chỉ lưu hash:** raw token **không bao giờ** được ghi vào database; DB chỉ chứa `TokenHash = SHA-256(raw)`. Nếu database bị lộ, kẻ tấn công chỉ có trong tay các chuỗi hash — **không đảo ngược được thành token dùng được**. Không dùng bcrypt/Argon2 vì token là chuỗi ngẫu nhiên 256-bit, không có nguy cơ dictionary attack; SHA-256 là đủ và nhanh hơn nhiều.

**Vì sao `UseForwardedHeaders` bật ở buổi này:** chính buổi này là lúc mỗi lần refresh ghi thêm một bản ghi token kèm `CreatedByIp`, và danh sách phiên (FR-AUTH-009, cũng làm buổi này) hiển thị IP đó cho người dùng. Để muộn hơn nghĩa là ghi **IP của container Nginx** vào cột audit — dữ liệu sai được ghi vĩnh viễn.

**Vì sao gom FR-AUTH-006/007/008/009 vào cùng buổi với refresh:** cả bốn FR đều xoay quanh **một** cặp đối tượng — `ApplicationUser` và tập `RefreshToken` của họ — và dùng chung `IUserRepository` vừa mở rộng ở bước 1. Làm cùng lúc thì các phương thức `RevokeAll…`, `GetActive…` được thiết kế một lần cho cả bốn nhu cầu (khóa tài khoản, danh sách phiên, rotation) thay vì vá dần qua ba buổi. Claim `sid` phải có trước khi danh sách phiên đánh dấu được "thiết bị này", nên nó theo luôn.

**Không cache danh sách người dùng và danh sách phiên** (`Cache-Control: no-store`): cả hai đều là dữ liệu riêng tư phụ thuộc danh tính người gọi — đúng nguyên tắc cách ly Public/Private của NFR-SEC-006 (giải trình đầy đủ ở Buổi 6 — Dev 2).

Giải trình thiết kế của hồ sơ (PATCH partial, chặn đổi email, `/auth/me` là nguồn sự thật về quyền) nằm ở **Buổi 5 — Dev 1**. Của quản lý tài khoản và phiên (revoke khi khóa, chặn tự khóa, 404 cho phiên của người khác, đánh đổi 15 phút của JWT stateless) nằm ở **Buổi 7 — Dev 1** — nơi các FR đó hoàn tất giao diện.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:**
  - (a) FR-AUTH-004/005 dùng cờ **`IsRevoked = true`** và kiểm tra `IsRevoked == false`, nhưng bảng `RefreshToken` ở Chương 7.8 **chỉ có `RevokedAt timestamptz NULL`** — *"NULL = còn hiệu lực"* — **không có cột `IsRevoked`**.
  - (b) FR-AUTH-004 bước 5 gán `ReplacedByToken = newToken` (**raw token**) trong khi Chương 7.8 và NFR-SEC-002 quy định chỉ lưu hash.
  - (c) Độ dài token 512-bit (FR) vs 128-bit (NFR).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-14, MT-15, MT-13**) chốt:
  - **bỏ hẳn `IsRevoked`**, token hợp lệ ⇔ `RevokedAt IS NULL AND ExpiresAt > NOW()`;
  - **`ReplacedByTokenHash = SHA256(newToken)`**;
  - **256-bit**.
- **Tại sao chọn:** hai cột cùng biểu diễn một trạng thái là **nguồn bug kinh điển**. Một nguồn sự thật duy nhất (`RevokedAt`) vừa gọn hơn, vừa mang thêm thông tin *khi nào* bị thu hồi phục vụ audit. 256-bit khớp đúng độ dài output SHA-256 và cột `TokenHash varchar(64)`.
- **Kéo API của FR-AUTH-006 → 009 lên Buổi 4 (29/09/2026):**
  - **Lý do:** yêu cầu "hoàn thành tất cả API endpoints" của giảng viên.
  - **Không đổi yêu cầu nào trong SRS**, chỉ đổi thời điểm cài đặt; giao diện vẫn ở Buổi 5 và Buổi 7.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2314236_HoangBinhQuan_buoiso4`.
```
feat(auth): complete FR-AUTH-004 refresh rotation and FR-AUTH-006/007/008/009 profile, admin user and session APIs
```

## DEV 2 — Nội dung Công thức: Nguyên liệu, Các bước & Cập nhật có Concurrency (API)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Phần API của:
- `FR-RCP-009` CRUD nguyên liệu (chiến lược 2 cột Quantity);
- `FR-RCP-010` CRUD các bước nấu (server tự đánh số + endpoint reorder);
- `FR-RCP-004` Cập nhật công thức có Optimistic Concurrency `RowVersion` *(API kéo từ Buổi 5)*.

Tổng cộng **8 endpoint mới**. Kèm **kích hoạt hai mảng inline `steps?`/`ingredients?` của `POST /recipes`** (phần còn lại của FR-RCP-003) và **retrofit D-16**. *(Wizard bước 3–4 và trang sửa công thức chuyển sang Buổi 5.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **DB migration `B4_Recipe_IngredientStep` (+ retrofit D-16):** Buổi 2 đã có sẵn `Quantity decimal(10,3) NULL`, `Unit` nullable và `OrderIndex` — **không đổi các cột này**. Chỉ bổ sung:
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
   - Thêm `Recipe.RemoveStep(stepId)` (xóa rồi **renumber 1..N**).
   - Thêm `Recipe.ReorderSteps(IReadOnlyList<Guid> stepIds)`: tập id phải **khớp chính xác** tập step hiện có — thiếu/thừa/trùng → `BusinessRuleViolationException(VALIDATION_ERROR)` → 400 — rồi gán `StepNumber = index + 1`.
   - **Mọi thao tác renumber nằm trong một `SaveChangesAsync()` duy nhất**; nhờ constraint deferred ở bước 1, trạng thái trung gian không làm vỡ ràng buộc.
   - `CreateRecipeCommand` (Buổi 3) nhận thêm `steps?`/`ingredients?` và **chỉ gọi lại** `AddStep`/`AddIngredient` — không có validator thứ hai.
4. **CQRS + Validator nguyên liệu / bước:**
   - `Add/Update/DeleteIngredientCommand`: `name` **1–200**, `quantityText` ≤ 50, `unit` ≤ 50, `notes` ≤ 500 — theo §7.9.
   - `Add/Update/DeleteStepCommand`: `title` **1–200 bắt buộc**, `description` ≤ 2000, `timerMinutes` ≥ 0.
   - **`ReorderStepsCommand { RecipeId, StepIds[] }`**.
   - Handler nạp recipe bằng `unitOfWork.Recipes.GetByIdWithDetailsAsync` (Buổi 3); tất cả đi qua `RecipeAuthorizationHandler` và implement `ICacheInvalidator` → xóa `recipe:{slug}` + prefix `recipes:list:`.
5. **FR-RCP-004 — `UpdateRecipeCommand`** *(kéo từ Buổi 5)*:
   - Body `{ title?, description?, categoryId?, prepTime?, cookTime?, servings?, difficulty?, instructions?, nutrition?, rowVersion }` — `rowVersion` nhận từ body **hoặc** header `If-Match`.
   - Validator dùng cùng bộ giới hạn như FR-RCP-003.
   - Handler gọi **`unitOfWork.Recipes.SetOriginalRowVersion(recipe, rowVersion)`** — phương thức mới trên `IRecipeRepository`, cài ở Infrastructure bằng `Entry(recipe).Property(r => r.RowVersion).OriginalValue = …` (handler không chạm `DbContext`).
   - Đổi dữ liệu qua domain method `recipe.Update(...)`, rồi `SaveChangesAsync()`.
   - **Không `try/catch`:** `DbUpdateConcurrencyException` đã được `RecipePersistenceExceptionTranslator` (Buổi 3) dịch sang `RecipeConcurrencyException` → **409 `RECIPE_CONCURRENCY_CONFLICT`**.
   - **Quy tắc slug:** chỉ sinh lại slug khi `recipe.Status == RecipeStatus.Draft`; recipe đã từng Published thì **slug khóa vĩnh viễn** dù đổi Title.
   - Trả `RecipeDetailDto` kèm **`rowVersion`** mới (base64) và header **`ETag`**.
   - Invalidate `recipe:{slug}` (cả slug cũ lẫn mới) + prefix `recipes:list:`.
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
   | **`PUT /api/v1/recipes/{id}`** | cập nhật có concurrency |
   | `POST /api/v1/recipes` | nay nhận thêm hai mảng inline `steps?`/`ingredients?` |

   Tất cả nằm trong `RecipesEndpoints.cs` (file ghi nội dung của Dev 2).
7. **Test:**
   - **Unit test:** renumber sau khi xóa bước giữa; reorder với mảng thiếu id → 400.
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

**Vì sao D-16 là điều kiện tiên quyết, không phải tối ưu:** unique **index** được PostgreSQL kiểm tra **ngay sau từng câu `UPDATE`**; `DEFERRABLE` chỉ tồn tại cho **constraint**. Bỏ qua chi tiết này thì reorder **không bao giờ chạy được** (`23505` ở câu `UPDATE` đầu tiên → 500), dù code Domain hoàn toàn đúng.

**Vì sao `PUT /recipes/{id}` về tay Dev 2 ở buổi này (không đợi Buổi 5):** cập nhật công thức và CRUD bước/nguyên liệu cùng thao tác trên **một** aggregate, cùng `GetByIdWithDetailsAsync`, cùng quy tắc phân quyền và cùng chiến lược cache. Làm chung một buổi thì `recipe.Update(...)`, `SetOriginalRowVersion` và bộ test concurrency được viết một lần. Giải trình Optimistic Concurrency, `ETag`, khóa slug sau publish nằm ở **Buổi 5 — Dev 2** (nơi trang sửa công thức dùng chúng).

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:**
  - (a) Chương 7.4 ghi `Quantity` nullable nhưng FR-RCP-009 đòi **bắt buộc**, còn Chương 8 ghi `quantity?`, `unit?` — ba chỗ ba kiểu.
  - (b) FR-RCP-010 quy định Server tự gán `StepNumber` nhưng Chương 8 cho client gửi `stepNumber`.
  - (c) Tên trường lệch: `SortOrder` vs `OrderIndex`; `DurationMinutes` vs `TimerMinutes`.
  - (d) `RecipeStep.Title` là `NOT NULL` nhưng body POST step không có `title`.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-04, MT-03, MT-20.4, MT-20.5, MT-20.7, MT-41.7**) chốt:
  - hai cột `Quantity` + `QuantityText`, ràng buộc mềm "không rỗng cả ba";
  - Server toàn quyền gán `StepNumber` + endpoint `reorder`;
  - thống nhất `orderIndex`, `timerMinutes`;
  - `title` bắt buộc.
- **Tại sao chọn:** điểm (d) làm **mọi request POST step thất bại ở tầng DB** — loại lỗi chỉ lộ ra khi chạy thật. Tên trường lệch khiến request từ FE bị bind thành `null` **âm thầm**. Chốt theo tên trong schema vì schema là thứ khó đổi nhất.
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
   - **`SortMapper`:** whitelist dictionary 5 trường `createdAt`, `publishedAt`, `title`, `cookTime`, `prepTime` (mã nguồn ở Buổi 5 — Dev 3).
     - `sortBy` ngoài whitelist → **400 `VALIDATION_ERROR`**; `sortOrder ∈ {asc, desc}`; mặc định `sortBy=createdAt&sortOrder=desc`.
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

   - **Gỡ `Skip`** khỏi test tái hiện MT-34 mà Dev 4 viết sẵn ở Buổi 3 và cập nhật `GetRecipeBySlug_DraftAsGuest_Returns403ForNow` thành 404 — hai test phải chuyển sang xanh.
5. **CQRS — `SearchRecipesQuery : ICacheable`:**
   - Validate `q` ≥ 2 ký tự → sai thì **400 `VALIDATION_ERROR`**.
   - Sanitize term (loại `& | ! ( ) : *`) và build chuỗi prefix `pho:* & bo:*`.
   - `.Where(r => r.SearchVector.Matches(EF.Functions.ToTsQuery("simple", query)))` + `RecipeFilterSpec`; `ORDER BY ts_rank(...) DESC`; **chỉ `Status == Published`**; trả `relevanceScore`.
   - `CacheKey = search:{queryHash}`, `Expiration = 1 phút`.
6. **FR-RCP-011 — `GetMyRecipesQuery`** *(kéo từ Buổi 6)*:
   - **Không** implement `ICacheable`; lọc `r.AuthorId == targetAuthorId` (`authorId` khi người gọi là Admin, ngược lại `ICurrentUser.UserId`).
   - Author truyền `authorId` của người khác → **403 `RECIPE_FORBIDDEN`**.
   - `status` ∈ `{Draft, Published, Archived}` (ngoài → 400); dùng chung `SortMapper` + phân trang.
   - Endpoint gắn **`Cache-Control: no-store`**.
7. **CR-2026-04 (d) — `GetMyRecipeByIdQuery`:**
   - Nạp recipe theo `id` ở **mọi trạng thái** qua `unitOfWork.Recipes.GetByIdWithDetailsAsync` (Buổi 3), rồi kiểm quyền bằng `RecipeAuthorizationHandler` (chủ sở hữu hoặc Admin).
   - Recipe không tồn tại **hoặc thuộc người khác** → **404 `RECIPE_NOT_FOUND`** (không phải 403, để không dò được id).
   - Trả `RecipeDetailDto` **kèm `rowVersion`** và header **`ETag`** + **`Cache-Control: no-store`**. Đây là nguồn dữ liệu của trang sửa công thức (`PUT` của Dev 2 dùng lại `rowVersion` này).
8. **Endpoint sitemap (MT-48)** *(kéo từ Buổi 7)*: `GetRecipeSitemapQuery : ICacheable` trả `{ slug, updatedAt }[]` của mọi recipe Published, không phân trang, TTL 1 giờ. Invalidate khi publish/unpublish/archive/delete: Dev 4 thêm khóa `recipes:sitemap` vào `ICacheInvalidator` của các command vòng đời.
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

Giải trình đầy đủ nguyên tắc *"cache nằm trước handler nên dữ liệu riêng tư không bao giờ được cache"* nằm ở **Buổi 6 — Dev 2**.

**Một `RecipeFilterSpec` cho bốn truy vấn:** bốn nơi tự viết bốn bộ lọc hơi khác nhau là điều gần như chắc chắn xảy ra khi bốn màn hình được làm ở bốn thời điểm. Dev 3 sở hữu **toàn bộ phía đọc** công thức từ buổi này nên không còn điểm phối hợp "sửa `GetRecipesQuery` của người khác" như kế hoạch cũ (§6). Giải trình whitelist, `sortBy`/`sortOrder`, `pageSize = 12` nằm ở **Buổi 5 — Dev 3**.

**Vì sao `GET /recipes/mine/{id}` trả 404 (không phải 403) cho công thức của người khác và nằm dưới `/mine`:**
- Nằm dưới tiền tố `/mine` thì mọi endpoint riêng tư của module Recipe có chung một dấu hiệu nhận biết: bắt buộc Bearer, `no-store`, không `ICacheable`. Quy tắc cách ly Public/Private vẫn đọc được từ chính URL.
- Trả 404 cho id của người khác theo đúng tiền lệ FR-AUTH-009 (phiên của người khác → 404): endpoint không trở thành công cụ dò id nào đang tồn tại.
- **Phương án bị loại — cho `GET /recipes/{slug}` trả Draft khi người gọi là chủ:** mở lại đúng lỗ hổng MT-34 (endpoint công khai lọc theo danh tính trong khi bị cache theo khóa công khai).

**Cache sitemap 1 giờ, invalidate khi vòng đời thay đổi:** crawler gọi sitemap vài lần mỗi ngày, nhưng một truy vấn toàn bảng mỗi lần thì phí; 1 giờ đúng bằng chu kỳ tái sinh `app/sitemap.ts` (Buổi 7). Invalidate chủ động ở các command publish/unpublish/archive/delete giúp slug mới xuất hiện ngay ở lượt tái sinh kế tiếp.

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

## DEV 4 — Vòng đời Công thức (API) & Job Nền: Publish/Unpublish, Archive/Unarchive, Xóa mềm, Image Resize, Permanent Purge

**Phần 1 – Chức năng hoàn thành trong buổi:**
- Phần API của `FR-RCP-005` Xuất bản / Hủy xuất bản *(kéo từ Buổi 5)*, `FR-RCP-006` Lưu trữ / Khôi phục *(kéo từ Buổi 7)* và `FR-RCP-007` Xóa mềm *(kéo từ Buổi 6)* — **5 endpoint mới**, cùng máy trạng thái khép kín.
- `FR-JOB-002` Image Resize Job + `FR-JOB-003` Permanent Purge Job.
- **Kèm retrofit D-15** (máy trạng thái) và **D-3** (partial unique slug).
- **Test bề mặt API 45/45** — bằng chứng tự động cho yêu cầu của buổi.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain — `Recipe` thành `partial class`; vòng đời nằm ở `Recipe.Lifecycle.cs`:**
   - Hiện thực `Publish()`, `Unpublish()`, `Archive()`, `Unarchive()`, `SoftDelete()`.
   - Gom toàn bộ luật chuyển trạng thái vào **một** nơi duy nhất — `RecipeStatusTransitions`, dictionary tra cứu `(from, action) → to`:

     | Từ trạng thái | Hành động | Endpoint | Sang trạng thái |
     |---|---|---|---|
     | Draft | publish | `PATCH /recipes/{id}/publish` | Published |
     | Published | unpublish | `PATCH /recipes/{id}/unpublish` | Draft |
     | Draft **hoặc** Published | archive | `PATCH /recipes/{id}/archive` | Archived |
     | Archived | **unarchive** | `PATCH /recipes/{id}/unarchive` | **Draft** |

   - Mọi chuyển đổi **ngoài bảng** → `InvalidRecipeStatusException` (Buổi 3) → **409 `RECIPE_INVALID_STATE_TRANSITION`**.
   - **Retrofit D-15 trên `Recipe.Publish()` có sẵn:**
     - **giữ** hai điểm Buổi 2 đã làm đúng: kiểm tra `_steps.Count == 0 || _ingredients.Count == 0`, và `PublishedAt ??= utcNow` (chỉ gán lần đầu);
     - **sửa** (a): bỏ nhánh `if (Status == Published) return;` và nhánh cho Archived lọt qua — nay đi qua bảng chuyển trạng thái;
     - **sửa** (b): thiếu step/ingredient ném **`RecipePublishIncompleteException : RecipeDomainException`** (mới, `RECIPE_PUBLISH_INCOMPLETE` → **400**, thêm một dòng vào `RecipeExceptionMappings`), thay cho `BusinessRuleViolationException` tạm dùng từ Buổi 3.
2. **CQRS — `PublishRecipeCommand`, `UnpublishRecipeCommand`, `ArchiveRecipeCommand`, `UnarchiveRecipeCommand`, `DeleteRecipeCommand`:**
   - Tất cả nạp recipe bằng `unitOfWork.Recipes.GetByIdWithDetailsAsync`. ⚠️ Dùng `GetByIdAsync` thì hai collection rỗng và recipe đủ điều kiện cũng bị từ chối publish.
   - Tất cả đi qua `RecipeAuthorizationHandler` (403 `RECIPE_FORBIDDEN`), không tìm thấy → 404 `RECIPE_NOT_FOUND`.
   - Tất cả implement `ICacheInvalidator` → xóa `recipe:{slug}` + prefix `recipes:list:` + prefix `categories:detail:` + khóa `recipes:sitemap`.
   - `DeleteRecipeCommand` gọi `recipe.SoftDelete()`: **KHÔNG** xóa child entity (vô hình cùng recipe qua Global Query Filter) và **KHÔNG** enqueue xóa file MinIO — việc đó thuộc `PermanentPurgeJob` ở bước 6.
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
   - Dùng **SixLabors.ImageSharp** tạo medium **800×600** và thumbnail **300×300** (giữ tỉ lệ, crop giữa).
   - Upload cả hai lên MinIO cùng folder với ảnh gốc, cập nhật `MediumUrl`/`ThumbnailUrl` vào DB.
   - `[AutomaticRetry(Attempts = 3)]`; fail hết retry → giữ nguyên `null`, **ảnh gốc vẫn hiển thị bình thường**.
   - Móc vào `UploadRecipeImageCommand` của Dev 2 (Buổi 3) bằng `BackgroundJob.Enqueue`.
6. **FR-JOB-003 — `PermanentPurgeJob`:** recurring **`"30 3 * * *"` (03:30 UTC)**. Thân job:
   - Query `_db.Recipes.IgnoreQueryFilters().Where(r => r.IsDeleted && r.UpdatedAt < DateTime.UtcNow.AddDays(-30))` — **bắt buộc `IgnoreQueryFilters()`** vì Global Query Filter đã ẩn chính những bản ghi ta cần tìm.
   - Với mỗi recipe:
     - **(1)** thu thập toàn bộ URL ảnh (`RecipeImages`: original/medium/thumbnail, và `RecipeStep.ImageUrl`) **trước khi xóa**;
     - **(2)** hard-delete recipe trong một transaction — FK `ON DELETE CASCADE` tự dọn Steps/Ingredients/Images. Job nằm ở Infrastructure nên thao tác thẳng trên `CulinaryBlogDbContext`: đây là **nơi duy nhất** được xóa vật lý (`IRepository<T>` cố ý không có hard delete — xem Buổi 3). Transaction mở **qua execution strategy** theo đúng mẫu `UnitOfWork.ExecuteInTransactionAsync`;
     - **(3)** **chỉ sau khi transaction commit thành công** mới gọi `IFileStorageService.DeleteAsync()` cho từng URL.
7. **Distributed lock + đăng ký:**
   - Job chạy trong container **`hangfire` (worker)**. Bọc thân `PermanentPurgeJob` bằng RedLock (`RedLock.net`) khóa `lock:purge-job`, TTL 10 phút, kèm `[DisableConcurrentExecution(600)]`. Không lấy được lock thì bỏ qua lượt và log Information.
   - Đăng ký: `RecurringJob.AddOrUpdate<PermanentPurgeJob>("permanent-purge", j => j.ExecuteAsync(), "30 3 * * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc })`; `[AutomaticRetry(Attempts = 2)]`; log số recipe đã dọn và số file đã xóa.
8. **Test:**
   - **Unit test ma trận trạng thái:** **12 tổ hợp** `(3 trạng thái × 4 hành động)` — 5 hợp lệ cho đúng trạng thái đích, **7 còn lại ném `InvalidRecipeStatusException`** (assert đúng kiểu); kịch bản `publish → unpublish → publish lại` giữ nguyên `PublishedAt` lần đầu.
   - **Integration test vòng đời:**
     - publish recipe thiếu nguyên liệu → 400 `RECIPE_PUBLISH_INCOMPLETE`;
     - publish recipe Archived → 409;
     - unarchive → Draft;
     - xóa recipe slug `pho-bo` → tạo recipe mới cùng tên → slug mới lại là `pho-bo` (D-3);
     - recipe đã xóa → `GET /recipes/{slug}` 404, và **file ảnh vẫn còn** trên MinIO giả.
   - **Integration test job:** seed recipe `IsDeleted = true, UpdatedAt = 40 ngày trước` → chạy `PermanentPurgeJob` trực tiếp → bản ghi biến mất **và** `IFileStorageService.DeleteAsync` được gọi đủ URL; `ImageResizeJob` với ảnh mẫu → `MediumUrl`/`ThumbnailUrl` được điền.
9. **Test bề mặt API — `ApiSurfaceTests` (bằng chứng cho yêu cầu của buổi):**
   - Test đọc toàn bộ route thật của ứng dụng từ `EndpointDataSource` và so với **danh sách 45 cặp (method, route) của SRS v1.2.2 Chương 8**, lưu thành hằng số trong test kèm số mục SRS.
   - Test đỏ nếu **thiếu** endpoint nào của SRS **hoặc thừa** endpoint `/api/v1` nào ngoài SRS.
   - Ba endpoint health đi ngoài tiền tố `/api/v1` và được liệt kê riêng.
10. **Kiểm chứng tay:** trên `/hangfire` trigger `permanent-purge`, xác nhận bản ghi biến mất khỏi DB **và** file biến mất khỏi MinIO console; upload ảnh qua Scalar → thấy đủ 3 biến thể ảnh trong MinIO. *(Hiển thị `ThumbnailUrl` trong `RecipeCard` chuyển sang Buổi 5.)*

**Phần 3 – Định hướng & Lý do thiết kế:**
**Vì sao máy trạng thái và xóa mềm về tay Dev 4, cùng buổi với purge job:** bốn hành động trạng thái + xóa mềm + dọn vĩnh viễn là **một chuỗi vòng đời liền mạch** của cùng một bản ghi: Draft ⇄ Published → Archived → (xóa mềm) → sau 30 ngày bị job xóa hẳn. Tách chuỗi này cho hai người sẽ tạo ra đúng loại khe hở mà kế hoạch đã phải sửa hai lần:
- Archived "vào được không ra được" (MT-35);
- xóa mềm nhưng không ai dọn (MT-05).

Gom vào một người, cùng một buổi, giúp bảng `RecipeStatusTransitions`, `SoftDelete()`, partial unique index (D-3) và truy vấn `IgnoreQueryFilters()` của purge job được thiết kế **nhất quán một lần**. Đồng thời cân bằng tải: Dev 2 giữ phần nội dung (8 endpoint), không phải gánh 15 endpoint.

**`partial class` và ba file endpoint thay vì một file mỗi module:**
- Buổi này có ba dev cùng sửa module Recipe.
- Một file `Recipe.cs` và một file `RecipesEndpoints.cs` cho ba người là xung đột merge chắc chắn xảy ra, vì cùng vùng code và cùng ngày.
- Chia file theo **trách nhiệm** (nội dung / truy vấn / vòng đời) giữ nguyên một aggregate, một route group `/recipes`, nhưng mỗi người chỉ sửa file của mình.

**Gom luật chuyển trạng thái vào một dictionary** thay vì rải `if` trong từng method: thêm một trạng thái mới (ví dụ `PendingReview`) chỉ là thêm dòng, và bảng đó là tài liệu sống của máy trạng thái. FE (Buổi 7) xuất chính bảng này thành hằng số để chỉ hiện nút hợp lệ.

**Test đủ 12 tổ hợp thay vì chỉ đường hạnh phúc:** lỗi ở v1.0.0 không phải "code sai" mà là "có một chuyển đổi không tồn tại và không ai nhận ra" — chỉ lộ ra khi liệt kê **toàn bộ** ma trận.

**`ApiSurfaceTests` biến "đã cài đặt tất cả API endpoints" thành điều kiện kiểm được:**
- "Đủ 45 endpoint" không còn là việc đếm tay trên Scalar, mà là một test chạy ở mọi commit từ nay tới Buổi 8.
- Chiều "thừa" quan trọng không kém chiều "thiếu": một endpoint không có trong SRS là yêu cầu không truy vết được (nguyên tắc §4.2).
- Người thêm endpoint mới buộc phải đi qua Change Request trước.

**Resize bất đồng bộ:** resize một ảnh 5MB tốn 1–3 giây CPU; làm đồng bộ thì 5 ảnh là 15 giây chờ. Quan trọng hơn, **resize thất bại không làm hỏng việc upload** — tách việc bắt buộc khỏi việc nên có.

**"Commit DB trước, xóa file sau" trong purge job là bắt buộc:**
- **Làm ngược lại (xóa file trước):** nếu transaction DB rollback thì recipe trỏ tới file không còn tồn tại — hỏng vĩnh viễn.
- **Làm đúng thứ tự:** trường hợp xấu nhất chỉ là **file mồ côi**, vô hại.
- **Nguyên tắc chung:** khi phối hợp hai hệ thống không có transaction chung, sắp xếp sao cho lỗi rơi vào phía **rác thừa**, không phải **mất dữ liệu**.

**Distributed lock dù Hangfire đã điều phối:** Hangfire bảo đảm mỗi *job instance* chỉ một server lấy, nhưng không bảo đảm hai *lượt* của cùng recurring job không chồng lên nhau (retry, trigger tay, worker thứ hai khi scale).

**Soft Delete và ba hệ quả bắt buộc:**
- **(1) Partial unique index** — nếu không, slug của recipe đã xóa chiếm chỗ vĩnh viễn.
- **(2) Child entity không xóa theo.**
- **(3) Không xóa file khi xóa mềm** — nếu xóa thì "khôi phục được trong 30 ngày" (NFR-REL-003, MT-49) thành lời hứa suông.

Giải trình đầy đủ về xóa mềm ở **Buổi 6 — Dev 2**; về Archive/Unarchive ở **Buổi 7 — Dev 2**; về `PublishedAt` và checklist publish ở **Buổi 5 — Dev 2** — nơi các FR này hoàn tất giao diện.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **FR-JOB-003 ở v1.0.0 là Sitemap Generation Job:** tạo `sitemap.xml` lên MinIO/`wwwroot` rồi ping Google. Đặc tả này sai vị trí phục vụ (crawler đọc sitemap ở domain Next.js). Endpoint ping cũng đã bị Google khai tử từ 6/2023.
  - **Hướng đi:** SRS v1.1.0 (**MT-28 + MT-05**) chuyển sitemap sang Next.js và dùng khe FR-JOB-003 cho **Permanent Purge Job** (xung đột **X-2** — §3).
- **Máy trạng thái ở v1.0.0 có Archived là hố đen:** vào được, không ra được.
  - **Hướng đi:** SRS v1.1.0 (**MT-35**) bổ sung **unarchive → Draft** và mã 409 cho mọi chuyển đổi ngoài bảng.
- **FR-RCP-007 ở v1.0.0 ghi hard delete, trong khi 4 chỗ khác ghi soft delete.**
  - **Hướng đi:** SRS v1.1.0 (**MT-05**) chốt **soft delete** + job dọn sau 30 ngày.
- **Chuyển API FR-RCP-005/006/007 từ Dev 2 sang Dev 4 (29/09/2026):**
  - **Lý do:** cân bằng tải khi yêu cầu "hoàn thành tất cả API endpoints" dồn 25 endpoint vào một buổi.
  - **Không đổi SRS**; Dev 2 vẫn là chủ giao diện của ba FR này (Buổi 5, 6, 7) và dùng API qua hợp đồng Chương 8.

**Phần 5 – Kết quả Commit Git:**
Nhánh `2312755_NguyenThangThieng_buoiso4`.
```
feat(recipes,jobs): complete FR-RCP-005/006/007 lifecycle APIs with closed state machine, FR-JOB-002 image resize & FR-JOB-003 purge job, api surface test 45/45
```

## Kiểm chứng cuối Buổi 4

1. **`ApiSurfaceTests` xanh:** route thật của ứng dụng khớp **đúng 45** endpoint của SRS v1.2.2 Chương 8 — không thiếu, không thừa.
2. **Mọi endpoint có ≥ 1 happy path + ≥ 1 error case** trong integration test (NFR-MAINT-002); mã lỗi đúng Phụ lục B; mọi lỗi là `application/problem+json`.
3. **Không còn lỗ hổng rò rỉ bản nháp:** test MT-34 đã gỡ `Skip` và xanh; không còn `CacheOutput` trong code.
4. **Tìm kiếm:** tìm "pho" ra "Phở bò".
5. **Vòng đời:** ma trận 12 tổ hợp trạng thái xanh; xóa rồi tạo lại cùng tên được slug cũ.
6. **Phiên đăng nhập:** dùng lại refresh token cũ → cả họ token bị thu hồi.
7. **Scalar `/scalar`** hiển thị đủ 45 endpoint, test tay từng nhóm.
8. **Job:** purge job xóa đúng bản ghi > 30 ngày và file của nó; resize job sinh đủ 3 biến thể.

---

# BUỔI 5 – GIAO DIỆN: PHIÊN & HỒ SƠ, SOẠN/SỬA/XUẤT BẢN CÔNG THỨC, TÌM KIẾM & BỘ LỌC; HIỆU NĂNG TRUY VẤN & HEALTH CHECKS

> **Mục tiêu buổi:** toàn bộ API đã hoàn thành ở **Buổi 4** (45/45 endpoint) — buổi này dựng **giao diện** trên hợp đồng API đó:
> - người dùng giữ phiên an toàn và quản lý được hồ sơ;
> - Author soạn trọn công thức (nguyên liệu, các bước), sửa an toàn dưới tải đồng thời và xuất bản có checklist;
> - người đọc tìm kiếm, lọc, sắp xếp, phân trang được;
> - truy vấn danh sách được chứng minh dùng index, và hệ thống tự báo cáo được sức khỏe.
>
> **Quy ước cho Buổi 5 → 7 (từ 29/09/2026):**
> - **Phần 2** của mỗi FR chỉ còn các bước giao diện và các việc không phải endpoint; bước API tương ứng ghi "đã làm ở Buổi 4" kèm tên chủ API.
> - **Phần 3–4 giữ nguyên**, vì giải trình thiết kế vẫn đúng cho cả phần API (Buổi 4) lẫn phần giao diện (buổi này).

## DEV 1 — Phiên đăng nhập phía Frontend (D-12) & Giao diện Hồ sơ Cá nhân

**Phần 1 – Chức năng hoàn thành trong buổi:**
- Giao diện `FR-AUTH-006` Xem Hồ sơ + `FR-AUTH-007` Cập nhật Hồ sơ & Avatar.
- Phần Frontend của `FR-AUTH-004`: **retrofit D-12** (access token rời `localStorage`) + interceptor refresh single-flight.

*(API `GET/PATCH /auth/me` và `POST /auth/refresh` đã làm ở **Buổi 4 — Dev 1**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4 (Dev 1):** `GET /api/v1/auth/me`, `PATCH /api/v1/auth/me`, `POST /api/v1/auth/refresh` cùng integration test. Buổi này không sửa handler; lệch hợp đồng thì sửa ở phía API trước.
2. **Retrofit D-12 + single-flight interceptor (chuyển từ Buổi 4):**
   - `AuthProvider` giữ **access token chỉ trong bộ nhớ** (React state/ref), không ghi `localStorage` nữa; chỉ **refresh token** được lưu bền.
   - Khi tải lại trang, `AuthProvider` gọi `/auth/refresh` một lần để lấy access token mới.
   - `lib/api-client.ts` gặp 401 `AUTH_TOKEN_EXPIRED` → gọi `/auth/refresh` **đúng một lần** dù có bao nhiêu request song song (biến `refreshPromise` làm mutex), rồi retry các request đang treo.
   - Refresh thất bại → xóa phiên, `queryClient.clear()`, redirect `/auth/login?callbackUrl=`.
3. **UI — `/profile` (CSR):** form RHF + Zod mirror validator backend (`displayName` 2–100, `bio` ≤ 1000).
   - Avatar dùng lại `ImageUploader` (Buổi 2), upload lên **`POST /api/v1/files/upload`** (FR-FILE-001) → nhận URL → PATCH `/auth/me`.
   - Cập nhật avatar ở header ngay bằng optimistic update của TanStack Query + toast; rollback khi API lỗi.
   - Avatar cũ được xóa qua `DELETE /api/v1/files/{**key}` **sau khi** PATCH thành công — không xóa trước, vì nếu PATCH lỗi người dùng sẽ mất cả ảnh cũ.
4. **Kiểm chứng:**
   - 5 request song song khi access token hết hạn → đúng **1** lời gọi `/auth/refresh` (xem tab Network);
   - `localStorage` không còn access token;
   - đổi avatar → header đổi ngay, F5 vẫn giữ.

**Phần 3 – Định hướng & Lý do thiết kế:**
`GET /auth/me` là **cách duy nhất Frontend biết được `roles` của người dùng**. Đây là lý do FR này được nâng lên mức **Must Have** (v1.0.0 xếp Should Have): mọi route `/dashboard*` ở mục 5.1 đều yêu cầu quyền Author/Admin, và menu "Quản lý danh mục" chỉ hiện với Admin — không có `/auth/me` thì FE không có cách nào biết ai là Admin.

Phương án thay thế **"FE tự decode JWT để lấy roles"** bị loại dứt khoát: nó biến payload của token thành nguồn sự thật về quyền, nghĩa là **quyền bị thu hồi sẽ không có hiệu lực cho tới khi access token hết hạn** (tối đa 15 phút), và tệ hơn là tạo thói quen nguy hiểm — tin dữ liệu client tự giải mã. Gọi `/auth/me` đảm bảo FE luôn thấy trạng thái quyền hiện tại từ server.

**PATCH partial thay vì PUT toàn phần:** người dùng đổi avatar không có lý do gì phải gửi lại cả `bio` và `displayName` — PUT toàn phần sẽ khiến FE phải luôn giữ bản sao đầy đủ của profile và vô tình ghi đè trường mà người dùng không định sửa (lost update).

**Tách `Email`/`UserName` khỏi endpoint này** vì đổi email là một luồng nghiệp vụ khác hẳn: nó cần xác minh email mới, cần cân nhắc ảnh hưởng tới liên kết Google đã gắn, và có thể cần đăng nhập lại. Gộp vào PATCH profile sẽ biến một thao tác nhạy cảm thành một field bình thường trong form.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-AUTH-001/006/007 dùng `fullName` + `userName`, trong khi Chương 7.7 chỉ có cột **`DisplayName`** — *"Tên hiển thị công khai (không phải username)"* — và **không có cột `FullName`**; Chương 8 cũng chỉ dùng `displayName`, `bio`. Nghĩa là FR-AUTH đang mô tả việc **ghi vào một cột không tồn tại**, đồng thời bỏ sót `bio` mà cả schema lẫn API đều có. (b) FR-AUTH-006 mô tả trả **404** khi *"user đã bị xóa"*, nhưng **không có FR nào xóa người dùng** và `AspNetUsers` không kế thừa `BaseEntity` (không có `IsDeleted`). (c) `UserProfileDto` có trường `emailConfirmed` trong khi policy `VerifiedAuthor` đã bị loại và không có luồng xác nhận email.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-12, MT-33.9, MT-21**) chốt: dùng **`displayName`**, **bỏ hẳn `fullName`**, bổ sung `bio` vào DTO; đổi 404 thành **401**; **bỏ `emailConfirmed`** khỏi DTO công khai. `UserName` vẫn tồn tại vì kế thừa `IdentityUser` nhưng **do BE sinh tự động** từ prefix email, người dùng không nhập.
- **Tại sao chọn:** Giữ cả `fullName` lẫn `displayName` là hai trường gần như trùng nghĩa — người dùng sẽ bối rối không biết điền gì vào đâu, và hệ thống phải quyết định hiển thị cái nào ở mỗi chỗ (một quyết định sẽ bị làm khác nhau ở các màn hình khác nhau). Về mã lỗi: trả 404 ngụ ý *"tài nguyên không tồn tại"*, nhưng tình huống thật ở đây là *"token của bạn trỏ tới một user không còn hợp lệ"* — đó chính xác là ngữ nghĩa của **401**, và nó cũng kích hoạt đúng luồng xử lý ở FE (gọi refresh, thất bại thì đăng xuất) thay vì hiện trang "Không tìm thấy".

**Phần 5 – Kết quả Commit Git:**
```
feat(auth-ui): in-memory access token with single-flight refresh (D-12) and profile page with validated avatar upload
```

## DEV 2 — Giao diện soạn & sửa công thức: Nguyên liệu, Các bước, Trang sửa (Concurrency) & Xuất bản

**Phần 1 – Chức năng hoàn thành trong buổi:**
Giao diện của:
- `FR-RCP-009`/`FR-RCP-010` — wizard bước 3 "Nguyên liệu" và bước 4 "Các bước" (chuyển từ Buổi 4);
- `FR-RCP-004` — trang sửa với xử lý 409;
- `FR-RCP-005` — nút xuất bản / hủy xuất bản có checklist điều kiện.

*(API đã làm ở **Buổi 4**:*
- *nguyên liệu, các bước, `PUT /recipes/{id}` — **Dev 2**;*
- *`GET /recipes/mine/{id}` — **Dev 3**;*
- *`PATCH /recipes/{id}/publish`, `/unpublish` và retrofit D-15 — **Dev 4**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4.** Hợp đồng dùng trong buổi này:

   | Nhóm | Endpoint | Chủ API |
   |---|---|---|
   | Nguyên liệu | `POST/PUT/DELETE /recipes/{id}/ingredients/{ingId?}` | Dev 2 |
   | Các bước | `POST/PUT/DELETE /recipes/{id}/steps/{stepId?}` + `PATCH /recipes/{id}/steps/reorder` | Dev 2 |
   | Sửa | `PUT /recipes/{id}` (409 `RECIPE_CONCURRENCY_CONFLICT`) | Dev 2 |
   | Đọc bản nháp | `GET /recipes/mine/{id}` (trả `rowVersion` + `ETag`) | Dev 3 |
   | Xuất bản | `PATCH /recipes/{id}/publish` (400 `RECIPE_PUBLISH_INCOMPLETE`, 409 `RECIPE_INVALID_STATE_TRANSITION`) và `/unpublish` | Dev 4 |

2. **Wizard bước 3 "Nguyên liệu" (chuyển từ Buổi 4):** `useFieldArray`, mỗi dòng có ô số + ô nguyên văn, gợi ý người dùng nhập một trong hai; lỗi `INGREDIENT_QUANTITY_REQUIRED` gắn vào đúng dòng.
3. **Wizard bước 4 "Các bước" (chuyển từ Buổi 4):** **kéo-thả bằng `dnd-kit` → gọi endpoint reorder**, ảnh minh họa bước qua `ImageUploader`, ô timer. Form **không bao giờ gửi `stepNumber`**.
4. **Trang `/dashboard/recipes/[id]/edit`:**
   - Nạp dữ liệu bằng **`GET /recipes/mine/{id}`**, tái dùng wizard, gửi kèm `rowVersion` (header `If-Match`).
   - Gặp 409 → dialog *"Dữ liệu đã được người khác cập nhật — Tải lại / Xem khác biệt"*.
   - Recipe đã từng Published: ô tiêu đề hiện ghi chú *"Đổi tiêu đề không đổi đường dẫn"* (quy tắc khóa slug).
5. **Nút Publish / Unpublish:**
   - **Checklist điều kiện** hiện trước khi bấm, tính từ dữ liệu vừa nạp (✓ đã có N bước, ✗ chưa có nguyên liệu); nút bị vô hiệu khi còn thiếu.
   - Server vẫn kiểm tra, nên vẫn xử lý 400 `RECIPE_PUBLISH_INCOMPLETE` và 409 `RECIPE_INVALID_STATE_TRANSITION` bằng thông báo dễ hiểu.
6. **Kiểm chứng:**
   - mở cùng một công thức ở hai tab, lưu tab 1 rồi lưu tab 2 → tab 2 hiện dialog 409;
   - kéo-thả đổi thứ tự bước → F5 vẫn đúng thứ tự;
   - publish công thức thiếu nguyên liệu → nút bị khóa kèm lý do.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Optimistic Concurrency thay vì khóa bi quan (pessimistic lock):** khóa bi quan sẽ giữ transaction mở suốt thời gian người dùng điền form — có thể là vài phút — làm nghẽn connection pool và tạo nguy cơ deadlock. Optimistic không khóa gì cả, chỉ phát hiện xung đột **lúc lưu**: nếu `RowVersion` trong DB đã khác giá trị client đọc lúc đầu, nghĩa là có người khác đã sửa trong lúc này. Với một blog nơi xung đột hiếm khi xảy ra, đây là đánh đổi đúng.

**Trả `rowVersion` trong DTO và gắn `ETag`** là cách làm cho cơ chế này hoạt động được: client phải có thứ để gửi lại. Đây cũng là lý do mã lỗi đúng phải là **409** — nó khớp chính xác ngữ nghĩa HTTP *"yêu cầu xung đột với trạng thái hiện tại của tài nguyên"*, vốn là tình huống kinh điển của mô hình ETag/If-Match.

**Checklist điều kiện publish hiển thị trước khi bấm** thay vì chỉ báo lỗi sau khi bấm: người dùng đã viết xong một công thức dài, bấm Publish rồi nhận thông báo lỗi là trải nghiệm bực bội. Server vẫn phải kiểm tra (không tin client), nhưng UI nên nói trước.

**Giải trình quy tắc `PublishedAt` chỉ gán một lần:** nếu ghi đè mỗi lần publish, thì thao tác *unpublish rồi publish lại* (ví dụ sửa lỗi chính tả) sẽ khiến bài viết cũ **nhảy lên đầu trang chủ** — vì trang chủ sắp xếp theo `publishedAt` giảm dần. Người đọc quay lại sẽ thấy "công thức mới" hóa ra là bài họ đọc tuần trước. Giữ đúng ngữ nghĩa *"ngày xuất bản lần đầu"* cũng là điều Google mong đợi ở `datePublished` trong JSON-LD.

**Giải trình quy tắc khóa slug sau publish:** cho đổi slug tự do sẽ làm chết mọi link đã chia sẻ và mọi URL Google đã lập chỉ mục, trong khi hệ thống **không có bảng lưu slug cũ** để redirect. Cho đổi khi còn Draft là đủ để sửa lỗi chính tả trước khi công khai — lúc đó chưa ai biết URL nên không có gì để làm hỏng.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-004 trả **409** cho RowVersion mismatch, nhưng Phụ lục A ghi *"422 = RowVersion conflict"* và Phụ lục B ghi `RECIPE_CONCURRENCY_CONFLICT` = **422**. (b) FR-RCP-005 quy định điều kiện publish chỉ cần *"ít nhất 1 bước thực hiện (`Steps.Count > 0`)"*, nhưng Phụ lục B mô tả `RECIPE_PUBLISH_INCOMPLETE` là *"phải có ít nhất 1 ingredient **và** 1 step"* — và FR-RCP-005 trả mã 422 trong khi Phụ lục B ghi 400. (c) Chương 7.2 ghi `PublishedAt` *"set khi Status chuyển sang Published"* nhưng FR-RCP-005 bước 6 chỉ ghi *"set `Status`, `UpdatedAt`"* — **không có `PublishedAt`**. (d) NFR-SEO-004 yêu cầu **301 redirect** từ slug cũ, nhưng không có bảng/cột nào lưu slug cũ.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-08, MT-09, MT-06, MT-35, MT-27**) chốt: **400** cho validation, **409** cho mọi xung đột trạng thái (bỏ hẳn 422); publish yêu cầu **cả step lẫn ingredient** với mã **400 `RECIPE_PUBLISH_INCOMPLETE`**; **`PublishedAt` gán ở lần publish đầu tiên, không ghi đè**; **khóa slug sau publish, bỏ yêu cầu 301**.
- **Tại sao chọn:** Về (c) — Buổi 2 đã đón đầu đúng (`PublishedAt ??= utcNow`), buổi này **giữ nguyên** và chỉ bổ sung test; nhưng cần hiểu vì sao đó là điểm không được "đơn giản hóa" khi refactor: nếu làm theo SRS v1.0.0, đây là loại lỗi *im lặng* nguy hiểm — cột `PublishedAt` sẽ **mãi mãi `NULL`**, kéo theo index `IDX_Recipe_PublishedAt` hoàn toàn vô dụng, `datePublished` trong JSON-LD rỗng (hỏng structured data), và trang chủ sắp xếp theo `publishedAt` không có gì để sắp. Không có lỗi nào được ném ra, chỉ là một tính năng âm thầm không hoạt động. Về (b): một công thức nấu ăn không có nguyên liệu là vô nghĩa với người đọc, và JSON-LD Schema.org Recipe **yêu cầu `recipeIngredient[]`** — thiếu là trượt Google Rich Results Test, tức là vi phạm chính NFR-SEO-001. Về (d): yêu cầu 301 không có cách nào hiện thực vì không có nơi lưu slug cũ; ba phương án được cân nhắc là *(A) slug không bao giờ đổi*, *(B) đổi được khi Draft rồi khóa sau publish*, *(C) thêm bảng `RecipeSlugHistory` + tra cứu 2 bước*. Chọn **B** vì cân bằng tốt nhất: sửa được lỗi chính tả trước khi công khai, không cần bảng lịch sử, và link công khai không bao giờ chết. Phương án C đúng chuẩn SEO nhất nhưng thêm một bảng và một nhánh tra cứu cho tình huống hiếm — không đáng với quy mô hệ thống này.

**Phần 5 – Kết quả Commit Git:**
```
feat(recipes-ui): ingredient & step wizard with drag-and-drop reorder, edit page with 409 handling and publish checklist
```

## DEV 3 — Giao diện Tìm kiếm & Bộ lọc, Composite Index & EXPLAIN ANALYZE

**Phần 1 – Chức năng hoàn thành trong buổi:**
- Giao diện `FR-SRCH-001`: thanh tìm kiếm + trang `/search` (chuyển từ Buổi 4).
- Giao diện `FR-SRCH-002/003/004`: `FilterPanel`, `SortSelect`, `Pagination` đồng bộ URL, kèm **retrofit D-10 phía FE**.
- Composite index + đo `EXPLAIN ANALYZE` (NFR-PERF-004).

*(API lọc/sắp xếp/phân trang, `RecipeFilterSpec`, `SortMapper` và D-10 phía API đã làm ở **Buổi 4 — Dev 3**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4 (Dev 3).** `SortMapper` (tham chiếu cho giải trình ở Phần 3):
   ```csharp
   private static readonly Dictionary<string, Expression<Func<Recipe, object>>> Allowed = new(StringComparer.OrdinalIgnoreCase)
   {
       ["createdAt"]   = r => r.CreatedAt,
       ["publishedAt"] = r => r.PublishedAt!,
       ["title"]       = r => r.Title,
       ["cookTime"]    = r => r.CookTime,
       ["prepTime"]    = r => r.PrepTime,
   };
   ```
2. **Index + kiểm chứng hiệu năng:** migration **`B5_Search_CompositeIndexes`** tạo 3 composite index của SRS §7.2 — vì Buổi 2 **chỉ có index đơn cột** và chưa buổi nào tạo chúng:
   - `(IsDeleted, Status, PublishedAt DESC)`
   - `(Status, CategoryId, PublishedAt DESC)`
   - `(Status, CookTime)`

   Chạy `EXPLAIN ANALYZE` trên **bộ dữ liệu ≥ 10.000 recipe** (seeder riêng `PerformanceSeeder`, chỉ bật bằng cờ cấu hình). Với 100 bản ghi seed (CR-2026-03), PostgreSQL **luôn chọn Seq Scan** vì quét cả bảng rẻ hơn dùng index, nên kết quả đo trên dữ liệu nhỏ không chứng minh được gì.
   - Nếu sort mặc định `createdAt` vẫn cho Seq Scan + Sort, bổ sung `(Status, CreatedAt DESC)` — đúng tinh thần NFR-PERF-004 *"chứng minh bằng đo đạc, không tạo index theo cảm tính"*.
   - Ghi kết quả vào `docs/explain-analyze-b5.md`.
3. **UI tìm kiếm (chuyển từ Buổi 4):** `SearchBar.tsx` trong header (debounce 300ms, Enter → điều hướng `/search?q=`); trang `/search` (SSR) hiển thị kết quả + highlight từ khóa + empty state kèm gợi ý.
4. **UI — đồng bộ state với URL:**
   - `FilterPanel`: danh mục, độ khó, thời gian nấu, thời gian chuẩn bị, khẩu phần.
   - `SortSelect`: dropdown "Cột" + "Chiều" ánh xạ **1-1** với `sortBy`/`sortOrder`.
   - `Pagination`.
   - **Toàn bộ state nằm trong `searchParams`** — link chia sẻ được, back/forward hoạt động đúng, SSR đọc được ngay.
   - **Retrofit D-10 phía FE:** `app/recipes/(list)/page.tsx` bỏ `query.set('sort', s)` và mặc định `'-createdAt'`, chuyển sang cặp `sortBy`/`sortOrder`.
   - Dùng chung trên `/recipes`, `/search`, `/categories/[slug]`; mobile hiển thị filter dạng drawer.
5. **Kiểm chứng:**
   - tìm "pho" trên giao diện ra "Phở bò";
   - F5 và back/forward giữ nguyên bộ lọc;
   - `EXPLAIN ANALYZE` cho Index Scan trên ≥ 10.000 bản ghi.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình chuẩn hóa Sort — vì sao `sortBy` + `sortOrder` thắng `sort=-field`:** Hai tham số độc lập cho phép viết **hai rule FluentValidation tách biệt** (`sortBy ∈ whitelist`, `sortOrder ∈ {asc,desc}`), dễ đọc và dễ kiểm chứng hơn hẳn việc tự parse tiền tố `-`. Quan trọng hơn về mặt vận hành: dấu `-` **rất dễ bị nuốt mất khi quên URL-encode**, và khi đó `sort=-createdAt` biến thành `sort= createdAt` → hệ thống sắp xếp tăng dần thay vì giảm dần, **không có lỗi nào được ném ra** — chỉ sai thứ tự âm thầm, loại bug khó phát hiện nhất khi test thủ công. Về phía Frontend, cặp `sortBy`/`sortOrder` ánh xạ 1-1 với cặp dropdown "cột"/"chiều" nên không cần lớp chuyển đổi nào. Nhược điểm duy nhất là muốn sắp xếp nhiều cột phải dùng mảng — nhu cầu không tồn tại trong phạm vi hệ thống này.

**Whitelist là rào chắn bảo mật, không chỉ là validation:** `sortBy` là tên cột, và tên cột **không tham số hóa được** trong SQL. Nếu ghép chuỗi trực tiếp (`ORDER BY {sortBy}`) thì đây là lỗ hổng SQL injection kinh điển. Ánh xạ qua dictionary ở tầng Application đảm bảo giá trị đi vào truy vấn **chỉ có thể là một trong 5 biểu thức đã viết sẵn** — kể cả khi validator bị bỏ sót ở đâu đó.

**Trả 400 cho giá trị ngoài whitelist thay vì im lặng dùng giá trị mặc định:** im lặng có vẻ "thân thiện" hơn nhưng thực ra tệ hơn — FE gửi sai tên trường sẽ không bao giờ biết mình sai, và bug tồn tại cho tới khi có người tinh mắt phát hiện thứ tự hiển thị không đúng.

**`RecipeFilterSpec` dùng chung cho cả bốn query** (làm ở Buổi 4) tránh việc bốn nơi tự implement bốn bộ lọc hơi khác nhau — tình huống rất dễ xảy ra khi bốn màn hình được làm ở bốn thời điểm. Từ Buổi 4, Dev 3 sở hữu toàn bộ phía đọc công thức, nên không còn phải phối hợp với Dev 2 khi sửa `GetRecipesQuery`.

**`pageSize = 12`** chia hết cho lưới 2, 3 và 4 cột — mọi breakpoint responsive đều cho hàng cuối đầy đủ, không có ô trống lẻ loi.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-001 và FR-SRCH-003 dùng **một tham số** `sort=title` / `sort=-title`, trong khi Chương 8 (Quy ước Pagination, `/recipes`, `/categories/{slug}`) dùng **hai tham số** `sortBy=createdAt&sortOrder=desc`. (b) `pageSize` mặc định là **12** ở FR-SRCH-004 nhưng **10** ở Chương 8. (c) Bộ lọc lệch: FR-SRCH-002 có `maxCookTime`/`minServings` còn Chương 8 có `minPrepTime`/`maxPrepTime`. (d) `difficulty` filter chỉ nhận `{Easy|Medium|Hard}` trong khi enum có **4** giá trị (thiếu `Expert`). (e) NFR-PERF-004 đòi *"mọi cột WHERE/ORDER BY đều có B-tree index"* nhưng schema **thiếu index cho đúng những cột đó** (`CreatedAt`, `CookTime`, `PrepTime`, `Servings`).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-01, MT-20.13, MT-20.14, MT-20.15, MT-26**) chốt **`sortBy` + `sortOrder`** với whitelist 5 trường, **`pageSize` = 12**, bộ lọc đầy đủ 5 tham số, bổ sung **`Expert`**, và thay yêu cầu index bằng tiêu chí **đo được qua `EXPLAIN ANALYZE`** với 3 composite index.
- **Tại sao chọn:** Điểm (e) đáng chú ý về tư duy dài hạn. Yêu cầu cũ *"mọi cột WHERE/ORDER BY đều có B-tree index"* nghe hợp lý nhưng dẫn tới việc tạo một loạt index đơn cột — làm **chậm mọi lệnh ghi** và tốn dung lượng, trong khi PostgreSQL thường chỉ chọn **một** index cho một truy vấn nên phần lớn số index đó nằm không. Ba composite index của v1.1.0 được chọn theo **hình dạng truy vấn thật** (mọi truy vấn danh sách đều lọc `Status = Published` trước rồi mới sort), nên phủ đúng nhu cầu với số index ít hơn và hiệu quả cao hơn. Tiêu chí nghiệm thu cũng đổi từ *"có index hay không"* (dễ tick nhưng không chứng minh được gì) sang *"`EXPLAIN ANALYZE` phải cho Index Scan, không chấp nhận Seq Scan trên `Recipes`"* — vừa đo được, vừa không ép tạo index thừa.

**Phần 5 – Kết quả Commit Git:**
```
feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze
```

## DEV 4 — Health Checks (hoàn tất: Docker healthcheck, định tuyến khi lỗi & Health UI)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Hoàn tất `FR-OBS-001` — phần còn lại sau khi **ba endpoint** `/health`, `/health/live`, `/health/ready` đã làm ở **Buổi 3** (yêu cầu bổ sung 29/09/2026, xem Buổi 3 — Dev 4 bước 4): nối endpoint vào khối `healthcheck:` của Docker Compose, `proxy_next_upstream` ở Nginx, và Health UI Indicator.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Checks — đã có từ Buổi 3, giữ nguyên:** `AspNetCore.HealthChecks.NpgSql` và `.Redis` gắn **tag `"ready"`** (hai dependency mà thiếu là API không phục vụ được); `MinioHealthCheck` tự viết (timeout 3s, `failureStatus: Degraded`) **không** gắn tag `ready` — mất MinIO thì ảnh không hiển thị nhưng API vẫn đọc/ghi công thức được.
2. **Endpoints — đã có từ Buổi 3, giữ nguyên:** `/health` → tất cả checks, JSON `{ status, entries }`, **503 khi Unhealthy**; `/health/live` → không chạy check nào, **luôn 200** trừ khi process chết; `/health/ready` → chỉ check có tag `ready`. Integration test của ba endpoint cũng đã có từ Buổi 3.
3. **Docker healthcheck — bổ sung phần còn thiếu (đúng bảng healthcheck của SRS v1.2.0 §6.5 — MT-51; bảng của v1.1.0 dùng `curl` không chạy được):** `postgres` (`pg_isready -h 127.0.0.1`), `redis` (`redis-cli ping`), `minio` (`mc ready local`) **đã có healthcheck từ Buổi 2** — giữ nguyên các lệnh đã kiểm chứng. Bổ sung cho **`api`**: `CMD curl -fsS http://127.0.0.1:8080/health/ready` (`interval: 10s`, `timeout: 3s`, `retries: 3`, `start_period: 30s`) — ⚠️ image `aspnet:10.0` **không có `curl`**, phải thêm `apt-get install -y --no-install-recommends curl` ở stage runtime của `backend/Dockerfile`. Bổ sung cho **`frontend`**: `CMD wget -qO- http://127.0.0.1:3000 >/dev/null` (`node:22-alpine` chỉ có `wget` của busybox). `nginx` đổi `depends_on` sang `api: { condition: service_healthy }`. Trong `nginx.conf`, thêm `proxy_next_upstream error timeout http_502 http_503 http_504;` vào `location /api/` — cơ chế này **vẫn hoạt động với resolver động của Buổi 2**, vì khi tên `api` phân giải ra nhiều địa chỉ, Nginx tự luân phiên và chuyển sang địa chỉ kế tiếp khi một instance lỗi.
4. **UI:** `app/api/health/route.ts` proxy sang API; component `HealthIndicator.tsx` (chấm xanh/vàng/đỏ, poll 30s) hiển thị trên dashboard Admin; trang `/dashboard/system` liệt kê trạng thái từng dependency kèm thời gian phản hồi.
5. **Kiểm chứng:** `docker compose stop redis` → `/health/ready` trả **503** và container `api` chuyển sang `unhealthy`; `/health/live` **vẫn 200**; `HealthIndicator` chuyển đỏ trong ≤ 30s. Khởi động lại Redis → tự hồi phục.
6. **Hiển thị ảnh đã resize (chuyển từ Buổi 4 — FR-JOB-002):** `RecipeCard` dùng `ThumbnailUrl`, fallback `OriginalUrl` khi job chưa chạy xong (còn `null`). Việc này xác nhận trên giao diện lưới rằng job resize của Buổi 4 thật sự được dùng.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Ba endpoint cho ba mục đích khác nhau, không phải ba bản sao.** `/health/live` trả lời câu hỏi *"process này còn sống không?"* — nếu không thì phải **restart container**. `/health/ready` trả lời *"instance này có sẵn sàng nhận traffic không?"* — nếu không thì phải **ngừng route traffic tới nó**, nhưng **không** restart. Phân biệt này rất quan trọng: nếu gộp làm một, thì Redis chết sẽ khiến toàn bộ container API bị restart liên tục — trong khi vấn đề nằm ở Redis, và restart API không giải quyết được gì mà còn làm mất luôn các job đang chạy.

**MinIO không gắn tag `ready`** theo cùng logic: đó là **degradation**, không phải **outage**. Người dùng vẫn đọc được công thức, chỉ là ảnh không tải lên được. Loại bỏ instance khỏi pool vì lý do này là phản ứng thái quá.

**Khối `healthcheck:` trong Docker Compose là phần bắt buộc, không phải trang trí.** Đây là điểm dễ bị bỏ sót nhất: viết xong ba endpoint mà không cấu hình `healthcheck:` thì **không có ai gọi chúng cả** — chúng chỉ là ba URL đẹp. Docker Compose, khác với Kubernetes, không tự biết đường tìm `/health/ready`; phải khai báo tường minh thì mới có hành vi "ngừng route traffic khi DB down" mà NFR-REL-001 yêu cầu.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** **Kubernetes xuất hiện ở 3 chỗ** dù hạ tầng chỉ có Docker Compose: FR-OBS-001 ghi *"Readiness fail khi DB/Redis down → **Kubernetes**/Nginx ngừng route traffic"*; NFR-REL-001 ghi *"`/health/ready` probe mỗi 10 giây (**Kubernetes readiness probe**)"*; NFR-SEC-007 ghi *"Production: ... / **Kubernetes Secrets**"*. Trong khi CONS-009 và mục 6.5 chỉ có Docker Compose, **không có manifest, Helm chart hay FR nào về K8s**.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-29**) gỡ bỏ mọi nhắc đến Kubernetes và **bổ sung khối `healthcheck:`** cho `api`/`postgres`/`redis` vào mục 6.5, kèm `depends_on: condition: service_healthy` và Nginx `proxy_next_upstream`.
- **Tại sao chọn:** Điểm mấu chốt **không phải** là việc xóa chữ "Kubernetes" cho sạch tài liệu — mà là: **Docker Compose không tự đọc `/health/ready`** như Kubernetes. Nếu chỉ xóa chữ K8s mà không bổ sung `healthcheck:`, thì cơ chế *"ngừng route traffic khi DB down"* mà FR-OBS-001 và NFR-REL-001 cùng hứa hẹn sẽ **không có ai thực thi** — hai yêu cầu đó trở thành không nghiệm thu được. Đây chính là loại mâu thuẫn "yêu cầu mồ côi": một yêu cầu tồn tại ở chương này nhưng không có cơ chế nào ở chương khác đỡ được nó. Phương án đưa K8s vào phạm vi bị loại vì quá lớn so với 8 buổi; phương án ghi chú *"định hướng tương lai"* bị loại vì vẫn để lại lỗ hổng thực thi.

**Phần 5 – Kết quả Commit Git:**
```
feat(obs): complete FR-OBS-001 docker healthcheck wiring, nginx failover and health ui indicator
```

---

# BUỔI 6 – SECURITY HARDENING, DASHBOARD CÁ NHÂN & XÓA MỀM (GIAO DIỆN), ISR/SEO & STRUCTURED LOGGING

> **Mục tiêu buổi:** khóa chặt bề mặt tấn công, Author quản lý được công thức của mình trên dashboard riêng tư, trang công khai đạt chuẩn SEO, và hệ thống quan sát được từ bên ngoài.
>
> *(Cập nhật 29/09/2026: các retrofit D-3, D-4, D-5, D-6 và lỗ hổng MT-34 đã được vá ở **Buổi 4**, cùng các endpoint `DELETE /recipes/{id}` và `GET /recipes/mine` — sớm hơn hai buổi. Buổi này chỉ còn D-7 phía FE.)*

## DEV 1 — Security Hardening

**Phần 1 – Chức năng hoàn thành trong buổi:**
Rate Limiting theo IP thật (dựa trên `UseForwardedHeaders` đã bật ở Buổi 4) + Policy-based Authorization toàn diện + Content-Security-Policy + Route Guards phía Frontend (NFR-SEC-003/005/006).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Rate limiting:** `AddRateLimiter` với 3 policy — `"auth"` sliding window **10 req/phút/IP** cho `/auth/*`, `"upload"` fixed window **5 req/phút/IP** cho `/files/upload` và `/recipes/{id}/images`, global fixed window **100 req/phút/IP**. `OnRejected` trả **429** + `Retry-After` + ProblemDetails `RATE_LIMIT_EXCEEDED`; thêm header `X-RateLimit-Limit/Remaining/Reset` (SRS §5.2). Partition key lấy từ `context.Connection.RemoteIpAddress` — giá trị này đã là IP thật nhờ `UseForwardedHeaders` (Buổi 4). **Thứ tự pipeline bắt buộc:** `UseForwardedHeaders` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization`; thêm test kiến trúc đọc thứ tự này để không ai vô tình đảo lại.
2. **Hai lớp giới hạn, hai vai trò:** Nginx đã có `limit_req_zone ... rate=100r/m` từ Buổi 2 làm **lớp chặn thô ở vòng ngoài** (rẻ, chặn sớm trước khi request tới .NET); lớp ASP.NET Core là **lớp nghiệp vụ** (phân biệt `/auth`, `upload`, trả ProblemDetails + header `X-RateLimit-*` đúng SRS). Tài liệu hóa trong `nginx.conf` rằng ngưỡng Nginx phải **≥** ngưỡng ứng dụng, để người dùng luôn nhận lỗi 429 có cấu trúc từ ứng dụng thay vì trang 503 trống của Nginx.
3. **Rà soát phân quyền toàn hệ thống:** mọi endpoint ghi đều gắn `AuthorPolicy`/`AdminPolicy`; **không còn tham chiếu nào tới policy `VerifiedAuthor`**; `IsActive == false → 403 AUTH_ACCOUNT_DISABLED` có mặt ở **cả** login (**đã có từ Buổi 2** — `LoginUserCommand.cs:42`) **và** refresh (Buổi 4).
4. **Security headers + CSP + CORS:** CORS whitelist đã có từ Buổi 2 — chỉ rà lại không có `*`. Bổ sung **Content-Security-Policy** chặt (`script-src 'self'` + nonce cho script inline của Next.js, `object-src 'none'`, `frame-ancestors 'none'`), `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`. CSP là **lớp phòng vệ cuối cho refresh token** — thứ buộc phải nằm ở nơi JavaScript đọc được theo SRS §5.2 (xem giải trình D-12, Buổi 4). HSTS + redirect HTTP→HTTPS cấu hình ở Nginx (Dev 4, Buổi 7).
5. **UI Route Guards:** `middleware.ts` của Next.js bảo vệ `/dashboard/**` và `/profile` (chưa đăng nhập → redirect `/auth/login?callbackUrl=`), `/dashboard/categories` và `/dashboard/users` chỉ Admin; `/auth/*` redirect về `/dashboard` nếu đã đăng nhập; component `<RequireRole role="Admin">` cho phần tử UI lẻ. Integration test: gửi 11 request login liên tiếp → request thứ 11 nhận **429**; Author gọi endpoint Admin → **403**.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình vì sao rate limiting phải đứng trên `UseForwardedHeaders` (đã bật ở Buổi 4):** khi API đứng sau Nginx mà không cấu hình header chuyển tiếp, `HttpContext.Connection.RemoteIpAddress` trả về **IP của container Nginx** — giống hệt nhau cho mọi người dùng. Hậu quả không phải là "rate limit kém chính xác" mà nghiêm trọng hơn nhiều: rate limit **biến thành giới hạn toàn hệ thống**. Tổng cộng 10 request/phút tới `/auth/*` cho **tất cả** người dùng cộng lại — chỉ cần vài người đăng nhập cùng lúc là cả hệ thống nhận 429. Một biện pháp bảo mật tự biến thành lỗi từ chối dịch vụ do chính mình gây ra. Song song đó, cột `RefreshToken.CreatedByIp` mất sạch giá trị audit vì mọi token đều ghi cùng một IP nội bộ (`172.18.0.5`), không truy vết được ai.

**Vì sao danh sách mạng tin cậy (`KnownIPNetworks`) là bắt buộc, không phải tùy chọn:** header `X-Forwarded-For` do client gửi lên nên **giả mạo được**. Nếu bật `UseForwardedHeaders` mà không giới hạn proxy tin cậy, kẻ tấn công chỉ cần gửi `X-Forwarded-For: <IP ngẫu nhiên>` mỗi request là **né được hoàn toàn rate limit** — tức là bật forwarded headers sai cách còn **tệ hơn** không bật. Khai báo dải mạng Docker nội bộ nghĩa là: chỉ tin header này khi request đến từ Nginx của chính ta. (SRS NFR-SEC-003 dùng tên khái niệm `KnownProxies`; trên .NET 10 thuộc tính đúng cho một **dải** mạng là `KnownIPNetworks` — `KnownProxies` chỉ nhận từng IP cố định, không phù hợp vì IP container Docker thay đổi mỗi lần tạo lại.)

**Thứ tự middleware quan trọng:** `UseForwardedHeaders` phải đứng **trước** `UseRateLimiter` và `UseAuthentication`. Đặt sai thứ tự thì rate limiter đọc IP trước khi nó được sửa lại — cấu hình đúng mà vẫn không có tác dụng, và đây là lỗi rất khó nhận ra vì không có thông báo nào.

**Sliding window cho `/auth/*` thay vì fixed window:** với fixed window, kẻ tấn công có thể bắn 10 request ở giây cuối cửa sổ này và 10 request ở giây đầu cửa sổ sau — tức 20 request trong 2 giây. Sliding window chặn được mẫu tấn công đó. Với API chung thì fixed window là đủ và rẻ hơn về bộ nhớ.

**Route guard ở FE là trải nghiệm, không phải bảo mật.** `middleware.ts` chỉ để người dùng không thấy màn hình trống rồi mới bị lỗi. Quyền thật luôn được kiểm tra ở Application Layer (NFR-SEC-006) — FE có thể bị bypass bằng cách gọi API trực tiếp, nên **không bao giờ được coi guard phía client là lớp bảo vệ**.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) NFR-SEC-003 quy định rate limit **theo IP**, Chương 7.8 có cột `CreatedByIp` *"lưu để audit"*, và Chương 6 vẽ mọi request đi qua Nginx — nhưng **không chỗ nào trong toàn tài liệu** đề cập `UseForwardedHeaders`, `X-Forwarded-For`, `X-Real-IP` hay `KnownProxies`. Hai bên đều đúng khi đọc riêng, chỉ sai khi ghép lại. (b) Mục 2.3 khai báo policy `"VerifiedAuthor"` *yêu cầu email đã xác nhận*, trong khi **không có FR nào** cho việc gửi/xác nhận email, và FR-AUTH-001 thì auto-login ngay sau đăng ký. (c) Cột `IsActive` và mã lỗi `AUTH_ACCOUNT_DISABLED` tồn tại nhưng FR-AUTH-002 **không kiểm tra cờ này**.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-38, MT-21, MT-22**) bổ sung yêu cầu bắt buộc bật `UseForwardedHeaders` với `KnownProxies` vào NFR-SEC-003; **loại bỏ policy `VerifiedAuthor`** khỏi phạm vi; bổ sung bước kiểm tra `IsActive` vào FR-AUTH-002 và FR-AUTH-004.
- **Tại sao chọn:** Về (b) — đây là loại lỗi làm **tắc toàn bộ luồng chính** nhưng chỉ lộ ra khi test end-to-end. `EmailConfirmed` của ASP.NET Core Identity mặc định là `false`; nếu gắn policy `VerifiedAuthor` vào các endpoint ghi thì **không người dùng nào tạo được công thức**, vì không tồn tại đường nào chuyển cờ đó thành `true`. Ba phương án: *(A) bỏ policy khỏi v1.0*, *(B) bổ sung đầy đủ luồng xác nhận email (2 FR + 2 endpoint + template + SMTP chạy thật khi demo)*, *(C) giữ policy nhưng seed `EmailConfirmed = true` khi đăng ký*. Chọn **A**: phương án C tệ hơn cả bỏ hẳn vì policy luôn đúng sẽ **gây hiểu nhầm là có bảo vệ** trong khi thực chất không bảo vệ gì; phương án B vượt phạm vi và biến SMTP thành điểm chết khi demo. Về (c): thiếu bước kiểm tra thì `AUTH_ACCOUNT_DISABLED` là **mã lỗi chết** — Admin khóa tài khoản trong DB mà người dùng vẫn đăng nhập bình thường.

**Phần 5 – Kết quả Commit Git:**
```
feat(security): ip-based rate limiting, content security policy, authorization audit and frontend route guards
```

## DEV 2 — Dashboard "Công thức của tôi" & Xóa mềm (Giao diện)

**Phần 1 – Chức năng hoàn thành trong buổi:**
Giao diện của `FR-RCP-011` (dashboard **"Công thức của tôi"**) và `FR-RCP-007` (xóa mềm có xác nhận).

*(API đã làm ở **Buổi 4**:*
- *`GET /recipes/mine` và retrofit D-4/D-6 (MT-34) — **Dev 3**;*
- *`DELETE /recipes/{id}` và retrofit D-3 — **Dev 4**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4:**
   - `GET /api/v1/recipes/mine?status&page&pageSize&sortBy&sortOrder` (`Cache-Control: no-store`);
   - `DELETE /api/v1/recipes/{id}` (204);
   - `PATCH /api/v1/recipes/{id}/publish|unpublish`.
2. **UI `/dashboard/recipes`:**
   - Bảng công thức của tôi gọi **`/recipes/mine`** (không bao giờ gọi `GET /recipes`).
   - Tab Draft/Published/Archived; tab Archived có nút thao tác từ Buổi 7.
   - Action Edit / Publish / Unpublish / Delete; hộp xác nhận xóa yêu cầu **gõ đúng tên recipe**.
   - Optimistic update + rollback khi API lỗi.
   - TanStack Query **không** dùng chung cache key giữa `/recipes/mine` và `/recipes` — tránh cache phía client trộn dữ liệu riêng tư với công khai.
3. **UI `/dashboard`:** thống kê số công thức theo trạng thái bằng cách đọc `totalCount` của `/recipes/mine?status=X&pageSize=1` cho từng trạng thái — không cần endpoint thống kê riêng.
4. **Kiểm chứng:**
   - xóa một công thức → biến mất khỏi dashboard và khỏi `/recipes`;
   - tạo lại cùng tên → đường dẫn cũ dùng lại được;
   - DevTools: response `/recipes/mine` có `Cache-Control: no-store`.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình No-Cache cho dữ liệu riêng tư — đây là trọng tâm của buổi:** nguyên tắc kiến trúc được chốt trong NFR-SEC-006 là *"không bao giờ đặt dữ liệu phụ thuộc danh tính vào cache dùng chung dưới khóa công khai"*. Lý do sâu xa: **cache nằm TRƯỚC handler**. Khi một response đã nằm trong cache, request thứ hai được trả thẳng từ bộ nhớ và **không bao giờ chạy tới handler** — nghĩa là mọi đoạn code kiểm tra quyền trong handler đều bị bỏ qua. Kiểm tra phân quyền hoàn hảo đến đâu cũng vô nghĩa nếu nó không được thực thi.

Từ đó suy ra hai loại endpoint không thể trộn lẫn: **endpoint công khai** trả cùng một response cho mọi người gọi → cache thoải mái, hit rate cao; **endpoint riêng tư** trả response khác nhau tùy người gọi → cấm cache tuyệt đối, gắn `Cache-Control: no-store` để cả proxy trung gian và trình duyệt cũng không lưu.

Phương án thay thế **thêm `userId` vào khóa cache** (`VaryByValue`) bị loại vì hai lý do: hit rate sụp đổ (mỗi user một bản cache, với 5.000 user thì cache phình to mà gần như không bao giờ trúng), và nguy hiểm hơn — nó **để ngỏ khả năng sai sót**: chỉ cần một endpoint nào đó quên khai báo `VaryBy` là lỗ hổng quay lại nguyên vẹn. Tách bạch về mặt kiến trúc thì **không thể quên**, vì hai loại dữ liệu đi qua hai đường khác nhau.

**Soft Delete và ba hệ quả bắt buộc:** (1) **Partial unique index** — nếu giữ unique thường thì slug của recipe đã xóa chiếm chỗ vĩnh viễn, người dùng không bao giờ tạo lại được công thức cùng tên. (2) **Child entity không xóa theo** — chúng vô hình cùng recipe qua Global Query Filter; FK `ON DELETE CASCADE` giữ lại chỉ để phòng khi hard delete thật sự (job purge). (3) **Job xóa file MinIO KHÔNG chạy khi soft delete** — nếu chạy, khôi phục recipe sẽ mất toàn bộ ảnh, tức là "khôi phục được" trở thành lời hứa suông. Việc dọn file thuộc về FR-JOB-003 sau 30 ngày.

**Draft trả 404 thay vì 403 ở endpoint công khai:** 403 nói với người lạ rằng *"có một bản nháp ở đúng slug này, chỉ là bạn không được xem"* — tức là **tiết lộ sự tồn tại** của nội dung chưa công bố (ví dụ tên món sắp ra mắt), và cho phép dò slug hàng loạt. Với endpoint chỉ phục vụ dữ liệu công khai, một bản nháp đơn giản là **không tồn tại** trong không gian đó; chủ sở hữu xem nó qua `/recipes/mine`.

**Vì sao chuyển trọn `FR-RCP-006` sang Buổi 7 (quyết định trước 29/09; từ Buổi 4, cả API archive/unarchive lẫn xóa mềm do Dev 4 làm cùng buổi, nên phía API của FR-RCP-006 vẫn không bị cắt đôi):** lộ trình ban đầu đặt Archive ở Buổi 6 và Unarchive ở Buổi 7 — tức là **cắt đôi một FR qua hai buổi**, và trong suốt một buổi hệ thống có trạng thái Archived **vào được mà không ra được** — đúng cái "hố đen" mà MT-35 vừa sửa trong SRS. Đồng thời Buổi 6 của Dev 2 đã là buổi nặng nhất (hai FR mới + ba hạng mục nợ, trong đó có lỗ hổng bảo mật). Gom Archive + Unarchive + kiểm thử ma trận trạng thái vào một buổi giúp FR-RCP-006 được giao **trọn gói DB → API → UI**, và buổi này tập trung đúng vào việc quan trọng nhất: vá MT-34.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) **Mâu thuẫn nặng nhất toàn tài liệu** — FR-RCP-007 ghi *"Xóa **vĩnh viễn**... Đây là **hard delete** (không dùng soft delete pattern cho recipe)"* với cascade delete và xóa file MinIO qua Hangfire; trong khi **4 chỗ khác** nói ngược lại: Chương 7 (mọi entity kế thừa `BaseEntity` có `IsDeleted` + Global Query Filter), NFR-REL-003 (*"Recipe được đánh dấu `IsDeleted` thay vì xóa vật lý, **có thể khôi phục**"*), Chương 8 (*"Xóa recipe (**soft delete**)"*), Phụ lục A (*"404 = ... hoặc đã soft-delete"*). Tỷ lệ **4 soft / 1 hard**. (b) Lỗi MT-34 đã phân tích ở Buổi 2 — FR-RCP-001 lọc theo danh tính nhưng cache theo khóa công khai; FR-RCP-002 A2 trả 403 cho Draft.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-05, MT-34**) chốt **Soft Delete đồng bộ toàn hệ thống** với 3 hệ quả bắt buộc (partial unique index, child entity không xóa theo, không xóa file khi xóa mềm); **tách bạch Public/Private** với `GET /recipes/mine` cấm cache; Draft ở endpoint công khai trả **404**.
- **Tại sao chọn:** Về (a) — hard delete cho riêng Recipe sẽ **phá vỡ thiết kế `BaseEntity`** (Recipe có cột `IsDeleted` mà không dùng đến, gây hiểu nhầm cho mọi người đọc code sau này), làm mất dữ liệu không khôi phục được, và buộc phải sửa 4 mục tài liệu khác cho khớp. Ba phương án cân nhắc: *(A) soft delete toàn bộ*, *(B) hard delete riêng Recipe*, *(C) soft delete + job dọn vĩnh viễn sau N ngày*. Chọn **A làm nền + C làm phần mở rộng** (chính là FR-JOB-003 ở Buổi 4) — được cả "thùng rác khôi phục được" như sản phẩm thật lẫn việc giải phóng dung lượng đúng lúc. Về (b) — xem giải trình đầy đủ ở Phần 3 và ở Buổi 2 Dev 2.

**Phần 5 – Kết quả Commit Git:**
```
feat(recipes-ui): my-recipes dashboard with status tabs, confirmed soft delete and per-status stats
```
> *(Commit `fix!` vá MT-34 nay nằm ở Buổi 4 — Dev 3.)*

## DEV 3 — Trang chủ, ISR & SEO Structured Data

**Phần 1 – Chức năng hoàn thành trong buổi:**
Trang chủ + Trang danh mục (ISR đúng chuẩn) + JSON-LD Schema.org Recipe + Open Graph metadata (NFR-SEO-001/002/004).
**Kèm retrofit D-7.** *(D-5 đã vá ở Buổi 4 — Dev 3.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-5 — đã xong ở Buổi 4 (Dev 3):** `GetCategoryBySlugQuery` chỉ trả recipe `Status == Published`, không đọc `ICurrentUser`, cache khóa `categories:detail:{slug}:{queryHash}` TTL 2 phút.
2. **Retrofit D-7 — chuẩn hóa thời gian tái sinh dữ liệu theo bảng TTL §2.3:** `/categories` đổi `fetch(..., { next: { revalidate: 3600 } })` → **`1800`**; `/categories/[slug]` đổi `600` → **`120`**. Lưu ý: hai trang này **render động** (Buổi 2 đã giải thích — trang chi tiết đọc `?page=` nên Next.js bắt buộc render theo request), nên con số cần sửa là **`revalidate` của Data Cache trên lời gọi `fetch`**, không phải `export const revalidate` của trang. `/recipes/[slug]` (ISR thuần) giữ **300**; trang chủ `/` (Buổi 6 mới dựng) đặt **120**.
3. **Trang chủ `/`:** hero + **"Công thức mới xuất bản"** gọi `GET /recipes?sortBy=publishedAt&sortOrder=desc&pageSize=8` + lưới danh mục; `next/image` với `sizes` và `priority` cho ảnh LCP.
4. **JSON-LD — `lib/seo/recipeJsonLd.ts`:** sinh `@type: "Recipe"` với `name`, `description`, `image`, `author`, **`datePublished`** (từ `PublishedAt` — nay đã được gán đúng nhờ FR-RCP-005), `prepTime`/`cookTime`/`totalTime` định dạng **ISO 8601 duration** (`PT15M`), `recipeYield`, **`recipeIngredient[]`** (ưu tiên `quantityText`, không có thì ghép `quantity + unit`), `recipeInstructions[]` dạng `HowToStep`, `nutrition` dạng `NutritionInformation`. **Không nhúng `aggregateRating`.**
5. **Metadata + kiểm chứng:** `generateMetadata()` cho recipe/category/home — `<title>` = `"{Tên} | Culinary Blog"` **cắt ở 60 ký tự khi render** (cắt theo ranh giới từ, thêm `…`), `<meta description>` lấy **160 ký tự đầu** của `Description`, Open Graph (`og:image` 1200×630), Twitter `summary_large_image`, canonical slug-URL. **Yêu cầu `noindex` cho Draft/Archived (NFR-SEO-002)** giờ được đáp ứng bằng cơ chế mạnh hơn: sau D-4, API công khai trả **404** cho Draft/Archived nên `/recipes/[slug]` gọi `notFound()` → trả **HTTP 404** (search engine không bao giờ lập chỉ mục trang 404); còn các trang dashboard/preview — nơi duy nhất hiển thị Draft — đặt `robots: { index: false }` và bị chặn trong `robots.txt` (Buổi 7). Kiểm chứng bằng **Google Rich Results Test** — phải pass 100% cho loại Recipe.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giới hạn SEO là quy tắc render, không phải ràng buộc dữ liệu.** `<title>` yêu cầu ≤ 60 ký tự, mà hậu tố `" | Culinary Blog"` đã chiếm 16 → tên công thức chỉ còn ≤ 44 ký tự, trong khi cột `Title` cho phép 200. Ép người dùng đặt tên món ≤ 44 ký tự để chiều Google là ràng buộc vô lý — "Bún bò Huế chuẩn vị cố đô nấu bằng nồi áp suất" đã 47 ký tự. Cắt chuỗi **lúc render** mới là đúng chỗ của yêu cầu này. Tương tự với `<meta description>` 150–160 ký tự so với `Description` ≤ 2000: lấy 160 ký tự đầu là đủ, và người viết vẫn được mô tả đầy đủ cho phần hiển thị trên trang.

**ISR `revalidate` phải ≤ TTL cache API tương ứng.** Nếu ISR dài hơn, Next.js trở thành **tầng cache cũ nhất** trong chuỗi và mọi nỗ lực invalidate ở Backend đều vô nghĩa — Admin sửa danh mục, Redis được xóa khóa ngay, nhưng trang tĩnh Next.js vẫn phục vụ bản cũ thêm 50 phút nữa. Đây là lý do `/categories` phải hạ từ 3600 xuống 1800 để khớp TTL 30 phút của `categories:all`.

**"Nổi bật" = mới xuất bản nhất** vì bảng `Recipes` không có cột `IsFeatured`/`ViewCount`/`Rating` nào, và Rating/Bookmark đều nằm ngoài phạm vi. Phương án thêm `ViewCount` rồi sắp xếp theo lượt xem bị loại vì phải **ghi DB mỗi lượt truy cập** — tạo write nóng ngay trên đường đọc, phá vỡ cả cache lẫn mục tiêu p95 ≤ 500ms, lại còn cần cơ chế chống spam đếm view. Dùng `PublishedAt` tận dụng ngay index có sẵn, không thêm cột, không thêm endpoint.

**Không nhúng `aggregateRating`** dù rich snippet có sao trông bắt mắt hơn: Rating System nằm ngoài phạm vi nên không có dữ liệu thật, và nhúng dữ liệu giả **vi phạm chính sách structured data của Google** (dữ liệu không phản ánh nội dung thật) — nguy cơ bị phạt toàn site, đánh đổi hoàn toàn không đáng.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) Mục 1.2.3 xếp **Rating System ngoài phạm vi** và danh sách thuộc tính JSON-LD của NFR-SEO-001 **không có `aggregateRating`**, nhưng câu kết lại hứa *"Rich Snippets trên Google Search (**star rating**, time, ingredients)"* — hạng mục này **không thể pass** nghiệm thu, không phải vì làm sai mà vì yêu cầu tự mâu thuẫn với phạm vi. (b) Mục 5.1 ghi trang chủ hiển thị *"danh sách recipe **nổi bật**"* nhưng không có cột, không có tham số API nào lấy được "nổi bật". (c) ISR `/categories` = 3600s trong khi TTL danh mục là 30 phút; `/categories/[slug]` = 600s không khớp chỗ nào. (d) NFR-SEO-002 đòi `<title>` ≤ 60 ký tự và `<meta description>` 150–160 trong khi `Title` cho phép 200 và `Description` tới 2000.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-24, MT-23, MT-33.3, MT-37**) **bỏ "star rating"** khỏi NFR-SEO-001; định nghĩa lại "nổi bật" = **mới xuất bản nhất**; đồng bộ ISR ≤ TTL; chuyển giới hạn SEO thành **quy tắc truncate lúc render**.
- **Tại sao chọn:** Nguyên tắc chung rút ra từ nhóm mâu thuẫn này: **một yêu cầu phi chức năng phải nghiệm thu được**, nếu không nó chỉ là câu văn trang trí. Cả bốn điểm trên đều thuộc loại "QA cầm SRS đi test sẽ không biết phải làm gì": không có `aggregateRating` thì không bao giờ có sao để kiểm; không có trường "nổi bật" thì không dựng được trang chủ đúng đặc tả; ISR lệch TTL thì test invalidate lúc pass lúc fail tùy thời điểm. Sửa theo hướng làm cho yêu cầu **đo được** quan trọng hơn là giữ nguyên câu chữ đẹp.

**Phần 5 – Kết quả Commit Git:**
```
feat(seo): homepage and category isr aligned with cache ttl, json-ld recipe schema and open graph metadata
```

## DEV 4 — Structured Logging & Distributed Tracing

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-OBS-002` Serilog Structured Logging + CorrelationId + `FR-OBS-003` OpenTelemetry Tracing & Metrics.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Serilog:** các package `Serilog.AspNetCore`, `Serilog.Formatting.Compact`, `Serilog.Sinks.Seq` **đã được tham chiếu trong `CulinaryBlog.API.csproj` từ Buổi 2 nhưng chưa được cấu hình** — buổi này nối dây: `builder.Host.UseSerilog(...)` đọc từ `appsettings`, sinks Console (compact JSON), File (rolling daily), Seq (`http://seq:5341`); log level Debug (dev) / Information (prod) / Warning-Error (luôn). `UseSerilogRequestLogging()` ghi method, path, status, elapsed ms. Áp dụng cho **cả hai** container `api` và `hangfire` (cùng image) — log của job nền cũng phải truy vết được.
2. **CorrelationId:** Nginx **đã sinh và chuyển tiếp `X-Correlation-ID`** từ Buổi 2 (`map $http_x_correlation_id $correlation_id` → mặc định `$request_id`). `CorrelationIdMiddleware` đọc header này (sinh mới nếu request đi thẳng không qua Nginx), **trả lại trong response**, và `LogContext.PushProperty` cho `CorrelationId`, `RequestPath`, `UserId` — thỏa CONS-010 (mọi log entry phải có 3 trường này). Khi enqueue job Hangfire, truyền kèm CorrelationId để log trong worker nối được với request đã sinh ra nó.
3. **Hoàn thiện `LoggingBehavior` + audit:** behavior đo elapsed và **cảnh báo khi > 500ms**; audit log mọi write command với `userId` + `timestamp` (NFR-SEC-006). Riêng ngưỡng **> 100ms cho query DB** cài bằng **EF Core `DbCommandInterceptor`**, **không** phải MediatR behavior.
4. **Lọc dữ liệu nhạy cảm:** cấu hình Serilog destructuring policy loại bỏ `password`, `refreshToken`, `idToken`, header `Authorization` khỏi mọi sink — thực thi yêu cầu NFR-SEC-007 *"raw refresh token không bao giờ vào log"*.
5. **OpenTelemetry + FE:** instrumentation AspNetCore/HttpClient/EF Core (Npgsql), `ActivitySource` tùy chỉnh, metrics `recipes_created_total`, `recipes_published_total`, request duration histogram, error rate; OTLP exporter → Seq (dev); enrich log với `TraceId`/`SpanId`. FE: `api-client` gửi `X-Correlation-ID` (uuid) mỗi request, error toast hiển thị mã correlation để tra cứu trên Seq.

**Phần 3 – Định hướng & Lý do thiết kế:**
**CorrelationId là thứ biến log từ "đống text" thành công cụ điều tra.** Khi người dùng báo *"tôi bấm Publish lúc 3 giờ chiều và bị lỗi"*, không có correlation id thì phải mò trong hàng nghìn dòng log quanh thời điểm đó. Có nó — và có nó hiển thị trên toast lỗi ở FE — thì người dùng đọc mã, dev dán vào Seq, ra đúng chuỗi log của request đó xuyên qua mọi tầng.

**Structured logging (JSON) thay vì log text thuần:** log text chỉ đọc được bằng mắt; log có cấu trúc thì truy vấn được (`UserId = 'x' and Elapsed > 500`). Với hệ thống nhiều instance, khả năng truy vấn là khác biệt giữa "có quan sát được" và "có ghi lại nhưng không dùng được".

**Tách hai ngưỡng cảnh báo đúng chủ thể:** `LoggingBehavior` đo **toàn bộ request** qua MediatR (> 500ms), còn `DbCommandInterceptor` đo **một câu lệnh SQL** (> 100ms). Gộp vào một chỗ là sai về mặt kỹ thuật — MediatR behavior không nhìn thấy từng query riêng lẻ, nó chỉ thấy tổng thời gian handler. Ghi rõ điều này vì v1.0.0 mô tả mập mờ khiến hai dev dễ implement trùng hoặc bỏ sót.

**Lọc dữ liệu nhạy cảm khỏi log là phần dễ quên nhất** của toàn bộ chuỗi bảo mật token. Ta đã cẩn thận hash token trước khi lưu DB (FR-AUTH-004), nhưng nếu `LoggingBehavior` log nguyên payload của `RefreshTokenCommand` thì **raw token nằm chình ình trong Seq và trong file log** — và log thường có chính sách lưu trữ dài hơn, quyền truy cập rộng hơn database. Toàn bộ công sức hash trở thành vô ích.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) Mục 6.2 liệt kê 4 behavior gồm **`PerformanceBehavior`** nhưng không có `CacheInvalidationBehavior`; mục 6.3 thì ngược lại — có `CacheInvalidationBehavior` nhưng không có `PerformanceBehavior`. (b) FR-OBS-002 giao việc *"cảnh báo khi request > 500ms"* cho `LoggingBehavior`, còn NFR-PERF-004 nhắc *"Serilog performance behavior"* cho ngưỡng > 100ms của query — **hai ngưỡng, hai chủ thể, không rõ ai làm gì**. (c) Mục 2.3 cho Admin quyền *"Xem structured logs"* nhưng mục 6.5 ghi Seq là *"Dev only — không deploy production"* → ở production Admin không có công cụ nào.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-31, MT-41.9**) chốt **4 behavior** theo đúng thứ tự mục 6.3, **gộp đo hiệu năng vào `LoggingBehavior`** (bỏ `PerformanceBehavior`), làm rõ ngưỡng > 100ms thuộc **EF Core interceptor**, và sửa mục 2.3 thành *"xem logs qua công cụ vận hành (Seq/Elastic — ngoài phạm vi ứng dụng)"*.
- **Tại sao chọn:** Hai danh sách behavior lệch nhau là loại lỗi khiến hai dev implement hai kiến trúc pipeline khác nhau rồi phát hiện xung đột lúc merge. Gộp đo hiệu năng vào `LoggingBehavior` cho **ít lớp hơn** và đúng thực tế: đo thời gian vốn là việc tự nhiên của logging, tách thành behavior riêng chỉ để chạy `Stopwatch` là phân mảnh không cần thiết. Về (c): trung thực hơn là thừa nhận việc xem log production thuộc về công cụ vận hành, thay vì hứa một tính năng trong ứng dụng mà không có FR nào đặc tả.

**Phần 5 – Kết quả Commit Git:**
```
feat(obs): complete FR-OBS-002 serilog correlation logging & FR-OBS-003 opentelemetry with secret redaction
```

---

# BUỔI 7 – QUẢN LÝ NGƯỜI DÙNG, ĐÓNG KÍN MÁY TRẠNG THÁI, SITEMAP NEXT.JS & TỐI ƯU HẠ TẦNG

> **Mục tiêu buổi:** hoàn tất 3 FR cuối cùng (`FR-AUTH-008`, `FR-AUTH-009`, `FR-RCP-006`), đưa sitemap về đúng nơi phục vụ, và tối ưu toàn bộ hạ tầng để sẵn sàng cho kiểm thử tải ở Buổi 8. **Kết thúc buổi này: 37/37 FR hoàn thành.**

## DEV 1 — Giao diện Quản lý Tài khoản [Admin] & Quản lý Phiên Đăng nhập

**Phần 1 – Chức năng hoàn thành trong buổi:**
Giao diện của **`FR-AUTH-008`** Quản lý Tài khoản Người dùng [Admin] (danh sách, khóa/mở khóa, "force revoke") và **`FR-AUTH-009`** Quản lý Phiên Đăng nhập của chính mình.

*(API — `GET /users`, `PATCH /users/{id}/status`, `GET /auth/sessions`, `DELETE /auth/sessions/{id}`, `POST /auth/sessions/revoke-all` — và claim `sid` đã làm ở **Buổi 4 — Dev 1**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4 (Dev 1)**, gồm integration test:
   - khóa user → refresh trả 403;
   - Admin tự khóa → 403;
   - phiên của người khác → 404;
   - `/auth/sessions` không chứa `tokenHash`.
2. **UI `/dashboard/users` (Admin):**
   - Bảng người dùng, ô tìm kiếm (debounce), lọc theo trạng thái, badge trạng thái.
   - Toggle Khóa/Mở khóa với confirm dialog **bắt buộc nhập lý do**.
   - Hàng của chính Admin ẩn nút khóa (server vẫn chặn 403).
3. **UI `/profile` — tab "Phiên đăng nhập":**
   - Liệt kê thiết bị (IP + thời gian), đánh dấu "Thiết bị này" theo `isCurrent`.
   - "Đăng xuất thiết bị này" và "Đăng xuất tất cả" — xong thì xóa phiên cục bộ, về trang đăng nhập.
4. **Kiểm chứng:**
   - Admin khóa một Author → trong ≤ 15 phút phiên của Author đó hết đường gia hạn;
   - thu hồi "thiết bị khác" ở tab phiên → thiết bị đó bị đăng xuất ở lần refresh kế tiếp.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Khóa tài khoản phải revoke toàn bộ refresh token, không chỉ gán cờ.** Nếu chỉ gán `IsActive = false`, người dùng bị khóa vẫn tiếp tục gia hạn phiên bình thường cho tới khi ai đó nhớ ra phải chặn ở luồng refresh. Revoke ngay khiến việc khóa có hiệu lực ở lần gia hạn kế tiếp, và bước kiểm tra `IsActive` ở FR-AUTH-004 (Buổi 4) là lớp chặn thứ hai.

**Access token cũ vẫn sống tối đa 15 phút sau khi khóa** — đây là đánh đổi cố hữu của JWT stateless, và SRS đã ghi rõ là **chấp nhận được** (cũng được ghi là đánh đổi O-1 trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md` §6). Phương án thu hồi tức thời đòi phải tra database ở **mọi** request (kiểm tra `SecurityStamp` hoặc blacklist), tức là vứt bỏ toàn bộ lợi ích của stateless và thêm một round-trip vào đường nóng nhất của hệ thống. Với một blog ẩm thực, 15 phút là chấp nhận được; với hệ thống ngân hàng thì không — đây là quyết định phụ thuộc bối cảnh và cần được ghi lại để người sau hiểu vì sao.

**Chặn Admin tự khóa chính mình** là loại lỗi nghiệp vụ nhỏ nhưng hậu quả lớn: nếu hệ thống chỉ có một Admin và người đó bấm nhầm, **không còn ai mở khóa được** — phải vào thẳng database sửa tay. Một dòng kiểm tra chặn đứng được kịch bản tự khóa cửa. Mã **403** (không phải 400 hay 409) là đúng ngữ nghĩa: yêu cầu hợp lệ về cú pháp và không xung đột trạng thái tài nguyên — chỉ là *người gọi không được phép thực hiện hành động này lên chính mình*.

**Vì sao nhận diện "phiên hiện tại" bằng claim `sid`:** giao diện cần đánh dấu "Thiết bị này" để người dùng không lỡ tay tự đăng xuất. Cách chuẩn (cũng là cách OpenID Connect dùng) là gắn định danh phiên vào chính access token lúc phát — server so `sid` với `id` của từng phiên mà client không phải gửi thêm gì. Các cách khác như bắt client gửi kèm refresh token hoặc một phần hash của nó vừa rườm rà vừa đưa dữ liệu nhạy cảm vào request không cần thiết.

**Tab "Phiên đăng nhập" trong profile** là phần đối xứng dành cho người dùng thường của cùng cơ chế mà Admin có: họ nhìn thấy các thiết bị đang đăng nhập (kèm IP thật — nhờ `UseForwardedHeaders` bật từ Buổi 4 nên IP này mới có ý nghĩa) và tự thu hồi được. Đây cũng là lối thoát khi người dùng nghi ngờ tài khoản bị xâm phạm, thay vì phải chờ Admin xử lý.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) SRS v1.0.0: Chương 7.7 có cột `IsActive` với chú thích *"Admin có thể deactivate user (ban)"*, Phụ lục B có mã `AUTH_ACCOUNT_DISABLED` — nhưng **không có FR nào, không có endpoint nào** để bật/tắt cờ này, và FR-AUTH-002 **không kiểm tra `IsActive`**. (b) SRS v1.1.0 đã thêm FR-AUTH-008 nhưng chỉ có endpoint theo `id` — route `/dashboard/users` không có API danh sách để dựng; và bảng Chương 8.1 ghi Admin tự khóa → 409 trong khi chính FR-AUTH-008 ghi 403. (c) Lộ trình đề bài yêu cầu "Quản lý phiên làm việc nâng cao" nhưng SRS v1.1.0 không có FR nào (xung đột X-5).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-22**) bổ sung FR-AUTH-008 và kiểm tra `IsActive` ở login/refresh. SRS v1.2.0 bổ sung `GET /users` vào FR-AUTH-008 (**MT-44**), thống nhất 403 (**MT-52**), và thêm **FR-AUTH-009** mức C cùng claim `sid` (**MT-45**).
- **Tại sao chọn:** Với FR-AUTH-008, ba phương án gốc: *(A) FR đầy đủ + endpoint*, *(B) bỏ cột `IsActive`*, *(C) chỉ kiểm tra ở login/refresh, Admin sửa DB tay*. Chọn **A** vì B mất khả năng chặn tài khoản spam — nhu cầu vận hành có thật của blog cho đăng ký tự do; C khiến "Admin có thể deactivate" là lời hứa suông ở tầng ứng dụng và buộc sửa DB tay (không audit được). Với quản lý phiên, FR được **tách riêng** thay vì gộp vào FR-AUTH-005 (Đăng xuất) vì phạm vi khác nhau — FR-AUTH-005 thu hồi phiên đang dùng, FR-AUTH-009 quản lý mọi phiên trên mọi thiết bị; mức **C** bảo đảm cắt nó không làm hỏng FR mức Must Have nào (quy tắc MT-40). Cách làm của bản kế hoạch trước — đặt endpoint ngoài SRS sau feature flag chờ duyệt — không còn cần thiết vì yêu cầu đã chính thức nằm trong SRS.

**Phần 5 – Kết quả Commit Git:**
```
feat(auth-ui): admin user management page and profile sessions tab for FR-AUTH-008/009
```

## DEV 2 — Giao diện Lưu trữ / Khôi phục & Nút thao tác theo Máy trạng thái

**Phần 1 – Chức năng hoàn thành trong buổi:**
Giao diện của `FR-RCP-006` Lưu trữ / Khôi phục Công thức:
- action **"Lưu trữ"**;
- tab **Archived** với nút **"Khôi phục về nháp"**;
- **nút thao tác sinh từ bảng chuyển trạng thái**.

*(API `PATCH /recipes/{id}/archive`, `/unarchive`, bảng `RecipeStatusTransitions` và unit test ma trận 12 tổ hợp đã làm ở **Buổi 4 — Dev 4**; `GET /recipes/sitemap` ở **Buổi 4 — Dev 3**.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **API — đã xong ở Buổi 4 (Dev 4).** Bảng chuyển trạng thái (tham chiếu cho giải trình ở Phần 3):

   | Từ trạng thái | Hành động | Endpoint | Sang trạng thái |
   |---|---|---|---|
   | Draft | publish | `PATCH /recipes/{id}/publish` | Published |
   | Published | unpublish | `PATCH /recipes/{id}/unpublish` | Draft |
   | Draft **hoặc** Published | archive | `PATCH /recipes/{id}/archive` | Archived |
   | Archived | **unarchive** | `PATCH /recipes/{id}/unarchive` | **Draft** |

2. **Hằng số dùng chung FE:** xuất đúng bảng trên thành `features/recipes/statusTransitions.ts`, kèm một test nhỏ đối chiếu với response 409 của API để phát hiện lệch khi bảng phía server đổi.
3. **UI `/dashboard/recipes`:**
   - Action **"Lưu trữ"** cho recipe Draft/Published, nút **"Khôi phục về nháp"** ở tab Archived.
   - Badge trạng thái đổi màu theo `RecipeStatus`.
   - **Chỉ hiện những nút hợp lệ với trạng thái hiện tại** (đọc từ hằng số ở bước 2).
   - Vẫn nhận 409 `RECIPE_INVALID_STATE_TRANSITION` (do người khác đổi trạng thái trước) → hiện thông báo giải thích và tải lại dữ liệu.
4. **Kiểm chứng:**
   - archive → công thức biến mất khỏi `/recipes` công khai, hiện ở tab Archived;
   - khôi phục → về Draft (không tự Published lại).

**Phần 3 – Định hướng & Lý do thiết kế:**
**Archive được từ cả Draft lẫn Published** thay vì chỉ từ Published: Author có thể có bản nháp cũ không muốn xóa nhưng cũng không muốn thấy trong danh sách làm việc. Hạn chế archive chỉ từ Published là ràng buộc không có lý do nghiệp vụ.

**Unarchive đưa về `Draft` chứ không về trạng thái trước khi archive.** Phương án "khôi phục đúng như cũ" đòi thêm cột `StatusBeforeArchive` — một cột tồn tại chỉ để phục vụ một thao tác hiếm, và phải duy trì đồng bộ ở mọi đường ghi. Đưa về `Draft` **buộc Author rà soát lại nội dung trước khi công khai lần nữa**, vốn là hành vi an toàn hơn: công thức bị lưu trữ thường vì nội dung đã cũ hoặc có vấn đề, tự động đưa thẳng lại lên Published là rủi ro.

**Gom luật chuyển trạng thái vào một dictionary** thay vì rải `if` trong từng method: khi luật nằm rải rác, thêm một trạng thái mới (ví dụ `PendingReview` sau này) đòi phải sửa 4-5 chỗ và chắc chắn sẽ sót. Một bảng tra cứu tập trung khiến việc mở rộng chỉ là thêm dòng, và bản thân bảng đó là tài liệu sống của máy trạng thái.

**Test đủ 12 tổ hợp thay vì chỉ test đường hạnh phúc** là điểm mấu chốt của buổi này. Lỗi ở v1.0.0 không phải là "code sai" mà là "có một chuyển đổi không tồn tại và không ai nhận ra" — loại lỗi chỉ lộ ra khi liệt kê **toàn bộ** ma trận chứ không phải khi test từng chức năng riêng lẻ.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** tổng hợp mọi chuyển trạng thái được đặc tả trong v1.0.0 cho thấy **Archived là hố đen — vào được, không ra được**: có `Draft → Published` (FR-RCP-005), `Published → Draft` (FR-RCP-005), `? → Archived` (FR-RCP-006, **không nói từ trạng thái nào**), nhưng **không có** `Archived → Draft` lẫn `Archived → Published`. Điều này mâu thuẫn với chính Mô tả của FR-RCP-006 — *"hữu ích để ẩn recipe cũ... **mà không mất dữ liệu**"* — vì ẩn vĩnh viễn không lấy lại được thì **về phía người dùng không khác gì xóa**. Tên yêu cầu còn ghi rõ *"Archive / **Unarchive**"* nhưng phần thân không có Unarchive. Ngoài ra không rõ `recipe.Publish()` gọi trên một recipe Archived thì điều gì xảy ra.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-35**) vẽ lại máy trạng thái đóng kín, bổ sung **`PATCH /recipes/{id}/unarchive`** đưa về **Draft**, cho phép archive từ **cả Draft lẫn Published**, và chốt mã lỗi **409 `RECIPE_INVALID_STATE_TRANSITION`** cho mọi chuyển đổi ngoài bảng.
- **Tại sao chọn:** Ba phương án: *(A) Archived → Draft*, *(B) Archived → trạng thái trước khi archive (thêm cột `StatusBeforeArchive`)*, *(C) bỏ hẳn trạng thái Archived, dùng Unpublish + soft delete là đủ*. Chọn **A** vì máy trạng thái đóng kín chỉ cần **1 endpoint mới**, không thêm cột nào, và luôn buộc rà soát lại nội dung. Phương án C tưởng đơn giản nhất nhưng thực ra **lan rộng nhất**: phải sửa enum `RecipeStatus`, FR-RCP-001/002, NFR-SEO-002 (quy định `noindex` cho archived), và Phụ lục C. Phương án B thêm một cột chỉ để phục vụ thao tác hiếm — phức tạp hóa vô ích.

**Phần 5 – Kết quả Commit Git:**
```
feat(recipes-ui): archive/unarchive actions and state-aware buttons generated from the shared transition table
```

## DEV 3 — Sitemap Next.js, robots.txt & Tối ưu hình ảnh

**Phần 1 – Chức năng hoàn thành trong buổi:**
`NFR-SEO-003` Sitemap XML + `robots.txt` sinh bởi Next.js + tối ưu `next/image` toàn site (NFR-PERF-005 Core Web Vitals). **Kèm retrofit D-14 (phía FE):** bỏ `unoptimized` khỏi `next/image`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **`app/sitemap.ts`:** export `default async function sitemap(): Promise<MetadataRoute.Sitemap>` — lấy danh sách slug + `lastmod` của **toàn bộ recipe Published** và **category**, cộng các trang tĩnh (`/`, `/recipes`, `/categories`, `/search`); mỗi entry có `url`, `lastModified`, `changeFrequency`, `priority`. Đặt `export const revalidate = 3600` (sitemap không cần tươi hơn một giờ).
2. **Nguồn dữ liệu sitemap:** gọi `GET /api/v1/recipes/sitemap` — endpoint chính Dev 3 đã làm ở **Buổi 4** (SRS v1.2.0 — MT-48) — một request trả toàn bộ `{ slug, updatedAt }` của recipe Published, cache 1 giờ đúng bằng chu kỳ tái sinh sitemap. Đặt lời gọi trong `lib/seo/sitemapSource.ts` (tách khỏi `app/sitemap.ts`) để phần sinh XML test được độc lập với API. **Không** duyệt `GET /recipes` theo trang: với 10.000 công thức cần khoảng 200 request mỗi lần, và dữ liệu có thể xê dịch giữa các trang làm lặp hoặc sót slug — chính là lý do SRS bổ sung endpoint riêng.
3. **`app/robots.ts`:** export `MetadataRoute.Robots` — `allow: '/'`, `disallow: ['/dashboard/', '/profile', '/api/', '/hangfire']`, và **khai báo `sitemap: 'https://<domain>/sitemap.xml'`**. **Không gọi ping Google.**
4. **Retrofit D-14 + tối ưu `next/image`:** sau khi Dev 4 đưa MinIO ra sau Nginx ở đường dẫn `/media/` (cùng buổi), đổi `MinIO__PublicBaseUrl` sang `https://<domain>/media`, **gỡ `unoptimized`**, khai báo `images.remotePatterns` cho `/media/**`. Nguyên nhân gốc của `unoptimized` ở Buổi 2: URL ảnh là `localhost:9000` — trình duyệt tải được nhưng **container frontend thì không** (trong container, `localhost` là chính nó), nên bộ tối ưu ảnh của Next.js chạy phía server luôn lỗi. Rà soát mọi `<Image>`: `sizes` chính xác theo breakpoint, `priority` cho ảnh LCP của trang chủ và trang chi tiết; lưới danh sách dùng `ThumbnailUrl`/`MediumUrl` (FR-JOB-002) thay vì `OriginalUrl`. URL ảnh trong JSON-LD và `og:image` (Buổi 6) tự động chuyển sang domain công khai.
5. **Kiểm chứng:** truy cập `https://<domain>/sitemap.xml` và `/robots.txt` — phải trả đúng XML/text ở **domain chính** (không phải domain API); đưa sitemap vào Google Search Console; chạy **Lighthouse CI** xác nhận LCP ≤ 2.5s, CLS ≤ 0.1, INP ≤ 200ms, First Load JS ≤ 200KB.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Sitemap phải nằm ở nơi crawler đi tìm.** Crawler tìm `https://domain.com/sitemap.xml` — domain do **Next.js** phục vụ. Sinh file ở Backend rồi để trên MinIO (`minio:9000/...`) hoặc trong `wwwroot` của API (`api.culinaryblog.com/sitemap.xml`) là đặt file ở một địa chỉ **không khớp `robots.txt` của site chính**, tức là công sức đổ ra cho một thứ không ai đọc. Đây là ví dụ điển hình của việc một yêu cầu đúng ở chương này nhưng đặt sai chỗ trong kiến trúc ở chương khác.

**Next.js sinh động thay vì lưu file tĩnh** còn giải quyết hai vấn đề vận hành mà phương án cũ vướng: `wwwroot` nằm **trong container** nên mỗi lần deploy là mất file; và khi chạy nhiều API instance thì **mỗi instance giữ một bản khác nhau**. Sinh động với ISR 1 giờ thì không có file nào để mất, không có bản nào để lệch.

**Bỏ ping Google** vì Google đã **ngừng hỗ trợ** endpoint `https://www.google.com/ping?sitemap=` từ **tháng 6/2023** — gọi vào chỉ nhận 404. Cách Google khuyến nghị hiện nay là khai báo sitemap trong `robots.txt`, chính là điều `app/robots.ts` đang làm. Giữ lại bước ping nghĩa là giữ một đoạn code chắc chắn thất bại, sinh log lỗi mỗi ngày và làm nhiễu việc giám sát.

**Dùng `ThumbnailUrl` ở lưới danh sách** là chỗ tiết kiệm băng thông lớn nhất: một trang danh sách 12 công thức tải 12 ảnh gốc 5MB là 60MB, trong khi 12 thumbnail 300×300 chỉ vài trăm KB. Đây là lý do FR-JOB-002 tồn tại, và không dùng đến nó thì job resize thành vô nghĩa.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** FR-JOB-003 giao **Backend** tạo `sitemap.xml` rồi *"upload lên MinIO **hoặc** lưu vào `wwwroot`"* và *"gửi thông báo đến Google Search Console (ping)"*; NFR-SEO-003 yêu cầu `robots.txt` khai báo Sitemap URL; trong khi mục 5.1 và 6.1 quy định site công khai do **Next.js** phục vụ ở domain chính, Backend nằm sau `/api`.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-28**) chuyển toàn bộ trách nhiệm sitemap sang **Next.js `app/sitemap.ts` + `app/robots.ts`**, **bỏ bước ping Google**, và dùng khe FR-JOB-003 cho Permanent Purge Job (Dev 4 đã làm ở Buổi 4). SRS v1.2.0 bổ sung nguồn dữ liệu chính thức cho `app/sitemap.ts` là `GET /recipes/sitemap` (**MT-48**), vì v1.1.0 chỉ ghi "gọi API" mà không có API nào trả đủ toàn bộ slug. *(Liên quan tới điểm xung đột **X-2** — xem §3.)*
- **Tại sao chọn:** Ba phương án: *(A) Next.js tự sinh* — đúng vị trí, không cần lưu file, không cần MinIO, framework hỗ trợ sẵn, và **bỏ được hẳn một job nền** cùng nhu cầu distributed lock đi kèm; *(B) giữ job BE + Nginx proxy `/sitemap.xml` → MinIO* — giữ nguyên FR cũ nhưng thêm rule Nginx, sitemap "đông cứng" tối đa 24h, vẫn phải dọn file cũ; *(C) job BE ghi vào volume dùng chung cho Nginx serve* — vướng ngay khi scale nhiều instance. Chọn **A**: nó là phương án duy nhất **giảm** tổng số thành phần trong hệ thống thay vì tăng, và trách nhiệm được đặt đúng chỗ — SEO là việc của tầng phục vụ nội dung công khai.

**Phần 5 – Kết quả Commit Git:**
```
feat(seo): nextjs sitemap and robots routes, drop deprecated google ping, re-enable next/image optimization
```

## DEV 4 — Redis Cache Invalidation, Nginx & SSL

**Phần 1 – Chức năng hoàn thành trong buổi:**
Tối ưu chiến lược Redis Cache Invalidation + Nginx production (SSL, `/media/`, scale ngang) + `docker-compose.prod.yml` (NFR-PERF-003, NFR-SCALE-003, NFR-SEC-005).
**Kèm retrofit D-14 (phía Nginx), D-17, audit D-6.** Topology Hangfire (D-9) đã được SRS v1.2.0 chuẩn hóa theo code — không có việc gì phải làm.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Audit `ICacheInvalidator` toàn hệ thống (+ audit D-6):** lập bảng đối chiếu **mọi** Command với các khóa nó phải xóa, so với cột *"Invalidate khi"* của bảng TTL chuẩn §2.3. Bổ sung `RedisCacheService.RemoveByPrefixAsync(prefix)` dùng `SCAN` (**không dùng `KEYS`** — lệnh này block Redis trên tập dữ liệu lớn) để xóa `recipes:list:*` và `categories:detail:*`. Xác nhận **không còn dấu vết Output Cache** nào (D-6) và **không khóa nào thuộc endpoint riêng tư** lọt vào cache. Lưu ý khóa thực tế trong Redis có tiền tố instance `culinaryblog:` (báo cáo Buổi 2 §2.3) — `SCAN` phải khớp cả tiền tố này.
2. **`docker-compose.prod.yml`:** **bỏ khối `ports` của `api`** (chỉ expose trong network nội bộ), `deploy: { replicas: 3 }` cho `api`; `hangfire` giữ **1 replica** (scale riêng khi cần — lợi ích của topology worker, SRS v1.2.0 §6.5, MT-47); bỏ `seq` và `mailhog` (dev-only); file dev giữ `5000:8080` để gọi thẳng Scalar/Postman. Kiểm chứng: `docker compose -f docker-compose.prod.yml up --scale api=3` **chạy được** (với cấu hình map cổng cố định sẽ lỗi vì 3 container không thể cùng bind cổng 5000 của host).
3. **Nginx production — giữ cơ chế resolver động của Buổi 2 (đúng SRS v1.2.0 §6.5, MT-50):** **KHÔNG** dùng khối `upstream api_pool { server api:8080; }` như SRS v1.1.0 từng ghi — khối `upstream` chỉ phân giải DNS một lần lúc khởi động và sẽ tái tạo bug 502 đã sửa ở commit `064f582`. Giữ `resolver 127.0.0.11 valid=10s` + `proxy_pass $api_upstream`: khi `--scale api=3`, Docker DNS trả 3 bản ghi A và Nginx tự luân phiên, đồng thời **nhận replica mới trong ≤ 10 giây** không cần reload. Giữ `proxy_next_upstream` (Buổi 5), header `X-Forwarded-*` và `X-Correlation-ID` (Buổi 2), `location /hangfire` + gate (Buổi 3); thêm gzip, cache `_next/static` với `expires 1y` + `Cache-Control: immutable`.
4. **Retrofit D-14 — phục vụ ảnh qua `/media/`:** `location /media/ { proxy_pass $minio_upstream/culinary-blog/; proxy_cache ...; expires 30d; }` với `set $minio_upstream http://minio:9000;` (cùng cơ chế resolver). Từ đây URL ảnh công khai là `https://<domain>/media/...` — **cùng một địa chỉ** truy cập được từ trình duyệt **và** từ container frontend (qua Nginx), nên bộ tối ưu ảnh của Next.js hoạt động được (Dev 3 gỡ `unoptimized` cùng buổi). MinIO không còn cần mở cổng 9000 ra ngoài ở production.
5. **SSL + D-17 + kiểm chứng:** redirect HTTP → HTTPS, chứng chỉ vào `./ssl/`, **HSTS** `max-age=31536000; includeSubDomains`; TLS 1.2+. **Retrofit D-17:** production gọi `.ProtectKeysWithCertificate(cert)` cho DataProtection (khóa trên volume `dpkeys` không còn nằm dạng rõ) — dùng chung chứng chỉ với bước SSL, nạp qua biến môi trường, không commit. Đo **cache hit rate ≥ 80%** bằng `redis-cli INFO stats` (`keyspace_hits / (keyspace_hits + keyspace_misses)`) sau kịch bản duyệt thực tế; ghi kết quả vào `docs/cache-report.md`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**`SCAN` thay vì `KEYS` khi xóa theo prefix** là chi tiết nhỏ nhưng có hậu quả lớn ở production: `KEYS pattern` là lệnh **O(n) blocking** — nó khóa toàn bộ Redis trong lúc quét. Với vài nghìn khóa thì không ai nhận ra; với vài trăm nghìn khóa thì mọi request đang chờ Redis đều treo. `SCAN` duyệt theo lô nên không block. Đây đúng là loại quyết định "chạy tốt lúc dev, sập lúc có tải" mà nguyên tắc chống nợ kỹ thuật nhắm tới.

**Bỏ `ports` của `api` ở production là điều kiện cần để scale ngang.** File compose cũ map cố định `5000:8080`, nên `docker compose up --scale api=3` **lỗi ngay lập tức** — ba container không thể cùng bind một cổng host. Nghĩa là mọi yêu cầu scale ngang trong NFR-SCALE-001/003 và NFR-PERF-002 (*"thêm instance tăng tuyến tính"*) đều **không thực hiện được** với cấu hình như đặc tả. Tách hai file (dev giữ cổng cho tiện gọi thẳng API, prod bỏ cổng + `replicas: 3`) giải quyết cả hai nhu cầu mà không đánh đổi gì.

**Nginx là nơi duy nhất tiếp xúc internet** — do đó nó gánh bốn trách nhiệm không nên nằm ở chỗ khác: SSL termination (API không cần biết gì về chứng chỉ), chuyển tiếp IP thật (điều kiện sống còn của rate limiting — xem Buổi 4, 5), Basic Auth cho `/hangfire` (giải pháp duy nhất khả thi vì JWT không dùng được cho điều hướng trình duyệt), và **một địa chỉ công khai duy nhất cho ảnh** (`/media/`).

**Vì sao giữ resolver động thay vì khối `upstream` mà SRS v1.1.0 từng ghi:** đây là điểm SRS v1.1.0 sai và đã được sửa ở v1.2.0 (MT-50) — cần hiểu lý do để không ai "đơn giản hóa" ngược lại về `upstream`. Buổi 2 đã gặp đúng lỗi này khi chạy thật: container `api` được tạo lại (đổi IP từ `172.30.2.3` sang `172.30.2.2`) → Nginx vẫn gửi request tới IP cũ → **502 cho toàn bộ `/api/*`** cho tới khi restart Nginx. Nguyên nhân là khối `upstream` của Nginx bản open-source phân giải tên **một lần duy nhất lúc khởi động**. Resolver động giải quyết tận gốc, và còn tốt hơn `upstream` ở chính mục tiêu của buổi này: khi scale từ 3 lên 5 replica, Nginx **tự thấy 2 replica mới trong 10 giây** mà không cần reload. Snippet của SRS v1.1.0 là lỗi của tài liệu, không phải một lựa chọn thiết kế khác — đó là lý do SRS v1.2.0 viết lại khối cấu hình kèm chú thích ngay tại chỗ (errata **E-1**, §4.3).

**Vì sao đưa ảnh ra sau Nginx (`/media/`) thay vì chỉ sửa cấu hình Next.js:** gốc rễ của D-14 là hệ thống có **hai "sự thật" về địa chỉ một tấm ảnh** — `localhost:9000` đúng với trình duyệt nhưng sai với container, `minio:9000` đúng với container nhưng sai với trình duyệt. Mọi cách vá ở phía Next.js (loader tùy biến, ghi đè hostname) đều là duy trì hai địa chỉ song song. Một đường dẫn công khai duy nhất qua Nginx xóa bỏ hẳn sự phân đôi đó, đồng thời cho phép cache ảnh ở Nginx và **đóng cổng 9000 của MinIO** khỏi internet ở production.

**D-17 làm cùng buổi với SSL** vì cả hai cần cùng một thứ: chứng chỉ. Khóa DataProtection dùng để ký/mã hóa dữ liệu nhạy cảm của ASP.NET Core; để nó nằm dạng rõ trên volume nghĩa là ai đọc được volume sẽ giả mạo được dữ liệu do ứng dụng ký.

**Cache `_next/static` với `immutable` 1 năm** an toàn vì Next.js đưa hash nội dung vào tên file — nội dung đổi thì tên file đổi, nên không bao giờ có chuyện phục vụ bản cũ. Đây là cách rẻ nhất để cải thiện Core Web Vitals cho khách quay lại.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) Mục 6.5 map cổng **cố định `5000:8080`** cho `api` và **không có `deploy.replicas`**, trong khi NFR-SCALE-003 đòi *"Nginx load balancer upstream pool cho **nhiều API instances**"*, NFR-SCALE-001 đòi stateless để *"hỗ trợ horizontal scaling"*, và NFR-PERF-002 hứa *"thêm instance tăng tuyến tính"* — bốn yêu cầu không thể thực hiện với file compose như đặc tả. (b) Mục 2.4.1 ghi Docker Compose dùng cho *"local dev và **staging**"* (hàm ý không dùng cho production) trong khi mục 6.5 lại có `docker-compose.prod.yml`. (c) Danh sách "8 services" của kế hoạch Buổi 2 (có `hangfire` riêng) lệch với SRS §6.5 (Hangfire in-process). (d) SRS v1.1.0 §6.5 cấu hình Nginx bằng khối `upstream` — trái với cách sửa lỗi 502 đã kiểm chứng ở Buổi 2.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-32, MT-33.7**, §6.5) chốt: dev giữ `ports`, **prod bỏ `ports` + `replicas: 3`**; sửa mục 2.4.1 thành *"local dev, staging **và production**"*. SRS v1.2.0 sửa thêm hai điểm theo hệ thống thật: Nginx dùng resolver động (**MT-50**) và Hangfire chạy ở worker riêng (**MT-47**). Kế hoạch làm theo đúng SRS v1.2.0.
- **Tại sao chọn:** Ba phương án cho (a): *(A) bỏ `ports` hoàn toàn, chỉ vào qua Nginx* — sạch nhất nhưng bất tiện khi dev muốn gọi thẳng Scalar; *(B) hai file compose — dev giữ cổng, prod bỏ cổng + replicas*; *(C) chấp nhận 1 instance, hạ NFR-SCALE xuống "thiết kế sẵn sàng scale, chưa triển khai"*. Chọn **B** vì mục 6.5 vốn đã quy định có hai file, nên chi phí thêm bằng không, mà lại đáp ứng đủ cả nhu cầu dev lẫn cam kết NFR. Phương án C trung thực với quy mô đồ án nhưng **vứt bỏ một phần nội dung có thể chứng minh được** — và thực tế `--scale api=3` chạy được là một minh chứng rất thuyết phục khi bảo vệ.

**Phần 5 – Kết quả Commit Git:**
```
chore(devops): production nginx with ssl and /media proxy, prod compose scaling, dataprotection keys and cache audit
```

---

# BUỔI 8 – KIỂM THỬ TÍCH HỢP TOÀN DIỆN, WCAG 2.1 AA, DOCKER DEPLOYMENT & BẢO VỆ ĐỒ ÁN

> **Mục tiêu buổi:** không viết thêm tính năng mới. Toàn bộ buổi dành cho **chứng minh** hệ thống đạt các chỉ tiêu đã cam kết trong SRS, và đóng gói để bảo vệ.
>
> **Điều kiện đầu vào:** 37/37 FR đã hoàn thành sau Buổi 7; 17/17 hạng mục nợ kỹ thuật cần sửa code (§4.1) đã hoàn trả. Integration test harness đã chạy từ Buổi 3, nên buổi này **mở rộng độ phủ**, không dựng hạ tầng mới.

## DEV 1 — E2E Auth & Kiểm thử Lỗ hổng Bảo mật

**Phần 1 – Chức năng hoàn thành trong buổi:**
Bộ kiểm thử E2E module Auth + kiểm thử lỗ hổng bảo mật (NFR-SEC-001 → 007, NFR-MAINT-002).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Playwright E2E — 5 luồng:** register → auto-login → vào thẳng `/dashboard` (chứng minh `AuthResponseDto` trả token); login sai 5 lần → **423 Locked**; Google login (mock provider); access token hết hạn → **auto refresh trong suốt** (người dùng không nhận ra); Admin khóa tài khoản → user đó bị đăng xuất ở lần refresh kế tiếp.
2. **Integration tests** (trên harness Testcontainers dựng ở Buổi 3 — đã có test từ từng buổi, buổi này lấp chỗ thiếu): mọi endpoint `/auth/*` và `/users/*` có ít nhất 1 happy path + 1 error case; test **Reuse Detection** (dùng lại token đã rotate → toàn bộ family bị revoke); test `IsActive = false` → 403 ở **cả** login lẫn refresh.
3. **Kiểm thử lỗ hổng bảo mật (checklist OWASP):**
   - **Giả mạo IP:** gửi `X-Forwarded-For: 1.2.3.4` thẳng vào cổng API (không qua Nginx) → xác nhận **bị bỏ qua** (nhờ `KnownIPNetworks`), rate limit vẫn tính theo IP thật.
   - **Dashboard:** `:5000/hangfire` không qua Nginx → 401; qua Nginx không có Basic Auth → 401.
   - **Rate limit:** 11 request login liên tiếp → request 11 nhận **429** kèm `Retry-After`.
   - **Phân quyền:** Author gọi `PATCH /users/{id}/status` → **403**; Author sửa recipe của người khác → **403**.
   - **Rò rỉ dữ liệu:** `GET /auth/me` không trả `PasswordHash`/`SecurityStamp`; `GET /auth/sessions` không trả `TokenHash`.
   - **Log:** grep toàn bộ file log và Seq, xác nhận **không có** raw refresh token, password hay `Authorization` header.
4. **Coverage + refactor:** unit test Application layer module Auth đạt **≥ 80% line coverage** (coverlet report); rà lại để **mọi** luồng phát token (register/login/google/refresh) đều đi qua `AuthResponseFactory` sẵn có từ Buổi 2 — không luồng nào tự dựng `AuthResponseDto`; bổ sung XML doc comment cho Scalar.
5. **Báo cáo:** `docs/security-test-report.md` ghi từng mục checklist kèm kết quả và ảnh chụp màn hình.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Kiểm thử giả mạo `X-Forwarded-For` là hạng mục quan trọng nhất** trong checklist bảo mật của buổi này, vì nó kiểm chứng đúng cái bẫy mà `UseForwardedHeaders` tạo ra. Bật forwarded headers **mà quên khai báo `KnownProxies`** sẽ khiến hệ thống **kém an toàn hơn trước khi bật** — kẻ tấn công chỉ cần đổi header mỗi request là né hoàn toàn rate limit. Đây là loại cấu hình "có vẻ đúng" mà chỉ kiểm thử chủ động mới phát hiện được.

**Buổi 8 là buổi mở rộng độ phủ, không phải buổi viết test đầu tiên.** Vì harness có từ Buổi 3, mỗi buổi đã tự mang test của mình; ở đây chỉ còn lấp khoảng trống và chạy kiểm thử bảo mật chủ động. Nếu để toàn bộ integration test dồn về buổi cuối như kế hoạch cũ, mọi lỗi phát hiện ở đây đều là lỗi của những buổi đã "xong" — sửa muộn luôn đắt hơn và rủi ro hơn.

**Kiểm tra log không chứa bí mật** khép lại chuỗi bảo vệ token: ta đã hash trước khi lưu DB (Buổi 2), chỉ lưu hash khi rotate (Buổi 4), lọc destructuring ở Serilog (Buổi 6) — nhưng chỉ khi `grep` thật vào file log và Seq mới biết chắc không có đường nào lọt. Log thường có chính sách lưu trữ dài và quyền truy cập rộng hơn database, nên rò rỉ ở đây còn nguy hiểm hơn.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** NFR-MAINT-002 yêu cầu *"Integration tests: tất cả API endpoints có ít nhất 1 happy path + 1 error case"* và *"E2E tests: 5 critical user flows"*, nhưng v1.0.0 có nhiều mã lỗi **không có đường nào sinh ra** (`AUTH_ACCOUNT_DISABLED` không có bước kiểm tra `IsActive`; `CATEGORY_NAME_EXISTS` không có bước kiểm tra Name) — nghĩa là **không thể viết được error case** cho chúng.
- **Hướng đi chọn lựa:** sau khi SRS v1.1.0 bổ sung đủ các bước kiểm tra (MT-22, MT-36), mọi mã lỗi trong Phụ lục B đều **có đường sinh ra** và do đó **đều test được**. Bộ integration test lấy **Phụ lục B làm checklist**: **27 mã lỗi = 27 test case tối thiểu** (SRS v1.2.0 — MT-46).
- **Tại sao chọn:** dùng Phụ lục B làm nguồn checklist thay vì tự liệt kê theo trí nhớ đảm bảo **không sót mã nào**, và đồng thời là phép kiểm chứng ngược cho chính tài liệu — nếu có mã nào không viết nổi test, đó là dấu hiệu SRS vẫn còn mã chết.

**Phần 5 – Kết quả Commit Git:**
```
test(auth): e2e auth flows, security vulnerability checklist and 80% coverage with token issuer refactor
```

## DEV 2 — E2E Recipe Domain & Kiểm thử Đồng thời

**Phần 1 – Chức năng hoàn thành trong buổi:**
Bộ kiểm thử E2E vòng đời Công thức + kiểm thử xung đột đồng thời (High-Concurrency Testing).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Playwright E2E — vòng đời đầy đủ:** tạo recipe qua wizard 4 bước (thông tin → ảnh → nguyên liệu → các bước) → publish → xuất hiện ở trang công khai → archive → **unarchive về Draft** → soft delete → biến mất khỏi mọi endpoint. Test riêng luồng **kéo-thả sắp xếp bước** → xác nhận `StepNumber` liên tục 1..N.
2. **Kiểm thử Optimistic Concurrency:** hai client cùng load một recipe (cùng `rowVersion`), client A lưu thành công, client B lưu → nhận **409 `RECIPE_CONCURRENCY_CONFLICT`**; kiểm thử song song bằng `Task.WhenAll` với 10 request `PUT` đồng thời → đúng **1 thành công, 9 nhận 409**, không có request nào trả 500.
3. **Kiểm thử cách ly Public/Private (MT-34 — hạng mục bắt buộc):**
   - Author tạo recipe Draft → **Guest gọi `GET /recipes` và `GET /recipes/{slug}` KHÔNG thấy nó** (dù gọi ngay sau khi Admin vừa truy cập cùng URL — kiểm chứng cache không rò rỉ).
   - **Admin gọi `GET /recipes` rồi Guest gọi lại đúng URL đó → Guest vẫn chỉ thấy Published.** Đây là test tái hiện chính xác kịch bản lỗ hổng của v1.0.0.
   - `GET /recipes/mine` trả header **`Cache-Control: no-store`**; Author A truyền `?authorId=<B>` → **403**.
4. **Domain unit tests:** publish thiếu step/ingredient → 400; **ma trận 12 tổ hợp chuyển trạng thái** (từ Buổi 7); renumber sau khi xóa bước giữa; chỉ 1 ảnh primary; slug suffix khi trùng; `PublishedAt` không bị ghi đè khi publish lại.
5. **Kiểm tra N+1:** bật EF Core logging, assert số query cho `GetRecipeBySlug` ≤ ngưỡng (kỳ vọng 1–2 query nhờ projection); dùng `AsSplitQuery()` nếu phát hiện cartesian explosion do nhiều collection include.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Test cách ly Public/Private là hạng mục không được bỏ qua** dù nó thuộc loại "test một thứ đã sửa rồi". Lý do: MT-34 là lỗi **rò rỉ dữ liệu giữa các tài khoản**, và loại lỗi này có đặc điểm là **không biểu hiện gì trong sử dụng thông thường** — mọi thứ trông đúng cho tới khi có người dùng đúng trình tự (Admin truy cập trước, Guest truy cập sau trong cửa sổ TTL). Một test tái hiện đúng trình tự đó là cách duy nhất đảm bảo nó không quay lại khi ai đó "tối ưu" cache trong tương lai.

**Kiểm thử đồng thời với `Task.WhenAll`** thay vì chỉ test tuần tự hai client: xung đột thật xảy ra ở mức mili-giây, và điều ta cần chứng minh không chỉ là "có trả 409" mà là **không có request nào trả 500**. Nếu cơ chế concurrency cài sai, kết quả điển hình là `DbUpdateConcurrencyException` lọt ra ngoài thành lỗi hệ thống thay vì lỗi nghiệp vụ.

**Kiểm tra N+1 bằng cách đếm query thay vì bằng cảm giác:** N+1 không làm test fail, không sinh lỗi, chỉ khiến hệ thống chậm dần khi dữ liệu lớn lên — và lúc phát hiện thì đã ở production. Assert số query trong integration test biến nó thành lỗi build.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 vừa khẳng định *"Recipe Draft chỉ được xem bởi tác giả sở hữu hoặc Admin"* (FR-RCP-002 Mô tả) vừa quy định cache endpoint đó **60 phút theo khóa công khai** (cùng FR, bước 5) — hai câu tự phủ định nhau trong cùng một bảng.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-34**) tách bạch Public/Private; buổi này **kiểm chứng bằng test tự động** thay vì tin vào việc "đã sửa rồi".
- **Tại sao chọn:** nguyên tắc rút ra và nên ghi vào quy ước nhóm: **mọi lỗi bảo mật đã sửa phải có một test tái hiện kịch bản gốc**. Không có test đó, lần refactor sau hoàn toàn có thể vô tình bật lại cache cho endpoint riêng tư mà không ai nhận ra — vì code trông vẫn hợp lý.

**Phần 5 – Kết quả Commit Git:**
```
test(recipes): e2e recipe lifecycle, concurrency conflict and public/private cache isolation regression tests
```

## DEV 3 — E2E Search/Filter & Tuân thủ WCAG 2.1 AA

**Phần 1 – Chức năng hoàn thành trong buổi:**
Kiểm thử E2E Tìm kiếm/Bộ lọc + hiệu năng + tuân thủ **WCAG 2.1 Level AA** (NFR-USE-001 → 004, NFR-PERF-005, NFR-SEO-001).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Playwright E2E — Search/Filter:** gõ **"pho"** (không dấu) → thấy **"Phở bò"** (chứng minh `unaccent` hoạt động); áp filter + sort + phân trang → **state giữ nguyên trong URL**, F5 và back/forward đều đúng; `sortBy=maliciousColumn` → nhận **400** (chứng minh whitelist chặn); Admin CRUD category + **409 khi xóa category còn recipe**.
2. **Accessibility — `@axe-core/playwright`:** quét **mọi route** công khai và dashboard, yêu cầu **0 violation mức serious/critical**. Rà thủ công: semantic HTML (`<main>`, `<nav>`, `<article>`, `<aside>`), ARIA cho phần tử tương tác, **focus trap trong modal**, điều hướng đầy đủ bằng bàn phím (Tab/Enter/Escape), contrast ratio ≥ **4.5:1** (text) và ≥ 3:1 (UI component), `alt` text cho mọi ảnh (dùng `AltText` từ FR-RCP-008).
3. **Responsive:** kiểm tra **320px / 768px / 1200px** — mobile single column, tablet 2 cột, desktop full layout; filter dạng drawer trên mobile hoạt động bằng cả chạm lẫn bàn phím.
4. **Hiệu năng FE — Lighthouse CI:** LCP ≤ **2.5s**, CLS ≤ **0.1**, INP ≤ **200ms**, First Load JS ≤ **200KB** (gzipped) — chạy trên trang chủ, trang danh sách và trang chi tiết.
5. **Kiểm chứng SEO + báo cáo:** **Google Rich Results Test** phải pass 100% cho loại Recipe; kiểm tra `<title>` thực tế ≤ 60 ký tự sau truncate, `<meta description>` 150–160 ký tự; truy cập `/recipes/<slug-của-một-Draft>` phải trả **HTTP 404** (không phải trang trống status 200 — lỗi `loading.tsx` bọc route mà báo cáo Buổi 2 §5 từng gặp), còn các trang `/dashboard/*` mang `noindex` và bị `robots.txt` chặn; `/sitemap.xml` **không chứa** slug nào của Draft/Archived. Sửa mọi lỗi phát hiện, ghi `docs/a11y-report.md` và `docs/lighthouse-report.md`.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Kiểm thử `sortBy` với giá trị ngoài whitelist** không chỉ là test validation mà là **test bảo mật**: `sortBy` là tên cột và tên cột không tham số hóa được trong SQL, nên nếu ở đâu đó có đường ghép chuỗi thì đây chính là lỗ hổng injection. Test này chứng minh rào chắn dictionary hoạt động.

**Yêu cầu 0 violation mức serious/critical thay vì "0 violation tuyệt đối":** axe-core báo cả những mục cần đánh giá thủ công và các cảnh báo mức minor mà nhiều trong số đó không áp dụng cho ngữ cảnh cụ thể. Đặt ngưỡng ở serious/critical cho một tiêu chí **đạt được và có ý nghĩa**, thay vì một con số lý tưởng khiến cả nhóm bỏ cuộc.

**Đo `<title>` thực tế sau truncate** là cách kiểm chứng đúng cho quyết định "giới hạn SEO là quy tắc render, không phải ràng buộc dữ liệu" (MT-37). Nếu chỉ kiểm tra cột `Title` ≤ 200 thì không chứng minh được gì về SEO; phải xem HTML render ra mới biết quy tắc cắt chuỗi có chạy không.

**Accessibility không phải là hạng mục "làm nếu còn thời gian".** NFR-USE-002 cam kết WCAG 2.1 AA, và phần lớn công việc (semantic HTML, alt text, contrast) đáng lẽ đã được làm đúng từ đầu ở mỗi buổi. Buổi này là **kiểm chứng và vá nốt**, không phải làm lại từ đầu — nếu phải làm lại từ đầu thì đó là dấu hiệu các buổi trước đã bỏ qua.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** NFR-SEO-001 hứa *"Rich Snippets trên Google Search (**star rating**, time, ingredients)"* trong khi Rating System nằm ngoài phạm vi và danh sách JSON-LD không có `aggregateRating` → **hạng mục này không thể pass** nghiệm thu. NFR-SEO-002 đòi `<title>` ≤ 60 ký tự trong khi cột `Title` cho phép 200 → một công thức đặt tên dài là NFR tự động fail.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-24, MT-37**) bỏ "star rating", giữ tiêu chí *"pass Google Rich Results Test cho loại Recipe"*; chuyển giới hạn độ dài thành **quy tắc truncate lúc render**.
- **Tại sao chọn:** cả hai sửa đổi đều theo một nguyên tắc: **tiêu chí nghiệm thu phải đạt được bằng cách làm đúng, không phải bằng cách làm sai.** Muốn có sao trong rich snippet thì chỉ còn cách nhúng `aggregateRating` giả — **vi phạm chính sách structured data của Google**, nguy cơ bị phạt toàn site. Muốn `<title>` luôn ≤ 60 ký tự bằng ràng buộc dữ liệu thì phải cấm người dùng đặt tên món quá 44 ký tự — vô lý với thực tế tiếng Việt. Khi một tiêu chí chỉ đạt được bằng cách làm sai, tiêu chí đó cần được sửa.

**Phần 5 – Kết quả Commit Git:**
```
test(search,a11y): e2e search and filter tests, wcag 2.1 aa compliance and lighthouse performance fixes
```

## DEV 4 — Docker Multi-stage, Load Testing & Đóng gói

**Phần 1 – Chức năng hoàn thành trong buổi:**
Gia cố image Docker + Load Testing (p50 ≤ 150ms) + đóng gói hoàn chỉnh hệ thống (NFR-PERF-001/002/003, NFR-SCALE-003, CONS-009).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Gia cố image (multi-stage đã có từ Buổi 2):** Buổi 2 đã có `sdk:10.0` → `aspnet:10.0` chạy user non-root `app`, và frontend `node:22-alpine` → `output: 'standalone'` chạy user `nextjs` (báo cáo Buổi 2 §2.4) — **không làm lại**. Buổi này: tách layer `dotnet restore`/`npm ci` để tận dụng cache build; rà `.dockerignore` (loại `node_modules`, `bin`, `obj`, `.git`, `.env*`); **ghim phiên bản** image nền theo tag cụ thể thay vì `latest` (đặc biệt `minio/minio:latest` và `datalust/seq:latest` trong compose hiện tại); quét lỗ hổng bằng `docker scout cves` hoặc Trivy, không chấp nhận lỗ hổng mức Critical. Ghi kích thước và kết quả quét vào báo cáo.
2. **Kiểm chứng toàn hệ thống + scale:** **Dev** (`docker compose up`): 9 container `healthy` — 8 service (`nginx, api, hangfire, frontend, postgres, redis, minio, seq`) + `mailhog`. **Prod** (`docker compose -f docker-compose.prod.yml up -d --build --scale api=3`): `nginx, api×3, hangfire, frontend, postgres, redis, minio` đều `healthy` (không có `seq`/`mailhog`); Nginx luân phiên qua 3 instance (kiểm chứng bằng `CorrelationId` rơi vào các container khác nhau trong log). Tạo lại một container `api` giữa chừng → **không có 502** (kiểm chứng MT-50 — cấu hình resolver động).
3. **Load test k6 — ba kịch bản:** *smoke* (1 VU, 1 phút) → *load* (**100 concurrent users**, 5 phút) → *stress* (tăng dần tới ngưỡng gãy). Kịch bản mô phỏng tỉ lệ thực tế: 70% đọc danh sách/chi tiết công thức (Guest), 20% tìm kiếm, 10% ghi (Author). Xác nhận **p50 ≤ 150ms, p95 ≤ 500ms, p99 ≤ 1000ms** và **Redis hit rate ≥ 80%**.
4. **Kiểm chứng khả năng chịu lỗi (NFR-REL-002):** `docker compose stop redis` trong lúc load test đang chạy → hệ thống **fallback về database, không throw exception**, chỉ chậm hơn; `docker compose stop minio` → API vẫn phục vụ công thức, chỉ upload ảnh trả 503.
5. **Đóng gói & bảo vệ:** `docs/load-test-report.md` (biểu đồ p50/p95/p99, throughput, hit rate), `README.md` hướng dẫn setup **< 5 phút**, `CHANGELOG.md` theo Keep a Changelog, **6 ADR** theo yêu cầu NFR-MAINT-003: 4 quyết định lớn của CR-2026 (MT-05 Soft Delete, MT-08/09 chuẩn hóa mã lỗi, MT-16 Redis, MT-34 cách ly cache) và **2 quyết định của CR-2026-02 mà SRS được sửa theo hệ thống thật** (Nginx resolver động — MT-50; Hangfire worker riêng — MT-47): đây là hai chỗ dễ bị người sau "đơn giản hóa" ngược lại nhất, nên càng cần ADR ghi lý do. Gắn tag **`v1.0.0`** — phiên bản **phần mềm** phát hành lần đầu, độc lập với số phiên bản của tài liệu SRS (v1.2.0).

**Phần 3 – Định hướng & Lý do thiết kế:**
**Multi-stage build** (đã có từ Buổi 2 — buổi này chỉ gia cố) tách môi trường biên dịch khỏi môi trường chạy: image production không chứa SDK, không chứa mã nguồn, không chứa `node_modules` của devDependencies. Lợi ích kép — image nhỏ hơn 5–6 lần (deploy nhanh hơn, tốn ít dung lượng registry) và **bề mặt tấn công nhỏ hơn** (không có compiler, không có công cụ build để kẻ xâm nhập lợi dụng). Chạy bằng non-root user là biện pháp tối thiểu để một lỗ hổng trong ứng dụng không trở thành quyền root trong container.

**Kịch bản load test phải phản ánh tỉ lệ truy cập thật.** Bắn 100% request vào endpoint ghi sẽ cho con số vô nghĩa vì nó không bao giờ xảy ra; bắn 100% vào một URL duy nhất thì mọi request đều cache hit và p50 đẹp giả tạo. Tỉ lệ 70/20/10 mô phỏng đúng đặc điểm của một blog: **phần lớn là Guest đọc nội dung công khai** — cũng chính là đặc điểm khiến quyết định cache ở MT-34 (endpoint công khai cache thoải mái) mang lại giá trị lớn.

**Kiểm chứng khả năng chịu lỗi trong lúc đang có tải** thay vì thử lúc hệ thống rảnh: NFR-REL-002 cam kết *"Redis down → fallback database, không throw exception"*. Thử lúc không tải thì không chứng minh được gì — điều cần biết là hệ thống **suy giảm có kiểm soát** hay sụp đổ dây chuyền khi mất một dependency giữa lúc đang phục vụ.

**Viết ADR cho 4 quyết định lớn** là yêu cầu của NFR-MAINT-003, nhưng giá trị thật nằm ở chỗ khác: người bảo trì hệ thống sau này (hoặc chính nhóm sau 6 tháng) sẽ nhìn thấy `GET /recipes` không trả Draft và tưởng đó là thiếu sót, rồi "sửa" nó — làm sống lại lỗ hổng MT-34. ADR ghi lại **vì sao** mới ngăn được điều đó; code và test chỉ ghi lại **cái gì**.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** NFR-PERF-001 đặt mục tiêu *"p50 ≤ 150ms cho GET endpoints với dữ liệu cache"* và NFR-PERF-003 đòi *"Redis hit rate ≥ 80%"*, nhưng bảng TTL của v1.0.0 lại lệch nhau giữa Chương 3 (60 phút) và NFR-PERF-003 (5 phút) — **không biết đo hit rate theo cấu hình nào**. Thêm nữa mục 6.5 map cổng cố định khiến `--scale api=3` không chạy được, làm NFR-PERF-002 (*"thêm instance tăng tuyến tính"*) không kiểm chứng được.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-17, MT-32**) chốt **bảng TTL chuẩn duy nhất** tại NFR-PERF-003 và cho phép scale ở `docker-compose.prod.yml`. Buổi này đo hit rate **theo đúng bảng TTL đó**.
- **Tại sao chọn:** cần lưu ý một đánh đổi trung thực: TTL mới **ngắn hơn** TTL cũ (recipe detail 5 phút thay vì 60 phút), nên **hit rate sẽ thấp hơn** và mốc 80% khó đạt hơn. Nhóm chấp nhận điều này vì dữ liệu tươi hơn và một cơ chế cache duy nhất quan trọng hơn một con số đẹp — và vì TTL 60 phút của Output Cache in-memory vốn đã **không dùng được** khi chạy nhiều instance. Nếu hit rate đo được dưới 80%, cách xử lý đúng là **ghi nhận trung thực trong báo cáo kèm phân tích nguyên nhân**, không phải nới TTL lên cho đạt chỉ tiêu.

**Phần 5 – Kết quả Commit Git:**
```
chore(release): harden docker images, k6 load tests, resilience verification and v1.0.0 release packaging
```

---

## 5. Ma trận truy vết FR → Buổi → Dev (37/37 FR — SRS v1.2.2)

> Buổi 1 (đọc đặc tả) không hiện thực FR nào; số buổi dưới đây theo cách đánh số 8 buổi. Từ 29/09/2026 cột "Buổi" ghi dạng **API B{n} · UI B{m}** khi API và giao diện của một FR làm ở hai buổi khác nhau (yêu cầu "hoàn thành tất cả API endpoints" ở Buổi 4); cột "Dev" ghi chủ API / chủ giao diện khi hai người khác nhau.

| Mã FR | Tên | Buổi | Dev | Ưu tiên |
|---|---|---|---|---|
| FR-AUTH-001 / 002 | Đăng ký / Đăng nhập local | 2 | 1 | M / M |
| FR-AUTH-003 / 005 | Google ID Token / Logout | 3 | 1 | **M** / M |
| FR-AUTH-004 | Refresh Rotation + Reuse Detection | **API 4 · FE (D-12, interceptor) 5** | 1 | M |
| FR-AUTH-006 / 007 | Xem / Cập nhật Profile | **API 4 · UI 5** | 1 | **M** / M |
| **FR-AUTH-008** | **Quản lý tài khoản [Admin]: danh sách + khóa / mở khóa** | **API 4 · UI 7** | **1** | **S** |
| **FR-AUTH-009** | **Quản lý phiên đăng nhập của chính mình** (mới ở v1.2.0 — MT-45) | **API 4 · UI 7** | **1** | **C** |
| FR-CAT-001 / 002 | Danh mục public + Redis cache | 2 | 3 | M / M |
| FR-CAT-003 / 004 / 005 | CRUD danh mục Admin | 3 | 3 | M / M / M |
| FR-RCP-001 / 002 | Danh sách / Chi tiết công thức | 2 | 2 | M / M |
| FR-RCP-003 / 008 | Tạo Draft / Quản lý ảnh | 3 *(mảng inline `steps?`/`ingredients?` của FR-RCP-003: 4)* | 2 | M / M |
| FR-RCP-009 / 010 | Nguyên liệu / Các bước + reorder | **API 4 · UI 5** | 2 | M / M |
| FR-RCP-004 | Update RowVersion | **API 4 · UI 5** | 2 | M |
| FR-RCP-005 | Publish / Unpublish (+ D-15) | **API 4 · UI 5** | **API: 4 · UI: 2** | M |
| FR-RCP-007 | Soft Delete (+ D-3) | **API 4 · UI 6** | **API: 4 · UI: 2** | M |
| **FR-RCP-011** | **`GET /recipes/mine` + `GET /recipes/mine/{id}` (cấm cache — CR-2026-04 d)** | **API 4 · UI 5 (trang sửa), 6 (dashboard)** | **API: 3 · UI: 2** | **M** |
| FR-RCP-006 | **Archive + Unarchive trọn gói**, đóng kín máy trạng thái | **API 4 · UI 7** | **API: 4 · UI: 2** | **M** |
| FR-SRCH-001 | Full-Text Search (generated column) | **API 4 · UI 5** | 3 | M |
| FR-SRCH-002 / 003 / 004 | Lọc / Sắp xếp / Phân trang (+ D-10) | **API 4 · UI + index 5** | 3 | **M / M / M** |
| FR-FILE-001 / 002 | MinIO Upload / Delete (service + endpoint `/files/*` — endpoint chính thức hóa ở v1.2.0, MT-43) | 2 | 4 | **M / M** |
| FR-JOB-001 | Welcome Email (job: **đã làm ở B2**; dashboard bảo vệ: B3) | **2 + 3** | 4 | S |
| FR-JOB-002 | Image Resize | 4 | 4 | S |
| **FR-JOB-003** | **Permanent Purge Job** (không phải Sitemap) | **4** | **4** | **S** |
| FR-OBS-001 | Health Checks (3 endpoint: **B3** — yêu cầu bổ sung 29/09; Docker `healthcheck:` + failover Nginx + UI: B5) | **3 + 5** | 4 | **M** |
| FR-OBS-002 | Structured Logging | 6 | 4 | **M** |
| FR-OBS-003 | Tracing & Metrics | 6 | 4 | **C** |

> **In đậm** = thay đổi so với kế hoạch 6 buổi (FR mới, mức ưu tiên được nâng theo MT-40, nội dung được định nghĩa lại, hoặc buổi thực hiện thay đổi).
>
> **Từ 29/09/2026, mọi FR có giao diện mà API chưa xong trước Buổi 4 đều được tách "API ở Buổi 4 · giao diện ở buổi sau"** — ngoại lệ có chủ đích với nguyên tắc "một FR không cắt đôi qua hai buổi" (§0.2), vì giảng viên yêu cầu hoàn thành toàn bộ API ở Buổi 4. Đổi lại: Frontend từ Buổi 5 làm việc trên hợp đồng API đã có integration test (không còn cảnh chờ API), lỗ hổng MT-34 được vá sớm hai buổi, và `ApiSurfaceTests` giữ bề mặt API ổn định tới Buổi 8. Mỗi phần giao ở mỗi buổi vẫn **chạy được và kiểm chứng được** (API qua Scalar + test).
>
> **Ba FR trải qua hai buổi vì lý do riêng (trước 29/09):** FR-JOB-001 vì Buổi 2 đã làm sớm phần job (theo quy tắc "không code giả" của Buổi 2), chỉ còn lại dashboard; FR-RCP-003 vì hai mảng inline dùng lại đúng bất biến của FR-RCP-009/010 — làm sớm sẽ phải viết validator nguyên liệu/bước hai lần; FR-OBS-001 vì yêu cầu "≥ 2 API/thành viên" của Buổi 3 (29/09/2026) — phần endpoint kéo lên B3 và dùng được ngay (Verification §7 gọi `/health` từ B3), phần nối vào Docker/Nginx/UI giữ ở B5. **FR-RCP-006 không còn bị cắt đôi** như lộ trình ban đầu.

**Hạ tầng chung không gắn với một FR cụ thể:** commit nền D-11 (B3 — Dev 4 dẫn) · Integration Test Harness (B3 — Dev 4) · **Nền Domain Exceptions / Repository & Unit of Work / Global Exception Middleware** (B3 — Dev 4 phần nền; Dev 1/2/3 phần module của mình — yêu cầu bổ sung 29/09/2026) · `UseForwardedHeaders` (B4 — Dev 1) · **Test bề mặt API 45/45 `ApiSurfaceTests`** (B4 — Dev 4) · Composite indexes (B5 — Dev 3) · Nginx production + `/media/` (B7 — Dev 4).

**Truy vết NFR:**

| Nhóm NFR | Buổi – Dev |
|---|---|
| SEC-001 / 002 (mật khẩu, JWT) | B2 Dev1 · B4 Dev1 (256-bit, hash, rotation — D-2) · B5 Dev1 (access token trong bộ nhớ — D-12) |
| SEC-003 (rate limit + ForwardedHeaders) | **B4 Dev1** (`UseForwardedHeaders`) · **B6 Dev1** (rate limit) · kiểm chứng B8 Dev1 |
| SEC-004 (input validation, file upload) | B2 Dev4 (magic bytes) · B3 commit nền (400 thống nhất — D-11) · xuyên suốt |
| SEC-005 (HTTPS, CORS, HSTS, CSP) | B2 (CORS) · B6 Dev1 (CSP) · **B7 Dev4** (SSL, HSTS) |
| SEC-006 (authorization + cách ly cache) | B3 Dev2 (`RecipeAuthorizationHandler`) · **B4 Dev3** (MT-34 — D-4, D-5; `/recipes/mine`, `/recipes/mine/{id}` cấm cache) · B6 Dev1 (rà soát policy) |
| SEC-007 (secrets, không log bí mật) | B2 Dev4 (gitleaks, User Secrets) · B3 Dev4 (htpasswd/gate secret) · **B6 Dev4** (redaction) · B7 Dev4 (DataProtection — D-17) |
| PERF-001 / 002 (response time, throughput) | **B8 Dev4** (k6) |
| PERF-003 (cache TTL + hit rate) | B3 Dev3 (D-13) · B4 Dev3 (D-6) · B6 Dev3 (D-7) · **B7 Dev4** (audit) |
| PERF-004 (index, EXPLAIN ANALYZE) | **B5 Dev3** (composite index + đo trên ≥ 10.000 bản ghi) |
| PERF-005 (Core Web Vitals) | B7 Dev3 (bật lại tối ưu ảnh — D-14) · **B8 Dev3** (Lighthouse) |
| USE-001 → 004 (responsive, a11y, lỗi, loading) | B2 Dev4 (progress bar) · B3 (lỗi từng ô sau D-11) · **B8 Dev3** (WCAG) |
| REL-001 (uptime, healthcheck) | B2 (healthcheck postgres/redis/minio) · **B3 Dev4** (health endpoints) · **B5 Dev4** (healthcheck api/frontend + `proxy_next_upstream`) |
| REL-002 (resilience, fallback) | B2 Dev3 (Redis fallback) · **B3 Dev4** (Global Exception Middleware — 500 không lộ stack trace; dịch lỗi ghi DB) · **B8 Dev4** (kiểm chứng có tải) |
| REL-003 (durability, soft delete 30 ngày) | **B4 Dev4** (xóa mềm + partial unique D-3 + purge job — cùng một người, cùng một buổi) · B6 Dev2 (giao diện xóa) |
| MAINT-001 → 004 (chất lượng, test, docs, Clean Arch) | xuyên suốt · **B3 Dev4** (integration test harness; test kiến trúc exception/repository) · **B4 Dev4** (`ApiSurfaceTests` — bề mặt API khớp SRS) · **B3 cả nhóm** (Domain Exceptions, Repository & UoW theo module) · **B8** (coverage, 6 ADR, CHANGELOG) |
| SCALE-001 → 003 (stateless, DB, hạ tầng) | B2 (worker Hangfire riêng — SRS v1.2.0 MT-47) · B4 Dev4 (distributed lock) · **B7 Dev4** (replicas, resolver động) |
| SEO-001 → 004 (JSON-LD, meta, sitemap, URL) | B3 Dev2 (slug dành riêng) · **B6 Dev3** (JSON-LD, OG) · **B7 Dev3** (sitemap, robots) |

---

## 6. Phụ thuộc liên dev (điểm bắt buộc phối hợp)

| Buổi | Phụ thuộc | Cách xử lý |
|---|---|---|
| **B3** | ⚠️ **Mọi dev** phụ thuộc commit nền D-11 (422 → 400) | ✅ Đã xong trong commit `67d29c0` của Dev 4 |
| **B3** | ⚠️ **Dev 1, 2, 3** phụ thuộc **commit nền kiến trúc** của Dev 4 (lớp gốc `DomainException`, `IRepository<T>`/`IUnitOfWork`, `ExceptionStatusMap`) | Dev 4 merge vào `develop` ở **mốc M1 (chậm nhất giữa buổi)**; trước M1 ba dev làm phần không phụ thuộc (D-1 + FE Google; D-8 + validator + wizard; validator + trang quản trị + D-13), sau M1 rebase rồi viết exception/repository/mapping của module |
| **B3** | Hai file dùng chung bị nhiều dev sửa: `IUnitOfWork.cs`, `UnitOfWork.cs` (`ErrorCodes.cs` đã có đủ 29 mã từ commit nền) | **Chỉ thêm dòng** vào khu vực của module mình (property `Users`/`Categories`/`Recipes`). Mapping HTTP và translator mỗi module một file, quét tự động — không ai sửa file DI hay middleware của người khác |
| **B3** | Dev 2 kiểm tra `categoryId` tồn tại — dữ liệu thuộc module của Dev 3 | Dùng `IRepository<Category>.ExistsAsync` (generic, của Dev 4) — **không** chờ `ICategoryRepository` của Dev 3 |
| **B3** | Hai mã lỗi mới (`AUTH_USER_NOT_FOUND`, `CONCURRENCY_CONFLICT`) | ✅ Đã áp dụng trong SRS v1.2.2 (CR-2026-04, §4.5) |
| **B3** | Mọi dev viết integration test trên harness của **Dev 4** | Dev 4 push harness (`CulinaryBlogApiFactory`, `CreateClientAs`) **trước giữa buổi**; trước đó các dev viết test theo mẫu đã thống nhất rồi chạy khi harness vào |
| **B3** | Dev 1 đổi `UserDto` (`fullName` → `displayName`, bỏ `userName` — D-1) | Mọi component FE đọc `user.fullName` phải đổi; Dev 1 merge trước Dev 2/Dev 3 theo thứ tự chuẩn, các dev sau rebase |
| **B3** | Dev 2 cần `SlugHelper`, `IFileStorageService`, `ImageFileInspector` | Đã có sẵn từ B2 (`Domain/Common`, `Application/Common`) — chỉ inject |
| **B4** | Dev 4 móc `ImageResizeJob` vào `UploadRecipeImageCommand` của Dev 2 (B3) | Dev 2 để sẵn điểm `BackgroundJob.Enqueue`; Dev 4 chỉ cắm job vào |
| **B4** | Dev 4 (`B4_Recipe_PartialUniqueSlug`), Dev 3 (`B4_Search_FTS`) và Dev 2 (`B4_Recipe_IngredientStep`) **cùng tạo migration trên bảng `Recipes`** | Theo thứ tự merge chuẩn: Dev 4 → Dev 3 → Dev 2; dev sau rebase rồi chạy lại `dotnet ef migrations add` |
| **B4** | ⚠️ Ba dev cùng sửa module Recipe (Dev 2 nội dung, Dev 3 truy vấn, Dev 4 vòng đời) | Dev 4 (merge đầu) đổi `Recipe` thành `partial class` và tách endpoint thành 3 file (`RecipesEndpoints`, `RecipeQueryEndpoints`, `RecipeLifecycleEndpoints`) **ngay đầu buổi**, đẩy lên `develop` trước; Dev 2, Dev 3 rebase rồi mỗi người chỉ sửa file của mình |
| **B4** | Dev 3 (`GET /recipes/mine/{id}`) và Dev 2 (`PUT /recipes/{id}`) dùng chung `rowVersion`/`ETag` | Hợp đồng đã chốt trong SRS v1.2.2 §8.3: `rowVersion` là base64 của cột `RowVersion`, `ETag` = `"{rowVersion}"`; `PUT` nhận cả body lẫn `If-Match` |
| **B4** | Dev 4 invalidate `recipes:sitemap` của Dev 3 | Khóa cache đặt tên trong `CacheKeys` dùng chung (Dev 3 tạo), Dev 4 chỉ tham chiếu |
| **B4** | Dev 1 bật `UseForwardedHeaders` — cần Nginx gửi `X-Forwarded-For` | **Đã có từ B2** (`nginx.conf` dòng 52) — không phụ thuộc ai |
| **B5** | Giao diện của Dev 2 (wizard, trang sửa, nút publish) dùng API của Dev 2, Dev 3, Dev 4 (Buổi 4) | Hợp đồng là SRS Chương 8 + integration test của chủ API; lệch hợp đồng thì chủ API sửa, Dev 2 không sửa handler của người khác |
| **B6** | *(Không còn — MT-34 do một mình Dev 3 vá trọn ở Buổi 4, gồm cả `RecipeVisibility` và Output Cache)* | — |
| **B7** | *(Không còn — `GET /recipes/sitemap` do chính Dev 3 làm ở Buổi 4)* | — |
| **B4** | Dev 1 thêm claim `sid` vào `AuthResponseFactory` — mọi luồng phát token (đăng ký, đăng nhập, Google, refresh) đều đi qua đây | Chỉ Dev 1 sửa file này trong B4; các dev khác không phụ thuộc vào `sid` |
| **B7** | Dev 3 gỡ `unoptimized` (D-14) cần Nginx `/media/` của **Dev 4** | Dev 4 merge trước (đúng thứ tự chuẩn); Dev 3 kiểm chứng sau khi rebase |
| **B8** | Cả 4 dev chạy E2E trên cùng một môi trường | Dev 4 dựng môi trường prod-like **đầu buổi 8**, 3 dev còn lại viết test trên đó |

**Thứ tự merge migration (giữ nguyên quy ước B2):** Dev4 → Dev1 → Dev3 → Dev2. Dev sau `rebase` + chạy lại `dotnet ef migrations add` nếu snapshot conflict.

---

## 7. Verification (cuối mỗi buổi & cuối dự án)

**Cuối mỗi buổi — Definition of Done:**
1. `docker compose up -d --build` → tất cả container `healthy`; `curl http://localhost/health` (qua Nginx) trả `Healthy` (endpoint có từ B3).
2. `dotnet build && dotnet test` xanh — build đã bật `TreatWarningsAsErrors`; test gồm unit + **integration (Testcontainers, từ B3)** + **ArchitectureTests** (Domain không reference assembly ngoài nào, Application không reference Infrastructure; từ B3: mọi `DomainException` cụ thể có ánh xạ HTTP, interface repository không trả `IQueryable`, `ErrorCodes` khớp Phụ lục B).
3. `cd frontend && npm run lint && npm run build && npm test` xanh.
4. Test tay các endpoint của buổi qua Scalar `/scalar` + UI tương ứng. **Từ B4:** `ApiSurfaceTests` xanh — route thật khớp đúng 45 endpoint SRS v1.2.2 Chương 8.
5. Từ B6: log trên Seq có đủ `CorrelationId`, `RequestPath`, `UserId` và **không có** bí mật nào.
6. Retrofit của buổi (§4.1) đã hoàn trả **và** có test chứng minh; hạng mục phụ thuộc CR chỉ được bật khi CR đã duyệt.

**Kiểm tra riêng theo buổi:**

| Buổi | Kiểm chứng đặc thù |
|---|---|
| B1 | Bảng đầu việc (README Phần 1) đủ 37 FR / 30 NFR / 10 CONS / 44 endpoint; mỗi thành viên trình bày được module của mình; danh sách mâu thuẫn đã ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` |
| B2 | Đăng ký/đăng nhập, xem công thức, xem danh mục, upload/xóa ảnh chạy qua Nginx; **CR-2026-03:** `SELECT` trên DB cho ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước; `RecipeSeedCatalogTests` xanh; khởi động lại API log *"0 recipes created, 0 recipes repaired"* |
| B3 | `grep -rE "Status422|status === 422" backend/src frontend/src` không còn kết quả (D-11); form đăng ký hiện lỗi từng ô với 400; đăng ký 2 email cùng prefix → `UserName` có hậu tố số; `/hangfire` qua Nginx hỏi Basic Auth, gọi thẳng `:5000/hangfire` → 401; `dotnet test` chạy integration test trên Testcontainers |
| B4 | Xóa bước giữa và kéo-thả đổi chỗ → `StepNumber` liên tục 1..N, **không có lỗi 23505** (D-16); `to_tsquery('simple','pho:*')` tìm được "Phở bò" **trên Testcontainers** (DDL nằm đủ trong migration — D-18); `localStorage` không còn chứa access token (D-12); `CreatedByIp` là IP thật |
| B5 | `EXPLAIN ANALYZE` trên **≥ 10.000 bản ghi** → Index Scan; `?sort=-title` → 400, `?sortBy=title&sortOrder=desc` → 200 (D-10); publish một recipe Archived → 409 (D-15); `docker compose stop redis` → `/health/ready` 503, `/health/live` 200 |
| B6 | Test tái hiện MT-34 (viết từ B3) **chuyển sang xanh**: Admin gọi `GET /recipes` rồi Guest gọi lại đúng URL → Guest KHÔNG thấy Draft; Draft qua `/recipes/{slug}` → **404**; `GET /recipes/mine` có `Cache-Control: no-store`; request thứ 11 tới `/auth/login` trong 1 phút → 429 |
| B7 | Ma trận **12 tổ hợp** chuyển trạng thái đúng hết; Admin tìm được người dùng qua `GET /users` và khóa → phiên của họ bị thu hồi; `GET /auth/sessions` đánh dấu đúng "Thiết bị này" nhờ claim `sid`; `https://<domain>/sitemap.xml` trả XML ở **domain chính**; ảnh tải qua `/media/` và `next/image` đã tối ưu (D-14); `docker compose -f docker-compose.prod.yml up --scale api=3` chạy được và tạo lại container `api` **không gây 502** |
| B8 | `npx playwright test` xanh; k6 đạt p50 ≤ 150ms / p95 ≤ 500ms / p99 ≤ 1000ms; axe 0 violation serious/critical; Lighthouse đạt CWV; Google Rich Results Test pass |

**Checklist nghiệm thu cuối dự án:**
- [ ] **37/37 FR** (SRS v1.2.2) hoàn thành và truy vết được theo ma trận §5.
- [ ] **45/45 endpoint** của SRS v1.2.2 Chương 8 hoạt động, có trong Scalar `/scalar` và được `ApiSurfaceTests` khóa (xong từ Buổi 4).
- [ ] **17/17 hạng mục nợ kỹ thuật cần sửa code** (§4.1) đã hoàn trả — đặc biệt **D-4, D-5 (lỗi bảo mật MT-34)** và **D-11 (422 → 400)**.
- [ ] **29/29 mã lỗi** ở SRS v1.2.2 Phụ lục B đều có đường sinh ra và có test tương ứng; mã `AUTH_USERNAME_EXISTS` đã bị xóa khỏi code.
- [ ] Dữ liệu mẫu đạt SRS v1.2.1 §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
- [ ] **0 vị trí** dùng mã **422** trong toàn bộ code và test.
- [ ] Bảng TTL cache trong code khớp **100%** với SRS NFR-PERF-003.
- [ ] Cấu hình Nginx và lệnh healthcheck trong repo khớp SRS v1.2.0 §6.5 (resolver động, không khối `upstream`; không lệnh healthcheck nào gọi công cụ không có trong image).
- [ ] **6 ADR** đã viết (MT-05, MT-08/09, MT-16, MT-34, MT-47, MT-50).
- [ ] Tag `v1.0.0` (phiên bản phần mềm), `CHANGELOG.md` cập nhật, `README.md` setup được trong < 5 phút.
- [ ] Nếu trong quá trình làm phát hiện thêm điểm SRS sai hoặc thiếu: ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` (MT-64 trở đi — MT-59 → MT-63 đã dùng cho CR-2026-04) và đi qua Change Request — **không** tự làm khác SRS trong code.

---

*— Hết kế hoạch —*

> **Nguồn chuẩn:** mọi yêu cầu trong tài liệu này bám theo `SPEC/SRS_Culinary_Blog_v1.2.2.md` (29/09/2026) — gồm CR-2026 (41 quyết định, SRS Phụ lục D), CR-2026-02 (16 quyết định, SRS Phụ lục E), CR-2026-03 (yêu cầu dữ liệu mẫu, SRS §2.6.1) và CR-2026-04 (5 quyết định, SRS Phụ lục G). Hiện trạng code được đối chiếu trực tiếp với nhánh `main` và `BAO_CAO_BUOI_2.md`. Mọi xung đột và mâu thuẫn — trong SRS, giữa SRS và code, giữa SRS và lộ trình được giao — đều được ghi đầy đủ tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md`. Kế hoạch **không có điểm nào làm khác SRS v1.2.2**; khi phát hiện khác biệt, **SRS là nguồn đúng** và mọi thay đổi SRS phải qua Change Request.
