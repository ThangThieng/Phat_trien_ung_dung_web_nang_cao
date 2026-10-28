# ADR-0002 — 400 cho kiểm tra hợp lệ, 409 cho xung đột trạng thái, bỏ hẳn 422

- **Trạng thái:** Đã chấp nhận (SRS v1.1.0 — CR-2026, MT-08 + MT-09; hiện thực bằng commit nền D-11 đầu Buổi 3)
- **Liên quan:** CONS-005, Phụ lục A/B, NFR-USE-003

## Bối cảnh

SRS v1.0.0 dùng **422** cho lỗi kiểm tra hợp lệ ở toàn Chương 3, nhưng Chương 8 và Phụ lục B dùng **400**; Phụ lục A lại dành 422 cho xung
đột `RowVersion`, trong khi FR-RCP-004 ghi 409. Mâu thuẫn xuyên suốt gần như mọi endpoint và mọi test case.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. 400 cho mọi lỗi kiểm tra hợp lệ; 409 cho mọi xung đột (unique, đồng thời, chuyển trạng thái sai) | Khớp 3/4 tài liệu và hành vi mặc định của ASP.NET Core; Frontend bắt một mã | 400 gộp chung "JSON hỏng" và "dữ liệu sai" |
| B. 422 cho dữ liệu sai, 400 cho body hỏng | Phân biệt tinh tế hơn | Ghi đè framework, sửa 3 chỗ SRS, Frontend bắt hai mã |
| 422 cho xung đột đồng thời | Tách khỏi lỗi trùng unique | Sai ngữ nghĩa: 422 không phải "xung đột trạng thái"; ngược thông lệ ETag/If-Match |

## Quyết định

**400 = mọi lỗi kiểm tra hợp lệ/đầu vào · 409 = mọi xung đột trạng thái · 423 = khóa tài khoản tạm thời · 422 bị loại bỏ hoàn toàn.** Frontend
phân biệt các lỗi cùng mã HTTP bằng trường `type` (Application Error Code, Phụ lục B) — đúng mục đích Phụ lục B đặt ra.

Mọi lỗi đi qua `GlobalExceptionMiddleware` → RFC 7807; `ValidationProblemDetails` có `errors` theo tên trường camelCase (kể cả mảng lồng
nhau `steps[0].title`) để Frontend gắn lỗi vào đúng ô nhập.

## Hệ quả

- Domain exception **không mang mã HTTP**; mã đăng ký ở `API/Middleware/ExceptionMapping/{Module}ExceptionMappings.cs` (CR-2026-04, MT-63).
- Thêm mã lỗi mới phải sửa cả `ErrorCodes.cs`, `ERROR_CODES` phía Frontend và SRS Phụ lục B — kiểm thử chống lệch giữ ba nơi khớp nhau.

## Bằng chứng

`ValidationStatusCodeTests` (ArchitectureTests — không còn 422 trong code), `ProblemDetailsContractTests`, `DomainExceptionRulesTests`,
`frontend/src/types/__tests__/error-codes.test.ts` (đọc chéo `ErrorCodes.cs`). Kiểm chứng B3: `grep -rE "Status422|status === 422"` rỗng.
