# BÁO CÁO KẾT QUẢ BUỔI 2 – CULINARY BLOG

> Ngày thực hiện: 11/09/2026 · Căn cứ lúc thực hiện: `SRS_Culinary_Blog_v1.0.0.pdf`, `KE_HOACH_PHAT_TRIEN_6_BUOI.md` (nay chỉ còn trong lịch sử git)
> Đối chiếu lại: 20/09/2026 với kế hoạch (nay là `KE_HOACH_PHAT_TRIEN_8_BUOI.md`, mục Buổi 2) và SRS v1.2.0 – xem §8
> Bổ sung: 21/09/2026 – giảng viên đánh số lại lộ trình thành 8 buổi (Buổi 1 là buổi đọc đặc tả, buổi này trước đây là Buổi 1 nay là **Buổi 2**) và giao **yêu cầu thứ 5 về dữ liệu mẫu — yêu cầu mới, khác file SRS v1.2.0**; đã cập nhật SRS lên v1.2.1 (CR-2026-03) – xem §9
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
| Dev 4 (bổ sung 21/09) | SRS v1.2.1 §2.6.1 (CR-2026-03) | **Yêu cầu mới, khác SRS v1.2.0:** dữ liệu mẫu ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước | ✅ Hoàn thành (xem §9) |

**Chất lượng:**

| Hạng mục | Kết quả |
|---|---|
| Backend build | ✅ 0 Warning, 0 Error (TreatWarningsAsErrors + StyleCop + SonarAnalyzer) |
| Backend test | ✅ 44/44 (Domain 14, Application 27, ArchUnit.NET 3); sau yêu cầu bổ sung 21/09: **49/49** (+5 `RecipeSeedCatalogTests`) |
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
| 4.1 | Bucket `culinary-blog-storage` (yêu cầu Buổi 2) | Bucket `culinary-blog` | SRS §3.5 ghi "culinary-blog", mà nguyên tắc là tuân thủ 100% SRS. Đổi bằng cấu hình `MinIO__BucketName` | Thấp |
| 4.2 | Route `/login`, `/register` | `/auth/login`, `/auth/register` + redirect 308 từ `/login`, `/register` | SRS §5.1 quy định `/auth/*`. Hai đường dẫn đều dùng được | Không ảnh hưởng |
| 4.3 | "Viết dịch vụ `IPasswordHasher` PBKDF2 ≥ 100.000 vòng" | Dùng `IPasswordHasher<ApplicationUser>` có sẵn của Identity, cấu hình `IterationCount = 100_000`, V3 (PBKDF2-HMAC-SHA512) | CONS-004 và NFR-SEC-001 yêu cầu hash "qua ASP.NET Core Identity". Tự viết thuật toán mật mã tăng rủi ro bảo mật | Không ảnh hưởng |
| 4.4 | Tên command `RegisterUserCommand` / `LoginUserCommand` | Đúng tên như yêu cầu | — | ✅ |
| 4.5 | `/categories/[slug]` ISR `revalidate: 3600` (yêu cầu) / 600 (SRS §5.1); `/categories` ISR 3600 | Trang render theo request, dữ liệu fetch revalidate 3600s (`/categories`) và **600s** (`/categories/[slug]`, theo SRS) qua Data Cache | (1) Lúc `next build` trong Docker, API chưa chạy nên không prerender được. (2) Trang chi tiết đọc `?page=`, nên Next bắt buộc render động. Dữ liệu vẫn tái sinh theo chu kỳ, nhưng HTML **không phải ISR thuần**. `/recipes/[slug]` là ISR đầy đủ | Trung bình |
| 4.6 | FR-JOB-001 thuộc Buổi 3 (Dev 4) | Đã làm Welcome Email Job thật (Hangfire + MailKit + retry 1'/5'/30') ngay Buổi 2 | FR-AUTH-001 bước 11 yêu cầu enqueue email, và quy tắc Buổi 2 cấm code giả/placeholder. Buổi 3 Dev 4 còn: Dashboard `/hangfire` bảo vệ Admin | Dời phạm vi |
| 4.7 | Mỗi dev một migration `B2_{Module}` (kế hoạch §1.4) | Một migration `B1_InitialSchema` | Cả 4 phần do một người làm cùng lúc; schema phụ thuộc chéo (Recipe → Category, AspNetUsers) | Thấp |
| 4.8 | FR-AUTH-001 A2/A3 ghi 422; Phụ lục A ghi 400 | 422 cho validation; 400 cho lỗi file | Theo quyết định §1.3 trong kế hoạch (ưu tiên FR chi tiết) | Đã thống nhất |
| 4.9 | FR-RCP-001: response `PagedResult {items, totalCount, …}`; SRS §8: `{data, meta}` | Dùng `PagedResult` của FR | Hai chỗ trong SRS mâu thuẫn; FR chi tiết hơn | Đã thống nhất |
| 4.10 | Đăng xuất | Nút đăng xuất chỉ xóa token phía client | Thu hồi refresh token trên server là FR-AUTH-005 (Buổi 3). Refresh token vẫn còn hiệu lực đến hết 7 ngày hoặc tới Buổi 3 | Chờ Buổi 3 |
| 4.11 | Access token hết hạn | Session phía client tự xóa khi hết 15 phút, người dùng phải đăng nhập lại | Refresh Token Rotation là FR-AUTH-004 (Buổi 4) | Chờ Buổi 4 |
| 4.12 | Lưu JWT | Lưu `localStorage` | Chưa có refresh flow nên cần giữ session qua reload. Rủi ro XSS được giảm bằng CSP ở Buổi 6; có thể chuyển sang in-memory khi có FR-AUTH-004 | Cần xem lại Buổi 4 |
| 4.13 | Ảnh hiển thị qua `next/image` tối ưu | `next/image` với `unoptimized` | Image optimizer chạy trong container frontend không truy cập được `localhost:9000` (URL public của MinIO) | Thấp – xử lý khi có Nginx/CDN Buổi 7 |
| 4.14 | Endpoint `/api/v1/files/*` | Bổ sung (SRS không định nghĩa) | Yêu cầu Buổi 2 chỉ định; SRS chỉ có `/recipes/{id}/images` (FR-RCP-008, Buổi 3) sẽ dùng lại `IFileStorageService` | ✅ Theo yêu cầu |
| 4.15 | Trang chủ `/` ISR | Trang chủ tĩnh giới thiệu | Trang chủ đầy đủ thuộc Buổi 6 (Dev 3) | Theo kế hoạch |
| 4.16 | Cổng Postgres 5432:5432 (§6.5) | `${POSTGRES_HOST_PORT:-5432}:5432`, máy này dùng 5434 | Máy có sẵn PostgreSQL 18 (Windows service) chiếm 5432, container khác chiếm 5433 | Chỉ ảnh hưởng máy này |

---

## 5. Sự cố gặp phải trong Buổi 2 và cách xử lý

| Sự cố | Nguyên nhân | Xử lý |
|---|---|---|
| Postgres báo sai mật khẩu | `.env` do bước cài đặt sinh ra có ký tự CRLF, JWT key bị ngắt dòng | Chuẩn hóa `.env`, đặt lại mật khẩu role `culinary`, cập nhật User Secrets |
| Kết nối nhầm Postgres của Windows | Service `postgresql-x64-18` chiếm 0.0.0.0:5432 | Thêm biến `POSTGRES_HOST_PORT` |
| Slug không tồn tại trả 200 thay vì 404 | `loading.tsx` của `/recipes` bọc cả `[slug]` → response stream với status 200 trước khi `notFound()` | Chuyển trang danh sách và `loading.tsx` vào route group `(list)` |
| Email chào mừng "không đến" | Hangfire.PostgreSql quét hàng đợi mỗi 15 giây | Không phải lỗi; email đến sau vài giây |
| Lỗi StyleCop / Sonar | Rule xung đột với EF Core (private setter), tài liệu tiếng Việt, gom type CQRS | Tắt có chọn lọc trong `.editorconfig`, có ghi chú lý do |

### 5.1 Rà soát sau khi commit (14/09/2026, Docker bật) – nhánh `fix/b1-runtime-hardening`

Kiểm tra lại từ bản clone sạch trên GitHub: gitleaks toàn lịch sử **không có secret**; backend build 0 warning, 44/44 test; frontend lint, `tsc`, Jest 15/15, `next build` đều đạt. Các lỗi dưới đây chỉ xuất hiện **khi chạy container**:

| # | Lỗi | Nguyên nhân gốc | Cách sửa | Kiểm chứng |
|---|---|---|---|---|
| 1 | API crash `57P03 the database system is starting up` / `Name or service not known` | Docker Desktop khởi động lại thì mọi container `restart: unless-stopped` chạy song song, bỏ qua `depends_on`. API migrate khi Postgres chưa sẵn sàng; lỗi DNS không được retry của EF coi là tạm thời | `DatabaseReadiness.WaitForDatabaseAsync` (chờ tối đa 2 phút) chạy trước migrate và trước worker Hangfire; bật `EnableRetryOnFailure` cho lỗi tạm thời lúc runtime; healthcheck `pg_isready -h 127.0.0.1` (kiểm qua TCP) | Bật api + hangfire khi Postgres tắt, 20s sau mới bật Postgres → cả hai chờ rồi chạy, **RestartCount 0 → 0** |
| 2 | Nginx trả **502** cho `/api/*` sau khi tạo lại container api | `upstream { server api:8080; }` chỉ phân giải DNS lúc Nginx khởi động → giữ IP cũ | `resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến | api đổi IP 172.30.2.3 → 172.30.2.2, Nginx **không restart** vẫn trả 200 |
| 3 | `libgssapi_krb5.so.2: cannot open shared object file` | Npgsql mặc định thử mã hóa GSS/Kerberos, image aspnet không có thư viện | Connection string thêm `GSS Encryption Mode=Disable` (dự án không dùng Kerberos) | Log sạch |
| 4 | `Overriding HTTP_PORTS '8080'… URLS` | Dockerfile đặt `ASPNETCORE_URLS` đè `ASPNETCORE_HTTP_PORTS` của image | Dùng `ASPNETCORE_HTTP_PORTS=8080` | Log sạch |
| 5 | Khóa DataProtection chỉ nằm trong container | Mất khóa khi tạo lại container; api và hangfire mỗi bên một khóa | `PersistKeysToFileSystem` + volume `dpkeys` dùng chung, `SetApplicationName("CulinaryBlog")` | api và hangfire cùng một file `key-*.xml` |
| 6 | EF cảnh báo `20606` RecipeNutrition optional dependent | Thiết kế có chủ đích: không nhập dinh dưỡng → `Nutrition = null` | Khai báo tường minh `ConfigureWarnings(Ignore(OptionalDependentWithoutIdentifyingPropertyWarning))`, không đổi schema | Log sạch, không cần migration |
| 7 | Container `hangfire` chạy image cũ | Image `culinaryblog-api` build lại nhưng hangfire không được tạo lại | `docker compose up -d` tạo lại cả api và hangfire | Cùng image ID |
| 8 | Pre-commit hook gitleaks **chặn mọi commit trên Windows** khi Docker bật, kèm thông báo sai "Phát hiện secret" | Git Bash đổi `-w /repo` thành `C:/Program Files/Git/repo` → `docker run` lỗi 125; hook coi mọi mã khác 0 là có secret. Buổi 2 không lộ lỗi vì Docker tắt nên hook bỏ qua | `MSYS_NO_PATHCONV=1`, đường dẫn host lấy bằng `pwd -W`; tách thông báo "có secret" (exit 1) với "không chạy được gitleaks" | Hook chạy thật trên commit sửa lỗi này: gitleaks quét phần stage, không có secret, commit thành công |

Trong lúc Postgres **thực sự không truy cập được**, log api/hangfire vẫn có các dòng `A transient exception occurred… will be retried` kèm `Name or service not known` – đó là log của cơ chế retry, đúng hành vi mong muốn, tự hết khi DB lên.

Còn lại một cảnh báo **chấp nhận được ở môi trường dev**: `XmlKeyManager[35] No XML encryptor configured` – khóa DataProtection lưu dạng không mã hóa trên volume. Production cần `ProtectKeysWithCertificate` (Buổi 7, cùng HTTPS).

### 5.2 Rà soát lần 2 (20/09/2026, Docker bật) – commit `6137d3e` trên nhánh `feature/b1-dev4-infra-file`

Kiểm tra toàn bộ: backend build 0 warning / 0 error, 44/44 test; frontend ESLint, `tsc`, Jest 15/15, `next build` đạt; chạy lại stack Docker và thử từng endpoint Buổi 2 (đăng ký/đăng nhập, danh sách/chi tiết công thức và danh mục, upload/xóa file, file giả mạo, path traversal, quyền xóa, CORS, 404). Phát hiện và sửa:

| # | Lỗi | Nguyên nhân gốc | Cách sửa | Kiểm chứng |
|---|---|---|---|---|
| 1 | Ảnh **hợp lệ** gần 5MB upload qua Nginx (`:80/api/...`) bị từ chối **413 trang HTML**; ảnh 6MB cũng nhận HTML thay vì `FILE_SIZE_EXCEEDED` | `client_max_body_size 5m` tính trên **cả body multipart** (file + boundary + header), nên file ≤ 5MB vẫn vượt; trang lỗi mặc định của Nginx là HTML nên Frontend không đọc được | `client_max_body_size 10m` (khớp `RequestSizeLimit` của `/files/upload`); kiểm tra 5MB chính xác vẫn nằm ở API; body > 10MB trả JSON RFC 7807 qua `error_page 413 @payload_too_large` | Qua Nginx: 5MB−100B → **201**; 6MB → **400 `FILE_SIZE_EXCEEDED`**; 11MB → **413 JSON `FILE_SIZE_EXCEEDED`** |
| 2 | Gọi thẳng API với body > 10MB trả 413 có `type` chung chung (`rfc9110#section-15.5.14`) | Request bị chặn ở tầng binding trước handler; `UseStatusCodePages` sinh ProblemDetails mặc định | `AddProblemDetails(CustomizeProblemDetails)`: status 413 → `type = FILE_SIZE_EXCEEDED` (Phụ lục B) | 11MB → 413 `FILE_SIZE_EXCEEDED` |
| 3 | Mỗi lần khởi động khi Postgres chưa sẵn sàng, log api/hangfire tràn **stack trace** `A transient exception occurred…` | `DatabaseReadiness` gọi `DbContext.Database.CanConnectAsync` – chạy **bên trong** execution strategy `EnableRetryOnFailure`, nên mỗi lần thử tự retry thêm 6 lần và ghi đầy đủ exception | Thăm dò bằng `NpgsqlConnection.OpenAsync` trực tiếp (không qua retry của EF); retry lúc runtime giữ nguyên | Tắt Postgres, restart api: log chỉ còn `not reachable yet (attempt 1..3)` → `ready after 4 attempt(s) in 19s`, **RestartCount 0** |

Các điểm lệch SRS v1.2.0 khác (422, lọc Draft theo danh tính, Output Cache, TTL…) **không sửa ở đây** – chúng là nợ kỹ thuật D-1 → D-18 đã được gán chủ và buổi retrofit trong `KE_HOACH_PHAT_TRIEN_8_BUOI.md` §4.1.

---

## 6. Việc tồn đọng chuyển sang Buổi 3+

- Dashboard `/hangfire` bảo vệ quyền Admin (Dev 4, Buổi 3).
- FR-AUTH-005 thu hồi refresh token khi đăng xuất (Dev 1, Buổi 3).
- Giảm `QueuePollInterval` của Hangfire cho môi trường dev (tùy chọn).
- Integration test (WebApplicationFactory + Testcontainers) cho các endpoint Buổi 2 (Buổi 7).
- ~~Commit Git theo thông điệp của 4 dev~~ → đã hoàn thành: 1 commit bootstrap + 4 commit dev trên 4 nhánh `feature/b1-dev{k}-*`, merge `--no-ff` vào `develop` theo thứ tự Dev4 → Dev1 → Dev3 → Dev2, rồi merge vào `main`. Vì code được viết cùng lúc (xem §4.7), chỉ trạng thái cuối trên `main` là build xanh; các commit trung gian không độc lập biên dịch được.

## 7. Lịch sử Git và chi tiết commit theo từng thành viên

Repo: https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao

### 7.1 Vai trò các nhánh (kế hoạch §1.4)

| Nhánh | Vai trò | Ai được đẩy code |
|---|---|---|
| `main` | Nhánh ổn định, **luôn build xanh**. Chỉ nhận merge từ `develop` khi hết một buổi và toàn bộ Definition of Done đạt. Là nhánh mặc định trên GitHub, dùng để nộp/chấm | Không commit trực tiếp; chỉ merge `develop` |
| `develop` | Nhánh **tích hợp**: gom code của 4 dev trong buổi, là nơi phát hiện xung đột (migration, `Program.cs`, DI) trước khi lên `main` | Không commit trực tiếp; nhận merge `--no-ff` từ các nhánh `feature/*` |
| `feature/b1-dev4-infra-file` | Nhánh riêng của **Dev 4** – Buổi 2: hạ tầng Docker + FR-FILE-001/002 | Nguyễn Thăng Thiêng |
| `feature/b1-dev1-auth-001-002` | Nhánh riêng của **Dev 1** – Buổi 2: FR-AUTH-001/002 | Hoàng Bình Quân |
| `feature/b1-dev3-cat-001-002` | Nhánh riêng của **Dev 3** – Buổi 2: FR-CAT-001/002 | Đoàn Hồng Tiến |
| `feature/b1-dev2-rcp-001-002` | Nhánh riêng của **Dev 2** – Buổi 2: FR-RCP-001/002 | Nguyễn Hồng Phúc Thọ |

**Quy ước tên nhánh (cập nhật 21/09/2026):** `{mssv}_{HoTenKhongDau}_buoiso{số}` – MSSV, họ tên đầy đủ viết liền không dấu (viết hoa chữ cái đầu mỗi từ), số buổi theo cách đánh số 8 buổi. *(Quy ước cũ ghi ngày 20/09 là `{mssv}_{tên}_buoi{số}` với số buổi theo cách đánh số 7 buổi.)* Nhánh `feature/b1-*` của Buổi 2 giữ nguyên tên cũ làm lịch sử (đã merge xong, không đổi tên để khỏi hỏng link GitHub); từ Buổi 3 mỗi dev tạo nhánh mới từ `develop` theo tên dưới đây:

| Thành viên | MSSV | Nhánh Buổi 2 | Nhánh Buổi 3 trở đi |
|---|---|---|---|
| Nguyễn Thăng Thiêng (Dev 4) | 2312755 | `2312755_NguyenThangThieng_buoiso2` (đổi tên 21/09 từ `feature/b1-dev4-infra-file` + `fix/b1-runtime-hardening`) | `2312755_NguyenThangThieng_buoiso3` |
| Hoàng Bình Quân (Dev 1) | 2314236 | `feature/b1-dev1-auth-001-002` (đã merge, đã xóa) | `2314236_HoangBinhQuan_buoiso3` |
| Đoàn Hồng Tiến (Dev 3) | 2314291 | `feature/b1-dev3-cat-001-002` (đã merge, đã xóa) | `2314291_DoanHongTien_buoiso3` |
| Nguyễn Hồng Phúc Thọ (Dev 2) | 2312758 | `feature/b1-dev2-rcp-001-002` (đã merge, đã xóa) | `2312758_NguyenHongPhucTho_buoiso3` |

Các buổi sau chỉ đổi số cuối: `2312755_NguyenThangThieng_buoiso4`, `2312755_NguyenThangThieng_buoiso5`…

> **Cập nhật 21/09/2026:** hai nhánh của Dev 4 là `feature/b1-dev4-infra-file` và `fix/b1-runtime-hardening` được **gộp và đổi tên** thành `2312755_NguyenThangThieng_buoiso2` theo quy ước mới, rồi xóa hai tên cũ trên GitHub. Không mất commit nào: `fix/b1-runtime-hardening` (`064f582`) nằm sẵn trong lịch sử của `feature/b1-dev4-infra-file`, nên nhánh mới chứa đủ `b058654`, `9dc3d81`, `064f582`, `6137d3e`, `e841e45` và các commit của yêu cầu bổ sung (§9). Mã commit không đổi nên mọi link commit cũ vẫn dùng được. Các nhánh của thành viên khác không đổi tên.

> **Cập nhật 20/09/2026:** đã **xóa** 3 nhánh `feature/b1-dev1-auth-001-002`, `feature/b1-dev3-cat-001-002`, `feature/b1-dev2-rcp-001-002` (cả local lẫn GitHub) để Dev 1, 2, 3 tự tạo nhánh của mình theo quy ước trên và tự commit phần việc từ Buổi 3. Commit của các nhánh này **không mất** – đã merge vào `develop`/`main`, xem qua link commit ở §7.4–7.6. Nhánh `feature/b1-dev4-infra-file` được giữ lại và đưa lên ngang `main` để nhận commit sửa lỗi `6137d3e` (§5.2). Bảng vai trò nhánh ở trên và số Behind dưới đây phản ánh thời điểm 14/09.

Cột **Behind/Ahead** trên GitHub so với `main`: mọi nhánh đều **Ahead 0** (không còn commit nào chưa merge). **Behind** là số commit `main` có mà nhánh đó chưa có – nhánh merge càng sớm thì Behind càng lớn. Số liệu sau lần merge `fix/b1-runtime-hardening` (commit `f313e95`): Dev 4: 11, Dev 1: 9, Dev 3: 7, Dev 2: 5, `fix/b1-runtime-hardening`: 3, `develop`: 2 – chính là 2 commit merge `develop → main` (`d2d8243`, `f313e95`). Đây là trạng thái bình thường.

```
                 d2d8243                          f313e95
main     ●──────────●────────────────────────────────●   (merge develop → main: lần 1 các FR, lần 2 hardening)
         │          │                                │
develop  └─●──●──●──┴──●─────────────────────────────┘
           │  │  │  │  │
           │  │  │  │  └─ 064f582 fix(infra) hardening – Nguyễn Thăng Thiêng  [fix/b1-runtime-hardening]
           │  │  │  └──── 0fc95d1 feat(recipe)   – Nguyễn Hồng Phúc Thọ  [feature/b1-dev2-rcp-001-002]
           │  │  └─────── fd27a71 feat(category) – Đoàn Hồng Tiến        [feature/b1-dev3-cat-001-002]
           │  └────────── 3d40be6 feat(auth)     – Hoàng Bình Quân       [feature/b1-dev1-auth-001-002]
           └───────────── 9dc3d81 feat(infra)    – Nguyễn Thăng Thiêng   [feature/b1-dev4-infra-file]
b058654 chore: bootstrap – Nguyễn Thăng Thiêng (gốc của mọi nhánh)
```

### 7.2 Tổng quan

| Thành viên | Vai trò | Commit | Nhánh | File | Dòng thêm |
|---|---|---|---|---|---|
| Nguyễn Thăng Thiêng | Dev 4 + trưởng nhóm | `b058654` bootstrap, `9dc3d81` feat(infra), `064f582` fix(infra), `6137d3e` fix(infra) | `main`, `feature/b1-dev4-infra-file`, `fix/b1-runtime-hardening` | 70 + 22 (+ 2 commit sửa lỗi) | 18.279 + 3.438 (+ sửa lỗi) |
| Hoàng Bình Quân | Dev 1 | `3d40be6` feat(auth) | `feature/b1-dev1-auth-001-002` | 24 | 1.343 |
| Đoàn Hồng Tiến | Dev 3 | `fd27a71` feat(category) | `feature/b1-dev3-cat-001-002` | 15 | 536 |
| Nguyễn Hồng Phúc Thọ | Dev 2 | `0fc95d1` feat(recipe) | `feature/b1-dev2-rcp-001-002` | 30 | 1.872 |

> Số dòng của bootstrap lớn vì gồm `package-lock.json` (~12.000 dòng) và tài liệu SPEC. 5 commit merge (`--no-ff`) do chủ repo thực hiện.

### 7.3 Nguyễn Thăng Thiêng – Dev 4 (hạ tầng) và trưởng nhóm

**Commit `b058654` – `chore: bootstrap clean architecture solution & nextjs app`** (trên `main`, là gốc cho 4 nhánh dev)

| Nhóm | Nội dung |
|---|---|
| Cấu hình repo | `.gitignore` (chặn `.env`), `.gitattributes`, `.gitleaks.toml` + `.githooks/pre-commit` quét secret, `.env.example`, `README.md` |
| Tài liệu | `SPEC/`: SRS v1.0.0, kế hoạch 6 buổi, bảng công nghệ – phiên bản, báo cáo buổi 2 |
| Solution .NET | `CulinaryBlog.sln`, `global.json` (SDK 10), `Directory.Build.props` (TreatWarningsAsErrors, StyleCop, Sonar), `.editorconfig`, csproj 4 tầng + 4 project test |
| Domain | `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion), `DomainException` |
| Application | 4 Pipeline Behavior: `LoggingBehavior`, `ValidationBehavior`, `CachingBehavior`, `CacheInvalidationBehavior`; `AppException`, `ErrorCodes` (Phụ lục B); `ICacheService`, `ICacheable`, `ICurrentUser`, `IUnitOfWork`; `PagedResult<T>`; `DependencyInjection` |
| Infrastructure | `CulinaryBlogDbContext`, `AuditInterceptor` (tự điền CreatedAt/UpdatedAt), `BaseEntityConfiguration`, `DependencyInjection` |
| API | `Program.cs` (Minimal API, CORS, OpenAPI + Scalar), `GlobalExceptionMiddleware` (RFC 7807), `CurrentUser`, `appsettings*.json` |
| Test | `LayerDependencyTests` (ArchUnit.NET kiểm tra Dependency Rule) |
| Frontend | Next.js 15 App Router: `package.json`, ESLint Airbnb, Prettier, Jest, `tsconfig`; `layout.tsx`, `providers.tsx` (TanStack Query), `page.tsx`, `not-found.tsx`; `lib/api-client.ts`, `lib/config.ts`, `lib/query-client.ts`; `types/api.ts` |

**Commit `9dc3d81` – `feat(infra): complete docker compose setup and FR-FILE minio upload component`**

| Chức năng | File |
|---|---|
| Docker Compose 8 service SRS + mailhog | `docker-compose.yml`, `backend/Dockerfile`, `frontend/Dockerfile` (multi-stage, non-root) |
| PostgreSQL | `docker/postgres/init.sql` (unaccent, pg_trgm, `vietnamese_unaccent`); migration `B1_InitialSchema` + model snapshot |
| Reverse proxy | `nginx/nginx.conf` (rate limit 100 req/phút, security header, giới hạn 5MB) |
| FR-FILE-001 Upload ảnh | `UploadFileCommand`, `ImageFileInspector` (kiểm magic bytes JPEG/PNG/WebP/AVIF), `PrefixedReadStream`, `IFileStorageService`, `MinioFileStorageService`, `MinioOptions`, `MinioBucketInitializer` |
| FR-FILE-002 Xóa ảnh | `DeleteFileCommand` (idempotent, chống path traversal, chỉ chủ file được xóa) |
| API | `FilesEndpoints`: `POST /api/v1/files/upload`, `DELETE /api/v1/files/{**fileId}` |
| FR-JOB-001 (làm sớm) | `MailKitEmailService`, `WelcomeEmailJob` (Hangfire) |
| Frontend | `ImageUploader.tsx` (kéo-thả, preview, progress %, ARIA), trang `/dashboard/media` |
| Test | `ImageFileInspectorTests` |

**Commit `064f582` – `fix(infra): harden runtime startup, nginx upstream DNS and gitleaks hook`** (nhánh `fix/b1-runtime-hardening`, 14/09) – chi tiết §5.1.

**Commit `6137d3e` – `fix(infra): accept near-5MB uploads through nginx and quiet DB readiness logs`** (nhánh `feature/b1-dev4-infra-file`, 20/09) – chi tiết §5.2.

Link commit: [b058654](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/b058654) · [9dc3d81](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/9dc3d81) · [064f582](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/064f582) · [6137d3e](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/6137d3e). Báo cáo Lab cá nhân (Word, kèm ảnh chụp commit): `SPEC/BAO_CAO_LAB_02_2312755_NguyenThangThieng.docx` (Lab 02 = Buổi 2; trước 21/09 tên là `BAO_CAO_LAB_01_…`), gồm cả hai commit của yêu cầu bổ sung [3efa7ae](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3efa7ae) · [7fb87bf](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/7fb87bf).

### 7.4 Hoàng Bình Quân – Dev 1 (xác thực)

**Commit [`3d40be6`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) – `feat(auth): complete FR-AUTH-001 & 002 register login flow`**

| Chức năng | File |
|---|---|
| FR-AUTH-001 Đăng ký | `RegisterUserCommand` + Validator + Handler (gán role Author, enqueue email chào mừng), `PasswordRules` |
| FR-AUTH-002 Đăng nhập | `LoginUserCommand` + Validator + Handler (khóa 15 phút sau 5 lần sai, thông báo lỗi chung chống user enumeration) |
| Phát hành token | `AuthResponseFactory`, `AuthContracts` (`IIdentityService`, `ITokenService`, `IRefreshTokenRepository`, `IWelcomeEmailScheduler`), `JwtTokenService`, `JwtOptions` (access 15 phút / refresh 7 ngày) |
| Identity | `ApplicationUser` (DisplayName, AvatarUrl, Bio, IsActive), `IdentityService` (UserManager, PBKDF2 100.000 vòng), `IdentityConfigurations` |
| Refresh token | Entity `RefreshToken` (chỉ lưu SHA-256), `RefreshTokenRepository` |
| API | `AuthEndpoints`: `POST /api/v1/auth/register`, `POST /api/v1/auth/login`; `AuthenticationExtensions` (JWT Bearer, `AuthorPolicy`) |
| Frontend | `auth-context.tsx` (`AuthProvider`/`useAuth`), `LoginForm`, `RegisterForm`, `AuthCard`, `schemas.ts` (Zod), trang `/auth/login`, `/auth/register`, `SiteHeader` (hiện user đăng nhập), `FormField` |
| Test | `schemas.test.ts` (Jest) |

### 7.5 Đoàn Hồng Tiến – Dev 3 (danh mục)

**Commit [`fd27a71`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/fd27a71) – `feat(category): complete FR-CAT-001 & 002 public categories API and UI`**

| Chức năng | File |
|---|---|
| FR-CAT-001 Danh sách danh mục | `GetCategoriesQuery : ICacheable` (Redis key `categories:all`, TTL 60 phút), `CategoryDtos` |
| FR-CAT-002 Chi tiết danh mục | `GetCategoryBySlugQuery` + Validator (kèm công thức phân trang, 404 `CATEGORY_NOT_FOUND`) |
| Domain | Entity `Category`, `SlugHelper` (bỏ dấu tiếng Việt, đ → d, hậu tố -2, -3 khi trùng) |
| Dữ liệu | `CategoryConfiguration` (Name/Slug UNIQUE), `CategoryReadRepository` |
| Cache | `RedisCacheService` (IDistributedCache + JSON, Redis lỗi → fallback DB) |
| API | `CategoriesEndpoints`: `GET /api/v1/categories`, `GET /api/v1/categories/{slug}` |
| Frontend | `/categories` (lưới `CategoryCard` + `loading.tsx`), `/categories/[slug]`, `features/categories/api.ts` |
| Test | `SlugHelperTests` |

### 7.6 Nguyễn Hồng Phúc Thọ – Dev 2 (công thức)

**Commit [`0fc95d1`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/0fc95d1) – `feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view`**

| Chức năng | File |
|---|---|
| FR-RCP-001 Danh sách công thức | `GetRecipesQuery` + Validator (lọc độ khó/thời gian, phân trang), `RecipeSortParser` (whitelist sắp xếp), phân quyền xem Draft theo Guest/Author/Admin |
| FR-RCP-002 Chi tiết công thức | `GetRecipeBySlugQuery` (Draft → 403 nếu không phải chủ hoặc Admin) |
| Domain | Aggregate `Recipe`, `RecipeStep`, `RecipeIngredient`, `RecipeImage`, owned `RecipeNutrition`, enum `RecipeDifficulty`, `RecipeStatus` |
| Dữ liệu | `RecipeConfiguration` (check constraint, 8 index, partial unique ảnh chính), `IRecipeReadRepository`, `RecipeReadRepository` (projection AsNoTracking), `RecipeDtos` |
| Dữ liệu mẫu | `DatabaseSeeder` (Bogus: 50 công thức, 5 tác giả, 8 danh mục) |
| API + cache | `RecipesEndpoints`: `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}`; `OutputCachePolicies` (RecipeList 15 phút, RecipeDetail 60 phút) |
| Frontend | `/recipes` (SSR + `loading.tsx`), `/recipes/[slug]` (ISR 300s), `RecipeCard`, `NutritionTable`, `StepChecklist`, `Pagination`, `RecipeGridSkeleton`, `format.ts`, `features/recipes/api.ts` |
| Test | `RecipeTests`, `ValidatorTests` (validator đăng ký + danh sách công thức), `format.test.ts` |

## 8. Đối chiếu Buổi 2 với kế hoạch (rà soát 20/09/2026)

Đối chiếu từng bước ở mục **BUỔI 2** của `KE_HOACH_PHAT_TRIEN_8_BUOI.md` với code trên `main`. Kết luận: **Buổi 2 đạt kế hoạch** – cả 4 lát cắt dọc (DB → API → UI) chạy end-to-end, build/test xanh; các khác biệt đều đã được kế hoạch ghi nhận (khung "Khác biệt giữa kế hoạch và thực tế Buổi 2" hoặc bảng nợ §4.1), trừ 3 hạng mục nhỏ đánh dấu ⚠️ dưới đây.

| Dev | Bước kế hoạch | Thực tế | Kết quả |
|---|---|---|---|
| Dev 4 | Bootstrap 30 phút đầu (solution 4 tầng, `BaseEntity`, DbContext, `AuditInterceptor`, 4 behavior, middleware, `PagedResult`, Next.js + `api-client`) | Commit `b058654` đủ các thành phần | ✅ |
| Dev 4 | Docker Compose 8 service + mailhog, volume, `init.sql`, `.env.example`, User Secrets, README < 5 phút | Đủ; thêm volume `dpkeys` (§5.1) | ✅ |
| Dev 4 | `IFileStorageService`, `MinioFileStorageService` (AWSSDK.S3, path-style), bucket tự tạo + public-read, tên `{folder}/{Guid}{ext}`, xóa idempotent | Đủ | ✅ |
| Dev 4 | Validation ≤ 5MB, MIME whitelist, magic bytes; unit test magic bytes | `ImageFileInspector` + `ImageFileInspectorTests` | ✅ |
| Dev 4 | API tạm `POST /api/v1/files`, `DELETE /api/v1/files` | `POST /files/upload`, `DELETE /files/{**fileId}` – SRS v1.2.0 đã chính thức hóa đúng 2 đường dẫn này (MT-43) | ✅ (khác tên, đã chuẩn hóa) |
| Dev 4 | `ImageUploader` kéo-thả, preview, progress %, callback | Đủ + trang `/dashboard/media` | ✅ |
| Dev 1 | Identity, `RefreshToken`, lockout 5 lần/15 phút, PBKDF2; JWT 15′/7 ngày; register/login 201/200/409/401/423 | Đủ | ✅ (512-bit → nợ D-2) |
| Dev 1 | `IWelcomeEmailScheduler` stub no-op | Đã cắm Hangfire thật (FR-JOB-001 làm sớm) | ✅ vượt kế hoạch |
| Dev 1 | UI login/register RHF + Zod; access token in-memory | Lưu `localStorage` | Nợ **D-12** (B4) |
| Dev 1 | xUnit validator **+ integration test register/login** | Có test validator; project `API.IntegrationTests` còn **rỗng** | ⚠️ Viết khi có harness Buổi 3 (Dev 4) |
| Dev 2 | Schema Recipe đầy đủ, seed 50 recipe/5 tác giả, `GetRecipesQuery`, `GetRecipeBySlugQuery`, Output Cache, UI SSR/ISR 300 | Đủ | ✅ (nợ D-3, D-4, D-6, D-8) |
| Dev 3 | `Category` + seed 8, `RedisCacheService` fallback DB, 2 query, 2 endpoint, UI | Đủ | ✅ (nợ D-5, D-7, D-13) |
| Dev 3 | **Value Object `Slug`** + `SlugHelper` + unit test | Có `SlugHelper` + `SlugHelperTests`; **chưa có VO `Slug`** | ⚠️ Bổ sung khi sinh slug Recipe (Buổi 3) |
| Dev 3 | Component **`CategoryNav`** cho header | Header chỉ có link "Danh mục" | ⚠️ Bổ sung Buổi 3 (Dev 3, cùng CRUD danh mục) |
| Chung | Mỗi dev một migration `B2_{Module}` | Một migration `B1_InitialSchema` | Đã ghi §4.7 |
| Chung | Thông điệp commit theo kế hoạch | Cùng loại/phạm vi, câu chữ ngắn hơn (ví dụ `feat(auth): complete FR-AUTH-001 & 002 register login flow`) | Không ảnh hưởng |

**Tài liệu SPEC sau rà soát:** `SRS_Culinary_Blog_v1.1.0.md` đã xóa – SRS v1.2.0 kế thừa toàn bộ v1.1.0 và đánh dấu mọi thay đổi `[CR-2026-02 / MT-xx]` tại chỗ, nên bản v1.1.0 riêng không còn tác dụng; các tham chiếu tới nó trong SRS v1.2.0, `SRS_MAU_THUAN_VA_GIAI_PHAP.md`, kế hoạch (lúc đó là `KE_HOACH_PHAT_TRIEN_7_BUOI.md`) và README đã được sửa. `Mau_Nop_Bao_Cao_Lab_Ca_Nhan_2026.pdf` là mẫu báo cáo Lab cá nhân để các thành viên tự điền.

---

## 9. Yêu cầu bổ sung của Buổi 2 – dữ liệu mẫu (21/09/2026)

> **Ghi chú: đây là yêu cầu mới, khác với file SRS.** SRS v1.2.0 §2.6.1 chỉ ghi *"Dữ liệu test (seed) được tạo bằng thư viện Bogus với 50 recipe mẫu và 5 tác giả mẫu"*. Giảng viên yêu cầu thêm cho Buổi 2: **CSDL chứa dữ liệu ngẫu nhiên cho ít nhất 20 categories, 100 recipes; mỗi recipe có ít nhất 10 nguyên liệu và ít nhất 5 bước chế biến.** Để code không lệch đặc tả, yêu cầu đã được ghi vào **SRS v1.2.1** qua Change Request **CR-2026-03** (MT-58 trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md`) trước khi hiện thực.

### 9.1 Năm yêu cầu của giảng viên cho Buổi 2

| # | Yêu cầu | Trạng thái | Minh chứng |
|---|---|---|---|
| 1 | Tạo cấu trúc dự án backend theo Clean Architecture | Hoàn thành (11/09) | Commit `b058654`; `LayerDependencyTests` |
| 2 | Cài đặt các gói thư viện cần thiết | Hoàn thành (11/09) | `*.csproj`, `Directory.Build.props`, `CONG_NGHE_VA_PHIEN_BAN.md` |
| 3 | Cài đặt các lớp entities, configuration, DbContext | Hoàn thành (11/09) | `Domain/Entities`, `Persistence/Configurations`, `CulinaryBlogDbContext` |
| 4 | Tạo migration, cài đặt các lớp để tạo dữ liệu ngẫu nhiên | Hoàn thành (11/09) | Migration `B1_InitialSchema`, `DatabaseSeeder` |
| 5 | CSDL có ≥ 20 categories, ≥ 100 recipes; mỗi recipe ≥ 10 nguyên liệu, ≥ 5 bước | **Hoàn thành (21/09) – yêu cầu mới** | Commit `3efa7ae`; §9.2 – §9.4 |

### 9.2 Hiện trạng trước khi làm (đo trên database thật)

| Chỉ số | Yêu cầu | Trước (21/09) |
|---|---|---|
| Danh mục | ≥ 20 | 8 |
| Công thức | ≥ 100 | 50 |
| Nguyên liệu ít nhất / công thức | ≥ 10 | 4 (chọn ngẫu nhiên từ một danh sách 30 nguyên liệu chung, không khớp món) |
| Bước ít nhất / công thức | ≥ 5 | 3 (mô tả Lorem ipsum) |

### 9.3 Đã làm

| Hạng mục | Nội dung |
|---|---|
| Catalog dữ liệu thật | `Infrastructure/Persistence/Seed/Data/categories.json`: 20 danh mục (8 cũ + Cơm & Xôi, Lẩu, Hải sản, Gỏi & Salad, Món cuốn, Món hấp, Món chiên, Ăn vặt, Món Hàn Quốc, Món Nhật Bản, Món Thái, Món Âu). 5 file `recipes-*.json`: 100 món (50 món cũ giữ nguyên tên, slug, danh mục + 50 món mới); mỗi món 10 – 14 nguyên liệu có định lượng, đơn vị, ghi chú và 5 – 6 bước có mô tả, thời gian; thời gian chuẩn bị/nấu, khẩu phần, độ khó đúng với món. Nhúng vào assembly (`EmbeddedResource`) |
| `RecipeSeedCatalog` | Đọc catalog; ngưỡng `MinCategories = 20`, `MinRecipes = 100`, `MinIngredientsPerRecipe = 10`, `MinStepsPerRecipe = 5` |
| `DatabaseSeeder` | Idempotent, tự bù, chạy trong một transaction qua execution strategy. Thêm danh mục thiếu (so theo tên và slug, kể cả bản ghi đã xóa mềm); thêm công thức thiếu theo slug; công thức mẫu cũ (tác giả mẫu, `UpdatedAt` null, dưới ngưỡng) → xóa nguyên liệu/bước sinh ngẫu nhiên và nạp lại nội dung đúng, `StepNumber` đánh lại từ 1. Bogus với seed cố định chỉ sinh tác giả, trạng thái (~85% Published), ngày xuất bản, dinh dưỡng theo khoảng hợp lý từng nhóm món |
| Cache | Có danh mục mới → xóa khóa Redis `categories:all`; dữ liệu thay đổi → `Program.cs` xóa Output Cache tag `recipes` (Redis lỗi không chặn API khởi động) |
| Test | `RecipeSeedCatalogTests` (5 test): đủ ngưỡng, tên/slug duy nhất, món thuộc danh mục có thật, không danh mục rỗng, độ dài khớp cột DB |

**Vì sao không chỉ tăng tham số Bogus:** chọn ngẫu nhiên từ một danh sách nguyên liệu chung sẽ ra "Sinh tố bơ" có nước mắm, "Phở bò" nấu 0 phút, bước nấu bằng chữ Latin. Dữ liệu này được dùng để kiểm thử tìm kiếm tiếng Việt (Buổi 4), đo hiệu năng (Buổi 5) và trình diễn khi bảo vệ, nên phải là công thức có nghĩa.

**Vì sao tự bù thay vì xóa volume:** database của mọi thành viên đang có 50 công thức cũ và seeder cũ bỏ qua bước tạo công thức khi bảng đã có dữ liệu. Seeder mới tự bù khi API khởi động; dữ liệu người dùng tạo hoặc đã chỉnh sửa không bị động tới.

### 9.4 Kiểm chứng

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build` | 0 warning, 0 error |
| `dotnet test` | **49/49** (Domain 14, Application 27, IntegrationTests 5, ArchUnit.NET 3) |
| Database cũ (8 danh mục / 50 công thức, khôi phục từ bản sao lưu) | Log: *"Database seeded: 20 categories, 5 authors, 50 recipes created, 50 recipes repaired"* |
| Database trống (cài mới) | Log: *"20 categories, 5 authors, 100 recipes created, 0 recipes repaired"* |
| Khởi động lại API | Log: *"0 recipes created, 0 recipes repaired"* – không sinh trùng |
| Truy vấn SQL | 20 danh mục · 100 công thức (89 Published) · ít nhất 10 nguyên liệu và 5 bước mỗi công thức · `StepNumber` liên tục 1..N · 0 mô tả Lorem ipsum · 0 danh mục rỗng |
| API qua Nginx | `GET /api/v1/categories` → 20 danh mục; `GET /api/v1/recipes` → `totalCount` 89; trang `/recipes/pho-bo-ha-noi` hiển thị đúng 12 nguyên liệu và 6 bước |

### 9.5 Tài liệu cập nhật cùng yêu cầu

| Tài liệu | Thay đổi |
|---|---|
| `SRS_Culinary_Blog_v1.2.1.md` | Đổi tên từ v1.2.0; §2.6.1 yêu cầu dữ liệu mẫu mới, lịch sử phiên bản 1.2.1, Phụ lục F (CR-2026-03) |
| `SRS_MAU_THUAN_VA_GIAI_PHAP.md` | Thêm MT-58 và CR-2026-03 |
| `KE_HOACH_PHAT_TRIEN_8_BUOI.md` | Đổi tên từ `_7_BUOI`; thêm Buổi 1 (đọc đặc tả), đôn Buổi 1 → 7 cũ thành Buổi 2 → 8 (nội dung giữ nguyên); §4.4 CR-2026-03; mục "Dev 4 – Yêu cầu bổ sung" ở Buổi 2 |
| `README.md` | Lịch 8 buổi, Buổi 1 mới, yêu cầu thứ 5 ở Buổi 2, quy ước nhánh `{mssv}_{HoTenKhongDau}_buoiso{n}` |
| `BAO_CAO_BUOI_2.md` | Đổi tên từ `BAO_CAO_BUOI_1.md`; thêm §9 |
