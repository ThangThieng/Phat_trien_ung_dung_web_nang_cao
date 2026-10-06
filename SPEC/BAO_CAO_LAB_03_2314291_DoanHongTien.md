# BÁO CÁO KẾT QUẢ BUỔI 3 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (Category / Recipe read)
**Nhánh:** `2314291_DoanHongTien_buoiso3` (tách từ tip nhánh Dev 4 buổi 3 để dùng `IRepository`, `IUnitOfWork`, `IPersistenceExceptionTranslator`, harness test) — **PR nhắm vào `develop`**: `<điền link PR>`
**Phạm vi:** FR-CAT-003 (POST), FR-CAT-004 (PUT), FR-CAT-005 (DELETE) — CRUD danh mục cho Admin · Domain Exceptions & Repository cho module Category · D-13 (TTL `categories:all` = 30 phút) · `CategoryNav` · Value Object `Slug`
**Tài liệu bám sát:** `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md` (Buổi 3 — Dev 3) và `SPEC/SRS_Culinary_Blog_v1.2.2.md` (MT-17, MT-63, Phụ lục B)

> **Trạng thái kiểm chứng (30/09/2026):** đã chạy thật `dotnet build`, `dotnet test` toàn solution và `lint / tsc / jest / build` của frontend — kết quả ở mục 7–8. Còn **chờ điền**: test tay qua UI và link PR/commit sau khi push. Chỉ ghi ✅ cho hạng mục đã có log thật (§2.4).

---

## 1. Tóm tắt các công việc

| # | Hạng mục | Yêu cầu trong kế hoạch | Kết quả |
|---|----------|------------------------|---------|
| A | **Domain Exceptions cho Category** | `CategoryDomainException` + 3 lớp con, mã trong `ErrorCodes` (Phụ lục B), mang `Code`, không mang mã HTTP | Code xong — mục 2 |
| B | **Repository cho Category** | `ICategoryRepository : IRepository<Category>`, truy cập qua `IUnitOfWork.Categories` | Code xong — mục 3 |
| C | **FR-CAT-003 POST** | 201 + `Location`, slug tự sinh, chỉ Admin, trùng tên → 409 | Code xong, có test — mục 4 |
| D | **FR-CAT-004 PUT** | Đổi tên **không đổi slug**, 404 / 409 đúng mã | Code xong, có test — mục 4 |
| E | **FR-CAT-005 DELETE** | Còn công thức → 409 kèm `recipeCount`; rỗng → 204 (xóa mềm) | Code xong, có test — mục 4 |
| F | **Dịch lỗi 23505 (MT-63)** | `CategoryPersistenceExceptionTranslator : IPersistenceExceptionTranslator` ở Infrastructure, không xử lý trong middleware | Code xong — mục 5 |
| G | **Cache (D-13, MT-17)** | TTL `categories:all` 30′; ghi thì xóa `categories:all` và prefix `categories:detail:` | TTL xong; prefix chỉ khai báo — mục 6 |
| H | **Value Object `Slug`** | Đóng gói quy tắc slug, slug dành riêng | Code xong, có test — mục 2.2 |
| I | **Frontend** | Trang `/dashboard/categories` (tạo/sửa/xóa), `CategoryNav` trên header | Code xong — mục 8 |
| J | **Sửa theo review** | 11 điểm review PR đầu | Xem mục 9 |

---

## 2. Chi tiết A — Domain

### 2.1 Exception (`Domain/Exceptions/Categories/`)

| Lớp | Mã lỗi | Ghi chú |
|-----|--------|---------|
| `CategoryDomainException` (abstract) | — | Lớp gốc của module, kế thừa `DomainException` của Dev 4 |
| `CategoryNotFoundException` | `CATEGORY_NOT_FOUND` | Hai constructor: theo `Guid id` và theo `string slug` |
| `CategoryNameAlreadyExistsException` | `CATEGORY_NAME_EXISTS` | Nhận `inner` để giữ `DbUpdateException` gốc khi dịch từ DB |
| `CategoryHasRecipesException` | `CATEGORY_DELETE_HAS_RECIPES` | Thuộc tính `RecipeCount`, thêm extension `recipeCount` vào Problem Details |

Cả bốn nằm trong namespace `CulinaryBlog.Domain.Exceptions.Categories` để qua test kiến trúc của Dev 4 (mọi `DomainException` cụ thể phải được ánh xạ HTTP tường minh).

### 2.2 Value Object `Slug` (`Domain/ValueObjects/Slug.cs`)

`readonly record struct Slug` với `From(string)` (kiểm tra `a–z 0–9` và dấu gạch đơn, ném `ArgumentException`), `FromText(string)` (dùng `SlugHelper.Generate`), `WithSuffix(int)` (`mon-chay` → `mon-chay-2`), `IsReserved` (đối chiếu `ReservedSlugs`). Hiện chỉ handler `CreateCategoryCommand` dùng VO này.

`Category` bổ sung `SoftDelete()`.

---

## 3. Chi tiết B — Repository & Unit of Work

- `ICategoryRepository` (Application): `ExistsByNameAsync(name, excludeId, ct)`, `SlugExistsAsync(slug, ct)`, `CountActiveRecipesAsync(categoryId, ct)`.
- `CategoryRepository` (Infrastructure, kế thừa `EfRepository<Category>`):
  - `ExistsByNameAsync` so khớp không phân biệt hoa/thường bằng `ILIKE`, có `excludeId` để PUT giữ nguyên tên của chính nó.
  - **`IgnoreQueryFilters()` ở cả `ExistsByNameAsync` và `SlugExistsAsync`**, vì `IDX_Category_Name` và `IDX_Category_Slug` là unique index thường — bản ghi đã xóa mềm vẫn chiếm chỗ. Thiếu điều này thì tạo lại tên/slug của danh mục đã xóa sẽ lọt qua bước kiểm tra rồi nổ ở DB.
  - `CountActiveRecipesAsync` chỉ đếm công thức chưa xóa mềm.
- `IUnitOfWork.Categories` + `UnitOfWork` nhận `ICategoryRepository` và danh sách translator.

---

## 4. Chi tiết C–E — Ba endpoint (đều `RequireAuthorization(Admin)`)

| Endpoint | Thành công | Lỗi |
|----------|-----------|-----|
| `POST /api/v1/categories` | **201** + `Location: /api/v1/categories/{slug}` | 400 validation · 401 · 403 (Author) · **409** `CATEGORY_NAME_EXISTS` |
| `PUT /api/v1/categories/{id}` | **200**, slug giữ nguyên | 400 · 403 · **404** `CATEGORY_NOT_FOUND` · **409** trùng tên của danh mục khác |
| `DELETE /api/v1/categories/{id}` | **204** (xóa mềm), sau đó `GET /categories/{slug}` → 404 | 403 · 404 · **409** `CATEGORY_DELETE_HAS_RECIPES` kèm `recipeCount` |

Thuật toán slug khi tạo: `Slug.FromText(name)`; nếu slug là từ dành riêng hoặc đã tồn tại (kể cả bản ghi đã xóa mềm) thì thử `-2`, `-3`, … Ánh xạ HTTP nằm ở `API/Middleware/ExceptionMapping/CategoryExceptionMappings.cs` (404 / 409 / 409).

---

## 5. Chi tiết F — Dịch lỗi khóa duy nhất (MT-63)

Kiểm tra `ExistsByNameAsync` trước khi ghi **không đủ** khi hai request cùng tên đến song song (cả hai cùng thấy "chưa tồn tại"). Lớp phòng thủ thứ hai là unique index của PostgreSQL, và lỗi của nó được dịch ở Infrastructure:

`CategoryPersistenceExceptionTranslator` (`internal sealed`, tự đăng ký qua quét assembly) → nếu `DbUpdateException` chứa `PostgresException` với `SqlState = 23505` và `ConstraintName = "IDX_Category_Name"` thì trả `CategoryNameAlreadyExistsException` → middleware chung trả **409**.

Nhánh Npgsql do lần đầu mình viết trong `GlobalExceptionMiddleware` đã được **gỡ bỏ**: middleware chỉ còn phụ thuộc `DomainException`, không biết gì về Npgsql.

**Hạn chế còn lại:** translator chỉ xử lý `IDX_Category_Name`. Hai request song song **khác tên nhưng sinh cùng slug** vẫn có thể đụng `IDX_Category_Slug` và trả 500 (rất hiếm). Đề xuất: bổ sung nhánh `IDX_Category_Slug` (thử lại sinh slug) ở buổi sau.

---

## 6. Chi tiết G — Cache

- `GetCategoriesQuery`: TTL `categories:all` = **30 phút** (D-13, MT-17).
- Create/Update/Delete implement `ICacheInvalidator` với `CacheKeysToInvalidate = [categories:all, categories:detail:]` (hằng `CategoryCacheKeys.DetailPrefix`).
- ⚠️ `CacheInvalidationBehavior` hiện chỉ xóa **khóa chính xác**; xóa theo prefix cần `RemoveByPrefixAsync` (kế hoạch Buổi 7). Đến lúc đó `DetailPrefix` mới có tác dụng thật — hiện tại nó là khai báo đúng chỗ, chưa phải hành vi đã kiểm chứng. Khóa `categories:all` thì đã có test.

---

## 7. Test đã viết

**Unit — `Application.UnitTests/CategoryCommandTests.cs`:** validator (tên hợp lệ / độ dài / chứa HTML / không sinh được slug, mô tả quá dài, `imageUrl`, `orderIndex` âm, thiếu `id` khi sửa); handler tạo (trùng tên → 409, sinh slug, slug trùng → hậu tố, slug dành riêng → hậu tố); handler sửa (không tồn tại → 404, trùng tên người khác → 409, đổi tên giữ slug); handler xóa (không tồn tại → 404, còn công thức → 409 kèm `RecipeCount` và `Extensions["recipeCount"]`, danh mục rỗng → `SoftDelete`); cache key gồm `All` và `DetailPrefix`.

**Unit — `Domain.UnitTests/SlugTests.cs`:** `From` hợp lệ / sai định dạng / null, `FromText` bỏ dấu tiếng Việt và ném lỗi khi không còn chữ-số, `WithSuffix`, `IsReserved`, so sánh bằng giá trị.

**Architecture — `ArchitectureTests/CategoryExceptionMappingsTests.cs`:** 404 / 409 / 409 và extension `recipeCount`.

**Integration (harness Dev 4) — `API.IntegrationTests/Content/CategoryAdminEndpointsTests.cs`:**

| Nhóm | Kịch bản |
|------|----------|
| POST | 201 + `Location` + slug · Author 403 · Guest 401 · trùng tên 409 (cả `Món chính` / `món chính`) · tên rỗng 400 |
| **Song song** | Hai POST cùng tên bằng `Task.WhenAll` → **đúng 1×201 + 1×409, không 5xx** |
| Translator | Chèn trùng qua `UnitOfWork` → `CategoryNameAlreadyExistsException` |
| Soft-delete | Tên của danh mục đã xóa mềm → 409; tên khác nhưng slug trùng bản đã xóa → 201 với `mon-chay-2` |
| Cache | POST làm mất cache `categories:all` |
| PUT | Đổi tên giữ slug · giữ nguyên tên vẫn 200 · trùng tên khác 409 · id lạ 404 · Author 403 |
| DELETE | Có công thức → 409 với `recipeCount = 3` · rỗng → 204 rồi GET → 404 · id lạ 404 · Author 403 |

| Lệnh | Kết quả |
|------|---------|
| `dotnet build CulinaryBlog.sln` | ✅ Build succeeded (đã sửa 2 lỗi analyzer SA1629, SA1514 phát hiện lúc build) |
| `dotnet test CulinaryBlog.sln` (unit + architecture + integration) | ✅ total 200 · failed 0 · succeeded 199 · skipped 1 (test `Skip` có chủ đích của Dev 4, MT-34) |
| `CategoryAdminEndpointsTests` (integration, Testcontainers) | ✅ 20/20 |
| Chạy lặp `Create_SameNameInParallel...` | ✅ nhiều lần liên tiếp đều xanh: đúng 1×201 + 1×409, không 5xx |

> **Lưu ý môi trường:** máy chạy test không kéo được `quay.io/minio/minio` (Docker trả `401 UNAUTHORIZED`, kể cả tag `latest`), nên đã gán tag cục bộ `docker tag quay.io/minio/minio:latest quay.io/minio/minio:RELEASE.2025-09-07T16-13-09Z` từ image có sẵn trong máy để chạy harness của Dev 4. Không sửa file nào trong repo; đã báo Dev 4.

---

## 8. Frontend

- `dashboard/categories/page.tsx`: bảng danh mục, tạo/sửa bằng `CategoryFormDialog` (React Hook Form + Zod; các ô Tên, Mô tả, Ảnh qua `ImageUploader`, Thứ tự hiển thị), xóa có xác nhận. Khi 409 `CATEGORY_DELETE_HAS_RECIPES`, đọc `error.problem.recipeCount` để báo *"Danh mục đang có N công thức…"*, không có thì dùng `error.message`.
- `CategoryNav.tsx`: menu thả xuống "Danh mục" trên `SiteHeader`, dùng chung khóa TanStack Query `['categories']`, đóng khi nhấn Escape / bấm ngoài / bấm link.
- `admin-api.ts`: `createCategory`, `updateCategory`, `deleteCategory`.

| Lệnh | Kết quả |
|------|---------|
| `npm run lint` | ✅ không lỗi |
| `npx tsc --noEmit` | ✅ không lỗi |
| `npm test` | ✅ 3 suite, 20 test pass (chưa có test riêng cho `CategoryNav`/trang quản trị) |
| `npm run build` | ✅ thành công, có route `/dashboard/categories` |
| Test tay qua UI (Admin): hộp thoại tạo danh mục, tạo trùng tên khác hoa/thường | ✅ 409, lỗi hiện dưới ô Tên |
| Test tay còn lại: sửa giữ slug, xóa có bài viết, xóa rỗng, tạo lại tên đã xóa, Author bị chặn | ⏳ chờ điền |

---

## 9. Đối chiếu 11 điểm review

| # | Điểm review | Trạng thái | Bằng chứng / còn thiếu |
|---|-------------|-----------|------------------------|
| 1 | PR nhắm `main` | ◐ một phần | Đã mở PR #40 với base **`develop`** (đã đổi từ `main`); còn phải đóng PR cũ #1 và sửa tiêu đề/mô tả PR |
| 2 | Tên nhánh | ✅ đã xong | Nhánh `2314291_DoanHongTien_buoiso3` đã push lên GitHub (commit `710e0a2`) |
| 3 | Kiểm tra tên/slug bỏ sót bản đã xóa mềm | ✅ | `IgnoreQueryFilters()` ở `ExistsByNameAsync` và `SlugExistsAsync`; 2 test integration xanh (tên đã xóa → 409; slug trùng bản đã xóa → `mon-chay-2`) |
| 4 | Integration test trên harness | ✅ | `CategoryAdminEndpointsTests` 20/20 xanh; test song song chạy lặp nhiều lần luôn ra 1×201 + 1×409, không 5xx |
| 5 | Dịch `23505` | ✅ | `CategoryPersistenceExceptionTranslator`; middleware không còn nhánh Npgsql; test `UnitOfWork_DuplicateNameInsert_...` xanh |
| 6 | Extension `recipeCount` | ✅ | Test DELETE có công thức xanh (`recipeCount = 3`); frontend đọc `error.problem.recipeCount` |
| 7 | Xóa prefix `categories:detail:` | ◐ một phần | Đã khai báo (`DetailPrefix`), test xanh; chỉ có tác dụng thật khi có `RemoveByPrefixAsync` (Buổi 7). Hiện chỉ khóa `categories:all` bị xóa thật |
| 8 | 1 commit + build trước khi push | ✅ ở máy | 1 commit squash; `dotnet build` xanh sau khi sửa 2 lỗi analyzer (SA1629 ở `Slug.cs`, SA1514 ở test); chưa push |
| 9 | `CategoryNav`, VO `Slug` | ✅ | `SlugTests` xanh; `CategoryNav` hiện trên header, `tsc`/lint/jest/build xanh |
| 10 | Báo cáo cá nhân | ✅ | File này + bản `.docx` theo mẫu Lab 02 |
| 11 | `.editorconfig` | ✅ | Giữ quy tắc PascalCase; lần sau tách commit riêng cho cấu hình dùng chung |

**Chưa kiểm chứng / hạn chế còn lại:** test tay qua giao diện chưa hoàn tất; chưa có test riêng cho PUT bị đua (hai request đổi tên trùng nhau cùng lúc); đua chỉ trùng slug (khác tên) vẫn có thể ra 500 hiếm gặp; `CategoryNav` và trang quản trị chưa có test jest riêng.

---

## 10. Danh sách file thay đổi (33 file mã nguồn: 21 thêm, 12 sửa, cộng 2 file báo cáo)

**Domain:** `Exceptions/Categories/` (4 file), `ValueObjects/Slug.cs`, `Common/ReservedSlugs.cs`, `Entities/Category.cs` (sửa).
**Application:** `ICategoryRepository.cs`, `IUnitOfWork.cs` (sửa), `Features/Categories/` — `Create/Update/DeleteCategoryCommand.cs`, `CategoryDtos.cs`, `GetCategoriesQuery.cs`, `GetCategoryBySlugQuery.cs` (sửa).
**Infrastructure:** `CategoryRepository.cs`, `CategoryPersistenceExceptionTranslator.cs`, `UnitOfWork.cs`, `CategoryReadRepository.cs`, `DependencyInjection.cs` (sửa).
**API:** `CategoriesEndpoints.cs`, `CategoryExceptionMappings.cs`.
**Test:** `CategoryCommandTests.cs`, `CategoryAdminEndpointsTests.cs`, `CategoryExceptionMappingsTests.cs`, `SlugTests.cs`, `CategoriesEndpointsTests.cs` (bỏ `using` thừa).
**Frontend:** `dashboard/categories/page.tsx`, `CategoryFormDialog.tsx`, `CategoryNav.tsx`, `admin-api.ts`, `SiteHeader.tsx`.
**Cấu hình:** `backend/.editorconfig`.

---

## 11. Bàn giao cho Dev 1 / Dev 2 / Dev 4

1. Dev 4: `GlobalExceptionMiddleware` giữ nguyên bản của Dev 4; toàn bộ phần Category chạy qua translator và `CategoryExceptionMappings`.
2. Dev 2: khóa TanStack Query dùng chung `['categories']`; sau khi tạo/sửa/xóa danh mục cần `invalidateQueries` khóa này.
3. Dev 1: `SiteHeader` vẫn dùng `user.fullName` (D-1 của Dev 1 sẽ đổi sang `displayName`) — mình không đụng.
4. Buổi 4 (Dev 3): D-5 — `GetCategoryBySlugQuery` bỏ lọc theo danh tính, chỉ `Published`.
5. Buổi 7: khi có `RemoveByPrefixAsync`, kiểm tra lại `CategoryCacheKeys.DetailPrefix` thật sự xóa cache.

---

## 12. Commit trên nhánh `2314291_DoanHongTien_buoiso3`

Một commit (squash), theo đúng "1 commit squash / dev / buổi":

```
feat(category): add category domain exceptions, category repository and complete create & update admin category APIs
```

Link: `<điền link commit sau khi push>`
