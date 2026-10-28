# Architecture Decision Records (ADR) — Culinary Blog

NFR-MAINT-003 yêu cầu ghi lại mọi quyết định kiến trúc quan trọng. Code và kiểm thử ghi lại **cái gì** hệ thống làm; ADR ghi lại **vì sao**
— để người bảo trì sau này thấy `GET /recipes` không trả bản nháp, access token không nằm trong `localStorage`, lớp gọi API dùng `ky` chứ
không phải `axios`… và **không "sửa cho gọn"** những chỗ đó thành lỗ hổng hay làm hỏng cơ chế cache.

Mỗi ADR gồm: bối cảnh → các phương án đã cân nhắc → quyết định → hệ quả (cả cái được lẫn cái mất) → bằng chứng trong code/kiểm thử.
Nguồn gốc từng quyết định: mục MT tương ứng trong [`SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`](../../SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md) và
kế hoạch [`SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`](../../SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md).

| # | Quyết định | Nguồn | Trạng thái |
|---|---|---|---|
| [0001](0001-xoa-mem-cong-thuc.md) | Xóa mềm toàn hệ thống + job dọn vĩnh viễn sau 30 ngày | MT-05, FR-RCP-007, FR-JOB-003 | Đã chấp nhận |
| [0002](0002-chuan-hoa-ma-loi-http.md) | 400 cho mọi lỗi kiểm tra hợp lệ, 409 cho mọi xung đột trạng thái, bỏ hẳn 422 | MT-08, MT-09, D-11 | Đã chấp nhận |
| [0003](0003-redis-la-cache-duy-nhat.md) | Redis cache-aside là cơ chế cache duy nhất (không `IMemoryCache`, không Output Cache) | MT-16, MT-17, D-6 | Đã chấp nhận |
| [0004](0004-cach-ly-cache-cong-khai-rieng-tu.md) | Tách bạch endpoint công khai / riêng tư — không bao giờ cache dữ liệu phụ thuộc danh tính | MT-34, D-4, D-5 | Đã chấp nhận |
| [0005](0005-hangfire-worker-rieng.md) | Hangfire chạy ở container worker riêng, dùng chung image với API | MT-47 | Đã chấp nhận |
| [0006](0006-nginx-resolver-dong.md) | Nginx phân giải tên service động (resolver), không dùng khối `upstream` | MT-50 | Đã chấp nhận |
| [0007](0007-phan-vai-state-frontend.md) | Phân vai state Frontend: Zustand cho phiên, TanStack Query cho dữ liệu server, chỉ refresh token lưu bền | NFR-SEC-002, X-7, Buổi 6 | Đã chấp nhận |
| [0008](0008-ky-thay-axios.md) | Lớp gọi API dùng `ky` (bọc `fetch`) thay vì `axios` để giữ Data Cache của Next.js | X-10, Buổi 5–7 | Đã chấp nhận |
| [0009](0009-versioning-va-rate-limit.md) | `Asp.Versioning` theo đoạn URL + thuật toán rate limit theo SRS, đếm thêm ở Nginx khi chạy nhiều bản sao | X-6, NFR-SEC-003, Buổi 5, Buổi 7 | Đã chấp nhận |
| [0010](0010-csp-nonce-va-render-theo-request.md) | CSP có nonce ⇒ mọi trang HTML render theo request; độ tươi dữ liệu đặt ở Data Cache | MT-67 | Đã chấp nhận |

**Thêm ADR mới:** chép khuôn của một ADR có sẵn, đánh số tiếp theo, thêm một dòng vào bảng trên. ADR không bao giờ bị sửa nội dung khi
quyết định thay đổi — viết ADR mới ghi "Thay thế ADR-xxxx" và đổi trạng thái ADR cũ thành "Bị thay thế".
