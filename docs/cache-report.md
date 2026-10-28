# Báo cáo audit cache — Buổi 8 (Dev 4)

**Phạm vi:** NFR-PERF-003 (Redis cache-aside, bảng TTL chuẩn, hit rate ≥ 80 %), NFR-SEC-006 (cách ly cache công khai/riêng tư — MT-34),
retrofit D-6 (gỡ Output Cache). **Ngày đo:** 21/10/2026, nhánh `2312755_NguyenThangThieng_buoiso8`.
**Kết luận ngắn:**
- tìm ra 1 lỗi thật (xóa cache theo tiền tố không có tác dụng);
- 3 chỗ lệch bảng TTL đã sửa;
- 1 mâu thuẫn trong chính SRS đã đi qua CR-2026-06;
- hit rate giai đoạn ổn định: **75,4 % → 97,3 %**.

## 1. Cơ chế được kiểm

| Hạng mục | Kết quả | Bằng chứng |
|---|---|---|
| Cơ chế cache duy nhất là Redis cache-aside | Đạt — không còn `AddOutputCache`/`CacheOutput`/`IMemoryCache` trong `backend/src` (D-6) | `grep -rn "OutputCache\|IMemoryCache" backend/src` rỗng |
| Chỉ query công khai được cache | Đạt — đúng 6 query cài `ICacheable`: `GetCategoriesQuery`, `GetCategoryBySlugQuery`, `GetRecipesQuery`, `GetRecipeBySlugQuery`, `GetRecipeSitemapQuery`, `SearchRecipesQuery`; `/recipes/mine`, `/auth/sessions`, `/users` không cache | `ObservabilityRulesTests.OnlyPublicQueries_ImplementICacheable` (danh sách đóng) |
| Query không xóa cache, command không được cache | Đạt | `ObservabilityRulesTests.CommandsNeverImplementICacheable_AndQueriesNeverInvalidate` |
| Tiền tố instance `culinaryblog:` | Đạt — `RedisCacheOptions.InstanceName`; mẫu SCAN mang đúng tiền tố này | `RedisCacheService.RemoveByPrefixAsync` |
| Xóa theo tiền tố dùng `SCAN`, không `KEYS` | **Trước Buổi 8: không có** — xem §2. Nay `IServer.KeysAsync` (SCAN, COUNT 250) + `DEL` theo lô | `ObservabilityInfrastructureTests.RemoveByPrefix_DeletesWholeFamilyOnly` |
| Redis lỗi → đọc DB, không ném exception (NFR-REL-002) | Đạt — đo dưới tải ở `docs/load-test-report.md` §5 | lượt "tắt Redis giữa lúc đo" |

## 2. Lỗi tìm ra: "xóa theo tiền tố" không xóa gì

Bảng TTL ghi `recipes:list:{queryHash}` *"xóa theo prefix `recipes:list:`"*. Code từ Buổi 4 khai báo đúng tiền tố (`RecipeCacheKeys.ListPrefix`,
`CategoryCacheKeys.DetailPrefix`), nhưng `CacheInvalidationBehavior` gọi `RemoveAsync(key)` cho mọi khóa — tức là xóa **một khóa tên
`recipes:list:`** không bao giờ tồn tại. Hệ quả: sửa hay xuất bản một công thức thì trang chủ và trang danh mục vẫn trả dữ liệu cũ tới hết TTL
2 phút. Chú thích trong code đã ghi "xóa theo tiền tố bằng SCAN là việc của Buổi 7" nhưng việc đó chưa ai làm.

**Sửa:** khóa kết thúc bằng `:` là tiền tố → `ICacheService.RemoveByPrefixAsync` (SCAN). Kiểm thử hồi quy:
`UpdatingPublishedRecipe_RefreshesCachedPublicListImmediately` — đọc danh sách (cache), sửa tiêu đề một công thức đã xuất bản, đọc lại ngay → thấy
tiêu đề mới.

## 3. Bảng đối chiếu: mọi command ↔ khóa phải xóa

Cột "SRS v1.2.4" là cột "Invalidate khi" của bảng TTL sau CR-2026-06; "chạm Published" = công thức đang Published đổi nội dung, hoặc vào/ra
trạng thái Published. `recipe:{slug}` = R, `recipes:list:` = L, `categories:detail:` = CD, `categories:all` = CA, `recipes:sitemap` = S.

| Command | Khóa SRS v1.2.4 yêu cầu | Code trước Buổi 8 | Code sau Buổi 8 |
|---|---|---|---|
| `CreateCategoryCommand`, `UpdateCategoryCommand`, `DeleteCategoryCommand` | CA, CD | CA, CD (CD không có tác dụng — §2) | CA, CD |
| `CreateRecipeCommand` (luôn tạo bản nháp) | — (không chạm Published) | L (không có tác dụng) | — |
| `UpdateRecipeCommand` | R (cả slug cũ và mới); nếu Published: L, CD; đổi danh mục + Published: CA | R, L, `search:` | R; nếu Published: L, CD (+ CA khi đổi danh mục) |
| `Add/Update/DeleteIngredientCommand`, `Add/Update/DeleteStepCommand`, `ReorderStepsCommand` | R; nếu Published: L, CD | R, L, `search:` | R; nếu Published: L, CD |
| `UploadRecipeImageCommand`, `UpdateRecipeImageCommand`, `DeleteRecipeImageCommand` | R; nếu Published: L, CD | R, L, `search:` | R; nếu Published: L, CD |
| `ImageResizeJob` (ghi thumbnail ngoài pipeline MediatR) | R; nếu Published: L, CD | **không xóa gì** | R; nếu Published: L, CD |
| `PublishRecipeCommand`, `UnpublishRecipeCommand` | R, L, CD, CA, S | R, L, `search:`, S, CD, CA | R, L, CD, CA, S |
| `ArchiveRecipeCommand`, `UnarchiveRecipeCommand`, `DeleteRecipeCommand` | trước hoặc sau là Published: R, L, CD, CA, S; còn lại: R | như trên, mọi trạng thái | theo trạng thái trước/sau |
| `UpdateProfileCommand` (tên hiển thị, ảnh đại diện) | — (bảng TTL không liệt kê) | — | — · tên/ảnh tác giả trên thẻ công thức cũ tối đa 2 phút (danh sách), 5 phút (chi tiết) — chấp nhận theo TTL |
| Các command Auth, Files, `SetUserStatusCommand` | — | — | — |

**Ba chỗ code lệch bảng đã sửa:**
1. `search:` bị liệt kê để xóa ở mọi thay đổi công thức — bảng ghi "không invalidate, để hết hạn tự nhiên" (TTL 1 phút). Trước Buổi 8 việc này vô
   hại vì xóa theo tiền tố không chạy (§2); bật SCAN mà giữ nguyên thì sẽ xóa thật, làm khác SRS.
2. `categories:detail:` không bị xóa khi nội dung công thức đổi, trong khi trang danh mục hiện thẻ công thức (tiêu đề, ảnh, thời gian).
3. `ImageResizeJob` ghi thumbnail mà không xóa cache — trang chi tiết giữ bản không có thumbnail tới hết TTL 5 phút.

**Một mâu thuẫn của chính SRS (MT-69 → CR-2026-06, SRS v1.2.4):** bảng TTL v1.2.3 xóa L và CD ở *mọi* thay đổi công thức, kể cả bản nháp không
bao giờ có trong hai họ khóa đó — đo được hit rate 75,4 %, dưới chỉ tiêu ≥ 80 % của cùng NFR (§4). Trưởng nhóm duyệt CR ngày 21/10/2026.

## 4. Hit rate đo trong lúc load test

Đo bằng `redis-cli INFO stats` (`keyspace_hits`, `keyspace_misses`; `CONFIG RESETSTAT` + `FLUSHALL` trước mỗi lượt), lấy mẫu mỗi ~10 giây; "ổn
định" = từ giây 90 tới hết giai đoạn giữ 100 người dùng (bỏ 1 phút tăng tải và 30 giây đầu sau khi cache lạnh). Môi trường, kịch bản và cách chạy:
`docs/load-test-report.md`.

| Lượt (k6 `load.js`, 100 người dùng, 3 bản sao API) | Hit rate cả lượt | Hit rate ổn định | Ghi chú |
|---|---|---|---|
| A. Code đúng chữ SRS v1.2.3 (sau khi sửa §2) | 75,0 % (23.192 / 30.904) | **75,4 %** | 10 % lượt ghi ≈ 8,8 lần/giây, đều lên bản nháp |
| B. Đối chứng — cùng tải, không ghi (phần ghi thay bằng đọc danh sách) | 97,0 % | 97,9 % | cô lập nguyên nhân: không có xóa cache |
| C. Sau CR-2026-06 | 96,7 % (29.867 / 30.894) | **97,3 %** | cùng kịch bản với A, 0 lỗi |

**Phân tích lượt A:** 32 % lượt đọc là trang chủ (20 %) và trang danh mục (12 %) — đúng hai họ khóa bị xóa ~9 lần mỗi giây, nên gần như luôn miss;
chi tiết công thức (30 %), danh sách danh mục (8 %) và tìm kiếm (20 %) vẫn trúng cao. TTL không phải nguyên nhân: không nới TTL nào.

**Phần còn lại ~2,7 % miss ở lượt C** đến từ: lần đọc đầu tiên của mỗi khóa sau khi hết TTL (tìm kiếm 1 phút, danh sách 2 phút); đuôi dài của
công thức ít người xem; vài khóa chi tiết bị xóa khi tác giả sửa bản nháp của chính mình (R luôn bị xóa — một khóa, rẻ).
