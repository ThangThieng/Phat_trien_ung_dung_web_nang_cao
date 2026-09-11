# KẾ HOẠCH PHÁT TRIỂN CHI TIẾT 6 BUỔI – CULINARY BLOG (FEATURE-DRIVEN GIT COMMIT)

> Nguồn: `SPEC/SRS_Culinary_Blog_v1.0.0.pdf` (v1.0.0, Approved 04/06/2026, 71 trang)
> File: **`SPEC/KE_HOACH_PHAT_TRIEN_6_BUOI.md`**

---

## 0. Context

Repo `d:\ptudwnc_nhom20` hiện chỉ có tài liệu SRS, chưa có mã nguồn. Nhóm có 4 dev full-stack và 6 buổi làm việc. Kế hoạch này chuyển SRS thành lộ trình theo nguyên tắc **"mỗi buổi – mỗi chức năng – mỗi commit"**: cuối mỗi buổi, mỗi dev commit một lát cắt dọc hoàn chỉnh (DB → API → UI), chạy được, không làm gãy build.

---

## 1. Quyết định kiến trúc chung (cả nhóm tuân thủ)

### 1.1 Cấu trúc repository (monorepo)
```
ptudwnc_nhom20/
├─ SPEC/                          # SRS + kế hoạch này
├─ backend/
│  ├─ CulinaryBlog.sln
│  ├─ src/CulinaryBlog.Domain/          # Entities, VO (Slug, EmailAddress), Enums, IRepository – KHÔNG NuGet
│  ├─ src/CulinaryBlog.Application/     # Commands/Queries/Handlers (MediatR), Validators, DTOs, Behaviors, Interfaces
│  ├─ src/CulinaryBlog.Infrastructure/  # EF Core DbContext, Configurations, Migrations, Repos, JwtService, MinIO, Redis, MailKit, Hangfire jobs
│  ├─ src/CulinaryBlog.API/             # Minimal API Endpoint groups, Middleware, Program.cs, Scalar /scalar
│  └─ tests/ (Domain.UnitTests, Application.UnitTests, API.IntegrationTests, ArchitectureTests)
├─ frontend/                      # Next.js 15 App Router + TS + Tailwind + TanStack Query + RHF + Zod + Auth.js v5
│  └─ src/{app, components, features/{auth,recipes,categories,search,files}, lib/{api-client,query-client}}
├─ nginx/nginx.conf
├─ docker-compose.yml / docker-compose.prod.yml / .env.example
└─ e2e/ (Playwright)
```

### 1.2 Quy ước kỹ thuật dùng chung
| Hạng mục | Quy ước |
|---|---|
| BaseEntity | `Id (uuid, gen_random_uuid())`, `CreatedAt`, `UpdatedAt`, `IsDeleted` (Global Query Filter), `RowVersion bytea` (concurrency token) |
| RowVersion trên PostgreSQL | Cột `bytea` cấu hình `.IsConcurrencyToken()`; `AuditInterceptor` gán `Guid.NewGuid().ToByteArray()` mỗi lần Added/Modified (Postgres không tự sinh rowversion như SQL Server) |
| Pipeline MediatR | `LoggingBehavior` → `ValidationBehavior` → `CachingBehavior` (`ICacheable`) → Handler → `CacheInvalidationBehavior` (`ICacheInvalidator`) |
| Lỗi | RFC 7807 qua `GlobalExceptionMiddleware`; `type` = Application Error Code (Phụ lục B, VD `AUTH_EMAIL_EXISTS`) |
| Response | `{ data, meta: { page, pageSize, total, totalPages } }`; `PagedResult<T>` có `hasNextPage/hasPreviousPage` |
| Endpoint | `/api/v1/...`, mỗi module 1 file `XxxEndpoints.cs` (`MapGroup`) – không Controller |
| Policy | `"AuthorPolicy"`, `"AdminPolicy"`, `RecipeAuthorizationHandler` (Owner/Admin) – không hardcode role string |
| FE API client | `lib/api-client.ts` (fetch wrapper gắn Bearer, auto-refresh 401, parse ProblemDetails), TanStack Query keys theo module |
| FE form | React Hook Form + Zod schema mirror validator backend, lỗi inline theo `errors{}` |

### 1.3 Giải quyết điểm mâu thuẫn trong SRS (quyết định Tech Lead)
| Mâu thuẫn | Quyết định |
|---|---|
| Validation: FR ghi **422**, Phụ lục A/B ghi 400 | **422** cho FluentValidation/Identity (theo 27 FR chi tiết); 400 cho file/MIME/business rule (`FILE_*`, `RECIPE_PUBLISH_INCOMPLETE`) |
| Concurrency: FR-RCP-004 ghi **409**, Phụ lục ghi 422 | **409 Conflict** + `type=RECIPE_CONCURRENCY_CONFLICT` (FR là nguồn chính) |
| Cache Category: IMemoryCache vs NFR-SCALE-001 (Redis) | **Redis `IDistributedCache`**, key `categories:all`, TTL 30 phút; Redis down → fallback DB (NFR-REL-002) |
| FR-RCP-007 hard delete vs BaseEntity soft delete | Recipe **hard delete** + cascade (theo FR-RCP-007); Category dùng soft delete |
| Refresh token 512-bit (FR) vs 128-bit (NFR) | **512-bit** random, lưu **SHA-256 hash** (`TokenHash varchar(64)`) |
| Điều kiện Publish: ≥1 step (FR) vs ≥1 step + ≥1 ingredient (Phụ lục B) | Bắt buộc **cả hai** (chặt hơn) |
| Docker "8 services" có Hangfire | Hangfire server **in-process trong API** (SRS §3.6); service `hangfire` = cùng image API chạy mode worker (`Hangfire__WorkerOnly=true`), dashboard `/hangfire` (Admin). `mailhog` chạy dưới `profiles: [dev]` |
| Tổng số FR: SRS ghi "27" nhưng liệt kê 34 mã (AUTH 7, CAT 5, RCP 10, SRCH 4, FILE 2, JOB 3, OBS 3) | Kế hoạch phủ **đủ 34 mã** |

### 1.4 Quy trình Git
- Nhánh: `main` (luôn xanh) ← `develop` ← `feature/b{n}-dev{k}-{fr}` (VD `feature/b1-dev1-auth-001-002`).
- Conventional Commits; **1 commit squash / dev / buổi**, PR có ≥1 reviewer (NFR-MAINT-001).
- **Migration**: mỗi dev tạo migration riêng tên `B{n}_{Module}_{FR}`; merge theo thứ tự Dev4 → Dev1 → Dev3 → Dev2, dev sau `rebase` + `dotnet ef migrations add` lại nếu snapshot conflict.
- Definition of Done mỗi commit: `dotnet build` 0 warning, `dotnet test` xanh, `npm run lint && npm run build` xanh, `docker compose up` chạy, test tay qua Scalar + UI.

---

## BUỔI 1 – NỀN MÓNG & LÁT CẮT ĐẦU TIÊN

> **30 phút đầu (Dev 4 dẫn):** push commit `chore: bootstrap clean architecture solution & nextjs app` gồm solution 4 tầng, `BaseEntity`, `CulinaryBlogDbContext`, `AuditInterceptor`, 4 Pipeline Behaviors, `GlobalExceptionMiddleware`, `PagedResult<T>`, Next.js skeleton + `api-client`. 3 dev còn lại branch từ commit này.

### DEV 1 – Auth
**Phần 1 – Chức năng hoàn thành:** `FR-AUTH-001` Đăng ký tài khoản + `FR-AUTH-002` Đăng nhập Email/Mật khẩu.

**Phần 2 – Các bước:**
1. **DB/Entity:** `ApplicationUser : IdentityUser` (DisplayName/FullName varchar(100), AvatarUrl, Bio, IsActive, CreatedAt), entity `RefreshToken` (TokenHash varchar(64) UNIQUE, ExpiresAt, RevokedAt, ReplacedByTokenHash, CreatedByIp); cấu hình Identity: PBKDF2 (Identity mặc định, ≥100k iterations), password ≥8 (hoa+thường+số+đặc biệt), Lockout 5 lần/15 phút; seed role `Author`, `Admin` + 1 admin → migration `B1_Auth_Identity`.
2. **Service:** `IJwtService`/`JwtService` – access token HS256 15 phút (claims userId, email, roles, jti), refresh token 512-bit random + SHA-256 hash, TTL 7 ngày; `IRefreshTokenRepository`.
3. **CQRS:** `RegisterCommand` + `RegisterCommandValidator` + Handler (FindByEmail → 409 `AUTH_EMAIL_EXISTS`, CreateAsync, AddToRole "Author", phát token, lưu RefreshToken, gọi `IWelcomeEmailScheduler` – stub no-op, Dev 4 cắm Hangfire ở Buổi 2); `LoginCommand` + Handler (CheckPassword, IsLockedOut → 423, AccessFailed, 401 generic `AUTH_INVALID_CREDENTIALS`).
4. **API:** `AuthEndpoints.cs` → `POST /api/v1/auth/register` (201 AuthResponseDto), `POST /api/v1/auth/login` (200); cấu hình `AddAuthentication().AddJwtBearer()`.
5. **UI:** `/auth/register`, `/auth/login` (CSR) – RHF + Zod, lỗi inline map từ ProblemDetails; `AuthProvider` lưu access token in-memory + refresh token; header hiển thị user đã đăng nhập. Test: xUnit cho validator + integration test register/login (happy + 409/401).

**Commit:** `feat(auth): complete FR-AUTH-001 & FR-AUTH-002 register and local login flow`

### DEV 2 – Recipe lõi
**Phần 1 – Chức năng hoàn thành:** `FR-RCP-001` Xem danh sách công thức (phân trang) + `FR-RCP-002` Xem chi tiết công thức.

**Phần 2 – Các bước:**
1. **DB/Entity:** `Recipe` (Title 200, Slug 220 UNIQUE, Description, Instructions, PrepTime, CookTime, Servings, `RecipeDifficulty` 1–4, `RecipeStatus` 0–2, CategoryId FK RESTRICT, AuthorId FK, PublishedAt), owned `RecipeNutrition` (prefix `Nutrition_`, decimal(8,2)?), `RecipeStep`, `RecipeIngredient`, `RecipeImage` (cascade); index Slug/Status/CategoryId/AuthorId/PublishedAt/Difficulty → migration `B1_Recipe_Schema`.
2. **Seed:** `RecipeSeeder` dùng **Bogus** – 50 recipe mẫu, 5 tác giả (theo SRS §2.6.1), đủ step/ingredient/nutrition để FE có dữ liệu thật.
3. **CQRS:** `GetRecipesQuery` (page ≥1, pageSize 1–50, lọc quyền: Guest→Published, Author→Published + Draft của mình, Admin→tất cả; `.AsNoTracking()` + projection sang `RecipeSummaryDto`, không N+1); `GetRecipeBySlugQuery` (Include Steps/Ingredients/Images/Category/Author, 404 `RECIPE_NOT_FOUND`, Draft/Archived → 403 nếu không phải owner/Admin).
4. **API:** `RecipesEndpoints.cs` → `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}`; Output Cache policy `RecipeList` (15 phút, vary query) và `RecipeDetail` (60 phút, tag `recipes`, `recipe:{slug}`).
5. **UI:** `/recipes` (SSR) lưới `RecipeCard` + phân trang + loading skeleton; `/recipes/[slug]` (ISR revalidate=300) hiển thị ảnh, nguyên liệu, các bước, bảng dinh dưỡng, tác giả.

**Commit:** `feat(recipes): complete FR-RCP-001 & FR-RCP-002 recipe listing and detail page`

### DEV 3 – Category
**Phần 1 – Chức năng hoàn thành:** `FR-CAT-001` Xem danh sách danh mục (cache) + `FR-CAT-002` Xem chi tiết danh mục kèm công thức.

**Phần 2 – Các bước:**
1. **DB/Entity:** `Category` (Name varchar(100) UNIQUE, Slug varchar(120) UNIQUE, Description, ImageUrl, OrderIndex) + Value Object `Slug` và `SlugHelper.Generate()` (lowercase, bỏ dấu tiếng Việt, đ→d, space→"-") + unit test slug; seed ~8 danh mục → migration `B1_Category_Schema`.
2. **Cache:** `ICacheService`/`RedisCacheService` (StackExchange.Redis, try/catch fallback DB khi Redis down); `CachingBehavior` dùng `ICacheable { CacheKey, Expiration }`.
3. **CQRS:** `GetCategoriesQuery : ICacheable` (key `categories:all`, TTL 30 phút, sort Name ASC, `recipeCount` chỉ đếm Published); `GetCategoryBySlugQuery` (404 `CATEGORY_NOT_FOUND`, recipes Published + Draft của currentUser, OFFSET pagination → `PagedResult<RecipeSummaryDto>`).
4. **API:** `CategoriesEndpoints.cs` → `GET /api/v1/categories`, `GET /api/v1/categories/{slug}?page&pageSize`.
5. **UI:** `/categories` (ISR 3600) lưới `CategoryCard`; `/categories/[slug]` (ISR 600) header danh mục + list recipe phân trang; component `CategoryNav` cho header site.

**Commit:** `feat(categories): complete FR-CAT-001 & FR-CAT-002 public categories with redis cache`

### DEV 4 – Hạ tầng & File
**Phần 1 – Chức năng hoàn thành:** Docker Compose 8 services + `FR-FILE-001` Upload file lên MinIO + `FR-FILE-002` Xóa file khỏi MinIO (kèm Reusable Image Upload Component).

**Phần 2 – Các bước:**
1. **DevOps:** `docker-compose.yml` gồm `nginx, api, frontend, postgres:16-alpine, redis:7-alpine (--appendonly yes), minio (console :9001), hangfire (worker), seq` + `mailhog` (profile dev); volumes `pgdata, redisdata, miniodata, seqdata`; `init.sql` bật extension `unaccent`, `pg_trgm`; `.env.example`, User Secrets cho dev (NFR-SEC-007); README "setup < 5 phút".
2. **Application/Infra:** `IFileStorageService { UploadAsync(stream, fileName, contentType, folder, ct), DeleteAsync(url, ct) }`; `MinioFileStorageService` (AWSSDK.S3 + ServiceURL override, `ForcePathStyle`), bucket `culinary-blog` tự tạo + policy public-read khi khởi động; tên file `{folder}/{Guid}{ext}` chống path traversal; Delete idempotent (object không tồn tại không throw).
3. **Validation:** `FileValidator` – ≤5MB kiểm tra trước khi đọc stream, MIME whitelist jpeg/png/webp/avif, **magic bytes** (FF D8 FF, 89 50 4E 47, RIFF…WEBP, ftypavif) → 400 `FILE_SIZE_EXCEEDED` / `FILE_MIME_INVALID`.
4. **API tạm để test độc lập:** `POST /api/v1/files` + `DELETE /api/v1/files` (Bearer, dùng lại ở avatar Buổi 4); unit test magic bytes.
5. **UI:** `components/ImageUploader.tsx` tái sử dụng – drag & drop, preview, validate client (size/MIME), **progress bar %** qua XHR `upload.onprogress` (NFR-USE-004), `onUploaded(url)` callback.

**Commit:** `feat(infra,files): docker compose 8 services & complete FR-FILE-001/002 minio storage with image uploader`

---

## BUỔI 2 – MỞ RỘNG XÁC THỰC, TẠO NỘI DUNG, QUẢN TRỊ

### DEV 1
**Phần 1:** `FR-AUTH-003` Đăng nhập Google OAuth 2.0 PKCE + `FR-AUTH-005` Đăng xuất / Revoke refresh token.

**Phần 2:**
1. **FE OAuth:** Auth.js v5 Google provider (Authorization Code + **PKCE**, scopes `openid email profile`), callback lấy `id_token`, gọi backend.
2. **CQRS:** `GoogleLoginCommand { IdToken }` – xác minh bằng `GoogleJsonWebSignature.ValidateAsync` (audience = GoogleClientId) → 400 `AUTH_GOOGLE_TOKEN_INVALID`; `FindByLoginAsync("Google", sub)` → nếu chưa có: tạo user (avatar Google, role Author) hoặc link vào email đã tồn tại `AddLoginAsync`; lỗi mạng Google → 502.
3. **CQRS:** `LogoutCommand { RefreshToken }` – hash, tìm token thuộc currentUser → `RevokedAt = UtcNow`; không tìm thấy vẫn 204 (idempotent).
4. **API:** `POST /api/v1/auth/google`, `POST /api/v1/auth/logout` (RequireAuthorization).
5. **UI:** nút "Đăng nhập bằng Google" trên `/auth/login` (thông báo fallback nếu Google lỗi – SRS §2.6.2), menu user → Đăng xuất (xóa token client, invalidate TanStack cache). Integration test logout idempotent.

**Commit:** `feat(auth): complete FR-AUTH-003 google oauth pkce & FR-AUTH-005 logout token revocation`

### DEV 2
**Phần 1:** `FR-RCP-003` Tạo công thức (Draft) + `FR-RCP-008` Upload / đặt ảnh chính / xóa ảnh (MinIO).

**Phần 2:**
1. **CQRS:** `CreateRecipeCommand` + Validator (title 5–200, prep/cook/servings > 0, categoryId tồn tại → 422, steps/ingredients/nutrition optional); `SlugHelper` (của Dev 3) + suffix `-2,-3` nếu trùng; `Recipe.Create()` domain factory, Status=Draft; `ICacheInvalidator` tag `recipes`.
2. **Authorization:** `RecipeAuthorizationHandler` (Owner hoặc Admin) + policy `AuthorPolicy`; dùng lại cho mọi FR recipe sau.
3. **CQRS ảnh:** `UploadRecipeImageCommand` (dùng `IFileStorageService` + `FileValidator` của Dev 4, path `recipes/{recipeId}/`, ảnh đầu tiên `IsPrimary=true`); `SetPrimaryImageCommand` (1 primary/recipe); `DeleteRecipeImageCommand` (xóa DB, `BackgroundJob.Enqueue` xóa MinIO, tự gán primary cho ảnh còn lại); `UpdateImageMetadataCommand` (altText, orderIndex).
4. **API:** `POST /recipes`, `POST /recipes/{id}/images` (multipart), `PATCH /recipes/{id}/images/{imageId}`, `PATCH /recipes/{id}/images/{imageId}/primary`, `DELETE /recipes/{id}/images/{imageId}`.
5. **UI:** `/dashboard/recipes/new` multi-step wizard (Bước 1 thông tin cơ bản + nutrition, Bước 2 ảnh dùng `ImageUploader`), gallery chọn ảnh chính, toast thành công.

**Commit:** `feat(recipes): complete FR-RCP-003 create draft recipe & FR-RCP-008 recipe image management`

### DEV 3
**Phần 1:** `FR-CAT-003` Tạo, `FR-CAT-004` Cập nhật, `FR-CAT-005` Xóa danh mục [Admin] (Auto Slug + 409).

**Phần 2:**
1. **CQRS:** `CreateCategoryCommand` (name 2–50, không HTML, trùng name → 409 `CATEGORY_NAME_EXISTS`, slug auto + suffix, trả 201 + Location `/api/v1/categories/{slug}`); `UpdateCategoryCommand` (**slug không đổi** khi đổi tên); `DeleteCategoryCommand` (đếm recipe > 0 → 409 `CATEGORY_DELETE_HAS_RECIPES` kèm số lượng; soft delete).
2. **Cache invalidation:** cả 3 command implement `ICacheInvalidator` → xóa `categories:all`.
3. **API:** `POST /categories`, `PUT /categories/{id:guid}`, `DELETE /categories/{id:guid}` với `RequireAuthorization("AdminPolicy")` → 403 cho Author.
4. **UI:** `/dashboard/categories` (Admin only) – bảng danh sách, modal form RHF+Zod (name, description, imageUrl qua `ImageUploader`, orderIndex), confirm dialog xóa, hiển thị lỗi 409 thân thiện; optimistic update TanStack Query.
5. **Test:** integration test 201/403/409 và xóa khi có recipe.

**Commit:** `feat(categories): complete FR-CAT-003/004/005 admin category crud with auto slug & conflict handling`

### DEV 4
**Phần 1:** Setup Hangfire (PostgreSQL storage) + `FR-JOB-001` Welcome Email Job (fire-and-forget).

**Phần 2:**
1. **Infra:** `Hangfire.Core/AspNetCore/PostgreSql`, schema `hangfire` dùng chung connection string; dashboard `/hangfire` bảo vệ bằng `HangfireAdminAuthorizationFilter` (JWT role Admin); retry mặc định `AutomaticRetry(Attempts=3)`.
2. **Email:** `IEmailService`/`MailKitEmailService` (SMTP TLS, config `Smtp__*`, dev → Mailhog :1025); template HTML (tên người dùng, link ứng dụng).
3. **Job:** `WelcomeEmailJob.ExecuteAsync(userId)` với backoff 1'/5'/30' (`[AutomaticRetry(Attempts=3, DelaysInSeconds=new[]{60,300,1800})]`), fail → Failed state + log error; thay stub `IWelcomeEmailScheduler` của Dev 1 bằng `BackgroundJob.Enqueue<WelcomeEmailJob>`.
4. **Integration:** service `hangfire` trong compose chạy worker; kiểm tra: đăng ký trên UI → email xuất hiện ở Mailhog UI :8025, job hiển thị Succeeded trên `/hangfire`.

**Commit:** `feat(jobs): setup hangfire postgres storage & complete FR-JOB-001 welcome email job`

---

## BUỔI 3 – BẢO MẬT TOKEN, NỘI DUNG CHI TIẾT, TÌM KIẾM

### DEV 1
**Phần 1:** `FR-AUTH-004` Refresh Token – Token Rotation & Reuse Detection.

**Phần 2:**
1. **Domain:** `RefreshToken.Revoke(replacedByHash)`, `IsActive` (chưa revoke & chưa hết hạn); repo `RevokeAllForUserAsync(userId)`.
2. **CQRS:** `RefreshTokenCommand` – hash token → không thấy: 401 `AUTH_TOKEN_INVALID`; hết hạn: 401 `AUTH_REFRESH_TOKEN_EXPIRED`; **đã revoke → Reuse Detected**: revoke toàn bộ token family của user, `Log.Warning("SECURITY ALERT…")`, 401 `AUTH_REFRESH_TOKEN_REVOKED`; user inactive/locked → 401; hợp lệ → revoke cũ (`ReplacedByTokenHash`) + phát cặp mới trong 1 transaction.
3. **API:** `POST /api/v1/auth/refresh`.
4. **UI:** `api-client` interceptor: gặp 401 `AUTH_TOKEN_EXPIRED` → gọi refresh **một lần** (mutex/single-flight cho request song song) → retry request; refresh thất bại → logout + redirect `/auth/login`.
5. **Test:** integration test rotation (token cũ dùng lại → cả family bị revoke).

**Commit:** `feat(auth): complete FR-AUTH-004 refresh token rotation with reuse detection`

### DEV 2
**Phần 1:** `FR-RCP-009` CRUD Nguyên liệu + `FR-RCP-010` CRUD Các bước nấu (kèm cập nhật Nutrition).

**Phần 2:**
1. **Domain:** `Recipe.AddIngredient/UpdateIngredient/RemoveIngredient`, `Recipe.AddStep` (StepNumber = max+1), `RemoveStep` **tự renumber** 1..n, unique (RecipeId, StepNumber).
2. **CQRS:** `Add/Update/DeleteIngredientCommand` (name 1–100/200, quantity > 0 nullable, unit ≤50, notes ≤500) và `Add/Update/DeleteStepCommand` (description ≤2000, timerMinutes ≥0) – tất cả qua `RecipeAuthorizationHandler` + invalidate tag `recipes`, `recipe:{slug}`.
3. **API:** `POST/PUT/DELETE /recipes/{id}/ingredients/{ingId?}`, `POST/PUT/DELETE /recipes/{id}/steps/{stepId?}` (201/200/204, 403/404/422).
4. **UI:** Wizard bước 3 "Nguyên liệu" (danh sách động `useFieldArray`, sắp xếp), bước 4 "Các bước" (thêm/xóa/sắp xếp, ảnh minh họa bước qua `ImageUploader`, timer), form Nutrition (6 chỉ số).
5. **Test:** unit test renumber steps.

**Commit:** `feat(recipes): complete FR-RCP-009 ingredients & FR-RCP-010 steps crud with nutrition`

### DEV 3
**Phần 1:** Cấu hình PostgreSQL FTS + `FR-SRCH-001` Tìm kiếm toàn văn bản (tsvector/unaccent) + UI.

**Phần 2:**
1. **DB:** migration `B3_Search_FTS`: cột `SearchVector tsvector`, text search config `vietnamese_unaccent` (copy `simple` + mapping `unaccent`), **TRIGGER** cập nhật `SearchVector = setweight(to_tsvector(cfg, Title),'A') || setweight(to_tsvector(cfg, Description),'B')`, **GIN index** `IDX_Recipe_Search`, backfill dữ liệu seed.
2. **CQRS:** `SearchRecipesQuery` (q ≥ 2 ký tự → 422); sanitize term (loại ký tự đặc biệt tsquery), build `pho:* & bo:*`; `EF.Functions.ToTsQuery(...)` + `Matches`, ORDER BY `Rank` DESC, chỉ Published, trả `relevanceScore`; cache Redis 1 phút theo query (NFR-PERF-003).
3. **API:** `GET /api/v1/recipes/search?q&page&pageSize` (khai báo **trước** route `{slug}` để không bị nuốt).
4. **UI:** `SearchBar` trong header (debounce 300ms, Enter → `/search?q=`), trang `/search` (SSR) hiển thị kết quả + highlight + empty state có gợi ý.
5. **Test:** integration test "pho" tìm được "Phở bò".

**Commit:** `feat(search): complete FR-SRCH-001 postgres full-text search with vietnamese unaccent`

### DEV 4
**Phần 1:** `FR-JOB-002` Image Resize Job + `FR-JOB-003` Recurring Sitemap Generator (02:00 UTC).

**Phần 2:**
1. **FR-JOB-002:** `ImageResizeJob(imageId)` dùng **SixLabors.ImageSharp** tạo medium 800×600 và thumbnail 300×300, upload MinIO, cập nhật `MediumUrl/ThumbnailUrl`; retry 3, fail → ảnh gốc vẫn dùng. Móc vào `UploadRecipeImageCommand` của Dev 2 (`BackgroundJob.Enqueue`).
2. **FR-JOB-003:** `SitemapGeneratorJob` – query Published recipes + categories + trang tĩnh → `sitemap.xml` (`loc, lastmod, changefreq, priority`), upload MinIO; ping `https://www.google.com/ping?sitemap=`; `[AutomaticRetry(Attempts=2)]`, log số URL.
3. **Đăng ký:** `RecurringJob.AddOrUpdate("sitemap", ..., "0 2 * * *", TimeZoneInfo.Utc)`; distributed lock Redis để chỉ 1 instance chạy (NFR-SCALE-001).
4. **FE:** `app/sitemap.xml/route.ts` proxy file từ MinIO, `app/robots.ts` khai báo Sitemap URL; UI hiển thị `ThumbnailUrl` trong `RecipeCard` (fallback ảnh gốc).
5. **Kiểm tra:** trigger thủ công trên `/hangfire`, xác nhận 3 biến thể ảnh trong MinIO console.

**Commit:** `feat(jobs): complete FR-JOB-002 image resize & FR-JOB-003 recurring sitemap generator`

---

## BUỔI 4 – HỒ SƠ, VÒNG ĐỜI CÔNG THỨC, BỘ LỌC, HEALTH

### DEV 1
**Phần 1:** `FR-AUTH-006` Xem hồ sơ + `FR-AUTH-007` Cập nhật hồ sơ & Avatar.

**Phần 2:**
1. **Application:** `ICurrentUser` (lấy `NameIdentifier` từ claims); `GetCurrentUserQuery` → `UserProfileDto {id, fullName, email, userName, avatarUrl, bio, roles, emailConfirmed, createdAt}` (không PasswordHash/SecurityStamp), 404 nếu user bị xóa.
2. **CQRS:** `UpdateProfileCommand` (PATCH partial: fullName 2–100, avatarUrl URL hợp lệ, bio) + Validator; Email/UserName không đổi.
3. **API:** `GET /api/v1/auth/me`, `PATCH /api/v1/auth/me`.
4. **UI:** `/profile` (CSR) – form RHF+Zod, avatar dùng `ImageUploader` (endpoint file của Dev 4) → lưu URL, cập nhật header avatar ngay (optimistic + toast).

**Commit:** `feat(auth): complete FR-AUTH-006 & FR-AUTH-007 view and update profile with avatar`

### DEV 2
**Phần 1:** `FR-RCP-004` Cập nhật công thức (Optimistic Concurrency RowVersion) + `FR-RCP-005` Publish/Unpublish.

**Phần 2:**
1. **Concurrency:** DTO trả `rowVersion` (base64) + header `ETag`; `UpdateRecipeCommand` nhận RowVersion từ `If-Match`/body, gán `OriginalValue` của property RowVersion; bắt `DbUpdateConcurrencyException` → 409 `RECIPE_CONCURRENCY_CONFLICT`.
2. **CQRS Update:** resource-based auth (403 `RECIPE_FORBIDDEN`), `recipe.Update(...)`, **slug không đổi sau khi đã publish** (NFR-SEO-004), evict tag `recipes` + `recipe:{slug}`.
3. **Domain Publish:** `recipe.Publish()` – yêu cầu ≥1 step và ≥1 ingredient → 400 `RECIPE_PUBLISH_INCOMPLETE`; set `PublishedAt`; `Unpublish()` → Draft; idempotent. `PublishRecipeCommand { IsPublish }`.
4. **API:** `PUT /recipes/{id:guid}`, `PATCH /recipes/{id}/publish`, `PATCH /recipes/{id}/unpublish`.
5. **UI:** `/dashboard/recipes/[id]/edit` (tái dùng wizard, gửi rowVersion), dialog xử lý 409 "Dữ liệu đã bị thay đổi – Tải lại / Ghi đè"; nút Publish/Unpublish với badge trạng thái và checklist điều kiện publish.

**Commit:** `feat(recipes): complete FR-RCP-004 update with rowversion concurrency & FR-RCP-005 publish unpublish`

### DEV 3
**Phần 1:** `FR-SRCH-002` Lọc đa tiêu chí + `FR-SRCH-003` Sắp xếp + `FR-SRCH-004` Phân trang offset (UI).

**Phần 2:**
1. **Application:** `RecipeFilterSpec` dùng chung cho `GetRecipesQuery` và `SearchRecipesQuery`: `categoryId`, `difficulty`, `maxCookTime`, `minServings` (AND logic); `SortParser` whitelist `createdAt|title|cookTime|publishedAt` với tiền tố `-` (mặc định `-createdAt`) – chống injection sort field.
2. **Pagination:** page mặc định 1, pageSize mặc định 12, max 50 (422 nếu sai); `PagedResult` đầy đủ `totalCount, totalPages, hasNextPage, hasPreviousPage`; kiểm tra index cho mọi cột WHERE/ORDER BY (EXPLAIN ANALYZE, NFR-PERF-004).
3. **API:** mở rộng query params `GET /recipes` và `GET /recipes/search`; Output Cache vary theo query.
4. **UI:** `FilterPanel` (category, difficulty, thời gian nấu, khẩu phần), `SortSelect`, `Pagination` – **đồng bộ state với URL searchParams** (shareable, SEO-safe), dùng trên `/recipes`, `/search`, `/categories/[slug]`; mobile: filter dạng drawer.

**Commit:** `feat(search): complete FR-SRCH-002/003/004 filtering sorting and offset pagination ui`

### DEV 4
**Phần 1:** `FR-OBS-001` Health Checks `/health`, `/health/live`, `/health/ready` + Health UI Indicator.

**Phần 2:**
1. **Checks:** `AspNetCore.HealthChecks.NpgSql`, `.Redis`, custom `MinioHealthCheck` (ListBuckets) – tag `ready` cho DB+Redis.
2. **Endpoints:** `/health` (tất cả, JSON `{status, entries}` qua `UIResponseWriter`, 503 khi Unhealthy), `/health/live` (predicate false – luôn 200), `/health/ready` (tag ready).
3. **Docker:** `healthcheck:` cho postgres/redis/minio/api trong compose, `depends_on: condition: service_healthy`; Nginx upstream dùng readiness.
4. **UI:** `app/api/health/route.ts` proxy + component `HealthIndicator` (chấm xanh/vàng/đỏ, poll 30s) trên dashboard Admin; trang `/dashboard/system` liệt kê trạng thái từng dependency.

**Commit:** `feat(obs): complete FR-OBS-001 health check endpoints with health ui indicator`

---

## BUỔI 5 – BẢO MẬT, VÒNG ĐỜI CUỐI, SEO, OBSERVABILITY

### DEV 1
**Phần 1:** Security Hardening – Rate Limiting, Policy-based Authorization, Route Guards UI (NFR-SEC-003/005/006).

**Phần 2:**
1. **Rate limiting:** `AddRateLimiter` – policy `auth` sliding window 10 req/phút/IP, `upload` 5 req/phút/IP, global fixed window 100 req/phút/IP; 429 + `Retry-After` + ProblemDetails `RATE_LIMIT_EXCEEDED`, header `X-RateLimit-*`.
2. **Policies:** rà soát mọi endpoint dùng `AuthorPolicy`/`AdminPolicy`/`VerifiedAuthor`; kiểm tra `IsActive=false` → 403 `AUTH_ACCOUNT_DISABLED` (login, refresh); CORS whitelist từ config (không `*`), security headers (CSP, X-Content-Type-Options, HSTS ở Nginx).
3. **UI Route Guards:** `middleware.ts` Next.js bảo vệ `/dashboard/**`, `/profile` (redirect login kèm `callbackUrl`), `/dashboard/categories` chỉ Admin; `/auth/*` redirect nếu đã login; component `<RequireRole>`.
4. **Test:** integration test 429 sau 11 request login, 403 Author vào endpoint Admin.

**Commit:** `feat(security): rate limiting policies, authorization hardening and frontend route guards`

### DEV 2
**Phần 1:** `FR-RCP-006` Archive + `FR-RCP-007` Xóa vĩnh viễn (cascade) + Author Dashboard UI.

**Phần 2:**
1. **Domain/CQRS:** `recipe.Archive()` / `Unarchive()` → `ArchiveRecipeCommand`; recipe Archived bị loại khỏi public listing & search.
2. **Delete:** `DeleteRecipeCommand` – lấy danh sách URL ảnh (original/medium/thumbnail), **hard delete** cascade Steps/Ingredients/Images, `BackgroundJob.Enqueue<IFileStorageService>(s => s.DeleteAsync(url))` cho từng file (retry 3), evict cache.
3. **API:** `PATCH /recipes/{id:guid}/archive`, `DELETE /recipes/{id:guid}` (204).
4. **UI:** `/dashboard` (tổng quan: số recipe theo trạng thái), `/dashboard/recipes` – bảng recipe của tôi, tab Draft/Published/Archived, action Edit/Publish/Archive/Delete (confirm dialog gõ tên recipe), optimistic update + rollback.

**Commit:** `feat(recipes): complete FR-RCP-006 archive & FR-RCP-007 cascade delete with author dashboard`

### DEV 3
**Phần 1:** Trang chủ (ISR), Trang danh mục (ISR) & SEO Metadata – JSON-LD Schema.org Recipe, Open Graph (NFR-SEO-001/002/004).

**Phần 2:**
1. **Trang chủ `/`** (ISR revalidate=3600): hero, recipe nổi bật mới publish, lưới danh mục; `next/image` với `sizes` + priority cho LCP.
2. **JSON-LD:** `lib/seo/recipeJsonLd.ts` sinh `@type: Recipe` (name, description, image, author, datePublished, prepTime/cookTime/totalTime ISO 8601 `PT15M`, recipeYield, recipeIngredient[], recipeInstructions[] HowToStep, nutrition NutritionInformation) nhúng vào `/recipes/[slug]`.
3. **Metadata:** `generateMetadata` cho recipe/category/home: `<title>{Name} | Culinary Blog</title>` ≤60 ký tự, description 150–160 ký tự, OG (`og:image` 1200×630), Twitter `summary_large_image`, canonical slug-URL, `robots: noindex` cho Draft/Archived.
4. **Revalidation:** API route `/api/revalidate` (secret) để backend/FE gọi `revalidateTag` khi publish/update; kiểm tra bằng Google Rich Results Test.

**Commit:** `feat(seo): homepage and category isr pages with json-ld recipe schema and open graph metadata`

### DEV 4
**Phần 1:** `FR-OBS-002` Serilog Structured Logging + CorrelationId + `FR-OBS-003` OpenTelemetry Tracing & Metrics.

**Phần 2:**
1. **FR-OBS-002:** `Serilog.AspNetCore` – sinks Console (JSON), File (rolling daily), Seq (`http://seq:5341`); `CorrelationIdMiddleware` đọc/sinh `X-Correlation-ID`, trả về response, `LogContext.PushProperty` CorrelationId/RequestPath/UserId (CONS-010); `UseSerilogRequestLogging` (method, path, status, elapsed ms).
2. **Behavior:** hoàn thiện `LoggingBehavior` + `PerformanceBehavior` cảnh báo > 500ms; audit log mọi write command (userId + timestamp); EF Core slow query > 100ms.
3. **FR-OBS-003:** OpenTelemetry SDK – instrumentation AspNetCore, HttpClient, EF Core (Npgsql); `ActivitySource` custom; metrics `recipes_created_total`, `recipes_published_total`, request duration histogram, error rate; OTLP exporter → Seq (dev); enrich log với `TraceId/SpanId`.
4. **FE:** `api-client` gửi `X-Correlation-ID` (uuid) mỗi request, error toast hiển thị mã correlation để tra Seq.

**Commit:** `feat(obs): complete FR-OBS-002 serilog correlation logging & FR-OBS-003 opentelemetry tracing`

---

## BUỔI 6 – KIỂM THỬ, TUÂN THỦ, SẴN SÀNG PRODUCTION

### DEV 1
**Phần 1:** E2E Auth Testing & Refactoring (NFR-MAINT-002, NFR-SEC-001/002).

**Phần 2:**
1. **Playwright E2E:** flow register → auto-login → logout; login sai 5 lần → lockout; Google login (mock provider); access token hết hạn → auto refresh trong suốt.
2. **Integration tests** (`WebApplicationFactory` + Testcontainers Postgres/Redis): mọi endpoint `/auth/*` có happy + error case; test reuse detection.
3. **Unit test** Application layer Auth ≥80% coverage (coverlet report).
4. **Refactor:** gom logic phát token vào `AuthTokenIssuer`, loại bỏ trùng lặp, cập nhật XML docs cho Scalar.

**Commit:** `test(auth): e2e and integration test suite for auth module with refactoring`

### DEV 2
**Phần 1:** E2E Recipe Domain Testing & Concurrency Conflict Testing.

**Phần 2:**
1. **Playwright E2E:** create recipe (wizard đủ 4 bước) → upload ảnh → publish → hiển thị public → archive → delete.
2. **Concurrency test:** 2 client cùng load recipe, client A lưu thành công, client B nhận **409** `RECIPE_CONCURRENCY_CONFLICT`; test song song bằng `Task.WhenAll` trong integration test.
3. **Domain unit tests:** Publish thiếu step/ingredient, renumber steps, 1 primary image, slug suffix; resource-based auth (Author khác → 403).
4. **Kiểm tra N+1:** bật EF logging, assert số query cho `GetRecipeBySlug` ≤ ngưỡng; dùng `AsSplitQuery` nếu cần.

**Commit:** `test(recipes): e2e recipe lifecycle and optimistic concurrency conflict tests`

### DEV 3
**Phần 1:** E2E Search/Category Testing + WCAG 2.1 AA Compliance (NFR-USE-001/002/003/004).

**Phần 2:**
1. **Playwright E2E:** search "pho" → thấy "Phở", filter + sort + phân trang giữ URL; admin CRUD category + 409 khi xóa category có recipe.
2. **A11y:** `@axe-core/playwright` quét mọi route (0 violation serious/critical), semantic HTML (`main/nav/article/aside`), ARIA, focus trap trong modal, điều hướng bàn phím (Tab/Enter/Escape), contrast ≥4.5:1.
3. **Responsive:** kiểm tra 320/768/1200px; Lighthouse CI: LCP ≤2.5s, CLS ≤0.1, INP ≤200ms, First Load JS ≤200KB.
4. **Fix** các lỗi phát hiện + ghi báo cáo `docs/a11y-report.md`.

**Commit:** `test(search,a11y): e2e search and category tests with wcag 2.1 aa compliance fixes`

### DEV 4
**Phần 1:** Nginx Reverse Proxy, Redis Cache Invalidation, Docker Multi-stage Build & Load Testing (NFR-PERF-001/002/003, NFR-SCALE-003).

**Phần 2:**
1. **Nginx:** reverse proxy `/api` → api:8080, `/` → frontend:3000, HTTP→HTTPS redirect, HSTS, gzip, cache `_next/static`, `client_max_body_size 5m`, rate limit cơ bản, upstream pool nhiều instance API.
2. **Docker multi-stage:** API `sdk:10.0` → `aspnet:10.0` (non-root user); Frontend `node:22-alpine` build → `output: 'standalone'`; `docker-compose.prod.yml` + env_file, bỏ seq/mailhog.
3. **Redis cache invalidation audit:** kiểm tra toàn bộ `ICacheInvalidator` (category, recipe detail TTL 5', search 1'), đo cache hit rate ≥80%.
4. **Load test k6:** smoke → load 100 VUs → stress; xác nhận p50 ≤150ms, p95 ≤500ms, p99 ≤1000ms; báo cáo `docs/load-test-report.md`; tag `v1.0.0`.

**Commit:** `chore(devops): nginx reverse proxy, multi-stage docker builds, cache invalidation and k6 load tests`

---

## 7. Ma trận truy vết FR → Buổi → Dev

| Mã FR | Tên | Buổi | Dev |
|---|---|---|---|
| FR-AUTH-001/002 | Đăng ký / Đăng nhập local | 1 | 1 |
| FR-AUTH-003/005 | Google OAuth PKCE / Logout | 2 | 1 |
| FR-AUTH-004 | Refresh Rotation + Reuse Detection | 3 | 1 |
| FR-AUTH-006/007 | Xem / Cập nhật Profile | 4 | 1 |
| FR-CAT-001/002 | Danh mục public + cache | 1 | 3 |
| FR-CAT-003/004/005 | CRUD danh mục Admin | 2 | 3 |
| FR-RCP-001/002 | Danh sách / Chi tiết | 1 | 2 |
| FR-RCP-003/008 | Tạo Draft / Quản lý ảnh | 2 | 2 |
| FR-RCP-009/010 | Nguyên liệu / Các bước | 3 | 2 |
| FR-RCP-004/005 | Update RowVersion / Publish | 4 | 2 |
| FR-RCP-006/007 | Archive / Delete cascade | 5 | 2 |
| FR-SRCH-001 | Full-Text Search | 3 | 3 |
| FR-SRCH-002/003/004 | Lọc / Sắp xếp / Phân trang | 4 | 3 |
| FR-FILE-001/002 | MinIO Upload / Delete | 1 | 4 |
| FR-JOB-001 | Welcome Email | 2 | 4 |
| FR-JOB-002/003 | Image Resize / Sitemap | 3 | 4 |
| FR-OBS-001 | Health Checks | 4 | 4 |
| FR-OBS-002/003 | Serilog / OpenTelemetry | 5 | 4 |

**NFR:** SEC-001/002 (B1,B3 Dev1) · SEC-003/005/006 (B5 Dev1) · SEC-004 (B1 Dev4) · SEC-007 (B1 Dev4) · SEO-001..004 (B3 Dev4, B5 Dev3) · USE-001..004 (B6 Dev3) · PERF-001..005 (B4 Dev3, B6 Dev4) · REL-001..003 (B4–B5 Dev4) · MAINT-001..004 (xuyên suốt + B6) · SCALE-001..003 (B3, B6 Dev4).

## 8. Phụ thuộc liên dev (điểm cần phối hợp)
- B1: tất cả phụ thuộc bootstrap Dev 4 (30 phút đầu). Dev 2 cần entity `Category` và `ApplicationUser` → Dev 1 & Dev 3 push entity + migration trước giờ giữa buổi; Dev 2 dùng seed Bogus.
- B2: Dev 2 dùng `SlugHelper` (Dev 3, B1), `IFileStorageService` (Dev 4, B1), JWT (Dev 1, B1). Dev 4 thay stub email của Dev 1.
- B3: Dev 4 móc `ImageResizeJob` vào command upload của Dev 2 (B2); Dev 3 dùng dữ liệu seed Published.
- B4: Dev 3 refactor `GetRecipesQuery` của Dev 2 → thống nhất `RecipeFilterSpec` trước khi code.

## 9. Verification (cuối mỗi buổi & cuối dự án)
1. `docker compose up -d --build` → tất cả service `healthy`; `curl http://localhost:5000/health` = Healthy.
2. `dotnet build -warnaserror && dotnet test` (unit + integration + ArchitectureTests: Domain/Application không reference Infrastructure).
3. `cd frontend && npm run lint && npm run build && npm test`.
4. Test tay các endpoint của buổi qua Scalar `/scalar` + UI tương ứng; kiểm tra log trên Seq có CorrelationId (từ B5).
5. Cuối B6: `npx playwright test` (5 critical flows: register, login, create recipe, publish, search), k6 report, Lighthouse CI, axe report.

