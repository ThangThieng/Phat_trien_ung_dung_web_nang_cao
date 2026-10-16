# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Lab:** 01<br>
**Từ ngày:** 09/09/2026 **đến ngày:** 15/09/2026

**MSSV:** 2312758<br>
**Họ và tên:** Nguyễn Hồng Phúc Thọ

**Nhóm:** 20

## Công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ % |
|---:|---|---|---:|
| 1 | **FR-RCP-001 — Danh sách công thức:** xây dựng API `GET /api/v1/recipes`, lọc theo danh mục/độ khó/thời gian nấu, sắp xếp theo whitelist, phân trang `PagedResult`, cache Redis và giao diện SSR `/recipes`. | [2312758_NguyenHongPhucTho_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso2)<br><br>Commit triển khai gốc `0fc95d1`: [feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/0fc95d1bfd62afc43050f3d55d57e0ea45629708) | 100% |
| 2 | **FR-RCP-002 — Chi tiết công thức:** xây dựng API `GET /api/v1/recipes/{slug}`, kiểm soát quyền xem Draft, cache Redis và trang ISR `/recipes/[slug]` hiển thị ảnh, nguyên liệu, bước nấu và dinh dưỡng. | [2312758_NguyenHongPhucTho_buoiso2](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso2)<br><br>Commit triển khai gốc `0fc95d1`: [feat(recipe): complete FR-RCP-001 & 002 recipe list and detail view](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/0fc95d1bfd62afc43050f3d55d57e0ea45629708) | 100% |

### 1. Cài đặt API danh sách công thức (FR-RCP-001)

**Đã hoàn thành:**

- Xây dựng Recipe aggregate và các entity `RecipeStep`, `RecipeIngredient`, `RecipeImage`; dùng owned type `RecipeNutrition`, enum `RecipeDifficulty` và `RecipeStatus`.
- Cấu hình bảng `Recipes`, `RecipeSteps`, `RecipeIngredients`, `RecipeImages`: ràng buộc thời gian/khẩu phần, unique `(RecipeId, StepNumber)`, một ảnh chính cho mỗi công thức và các index `IDX_Recipe_*`.
- Cài đặt `GetRecipesQuery`, Validator, `IRecipeReadRepository`, DTO và `RecipeSortParser` với whitelist trường sắp xếp.
- Cài đặt `RecipeReadRepository` dùng projection `AsNoTracking` và split query; `DatabaseSeeder` dùng Bogus tạo 50 công thức và 5 tác giả mẫu.
- Cài đặt endpoint `GET /api/v1/recipes`: lọc theo danh mục, độ khó, thời gian nấu; sắp xếp và phân trang với `PagedResult` gồm `items`, `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`.
- Cấu hình Output Cache Redis: policy `RecipeList` 15 phút, tag `recipes`.
- Xây dựng trang `/recipes` SSR, bộ sắp xếp, phân trang, `RecipeCard`, `Pagination` và `RecipeGridSkeleton`.

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| `?page=2&pageSize=5&sort=title` | 200, trả đầy đủ thông tin phân trang |
| `?difficulty=Hard&maxCookTime=60` | Chỉ trả công thức Hard có thời gian nấu ≤ 60 phút |
| `pageSize=100`, `sort=hack` | 422, thông báo lỗi theo field tại thời điểm thực hiện Lab 01 |
| Tổng số công thức thấy được: Guest / Author1 / Admin | 46 / 47 / 50 theo quy tắc hiển thị Draft của bản Lab 01 |
| Output Cache | Lần gọi thứ hai có header `Age`, xác nhận cache hit |

### 2. Cài đặt API và giao diện chi tiết công thức (FR-RCP-002)

**Đã hoàn thành:**

- Cài đặt `GetRecipeBySlugQuery` và endpoint `GET /api/v1/recipes/{slug}` để lấy công thức theo slug.
- Trả dữ liệu chi tiết gồm ảnh, thông tin thời gian/khẩu phần, nguyên liệu, các bước thực hiện và dinh dưỡng.
- Áp dụng phân quyền xem công thức Draft: chủ sở hữu và Admin xem được; Guest hoặc Author khác nhận 403 theo phạm vi Lab 01.
- Cấu hình Output Cache Redis policy `RecipeDetail` 60 phút, gắn tag `recipe:{slug}`.
- Xây dựng trang `/recipes/[slug]` ISR 300 giây; hiển thị ảnh, badge độ khó, bốn ô thời gian/khẩu phần, danh sách nguyên liệu, `StepChecklist` và `NutritionTable`.
- Bổ sung trang 404 cho slug không tồn tại hoặc công thức Draft không được phép xem ở giao diện web.

**Kết quả kiểm thử thực tế:**

| Kịch bản | Kết quả |
|---|---|
| Draft `bun-bo-hue`: Guest / Author khác / Chủ / Admin | 403 / 403 / 200 / 200 |
| Slug không tồn tại hoặc Draft trên web | Hiển thị trang 404 |
| Output Cache chi tiết | Lần gọi tiếp theo trả cache hit |
| ISR trang chi tiết | `x-nextjs-cache: HIT`, `Cache-Control: s-maxage=300` |

### Ghi chú đối chiếu tài liệu

Nội dung trên phản ánh đúng phần Dev 2 đã hoàn thành trong Báo cáo Buổi 1 và commit `0fc95d1`. SRS v1.2.0 sau đó đã chuẩn hóa một số quy ước (như response validation, cache và phạm vi công khai/Draft) qua Change Request; các thay đổi retrofit này không được tính là hạng mục Lab 01 ban đầu của Dev 2.
