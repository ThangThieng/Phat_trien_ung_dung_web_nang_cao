# BÁO CÁO LAB 05 – PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

## Thông tin

- MSSV / Họ tên: 2312758 – Nguyễn Hồng Phúc Thọ
- Nhóm / Vai trò: Nhóm 20 / Dev 2 – API docs và Recipe types
- Nhánh: `2312758_NguyenHongPhucTho_buoiso5`

## Công việc được giao và kết quả

- `AddCulinaryOpenApi()` đăng ký OpenAPI 3.1 với thông tin API, Bearer scheme, security 401/403 và lỗi `application/problem+json`.
- Scalar phục vụ tại `/scalar/v1`, bật/tắt bằng `OpenApi:Enabled`; Nginx chuyển tiếp `/scalar/` và `/openapi/`.
- Đủ 45 operation có operationId ổn định, summary, description, tag và response contract.
- `OpenApiDocumentTests` kiểm số operation, metadata, security và snapshot `docs/api/openapi.v1.json`.
- `types/recipe.ts` có các enum `as const`, model Recipe/Nutrition và request types; dùng lại `CategoryRef` và `SortBy` từ module tương ứng.
- Đồng bộ `develop`, giữ query/repository/migration của Dev 3 và xóa phần triển khai trùng.

## Quyết định kỹ thuật

Tài liệu OpenAPI sinh từ endpoint metadata để tránh lệch giữa tài liệu và mã nguồn. Snapshot JSON giúp mọi thay đổi hợp đồng API xuất hiện trong code review. Kiểu TypeScript được viết theo miền để dễ đọc, còn contract test giữ chúng đồng bộ với backend.

## Minh chứng GitHub

- Nhánh: https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso5
- Commit OpenAPI: `12f79f0`

## Kiểm chứng

- .NET solution: build thành công, 0 lỗi và 0 cảnh báo.
- `OpenApiDocumentTests`: 4/4 đạt, đúng 45 operation.
- Frontend: lint và typecheck đạt; Jest 231/231; Next.js production build thành công.
- Integration test phụ thuộc Docker chưa chạy vì Docker Desktop đang tắt trên máy kiểm tra.
