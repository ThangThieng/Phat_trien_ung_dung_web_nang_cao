# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

Lab: **02**  
Từ ngày: **16/09/2026** đến ngày: **22/09/2026**

MSSV: **2314236**  
Họ và tên: **Hoàng Bình Quân**  
Nhóm: **20**  
Vai trò: **Dev 1**

## Công việc

| STT | Công việc được giao | Nhánh | Tiến độ |
|---:|---|---|---:|
| 1 | FR-AUTH-003: Đăng nhập/đăng ký bằng Google theo ID Token flow | [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3) | 100% |
| 2 | FR-AUTH-005: Đăng xuất và thu hồi refresh token | [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3) | 100% |
| 3 | Retrofit D-1: chuẩn hóa `displayName`, backend tự sinh `UserName` | [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3) | 100% |
| 4 | Phối hợp D-11: validation trả HTTP 400 thay cho 422 | [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3) | 100% |

## 1. Tóm tắt

Buổi 2 hoàn thiện hai chức năng xác thực tiếp theo của Dev 1: Google Sign-In và logout có thu hồi refresh token. Đồng thời phần đăng ký từ Buổi 1 được retrofit theo SRS v1.2.0: client chỉ gửi `displayName`, `email`, `password`; `UserName` được sinh ở backend từ phần trước dấu `@` của email và thêm hậu tố số khi trùng.

Google dùng **ID Token flow**: frontend lấy `credential` từ Google Identity Services và gửi `{ idToken }` về API. Backend tự xác thực token với `GoogleJsonWebSignature.ValidateAsync`, gồm chữ ký, issuer, audience và thời hạn; không tin email do frontend tự khai báo.

## 2. Chi tiết công việc đã thực hiện

### 2.1 FR-AUTH-003 - Đăng nhập/đăng ký bằng Google

| Tầng | Thành phần |
|---|---|
| Application | `GoogleLoginCommand`, validator và handler phát cặp token qua `AuthResponseFactory`. |
| Infrastructure | `GoogleAuthOptions`, `IdentityService.AuthenticateGoogleAsync`; xác minh ID token, tìm/liên kết `UserLoginInfo("Google", subject)`, tạo user Author khi chưa có. |
| API | `POST /api/v1/auth/google`, trả `AuthResponseDto`; mô tả đủ lỗi 400/401/403/502. |
| Frontend | `@react-oauth/google`, `GoogleOAuthProvider`, component `GoogleSignIn` trong form login. |

Quy tắc đã áp dụng:

- ID token thiếu/sai định dạng trả validation 400; token không hợp lệ hoặc hết hạn trả `AUTH_GOOGLE_TOKEN_INVALID` (401).
- Google không truy cập được để xác minh token trả `AUTH_GOOGLE_UNAVAILABLE` (502).
- Chỉ liên kết tài khoản khi `email_verified = true`; tài khoản có cùng email được liên kết thay vì tạo bản ghi trùng.
- Tài khoản bị vô hiệu hóa trả `AUTH_ACCOUNT_DISABLED` (403).
- User mới nhận role `Author`, `DisplayName`/avatar lấy từ payload Google, và `UserName` được sinh tự động.

### 2.2 FR-AUTH-005 - Đăng xuất và thu hồi refresh token

`POST /api/v1/auth/logout` yêu cầu JWT Bearer và nhận `refreshToken`. Backend hash token SHA-256 rồi chỉ thu hồi token thuộc chính `currentUser`; dù token không tồn tại, đã bị thu hồi, hoặc thuộc người dùng khác, API vẫn trả `204 No Content`. Thiết kế idempotent này không biến endpoint thành oracle để dò token hợp lệ.

Frontend gọi API logout trước, sau đó luôn xóa session phía client trong `finally`, nên người dùng vẫn được thoát an toàn khi mạng có lỗi.

### 2.3 Retrofit D-1 và D-11

- Hợp đồng `RegisterRequest`, `RegisterUserCommand`, `UserDto` và kiểu TypeScript đổi sang `displayName`; bỏ `fullName` và `userName` khỏi API response/client form.
- `IdentityService` chuẩn hóa username từ email về chữ thường, chỉ giữ ký tự Identity cho phép, dùng `user` làm fallback và thêm `2`, `3`, ... khi trùng.
- Mã `AUTH_USERNAME_EXISTS` bị xóa vì username không còn do client cung cấp.
- `GlobalExceptionMiddleware`, metadata OpenAPI và form frontend dùng HTTP 400 cho validation, đúng D-11/SRS v1.2.0.

## 3. Kết quả kiểm thử

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build CulinaryBlog.sln --no-restore` | Thành công, 0 warning / 0 error. |
| `dotnet test tests/CulinaryBlog.Application.UnitTests/...` | 27/27 test passed. |
| `npm run lint` | Thành công. |
| `npm run typecheck` | Thành công. |
| `npm test -- --runInBand` | 14/14 test passed. |

Lưu ý: để thử Google Sign-In trên trình duyệt, cần tạo OAuth Web Client trong Google Cloud Console, khai báo origin của frontend và đặt cùng Client ID tại `GOOGLE_CLIENT_ID` (backend) và `NEXT_PUBLIC_GOOGLE_CLIENT_ID` (frontend). Không commit các giá trị bí mật vào repository.

## 4. Thành phần và file chính

- `GoogleLoginCommand.cs`, `LogoutCommand.cs`.
- `IdentityService.cs`, `GoogleAuthOptions.cs`, `RefreshTokenRepository.cs`.
- `AuthEndpoints.cs`, `AppException.cs`, `ErrorCodes.cs`, `GlobalExceptionMiddleware.cs`.
- `auth-context.tsx`, `GoogleSignIn.tsx`, `LoginForm.tsx`, `SiteHeader.tsx`.
- `RegisterUserCommand.cs`, `AuthContracts.cs`, `RegisterForm.tsx`, schema và test liên quan.

## 5. Kiến trúc áp dụng

- Clean Architecture và CQRS/MediatR: API chỉ map request sang command; logic xác thực nằm ở Application/Infrastructure.
- ASP.NET Core Identity giữ vai trò quản lý người dùng, role và external login.
- Google ID Token được kiểm chứng ở backend bằng thư viện chính thức `Google.Apis.Auth`.
- Refresh token vẫn chỉ tồn tại dạng SHA-256 hash trong database.
- Frontend dùng React Hook Form/Zod, `@react-oauth/google` và context xác thực dùng chung.

## 6. Đối chiếu SRS và phần việc tiếp theo

Các yêu cầu Buổi 2 của Dev 1 (FR-AUTH-003, FR-AUTH-005, D-1) đã được xử lý. Các phần Dev 1 còn theo kế hoạch là FR-AUTH-004/D-2/D-12 ở Buổi 3, profile ở Buổi 4 và các yêu cầu bảo mật/quản trị ở các buổi sau.

## 7. Minh chứng GitHub

- Repository: [Phat_trien_ung_dung_web_nang_cao](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao)
- Nhánh Buổi 2 của Dev 1: [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3)
- Commit hoàn thành: [369cdfa](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/369cdfa)
- Nội dung commit: `feat(auth): complete Google sign-in, logout revocation and displayName contract`

## 8. Kết luận

Phần việc Dev 1 của Buổi 2 hoàn thành end-to-end từ API, domain/application, infrastructure đến frontend. Luồng Google không dựa vào dữ liệu frontend tự khai báo; logout đã thu hồi token ở server thay vì chỉ xóa dữ liệu cục bộ. Báo cáo này và mã nguồn được commit trên nhánh `2314236_HoangBinhQuan_buoiso3`.
