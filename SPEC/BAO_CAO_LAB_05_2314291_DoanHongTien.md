# BÁO CÁO KẾT QUẢ BUỔI 5 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (Category / Recipe read / Search)
**Nhánh:** [`2314291_DoanHongTien_buoiso5`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2314291_DoanHongTien_buoiso5) — tách từ nhánh Buổi 4 vì `develop` chưa có code Buổi 3–4. **PR vào `develop`:** mở sau khi PR Buổi 4 (#41) được merge và nhánh đã rebase lên `develop`.
**Phạm vi (KE_HOACH_PHAT_TRIEN_8_BUOI.md — Buổi 5, Dev 3):** giao diện FR-SRCH-001 (thanh tìm kiếm + trang `/search`), giao diện FR-SRCH-002/003/004 (`FilterPanel`, `SortSelect`, `Pagination` đồng bộ URL) kèm **retrofit D-10 phía FE**, composite index + `EXPLAIN ANALYZE` (NFR-PERF-004).
**Tài liệu bám sát:** `SPEC/SRS_Culinary_Blog_v1.2.2.md` (§3.4 FR-SRCH, §5.1, §7.2, §8.2, §8.3, NFR-PERF-004) và `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` (MT-01, MT-26, MT-57; §4 D-10).

---

## 1. Tóm tắt công việc

| # | Hạng mục | Yêu cầu trong kế hoạch | Kết quả |
|---|---|---|---|
| A | Migration `B5_Search_CompositeIndexes` | 3 composite index của SRS §7.2 | ✅ Áp dụng thành công từ B1→B5 trên PostgreSQL 16; `HasPendingModelChanges() = False` |
| B | `PerformanceSeeder` | ≥ 10.000 recipe, chỉ bật bằng cờ cấu hình | ✅ Tạo 10.000 recipe (tổng 10.100, 8.523 Published); mặc định tắt |
| C | `EXPLAIN ANALYZE` | Index Scan, không Seq Scan trên `Recipes` | ✅ Đo bằng SQL thật của EF; sau B5 không còn Seq Scan — `docs/explain-analyze-b5.md` |
| D | `SearchBar` + trang `/search` | debounce 300 ms, Enter → `/search?q=`, SSR, highlight, empty state | ✅ Kiểm bằng trình duyệt thật (Playwright) với API thật |
| E | `FilterPanel`, `SortSelect`, `Pagination` đồng bộ URL | Toàn bộ state trong `searchParams`; drawer trên mobile | ✅ Kiểm bằng trình duyệt thật, cả màn hình 390 px |
| F | Retrofit D-10 phía FE | Bỏ `sort=-createdAt`, dùng `sortBy`/`sortOrder` | ✅ |

## 2. Backend

- `Persistence/Migrations/20261007180000_B5_Search_CompositeIndexes.cs` (viết tay, theo tiền lệ B3/B4): `IDX_Recipe_List (IsDeleted, Status, PublishedAt DESC)`, `IDX_Recipe_ByCategory (Status, CategoryId, PublishedAt DESC)`, `IDX_Recipe_CookTime (Status, CookTimeMinutes)`; `Down` xóa cả ba. Khai báo tương ứng trong `RecipeConfiguration` và `CulinaryBlogDbContextModelSnapshot.cs`.
- `Persistence/Seed/PerformanceSeeder.cs` + `PerformanceSeedOptions` (`PerformanceSeed:Enabled`, mặc định `false`; `RecipeCount` mặc định 10.000). Chạy sau `DatabaseSeeder` trong `Program.cs`; idempotent (nhận diện slug `perf-…`, lần chạy sau không nạp thêm); ~85 % Published; Bogus với seed cố định; ghi theo lô 1.000.
- **Không** tạo index `(Status, CreatedAt DESC)` dự phòng: truy vấn `createdAt desc` đã Index Scan nhờ `IDX_Recipe_CreatedAt` (có từ B1). NFR-PERF-004: không tạo index theo cảm tính.

**Kết quả `EXPLAIN ANALYZE`** — schema tạo bằng migration thật, dữ liệu từ `DatabaseSeeder` + `PerformanceSeeder`, SQL lấy từ log EF Core khi gọi API, PostgreSQL 16.15, `VACUUM ANALYZE` trước khi đo:

| # | Truy vấn (endpoint) | Trước B5 | Sau B5 |
|---|---|---|---|
| Q1 | `/recipes` — COUNT tổng | **Seq Scan** · 5,83 ms | Index Only Scan `IDX_Recipe_List` · 2,82 ms |
| Q2 | `/recipes` mặc định `createdAt desc` | Index Scan `IDX_Recipe_CreatedAt` · 1,21 ms | không đổi · 1,32 ms |
| Q3 | `sortBy=publishedAt desc` | Index Scan `IDX_Recipe_PublishedAt` · 1,16 ms | Index Scan `IDX_Recipe_List` · 0,15 ms |
| Q4b | `categoryId` + `publishedAt desc` | Index Scan `IDX_Recipe_PublishedAt` · 1,51 ms | Index Scan `IDX_Recipe_ByCategory` · 0,14 ms |
| Q5a | `maxCookTime=30` — COUNT | **Seq Scan** · 3,44 ms | Bitmap Index Scan `IDX_Recipe_CookTime` · 1,80 ms |
| Q5b | `maxCookTime=30` + `cookTime asc` | **Seq Scan** · 3,13 ms | Index Scan `IDX_Recipe_CookTime` · 0,20 ms |
| Q6 | `/recipes/search?q=pho bo` | Bitmap Index Scan GIN `IDX_Recipe_Search` | không đổi |
| Q7 | `/categories/{slug}` `publishedAt desc` | Index Scan `IDX_Recipe_PublishedAt` · 1,27 ms | Index Scan `IDX_Recipe_ByCategory` · 0,22 ms |

Ba truy vấn còn Seq Scan trước B5 đều chuyển sang dùng index; cả ba index mới đều được planner dùng. Bảng đầy đủ (10 câu), plan chi tiết và cách chạy lại trên Docker: `docs/explain-analyze-b5.md`; SQL: `docs/explain-analyze-b5.sql`.

## 3. Frontend — chức năng

| Thành phần | File | Ghi chú |
|---|---|---|
| Trạng thái URL dùng chung | `features/recipes/search-params.ts` | `parseRecipeListState` / `parseCategoryState` / `parseSearchState`, `buildQueryString`, `buildHref`. Bỏ giá trị mặc định khỏi URL; đổi lọc/sắp xếp thì về trang 1 |
| `SearchBar` | `features/recipes/components/SearchBar.tsx` | Enter → `/search?q=`; đang ở `/search` thì ngừng gõ 300 ms tự cập nhật (`router.replace`, giữ bộ lọc); dưới 2 ký tự không gọi |
| Trang `/search` | `app/search/page.tsx` | SSR; `q` < 2 ký tự không gọi API (sẽ là 400); highlight không dấu theo đầu từ; empty state có gợi ý (FR-SRCH-001 A2); form GET thuần cho mobile/không JS |
| `FilterPanel` | `.../FilterPanel.tsx` | `/recipes`: danh mục, độ khó (4 mức), thời gian nấu, chuẩn bị, khẩu phần. `/search`: chỉ danh mục + độ khó (SRS §8.3). Dưới `lg` là drawer (Escape/nền/nút đóng), có badge số bộ lọc |
| `SortSelect` | `.../SortSelect.tsx` | Hai dropdown cột/chiều ↔ `sortBy`/`sortOrder` (whitelist 5 cột) |
| Highlight | `features/recipes/highlight.ts`, `HighlightText.tsx`, `RecipeCard.tsx` (prop `terms`) | So khớp không dấu (`pho` tô `Phở`), khớp tiền tố như `pho:*` ở backend |
| Lớp gọi API | `features/recipes/api.ts`, `features/categories/api.ts` | `searchRecipes`, `toRecipeListParams`; kiểu `RecipeSearchResult` trong `types/api.ts` |

Phạm vi bộ lọc theo từng trang bám đúng hợp đồng API của SRS: `/recipes` đủ 5 bộ lọc + sort (§8.3); `/search` chỉ `categoryId`, `difficulty` và xếp theo `ts_rank` nên không có sắp xếp (§8.3); `/categories/[slug]` chỉ `sortBy`/`sortOrder` (§8.2). **Không mở rộng API** ngoài SRS.

## 4. Retrofit D-10 phía FE

- Xóa hàm cầu nối `toSortParams` (Buổi 4) và `sort=-createdAt` trong `app/recipes/(list)/page.tsx`; `RecipeListParams` dùng `sortBy`/`sortOrder`.
- Kiểm bằng trình duyệt: chọn sắp xếp cho URL `?difficulty=Hard&maxCookTime=30&sortBy=title&sortOrder=asc` — không còn tham số `sort`; danh sách hiển thị đúng thứ tự API trả về.
- Giá trị hỏng trong URL gõ tay (`sortBy=password`, `page=abc`) được đưa về mặc định **ở phía giao diện** để người đọc không gặp trang lỗi; API vẫn trả 400 cho client gọi trực tiếp (đã kiểm: `GET /recipes?sortBy=password` → 400, `GET /recipes/search?q=p` → 400).

## 5. Điểm lệch / quyết định (theo SRS)

| # | Điểm | Căn cứ | Cách xử lý |
|---|---|---|---|
| 1 | SRS §5.1 / MT-57: `/categories/[slug]` là SSR + Data Cache `revalidate: 120` (≤ TTL `categories:detail` 2 phút) | SRS §5.1 | `getCategoryBySlug` đặt `revalidate: 120` + tag, đồng thời nhận `sortBy`/`sortOrder` |
| 2 | `getCategories` `revalidate: 3600` > TTL API 30 phút (SRS §5.1 ghi 1800) | SRS §5.1, D-13 | **Không sửa** — không thuộc việc Buổi 5; ghi nhận để xử lý cùng ISR/SEO ở Buổi 6 |
| 3 | Kế hoạch ghi "dùng chung bộ lọc trên `/categories/[slug]`" nhưng SRS §8.2 chỉ cho danh mục nhận `sortBy`/`sortOrder` | SRS §8.2 | Danh mục chỉ có `SortSelect` + phân trang, không có `FilterPanel` |
| 4 | `develop` chưa có code Buổi 3–4 nên không tách nhánh Buổi 5 từ `develop` được | Quy trình nhánh | Tách từ nhánh Buổi 4; rebase lên `develop` sau khi PR #41 merge rồi mới mở PR |

## 6. Test và kiểm chứng đã chạy (12/10/2026)

| Lệnh / kiểm tra | Kết quả |
|---|---|
| `dotnet build CulinaryBlog.sln` (TreatWarningsAsErrors, SonarAnalyzer, StyleCop) | ✅ Build succeeded, 0 warning |
| `dotnet test` Domain.UnitTests / Application.UnitTests / ArchitectureTests | ✅ 64 / 91 / 18 đạt |
| `dotnet test` toàn bộ solution trên máy dev với Docker (Testcontainers: PostgreSQL 16, Redis, MinIO) | ✅ **319/319 đạt, 0 lỗi, 0 bỏ qua** (64 Domain + 91 Application + 18 Architecture + 146 Integration) |
| Migration B1 → B5 trên database trống + `HasPendingModelChanges()` | ✅ áp dụng đủ 5 migration; `False` (snapshot sửa tay khớp model) |
| `PerformanceSeeder` qua API thật | ✅ "đã tạo 10000 công thức mẫu hiệu năng"; 10.100 bản ghi, 8.523 Published |
| `EXPLAIN ANALYZE` 10 truy vấn thật | ✅ sau B5 không còn Seq Scan trên `Recipes` |
| Frontend `tsc --noEmit` / `npm run lint` / `jest` | ✅ không lỗi / không lỗi / 52 test đạt |
| Trình duyệt thật (Playwright + Chromium, `next dev` nối API thật) | ✅ 23 kiểm tra đạt — xem mục 6.1 |

### 6.1 Kiểm tra giao diện bằng trình duyệt

| Kịch bản | Kết quả |
|---|---|
| Gõ `pho bo` ở header + Enter | ✅ sang `/search?q=pho%20bo`, có thẻ `<mark>` tô "Phở", "bò" |
| Debounce trên `/search` (gõ `bun bo`, giữ `difficulty=Easy`) | ✅ đúng **1** request, phát ra ~300 ms sau phím cuối; URL thành `?q=bun+bo&difficulty=Easy` |
| `/search?q=p` | ✅ trang 200, hiện nhắc nhập thêm, không gọi API |
| `/search?q=zzzxxqq` | ✅ empty state có gợi ý (tìm không dấu, bỏ lọc, link `/recipes`, `/categories`) |
| `/recipes`: độ khó Hard + nấu ≤ 30 phút → Áp dụng | ✅ URL `?difficulty=Hard&maxCookTime=30` |
| Đổi sắp xếp tên A→Z | ✅ `sortBy=title&sortOrder=asc`, không có `sort=` |
| Trang 2 → Back → Forward → Reload | ✅ giữ bộ lọc + sort; form hiển thị đúng giá trị sau Back |
| Xóa lọc | ✅ bỏ tham số lọc, giữ sort |
| `/recipes?sortBy=password&page=abc` | ✅ trang 200, hiển thị danh sách mặc định |
| `/categories/mon-khai-vi` đổi sắp xếp | ✅ `?sortBy=cookTime`; không có `FilterPanel` |
| Mobile 390 px: mở drawer, Escape, áp dụng lọc trong drawer | ✅ mở/đóng đúng; URL `?difficulty=Easy`; badge "Bộ lọc 1" |
| Lỗi JavaScript trên trang | ✅ không có |

### 6.2 Môi trường kiểm chứng

- Lần chạy đầu trong môi trường đo của tôi không có Docker nên 11 integration test cần MinIO không chạy được (135/146). Sau đó chạy lại toàn bộ solution trên máy dev bằng Docker thật: **319/319 đạt**.
- `EXPLAIN ANALYZE` đo trên PostgreSQL 16.15 chạy trực tiếp (không phải container `docker compose`); schema, dữ liệu và SQL đều là của dự án. Cách chạy lại trên Docker ở `docs/explain-analyze-b5.md`.
- `global.json` của repo ghim SDK `10.0.401`; bản build đo của tôi tạm dùng SDK 10.0.112 và **không đổi** `global.json` trong repo.

## 7. Danh sách file thay đổi

**Backend (mới):** `Migrations/20261007180000_B5_Search_CompositeIndexes.cs`, `Seed/PerformanceSeeder.cs`. **Backend (sửa):** `Configurations/RecipeConfiguration.cs`, `Migrations/CulinaryBlogDbContextModelSnapshot.cs`, `Infrastructure/DependencyInjection.cs`, `API/Program.cs`.
**Frontend (mới):** `app/search/page.tsx`, `features/recipes/{search-params,highlight}.ts` (+ `.test.ts`), `components/{FilterPanel,SortSelect,SearchBar,HighlightText}.tsx`. **Frontend (sửa):** `app/recipes/(list)/page.tsx`, `app/categories/[slug]/page.tsx`, `components/layout/SiteHeader.tsx`, `features/recipes/api.ts`, `features/recipes/components/RecipeCard.tsx`, `features/categories/api.ts`, `types/api.ts`.
**Tài liệu:** `docs/explain-analyze-b5.md`, `docs/explain-analyze-b5.sql`, báo cáo này (`.md` + `.docx`).

## 8. Commit trên nhánh `2314291_DoanHongTien_buoiso5`

1. [`bd9c0f7`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/bd9c0f7) — `feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze`
2. `docs(perf): explain analyze on real EF SQL and migrated schema, lab 05 report` — số đo thật, báo cáo `.md` + `.docx`
