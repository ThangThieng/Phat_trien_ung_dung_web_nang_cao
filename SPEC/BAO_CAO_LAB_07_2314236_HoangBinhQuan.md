# BÁO CÁO LAB 07 — KHÔI PHỤC MẬT KHẨU VÀ PHIÊN HỒ SƠ

**Sinh viên:** Hoàng Bình Quân

**MSSV:** 2314236

**Vai trò:** Dev1 — Authentication / Authorization / User Management / Security

**Nhánh:** `2314236_HoangBinhQuan_buoiso7`
**Phạm vi:** Dev1 Buổi 7 — khôi phục mật khẩu, thay đổi mật khẩu, quản lý phiên và trải nghiệm hồ sơ.

## 1. Kết quả

Lab 07 bổ sung luồng quên mật khẩu và đặt lại mật khẩu hoàn chỉnh qua background job, email SMTP và giao diện frontend. Phần hồ sơ được kiểm chứng về cập nhật avatar, rollback khi cập nhật hồ sơ thất bại và quản lý phiên. Các thay đổi không làm thay đổi `nginx/` hoặc `frontend/src/lib/api-client.ts`.

## 2. Change password

- Sai `currentPassword` trả HTTP 400 với lỗi field `errors.currentPassword`.
- Tài khoản Google-only không thể đổi mật khẩu cục bộ và trả HTTP 400.
- Đổi mật khẩu thành công trả HTTP 204; mật khẩu cũ bị từ chối.
- Phiên refresh hiện tại vẫn dùng được, còn các phiên khác bị revoke.

## 3. Forgot password và an toàn token

- `POST /api/v1/auth/forgot-password` trả HTTP 202 cho cả email tồn tại và không tồn tại, không tiết lộ trạng thái tài khoản.
- Email không tồn tại không enqueue job; email tồn tại enqueue đúng một `PasswordResetEmailJob`.
- Background job chỉ nhận `userId` và correlation ID; reset token được tạo bên trong job, không được đưa vào Hangfire arguments hoặc log.
- Correlation ID chỉ chấp nhận ký tự `[A-Za-z0-9-]` dài 1–64; header sai hoặc quá dài fallback về server trace ID.
- Forgot-password rate limit trả HTTP 429 khi vượt quota.

## 4. Reset password và domain error contract

- Identity đăng ký `AddDefaultTokenProviders()`; password-reset token có default lifespan 30 phút.
- Email/reset integration thật tạo link reset; reset thành công revoke các refresh session trước đó và đặt `AccessFailedCount` về 0.
- Mật khẩu mới đăng nhập được; mật khẩu cũ bị từ chối.
- Token reused, malformed, unknown hoặc expired cùng trả `AUTH_PASSWORD_RESET_TOKEN_INVALID`; tài khoản disabled trả `AUTH_ACCOUNT_DISABLED`.
- `PasswordResetTokenInvalidException` kế thừa `AuthDomainException`, dùng `ErrorCodes.AuthPasswordResetTokenInvalid` và được map HTTP 400.

## 5. Profile và session UX

- Hồ sơ dùng cache/query key riêng cho `GET /auth/me` và `/auth/sessions`.
- Cập nhật avatar theo optimistic flow: ảnh mới được giữ làm transient state; nếu PATCH hồ sơ thất bại, UI rollback về avatar trước đó và gọi xóa đúng key ảnh mới.
- Chỉ xóa file mới sau khi PATCH `/auth/me` thất bại; không suy diễn hoặc xóa file avatar cũ.
- Trường URL avatar thủ công đã bị bỏ; `bio` tối đa 1000 ký tự.
- Tabs có semantics truy cập được và hỗ trợ `ArrowLeft` / `ArrowRight`; session hiển thị IP, created, expiry và nhãn `Thiết bị này`.
- Thao tác xác nhận revoke session dùng hộp thoại người dùng trước khi gửi request.

## 6. Giới hạn đã xác nhận

`OLD_AVATAR_DELETE_CONTRACT_NOT_CONFIRMED`: contract hiện tại chỉ trả `avatarUrl`, không trả storage key đáng tin cậy của avatar cũ. Vì vậy Lab 07 không xóa avatar cũ một cách suy đoán; việc này cần backend contract trả key cũ hoặc endpoint xóa có quyền sở hữu rõ ràng.

## 7. Kiểm chứng Docker cô lập

Một stack Docker cục bộ độc lập được chạy với project `culinaryblog_buoi7_quan`, network và named volumes riêng; không sử dụng stack main hoặc Buổi 8. Các cổng host riêng là Nginx 3200, API 5200, frontend 3201, PostgreSQL 5445, Redis 6390, MinIO 9200/9201, Seq 5451 và MailHog 8225/1225. Sau kiểm chứng, chỉ stack này được `down -v` và các tài nguyên riêng của nó được xóa.

Kết quả thực tế trên stack cô lập:

| Hành vi | Kết quả |
|---|---:|
| API readiness | HTTP 200 |
| Frontend | HTTP 200 |
| MailHog API | HTTP 200 |
| Đăng ký tài khoản tổng hợp | thành công |
| Yêu cầu quên mật khẩu | thành công |
| Email reset tới MailHog | nhận được |
| Reset bằng link email | HTTP 204 |
| Đăng nhập bằng mật khẩu cũ | HTTP 401 |
| Đăng nhập bằng mật khẩu mới | HTTP 200 |
| Dùng lại reset token | HTTP 400 |

Token reset chỉ được decode từ email quoted-printable trong bộ nhớ của tiến trình kiểm chứng. Báo cáo, log và lệnh đầu ra không ghi token, email tổng hợp hoặc mật khẩu.

Các hành vi UX sau được kiểm chứng bằng Jest rendered-DOM, không phải browser manual: login hiển thị `Quên mật khẩu?` trỏ tới `/auth/forgot-password`; khi token reset invalid, UI hiển thị chính xác `Liên kết đã hết hạn hoặc đã được dùng`, recovery link trỏ tới `/auth/forgot-password`; reset thành công điều hướng `/auth/login`. Hai suite focused pass 2/2, 4/4 tests.

## 8. Kết quả kiểm thử

| Gate | Kết quả |
|---|---:|
| Backend build | PASS — 0 warning, 0 error |
| Domain tests | PASS — 81/81 |
| Application tests | PASS — 113/113 |
| Architecture tests | PASS — 18/18 |
| Integration inventory | PASS — 373/373, 0 fail, 0 skip |
| Password endpoint integration | PASS — 12/12 |
| Targeted backend regression | PASS — 20/20 |
| Frontend Jest | PASS — 38 suites, 287 tests |
| Frontend lint | PASS — 0 error; 2 intentional `window.confirm` warnings |
| Frontend typecheck | PASS |
| Frontend production build | PASS — 21/21 static pages |
| `git diff --check` | PASS |

## 9. Git workflow và kết luận

- Shared develop fix `a8c1f97` đã được merge qua `67c161d`; không có code Buổi 8 trong thay đổi này.
- Các flow password recovery và profile/session trong phạm vi Dev1 Buổi 7 đã được kiểm chứng bằng integration tests, frontend tests và stack Docker cô lập. Phần còn lại duy nhất liên quan avatar cũ được ghi nhận minh bạch ở mục 6 và không được xử lý bằng suy đoán trên client.
