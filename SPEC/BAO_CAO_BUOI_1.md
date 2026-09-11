# BÁO CÁO KẾT QUẢ BUỔI 1 – CULINARY BLOG

> Ngày thực hiện: 11/09/2026 · Căn cứ: `SRS_Culinary_Blog_v1.0.0.pdf`, `KE_HOACH_PHAT_TRIEN_6_BUOI.md`
> Trạng thái: **HOÀN THÀNH** – toàn bộ 4 phần việc chạy end-to-end trên PostgreSQL 16, Redis 7, MinIO thật (Docker Compose), **đã commit Git theo quy trình §1.4** (`main` ← `develop` ← 4 nhánh `feature/b1-dev{k}-*`).

---

## 1. Tóm tắt

| Dev | Mã FR | Chức năng | Trạng thái |
|---|---|---|---|
| Dev 1 | FR-AUTH-001, FR-AUTH-002 | Đăng ký + Đăng nhập Local | ✅ Hoàn thành |
| Dev 2 | FR-RCP-001, FR-RCP-002 | Danh sách + Chi tiết công thức | ✅ Hoàn thành |
| Dev 3 | FR-CAT-001, FR-CAT-002 | Danh sách + Chi tiết danh mục (Redis cache) | ✅ Hoàn thành |
| Dev 4 | Hạ tầng, FR-FILE-001, FR-FILE-002 | Docker Compose 8 service + Upload/Delete MinIO + ImageUploader | ✅ Hoàn thành |
| (thêm) | FR-JOB-001 (một phần) | Welcome Email Job qua Hangfire + MailKit | ✅ Làm sớm (xem mục 4.6) |

**Chất lượng:**

| Hạng mục | Kết quả |
|---|---|
| Backend build | ✅ 0 Warning, 0 Error (TreatWarningsAsErrors + StyleCop + SonarAnalyzer) |
| Backend test | ✅ 44/44 (Domain 14, Application 27, ArchUnit.NET 3) |
| Frontend | ✅ ESLint Airbnb 0 lỗi · `tsc` 0 lỗi · Jest 15/15 · `next build` OK |
| First Load JS | ✅ 103–154 kB (NFR-PERF-005 ≤ 200 kB) |
| Docker | ✅ 9 container Up (8 service SRS + mailhog dev) |

---

## 2. Chi tiết theo từng Dev

### 2.1 DEV 1 – FR-AUTH-001 & FR-AUTH-002

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| DB | `AspNetUsers` (+ DisplayName, AvatarUrl, Bio, IsActive, CreatedAt), `AspNetRoles`…, `RefreshTokens` (TokenHash varchar(64) UNIQUE, ExpiresAt, RevokedAt, ReplacedByTokenHash, CreatedByIp) |
| Domain | `RefreshToken` |
| Application | `RegisterUserCommand` + Validator + Handler, `LoginUserCommand` + Validator + Handler, `AuthResponseFactory`, `PasswordRules`, interface `IIdentityService`, `ITokenService`, `IRefreshTokenRepository`, `IWelcomeEmailScheduler` |
| Infrastructure | `ApplicationUser`, `IdentityService` (UserManager), `JwtTokenService`, `JwtOptions`, cấu hình Identity (password policy, lockout, PBKDF2 100.000 vòng) |
| API | `POST /api/v1/auth/register` (201), `POST /api/v1/auth/login` (200) |
| Frontend | `AuthProvider`/`useAuth` (auth-context.tsx), `LoginForm`, `RegisterForm` (React Hook Form + Zod), trang `/auth/login`, `/auth/register`, header hiển thị user |

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| Đăng ký hợp lệ | 201, trả `accessToken` (15 phút, `expiresIn=900`), `refreshToken` 512-bit, role `Author` |
| Email trùng (khác hoa/thường) | 409 `AUTH_EMAIL_EXISTS` |
| Dữ liệu sai (tên ngắn, email sai, username có dấu cách, mật khẩu yếu) | 422 `VALIDATION_ERROR`, lỗi theo từng field |
| Đăng nhập đúng | 200 |
| Sai mật khẩu lần 1–4 | 401 `AUTH_INVALID_CREDENTIALS` (thông báo chung, chống user enumeration) |
| Sai lần 5 và sau đó (kể cả mật khẩu đúng) | 423 `AUTH_ACCOUNT_LOCKED`, `retryAfterMinutes=15` |
| Email không tồn tại | 401 (cùng thông báo) |
| Mật khẩu trong DB | Tiền tố `AQAAAAIA` = Identity V3 PBKDF2-HMAC-SHA512 |
| Refresh token trong DB | Chỉ lưu SHA-256 hex (64 ký tự) |
| Tên tiếng Việt UTF-8 | Lưu và gửi email đúng ("Trần Thị Bích Ngọc") |
| Email chào mừng | Container `hangfire` gửi thành công → Mailhog |

### 2.2 DEV 2 – FR-RCP-001 & FR-RCP-002

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| DB | `Recipes` (check constraint PrepTime > 0, CookTime ≥ 0, Servings > 0; cột `Nutrition_*` decimal(8,2)), `RecipeSteps` (unique RecipeId + StepNumber), `RecipeIngredients`, `RecipeImages` (partial unique index: 1 ảnh chính/recipe), 8 index `IDX_Recipe_*` |
| Domain | `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion bytea), aggregate `Recipe`, `RecipeStep`, `RecipeIngredient`, `RecipeImage`, owned `RecipeNutrition`, enum `RecipeDifficulty`, `RecipeStatus` |
| Application | `GetRecipesQuery` (+ Validator), `GetRecipeBySlugQuery`, `RecipeSortParser` (whitelist), `IRecipeReadRepository`, DTOs |
| Infrastructure | `RecipeReadRepository` (projection AsNoTracking, split query), `AuditInterceptor`, `DatabaseSeeder` (50 công thức Bogus) |
| API | `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}` + Output Cache Redis ("RecipeList" 15 phút, "RecipeDetail" 60 phút, tag `recipes` + `recipe:{slug}`) |
| Frontend | `/recipes` (SSR, sắp xếp, phân trang, skeleton), `/recipes/[slug]` (ISR 300s: ảnh, badge độ khó, 4 ô thời gian/khẩu phần, nguyên liệu, checklist các bước, bảng dinh dưỡng), `RecipeCard`, `Pagination`, `NutritionTable`, `StepChecklist` |

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| `?page=2&pageSize=5&sort=title` | 200, đủ `totalCount, totalPages, hasNextPage, hasPreviousPage` |
| `?difficulty=Hard&maxCookTime=60` | Chỉ trả recipe Hard có thời gian nấu ≤ 60 |
| `pageSize=100`, `sort=hack` | 422 với thông báo field |
| Tổng số thấy được: Guest / Author1 / Admin | 46 / 47 / 50 (đúng phân quyền Draft) |
| Draft `bun-bo-hue`: Guest / Author khác / Chủ / Admin | 403 / 403 / 200 / 200 |
| Web: slug không tồn tại, Draft | Trang 404 |
| Output Cache | Lần gọi thứ 2 có header `Age` (hit) |
| ISR trang chi tiết | `x-nextjs-cache: HIT`, `Cache-Control: s-maxage=300` |

### 2.3 DEV 3 – FR-CAT-001 & FR-CAT-002

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| DB | `Categories` (Name 100 UNIQUE, Slug 120 UNIQUE, Description, ImageUrl, OrderIndex) + seed 8 danh mục |
| Domain | `Category`, `SlugHelper` (bỏ dấu tiếng Việt, đ→d, suffix -2, -3) |
| Application | `GetCategoriesQuery : ICacheable` (key `categories:all`, TTL 60 phút), `GetCategoryBySlugQuery` (+ Validator), `ICategoryReadRepository`, `CachingBehavior`, `CacheInvalidationBehavior` |
| Infrastructure | `CategoryReadRepository`, `RedisCacheService` (IDistributedCache + JSON, lỗi Redis → fallback DB) |
| API | `GET /api/v1/categories`, `GET /api/v1/categories/{slug}?page&pageSize` |
| Frontend | `/categories` (lưới `CategoryCard`), `/categories/[slug]` (header + lưới công thức + phân trang) |

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| Danh sách | 8 danh mục, sắp theo tên, `recipeCount` chỉ đếm Published |
| Redis | Có key `culinaryblog:categories:all` sau lần gọi đầu |
| `bun-pho?page=1&pageSize=3` | 7 công thức, 3 trang |
| Slug không tồn tại | 404 `CATEGORY_NOT_FOUND` (API và web) |

### 2.4 DEV 4 – Hạ tầng Docker + FR-FILE-001/002

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| Docker | `docker-compose.yml`: nginx, api, frontend, postgres, redis, minio, hangfire (worker), seq + mailhog (dev). Dockerfile multi-stage cho API (non-root `app`) và Frontend (standalone, non-root `nextjs`). `init.sql` (unaccent, pg_trgm, `vietnamese_unaccent`), `nginx.conf` |
| Application | `IFileStorageService`, `ImageFileInspector` (magic bytes JPEG/PNG/WebP/AVIF), `PrefixedReadStream`, `UploadFileCommand`, `DeleteFileCommand` (+ Validator chống path traversal) |
| Infrastructure | `MinioFileStorageService` (AWSSDK.S3), `MinioBucketInitializer` (tạo bucket + policy public-read), `MailKitEmailService`, `WelcomeEmailJob`, Hangfire PostgreSQL |
| API | `POST /api/v1/files/upload`, `DELETE /api/v1/files/{**fileId}` (yêu cầu `AuthorPolicy`) |
| Frontend | `components/ui/ImageUploader.tsx` (kéo-thả, bàn phím, preview, progress bar % qua XHR, nút xóa, ARIA), trang `/dashboard/media` |

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| Upload PNG hợp lệ | 201, key `uploads/{userId}/{guid}.png`, URL public trả 200 `image/png` |
| File giả mạo (header EXE, khai báo PNG) | 400 `FILE_MIME_INVALID` |
| Khai báo `image/gif` | 400 `FILE_MIME_INVALID` |
| File 6MB | 400 `FILE_SIZE_EXCEEDED` |
| Không đăng nhập | 401 |
| Author khác xóa file | 403 |
| Chủ xóa / xóa lần 2 | 204 / 204 (idempotent) – URL sau đó 404 |
| Key có `..` | 422 |
| CORS | `localhost:3000` được phép, origin lạ bị chặn |

---

## 3. Phần làm ĐÚNG với chức năng gốc (SRS)

| Yêu cầu SRS | Đã tuân thủ |
|---|---|
| CONS-001 Clean Architecture 4 tầng | ✅ Domain không có NuGet; ArchUnit.NET kiểm tra Dependency Rule |
| CONS-002 CQRS + MediatR | ✅ Mỗi use case là 1 Command/Query + Handler; pipeline Logging → Validation → Caching → Handler → CacheInvalidation (§6.3) |
| CONS-003 Minimal API, App Router | ✅ Không Controller, không Pages Router |
| CONS-004 JWT 15 phút / 7 ngày, PBKDF2 qua Identity | ✅ |
| CONS-005 REST + RFC 7807 + `/api/v1` | ✅ `type` = Application Error Code (Phụ lục B) |
| CONS-006 EF Core Code-First | ✅ Migration `B1_InitialSchema` |
| CONS-007 Upload ≤ 5MB, 4 MIME, magic bytes | ✅ |
| CONS-008 FluentValidation qua Pipeline | ✅ Không validate trong endpoint |
| CONS-009 Docker multi-stage | ✅ |
| Data model §7.1–7.8 | ✅ Đúng tên bảng, kiểu cột, độ dài, FK (Restrict/Cascade), index, owned `Nutrition_*`, RowVersion bytea |
| FR-AUTH-002 A1 chống user enumeration | ✅ Thông báo chung cho sai email/mật khẩu |
| FR-RCP-001 bước 4 phân quyền xem | ✅ Guest / Author / Admin |
| FR-RCP-002 A2 Draft 403 | ✅ |
| FR-CAT-001 key cache `categories:all`, TTL 60 phút | ✅ |
| FR-FILE-001 tên file `{folder}/{Guid}{ext}`, bucket public-read | ✅ |
| FR-FILE-002 xóa idempotent, trích key từ URL | ✅ |
| NFR-SEC-005 CORS không wildcard | ✅ |
| NFR-SEC-007 không commit secret | ✅ `.env` bị ignore, User Secrets, gitleaks pre-commit |
| NFR-REL-002 Redis lỗi → fallback DB | ✅ `abortConnect=false`, try/catch |
| NFR-USE-002/003/004 | ✅ Label + `aria-*`, lỗi inline, skeleton, toast, progress % |
| SRS §2.6.1 Seed | ✅ Bogus: 50 công thức, 5 tác giả |

---

## 4. Phần làm KHÁC / CHƯA ĐÚNG với yêu cầu gốc và lý do

| # | Yêu cầu gốc | Đã làm | Lý do | Mức độ |
|---|---|---|---|---|
| 4.1 | Bucket `culinary-blog-storage` (yêu cầu Buổi 1) | Bucket `culinary-blog` | SRS §3.5 ghi "culinary-blog", mà nguyên tắc là tuân thủ 100% SRS. Đổi bằng cấu hình `MinIO__BucketName` | Thấp |
| 4.2 | Route `/login`, `/register` | `/auth/login`, `/auth/register` + redirect 308 từ `/login`, `/register` | SRS §5.1 quy định `/auth/*`. Hai đường dẫn đều dùng được | Không ảnh hưởng |
| 4.3 | "Viết dịch vụ `IPasswordHasher` PBKDF2 ≥ 100.000 vòng" | Dùng `IPasswordHasher<ApplicationUser>` có sẵn của Identity, cấu hình `IterationCount = 100_000`, V3 (PBKDF2-HMAC-SHA512) | CONS-004 và NFR-SEC-001 yêu cầu hash "qua ASP.NET Core Identity". Tự viết thuật toán mật mã tăng rủi ro bảo mật | Không ảnh hưởng |
| 4.4 | Tên command `RegisterUserCommand` / `LoginUserCommand` | Đúng tên như yêu cầu | — | ✅ |
| 4.5 | `/categories/[slug]` ISR `revalidate: 3600` (yêu cầu) / 600 (SRS §5.1); `/categories` ISR 3600 | Trang render theo request, dữ liệu fetch revalidate 3600s (`/categories`) và **600s** (`/categories/[slug]`, theo SRS) qua Data Cache | (1) Lúc `next build` trong Docker, API chưa chạy nên không prerender được. (2) Trang chi tiết đọc `?page=`, nên Next bắt buộc render động. Dữ liệu vẫn tái sinh theo chu kỳ, nhưng HTML **không phải ISR thuần**. `/recipes/[slug]` là ISR đầy đủ | Trung bình |
| 4.6 | FR-JOB-001 thuộc Buổi 2 (Dev 4) | Đã làm Welcome Email Job thật (Hangfire + MailKit + retry 1'/5'/30') ngay Buổi 1 | FR-AUTH-001 bước 11 yêu cầu enqueue email, và quy tắc Buổi 1 cấm code giả/placeholder. Buổi 2 Dev 4 còn: Dashboard `/hangfire` bảo vệ Admin | Dời phạm vi |
| 4.7 | Mỗi dev một migration `B1_{Module}` (kế hoạch §1.4) | Một migration `B1_InitialSchema` | Cả 4 phần do một người làm cùng lúc; schema phụ thuộc chéo (Recipe → Category, AspNetUsers) | Thấp |
| 4.8 | FR-AUTH-001 A2/A3 ghi 422; Phụ lục A ghi 400 | 422 cho validation; 400 cho lỗi file | Theo quyết định §1.3 trong kế hoạch (ưu tiên FR chi tiết) | Đã thống nhất |
| 4.9 | FR-RCP-001: response `PagedResult {items, totalCount, …}`; SRS §8: `{data, meta}` | Dùng `PagedResult` của FR | Hai chỗ trong SRS mâu thuẫn; FR chi tiết hơn | Đã thống nhất |
| 4.10 | Đăng xuất | Nút đăng xuất chỉ xóa token phía client | Thu hồi refresh token trên server là FR-AUTH-005 (Buổi 2). Refresh token vẫn còn hiệu lực đến hết 7 ngày hoặc tới Buổi 2 | Chờ Buổi 2 |
| 4.11 | Access token hết hạn | Session phía client tự xóa khi hết 15 phút, người dùng phải đăng nhập lại | Refresh Token Rotation là FR-AUTH-004 (Buổi 3) | Chờ Buổi 3 |
| 4.12 | Lưu JWT | Lưu `localStorage` | Chưa có refresh flow nên cần giữ session qua reload. Rủi ro XSS được giảm bằng CSP ở Buổi 5; có thể chuyển sang in-memory khi có FR-AUTH-004 | Cần xem lại Buổi 3 |
| 4.13 | Ảnh hiển thị qua `next/image` tối ưu | `next/image` với `unoptimized` | Image optimizer chạy trong container frontend không truy cập được `localhost:9000` (URL public của MinIO) | Thấp – xử lý khi có Nginx/CDN Buổi 6 |
| 4.14 | Endpoint `/api/v1/files/*` | Bổ sung (SRS không định nghĩa) | Yêu cầu Buổi 1 chỉ định; SRS chỉ có `/recipes/{id}/images` (FR-RCP-008, Buổi 2) sẽ dùng lại `IFileStorageService` | ✅ Theo yêu cầu |
| 4.15 | Trang chủ `/` ISR | Trang chủ tĩnh giới thiệu | Trang chủ đầy đủ thuộc Buổi 5 (Dev 3) | Theo kế hoạch |
| 4.16 | Cổng Postgres 5432:5432 (§6.5) | `${POSTGRES_HOST_PORT:-5432}:5432`, máy này dùng 5434 | Máy có sẵn PostgreSQL 18 (Windows service) chiếm 5432, container khác chiếm 5433 | Chỉ ảnh hưởng máy này |

---

## 5. Sự cố gặp phải trong Buổi 1 và cách xử lý

| Sự cố | Nguyên nhân | Xử lý |
|---|---|---|
| Postgres báo sai mật khẩu | `.env` do bước cài đặt sinh ra có ký tự CRLF, JWT key bị ngắt dòng | Chuẩn hóa `.env`, đặt lại mật khẩu role `culinary`, cập nhật User Secrets |
| Kết nối nhầm Postgres của Windows | Service `postgresql-x64-18` chiếm 0.0.0.0:5432 | Thêm biến `POSTGRES_HOST_PORT` |
| Slug không tồn tại trả 200 thay vì 404 | `loading.tsx` của `/recipes` bọc cả `[slug]` → response stream với status 200 trước khi `notFound()` | Chuyển trang danh sách và `loading.tsx` vào route group `(list)` |
| Email chào mừng "không đến" | Hangfire.PostgreSql quét hàng đợi mỗi 15 giây | Không phải lỗi; email đến sau vài giây |
| Lỗi StyleCop / Sonar | Rule xung đột với EF Core (private setter), tài liệu tiếng Việt, gom type CQRS | Tắt có chọn lọc trong `.editorconfig`, có ghi chú lý do |

---

## 6. Việc tồn đọng chuyển sang Buổi 2+

- Dashboard `/hangfire` bảo vệ quyền Admin (Dev 4, Buổi 2).
- FR-AUTH-005 thu hồi refresh token khi đăng xuất (Dev 1, Buổi 2).
- Giảm `QueuePollInterval` của Hangfire cho môi trường dev (tùy chọn).
- Integration test (WebApplicationFactory + Testcontainers) cho các endpoint Buổi 1 (Buổi 6).
- ~~Commit Git theo thông điệp của 4 dev~~ → đã hoàn thành: 1 commit bootstrap + 4 commit dev trên 4 nhánh `feature/b1-dev{k}-*`, merge `--no-ff` vào `develop` theo thứ tự Dev4 → Dev1 → Dev3 → Dev2, rồi merge vào `main`. Vì code được viết cùng lúc (xem §4.7), chỉ trạng thái cuối trên `main` là build xanh; các commit trung gian không độc lập biên dịch được.

## 7. Thông điệp commit đề xuất

```
chore: bootstrap clean architecture solution & nextjs app
feat(infra): complete docker compose setup and FR-FILE minio upload component
feat(auth): complete FR-AUTH-001 & 002 register login flow
feat(category): complete FR-CAT-001 & 002 public categories API and UI
feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view
```
