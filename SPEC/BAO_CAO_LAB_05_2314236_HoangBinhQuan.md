# BÁO CÁO LAB 05 — BẢO MẬT VÀ PHIÊN BẢN API

**Sinh viên:** Hoàng Bình Quân  
**MSSV:** 2314236  
**Nhánh:** `2314236_HoangBinhQuan_buoiso5`  
**Phạm vi:** PR #54 — CORS, rate limiting, API versioning và kiểm chứng bề mặt API

## 1. Kết quả

Phần Dev1 Lab 5 đã hoàn thành và được đồng bộ với `origin/develop` tại commit `847c7e7`. Diff frontend so với develop rỗng, vì vậy PR #54 không còn chứa chức năng Lab 6.

## 2. CORS

- Allowlist được bind qua `CorsSettings` và kiểm tra bằng `IValidateOptions<CorsSettings>` với `ValidateOnStart()`.
- Từ chối wildcard, origin sai định dạng, URL có path hoặc user info; môi trường Production từ chối allowlist rỗng.
- Cho phép các request header cần thiết, gồm `If-Match`.
- Expose `ETag`, `Retry-After`, `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` và `api-supported-versions`.
- Không bật credentials; preflight max age là 10 phút.

## 3. Rate limiting

- Auth dùng sliding window; general và upload dùng fixed window, partition theo IP sau trusted forwarded headers.
- `POST /files/upload` và `POST /recipes/{id}/images` cùng dùng upload limiter.
- Response vượt quota trả HTTP 429, Problem Details `RATE_LIMIT_EXCEEDED`, `Retry-After` và các header quota.
- Test kiểm chứng request thứ 6 tới endpoint upload ảnh công thức bị từ chối, IP độc lập không dùng chung quota và `X-Forwarded-For` giả từ proxy không tin cậy không thể né quota.

## 4. API versioning và bề mặt API

- API dùng URL segment `/api/v1`; response công bố `api-supported-versions`.
- `/api/v2` trả HTTP 400 với Problem Details `VALIDATION_ERROR`.
- OpenAPI chỉ công bố route phiên bản v1; Scalar và OpenAPI endpoint được kiểm chứng bởi integration suite.
- `ApiSurfaceTests` nằm trong shared integration infrastructure và chạy cùng toàn bộ suite.

## 5. Auth và test infrastructure

- Refresh token đi qua interface chuẩn `Application.Common.Interfaces.Persistence.IUserRepository` tại `IUnitOfWork.Users`; không còn repository song song.
- Giữ PostgreSQL user-row lock, conditional atomic update, family-only reuse revocation, rollback và account-disable semantics.
- Tất cả integration test Dev1 dùng shared `CulinaryBlogApiFactory` qua `IntegrationTestSuite`; rate-limit dùng child host và dùng chung containers.
- URL file MinIO được kiểm chứng với host khác, dot-segment traversal, user info và URL hợp lệ.

## 6. Kết quả kiểm thử ngày 18 tháng 10 năm 2026

| Gate | Kết quả |
|---|---:|
| Backend build | PASS — 0 warning, 0 error |
| Domain unit tests | PASS — 81/81 |
| Application unit tests | PASS — 105/105 |
| Architecture tests | PASS — 18/18 |
| Integration run 1 | PASS — 346/346 — 131,32 giây |
| Integration run 2 | PASS — 346/346 — 132,74 giây |
| Integration run 3 | PASS — 346/346 — 129,15 giây |
| Frontend lint | PASS |
| Frontend typecheck | PASS |
| Frontend Jest | PASS — 23 suites, 225 tests |
| Frontend production build | PASS |

## 7. Commit liên quan

- `ac1dd7c` — `merge: sync develop before repairing pr 54`
- `feat(security): cors allowlist with exposed headers, ip-based rate limiting (sliding auth, fixed global/upload) and url-segment api versioning`
- `fix(infra): preserve google error mapping and harden stored file url validation`
- `test(integration): consolidate dev1 coverage on shared test host`

Các hash của commit repair được ghi nhận trong lịch sử Git sau khi tạo checkpoint cuối.
