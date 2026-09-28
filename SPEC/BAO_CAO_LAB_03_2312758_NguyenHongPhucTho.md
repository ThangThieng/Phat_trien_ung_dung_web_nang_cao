# BÁO CÁO LAB 03 – PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

| Thông tin | Giá trị |
|---|---|
| Lab | 03 – Tạo Draft Recipe và Quản lý ảnh Recipe |
| MSSV / Họ tên | 2312758 – Nguyễn Hồng Phúc Thọ |
| Nhóm / Vai trò | Nhóm 20 / Dev 2 – Module Recipe |
| Nhánh | `2312758_NguyenHongPhucTho_buoiso3` |

## Công việc được giao và kết quả

| Công việc | Các bước đã thực hiện | Tiến độ |
|---|---|---:|
| FR-RCP-003 – Tạo công thức nháp | Thêm `CreateRecipeCommand`/validator; kiểm tra title 5–200, description 20–2000, thời gian, servings và category; tạo recipe bằng domain factory với trạng thái Draft; sinh slug, tự thêm hậu tố khi trùng và từ chối slug dành riêng (`search`, `mine`, `sitemap`, `new`, `edit`); nhận nutrition trong cùng request. Thêm `POST /api/v1/recipes`. | 100% |
| D-8 – Instructions nullable | Đổi cấu hình EF Core để `Instructions` là trường tùy chọn và thêm migration `B3_Recipe_InstructionsNullable`. Điều này khớp SRS: bước nấu sẽ được bổ sung riêng ở FR-RCP-010, không ép client gửi instructions cũ. | 100% |
| FR-RCP-008 – Quản lý ảnh recipe | Thêm upload, cập nhật metadata và xóa ảnh qua các endpoint ảnh recipe; chỉ owner/Admin được thao tác; ảnh đầu tiên tự là primary; khi xóa ảnh primary, ảnh thay thế được chọn xác định theo `OrderIndex`, rồi `CreatedAt`. Tái sử dụng `IFileStorageService` để lưu MinIO theo `recipes/{recipeId}`. | 100% |
| Đối chiếu SRS / mâu thuẫn | Áp dụng MT-02 (nutrition là Owned Entity, không tạo endpoint riêng), MT-19 (PATCH metadata gộp), MT-41.6 (tiêu chí thay primary xác định) và D-8 trong báo cáo Buổi 2. | 100% |

## API đã bổ sung

- `POST /api/v1/recipes` – tạo Draft, yêu cầu role Author/Admin, trả 201.
- `POST /api/v1/recipes/{id}/images` – upload ảnh multipart, trả 201.
- `PATCH /api/v1/recipes/{id}/images/{imageId}` – cập nhật `altText`, `orderIndex`, `isPrimary`.
- `DELETE /api/v1/recipes/{id}/images/{imageId}` – xóa ảnh, trả 204.

## Quyết định kỹ thuật

Recipe luôn được tạo ở trạng thái Draft, vì publish chỉ được phép sau khi có đủ ingredient và step (FR-RCP-005). Nutrition đi cùng lệnh tạo/sửa Recipe do nó là owned entity, tránh tạo hai đường ghi cùng một nhóm cột. Phân quyền được kiểm tra tập trung theo owner/Admin trước các thao tác ghi ảnh.

Với ảnh primary, cập nhật metadata dùng một endpoint PATCH thay vì các endpoint hành động rời rạc. Khi ảnh primary bị xóa, cách chọn ảnh thay thế được xác định rõ nên không phụ thuộc thứ tự trả về không bảo đảm của database.

## Minh chứng commit

Commit Buổi 3: `feat(recipes): complete FR-RCP-003 draft creation and FR-RCP-008 image management`.

Liên kết nhánh: `https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commits/2312758_NguyenHongPhucTho_buoiso3`

## Kiểm chứng

Đã chạy `dotnet restore CulinaryBlog.sln --force --no-cache`. Lệnh build hiện không trả diagnostics cụ thể từ MSBuild trong môi trường này (trạng thái `Build FAILED`, 0 error/0 warning), nên cần chạy lại `dotnet build CulinaryBlog.sln` trên máy phát triển trước khi merge để xác nhận toàn bộ solution.
