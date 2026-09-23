# KẾ HOẠCH PHÁT TRIỂN CHI TIẾT 8 BUỔI LÀM VIỆC – CULINARY BLOG
## (FEATURE-DRIVEN & ARCHITECTURAL RATIONALE)

> **Nguồn chuẩn yêu cầu:** `SPEC/SRS_Culinary_Blog_v1.2.1.md` (v1.2.1, Đã duyệt 21/09/2026) = v1.1.0 (CR-2026, MT-01 → MT-41) + **CR-2026-02** (MT-42 → MT-57, v1.2.0) + **CR-2026-03** (yêu cầu dữ liệu mẫu, v1.2.1). Các câu "SRS v1.1.0 (MT-xx) chốt…" hay "SRS v1.2.0…" trong tài liệu chỉ thời điểm quyết định được đưa ra; mọi quyết định đó vẫn còn nguyên hiệu lực trong v1.2.1.
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
| Tài liệu đổi tên | `KE_HOACH_PHAT_TRIEN_7_BUOI.md` → `KE_HOACH_PHAT_TRIEN_8_BUOI.md`; `BAO_CAO_BUOI_1.md` → `BAO_CAO_BUOI_2.md`; `SRS_Culinary_Blog_v1.2.0.md` → `SRS_Culinary_Blog_v1.2.1.md` |
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

**Buổi 2 thực tế đã làm nhiều hơn kế hoạch** (theo `BAO_CAO_BUOI_2.md` §4.6 và code): `FR-JOB-001` Welcome Email đã chạy thật qua Hangfire + MailKit (retry 1′/5′/30′), Hangfire chạy ở **container worker riêng**, healthcheck Postgres/Redis/MinIO đã có, Nginx đã chuyển tiếp `X-Forwarded-For` và dùng **resolver DNS động**. Kế hoạch này **không giao làm lại** những việc đã xong; phần việc trống ra ở Buổi 3 của Dev 4 được dùng để dựng **integration test harness** — thứ mà mọi dev cần từ Buổi 3 trở đi.

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

---

## 2. Quyết định kiến trúc chung (cả nhóm tuân thủ — theo SRS v1.2.1)

### 2.1 Cấu trúc repository (monorepo — giữ nguyên từ Buổi 2)

```
ptudwnc_nhom20/
├─ SPEC/                          # SRS v1.2.0 (+ bản gốc v1.0.0 PDF để đối chiếu), hồ sơ mâu thuẫn, kế hoạch này
├─ backend/
│  ├─ CulinaryBlog.sln
│  ├─ src/CulinaryBlog.Domain/          # Entities, VO (Slug, EmailAddress), Enums, IRepository – KHÔNG NuGet
│  ├─ src/CulinaryBlog.Application/     # Commands/Queries/Handlers (MediatR), Validators, DTOs, Behaviors, Interfaces
│  ├─ src/CulinaryBlog.Infrastructure/  # EF Core DbContext, Configurations, Migrations, Repos, JwtService, MinIO, Redis, MailKit, Hangfire jobs
│  ├─ src/CulinaryBlog.API/             # Minimal API Endpoint groups, Middleware, Program.cs, Scalar /scalar
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
| Lỗi | RFC 7807 qua `GlobalExceptionMiddleware`; `type` = Application Error Code (SRS Phụ lục B, **27 mã** ở v1.2.0) |
| Response danh sách | `PagedResult<T> { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }` — khớp hình dạng Buổi 2 đã hiện thực; SRS v1.2.0 đã chốt (§5.2, **MT-42**), bỏ hẳn dạng `{ data, meta }`. Đối tượng đơn trả thẳng DTO |
| Phân trang | `pageSize` mặc định **12**, max 50; `sortBy ∈ {createdAt, publishedAt, title, cookTime, prepTime}`, `sortOrder ∈ {asc, desc}` — ngoài whitelist → **400** |
| Cache | **Redis cache-aside là cơ chế duy nhất.** Output Cache của .NET **không được dùng**. TTL theo bảng chuẩn NFR-PERF-003 |
| Endpoint | `/api/v1/...`, mỗi module 1 file `XxxEndpoints.cs` (`MapGroup`) – không Controller |
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
| **D-3** | `Recipe.Slug` là UNIQUE thường | `RecipeConfiguration.cs:60` | **Partial unique** `WHERE "IsDeleted" = false` | **B6 – Dev 2** | MT-05 |
| **D-4** 🔴 | `GetRecipesQuery` lọc theo danh tính (Guest 46 / Author 47 / Admin 50 bản ghi) **và** bị Output Cache theo khóa công khai; `GetRecipeBySlug` trả **403** cho Draft (lộ sự tồn tại của bản nháp) | `RecipeReadRepository.cs:87-97`, `GetRecipeBySlugQuery.cs:31-33` | **Chỉ `Published` cho mọi người gọi**; Draft/Archived → **404** như slug không tồn tại; dữ liệu riêng qua `/recipes/mine` | **B6 – Dev 2** | **MT-34** |
| **D-5** 🔴 | `GetCategoryBySlugQuery` cũng lọc theo danh tính | `GetCategoryBySlugQuery.cs:36` | **Chỉ `Published`**, không đọc `ICurrentUser` | **B6 – Dev 3** | **MT-34** |
| **D-6** | Output Cache (Redis-backed) `RecipeList` 15′ / `RecipeDetail` 60′ | `OutputCachePolicies.cs:16-24`, `RecipesEndpoints.cs:19,26` | **Bỏ Output Cache**, dùng `ICacheable` Redis cache-aside TTL 2′/5′ | **B6 – Dev 2**; audit **B7 – Dev 4** | MT-16, MT-17, MT-34 |
| **D-7** | FE fetch `revalidate` 3600 (`/categories`) và 600 (`/categories/[slug]`) | `BAO_CAO_BUOI_2.md` §4.5 | **1800** và **120** | **B6 – Dev 3** | MT-33.3 |
| **D-8** | `Recipe.Instructions` là `NOT NULL` | `RecipeConfiguration.cs:25` `.IsRequired()` | **NULL** (legacy field) | **B3 – Dev 2** — *phải làm trước khi `POST /recipes` nhận `instructions?` tùy chọn, nếu không mọi request thiếu trường này sẽ lỗi ở DB* | MT-20.8 |
| **D-9** | Hangfire chạy ở **container worker riêng** (`Hangfire__WorkerOnly=true`), trong khi SRS v1.1.0 §3.6 ghi "in-process" | `docker-compose.yml` service `hangfire`, `Program.cs:17-18` | **SRS v1.2.0 đã chuẩn hóa theo code** — worker riêng (MT-47). Không còn lệch, không retrofit | — | MT-47 |
| **D-10** | Sắp xếp bằng **một tham số `sort=-field`**; enum thiếu `PrepTime`; FE gửi `sort=-createdAt` | `RecipeSortParser.cs`, `IRecipeReadRepository.cs:7-13`, `RecipesEndpoints.cs:42`, `frontend/src/app/recipes/(list)/page.tsx:37` | **`sortBy` + `sortOrder`**, whitelist 5 trường | **B5 – Dev 3** (FR-SRCH-003) | MT-01 |
| **D-11** | Mọi lỗi validation trả **422**; FE bắt lỗi field bằng `status === 422` | `GlobalExceptionMiddleware.cs:65-83`, `AuthEndpoints.cs:17,26`, `RecipesEndpoints.cs:21`, `LoginForm.tsx:35`, `RegisterForm.tsx:44` | **400** cho mọi lỗi validation; **bỏ hẳn 422** | **B3 – commit nền đầu buổi** (Dev 4 dẫn) | MT-08 |
| **D-12** | Access token **và** refresh token lưu trong **`localStorage`** | `auth-context.tsx:39-56`; `BAO_CAO_BUOI_2.md` §4.12 ("cần xem lại Buổi 4") | Access token **chỉ trong bộ nhớ**; refresh token là thứ duy nhất được lưu bền (xem giải trình B4 Dev 1) | **B4 – Dev 1** | NFR-SEC-002 |
| **D-13** | TTL `categories:all` = **60 phút** | `GetCategoriesQuery.cs:11` | **30 phút** | **B3 – Dev 3** | MT-17 |
| **D-14** | `next/image` chạy `unoptimized`; URL ảnh là `localhost:9000` — **container frontend không truy cập được** | `BAO_CAO_BUOI_2.md` §4.13, `MinioOptions.PublicBaseUrl` | Ảnh phục vụ qua Nginx `/media/` → MinIO, bật lại tối ưu ảnh | **B7 – Dev 4** (Nginx) + **B7 – Dev 3** (Next.js) | NFR-PERF-005 |
| **D-15** | `Recipe.Publish()` **cho phép Archived → Published** và coi Published → publish là no-op; ném `DomainException` chung không có mã lỗi | `Recipe.cs:114-128` | Chuyển trạng thái ngoài bảng → **409 `RECIPE_INVALID_STATE_TRANSITION`**; thiếu step/ingredient → **400 `RECIPE_PUBLISH_INCOMPLETE`** | **B5 – Dev 2** | MT-35, MT-06 |
| **D-16** | UNIQUE `(RecipeId, StepNumber)` khai báo bằng **unique index** — PostgreSQL **không cho phép index là `DEFERRABLE`** | `RecipeConfiguration.cs:87` `HasIndex(...).IsUnique()` | Đổi thành **unique constraint `DEFERRABLE INITIALLY DEFERRED`** để renumber trong một transaction | **B4 – Dev 2** | MT-03 |
| **D-17** | Khóa DataProtection lưu **không mã hóa** trên volume `dpkeys` | `BAO_CAO_BUOI_2.md` §5.1 (cảnh báo `XmlKeyManager[35]`) | `ProtectKeysWithCertificate` ở production | **B7 – Dev 4** (cùng HTTPS) | NFR-SEC-007 |
| **D-18** | `init.sql` tạo text search config `vietnamese_unaccent` — **cơ chế FTS thứ hai**, lại chỉ chạy trong Docker (Testcontainers không chạy file này) | `docker/postgres/init.sql` | Một cơ chế duy nhất theo SRS: `simple` + `unaccent_immutable`, khai báo **trong migration** | **B4 – Dev 3** | MT-25 |

**Phân bổ retrofit theo buổi:** B3 → D-1, D-8, D-11, D-13 · B4 → D-2, D-12, D-16, D-18 · B5 → D-10, D-15 · B6 → D-3, D-4, D-5, D-6, D-7 · B7 → D-14, D-17 (+ audit D-6).

> **Vì sao không dồn retrofit vào đầu Buổi 3:** dồn 17 hạng mục vào một buổi sẽ tạo một commit khổng lồ chạm vào code của cả 4 dev cùng lúc → xung đột merge và không ai review nổi. Mỗi hạng mục được gắn vào **đúng buổi mà dev đó đang mở lại file liên quan** — sửa khi tay đã ở trong file là rẻ nhất và dễ review nhất. **Ngoại lệ duy nhất là D-11** (422 → 400): nó chạm vào middleware dùng chung và mọi endpoint mới của Buổi 3 đều phụ thuộc vào nó, nên phải làm **trước tiên** dưới dạng commit nền.

### 4.2 CR-2026-02 — Bổ sung và làm rõ SRS (đã duyệt, áp dụng trong SRS v1.2.0)

Khi đối chiếu SRS v1.1.0 với code Buổi 2, kế hoạch phát hiện 8 điểm SRS còn thiếu hoặc tự mâu thuẫn. SRS quy định *mọi thay đổi sau khi duyệt phải qua Change Request*, nên kế hoạch **không tự thêm endpoint ngoài SRS**: cả 8 điểm được gom thành **CR-2026-02**, đã được duyệt và áp dụng vào **SRS v1.2.0** (19/09/2026). Phân tích đầy đủ từng điểm (hiện trạng, phương án, lý do chọn) nằm trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md` theo số MT tương ứng. Nhờ vậy, **mọi việc trong kế hoạch này đều truy vết được về một yêu cầu trong SRS** — không còn hạng mục nào phải chờ duyệt hay cần phương án dự phòng.

| # | Vấn đề ở SRS v1.1.0 | Quyết định trong SRS v1.2.0 | MT | Buổi thực hiện |
|---|---|---|---|---|
| **C-1** | Hình dạng response danh sách: §5.2/§8 ghi `{ data, meta }` (kèm `pageSize: 10`), FR-SRCH-004 và code dùng `PagedResult` | Chốt **`PagedResult<T>`**; đối tượng đơn trả thẳng DTO | MT-42 | Đã đúng từ B2 — không phải sửa code |
| **C-2** | `POST /files/upload`, `DELETE /files/{**key}` có từ Buổi 2 nhưng ngoài SRS — trong khi `avatarUrl`, `imageUrl` danh mục và ảnh bước nấu **chỉ nhận URL** | **Chính thức hóa** vào FR-FILE-001/002, thêm Chương 8.8 | MT-43 | Đã có từ B2; dùng ở B3 (Dev 3), B4 (Dev 2), B5 (Dev 1) |
| **C-3** | `/dashboard/users` (§5.1) không có API danh sách người dùng | Thêm `GET /users` vào **FR-AUTH-008** | MT-44 | B7 – Dev 1 |
| **C-4** | Quản lý phiên có trong lộ trình (X-5) nhưng không có FR | Thêm **FR-AUTH-009** (mức C) + 3 endpoint `/auth/sessions` + claim `sid` | MT-45 | B7 – Dev 1 |
| **C-5** | 3 mã lỗi code đã dùng nhưng thiếu trong Phụ lục B | Thêm `FILE_FORBIDDEN` (403), `FILE_STORAGE_UNAVAILABLE` (503), `INTERNAL_ERROR` (500) — 24 → **27 mã**. **Không** thêm `AUTH_USERNAME_EXISTS` (bị xóa khi làm D-1) | MT-46 | Đã có từ B2; xóa `AUTH_USERNAME_EXISTS` ở B3 |
| **C-6** | Hangfire "in-process" (SRS) vs worker riêng (code) — xem **D-9** | Chuẩn hóa **worker riêng** | MT-47 | Không phải sửa code |
| **C-7** | Sitemap cần toàn bộ slug, `GET /recipes` giới hạn 50 | Thêm **`GET /recipes/sitemap`** gọn nhẹ, cache 1 giờ | MT-48 | B7 – Dev 2 |
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

# BUỔI 3 – ĐĂNG NHẬP GOOGLE / LOGOUT, TẠO DRAFT RECIPE, CRUD CATEGORIES ADMIN & HẠ TẦNG KIỂM THỬ

> **Mục tiêu buổi:** hệ thống có đủ đường vào (local + Google + thoát), Author tạo được nội dung nháp kèm ảnh, Admin quản trị được danh mục, job nền quan sát được qua dashboard an toàn, và **cả nhóm có hạ tầng integration test** để mọi buổi sau viết test thật.

> **30 phút đầu — commit nền (Dev 4 dẫn, cả nhóm review):** hoàn trả **D-11 (422 → 400)** trước khi 4 dev tách nhánh, vì mọi endpoint mới của buổi này đều trả lỗi validation.
> 1. `GlobalExceptionMiddleware.cs`: nhánh `ValidationException` và lỗi Identity trả **`StatusCodes.Status400BadRequest`** + `type = VALIDATION_ERROR`; xóa chú thích *"Quyết định dự án: lỗi validation → 422"*.
> 2. Đổi mọi `.ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)` thành `Status400BadRequest` (`AuthEndpoints.cs`, `RecipesEndpoints.cs`) để tài liệu Scalar khớp hành vi thật.
> 3. **Frontend:** `LoginForm.tsx`, `RegisterForm.tsx` đổi `error.status === 422` → **`=== 400`**, đồng thời đưa việc ánh xạ `errors{}` → field vào một helper dùng chung `mapProblemDetailsToForm()` trong `lib/api-client.ts` để các form mới không lặp lại điều kiện này.
> 4. Test hiện có đang assert 422 → sửa thành 400; thêm một test kiến trúc/grep chặn chuỗi `Status422UnprocessableEntity` quay lại.
>
> Commit: `refactor(api): unify validation errors to 400 per SRS v1.1.0 MT-08`
>
> **Vì sao phải làm trước tiên:** nếu đổi mã lỗi ở BE mà quên FE, form đăng ký sẽ **không còn hiện lỗi từng ô** — không crash, không log lỗi, chỉ âm thầm mất phản hồi cho người dùng. Loại hỏng hóc này chỉ phát hiện được khi có người thử nhập sai. Làm một lần, đúng một chỗ, trước khi có thêm form mới, là cách duy nhất không để lọt.

## DEV 1 — Google Sign-In & Đăng xuất

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-AUTH-003` Đăng nhập / Đăng ký bằng Google OAuth 2.0 (**ID Token flow**) + `FR-AUTH-005` Đăng xuất & Revoke Refresh Token. **Kèm retrofit D-1:** chuẩn hóa hợp đồng đăng ký theo `displayName` và BE tự sinh `UserName`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-1 — `UserNameGenerator` + hợp đồng mới:** tạo service `IUserNameGenerator` sinh `UserName` từ phần trước `@` của email (chuẩn hóa chữ thường, loại ký tự không hợp lệ theo `IdentityOptions.User.AllowedUserNameCharacters`, thêm hậu tố số `2, 3…` nếu trùng). Đổi `RegisterRequest`/`RegisterUserCommand` thành `{ email, password, displayName }`; `UserDto` thành `{ id, email, displayName, avatarUrl, bio, roles }`; xóa mã `AUTH_USERNAME_EXISTS`. FE: `RegisterForm` bỏ ô "Tên đăng nhập", đổi "Họ tên" thành "Tên hiển thị"; `AuthProvider` đọc `user.displayName`. **Không cần migration** — cột DB đã là `DisplayName`.
2. **FE — Google Identity Services:** cài `@react-oauth/google`, bọc `<GoogleOAuthProvider>` ở layout; dùng **component `<GoogleLogin onSuccess={({ credential }) => ...}>`** để lấy **ID Token**, POST lên `/api/v1/auth/google`. ⚠️ **Không dùng hook `useGoogleLogin`** — hook đó trả *access token/authorization code*, **không phải ID Token**, và backend sẽ không verify được. Không cấu hình redirect URI ở Backend, không dùng PKCE.
3. **CQRS — `GoogleLoginCommand { IdToken }`:** `GoogleJsonWebSignature.ValidateAsync(idToken, new ValidationSettings { Audience = [_cfg.GoogleClientId] })` — thư viện tự kiểm tra **chữ ký, `iss`, `aud`, `exp`**. Thiếu/sai định dạng → 400 `AUTH_GOOGLE_TOKEN_INVALID`; chữ ký/`aud`/`exp` sai → 401; không lấy được JWKS → **502 `AUTH_GOOGLE_UNAVAILABLE`**. Liên kết tài khoản: `FindByLoginAsync("Google", payload.Subject)` → có thì đăng nhập; chưa có mà email đã tồn tại thì **chỉ `AddLoginAsync` khi `payload.EmailVerified == true`** (ngược lại → 400); hoàn toàn mới thì tạo user với `DisplayName = payload.Name`, `AvatarUrl = payload.Picture`, `UserName` từ `IUserNameGenerator`, role `Author`. `IsActive == false` → **403 `AUTH_ACCOUNT_DISABLED`**.
4. **CQRS — `LogoutCommand { RefreshToken }`:** hash SHA-256 chuỗi nhận được, tìm token thuộc `ICurrentUser.UserId`, gán `RevokedAt = DateTime.UtcNow`. **Idempotent:** không tìm thấy vẫn trả 204 (không tiết lộ token có tồn tại hay không).
5. **API + UI + Test:** `POST /api/v1/auth/google` (200 `AuthResponseDto`), `POST /api/v1/auth/logout` (`RequireAuthorization()`, 204). UI: nút Google trên `/auth/login` kèm thông báo dự phòng khi nhận 502 (SRS §2.6.2); menu user → "Đăng xuất" gọi API rồi mới xóa phiên phía client (trả nợ `BAO_CAO_BUOI_2.md` §4.10 — trước đây nút này chỉ xóa token phía client, refresh token vẫn sống 7 ngày). Integration test (harness của Dev 4 buổi này): đăng ký với email `an.nguyen@x.com` hai lần khác nhau → `UserName` là `an.nguyen` và `an.nguyen2`; logout hai lần liên tiếp đều 204.

**Phần 3 – Định hướng & Lý do thiết kế:**
**ID Token flow** đẩy toàn bộ vòng lặp redirect về phía trình duyệt và Google, Backend chỉ còn một việc duy nhất: nhận một chuỗi JWT rồi xác minh chữ ký. Hệ quả kiến trúc là Backend **giữ nguyên tính stateless** — không phải lưu `state`, không phải lưu PKCE `code_verifier`, không phải mở thêm route callback, không cần `GoogleClientSecret`. Với một hệ thống đặt mục tiêu scale ngang (NFR-SCALE-001), mọi thứ phải lưu giữa hai request đều là gánh nặng: nó buộc ta phải chọn giữa sticky session hoặc một kho state dùng chung.

Điểm **bắt buộc không được cắt**: Backend phải **tự verify chữ ký** ID Token bằng `Google.Apis.Auth`. Nếu tin dữ liệu Frontend gửi lên (kiểu "FE đã đăng nhập Google rồi, BE cứ tạo user theo email FE báo") thì bất kỳ ai cũng có thể `curl` thẳng vào endpoint với email của người khác và chiếm tài khoản. Đây là lỗ hổng nghiêm trọng nhất có thể mắc trong luồng OAuth.

**Logout idempotent** là lựa chọn có chủ đích: trả 404 khi token không tồn tại sẽ biến endpoint thành một *oracle* cho phép kẻ tấn công dò xem chuỗi token nào đang hợp lệ. Ngoài ra người dùng bấm Đăng xuất hai lần (hoặc mạng chập chờn gây retry) không có lý do gì phải nhận lỗi.

Việc **link Google vào email đã tồn tại** thay vì tạo tài khoản trùng email giải quyết một tình huống rất thực tế: người dùng đăng ký bằng email/mật khẩu hôm trước, hôm sau bấm "Đăng nhập bằng Google" với cùng email đó. Nếu tạo tài khoản mới, họ sẽ mất toàn bộ công thức đã viết mà không hiểu vì sao. Nhưng tự động liên kết **chỉ an toàn khi Google xác nhận email đã được kiểm chứng** (`email_verified`): nếu liên kết theo một email chưa kiểm chứng, kẻ tấn công chỉ cần tạo tài khoản Google gắn email của nạn nhân là **chiếm được tài khoản Culinary Blog của nạn nhân**. Một dòng kiểm tra `payload.EmailVerified` chặn đứng kịch bản đó.

**Vì sao làm D-1 ngay buổi này, không đợi Buổi 5 (Profile):** luồng Google tạo tài khoản mới mà **không có ai nhập `userName`** — nên bắt buộc phải có bộ sinh `UserName` tự động ngay bây giờ. Khi đã có nó, giữ ô "Tên đăng nhập" ở form đăng ký thường là giữ hai cách tạo `UserName` song song cho cùng một hệ thống. Ngoài ra `UserDto` là hợp đồng mà **mọi màn hình** FE đọc để hiển thị người dùng; đổi nó ở Buổi 3 khi mới có 2 màn hình dùng là rẻ nhất — càng để muộn, càng nhiều component phải sửa.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 mô tả **ba kiến trúc Google OAuth khác nhau ở ba chỗ**: FR-AUTH-003 ghi *"Authorization Code Flow + PKCE, Auth.js v5 ở Next.js xử lý callback, FE gửi `ExternalLoginInfo` lên BE"*; mục 5.3 khai báo redirect URI là `/api/v1/auth/google/callback` (tức **BE** nhận callback); Chương 8 lại nhận body `{ idToken }` từ Google Sign-In JS SDK (ID-token flow, không phải Authorization Code). Nghiêm trọng hơn, `ExternalLoginInfo` là **kiểu nội bộ của ASP.NET Core Identity — không serialize qua HTTP được**, nên đặc tả đó không khả thi về mặt kỹ thuật.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-11**) chốt **ID Token flow**: body `{ idToken }`, BE verify bằng `GoogleJsonWebSignature.ValidateAsync`, **bỏ `ExternalLoginInfo`, bỏ Authorization Code + PKCE, bỏ redirect URI phía BE**. *(Đây là điểm xung đột **X-1** với lộ trình đề bài — xem §3.)*
- **Tại sao chọn:** So sánh ba phương án theo tầm nhìn dài hạn. **(A) ID Token** — đơn giản nhất, BE stateless, chỉ cần `GoogleClientId`, khớp Chương 8; nhược điểm là không lấy được refresh token của Google, nhưng ta **không cần** vì hệ thống tự phát JWT riêng. **(B) Tin Auth.js ở FE** — tận dụng thư viện có sẵn nhưng BE phải tin dữ liệu FE gửi lên → **lỗ hổng bảo mật nghiêm trọng**, loại ngay. **(C) BE tự chạy Authorization Code + PKCE** — chuẩn mực nhất về lý thuyết, nhưng BE phải quản lý state/verifier qua nhiều request (phá vỡ stateless), phải xử lý redirect qua lại giữa BE và FE, và tăng đáng kể số đường code cần test. Với hệ thống mà JWT do chính ta phát hành và Google chỉ đóng vai trò *"chứng minh người này sở hữu email X"*, phương án A đạt đúng mục tiêu bảo mật với chi phí kiến trúc thấp nhất.

**Phần 5 – Kết quả Commit Git:**
```
feat(auth): complete FR-AUTH-003 google id-token sign-in, FR-AUTH-005 logout revocation and displayName contract
```

## DEV 2 — Tạo công thức nháp & Quản lý ảnh

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-003` Tạo Công thức Nấu ăn Mới (trạng thái Draft) + `FR-RCP-008` Quản lý Ảnh Công thức (Upload / Metadata / Delete). **Kèm retrofit D-8:** `Instructions` chuyển sang `NULL`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-8 — migration `B3_Recipe_InstructionsNullable`:** `Instructions` bỏ `.IsRequired()` → cột **NULL**. Phải làm **trước** bước 2, vì body mới coi `instructions?` là tùy chọn — nếu cột còn `NOT NULL` thì mọi request không gửi trường này sẽ nổ ở `SaveChangesAsync()`.
2. **CQRS — `CreateRecipeCommand` + Validator:** body `{ title, description, categoryId, prepTime, cookTime, servings, difficulty, instructions?, nutrition? }` — `title` 5–200, `description` **20–2000** (Bảng Giới hạn Chuẩn §7.9), `prepTime > 0`, `cookTime >= 0`, `servings > 0`, `categoryId` phải tồn tại → sai thì **400 `VALIDATION_ERROR`**. Dùng factory **`Recipe.Create(...)` đã có sẵn từ Buổi 2** (`Status = Draft`, `PublishedAt = null`); slug qua `SlugHelper.Generate(title)` (đã có ở `Domain/Common`), trùng thì hậu tố `-2`, `-3`; **cấm trùng từ khóa dành riêng** `search`, `mine`, `sitemap`, `new`, `edit`. Nutrition gán qua `recipe.SetNutrition(...)` — **không có endpoint riêng**. Hai mảng tùy chọn `steps?`/`ingredients?` trong cùng body được kích hoạt ở **Buổi 4** (xem giải trình).
3. **Authorization dùng chung:** `RecipeAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Recipe>` (pass khi `recipe.AuthorId == currentUser.Id` **hoặc** user có role `Admin`); `AuthorPolicy = RequireRole("Author","Admin")`. Đây là handler dùng lại cho **toàn bộ** FR-RCP còn lại.
4. **CQRS ảnh:** `UploadRecipeImageCommand` gọi `IFileStorageService` + `ImageFileInspector` (magic bytes, có sẵn từ Buổi 2), folder `recipes/{recipeId}/`, rồi `recipe.AddImage(...)` (**đã có sẵn** — ảnh đầu tiên tự `IsPrimary = true`); `UpdateImageMetadataCommand { altText?, isPrimary?, orderIndex? }`; `DeleteRecipeImageCommand` xóa bản ghi + `BackgroundJob.Enqueue` xóa file MinIO, xóa đúng ảnh primary thì ảnh có **`OrderIndex` nhỏ nhất (hòa thì `CreatedAt` sớm nhất)** lên thay. ⚠️ **Đổi ảnh primary phải làm hai bước trong một transaction tường minh:** `SaveChanges` lần 1 hạ primary cũ, lần 2 nâng primary mới — vì index `IDX_RecipeImage_Primary` (partial unique, có từ Buổi 2) **không thể deferrable**, và EF Core không đảm bảo thứ tự hai lệnh `UPDATE` trong một batch. Tất cả implement `ICacheInvalidator` → xóa prefix `recipes:list:` + khóa `recipe:{slug}`.
5. **API + UI:** `POST /api/v1/recipes` (201); `POST /api/v1/recipes/{id}/images` (multipart, 201 trả `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }`, **503 `FILE_STORAGE_UNAVAILABLE`** nếu MinIO lỗi); `PATCH` và `DELETE /api/v1/recipes/{id}/images/{imageId}`. UI `/dashboard/recipes/new` — wizard bước 1 (thông tin cơ bản + nutrition), bước 2 (gallery ảnh dùng `ImageUploader`, chọn ảnh chính bằng radio → gọi PATCH), toast xác nhận. Integration test: đổi primary giữa hai ảnh 10 lần liên tiếp không lần nào vỡ unique index.

**Phần 3 – Định hướng & Lý do thiết kế:**
Công thức **luôn sinh ra ở trạng thái `Draft`** chứ không cho tạo thẳng Published: người viết cần không gian nháp để hoàn thiện dần, và hệ thống cần một điểm chặn để kiểm tra điều kiện xuất bản (đủ step + ingredient) ở FR-RCP-005. Cho phép tạo thẳng Published sẽ mở đường cho công thức rỗng lọt ra ngoài và làm hỏng structured data SEO.

**Nutrition đi kèm trong body `POST /recipes`** chứ không có endpoint riêng — hệ quả trực tiếp của việc nó là Owned Entity. Nếu làm `PUT /recipes/{id}/nutrition` riêng, ta phải tạo thêm command, validator, DTO và repository cho một thứ vốn chỉ là nhóm cột trong bảng `Recipes`, đồng thời mở ra khả năng **hai đường code cùng ghi một nhóm cột** — nguồn bug kinh điển. Frontend dùng wizard nhiều bước thì gom state ở client rồi submit một lần.

`RecipeAuthorizationHandler` được dựng **ngay ở FR đầu tiên có ghi dữ liệu**, không đợi tới lúc có nhiều endpoint mới gom lại. Lý do: NFR-SEC-006 yêu cầu kiểm tra phân quyền **tại Application Layer**, không chỉ ở Presentation. Nếu mỗi handler tự viết `if (recipe.AuthorId != userId) throw` thì chỉ cần một handler quên là có lỗ hổng, và không có cách nào kiểm chứng tập trung.

Quy tắc **"xóa ảnh primary thì ảnh nào lên thay"** được ghi thành tiêu chí xác định (`OrderIndex` nhỏ nhất, hòa thì `CreatedAt` sớm nhất) thay vì "ảnh đầu tiên còn lại": "đầu tiên" theo thứ tự nào là câu hỏi không có đáp án nếu không nói rõ, và hai dev sẽ hiểu hai kiểu — một người lấy theo thứ tự trả về của DB (vốn không đảm bảo), một người lấy theo thời gian tạo.

**Vì sao hai mảng inline `steps?`/`ingredients?` của `POST /recipes` để sang Buổi 4:** chúng cần đúng những bất biến mà FR-RCP-009/010 xây ở Buổi 4 — cột `QuantityText`, ràng buộc "không rỗng cả ba", `StepNumber` do server gán. Làm chúng ngay bây giờ nghĩa là viết validator nguyên liệu/bước **hai lần** (một lần cho body inline, một lần cho endpoint riêng) rồi phải giữ đồng bộ mãi mãi — đúng loại trùng lặp logic sinh nợ kỹ thuật. Ở Buổi 4, body inline chỉ việc gọi lại `recipe.AddIngredient(...)`/`recipe.AddStep(...)`, nên mỗi quy tắc chỉ tồn tại ở một chỗ. Giao diện của Buổi 3 cũng không cần chúng (wizard bước 3–4 thuộc Buổi 4), nên lát cắt DB → API → UI của buổi này vẫn trọn vẹn.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-008 của v1.0.0 thiết kế endpoint hành động riêng `PATCH /recipes/{id}/images/{imgId}/primary` với response upload `{ url, isPrimary }`, trong khi Chương 8.4 thiết kế `PATCH /recipes/{id}/images/{imageId}` với body gộp `{ altText?, isPrimary?, orderIndex? }` và response 4 trường — hai thiết kế API khác hẳn nhau. (b) FR-RCP-008 bước 1 nói form-data chỉ chứa field `"file"` nhưng bước 6 lại dùng `altText`. (c) Nhánh A4 trả **503** khi MinIO lỗi nhưng ô "HTTP Status Code trả về" của chính FR đó **không liệt kê 503**. (d) FR-RCP-003 nói Steps/Ingredients "có thể tạo cùng lúc hoặc thêm riêng lẻ sau", nhưng nutrition bị bỏ lửng — không FR nào đặc tả endpoint riêng cho nó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-19, MT-41.4, MT-41.5, MT-41.6, MT-02**) chốt **PATCH metadata gộp**, response upload đủ 7 trường (thêm `mediumUrl`, `thumbnailUrl` vì FR-JOB-002 sinh ra chúng), form-data có `altText?`/`isPrimary?`, bổ sung **503** vào danh sách status code, và ghi rõ **nutrition chỉ tạo/sửa cùng recipe**.
- **Tại sao chọn:** PATCH gộp **RESTful hơn** — một tài nguyên, một endpoint, sửa được nhiều trường trong một request; endpoint hành động riêng `/primary` sẽ kéo theo `/alt-text`, `/order` khi cần sửa các trường khác, làm số endpoint phình theo số thuộc tính. Nhược điểm của PATCH gộp là logic "chỉ 1 ảnh primary" nằm lẫn trong handler chung nên phải xử lý cẩn thận — chấp nhận được, và đã được ghi thành hai quy tắc nghiệp vụ tường minh trong SRS Chương 8.4 để không ai quên. Về `mediumUrl`/`thumbnailUrl`: trả về ngay cả khi còn `null` (job resize chưa chạy) giúp **hợp đồng API ổn định** — FE không phải xử lý hai hình dạng response khác nhau tùy thời điểm.

**Phần 5 – Kết quả Commit Git:**
```
feat(recipes): complete FR-RCP-003 create draft recipe & FR-RCP-008 image management with merged patch metadata
```

## DEV 3 — CRUD Danh mục [Admin]

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-CAT-003` Tạo Danh mục + `FR-CAT-004` Cập nhật Danh mục + `FR-CAT-005` Xóa Danh mục (Admin, Auto Slug, Conflict 409). **Kèm retrofit D-13:** TTL `categories:all` 60 → **30 phút**.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **CQRS — `CreateCategoryCommand`:** Validator `name` **2–100 ký tự** (theo §7.9, **không phải 2–50** như v1.0.0), `description?`, `imageUrl?` URL hợp lệ, `orderIndex?` ≥ 0. Handler **kiểm tra `Name` trùng chủ động** trước khi ghi → ném `ConflictException` → **409 `CATEGORY_NAME_EXISTS`**; sinh slug tự động + hậu tố `-2`,`-3` nếu trùng; trả 201 kèm header `Location: /api/v1/categories/{slug}`.
2. **CQRS — `UpdateCategoryCommand`:** cùng bộ validator; **cũng phải kiểm tra `Name` trùng** (đổi sang tên đã có → 409); **slug KHÔNG đổi** khi đổi tên (tránh chết link đã chia sẻ).
3. **CQRS — `DeleteCategoryCommand`:** đếm recipe thuộc danh mục (`IsDeleted = false`); nếu > 0 → **409 `CATEGORY_DELETE_HAS_RECIPES`** kèm số lượng trong `detail` để Admin biết cần chuyển bao nhiêu công thức; nếu = 0 → **soft delete** (`IsDeleted = true`), **không xóa vật lý**.
4. **Lớp phòng vệ thứ hai:** trong `GlobalExceptionMiddleware`, bắt `DbUpdateException` có `PostgresException.SqlState == "23505"` (unique_violation) → dịch thành 409 thay vì 500 — phòng tình huống hai Admin tạo trùng tên cùng lúc (race condition) lọt qua bước kiểm tra chủ động.
5. **Cache + API + UI:** **retrofit D-13** — `GetCategoriesQuery.Expiration` đổi `TimeSpan.FromMinutes(60)` → **`30`** theo bảng TTL chuẩn §2.3 (buổi này mới có đường ghi danh mục, nên đây là lúc TTL bắt đầu có ý nghĩa thật); cả 3 command implement `ICacheInvalidator` → xóa `categories:all` và prefix `categories:detail:`. API: `POST /categories`, `PUT /categories/{id:guid}`, `DELETE /categories/{id:guid}` — tất cả `RequireAuthorization("AdminPolicy")` → 403 cho Author. UI `/dashboard/categories` (Admin only): bảng danh sách, modal form RHF+Zod (name, description, `ImageUploader` cho imageUrl, orderIndex), confirm dialog khi xóa, **hiển thị lỗi 409 thân thiện** ("Danh mục này đang có 7 công thức, hãy chuyển chúng sang danh mục khác trước"). Integration test: 201 / 403 / 409-trùng-tên / 409-còn-recipe.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Kiểm tra `Name` trùng chủ động ở tầng Application** thay vì để ràng buộc UNIQUE của database bắt, vì hai lý do. Thứ nhất về trải nghiệm: dựa vào exception của DB thì người dùng nhận **HTTP 500 "Lỗi hệ thống"** thay vì một thông báo nghiệp vụ rõ ràng, và log bị nhiễu bởi exception không đáng có. Thứ hai về khả năng bảo trì: dò mã lỗi PostgreSQL `23505` trong middleware là code khó đọc và phụ thuộc vào chi tiết của một DBMS cụ thể. Tuy nhiên **vẫn giữ lớp bắt `23505`** làm phòng vệ thứ hai — kiểm tra chủ động không loại bỏ được hoàn toàn khe hở race condition giữa lúc `SELECT` và lúc `INSERT`.

**Slug không đổi khi đổi tên** là quyết định bảo vệ SEO và người dùng: URL đã được chia sẻ, đã được Google lập chỉ mục, đã nằm trong bookmark. Đổi slug mà không có bảng lịch sử thì mọi link cũ chết ngay lập tức. Hệ thống cũng **không xây bảng lịch sử slug** — xem phân tích ở FR-RCP-004 (Buổi 5) và NFR-SEO-004.

**Soft delete cho Category** (không hard delete) giữ đúng thiết kế `BaseEntity` áp dụng cho toàn hệ thống. Ràng buộc FK `Recipes.CategoryId → Categories.Id` vẫn giữ `ON DELETE RESTRICT` — không phải để chặn thao tác xóa mềm (xóa mềm không đụng tới FK), mà là **lớp bảo vệ cuối cùng** cho mọi thao tác xóa **vật lý** danh mục: hiện FR-JOB-003 chỉ dọn Recipe, nhưng nếu sau này mở rộng sang Category (hoặc có người xóa tay trong DB), database sẽ từ chối xóa một danh mục còn công thức tham chiếu thay vì để lại dữ liệu mồ côi.

Thông báo lỗi 409 hiển thị **số lượng công thức cụ thể** thay vì câu chung chung: Admin cần biết quy mô việc phải làm trước khi quyết định, và con số đó server đã đếm sẵn rồi — không trả về là lãng phí.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** bốn ô trong **cùng một bảng FR-CAT-003** không khớp nhau: ô "Điều kiện tiên quyết" ghi *"Name chưa tồn tại trong database"*; ô "HTTP Status Code" ghi *"409 Conflict – Name đã tồn tại"*; nhưng **Luồng chính bước 6 chỉ kiểm tra slug** (trùng thì tự thêm `-2`, `-3`), **không kiểm tra Name**; và Luồng thay thế chỉ có A1 (403) và A2 (422) — **không có nhánh nào dẫn tới 409**. Thêm nữa, validator ghi `Name` 2–50 ký tự trong khi cột DB là `varchar(100)` — lệch 2 lần. FR-CAT-005 ghi "Xóa entity" còn Chương 8 ghi "soft delete".
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-36, MT-37, MT-20.11**) bổ sung **bước kiểm tra Name trùng** vào cả FR-CAT-003 lẫn FR-CAT-004, chốt `Name` **2–100** theo Bảng Giới hạn Chuẩn §7.9, và chốt **soft delete**.
- **Tại sao chọn:** mã 409 ở v1.0.0 là **mã không bao giờ được sinh ra** — tạo danh mục trùng tên "Món chính" lần thứ hai sẽ đi hết luồng (slug `mon-chinh` trùng → tự đổi thành `mon-chinh-2` → `AddAsync` → `SaveChangesAsync`), rồi mới bị PostgreSQL chặn vì `Name` là UNIQUE → HTTP 500. Một mã lỗi được đặc tả mà không có đường nào sinh ra nó là dấu hiệu đặc tả chưa được đọc ngang qua bốn ô. Về giới hạn độ dài: nguyên tắc dài hạn chốt trong SRS §7.9 là **validator và cột DB phải BẰNG NHAU**, không phải "validator chặt hơn cho an toàn" — khi hai con số lệch nhau, không ai biết con số nào là yêu cầu thật, QA không biết lấy đâu làm chuẩn viết test, và dữ liệu nhập qua seeding sẽ lọt qua validator rồi vẫn nằm được trong DB.

**Phần 5 – Kết quả Commit Git:**
```
feat(categories): complete FR-CAT-003/004/005 admin crud with proactive name conflict check and soft delete
```

## DEV 4 — Hangfire Dashboard an toàn & Integration Test Harness

**Phần 1 – Chức năng hoàn thành trong buổi:**
Hoàn tất `FR-JOB-001` — phần còn lại là **Hangfire Dashboard `/hangfire` được bảo vệ** (bản thân job email đã chạy thật từ Buổi 2, `BAO_CAO_BUOI_2.md` §4.6) + **Integration Test Harness** dùng chung cho cả nhóm (NFR-MAINT-002) + dẫn **commit nền D-11**.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Map dashboard với filter "cổng Nginx":** trong `Program.cs` (chỉ ở container `api`, không ở worker) gọi `app.MapHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [new NginxGateDashboardFilter(gateSecret)] })`. `NginxGateDashboardFilter : IDashboardAuthorizationFilter` chỉ cho qua khi header `X-Hangfire-Gate` **khớp secret** trong cấu hình (`Hangfire__DashboardGateSecret`, so sánh bằng `CryptographicOperations.FixedTimeEquals`). ⚠️ Bắt buộc thay filter mặc định: `LocalRequestsOnlyAuthorizationFilter` của Hangfire **từ chối mọi request đi qua proxy** (request đến từ IP container Nginx, không phải localhost) → dashboard sẽ luôn trả 401.
2. **Nginx Basic Auth (giữ nguyên cơ chế resolver động của Buổi 2):** thêm block riêng trước block `location ~ ^/(health|hangfire|scalar|openapi)`:
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
3. **Integration Test Harness — `tests/CulinaryBlog.API.IntegrationTests`:** project này **đã được tạo khung ở Buổi 2** (đã tham chiếu `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `Microsoft.AspNetCore.Mvc.Testing`, `coverlet`) nhưng **chưa có file test nào** — buổi này lấp đầy nó, bổ sung package `Respawn` và `public partial class Program;` ở API để `WebApplicationFactory` truy cập được. `CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime` khởi động **Testcontainers** `postgres:16-alpine` + `redis:7-alpine`, ghi đè connection string, chạy migration, tắt Hangfire server (`Hangfire:ServerEnabled=false`) và thay `IBackgroundJobClient`/`IFileStorageService` bằng bản giả ghi lại lời gọi. Kèm `Respawn` để dọn DB giữa các test và helper `CreateClientAs(role)` phát JWT thật qua `ITokenService`.  Testcontainers **không chạy `docker/postgres/init.sql`** → mọi extension/DDL mà code cần phải nằm **trong migration** (điều kiện tiên quyết của D-18, Buổi 4).
4. **Trả nợ test Buổi 2:** viết integration test cho toàn bộ endpoint Buổi 2 (`BAO_CAO_BUOI_2.md` §6 ghi tồn đọng): register/login (201/400/401/409/423), recipes list/detail, categories list/detail, files upload/delete (201/400/401/403/204). Thêm sẵn **test tái hiện lỗ hổng MT-34** (Admin gọi `GET /recipes` rồi Guest gọi lại cùng URL) đánh dấu `[Fact(Skip = "Lỗ hổng đã biết — retrofit D-4 ở Buổi 6")]` để lỗ hổng được **ghi thành test ngay hôm nay**, không phụ thuộc trí nhớ.
5. **Kiểm chứng:** `http://localhost/hangfire` qua Nginx → hỏi Basic Auth → vào được dashboard, thấy job `WelcomeEmailJob` Succeeded; gọi thẳng `http://localhost:5000/hangfire` (bỏ qua Nginx) → **401**; `dotnet test` chạy toàn bộ integration test xanh trên máy có Docker. Giảm `QueuePollInterval` xuống 1 giây ở môi trường dev (tồn đọng §6 của báo cáo Buổi 2).

**Phần 3 – Định hướng & Lý do thiết kế:**
**Vì sao dựng integration test harness ngay Buổi 3 — đây là quyết định quan trọng nhất của Dev 4 trong buổi này.** Definition of Done của nhóm (§2.4) đòi `dotnet test` xanh ở **mọi** commit, NFR-MAINT-002 đòi *"mọi endpoint có ít nhất 1 happy path + 1 error case"*, và các buổi sau liên tục yêu cầu test kiểu *"10 request `PUT` đồng thời chỉ 1 thành công"*. Kế hoạch 6 buổi cũ đặt harness ở Buổi 6–7 (theo cách đánh số của kế hoạch đó) — nghĩa là 5 buổi liền **không ai viết được integration test**, lỗi tích tụ rồi dồn cục vào cuối dự án. Buổi 2 đã làm sớm FR-JOB-001, nên phần thời gian trống ra của Dev 4 được dùng đúng vào chỗ có đòn bẩy lớn nhất: một hạ tầng mà cả 4 dev dùng mỗi ngày.

**Testcontainers thay vì EF Core In-Memory:** provider In-Memory **không có** ràng buộc UNIQUE, không có FK cascade, không có `tsvector`, không có partial index, không có concurrency token thật — tức là đúng những thứ cần kiểm chứng nhất (MT-03, MT-05, MT-25, MT-09) đều biến mất. Test xanh trên In-Memory rồi vỡ trên PostgreSQL thật là kịch bản tệ nhất vì nó tạo ra **niềm tin sai**. Testcontainers chạy đúng `postgres:16-alpine` như production.

**Hai lớp bảo vệ cho dashboard, không phải một:** Basic Auth ở Nginx là lớp chính; header `X-Hangfire-Gate` là lớp thứ hai cho tình huống **có đường vào API không đi qua Nginx** — ở môi trường dev, cổng `5000:8080` của `api` được mở ra ngoài (§6.5), nên ai gõ thẳng `:5000/hangfire` sẽ vượt qua Basic Auth nếu chỉ có một lớp. Secret chỉ Nginx biết, được Nginx gắn vào **sau khi** người dùng qua Basic Auth. Chi phí: khoảng 15 dòng code; lợi ích: dashboard (vốn có quyền xóa/chạy lại mọi job) không bao giờ lộ ra dù cấu hình cổng thay đổi thế nào.

**Không làm lại những gì Buổi 2 đã xong:** PostgreSQL storage, `IEmailService`/`MailKitEmailService`, `WelcomeEmailJob` với backoff 1′/5′/30′, seam `IWelcomeEmailScheduler` → `HangfireWelcomeEmailScheduler` đều đã chạy thật (báo cáo Buổi 2 §2.1: *"Container `hangfire` gửi thành công → Mailhog"*). Giao lại việc đã hoàn thành chỉ vì kế hoạch cũ ghi vậy là lãng phí nguồn lực và dễ làm hỏng thứ đang chạy.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) v1.0.0 quy định Hangfire Dashboard *"chỉ Admin, policy-protected"* trong khi hệ thống xác thực bằng **JWT Bearer stateless** và mục 5.2 ghi rõ *"không dùng cookie để tránh CSRF"*. (b) FR-JOB-001 mô tả email chào mừng chứa *"link kích hoạt email (nếu cần)"* trong khi **không có FR nào** đặc tả luồng xác nhận email — cần rà lại template của `WelcomeEmailJob` Buổi 2 để chắc chắn không còn link này. (c) Kế hoạch 6 buổi cũ tự mâu thuẫn: Definition of Done đòi test ở mỗi commit, nhưng integration test harness lại xếp ở Buổi 6 của kế hoạch đó.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-39, MT-21**) chốt: Dashboard bảo vệ bằng **HTTP Basic Auth tại Nginx**; **bỏ link kích hoạt email**. Kế hoạch bổ sung lớp thứ hai (header gate) và đưa harness lên Buổi 3.
- **Tại sao chọn:** Hangfire Dashboard là **trang HTML mở trực tiếp trong trình duyệt** — khi Admin gõ `https://domain.com/hangfire`, trình duyệt gửi một request điều hướng thông thường, **không có cách nào đính kèm header `Authorization: Bearer`**. Mà SRS lại loại bỏ cookie. Kết quả ở v1.0.0: `IDashboardAuthorizationFilter` luôn thấy người dùng ẩn danh → **Admin không bao giờ vào được dashboard**, dù đây là quyền đã liệt kê ở mục 2.3. Ba phương án được cân nhắc: **(A)** chặn `/hangfire` khỏi internet, chỉ vào qua SSH tunnel — an toàn nhất nhưng bất tiện khi demo; **(B)** Basic Auth ở Nginx — ba dòng cấu hình, mở được từ trình duyệt, **không đụng gì tới JWT của ứng dụng**, đánh đổi là phải quản lý một bộ thông tin đăng nhập riêng; **(C)** cấp cookie riêng cho dashboard — mở lại cánh cửa cookie mà mục 5.2 vừa đóng, phải xử lý CSRF, thêm code chỉ để phục vụ một trang quản trị. Chọn **B cho môi trường demo/nộp bài**, khuyến nghị **A cho production thật**. Về link kích hoạt email: giữ nó sẽ tạo lời hứa suông với người dùng (bấm vào không có gì xảy ra) — tệ hơn là không có gì.

**Phần 5 – Kết quả Commit Git:**
```
feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness
```

---

# BUỔI 4 – REFRESH TOKEN ROTATION, NGUYÊN LIỆU & CÁC BƯỚC NẤU, POSTGRES FTS & JOB NỀN

> **Mục tiêu buổi:** phiên đăng nhập tự gia hạn an toàn, nội dung công thức đầy đủ (nguyên liệu + các bước), tìm kiếm tiếng Việt không dấu hoạt động, và hai job nền còn lại chạy thật.

## DEV 1 — Refresh Token Rotation & Reuse Detection

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-AUTH-004` Làm mới Access Token — Token Rotation & Reuse Detection. **Kèm retrofit D-2** (512 → **256-bit**), **D-12** (access token rời `localStorage`) và bật **`UseForwardedHeaders`** để `CreatedByIp` ghi đúng IP thật.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-2 + Domain:** trong `JwtTokenService` đổi `RefreshTokenBytes = 64` → **`32`** (**256-bit**), vẫn encode Base64URL. Bổ sung vào entity `RefreshToken` (hiện chỉ có factory `Create`): `Revoke(DateTime now, string? replacedByHash)` và `IsUsable(DateTime now) => RevokedAt == null && ExpiresAt > now` — nhận `now` làm tham số để Domain không gọi thẳng đồng hồ hệ thống (test được). `IRefreshTokenRepository` thêm `GetByHashAsync`, `RevokeAllForUserAsync(userId, now)`.
2. **CQRS — `RefreshTokenCommand` (cây quyết định đầy đủ):**
   - Hash chuỗi nhận được → tra `TokenHash`. Không thấy → **401 `AUTH_TOKEN_INVALID`**.
   - `ExpiresAt <= UtcNow` → **401 `AUTH_REFRESH_TOKEN_EXPIRED`**.
   - **`RevokedAt != null` → REUSE DETECTED:** gọi `RevokeAllForUserAsync(token.UserId)` (**revoke toàn bộ token family**), `Log.Warning("SECURITY ALERT: refresh token reuse detected for user {UserId} from IP {Ip}")`, trả **401 `AUTH_REFRESH_TOKEN_REVOKED`**.
   - `user.IsActive == false` → **403 `AUTH_ACCOUNT_DISABLED`** *(bước này khiến mã lỗi ở Phụ lục B thực sự được sinh ra — xem MT-22)*.
   - Hợp lệ → trong **một transaction**: revoke token cũ với `ReplacedByTokenHash = SHA256(newToken)`, tạo bản ghi token mới, phát `AuthResponseDto`.
3. **Lấy IP thật — `UseForwardedHeaders` (API đúng của .NET 10):** Nginx đã gửi `X-Forwarded-For` từ Buổi 2; phía API bật:
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
4. **API:** `POST /api/v1/auth/refresh` — **không yêu cầu Bearer** (access token đã hết hạn thì không dùng để xác thực được), chỉ cần refresh token trong body.
5. **UI — retrofit D-12 + single-flight interceptor:** `AuthProvider` giữ **access token chỉ trong bộ nhớ** (React state/ref), không ghi `localStorage` nữa; chỉ **refresh token** được lưu bền. Khi tải lại trang, `AuthProvider` gọi `/auth/refresh` một lần để lấy access token mới. `lib/api-client.ts` gặp 401 `AUTH_TOKEN_EXPIRED` → gọi `/auth/refresh` **đúng một lần** dù có bao nhiêu request song song (biến `refreshPromise` làm mutex), rồi retry các request đang treo; refresh thất bại → xóa phiên, `queryClient.clear()`, redirect `/auth/login?callbackUrl=`. Integration test: dùng lại token cũ sau khi rotate → cả family bị revoke; request đi qua header `X-Forwarded-For` từ mạng tin cậy → `CreatedByIp` ghi đúng IP đó.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Token Rotation** (mỗi lần refresh là cấp cặp mới và thu hồi cặp cũ) biến refresh token từ một *bí mật dài hạn 7 ngày* thành một *bí mật dùng một lần*. Nếu không rotate, kẻ trộm được token sẽ dùng thoải mái suốt 7 ngày mà hệ thống không có cách nào biết.

**Reuse Detection** là phần khiến rotation trở nên có giá trị thật. Logic đằng sau: sau khi rotate, token cũ **không còn ai có lý do chính đáng để dùng nữa**. Vậy nên nếu một token đã revoke bất ngờ được sử dụng, chỉ có hai khả năng — hoặc token đã bị đánh cắp và kẻ trộm đang dùng, hoặc chính người dùng đang dùng token mà kẻ trộm đã rotate mất. **Cả hai trường hợp đều có nghĩa là tài khoản đã bị xâm phạm**, nên phản ứng đúng là revoke toàn bộ family, buộc đăng nhập lại. Đây là cơ chế duy nhất trong thiết kế JWT stateless cho phép **phát hiện** việc token bị đánh cắp, thay vì chỉ chờ nó hết hạn.

**Vì sao chỉ lưu hash — giải trình kỹ theo yêu cầu:** raw token **không bao giờ** được ghi vào database; DB chỉ chứa `TokenHash = SHA-256(raw)`. Khi client gửi token lên, server hash chuỗi nhận được rồi tra theo `TokenHash` — cùng input thì cùng hash, nên vẫn tra cứu chính xác. Giá trị bảo mật: nếu database bị lộ (SQL injection, backup rò rỉ, nhân viên nội bộ đọc trộm), kẻ tấn công chỉ có trong tay các chuỗi hash — **không đảo ngược được thành token dùng được**. Đối chiếu với kịch bản lưu raw: người đọc được DB mạo danh được **mọi** người dùng cho tới khi token hết hạn, và hệ thống không có dấu hiệu gì để phát hiện. Đây chính xác là rủi ro mà NFR-SEC-002 cam kết chặn. Không dùng bcrypt/Argon2 ở đây (dù chúng mạnh hơn cho mật khẩu) vì token là **chuỗi ngẫu nhiên 256-bit**, không phải mật khẩu người dùng đặt — không có nguy cơ dictionary attack, nên SHA-256 là đủ và nhanh hơn nhiều (refresh token được tra cứu ở mọi lần gia hạn).

**Single-flight ở FE** là chi tiết nhỏ nhưng bắt buộc: một trang dashboard có thể bắn 5 request song song, cả 5 cùng nhận 401 khi access token hết hạn. Nếu mỗi request tự gọi refresh, **5 lần rotation liên tiếp** sẽ xảy ra — 4 lần sau dùng token đã bị lần trước revoke → kích hoạt nhầm Reuse Detection → người dùng bị đăng xuất dù không có ai tấn công.

**Giải trình D-12 — lưu token ở đâu, và vì sao không phải cookie:** SRS §5.2 quy định refresh token đi trong **body request, không dùng cookie** (để tránh CSRF). Hệ quả là refresh token buộc phải nằm ở nơi JavaScript đọc được — nên **không thể loại bỏ hoàn toàn rủi ro XSS** cho nó; điều làm được là **giảm tối đa những gì XSS lấy được và thời gian nó còn giá trị**. Access token (thứ mở được mọi API) chuyển về bộ nhớ nên biến mất khi đóng tab và không nằm trong `localStorage` để script lạ quét. Refresh token vẫn lưu bền nhưng giờ đã **dùng một lần** (rotation) và **bị phát hiện khi dùng lại** (reuse detection) — một token bị đánh cắp sẽ tự tố cáo ngay lần đầu cả kẻ trộm lẫn chủ thật cùng dùng. Lớp cuối là **CSP** ở Buổi 6 để chặn script lạ chạy ngay từ đầu. Phương án chuyển refresh token sang cookie `HttpOnly` an toàn hơn trước XSS nhưng **trái SRS §5.2** và kéo theo cơ chế chống CSRF — nếu nhóm muốn đi hướng đó thì phải qua Change Request, không tự quyết trong kế hoạch.

**Vì sao `UseForwardedHeaders` bật ở buổi này chứ không đợi Buổi 6 (Rate limiting):** chính buổi này là lúc mỗi lần refresh ghi thêm một bản ghi token kèm `CreatedByIp`. Để đến Buổi 6 mới bật nghĩa là hai buổi liền ghi **IP của container Nginx** vào cột audit — dữ liệu sai được ghi vĩnh viễn, không sửa lại được. Cấu hình chỉ khoảng mười dòng, không có lý do gì để trì hoãn và phải để lại "TODO".

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-AUTH-004/005 dùng cờ **`IsRevoked = true`** và kiểm tra `IsRevoked == false`, nhưng bảng `RefreshToken` ở Chương 7.8 **chỉ có `RevokedAt timestamptz NULL`** — *"NULL = còn hiệu lực"* — **không có cột `IsRevoked`**. (b) FR-AUTH-004 bước 5 gán `ReplacedByToken = newToken` (**raw token**) trong khi Chương 7.8 và NFR-SEC-002 quy định chỉ lưu hash. (c) Độ dài token 512-bit (FR) vs 128-bit (NFR).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-14, MT-15, MT-13**) chốt: **bỏ hẳn `IsRevoked`**, token hợp lệ ⇔ `RevokedAt IS NULL AND ExpiresAt > NOW()`; **`ReplacedByTokenHash = SHA256(newToken)`**; **256-bit**.
- **Tại sao chọn:** Hai cột cùng biểu diễn một trạng thái (`IsRevoked` + `RevokedAt`) là **nguồn bug kinh điển** — chỉ cần một đường code quên cập nhật một trong hai là hệ thống có token *"đã thu hồi nhưng vẫn dùng được"* hoặc ngược lại, và loại bug này cực khó phát hiện vì nó chỉ xuất hiện ở một nhánh code hiếm. Phương án biến `IsRevoked` thành computed property trong C# cũng bị loại vì **EF Core không dịch được property không map sang SQL** — mọi truy vấn lọc theo nó sẽ phải kéo toàn bộ bảng về bộ nhớ. Một nguồn sự thật duy nhất (`RevokedAt`) vừa gọn hơn, vừa mang thêm thông tin *khi nào* bị thu hồi phục vụ audit. Về 256-bit: khớp đúng độ dài output SHA-256 và cột `TokenHash varchar(64)` (64 ký tự hex); 512-bit là thừa vì entropy vượt 256-bit không tăng thực chất sau khi hash, chỉ tốn băng thông mỗi lần refresh.

**Phần 5 – Kết quả Commit Git:**
```
feat(auth): complete FR-AUTH-004 refresh rotation with reuse detection, in-memory access token and forwarded headers
```

## DEV 2 — Nguyên liệu & Các bước thực hiện

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-009` CRUD Nguyên liệu (chiến lược 2 cột Quantity) + `FR-RCP-010` CRUD Các bước nấu (Server tự đánh số + endpoint reorder) + **kích hoạt hai mảng inline `steps?`/`ingredients?` của `POST /recipes`** (phần còn lại của FR-RCP-003). **Kèm retrofit D-16.**

**Phần 2 – Các bước tiến hành chi tiết:**
1. **DB migration `B4_Recipe_IngredientStep` (+ retrofit D-16):** Buổi 2 đã có sẵn `Quantity decimal(10,3) NULL`, `Unit` nullable và `OrderIndex` — **không đổi các cột này**. Chỉ bổ sung: cột **`QuantityText varchar(50) NULL`**; `CHECK ("Quantity" IS NULL OR "Quantity" > 0)`. Riêng ràng buộc thứ tự bước phải **đổi từ unique index sang unique constraint**, vì PostgreSQL **chỉ cho constraint là `DEFERRABLE`, không cho index**:
   ```sql
   DROP INDEX "IDX_RecipeStep_Recipe_StepNumber";
   ALTER TABLE "RecipeSteps" ADD CONSTRAINT "UQ_RecipeStep_Recipe_StepNumber"
       UNIQUE ("RecipeId", "StepNumber") DEFERRABLE INITIALLY DEFERRED;
   ```
   Viết bằng `migrationBuilder.Sql(...)` (DDL — hợp lệ theo CONS-006 đã diễn giải lại) và **bỏ `.IsUnique()`** khỏi `HasIndex` trong `RecipeConfiguration` (giữ index thường cho truy vấn), để model snapshot của EF không cố tạo lại unique index ở migration sau.
2. **Domain — nguyên liệu (+ mã lỗi cho `DomainException`):** `DomainException` hiện chỉ có message, và middleware xử lý mọi `DomainException` như nhau — nên trước hết thêm thuộc tính **`Code`** (chuỗi thuần; Domain vẫn không có NuGet nào — CONS-001) và một bảng ánh xạ `Code → HTTP status` trong `GlobalExceptionMiddleware` (mặc định 400). Sau đó mở rộng `Recipe.AddIngredient(...)` đã có thêm tham số `quantityText`; thêm `UpdateIngredient`, `RemoveIngredient`. Invariant: **không được rỗng cả `Quantity`, `QuantityText` lẫn `Unit`** → `DomainException` với `Code = "INGREDIENT_QUANTITY_REQUIRED"` → **400**.
3. **Domain — các bước:** `Recipe.AddStep(...)` **đã tự gán `StepNumber = Max + 1` từ Buổi 2** — giữ nguyên. Thêm `Recipe.RemoveStep(stepId)` (xóa rồi **renumber 1..N**) và `Recipe.ReorderSteps(IReadOnlyList<Guid> stepIds)` (tập id phải **khớp chính xác** tập step hiện có — thiếu/thừa/trùng → `DomainException` → 400 — rồi gán `StepNumber = index + 1`). **Mọi thao tác renumber nằm trong một `SaveChangesAsync()` duy nhất**; nhờ constraint deferred ở bước 1, trạng thái trung gian (hai bước tạm cùng số) không làm vỡ ràng buộc. Cuối cùng `CreateRecipeCommand` (Buổi 3) nhận thêm `steps?`/`ingredients?` và **chỉ gọi lại** `AddStep`/`AddIngredient` — không có validator thứ hai.
4. **CQRS + Validator:** `Add/Update/DeleteIngredientCommand` (`name` **1–200**, `quantityText` ≤ 50, `unit` ≤ 50, `notes` ≤ 500 — theo §7.9); `Add/Update/DeleteStepCommand` (`title` **1–200 bắt buộc**, `description` ≤ 2000, `timerMinutes` ≥ 0) + **`ReorderStepsCommand { RecipeId, StepIds[] }`**. Tất cả đi qua `RecipeAuthorizationHandler` và implement `ICacheInvalidator` → xóa `recipe:{slug}` + prefix `recipes:list:`.
5. **API + UI:** `POST/PUT/DELETE /api/v1/recipes/{id}/ingredients/{ingId?}`; `POST/PUT/DELETE /api/v1/recipes/{id}/steps/{stepId?}` — **body KHÔNG chứa `stepNumber`**; **`PATCH /api/v1/recipes/{id}/steps/reorder`** nhận `{ stepIds: [...] }`. UI: wizard bước 3 "Nguyên liệu" (`useFieldArray`, mỗi dòng có ô số + ô nguyên văn, gợi ý người dùng nhập một trong hai), bước 4 "Các bước" (**kéo-thả bằng `dnd-kit` → gọi endpoint reorder**, ảnh minh họa bước qua `ImageUploader`, ô timer). Unit test: renumber sau khi xóa bước giữa; reorder với mảng thiếu id → 400.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình chiến lược 2 cột `Quantity` / `QuantityText`:** đây là bài toán kinh điển giữa *dữ liệu tính toán được* và *dữ liệu nguyên văn của người dùng*. Nấu ăn tiếng Việt đầy những định lượng không quy ra số được: "1/2 muỗng", "nửa củ", "một nhúm", "vừa đủ", "1–2 quả". Ba phương án từng được cân nhắc:
- **Chỉ `decimal`** — tính toán được (scale khẩu phần ×2, cộng dồn nguyên liệu, map sang `recipeIngredient` cho JSON-LD) nhưng **mất hoàn toàn nguyên văn**: FE phải tự quy đổi "1/2" → 0.5, còn "vừa đủ" thì không có cách nào lưu.
- **Chỉ `varchar`** — nhập tự do đúng thói quen người Việt, nhưng **giết chết mọi khả năng tính toán**: không scale khẩu phần được, không validate được, dữ liệu bẩn, và `recipeIngredient` trong JSON-LD (NFR-SEO-001) trở thành chuỗi vô nghĩa với Google.
- **Hai cột song song (đã chọn)** — giữ được **cả hai**. Quy tắc rõ ràng: hiển thị thì ưu tiên `QuantityText`, không có thì format từ `Quantity` + `Unit`; scale khẩu phần chỉ áp dụng cho nguyên liệu có `Quantity`, nguyên liệu chỉ có `QuantityText` giữ nguyên văn. Chi phí là thêm một cột `varchar(50)` — rẻ hơn rất nhiều so với việc phải viết parser phân số rồi vẫn không xử lý được "vừa đủ".

**Giải trình Server tự gán `StepNumber`:** Client **không bao giờ** gửi `stepNumber`, kể cả ở POST lẫn PUT. Lý do là **toàn vẹn dữ liệu**: ràng buộc `UNIQUE (RecipeId, StepNumber)` sẽ bị vi phạm ngay khi hai request thêm bước chạy gần như đồng thời, hoặc khi người dùng nhập trùng số — và kết quả là `DbUpdateException` → **HTTP 500** thay vì một lỗi nghiệp vụ rõ ràng. Giao quyền đánh số cho Server cũng xóa bỏ gánh nặng *"tôi đang ở bước mấy rồi"* khỏi phía người dùng. Muốn đổi thứ tự thì dùng endpoint `reorder` — Server nhận **ý định** ("thứ tự mới là mảng id này") chứ không nhận **giá trị cột**, rồi tự renumber. Đây là khác biệt thiết kế quan trọng: API phơi bày *hành động nghiệp vụ*, không phơi bày *chi tiết lưu trữ*.

**Ràng buộc kỹ thuật bắt buộc khi renumber:** mọi thao tác gán lại `StepNumber` phải nằm trong **một** transaction. Nếu cập nhật từng bước một, sẽ có trạng thái trung gian mà hai bước cùng mang số 2 trong chốc lát → vỡ UNIQUE giữa chừng. Khai báo constraint `DEFERRABLE INITIALLY DEFERRED` để PostgreSQL dời việc kiểm tra tới cuối transaction là cách xử lý sạch nhất, thay vì phải nghĩ ra thuật toán gán số tạm âm rồi gán lại.

**Vì sao D-16 là điều kiện tiên quyết, không phải tối ưu:** Buổi 2 khai báo ràng buộc bằng `HasIndex(...).IsUnique()` — EF Core sinh ra một **unique index**. PostgreSQL kiểm tra unique index **ngay sau từng câu `UPDATE`** và không có cách nào hoãn lại; từ khóa `DEFERRABLE` chỉ tồn tại cho **constraint**. Nếu bỏ qua chi tiết này, thao tác kéo-thả đổi chỗ bước 2 và bước 3 sẽ ném `23505 unique_violation` ở câu `UPDATE` đầu tiên → HTTP 500 — tức là tính năng reorder **không bao giờ chạy được**, dù code Domain hoàn toàn đúng. (Trường hợp ảnh primary ở Buổi 3 khác: index đó là **partial** — có `WHERE` — nên không thể đổi thành constraint; ở đó phải dùng hai lần `SaveChanges`.)

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) Chương 7.4 ghi `Quantity decimal(10,3) NULL` — *"nullable cho nguyên liệu vừa đủ"*, nhưng FR-RCP-009 Điều kiện tiên quyết lại đòi **"Quantity > 0, Unit không rỗng"** (bắt buộc), còn Chương 8 thì ghi `quantity?`, `unit?` (cả hai tùy chọn) — **ba chỗ ba kiểu**. (b) FR-RCP-010 quy định Server tự gán `StepNumber` và tự renumber, nhưng Chương 8 lại cho client gửi `stepNumber` trong body POST **và sửa được qua PUT**. (c) Tên trường lệch: `SortOrder` (FR) vs `OrderIndex` (schema); `DurationMinutes` (FR) vs `TimerMinutes` (schema). (d) `RecipeStep.Title` là `NOT NULL` trong schema nhưng FR-RCP-010 **không có `title` trong body POST**.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-04, MT-03, MT-20.4, MT-20.5, MT-20.7, MT-41.7**) chốt: **hai cột `Quantity` + `QuantityText`, cả hai nullable**, `Unit` tùy chọn, ràng buộc mềm "không rỗng cả ba"; **Server toàn quyền gán `StepNumber`** + bổ sung endpoint `reorder`; thống nhất **`orderIndex`** và **`timerMinutes`**; **bổ sung `title` bắt buộc** vào body POST step.
- **Tại sao chọn:** Điểm (d) là lỗi sẽ làm **mọi request POST step thất bại ở tầng DB** — cột `NOT NULL` mà body không có trường tương ứng thì `SaveChangesAsync` ném ngay; đây là loại lỗi chỉ lộ ra khi chạy thật, không phải lúc biên dịch. Về tên trường: hai cái tên cho cùng một thứ nghĩa là BE và FE sẽ implement hai hợp đồng khác nhau, và request từ FE sẽ bị bind thành `null` **âm thầm** — không lỗi, chỉ sai dữ liệu, loại bug khó phát hiện nhất khi test thủ công. Chốt theo tên trong schema (`orderIndex`, `timerMinutes`) vì schema là thứ khó đổi nhất.

**Phần 5 – Kết quả Commit Git:**
```
feat(recipes): complete FR-RCP-009 dual-column quantity & FR-RCP-010 server-assigned steps with deferrable reorder
```

## DEV 3 — PostgreSQL Full-Text Search tiếng Việt

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-SRCH-001` Tìm kiếm Toàn văn bản (generated column + `simple` + `unaccent`, tiếng Việt không dấu) + UI tìm kiếm. **Kèm retrofit D-18:** bỏ cơ chế FTS thứ hai trong `init.sql`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **DB migration `B4_Search_FTS` — DDL đặc thù PostgreSQL, thứ tự bắt buộc:** trong `Up()`, các lệnh `migrationBuilder.Sql(...)` sau phải đứng **trước** lệnh `AddColumn` do EF sinh ra (hàm phải tồn tại trước khi cột generated tham chiếu tới nó):
   ```sql
   CREATE EXTENSION IF NOT EXISTS unaccent;   -- idempotent: init.sql đã tạo trong Docker,
   CREATE EXTENSION IF NOT EXISTS pg_trgm;    -- nhưng Testcontainers/môi trường khác KHÔNG chạy init.sql

   -- unaccent() mặc định KHÔNG immutable → không dùng trực tiếp trong generated column
   CREATE OR REPLACE FUNCTION unaccent_immutable(text)
   RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
   LANGUAGE sql IMMUTABLE STRICT;
   ```
   Khai báo cột qua EF Core: `.HasComputedColumnSql("to_tsvector('simple', unaccent_immutable(coalesce(\"Title\",'') || ' ' || coalesce(\"Description\",'')))", stored: true)` → **generated column `STORED`, không dùng trigger**; tạo **GIN index** `IDX_Recipe_Search`. `Down()` xóa theo thứ tự ngược lại. **Retrofit D-18:** xóa khối tạo `vietnamese_unaccent` khỏi `docker/postgres/init.sql` (chỉ giữ hai dòng `CREATE EXTENSION`) để hệ thống chỉ có **một** cơ chế FTS.
2. **CQRS — `SearchRecipesQuery : ICacheable`:** validate `q` ≥ 2 ký tự → sai thì **400 `VALIDATION_ERROR`**; sanitize term (loại ký tự đặc biệt của tsquery: `& | ! ( ) : *`), build chuỗi prefix `pho:* & bo:*`; `.Where(r => r.SearchVector.Matches(EF.Functions.ToTsQuery("simple", query)))`; `ORDER BY ts_rank(...) DESC`; **chỉ `Status == Published`**; trả `relevanceScore` trong DTO. `CacheKey = search:{queryHash}`, `Expiration = 1 phút`.
3. **API:** `GET /api/v1/recipes/search?q&page&pageSize&categoryId&difficulty`. ASP.NET Core endpoint routing **ưu tiên segment literal hơn tham số**, nên `/recipes/search` luôn thắng `/recipes/{slug}` bất kể thứ tự khai báo — hệ quả ngược lại mới là điều cần xử lý: một công thức có slug `search` sẽ **không bao giờ truy cập được**, vì vậy `search` nằm trong danh sách slug dành riêng mà `CreateRecipeCommand` (Buổi 3) đã chặn.
4. **Backfill + kiểm chứng:** generated column tự tính cho toàn bộ 100 recipe seed (CR-2026-03) ngay khi migration chạy (không cần script backfill riêng — đây là lợi thế của generated column so với trigger). Integration test chạy trên harness Testcontainers (Buổi 3) — chính là phép thử cho việc DDL đã nằm đủ trong migration. Kiểm chứng SQL trực tiếp: `SELECT "Title" FROM "Recipes" WHERE "SearchVector" @@ to_tsquery('simple','pho:*')` phải trả về "Phở bò".
5. **UI:** `SearchBar.tsx` trong header (debounce 300ms, Enter → điều hướng `/search?q=`), trang `/search` (SSR) hiển thị kết quả + highlight từ khóa + empty state kèm gợi ý. Integration test: gõ **"pho"** (không dấu) phải tìm được **"Phở bò"**; gõ "bánh" tìm được "Bánh mì".

**Phần 3 – Định hướng & Lý do thiết kế:**
**Generated column `STORED` thay vì trigger** là quyết định về tính đúng đắn chứ không phải sở thích. Với generated column, PostgreSQL **tự bảo đảm** `SearchVector` luôn khớp với `Title`/`Description` — không tồn tại đường code nào có thể cập nhật tiêu đề mà quên cập nhật vector. Với trigger, việc đồng bộ phụ thuộc vào trigger được cài đúng và không bị vô hiệu hóa; và nếu ai đó thêm một đường ghi dữ liệu mới (seeding, import, migration vá) thì phải nhớ trigger tồn tại. Thêm nữa, generated column khai báo được **trọn vẹn trong `OnModelCreating`** nên không vi phạm CONS-006.

**Chọn configuration `simple` + `unaccent` thay vì dictionary tiếng Việt tự build:** PostgreSQL 16 **không có sẵn** configuration tên `vietnamese`. Tự build một cái đòi hỏi image PostgreSQL riêng, tăng độ phức tạp Docker và tạo rủi ro môi trường lúc demo. Trong khi đó `simple` + `unaccent` chạy ngay trên image gốc và đạt đúng yêu cầu đã nêu: bỏ dấu để "pho" khớp "phở". Cái mất là stemming (không tự hiểu "nấu"/"nấu nướng" cùng gốc) — nhưng **tiếng Việt vốn không có biến cách**, nên stemming gần như không mang lại giá trị gì, khác hẳn với tiếng Anh.

Phương án thay thế **bỏ FTS, dùng `pg_trgm` + `ILIKE`** cũng bị loại: nó đơn giản hơn thật, nhưng mất `ts_rank` — mà FR-SRCH-001 lại **bắt buộc** trả `relevanceScore`. Không có xếp hạng độ liên quan thì kết quả tìm kiếm trả về theo thứ tự tùy ý, trải nghiệm rất kém khi có nhiều kết quả.

**Vì sao gỡ `vietnamese_unaccent` khỏi `init.sql` (D-18) dù nó "đang không hại gì":** một text search config tự tạo (copy `simple` + ánh xạ `unaccent`) là cách làm hợp lệ — nhưng Buổi 2 tạo nó trong `init.sql`, file **chỉ chạy một lần khi volume Postgres được tạo lần đầu**, và **không chạy** trong Testcontainers hay bất kỳ môi trường nào không dùng đúng file compose này. Nếu code tìm kiếm tham chiếu tới nó, integration test và mọi môi trường mới sẽ lỗi `text search configuration does not exist` — **chính xác loại lỗi của `"vietnamese"` trong SRS v1.0.0 mà MT-25 vừa sửa**. Để nó nằm đó mà không dùng thì là hai cơ chế song song cho cùng một việc, sớm muộn có người dùng nhầm. Nguyên tắc: **mọi đối tượng schema mà code phụ thuộc phải sinh ra từ migration**.

**Cache 1 phút cho kết quả tìm kiếm, không invalidate chủ động:** query string tìm kiếm rất đa dạng nên hit rate vốn thấp, và việc cố gắng invalidate mọi khóa `search:*` khi có bất kỳ recipe nào thay đổi sẽ tốn kém hơn lợi ích thu được. TTL ngắn 1 phút là đủ để hấp thụ các lượt gõ lặp lại (người dùng bấm back/forward, tải lại trang) mà không giữ dữ liệu cũ quá lâu.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** **ba lỗi chồng nhau ở cùng một tính năng**. (a) FR-SRCH-001 ghi *"Trường SearchVector (**computed column**) được tự động cập nhật bởi PostgreSQL **trigger**"* — hai cơ chế loại trừ nhau trong **cùng một câu**. (b) Luồng chính bước 4 dùng `EF.Functions.ToTsQuery("**vietnamese**", query)` trong khi PostgreSQL 16 **không có** configuration tên đó, và mục 2.4.1 chỉ cài `unaccent` + `pg_trgm`. (c) Dùng trigger thì phải viết raw SQL trong migration, trong khi CONS-006 ghi *"Migrations qua EF Core Code-First. Không viết raw SQL trực tiếp"*.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-25**) chốt **generated column `STORED`** + configuration **`simple`** + hàm wrapper **`unaccent_immutable`**, đồng thời **diễn giải lại CONS-006**: ràng buộc nhắm vào *truy vấn dữ liệu*, còn DDL đặc thù PostgreSQL (extension, generated column, hàm `IMMUTABLE`, index GIN) **được phép** khai báo trong migration.
- **Tại sao chọn:** Lỗi (b) là loại nguy hiểm nhất — nó **không sai lúc biên dịch**, chỉ ném `text search configuration "vietnamese" does not exist` **khi chạy thật**, nghĩa là tính năng tìm kiếm chết hẳn và chỉ phát hiện được khi có người thử tìm kiếm. Về (c): nếu diễn giải CONS-006 theo nghĩa đen (cấm mọi raw SQL) thì **ngay cả `CREATE EXTENSION unaccent` cũng bị chặn** — tức là không thể hiện thực được chính yêu cầu tìm kiếm không dấu mà SRS đặt ra. Diễn giải lại theo ý định thật của ràng buộc (chống SQL injection qua truy vấn dữ liệu người dùng) vừa giữ được tinh thần bảo mật, vừa không tự trói tay ở tầng schema. Ràng buộc kèm theo của generated column là biểu thức phải `IMMUTABLE`, mà `unaccent()` mặc định thì không — nên hàm wrapper `unaccent_immutable` là **bắt buộc**, không phải tùy chọn.

**Phần 5 – Kết quả Commit Git:**
```
feat(search): complete FR-SRCH-001 fts with stored generated column simple config and unaccent
```

## DEV 4 — Image Resize Job & Permanent Purge Job

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-JOB-002` Image Resize / Thumbnail Job + `FR-JOB-003` **Permanent Purge Job** (dọn dữ liệu soft-deleted vĩnh viễn).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **FR-JOB-002 — `ImageResizeJob(Guid imageId)`:** dùng **SixLabors.ImageSharp** tạo medium **800×600** và thumbnail **300×300** (giữ tỉ lệ, crop giữa), upload cả hai lên MinIO cùng folder với ảnh gốc, cập nhật `MediumUrl`/`ThumbnailUrl` vào DB. `[AutomaticRetry(Attempts = 3)]`; fail hết retry → giữ nguyên `null`, **ảnh gốc vẫn hiển thị bình thường**. Móc vào `UploadRecipeImageCommand` của Dev 2 (Buổi 3) bằng `BackgroundJob.Enqueue`.
2. **FR-JOB-003 — `PermanentPurgeJob`:** recurring **`"30 3 * * *"` (03:30 UTC)**. Thân job:
   - Query `_db.Recipes.IgnoreQueryFilters().Where(r => r.IsDeleted && r.UpdatedAt < DateTime.UtcNow.AddDays(-30))` — **bắt buộc `IgnoreQueryFilters()`** vì Global Query Filter đã ẩn chính những bản ghi ta cần tìm.
   - Với mỗi recipe: **(1)** thu thập toàn bộ URL ảnh (`RecipeImages`: original/medium/thumbnail, và `RecipeStep.ImageUrl`) vào danh sách **trước khi xóa**; **(2)** hard-delete recipe trong một transaction — FK `ON DELETE CASCADE` tự dọn Steps/Ingredients/Images; **(3)** **chỉ sau khi transaction commit thành công** mới gọi `IFileStorageService.DeleteAsync()` cho từng URL.
3. **Distributed lock:** job chạy trong container **`hangfire` (worker)** — container `api` đặt `Hangfire__ServerEnabled=false` nên không bao giờ nhận job. Hangfire đã bảo đảm một job instance chỉ được một server lấy, nhưng recurring job có thể **bị kích hoạt trùng** khi lượt trước chạy quá lâu hoặc khi scale nhiều worker; vì vậy bọc thân job bằng RedLock (`RedLock.net`) khóa `lock:purge-job`, TTL 10 phút, kèm `[DisableConcurrentExecution(600)]`. Không lấy được lock thì bỏ qua lượt chạy và log Information (không phải lỗi).
4. **Đăng ký + retry:** `RecurringJob.AddOrUpdate<PermanentPurgeJob>("permanent-purge", j => j.ExecuteAsync(), "30 3 * * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc })`; `[AutomaticRetry(Attempts = 2)]`; log số recipe đã dọn và số file đã xóa qua Serilog.
5. **Kiểm chứng:** FE hiển thị `ThumbnailUrl` trong `RecipeCard` (fallback `OriginalUrl` khi còn `null`) — xác nhận 3 biến thể ảnh trong MinIO console. Với purge job: seed một recipe có `IsDeleted = true, UpdatedAt = 40 ngày trước`, trigger thủ công trên `/hangfire`, xác nhận bản ghi biến mất khỏi DB **và** file biến mất khỏi MinIO.

**Phần 3 – Định hướng & Lý do thiết kế:**
**Resize bất đồng bộ** thay vì xử lý ngay trong request upload: resize một ảnh 5MB tốn 1–3 giây CPU. Làm đồng bộ thì người dùng phải chờ, và với 5 ảnh là 15 giây — không chấp nhận được. Quan trọng hơn, thiết kế này khiến **resize thất bại không làm hỏng việc upload**: ảnh gốc đã nằm trên MinIO và hiển thị được, thumbnail chỉ là tối ưu hiển thị. Đây là nguyên tắc *"tách việc bắt buộc khỏi việc nên có"*.

**Thứ tự "commit DB trước, xóa file sau" trong purge job là bắt buộc, không phải tùy chọn.** Nếu xóa file trước rồi transaction DB bị rollback, ta sẽ có bản ghi recipe trỏ tới file không còn tồn tại — hỏng vĩnh viễn và không khôi phục được. Làm ngược lại thì trường hợp xấu nhất chỉ là **file mồ côi** trên MinIO — tốn dung lượng nhưng vô hại, và lượt chạy sau có thể dọn. Nguyên tắc chung: khi phải phối hợp hai hệ thống không có transaction chung, luôn sắp xếp sao cho lỗi rơi vào phía **rác thừa** chứ không phải phía **mất dữ liệu**.

**Vì sao vẫn cần distributed lock dù Hangfire đã điều phối job:** Hangfire bảo đảm mỗi *job instance* chỉ được một server lấy, nhưng không bảo đảm hai *lượt* của cùng một recurring job không chồng lên nhau — nếu lượt 03:30 hôm nay còn đang xóa hàng nghìn bản ghi mà lượt được kích hoạt lại (retry, trigger tay trên dashboard, hoặc worker thứ hai khi scale), hai lượt sẽ cùng query ra một tập recipe → cùng cố xóa → lỗi và log đầy exception. Khóa phân tán qua Redis là cách rẻ nhất để đảm bảo "đúng một lượt chạy trên toàn cluster" mà không cần bầu leader.

**Job nặng chạy ở worker, không ở `api`:** cả resize ảnh (CPU) lẫn purge (I/O DB + MinIO hàng loạt) đều chạy trong container `hangfire`, nên không tranh tài nguyên với request của người đọc — đây là lợi ích cụ thể của topology worker riêng mà SRS v1.2.0 đã chuẩn hóa (§3.6, **MT-47**).

**Thời gian lưu giữ 30 ngày** là đánh đổi giữa khả năng khôi phục và dung lượng: đủ dài để người dùng nhận ra mình xóa nhầm và báo Admin, đủ ngắn để MinIO không phình mãi. Đây cũng là con số đã ghi trong NFR-REL-003.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** v1.0.0 giao FR-JOB-003 làm **Sitemap Generation Job** — *"tạo `sitemap.xml`, upload lên MinIO hoặc lưu vào `wwwroot`, gửi ping đến Google Search Console"*. Đặc tả này không nghiệm thu được vì hai lý do độc lập: **(1) Sai vị trí phục vụ** — crawler tìm sitemap tại `https://domain.com/sitemap.xml`, domain do **Next.js** phục vụ, trong khi file lại nằm trên MinIO (`minio:9000`) hoặc trong `wwwroot` của container API (`api.culinaryblog.com`), không khớp `robots.txt` của site chính; tệ hơn, `wwwroot` nằm **trong container** nên mỗi lần deploy là mất file, và với nhiều instance thì mỗi instance giữ một bản khác nhau. **(2) Ping đã bị khai tử** — Google ngừng hỗ trợ endpoint `https://www.google.com/ping?sitemap=` từ **tháng 6/2023**, gọi vào chỉ nhận 404.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-28 + MT-05**) chuyển sitemap sang **Next.js `app/sitemap.ts` + `app/robots.ts`** (Dev 3 làm ở Buổi 7), và dùng khe FR-JOB-003 cho **Permanent Purge Job**. *(Đây là điểm xung đột **X-2** với lộ trình đề bài — xem §3.)*
- **Tại sao chọn:** Quyết định Soft Delete toàn cục (MT-05) tạo ra một nhu cầu **bắt buộc phải có ai đó gánh**: nếu không có chỗ dọn dữ liệu và file vĩnh viễn thì MinIO phình mãi mãi và slug của recipe đã xóa chiếm chỗ không bao giờ được giải phóng — đúng loại nợ kỹ thuật tích tụ âm thầm rồi bùng phát khi hệ thống đã chạy lâu. Trong khi đó sitemap sinh ở Backend là công sức bỏ ra cho một thứ **crawler không bao giờ đọc được**. Đổi chỗ hai việc này vừa lấp được lỗ hổng thật, vừa bỏ được công việc vô ích. Ba phương án sitemap từng cân nhắc: **(A) Next.js tự sinh** — đúng vị trí, không cần lưu file, không cần MinIO, Next.js hỗ trợ sẵn, và **bỏ được hẳn một job nền**; **(B) giữ job BE + Nginx proxy `/sitemap.xml` → MinIO** — giữ nguyên FR cũ nhưng thêm rule Nginx, sitemap "đông cứng" tối đa 24h, vẫn cần dọn file cũ; **(C) job BE ghi vào volume dùng chung** — vướng ngay khi scale nhiều instance. Chọn **A**.

**Phần 5 – Kết quả Commit Git:**
```
feat(jobs): complete FR-JOB-002 async image resize & FR-JOB-003 permanent purge job with distributed lock
```

---

# BUỔI 5 – HỒ SƠ CÁ NHÂN, CONCURRENCY & PUBLISH, BỘ LỌC NÂNG CAO & HEALTH CHECKS

> **Mục tiêu buổi:** người dùng quản lý được hồ sơ, công thức đi hết vòng đời tới Published an toàn dưới tải đồng thời, bộ lọc/sắp xếp/phân trang hoàn chỉnh, và hệ thống tự báo cáo được sức khỏe.

## DEV 1 — Xem & Cập nhật Hồ sơ Cá nhân

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-AUTH-006` Xem Hồ sơ Cá nhân + `FR-AUTH-007` Cập nhật Hồ sơ & Avatar. *(Hợp đồng `displayName` đã chuẩn hóa ở Buổi 3 — retrofit D-1 — nên buổi này không có migration nào.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Query — `GetCurrentUserQuery`:** dùng `ICurrentUser` **đã có từ Buổi 2** (`API/Services/CurrentUser.cs`) lấy `UserId`; trả `UserDto { id, email, displayName, avatarUrl, bio, roles }` — đúng hợp đồng đã chốt ở Buổi 3. **Không bao gồm** `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `UserName` và **`emailConfirmed`** (hệ thống không có luồng xác nhận email). Token hợp lệ nhưng user không còn tồn tại → **401** (không phải 404).
2. **CQRS — `UpdateProfileCommand`:** PATCH partial `{ displayName?, avatarUrl?, bio? }`; Validator: `displayName` **2–100**, `bio` ≤ **1000**, `avatarUrl` là URL hợp lệ ≤ 500 ký tự **và phải trỏ về đúng bucket MinIO của hệ thống** (`MinioOptions.PublicBaseUrl`) — không nhận URL ảnh ngoài để tránh biến hệ thống thành nơi nhúng nội dung tùy ý. **`Email` và `UserName` không cho đổi** ở endpoint này.
3. **API:** `GET /api/v1/auth/me` (200, `RequireAuthorization()`), `PATCH /api/v1/auth/me` (200 trả `UserDto` đã cập nhật kèm `roles`).
4. **UI — `/profile` (CSR):** form RHF + Zod; avatar dùng lại `ImageUploader` (Buổi 2) upload lên **`POST /api/v1/files/upload`** (endpoint có từ Buổi 2, nay là **FR-FILE-001** trong SRS v1.2.0 Chương 8.8 — MT-43) → nhận URL → PATCH `/auth/me`; cập nhật avatar ở header ngay bằng optimistic update của TanStack Query + toast; rollback khi API lỗi. Avatar cũ được xóa qua `DELETE /api/v1/files/{**fileId}` **sau khi** PATCH thành công (không xóa trước — nếu PATCH lỗi, người dùng mất cả ảnh cũ).
5. **Test:** integration test `GET /auth/me` không có trường nhạy cảm nào trong JSON; PATCH với `avatarUrl` trỏ ra domain ngoài → 400; token của user đã bị xóa khỏi DB → 401.

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
feat(auth): complete FR-AUTH-006 & FR-AUTH-007 profile management with validated avatar upload
```

## DEV 2 — Cập nhật công thức (Concurrency) & Publish/Unpublish

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-004` Cập nhật Công thức (Optimistic Concurrency `RowVersion`) + `FR-RCP-005` Xuất bản / Hủy Xuất bản. **Kèm retrofit D-15:** sửa `Recipe.Publish()` có sẵn cho đúng máy trạng thái.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Chuẩn bị — mã lỗi Domain:** bổ sung hai mã mới vào bảng ánh xạ `DomainException.Code → HTTP status` đã dựng ở Buổi 4: `RECIPE_PUBLISH_INCOMPLETE` → **400**, `RECIPE_INVALID_STATE_TRANSITION` → **409**. Không cần migration ở buổi này (`Instructions` đã sang `NULL` ở Buổi 3).
2. **Concurrency — hợp đồng API:** `RecipeDetailDto` trả thêm **`rowVersion`** (base64) và endpoint gắn header `ETag`. `UpdateRecipeCommand` nhận `rowVersion` từ body (hoặc header `If-Match`), gán vào `_db.Entry(recipe).Property(r => r.RowVersion).OriginalValue` trước khi `SaveChangesAsync()`; bắt `DbUpdateConcurrencyException` → **409 `RECIPE_CONCURRENCY_CONFLICT`**.
3. **CQRS Update:** đi qua `RecipeAuthorizationHandler` (403 `RECIPE_FORBIDDEN`); **quy tắc slug:** chỉ sinh lại slug khi `recipe.Status == RecipeStatus.Draft`; recipe đã từng Published thì **slug khóa vĩnh viễn** dù đổi Title. Invalidate `recipe:{slug}` + prefix `recipes:list:`.
4. **Domain — retrofit D-15 trên `Recipe.Publish()` có sẵn:** Buổi 2 đã viết đúng hai điểm — kiểm tra `_steps.Count == 0 || _ingredients.Count == 0` và `PublishedAt ??= utcNow` (chỉ gán ở lần đầu) — **giữ nguyên**. Sửa hai điểm sai: **(a)** thay nhánh `if (Status == Published) return;` (no-op) bằng ràng buộc **chỉ `Draft` mới publish được** — mọi trạng thái khác (kể cả **Archived**, vốn hiện đang lọt qua) ném `DomainException("RECIPE_INVALID_STATE_TRANSITION")` → **409**; **(b)** thêm mã `RECIPE_PUBLISH_INCOMPLETE` vào exception thiếu step/ingredient → **400**. Thêm `Unpublish()` (`Published → Draft`, sai trạng thái → 409). ⚠️ Handler phải `Include(Steps)` và `Include(Ingredients)` trước khi gọi `Publish()` — nếu không, hai collection rỗng và recipe đủ điều kiện cũng bị từ chối.
5. **API + UI:** `PUT /api/v1/recipes/{id:guid}`, `PATCH /api/v1/recipes/{id}/publish`, `PATCH /api/v1/recipes/{id}/unpublish`. UI `/dashboard/recipes/[id]/edit` tái dùng wizard, gửi kèm `rowVersion`; gặp 409 → hiện dialog *"Dữ liệu đã được người khác cập nhật — Tải lại / Xem khác biệt"*; nút Publish có **checklist điều kiện** (✓ đã có N bước, ✗ chưa có nguyên liệu) để người dùng biết còn thiếu gì **trước khi** bấm.

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
feat(recipes): complete FR-RCP-004 rowversion concurrency & FR-RCP-005 publish with state-guarded transitions
```

## DEV 3 — Bộ lọc, Sắp xếp & Phân trang

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-SRCH-002` Lọc đa tiêu chí + `FR-SRCH-003` Sắp xếp (`sortBy`/`sortOrder`) + `FR-SRCH-004` Phân trang offset — cả Backend lẫn UI. **Kèm retrofit D-10:** thay cơ chế `sort=-field` của Buổi 2.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Application — `RecipeFilterSpec` dùng chung:** một lớp specification duy nhất áp dụng cho `GetRecipesQuery`, `SearchRecipesQuery` và `GetCategoryBySlugQuery`, gồm `categoryId?`, `difficulty?`, `maxCookTime?`, `maxPrepTime?`, `minServings?` — kết hợp bằng **AND logic**. `difficulty` phải thuộc enum **4 giá trị** `{Easy, Medium, Hard, Expert}` → ngoài enum thì **400**.
2. **Application — `SortMapper` (whitelist dictionary):**
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
   `sortBy` không có trong dictionary → **400 `VALIDATION_ERROR`** (không im lặng bỏ qua); `sortOrder ∈ {asc, desc}`; mặc định **`sortBy=createdAt&sortOrder=desc`**. **Tuyệt đối không ghép chuỗi SQL từ `sortBy`.** **Retrofit D-10:** xóa `RecipeSortParser` và enum `RecipeSortField` (thiếu `PrepTime`); endpoint `GET /recipes` đổi tham số `string? sort` thành `string? sortBy, string? sortOrder` và nhận thêm `maxPrepTime`, `minServings`.
3. **Pagination:** `page` mặc định 1 (≥ 1), **`pageSize` mặc định 12**, max 50 → sai thì 400; `PagedResult<T>` trả đủ `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage` (hình dạng đã chốt trong SRS v1.2.0 §5.2 — MT-42).
4. **Index + kiểm chứng hiệu năng:** migration **`B5_Search_CompositeIndexes`** tạo 3 composite index của SRS §7.2 — `(IsDeleted, Status, PublishedAt DESC)`, `(Status, CategoryId, PublishedAt DESC)`, `(Status, CookTime)` — vì Buổi 2 **chỉ có index đơn cột** và chưa buổi nào tạo chúng. Chạy `EXPLAIN ANALYZE` trên **bộ dữ liệu ≥ 10.000 recipe** (seeder riêng `PerformanceSeeder`, chỉ bật bằng cờ cấu hình): với 100 bản ghi seed (CR-2026-03), PostgreSQL **luôn chọn Seq Scan** vì quét cả bảng rẻ hơn dùng index, nên kết quả đo trên dữ liệu nhỏ không chứng minh được gì. Nếu sort mặc định `createdAt` vẫn cho Seq Scan + Sort, bổ sung `(Status, CreatedAt DESC)` — đúng tinh thần NFR-PERF-004 *"chứng minh bằng đo đạc, không tạo index theo cảm tính"*. Ghi kết quả vào `docs/explain-analyze-b5.md`.
5. **UI — đồng bộ state với URL:** `FilterPanel` (danh mục, độ khó, thời gian nấu, thời gian chuẩn bị, khẩu phần), `SortSelect` (dropdown "Cột" + "Chiều" ánh xạ **1-1** với `sortBy`/`sortOrder`), `Pagination`. **Toàn bộ state nằm trong `searchParams`** — link chia sẻ được, back/forward hoạt động đúng, SSR đọc được ngay. **Retrofit D-10 phía FE:** `app/recipes/(list)/page.tsx` bỏ `query.set('sort', s)` và mặc định `'-createdAt'`, chuyển sang cặp `sortBy`/`sortOrder`. Dùng chung trên `/recipes`, `/search`, `/categories/[slug]`; mobile hiển thị filter dạng drawer.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình chuẩn hóa Sort — vì sao `sortBy` + `sortOrder` thắng `sort=-field`:** Hai tham số độc lập cho phép viết **hai rule FluentValidation tách biệt** (`sortBy ∈ whitelist`, `sortOrder ∈ {asc,desc}`), dễ đọc và dễ kiểm chứng hơn hẳn việc tự parse tiền tố `-`. Quan trọng hơn về mặt vận hành: dấu `-` **rất dễ bị nuốt mất khi quên URL-encode**, và khi đó `sort=-createdAt` biến thành `sort= createdAt` → hệ thống sắp xếp tăng dần thay vì giảm dần, **không có lỗi nào được ném ra** — chỉ sai thứ tự âm thầm, loại bug khó phát hiện nhất khi test thủ công. Về phía Frontend, cặp `sortBy`/`sortOrder` ánh xạ 1-1 với cặp dropdown "cột"/"chiều" nên không cần lớp chuyển đổi nào. Nhược điểm duy nhất là muốn sắp xếp nhiều cột phải dùng mảng — nhu cầu không tồn tại trong phạm vi hệ thống này.

**Whitelist là rào chắn bảo mật, không chỉ là validation:** `sortBy` là tên cột, và tên cột **không tham số hóa được** trong SQL. Nếu ghép chuỗi trực tiếp (`ORDER BY {sortBy}`) thì đây là lỗ hổng SQL injection kinh điển. Ánh xạ qua dictionary ở tầng Application đảm bảo giá trị đi vào truy vấn **chỉ có thể là một trong 5 biểu thức đã viết sẵn** — kể cả khi validator bị bỏ sót ở đâu đó.

**Trả 400 cho giá trị ngoài whitelist thay vì im lặng dùng giá trị mặc định:** im lặng có vẻ "thân thiện" hơn nhưng thực ra tệ hơn — FE gửi sai tên trường sẽ không bao giờ biết mình sai, và bug tồn tại cho tới khi có người tinh mắt phát hiện thứ tự hiển thị không đúng.

**`RecipeFilterSpec` dùng chung cho cả ba query** tránh việc ba nơi tự implement ba bộ lọc hơi khác nhau — tình huống rất dễ xảy ra khi ba màn hình được làm ở ba thời điểm. Cần phối hợp với Dev 2 trước khi code vì `GetRecipesQuery` là của Dev 2 (xem §Phụ thuộc liên dev).

**`pageSize = 12`** chia hết cho lưới 2, 3 và 4 cột — mọi breakpoint responsive đều cho hàng cuối đầy đủ, không có ô trống lẻ loi.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) FR-RCP-001 và FR-SRCH-003 dùng **một tham số** `sort=title` / `sort=-title`, trong khi Chương 8 (Quy ước Pagination, `/recipes`, `/categories/{slug}`) dùng **hai tham số** `sortBy=createdAt&sortOrder=desc`. (b) `pageSize` mặc định là **12** ở FR-SRCH-004 nhưng **10** ở Chương 8. (c) Bộ lọc lệch: FR-SRCH-002 có `maxCookTime`/`minServings` còn Chương 8 có `minPrepTime`/`maxPrepTime`. (d) `difficulty` filter chỉ nhận `{Easy|Medium|Hard}` trong khi enum có **4** giá trị (thiếu `Expert`). (e) NFR-PERF-004 đòi *"mọi cột WHERE/ORDER BY đều có B-tree index"* nhưng schema **thiếu index cho đúng những cột đó** (`CreatedAt`, `CookTime`, `PrepTime`, `Servings`).
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-01, MT-20.13, MT-20.14, MT-20.15, MT-26**) chốt **`sortBy` + `sortOrder`** với whitelist 5 trường, **`pageSize` = 12**, bộ lọc đầy đủ 5 tham số, bổ sung **`Expert`**, và thay yêu cầu index bằng tiêu chí **đo được qua `EXPLAIN ANALYZE`** với 3 composite index.
- **Tại sao chọn:** Điểm (e) đáng chú ý về tư duy dài hạn. Yêu cầu cũ *"mọi cột WHERE/ORDER BY đều có B-tree index"* nghe hợp lý nhưng dẫn tới việc tạo một loạt index đơn cột — làm **chậm mọi lệnh ghi** và tốn dung lượng, trong khi PostgreSQL thường chỉ chọn **một** index cho một truy vấn nên phần lớn số index đó nằm không. Ba composite index của v1.1.0 được chọn theo **hình dạng truy vấn thật** (mọi truy vấn danh sách đều lọc `Status = Published` trước rồi mới sort), nên phủ đúng nhu cầu với số index ít hơn và hiệu quả cao hơn. Tiêu chí nghiệm thu cũng đổi từ *"có index hay không"* (dễ tick nhưng không chứng minh được gì) sang *"`EXPLAIN ANALYZE` phải cho Index Scan, không chấp nhận Seq Scan trên `Recipes`"* — vừa đo được, vừa không ép tạo index thừa.

**Phần 5 – Kết quả Commit Git:**
```
feat(search): complete FR-SRCH-002/003/004 sortby-sortorder whitelist, composite indexes and url-synced pagination
```

## DEV 4 — Health Checks

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-OBS-001` Ba Health Check Endpoints (`/health`, `/health/live`, `/health/ready`) + Health UI Indicator.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Checks:** cài `AspNetCore.HealthChecks.NpgSql`, `.Redis`; viết `MinioHealthCheck` tùy chỉnh (gọi `ListBucketsAsync` với timeout 3s). Gắn **tag `"ready"`** cho PostgreSQL và Redis (hai dependency mà thiếu là API không phục vụ được); MinIO **không** gắn tag `ready` — mất MinIO thì ảnh không hiển thị nhưng API vẫn đọc/ghi công thức được.
2. **Endpoints:** `/health` → tất cả checks, JSON `{ status, entries }` qua `UIResponseWriter.WriteHealthCheckUIResponse`, **503 khi Unhealthy**; `/health/live` → `Predicate = _ => false` (không chạy check nào, **luôn 200** trừ khi process chết); `/health/ready` → `Predicate = c => c.Tags.Contains("ready")`.
3. **Docker healthcheck — bổ sung phần còn thiếu (đúng bảng healthcheck của SRS v1.2.0 §6.5 — MT-51; bảng của v1.1.0 dùng `curl` không chạy được):** `postgres` (`pg_isready -h 127.0.0.1`), `redis` (`redis-cli ping`), `minio` (`mc ready local`) **đã có healthcheck từ Buổi 2** — giữ nguyên các lệnh đã kiểm chứng. Bổ sung cho **`api`**: `CMD curl -fsS http://127.0.0.1:8080/health/ready` (`interval: 10s`, `timeout: 3s`, `retries: 3`, `start_period: 30s`) — ⚠️ image `aspnet:10.0` **không có `curl`**, phải thêm `apt-get install -y --no-install-recommends curl` ở stage runtime của `backend/Dockerfile`. Bổ sung cho **`frontend`**: `CMD wget -qO- http://127.0.0.1:3000 >/dev/null` (`node:22-alpine` chỉ có `wget` của busybox). `nginx` đổi `depends_on` sang `api: { condition: service_healthy }`. Trong `nginx.conf`, thêm `proxy_next_upstream error timeout http_502 http_503 http_504;` vào `location /api/` — cơ chế này **vẫn hoạt động với resolver động của Buổi 2**, vì khi tên `api` phân giải ra nhiều địa chỉ, Nginx tự luân phiên và chuyển sang địa chỉ kế tiếp khi một instance lỗi.
4. **UI:** `app/api/health/route.ts` proxy sang API; component `HealthIndicator.tsx` (chấm xanh/vàng/đỏ, poll 30s) hiển thị trên dashboard Admin; trang `/dashboard/system` liệt kê trạng thái từng dependency kèm thời gian phản hồi.
5. **Kiểm chứng:** `docker compose stop redis` → `/health/ready` trả **503** và container `api` chuyển sang `unhealthy`; `/health/live` **vẫn 200**; `HealthIndicator` chuyển đỏ trong ≤ 30s. Khởi động lại Redis → tự hồi phục.

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
feat(obs): complete FR-OBS-001 health checks with docker healthcheck wiring and ui indicator
```

---

# BUỔI 6 – SECURITY HARDENING, ARCHIVE & SOFT DELETE, ISR/SEO & STRUCTURED LOGGING

> **Mục tiêu buổi:** khóa chặt bề mặt tấn công, hoàn tất vòng đời công thức với cách ly Public/Private, trang công khai đạt chuẩn SEO, và hệ thống quan sát được từ bên ngoài.
>
> ⚠️ **Buổi nặng nhất về retrofit:** 5/17 hạng mục nợ kỹ thuật (D-3 → D-7) được hoàn trả trong buổi này, trong đó **D-4 và D-5 là lỗi bảo mật MT-34**. Để bù lại, `FR-RCP-006` đã được chuyển trọn sang Buổi 7. Dev 2 và Dev 3 cần thống nhất thứ tự merge trước khi bắt đầu (xem §6).

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

## DEV 2 — Soft Delete, Cách ly Public/Private & Dashboard cá nhân

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-007` Xóa Công thức (**Soft Delete**) + **`FR-RCP-011`** Danh sách Công thức Cá nhân (`GET /recipes/mine`).
**Kèm retrofit D-3, D-4, D-6** — trong đó **D-4 là lỗi bảo mật MT-34**. *(`FR-RCP-006` Archive/Unarchive chuyển trọn gói sang Buổi 7 — xem giải trình ở Phần 3.)*

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Thứ tự làm việc trong buổi:** làm **bước 2 trước tiên** (vá lỗ hổng bảo mật), rồi mới tới tính năng mới. DDL cho D-3 dùng ở bước 4:
   ```sql
   DROP INDEX "IDX_Recipe_Slug";
   CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;
   ```
2. **Retrofit D-4 + D-6 — gỡ lọc theo danh tính và gỡ Output Cache (ưu tiên cao nhất buổi này):** trong `RecipeReadRepository`, xóa cơ chế `RecipeVisibility` (Guest/Author/Admin) → **chỉ `Status == Published` cho mọi người gọi**; `GetRecipesQuery`/`GetRecipeBySlugQuery` **không inject `ICurrentUser` nữa**. `GetRecipeBySlugQuery`: Draft/Archived đổi từ **403 `RECIPE_FORBIDDEN` → 404 `RECIPE_NOT_FOUND`** (cùng thông điệp như slug không tồn tại). Xóa `OutputCachePolicies.cs`, `AddCulinaryOutputCache` và hai lời gọi `.CacheOutput(...)` trong `RecipesEndpoints.cs`; chuyển sang `ICacheable` với `recipes:list:{queryHash}` **TTL 2 phút** và `recipe:{slug}` **TTL 5 phút**. Gỡ `Skip` khỏi **test tái hiện MT-34** mà Dev 4 viết sẵn ở Buổi 3 — test đó phải chuyển sang xanh.
3. **`FR-RCP-011` — `GetMyRecipesQuery`:** **không** implement `ICacheable` (để `CachingBehavior` bỏ qua hoàn toàn); lọc `r.AuthorId == targetAuthorId` với `targetAuthorId` = `authorId` khi người gọi là Admin, ngược lại là `ICurrentUser.UserId`; Author truyền `authorId` của người khác → **403 `RECIPE_FORBIDDEN`**; lọc thêm theo `status` nếu có; endpoint gắn header **`Cache-Control: no-store`**. Route `/recipes/mine` là segment literal nên luôn thắng `/recipes/{slug}`; `mine` đã nằm trong danh sách slug dành riêng (Buổi 3).
4. **Soft Delete + retrofit D-3:** migration **`B6_Recipe_PartialUnique`** drop unique index cũ trên `Slug`, tạo lại dạng **partial** (xem khối SQL ở bước 1). `DeleteRecipeCommand` gán `IsDeleted = true` — **KHÔNG** xóa child entity (chúng vô hình cùng recipe qua Global Query Filter) và **KHÔNG** enqueue job xóa file MinIO (việc đó thuộc FR-JOB-003, Buổi 4). Integration test: xóa recipe slug `pho-bo` → tạo recipe mới cùng tên → slug mới lại là `pho-bo` (partial index đã giải phóng slug).
5. **API + UI:** `DELETE /api/v1/recipes/{id}` (204), **`GET /api/v1/recipes/mine`**. UI `/dashboard/recipes` — bảng công thức của tôi gọi **`/recipes/mine`**, tab Draft/Published/Archived (tab Archived sẽ có dữ liệu từ Buổi 7), action Edit/Publish/Unpublish/Delete với confirm dialog (gõ đúng tên recipe mới cho xóa), optimistic update + rollback; `/dashboard` hiện thống kê số công thức theo trạng thái.

**Phần 3 – Định hướng & Lý do thiết kế:**

**Giải trình No-Cache cho dữ liệu riêng tư — đây là trọng tâm của buổi:** nguyên tắc kiến trúc được chốt trong NFR-SEC-006 là *"không bao giờ đặt dữ liệu phụ thuộc danh tính vào cache dùng chung dưới khóa công khai"*. Lý do sâu xa: **cache nằm TRƯỚC handler**. Khi một response đã nằm trong cache, request thứ hai được trả thẳng từ bộ nhớ và **không bao giờ chạy tới handler** — nghĩa là mọi đoạn code kiểm tra quyền trong handler đều bị bỏ qua. Kiểm tra phân quyền hoàn hảo đến đâu cũng vô nghĩa nếu nó không được thực thi.

Từ đó suy ra hai loại endpoint không thể trộn lẫn: **endpoint công khai** trả cùng một response cho mọi người gọi → cache thoải mái, hit rate cao; **endpoint riêng tư** trả response khác nhau tùy người gọi → cấm cache tuyệt đối, gắn `Cache-Control: no-store` để cả proxy trung gian và trình duyệt cũng không lưu.

Phương án thay thế **thêm `userId` vào khóa cache** (`VaryByValue`) bị loại vì hai lý do: hit rate sụp đổ (mỗi user một bản cache, với 5.000 user thì cache phình to mà gần như không bao giờ trúng), và nguy hiểm hơn — nó **để ngỏ khả năng sai sót**: chỉ cần một endpoint nào đó quên khai báo `VaryBy` là lỗ hổng quay lại nguyên vẹn. Tách bạch về mặt kiến trúc thì **không thể quên**, vì hai loại dữ liệu đi qua hai đường khác nhau.

**Soft Delete và ba hệ quả bắt buộc:** (1) **Partial unique index** — nếu giữ unique thường thì slug của recipe đã xóa chiếm chỗ vĩnh viễn, người dùng không bao giờ tạo lại được công thức cùng tên. (2) **Child entity không xóa theo** — chúng vô hình cùng recipe qua Global Query Filter; FK `ON DELETE CASCADE` giữ lại chỉ để phòng khi hard delete thật sự (job purge). (3) **Job xóa file MinIO KHÔNG chạy khi soft delete** — nếu chạy, khôi phục recipe sẽ mất toàn bộ ảnh, tức là "khôi phục được" trở thành lời hứa suông. Việc dọn file thuộc về FR-JOB-003 sau 30 ngày.

**Draft trả 404 thay vì 403 ở endpoint công khai:** 403 nói với người lạ rằng *"có một bản nháp ở đúng slug này, chỉ là bạn không được xem"* — tức là **tiết lộ sự tồn tại** của nội dung chưa công bố (ví dụ tên món sắp ra mắt), và cho phép dò slug hàng loạt. Với endpoint chỉ phục vụ dữ liệu công khai, một bản nháp đơn giản là **không tồn tại** trong không gian đó; chủ sở hữu xem nó qua `/recipes/mine`.

**Vì sao chuyển trọn `FR-RCP-006` sang Buổi 7:** lộ trình ban đầu đặt Archive ở Buổi 6 và Unarchive ở Buổi 7 — tức là **cắt đôi một FR qua hai buổi**, và trong suốt một buổi hệ thống có trạng thái Archived **vào được mà không ra được** — đúng cái "hố đen" mà MT-35 vừa sửa trong SRS. Đồng thời Buổi 6 của Dev 2 đã là buổi nặng nhất (hai FR mới + ba hạng mục nợ, trong đó có lỗ hổng bảo mật). Gom Archive + Unarchive + kiểm thử ma trận trạng thái vào một buổi giúp FR-RCP-006 được giao **trọn gói DB → API → UI**, và buổi này tập trung đúng vào việc quan trọng nhất: vá MT-34.

**Phần 4 – Ghi chú Mâu thuẫn & Quyết định Kiến trúc:**
- **Mâu thuẫn ở tài liệu cũ:** (a) **Mâu thuẫn nặng nhất toàn tài liệu** — FR-RCP-007 ghi *"Xóa **vĩnh viễn**... Đây là **hard delete** (không dùng soft delete pattern cho recipe)"* với cascade delete và xóa file MinIO qua Hangfire; trong khi **4 chỗ khác** nói ngược lại: Chương 7 (mọi entity kế thừa `BaseEntity` có `IsDeleted` + Global Query Filter), NFR-REL-003 (*"Recipe được đánh dấu `IsDeleted` thay vì xóa vật lý, **có thể khôi phục**"*), Chương 8 (*"Xóa recipe (**soft delete**)"*), Phụ lục A (*"404 = ... hoặc đã soft-delete"*). Tỷ lệ **4 soft / 1 hard**. (b) Lỗi MT-34 đã phân tích ở Buổi 2 — FR-RCP-001 lọc theo danh tính nhưng cache theo khóa công khai; FR-RCP-002 A2 trả 403 cho Draft.
- **Hướng đi chọn lựa:** SRS v1.1.0 (**MT-05, MT-34**) chốt **Soft Delete đồng bộ toàn hệ thống** với 3 hệ quả bắt buộc (partial unique index, child entity không xóa theo, không xóa file khi xóa mềm); **tách bạch Public/Private** với `GET /recipes/mine` cấm cache; Draft ở endpoint công khai trả **404**.
- **Tại sao chọn:** Về (a) — hard delete cho riêng Recipe sẽ **phá vỡ thiết kế `BaseEntity`** (Recipe có cột `IsDeleted` mà không dùng đến, gây hiểu nhầm cho mọi người đọc code sau này), làm mất dữ liệu không khôi phục được, và buộc phải sửa 4 mục tài liệu khác cho khớp. Ba phương án cân nhắc: *(A) soft delete toàn bộ*, *(B) hard delete riêng Recipe*, *(C) soft delete + job dọn vĩnh viễn sau N ngày*. Chọn **A làm nền + C làm phần mở rộng** (chính là FR-JOB-003 ở Buổi 4) — được cả "thùng rác khôi phục được" như sản phẩm thật lẫn việc giải phóng dung lượng đúng lúc. Về (b) — xem giải trình đầy đủ ở Phần 3 và ở Buổi 2 Dev 2.

**Phần 5 – Kết quả Commit Git:**
```
fix(recipes)!: close MT-34 draft leak; complete FR-RCP-007 soft delete & FR-RCP-011 private my-recipes
```
> Dùng `fix!` (breaking) vì hành vi công khai thay đổi: Admin/Author không còn thấy Draft ở `GET /recipes`, và Draft trả 404 thay vì 403 — FE phải chuyển sang `/recipes/mine`.

## DEV 3 — Trang chủ, ISR & SEO Structured Data

**Phần 1 – Chức năng hoàn thành trong buổi:**
Trang chủ + Trang danh mục (ISR đúng chuẩn) + JSON-LD Schema.org Recipe + Open Graph metadata (NFR-SEO-001/002/004).
**Kèm retrofit D-5, D-7.**

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Retrofit D-5 — gỡ lọc theo danh tính ở Category:** sửa `GetCategoryBySlugQuery` → **chỉ trả recipe `Status == Published`**, không đọc `ICurrentUser`; chuyển sang `ICacheable` khóa `categories:detail:{slug}:{queryHash}` **TTL 2 phút**. Cùng bản chất lỗi MT-34 như Dev 2 đang xử lý.
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

## DEV 1 — Quản lý Tài khoản [Admin] & Quản lý Phiên Đăng nhập

**Phần 1 – Chức năng hoàn thành trong buổi:**
**`FR-AUTH-008`** Quản lý Tài khoản Người dùng [Admin] — danh sách, khóa/mở khóa, "force revoke" toàn bộ phiên của người bị khóa + **`FR-AUTH-009`** Quản lý Phiên Đăng nhập của chính mình. Cả hai đều nằm trong SRS v1.2.0 (MT-44, MT-45).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Claim `sid` cho mọi access token (điều kiện của FR-AUTH-009):** trong `AuthResponseFactory` (dùng chung cho register/login/Google/refresh từ các buổi trước), gắn claim **`sid` = `Id` của bản ghi `RefreshToken`** được phát cùng cặp. Token phát trước khi triển khai không có `sid` — chúng tự hết hạn sau tối đa 15 phút, nên không cần di trú gì.
2. **FR-AUTH-008 — `SetUserStatusCommand { UserId, IsActive, Reason? }` + `GetUsersQuery`:** chỉ Admin (`AdminPolicy`). Khóa/mở khóa: **chặn Admin tự khóa chính mình → 403 Forbidden** kèm thông báo lý do (FR-AUTH-008 A2; bản v1.1.0 của Chương 8.1 ghi 409 là lỗi đã sửa — MT-52); user không tồn tại → 404; khi `isActive = false` gán cờ **và** gọi `RevokeAllForUserAsync(userId, now)` (dựng ở Buổi 4) — đây chính là "force revoke"; ghi audit log `Log.Warning("ADMIN ACTION: user {TargetUserId} deactivated by {AdminId}, reason: {Reason}")`. Danh sách: phân trang `page`, `pageSize`, `search?` (khớp một phần email/displayName), `isActive?` → `PagedResult<UserAdminDto { id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }>`, **không cache**.
3. **FR-AUTH-009 — `GetMySessionsQuery`, `RevokeSessionCommand`, `RevokeAllSessionsCommand`:** liệt kê refresh token còn hiệu lực của người gọi (`RevokedAt IS NULL AND ExpiresAt > NOW()`) → `SessionDto { id, createdAt, createdByIp, expiresAt, isCurrent }` với `isCurrent = (id == claim sid)`; **không bao giờ trả `TokenHash`**. Thu hồi một phiên: tìm theo `id` **và** `UserId == currentUserId` — phiên của người khác trả **404** (không phải 403) để không lộ id; đã thu hồi rồi vẫn trả 204. Thu hồi tất cả: revoke mọi phiên kể cả phiên hiện tại.
4. **API:** `GET /api/v1/users`, `PATCH /api/v1/users/{id}/status` (Admin — FR-AUTH-008); `GET /api/v1/auth/sessions`, `DELETE /api/v1/auth/sessions/{id}`, `POST /api/v1/auth/sessions/revoke-all` (Bearer — FR-AUTH-009). Ba endpoint trả danh sách/dữ liệu cá nhân gắn `Cache-Control: no-store` (quy ước Chương 8).
5. **UI + Test:** `/dashboard/users` (Admin) — bảng người dùng, ô tìm kiếm, toggle Khóa/Mở khóa với confirm dialog yêu cầu nhập lý do, badge trạng thái; `/profile` thêm tab **"Phiên đăng nhập"** liệt kê thiết bị (IP + thời gian, đánh dấu "Thiết bị này") kèm "Đăng xuất thiết bị này" và "Đăng xuất tất cả" (xong thì xóa phiên cục bộ, về trang đăng nhập). Integration test: khóa user → refresh token của họ trả **403 `AUTH_ACCOUNT_DISABLED`**; Admin tự khóa mình → **403**; user A thu hồi phiên của user B → **404**; JSON của `/auth/sessions` không chứa `tokenHash`.

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
feat(auth): complete FR-AUTH-008 admin user management & FR-AUTH-009 session management with sid claim
```

## DEV 2 — Archive/Unarchive & Đóng kín Máy trạng thái RecipeStatus

**Phần 1 – Chức năng hoàn thành trong buổi:**
`FR-RCP-006` Lưu trữ / Khôi phục Công thức — **trọn gói** `PATCH /recipes/{id}/archive` (Draft hoặc Published → Archived) + `PATCH /recipes/{id}/unarchive` (Archived → Draft) + kiểm thử máy trạng thái khép kín. Kèm endpoint `GET /recipes/sitemap` phục vụ sitemap cho Dev 3 (SRS v1.2.0 Chương 8.3 — MT-48).

**Phần 2 – Các bước tiến hành chi tiết:**
1. **Domain — bảng chuyển trạng thái tường minh:** hiện thực `Recipe.Archive()` và `Recipe.Unarchive()`. Gom toàn bộ luật chuyển trạng thái vào **một** nơi duy nhất (`RecipeStatusTransitions` — dictionary tra cứu `(from, action) → to`); refactor `Publish()`/`Unpublish()` (Buổi 5) để cũng đi qua nó:

   | Từ trạng thái | Hành động | Endpoint | Sang trạng thái |
   |---|---|---|---|
   | Draft | publish | `PATCH /recipes/{id}/publish` | Published |
   | Published | unpublish | `PATCH /recipes/{id}/unpublish` | Draft |
   | Draft **hoặc** Published | archive | `PATCH /recipes/{id}/archive` | Archived |
   | Archived | **unarchive** | `PATCH /recipes/{id}/unarchive` | **Draft** |

   Mọi chuyển đổi **ngoài bảng** → `DomainException` → **409 `RECIPE_INVALID_STATE_TRANSITION`**.
2. **CQRS + API:** `ArchiveRecipeCommand`, `UnarchiveRecipeCommand` qua `RecipeAuthorizationHandler`; `PATCH /api/v1/recipes/{id:guid}/archive` và `/unarchive` (200; 403; 404; **409** khi chuyển trạng thái sai); invalidate `recipe:{slug}` + prefix `recipes:list:` + prefix `categories:detail:`. **Endpoint sitemap (MT-48):** `GET /api/v1/recipes/sitemap` trả `{ slug, updatedAt }[]` của mọi recipe Published, không phân trang, `ICacheable` TTL 1 giờ; `sitemap` đã nằm trong danh sách slug dành riêng (Buổi 3).
3. **Kiểm thử máy trạng thái khép kín (unit test theo ma trận):** viết test cho **toàn bộ 12 tổ hợp** `(3 trạng thái × 4 hành động)` — 5 tổ hợp hợp lệ phải thành công và cho đúng trạng thái đích, **7 tổ hợp còn lại phải ném `DomainException`**. Đây là cách duy nhất chứng minh máy trạng thái thực sự đóng kín, thay vì chỉ test đường hạnh phúc.
4. **Kiểm thử `PublishedAt` không bị ghi đè:** test kịch bản `publish → unpublish → publish lại`, assert `PublishedAt` **giữ nguyên giá trị lần đầu**.
5. **UI:** `/dashboard/recipes` bổ sung action **"Lưu trữ"** cho recipe Draft/Published và nút **"Khôi phục về nháp"** ở tab "Archived"; badge trạng thái đổi màu theo `RecipeStatus`; **chỉ hiện những nút hợp lệ với trạng thái hiện tại** (đọc từ cùng bảng chuyển trạng thái, xuất sang FE dưới dạng hằng số dùng chung); khi vẫn nhận 409 `RECIPE_INVALID_STATE_TRANSITION` (do người khác đổi trạng thái trước) thì hiện thông báo giải thích và tải lại dữ liệu.

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
feat(recipes): complete FR-RCP-006 archive/unarchive closing the recipe status machine with transition matrix tests
```

## DEV 3 — Sitemap Next.js, robots.txt & Tối ưu hình ảnh

**Phần 1 – Chức năng hoàn thành trong buổi:**
`NFR-SEO-003` Sitemap XML + `robots.txt` sinh bởi Next.js + tối ưu `next/image` toàn site (NFR-PERF-005 Core Web Vitals). **Kèm retrofit D-14 (phía FE):** bỏ `unoptimized` khỏi `next/image`.

**Phần 2 – Các bước tiến hành chi tiết:**
1. **`app/sitemap.ts`:** export `default async function sitemap(): Promise<MetadataRoute.Sitemap>` — lấy danh sách slug + `lastmod` của **toàn bộ recipe Published** và **category**, cộng các trang tĩnh (`/`, `/recipes`, `/categories`, `/search`); mỗi entry có `url`, `lastModified`, `changeFrequency`, `priority`. Đặt `export const revalidate = 3600` (sitemap không cần tươi hơn một giờ).
2. **Nguồn dữ liệu sitemap:** gọi `GET /api/v1/recipes/sitemap` do Dev 2 bổ sung cùng buổi (SRS v1.2.0 — MT-48) — một request trả toàn bộ `{ slug, updatedAt }` của recipe Published, cache 1 giờ đúng bằng chu kỳ tái sinh sitemap. Đặt lời gọi trong `lib/seo/sitemapSource.ts` (tách khỏi `app/sitemap.ts`) để phần sinh XML test được độc lập với API. **Không** duyệt `GET /recipes` theo trang: với 10.000 công thức cần khoảng 200 request mỗi lần, và dữ liệu có thể xê dịch giữa các trang làm lặp hoặc sót slug — chính là lý do SRS bổ sung endpoint riêng.
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

## 5. Ma trận truy vết FR → Buổi → Dev (37/37 FR — SRS v1.2.1)

> Buổi 1 (đọc đặc tả) không hiện thực FR nào; số buổi dưới đây theo cách đánh số 8 buổi.

| Mã FR | Tên | Buổi | Dev | Ưu tiên |
|---|---|---|---|---|
| FR-AUTH-001 / 002 | Đăng ký / Đăng nhập local | 2 | 1 | M / M |
| FR-AUTH-003 / 005 | Google ID Token / Logout | 3 | 1 | **M** / M |
| FR-AUTH-004 | Refresh Rotation + Reuse Detection | 4 | 1 | M |
| FR-AUTH-006 / 007 | Xem / Cập nhật Profile | 5 | 1 | **M** / M |
| **FR-AUTH-008** | **Quản lý tài khoản [Admin]: danh sách + khóa / mở khóa** | **7** | **1** | **S** |
| **FR-AUTH-009** | **Quản lý phiên đăng nhập của chính mình** (mới ở v1.2.0 — MT-45) | **7** | **1** | **C** |
| FR-CAT-001 / 002 | Danh mục public + Redis cache | 2 | 3 | M / M |
| FR-CAT-003 / 004 / 005 | CRUD danh mục Admin | 3 | 3 | M / M / M |
| FR-RCP-001 / 002 | Danh sách / Chi tiết công thức | 2 | 2 | M / M |
| FR-RCP-003 / 008 | Tạo Draft / Quản lý ảnh | 3 *(mảng inline `steps?`/`ingredients?` của FR-RCP-003: 4)* | 2 | M / M |
| FR-RCP-009 / 010 | Nguyên liệu / Các bước + reorder | 4 | 2 | M / M |
| FR-RCP-004 / 005 | Update RowVersion / Publish | 5 | 2 | M / M |
| FR-RCP-007 | Soft Delete | 6 | 2 | M |
| **FR-RCP-011** | **`GET /recipes/mine` (cấm cache)** | **6** | **2** | **M** |
| FR-RCP-006 | **Archive + Unarchive trọn gói**, đóng kín máy trạng thái | **7** | 2 | **M** |
| FR-SRCH-001 | Full-Text Search (generated column) | 4 | 3 | M |
| FR-SRCH-002 / 003 / 004 | Lọc / Sắp xếp / Phân trang | 5 | 3 | **M / M / M** |
| FR-FILE-001 / 002 | MinIO Upload / Delete (service + endpoint `/files/*` — endpoint chính thức hóa ở v1.2.0, MT-43) | 2 | 4 | **M / M** |
| FR-JOB-001 | Welcome Email (job: **đã làm ở B2**; dashboard bảo vệ: B3) | **2 + 3** | 4 | S |
| FR-JOB-002 | Image Resize | 4 | 4 | S |
| **FR-JOB-003** | **Permanent Purge Job** (không phải Sitemap) | **4** | **4** | **S** |
| FR-OBS-001 | Health Checks | 5 | 4 | **M** |
| FR-OBS-002 | Structured Logging | 6 | 4 | **M** |
| FR-OBS-003 | Tracing & Metrics | 6 | 4 | **C** |

> **In đậm** = thay đổi so với kế hoạch 6 buổi (FR mới, mức ưu tiên được nâng theo MT-40, nội dung được định nghĩa lại, hoặc buổi thực hiện thay đổi).
>
> **Hai FR trải qua hai buổi — đều có lý do kiến trúc:** FR-JOB-001 vì Buổi 2 đã làm sớm phần job (theo quy tắc "không code giả" của Buổi 2), chỉ còn lại dashboard; FR-RCP-003 vì hai mảng inline dùng lại đúng bất biến của FR-RCP-009/010 — làm sớm sẽ phải viết validator nguyên liệu/bước hai lần. **FR-RCP-006 không còn bị cắt đôi** như lộ trình ban đầu.

**Hạ tầng chung không gắn với một FR cụ thể:** commit nền D-11 (B3 — Dev 4 dẫn) · Integration Test Harness (B3 — Dev 4) · `UseForwardedHeaders` (B4 — Dev 1) · Composite indexes (B5 — Dev 3) · Nginx production + `/media/` (B7 — Dev 4).

**Truy vết NFR:**

| Nhóm NFR | Buổi – Dev |
|---|---|
| SEC-001 / 002 (mật khẩu, JWT) | B2 Dev1 · B4 Dev1 (256-bit, hash, rotation, access token trong bộ nhớ — D-2, D-12) |
| SEC-003 (rate limit + ForwardedHeaders) | **B4 Dev1** (`UseForwardedHeaders`) · **B6 Dev1** (rate limit) · kiểm chứng B8 Dev1 |
| SEC-004 (input validation, file upload) | B2 Dev4 (magic bytes) · B3 commit nền (400 thống nhất — D-11) · xuyên suốt |
| SEC-005 (HTTPS, CORS, HSTS, CSP) | B2 (CORS) · B6 Dev1 (CSP) · **B7 Dev4** (SSL, HSTS) |
| SEC-006 (authorization + cách ly cache) | B3 Dev2 (`RecipeAuthorizationHandler`) · B6 Dev1 (rà soát policy) · **B6 Dev2 + Dev3** (MT-34 — D-4, D-5) |
| SEC-007 (secrets, không log bí mật) | B2 Dev4 (gitleaks, User Secrets) · B3 Dev4 (htpasswd/gate secret) · **B6 Dev4** (redaction) · B7 Dev4 (DataProtection — D-17) |
| PERF-001 / 002 (response time, throughput) | **B8 Dev4** (k6) |
| PERF-003 (cache TTL + hit rate) | B3 Dev3 (D-13) · B6 Dev2/Dev3 (D-6, D-7) · **B7 Dev4** (audit) |
| PERF-004 (index, EXPLAIN ANALYZE) | **B5 Dev3** (composite index + đo trên ≥ 10.000 bản ghi) |
| PERF-005 (Core Web Vitals) | B7 Dev3 (bật lại tối ưu ảnh — D-14) · **B8 Dev3** (Lighthouse) |
| USE-001 → 004 (responsive, a11y, lỗi, loading) | B2 Dev4 (progress bar) · B3 (lỗi từng ô sau D-11) · **B8 Dev3** (WCAG) |
| REL-001 (uptime, healthcheck) | B2 (healthcheck postgres/redis/minio) · **B5 Dev4** (health endpoints + healthcheck api/frontend) |
| REL-002 (resilience, fallback) | B2 Dev3 (Redis fallback) · **B8 Dev4** (kiểm chứng có tải) |
| REL-003 (durability, soft delete 30 ngày) | B4 Dev4 (purge job) · B6 Dev2 |
| MAINT-001 → 004 (chất lượng, test, docs, Clean Arch) | xuyên suốt · **B3 Dev4** (integration test harness) · **B8** (coverage, 6 ADR, CHANGELOG) |
| SCALE-001 → 003 (stateless, DB, hạ tầng) | B2 (worker Hangfire riêng — SRS v1.2.0 MT-47) · B4 Dev4 (distributed lock) · **B7 Dev4** (replicas, resolver động) |
| SEO-001 → 004 (JSON-LD, meta, sitemap, URL) | B3 Dev2 (slug dành riêng) · **B6 Dev3** (JSON-LD, OG) · **B7 Dev3** (sitemap, robots) |

---

## 6. Phụ thuộc liên dev (điểm bắt buộc phối hợp)

| Buổi | Phụ thuộc | Cách xử lý |
|---|---|---|
| **B3** | ⚠️ **Mọi dev** phụ thuộc commit nền D-11 (422 → 400) | Dev 4 dẫn làm trong **30 phút đầu**, cả nhóm review; 4 dev tách nhánh **sau** commit này |
| **B3** | Mọi dev viết integration test trên harness của **Dev 4** | Dev 4 push harness (`CulinaryBlogApiFactory`, `CreateClientAs`) **trước giữa buổi**; trước đó các dev viết test theo mẫu đã thống nhất rồi chạy khi harness vào |
| **B3** | Dev 1 đổi `UserDto` (`fullName` → `displayName`, bỏ `userName` — D-1) | Mọi component FE đọc `user.fullName` phải đổi; Dev 1 merge trước Dev 2/Dev 3 theo thứ tự chuẩn, các dev sau rebase |
| **B3** | Dev 2 cần `SlugHelper`, `IFileStorageService`, `ImageFileInspector` | Đã có sẵn từ B2 (`Domain/Common`, `Application/Common`) — chỉ inject |
| **B4** | Dev 4 móc `ImageResizeJob` vào `UploadRecipeImageCommand` của Dev 2 (B3) | Dev 2 để sẵn điểm `BackgroundJob.Enqueue`; Dev 4 chỉ cắm job vào |
| **B4** | Dev 2 (`B4_Recipe_IngredientStep`) và Dev 3 (`B4_Search_FTS`) **cùng tạo migration trên bảng `Recipes`** | Theo thứ tự merge chuẩn: Dev 3 trước, Dev 2 rebase rồi chạy lại `dotnet ef migrations add` |
| **B4** | Dev 1 bật `UseForwardedHeaders` — cần Nginx gửi `X-Forwarded-For` | **Đã có từ B2** (`nginx.conf` dòng 52) — không phụ thuộc ai |
| **B5** | ⚠️ Dev 3 refactor `GetRecipesQuery` + trang FE `/recipes` (**code của Dev 2**) để dùng chung `RecipeFilterSpec` và bỏ `sort=-field` (D-10) | **Thống nhất interface trước khi code**; Dev 2 không sửa hai file này trong B5 |
| **B6** | ⚠️ **Dev 2 và Dev 3 cùng sửa lỗi MT-34**; kiểu `RecipeVisibility` được **dùng chung** bởi `RecipeReadRepository` (Dev 2) và `GetCategoryBySlugQuery` (Dev 3) | Dev 3 gỡ tham chiếu `RecipeVisibility` trong query danh mục và merge trước; Dev 2 xóa hẳn kiểu này khi rebase. Output Cache chỉ có ở `RecipesEndpoints` nên **chỉ Dev 2** gỡ |
| **B7** | Dev 3 (sitemap) cần `GET /recipes/sitemap` từ **Dev 2** (MT-48) | Dev 2 làm endpoint này **đầu buổi** và merge sớm — endpoint không có migration nên không vướng thứ tự merge chuẩn (thứ tự Dev4 → Dev1 → Dev3 → Dev2 chỉ ràng buộc migration, §2.4); Dev 3 viết `lib/seo/sitemapSource.ts` theo hợp đồng `{ slug, updatedAt }[]` đã chốt trong SRS, dùng dữ liệu giả cho tới khi endpoint vào |
| **B7** | Dev 1 thêm claim `sid` vào `AuthResponseFactory` — mọi luồng phát token (đăng ký, đăng nhập, Google, refresh) đều đi qua đây | Chỉ Dev 1 sửa file này trong B7; các dev khác không phụ thuộc vào `sid` |
| **B7** | Dev 3 gỡ `unoptimized` (D-14) cần Nginx `/media/` của **Dev 4** | Dev 4 merge trước (đúng thứ tự chuẩn); Dev 3 kiểm chứng sau khi rebase |
| **B8** | Cả 4 dev chạy E2E trên cùng một môi trường | Dev 4 dựng môi trường prod-like **đầu buổi 8**, 3 dev còn lại viết test trên đó |

**Thứ tự merge migration (giữ nguyên quy ước B2):** Dev4 → Dev1 → Dev3 → Dev2. Dev sau `rebase` + chạy lại `dotnet ef migrations add` nếu snapshot conflict.

---

## 7. Verification (cuối mỗi buổi & cuối dự án)

**Cuối mỗi buổi — Definition of Done:**
1. `docker compose up -d --build` → tất cả container `healthy`; `curl http://localhost/health` (qua Nginx) trả `Healthy`.
2. `dotnet build && dotnet test` xanh — build đã bật `TreatWarningsAsErrors`; test gồm unit + **integration (Testcontainers, từ B3)** + **ArchitectureTests** (Domain không reference assembly ngoài nào, Application không reference Infrastructure).
3. `cd frontend && npm run lint && npm run build && npm test` xanh.
4. Test tay các endpoint của buổi qua Scalar `/scalar` + UI tương ứng.
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
- [ ] **37/37 FR** (SRS v1.2.1) hoàn thành và truy vết được theo ma trận §5.
- [ ] **44/44 endpoint** của SRS v1.2.0 Chương 8 hoạt động và có trong Scalar `/scalar`.
- [ ] **17/17 hạng mục nợ kỹ thuật cần sửa code** (§4.1) đã hoàn trả — đặc biệt **D-4, D-5 (lỗi bảo mật MT-34)** và **D-11 (422 → 400)**.
- [ ] **27/27 mã lỗi** ở SRS v1.2.0 Phụ lục B đều có đường sinh ra và có test tương ứng; mã `AUTH_USERNAME_EXISTS` đã bị xóa khỏi code.
- [ ] Dữ liệu mẫu đạt SRS v1.2.1 §2.6.1 (CR-2026-03): ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước.
- [ ] **0 vị trí** dùng mã **422** trong toàn bộ code và test.
- [ ] Bảng TTL cache trong code khớp **100%** với SRS NFR-PERF-003.
- [ ] Cấu hình Nginx và lệnh healthcheck trong repo khớp SRS v1.2.0 §6.5 (resolver động, không khối `upstream`; không lệnh healthcheck nào gọi công cụ không có trong image).
- [ ] **6 ADR** đã viết (MT-05, MT-08/09, MT-16, MT-34, MT-47, MT-50).
- [ ] Tag `v1.0.0` (phiên bản phần mềm), `CHANGELOG.md` cập nhật, `README.md` setup được trong < 5 phút.
- [ ] Nếu trong quá trình làm phát hiện thêm điểm SRS sai hoặc thiếu: ghi vào `SRS_MAU_THUAN_VA_GIAI_PHAP.md` (MT-58 trở đi) và đi qua Change Request — **không** tự làm khác SRS trong code.

---

*— Hết kế hoạch —*

> **Nguồn chuẩn:** mọi yêu cầu trong tài liệu này bám theo `SPEC/SRS_Culinary_Blog_v1.2.1.md` (21/09/2026) — gồm CR-2026 (41 quyết định, SRS Phụ lục D), CR-2026-02 (16 quyết định, SRS Phụ lục E) và CR-2026-03 (yêu cầu dữ liệu mẫu, SRS §2.6.1). Hiện trạng code được đối chiếu trực tiếp với nhánh `main` và `BAO_CAO_BUOI_2.md`. Mọi xung đột và mâu thuẫn — trong SRS, giữa SRS và code, giữa SRS và lộ trình được giao — đều được ghi đầy đủ tại `SRS_MAU_THUAN_VA_GIAI_PHAP.md`. Kế hoạch **không có điểm nào làm khác SRS v1.2.1**; khi phát hiện khác biệt, **SRS là nguồn đúng** và mọi thay đổi SRS phải qua Change Request.
