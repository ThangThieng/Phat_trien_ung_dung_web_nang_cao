# CÔNG NGHỆ & PHIÊN BẢN ĐÃ CÀI ĐẶT – CULINARY BLOG

> Căn cứ: `SPEC/SRS_Culinary_Blog_v1.0.0.pdf` · Ngày cài đặt: 11/09/2026 · Máy: Windows 11 Home (10.0.26200)
> Phiên bản trong tài liệu này là **phiên bản thực tế đã cài** (lấy từ `*.csproj`, `package-lock.json` và container đang chạy).

**Nguyên tắc áp dụng:** không thay thế bất kỳ công nghệ nào SRS đã chỉ định. Chỉ **ghim phiên bản** hoặc **chỉnh cấu hình** khi bắt buộc, và mọi trường hợp đều ghi rõ lý do ở [Mục 6](#6-nhật-ký-thay-đổi-so-với-srs--lý-do).

Ký hiệu cột "Trạng thái":
- ✅ đúng như SRS
- 📌 đúng công nghệ SRS nhưng ghim/chỉnh phiên bản
- ➕ bổ sung (SRS không chỉ định công cụ cụ thể)
- ⚠️ khác SRS vì bắt buộc

---

## 1. Công cụ trên máy phát triển

| Công cụ | SRS yêu cầu | Đã cài | Trạng thái | Ghi chú |
|---|---|---|---|---|
| .NET SDK | 10.0.x | **10.0.401** | ✅ | Cài mới qua `winget install Microsoft.DotNet.SDK.10`. Máy trước đó chỉ có 8.0.400 (vẫn giữ, không ảnh hưởng). Ghim bằng `backend/global.json` |
| dotnet-ef (EF Core CLI) | (dùng cho migrations – CONS-006) | **10.0.12** | ✅ | `dotnet tool install --global dotnet-ef` |
| Node.js | 20+ LTS | **24.13.0** (có sẵn) | ✅ | Đạt yêu cầu tối thiểu. Container dùng Node 22 LTS (xem Mục 4) |
| npm | 10+ | **11.6.2** (có sẵn) | ✅ | Lockfile được sinh bằng npm 10 để tương thích container (xem 6.5) |
| Docker Engine / Compose | 24.x+ / v2 | **29.4.1 / v5.1.3** (có sẵn) | ✅ | |
| Git | 2.40+ | **2.52.0** (có sẵn) | ✅ | Repo khởi tạo nhánh `main`, `core.hooksPath=.githooks` |
| IDE | VS 2022 / Rider / VS Code + C# Dev Kit | VS Code | ✅ | |

---

## 2. Backend – .NET 10 Minimal APIs, Clean Architecture

### 2.1 Cấu trúc solution (CONS-001)
```
backend/
├─ global.json                      # ghim SDK 10.0.401
├─ Directory.Build.props            # net10.0, Nullable, TreatWarningsAsErrors, StyleCop, SonarAnalyzer
├─ .editorconfig                    # naming/style conventions
├─ CulinaryBlog.sln
├─ src/CulinaryBlog.Domain          # KHÔNG có NuGet package (chỉ .NET BCL)
├─ src/CulinaryBlog.Application     # → Domain
├─ src/CulinaryBlog.Infrastructure  # → Application
├─ src/CulinaryBlog.API             # → Application + Infrastructure
└─ tests/ Domain.UnitTests · Application.UnitTests · API.IntegrationTests · ArchitectureTests
```
Runtime: **ASP.NET Core 10.0.12**. Target framework: `net10.0`.

### 2.2 Tầng Application
| Package | Phiên bản | SRS | Trạng thái |
|---|---|---|---|
| MediatR | **12.5.0** | CQRS + MediatR (CONS-002) | 📌 xem 6.1 |
| FluentValidation | 12.1.1 | CONS-008 | ✅ |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | CONS-008 | ✅ |
| Microsoft.Extensions.Logging.Abstractions | 10.0.12 | ILogger cho LoggingBehavior (SRS §6.3) | ✅ *(thêm ở Buổi 2)* |
| Microsoft.Extensions.Identity.Stores | 10.0.12 | ASP.NET Core Identity (UserManager trong handler) | ✅ |

### 2.3 Tầng Infrastructure
| Package | Phiên bản | Mục đích theo SRS | Trạng thái |
|---|---|---|---|
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | EF Core 10 + PostgreSQL 16, FTS tsvector | ✅ |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 | Migrations Code-First | ✅ |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.12 | Identity, PBKDF2 (NFR-SEC-001) | ✅ |
| System.IdentityModel.Tokens.Jwt | 8.22.0 | JwtService HS256 (§6.2) | ✅ |
| Google.Apis.Auth | 1.76.0 | Xác minh Google ID Token (FR-AUTH-003) | ✅ |
| Microsoft.Extensions.Caching.StackExchangeRedis | 10.0.12 | Redis 7 distributed cache (StackExchange.Redis) | ✅ |
| AWSSDK.S3 | 4.0.103.1 | MinIO S3-compatible (§5.3) | ✅ |
| MailKit | 4.17.0 | Welcome email (FR-JOB-001) | ✅ |
| Hangfire.Core | 1.8.25 | Background jobs (§3.6) | ✅ |
| Hangfire.PostgreSql | 1.21.1 | Hangfire storage trên PostgreSQL | ✅ |
| Bogus | 35.6.5 | Seed 50 recipe / 5 tác giả (§2.6.1) | ✅ |
| Hangfire.AspNetCore | 1.8.25 | Đăng ký `AddHangfire` / `AddHangfireServer` trong DI Infrastructure | ✅ *(thêm ở Buổi 2)* |
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | FR-OBS-001 | ✅ |
| AspNetCore.HealthChecks.Redis | 9.0.0 | FR-OBS-001 | ✅ |
| SixLabors.ImageSharp | **3.1.12** | Resize ảnh 800×600 / 300×300 (FR-JOB-002) | ➕📌 xem 6.2 |

### 2.4 Tầng API (Presentation)
| Package | Phiên bản | Mục đích theo SRS | Trạng thái |
|---|---|---|---|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.12 | JWT Bearer | ✅ |
| Microsoft.AspNetCore.OpenApi | 10.0.12 | OpenAPI document | ✅ |
| Scalar.AspNetCore | 2.17.3 | Scalar UI tại `/scalar` | ✅ |
| Microsoft.AspNetCore.OutputCaching.StackExchangeRedis | 10.0.12 | Output Cache "RecipeList"/"RecipeDetail" (FR-RCP-001/002) lưu trên Redis – distributed (NFR-SCALE-001) | ✅ *(thêm ở Buổi 2)* |
| Hangfire.AspNetCore | 1.8.25 | Dashboard `/hangfire` | ✅ |
| Serilog.AspNetCore | 10.0.0 | Structured logging (CONS-010), gồm Console + File sink | ✅ |
| Serilog.Sinks.Seq | 9.1.0 | Seq (dev) | ✅ |
| Serilog.Formatting.Compact | 3.0.0 | Console JSON | ✅ |
| OpenTelemetry.Extensions.Hosting | 1.18.0 | FR-OBS-003 | ✅ |
| OpenTelemetry.Instrumentation.AspNetCore | 1.18.0 | Trace HTTP | ✅ |
| OpenTelemetry.Instrumentation.Http | 1.18.0 | Trace HttpClient | ✅ |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.18.0 | OTLP exporter | ✅ |
| Npgsql.OpenTelemetry | 10.0.3 | Trace truy vấn database (EF Core → Npgsql) | ✅ |
| AspNetCore.HealthChecks.UI.Client | 9.0.0 | JSON response `/health` | ✅ |
| *(Rate Limiting)* | built-in ASP.NET Core 10 | NFR-SEC-003 | ✅ không cần package |
| *(Output Cache)* | built-in ASP.NET Core 10 | FR-RCP-001/002 | ✅ không cần package |
| *(Health check MinIO)* | tự viết `MinioHealthCheck : IHealthCheck` | FR-OBS-001 | ⚠️ xem 6.4 |

### 2.5 Chất lượng mã & kiểm thử (NFR-MAINT-001/002/004)
| Package | Phiên bản | SRS | Trạng thái |
|---|---|---|---|
| SonarAnalyzer.CSharp | 10.34.0.3385 | NFR-MAINT-001 | ✅ |
| StyleCop.Analyzers | 1.2.0-beta.556 | NFR-MAINT-001 | ✅ (bản beta là bản mới nhất, bản stable 1.1.118 không hỗ trợ C# mới) |
| EditorConfig | `backend/.editorconfig` | NFR-MAINT-001 | ✅ |
| xunit / xunit.runner.visualstudio | 2.9.3 / 3.1.4 | xUnit (NFR-MAINT-002) | ✅ |
| Microsoft.NET.Test.Sdk / coverlet.collector | 17.14.1 / 6.0.4 | Test runner + coverage ≥80% | ✅ |
| TngTech.ArchUnitNET.xUnit | 0.13.4 | ArchUnit.NET (NFR-MAINT-004) | ✅ đã có 3 test Dependency Rule |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.12 | Integration test endpoints | ✅ |
| Testcontainers.PostgreSql / Redis | 4.15.0 | Integration test trên DB thật | ➕ |
| NSubstitute | 6.2.0 | Mock trong unit test | ➕ |

---

## 3. Frontend – Next.js App Router

| Package | Phiên bản | SRS | Trạng thái |
|---|---|---|---|
| next | **15.5.25** | Next.js 15 App Router (tài liệu tham chiếu #10) | ✅ |
| react / react-dom | 19.1.0 | (đi kèm Next 15) | ✅ |
| typescript | 5.9.3 | TypeScript | ✅ |
| tailwindcss / @tailwindcss/postcss | 4.3.3 | Tailwind CSS utility-first (NFR-USE-001) | ✅ |
| @tanstack/react-query (+ devtools) | 5.102.8 | TanStack Query | ✅ |
| react-hook-form | 7.87.0 | React Hook Form | ✅ |
| zod | 4.6.2 | Zod | ✅ |
| @hookform/resolvers | 5.9.1 | Kết nối RHF ↔ Zod | ✅ |
| next-auth | 5.0.0-beta.32 | **Auth.js v5** (Google OAuth PKCE) | ✅ Auth.js v5 chỉ phát hành dưới tag beta |
| sonner | 2.0.8 | Toast notification (NFR-USE-004) | ➕ |
| clsx / tailwind-merge | 2.1.1 / 3.6.0 | Gộp className Tailwind | ➕ |
| lucide-react | 1.44.0 | Icon | ➕ |

**Chất lượng mã & kiểm thử**
| Package | Phiên bản | SRS | Trạng thái |
|---|---|---|---|
| eslint | **8.57.1** | ESLint | 📌 xem 6.3 |
| eslint-config-airbnb | 19.0.4 | **ESLint Airbnb ruleset** (NFR-MAINT-001) | ✅ |
| eslint-config-airbnb-typescript | 18.0.0 | Airbnb cho TypeScript | ✅ |
| @typescript-eslint/eslint-plugin / parser | 7.18.0 | Peer của airbnb-typescript | ✅ |
| eslint-plugin-import / react / jsx-a11y | 2.32.0 / 7.37.5 / 6.10.2 | Peer của Airbnb | ✅ |
| eslint-plugin-react-hooks | **5.2.0** | Peer của Airbnb (`airbnb/hooks`) | 📌 xem 6.3 |
| eslint-config-next | 15.5.25 | Rule của Next.js | ✅ |
| prettier / eslint-config-prettier | 3.9.6 / 10.1.8 | Prettier | ✅ |
| prettier-plugin-tailwindcss | 0.8.1 | Sắp xếp class Tailwind | ➕ |
| ajv | **8.20.0** | — | 📌 xem 6.3 |
| jest / jest-environment-jsdom | 30.5.1 | Jest (NFR-MAINT-002) | ✅ |
| @testing-library/react / dom / jest-dom | 16.3.3 / 10.4.1 / 7.0.1 | Testing Library | ✅ |
| @playwright/test | 1.63.0 | Playwright E2E | ✅ |
| @axe-core/playwright | 4.13.0 | Kiểm tra WCAG 2.1 AA (NFR-USE-002) | ✅ |
| @lhci/cli | 0.15.1 | Lighthouse CI (NFR-PERF-005) | ✅ |

---

## 4. Hạ tầng Docker Compose (SRS §6.5)

| Service | Image | Phiên bản chạy thực tế | SRS | Port (host:container) |
|---|---|---|---|---|
| nginx | nginx:alpine | nginx **1.31.5** | Nginx 1.24+ | 80:80 |
| api | culinaryblog-api (build) | ASP.NET Core **10.0.12** | .NET 10 | 5000:8080 |
| hangfire | culinaryblog-api (cùng image) | ASP.NET Core 10.0.12 | Hangfire | — (worker) |
| frontend | culinaryblog-web (build) | Node **22.23.2** | Node 22 LTS (khuyến nghị) | 3000:3000 |
| postgres | postgres:16-alpine | PostgreSQL **16.15** | PostgreSQL 16 | `${POSTGRES_HOST_PORT:-5432}`:5432 – máy này dùng **5434** (xem 6.10) |
| redis | redis:7-alpine | Redis **7.4.11** (AOF bật) | Redis 7 | 6379:6379 |
| minio | minio/minio:latest | RELEASE.**2025-09-07** | MinIO RELEASE.2024+ | 9000, 9001 (console) |
| seq | datalust/seq:latest | latest | Seq | 5341:80 |
| mailhog | mailhog/mailhog | latest (profile `dev`) | Mailhog (dev only) | 8025, 1025 |

**PostgreSQL extensions** (`docker/postgres/init.sql`): `unaccent`, `pg_trgm`, cùng text search configuration `vietnamese_unaccent`. Đã kiểm tra: `to_tsvector('vietnamese_unaccent','Phở bò Hà Nội') @@ to_tsquery('pho & bo')` = **true**.

**Dockerfile multi-stage (CONS-009):**
- `backend/Dockerfile`: `dotnet/sdk:10.0` → `dotnet/aspnet:10.0`, chạy bằng user non-root `app`.
- `frontend/Dockerfile`: `node:22-alpine` ×3 stage → Next.js `output: 'standalone'`, chạy bằng user non-root `nextjs`.

---

## 5. Bảo mật & cấu hình (NFR-SEC-007)

| Hạng mục | Thực hiện |
|---|---|
| Secret local | `.env` sinh ngẫu nhiên (mật khẩu Postgres/MinIO, JWT key, AUTH_SECRET); nằm trong `.gitignore` |
| Secret khi `dotnet run` | **ASP.NET Core User Secrets** (connection string, JWT key, MinIO key). `appsettings*.json` không chứa mật khẩu |
| Mẫu cấu hình | `.env.example`, `frontend/.env.local.example` (không có giá trị thật) |
| Quét secret | Pre-commit hook `.githooks/pre-commit` chạy **gitleaks** qua Docker (`zricethezav/gitleaks`), cấu hình `.gitleaks.toml` |
| Chưa cấu hình | `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` – cần tạo trên Google Cloud Console trước Buổi 3 |

---

## 6. Nhật ký thay đổi so với SRS & lý do

### 6.1 MediatR – ghim phiên bản 12.5.0 📌
- **Thay đổi:** không dùng bản mới nhất mà ghim `MediatR 12.5.0`.
- **Lý do:** từ **v13 (2025)**, MediatR chuyển sang **license thương mại** (Lucky Penny Software), bắt buộc license key khi chạy. **12.5.0 là bản Apache-2.0 cuối cùng**, miễn phí và đầy đủ tính năng SRS cần (IRequest, IRequestHandler, IPipelineBehavior).
- **Ảnh hưởng:** không có. Vẫn là công nghệ MediatR mà CONS-002 yêu cầu.

### 6.2 SixLabors.ImageSharp – ghim phiên bản 3.1.12 ➕📌
- **Bối cảnh:** SRS (FR-JOB-002) yêu cầu tạo ảnh medium 800×600 và thumbnail 300×300 nhưng **không chỉ định thư viện**, nên đây là thư viện bổ sung.
- **Thay đổi:** ban đầu cài bản 4.1.1, sau đó hạ về **3.1.12**.
- **Lý do:** bản 4.x làm build báo lỗi *"No Six Labors license found"* nếu thiếu license key. Vì dự án bật `TreatWarningsAsErrors`, lỗi này làm hỏng build. Bản 3.1.x dùng **Six Labors Split License**: miễn phí cho dự án mã nguồn mở, học tập và doanh nghiệp doanh thu dưới 1 triệu USD, không cần key.

### 6.3 ESLint – dùng ESLint 8.57.1 (không dùng ESLint 9) để giữ bộ luật Airbnb 📌
- **Yêu cầu SRS (NFR-MAINT-001):** "TypeScript/React: ESLint (**Airbnb ruleset**), Prettier". Đây là yêu cầu được **giữ nguyên**.
- **Vấn đề:** `create-next-app@15` mặc định sinh **ESLint 9** với `eslint.config.mjs` (flat config). Trong khi đó:
  - `eslint-config-airbnb@19.0.4` (bản mới nhất) chỉ hỗ trợ `eslint ^7.32.0 || ^8.2.0`;
  - `eslint-config-airbnb-typescript@18.0.0` yêu cầu `eslint ^8.56.0`.

  Airbnb **không chạy chính thức trên ESLint 9**.
- **Thay đổi đã làm:**
  1. Gỡ ESLint 9 và `eslint.config.mjs`, cài **`eslint@8.57.1`** với file `.eslintrc.json` (Next.js 15 vẫn hỗ trợ ESLint 8).
  2. `extends: ["airbnb", "airbnb/hooks", "airbnb-typescript", "next/core-web-vitals", "prettier"]`.
  3. **`eslint-plugin-react-hooks` override lên 5.2.0** (trong `package.json` → `overrides`). Airbnb khai báo peer `^4.3.0`, còn `eslint-config-next@15` phụ thuộc `^5.0.0`, nên hai bản không cùng tồn tại được ở top-level. Bản 5.x vẫn tương thích ESLint 8 và giữ nguyên 2 rule `rules-of-hooks` và `exhaustive-deps` mà `airbnb/hooks` bật. Cách này **không dùng** `--legacy-peer-deps` toàn cục.
  4. **Thêm `ajv@8.20.0`** vào devDependencies. `@hookform/resolvers` khai báo peer `ajv ^8.12`, trong khi ESLint 8 kéo `ajv@6` lên top-level, làm `npm ci` báo lockfile không hợp lệ. Khai báo rõ `ajv@8` thì npm đặt `ajv@6` lồng riêng dưới `eslint`, và mỗi bên dùng đúng bản của mình.
- **Lưu ý:** ESLint 8 đã hết hỗ trợ chính thức (EOL 10/2024), nhưng chỉ dùng lúc phát triển, không ảnh hưởng runtime hay bảo mật sản phẩm. Khi Airbnb phát hành bản hỗ trợ ESLint 9 thì nâng cấp.

### 6.4 Health check MinIO – tự viết `IHealthCheck` ⚠️
- **SRS (FR-OBS-001)** ghi package `AspNetCore.HealthChecks.Minio`.
- **Thực tế:** tra NuGet ngày 11/09/2026 thì package này **không tồn tại** (0 kết quả).
- **Giải pháp:** viết `MinioHealthCheck : IHealthCheck` dùng `AWSSDK.S3` (ListBuckets) ở Buổi 5. `IHealthCheck` cũng nằm trong danh sách kỹ thuật của FR-OBS-001, nên vẫn bám SRS và không thêm package ngoài.
- **Đây là thay đổi duy nhất bắt buộc phải khác SRS.**

### 6.5 Lockfile npm sinh bằng npm 10 📌
- **Lý do:** máy dùng Node 24 / npm 11, còn container dùng **Node 22 LTS / npm 10** theo khuyến nghị SRS §2.4.1. Lockfile do npm 11 sinh ra khiến `npm ci` trong container báo lỗi thiếu package (khác biệt cách ghi optional dependency).
- **Thay đổi:** sinh lại `package-lock.json` bằng `npx npm@10 install`. Đã kiểm tra `npm ci` chạy được với **cả npm 10 và npm 11**, nên thành viên dùng Node 20/22/24 đều cài được.

### 6.6 Jest config dùng `.mjs` thay vì `.ts`
- **Lý do:** Jest cần thêm package `ts-node` mới đọc được `jest.config.ts`. Dùng `jest.config.mjs` thì không phải cài thêm dependency. Công cụ vẫn là Jest.

### 6.7 Font Inter (subset `vietnamese`) thay font Geist mặc định
- **Lý do:** font Geist do `create-next-app` sinh ra **không có subset tiếng Việt**, nên dấu hiển thị bằng font dự phòng, lệch và gây layout shift (ảnh hưởng CLS, NFR-PERF-005). Inter có subset `vietnamese`. SRS không chỉ định font.

### 6.8 Cấu hình StyleCop – tắt một số rule
- **Rule đã tắt** (trong `.editorconfig`): `SA1101` (bắt buộc `this.`), `SA1309` (cấm `_field`), `SA1200`, `SA1600–1602` (bắt buộc XML doc mọi element), `SA1633` (file header), `SA1402/SA1649` (1 type / file), `SA1413`, `SA1010`, `SA1000`.
- **Lý do:** các rule này xung đột với convention của team (`_camelCase` field, gom Command + Handler + Validator trong 1 file CQRS, collection expression C# 12+). StyleCop vẫn được dùng đúng như NFR-MAINT-001; phần còn lại của bộ rule vẫn bật và **coi warning là lỗi**.

### 6.10 Cổng PostgreSQL phía host cấu hình được (Buổi 2)
- **Vấn đề:** máy phát triển đã có service Windows `postgresql-x64-18` chiếm `0.0.0.0:5432`, và container `lms-postgres-dwh` chiếm 5433. Kết nối `localhost:5432` rơi vào Postgres của Windows nên xác thực thất bại.
- **Thay đổi:** `docker-compose.yml` map `${POSTGRES_HOST_PORT:-5432}:5432`. Mặc định vẫn 5432 đúng SRS §6.5; `.env` của máy này đặt `5434`. Container và các service nội bộ vẫn dùng 5432. Không tắt hay sửa service của người dùng.

### 6.11 Trang /categories và /categories/[slug]: revalidate ở tầng dữ liệu
- **SRS §5.1:** `/categories` ISR 3600s, `/categories/[slug]` ISR 600s.
- **Thực hiện:** trang render theo request (ƒ), còn dữ liệu lấy qua `fetch(..., { next: { revalidate: 3600 | 600 } })` (Data Cache của Next.js), nên API chỉ bị gọi tối đa 1 lần mỗi chu kỳ. `/recipes/[slug]` là ISR đầy đủ (●, `x-nextjs-cache: HIT`, `s-maxage=300`).
- **Lý do:** (1) Khi `next build` image Docker, backend chưa chạy, nên prerender `/categories` lúc build sẽ lỗi. (2) `/categories/[slug]` đọc `?page=` (searchParams), nên Next bắt buộc render động. Cả hai vẫn đạt mục tiêu "tái sinh dữ liệu theo chu kỳ".

### 6.9 Các điểm SRS tự mâu thuẫn
Bảng §1.3 của kế hoạch 6 buổi cũ (`KE_HOACH_PHAT_TRIEN_6_BUOI.md`, nay chỉ còn trong lịch sử git) đã chốt các điểm này theo SRS v1.0.0 (mã lỗi 422/409, Redis hay IMemoryCache cho danh mục, hard/soft delete recipe, độ dài refresh token, điều kiện publish, Hangfire worker). Ở mỗi điểm đều chọn **một trong các phương án chính SRS nêu**, không đưa công nghệ ngoài vào. Từ 19/09/2026 các quyết định này được thay bằng SRS v1.2.0 và `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` (57 mục MT); kế hoạch hiện hành là `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`.

---

## 7. Lệnh kiểm tra nhanh

```bash
dotnet --list-sdks                  # phải có 10.0.401
cd backend  && dotnet build && dotnet test            # 0 warning · ArchitectureTests 3/3
cd frontend && npm ci && npm run lint && npm run typecheck && npm run build
docker compose --profile dev up -d --build && docker compose ps
```

| Kiểm tra (11/09/2026) | Kết quả |
|---|---|
| `dotnet build` | ✅ 0 Warning, 0 Error |
| ArchUnit.NET | ✅ 3/3 passed |
| Frontend lint (Airbnb) / typecheck / build | ✅ – First Load JS 103 kB (ngưỡng NFR ≤ 200 kB) |
| 9 container | ✅ Up; postgres/redis/minio healthy |
| HTTP :80 / :3000 / :5000 / :9001 / :5341 / :8025 | ✅ 200 |
