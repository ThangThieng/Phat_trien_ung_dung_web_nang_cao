# BÁO CÁO KẾT QUẢ BUỔI 3 — DEV 4

**Dev 4:** Nguyễn Thăng Thiêng — MSSV 2312755 — Trưởng nhóm / Kỹ sư Hạ tầng & DevOps
**Nhánh:** `2312755_NguyenThangThieng_buoiso3` (tách từ `develop`)
**Phạm vi:** commit nền **D-11 (422 → 400)** · **FR-JOB-001** Hangfire Dashboard sau Nginx Basic Auth · **NFR-MAINT-002** Integration Test Harness (Testcontainers) dùng chung cả nhóm
**Tài liệu bám sát:** `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md` (Buổi 3 — Dev 4) và `SPEC/SRS_Culinary_Blog_v1.2.1.md` (MT-08, MT-39, MT-21, MT-34)

---

## 1. Tóm tắt các công việc đã hoàn thành

| # | Hạng mục | Yêu cầu trong kế hoạch | Kết quả |
|---|----------|------------------------|---------|
| A | **Commit nền D-11 (422 → 400)** | Middleware, `.ProducesValidationProblem`, Frontend, test, chặn 422 quay lại | ✅ Hoàn thành — mã 422 không còn tồn tại trong `backend/src`, có test kiến trúc chặn tái phạm |
| B | **FR-JOB-001 — Hangfire Dashboard an toàn** | Basic Auth ở Nginx + header gate ở API (2 lớp), `.htpasswd` không commit | ✅ Hoàn thành — kiểm chứng thật 4/4 kịch bản qua `docker compose` |
| C | **Integration Test Harness (NFR-MAINT-002)** | `CulinaryBlogApiFactory` + Testcontainers (PostgreSQL 16, Redis 7, MinIO) + `CreateClientAs` + Respawn | ✅ Hoàn thành — 44 integration test, 43 xanh, 1 Skip có chủ đích (MT-34) |
| D | **Trả nợ test Buổi 2** | Register/Login/Recipes/Categories/Files | ✅ Hoàn thành — mọi endpoint của Buổi 2 có ≥ 1 happy path + ≥ 1 error case |
| E | **Tồn đọng Buổi 2 §6 — poll interval** | Giảm xuống 1 giây ở môi trường dev | ✅ Hoàn thành — `Hangfire:PollIntervalSeconds` = 1 ở `appsettings.Development.json` |

---

## 2. Chi tiết A — Commit nền D-11 (422 → 400, MT-08)

SRS v1.2.1 §3 (MT-08) chốt: **mã 422 bị loại bỏ hoàn toàn khỏi hệ thống, mọi lỗi validation/input trả 400.**
Làm trước tiên vì mọi endpoint mới của Buổi 3 (Dev 1, Dev 2, Dev 3) đều trả lỗi validation qua cùng một middleware.

**Backend**

- `GlobalExceptionMiddleware.cs`: nhánh `ValidationException` (bao gồm lỗi Identity, vì `IdentityService.ThrowIfFailed` chuyển `IdentityError` thành `ValidationFailure`) và nhánh `DomainException` đổi từ `Status422UnprocessableEntity` → `Status400BadRequest`; xóa chú thích *"Quyết định dự án: lỗi validation → 422"*, thay bằng dẫn chiếu MT-08.
- `AuthEndpoints.cs` (2 chỗ), `RecipesEndpoints.cs` (1 chỗ): `.ProducesValidationProblem(...)` đổi sang `Status400BadRequest` để tài liệu Scalar/OpenAPI khớp hành vi thật.
- `IdentityService.cs`: cập nhật XML doc (`→ HTTP 400 VALIDATION_ERROR`).

**Frontend**

- `lib/api-client.ts`: thêm helper dùng chung `mapProblemDetailsToForm(error, fields, setError)` — nhận `ApiError`, chỉ xử lý **status 400**, ánh xạ `errors{}` của RFC 7807 vào đúng ô form, trả `true` khi đã xử lý.
- `LoginForm.tsx`, `RegisterForm.tsx`: bỏ điều kiện `error.status === 422` viết tay (kèm hàm `isServerField` trùng lặp trong RegisterForm), thay bằng lời gọi helper.

**Chặn tái phạm**

- `tests/CulinaryBlog.ArchitectureTests/ValidationStatusCodeTests.cs` — quét toàn bộ `backend/src/**/*.cs`, fail nếu chuỗi `Status422UnprocessableEntity` xuất hiện trở lại. Quét **mã nguồn** chứ không quét IL vì `StatusCodes.Status422UnprocessableEntity` là hằng số `int`; sau khi biên dịch nó chỉ còn con số 422 lẫn giữa mọi số nguyên khác nên không thể phân biệt ở mức assembly.

**Kiểm chứng thật (qua Nginx, stack đang chạy):**

```
POST /api/v1/auth/register  {"fullName":"A","email":"khong-phai-email","password":"123", ...}
→ HTTP 400
{"type":"VALIDATION_ERROR","title":"Dữ liệu không hợp lệ","status":400,
 "detail":"Một hoặc nhiều trường không hợp lệ. Xem \"errors\".",
 "instance":"/api/v1/auth/register",
 "errors":{"fullName":["Họ tên phải từ 2 đến 100 ký tự."],
           "email":["Email không đúng định dạng."],
           "password":["Mật khẩu tối thiểu 8 ký tự.", ...]}}
```

---

## 3. Chi tiết B — Hangfire Dashboard bảo vệ hai lớp (FR-JOB-001, MT-39)

Bản thân Welcome Email Job đã chạy thật qua Hangfire + MailKit từ Buổi 2; buổi này chỉ hoàn thiện phần **bảo vệ Dashboard**.

**Lớp 1 — HTTP Basic Auth tại Nginx**

- `nginx/nginx.conf` được chuyển thành `nginx/templates/nginx.conf.template` (giữ nguyên toàn bộ cơ chế của Buổi 2: resolver DNS động `127.0.0.11 valid=10s`, `proxy_pass $api_upstream`, `X-Forwarded-*`, `X-Correlation-ID`, rate limit, `error_page 413`). Image `nginx:alpine` tự chạy `envsubst` lúc khởi động với `NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx` và **`NGINX_ENVSUBST_FILTER=^HANGFIRE_`** — giới hạn phạm vi thay thế đúng các biến `HANGFIRE_*`, nên mọi biến của chính Nginx (`$host`, `$remote_addr`, `$binary_remote_addr`…) được giữ nguyên.
- Thêm block `location /hangfire` **đứng trước** block regex `location ~ ^/(health|scalar|openapi)` (đã bỏ `hangfire` khỏi regex đó). Thứ tự này là bắt buộc: Nginx chỉ dùng prefix match khi **không có regex nào khớp**, nên nếu để `hangfire` nằm trong regex cũ thì block Basic Auth sẽ không bao giờ được chọn.
  ```nginx
  location /hangfire {
      auth_basic           "Hangfire Dashboard Restricted";
      auth_basic_user_file /etc/nginx/.htpasswd;
      proxy_set_header     X-Hangfire-Gate ${HANGFIRE_GATE_SECRET};
      proxy_pass           $api_upstream;
      ...
  }
  ```

**Lớp 2 — header gate tại API**

- `API/Extensions/NginxGateDashboardFilter.cs` (`IDashboardAuthorizationFilter`): chỉ cho qua khi header `X-Hangfire-Gate` khớp `Hangfire:DashboardGateSecret`, so sánh bằng `CryptographicOperations.FixedTimeEquals`. Secret rỗng ⇒ **từ chối** (fail closed), để cấu hình thiếu không biến dashboard thành công khai.
- `Program.cs`: `app.MapHangfireDashboard(...)` chỉ chạy ở container `api` (container worker đã `return` trước đó), `DisplayStorageConnectionString = false`.
- **Không dùng** `LocalRequestsOnlyAuthorizationFilter` mặc định của Hangfire: request đi qua reverse proxy mang IP container Nginx chứ không phải loopback, nên filter đó sẽ từ chối **mọi** truy cập hợp lệ.

**Quản lý credential (NFR-SEC-007)**

- `nginx/.htpasswd` nằm trong `.gitignore` (hai dòng: `nginx/.htpasswd` và `.htpasswd`) — **không bao giờ commit**, gitleaks pre-commit hook vẫn quét như cũ.
- Vì file bị gitignore nên mỗi máy phải tự sinh một lần, nếu không Docker sẽ tạo một **thư mục rỗng** ở vị trí mount và Nginx báo `auth_basic_user_file ... is a directory`. Đã bổ sung hai script (dùng `htpasswd -B`, bcrypt, chạy qua image `httpd:alpine` nên không ai phải cài `apache2-utils`):
  - `nginx/generate-htpasswd.sh` (Linux/macOS/Git Bash)
  - `nginx/generate-htpasswd.ps1` (Windows PowerShell)
- `.env.example` bổ sung `HANGFIRE_GATE_SECRET`, `HANGFIRE_DASHBOARD_USER`, `HANGFIRE_DASHBOARD_PASSWORD` (giá trị giả/rỗng).

**Kiểm chứng thật — `docker compose --profile dev up -d --build`, 9 container Up:**

| # | Kịch bản | Kỳ vọng | Kết quả thực tế |
|---|----------|---------|-----------------|
| 1 | `GET http://localhost/hangfire` (không Basic Auth) | 401 + hỏi mật khẩu | **HTTP 401**, `WWW-Authenticate: Basic realm="Hangfire Dashboard Restricted"` |
| 2 | `GET http://localhost/hangfire` (sai mật khẩu) | 401 | **HTTP 401** |
| 3 | `GET http://localhost/hangfire` (đúng Basic Auth) | 200, vào được dashboard | **HTTP 200**, `<title>Overview – Culinary Blog – Background Jobs</title>` |
| 4 | `GET http://localhost:5000/hangfire` (**bỏ qua Nginx**) | 401 | **HTTP 401** — lớp header gate chặn |

Job nền vẫn chạy thật sau thay đổi:

```
POST /api/v1/auth/register (qua Nginx) → HTTP 201
Hangfire /jobs/succeeded  → WelcomeEmailJob.ExecuteAsync   (Succeeded)
Mailhog  /api/v2/messages → To: Kiem Chung Buoi 3 <buoi3-…@culinaryblog.test>
                            Subject: Chào mừng bạn đến với Culinary Blog
```

**Tồn đọng Buổi 2 §6 — poll interval:** `Hangfire:PollIntervalSeconds` (mặc định 15) áp cho **cả** `PostgreSqlStorageOptions.QueuePollInterval` (job fire-and-forget như Welcome Email) **và** `BackgroundJobServerOptions.SchedulePollingInterval` (các lần retry 1′/5′/30′). `appsettings.Development.json` đặt **1 giây**; production giữ 15 giây.

---

## 4. Chi tiết C — Integration Test Harness dùng chung (NFR-MAINT-002)

### 4.1 `CulinaryBlogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime`

- Khởi động **ba container thật** bằng Testcontainers: `postgres:16-alpine`, `redis:7-alpine`, `quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z` — đúng bộ phụ thuộc runtime của `docker-compose.yml` (SRS §6.5).
- Ghi đè cấu hình qua `UseSetting`: connection string Postgres/Redis, endpoint + bucket MinIO riêng (`culinary-blog-tests`), `Jwt:SigningKey` của test, `Database:MigrateOnStartup=false`, `Seed:Enabled=false` (test tự kiểm soát thời điểm migrate/seed), `Hangfire:ServerEnabled=false`.
- `Program.cs` bổ sung `public partial class Program` (kèm constructor `protected`) để `WebApplicationFactory<Program>` tham chiếu được.
- Thay `IBackgroundJobClient` bằng `FakeBackgroundJobClient` ghi lại lời gọi: BackgroundJobServer chạy song song sẽ khiến kết quả test **không tất định**, trong khi thứ cần kiểm chứng ở tầng API chỉ là *"endpoint có enqueue đúng job với đúng tham số"* — bản thân `WelcomeEmailJob` đã chạy thật (mục 3).
- **`IFileStorageService` KHÔNG bị thay bằng bản giả** (khác gợi ý trong kế hoạch, theo yêu cầu bổ sung Testcontainers MinIO): test upload/xóa chạy trên MinIO thật, vì chính đường đi tới S3 — object key do server sinh, bucket tồn tại, URL public tải về được — là thứ cần kiểm chứng. Bucket được tạo tường minh trong `InitializeAsync` thay vì chờ `MinioBucketInitializer` (BackgroundService chạy bất đồng bộ với test đầu tiên).
- **Respawn 7.0.0**: `ResetDatabaseAsync()` xóa dữ liệu schema `public` (giữ `__EFMigrationsHistory`, không đụng schema `hangfire`) rồi nạp lại bộ dữ liệu cố định. Đang được `AuthEndpointsTests` dùng ở `InitializeAsync` — lớp duy nhất tạo tài khoản và làm khóa tài khoản.
- **`CreateClientAs(string roles, string? userId = null)`** (kèm overload nhận `Guid`): phát **JWT thật** qua `ITokenService` của ứng dụng, hỗ trợ Guest / Author / Admin / nhiều role (`"Author,Admin"`), để Dev 1–3 viết integration test mà không phải đăng nhập thật trong từng test.
- `TestDataSeeder`: bộ dữ liệu **cố tình nhỏ và tất định** (3 user, 2 danh mục, 3 công thức Published + 1 Draft) thay vì `DatabaseSeeder` 100 công thức sinh bằng Bogus — test khẳng định được *"đúng 3 công thức Published"* thay vì *"ít nhất 1"*, và mỗi lần Respawn reset nạp lại trong vài mili giây. Độ đầy đủ của bộ seed thật vẫn do `RecipeSeedCatalogTests` (đã có từ Buổi 2) đảm nhiệm.

> ⚠️ **Ghi nhận cho D-18 (Buổi 4):** Testcontainers **không** chạy `docker/postgres/init.sql`. Migration `B1_InitialSchema` đã khai báo sẵn `unaccent` + `pg_trgm` (annotation `Npgsql:PostgresExtension`) nên harness chạy được; **`CREATE TEXT SEARCH CONFIGURATION vietnamese_unaccent` hiện mới chỉ nằm trong `init.sql`**, chưa có trong migration. Buổi 4 khi làm FTS bắt buộc phải đưa nó vào migration, nếu không integration test sẽ đỏ.

### 4.2 Bộ test — 44 test (43 Passed, 1 Skipped)

| Nhóm | File | Số test | Nội dung |
|------|------|---------|----------|
| Auth | `Auth/AuthEndpointsTests.cs` | 8 | Register 201 + enqueue Welcome Email · 409 email trùng · **400** validation (D-11) · 400 chỉ ở ô `email` · 400 fullName quá dài · Login 200 · Login 401 **thông điệp giống hệt nhau** giữa sai mật khẩu và email không tồn tại (chống user enumeration) · Login 400 mật khẩu rỗng · **Lockout: sai 5 lần → 423 + `retryAfterMinutes`, mật khẩu đúng vẫn 423** |
| Files | `Files/FilesEndpointsTests.cs` | 12 | Upload 201 (key do server sinh, tên file người dùng không lọt vào key, tải về được qua URL public) · 401 không token · **magic bytes**: file văn bản đội lốt PNG → 400 · JPEG khai báo PNG → 400 · `image/gif` → 400 · WebP hợp lệ → 201 · Delete 204 + object biến mất · Delete idempotent · **403 khi Author khác xóa file của tôi** (file còn nguyên) · Admin xóa được file của người khác · **path traversal** (2 biến thể `..`) → 400 · 401 không token |
| Categories | `Content/CategoriesEndpointsTests.cs` | 3 | Danh sách + `recipeCount` **chỉ đếm Published** (2, không phải 3) · Chi tiết + recipes phân trang · 404 `CATEGORY_NOT_FOUND` |
| Recipes | `Content/RecipesEndpointsTests.cs` | 8 | Guest chỉ thấy Published · lọc theo category + difficulty · **400** với `page=0`, `pageSize=500`, `sort` sai (D-11) · chi tiết có steps/ingredients · 404 · Draft với Guest → 403 · Draft với chủ sở hữu → 200 |
| Hangfire | `Jobs/HangfireDashboardTests.cs` | 4 | Không header gate → 401 · **JWT Admin cũng không vào được** (403 — đúng MT-39: dashboard nằm ngoài hệ thống phân quyền Bearer) · sai secret → 401 · đúng secret (như Nginx gắn) → 200 |
| Seed | `Seed/RecipeSeedCatalogTests.cs` | 8 | (đã có từ Buổi 2, giữ nguyên) |
| **MT-34** | `Content/RecipeCacheIsolationTests.cs` | 1 | **`[Fact(Skip = ...)]` có chủ đích** — xem mục 4.3 |

### 4.3 Test tái hiện lỗ hổng MT-34 (Skip có chủ đích)

`GetRecipes_AfterAdminRequest_DoesNotLeakDraftsToGuest`: Admin gọi `GET /api/v1/recipes?page=1&pageSize=50`, sau đó Guest gọi **đúng URL đó** và không được nhận nội dung của Admin.

Cache của danh sách công thức được đánh key theo query string, nhưng **nội dung** lại phụ thuộc danh tính người gọi (`RecipeVisibility` trong `GetRecipesQuery`). Test được viết **ngay hôm nay** và đánh dấu Skip để lỗ hổng nằm trong bộ test thay vì nằm trong trí nhớ của ai đó. Retrofit **D-4 + D-6 ở Buổi 6** (gỡ lọc theo danh tính khỏi `RecipeReadRepository`, xóa Output Cache) sẽ bỏ `Skip` và test này phải chuyển sang xanh.

Tương tự, `GetRecipeBySlug_DraftAsGuest_Returns403ForNow` ghi rõ trong XML doc rằng Buổi 6 sẽ đổi 403 → 404 và test phải được cập nhật **cùng commit đó**.

---

## 5. Sửa hạ tầng phát sinh — image MinIO không còn kéo được

> Đây là **sự cố phát sinh ngoài phạm vi kế hoạch Buổi 3**, phát hiện trong lúc dựng integration test harness. Mục này ghi lại đầy đủ dữ kiện, các phương án đã cân nhắc kèm lợi/hại, và lý do chọn — để buổi sau không ai phải điều tra lại từ đầu.

### 5.1 Hiện tượng

Testcontainers không khởi động được container MinIO:

```
Docker.DotNet.DockerApiException: Docker API responded with status code='NotFound',
response='{"message":"pull access denied for minio/minio,
           repository does not exist or may require 'docker login'"}'
```

Điều tra cho thấy đây **không phải lỗi của test**, mà là `docker-compose.yml` từ Buổi 1 cũng đã hỏng ngầm.

### 5.2 Dữ kiện đã kiểm chứng

| Kiểm tra | Kết quả |
|---|---|
| `docker pull minio/minio:latest` | ❌ `pull access denied ... repository does not exist` |
| `docker pull minio/minio:RELEASE.2025-09-07T16-13-09Z` | ❌ cùng lỗi — **không phải** do dùng tag `latest` |
| `GET https://hub.docker.com/v2/repositories/minio/minio/` | ❌ `{"message":"object not found"}` — repository đã bị **gỡ hẳn** khỏi Docker Hub, không phải rate-limit hay cần `docker login` |
| `docker pull quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z` | ✅ kéo được |
| `docker manifest inspect quay.io/minio/minio:latest` | ❌ không tồn tại — quay.io **chỉ** phát hành tag dạng `RELEASE.*` |
| `docker manifest inspect quay.io/minio/minio:RELEASE.2025-02-28T09-55-16Z` / `…2025-01-20T14-49-07Z` | ✅ các bản cũ cũng có trên quay.io |
| Console `http://localhost:9001` trên bản đã ghim | ✅ HTTP 200, `<meta content="MinIO Console" name="description">` — **không mất tính năng nào** |
| `docker compose logs minio` | `Version: RELEASE.2025-09-07T16-13-09Z`, `API: :9000`, `WebUI: :9001` |

**Điểm mấu chốt:** máy đang phát triển vẫn chạy được **chỉ vì image cũ còn nằm trong cache Docker local**. Một thành viên clone repo về máy sạch — hoặc máy chấm bài — sẽ thất bại ngay ở service `minio` khi `docker compose up`, và integration test MinIO cũng không chạy được ở đó. Đây là loại hỏng hóc ẩn, chỉ lộ ra đúng vào lúc demo hoặc nộp bài.

### 5.3 Bốn phương án đã cân nhắc

#### Phương án A — quay.io + ghim tag `RELEASE.2025-09-07T16-13-09Z` ✅ **ĐÃ CHỌN**

| | |
|---|---|
| **Lợi** | • quay.io là registry **chính thức** MinIO còn phát hành → công nghệ **không** bị thay thế, vẫn đúng SRS §3.5 và §5.3.<br>• Không phải sửa biến môi trường, healthcheck `mc ready local`, hay bất kỳ dòng code nào — chỉ đổi tên image.<br>• Đúng **chính xác** release đang chạy trên máy phát triển, nên hành vi không thay đổi một chút nào; Console `:9001` vẫn nguyên vẹn.<br>• Ghim tag ⇒ `docker compose up` và integration test cho kết quả như nhau ở mọi máy, mọi ngày chạy — điều kiện cần của Definition of Done §2.4.<br>• Diff nhỏ nhất trong bốn phương án: 3 dòng ở 3 file. |
| **Hại** | • Thêm một registry thứ hai (quay.io) bên cạnh Docker Hub vào chuỗi phụ thuộc — nếu quay.io đổi chính sách thì sẽ phải sửa lại lần nữa.<br>• quay.io không có tag `latest`, nên muốn lên bản vá bảo mật phải **nâng tag thủ công**; đổi lại là không bao giờ bị một bản mới làm đỏ test bất ngờ. |

#### Phương án B — quay.io + ghim digest `@sha256:…`

| | |
|---|---|
| **Lợi** | • Mọi ưu điểm của A, cộng tính bất biến tuyệt đối: một tag vẫn có thể bị đẩy đè bằng nội dung khác, còn digest thì không. |
| **Hại** | • Dòng cấu hình dài và không đọc được bằng mắt — người sau không biết đang dùng bản nào nếu không tra.<br>• Mỗi lần nâng phiên bản phải tra digest thủ công.<br>• Image là **manifest list đa kiến trúc** (đã kiểm: `application/vnd.docker.distribution.manifest.list.v2+json`), rất dễ ghim nhầm digest của một nền tảng cụ thể thay vì của manifest list → máy kiến trúc khác không chạy được.<br>• Lợi ích cận biên so với A rất nhỏ ở quy mô bài tập môn học; chỉ đáng làm khi nhóm dựng CI thật. |

#### Phương án C — giữ nguyên `minio/minio:latest`, không sửa gì

| | |
|---|---|
| **Lợi** | • Diff của Buổi 3 gọn đúng phạm vi kế hoạch giao cho Dev 4.<br>• Không đụng vào cấu hình đang chạy ổn trên máy phát triển. |
| **Hại** | • **Repo hỏng với mọi máy chưa có image trong cache** — đây là cái giá quyết định.<br>• Integration test harness (thành quả chính của buổi này) sẽ không chạy được ở máy khác, tức là NFR-MAINT-002 coi như không hoàn thành.<br>• Nợ kỹ thuật loại "bom hẹn giờ": nó nổ vào lúc tệ nhất và khi đó không ai còn nhớ nguyên nhân. |

#### Phương án D — đổi sang image khác (`bitnami/minio`) hoặc S3 server khác

| | |
|---|---|
| **Lợi** | • `bitnami/minio` nằm trên Docker Hub nên về hình thức giữ được một registry duy nhất. |
| **Hại** | • Là bản **đóng gói lại của bên thứ ba**, không phải kênh phát hành chính thức của MinIO → rủi ro bị gỡ/đổi chính sách y hệt, không giải quyết gốc vấn đề.<br>• Khác biến môi trường, khác UID chạy, khác entrypoint → phải sửa thêm healthcheck và cấu hình volume, tức là thay đổi lớn hơn nhiều so với A trong khi lợi ích nhỏ hơn.<br>• Đổi hẳn sang S3 server khác thì **trái nguyên tắc không thay công nghệ do SRS quy định** (§3.5, §5.3) — nếu thực sự cần thì phải qua Change Request, không tự quyết trong một buổi làm bài. |

### 5.4 Quyết định và cách ghi nhận

**Chọn Phương án A.** Lý do: đây là thay đổi **nhỏ nhất** khôi phục được tính chất "clone về là chạy được", đồng thời **không thay thế công nghệ nào** mà SRS đã quy định — chỉ đổi kênh phân phối và ghim phiên bản, đúng nguyên tắc dài hạn của nhóm. Phương án B để dành cho lúc dựng CI thật.

Thay đổi được áp dụng đồng bộ ở **ba nơi** để không còn chỗ nào trỏ về image đã chết:

| File | Thay đổi |
|---|---|
| `docker-compose.yml` | service `minio` → `quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z` (kèm comment giải thích) |
| `backend/tests/.../Infrastructure/CulinaryBlogApiFactory.cs` | hằng `MinioImage` dùng cùng image |
| `SPEC/CONG_NGHE_VA_PHIEN_BAN.md` (dòng 159) | bảng phiên bản container cập nhật `quay.io/minio/minio` + tag đã ghim |

**Tách thành commit riêng đứng trước commit Buổi 3:**

```
fix(infra): pin minio image to quay.io registry after docker hub repo removal
```

Vì hạng mục này không nằm trong phạm vi kế hoạch Buổi 3, tách ra thì người review thấy ngay lý do và bối cảnh, thay vì phải mò một thay đổi hạ tầng lẫn giữa một commit lớn về test harness. Phần `CulinaryBlogApiFactory.cs` là file **mới hoàn toàn** của Buổi 3 nên nằm ở commit thứ hai; nó sinh ra sau khi sự cố đã được xử lý nên không có trạng thái trung gian nào bị hỏng.

## 6. Danh sách file thay đổi

### 6.1 Thêm mới (11 file)

| File | Vai trò |
|------|---------|
| `backend/src/CulinaryBlog.API/Extensions/NginxGateDashboardFilter.cs` | Lớp bảo vệ thứ hai của Hangfire Dashboard |
| `backend/tests/CulinaryBlog.ArchitectureTests/ValidationStatusCodeTests.cs` | Chặn `Status422UnprocessableEntity` quay lại (D-11) |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/CulinaryBlogApiFactory.cs` | Harness Testcontainers + `CreateClientAs` + Respawn |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/IntegrationTestSuite.cs` | Collection fixture dùng chung 3 container |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/TestDataSeeder.cs` | Bộ dữ liệu test cố định, tất định |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/HttpAssertions.cs` | Helper đọc RFC 7807 / ValidationProblemDetails |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/TestImages.cs` | Byte ảnh dựng tay cho test magic bytes |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Infrastructure/Fakes/FakeBackgroundJobClient.cs` | `IBackgroundJobClient` ghi lại lời gọi |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Auth/AuthEndpointsTests.cs` | 8 test |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Files/FilesEndpointsTests.cs` | 12 test |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Content/CategoriesEndpointsTests.cs` | 3 test |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Content/RecipesEndpointsTests.cs` | 8 test |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Content/RecipeCacheIsolationTests.cs` | 1 test Skip — MT-34 |
| `backend/tests/CulinaryBlog.API.IntegrationTests/Jobs/HangfireDashboardTests.cs` | 4 test |
| `frontend/src/lib/api-client.test.ts` | 5 test cho `mapProblemDetailsToForm` |
| `nginx/generate-htpasswd.sh` / `nginx/generate-htpasswd.ps1` | Sinh `.htpasswd` bcrypt qua Docker |

### 6.2 Sửa đổi

| File | Thay đổi |
|------|----------|
| `backend/src/CulinaryBlog.API/Middleware/GlobalExceptionMiddleware.cs` | **D-11**: `ValidationException` + `DomainException` → 400 |
| `backend/src/CulinaryBlog.API/Endpoints/AuthEndpoints.cs` | **D-11**: `.ProducesValidationProblem(400)` ×2 |
| `backend/src/CulinaryBlog.API/Endpoints/RecipesEndpoints.cs` | **D-11**: `.ProducesValidationProblem(400)` |
| `backend/src/CulinaryBlog.Infrastructure/Identity/IdentityService.cs` | **D-11**: cập nhật XML doc |
| `backend/src/CulinaryBlog.API/Program.cs` | `MapHangfireDashboard` + `public partial class Program` |
| `backend/src/CulinaryBlog.Infrastructure/DependencyInjection.cs` | `Hangfire:PollIntervalSeconds` cho `QueuePollInterval` + `SchedulePollingInterval` |
| `backend/src/CulinaryBlog.API/appsettings.json` | `Hangfire:PollIntervalSeconds`, `Hangfire:DashboardGateSecret` |
| `backend/src/CulinaryBlog.API/appsettings.Development.json` | `Hangfire:PollIntervalSeconds = 1` |
| `backend/tests/.../CulinaryBlog.API.IntegrationTests.csproj` | `Respawn 7.0.0`, `Testcontainers.Minio 4.15.0` (ghim version) |
| `frontend/src/lib/api-client.ts` | Thêm `mapProblemDetailsToForm()` |
| `frontend/src/features/auth/components/LoginForm.tsx` | **D-11**: dùng helper chung |
| `frontend/src/features/auth/components/RegisterForm.tsx` | **D-11**: dùng helper chung, xóa `isServerField` trùng lặp |
| `nginx/nginx.conf` → `nginx/templates/nginx.conf.template` | `git mv` + block `location /hangfire` Basic Auth |
| `docker-compose.yml` | nginx dùng templates + envsubst; `Hangfire__DashboardGateSecret`; image MinIO → quay.io |
| `.env.example` | 3 biến `HANGFIRE_*` |
| `.gitignore` | `nginx/.htpasswd`, `.htpasswd` |
| `SPEC/CONG_NGHE_VA_PHIEN_BAN.md` | Cập nhật dòng image MinIO |

---

## 7. Kết quả chạy test thực tế

### 7.1 Backend — `dotnet build` + `dotnet test`

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
*(repo bật `TreatWarningsAsErrors` + SonarAnalyzer + StyleCop → 0 warning là bắt buộc)*

```
Passed! - Failed: 0, Passed: 14, Skipped: 0, Total: 14, Duration:  75 ms - CulinaryBlog.Domain.UnitTests.dll
Passed! - Failed: 0, Passed: 27, Skipped: 0, Total: 27, Duration:  97 ms - CulinaryBlog.Application.UnitTests.dll
Passed! - Failed: 0, Passed:  4, Skipped: 0, Total:  4, Duration:   1 s  - CulinaryBlog.ArchitectureTests.dll
Passed! - Failed: 0, Passed: 43, Skipped: 1, Total: 44, Duration:  16 s  - CulinaryBlog.API.IntegrationTests.dll
```

**Tổng: 88 Passed · 0 Failed · 1 Skipped** (test MT-34, Skip có chủ đích — mục 4.3).

Trước Buổi 3 project `CulinaryBlog.API.IntegrationTests` chỉ có 8 test seed catalog và **không có test nào gọi API**; nay có **36 test gọi API thật** trên PostgreSQL/Redis/MinIO thật.

### 7.2 Log Testcontainers

Toàn bộ 44 integration test chạy trong **16 giây**, gồm cả thời gian kéo/khởi động ba container:

```
postgres:16-alpine                                → culinaryblog_tests, migration B1_InitialSchema
redis:7-alpine                                    → cache + output cache
quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z  → bucket culinary-blog-tests
```

Ba container được dựng **một lần** cho cả collection (`IntegrationTestSuite`) rồi dùng chung — đánh đổi là các lớp test chạy tuần tự, nhưng tổng thời gian vẫn thấp hơn nhiều so với dựng container cho từng lớp.

### 7.3 Frontend — `npm run lint` / `typecheck` / `test` / `build`

```
> eslint src --ext .ts,.tsx        → không có lỗi
> tsc --noEmit                     → không có lỗi

Test Suites: 3 passed, 3 total
Tests:       20 passed, 20 total

> next build → thành công, 9 route (○ Static / ● SSG / ƒ Dynamic)
```

### 7.4 `docker compose` — 9 container Up

```
SERVICE    STATUS
api        Up
frontend   Up
hangfire   Up
mailhog    Up
minio      Up (healthy)
nginx      Up
postgres   Up (healthy)
redis      Up (healthy)
seq        Up
```

Kết quả kiểm chứng Hangfire Dashboard và Welcome Email: xem bảng ở **mục 3**.

---

## 8. Định nghĩa Hoàn thành (§2.4 của kế hoạch)

| Tiêu chí | Trạng thái |
|----------|-----------|
| `dotnet build` 0 warning | ✅ |
| `dotnet test` xanh **kể cả integration test** | ✅ 88 passed |
| `npm run lint && npm run build` xanh | ✅ |
| `docker compose up` chạy | ✅ 9/9 container Up |
| Test tay qua Scalar + UI | ✅ curl qua Nginx (mục 3, mục 2) |

---

## 9. Ghi chú bàn giao cho Dev 1 / Dev 2 / Dev 3

1. **Trước khi `docker compose up` lần đầu**, mỗi máy chạy một lần:
   `./nginx/generate-htpasswd.sh` (hoặc `powershell -File .\nginx\generate-htpasswd.ps1`) sau khi điền `HANGFIRE_*` trong `.env`. Không có file này thì Nginx không khởi động được.
2. **Viết integration test** — mẫu ngắn nhất:
   ```csharp
   [Collection(IntegrationTestSuite.Name)]
   public class MyTests(CulinaryBlogApiFactory factory)
   {
       [Fact]
       public async Task Admin_Can_Create_Category()
       {
           var client = factory.CreateClientAs("Admin", TestDataSeeder.AdminUserId);
           var response = await client.PostAsJsonAsync("/api/v1/categories", new { name = "Món mới" });
           Assert.Equal(HttpStatusCode.Created, response.StatusCode);
       }
   }
   ```
   Lớp test nào **ghi dữ liệu** thì thêm `: IAsyncLifetime` với `InitializeAsync() => factory.ResetDatabaseAsync();`.
3. **Lỗi validation từ nay là 400**, không còn 422 — ở Frontend dùng `mapProblemDetailsToForm(error, FIELDS, setError)` thay vì tự viết điều kiện status.
4. **Buổi 4 (D-18):** `CREATE TEXT SEARCH CONFIGURATION vietnamese_unaccent` phải được đưa vào migration, vì Testcontainers không chạy `docker/postgres/init.sql`.
5. **Buổi 6 (D-4/D-6):** gỡ `Skip` khỏi `RecipeCacheIsolationTests` và cập nhật `GetRecipeBySlug_DraftAsGuest_Returns403ForNow` thành 404.

---

## 10. Commit trên nhánh `2312755_NguyenThangThieng_buoiso3`

Hai commit, theo đúng thứ tự:

**Commit 1 — sửa sự cố hạ tầng phát sinh (mục 5):**

```
fix(infra): pin minio image to quay.io registry after docker hub repo removal
```

Chỉ chứa `docker-compose.yml` (service `minio`) và `SPEC/CONG_NGHE_VA_PHIEN_BAN.md`. Tách riêng vì hạng mục này nằm ngoài phạm vi kế hoạch Buổi 3 — người review thấy ngay bối cảnh thay vì phải mò một thay đổi hạ tầng lẫn giữa commit lớn về test harness.

**Commit 2 — toàn bộ phần việc Dev 4 của Buổi 3:**

```
feat(infra): secure hangfire dashboard behind nginx basic auth and add testcontainers integration test harness
```

*(Commit nền D-11 được gộp trong commit này; nếu nhóm muốn tách thêm, phần D-11 dùng message đã chốt trong kế hoạch: `refactor(api): unify validation errors to 400 per SRS v1.1.0 MT-08`.)*
