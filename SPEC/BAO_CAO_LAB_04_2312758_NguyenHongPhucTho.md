# BÁO CÁO LAB 04 – PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

## Thông tin

- MSSV / Họ tên: 2312758 – Nguyễn Hồng Phúc Thọ
- Nhóm / Vai trò: Nhóm 20 / Dev 2 – Module Recipe
- Nhánh: `2312758_NguyenHongPhucTho_buoiso4`

## Công việc được giao và kết quả

- Migration `B4_Recipe_IngredientStep`: thêm `QuantityText`, kiểm tra quantity dương và unique constraint hoãn kiểm tra cho thứ tự bước.
- FR-RCP-009: hỗ trợ định lượng số hoặc nguyên văn, cùng thao tác thêm, cập nhật và xóa nguyên liệu.
- FR-RCP-010: server quản lý số thứ tự bước, cập nhật, xóa, đánh số lại và sắp xếp bằng tập ID đầy đủ.
- Nền tảng API tạo/cập nhật công thức, ảnh, nguyên liệu và bước được triển khai trên nhánh Buổi 4.

## Quyết định kỹ thuật

Hai cách định lượng `Quantity` và `QuantityText` hỗ trợ cả dữ liệu tính toán được và cách ghi tự nhiên như “vừa đủ”. Constraint thứ tự bước được phép hoãn kiểm tra để PostgreSQL có thể hoán đổi thứ tự trong cùng transaction mà không phát sinh xung đột unique tạm thời.

## Minh chứng GitHub

- Nhánh: https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso4
- Commit tích hợp: `3a0750a`

## Kiểm chứng

Các thay đổi Buổi 4 đã được tích hợp làm nền cho giao diện soạn và chỉnh sửa công thức ở Lab 5.
