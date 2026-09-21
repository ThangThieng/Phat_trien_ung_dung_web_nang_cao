# PHÂN TÍCH MÂU THUẪN TRONG SRS & ĐỀ XUẤT GIẢI PHÁP

**Tài liệu nguồn:** `SPEC/SRS_Culinary_Blog_v1.0.0.md` (chuyển đổi từ `SRS_Culinary_Blog_v1.0.0.pdf`, 71 trang) — lượt 1–3; SRS v1.1.0 (nay nằm trong `SPEC/SRS_Culinary_Blog_v1.2.1.md`, phần không mang dấu `[CR-2026-02]`) đối chiếu với code trên nhánh `main` và `SPEC/BAO_CAO_BUOI_2.md` — lượt 4
**Ngày rà soát:** 16/09/2026 (lượt 1–3) · 19/09/2026 (lượt 4) · 21/09/2026 (lượt 5)
**Phạm vi:** Toàn bộ 8 chương + các phụ lục; lượt 4 mở rộng sang **hệ thống đang chạy** (code, cấu hình Docker/Nginx, báo cáo Buổi 2)
**Kết quả:** **58 mâu thuẫn / bất nhất** — 41 mục ở lượt 1–3 (4 do nhóm phát hiện trước, 37 phát hiện thêm), 16 mục ở lượt 4 và 1 mục ở lượt 5. Ngoài ra ghi nhận 18 điểm lệch giữa code Buổi 2 và SRS (§4) cùng 5 điểm xung đột giữa lộ trình phát triển và SRS (§5).
**Trạng thái:** MT-01 → MT-41 đã áp dụng vào **SRS v1.1.0** (CR-2026, 17/09/2026). MT-42 → MT-57 đã áp dụng vào **SRS v1.2.0** (CR-2026-02, 19/09/2026). MT-58 đã áp dụng vào **SRS v1.2.1** (CR-2026-03, 21/09/2026).

**Lịch sử rà soát**

| Lượt | Phạm vi | Phát hiện |
| --- | --- | --- |
| 1 | Đối chiếu chéo Chương 3 ↔ 7 ↔ 8 ↔ Phụ lục (hợp đồng API, schema, mã lỗi) | MT-01 → MT-20 |
| 2 | Đối chiếu Chương 1–2 (phạm vi, ràng buộc, phân quyền) ↔ Chương 4–6 (NFR, UI, kiến trúc); rà "yêu cầu mồ côi" — thứ tồn tại ở một nơi nhưng không có dữ liệu/API/FR tương ứng | MT-21 → MT-33 |
| 3 | Rà **nội bộ từng FR** (Mô tả ↔ Điều kiện tiên quyết ↔ Luồng chính ↔ Status code có tự khớp nhau không); máy trạng thái nghiệp vụ; mức ưu tiên MoSCoW; các con số định lượng (độ dài, thời gian, kích thước) | MT-34 → MT-41 |
| 4 | Đối chiếu **SRS v1.1.0 ↔ hệ thống đang chạy**: từng quy định kỹ thuật được kiểm tra với code, `docker-compose.yml`, `nginx.conf`, image Docker thực tế và các sự cố đã ghi trong báo cáo Buổi 2 | MT-42 → MT-57 |
| 5 | Đối chiếu **yêu cầu mới của giảng viên cho Buổi 2** (dữ liệu mẫu) ↔ SRS v1.2.0 §2.6.1 ↔ dữ liệu đang có trong database | MT-58 |

> Lượt 3 cho ra ít mục hơn nhưng **có mục nặng nhất toàn tài liệu là MT-34** — một lỗi rò rỉ dữ liệu giữa các tài khoản, sinh ra từ việc hai câu nằm cách nhau 5 dòng trong **cùng một FR** mà không ai đối chiếu.

> Lượt 4 cho thấy một loại lỗi mà ba lượt đầu không thể phát hiện: **7/16 mục là lỗi do chính bản sửa v1.1.0 đưa vào** — đặc tả nghe hợp lý khi đọc trên giấy nhưng hỏng khi chạy thật, nặng nhất là **MT-50** (cấu hình Nginx tái tạo nguyên vẹn lỗi 502 mà Buổi 2 đã mất công tìm ra và sửa).

> **Lưu ý:** MT-01 → MT-41 đã tồn tại trong file PDF gốc (quá trình chuyển sang Markdown giữ nguyên 100% nội dung). MT-42 → MT-57 được phát hiện trên SRS v1.1.0. Mọi thay đổi đều đi qua quy trình Change Request (CR) như mục "Phê duyệt tài liệu" của SRS yêu cầu.

---

## 1. Quy ước mức độ nghiêm trọng

| Mức | Ý nghĩa | Hành động |
| --- | --- | --- |
| 🔴 **Cao** | Gây sai lệch kiến trúc/dữ liệu; nếu code theo 2 chỗ khác nhau sẽ phải viết lại | Chốt trước khi code |
| 🟡 **Trung bình** | Gây lệch hợp đồng API giữa BE và FE; sửa được nhưng tốn công đồng bộ | Chốt trước khi làm module liên quan |
| 🟢 **Thấp** | Sai sót biên tập, không ảnh hưởng code | Sửa khi cập nhật tài liệu |

---

## 2. Bảng tổng hợp

| ID | Chủ đề | Mức | Khuyến nghị tóm tắt |
| --- | --- | --- | --- |
| MT-01 | Tham số sắp xếp: `sort=-field` vs `sortBy`+`sortOrder` | 🟡 | Chọn `sortBy` + `sortOrder` |
| MT-02 | Nutrition tạo kèm recipe vs tạo riêng | 🟡 | Tạo kèm + cho phép PUT cập nhật, không tách endpoint |
| MT-03 | `StepNumber` do client gửi vs server tự gán | 🟡 | Server tự gán + tự renumber |
| MT-04 | `Quantity` kiểu số vs kiểu chuỗi | 🟡 | Tách 2 cột: `Quantity decimal?` + `QuantityText varchar` |
| MT-05 | Recipe: hard delete vs soft delete | 🔴 | Soft delete toàn hệ thống |
| MT-06 | Điều kiện Publish: chỉ cần step vs cần cả ingredient | 🟡 | Yêu cầu ≥1 step **và** ≥1 ingredient |
| MT-07 | Đếm sai tổng số FR: ghi 27, thực tế 34 | 🟢 | Sửa thành 34 |
| MT-08 | Validation trả 422 vs 400 | 🔴 | Dùng 400 cho validation, 409 cho concurrency |
| MT-09 | Concurrency conflict: 409 vs 422 | 🔴 | Gộp vào MT-08 → dùng 409 |
| MT-10 | `/auth/register` có trả token hay không (auto-login) | 🔴 | Có trả token (auto-login) |
| MT-11 | Google login: `ExternalLoginInfo` vs `idToken` | 🔴 | Dùng `{ idToken }` |
| MT-12 | Tên trường user: `fullName`+`userName` vs `displayName` | 🔴 | Dùng `displayName`, bỏ `fullName` |
| MT-13 | Refresh token 512-bit vs 128-bit | 🟡 | 256-bit (32 bytes) |
| MT-14 | `IsRevoked` không tồn tại trong schema `RefreshToken` | 🔴 | Bỏ `IsRevoked`, dùng `RevokedAt IS NULL` |
| MT-15 | Lưu raw token vs lưu hash (`ReplacedByToken`) | 🔴 | Chỉ lưu hash |
| MT-16 | Cache: `IMemoryCache` vs Redis bắt buộc | 🔴 | Redis cho mọi shared cache |
| MT-17 | TTL cache lệch nhau giữa Chương 3 và NFR-PERF-003 | 🟡 | Lấy NFR-PERF-003 làm chuẩn |
| MT-18 | SDK MinIO: MinIO .NET SDK vs `AWSSDK.S3` | 🟡 | `AWSSDK.S3` |
| MT-19 | API ảnh: `/images/{id}/primary` vs PATCH metadata | 🟡 | PATCH metadata gộp |
| MT-20 | Các bất nhất nhỏ đợt 1 (phiên bản, trình duyệt, email, đặt tên) | 🟢 | Sửa biên tập |
| MT-21 | Policy `"VerifiedAuthor"` đòi email đã xác nhận nhưng không có luồng xác nhận email | 🔴 | Bỏ policy khỏi v1.0 |
| MT-22 | `IsActive` + `AUTH_ACCOUNT_DISABLED` tồn tại nhưng không có FR khóa tài khoản | 🟡 | Thêm FR-AUTH-008 + check khi login |
| MT-23 | Trang chủ có "recipe nổi bật" nhưng không có trường/API nào | 🟡 | Dùng `sortBy=publishedAt` thay cho "nổi bật" |
| MT-24 | Hứa Rich Snippets "star rating" trong khi Rating System ngoài phạm vi | 🟡 | Bỏ "star rating" khỏi NFR-SEO-001 |
| MT-25 | FTS: "computed column" vs "trigger"; config `"vietnamese"` không tồn tại; trigger vs CONS-006 | 🔴 | Dùng GENERATED column + config `simple` + unaccent |
| MT-26 | Thiếu index cho chính các cột dùng để sort/filter | 🟡 | Bổ sung index `CreatedAt`, `CookTime`, `PrepTime` |
| MT-27 | Yêu cầu 301 redirect slug cũ nhưng không có chỗ lưu slug cũ | 🟡 | Khóa slug sau publish, bỏ yêu cầu 301 |
| MT-28 | `sitemap.xml` sinh ở BE/MinIO nhưng site chạy ở Next.js | 🟡 | Next.js route `sitemap.xml` đọc từ API |
| MT-29 | Kubernetes xuất hiện 3 chỗ dù hạ tầng chỉ có Docker Compose | 🟢 | Bỏ hoặc ghi rõ "định hướng tương lai" |
| MT-30 | Domain layer được dùng FluentValidation hay không (3 chỗ nói 3 kiểu) | 🟡 | Domain không có NuGet nào |
| MT-31 | `PerformanceBehavior` / `CacheInvalidationBehavior` lệch giữa mục 6.2 và 6.3 | 🟢 | Thống nhất 5 behavior |
| MT-32 | Compose map cố định `5000:8080` nhưng NFR đòi nhiều API instance | 🟡 | Bỏ port mapping, để Nginx vào network nội bộ |
| MT-33 | Các bất nhất nhỏ đợt 2 (đặt tên interface, ISR vs TTL, route trùng…) | 🟢 | Sửa biên tập |
| MT-34 | **Output Cache không phân biệt danh tính → Guest đọc được recipe Draft** | 🔴 | Không cache endpoint phụ thuộc danh tính |
| MT-35 | Máy trạng thái `RecipeStatus` không đóng kín; `PublishedAt` không được set | 🔴 | Vẽ lại máy trạng thái, bổ sung unarchive |
| MT-36 | FR-CAT-003 khai báo 409 "Name đã tồn tại" nhưng luồng không bao giờ sinh ra 409 | 🟡 | Thêm bước kiểm tra Name trùng |
| MT-37 | Giới hạn độ dài lệch giữa validator ↔ schema ↔ yêu cầu SEO | 🟡 | Lập bảng độ dài chuẩn duy nhất |
| MT-38 | Rate limit theo IP sau Nginx mà không cấu hình `ForwardedHeaders` | 🟡 | Bật `UseForwardedHeaders` |
| MT-39 | Hangfire Dashboard chỉ Admin, nhưng JWT Bearer không dùng được từ browser | 🟡 | Dùng filter riêng cho dashboard |
| MT-40 | 11 FR không có mức ưu tiên; nhiều FR mức S lại đỡ cho thành phần mức M | 🟡 | Gán MoSCoW đủ, nâng 3 FR lên M |
| MT-41 | Các bất nhất nhỏ đợt 3 (mô tả sai cơ chế, thuật ngữ rác…) | 🟢 | Sửa biên tập |
| MT-42 | Hai hình dạng response danh sách: `{ data, meta }` vs `PagedResult` | 🟡 | Chốt `PagedResult` |
| MT-43 | Ba FR chỉ nhận URL ảnh nhưng không có endpoint upload | 🟡 | Chính thức hóa `/files/upload` |
| MT-44 | `/dashboard/users` không có API danh sách người dùng | 🟡 | Thêm `GET /users` vào FR-AUTH-008 |
| MT-45 | Quản lý phiên có trong lộ trình nhưng không có FR | 🟢 | Thêm FR-AUTH-009 (mức C) |
| MT-46 | 3 mã lỗi đang dùng nhưng thiếu trong Phụ lục B | 🟢 | Bổ sung (24 → 27 mã) |
| MT-47 | Hangfire "in-process" vs container worker riêng | 🟡 | Chuẩn hóa worker riêng |
| MT-48 | Sitemap cần toàn bộ slug, API danh sách giới hạn 50 | 🟢 | Thêm `GET /recipes/sitemap` |
| MT-49 | "Có thể khôi phục" nhưng không có chức năng khôi phục | 🟡 | Khôi phục do vận hành; tự phục vụ ngoài phạm vi |
| MT-50 | **Cấu hình Nginx `upstream` tái tạo lỗi 502 đã sửa ở Buổi 2** | 🔴 | Resolver DNS động |
| MT-51 | Lệnh healthcheck không chạy được trong image thật | 🟡 | Dùng lệnh đã kiểm chứng |
| MT-52 | Admin tự khóa: 403 (FR) vs 409 (Chương 8) | 🟡 | Thống nhất 403 |
| MT-53 | Giải thích sai cơ chế route; slug dành riêng mỗi nơi một kiểu | 🟢 | Một danh sách duy nhất |
| MT-54 | `KnownProxies` không nhận dải mạng; `KnownNetworks` obsolete | 🟢 | Dùng `KnownIPNetworks` |
| MT-55 | Không quy định nơi lưu token phía Frontend | 🟡 | Access token chỉ trong bộ nhớ |
| MT-56 | `DEFERRABLE` không nói phải là constraint | 🟢 | Ghi rõ UNIQUE CONSTRAINT |
| MT-57 | `/categories/[slug]` ghi ISR nhưng không thể dựng tĩnh | 🟢 | SSR + Data Cache |
| MT-58 | Yêu cầu dữ liệu mẫu của giảng viên (≥ 20 danh mục, ≥ 100 công thức, ≥ 10 nguyên liệu, ≥ 5 bước) khác SRS §2.6.1 (50 công thức) | 🟡 | Cập nhật SRS v1.2.1 (CR-2026-03); nội dung món ăn thật, seeder tự bù |

---

## 3. Chi tiết từng mâu thuẫn

### MT-01 🟡 — Hai cách truyền tham số sắp xếp *(nhóm phát hiện)*

**Hiện trạng**

| Vị trí | Quy ước |
| --- | --- |
| FR-RCP-001, FR-SRCH-003 (dòng 747, 757, 1122) | 1 tham số: `sort=title` (ASC), `sort=-title` (DESC) |
| Chương 8 – Quy ước Pagination, `/recipes`, `/categories/{slug}` (dòng 1517, 1537, 1546) | 2 tham số: `sortBy=createdAt&sortOrder=desc` |

**Vì sao là vấn đề:** BE và FE đọc hai chương khác nhau sẽ implement hai kiểu; request từ FE sẽ bị bỏ qua âm thầm (không lỗi, chỉ sai thứ tự) — loại bug rất khó phát hiện khi test thủ công.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`sortBy` + `sortOrder`** | Dễ đọc, dễ validate bằng FluentValidation (2 rule độc lập: `sortBy ∈ whitelist`, `sortOrder ∈ {asc,desc}`); dễ bind vào record C#; FE dựng dropdown "cột" + "chiều" ánh xạ 1-1; hợp thông lệ Swagger/OpenAPI | Dài hơn; muốn sắp xếp nhiều cột phải dùng mảng |
| B | **`sort=-field`** | Ngắn, đúng chuẩn JSON:API; mở rộng nhiều cột dễ (`sort=-createdAt,title`) | Phải tự parse tiền tố `-`; dấu `-` dễ bị nuốt khi quên URL-encode; validate phức tạp hơn; SV mới dễ nhầm |
| C | Hỗ trợ cả hai | Không phải sửa tài liệu | Hai đường code cho một việc, tăng bug và test case; vi phạm nguyên tắc "một cách làm đúng" |

**➡️ Khuyến nghị: Phương án A.** Đây là đồ án môn học, ưu tiên rõ ràng và dễ validate hơn là ngắn gọn. Sửa FR-RCP-001, FR-SRCH-003 theo Chương 8. Whitelist `sortBy ∈ { createdAt, title, cookTime, prepTime }`, mặc định `sortBy=createdAt&sortOrder=desc`; giá trị ngoài whitelist → 400 (không im lặng bỏ qua, tránh SQL injection qua tên cột).

---

### MT-02 🟡 — Thời điểm tạo thông tin dinh dưỡng *(nhóm phát hiện)*

**Hiện trạng**

- FR-RCP-003 (dòng 815, 823) và Chương 8 `POST /recipes`: `nutrition?` nằm trong body tạo recipe, gọi `recipe.SetNutrition(...)`.
- FR-RCP-003 phần Mô tả (dòng 807): "Steps và Ingredients có thể được tạo cùng lúc **hoặc thêm riêng lẻ sau** qua FR-RCP-009/010" — nutrition bị bỏ lửng, không có FR nào đặc tả endpoint riêng cho nutrition, cũng không có endpoint `/recipes/{id}/nutrition` trong Chương 8.

**Vì sao là vấn đề:** Nutrition là **Owned Entity** (Chương 7.2.1 — các cột `Nutrition_*` nằm ngay trong bảng `Recipes`), nên nó không có vòng đời độc lập như Step/Ingredient. Nếu team hiểu nhầm và làm endpoint riêng thì sẽ tạo repository/DTO thừa cho một thứ không phải entity độc lập.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Chỉ qua POST/PUT `/recipes`** (nutrition là một phần của recipe) | Đúng bản chất Owned Entity; ít endpoint; 1 transaction; khớp Chương 7 | Muốn sửa riêng calories vẫn phải gửi cả object recipe |
| B | Thêm endpoint `PUT /recipes/{id}/nutrition` | Sửa riêng phần dinh dưỡng gọn; hợp form nhiều bước ở FE | Mâu thuẫn với mô hình Owned Entity; phải thêm command/validator/DTO; không có FR nào đặc tả |
| C | Cả hai | Linh hoạt nhất | Trùng lặp logic, hai chỗ cùng ghi một nhóm cột |

**➡️ Khuyến nghị: Phương án A.** Nutrition đi kèm `POST /recipes` (tùy chọn) và cập nhật qua `PUT /recipes/{id}`. Bổ sung một câu vào FR-RCP-003 nói rõ: *"Nutrition là Owned Entity, chỉ tạo/sửa cùng recipe, không có endpoint riêng."* Nếu FE dùng wizard nhiều bước thì gom state ở client rồi submit một lần — không cần API riêng.

---

### MT-03 🟡 — `StepNumber` do client gửi hay server tự gán *(nhóm phát hiện)*

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-RCP-010 (dòng 1057, 1065–1066) | Server tự gán: `StepNumber = Max(StepNumber) + 1`; body chỉ có `{ description, durationMinutes?, imageUrl? }`; xóa bước → tự renumber |
| Chương 8 `POST /recipes/{id}/steps` (dòng 1568–1569) | Client gửi `stepNumber` trong body, thậm chí sửa được qua PUT |
| Chương 7.3 (dòng 1439) | `StepNumber NOT NULL, CHECK > 0`, UNIQUE cùng `RecipeId` |

**Vì sao là vấn đề:** Nếu client tự gửi, ràng buộc UNIQUE `(RecipeId, StepNumber)` sẽ bị vi phạm ngay khi hai request thêm bước gần như đồng thời, hoặc khi người dùng nhập trùng số → lỗi 500 từ DB thay vì lỗi nghiệp vụ rõ ràng. Ngoài ra bắt người dùng nhớ "mình đang ở bước mấy" là UX kém.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Server tự gán + tự renumber khi xóa** | UX đơn giản (người dùng chỉ gõ nội dung); không bao giờ vỡ UNIQUE; đúng tinh thần FR-RCP-010 | Không chèn được bước vào giữa nếu chỉ có POST |
| B | Client gửi `stepNumber` | Chèn/sắp xếp tùy ý | Dồn trách nhiệm toàn vẹn dữ liệu cho client; lỗi UNIQUE; phải validate liên tục |
| C | Server tự gán khi thêm + thêm endpoint `PATCH /recipes/{id}/steps/reorder` nhận mảng id theo thứ tự mới | Vừa đơn giản khi thêm, vừa kéo-thả sắp xếp được ở FE; server vẫn giữ quyền renumber | Thêm 1 endpoint + 1 command |

**➡️ Khuyến nghị: Phương án C** (A cho bản tối thiểu, nâng lên C nếu còn thời gian). Bỏ `stepNumber` khỏi body POST/PUT ở Chương 8. Khi renumber, cập nhật trong **một** `SaveChangesAsync()` để UNIQUE không bị vi phạm giữa chừng — hoặc khai báo constraint là `DEFERRABLE INITIALLY DEFERRED` trong PostgreSQL.

---

### MT-04 🟡 — Kiểu dữ liệu `Quantity` của nguyên liệu *(nhóm phát hiện)*

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| Chương 7.4 (dòng 1452) | `Quantity decimal(10,3) NULL` — "Nullable cho nguyên liệu vừa đủ" |
| FR-RCP-009 – Điều kiện tiên quyết (dòng 1018) | "**Quantity > 0**, Unit không rỗng" → bắt buộc, phải dương |
| Chương 8 `POST /ingredients` (dòng 1576) | `quantity?`, `unit?` → cả hai đều tùy chọn |

Đây thực chất là **hai mâu thuẫn chồng nhau**: (a) bắt buộc hay tùy chọn, (b) kiểu số có biểu diễn được "1/2 muỗng", "một nhúm" hay không.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`decimal(10,3)` nullable** | Tính toán được (scale khẩu phần ×2, cộng dồn nguyên liệu, map `recipeIngredient` cho JSON-LD); sắp xếp/lọc được | Không lưu được "1/2", "vừa đủ", "1-2 quả" — FE phải tự quy đổi 1/2 → 0.5, mất nguyên văn người dùng nhập |
| B | `varchar(50)` | Nhập tự do đúng thói quen nấu ăn tiếng Việt ("nửa gói", "1/2 muỗng") | Mất hoàn toàn khả năng tính toán/scale khẩu phần; dữ liệu bẩn, không validate được; khó chuẩn hóa cho SEO |
| C | **Hai cột: `Quantity decimal(10,3) NULL` + `QuantityText varchar(50) NULL`** | Giữ được cả nguyên văn lẫn giá trị tính toán; FE hiển thị `QuantityText` nếu có, ngược lại format từ `Quantity`; scale khẩu phần vẫn chạy khi có số | Thêm 1 cột; cần quy tắc rõ khi cả hai cùng có giá trị |
| D | Lưu `decimal` + parse phân số ở BE ("1/2" → 0.5) | Một cột duy nhất, vẫn nhập được phân số | Vẫn mất nguyên văn; phải viết parser và test nhiều case; không xử lý được "vừa đủ" |

**➡️ Khuyến nghị: Phương án C**, và thống nhất **cả hai đều nullable** (theo Chương 7.4 và Chương 8), bỏ ràng buộc "Quantity > 0" ở FR-RCP-009 — thay bằng: *"Nếu có `Quantity` thì phải > 0"*. Quy tắc hiển thị: ưu tiên `QuantityText`; validate: không được để cả `Quantity`, `QuantityText`, `Unit` cùng rỗng.

> Nếu muốn giữ phạm vi đúng SRS gốc (không thêm cột), chọn **Phương án A** và chấp nhận FE tự quy đổi — nhưng phải sửa FR-RCP-009 cho khớp ("Quantity tùy chọn").

---

### MT-05 🔴 — Recipe: hard delete hay soft delete

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-RCP-007 – Mô tả (dòng 937) | "Xóa **vĩnh viễn**... Đây là **hard delete** (không dùng soft delete pattern cho recipe)"; cascade delete Steps/Ingredients/Images; xóa file MinIO qua Hangfire |
| Chương 7 mở đầu + 7.1 + 7.2 (dòng 1379, 1390, 1415) | Mọi entity kế thừa `BaseEntity` có `IsDeleted`, Global Query Filter `.Where(x => !x.IsDeleted)`; Recipe có index `IDX_Recipe_IsDeleted` |
| NFR-REL-003 (dòng 1211) | "Soft delete: Recipe được đánh dấu `IsDeleted` thay vì xóa vật lý (**có thể khôi phục**)" |
| Chương 8 `DELETE /recipes/{id}` (dòng 1554) | "Xóa recipe (**soft delete**)" |
| Phụ lục A – 404 (dòng 1602) | "Resource không tồn tại **hoặc đã soft-delete**" |

Tỷ lệ 4 chỗ soft delete / 1 chỗ hard delete. Đây là mâu thuẫn nặng nhất vì kéo theo hành vi của cascade delete, xóa file MinIO, và tính duy nhất của Slug.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Soft delete toàn bộ** | Khôi phục được (đúng NFR-REL-003); an toàn cho đồ án demo; đúng thiết kế `BaseEntity`; không cần cascade xóa vật lý | Slug đã dùng vẫn chiếm chỗ (phải thêm `IsDeleted` vào unique index); ảnh MinIO còn rác nếu không dọn; mọi query phải nhớ Global Query Filter |
| B | Hard delete cho riêng Recipe | Sạch DB và MinIO ngay; giải phóng Slug | Phá vỡ `BaseEntity` (Recipe có `IsDeleted` mà không dùng); mất dữ liệu không khôi phục; mâu thuẫn 4 mục khác phải sửa hết |
| C | Soft delete + Hangfire recurring job dọn vĩnh viễn sau N ngày (vd 30) | Có "thùng rác" như sản phẩm thật; vẫn giải phóng dung lượng; MinIO được dọn đúng lúc | Thêm 1 recurring job + FR mới; phức tạp hơn phạm vi môn học |

**➡️ Khuyến nghị: Phương án A** (C là phương án mở rộng nếu dư thời gian). Sửa FR-RCP-007 thành soft delete. Hệ quả cần ghi rõ trong SRS:
- Unique index Slug đổi thành **partial unique**: `CREATE UNIQUE INDEX ... ON "Recipes"("Slug") WHERE "IsDeleted" = false`.
- Child entity (Steps/Ingredients/Images) **không** xóa theo — chúng vô hình cùng recipe qua query filter; giữ FK `ON DELETE CASCADE` chỉ để phòng khi hard delete thật sự.
- Job xóa file MinIO **không** chạy khi soft delete (nếu không, khôi phục recipe sẽ mất ảnh). Chuyển job này sang bước dọn vĩnh viễn.

---

### MT-06 🟡 — Điều kiện để Publish một công thức

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-RCP-005 (dòng 878, 884, 890) | "KHÔNG thể publish nếu recipe không có ít nhất **1 bước thực hiện** (`Steps.Count > 0`)" |
| Phụ lục B – `RECIPE_PUBLISH_INCOMPLETE` (dòng 1626) | "phải có ít nhất **1 ingredient và 1 step**" |
| Phụ lục A – 400 (dòng 1600) | ví dụ: "publish recipe **thiếu ingredients**" |

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Yêu cầu ≥1 step và ≥1 ingredient** | Một công thức nấu ăn không có nguyên liệu là vô nghĩa; JSON-LD Schema.org Recipe (NFR-SEO-001) yêu cầu `recipeIngredient[]` — thiếu sẽ trượt Rich Results Test | Chặt hơn, Author phải nhập đủ mới publish được |
| B | Chỉ yêu cầu ≥1 step | Dễ publish nhanh | Sinh ra recipe rỗng nguyên liệu, hỏng SEO structured data — vi phạm chính NFR-SEO-001 |
| C | Thêm điều kiện ảnh (≥1 image) nữa | `og:image` (NFR-SEO-002) luôn có ảnh | Quá chặt cho đồ án; Author phải upload mới publish được |

**➡️ Khuyến nghị: Phương án A.** Sửa FR-RCP-005 thành `Steps.Count > 0 && Ingredients.Count > 0`. Đồng thời đổi mã lỗi trả về cho khớp: dùng `RECIPE_PUBLISH_INCOMPLETE` với HTTP **400** (xem MT-08), thay vì 422 như FR-RCP-005 đang ghi.

---

### MT-07 🟢 — Tổng số FR ghi sai (27 vs 34)

**Hiện trạng:** Tài liệu lặp lại "27 Functional Requirements" ở 3 nơi (mục 1.5 dòng 185, mục 2.2 dòng 241, đầu Chương 3 dòng 344). Nhưng chính bảng ở mục 2.2 liệt kê: AUTH 7 + CAT 5 + RCP 10 + SRCH 4 + FILE 2 + JOB 3 + OBS 3 = **34**. Đếm số FR thực tế đặc tả trong Chương 3 cũng ra 34.

**Phương án:** (A) Sửa "27" → "34" ở cả 3 chỗ. (B) Giảm phạm vi xuống còn 27 FR.

**➡️ Khuyến nghị: Phương án A** — chỉ là lỗi cộng số, không đụng phạm vi. Ghi chú thêm: FR-SRCH-002/003/004 không có mục đặc tả riêng mà được gộp trong bảng tóm tắt, nên nếu tính "số FR có đặc tả đầy đủ" thì là 31 — cần nói rõ cách đếm để tránh hiểu nhầm.

---

### MT-08 🔴 — Validation lỗi trả 422 hay 400

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| Toàn bộ Chương 3 (dòng 367, 387, 388, 404, 563, 653, 749, 811, 1096...) | Validation fail → **422 Unprocessable Entity** |
| Chương 8 (dòng 1524, 1538, 1560, 1568...) | Validation fail → **400 Bad Request** |
| Phụ lục A (dòng 1600, 1604) | 400 = "Validation lỗi (FluentValidation)"; 422 = **chỉ dành cho** RowVersion conflict |
| Phụ lục B (dòng 1633) | `VALIDATION_ERROR` = **400** |

Đây là mâu thuẫn xuyên suốt hai chương, ảnh hưởng gần như mọi endpoint và mọi test case.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Dùng 400 cho mọi validation** | Khớp Chương 8 + cả hai phụ lục (3/4 tài liệu); là mặc định của ASP.NET Core `ValidationProblemDetails`; FE chỉ cần bắt 1 mã | 400 gộp chung lỗi cú pháp JSON hỏng và lỗi nghiệp vụ, kém tinh tế về mặt học thuật |
| B | Dùng 422 cho validation, 400 cho body malformed | Phân biệt rõ "JSON hỏng" vs "JSON đúng nhưng dữ liệu sai"; đúng tinh thần RFC 4918 | Phải override hành vi mặc định của ASP.NET Core; phải sửa Chương 8 + 2 phụ lục; FE bắt 2 mã |
| C | Giữ nguyên (mỗi chương một kiểu) | Không phải làm gì | Không chấp nhận được — test và FE sẽ sai chắc chắn |

**➡️ Khuyến nghị: Phương án A.** Sửa toàn bộ Chương 3 từ 422 → 400. Giữ 422 **không dùng nữa** (xem MT-09) hoặc chỉ dành riêng cho một trường hợp duy nhất nếu nhóm muốn giữ. Lý do: khớp với đa số tài liệu và với hành vi mặc định của framework, giảm code tùy biến.

---

### MT-09 🔴 — Xung đột đồng thời (Optimistic Concurrency) trả 409 hay 422

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-RCP-004 (dòng 843, 847, 861) | RowVersion mismatch → **409 Conflict** |
| Phụ lục A – 422 (dòng 1604) | 422 = "RowVersion conflict — Optimistic Concurrency" |
| Phụ lục B – `RECIPE_CONCURRENCY_CONFLICT` (dòng 1627) | **422** |

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **409 Conflict** | Đúng ngữ nghĩa HTTP: "yêu cầu xung đột với trạng thái hiện tại của tài nguyên" — chính xác là tình huống ETag/If-Match; thông lệ phổ biến của REST + ETag | Trùng mã với lỗi trùng email/slug → FE phải đọc `type` để phân biệt (nhưng đó chính là công dụng của Application Error Code) |
| B | 422 | Tách bạch với lỗi trùng unique | Sai ngữ nghĩa: 422 là "cú pháp đúng, ngữ nghĩa không xử lý được", không phải "xung đột trạng thái"; ngược với chuẩn dùng ETag |

**➡️ Khuyến nghị: Phương án A (409).** Sửa Phụ lục A và B. FE phân biệt bằng trường `type` trong RFC 7807 — đúng mục đích mà chính Phụ lục B đặt ra ("để frontend xử lý lỗi theo programmatic way"). Kết hợp MT-08: **400 = validation, 409 = mọi xung đột trạng thái, bỏ hẳn 422.**

---

### MT-10 🔴 — `POST /auth/register` có trả token hay không

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-AUTH-001 (dòng 365, 368, 382) | "auto-login sau đăng ký"; trả `AuthResponseDto` gồm `accessToken`, `refreshToken`, `expiresAt`, `user{}`; "refresh token được persist" |
| Chương 8 (dòng 1524) | 201 trả `{ userId, email, displayName }` — **không có token** |

**Vì sao là vấn đề:** Quyết định này thay đổi hẳn luồng FE: có token thì vào thẳng `/dashboard`, không có token thì phải đẩy người dùng sang `/auth/login`. Làm sai một bên là phải viết lại màn hình đăng ký.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Trả token (auto-login)** | UX tốt, bớt một bước; khớp FR-AUTH-001 vốn chi tiết hơn (12 bước có mô tả rõ persist refresh token) | Nếu sau này bắt buộc xác thực email thì phải sửa lại; token cấp cho email chưa verify |
| B | Không trả token | An toàn hơn khi có luồng xác thực email; đơn giản hóa endpoint | Thêm một bước đăng nhập ngay sau đăng ký; mâu thuẫn với chính mô tả "auto-login" |

**➡️ Khuyến nghị: Phương án A.** SRS đã loại xác thực email khỏi phạm vi bắt buộc (`emailConfirmed` chỉ là field hiển thị, policy `"VerifiedAuthor"` là tùy chọn), nên auto-login không tạo rủi ro. Sửa bảng Chương 8 thành `201: AuthResponseDto`.

---

### MT-11 🔴 — Luồng Google OAuth: `ExternalLoginInfo` hay `idToken`

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-AUTH-003 (dòng 435, 443–447) | Authorization Code Flow + PKCE; Auth.js v5 ở Next.js xử lý callback; FE gửi `ExternalLoginInfo` lên BE |
| Chương 5.3 (dòng 1287) | Redirect URI là `/api/v1/auth/google/callback` → tức là **BE** nhận callback, không phải Next.js |
| Chương 8 (dòng 1526) | Body là `{ idToken }` từ **Google Sign-In JS SDK** (đây là Implicit/ID-token flow, không phải Authorization Code) |

Ba chỗ mô tả ba kiến trúc khác nhau. `ExternalLoginInfo` còn là kiểu nội bộ của ASP.NET Core Identity — không serialize qua HTTP được, nên đặc tả đó không khả thi.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **FE lấy `idToken`, BE verify bằng `Google.Apis.Auth`** | Đơn giản nhất; BE stateless, chỉ cần `GoogleClientId`; không cần redirect URI ở BE; khớp Chương 8 | Không có refresh token của Google (không cần, vì ta tự cấp JWT); phụ thuộc Google JS SDK ở FE |
| B | Auth.js v5 ở Next.js + BE tin tưởng FE | Tận dụng Auth.js có sẵn | BE phải tin dữ liệu FE gửi lên → **lỗ hổng bảo mật nghiêm trọng** nếu không verify lại chữ ký |
| C | BE tự chạy Authorization Code + PKCE, nhận callback ở `/api/v1/auth/google/callback` | Chuẩn mực nhất, BE kiểm soát toàn bộ | Phức tạp nhất: phải quản lý state/PKCE verifier, redirect qua lại giữa BE và FE; nặng cho đồ án |

**➡️ Khuyến nghị: Phương án A.** Chốt body `{ idToken }`, BE verify bằng `GoogleJsonWebSignature.ValidateAsync` (kiểm tra `aud == ClientId`, `iss`, `exp`), rồi mới `FindByLoginAsync` / `AddLoginAsync`. Sửa FR-AUTH-003 (bỏ `ExternalLoginInfo`, bỏ PKCE) và Chương 5.3 (bỏ redirect URI phía BE).

---

### MT-12 🔴 — Tên trường hồ sơ người dùng: `fullName` + `userName` hay `displayName`

**Hiện trạng**

| Vị trí | Trường dùng |
| --- | --- |
| FR-AUTH-001/006/007 (dòng 371, 382, 543, 559, 567) | `fullName`, `userName` |
| Chương 7.7 (dòng 1487) | Cột custom là **`DisplayName`** — "Tên hiển thị công khai (không phải username)"; thêm `Bio`, `IsActive`; **không có cột `FullName`** |
| Chương 8 (dòng 1524, 1529, 1530) | `displayName`, `bio` — **không có** `fullName`, `userName` |

Nghĩa là FR-AUTH đang mô tả việc ghi vào một cột không tồn tại trong schema, và bỏ sót `bio` mà cả schema lẫn API đều có.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Dùng `displayName`, bỏ `fullName`** | Khớp schema (Chương 7) và API (Chương 8) — 2/3 tài liệu; `UserName` vẫn tồn tại vì kế thừa IdentityUser, chỉ không cho người dùng nhập khi đăng ký (sinh tự động từ email) | Phải sửa 6 chỗ trong FR-AUTH |
| B | Thêm cột `FullName` vào schema, giữ cả hai | Không phải sửa FR-AUTH | Hai trường gần như trùng nghĩa → người dùng bối rối, dữ liệu dư; phải sửa Chương 7 + 8 |
| C | Đổi `DisplayName` → `FullName` trong schema | Giữ được FR-AUTH | Phải sửa Chương 7 + toàn bộ Chương 8; `bio` vẫn lạc lõng |

**➡️ Khuyến nghị: Phương án A.** Chốt hợp đồng:
- `POST /auth/register`: `{ email, password, displayName }` — `UserName` do BE sinh từ phần trước `@` của email, thêm hậu tố số nếu trùng.
- `GET/PATCH /auth/me`: `{ id, email, displayName, avatarUrl, bio, roles }`.
- Bổ sung `bio` vào FR-AUTH-006/007 (hiện đang thiếu).

---

### MT-13 🟡 — Độ dài Refresh Token: 512-bit hay 128-bit

**Hiện trạng:** FR-AUTH-001 (dòng 379) ghi "refresh token ngẫu nhiên (**512-bit**, 7 ngày)"; NFR-SEC-002 (dòng 1189) ghi "**128-bit** cryptographically secure random bytes".

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | 128-bit (16 bytes) | Đủ an toàn chống brute-force; chuỗi Base64URL ngắn (~22 ký tự) | Biên an toàn thấp nhất trong ba lựa chọn |
| B | **256-bit (32 bytes)** | Khớp đúng độ dài output SHA-256 dùng để hash lưu DB; mức khuyến nghị phổ biến của OWASP; chuỗi ~43 ký tự vẫn gọn | Không khớp con số nào trong SRS hiện tại → phải sửa cả hai chỗ |
| C | 512-bit (64 bytes) | An toàn nhất về lý thuyết | Thừa thãi (entropy > 256-bit không tăng thực chất vì đằng nào cũng hash SHA-256); token dài, tốn băng thông mỗi request refresh |

**➡️ Khuyến nghị: Phương án B (256-bit).** Sinh bằng `RandomNumberGenerator.GetBytes(32)`, encode Base64URL. Sửa cả FR-AUTH-001 lẫn NFR-SEC-002 cho thống nhất. Lưu ý cột `TokenHash varchar(64)` trong Chương 7.8 chính là 64 ký tự hex của SHA-256 — hoàn toàn khớp.

---

### MT-14 🔴 — Trường `IsRevoked` không tồn tại trong schema

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| FR-AUTH-004/005 (dòng 469, 480, 481, 505, 513) | Dùng `IsRevoked = true`, kiểm tra `IsRevoked == false` |
| Chương 7.8 (dòng 1500–1506) | Bảng `RefreshToken` chỉ có `RevokedAt timestamptz NULL` — "**NULL = còn hiệu lực**"; **không có cột `IsRevoked`** |

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bỏ `IsRevoked`, dùng `RevokedAt IS NULL`** | Một nguồn sự thật duy nhất, không thể lệch trạng thái (ví dụ `IsRevoked=true` mà `RevokedAt=null`); ít cột hơn; khớp Chương 7 | Query dài hơn chút: `.Where(t => t.RevokedAt == null)` |
| B | Thêm cột `IsRevoked boolean` | Query ngắn, index bool rẻ | Dữ liệu dư thừa, hai cột phải luôn đồng bộ — nguồn bug kinh điển |
| C | `IsRevoked` là computed property trong C# (`=> RevokedAt != null`), không map DB | Code đọc tự nhiên, DB vẫn sạch | Không dùng được trực tiếp trong LINQ-to-SQL (EF không dịch được property không map) |

**➡️ Khuyến nghị: Phương án A.** Sửa FR-AUTH-004/005 thành `RevokedAt == null` / `RevokedAt = DateTime.UtcNow`. Thêm ràng buộc kiểm tra tính hợp lệ của token: `RevokedAt IS NULL AND ExpiresAt > NOW()`.

---

### MT-15 🔴 — Lưu raw token hay hash

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| FR-AUTH-004 bước 5 (dòng 481) | `ReplacedByToken = newToken` — gán **raw token** |
| Chương 7.8 (dòng 1501, 1504) | `TokenHash` — "SHA-256 hash của raw token. **Không lưu raw token**"; `ReplacedByTokenHash` |
| NFR-SEC-002 (dòng 1189) | "hash SHA-256 trước khi lưu DB" |

**Vì sao là vấn đề:** Nếu lưu raw token, kẻ đọc được DB (SQL injection, backup rò rỉ, log lộ) có thể mạo danh mọi người dùng cho đến khi token hết hạn — đúng rủi ro mà NFR-SEC-002 muốn chặn.

**Phương án:** (A) Chỉ lưu hash, mọi so khớp đều hash trước rồi tra `TokenHash`. (B) Lưu raw cho dễ debug — **không chấp nhận được** trong tài liệu đã cam kết OWASP Top 10.

**➡️ Khuyến nghị: Phương án A.** Sửa FR-AUTH-004 bước 5 thành `ReplacedByTokenHash = SHA256(newToken)`. Raw token chỉ tồn tại trong response HTTP trả về client, không bao giờ ghi DB và không bao giờ đưa vào log (bổ sung rule vào NFR-SEC-007).

---

### MT-16 🔴 — `IMemoryCache` hay Redis

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| Mục 3.2 mở đầu + FR-CAT-001/003 (dòng 580, 591, 601, 604, 649) | Danh mục cache bằng **`IMemoryCache`**, key `"categories:all"` |
| NFR-SCALE-001 (dòng 1226) | "Distributed cache (**Redis, không in-memory `IMemoryCache`**) cho mọi shared state" |
| Chương 6.1 + 6.3 (dòng 1321, 1345) | Cache Layer = Redis; `CachingBehavior` kiểm tra **Redis** cache |

**Vì sao là vấn đề:** Với `IMemoryCache`, khi chạy nhiều instance API sau Nginx (đúng kịch bản NFR-SCALE-003), Admin sửa danh mục trên instance 1 thì instance 2 vẫn phục vụ dữ liệu cũ tới 60 phút — và không có cách nào invalidate.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Redis cho mọi cache** | Invalidate đúng trên mọi instance; khớp NFR-SCALE-001 và Chương 6; một cơ chế duy nhất | Mỗi lần đọc tốn một round-trip mạng (~1ms); phụ thuộc Redis (đã có fallback ở NFR-REL-002) |
| B | `IMemoryCache` cho danh mục (dữ liệu ít đổi), Redis cho phần còn lại | Nhanh nhất cho danh mục | Vẫn stale khi scale; hai cơ chế cache trong một hệ thống; mâu thuẫn NFR vẫn còn |
| C | Hai tầng: `IMemoryCache` (TTL ngắn 30–60s) làm L1 + Redis làm L2 | Rất nhanh, vẫn khá nhất quán | Phức tạp nhất; vẫn có cửa sổ stale; quá mức cần thiết cho ≤ 50 danh mục |

**➡️ Khuyến nghị: Phương án A.** Sửa mục 3.2 và FR-CAT-001/003/004/005 từ `IMemoryCache` → Redis (`IDistributedCache` hoặc `RedisCacheService` như Chương 6.2 đã định nghĩa). Danh mục chỉ ≤ 50 bản ghi nên chi phí round-trip không đáng kể.

---

### MT-17 🟡 — Giá trị TTL cache không khớp nhau

**Hiện trạng**

| Đối tượng | Chương 3 | NFR-PERF-003 |
| --- | --- | --- |
| Danh sách danh mục | 60 phút (dòng 580, 591, 604) — riêng dòng 580 ghi "1 giờ" | **30 phút** (dòng 1178) |
| Chi tiết recipe | 60 phút, Output Cache (dòng 777) | **5 phút**, cache-aside |
| Danh sách recipe | 15 phút, Output Cache (dòng 745) | không nhắc |
| Kết quả tìm kiếm | không cache hoặc 5 phút (dòng 1108) | **1 phút** |

Ngoài lệch số, còn lệch **cơ chế**: Chương 3 dùng Output Cache (.NET 10) tag-based, NFR-PERF-003 dùng cache-aside qua Redis — hai kiến trúc cache khác nhau cho cùng một endpoint.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Lấy NFR-PERF-003 làm chuẩn, cache-aside qua Redis** | NFR là nơi quy định chỉ tiêu chất lượng — đúng vai trò; TTL ngắn hơn nên dữ liệu tươi hơn; một cơ chế duy nhất, khớp MT-16 và `CachingBehavior` ở Chương 6.3 | TTL ngắn → hit rate thấp hơn, khó đạt mốc ≥ 80% của chính NFR-PERF-003 |
| B | Lấy Chương 3 làm chuẩn, dùng Output Cache | TTL dài, hit rate cao; tag-based invalidation rất gọn (`EvictByTagAsync("recipes")`) | Output Cache mặc định lưu in-memory → stale khi multi-instance (lặp lại lỗi MT-16), trừ khi cấu hình Redis output cache store |
| C | Output Cache cho GET public (list/detail) + Redis cache-aside cho dữ liệu dùng lại ở nhiều chỗ | Tận dụng ưu điểm từng loại | Hai cơ chế, hai chỗ phải invalidate — dễ sót |

**➡️ Khuyến nghị: Phương án A.** Chốt bảng TTL duy nhất và đưa vào một chỗ (NFR-PERF-003), Chương 3 chỉ tham chiếu:

| Đối tượng cache | TTL | Invalidate khi |
| --- | --- | --- |
| `categories:all` | 30 phút | Create/Update/Delete category |
| `recipe:{slug}` | 5 phút | Update/Publish/Archive/Delete recipe đó |
| `recipes:list:{queryHash}` | 2 phút | Bất kỳ thay đổi recipe nào (xóa theo prefix) |
| `search:{queryHash}` | 1 phút | Không invalidate, để hết hạn tự nhiên |

---

### MT-18 🟡 — SDK truy cập MinIO

**Hiện trạng:** Mục 2.1.2 (dòng 230) ghi "HTTP/S3 API + **MinIO .NET SDK**"; trong khi Chương 5.3 (dòng 1288), Chương 6.1 (dòng 1322) và Chương 6.2 (dòng 1334) đều ghi **`AWSSDK.S3`**.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`AWSSDK.S3`** | Đổi sang AWS S3 thật chỉ cần bỏ `ServiceURL` override — đúng tinh thần "swap implementation" của `IFileStorageService`; khớp 3/4 tài liệu; hỗ trợ presigned URL đầy đủ | API hơi rườm rà; cấu hình `ForcePathStyle = true` cho MinIO dễ quên |
| B | `Minio` (SDK chính chủ) | API gọn hơn cho MinIO; tài liệu MinIO trực tiếp | Khóa vào MinIO; đổi sang S3 phải viết lại implementation |

**➡️ Khuyến nghị: Phương án A.** Sửa mục 2.1.2. Lưu ý cấu hình bắt buộc cho MinIO: `ForcePathStyle = true`, `ServiceURL = MinIO__Endpoint`.

---

### MT-19 🟡 — API quản lý ảnh: hai thiết kế khác nhau

**Hiện trạng**

| Vị trí | Thiết kế |
| --- | --- |
| FR-RCP-008 (dòng 993, 1004–1006) | Endpoint riêng: `PATCH /recipes/{id}/images/{imgId}/primary`; response upload là `{ url, isPrimary }` |
| Chương 8.4 (dòng 1560–1562) | `PATCH /recipes/{id}/images/{imageId}` với body `{ altText?, isPrimary?, orderIndex? }`; response upload là `{ imageId, originalUrl, altText, isPrimary }` |

Chương 8 còn có `orderIndex` và `altText` mà FR-RCP-008 không nhắc; FR-RCP-008 lại có quy tắc "tự chuyển primary cho ảnh còn lại khi xóa ảnh primary" mà Chương 8 không nhắc.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **PATCH metadata gộp** (theo Chương 8) | RESTful hơn (1 tài nguyên, 1 endpoint); sửa nhiều trường trong 1 request; ít endpoint | Logic "chỉ 1 ảnh primary" nằm lẫn trong handler chung, phải xử lý cẩn thận |
| B | Endpoint hành động riêng `/primary` | Ý định rõ ràng, dễ phân quyền/audit riêng | Nhiều endpoint; muốn sửa `altText` vẫn phải có endpoint khác |

**➡️ Khuyến nghị: Phương án A.** Dùng `PATCH /recipes/{id}/images/{imageId}` với body gộp. Response upload thống nhất là `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }` (bổ sung `mediumUrl`/`thumbnailUrl` vì FR-JOB-002 sinh ra chúng). Giữ lại từ FR-RCP-008 hai quy tắc nghiệp vụ và ghi vào Chương 8: (1) đặt `isPrimary=true` cho một ảnh thì tự bỏ primary các ảnh khác; (2) xóa ảnh primary thì ảnh còn lại có `orderIndex` nhỏ nhất tự lên primary.

---

### MT-20 🟢 — Nhóm bất nhất nhỏ (sửa biên tập)

| # | Nội dung | Vị trí | Đề xuất |
| --- | --- | --- | --- |
| 20.1 | Next.js: "**14+**" vs tài liệu tham chiếu "**15**" | dòng 1318 vs 168 | Chốt **Next.js 15** (khớp .NET 10 / EF Core 10 cùng thế hệ) |
| 20.2 | Trình duyệt tối thiểu: Chrome 90 / Safari 14 (mục 2.4.3) vs Chrome 112 / Safari 16 (mục 5.4) | dòng 296–302 vs 1305 | Chốt **Chrome 112+, Firefox 113+, Safari 16+, Edge 112+** (mốc ES2020 thực tế); sửa mục 2.4.3 |
| 20.3 | Email: "SendGrid hoặc SMTP" (mục 1.5, 2.6.1) vs MailKit + SMTP + Mailhog (Chương 5.3, 6.2) | dòng 187, 329 vs 1292, 1334 | Chốt **MailKit + SMTP** (Mailhog cho dev); bỏ SendGrid khỏi mục 1.5 và 2.6.1 |
| 20.4 | Đặt tên thứ tự: `SortOrder` (FR-RCP-009) vs `OrderIndex` (Chương 7, 8) | dòng 1017, 1027 vs 1455, 1576 | Chốt **`orderIndex`** (khớp schema) |
| 20.5 | Đặt tên thời lượng bước: `DurationMinutes` (FR-RCP-010) vs `TimerMinutes` (Chương 7.3, 8.5) | dòng 1057, 1065 vs 1442, 1568 | Chốt **`timerMinutes`** (khớp schema) |
| 20.6 | Tên trường thời gian: `prepTimeMinutes`/`cookTimeMinutes` (FR-RCP-003) vs `prepTime`/`cookTime` (Chương 7, 8) | dòng 815 vs 851, 1549 | Chốt **`prepTime`/`cookTime`** |
| 20.7 | `RecipeStep.Title` là `NOT NULL` trong schema nhưng FR-RCP-010 không có `title` trong body POST | dòng 1439 vs 1065 | Bổ sung `title` (bắt buộc) vào FR-RCP-010, hoặc đổi cột thành NULL — **khuyến nghị bổ sung vào FR** |
| 20.8 | `Recipe.Instructions` là `NOT NULL` nhưng POST ghi `instructions?` (tùy chọn) | dòng 1403 vs 815 | Đổi cột thành **NULL** (đã có `RecipeSteps` thay thế, chính schema gọi đây là "legacy field") |
| 20.9 | Mật khẩu: FR-AUTH-001 yêu cầu "1 hoa, 1 số, 1 đặc biệt" — **thiếu chữ thường**; NFR-SEC-001 yêu cầu đủ 4 loại | dòng 373 vs 1188 | Lấy **NFR-SEC-001** làm chuẩn (đủ 4 loại) |
| 20.10 | Mã 423 Locked (FR-AUTH-002) và 502 Bad Gateway (FR-AUTH-003) **không có** trong Phụ lục A | dòng 404, 458 vs Phụ lục A | Bổ sung 423 và 502 vào Phụ lục A, kèm error code tương ứng ở Phụ lục B (`AUTH_ACCOUNT_LOCKED`, `AUTH_GOOGLE_UNAVAILABLE`) |
| 20.11 | `DELETE /categories/{id}` ghi "soft delete" ở Chương 8 nhưng FR-CAT-005 ghi "Xóa entity" | dòng 1540 vs 724 | Theo MT-05: **soft delete**, sửa FR-CAT-005 |
| 20.12 | FR-RCP-006 tên là "Archive / **Unarchive**" nhưng không có endpoint unarchive nào | dòng 905 vs 911, 1553 | Bổ sung `PATCH /recipes/{id}/unarchive` (Archived → Draft), hoặc bỏ chữ "Unarchive" khỏi tên |
| 20.13 | `pageSize` mặc định: 12 (FR-SRCH-004, các ví dụ ở Chương 3) vs 10 (Chương 8) | dòng 1123 vs 1517, 1537 | Chốt **12** (chia hết cho lưới 2/3/4 cột của FE) |
| 20.14 | Bộ lọc: `maxCookTime`/`minServings` (FR-SRCH-002) vs `minPrepTime`/`maxPrepTime` (Chương 8) | dòng 1121 vs 1546 | Chốt bộ đầy đủ: `maxCookTime`, `maxPrepTime`, `minServings`, `difficulty`, `categoryId` |
| 20.15 | `difficulty` filter nhận `{Easy\|Medium\|Hard}` nhưng enum có **4** giá trị (thêm `Expert`) | dòng 1121 vs 1407 | Bổ sung `Expert` vào danh sách giá trị hợp lệ |
| 20.16 | Mục 1.5 ghi "~30 endpoint", đếm thực tế Chương 8 là **31** | dòng 190 | Sửa thành 31, hoặc giữ "~30" là ước lượng |

---

> **Từ đây là các mục phát hiện ở lượt rà soát thứ 2.** Điểm chung của nhóm này khác lượt 1: lượt 1 là "hai chỗ nói hai kiểu", lượt 2 phần lớn là **"yêu cầu mồ côi"** — một yêu cầu tồn tại ở chương này nhưng không có dữ liệu, API hoặc FR nào ở chương khác đỡ được nó. Loại này nguy hiểm vì khi test theo SRS sẽ phát hiện "thiếu chức năng" chứ không phải "sai chức năng".

---

### MT-21 🔴 — Policy `"VerifiedAuthor"` đòi email đã xác nhận, nhưng không có luồng xác nhận email

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| Mục 2.3 – Ghi chú phân quyền (dòng 261) | "(3) **Policy-Based Authorization:** Policy `"VerifiedAuthor"` yêu cầu **email đã xác nhận**" |
| FR-AUTH-001 (dòng 365, 377) | Đăng ký xong **tự động gán role "Author"** và **auto-login ngay** — không có bước xác nhận email |
| FR-JOB-001 (dòng 1140) | Email chào mừng chứa "link kích hoạt email **(nếu cần)**" — bỏ ngỏ |
| FR-AUTH-006 (dòng 543) | `UserProfileDto` có trường `emailConfirmed` |
| Toàn tài liệu | **Không có FR nào** cho: gửi email xác nhận, endpoint xác nhận token, gửi lại email xác nhận |

**Vì sao là vấn đề:** `EmailConfirmed` của ASP.NET Core Identity mặc định là `false`. Nếu gắn policy `"VerifiedAuthor"` vào các endpoint ghi, thì **không người dùng nào tạo được công thức** — vì không tồn tại đường nào để chuyển `EmailConfirmed` thành `true`. Đây là loại lỗi làm tắc toàn bộ luồng chính, nhưng chỉ lộ ra khi test end-to-end.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bỏ policy `"VerifiedAuthor"` khỏi v1.0**, chỉ dùng 2 tầng phân quyền (Role + Resource) | Khớp với FR-AUTH-001 (auto-login) và với mục 1.2.3 vốn không liệt kê xác thực email trong phạm vi; luồng chính chạy thông ngay | Người dùng có thể đăng ký bằng email không có thật (chấp nhận được với đồ án) |
| B | Bổ sung đầy đủ luồng xác nhận email: FR mới (gửi mail có token, `GET /auth/confirm-email?token=`, gửi lại), rồi mới gắn policy | Đúng chuẩn sản phẩm thật; tận dụng được `emailConfirmed` và FR-JOB-001 | Thêm ~2 FR + 2 endpoint + template email; cần SMTP chạy thật khi demo, nếu mail lỗi là tắc demo |
| C | Giữ policy nhưng seed `EmailConfirmed = true` ngay khi đăng ký | Không phải sửa mục 2.3 | Policy trở thành vô nghĩa (luôn đúng) — tệ hơn bỏ hẳn vì gây hiểu nhầm là có bảo vệ |

**➡️ Khuyến nghị: Phương án A.** Sửa mục 2.3 còn 2 tầng phân quyền; giữ `emailConfirmed` trong DTO như thông tin hiển thị (luôn `false`) hoặc bỏ luôn; sửa FR-JOB-001 bỏ "link kích hoạt email". Nếu nhóm muốn ghi điểm thêm thì chọn B và **phải** viết đủ FR trước khi code.

---

### MT-22 🟡 — Chức năng khóa tài khoản tồn tại trong schema và mã lỗi nhưng không có FR nào

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| Chương 7.7 (dòng 1490) | Cột `IsActive boolean NOT NULL DEFAULT true` — "**Admin có thể deactivate user (ban)**" |
| Phụ lục B (dòng 1622) | `AUTH_ACCOUNT_DISABLED` / 403 — "Tài khoản bị vô hiệu hóa (`IsActive=false`) **bởi Admin**" |
| Mục 2.3 – quyền Admin (dòng 259) | Liệt kê 5 quyền Admin: CRUD danh mục, sửa/xóa recipe bất kỳ, Hangfire Dashboard, xem logs — **không có quản lý người dùng** |
| Chương 3 & 8 | **Không có FR nào**, **không có endpoint nào** để bật/tắt `IsActive` |
| FR-AUTH-002 (dòng 409–411) | Luồng đăng nhập chỉ kiểm tra mật khẩu và lockout — **không kiểm tra `IsActive`** |

**Vì sao là vấn đề:** Hai mâu thuẫn lồng nhau: (a) tính năng ban user không có FR nào mô tả; (b) kể cả khi Admin sửa tay `IsActive=false` trong DB, người dùng đó **vẫn đăng nhập được** vì FR-AUTH-002 không kiểm tra cờ này — mã lỗi `AUTH_ACCOUNT_DISABLED` sẽ không bao giờ được trả về.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Thêm FR-AUTH-008 "Khóa/Mở khóa tài khoản [Admin]"** + `PATCH /users/{id}/status`; bổ sung bước kiểm tra `IsActive` vào FR-AUTH-002 và FR-AUTH-004 | Tính năng hoàn chỉnh, đúng như schema và mã lỗi đã hứa; bổ sung quyền Admin thực chất | Thêm 1 FR + 1 endpoint + cập nhật mục 2.3; phát sinh câu hỏi "ban rồi thì token đang có còn dùng được không" |
| B | Bỏ cột `IsActive` và mã lỗi `AUTH_ACCOUNT_DISABLED` khỏi SRS | Gọn nhất, đúng phạm vi tối thiểu | Mất khả năng chặn tài khoản spam; phải sửa Chương 7 + Phụ lục B |
| C | Giữ cột, chỉ bổ sung kiểm tra `IsActive` ở login/refresh, không làm endpoint (Admin sửa qua DB) | Rẻ nhất, mã lỗi có tác dụng thật | "Admin có thể deactivate" vẫn là lời hứa suông ở tầng ứng dụng |

**➡️ Khuyến nghị: Phương án A** nếu còn thời gian, **C** nếu cần cắt phạm vi. Dù chọn gì cũng **bắt buộc** bổ sung bước kiểm tra `IsActive == true` vào FR-AUTH-002 (login) và FR-AUTH-004 (refresh) — nếu không, mã lỗi đã đặc tả là mã chết. Khi ban, nên revoke toàn bộ refresh token của user đó (access token cũ vẫn sống tối đa 15 phút — chấp nhận được, ghi rõ vào SRS).

---

### MT-23 🟡 — Trang chủ hiển thị "recipe nổi bật" nhưng không có dữ liệu lẫn API

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| Mục 5.1 (dòng 1251) | Route `/` — "Trang chủ: **danh sách recipe nổi bật** + categories", ISR revalidate=3600 |
| Chương 7.2 | Bảng `Recipes` **không có** cột `IsFeatured` / `FeaturedAt` / `ViewCount` / `Rating` |
| Chương 8.3 | `GET /recipes` chỉ có filter `categoryId`, `difficulty`, `prepTime` và sort theo `createdAt`/`title`/`cookTime` — **không có tham số nào lấy "nổi bật"** |
| Mục 1.2.3 | Rating và Bookmark — hai tiêu chí "nổi bật" tự nhiên nhất — đều **ngoài phạm vi** |

**Vì sao là vấn đề:** FE không có cách nào dựng trang chủ đúng đặc tả. Đây là màn hình đầu tiên người chấm nhìn thấy, nên cần chốt sớm.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Định nghĩa lại "nổi bật" = mới xuất bản nhất**: trang chủ gọi `GET /recipes?sortBy=publishedAt&sortOrder=desc&pageSize=8` | Không thêm cột, không thêm endpoint; dùng ngay index `IDX_Recipe_PublishedAt` đã có sẵn trong schema | "Nổi bật" chỉ là tên gọi, không phản ánh chất lượng nội dung |
| B | Thêm cột `IsFeatured boolean` + Admin bật/tắt qua `PATCH /recipes/{id}/feature` | Admin chủ động chọn bài lên trang chủ — giống blog thật; dễ demo | Thêm cột + FR + endpoint + quyền; Admin phải bật tay thì trang chủ mới có bài |
| C | Thêm `ViewCount` và sắp xếp theo lượt xem | "Nổi bật" có căn cứ tự nhiên | Phải ghi DB mỗi lượt xem → đụng NFR-PERF (write nóng, hỏng cache); cần chống spam đếm view |

**➡️ Khuyến nghị: Phương án A** cho v1.0 (sửa mục 5.1 thành "công thức mới xuất bản"), **B** nếu muốn có điểm nhấn cho phần demo. Tránh C vì xung đột trực tiếp với mục tiêu cache và p95 ≤ 500ms.

---

### MT-24 🟡 — Hứa Rich Snippets "star rating" trong khi Rating System ngoài phạm vi

**Hiện trạng**

- Mục 1.2.3 (dòng 115): "Hệ thống bình luận (Comment System) và **đánh giá sao (Rating System)**" — nằm **ngoài phạm vi** v1.0.
- NFR-SEO-001 (dòng 1234): danh sách thuộc tính JSON-LD **không có** `aggregateRating`, nhưng câu kết lại ghi: "Kết quả: Rich Snippets trên Google Search (**star rating**, time, ingredients)".

**Vì sao là vấn đề:** Không có `aggregateRating` thì Google không bao giờ hiển thị sao. Nếu QA lấy NFR-SEO-001 làm tiêu chí nghiệm thu thì hạng mục này **không thể pass** — không phải do làm sai mà do yêu cầu tự mâu thuẫn với phạm vi.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bỏ "star rating" khỏi NFR-SEO-001**, giữ tiêu chí "pass Google Rich Results Test cho loại Recipe" | Trung thực với phạm vi; vẫn nghiệm thu được bằng công cụ của Google | Rich snippet trông kém bắt mắt hơn |
| B | Đưa Rating System vào phạm vi | Có sao thật | Phá vỡ phạm vi đã duyệt, kéo theo bảng `Ratings`, FR mới, chống spam vote — quá lớn |
| C | Nhúng `aggregateRating` cố định/giả | Có sao hiển thị | **Vi phạm chính sách structured data của Google** (dữ liệu không phản ánh nội dung thật) → nguy cơ bị phạt; không nên làm |

**➡️ Khuyến nghị: Phương án A.** Sửa câu kết thành "Rich Snippets (thời gian nấu, khẩu phần, nguyên liệu, calo)" — đều là dữ liệu hệ thống có thật.

---

### MT-25 🔴 — Ba vấn đề chồng nhau ở Full-Text Search

**Hiện trạng**

| # | Nội dung | Vị trí |
| --- | --- | --- |
| (a) | "Trường SearchVector (**computed column**) được tự động cập nhật bởi **PostgreSQL trigger**" — hai cơ chế loại trừ nhau trong cùng một câu | dòng 1092 |
| (b) | `EF.Functions.ToTsQuery("**vietnamese**", query)` — PostgreSQL 16 **không có sẵn** text search configuration tên `vietnamese`; mục 2.4.1 chỉ yêu cầu cài `unaccent` và `pg_trgm`, không có dictionary tiếng Việt | dòng 1103 vs 272 |
| (c) | Nếu dùng trigger thì phải viết raw SQL trong migration, trong khi CONS-006 ghi "Migrations qua EF Core Code-First. **Không viết raw SQL trực tiếp**" | dòng 1411 vs 315 |

**Vì sao là vấn đề:** (b) làm query **ném exception lúc chạy** (`text search configuration "vietnamese" does not exist`) — tính năng tìm kiếm chết hẳn, mà chỉ phát hiện được khi chạy thật chứ không phải lúc biên dịch. (a) khiến hai người làm hai kiểu khác nhau cho cùng một cột.

**Phương án cho (a) + (c)**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`GENERATED ALWAYS AS (...) STORED`** khai báo qua EF Core (`.HasComputedColumnSql(..., stored: true)`) | PostgreSQL tự cập nhật, không cần trigger; khai báo hoàn toàn trong `OnModelCreating` → **không vi phạm CONS-006**; không thể quên đồng bộ | Biểu thức phải `IMMUTABLE` — `unaccent()` mặc định **không** immutable, phải bọc bằng một hàm wrapper immutable (một đoạn SQL nhỏ trong migration) |
| B | Trigger + function viết trong migration `migrationBuilder.Sql(...)` | Linh hoạt, dùng `unaccent` trực tiếp thoải mái | Vi phạm chữ "không raw SQL" của CONS-006 (dù CONS-006 nhắm vào query, không phải DDL — nên diễn giải lại cho rõ) |
| C | Cập nhật `SearchVector` trong C# ở tầng Application mỗi khi lưu | Không cần SQL nào | Dễ quên khi update qua đường khác; logic FTS lọt ra khỏi DB; khó tái tạo index khi seed dữ liệu |

**Phương án cho (b)**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| B1 | **`simple` + `unaccent`**: `to_tsvector('simple', unaccent(coalesce(Title,'') \|\| ' ' \|\| coalesce(Description,'')))` | Chạy được ngay trên PostgreSQL 16 gốc; bỏ dấu nên "pho" khớp "phở" — đúng yêu cầu đã nêu; không cần cài thêm gì | Không có stemming (không tự hiểu "nấu"/"nấu nướng" cùng gốc) — với tiếng Việt thì stemming vốn cũng ít giá trị |
| B2 | Cài dictionary tiếng Việt tự build (`CREATE TEXT SEARCH CONFIGURATION vietnamese ...`) | Đúng chữ "cấu hình tiếng Việt" trong SRS | Phải build image PostgreSQL riêng; tăng độ phức tạp Docker; rủi ro môi trường khi demo |
| B3 | Bỏ FTS, dùng `pg_trgm` + `ILIKE`/similarity | Rất đơn giản | Mất `ts_rank` (xếp hạng độ liên quan) — mà FR-SRCH-001 lại bắt buộc có `relevanceScore` |

**➡️ Khuyến nghị: A + B1.** Cụ thể ghi vào SRS: cột `SearchVector` là **generated column STORED**, biểu thức `to_tsvector('simple', unaccent_immutable(Title || ' ' || Description))`, index GIN; query dùng `ToTsQuery('simple', ...)`. Đồng thời **diễn giải lại CONS-006** thành: *"Không viết raw SQL cho truy vấn dữ liệu; DDL đặc thù PostgreSQL (extension, generated column, index GIN) được phép khai báo trong migration."* — nếu không, CONS-006 sẽ chặn cả việc `CREATE EXTENSION unaccent`.

---

### MT-26 🟡 — NFR đòi index cho mọi cột sort/filter, nhưng schema thiếu đúng những cột đó

**Hiện trạng**

- NFR-PERF-004 (dòng 1179): "**Index: đảm bảo mọi WHERE/ORDER BY column đều có B-tree index tương ứng**".
- Bảng `Recipes` (Chương 7.2) có index cho: `Slug`, `Difficulty`, `Status`, `CategoryId`, `AuthorId`, `SearchVector`, `PublishedAt`, `IsDeleted`.
- Nhưng các cột **được dùng để sort/filter mà không có index**:

| Cột | Dùng ở đâu | Index? |
| --- | --- | --- |
| `CreatedAt` | **Sort mặc định** `sort=-createdAt` (FR-SRCH-003) | ❌ |
| `CookTime` | Filter `maxCookTime` (FR-SRCH-002) + sort `cookTime` (FR-RCP-001) | ❌ |
| `PrepTime` | Filter `minPrepTime`/`maxPrepTime` (Chương 8.3) | ❌ |
| `Servings` | Filter `minServings` (FR-SRCH-002) | ❌ |
| `Title` | Sort `title` (FR-RCP-001) | Chỉ có GIN trigram, **ghi rõ "optional"** — GIN trigram không phục vụ ORDER BY |

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bổ sung index composite theo đúng truy vấn thật**, ví dụ `(Status, PublishedAt DESC)` và `(Status, CategoryId, PublishedAt DESC)` | Hiệu quả nhất: mọi truy vấn danh sách đều lọc `Status = Published` trước rồi mới sort → composite phủ đúng; ít index hơn mà nhanh hơn | Phải phân tích truy vấn trước khi khai báo; người mới khó hình dung |
| B | Thêm B-tree đơn cho từng cột thiếu | Đúng nguyên văn NFR-PERF-004, dễ làm | Nhiều index → chậm mọi lệnh ghi, tốn dung lượng; PostgreSQL thường chỉ chọn 1 index cho 1 truy vấn nên phần lớn nằm không |
| C | Hạ yêu cầu NFR-PERF-004 xuống "các truy vấn chính phải pass EXPLAIN ANALYZE" | Thực dụng, khớp với câu "EXPLAIN ANALYZE phải pass review" ngay bên dưới trong cùng NFR | Mất tiêu chí định lượng rõ ràng |

**➡️ Khuyến nghị: A + C.** Khai báo 3 index composite: `(IsDeleted, Status, PublishedAt DESC)`, `(Status, CategoryId, PublishedAt DESC)`, `(Status, CookTime)`; sửa NFR-PERF-004 thành *"mọi truy vấn danh sách/tìm kiếm phải dùng Index Scan (chứng minh bằng EXPLAIN ANALYZE), không chấp nhận Seq Scan trên bảng Recipes"* — tiêu chí này vừa đo được vừa không ép tạo index thừa.

---

### MT-27 🟡 — Yêu cầu 301 redirect slug cũ nhưng không có chỗ lưu slug cũ

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| NFR-SEO-004 (dòng 1237) | "Slug... **không thay đổi sau khi publish**. Redirect: Nếu slug thay đổi (draft) → **301 redirect từ slug cũ sang slug mới**" |
| Chương 7.2 (dòng 1400) | `Slug` là một cột đơn, UNIQUE — **không có bảng `RecipeSlugHistory` hay cột lưu slug cũ** |
| FR-RCP-004 | Luồng cập nhật recipe **không nhắc gì** đến việc sinh lại slug khi Title đổi, nhưng mã trạng thái lại có "409 Conflict – Concurrency **hoặc slug trùng**" → hàm ý slug **có** được sinh lại |

Ba chỗ mô tả ba hành vi khác nhau: không đổi / có đổi / đổi rồi redirect từ giá trị cũ không được lưu ở đâu cả.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Slug sinh một lần lúc tạo, không bao giờ đổi** (kể cả khi Draft); bỏ yêu cầu 301 | Đơn giản nhất, không cần bảng lịch sử; link không bao giờ chết; bỏ luôn được vế "slug trùng" ở FR-RCP-004 | Sửa lỗi chính tả ở tiêu đề thì URL vẫn giữ chữ sai |
| B | Cho đổi slug khi còn **Draft** (lúc này chưa ai biết URL, không cần redirect), **khóa** sau khi Publish | Cân bằng tốt: sửa được lỗi chính tả trước khi công khai; vẫn không cần lịch sử slug | Phải thêm điều kiện "chỉ đổi khi Status == Draft" vào FR-RCP-004 |
| C | Cho đổi bất kỳ lúc nào + bảng `RecipeSlugHistory(OldSlug PK, RecipeId, CreatedAt)`; `GET /recipes/{slug}` không thấy thì tra lịch sử → 301 | Đúng chuẩn SEO nhất, giữ mọi link cũ | Thêm 1 bảng + logic tra cứu 2 bước; phải đảm bảo OldSlug không đụng slug đang dùng |

**➡️ Khuyến nghị: Phương án B.** Sửa NFR-SEO-004 thành: *"Slug có thể sinh lại khi recipe ở trạng thái Draft; sau lần Publish đầu tiên slug bị khóa vĩnh viễn. Do đó hệ thống không cần cơ chế 301 redirect."* Bổ sung vào FR-RCP-004 bước kiểm tra `Status == Draft` trước khi sinh lại slug.

---

### MT-28 🟡 — `sitemap.xml` sinh ra ở sai nơi so với nơi cần phục vụ

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| FR-JOB-003 (dòng 1142) | Job của **Backend** tạo `sitemap.xml`, "Upload lên **MinIO** hoặc lưu vào **`wwwroot`**"; "Gửi thông báo đến Google Search Console (**ping**)" |
| NFR-SEO-003 (dòng 1236) | "`robots.txt`: cho phép tất cả crawlers, **khai báo Sitemap URL**" |
| Mục 5.1 + 6.1 | Site công khai do **Next.js** phục vụ ở domain chính; Backend nằm sau `/api` (dev: cổng 5000, prod: `api.culinaryblog.com`) |

Hai vấn đề:
1. **Sai vị trí phục vụ:** crawler tìm sitemap tại `https://domain.com/sitemap.xml`, nhưng file lại nằm ở MinIO (`minio:9000/...`) hoặc `wwwroot` của API (`api.culinaryblog.com/sitemap.xml`) — không khớp `robots.txt` của site chính. Ngoài ra `wwwroot` nằm **trong container**, mỗi lần deploy lại là mất file, và với nhiều API instance thì mỗi instance một bản khác nhau.
2. **Ping đã bị khai tử:** Google **ngừng hỗ trợ** endpoint `https://www.google.com/ping?sitemap=` từ tháng 6/2023 — gọi vào chỉ nhận 404. (Đây là lỗi so với thực tế bên ngoài, không phải mâu thuẫn nội bộ, nhưng vẫn khiến FR-JOB-003 không nghiệm thu được.)

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Next.js tự sinh sitemap** bằng `app/sitemap.ts` (gọi API lấy danh sách slug + `lastmod`), revalidate theo giờ | Đúng vị trí `https://domain.com/sitemap.xml`; không cần lưu file, không cần MinIO; Next.js hỗ trợ sẵn; **bỏ được FR-JOB-003** | Mỗi lần crawler gọi là một lần query API (nhẹ, lại có ISR cache) |
| B | Giữ job BE, upload lên MinIO, **Nginx proxy** `/sitemap.xml` → MinIO | Giữ nguyên FR-JOB-003 | Thêm một rule Nginx; sitemap "đông cứng" tối đa 24h; vẫn cần dọn file cũ |
| C | Job BE ghi vào volume dùng chung, Nginx serve trực tiếp | Không phụ thuộc MinIO | Volume dùng chung giữa API và Nginx — vướng khi scale nhiều instance |

**➡️ Khuyến nghị: Phương án A.** Chuyển trách nhiệm sitemap sang Next.js (`app/sitemap.ts` + `app/robots.ts`), sửa FR-JOB-003 thành *"(Tùy chọn) job cảnh báo khi số URL trong sitemap giảm bất thường"* hoặc bỏ hẳn FR này và chỉnh mục 2.2 còn **FR-JOB 2**. Bỏ bước ping Google; thay bằng khai báo sitemap trong `robots.txt` — đó là cách Google khuyến nghị hiện nay.

---

### MT-29 🟢 — Kubernetes xuất hiện 3 chỗ dù hạ tầng chỉ có Docker Compose

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| FR-OBS-001 (dòng 1150) | "Readiness fail khi DB/Redis down → **Kubernetes**/Nginx ngừng route traffic" |
| NFR-REL-001 (dòng 1209) | "`/health/ready` probe mỗi 10 giây (**Kubernetes readiness probe**)" |
| NFR-SEC-007 (dòng 1194) | "Production: Environment variables (Docker Compose `env_file` / **Kubernetes Secrets**)" |
| CONS-009 + mục 6.5 | Triển khai **chỉ có Docker Compose**; không có manifest, Helm chart hay FR nào về K8s |

**Vì sao là vấn đề:** Nhẹ, nhưng khiến người đọc tưởng phải dựng K8s. Đáng nói hơn: **Docker Compose không tự đọc `/health/ready`** như K8s — muốn có hành vi tương đương phải khai báo `healthcheck:` trong compose và `depends_on: condition: service_healthy`, mà mục 6.5 không có. Vậy cơ chế "ngừng route traffic khi DB down" hiện **không có ai thực thi**.

**Phương án:** (A) Bỏ mọi nhắc đến K8s, thay bằng `healthcheck:` trong Docker Compose + Nginx `proxy_next_upstream`. (B) Giữ và ghi chú "(định hướng tương lai, ngoài phạm vi v1.0)". (C) Đưa K8s vào phạm vi — không nên, quá lớn cho 6 buổi.

**➡️ Khuyến nghị: Phương án A**, và **bổ sung khối `healthcheck:` cho service `api`, `postgres`, `redis` vào mục 6.5** — nếu không, NFR-REL-001 không có cách nào nghiệm thu.

---

### MT-30 🟡 — Domain layer có được phụ thuộc FluentValidation hay không

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| CONS-001 (dòng 310) | "Tầng Domain **không được phụ thuộc bất kỳ thư viện ngoài nào**" |
| Mục 6.2 – Domain Layer (dòng 1332) | "**Không có NuGet dependencies (chỉ .NET BCL)**" |
| NFR-MAINT-004 (dòng 1220) | "Domain layer: ... Không có nuget packages **ngoài FluentValidation**" ← cho phép |
| Mục 6.2 – Application Layer (dòng 1333) | Validators (FluentValidation) nằm ở **Application** |
| CONS-008 (dòng 317) | Validation qua FluentValidation + MediatR Pipeline Behavior → cũng là Application |

4 chỗ nói Domain sạch / 1 chỗ cho phép FluentValidation. Quan trọng vì NFR-MAINT-004 yêu cầu **ArchUnit.NET test tự động kiểm tra** — viết test theo câu nào thì build sẽ đỏ theo câu đó.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Domain tuyệt đối sạch** (chỉ .NET BCL); mọi validation ở Application; Domain chỉ ném `DomainException` từ các invariant tự viết | Đúng Clean Architecture nguyên bản; khớp 4/5 chỗ; ArchUnit test viết đơn giản (`Domain` không tham chiếu assembly ngoài nào) | Validate invariant trong Domain phải viết tay bằng `if/throw` |
| B | Cho Domain dùng FluentValidation | Tái sử dụng cú pháp validator ở cả hai tầng | Domain "rò rỉ" ra thư viện ngoài; phải sửa CONS-001 vốn là ràng buộc "không thể thương lượng" |

**➡️ Khuyến nghị: Phương án A.** Sửa NFR-MAINT-004 bỏ cụm "ngoài FluentValidation". Phân vai rõ: **FluentValidation** kiểm tra *dữ liệu đầu vào* (định dạng, độ dài, bắt buộc) ở Application; **DomainException** bảo vệ *bất biến nghiệp vụ* (ví dụ "Recipe phải có ≥1 step mới publish được") ở Domain — đúng như FR-RCP-005 đang mô tả.

---

### MT-31 🟢 — Danh sách Pipeline Behavior lệch nhau giữa mục 6.2 và 6.3

**Hiện trạng**

| Nguồn | Danh sách behavior |
| --- | --- |
| Mục 6.2 – Application Layer (dòng 1333) | `ValidationBehavior`, `LoggingBehavior`, `CachingBehavior`, **`PerformanceBehavior`** |
| Mục 6.3 – bảng pipeline (dòng 1342–1347) | `LoggingBehavior`, `ValidationBehavior`, `CachingBehavior`, Handler, **`CacheInvalidationBehavior`** |

`PerformanceBehavior` có ở 6.2 nhưng không có trong pipeline; `CacheInvalidationBehavior` thì ngược lại. Thêm nữa, FR-OBS-002 (dòng 1151) giao việc "cảnh báo khi request > 500ms" cho `LoggingBehavior`, còn NFR-PERF-004 (dòng 1179) lại nhắc "Serilog performance behavior" cho ngưỡng > 100ms của query — hai ngưỡng, hai chủ thể, không rõ ai làm gì.

**Phương án:** (A) Chốt **5 behavior** theo đúng thứ tự của 6.3 và bổ sung `PerformanceBehavior` thành bước riêng. (B) Gộp đo hiệu năng vào `LoggingBehavior`, bỏ `PerformanceBehavior` khỏi 6.2.

**➡️ Khuyến nghị: Phương án B** (ít lớp hơn, đúng thực tế: đo thời gian là việc tự nhiên của logging behavior). Chốt 4 behavior: `Logging` (kèm cảnh báo > 500ms) → `Validation` → `Caching` → Handler → `CacheInvalidation`. Ngưỡng > 100ms ở NFR-PERF-004 là của **EF Core command interceptor**, không phải MediatR behavior — ghi rõ để khỏi nhầm.

---

### MT-32 🟡 — Docker Compose map cổng cố định, mâu thuẫn với yêu cầu chạy nhiều API instance

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| Mục 6.5 (bảng services) | `api` map cổng **`5000:8080`** cố định; không có `deploy.replicas` |
| NFR-SCALE-003 (dòng 1228) | "Nginx: **load balancer upstream pool cho nhiều API instances**" |
| NFR-SCALE-001 (dòng 1226) | Stateless để "hỗ trợ **horizontal scaling**"; Hangfire chạy **multiple workers** |
| NFR-PERF-002 (dòng 1176) | "Horizontal scaling: thêm instance tăng tuyến tính" |

**Vì sao là vấn đề:** `docker compose up --scale api=3` sẽ **lỗi ngay** vì ba container không thể cùng bind cổng 5000 của host. Vậy mọi yêu cầu scale ngang trong NFR đều không thực hiện được với file compose như đặc tả.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bỏ `ports` của `api`**, chỉ expose trong network nội bộ; Nginx trỏ `upstream api:8080` và Docker DNS tự round-robin | `--scale api=N` chạy được ngay; đúng mô hình reverse proxy; an toàn hơn (API không lộ ra host) | Muốn gọi thẳng API lúc dev phải qua Nginx hoặc tạm mở cổng ở file compose dev |
| B | Giữ cổng cho dev, thêm `docker-compose.prod.yml` bỏ `ports` + `deploy.replicas: 3` | Dev tiện, prod scale được | Hai file phải giữ đồng bộ (mục 6.5 vốn đã nói có 2 file) |
| C | Chấp nhận 1 instance, hạ NFR-SCALE xuống "thiết kế sẵn sàng scale, chưa triển khai" | Trung thực với quy mô đồ án (≤ 10.000 recipe, single-server) | Mất một phần nội dung có thể ghi điểm |

**➡️ Khuyến nghị: Phương án B.** Dev giữ `5000:8080` để gọi Scalar/Postman cho nhanh; `docker-compose.prod.yml` bỏ `ports`, thêm `deploy: replicas: 3`, Nginx dùng `upstream`. Ghi rõ cấu hình này vào mục 6.5 để NFR-SCALE-003 có căn cứ nghiệm thu. Lưu ý: khi scale, **recurring job sinh sitemap phải có distributed lock** như NFR-SCALE-001 đã nêu (hoặc bỏ hẳn theo MT-28).

---

### MT-33 🟢 — Nhóm bất nhất nhỏ đợt 2

| # | Nội dung | Vị trí | Đề xuất |
| --- | --- | --- | --- |
| 33.1 | Tên interface email: `IEmailService` (mục 6.2) vs `IEmailSender` (mục 5.3) | dòng 1333 vs 1292 | Chốt **`IEmailService`** (tránh trùng tên `IEmailSender` của ASP.NET Core Identity) |
| 33.2 | Mục 2.6.2 nói Hangfire hỏng thì "fire-and-forget jobs **sẽ bị mất**", trong khi mục 3.6 nói job queue lưu **persistent trên PostgreSQL** (đã enqueue thì sống sót restart) | dòng 340 vs 1136 | Sửa thành: "job đã enqueue vẫn nằm trong hàng đợi và chạy lại khi API khởi động; chỉ bị trễ, không mất" |
| 33.3 | ISR revalidate lệch TTL cache: `/categories` revalidate=**3600s** (mục 5.1) trong khi TTL danh mục là 30 phút (NFR-PERF-003) / 60 phút (Chương 3); `/categories/[slug]` = 600s không khớp chỗ nào | dòng 1254–1255 vs 1178 | Đồng bộ với bảng TTL đã chốt ở **MT-17**: ISR nên ≤ TTL cache của API tương ứng |
| 33.4 | Route `GET /recipes/search` và `GET /recipes/{slug}` — chuỗi `search` khớp cả hai mẫu | dòng 1547–1548 | Đăng ký `/recipes/search` **trước** trong Minimal API, hoặc thêm ràng buộc `{slug:regex(...)}`; đồng thời cấm slug trùng các từ khóa dành riêng (`search`, `new`, `edit`) khi sinh slug |
| 33.5 | Chương 8 ghi quy ước "(Author ⊂ Admin)" nhưng role trong ASP.NET Core Identity **không phân cấp** | dòng 1516 | Khi seed, gán Admin **cả hai** role (`Author` + `Admin`), hoặc policy `AuthorPolicy` chấp nhận `RequireRole("Author","Admin")` — ghi rõ vào SRS |
| 33.6 | Định danh không thống nhất: đọc theo **slug** (`GET /recipes/{slug}`), ghi theo **id** (`PUT/PATCH/DELETE /recipes/{id}`) | dòng 1547 vs 1549–1554 | Không sai, nhưng cần ghi rõ quy ước để FE luôn giữ cả `id` lẫn `slug` trong `RecipeSummaryDto` |
| 33.7 | Mục 2.4.1 ghi Docker Compose dùng cho "local dev và **staging**" (hàm ý không dùng cho production), mục 6.5 lại có `docker-compose.prod.yml` | dòng 275 vs 1362 | Sửa mục 2.4.1 thành "local dev, staging và production" |
| 33.8 | FR-CAT-001 trả `{ id, name, slug, description, recipeCount }` — thiếu `imageUrl`, `orderIndex` mà Chương 7.6 và Chương 8.2 đều có | dòng 594 vs 1479, 1536 | Bổ sung `imageUrl`, `orderIndex` vào `CategoryDto`; FR-CAT-003 cũng cần nhận `imageUrl` khi tạo |
| 33.9 | FR-AUTH-006 mô tả trả 404 khi "user đã bị xóa", nhưng **không có FR nào** xóa người dùng và `AspNetUsers` không kế thừa `BaseEntity` (không có `IsDeleted`) | dòng 548 | Đổi thành 401 (token trỏ tới user không còn hợp lệ) hoặc bỏ nhánh này |

---

> **Từ đây là các mục phát hiện ở lượt rà soát thứ 3.** Góc soi lần này là **bên trong từng FR**: đọc ngang bốn ô "Mô tả → Điều kiện tiên quyết → Luồng chính → HTTP Status Code" của cùng một yêu cầu xem chúng có tự khớp nhau không. Loại lỗi này khó thấy nhất vì mọi thứ nằm trong cùng một bảng, người đọc mặc nhiên cho rằng chúng đã thống nhất.

---

### MT-34 🔴 — Output Cache không phân biệt danh tính người gọi → rò rỉ công thức Draft

**Đây là lỗi nghiêm trọng nhất trong toàn bộ 41 mục.** Nó là lỗi bảo mật, không chỉ là lỗi tài liệu.

**Hiện trạng** — trong **cùng một FR**, cách nhau 5 dòng:

| Vị trí | Nội dung |
| --- | --- |
| FR-RCP-001, bước 4 (dòng 756) | "Áp dụng Authorization filter: nếu Guest → chỉ Published; nếu Author → Published OR (Draft AND `AuthorId == userId`); nếu **Admin → tất cả**" |
| FR-RCP-001, bước 9 (dòng 761) | "Output Cache lưu response theo key = **`{path}?{queryString}`**" |
| FR-RCP-001, Mô tả (dòng 745) | "cache với Output Cache (.NET 10) theo policy "RecipeList" (TTL 15 phút, **vary by query string**)" |
| FR-RCP-002, Mô tả + bước 5 (dòng 777, 789) | "Recipe Draft chỉ được xem bởi tác giả sở hữu hoặc Admin" + "Endpoint được cache với Output Cache policy "RecipeDetail" (**TTL 60 phút**)" |

**Vì sao là vấn đề:** Response **phụ thuộc vào danh tính người gọi**, nhưng khóa cache **chỉ gồm đường dẫn và query string**. Hậu quả cụ thể:

1. Admin mở `GET /api/v1/recipes?page=1` → response chứa **toàn bộ recipe Draft của mọi Author** được nạp vào cache dưới khóa `/api/v1/recipes?page=1`.
2. Trong 15 phút tiếp theo, **mọi Guest** gọi đúng URL đó sẽ nhận lại **nguyên response đã cache** — tức là đọc được Draft của người khác.
3. Với FR-RCP-002 còn tệ hơn: Author mở Draft của mình tại `/api/v1/recipes/{slug}` → nội dung Draft bị cache **60 phút**; bất kỳ ai biết slug đều đọc được, dù bước 5 đã kiểm tra quyền — vì request thứ hai **không bao giờ chạy tới handler**, Output Cache trả thẳng từ bộ nhớ.

Điều này vi phạm trực tiếp NFR-SEC-006 ("Kiểm tra phân quyền tại Application Layer") và làm vô hiệu hóa toàn bộ cơ chế Resource-Based Authorization ở mục 2.3. Nghịch lý: **chính cơ chế cache được đặc tả để đạt NFR-PERF lại phá vỡ NFR-SEC.**

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Chỉ cache phần dữ liệu công khai**: endpoint công khai (`Status == Published`) được cache bình thường; dữ liệu Draft/Archived tách sang endpoint riêng `GET /recipes/mine` **không cache**, có `Cache-Control: no-store` | Tách bạch rõ ràng công khai / riêng tư — đúng nguyên tắc "không bao giờ cache dữ liệu riêng tư dưới khóa dùng chung"; endpoint công khai giữ được hit rate cao; trang dashboard vốn cũng cần danh sách riêng | Thêm 1 endpoint + 1 query; FE phải gọi đúng endpoint theo ngữ cảnh |
| B | Thêm `userId`/`role` vào khóa cache (`VaryByValue`) | Giữ được một endpoint duy nhất | Mỗi user một bản cache → hit rate sụp đổ, gần như vô nghĩa; với 5.000 user thì cache phình to; vẫn rủi ro nếu quên vary ở một endpoint nào đó |
| C | Bỏ cache hoàn toàn cho `/recipes` và `/recipes/{slug}` | An toàn tuyệt đối, đơn giản | Mất luôn mục tiêu p50 ≤ 150ms của NFR-PERF-001; lãng phí vì phần lớn lượt truy cập là Guest xem nội dung công khai |
| D | Chỉ cache khi request **không có** `Authorization` header (ẩn danh), bỏ qua cache khi đã đăng nhập | Một endpoint; Guest (đại đa số, theo mục 2.3) vẫn được cache | Người đã đăng nhập không hưởng cache; dễ sai sót nếu cấu hình policy thiếu điều kiện |

**➡️ Khuyến nghị: Phương án A, kết hợp D làm lớp phòng vệ.** Cụ thể ghi vào SRS:

- `GET /recipes` và `GET /recipes/{slug}` **chỉ trả về `Status == Published`** cho mọi người gọi, không ngoại lệ → response thuần công khai, cache thoải mái.
- Thêm `GET /recipes/mine?status=` (yêu cầu đăng nhập, trả Draft/Archived/Published của chính mình; Admin có thêm `?authorId=`) — **cấm cache**, gắn `Cache-Control: no-store`.
- Cấu hình Output Cache policy bỏ qua mọi request có `Authorization` header (phòng khi sót).
- Bổ sung vào NFR-SEC-006 một dòng nguyên tắc: *"Không bao giờ đặt dữ liệu phụ thuộc danh tính vào cache dùng chung khóa công khai."*

Sửa lại FR-RCP-001 bước 4 và FR-RCP-002 bước 5 cho khớp. Đây là mục nên chốt **trước tiên** trong nhóm 1.

---

### MT-35 🔴 — Máy trạng thái `RecipeStatus` không đóng kín, `PublishedAt` không bao giờ được gán

**Hiện trạng** — tổng hợp mọi chuyển trạng thái được đặc tả:

| Chuyển đổi | Đặc tả ở đâu | Có không? |
| --- | --- | --- |
| (mới tạo) → Draft | FR-RCP-003 | ✅ |
| Draft → Published | FR-RCP-005 `recipe.Publish()` | ✅ |
| Published → Draft | FR-RCP-005 `recipe.Unpublish()` | ✅ |
| ? → Archived | FR-RCP-006 `recipe.Archive()` — **không nói từ trạng thái nào** | ⚠️ mơ hồ |
| Archived → Draft | — | ❌ **không có** |
| Archived → Published | — | ❌ **không có** |

Ba vấn đề lồng nhau:

1. **Archived là hố đen.** Vào được, không ra được. Điều này mâu thuẫn với chính Mô tả của FR-RCP-006 (dòng 909): *"Hữu ích để ẩn recipe cũ... **mà không mất dữ liệu**"* — ẩn vĩnh viễn không lấy lại được thì về mặt người dùng không khác gì xóa. Tên yêu cầu còn ghi rõ "Archive / **Unarchive**" (dòng 905) nhưng phần thân không có Unarchive.
2. **Không rõ archive được từ đâu.** Archive một recipe đang Draft có hợp lệ không? Còn `recipe.Publish()` gọi trên recipe Archived thì sao — FR-RCP-005 chỉ mô tả Draft ↔ Published.
3. **`PublishedAt` không bao giờ được gán.** Chương 7.2 (dòng 1412) ghi `PublishedAt` — *"Set khi Status chuyển sang Published"*, nhưng FR-RCP-005 bước 6 (dòng 891) chỉ ghi: *"Set `Status = Published/Draft`, `UpdatedAt = DateTime.UtcNow`"* — **không có `PublishedAt`**. Cột này sẽ mãi `NULL`, kéo theo index `IDX_Recipe_PublishedAt` vô dụng và `datePublished` trong JSON-LD (NFR-SEO-001) rỗng.

**Phương án cho máy trạng thái**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Archived → Draft** (unarchive đưa về Draft, muốn công khai lại thì publish tiếp) | Máy trạng thái đóng kín, chỉ cần 1 endpoint mới; luôn buộc rà soát lại nội dung trước khi công khai lần nữa — an toàn | Mất 2 bước để đưa recipe cũ trở lại công khai |
| B | Archived → về đúng trạng thái trước khi archive (cần thêm cột `StatusBeforeArchive`) | Khôi phục "đúng như cũ" | Thêm cột chỉ để phục vụ một thao tác hiếm; phức tạp hóa vô ích |
| C | Bỏ hẳn trạng thái Archived, dùng Unpublish + soft delete là đủ | Đơn giản nhất, bớt một trạng thái | Phải sửa enum, FR-RCP-001/002, NFR-SEO-002, Phụ lục C — lan rộng |

**➡️ Khuyến nghị: Phương án A.** Chốt máy trạng thái và vẽ vào SRS:

```text
          publish              archive
  Draft ──────────► Published ──────────► Archived
    ▲   ◄──────────     │                    │
    │     unpublish     │     archive        │
    └───────────────────┴────────────────────┘
                    unarchive
```

Quy tắc kèm theo:
- Archive được từ **cả Draft lẫn Published**; `PATCH /recipes/{id}/unarchive` đưa về **Draft**.
- Gọi thao tác không hợp lệ (ví dụ publish một recipe Archived) → **409 Conflict** kèm mã lỗi mới `RECIPE_INVALID_STATE_TRANSITION`.
- Bổ sung vào FR-RCP-005 bước 6: *"Nếu là lần publish đầu tiên (`PublishedAt == null`) thì gán `PublishedAt = DateTime.UtcNow`; các lần publish sau **không** ghi đè"* — giữ đúng ngữ nghĩa "ngày xuất bản" cho SEO và cho phương án sắp xếp ở MT-23.

---

### MT-36 🟡 — FR-CAT-003 khai báo lỗi 409 mà luồng của nó không bao giờ tạo ra được

**Hiện trạng** — bốn ô trong cùng một bảng FR-CAT-003 không khớp nhau:

| Ô | Nội dung |
| --- | --- |
| Điều kiện tiên quyết (dòng 650) | "2. **Name chưa tồn tại** trong database" |
| HTTP Status Code (dòng 653) | "**409 Conflict – Name đã tồn tại**" |
| Luồng chính, bước 6 (dòng 662) | "Kiểm tra **slug** chưa tồn tại. Nếu trùng, **thêm "-2", "-3",... cho đến khi unique**" — chỉ xử lý slug, **không kiểm tra Name** |
| Luồng thay thế (dòng 671–672) | Chỉ có A1 (403 thiếu quyền) và A2 (422 dữ liệu sai) — **không có nhánh nào dẫn tới 409** |

**Vì sao là vấn đề:** Tạo danh mục trùng tên "Món chính" lần thứ hai sẽ đi hết luồng: slug `mon-chinh` trùng → tự đổi thành `mon-chinh-2` → `AddAsync` → `SaveChangesAsync`. Lúc này PostgreSQL mới chặn vì Chương 7.6 khai báo `Name varchar(100) NOT NULL, **UNIQUE**` → EF ném `DbUpdateException` → `GlobalExceptionMiddleware` trả **500 Internal Server Error**, không phải 409 như đặc tả. Người dùng nhận lỗi hệ thống thay vì lỗi nghiệp vụ, và log bị nhiễu bởi exception không đáng có.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Thêm bước kiểm tra Name trùng trước khi tạo** → ném `ConflictException` → 409 + mã `CATEGORY_NAME_EXISTS` (mã này Phụ lục B đã có sẵn) | Khớp đúng điều kiện tiên quyết, status code và Phụ lục B đã viết; thông báo lỗi rõ ràng cho Admin | Vẫn còn khe hở đua (race) nếu hai request đồng thời — cần bắt thêm `DbUpdateException` làm lớp hai |
| B | Bỏ ràng buộc UNIQUE trên Name, cho phép trùng tên (phân biệt bằng slug) | Không cần kiểm tra thêm; đúng tinh thần bước 6 vốn đã tự sinh suffix | Hai danh mục cùng tên trên giao diện → người dùng không phân biệt được; phải sửa Chương 7.6 và bỏ 409 khỏi FR + Phụ lục B |
| C | Giữ nguyên, dựa vào UNIQUE của DB và dịch `DbUpdateException` thành 409 | Ít code nhất | Dựa vào exception cho luồng nghiệp vụ bình thường — khó đọc, khó log, và phải dò mã lỗi PostgreSQL `23505` |

**➡️ Khuyến nghị: Phương án A** (kèm bắt `23505` làm lớp phòng vệ thứ hai cho tình huống đua). Bổ sung nhánh **A3 – Name đã tồn tại: HTTP 409 với `CATEGORY_NAME_EXISTS`** vào FR-CAT-003. Lưu ý FR-CAT-004 (cập nhật danh mục) cũng có cùng lỗ hổng: đổi tên thành tên đã có sẽ 500 — cần bổ sung kiểm tra tương tự.

---

### MT-37 🟡 — Giới hạn độ dài lệch nhau giữa validator, schema và yêu cầu SEO

**Hiện trạng**

| Trường | Validator (Chương 3) | Schema (Chương 7) | Khớp? |
| --- | --- | --- | --- |
| `Category.Name` | **2–50** ký tự (FR-CAT-003 bước 4, dòng 660) | `varchar(**100**)` (dòng 1475) | ❌ lệch 2× |
| `RecipeIngredient.Name` | **1–100** ký tự (FR-RCP-009, dòng 1018) | `varchar(**200**)` (dòng 1451) | ❌ lệch 2× |
| `Recipe.Description` | **không có validator nào** (FR-RCP-003 bước 4 chỉ kiểm tra title/time/servings) | `text`, ghi chú "≤ 2000 ký tự" (dòng 1402) | ❌ giới hạn 2000 **không ai thực thi** |
| `Recipe.Title` | 5–200 (dòng 818) | `varchar(200)` | ✅ |
| `RecipeStep.Description` | ≤ 2000 (dòng 1058) | `text` | ✅ (validator gánh) |

Thêm hai xung đột với yêu cầu SEO:

- **NFR-SEO-002** yêu cầu `<title>` = `"{Recipe Name} | Culinary Blog"` **≤ 60 ký tự**. Hậu tố `" | Culinary Blog"` đã chiếm 16 ký tự → tên công thức chỉ còn **≤ 44 ký tự**, trong khi `Title` cho phép tới **200**. Một công thức đặt tên dài là NFR-SEO-002 tự động fail.
- **NFR-SEO-002** yêu cầu `<meta description>` **150–160 ký tự**, mà Chương 7.2 lại ghi `Description` (≤ 2000 ký tự) chính là nguồn cho "SEO meta description". Lệch hơn 12 lần.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Lập một bảng "Giới hạn dữ liệu chuẩn" duy nhất** trong Chương 7, mọi validator và cột DB đều tham chiếu về đó; phần SEO dùng **truncate khi render** chứ không ràng buộc dữ liệu | Một nguồn sự thật; không bắt người dùng đếm chữ để chiều Google; sửa giới hạn chỉ phải sửa một chỗ | Phải rà lại toàn bộ validator một lượt |
| B | Siết giới hạn dữ liệu xuống đúng mức SEO (Title ≤ 44, Description ≤ 160) | Không cần truncate, chắc chắn pass SEO | Ràng buộc vô lý với người dùng — tên món ăn kèm mô tả ngắn rất dễ vượt 44 ký tự; Description dùng cho cả card preview nên quá ngắn |
| C | Giữ nguyên, coi đây là chuyện của FE | Không sửa gì | Hai chương tiếp tục mâu thuẫn; QA không biết lấy con số nào làm chuẩn để viết test |

**➡️ Khuyến nghị: Phương án A.** Chốt bảng giới hạn: `Category.Name` **2–100**, `RecipeIngredient.Name` **1–200**, `Recipe.Description` **20–2000** (bổ sung validator còn thiếu), `Recipe.Title` **5–200**. Về SEO, ghi rõ trong NFR-SEO-002: *"`<title>` được cắt ở 60 ký tự khi render (cắt theo từ, thêm `…`); `<meta description>` lấy 160 ký tự đầu của Description"* — chuyển yêu cầu từ ràng buộc dữ liệu thành quy tắc hiển thị, đúng chỗ của nó.

> **Nguyên tắc chung nên ghi vào SRS:** validator ở Application và độ dài cột ở DB phải **bằng nhau**, không phải "validator chặt hơn cho an toàn" — vì khi lệch, không ai biết con số nào mới là yêu cầu thật, và dữ liệu nhập qua seeding/migration sẽ lọt qua validator.

---

### MT-38 🟡 — Rate limit theo IP nhưng đứng sau Nginx mà không cấu hình `ForwardedHeaders`

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| NFR-SEC-003 (dòng 1190) | "Giới hạn yêu cầu **theo IP**: Auth 10 req/phút/IP; API chung 100 req/phút/IP; Upload 5 req/phút/IP" |
| Mục 6.1 + 6.5 | Mọi request đi qua **Nginx reverse proxy** rồi mới tới API (`api:8080`) |
| Chương 7.8 (dòng 1506) | `CreatedByIp varchar(45)` — "IP address tạo token. **Lưu để audit**" |
| Toàn tài liệu | **Không có chỗ nào** đề cập `UseForwardedHeaders`, `X-Forwarded-For`, `X-Real-IP` hay `KnownProxies` |

**Vì sao là vấn đề:** Khi API đứng sau proxy, `HttpContext.Connection.RemoteIpAddress` trả về **IP của container Nginx**, giống hệt nhau cho mọi người dùng. Hậu quả:

1. **Rate limit biến thành giới hạn toàn hệ thống**: tổng cộng 10 request/phút tới `/auth/*` cho **tất cả** người dùng cộng lại. Chỉ cần vài người đăng nhập cùng lúc là cả hệ thống nhận 429 — biến một biện pháp bảo mật thành lỗi từ chối dịch vụ tự gây ra.
2. **`CreatedByIp` mất giá trị audit**: mọi refresh token đều ghi cùng một IP nội bộ (ví dụ `172.18.0.5`), không truy vết được ai.

Đây là mâu thuẫn giữa **NFR-SEC-003 + Chương 7.8** và **kiến trúc triển khai ở Chương 6** — hai bên đều đúng khi đọc riêng, chỉ sai khi ghép lại.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Bật `UseForwardedHeaders`** ở API (`ForwardedHeaders.XForwardedFor \| XForwardedProto`, khai báo `KnownProxies` là dải mạng Docker); Nginx set `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for` | Khôi phục đúng IP thật cho cả rate limit lẫn audit; đồng thời sửa luôn việc nhận biết HTTPS (cần cho HSTS/redirect ở NFR-SEC-005) | Phải khai báo `KnownProxies` đúng, nếu không client có thể **giả mạo** `X-Forwarded-For` để né rate limit |
| B | Đưa rate limiting lên Nginx (`limit_req_zone $binary_remote_addr`) | Nginx thấy IP thật ngay, chặn sớm trước khi tốn tài nguyên API | Mâu thuẫn với NFR-SEC-003 vốn chỉ định "ASP.NET Core Rate Limiting middleware"; khó trả RFC 7807 và header `X-RateLimit-*` như Chương 5.2 yêu cầu |
| C | Rate limit theo **user/token** thay vì IP | Công bằng hơn cho người dùng hợp lệ | Không bảo vệ được chính các endpoint cần nhất — `/auth/login` và `/auth/register` vốn chưa có danh tính |

**➡️ Khuyến nghị: Phương án A** (có thể thêm B làm lớp phòng vệ ngoài cùng). Bổ sung vào NFR-SEC-003 một dòng: *"API PHẢI bật `ForwardedHeaders` với `KnownProxies` giới hạn ở dải mạng nội bộ Docker; rate limit và `CreatedByIp` lấy IP từ `X-Forwarded-For`."* Và bổ sung `proxy_set_header` tương ứng vào phần cấu hình Nginx ở mục 6.5.

---

### MT-39 🟡 — Hangfire Dashboard dành cho Admin nhưng không thể đăng nhập bằng JWT từ trình duyệt

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| Mục 2.3 (dòng 259) | Quyền Admin: "Truy cập **Hangfire Dashboard**" |
| Mục 3.6 (dòng 1136) | "Dashboard quản lý jobs tại `/hangfire` (**chỉ Admin**)" |
| Mục 5.3 (dòng 1289) | "Dashboard: `/hangfire` (Admin only, **policy-protected**)" |
| NFR-SEC-002 + CONS-004 | Xác thực là **JWT Bearer stateless** |
| Mục 5.2 (dòng 1277) | "Refresh token: trong request body (**không dùng cookie** để tránh CSRF)" |

**Vì sao là vấn đề:** Hangfire Dashboard là một **trang HTML mở trực tiếp trong trình duyệt**. Khi Admin gõ `https://domain.com/hangfire`, trình duyệt gửi một request điều hướng thông thường — **không có cách nào đính kèm header `Authorization: Bearer ...`**. Mà SRS lại loại bỏ cookie. Kết quả: `IDashboardAuthorizationFilter` luôn thấy người dùng ẩn danh → **Admin không bao giờ vào được dashboard**, dù đây là quyền đã được liệt kê ở mục 2.3.

Cùng một vấn đề áp dụng cho `/scalar` (mục 2.4.2) nếu sau này muốn bảo vệ nó.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Không expose `/hangfire` ra ngoài**: Nginx chặn đường dẫn này; Admin truy cập qua SSH tunnel hoặc chỉ mở ở môi trường dev | An toàn nhất, không cần thêm cơ chế xác thực nào; dashboard vốn là công cụ vận hành, không phải tính năng người dùng | Admin phải có quyền truy cập máy chủ; bớt tiện khi demo |
| B | Bảo vệ bằng **HTTP Basic Auth ở tầng Nginx** (`auth_basic` + `htpasswd`) | Đơn giản, hiệu quả ngay, không đụng gì tới JWT của ứng dụng; vẫn mở được từ trình duyệt để demo | Thêm một bộ thông tin đăng nhập tách rời tài khoản Admin của hệ thống — phải quản lý riêng |
| C | Cấp **cookie riêng cho dashboard**: thêm endpoint `POST /hangfire/login` nhận JWT rồi set cookie `HttpOnly` ngắn hạn | Dùng chung tài khoản Admin của hệ thống | Mở lại cánh cửa cookie mà mục 5.2 vừa đóng; phải xử lý CSRF; thêm code chỉ để phục vụ một trang quản trị |

**➡️ Khuyến nghị: Phương án B** cho môi trường demo/nộp bài (mở được từ trình duyệt, cấu hình 3 dòng trong Nginx), **A** cho production thật. Sửa mục 3.6 và 5.3 thành: *"Dashboard `/hangfire` được bảo vệ bằng HTTP Basic Auth tại tầng Nginx, không dùng JWT (JWT Bearer không khả dụng cho điều hướng trình duyệt)."* Đồng thời sửa mục 2.3 để quyền này không gây hiểu nhầm là phân quyền ở tầng ứng dụng.

---

### MT-40 🟡 — Mức ưu tiên MoSCoW: 11 FR bị bỏ trống, và nhiều FR mức "S" lại đỡ cho thành phần mức "M"

**Hiện trạng – vấn đề 1: thiếu mức ưu tiên**

Đầu Chương 3 (dòng 346) tuyên bố: *"Mỗi FR được mô tả theo template chuẩn bao gồm: ... **Mức ưu tiên (MoSCoW)** ..."*. Nhưng chỉ **23/34 FR** có mức ưu tiên. **11 FR không có**: FR-SRCH-002, 003, 004 (bảng tóm tắt), FR-FILE-001, 002, FR-JOB-001, 002, 003, FR-OBS-001, 002, 003 — tất cả đều được trình bày dạng bảng gộp thay vì template đầy đủ.

Ngoài ra quy ước MoSCoW định nghĩa 4 mức M/S/C/W nhưng **không FR nào dùng C hoặc W** — hai mức này thừa.

**Hiện trạng – vấn đề 2 (nghiêm trọng hơn): "Should Have" đỡ cho "Must Have"**

| FR mức **S** | Nhưng lại là điều kiện cần của… |
| --- | --- |
| **FR-RCP-006** Archive (S) | FR-RCP-001 (M) và FR-RCP-002 (M) đều đặc tả hành vi cho `Status == Archived`; NFR-SEO-002 (SEO, ưu tiên Cao) quy định `noindex` cho archived. Cắt FR-RCP-006 → trạng thái Archived không bao giờ tồn tại → các nhánh xử lý trong FR mức M thành mã chết |
| **FR-AUTH-003** Google OAuth (S) | Mục 1.2.2 liệt kê Google OAuth là **1 trong 6 tính năng cốt lõi** của sản phẩm; mục 5.1 bắt `/auth/login` phải có nút Google; mục 2.6.2 dành hẳn một dòng kế hoạch dự phòng cho nó |
| **FR-AUTH-006** Xem hồ sơ (S) | `GET /auth/me` là **cách duy nhất** FE lấy được `roles` của người dùng. Mọi route `/dashboard*` ở mục 5.1 đều "Bắt buộc (Author/Admin)" — không có `/auth/me` thì FE không biết ai là Admin để hiện menu quản lý danh mục |

**Vì sao là vấn đề:** MoSCoW tồn tại để trả lời câu hỏi *"nếu hết thời gian thì cắt cái gì?"*. Với bảng hiện tại, cắt đúng những mục ghi "Should Have" sẽ **làm hỏng các tính năng ghi "Must Have"** — tức là bảng ưu tiên đang hướng dẫn sai. Với đồ án 6 buổi, đây là rủi ro thật chứ không phải lý thuyết.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Nâng FR-RCP-006, FR-AUTH-003, FR-AUTH-006 lên M**; gán MoSCoW cho 11 FR còn thiếu | Bảng ưu tiên trở nên dùng được thật; phản ánh đúng phụ thuộc | Số FR "Must" tăng → phải cam kết làm nhiều hơn |
| B | Giữ mức S, nhưng sửa các FR mức M để **không phụ thuộc** vào chúng (bỏ nhánh Archived khỏi FR-RCP-001/002, bỏ nút Google khỏi mục 5.1, FE tự decode JWT lấy roles) | Giữ được phạm vi tối thiểu nhỏ gọn | Sửa lan nhiều chỗ; FE tự decode JWT là thói quen xấu, mất khả năng thu hồi quyền tức thời |
| C | Bổ sung cột **"Phụ thuộc"** vào mỗi FR và quy tắc: *"FR mức M không được phụ thuộc FR mức thấp hơn"*, rồi rà lại một lượt | Chặt chẽ nhất về mặt phương pháp, đúng tinh thần IEEE 830 về truy vết yêu cầu | Tốn công nhất; hơi nặng so với quy mô môn học |

**➡️ Khuyến nghị: Phương án A**, và bổ sung một câu quy tắc vào mục quy ước MoSCoW: *"Một FR mức Must Have không được phụ thuộc vào FR có mức ưu tiên thấp hơn."* Gán cho 11 FR còn thiếu: FR-FILE-001/002 = **M** (FR-RCP-008 mức M gọi thẳng vào chúng), FR-OBS-001 = **M** (NFR-REL-001 cần readiness probe), FR-SRCH-002/003/004 = **M** (đã tích hợp sẵn trong FR-RCP-001 mức M), FR-JOB-001/002 = **S**, FR-JOB-003 = **C** (nhất là khi theo MT-28 thì nên chuyển sang Next.js), FR-OBS-002 = **M** (CONS-010 bắt buộc logging), FR-OBS-003 = **C**.

Riêng cặp mức **C/W**: nếu không dùng thì nên bỏ khỏi quy ước, hoặc dùng W để ghi lại chính danh sách "ngoài phạm vi" ở mục 1.2.3 — như vậy mục 1.2.3 và MoSCoW mới nối được với nhau.

---

### MT-41 🟢 — Nhóm bất nhất nhỏ đợt 3

| # | Nội dung | Vị trí | Đề xuất |
| --- | --- | --- | --- |
| 41.1 | FR-CAT-002 ghi Điều kiện tiên quyết "**Không yêu cầu xác thực**", nhưng Mô tả và bước 4 lại đọc `currentUser` để lọc Draft → thực chất là **optional authentication** | dòng 621 vs 620, 631 | Sửa thành "Xác thực **tùy chọn**: nếu có JWT hợp lệ thì bổ sung Draft của chính người gọi". Lưu ý điều này kéo theo **MT-34** — endpoint có cache mà phụ thuộc danh tính |
| 41.2 | FR-CAT-002 chỉ cho Author thấy thêm **Draft**, trong khi FR-RCP-001 cho thấy **Draft/Archived**; FR-CAT-002 cũng **không nói Admin** thấy gì | dòng 620 vs 745 | Thống nhất một quy tắc hiển thị dùng chung cho mọi endpoint danh sách (nên gộp vào phương án của MT-34) |
| 41.3 | FR-RCP-005 mô tả "Khi publish, Recipe... **được đưa vào index tìm kiếm**" — sai cơ chế: `SearchVector` do trigger/generated column cập nhật theo Title/Description, **không phụ thuộc Status**; việc lọc Published nằm ở câu truy vấn | dòng 878 vs 1411 | Sửa thành: "Khi publish, Recipe bắt đầu xuất hiện trong kết quả tìm kiếm (truy vấn chỉ lấy `Status == Published`)" |
| 41.4 | FR-RCP-008 bước 1 nói form-data chỉ chứa field `"file"`, nhưng bước 6 lại dùng `altText` | dòng 978 vs 983 | Bổ sung `altText?`, `isPrimary?` vào mô tả form-data (Chương 8.4 đã có) |
| 41.5 | FR-RCP-008 nhánh A4 trả **503** khi MinIO lỗi, nhưng ô "HTTP Status Code trả về" của chính FR đó **không liệt kê 503** | dòng 1006 vs 972 | Bổ sung 503 vào ô status code |
| 41.6 | FR-RCP-008 bước 15: khi xóa ảnh primary thì "tự động đặt **ảnh đầu tiên** còn lại làm primary" — không nói "đầu tiên" theo tiêu chí nào | dòng 998 | Ghi rõ: theo `OrderIndex` tăng dần, nếu bằng nhau thì theo `CreatedAt` |
| 41.7 | FR-RCP-009 Điều kiện tiên quyết đòi "**Unit không rỗng**", trong khi Chương 7.4 để `Unit` **NULL** và Chương 8.6 ghi `unit?` | dòng 1018 vs 1453, 1576 | Thống nhất `Unit` **tùy chọn** (xử lý cùng **MT-04**) |
| 41.8 | Mục 1.3 định nghĩa **DXA** — "đơn vị pixel trong **OOXML** (1 inch = 1440 DXA)" — hoàn toàn không liên quan tới dự án web và không xuất hiện ở bất kỳ đâu khác; dấu hiệu sót lại từ template tài liệu khác. Tương tự, **SSG** được định nghĩa nhưng mục 5.1 chỉ dùng ISR/SSR/CSR | dòng 143, 146 | Bỏ DXA khỏi mục 1.3; giữ SSG hoặc bổ sung chú thích "không dùng trong v1.0" |
| 41.9 | Mục 2.3 cho Admin quyền "**Xem structured logs**", nhưng mục 6.5 ghi Seq là "**Dev only — không deploy production**" → ở production Admin không có công cụ nào để xem | dòng 259 vs 1372 | Hoặc deploy Seq cho production, hoặc sửa mục 2.3 thành "xem logs qua công cụ vận hành (ngoài phạm vi ứng dụng)" |
| 41.10 | Phụ lục C có mục tự tham chiếu "**CQRS** — Xem Command Query Responsibility Segregation" trong khi mục đầy đủ đã nằm ngay phía trên | Phụ lục C | Bỏ dòng trùng |

---

> **Từ đây là các mục phát hiện ở lượt rà soát thứ 4 (19/09/2026).** Khác hẳn ba lượt đầu, lượt này **không đọc tài liệu với tài liệu** mà đối chiếu SRS v1.1.0 với **hệ thống đã chạy thật**: code trên nhánh `main`, `docker-compose.yml`, `nginx.conf`, image Docker được dùng, và các sự cố được ghi trong `BAO_CAO_BUOI_2.md`. Mỗi mục được phân loại: **(a)** lỗi kỹ thuật trong chính v1.1.0 — đặc tả mà làm đúng từng chữ thì hệ thống hỏng; **(b)** yêu cầu mồ côi còn sót sau CR-2026; **(c)** SRS mâu thuẫn với một thiết kế đã được kiểm chứng tốt hơn trong code. Tất cả đã được áp dụng vào **SRS v1.2.0** (CR-2026-02); vị trí sửa ghi ở cuối mỗi mục và ở SRS Phụ lục E.

---

### MT-42 🟡 (c) — Hai hình dạng response cho danh sách phân trang

**Hiện trạng**

| Vị trí (SRS v1.1.0) | Quy định |
| --- | --- |
| §5.2 "Response Format" | `{ "data": {...}, "meta": { "page":1, "pageSize":10, "total":100 } }` — kèm `pageSize: 10` |
| Chương 8, quy ước Pagination | `{ "data":[], "meta":{ "page", "pageSize", "total", "totalPages" } }` |
| FR-RCP-001, FR-SRCH-004 | `PagedResult<RecipeSummaryDto>` với `totalCount`, `totalPages`, **`hasNextPage`, `hasPreviousPage`** |
| Code Buổi 2 + Frontend | Dùng `PagedResult { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }` (`BAO_CAO_BUOI_2.md` §4.9) |

**Vì sao là vấn đề:** Mâu thuẫn này có từ v1.0.0 nhưng **sót khỏi cả 41 mục của CR-2026**. Frontend đọc §5.2 sẽ tìm `response.data`, Backend làm theo FR trả `response.items` — mọi màn hình danh sách hiển thị rỗng mà không có lỗi nào. Thêm vào đó `pageSize: 10` ở §5.2 lệch với giá trị mặc định 12 đã chốt ở MT-20.13.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`PagedResult` phẳng** (theo FR và code) | FR-SRCH-004 đòi đúng `hasNextPage`/`hasPreviousPage` mà `{ data, meta }` không có; code + FE đã dùng — không phá vỡ gì | Không có lớp vỏ chung cho mọi response |
| B | `{ data, meta }` (theo §5.2) | Một lớp vỏ thống nhất cho mọi response | Phải sửa mọi endpoint danh sách và mọi màn hình FE; vẫn phải thêm `hasNextPage` vào `meta` — tức là vẫn sửa SRS |
| C | Bọc `PagedResult` trong `{ data }` | Có lớp vỏ | Lớp vỏ không mang thông tin gì thêm; chỉ tăng độ sâu truy cập |

**➡️ Quyết định (v1.2.0): Phương án A.** Danh sách trả `PagedResult<T>`, đối tượng đơn trả thẳng DTO. Sửa: SRS §5.2, quy ước Chương 8.

---

### MT-43 🟡 (b) — Ba FR chỉ nhận URL ảnh nhưng không có endpoint nào để upload ảnh

**Hiện trạng**

| Vị trí | Quy định |
| --- | --- |
| FR-AUTH-007 | `PATCH /auth/me` nhận `avatarUrl` — **một URL** |
| FR-CAT-003/004 | Danh mục nhận `imageUrl` — **một URL** |
| FR-RCP-010 | Bước nấu nhận `imageUrl` — **một URL** |
| FR-FILE-001/002 | Chỉ đặc tả tầng service `IFileStorageService`, **không có endpoint HTTP** |
| Chương 8 v1.1.0 | Endpoint upload duy nhất là `POST /recipes/{id}/images` — gắn chặt với công thức |
| Code Buổi 2 | Đã tự bổ sung `POST /api/v1/files/upload`, `DELETE /api/v1/files/{**fileId}` — **ngoài SRS** (`BAO_CAO_BUOI_2.md` §4.14) |

**Vì sao là vấn đề:** Người dùng **không có cách hợp lệ nào** để có một URL ảnh đưa vào `avatarUrl` — tính năng đổi avatar không thể hoàn thành theo đúng SRS. Nếu để code tự thêm endpoint ngoài SRS, tài liệu không còn là nguồn sự thật.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Chính thức hóa endpoint tổng quát `/files/upload`** | Mọi quy tắc bảo mật tệp (5MB, magic bytes, tên GUID, phạm vi thư mục theo người dùng) nằm **ở một chỗ**; đã chạy từ Buổi 2 | Có thể sinh tệp mồ côi (upload rồi không gán vào đâu) |
| B | Ba endpoint con `/auth/me/avatar`, `/categories/{id}/image`, `/recipes/{id}/steps/{stepId}/image` | Gắn chặt ngữ cảnh, không có tệp mồ côi | Lặp quy tắc bảo mật ba lần; thêm ba command/validator |
| C | Cho nhập URL ảnh ngoài tùy ý | Không cần upload | Biến hệ thống thành nơi nhúng nội dung tùy ý; ảnh ngoài có thể chết bất cứ lúc nào |

**➡️ Quyết định (v1.2.0): Phương án A.** Endpoint gắn vào FR-FILE-001/002, thêm Chương 8.8. Tệp mồ côi là đánh đổi được chấp nhận (ghi ở §6 của tài liệu này). Sửa: SRS §3.5, Chương 8.8.

---

### MT-44 🟡 (b) — Route quản trị người dùng không có API danh sách

**Hiện trạng:** SRS §5.1 có route `/dashboard/users` — *"Khóa / mở khóa tài khoản người dùng (FR-AUTH-008)"* — nhưng Chương 8 chỉ có `PATCH /users/{id}/status`: muốn khóa ai thì phải **biết trước `id`** của người đó.

**Vì sao là vấn đề:** Admin không có cách nào tìm ra người cần khóa từ giao diện; màn hình `/dashboard/users` không dựng được. Đây đúng là loại "yêu cầu mồ côi" của lượt 2, sót lại vì FR-AUTH-008 được viết mới trong CR-2026.

**Phương án:** (A) **Thêm `GET /users` phân trang vào chính FR-AUTH-008**; (B) tách FR mới "Xem danh sách người dùng"; (C) Admin nhập email → tra từng người.

**➡️ Quyết định (v1.2.0): Phương án A.** Endpoint chỉ phục vụ đúng mục đích của FR-AUTH-008 (tìm người để khóa) nên không đáng tách FR; phương án C bất tiện và vẫn cần một endpoint mới. DTO **không** chứa `PasswordHash`, `SecurityStamp`, `UserName`; không cache. Sửa: FR-AUTH-008, Chương 8.1, §5.1.

---

### MT-45 🟢 (b) — Quản lý phiên đăng nhập có trong lộ trình nhưng không có FR

**Hiện trạng:** Lộ trình phát triển (Buổi 7, Dev 1) yêu cầu *"Quản lý phiên làm việc nâng cao (Force revoke, Session Management UI)"*. SRS v1.1.0 **không có FR và endpoint** nào cho việc liệt kê/thu hồi phiên; riêng "force revoke" đã có sẵn trong FR-AUTH-008 (khóa tài khoản ⇒ thu hồi mọi refresh token).

**Vì sao là vấn đề:** Hiện thực thẳng theo lộ trình thì hệ thống có 3 endpoint không truy vết được về yêu cầu nào — phá vỡ nguyên tắc SRS là nguồn sự thật. Bỏ hẳn thì trái lộ trình đã giao.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **FR mới FR-AUTH-009, mức C** | Truy vết đầy đủ; mức C bảo đảm không FR mức M/S nào phụ thuộc vào nó (quy tắc MT-40) | Tổng FR 36 → 37 |
| B | Gộp vào FR-AUTH-005 (Đăng xuất) | Không tăng số FR | Phạm vi khác nhau: FR-AUTH-005 thu hồi phiên **đang dùng**, còn đây là **mọi phiên trên mọi thiết bị** |
| C | Bỏ khỏi phạm vi | Gọn | Trái lộ trình; người dùng không tự thu hồi được phiên đã đăng nhập ở máy công cộng |

**➡️ Quyết định (v1.2.0): Phương án A.** Thêm claim **`sid`** vào access token để nhận diện "phiên hiện tại" mà client không phải gửi thêm dữ liệu; thu hồi phiên của người khác trả **404** (không phải 403) để endpoint không bị dùng dò id. Sửa: FR-AUTH-009 (mới), NFR-SEC-002, Chương 8.1, §2.2, §2.3, §5.1.

---

### MT-46 🟢 (c) — Mã lỗi đang dùng trong code nhưng thiếu trong Phụ lục B

**Hiện trạng:** Code Buổi 2 (`ErrorCodes.cs`) dùng `FILE_FORBIDDEN` (403 — xóa tệp của người khác), `FILE_STORAGE_UNAVAILABLE` (503 — MinIO lỗi, đúng tình huống FR-RCP-008 A4) và `INTERNAL_ERROR` (500). Cả ba **không có** trong Phụ lục B (24 mã). Code còn có `AUTH_USERNAME_EXISTS`.

**Vì sao là vấn đề:** Frontend nhận những giá trị `type` không có trong tài liệu — trái mục đích của Phụ lục B. Riêng FR-RCP-008 A4 có status 503 nhưng không có mã, nên Frontend không phân biệt được "MinIO lỗi" với "server quá tải".

**➡️ Quyết định (v1.2.0):** Bổ sung 3 mã (24 → **27**). **Không** bổ sung `AUTH_USERNAME_EXISTS`: mã này chỉ tồn tại vì form đăng ký cũ bắt người dùng tự nhập `userName` — trái MT-12 — và sẽ biến mất khi code được sửa (nợ D-1, §4). Sửa: Phụ lục A, Phụ lục B, FR-RCP-008.

---

### MT-47 🟡 (c) — Hangfire "in-process" (SRS) và container worker riêng (hệ thống thật)

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| SRS §3.6, §5.3, §2.1, §2.6.2 (kế thừa nguyên văn v1.0.0) | *"Hangfire chạy **in-process** trong .NET API"* |
| SRS §6.5 v1.1.0 | Không có service Hangfire nào trong bảng |
| Code Buổi 2 | Container **`hangfire`** riêng, dùng chung image với API (`Hangfire__WorkerOnly=true`); container `api` đặt `Hangfire__ServerEnabled=false` — chỉ enqueue |

**Vì sao là vấn đề:** Hai mô tả khác nhau cho cùng một phần hạ tầng, và 41 mục của CR-2026 **chưa từng phân tích** điểm này. Người đọc SRS để "sửa cho đúng" sẽ gộp worker vào API và đánh mất những lợi ích dưới đây.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Worker riêng** (như code) | Resize ảnh (ImageSharp, nặng CPU) **không tranh CPU** với request của người đọc → bảo vệ p95 ≤ 500ms; API và job scale độc lập; chi phí đã trả xong ở Buổi 2 (khóa DataProtection dùng chung qua volume, chờ DB sẵn sàng trước khi chạy) | Thêm một container |
| B | In-process (như SRS cũ) | Ít container hơn | Mỗi lượt upload ảnh làm chậm mọi request; `--scale api=3` nhân ba số job server |

**➡️ Quyết định (v1.2.0): Phương án A.** Sửa: SRS §2.1, §2.6.2, §3.6, §5.3, §6.1, §6.5, NFR-SCALE-001, Phụ lục C.

---

### MT-48 🟢 (b) — Sitemap cần toàn bộ slug nhưng API chỉ trả tối đa 50 bản ghi

**Hiện trạng:** NFR-SEO-003 (v1.1.0) giao `app/sitemap.ts` của Next.js *"gọi API lấy danh sách slug + lastmod"* nhưng không chỉ định endpoint; API danh sách duy nhất (`GET /recipes`) giới hạn `pageSize ≤ 50`.

**Vì sao là vấn đề:** Với quy mô 10.000 công thức, mỗi lần tái sinh sitemap phải gọi khoảng 200 request; và dữ liệu có thể **xê dịch giữa các trang** trong lúc duyệt (công thức mới xuất bản chen vào) làm lặp hoặc sót slug.

**Phương án:** (A) **Endpoint gọn `GET /recipes/sitemap`** chỉ trả `{ slug, updatedAt }`, cache 1 giờ; (B) duyệt `GET /recipes` theo trang; (C) nới `pageSize` tối đa cho mọi người gọi.

**➡️ Quyết định (v1.2.0): Phương án A.** Phương án C bị loại vì mở đường cho request cực lớn từ bất kỳ ai. `sitemap` được thêm vào danh sách slug dành riêng (MT-53). Sửa: NFR-SEO-003, Chương 8.3.

---

### MT-49 🟡 (b) — Hứa "có thể khôi phục" nhưng không có chức năng khôi phục

**Hiện trạng:** NFR-REL-003 và Phụ lục C (định nghĩa Soft Delete) ghi dữ liệu xóa mềm *"có thể khôi phục"*; FR-RCP-007 ghi *"Dữ liệu vẫn có thể khôi phục đến khi job dọn chạy"*. **Không có FR hay endpoint nào** để khôi phục.

**Vì sao là vấn đề:** Lời hứa không có chủ thể thực thi — QA không biết nghiệm thu "có thể khôi phục" bằng cách nào; người dùng xóa nhầm không có đường tự cứu.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Làm rõ: khôi phục do quản trị viên vận hành trên DB trong 30 ngày; chức năng tự phục vụ ngoài phạm vi** | Trung thực với phạm vi; không thêm FR khi không có trong lộ trình | Người dùng không tự khôi phục được |
| B | Thêm FR "Thùng rác" + `GET /recipes/trash`, `PATCH /recipes/{id}/restore` | Tính năng hoàn chỉnh | Thêm FR + 2 endpoint + UI ngoài lộ trình 8 buổi |
| C | Bỏ chữ "có thể khôi phục" | Gọn | Mất giá trị thật của soft delete (dữ liệu vẫn nằm đó 30 ngày) |

**➡️ Quyết định (v1.2.0): Phương án A.** Chức năng tự phục vụ được ghi vào danh sách "ngoài phạm vi" (§1.2.3) để phiên bản sau có điểm bắt đầu. Sửa: §1.2.3, NFR-REL-003, Phụ lục C.

---

### MT-50 🔴 (a) — Cấu hình Nginx của SRS tái tạo lỗi 502 đã sửa ở Buổi 2

**Đây là mục nặng nhất của lượt 4.** Nó không phải "hai chỗ ghi khác nhau" mà là **đặc tả sai sẽ gây sự cố sản xuất**, và sự cố đó đã từng xảy ra thật.

**Hiện trạng**

| Vị trí | Nội dung |
| --- | --- |
| SRS v1.1.0 §6.5 (khối cấu hình Nginx), §6.1, NFR-SCALE-003, Phụ lục D (dòng MT-32) | `upstream api_pool { server api:8080; }` + `proxy_pass http://api_pool;` |
| `BAO_CAO_BUOI_2.md` §5.1, lỗi #2 | Nginx trả **502** cho toàn bộ `/api/*` sau khi container `api` được tạo lại: *"`upstream { server api:8080; }` chỉ phân giải DNS lúc Nginx khởi động → giữ IP cũ"* |
| Code (commit `064f582`) | Đã sửa bằng `resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến; kiểm chứng: api đổi IP `172.30.2.3` → `172.30.2.2`, Nginx **không restart** vẫn trả 200 |

**Vì sao là vấn đề:** Nginx bản open-source phân giải tên trong khối `upstream` **một lần duy nhất lúc khởi động**. Trong Docker, IP container thay đổi mỗi khi container được tạo lại (deploy, restart do lỗi, scale). Làm đúng SRS v1.1.0 nghĩa là: mỗi lần deploy API, trang web chết cho tới khi ai đó nhớ ra restart Nginx. SRS v1.1.0 đã viết cấu hình này **sau khi** code đã sửa lỗi — tức là tài liệu đẩy hệ thống lùi về trạng thái lỗi.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **`resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến** (như code) | Hết 502 khi tạo lại container; khi `--scale api=3`, Docker DNS trả 3 bản ghi A và Nginx tự luân phiên; replica mới được nhận **trong ≤ 10 giây không cần reload** — tốt hơn `upstream` ở chính mục tiêu scale ngang | Không dùng được các tính năng chỉ có trong khối `upstream` (ví dụ `least_conn`) |
| B | `upstream { server api:8080 resolve; }` | Giữ được khối `upstream` | Tham số `resolve` chỉ có ở Nginx Plus hoặc bản open-source rất mới — không bảo đảm với `nginx:alpine` đang dùng |
| C | Giữ `upstream`, restart Nginx sau mỗi lần deploy | Không sửa cấu hình | Quy trình thủ công dễ quên; vẫn chết khi container tự restart do lỗi |

**➡️ Quyết định (v1.2.0): Phương án A.** Khối cấu hình trong SRS §6.5 được viết lại theo cấu hình đang chạy, kèm chú thích tại chỗ giải thích vì sao **không** dùng `upstream`, để người sau không "sửa cho gọn" và tái tạo lỗi. Sửa: §6.1, §6.5, NFR-SCALE-003, Phụ lục D.

---

### MT-51 🟡 (a) — Lệnh healthcheck trong SRS không chạy được trong image thật

**Hiện trạng**

| Service | SRS v1.1.0 §6.5 | Thực tế |
| --- | --- | --- |
| `api` | `curl -f http://localhost:8080/health/ready` | Image `mcr.microsoft.com/dotnet/aspnet:10.0` **không có `curl`** |
| `frontend` | `curl -f http://localhost:3000` | `node:22-alpine` **không có `curl`**, chỉ có `wget` của busybox |
| `minio` | `curl -f http://localhost:9000/minio/health/live` | Image `minio/minio` **không có `curl`**; Buổi 2 dùng `mc ready local` |
| `postgres` | `pg_isready -U $POSTGRES_USER` | Không có `-h` → kiểm tra qua **unix socket**, báo "ready" khi Postgres **chưa mở cổng TCP** — nguyên nhân lỗi `57P03 the database system is starting up` Buổi 2 đã gặp (§5.1 lỗi #1); Buổi 2 sửa bằng `pg_isready -h 127.0.0.1` |

**Vì sao là vấn đề:** Healthcheck không chạy được thì container **luôn `unhealthy`**, và mọi service khai báo `depends_on: condition: service_healthy` **không bao giờ khởi động** — cả hệ thống đứng yên dù không có lỗi nào trong code.

**Phương án:** (A) **Dùng lệnh đã kiểm chứng ở Buổi 2**; riêng `api` cài thêm `curl` ở stage runtime vì healthcheck phải gọi đúng `/health/ready` (kiểm tra kết nối DB + Redis), không chỉ kiểm tra cổng mở; (B) viết một chương trình probe nhỏ bằng .NET trong image — không phụ thuộc `curl` nhưng thêm code phải bảo trì; (C) chỉ kiểm tra cổng TCP — sai ngữ nghĩa readiness.

**➡️ Quyết định (v1.2.0): Phương án A.** Mỗi dòng trong bảng §6.5 ghi kèm lý do chọn lệnh. Sửa: §6.5.

---

### MT-52 🟡 (a) — Admin tự khóa chính mình: ba chỗ, ba câu trả lời

**Hiện trạng**

| Vị trí (SRS v1.1.0) | Quy định |
| --- | --- |
| FR-AUTH-008, ô Status Code và luồng A2 | **403 Forbidden** |
| Chương 8.1, dòng `PATCH /users/{id}/status` | **409** với mã `VALIDATION_ERROR` |
| Phụ lục B | `VALIDATION_ERROR` luôn là **400** |

**Vì sao là vấn đề:** Mâu thuẫn do chính CR-2026 tạo ra khi viết mới FR-AUTH-008 và bảng Chương 8.1 ở hai thời điểm. Cặp "409 + `VALIDATION_ERROR`" còn vi phạm quy tắc hai vế mà MT-08/09 vừa chốt (400 = đầu vào sai, 409 = xung đột trạng thái).

**Phương án:** (A) **403** — người gọi hợp lệ nhưng *không được phép thực hiện hành động này lên chính mình*; (B) 400 — coi là vi phạm quy tắc nghiệp vụ của dữ liệu đầu vào; (C) 409 — coi là xung đột trạng thái.

**➡️ Quyết định (v1.2.0): Phương án A.** Yêu cầu hoàn toàn hợp lệ về cú pháp (loại trừ 400) và không xung đột với trạng thái của tài nguyên (loại trừ 409); điều bị từ chối là **quyền** của người gọi với đích là chính họ — đúng ngữ nghĩa 403. Phương án này cũng khớp FR (nguồn chi tiết hơn). Sửa: FR-AUTH-008, Chương 8.1, Phụ lục A.

---

### MT-53 🟢 (a) — Giải thích sai cơ chế định tuyến và danh sách slug dành riêng mỗi nơi một kiểu

**Hiện trạng**

| Vị trí (SRS v1.1.0) | Nội dung |
| --- | --- |
| Chương 8 quy ước + ghi chú §8.3 | *"`/recipes/search` phải được đăng ký **trước** `/recipes/{slug}` trong Minimal API"* |
| NFR-SEO-004, §7.2 | Slug dành riêng: `search`, `new`, `edit` |
| FR-CAT-003 bước 6 | `search`, `new`, `edit`, `mine` |
| Chương 8.3 (sau CR-2026) | Có thêm route literal `/recipes/mine` |

**Vì sao là vấn đề:** (1) ASP.NET Core endpoint routing **ưu tiên segment literal hơn tham số** — `/recipes/search` luôn thắng `/recipes/{slug}` **bất kể thứ tự khai báo**. Giải thích sai cơ chế khiến người đọc tin rằng chỉ cần "đăng ký đúng thứ tự" là đủ, và bỏ qua vấn đề thật: một công thức nhận slug `search` hoặc `mine` sẽ **không bao giờ truy cập được**. (2) Ba danh sách slug dành riêng khác nhau ở ba chỗ — `mine` thiếu ở NFR-SEO-004, và sau MT-48 cần thêm `sitemap`.

**➡️ Quyết định (v1.2.0):** Sửa lời giải thích theo đúng cơ chế; chốt **một danh sách duy nhất** tại NFR-SEO-004 — `search`, `mine`, `sitemap`, `new`, `edit` — áp dụng cho cả Recipe và Category, các chỗ khác chỉ tham chiếu. Sửa: NFR-SEO-004, FR-CAT-003, FR-RCP-003, Chương 8, Phụ lục C, Phụ lục D.

---

### MT-54 🟢 (a) — Khai báo proxy tin cậy bằng API không nhận dải mạng hoặc đã lỗi thời

**Hiện trạng:** SRS v1.1.0 (NFR-SEC-003, §4.2, §6.2, Phụ lục C, Phụ lục D) ghi *"khai báo **`KnownProxies`** giới hạn ở dải mạng Docker nội bộ"*. Dự án chạy .NET 10 và bật `TreatWarningsAsErrors`.

**Vì sao là vấn đề:** `KnownProxies` chỉ nhận **từng địa chỉ IP cố định** — không nhận dải mạng, trong khi IP container Docker thay đổi mỗi lần tạo lại. Thuộc tính nhận dải mạng quen thuộc trong tài liệu cũ là `KnownNetworks` (kèm `Microsoft.AspNetCore.HttpOverrides.IPNetwork`) — nhưng cả hai đã bị đánh dấu **obsolete trên .NET 10**; với `TreatWarningsAsErrors`, dùng chúng là **gãy build**. Làm theo chữ của SRS hoặc theo tài liệu cũ đều hỏng.

**➡️ Quyết định (v1.2.0):** Chỉ đích danh **`ForwardedHeadersOptions.KnownIPNetworks`** với `System.Net.IPNetwork` (ví dụ `172.16.0.0/12`, cấu hình được qua appsettings), ghi rõ lý do không dùng hai lựa chọn kia, và yêu cầu `UseForwardedHeaders` đứng đầu pipeline. Sửa: NFR-SEC-003, §4.2, §6.2, Phụ lục C, Phụ lục D.

---

### MT-55 🟡 (b) — SRS không quy định Frontend được lưu token ở đâu

**Hiện trạng:** SRS v1.1.0 §5.2 quy định refresh token đi trong **body request, không dùng cookie**; NFR-SEC-002 quy định cách phát và lưu token **phía server**. **Không có chỗ nào** nói Frontend lưu access token và refresh token ở đâu. Buổi 2 đã lưu **cả hai** vào `localStorage` (`BAO_CAO_BUOI_2.md` §4.12 tự ghi *"cần xem lại Buổi 4"*).

**Vì sao là vấn đề:** `localStorage` đọc được bởi mọi script trên trang — một lỗ hổng XSS là đủ để lấy cả access token (dùng ngay được với mọi API) lẫn refresh token (gia hạn phiên tới 7 ngày). Khoảng trống trong đặc tả đã trực tiếp dẫn tới cách hiện thực kém an toàn nhất.

**Phương án**

| # | Phương án | Ưu điểm | Nhược điểm |
| --- | --- | --- | --- |
| A | **Access token chỉ trong bộ nhớ; refresh token là thứ duy nhất lưu bền; CSP bắt buộc** | Access token biến mất khi đóng tab, không nằm ở nơi script lạ quét được; refresh token bị đánh cắp sẽ **tự lộ** ở lần dùng lại nhờ Rotation + Reuse Detection; tuân thủ §5.2 | Refresh token vẫn nằm ở nơi JavaScript đọc được |
| B | Refresh token trong cookie `HttpOnly` | JavaScript không đọc được refresh token | **Trái §5.2**; phải thêm cơ chế chống CSRF — là một Change Request riêng |
| C | Giữ `localStorage` cho cả hai | Không phải sửa | Rủi ro XSS tối đa |

**➡️ Quyết định (v1.2.0): Phương án A.** Phương án B được ghi nhận là hướng nâng cấp nếu sau này nhóm chấp nhận đổi §5.2. Sửa: NFR-SEC-002, NFR-SEC-005 (CSP thành yêu cầu tường minh).

---

### MT-56 🟢 (a) — Ràng buộc `DEFERRABLE` không nói rõ phải là constraint

**Hiện trạng:** SRS v1.1.0 (FR-RCP-010, §7.3) yêu cầu `UNIQUE (RecipeId, StepNumber)` khai báo `DEFERRABLE INITIALLY DEFERRED` nhưng không nói dạng khai báo. Code Buổi 2 dùng `HasIndex(...).IsUnique()` của EF Core — sinh ra **unique index**. §7.5 ghi *"Chỉ có 1 ảnh `IsPrimary=true` / Recipe"* mà không nói cơ chế; code dùng **partial unique index** `WHERE "IsPrimary" AND NOT "IsDeleted"`.

**Vì sao là vấn đề:** PostgreSQL **không cho phép index là `DEFERRABLE`** — chỉ constraint mới được. Làm theo cách phổ biến nhất trong EF Core thì tính năng kéo-thả sắp xếp bước (FR-RCP-010) sẽ lỗi `23505` ở câu `UPDATE` đầu tiên, dù code Domain hoàn toàn đúng. Tương tự, partial index của ảnh chính **không thể** chuyển thành constraint (constraint không có `WHERE`), nên đổi ảnh chính phải làm hai bước — điều SRS không nói tới.

**➡️ Quyết định (v1.2.0):** Ghi rõ "**UNIQUE CONSTRAINT** `DEFERRABLE INITIALLY DEFERRED` — không phải unique index" và tạo bằng DDL trong migration; ghi rõ partial unique index cho ảnh chính và quy tắc đổi ảnh chính **hai bước trong một transaction**. Sửa: FR-RCP-010, §7.3, §7.5.

---

### MT-57 🟢 (a) — `/categories/[slug]` được ghi là ISR nhưng không thể dựng tĩnh

**Hiện trạng:** SRS v1.1.0 §5.1 ghi `/categories/[slug]` là *"ISR (revalidate=120)"*; trang này phân trang bằng `?page=`. Buổi 2 ghi nhận: *"Trang chi tiết đọc `?page=`, nên Next bắt buộc render động... HTML không phải ISR thuần"* (`BAO_CAO_BUOI_2.md` §4.5).

**Vì sao là vấn đề:** Trong Next.js App Router, trang đọc `searchParams` **bắt buộc render theo request** — không thể là ISR. Đặc tả mô tả một cơ chế không khả thi; QA kiểm tra header `x-nextjs-cache` sẽ luôn thấy "không đạt".

**Phương án:** (A) **SSR + Data Cache** (`fetch` với `revalidate: 120`) — đạt đúng mục tiêu thật (dữ liệu không cũ hơn TTL API); (B) đổi URL phân trang thành `/categories/[slug]/page/[n]` để giữ ISR — URL phức tạp và phải sinh trước số trang chưa biết; (C) bỏ phân trang — không khả thi với danh mục nhiều công thức.

**➡️ Quyết định (v1.2.0): Phương án A.** Quy tắc "thời gian tái sinh ≤ TTL cache API" được phát biểu lại để áp dụng cho cả ISR lẫn Data Cache. Sửa: §5.1.

### MT-58 🟡 — Yêu cầu dữ liệu mẫu của giảng viên khác SRS §2.6.1

**Hiện trạng:** SRS v1.2.0 §2.6.1 ghi *"Dữ liệu test (seed) được tạo bằng thư viện Bogus với 50 recipe mẫu và 5 tác giả mẫu"*; kế hoạch Buổi 2 seed ~8 danh mục. Khi đánh số lại 8 buổi (21/09/2026), giảng viên yêu cầu Buổi 2 phải có CSDL chứa dữ liệu ngẫu nhiên cho **ít nhất 20 categories, 100 recipes; mỗi recipe ít nhất 10 nguyên liệu và 5 bước chế biến**. Database thực tế lúc đó: 8 danh mục, 50 công thức, mỗi công thức 4–9 nguyên liệu chọn ngẫu nhiên từ một danh sách chung và 3–6 bước có mô tả Lorem ipsum.

**Vì sao là vấn đề:** code làm theo yêu cầu mới thì lệch SRS; giữ theo SRS thì không đạt yêu cầu của giảng viên. Ngoài ra, nếu chỉ tăng số lượng bằng Bogus như cũ, dữ liệu sẽ vô nghĩa (món tráng miệng có nước mắm, mô tả bước bằng tiếng Latin) — chính dữ liệu này dùng để kiểm thử tìm kiếm tiếng Việt, đo hiệu năng và trình diễn khi bảo vệ.

**Phương án:** (A) Cập nhật SRS qua Change Request, nội dung món ăn viết tay (catalog), Bogus chỉ sinh phần ngẫu nhiên hợp lý, seeder tự bù cho database đang có; (B) giữ SRS, chỉ tăng tham số Bogus — lệch SRS và dữ liệu sai; (C) yêu cầu cả nhóm xóa volume để seed lại — mất dữ liệu thử nghiệm của từng người.

**➡️ Quyết định (v1.2.1, CR-2026-03): Phương án A.** Sửa: §2.6.1. Hiện thực: `RecipeSeedCatalog` + `DatabaseSeeder` (Buổi 2 — Dev 4, mục "Yêu cầu bổ sung" trong `KE_HOACH_PHAT_TRIEN_8_BUOI.md`).

---

## 4. Xung đột giữa SRS và hiện trạng code Buổi 2 (nợ kỹ thuật)

Các điểm dưới đây **không phải mâu thuẫn bên trong SRS** — SRS v1.2.0 đã nhất quán ở những điểm này — mà là chỗ **code đã chạy khác với SRS**, vì Buổi 2 được hiện thực theo SRS v1.0.0 (trước CR-2026). Mỗi điểm được kiểm chứng trực tiếp trong code. Kế hoạch hoàn trả chi tiết (buổi nào, dev nào, bước nào) nằm tại `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md` §4.1; bảng này ghi lại để tài liệu mâu thuẫn có bức tranh đầy đủ.

| # | Code Buổi 2 | SRS v1.2.0 quy định | Căn cứ SRS | Hoàn trả |
| --- | --- | --- | --- | --- |
| D-1 | Đăng ký nhận `{ fullName, email, userName, password }`; mã `AUTH_USERNAME_EXISTS` | `{ email, password, displayName }`, BE tự sinh `UserName` | MT-12 | Buổi 3 |
| D-2 | Refresh token 64 byte (512-bit) | 32 byte (256-bit) | MT-13 | Buổi 4 |
| D-3 | `Recipe.Slug` unique thường | Partial unique `WHERE "IsDeleted" = false` | MT-05 | Buổi 6 |
| D-4 🔴 | Danh sách công thức lọc theo danh tính + Output Cache khóa công khai; Draft trả 403 | Chỉ Published cho mọi người gọi; Draft → 404; riêng tư qua `/recipes/mine` | **MT-34** | Buổi 6 |
| D-5 🔴 | Chi tiết danh mục lọc theo danh tính | Chỉ Published | **MT-34** | Buổi 6 |
| D-6 | Output Cache (Redis-backed) 15′/60′ | Redis cache-aside 2′/5′, không Output Cache | MT-16, MT-17 | Buổi 6 |
| D-7 | Tái sinh dữ liệu danh mục 3600s/600s | 1800s/120s | MT-33.3, MT-57 | Buổi 6 |
| D-8 | `Instructions` NOT NULL | NULL | MT-20.8 | Buổi 3 |
| D-9 | Hangfire worker riêng | **Đã chuẩn hóa theo code** | MT-47 | — (không còn lệch) |
| D-10 | `sort=-field` | `sortBy` + `sortOrder` | MT-01 | Buổi 5 |
| D-11 | Validation trả **422** | 400 | MT-08 | Buổi 3 (commit nền) |
| D-12 | Access + refresh token trong `localStorage` | Access token chỉ trong bộ nhớ | MT-55 | Buổi 4 |
| D-13 | TTL `categories:all` 60′ | 30′ | MT-17 | Buổi 3 |
| D-14 | `next/image unoptimized`; ảnh ở `localhost:9000` | Ảnh qua Nginx `/media/` | MT-51 (§6.5) | Buổi 7 |
| D-15 | `Publish()` cho Archived → Published; Published → publish là no-op | 409 `RECIPE_INVALID_STATE_TRANSITION` | MT-35 | Buổi 5 |
| D-16 | `(RecipeId, StepNumber)` là unique index | Unique **constraint** deferrable | MT-56 | Buổi 4 |
| D-17 | Khóa DataProtection không mã hóa | `ProtectKeysWithCertificate` ở production | NFR-SEC-007 | Buổi 7 |
| D-18 | `init.sql` tạo config FTS `vietnamese_unaccent` (cơ chế thứ hai) | Một cơ chế: `simple` + `unaccent_immutable` trong migration | MT-25 | Buổi 4 |

> **Hai điểm ngược chiều:** D-9 là trường hợp **code đúng hơn SRS** — thay vì sửa code, SRS được sửa (MT-47). Tương tự, cấu hình Nginx và lệnh healthcheck của code là đúng và SRS v1.1.0 sai (MT-50, MT-51). Nguyên tắc áp dụng: SRS là nguồn sự thật **về yêu cầu**, nhưng khi code đã chứng minh một lựa chọn kỹ thuật tốt hơn qua sự cố thật, cách đúng là sửa SRS qua Change Request chứ không phải ép code lùi về đặc tả kém hơn.

---

## 5. Xung đột giữa lộ trình phát triển được giao và SRS

Lộ trình được giao (bản mô tả ban đầu của từng buổi, nay đánh số 8 buổi vì Buổi 1 là buổi đọc đặc tả) được viết trước khi SRS v1.1.0/v1.2.0 hoàn tất, nên có 5 điểm lệch. Kế hoạch 8 buổi giải quyết **theo SRS**; điểm X-5 dẫn tới việc bổ sung SRS (MT-45).

| # | Lộ trình ghi | SRS quy định | Cách giải quyết | Căn cứ |
| --- | --- | --- | --- | --- |
| X-1 | Buổi 3: *"Google OAuth PKCE"* | ID Token flow, không PKCE, không redirect URI ở Backend | Làm ID Token flow | MT-11 |
| X-2 | Buổi 4: *"FR-JOB-003 Recurring Sitemap Generator 02:00 UTC"* | FR-JOB-003 = Permanent Purge Job 03:30 UTC; sitemap do Next.js sinh | Buổi 4 làm Purge Job; sitemap chuyển sang Dev 3 Buổi 7 | MT-05, MT-28 |
| X-3 | Buổi 6: *"ISR revalidate 3600s"* cho trang danh mục | 1800s / 120s | Theo SRS | MT-33.3 |
| X-4 | Phân công *"FR-AUTH-001 đến 007"*, *"FR-RCP-001 đến 010"* | Có thêm FR-AUTH-008, FR-AUTH-009, FR-RCP-011 | Gán vào đúng module của Dev 1 và Dev 2 | MT-22, MT-34, MT-45 |
| X-5 | Buổi 7: *"Quản lý phiên làm việc nâng cao"* | v1.1.0 không có FR nào | Bổ sung **FR-AUTH-009** vào SRS v1.2.0 | MT-45 |

---

## 6. Đánh đổi đã chấp nhận và điểm còn mở sau SRS v1.2.0

Sau khi áp dụng CR-2026-02, SRS không còn mâu thuẫn nội bộ nào được biết. Các điểm dưới đây **không phải mâu thuẫn** mà là **đánh đổi có chủ đích** hoặc **việc để dành cho phiên bản sau** — được ghi lại để không ai nhầm chúng là sai sót và "sửa" theo hướng làm hỏng thiết kế.

| # | Điểm | Vì sao chấp nhận | Hướng xử lý về sau |
| --- | --- | --- | --- |
| O-1 | Access token của người bị khóa vẫn dùng được tối đa **15 phút** (FR-AUTH-008) | Thu hồi tức thời đòi tra DB ở **mọi** request — vứt bỏ lợi ích stateless của JWT (CONS-004) | Rút TTL access token, hoặc thêm kiểm tra `SecurityStamp` định kỳ nếu yêu cầu an ninh tăng |
| O-2 | Refresh token vẫn nằm ở nơi JavaScript đọc được (MT-55) | §5.2 cấm cookie; rủi ro được kiểm soát bằng Rotation + Reuse Detection + CSP | Chuyển sang cookie `HttpOnly` + chống CSRF qua một CR riêng |
| O-3 | Tệp upload qua `/files/upload` nhưng không được gán vào đâu sẽ nằm lại trên MinIO (MT-43) | Đơn giản hóa endpoint; dung lượng nhỏ ở quy mô đồ án | Job dọn tệp mồ côi (không được tham chiếu sau N ngày) |
| O-4 | Công thức vừa bị unpublish/archive còn hiển thị tối đa **5 phút** ở trang ISR `/recipes/[slug]` | Hệ quả của TTL cache 5′ và quy tắc "tái sinh ≤ TTL" | Gọi on-demand revalidation của Next.js từ Backend khi đổi trạng thái |
| O-5 | Khôi phục dữ liệu soft-deleted chỉ do quản trị viên vận hành làm trên DB (MT-49) | Không có trong lộ trình 8 buổi | FR "Thùng rác" ở phiên bản sau |
| O-6 | TTL cache ngắn hơn (5′ thay 60′) nên **hit rate thấp hơn**, mốc ≥ 80% của NFR-PERF-003 khó đạt hơn | Dữ liệu tươi và một cơ chế cache duy nhất quan trọng hơn con số đẹp | Nếu đo được < 80%: ghi nhận trung thực và phân tích, **không** nới TTL chỉ để đạt chỉ tiêu |

---

## 7. Thứ tự xử lý đề xuất

**Nhóm 0 — Chốt đầu tiên, vì là lỗi bảo mật / logic nghiệp vụ:**
**MT-34** (cache rò rỉ Draft) → **MT-35** (máy trạng thái Recipe)

> Hai mục này không chỉ là "hai chỗ ghi khác nhau" mà là **thiết kế sai**: MT-34 làm lộ dữ liệu riêng tư giữa các tài khoản, MT-35 khiến recipe bị kẹt vĩnh viễn ở trạng thái Archived. Chốt xong hai mục này rồi mới bàn tới phần còn lại, vì MT-34 quyết định luôn hình dạng của endpoint danh sách (ảnh hưởng MT-01, MT-17, MT-23).

**Nhóm 1 — Chốt trước khi viết dòng code nào** (ảnh hưởng schema và kiến trúc):
MT-05 (soft delete) → MT-25 (cơ chế FTS) → MT-12 (tên trường user) → MT-14, MT-15 (RefreshToken) → MT-21 (bỏ policy VerifiedAuthor) → MT-16 (Redis) → MT-04 (Quantity) → MT-26 (index) → MT-37 (bảng độ dài chuẩn) → MT-30 (dependency của Domain)

> MT-25, MT-26 và MT-37 phải nằm ở nhóm này vì chúng quyết định nội dung **migration đầu tiên**. Sửa sau khi đã có dữ liệu sẽ phải viết migration vá.

**Nhóm 2 — Chốt trước khi làm module tương ứng** (hợp đồng API):
MT-08, MT-09 (mã lỗi) → MT-10, MT-11 (auth) → MT-22 (khóa tài khoản) → MT-01 (sorting) → MT-02, MT-03 (recipe con) → MT-19 (ảnh) → MT-06 (publish) → MT-36 (409 danh mục) → MT-27 (slug) → MT-23 (trang chủ)

**Nhóm 3 — Chốt khi dựng hạ tầng / triển khai:**
MT-38 (ForwardedHeaders) → MT-39 (bảo vệ Hangfire Dashboard) → MT-28 (sitemap) → MT-29 (healthcheck thay K8s) → MT-32 (scale với Compose) → MT-17 (TTL cache) → MT-18 (SDK MinIO)

**Nhóm 4 — Cập nhật khi rà soát tài liệu:**
MT-07, MT-13, MT-20, MT-24, MT-31, MT-33, MT-40 (MoSCoW), MT-41

**Lưu ý phụ thuộc giữa các mục:** MT-34 → MT-01, MT-17, MT-23 (hình dạng endpoint danh sách quyết định cách sort/cache/trang chủ); MT-34 ↔ MT-41.1, MT-41.2 (cùng gốc: endpoint công khai nhưng phụ thuộc danh tính); MT-08 ↔ MT-09 ↔ MT-35, MT-36 (cùng bộ mã lỗi, MT-35 thêm `RECIPE_INVALID_STATE_TRANSITION`); MT-16 → MT-17 (chốt Redis rồi mới chốt TTL); MT-28 → MT-32 (bỏ sitemap job thì bớt luôn nhu cầu distributed lock); MT-05 → MT-27 (soft delete ảnh hưởng unique index của Slug); MT-21 → MT-22 (cùng nhóm vòng đời tài khoản); MT-35 → MT-23 (`PublishedAt` phải được gán thì mới sắp xếp trang chủ theo nó được).

**Thứ tự xử lý lượt 4 (MT-42 → MT-57):** tất cả được áp dụng **cùng lúc** trong một Change Request (CR-2026-02) vì không mục nào đòi thay đổi schema dữ liệu đang có. Ưu tiên trong lúc sửa: **MT-50, MT-51** trước (lỗi làm hệ thống không chạy được) → **MT-52 → MT-54, MT-56, MT-57** (lỗi kỹ thuật khác của v1.1.0) → **MT-42 → MT-49, MT-55** (bổ sung và làm rõ yêu cầu).

---

## 8. Quy trình Change Request (đã thực hiện ba lần)

**Quy trình áp dụng cho mỗi lượt:**

1. Nhóm chốt từng mục (theo khuyến nghị hoặc phương án khác — cột "Phương án" đã nêu đủ đánh đổi để quyết định).
2. Ghi quyết định vào bảng "Lịch sử thay đổi tài liệu" của SRS, nâng phiên bản, trạng thái `Approved`.
3. Tạo file SRS phiên bản mới, **giữ nguyên các bản cũ** để đối chiếu.
4. Mỗi thay đổi trong SRS được đánh dấu `[CR-xxxx / MT-xx]` tại chỗ và truy vết trong một phụ lục riêng.
5. Với các quyết định kiến trúc quan trọng, viết **ADR** tương ứng (NFR-MAINT-003).
6. Cập nhật kế hoạch phát triển nếu quyết định làm thay đổi khối lượng công việc của buổi nào.

**Các lần đã thực hiện:**

| CR | Ngày | Mục xử lý | Kết quả | Truy vết |
| --- | --- | --- | --- | --- |
| **CR-2026** | 17/09/2026 | MT-01 → MT-41 (lượt 1–3) | SRS v1.1.0 — 36 FR, 37 endpoint, 24 mã lỗi (đã được v1.2.0 kế thừa toàn bộ; không lưu tệp riêng) | SRS Phụ lục D |
| **CR-2026-02** | 19/09/2026 | MT-42 → MT-57 (lượt 4) | SRS v1.2.0 — **37 FR, 44 endpoint, 27 mã lỗi** (đã được v1.2.1 kế thừa toàn bộ) | SRS Phụ lục E |
| **CR-2026-03** | 21/09/2026 | MT-58 (lượt 5) | `SPEC/SRS_Culinary_Blog_v1.2.1.md` — §2.6.1 dữ liệu mẫu ≥ 20 danh mục / ≥ 100 công thức / ≥ 10 nguyên liệu / ≥ 5 bước; FR, endpoint, mã lỗi không đổi | SRS Phụ lục F |

Kế hoạch phát triển tương ứng: `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md` (làm theo SRS v1.2.1).

> **Bài học quy trình từ lượt 4 — áp dụng cho mọi CR sau:** 7/16 mục của lượt 4 là **lỗi do chính CR-2026 đưa vào** (cấu hình Nginx, lệnh healthcheck, mã lỗi tự khóa, tên API .NET, cơ chế route, dạng ràng buộc DB, kiểu render). Chúng không thể phát hiện bằng cách đọc chéo các chương với nhau — chỉ lộ ra khi đối chiếu với hệ thống đang chạy. Vì vậy, từ CR-2026-02 trở đi, **mọi thay đổi chạm tới cấu hình hạ tầng, mã lỗi, ràng buộc cơ sở dữ liệu hoặc API của framework phải được kiểm chứng trên code/môi trường thật trước khi duyệt**.

---

*Trạng thái tài liệu: toàn bộ 58 mục đã được xử lý — MT-01 → MT-41 trong SRS v1.1.0, MT-42 → MT-57 trong SRS v1.2.0, MT-58 trong SRS v1.2.1. Các điểm lệch giữa code Buổi 2 và SRS (§4) được hoàn trả theo `KE_HOACH_PHAT_TRIEN_8_BUOI.md`; các đánh đổi còn mở được ghi tại §6.*
