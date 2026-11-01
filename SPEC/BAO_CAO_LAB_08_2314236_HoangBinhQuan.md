# BÁO CÁO LAB 08 — KIỂM CHỨNG AUTH, BẢO MẬT VÀ HIỆU NĂNG

**Sinh viên:** Hoàng Bình Quân

**MSSV:** 2314236

**Vai trò:** Dev1 — Authentication / Authorization / User Management / Security

**Nhánh làm việc:** `2314236_HoangBinhQuan_buoiso8` (official)

**Base:** `61fb450f396643a0be99e3399de151423f416d99`

## 1. Phạm vi Buổi 8

Phần việc Dev1 gồm Admin User Management, năm luồng Auth E2E, checklist bảo mật, nâng coverage Auth Application tối thiểu 80% và đo hiệu năng tìm kiếm người dùng với 10.000 bản ghi. Các thay đổi được chuyển có kiểm soát từ scratch sang branch official, nền `origin/develop` tại commit base ghi ở trên.

## 2. Admin User Management

Trang `/dashboard/users` yêu cầu `RequireRole` Admin. Trang hỗ trợ tìm theo email hoặc display name, debounce 300 ms, lọc trạng thái, phân trang và hiển thị bảng gồm avatar, email, display name, roles, trạng thái, thời gian tạo và số công thức. Roles chỉ hiển thị, không sửa tại đây.

Admin có thể khóa/mở khóa với lý do 5–500 ký tự. UI cập nhật lạc quan và rollback khi API lỗi; admin không thể tự khóa. Frontend test đạt 10/10; backend test cho reason đạt 8/8.

## 3. Auth E2E

Playwright kiểm chứng năm luồng:

1. Đăng ký → đã xác thực tự động → dashboard.
2. Năm lần sai mật khẩu → HTTP 423.
3. Access token hết hạn → refresh minh bạch.
4. Admin khóa Author → refresh trả 403 → điều hướng đăng nhập.
5. Forgot password → MailHog → reset → đăng nhập bằng mật khẩu mới.

Full suite đạt 5/5 trong 1,4 phút trên isolated official stack `culinaryblog_buoi8_quan_official`. Các lỗi thực tế đã phát hiện và sửa gồm bật lockout cho user mới, reload persisted failed-login state để lần sai thứ năm đúng, đăng ký Identity default token providers và redirect khi AuthBootstrap refresh thất bại ở public route.

## 4. Checklist bảo mật

`docs/security-test-report.md` là evidence chi tiết. 63/63 kiểm tra đạt: chống giả mạo X-Forwarded-For; request thứ 11 trả 429 và Retry-After; Hangfire direct/proxy trả 401; Author bị từ chối 403; DTO `/auth/me` và `/auth/sessions` không lộ trường nhạy cảm; forgot password chống enumeration; reset URL được làm sạch; chặn open redirect; CSP không vi phạm; log/Seq không có secret.

Kiến trúc token hiện tại được phê duyệt bởi MT-55 Decision A: access token chỉ ở memory, refresh token được lưu bền trong localStorage, reset token không được persist. Refresh token localStorage không được ghi nhận là lỗ hổng trong báo cáo này.

## 5. Coverage Auth Application

Scope là namespace `CulinaryBlog.Application.Features.Auth` gồm 12 file production: AuthContracts, AuthResponseFactory, GoogleLogin, LoginUser, Logout, PasswordCommands, PasswordRules, ProfileCommands, RefreshToken, RegisterUser, SessionCommands và UserAdministration.

| Chỉ số | Baseline | Sau kiểm chứng |
|---|---:|---:|
| Dòng covered | 40 | 424 |
| Dòng hợp lệ | 459 | 459 |
| Line coverage | 8,71% | 92,37% |
| Yêu cầu | ≥80% | PASS |
| Application tests | 113 | 162/162 PASS |

Test mới kiểm chứng validator, register/login/Google, AuthResponseFactory, refresh rotation/reuse, logout, password flows, profile, sessions và quản trị người dùng. Register, login và Google dùng `AuthResponseFactory.IssueAsync`; refresh dùng path rotation chuyên biệt trong atomic user lock, giữ `sid`, persistence và conditional revoke. Không có bypass vi phạm.

## 6. Hiệu năng tìm kiếm người dùng

PostgreSQL scratch độc lập đã có 10.066 users: 8.057 active và 2.009 inactive. Query production được giữ nguyên khi transfer official: `ILIKE` trên Email hoặc DisplayName, optional `IsActive`, count, `CreatedAt DESC`/`Id`, paging và projection.

| Case | Lần 1 | Lần 2 | Lần 3 |
|---|---:|---:|---:|
| Email substring chọn lọc | 9,361 ms | 9,483 ms | 9,607 ms |
| Display-name + inactive | 2,829 ms | 2,752 ms | 2,668 ms |
| Không có kết quả | 10,563 ms | 10,841 ms | 10,456 ms |
| Ordered/paged representative | 13,739 ms | — | — |

`EXPLAIN ANALYZE` ghi nhận sequential scan nhưng worst 13,739 ms, dưới ngưỡng 100 ms. Vì vậy **không tạo** migration `B8_Auth_UserSearchTrgm` và không thêm `pg_trgm`/index khi chưa có evidence cần thiết. Regression email search, display-name search, status filter, paging và Admin authorization đạt 5/5.

## 7. Thay đổi quan trọng

- Admin user management UI, contract và regression tests.
- Auth E2E flow cùng các sửa lỗi Identity/bootstrapping thực tế.
- Security tests, CSP hardening và báo cáo test bảo mật.
- `AuthApplicationCoverageTests.cs` và chuẩn hóa LF cho OpenAPI snapshot assertion trên Windows.

## 8. Tóm tắt kiểm thử

| Gate đã xác minh | Kết quả |
|---|---|
| Auth E2E Playwright | 5/5 PASS, 1,4 phút, official isolated stack |
| Security | 63/63 PASS |
| Application Auth coverage | 424/459, 92,37% |
| Admin users frontend | 10/10 PASS |
| Frontend Jest | 59 suites, 399 tests PASS |
| Frontend build | 22/22 static pages PASS |
| API integration monolith | Không có summary do runner batch lifecycle; targeted security 63/63 và self-lock regression 1/1 PASS |
| Performance regression | 5/5 PASS |
| Domain unit suite | 81/81 PASS |
| Application unit suite | 162/162 PASS |

## 9. Git và workflow

Các checkpoint scratch được transfer với `git cherry-pick -n`: `72710a8` đã staged trước transfer, tiếp theo `55de98d`, `0ed0621`, `6e9005a` và `ef39404`. Hai conflict được giải theo develop/Buổi 7 là nguồn chân lý: `DependencyInjection.cs` giữ DefaultTokenProviders và thời hạn reset token hiện hành; `OpenApiDocumentTests.cs` giữ chuẩn hóa LF hiện hành. Không tạo migration `B8_Auth_UserSearchTrgm`, không mang compose/env scratch vào commit, không claim tag hay PR.

## 10. Evidence hình ảnh

Không có ảnh chụp sanitized theo convention repository để nhúng vào báo cáo. Báo cáo tham chiếu evidence kiểm thử đã kiểm chứng, đặc biệt `docs/security-test-report.md`, kết quả Playwright, Coverlet và EXPLAIN ANALYZE; không tạo screenshot giả hoặc chứa credential/token.
