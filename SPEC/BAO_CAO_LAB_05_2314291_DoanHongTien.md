# BÁO CÁO KẾT QUẢ BUỔI 5 — DEV 3

**Dev 3:** Đoàn Hồng Tiến — MSSV 2314291 — Nhóm 20 — Kỹ sư Nội dung (Category / Recipe read / Search)
**Nhánh dự kiến:** `2314291_DoanHongTien_buoiso5` (tách từ `develop` đã gồm code Buổi 4 của cả 4 dev) — **PR nhắm vào `develop`**: `<điền link PR>`
**Phạm vi (KE_HOACH_PHAT_TRIEN_8_BUOI.md — Buổi 5, Dev 3):** giao diện FR-SRCH-001 (thanh tìm kiếm + trang `/search`), giao diện FR-SRCH-002/003/004 (`FilterPanel`, `SortSelect`, `Pagination` đồng bộ URL) kèm **retrofit D-10 phía FE**, composite index + `EXPLAIN ANALYZE` (NFR-PERF-004).
**Tài liệu bám sát:** `SPEC/SRS_Culinary_Blog_v1.2.2.md` (§3.4 FR-SRCH, §5.1, §7.2, §8.2, §8.3, NFR-PERF-004) và `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` (MT-01, MT-26, MT-57; §4 D-10).

> **Trạng thái kiểm chứng (07/10/2026):** frontend đã chạy thật `tsc`, `eslint`, `jest`, `next build` và một lượt SSR thử với API giả. Backend **chưa build/test được** (môi trường làm việc không có .NET SDK và Docker). Số đo `EXPLAIN ANALYZE` lấy trên PostgreSQL 16 dựng riêng, **chưa chạy trên database Docker của dự án**. Các hạng mục chưa kiểm chứng ghi rõ ở mục 7.

---

## 1. Tóm tắt công việc

| # | Hạng mục | Yêu cầu trong kế hoạch | Kết quả |
|---|---|---|---|
| A | Migration `B5_Search_CompositeIndexes` | 3 composite index của SRS §7.2 | Code xong; chưa chạy `dotnet build`/migrate — mục 2 |
| B | `PerformanceSeeder` | ≥ 10.000 recipe, chỉ bật bằng cờ cấu hình | Code xong (mặc định tắt); chưa build — mục 2 |
| C | `EXPLAIN ANALYZE` | Index Scan, không Seq Scan trên `Recipes` | Đã đo trên PG 16 dựng riêng, đạt — `docs/explain-analyze-b5.md` |
| D | `SearchBar` + trang `/search` | debounce 300 ms, Enter → `/search?q=`, SSR, highlight, empty state | Xong, đã kiểm SSR với API giả — mục 3 |
| E | `FilterPanel`, `SortSelect`, `Pagination` đồng bộ URL | Toàn bộ state trong `searchParams`; drawer trên mobile | Xong; drawer và debounce chưa thử trong trình duyệt thật — mục 3 |
| F | Retrofit D-10 phía FE | Bỏ `sort=-createdAt`, dùng cặp `sortBy`/`sortOrder` | Xong — mục 4 |

## 2. Backend

- `Persistence/Migrations/20261007180000_B5_Search_CompositeIndexes.cs` (viết tay, theo tiền lệ B3/B4): `IDX_Recipe_List (IsDeleted, Status, PublishedAt DESC)`, `IDX_Recipe_ByCategory (Status, CategoryId, PublishedAt DESC)`, `IDX_Recipe_CookTime (Status, CookTimeMinutes)`. Khai báo tương ứng trong `RecipeConfiguration` và cập nhật `CulinaryBlogDbContextModelSnapshot.cs` bằng tay.
- `Persistence/Seed/PerformanceSeeder.cs` + `PerformanceSeedOptions` (`PerformanceSeed:Enabled`, mặc định `false`; `RecipeCount` mặc định 10.000). Chạy sau `DatabaseSeeder` trong `Program.cs`; idempotent (nhận diện slug `perf-…`); ~85 % Published; Bogus với seed cố định.
- **Không** tạo index `(Status, CreatedAt DESC)` dự phòng: truy vấn `createdAt desc` đã Index Scan nhờ `IDX_Recipe_CreatedAt` (có từ B1). NFR-PERF-004: không tạo index theo cảm tính.

**Kết quả đo (PG 16.15, 10.100 bản ghi, 8.563 Published, `VACUUM ANALYZE`):**

| Truy vấn | Trước B5 | Sau B5 |
|---|---|---|
| COUNT tổng | Seq Scan · 3,07 ms | Index Only Scan `IDX_Recipe_List` · 1,68 ms |
| `publishedAt desc` | Index Scan Backward (`IDX_Recipe_PublishedAt`) · 0,75 ms | Index Scan `IDX_Recipe_List` · 0,13 ms |
| `categoryId` + `publishedAt desc` | Index Scan Backward (`IDX_Recipe_PublishedAt`) · 0,68 ms | Index Scan `IDX_Recipe_ByCategory` · 0,11 ms |
| `maxCookTime ≤ 30`, sort `cookTime` | Seq Scan · 1,93 ms | Index Scan `IDX_Recipe_CookTime` · 0,22 ms |

Chi tiết, cách chạy lại và bảng để điền kết quả trên DB thật: `docs/explain-analyze-b5.md`, truy vấn: `docs/explain-analyze-b5.sql`.

## 3. Frontend — chức năng

| Thành phần | File | Ghi chú |
|---|---|---|
| Trạng thái URL dùng chung | `features/recipes/search-params.ts` | `parseRecipeListState` / `parseCategoryState` / `parseSearchState`, `buildQueryString`, `buildHref`. Bỏ giá trị mặc định khỏi URL; đổi lọc/sắp xếp thì về trang 1 |
| `SearchBar` | `features/recipes/components/SearchBar.tsx` | Enter → `/search?q=`; đang ở `/search` thì gõ xong 300 ms tự cập nhật (`router.replace`, giữ bộ lọc); dưới 2 ký tự không gọi |
| Trang `/search` | `app/search/page.tsx` | SSR; `q` < 2 ký tự không gọi API (sẽ là 400); highlight không dấu theo đầu từ; empty state có gợi ý (FR-SRCH-001 A2); form GET thuần cho mobile/không JS |
| `FilterPanel` | `.../FilterPanel.tsx` | `/recipes`: danh mục, độ khó (4 mức), thời gian nấu, chuẩn bị, khẩu phần. `/search`: chỉ danh mục + độ khó (SRS §8.3). Dưới `lg` là drawer (Escape/nền/nút đóng) |
| `SortSelect` | `.../SortSelect.tsx` | Hai dropdown cột/chiều ↔ `sortBy`/`sortOrder` (whitelist 5 cột) |
| Highlight | `features/recipes/highlight.ts`, `HighlightText.tsx`, `RecipeCard.tsx` (prop `terms`) | So khớp không dấu (`pho` tô `Phở`), khớp tiền tố như `pho:*` ở backend |
| Lớp gọi API | `features/recipes/api.ts`, `features/categories/api.ts` | `searchRecipes`, `toRecipeListParams`; kiểu `RecipeSearchResult` trong `types/api.ts` |

Phạm vi bộ lọc theo từng trang bám đúng hợp đồng API của SRS: `/recipes` đủ 5 bộ lọc + sort (§8.3); `/search` chỉ `categoryId`, `difficulty` và xếp theo `ts_rank` nên không có sắp xếp (§8.3); `/categories/[slug]` chỉ `sortBy`/`sortOrder` (§8.2). **Không mở rộng API** ngoài SRS.

## 4. Retrofit D-10 phía FE

- Xóa hàm cầu nối `toSortParams` (Buổi 4) và `sort=-createdAt` trong `app/recipes/(list)/page.tsx`; `RecipeListParams` dùng `sortBy`/`sortOrder`.
- Đã kiểm với API giả: yêu cầu gửi đi là `GET /recipes?page=2&pageSize=12&difficulty=Hard&maxCookTime=30&sortBy=title&sortOrder=asc` — không còn tham số `sort`.
- Giá trị hỏng trong URL gõ tay (ví dụ `sortBy=password`, `page=abc`) được đưa về mặc định **ở phía giao diện** để người đọc không gặp trang lỗi; API vẫn trả 400 cho client gọi trực tiếp (FR-SRCH-003), nên không có giá trị nào bị bỏ qua im lặng ở API.

## 5. Điểm lệch / quyết định (theo SRS)

| # | Điểm | Căn cứ | Cách xử lý |
|---|---|---|---|
| 1 | SRS §5.1 / MT-57: `/categories/[slug]` là SSR + Data Cache `revalidate: 120` (≤ TTL `categories:detail` 2 phút) | SRS §5.1 | `getCategoryBySlug` đặt `revalidate: 120` + tag, đồng thời nhận `sortBy`/`sortOrder` |
| 2 | `getCategories` `revalidate: 3600` > TTL API 30 phút (SRS §5.1 ghi 1800) | SRS §5.1, D-13 | **Không sửa** — không thuộc việc Buổi 5; ghi nhận để xử lý cùng ISR/SEO ở Buổi 6 |
| 3 | Kế hoạch ghi "dùng chung bộ lọc trên `/categories/[slug]`" nhưng SRS §8.2 chỉ cho danh mục nhận `sortBy`/`sortOrder` | SRS §8.2 | Danh mục chỉ có `SortSelect`, không có `FilterPanel` |

## 6. Test và kiểm chứng đã chạy (trên bản sao môi trường Linux)

| Lệnh | Kết quả |
|---|---|
| `npx tsc --noEmit` | ✅ không lỗi |
| `npm run lint` | ✅ không lỗi (đã sửa các lỗi `jsx-a11y/label-has-associated-control` và `react/jsx-no-useless-fragment` phát hiện lúc chạy) |
| `npx jest` | ✅ 52 test đạt (gồm `search-params.test.ts`, `highlight.test.ts` mới) |
| `next build` | ✅ biên dịch + kiểm kiểu thành công, `/search` và `/recipes` là dynamic (đã thay font Google bằng bản giả trong bản sao thử vì môi trường không có mạng tới Google Fonts; repo không bị đổi) |
| SSR thử với API giả (`next start` + máy chủ giả) | ✅ `/search?q=pho%20bo` tô sáng `Phở`, `bò`; `q=p` và `/search` không gọi API; `/recipes` và `/categories/bun-pho` gửi đúng `sortBy`/`sortOrder` |

## 7. Chưa kiểm chứng / hạn chế

- **Backend chưa build và chưa chạy test** (`dotnet build`, `dotnet test`, `dotnet ef migrations has-pending-model-changes` đều chưa chạy). Snapshot được sửa tay nên cần chạy `has-pending-model-changes` để chắc không báo thay đổi. `PerformanceSeeder` cần build qua analyzer (TreatWarningsAsErrors).
- `EXPLAIN ANALYZE` chưa chạy trên DB Docker của dự án; SQL là bản mô phỏng câu EF sinh ra (xem `docs/explain-analyze-b5.md`).
- Chưa thử trong trình duyệt thật: drawer mobile, debounce 300 ms của `SearchBar`, back/forward với bộ lọc. Chưa có test jest cho `FilterPanel`/`SortSelect`/`SearchBar`.
- Chưa có Playwright/axe; kiểm tra WCAG thuộc Buổi 8.

## 8. Danh sách file thay đổi

**Backend (mới):** `Migrations/20261007180000_B5_Search_CompositeIndexes.cs`, `Seed/PerformanceSeeder.cs`. **Backend (sửa):** `Configurations/RecipeConfiguration.cs`, `Migrations/CulinaryBlogDbContextModelSnapshot.cs`, `Infrastructure/DependencyInjection.cs`, `API/Program.cs`.
**Frontend (mới):** `app/search/page.tsx`, `features/recipes/{search-params,highlight}.ts` (+ `.test.ts`), `components/{FilterPanel,SortSelect,SearchBar,HighlightText}.tsx`. **Frontend (sửa):** `app/recipes/(list)/page.tsx`, `app/categories/[slug]/page.tsx`, `components/layout/SiteHeader.tsx`, `features/recipes/api.ts`, `features/recipes/components/RecipeCard.tsx`, `features/categories/api.ts`, `types/api.ts`.
**Tài liệu:** `docs/explain-analyze-b5.md`, `docs/explain-analyze-b5.sql`.

## 9. Commit dự kiến

```
feat(search-ui): search page and url-synced filters, sort and pagination (D-10 FE) with composite indexes proven by explain analyze
```

Link: `<điền link commit sau khi push>`
