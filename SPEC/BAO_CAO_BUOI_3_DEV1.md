# Báo cáo kiểm chứng Buổi 3 — Dev 1

Ngày kiểm tra: 05/10/2026. Phạm vi: FR-AUTH-004, D-2, D-12 và IP thật theo `KE_HOACH_PHAT_TRIEN_7_BUOI.md`.

## Phần bổ sung

- Mở entry point `Program` cho `WebApplicationFactory` để test chạy API thực tế.
- Thêm `CulinaryBlogApiFactory`: PostgreSQL 16 và Redis 7 qua Testcontainers, chạy migration thật; tắt worker và khởi tạo MinIO, thay lịch gửi email bằng bản không gửi. Các test dùng email riêng, không đụng dữ liệu Docker Compose đang có. Container test được dọn khi fixture kết thúc.
- Thêm 7 integration test trong `RefreshTokenTests.cs`:
  1. Rotation tạo token 32 byte, thu hồi token cũ, lưu hash và liên kết token thay thế.
  2. Dùng lại token cũ trả 401 `AUTH_REFRESH_TOKEN_REVOKED`, thu hồi cả phiên khác của cùng user; không ảnh hưởng user khác.
  3. Token không tồn tại trả 401 `AUTH_TOKEN_INVALID`.
  4. Token hết hạn trả 401 `AUTH_REFRESH_TOKEN_EXPIRED`.
  5. Tài khoản bị vô hiệu hóa không refresh được, trả 403 `AUTH_ACCOUNT_DISABLED`.
  6. Proxy thuộc mạng tin cậy ghi IP từ `X-Forwarded-For` vào `CreatedByIp`.
  7. Proxy ngoài mạng tin cậy không giả mạo được `CreatedByIp` qua header này.
- Thêm 5 test frontend: single-flight cho 5 request hết hạn cùng lúc; xử lý refresh thất bại; không refresh token sai/không lặp retry vô hạn; không lưu access token bền; tải lại trang chỉ refresh một lần kể cả React StrictMode và loại access token của phiên lưu kiểu cũ.
- Jest bỏ qua `.next` khi lập danh sách module, tránh trùng tên package sau production build.
- Docker Compose đổi MinIO sang `quay.io/minio/minio:latest` theo [hướng dẫn Docker của MinIO](https://github.com/minio/minio/blob/master/docs/docker/README.md), vì `minio/minio:latest` trả lỗi pull access denied trong lần chạy kiểm tra.

## Kết quả đã xác minh

| Kiểm tra | Kết quả |
|---|---|
| Build toàn bộ backend | Thành công, 0 warning, 0 error |
| Domain unit test | 14/14 đạt |
| Application unit test | 27/27 đạt |
| Architecture test | 3/3 đạt |
| API integration test trên PostgreSQL/Redis thật | 7/7 đạt, không skip |
| Frontend lint | Đạt |
| Frontend Jest | 19/19 đạt, 4 suite |
| Frontend production build | Đạt, gồm kiểm tra kiểu TypeScript |

Lệnh chạy lại (Docker Desktop phải hoạt động):

```powershell
cd backend
dotnet build CulinaryBlog.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet test CulinaryBlog.sln --no-build --no-restore -m:1
cd ../frontend
npm run lint
npm test -- --runInBand
npm run build
```

Nếu máy mới chưa restore package, chạy `dotnet restore` và `npm ci` trước các lệnh trên. `UseSharedCompilation=false` tránh timeout kết nối compiler server đã gặp trên máy kiểm tra. Trong sandbox Windows, test Docker và production build Next.js cần quyền tạo tiến trình/kết nối Docker.

## Giới hạn nghiệm thu

Các kết quả trên xác nhận mã nguồn và test tự động của Dev1 Buổi 3. Chưa xác nhận toàn stack Docker, Scalar và UI thủ công: lần build Docker đầu gặp TLS handshake timeout khi tải image .NET; lần thử lại vẫn đang tải image khi lượt làm việc bị ngắt. Không suy ra các bước đó đã đạt chỉ từ test xanh.

Các bổ sung kiểm thử và báo cáo được commit trên nhánh `2314236_HoangBinhQuan_buoiso4`; chưa tạo PR.
