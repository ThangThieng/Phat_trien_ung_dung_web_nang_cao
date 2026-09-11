# Culinary Blog – Nhóm 20

Blog ẩm thực & nấu ăn theo `SPEC/SRS_Culinary_Blog_v1.0.0.pdf`. Kế hoạch 6 buổi: `SPEC/KE_HOACH_PHAT_TRIEN_6_BUOI.md`.

## Yêu cầu môi trường
| Công cụ | Phiên bản |
|---|---|
| .NET SDK | 10.0.x (ghim trong `backend/global.json`) |
| Node.js | 20+ LTS (Docker dùng 22 LTS) |
| Docker Desktop | 4.x+ (Compose v2) |
| dotnet-ef | `dotnet tool install --global dotnet-ef` |

## Chạy nhanh (< 5 phút – NFR-MAINT-003)
```bash
cp .env.example .env              # điền JWT_SIGNING_KEY, AUTH_SECRET... (openssl rand -base64 64)
git config core.hooksPath .githooks   # bật pre-commit gitleaks (NFR-SEC-007)
docker compose --profile dev up -d --build
```

| Service | URL |
|---|---|
| Nginx (reverse proxy) | http://localhost |
| Frontend (Next.js) | http://localhost:3000 |
| API (.NET 10) | http://localhost:5000 – Scalar: `/scalar` |
| MinIO Console | http://localhost:9001 |
| Seq (logs/traces) | http://localhost:5341 |
| Mailhog (dev SMTP) | http://localhost:8025 |
| PostgreSQL / Redis | localhost:`${POSTGRES_HOST_PORT}` (mặc định 5432) / localhost:6379 |

## Tài khoản & dữ liệu mẫu (seed – SRS §2.6.1)
Khi `Seed__Enabled=true` (docker-compose dev), API tự migrate DB và seed: 2 role (Author, Admin), 8 danh mục, 5 tác giả, 50 công thức (Bogus).
| Tài khoản | Mật khẩu |
|---|---|
| `admin@culinaryblog.local` (Admin) | `SEED_ADMIN_PASSWORD` trong `.env` |
| `author1..5@culinaryblog.local` (Author) | `SEED_AUTHOR_PASSWORD` trong `.env` |

> Máy đã cài PostgreSQL chiếm cổng 5432 → đặt `POSTGRES_HOST_PORT=5434` trong `.env` (container vẫn dùng 5432).

## API Buổi 1
| Endpoint | FR |
|---|---|
| `POST /api/v1/auth/register`, `POST /api/v1/auth/login` | FR-AUTH-001/002 |
| `GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}` | FR-RCP-001/002 |
| `GET /api/v1/categories`, `GET /api/v1/categories/{slug}` | FR-CAT-001/002 |
| `POST /api/v1/files/upload`, `DELETE /api/v1/files/{fileKey}` | FR-FILE-001/002 |

## Phát triển ngoài Docker
```bash
docker compose up -d postgres redis minio seq
# Backend – secrets dev lưu bằng User Secrets, không để trong appsettings
cd backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=<POSTGRES_HOST_PORT>;Database=culinaryblog;Username=culinary;Password=<POSTGRES_PASSWORD>" --project src/CulinaryBlog.API
dotnet user-secrets set "Jwt:SigningKey" "<JWT_SIGNING_KEY>" --project src/CulinaryBlog.API
dotnet user-secrets set "MinIO:AccessKey" "<MINIO_ROOT_USER>" --project src/CulinaryBlog.API
dotnet user-secrets set "MinIO:SecretKey" "<MINIO_ROOT_PASSWORD>" --project src/CulinaryBlog.API
dotnet user-secrets set "Seed:AdminPassword" "<SEED_ADMIN_PASSWORD>" --project src/CulinaryBlog.API
dotnet run --project src/CulinaryBlog.API
# Frontend
cd frontend && cp .env.local.example .env.local && npm install && npm run dev
```

## Kiểm tra chất lượng (Definition of Done mỗi commit)
```bash
cd backend  && dotnet build && dotnet test          # TreatWarningsAsErrors + StyleCop + SonarAnalyzer + ArchUnitNET
cd frontend && npm run lint && npm run typecheck && npm test && npm run build   # ESLint Airbnb + Prettier + Jest
```

## Cấu trúc
```
backend/   CulinaryBlog.{Domain,Application,Infrastructure,API} + tests/
frontend/  Next.js 15 App Router (src/app, src/lib)
nginx/     nginx.conf
docker/    postgres/init.sql (unaccent, pg_trgm, text search config vietnamese_unaccent)
```

## Ghi chú phiên bản (quyết định kỹ thuật)
- **MediatR 12.5.0** – bản Apache-2.0 cuối cùng (từ v13 yêu cầu license thương mại).
- **SixLabors.ImageSharp 3.1.x** – Six Labors Split License, không cần license key (v4 bắt buộc key).
- **ESLint 8.57 + eslint-config-airbnb** – Airbnb chưa hỗ trợ ESLint 9; `eslint-plugin-react-hooks` được override lên 5.x (tương thích ESLint 8) để khớp `eslint-config-next@15`.
- **Health check MinIO** – NuGet không có `AspNetCore.HealthChecks.Minio` như SRS ghi; sẽ viết `MinioHealthCheck : IHealthCheck` (Buổi 4).
