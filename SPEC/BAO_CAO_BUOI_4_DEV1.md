# Báo cáo Buổi 4 — Dev 1: API phiên và tài khoản

Ngày kiểm chứng: **07/10/2026**. Thành viên: **2314236 — Hoàng Bình Quân**.

Nhánh bàn giao theo yêu cầu: `2314236_HoangBinhQuan_buoiso5`.
Phạm vi công việc vẫn là **Buổi 4** trong `KE_HOACH_PHAT_TRIEN_8_BUOI.md`, không phải phần giao diện Buổi 5.
Nguồn yêu cầu: `SRS_Culinary_Blog_v1.2.2.md`, FR-AUTH-004 và FR-AUTH-006 → 009.

## 1. Phạm vi API đã kiểm chứng

| Endpoint | Kết quả |
|---|---|
| `POST /api/v1/auth/refresh` | Không cần Bearer; token 32 byte, hash SHA-256; rotation nguyên tử; reuse detection; 401 cho token sai/hết hạn/thu hồi; 403 khi tài khoản bị vô hiệu hóa |
| `GET /api/v1/auth/me` | Bearer; hồ sơ không chứa trường nhạy cảm; user không còn tồn tại trả 401; `no-store` |
| `PATCH /api/v1/auth/me` | PATCH từng trường; bỏ qua email/username/roles; giữ trường không gửi; cho xóa bio/avatar bằng null; validation 400 |
| `GET /api/v1/users` | Chỉ Admin; phân trang, tìm theo email/tên và lọc trạng thái; DTO có roles/recipeCount; `no-store` |
| `PATCH /api/v1/users/{id}/status` | Chỉ Admin; không tự khóa; user không tồn tại trả 404 `AUTH_USER_NOT_FOUND`; khóa thu hồi mọi refresh token; mở lại không khôi phục token cũ |
| `GET /api/v1/auth/sessions` | Chỉ phiên đang hoạt động của người gọi; đánh dấu `isCurrent` bằng `sid`; không trả hash; `no-store` |
| `DELETE /api/v1/auth/sessions/{id}` | Phiên người khác trả 404; thu hồi lặp lại trả 204 |
| `POST /api/v1/auth/sessions/revoke-all` | Thu hồi mọi phiên của người gọi, bao gồm phiên hiện tại; không ảnh hưởng user khác |

Refresh token, `sid`, trusted forwarded IP và các endpoint đã được triển khai trong lịch sử nhánh gốc (`37539e2`, `5239618`). Lần bàn giao này kế thừa chúng, hoàn thiện validation/kiến trúc Auth và bổ sung bằng chứng kiểm thử cho các API hồ sơ, quản trị, phiên.

## 2. Thay đổi lần này

- Đưa kiểm tra hồ sơ, phân trang người dùng và lý do khóa tài khoản vào FluentValidation/`ValidationBehavior`.
- Sửa kiểm tra avatar: so sánh scheme, host, port và đường dẫn URI đã chuẩn hóa; giữ phân biệt hoa/thường của tên bucket; chặn user-info, bucket khác, đường dẫn `../` và biến thể mã hóa. Trước đây kiểm tra bằng tiền tố chuỗi có thể chấp nhận URL thoát khỏi bucket.
- Chặn phép tính offset phân trang tràn `int`, trả 400 thay vì lỗi truy vấn 500.
- Tách truy vấn danh sách sang `IUserRepository`/`UserRepository`, dùng `AsNoTracking`, lấy roles theo lô để tránh truy vấn lặp từng user. Thao tác ghi tài khoản vẫn qua `IIdentityService`/`UserManager`.
- Escape `%`, `_`, `\` trong tìm kiếm để khớp một phần văn bản theo đúng nghĩa literal, vẫn không phân biệt hoa/thường.
- Bổ sung `AuthDomainException`, `InvalidTokenException` (kèm factory hết hạn/thu hồi), `AccountDisabledException`, `UserNotFoundException`. Domain chỉ mang mã nghiệp vụ; ánh xạ HTTP nằm trong `API/Middleware/ExceptionMapping/AuthExceptionMappings.cs`.
- Thêm `AccountApiTests` và `ProfileValidatorTests`; không đổi schema, không cần migration.

### Quyết định tích hợp

- **Reuse detection:** tuân thủ SRS FR-AUTH-004 A3: đi theo `ReplacedByTokenHash`, thu hồi family liên quan, giữ phiên đăng nhập độc lập. Không dùng cách diễn giải “thu hồi mọi phiên” trong một số đoạn của kế hoạch. Khóa tài khoản và revoke-all vẫn thu hồi mọi phiên.
- Nhánh hiện tại dùng `IRefreshTokenRepository` chuyên biệt và `IUnitOfWork.SaveChangesAsync`; rotation và khóa tài khoản có transaction/execution strategy ở Infrastructure. Giữ ranh giới này cùng các test cạnh tranh/rollback hiện có. Việc hợp nhất toàn bộ repository vào `IUnitOfWork.Users` và nền `ExceptionStatusMap` dùng chung theo kế hoạch Buổi 3 cần phối hợp commit nền của Dev 4; báo cáo này không coi đó là nền kiến trúc toàn nhóm đã hoàn tất.
- Không gộp các thay đổi có sẵn của người dùng trong `README.md` và `BAO_CAO_LAB_02_2314236_HoangBinhQuan.md` vào commit này.

## 3. Kết quả kiểm chứng

| Kiểm tra | Kết quả |
|---|---|
| Backend build | Thành công, 0 warning, 0 error |
| API integration, PostgreSQL/Redis qua Testcontainers | **47/47**, không skip |
| Application unit tests | **41/41** |
| Domain unit tests | **14/14** |
| Architecture tests hiện có | **3/3** |
| Tổng backend | **105/105** |
| Frontend lint | Đạt |
| Frontend Jest | **40/40**, 6 suite |
| Frontend production build | Đạt, gồm TypeScript |
| `docker compose --profile dev up -d --build` | Thành công với mã nguồn cuối |
| Docker Compose | 9 service running; PostgreSQL, Redis, MinIO có healthcheck và đều healthy |
| HTTP qua Nginx | `/`, `/auth/login`, `/scalar`, `/openapi/v1.json` đều 200 |
| OpenAPI chạy thật | Có đủ 8 endpoint Dev1 Buổi 4 |
| Smoke qua Nginx | Đăng ký → xem/sửa hồ sơ → refresh → phiên hiện tại → revoke-all → danh sách rỗng: đạt |

Smoke test tạo tài khoản local có email tiền tố `dev1-b4-` và thu hồi toàn bộ phiên sau kiểm tra. Không ghi token/mật khẩu ra báo cáo hoặc commit.

Lệnh chạy lại:

```powershell
cd backend
dotnet build CulinaryBlog.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet test CulinaryBlog.sln --no-build --no-restore -m:1
cd ../frontend
npm run lint
npm test -- --runInBand
npm run build
cd ..
docker compose --profile dev up -d --build
```

Docker Desktop phải hoạt động. Testcontainers và Next.js production build cần quyền kết nối Docker/tạo tiến trình ngoài sandbox Windows. Máy mới cần `dotnet restore` và `npm ci` trước.

## 4. Giới hạn nghiệm thu

Kết quả trên xác nhận phần API của Dev1. Không suy ra 45/45 endpoint của cả nhóm đã hoàn tất. Giao diện hồ sơ, quản trị tài khoản và quản lý phiên thuộc các buổi sau. `/health` và nền kiến trúc chung của Dev4 chưa có đầy đủ trên nhánh gốc, nên không ghi nhận tiêu chí toàn nhóm đó là đạt. Kiểm tra trang web ở đây là HTTP smoke, không phải kiểm thử thao tác giao diện bằng trình duyệt.
