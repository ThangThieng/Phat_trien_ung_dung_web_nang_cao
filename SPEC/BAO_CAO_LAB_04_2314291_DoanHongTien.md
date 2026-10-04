# BÁO CÁO KẾT QUẢ BUỔI 4 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (Category / Recipe read / Search)
**Nhánh:** `2314291_DoanHongTien_buoiso4` (tách từ tip nhánh Dev 4 `2312755_NguyenThangThieng_buoiso4` — đã chứa code Buổi 3 của cả 4 dev, `Recipe.Lifecycle.cs`, `RecipeLifecycleEndpoints.cs`, migration `B4_Recipe_PartialUniqueSlug`; merge chuẩn Dev 4 → Dev 1 → Dev 3 → Dev 2) — **PR nhắm vào `develop`**: `<điền link PR>`
**Phạm vi (KE_HOACH_PHAT_TRIEN_8_BUOI.md — Buổi 4, Dev 3):** API của FR-SRCH-001 → 004, FR-RCP-011, CR-2026-04 (d) `GET /recipes/mine/{id}`, MT-48 `GET /recipes/sitemap`; sửa hợp đồng `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}`; retrofit **D-4, D-5 (lỗ hổng MT-34), D-6, D-10 (API), D-18**

> **Trình tự làm:** giai đoạn 1 vá/sửa trước (D-4, D-5, D-6, D-10 API, D-18 — theo kế hoạch bước 1 *"làm bước 4 trước tiên — vá MT-34"*), build + test xanh 277/277; giai đoạn 2 thêm bốn endpoint mới (`/recipes/search`, `/recipes/mine`, `/recipes/mine/{id}`, `/recipes/sitemap`), build + test xanh **319/319**. Báo cáo này gồm cả hai giai đoạn.
**Tài liệu bám sát:** `SPEC/SRS_Culinary_Blog_v1.2.2.md` (§3.3 nguyên tắc Public/Private, FR-RCP-001/002/011, FR-CAT-002, FR-SRCH-001→004, NFR-PERF-003, NFR-SEC-006, NFR-SEO-003/004, §7.2, §8.3) và `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` (MT-01, MT-16, MT-17, MT-25, MT-34, MT-48, MT-53, MT-62; §4 D-4/D-5/D-6/D-10/D-18)

> **Trạng thái kiểm chứng (04/10/2026):** `dotnet build` thành công và `dotnet test` **319/319 đạt, 0 lỗi, 0 bỏ qua** trên máy Windows của Dev 3 (Testcontainers: PostgreSQL, Redis, MinIO). Các hạng mục chưa kiểm chứng được ghi rõ ở mục 8.

---

## 1. Tóm tắt các công việc

| # | Hạng mục | Yêu cầu trong kế hoạch | Kết quả |
|---|----------|------------------------|---------|
| A | **Vá MT-34 (D-4, D-5)** — làm trước tiên | `GET /recipes`, `GET /recipes/{slug}`, `GET /categories/{slug}` chỉ trả Published cho mọi người gọi; Draft/Archived → 404; bỏ `RecipeVisibility`, bỏ `ICurrentUser` khỏi 3 query | Đã test xanh — mục 2 |
| B | **D-6 — bỏ Output Cache** | Xóa `OutputCachePolicies.cs`, `AddCulinaryOutputCache`, `.CacheOutput(...)`; chuyển sang `ICacheable` Redis cache-aside 2′/5′/2′ | Đã test xanh — mục 2 |
| C | **D-10 (API) — `RecipeFilterSpec`, `SortMapper`, phân trang** | `sortBy` + `sortOrder` whitelist 5 trường → 400; thêm `maxPrepTime`, `minServings`; `pageSize` 12/50; xóa `RecipeSortParser`, `RecipeSortField` | Đã test xanh — mục 3 |
| D | **D-18 + migration `B4_Search_FTS`** | `unaccent_immutable` IMMUTABLE, generated column `SearchVector` STORED, GIN `IDX_Recipe_Search`; gỡ `vietnamese_unaccent` khỏi `init.sql` | Đã test xanh — mục 4 |
| E | **FR-SRCH-001 `GET /recipes/search`** | `q` ≥ 2 ký tự, làm sạch tsquery, `ts_rank` DESC, chỉ Published, `relevanceScore`, cache `search:{hash}` 1′ | Đã test xanh — `SearchRecipesQuery`, `SearchTermBuilder` |
| F | **FR-RCP-011 `GET /recipes/mine`** | Bearer bắt buộc, không `ICacheable`, `no-store`, `authorId` chỉ Admin (khác → 403), `status` whitelist | Đã test xanh — `GetMyRecipesQuery` |
| G | **CR-2026-04 (d) `GET /recipes/mine/{id}`** | Mọi trạng thái, Owner/Admin qua `RecipeAuthorizationHandler`, người khác/không tồn tại → 404, `rowVersion` + `ETag` + `no-store` | Đã test xanh — `GetMyRecipeByIdQuery` |
| H | **MT-48 `GET /recipes/sitemap`** | `{ slug, updatedAt }[]` mọi Published, không phân trang, cache `recipes:sitemap` 1 giờ | Đã test xanh — `GetRecipeSitemapQuery` |
| I | **`RecipeQueryEndpoints.cs`** | Gom mọi `GET /recipes` về một file, chuyển hai GET cũ ra khỏi `RecipesEndpoints.cs` | Xong — đủ 6 GET của `/recipes` trong một file |
| J | **Integration test phần vá** | Gỡ `Skip` test MT-34 + cập nhật test 403 → 404 + test sort/lọc/phân trang | Đã test xanh — mục 7 |

---

## 2. Chi tiết A + B — Vá MT-34 (D-4, D-5) và bỏ Output Cache (D-6)

**Lỗ hổng (SRS §3.3, MT-34):** ở Buổi 2, `GetRecipesQuery` lọc theo danh tính (Guest thấy Published, Author thấy thêm Draft của mình, Admin thấy tất cả) trong khi Output Cache lưu response dưới khóa chỉ gồm query string → một lượt gọi của Admin nạp Draft của mọi tác giả vào cache, Guest gọi cùng URL đọc được.

**Cách vá — tách bạch về kiến trúc, không thêm `userId` vào khóa cache (SRS FR-RCP-011, phần giải trình MT-34):**

- `IRecipeReadRepository` bỏ `RecipeVisibility`. Phương thức công khai mang tiền tố `Published…` (`GetPublishedPagedAsync`, `GetPublishedBySlugAsync`, `SearchPublishedAsync`, `GetPublishedSitemapAsync`) cố định `Status == Published` **trong repository** và **không nhận danh tính làm tham số** — không có đường nào vô tình trả bản nháp qua endpoint công khai.
- `GetRecipesQuery`, `GetRecipeBySlugQuery`, `GetCategoryBySlugQuery` không còn inject `ICurrentUser`.
- `GetRecipeBySlugQuery`: Draft/Archived → **404 `RECIPE_NOT_FOUND`**, cùng thông điệp với slug không tồn tại (FR-RCP-002 A2), thay cho 403 `RECIPE_FORBIDDEN` của Buổi 2.
- Phần riêng tư đối xứng (`/recipes/mine`, `/recipes/mine/{id}`) làm cùng buổi → gỡ lọc danh tính khỏi endpoint công khai không làm mất đường nào để tác giả xem bản nháp của mình.

**D-6 — một cơ chế cache duy nhất (NFR-PERF-003, MT-16/MT-17):**

| Query | `ICacheable` | Khóa | TTL |
|---|---|---|---|
| `GetRecipesQuery` | ✔ | `recipes:list:{sha256(query chuẩn hóa)}` | 2 phút |
| `GetRecipeBySlugQuery` | ✔ | `recipe:{slug}` | 5 phút |
| `GetCategoryBySlugQuery` | ✔ | `categories:detail:{slug}:{sha256}` | 2 phút |
| `SearchRecipesQuery` | ✔ | `search:{sha256(tsquery + tham số)}` | 1 phút, không invalidate |
| `GetRecipeSitemapQuery` | ✔ | `recipes:sitemap` | 1 giờ |
| `GetMyRecipesQuery`, `GetMyRecipeByIdQuery` | **✘ cố ý** | — | cấm cache + `Cache-Control: no-store` |

- Xóa `API/Extensions/OutputCachePolicies.cs`, lời gọi `AddCulinaryOutputCache`, `UseOutputCache`, hai `.CacheOutput(...)`, khối `EvictByTagAsync` sau seed trong `Program.cs`, và gói `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis` khỏi `CulinaryBlog.API.csproj`.
- `{queryHash}` = SHA-256 hex của chuỗi tham số chuẩn hóa (`Application/Common/Caching/QueryHash.cs`); chuẩn hóa không phân biệt hoa/thường nên `sortBy=Title` và `sortBy=title` dùng chung một khóa.

---

## 3. Chi tiết C — D-10 phía API: `RecipeFilterSpec`, `SortMapper`, phân trang

- **`SortMapper`** (`Application/Features/Recipes/SortMapper.cs`): dictionary whitelist `createdAt, publishedAt, title, cookTime, prepTime` → `Expression<Func<Recipe, object?>>`; `sortOrder ∈ {asc, desc}`; mặc định `createdAt desc`. Tên cột **không bao giờ** ghép vào SQL (FR-SRCH-003, rào chắn SQL injection). Tie-breaker `ThenBy(Id)` để phân trang ổn định.
- **`RecipeFilterSpec`**: `categoryId`, `difficulty` (Easy/Medium/Hard/Expert), `maxCookTime`, `maxPrepTime`, `minServings` — AND; dùng chung cho `GetRecipesQuery`, `SearchRecipesQuery` (chỉ `categoryId`, `difficulty` theo §8.3), `GetCategoryBySlugQuery` (chỉ `categoryId`), `GetMyRecipesQuery`.
- **`RecipeQueryRules`**: một bộ rule FluentValidation dùng chung (`page ≥ 1`, `pageSize ∈ [1, 50]`, whitelist sort, enum difficulty, số phút ≥ 0, `minServings ≥ 1`) → mọi vi phạm là **400 `VALIDATION_ERROR`**.
- Xóa `RecipeSortParser.cs` và enum `RecipeSortField`. `GET /categories/{slug}` nhận thêm `sortBy`/`sortOrder` (FR-CAT-002).
- **Tham số `sort` cũ:** nếu client còn gửi `sort=...` → 400 (thông báo chỉ sang `sortBy`/`sortOrder`). Tham số này **không** khai báo trên endpoint (không xuất hiện lại trên Scalar), endpoint chỉ đọc nó từ query string để chuyển cho validator — xem mâu thuẫn #4 ở mục 6.

---

## 4. Chi tiết D — Migration `B4_Search_FTS` + retrofit D-18

`Infrastructure/Persistence/Migrations/20260930120000_B4_Search_FTS.cs` (viết tay theo tiền lệ `B3_Recipe_InstructionsNullable`, tự khai báo `[DbContext]` + `[Migration]`), thứ tự bắt buộc trong `Up()`:

1. `CREATE EXTENSION IF NOT EXISTS unaccent;` / `pg_trgm;` — idempotent, không giả định `init.sql` đã chạy.
2. `CREATE OR REPLACE FUNCTION unaccent_immutable(text) … LANGUAGE sql IMMUTABLE STRICT;` — đúng nguyên văn SRS §7.2.
3. `AddColumn<NpgsqlTsVector>("SearchVector", computedColumnSql: to_tsvector('simple', unaccent_immutable(coalesce("Title",'') || ' ' || coalesce("Description",''))), stored: true)` — generated column STORED, **không trigger** (MT-25). 100 công thức mẫu được tính ngay khi migration chạy, không cần backfill.
4. GIN index `IDX_Recipe_Search`.

`Down()` xóa theo thứ tự ngược lại (không DROP EXTENSION vì thuộc B1).

- **Model:** `SearchVector` là **shadow property** trong `RecipeConfiguration` — Domain chỉ dùng .NET BCL nên entity `Recipe` không mang kiểu `NpgsqlTsVector` (CONS-001, NFR-MAINT-004). Model snapshot cập nhật tay tương ứng.
- **D-18:** `docker/postgres/init.sql` chỉ còn hai dòng `CREATE EXTENSION`; khối `vietnamese_unaccent` bị gỡ → hệ thống chỉ còn **một** cơ chế FTS (`simple` + `unaccent_immutable`) và nó nằm trong migration nên chạy được cả trên Testcontainers.

---

## 5. Chi tiết E → I — Bốn endpoint mới (GIAI ĐOẠN 2 — thiết kế đã chốt, chưa đưa vào nhánh) và `RecipeQueryEndpoints.cs`

> `RecipeQueryEndpoints.cs` chứa đủ 6 `GET` của `/recipes`. Bảng dưới là hợp đồng đã cài đặt.

| Endpoint | Query | Auth | Hợp đồng chính |
|---|---|---|---|
| `GET /api/v1/recipes` | `GetRecipesQuery` | Không | Chỉ Published; 5 bộ lọc AND; `sortBy`/`sortOrder`; 400 khi ngoài whitelist |
| `GET /api/v1/recipes/mine` | `GetMyRecipesQuery` | Bearer (Author/Admin) | Mọi trạng thái của người gọi; `status` whitelist; `authorId` chỉ Admin → khác 403 `RECIPE_FORBIDDEN`; `Cache-Control: no-store` |
| `GET /api/v1/recipes/mine/{id}` | `GetMyRecipeByIdQuery` | Bearer (Owner/Admin) | Mọi trạng thái; quyền qua `RecipeAuthorizationHandler`; không phải chủ hoặc không tồn tại → **404** (không 403 — FR-RCP-011 A5); `rowVersion` + `ETag: "{rowVersion}"` + `no-store` |
| `GET /api/v1/recipes/search` | `SearchRecipesQuery` | Không | `q` ≥ 2 ký tự; `SearchTermBuilder` bỏ dấu + làm sạch → `pho:* & bo:*`; `@@ to_tsquery('simple', …)` tham số hóa; `ts_rank` DESC; `relevanceScore` |
| `GET /api/v1/recipes/sitemap` | `GetRecipeSitemapQuery` | Không | `[{ slug, updatedAt }]` mọi Published, `updatedAt = UpdatedAt ?? PublishedAt ?? CreatedAt` |
| `GET /api/v1/recipes/{slug}` | `GetRecipeBySlugQuery` | Không | Chỉ Published; Draft/Archived → 404 |
| `GET /api/v1/categories/{slug}` | `GetCategoryBySlugQuery` | Không | Chỉ Published; `sortBy`/`sortOrder` |

- **`SearchTermBuilder`** dùng lại `SlugHelper.Generate` (lower → đ→d → bỏ dấu → chỉ giữ `a-z0-9`) để vế truy vấn khớp đúng vế `unaccent_immutable` của cột; mọi ký tự không phải chữ/số (`& | ! ( ) : * ' < >`) bị bỏ → người dùng không tự viết được toán tử tsquery. Còn rỗng sau làm sạch (ví dụ `q=!!`) → 200 trang rỗng, không gửi truy vấn xuống DB.
- **`RecipeSearchResultDto`**: đúng các trường của `RecipeSummaryDto` + `relevanceScore`, khai báo phẳng để JSON cùng hình dạng với danh sách công khai và Redis đọc lại được.
- **`RecipeQueryEndpoints.cs`** gắn vào cùng `MapGroup("/recipes")` với `RecipesEndpoints` (Dev 2) và `RecipeLifecycleEndpoints` (Dev 4); `Program.cs` thêm `.MapRecipeQueryEndpoints()`. Segment literal `mine`/`search`/`sitemap` luôn thắng `{slug}` và cả ba nằm trong `ReservedSlugs` (NFR-SEO-004).
- **Sitemap invalidation:** khóa `recipes:sitemap` đã có trong `RecipeCacheKeys.ForVisibilityChange` do Dev 4 dùng ở các command vòng đời — không phải sửa file của Dev 4.
- `ApiSurfaceTests`: xóa 4 dòng `PendingEndpoints` của Dev 3 (test `PendingEndpoints_AreReallyMissing` sẽ đỏ nếu quên).

---

## 6. Mâu thuẫn / điểm lệch gặp trong Buổi 4 và cách giải quyết

Nguyên tắc áp dụng (kế hoạch §3, file mâu thuẫn §4): **mâu thuẫn thì giải quyết theo SRS v1.2.2 và biện pháp đã chốt trong `SRS_MAU_THUAN_VA_GIAI_PHAP.md`; không thêm chức năng ngoài SRS; điểm nào SRS chưa chốt thì chọn cách hẹp nhất và ghi lại.**

| # | Điểm lệch | Căn cứ | Cách giải quyết |
|---|---|---|---|
| 1 | *(giai đoạn 2)* FR-RCP-011: A2 ghi *"Author truyền `authorId` **của người khác** → 403"*, còn Điều kiện tiên quyết 5 + bước 4 + §8.3 ghi *"`authorId` **chỉ** được chấp nhận khi người gọi là Admin"* | SRS FR-RCP-011, §8.3 | Không phải mâu thuẫn thật: A2 là một trường hợp của quy tắc chung. Áp dụng quy tắc chung (chặt hơn): người gọi không phải Admin mà truyền `authorId` bất kỳ → 403 `RECIPE_FORBIDDEN`. Không cần CR |
| 2 | Bảng TTL (NFR-PERF-003) yêu cầu xóa theo tiền tố `recipes:list:` / `categories:detail:` khi dữ liệu đổi, nhưng `CacheInvalidationBehavior` hiện chỉ xóa theo khóa chính xác; `RemoveByPrefixAsync` (SCAN) là việc của **Buổi 7 — Dev 4** | Kế hoạch §2.3, B7 Dev 4; báo cáo Lab 03 §11.5 | Giữ đúng phân công: không tự làm việc của Dev 4. Hệ quả chấp nhận được cho tới B7: danh sách/chi tiết danh mục cũ tối đa **2 phút** (hết hạn tự nhiên theo TTL). `recipe:{slug}` và `recipes:sitemap` là khóa chính xác nên được xóa ngay. Ghi rõ trong `RecipeCacheKeys`/`CategoryCacheKeys` |
| 3 | Integration test chạy chung một Redis: sau khi bật cache cho endpoint công khai, response của lớp test trước có thể được trả cho lớp test sau | NFR-MAINT-002 | `CulinaryBlogApiFactory.ResetDatabaseAsync` thêm `redis-cli FLUSHALL` (sạch cache cùng lúc sạch DB); các lớp test đọc mà tôi sửa (`RecipesEndpointsTests`, `CategoriesEndpointsTests`, `RecipeCacheIsolationTests`) chuyển sang reset ở `InitializeAsync`. Sửa nhỏ trên harness của Dev 4 — cần Dev 4 review |
| 4 | Kế hoạch bước 10 yêu cầu `?sort=-title → 400`, nhưng SRS không còn tham số `sort` nào (MT-01) — ASP.NET Core mặc định im lặng bỏ qua tham số lạ, trái FR-SRCH-003 *"không im lặng bỏ qua"* | MT-01, FR-SRCH-003 | Validator trả 400 khi có `sort`; endpoint đọc giá trị từ query string, **không** khai báo tham số → không thêm gì vào hợp đồng API/Scalar |
| 10 | Vá D-4 trước khi có `/recipes/mine` (giai đoạn 2): trong khoảng giữa hai giai đoạn (đã đóng — `/recipes/mine` và `/recipes/mine/{id}` đã có), tác giả tạm **không xem lại được bản nháp** của mình qua API (FR-RCP-011 là phần đối xứng của MT-34) | Kế hoạch bước 1, FR-RCP-011 | Chấp nhận trong thời gian ngắn giữa hai giai đoạn cùng buổi; không mở lại lỗ hổng (không cho `/recipes/{slug}` trả Draft cho chủ — phương án (A) đã bị loại ở MT-62). Commit/merge phải chứa cả hai giai đoạn |
| 5 | Frontend `/recipes` (Buổi 2) đang gửi `sort=-createdAt`; D-10 phía giao diện thuộc **Buổi 5** — nếu để nguyên, trang `/recipes` trả 400 ngay sau khi merge Buổi 4, trái nguyên tắc *"cuối mỗi buổi hệ thống phải chạy được"* | Kế hoạch §0, D-10 | Cầu nối tối thiểu trong `frontend/src/features/recipes/api.ts` (thuộc module của Dev 3): đổi `sort` → `sortBy` + `sortOrder` trước khi gọi API. FilterPanel, trang `/search` và D-10 FE đầy đủ vẫn ở Buổi 5 |
| 6 | *(giai đoạn 2)* Kế hoạch bước 7 ghi nạp qua `GetByIdWithDetailsAsync` cho `/mine/{id}` | Kế hoạch B4 Dev 3 bước 7 | Phân quyền chỉ cần `AuthorId` nên dùng `unitOfWork.Recipes.GetByIdAsync` (không kéo bước/nguyên liệu/ảnh hai lần), DTO chi tiết lấy từ read repository. Vẫn đúng tinh thần: nạp ở mọi trạng thái + kiểm quyền bằng **chính** `RecipeAuthorizationHandler` |
| 7 | *(giai đoạn 2)* SRS FR-SRCH-001 ghi kết quả là *"`PagedResult<RecipeSummaryDto>` với field `relevanceScore`"* nhưng `RecipeSummaryDto` không có trường này | FR-SRCH-001 bước 7 | `RecipeSearchResultDto` = đúng các trường của `RecipeSummaryDto` + `relevanceScore` (JSON cùng hình dạng, chỉ thêm một trường) |
| 8 | SRS §3.3 quy tắc 2: *"cấu hình cache bỏ qua mọi request có header `Authorization`"* — viết cho Output Cache; `CachingBehavior` (Dev 4) chưa có lớp phòng vệ này | SRS §3.3 | Không sửa `CachingBehavior` (file dùng chung của Dev 4). Endpoint công khai nay không phụ thuộc danh tính nên không rò rỉ; ghi nhận để Dev 4 cân nhắc ở lượt audit cache B7 |
| 9 | Volume `pgdata` đã tạo trước đây vẫn còn text search config `vietnamese_unaccent` (init.sql chỉ chạy lần đầu) | D-18 | Không ảnh hưởng (code không còn tham chiếu). Muốn sạch hẳn: `DROP TEXT SEARCH CONFIGURATION IF EXISTS vietnamese_unaccent;` hoặc tạo lại volume |

Không phát sinh điểm nào cần Change Request mới: không thêm endpoint, mã lỗi hay trường schema nào ngoài SRS v1.2.2 (cột `SearchVector` và index `IDX_Recipe_Search` có trong §7.2).

---

## 7. Test đã viết (đã chạy: 319/319 đạt)

**Unit — `Application.UnitTests/RecipeQueryTests.cs` (mới):** whitelist `SortMapper` (5 trường, không phân biệt hoa/thường, từ chối `password`, `-createdAt`, `servings`); `sortOrder`; mặc định `createdAt desc`; `pageSize` mặc định 12; tham số `sort` cũ → lỗi; `difficulty` ngoài enum; bộ lọc âm; khóa cache không phân biệt hoa/thường và đúng tiền tố. `ValidatorTests`: bỏ test `RecipeSortParser` (đã xóa). Thêm test `SearchTermBuilder` và validator của `SearchRecipesQuery` / `GetMyRecipesQuery`.

**Integration (Testcontainers):**

| Lớp | Nội dung |
|---|---|
| `RecipeCacheIsolationTests` | **Gỡ `Skip`**. Admin gọi `GET /recipes` rồi Guest gọi cùng URL → không có Draft, `totalCount = 3`; chi tiết danh mục cho chủ bản nháp chỉ có Published; Draft qua `/recipes/{slug}` với chủ sở hữu → 404 |
| `RecipesEndpointsTests` | `GetRecipeBySlug_DraftAsGuest_Returns403ForNow` → **`GetRecipeBySlug_Draft_Returns404ForEveryCaller`** (Guest/Author/Admin); xóa `DraftAsOwner_Returns200`; 400 cho `sort=-title`, `sortBy=password`, `sortOrder=up`, `difficulty=Legendary`, `pageSize=51`, `maxPrepTime=-1`, `minServings=0`; `sortBy=title&sortOrder=desc` đúng thứ tự; `difficulty=Expert` → 200; `maxPrepTime` + `minServings`; `categoryId` lạ → 200 rỗng (A3); cờ phân trang |
| `RecipeQueryEndpointsTests` (mới, 18 test) | **Tìm kiếm:** "pho"/không dấu ra "Phở bò", xếp theo độ liên quan và chỉ Published, lọc `categoryId` kết hợp AND, `q` < 2 ký tự → 400, ký tự đặc biệt → 200. **Của tôi:** mọi trạng thái + `no-store`, lọc `status`, tác giả chưa có công thức → 200 rỗng, không token → 401, Author truyền `authorId` → 403, Admin truyền `authorId` → 200, tham số sai → 400. **Chi tiết riêng tư:** Draft của mình → 200 kèm `rowVersion` + `ETag` + `no-store`, Admin → 200, của người khác/id lạ → 404, không token → 401. **Sitemap:** chỉ slug Published; `POST` → 405 |
| `CategoriesEndpointsTests` | Reset mỗi test; `sortBy=title&sortOrder=asc`; 400 cho `sortBy=name`, `sortOrder=sideways`, `pageSize=51` |
| `ApiSurfaceTests` | Xóa 4 endpoint của Dev 3 khỏi danh sách chờ (đã cài đặt) |

---

## 8. Kiểm chứng cần chạy (Definition of Done §2.4) — điền kết quả thật

```powershell
cd backend
dotnet build                                   # 0 warning (TreatWarningsAsErrors)
dotnet ef migrations has-pending-model-changes -p src/CulinaryBlog.Infrastructure -s src/CulinaryBlog.API   # phải báo "No changes"
dotnet test                                    # kể cả integration test (cần Docker Desktop chạy)
cd ../frontend
npm run lint; npm run build
cd ..
docker compose up -d --build                   # migration B4_Search_FTS chạy lúc khởi động API
```

| Tiêu chí | Trạng thái |
|----------|-----------|
| `dotnet build` | ☑ 04/10/2026 — `Build succeeded` |
| Không còn thay đổi model chờ migration | ☐ chưa chạy lệnh `has-pending-model-changes` |
| `dotnet test` xanh, test MT-34 đã gỡ `Skip` và xanh | ☑ 04/10/2026 — `total: 319, failed: 0, succeeded: 319, skipped: 0` |
| `npm run lint && npm run build` xanh | ☑ `npm run build` thành công 04/10/2026 (sau `npm ci`); kết quả `lint` chưa ghi lại. Frontend không đổi ở giai đoạn 2 |
| `docker compose up` chạy, `/recipes` trên giao diện vẫn hiển thị | ☑ 04/10/2026 với code giai đoạn 1 — `/recipes` hiện 89 công thức Published, ba nút sắp xếp hoạt động; chưa dựng lại stack với code giai đoạn 2 |
| Scalar `/scalar`: đủ 4 endpoint mới, tìm "pho" ra "Phở bò" | ☐ chưa thử tay trên Scalar (đã có integration test tương ứng xanh) |
| `grep -r CacheOutput backend/src` không còn kết quả | ☑ không còn kết quả |

---

## 9. Danh sách file thay đổi

**Application (mới):** `Common/Caching/QueryHash.cs`, `Features/Recipes/SortMapper.cs`, `RecipeFilterSpec.cs`, `SearchRecipesQuery.cs`, `SearchTermBuilder.cs`, `GetMyRecipesQuery.cs`, `GetMyRecipeByIdQuery.cs`, `GetRecipeSitemapQuery.cs`.
**Application (sửa):** `Features/Recipes/GetRecipesQuery.cs`, `GetRecipeBySlugQuery.cs`, `IRecipeReadRepository.cs`, `RecipeCacheKeys.cs`, `RecipeDtos.cs`; `Features/Categories/GetCategoryBySlugQuery.cs`, `CategoryDtos.cs`. **Xóa:** `Features/Recipes/RecipeSortParser.cs`.
**Infrastructure:** `Persistence/Repositories/RecipeReadRepository.cs`, `Persistence/Configurations/RecipeConfiguration.cs`, `Persistence/Migrations/20260930120000_B4_Search_FTS.cs` (mới), `CulinaryBlogDbContextModelSnapshot.cs`.
**API:** `Endpoints/RecipeQueryEndpoints.cs` (mới), `RecipesEndpoints.cs`, `CategoriesEndpoints.cs`, `Program.cs`, `CulinaryBlog.API.csproj`. **Xóa:** `Extensions/OutputCachePolicies.cs`.
**Test:** `Application.UnitTests/RecipeQueryTests.cs` (mới), `ValidatorTests.cs`; `API.IntegrationTests/Content/RecipeQueryEndpointsTests.cs` (mới), `Platform/ApiSurfaceTests.cs`, `Content/RecipesEndpointsTests.cs`, `RecipeCacheIsolationTests.cs`, `CategoriesEndpointsTests.cs`, `Infrastructure/CulinaryBlogApiFactory.cs`.
**Khác:** `docker/postgres/init.sql` (D-18), `frontend/src/features/recipes/api.ts` (cầu nối sort), `SPEC/CONG_NGHE_VA_PHIEN_BAN.md` (đánh dấu gói Output Cache đã gỡ).

---

## 10. Bàn giao

1. **Dev 4:** review thay đổi nhỏ trên harness (`FLUSHALL` trong `ResetDatabaseAsync`); B7 khi làm `RemoveByPrefixAsync` thì các khóa `recipes:list:*`, `search:*`, `categories:detail:*` bắt đầu bị xóa thật — không cần sửa code của Dev 3. Cân nhắc điểm #8 mục 6 ở lượt audit cache.
2. **Dev 2:** trang sửa (Buổi 5) đọc `GET /recipes/mine/{id}` — lấy `rowVersion` trong body hoặc header `ETag` gửi lại cho `PUT /recipes/{id}`. Trang "Công thức của tôi" (Buổi 6) dùng `GET /recipes/mine`. `GET /recipes/{slug}` **không** còn trả bản nháp cho chủ sở hữu. Migration `B4_Recipe_IngredientStep` của Dev 2 merge sau Dev 3 → rebase rồi `dotnet ef migrations add` lại nếu snapshot xung đột.
3. **Dev 1:** không phụ thuộc.
4. **Buổi 5 (Dev 3):** FilterPanel + trang `/search` + D-10 FE (thay cầu nối trong `api.ts`), composite index + `EXPLAIN ANALYZE` (NFR-PERF-004).
5. **Buổi 7 (Dev 3):** `app/sitemap.ts` đọc `GET /recipes/sitemap`.

---

## 11. Commit trên nhánh `2314291_DoanHongTien_buoiso4`

Một commit (squash):

```
fix(search)!: close MT-34 draft leak; complete FR-SRCH-001..004 query APIs, FR-RCP-011 private my-recipes and sitemap source
```

> Dùng `fix!` (breaking) vì hành vi công khai thay đổi: Admin/Author không còn thấy Draft ở `GET /recipes`; Draft trả 404 thay vì 403; `sort=-field` bị thay bằng `sortBy`/`sortOrder`.

Link: `<điền link commit sau khi push>`
