# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

Lab: **03**  
Từ ngày: **23/09/2026** đến ngày: **29/09/2026**

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

Buổi 3 hoàn thiện hai chức năng xác thực tiếp theo của Dev 1: Google Sign-In và logout có thu hồi refresh token. Đồng thời phần đăng ký từ Buổi 1 được retrofit theo SRS v1.2.0: client chỉ gửi `displayName`, `email`, `password`; `UserName` được sinh ở backend từ phần trước dấu `@` của email và thêm hậu tố số khi trùng.

Google dùng **ID Token flow**: frontend lấy `credential` từ Google Identity Services và gửi `{ idToken }` về API. Backend tự xác thực token với `GoogleJsonWebSignature.ValidateAsync`, gồm chữ ký, issuer, audience và thời hạn; không tin email do frontend tự khai báo.

## 2. Chi tiết công việc đã thực hiện

### 2.1 FR-AUTH-003 - Đăng nhập/đăng ký bằng Google

| Tầng | Thành phần |
|---|---|
| Application | `GoogleLoginCommand`, validator và handler phát cặp token qua `AuthResponseFactory`. |
| Infrastructure | `GoogleAuthOptions`, `GoogleIdTokenValidator` (xác minh ID token), `IdentityService.FindOrCreateGoogleUserAsync` (tìm/liên kết `UserLoginInfo("Google", subject)`, tạo user Author khi chưa có — trong một transaction). |
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
- `UserNameGenerator` chuẩn hóa username từ email về chữ thường, chỉ giữ ký tự Identity cho phép, dùng `user` làm fallback và thêm `2`, `3`, ... khi trùng.
- Mã `AUTH_USERNAME_EXISTS` bị xóa vì username không còn do client cung cấp.
- D-11 (validation trả HTTP 400) dùng commit nền chung của Dev 4 (`mapProblemDetailsToForm()` ở frontend); phần tự làm trùng đã bỏ khi hợp nhất với mốc M1.

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
- `Domain/Exceptions/Auth/*` (6 lớp lỗi Auth), `AuthExceptionMappings.cs`.
- `IUserRepository.cs` / `UserRepository.cs` (thay `IRefreshTokenRepository`), `IUnitOfWork.Users`.
- `IdentityService.cs`, `UserNameGenerator.cs`, `GoogleIdTokenValidator.cs`, `GoogleAuthOptions.cs`.
- `AuthEndpoints.cs`.
- `auth-context.tsx`, `GoogleSignIn.tsx`, `LoginForm.tsx`, `SiteHeader.tsx`.
- `RegisterUserCommand.cs`, `AuthContracts.cs`, `RegisterForm.tsx`, schema và test liên quan.

## 5. Kiến trúc áp dụng

- Clean Architecture và CQRS/MediatR: API chỉ map request sang command; logic xác thực nằm ở Application/Infrastructure.
- ASP.NET Core Identity giữ vai trò quản lý người dùng, role và external login.
- Google ID Token được kiểm chứng ở backend bằng thư viện chính thức `Google.Apis.Auth`.
- Refresh token vẫn chỉ tồn tại dạng SHA-256 hash trong database.
- Frontend dùng React Hook Form/Zod, `@react-oauth/google` và context xác thực dùng chung.

## 6. Đối chiếu SRS và phần việc tiếp theo

Các yêu cầu Buổi 3 của Dev 1 (FR-AUTH-003, FR-AUTH-005, D-1, cụm lỗi Auth, `IUserRepository`) đã được xử lý. Theo kế hoạch viết lại ngày 29/09, Buổi 4 Dev 1 làm phần API của FR-AUTH-004 (refresh + D-2 + claim `sid`), FR-AUTH-006/007 (hồ sơ), FR-AUTH-008 (quản lý tài khoản) và FR-AUTH-009 (quản lý phiên); D-12 và giao diện chuyển sang Buổi 5.

## 7. Minh chứng GitHub

- Repository: [Phat_trien_ung_dung_web_nang_cao](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao)
- Nhánh Buổi 3 của Dev 1: [2314236_HoangBinhQuan_buoiso3](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314236_HoangBinhQuan_buoiso3)
- Commit hoàn thành: [369cdfa](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/369cdfa)
- Nội dung commit: `feat(auth): complete Google sign-in, logout revocation and displayName contract`

## 8. Kết luận

Phần việc Dev 1 của Buổi 3 hoàn thành end-to-end từ API, domain/application, infrastructure đến frontend. Luồng Google không dựa vào dữ liệu frontend tự khai báo; logout đã thu hồi token ở server thay vì chỉ xóa dữ liệu cục bộ. Báo cáo này và mã nguồn được commit trên nhánh `2314236_HoangBinhQuan_buoiso3`.

## 9. Khắc phục sau review ngày 29/09/2026

Review trên commit `369cdfa` kết luận nhánh **chưa đạt để merge**. Các mục dưới đây đã được **Nguyễn Thăng Thiêng (Dev 4, trưởng nhóm)** sửa trên nhánh này ngày 30/09/2026 trong một commit riêng, đứng tên người sửa; các commit gốc của Dev 1 giữ nguyên.

| Mục review | Đã xử lý |
|---|---|
| 2 – Thiếu integration test | `GoogleLoginAndLogoutEndpointsTests`: Google 200 / 400 (sai định dạng, thiếu email, email chưa xác minh) / 401 / 403 / 502, liên kết email đã xác minh, `an.nguyen` → `an.nguyen2`, 3 người đăng ký đồng thời cùng tiền tố, logout 2 lần đều 204 và DB có `RevokedAt`; hồi quy 423 giữ ở `AuthEndpointsTests`. `IGoogleIdTokenValidator` tách khỏi handler, test dùng bản giả. |
| 3 – `UserDto` thiếu `bio` | Thêm `bio` ở backend và `frontend/src/types/api.ts`. |
| 4, 5 – Xung đột D-11, nhánh cũ | Hợp nhất với mốc M1 (nhánh B3 của Dev 4); phần D-11 lấy bản dùng chung. |
| 6 – Báo cáo sai buổi | Đổi tên thành `BAO_CAO_LAB_03_...`, sửa số lab và ngày. |
| 7 – Thiếu `ClientId` bị báo như Google sập | `GoogleIdTokenValidator` ghi log mức Error nêu rõ khóa `Authentication:Google:ClientId`; người dùng nhận thông báo đăng nhập bằng email. |
| 8 – Tạo user Google gồm 3 lần ghi rời | `CreateAsync` → `AddToRoleAsync` → `AddLoginAsync` (và đăng ký thường) chạy trong `IUnitOfWork.ExecuteInTransactionAsync`. |
| 9, 11 – `"Author"` viết cứng, chữ cũ | Dùng `Roles.Author`; thông báo đổi thành "Tên hiển thị"; bỏ chú thích "HTTP 422". |
| 10 – Sinh `UserName` nhiều truy vấn, đua khi đăng ký đồng thời | Một truy vấn `GetUserNamesStartingWithAsync`; trùng tên do đăng ký đồng thời thì thử lại với hậu tố kế tiếp. |
| Kế hoạch Buổi 3 bước 1–3 | Cụm `Domain/Exceptions/Auth` (6 lớp) + `AuthExceptionMappings`; `IUserRepository` thay `IRefreshTokenRepository`, gắn vào `IUnitOfWork.Users`; `RefreshToken.Revoke(utcNow, replacedByTokenHash)`; xóa mã `AUTH_USERNAME_EXISTS`. |

**Còn phải tự làm (không ai làm thay được):**
- **Mục 1 – danh tính commit.** Commit `369cdfa`, `8511d8e` vẫn mang tác giả `Nguyen Van Teo <nvteo@gmail.com>`, GitHub không ghi nhận cho tài khoản `2314236HoangBinhQuan`. Cần đặt lại `git config user.name/user.email` bằng email đã gắn GitHub rồi sửa tác giả các commit này.
- Xuất bản `.docx` của báo cáo này theo mẫu nộp bài.

**Hạn chế đã biết (review mục 12):** nút Đăng xuất gọi API bằng access token hiện tại. Nếu access token đã hết hạn (sau 15 phút), API trả 401 và refresh token **không bị thu hồi ở server**, chỉ bị xóa ở client. Sẽ hết khi có interceptor làm mới token ở Buổi 5 (D-12).

**Kiểm chứng sau khi sửa (30/09/2026):** `dotnet build` 0 warning; Domain 14, Application 27, Architecture 15, Integration 70 test xanh (1 Skip có chủ đích: tái hiện lỗ hổng MT-34, chờ Dev 3 vá ở Buổi 4).
