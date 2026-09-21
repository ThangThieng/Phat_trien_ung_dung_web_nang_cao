# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

Lab: **01**  
Từ ngày: **09/09/2026** đến ngày: **15/09/2026**

MSSV: **2314236**  
Họ và tên: **Hoàng Bình Quân**

Nhóm: **20**

## Công việc:

| STT | Công việc được giao | Liên kết đến github branch | Tiến độ % |
|---:|---|---|---:|
| 1 | Module xác thực người dùng - FR-AUTH-001: Đăng ký tài khoản | [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)  
Commit Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) | 100% |
| 2 | Module xác thực người dùng - FR-AUTH-002: Đăng nhập bằng email/mật khẩu | [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)  
Commit Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) | 100% |
| 3 | Xây dựng lớp Identity, JWT và refresh token phục vụ FR-AUTH-001/002 | [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)  
Commit Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) | 100% |
| 4 | Xây dựng giao diện đăng ký, đăng nhập và quản lý trạng thái xác thực phía Frontend | [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)  
Commit Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) | 100% |
| 5 | Kiểm thử luồng đăng ký/đăng nhập, validation dữ liệu và các trường hợp lỗi xác thực | [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)  
Commit Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6) | 100% |

## 1. Tóm tắt

Trong Buổi 1, Dev 1 phụ trách hoàn thành hai yêu cầu chức năng xác thực đầu tiên của hệ thống Culinary Blog:

- **FR-AUTH-001:** Đăng ký tài khoản mới.
- **FR-AUTH-002:** Đăng nhập bằng email và mật khẩu.

Phần việc đã được triển khai theo kiến trúc Clean Architecture, CQRS + MediatR, ASP.NET Core Identity và JWT. Code được commit trong commit `3d40be6` với thông điệp `feat(auth): complete FR-AUTH-001 & 002 register login flow`.

## 2. Chi tiết công việc đã thực hiện

### 2.1 FR-AUTH-001 - Đăng ký tài khoản

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| Domain | Entity `RefreshToken` và các quy tắc dữ liệu liên quan đến vòng đời token. |
| Application | `RegisterUserCommand`, `RegisterUserCommandValidator`, `RegisterUserCommandHandler`, `AuthResponseFactory`, `PasswordRules`. |
| Application contracts | `IIdentityService`, `ITokenService`, `IRefreshTokenRepository`, `IWelcomeEmailScheduler`. |
| Infrastructure | `ApplicationUser`, `IdentityService` sử dụng `UserManager`, `JwtTokenService`, `JwtOptions`, cấu hình ASP.NET Core Identity. |
| API | Endpoint `POST /api/v1/auth/register`, trả HTTP 201 khi đăng ký thành công. |
| Frontend | `RegisterForm`, schema Zod, trang `/auth/register`, `AuthProvider` và hook `useAuth`. |

**Luồng chính đã hoàn thành:**

1. Nhận thông tin đăng ký từ client.
2. Validate email, tên hiển thị, username và mật khẩu qua FluentValidation/Zod.
3. Kiểm tra email không bị trùng.
4. Tạo tài khoản bằng ASP.NET Core Identity.
5. Hash mật khẩu bằng PBKDF2-HMAC-SHA512 của Identity.
6. Gán role mặc định `Author`.
7. Phát access token và refresh token.
8. Lưu refresh token dưới dạng SHA-256 hash trong cơ sở dữ liệu.
9. Trả về `AuthResponseDto` cho client.

### 2.2 FR-AUTH-002 - Đăng nhập bằng email/mật khẩu

**Đã làm:**

| Tầng | Thành phần |
|---|---|
| Application | `LoginUserCommand`, `LoginUserCommandValidator`, `LoginUserCommandHandler`, xử lý kiểm tra thông tin đăng nhập và phát token. |
| Infrastructure | `IdentityService` dùng `UserManager.CheckPasswordAsync`, cấu hình lockout và password policy. |
| API | Endpoint `POST /api/v1/auth/login`, trả HTTP 200 khi đăng nhập thành công. |
| Frontend | `LoginForm`, schema Zod, trang `/auth/login`, hiển thị trạng thái đăng nhập trên header. |

**Các quy tắc bảo mật đã hoàn thành:**

- Không tiết lộ tài khoản có tồn tại hay không khi email/mật khẩu sai.
- Sau 5 lần nhập sai, tài khoản bị khóa trong 15 phút.
- Mật khẩu được kiểm tra theo chính sách độ mạnh của ASP.NET Core Identity.
- Access token có thời hạn 15 phút.
- Refresh token có thời hạn 7 ngày.
- Database chỉ lưu SHA-256 hash của refresh token, không lưu raw token.

## 3. Kết quả kiểm thử thực tế

| Kịch bản | Kết quả |
|---|---|
| Đăng ký hợp lệ | HTTP 201, trả access token, refresh token và role `Author`. |
| Email đã tồn tại | HTTP 409, mã lỗi `AUTH_EMAIL_EXISTS`. |
| Dữ liệu đăng ký không hợp lệ | Trả lỗi validation theo từng field. |
| Đăng nhập đúng thông tin | HTTP 200. |
| Sai mật khẩu lần 1-4 | HTTP 401, mã lỗi `AUTH_INVALID_CREDENTIALS`, thông báo dùng chung. |
| Sai mật khẩu lần 5 | HTTP 423, mã lỗi `AUTH_ACCOUNT_LOCKED`, khóa 15 phút. |
| Sai mật khẩu sau khi bị khóa | Vẫn bị từ chối cho đến khi hết thời gian lockout. |
| Email không tồn tại | HTTP 401 với thông báo giống trường hợp sai mật khẩu. |
| Hash mật khẩu trong DB | Identity V3 PBKDF2-HMAC-SHA512, tiền tố `AQAAAAIA`. |
| Refresh token trong DB | Chỉ lưu chuỗi SHA-256 dạng hex 64 ký tự. |
| Validation Frontend | React Hook Form + Zod kiểm tra dữ liệu trước khi gửi API. |

## 4. Thành phần và file chính

- `RegisterUserCommand` và các lớp validator/handler.
- `LoginUserCommand` và các lớp validator/handler.
- `AuthResponseFactory`.
- `JwtTokenService` và `JwtOptions`.
- `ApplicationUser` và `IdentityService`.
- `RefreshToken` và `RefreshTokenRepository`.
- `AuthEndpoints`.
- `AuthenticationExtensions`.
- `auth-context.tsx`.
- `LoginForm`, `RegisterForm`, `AuthCard`, `FormField`.
- Các schema validation bằng Zod và Jest test tương ứng.

## 5. Kiến trúc áp dụng

- Backend dùng .NET 10 Minimal API, không dùng MVC Controller.
- Tổ chức code theo Clean Architecture: Domain, Application, Infrastructure và API.
- Use case được triển khai bằng Command/Handler qua MediatR.
- Validation được thực hiện ở Application Layer.
- Xác thực API sử dụng JWT Bearer.
- Quản lý tài khoản sử dụng ASP.NET Core Identity.
- Frontend sử dụng Next.js 15 App Router, TypeScript, React Hook Form và Zod.

## 6. Ghi chú đối chiếu SRS v1.2.0

Phần xác thực Buổi 1 đã chạy theo SRS v1.0.0 tại thời điểm thực hiện. Khi đối chiếu với SRS v1.2.0, các điểm sau được ghi nhận là nợ kỹ thuật cho các buổi tiếp theo:

| Mã | Hiện trạng Buổi 1 | Yêu cầu SRS v1.2.0 | Dự kiến xử lý |
|---|---|---|---|
| D-1 | Form đăng ký còn dùng `fullName` và `userName` do người dùng nhập. | Dùng `displayName`; backend tự sinh `UserName` từ email. | Buổi 2, Dev 1. |
| D-2 | Refresh token đang sinh 512-bit. | Chuẩn hóa thành 256-bit. | Buổi 3, Dev 1. |
| D-11 | Một số validation của code Buổi 1 trả HTTP 422. | Chuẩn hóa validation về HTTP 400, conflict về HTTP 409. | Commit nền Buổi 2. |
| D-12 | Access token và refresh token được lưu ở `localStorage`. | Access token chỉ lưu trong bộ nhớ; refresh token là token lưu bền duy nhất theo thiết kế đã duyệt. | Buổi 3, Dev 1. |
| D-13 | TTL cache danh mục đang là 60 phút ở phần code hiện có. | SRS v1.2.0 chốt TTL `categories:all` là 30 phút. | Buổi 2, Dev 1 phối hợp nhóm. |

Các nợ kỹ thuật trên không làm thay đổi phạm vi hoàn thành của Lab 01; chúng được ghi nhận để tiếp tục retrofit theo SRS v1.2.0 và kế hoạch phát triển 7 buổi.

## 7. Minh chứng GitHub

- Repository: [Phat_trien_ung_dung_web_nang_cao](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao)
- Nhánh Buổi 2 của Dev 1: [2314236_HoangBinhQuan_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso2)
- Commit phần xác thực Buổi 1: [3d40be6](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/3d40be6)
- Nội dung commit: `feat(auth): complete FR-AUTH-001 & 002 register login flow`

## 8. Kết luận

Phần việc Dev 1 trong Lab 01 đã hoàn thành 100%. Hai chức năng đăng ký và đăng nhập local đã được triển khai từ database, domain, application, infrastructure, API đến frontend; có validation, phân quyền role Author, JWT, lockout và bảo vệ refresh token bằng hash SHA-256.

Các thay đổi đã được commit trong Git với commit `3d40be6`. Báo cáo cá nhân này được tạo trên nhánh `2314236_HoangBinhQuan_buoiso2` để làm minh chứng nộp Lab.
