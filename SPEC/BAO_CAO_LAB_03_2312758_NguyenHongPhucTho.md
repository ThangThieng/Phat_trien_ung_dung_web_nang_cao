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

> (Bản ghi trước review, giữ nguyên để đối chiếu.) Đã chạy `dotnet restore CulinaryBlog.sln --force --no-cache`. Lệnh build hiện không trả diagnostics cụ thể từ MSBuild trong môi trường này (trạng thái `Build FAILED`, 0 error/0 warning), nên cần chạy lại `dotnet build CulinaryBlog.sln` trên máy phát triển trước khi merge để xác nhận toàn bộ solution.

## Khắc phục sau review ngày 29/09/2026

Review trên commit `75432b2` kết luận nhánh **chưa đạt — không build được**. Các mục dưới đây đã được **Nguyễn Thăng Thiêng (Dev 4, trưởng nhóm)** sửa trên nhánh này ngày 30/09/2026 trong một commit riêng, đứng tên người sửa; các commit gốc của Dev 2 giữ nguyên. Nhánh đã hợp nhất mốc M1 (nhánh Buổi 3 của Dev 4).

| Mục review | Đã xử lý |
|---|---|
| 1 – Build lỗi (vi phạm analyzer) | Viết lại theo style chung; `dotnet build` 0 warning. `RecipeWriteCommands.cs`/`RecipeWriteRepository.cs` thay bằng `CreateRecipeCommand.cs`, `RecipeImageCommands.cs`, `RecipeRepository.cs`. |
| 2 – Migration D-8 không bao giờ chạy | Thêm `[DbContext]` + `[Migration("20260928084500_B3_Recipe_InstructionsNullable")]`; giữ nguyên Id migration để không trùng với nhánh khác. Tạo nháp không gửi `instructions` → 201. |
| 3 – Upload không kiểm tra magic bytes | Dùng chung `ImageUploadInspector` (kích thước → MIME → magic bytes) với `/files/upload`; đuôi file lấy từ kết quả magic bytes. |
| 4 – Đổi ảnh chính trong một `SaveChanges` | Hạ ảnh chính cũ → lưu → nâng ảnh mới → lưu, trong `IUnitOfWork.ExecuteInTransactionAsync`; áp dụng cả khi tải ảnh mới với `isPrimary` và khi xóa ảnh chính. Test đổi ảnh chính 10 lần liên tiếp. |
| 5 – Xóa tệp MinIO đồng bộ | Xóa mềm bản ghi ảnh, sau khi commit mới xếp hàng `DeleteStoredFilesJob` qua Hangfire. |
| 6 – Không có `RecipeAuthorizationHandler` | `RecipeAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Recipe>` (chủ sở hữu hoặc Admin), dùng chung cho mọi command ghi Recipe. |
| 7 – Không có giao diện | `/dashboard/recipes/new`: bước 1 thông tin + dinh dưỡng (zod mirror validator backend, lỗi 400 gắn đúng ô), bước 2 thư viện ảnh với radio chọn ảnh chính và nút xóa; liên kết "Viết công thức" trên header. |
| 8 – Không có test | Unit: 7 test quy tắc ảnh. Integration (PostgreSQL + MinIO thật): tạo nháp 201, body sai 400, `categoryId` sai 400 tại trường, Guest 401, người khác 403, Admin được phép, ảnh/công thức không tồn tại 404, MinIO lỗi 503, magic bytes giả 400, file rỗng, đổi ảnh chính 10 lần, xóa ảnh chính, bộ dịch lỗi RowVersion. Frontend: test schema. |
| 9 – `categoryId` sai trả 404 | Rule bất đồng bộ trong validator qua `IRepository<Category>` → 400 lỗi ở trường `categoryId`. |
| 10 – Tự đặt mã `RECIPE_IMAGE_NOT_FOUND` | Bỏ; ảnh không tồn tại dùng `RECIPE_NOT_FOUND` qua `RecipeImageNotFoundException`. |
| 11 – Slug dành riêng bị từ chối | Dùng `ReservedSlugs` chung (bản giống hệt của Dev 3), thêm hậu tố như slug trùng: "Search" → `search-2`. |
| 12 – `SlugExistsAsync` bỏ qua bản đã xóa | `IgnoreQueryFilters()` cho tới khi D-3 (Buổi 4) đổi index sang partial. |
| 13 – DTO trả về tên danh mục/tác giả rỗng | Đọc lại chi tiết qua `IRecipeReadRepository.GetDetailByIdAsync`. |
| 14 – Invalidate cache không tác dụng | Command khai báo `recipe:{slug}` + tiền tố danh sách/tìm kiếm qua `ICacheInvalidator`; Output Cache cũ sẽ gỡ ở Buổi 4 (D-6, Dev 3). |
| 15 – Upload thiếu `isPrimary?`, `orderIndex?` | Đã thêm vào form-data và response đủ 7 trường. |
| 16 – File rỗng trả `FILE_SIZE_EXCEEDED` | File rỗng trả `FILE_MIME_INVALID` (không phải ảnh hợp lệ). |
| Kế hoạch Buổi 3 bước 2–5 | Cụm `Domain/Exceptions/Recipes` (5 lớp) + `RecipeExceptionMappings`; `IRecipeRepository` (`GetByIdWithDetails/WithImages`, `SlugExists`) gắn `IUnitOfWork.Recipes`; `RecipePersistenceExceptionTranslator` (RowVersion → 409, trùng slug → 409). |

**Còn phải tự làm:** mục 17 — squash các commit của Dev 2 thành một commit theo quy ước §2.4; xuất lại `.docx` của báo cáo theo nội dung mới.

**Kiểm chứng sau khi sửa (30/09/2026):** `dotnet build` 0 warning; Domain 21, Application 27, Architecture 15, Integration 74 test xanh (1 Skip có chủ đích: tái hiện MT-34, chờ Dev 3 vá ở Buổi 4); frontend `lint` / `tsc` / `jest` (32 test) / `next build` xanh.
