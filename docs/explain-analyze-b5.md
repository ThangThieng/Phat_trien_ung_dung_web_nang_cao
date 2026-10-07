# EXPLAIN ANALYZE – Buổi 5 (Dev 3) · NFR-PERF-004

> **Tiêu chí (SRS NFR-PERF-004):** mọi truy vấn danh sách/tìm kiếm phải dùng Index Scan, chứng minh bằng `EXPLAIN ANALYZE`; không chấp nhận Seq Scan trên bảng `Recipes`. Ba composite index bắt buộc (SRS §7.2): `IDX_Recipe_List`, `IDX_Recipe_ByCategory`, `IDX_Recipe_CookTime` — tạo bởi migration `B5_Search_CompositeIndexes`.

## ⚠️ Phạm vi của số đo này (đọc trước)

Số đo dưới đây lấy trên **PostgreSQL 16.15 dựng riêng**, bảng `Recipes` tạo **bằng tay theo snapshot EF** (cùng cột, cùng các index đơn cột cũ, cùng `SearchVector` generated column), nạp **10.100 bản ghi** (100 "thật" + 10.000 kiểu `PerformanceSeeder`, ~85 % Published), `VACUUM ANALYZE` trước khi đo. Câu SQL là **bản mô phỏng** câu EF Core sinh ra (cùng điều kiện `IsDeleted = false AND Status = 1`, cùng `ORDER BY ..., Id`, `LIMIT 12`, join `AspNetUsers`/`Categories`), **không** phải log SQL thật của ứng dụng. Môi trường đo không có .NET/Docker nên **chưa chạy trên database Docker của dự án**. Cần chạy lại các bước ở mục "Chạy lại trên database của dự án" và dán kết quả thật vào mục cuối trước khi nộp.

## Kết quả (10.100 bản ghi, 8.563 Published)

| # | Truy vấn (mô phỏng) | Trước B5 (chỉ index đơn cột) | Sau B5 (3 composite index) |
|---|---|---|---|
| Q1 | `COUNT(*)` — tổng cho phân trang `/recipes` | **Seq Scan** · 3,07 ms | **Index Only Scan** `IDX_Recipe_List` · 1,68 ms |
| Q2 | `/recipes` mặc định (`createdAt desc`), trang 1 | Index Scan Backward `IDX_Recipe_CreatedAt` · 0,16 ms | không đổi · 0,18 ms |
| Q3 | `sortBy=publishedAt desc` (trang chủ), trang 1 | Index Scan Backward `IDX_Recipe_PublishedAt` · 0,75 ms | Index Scan `IDX_Recipe_List` · 0,13 ms |
| Q4 | lọc `categoryId` + `publishedAt desc` | Index Scan Backward `IDX_Recipe_PublishedAt` · 0,68 ms | Index Scan `IDX_Recipe_ByCategory` · 0,11 ms |
| Q5 | `maxCookTime ≤ 30`, `sortBy=cookTime asc` | **Seq Scan** · 1,93 ms | Index Scan `IDX_Recipe_CookTime` · 0,22 ms |
| Q6 | tìm kiếm `pho bo` (FTS) | Bitmap Index Scan `IDX_Recipe_Search` (GIN) · 0,48 ms | không đổi · 0,42 ms |

**Đọc kết quả**
- Hai truy vấn Seq Scan trước B5 (Q1, Q5) đều chuyển sang Index Scan sau B5 → đạt tiêu chí NFR-PERF-004.
- Q2 đã là Index Scan nhờ `IDX_Recipe_CreatedAt` (có từ B1), nên **không cần** thêm index `(Status, CreatedAt DESC)` như kế hoạch dự phòng. Không tạo index theo cảm tính.
- Q3/Q4 vốn dùng index đơn cột nhưng phải lọc `Status`/`IsDeleted` sau khi đọc; composite index lọc ngay trong index nên nhanh hơn ~5–6 lần ở dữ liệu này (con số chỉ mang tính so sánh tương đối, chạy một lần).
- Q1 chỉ thành Index Only Scan khi bảng đã được `VACUUM` (visibility map) — autovacuum của PostgreSQL làm việc này; ngay sau khi nạp hàng loạt chưa vacuum thì planner vẫn có thể chọn Seq Scan.
- Q6 không thay đổi vì B5 không đụng tới GIN `IDX_Recipe_Search` (làm ở B4); đưa vào để bảng đủ mọi truy vấn danh sách/tìm kiếm.

## Vì sao phải đo trên ≥ 10.000 bản ghi

Với 100 bản ghi seed (CR-2026-03) planner luôn chọn Seq Scan vì quét cả bảng rẻ hơn dùng index, nên số đo trên dữ liệu nhỏ không chứng minh được gì. `PerformanceSeeder` sinh bộ dữ liệu lớn, **mặc định tắt**.

## Chạy lại trên database của dự án

1. Bật seeder (không bao giờ bật ở production) và khởi động stack — migration `B5_Search_CompositeIndexes` chạy lúc API khởi động:
   ```bash
   # .env hoặc environment của service api
   PerformanceSeed__Enabled=true      # tùy chọn: PerformanceSeed__RecipeCount=10000
   Seed__Enabled=true                 # cần danh mục + tác giả mẫu
   docker compose --profile dev up -d --build
   ```
2. Cập nhật thống kê rồi chạy các truy vấn trong `psql`:
   ```bash
   docker compose exec postgres psql -U culinary -d culinaryblog -c "VACUUM ANALYZE"
   docker compose exec -T postgres psql -U culinary -d culinaryblog < docs/explain-analyze-b5.sql
   ```
   (Nếu muốn so "trước", `DROP INDEX "IDX_Recipe_List", "IDX_Recipe_ByCategory", "IDX_Recipe_CookTime";` rồi `VACUUM ANALYZE` và chạy lại.)
3. Muốn lấy SQL thật của ứng dụng thay cho bản mô phỏng: bật log EF Core (`Microsoft.EntityFrameworkCore.Database.Command` ở mức Information) và gọi `GET /api/v1/recipes?...`.
4. Tắt `PerformanceSeed__Enabled` sau khi đo; dữ liệu `perf-…` có thể xóa bằng `DELETE FROM "Recipes" WHERE "Slug" LIKE 'perf-%'` (xóa cứng, chỉ trên môi trường đo).

## Kết quả trên database của dự án (điền sau khi chạy thật)

| # | Plan thực tế | Thời gian | Đạt? |
|---|---|---|---|
| Q1 | ☐ | ☐ | ☐ |
| Q2 | ☐ | ☐ | ☐ |
| Q3 | ☐ | ☐ | ☐ |
| Q4 | ☐ | ☐ | ☐ |
| Q5 | ☐ | ☐ | ☐ |
| Q6 | ☐ | ☐ | ☐ |

## Phụ lục — kịch bản đo đã dùng

<details><summary>Schema và dữ liệu mô phỏng</summary>

```sql
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE OR REPLACE FUNCTION unaccent_immutable(text) RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$ LANGUAGE sql IMMUTABLE STRICT;

CREATE TABLE "AspNetUsers"("Id" varchar(450) PRIMARY KEY, "DisplayName" text, "AvatarUrl" text);
CREATE TABLE "Categories"("Id" uuid PRIMARY KEY, "Name" varchar(100), "Slug" varchar(120), "IsDeleted" boolean NOT NULL DEFAULT false);
CREATE TABLE "Recipes"(
 "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
 "Title" varchar(200) NOT NULL, "Slug" varchar(220) NOT NULL, "Description" text NOT NULL,
 "PrepTimeMinutes" int NOT NULL, "CookTimeMinutes" int NOT NULL, "Servings" int NOT NULL,
 "Difficulty" smallint NOT NULL DEFAULT 1, "Status" smallint NOT NULL DEFAULT 0,
 "CategoryId" uuid NOT NULL REFERENCES "Categories"("Id"), "AuthorId" varchar(450) NOT NULL REFERENCES "AspNetUsers"("Id"),
 "PublishedAt" timestamptz, "CreatedAt" timestamptz NOT NULL, "UpdatedAt" timestamptz, "IsDeleted" boolean NOT NULL DEFAULT false,
 "SearchVector" tsvector GENERATED ALWAYS AS (to_tsvector('simple', unaccent_immutable(coalesce("Title",'') || ' ' || coalesce("Description",'')))) STORED
);
-- chỉ số ĐƠN CỘT đã có từ Buổi 2/4 (đúng như snapshot)
CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;
CREATE INDEX "IDX_Recipe_Status" ON "Recipes"("Status");
CREATE INDEX "IDX_Recipe_CategoryId" ON "Recipes"("CategoryId");
CREATE INDEX "IDX_Recipe_AuthorId" ON "Recipes"("AuthorId");
CREATE INDEX "IDX_Recipe_Difficulty" ON "Recipes"("Difficulty");
CREATE INDEX "IDX_Recipe_PublishedAt" ON "Recipes"("PublishedAt");
CREATE INDEX "IDX_Recipe_CreatedAt" ON "Recipes"("CreatedAt");
CREATE INDEX "IDX_Recipe_IsDeleted" ON "Recipes"("IsDeleted") WHERE "IsDeleted" = false;
CREATE INDEX "IDX_Recipe_Search" ON "Recipes" USING GIN("SearchVector");

INSERT INTO "AspNetUsers" SELECT 'author'||g, 'Tác giả '||g, NULL FROM generate_series(1,5) g;
INSERT INTO "Categories" SELECT gen_random_uuid(), 'Danh mục '||g, 'danh-muc-'||g FROM generate_series(1,20) g;

-- 100 công thức "thật" + 10.000 công thức PerformanceSeeder (85% Published, ngày xuất bản trải 365 ngày, CreatedAt tăng dần)
INSERT INTO "Recipes"("Title","Slug","Description","PrepTimeMinutes","CookTimeMinutes","Servings","Difficulty","Status","CategoryId","AuthorId","PublishedAt","CreatedAt")
SELECT
  (ARRAY['Phở','Bún','Cơm','Gỏi','Canh','Chè','Bánh','Lẩu','Nem','Xôi','Cháo','Mì'])[1+floor(random()*12)::int]||' '||
  (ARRAY['bò','gà','heo','tôm','cá','chay','nấm','rau củ','hải sản','trứng'])[1+floor(random()*10)::int]||' '||g,
  CASE WHEN g<=100 THEN 'real-'||g ELSE 'perf-'||lpad((g-100)::text,6,'0') END,
  'Công thức mẫu số '||g,
  5+floor(random()*56)::int, floor(random()*181)::int, 1+floor(random()*8)::int, 1+floor(random()*4)::int,
  CASE WHEN random()<0.85 THEN 1 ELSE 0 END,
  (SELECT "Id" FROM "Categories" OFFSET floor(random()*20)::int LIMIT 1),
  'author'||(1+floor(random()*5)::int),
  now() - (random()*365||' days')::interval,
  now() - ((10100-g)||' seconds')::interval
FROM generate_series(1,10100) g;
UPDATE "Recipes" SET "PublishedAt"=NULL WHERE "Status"=0;
ANALYZE;
```
</details>

<details><summary>Index B5</summary>

```sql
CREATE INDEX "IDX_Recipe_List" ON "Recipes"("IsDeleted","Status","PublishedAt" DESC);
CREATE INDEX "IDX_Recipe_ByCategory" ON "Recipes"("Status","CategoryId","PublishedAt" DESC);
CREATE INDEX "IDX_Recipe_CookTime" ON "Recipes"("Status","CookTimeMinutes");
ANALYZE;
```
</details>

<details><summary>Truy vấn đo (cũng là nội dung <code>docs/explain-analyze-b5.sql</code>)</summary>

```sql
\echo === Q1 /recipes: COUNT (đếm tổng)
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT count(*) FROM "Recipes" r WHERE r."IsDeleted" = false AND r."Status" = 1;
\echo === Q2 /recipes mặc định (sortBy=createdAt desc) trang 1
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT r."Id", r."Title", c."Name", u."DisplayName" FROM "Recipes" r JOIN "AspNetUsers" u ON r."AuthorId"=u."Id" JOIN "Categories" c ON r."CategoryId"=c."Id" WHERE r."IsDeleted" = false AND r."Status" = 1 ORDER BY r."CreatedAt" DESC, r."Id" LIMIT 12 OFFSET 0;
\echo === Q3 sortBy=publishedAt desc (trang chủ) trang 1
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT r."Id", r."Title", c."Name", u."DisplayName" FROM "Recipes" r JOIN "AspNetUsers" u ON r."AuthorId"=u."Id" JOIN "Categories" c ON r."CategoryId"=c."Id" WHERE r."IsDeleted" = false AND r."Status" = 1 ORDER BY r."PublishedAt" DESC, r."Id" LIMIT 12 OFFSET 0;
\echo === Q4 lọc categoryId + publishedAt desc
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT r."Id", r."Title" FROM "Recipes" r WHERE r."IsDeleted" = false AND r."Status" = 1 AND r."CategoryId" = (SELECT "Id" FROM "Categories" ORDER BY "Slug" OFFSET 3 LIMIT 1) ORDER BY r."PublishedAt" DESC, r."Id" LIMIT 12 OFFSET 0;
\echo === Q5 lọc maxCookTime<=30, sortBy=cookTime asc
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT r."Id", r."Title" FROM "Recipes" r WHERE r."IsDeleted" = false AND r."Status" = 1 AND r."CookTimeMinutes" <= 30 ORDER BY r."CookTimeMinutes", r."Id" LIMIT 12 OFFSET 0;
\echo === Q6 tìm kiếm FTS "pho bo"
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF) SELECT r."Id", ts_rank(r."SearchVector", to_tsquery('simple','pho:* & bo:*')) FROM "Recipes" r WHERE r."IsDeleted" = false AND r."Status" = 1 AND r."SearchVector" @@ to_tsquery('simple','pho:* & bo:*') ORDER BY 2 DESC, r."PublishedAt" DESC, r."Id" LIMIT 12;
```
</details>
