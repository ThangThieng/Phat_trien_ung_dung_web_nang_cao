# EXPLAIN ANALYZE – Buổi 5 (Dev 3) · NFR-PERF-004

> **Tiêu chí (SRS NFR-PERF-004):** mọi truy vấn danh sách/tìm kiếm phải dùng Index Scan, chứng minh bằng `EXPLAIN ANALYZE`; không chấp nhận Seq Scan trên bảng `Recipes`. Ba composite index bắt buộc (SRS §7.2): `IDX_Recipe_List`, `IDX_Recipe_ByCategory`, `IDX_Recipe_CookTime` — tạo bởi migration `B5_Search_CompositeIndexes`.

## Môi trường đo (12/10/2026)

| Hạng mục | Giá trị |
|---|---|
| PostgreSQL | 16.15 (cùng major với image `postgres:16-alpine` của `docker-compose.yml`), cấu hình mặc định |
| Schema | Tạo bằng **chính các migration của dự án** (`B1_InitialSchema` → `B5_Search_CompositeIndexes`) khi API khởi động với `Database__MigrateOnStartup=true` |
| Dữ liệu | `DatabaseSeeder` (100 công thức, 20 danh mục) + `PerformanceSeeder` (`PerformanceSeed__Enabled=true`) = **10.100 công thức, 8.523 Published** |
| SQL | **SQL thật do EF Core sinh ra**, lấy từ log `Microsoft.EntityFrameworkCore.Database.Command` khi gọi API; tham số được thay bằng giá trị cụ thể (`pageSize=12`, `page=1`) |
| Cách đo | `VACUUM ANALYZE` trước khi đo; mỗi câu chạy 3 lần, lấy lần cuối. "Trước B5" = cùng database, `DROP INDEX` ba composite index trong một transaction rồi `ROLLBACK` |
| Kiểm tra mô hình | `HasPendingModelChanges() = False` sau migration B5 (snapshot khớp model) |

> Số đo là từ một PostgreSQL 16 chạy trực tiếp (môi trường đo không có Docker daemon), không phải container trong `docker compose` của dự án. Schema, dữ liệu và SQL đều là của dự án nên plan sẽ giống; thời gian tuyệt đối thay đổi theo máy. Cách chạy lại trên Docker ở mục cuối.

## Kết quả

| # | Truy vấn (endpoint) | Trước B5 (chỉ index đơn cột) | Sau B5 (3 composite index) |
|---|---|---|---|
| Q1 | `/recipes` — COUNT tổng (phân trang) | **Seq Scan** · 5,826 ms | Index Only Scan `IDX_Recipe_List` · 2,818 ms |
| Q2 | `/recipes` mặc định `createdAt desc`, trang 1 | Index Scan Backward `IDX_Recipe_CreatedAt` · 1,214 ms | Index Scan Backward `IDX_Recipe_CreatedAt` · 1,319 ms |
| Q3 | `sortBy=publishedAt&sortOrder=desc`, trang 1 | Index Scan Backward `IDX_Recipe_PublishedAt` · 1,155 ms | Index Scan `IDX_Recipe_List` · 0,151 ms |
| Q4a | `categoryId` — COUNT | Bitmap Index Scan `IDX_Recipe_CategoryId` · 0,963 ms | Bitmap Index Scan `IDX_Recipe_CategoryId` · 0,452 ms |
| Q4b | `categoryId` + `publishedAt desc`, trang 1 | Index Scan Backward `IDX_Recipe_PublishedAt` · 1,508 ms | Index Scan `IDX_Recipe_ByCategory` · 0,136 ms |
| Q5a | `maxCookTime=30` — COUNT | **Seq Scan** · 3,435 ms | Bitmap Index Scan `IDX_Recipe_CookTime` · 1,798 ms |
| Q5b | `maxCookTime=30` + `cookTime asc`, trang 1 | **Seq Scan** · 3,126 ms | Index Scan `IDX_Recipe_CookTime` · 0,199 ms |
| Q6a | Tìm kiếm `q=pho bo` — COUNT | Bitmap Index Scan `IDX_Recipe_Search` · 0,363 ms | Bitmap Index Scan `IDX_Recipe_Search` · 0,412 ms |
| Q6b | Tìm kiếm `q=pho bo` — xếp hạng `ts_rank`, trang 1 | Bitmap Index Scan `IDX_Recipe_Search` · 0,572 ms | Bitmap Index Scan `IDX_Recipe_Search` · 0,527 ms |
| Q7 | `/categories/{slug}` `publishedAt desc`, trang 1 | Index Scan Backward `IDX_Recipe_PublishedAt` · 1,265 ms | Index Scan `IDX_Recipe_ByCategory` · 0,216 ms |

**Đọc kết quả**
- **Sau B5 không còn Seq Scan nào trên `Recipes`** → đạt NFR-PERF-004. Ba truy vấn còn Seq Scan trước B5 (Q1, Q5a, Q5b — đếm tổng và lọc `maxCookTime`) đều chuyển sang dùng index.
- Mỗi index mới đều được planner dùng: `IDX_Recipe_List` (Q1, Q3), `IDX_Recipe_ByCategory` (Q4b, Q7), `IDX_Recipe_CookTime` (Q5a, Q5b).
- Truy vấn trang 1 có `ORDER BY` khớp index nhanh hơn rõ rệt (Q3, Q4b, Q7, Q5b: ~6–15 lần) vì index trả đúng thứ tự, chỉ đọc khoảng 13 dòng thay vì lọc `Status`/`IsDeleted` sau khi đọc.
- Q2 (`createdAt desc`, mặc định của `/recipes`) đã là Index Scan nhờ `IDX_Recipe_CreatedAt` có từ B1, nên **không** thêm index dự phòng `(Status, CreatedAt DESC)` — không tạo index theo cảm tính.
- Q4a/Q6a/Q6b dùng `IDX_Recipe_CategoryId` và GIN `IDX_Recipe_Search` có sẵn (Bitmap Index Scan); B5 không đổi các truy vấn này.
- Seq Scan còn thấy trong plan chỉ nằm ở `Categories` (20 dòng), `AspNetUsers` (6 dòng) và `RecipeImages` (rỗng) — bảng nhỏ, planner chọn đúng; tiêu chí chỉ áp cho `Recipes`.
- Q1 thành Index Only Scan nhờ visibility map sau `VACUUM` (autovacuum làm việc này); ngay sau khi nạp hàng loạt chưa vacuum, planner có thể tạm chọn cách khác.

## Vì sao phải đo trên ≥ 10.000 bản ghi

Với 100 bản ghi seed (CR-2026-03), planner luôn chọn Seq Scan vì quét cả bảng rẻ hơn dùng index, nên số đo trên dữ liệu nhỏ không chứng minh được gì. `PerformanceSeeder` sinh bộ dữ liệu lớn, **mặc định tắt**.

## Chạy lại trên Docker của dự án

1. Bật seeder (không bao giờ bật ở production) và khởi động stack — migration chạy lúc API khởi động:
   ```bash
   # .env hoặc environment của service api
   PerformanceSeed__Enabled=true      # tùy chọn: PerformanceSeed__RecipeCount=10000
   Seed__Enabled=true                 # cần danh mục + tác giả mẫu
   docker compose --profile dev up -d --build
   ```
2. Cập nhật thống kê rồi chạy file truy vấn (SQL thật của EF, danh mục lấy theo slug `mon-khai-vi`):
   ```bash
   docker compose exec postgres psql -U culinary -d culinaryblog -c "VACUUM ANALYZE"
   docker compose exec -T postgres psql -U culinary -d culinaryblog < docs/explain-analyze-b5.sql
   ```
   Muốn so "trước": chạy trong `BEGIN; DROP INDEX "IDX_Recipe_List", "IDX_Recipe_ByCategory", "IDX_Recipe_CookTime"; … ROLLBACK;`.
3. Tắt `PerformanceSeed__Enabled` sau khi đo; dữ liệu `perf-…` xóa được bằng `DELETE FROM "Recipes" WHERE "Slug" LIKE 'perf-%'` (xóa cứng, chỉ trên môi trường đo).

## Phụ lục — plan đầy đủ sau B5

Truy vấn tương ứng nằm trong `docs/explain-analyze-b5.sql`.

<details><summary>Q1 — `/recipes` — COUNT tổng (phân trang): plan sau B5</summary>

```
Aggregate  (cost=360.05..360.07 rows=1 width=4) (actual time=2.791..2.791 rows=1 loops=1)
  Buffers: shared hit=47
  ->  Index Only Scan using "IDX_Recipe_List" on "Recipes" r  (cost=0.29..338.75 rows=8523 width=0) (actual time=0.021..2.027 rows=8523 loops=1)
        Index Cond: (("IsDeleted" = false) AND ("Status" = 1))
        Heap Fetches: 0
        Buffers: shared hit=47
Planning Time: 0.163 ms
Execution Time: 2.818 ms
```
</details>

<details><summary>Q2 — `/recipes` mặc định `createdAt desc`, trang 1: plan sau B5</summary>

```
Sort  (cost=83.48..83.51 rows=12 width=1241) (actual time=1.229..1.233 rows=12 loops=1)
  Sort Key: r."CreatedAt" DESC, r."Id"
  Sort Method: quicksort  Memory: 27kB
  Buffers: shared hit=56
  ->  Hash Join  (cost=81.79..83.27 rows=12 width=1241) (actual time=1.097..1.131 rows=12 loops=1)
        Hash Cond: (r."CategoryId" = c."Id")
        Buffers: shared hit=56
        ->  Hash Join  (cost=80.34..81.78 rows=12 width=704) (actual time=1.038..1.052 rows=12 loops=1)
              Hash Cond: ((r."AuthorId")::text = a."Id")
              Buffers: shared hit=55
              ->  Limit  (cost=79.21..80.47 rows=12 width=172) (actual time=1.017..1.021 rows=12 loops=1)
                    Buffers: shared hit=54
                    ->  Incremental Sort  (cost=79.21..975.18 rows=8523 width=172) (actual time=1.016..1.018 rows=12 loops=1)
                          Sort Key: r."CreatedAt" DESC, r."Id"
                          Presorted Key: r."CreatedAt"
                          Full-sort Groups: 1  Sort Method: top-N heapsort  Average Memory: 28kB  Peak Memory: 28kB
                          Pre-sorted Groups: 1  Sort Method: top-N heapsort  Average Memory: 28kB  Peak Memory: 28kB
                          Buffers: shared hit=54
                          ->  Index Scan Backward using "IDX_Recipe_CreatedAt" on "Recipes" r  (cost=0.29..673.04 rows=8523 width=172) (actual time=0.016..0.628 rows=835 loops=1)
                                Filter: ((NOT "IsDeleted") AND ("Status" = 1))
                                Rows Removed by Filter: 166
                                Buffers: shared hit=54
              ->  Hash  (cost=1.06..1.06 rows=6 width=569) (actual time=0.009..0.010 rows=6 loops=1)
                    Buckets: 1024  Batches: 1  Memory Usage: 9kB
                    Buffers: shared hit=1
                    ->  Seq Scan on "AspNetUsers" a  (cost=0.00..1.06 rows=6 width=569) (actual time=0.004..0.006 rows=6 loops=1)
                          Buffers: shared hit=1
        ->  Hash  (cost=1.20..1.20 rows=20 width=37) (actual time=0.030..0.031 rows=20 loops=1)
              Buckets: 1024  Batches: 1  Memory Usage: 10kB
              Buffers: shared hit=1
              ->  Seq Scan on "Categories" c  (cost=0.00..1.20 rows=20 width=37) (actual time=0.013..0.021 rows=20 loops=1)
                    Filter: (NOT "IsDeleted")
                    Buffers: shared hit=1
        SubPlan 1
          ->  Limit  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                ->  Seq Scan on "RecipeImages" r1  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                      Filter: ((NOT "IsDeleted") AND "IsPrimary" AND (r."Id" = "RecipeId"))
Planning:
  Buffers: shared hit=11
Planning Time: 0.543 ms
Execution Time: 1.319 ms
```
</details>

<details><summary>Q3 — `sortBy=publishedAt&sortOrder=desc`, trang 1: plan sau B5</summary>

```
Sort  (cost=5.85..5.88 rows=12 width=1241) (actual time=0.114..0.116 rows=12 loops=1)
  Sort Key: r."PublishedAt" DESC, r."Id"
  Sort Method: quicksort  Memory: 27kB
  Buffers: shared hit=17
  ->  Hash Join  (cost=3.06..5.64 rows=12 width=1241) (actual time=0.086..0.104 rows=12 loops=1)
        Hash Cond: (r."CategoryId" = c."Id")
        Buffers: shared hit=17
        ->  Hash Join  (cost=1.61..4.14 rows=12 width=704) (actual time=0.057..0.064 rows=12 loops=1)
              Hash Cond: ((r."AuthorId")::text = a."Id")
              Buffers: shared hit=16
              ->  Limit  (cost=0.47..2.84 rows=12 width=172) (actual time=0.043..0.045 rows=12 loops=1)
                    Buffers: shared hit=15
                    ->  Incremental Sort  (cost=0.47..1679.81 rows=8523 width=172) (actual time=0.043..0.043 rows=12 loops=1)
                          Sort Key: r."PublishedAt" DESC, r."Id"
                          Presorted Key: r."PublishedAt"
                          Full-sort Groups: 1  Sort Method: quicksort  Average Memory: 28kB  Peak Memory: 28kB
                          Buffers: shared hit=15
                          ->  Index Scan using "IDX_Recipe_List" on "Recipes" r  (cost=0.29..1330.68 rows=8523 width=172) (actual time=0.013..0.031 rows=13 loops=1)
                                Index Cond: (("IsDeleted" = false) AND ("Status" = 1))
                                Buffers: shared hit=15
              ->  Hash  (cost=1.06..1.06 rows=6 width=569) (actual time=0.006..0.007 rows=6 loops=1)
                    Buckets: 1024  Batches: 1  Memory Usage: 9kB
                    Buffers: shared hit=1
                    ->  Seq Scan on "AspNetUsers" a  (cost=0.00..1.06 rows=6 width=569) (actual time=0.003..0.004 rows=6 loops=1)
                          Buffers: shared hit=1
        ->  Hash  (cost=1.20..1.20 rows=20 width=37) (actual time=0.014..0.014 rows=20 loops=1)
              Buckets: 1024  Batches: 1  Memory Usage: 10kB
              Buffers: shared hit=1
              ->  Seq Scan on "Categories" c  (cost=0.00..1.20 rows=20 width=37) (actual time=0.005..0.009 rows=20 loops=1)
                    Filter: (NOT "IsDeleted")
                    Buffers: shared hit=1
        SubPlan 1
          ->  Limit  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                ->  Seq Scan on "RecipeImages" r1  (cost=0.00..0.00 rows=1 width=516) (actual time=0.000..0.000 rows=0 loops=12)
                      Filter: ((NOT "IsDeleted") AND "IsPrimary" AND (r."Id" = "RecipeId"))
Planning:
  Buffers: shared hit=11
Planning Time: 0.374 ms
Execution Time: 0.151 ms
```
</details>

<details><summary>Q4a — `categoryId` — COUNT: plan sau B5</summary>

```
Aggregate  (cost=484.53..484.54 rows=1 width=4) (actual time=0.438..0.438 rows=1 loops=1)
  Buffers: shared hit=305
  ->  Bitmap Heap Scan on "Recipes" r  (cost=8.29..483.43 rows=439 width=0) (actual time=0.061..0.416 rows=441 loops=1)
        Recheck Cond: ("CategoryId" = '4027f93f-cb24-41d9-9f7e-2e88da7ef0e2'::uuid)
        Filter: ((NOT "IsDeleted") AND ("Status" = 1))
        Rows Removed by Filter: 79
        Heap Blocks: exact=303
        Buffers: shared hit=305
        ->  Bitmap Index Scan on "IDX_Recipe_CategoryId"  (cost=0.00..8.19 rows=520 width=0) (actual time=0.028..0.028 rows=520 loops=1)
              Index Cond: ("CategoryId" = '4027f93f-cb24-41d9-9f7e-2e88da7ef0e2'::uuid)
              Buffers: shared hit=2
Planning Time: 0.078 ms
Execution Time: 0.452 ms
```
</details>

<details><summary>Q4b — `categoryId` + `publishedAt desc`, trang 1: plan sau B5</summary>

```
Sort  (cost=28.97..29.00 rows=12 width=1241) (actual time=0.104..0.106 rows=12 loops=1)
  Sort Key: r."PublishedAt" DESC, r."Id"
  Sort Method: quicksort  Memory: 27kB
  Buffers: shared hit=17
  ->  Hash Join  (cost=4.82..28.76 rows=12 width=1241) (actual time=0.080..0.096 rows=12 loops=1)
        Hash Cond: (r."CategoryId" = c."Id")
        Buffers: shared hit=17
        ->  Hash Join  (cost=3.37..27.27 rows=12 width=704) (actual time=0.053..0.059 rows=12 loops=1)
              Hash Cond: ((r."AuthorId")::text = a."Id")
              Buffers: shared hit=16
              ->  Limit  (cost=2.24..25.96 rows=12 width=172) (actual time=0.040..0.042 rows=12 loops=1)
                    Buffers: shared hit=15
                    ->  Incremental Sort  (cost=2.24..869.92 rows=439 width=172) (actual time=0.039..0.040 rows=12 loops=1)
                          Sort Key: r."PublishedAt" DESC, r."Id"
                          Presorted Key: r."PublishedAt"
                          Full-sort Groups: 1  Sort Method: quicksort  Average Memory: 28kB  Peak Memory: 28kB
                          Buffers: shared hit=15
                          ->  Index Scan using "IDX_Recipe_ByCategory" on "Recipes" r  (cost=0.29..850.24 rows=439 width=172) (actual time=0.013..0.029 rows=13 loops=1)
                                Index Cond: (("Status" = 1) AND ("CategoryId" = '4027f93f-cb24-41d9-9f7e-2e88da7ef0e2'::uuid))
                                Filter: (NOT "IsDeleted")
                                Buffers: shared hit=15
              ->  Hash  (cost=1.06..1.06 rows=6 width=569) (actual time=0.006..0.006 rows=6 loops=1)
                    Buckets: 1024  Batches: 1  Memory Usage: 9kB
                    Buffers: shared hit=1
                    ->  Seq Scan on "AspNetUsers" a  (cost=0.00..1.06 rows=6 width=569) (actual time=0.002..0.004 rows=6 loops=1)
                          Buffers: shared hit=1
        ->  Hash  (cost=1.20..1.20 rows=20 width=37) (actual time=0.014..0.014 rows=20 loops=1)
              Buckets: 1024  Batches: 1  Memory Usage: 10kB
              Buffers: shared hit=1
              ->  Seq Scan on "Categories" c  (cost=0.00..1.20 rows=20 width=37) (actual time=0.004..0.009 rows=20 loops=1)
                    Filter: (NOT "IsDeleted")
                    Buffers: shared hit=1
        SubPlan 1
          ->  Limit  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                ->  Seq Scan on "RecipeImages" r1  (cost=0.00..0.00 rows=1 width=516) (actual time=0.000..0.000 rows=0 loops=12)
                      Filter: ((NOT "IsDeleted") AND "IsPrimary" AND (r."Id" = "RecipeId"))
Planning:
  Buffers: shared hit=11
Planning Time: 0.297 ms
Execution Time: 0.136 ms
```
</details>

<details><summary>Q5a — `maxCookTime=30` — COUNT: plan sau B5</summary>

```
Aggregate  (cost=500.02..500.03 rows=1 width=4) (actual time=1.773..1.774 rows=1 loops=1)
  Buffers: shared hit=442
  ->  Bitmap Heap Scan on "Recipes" r  (cost=28.07..496.17 rows=1540 width=0) (actual time=0.155..1.640 rows=1530 loops=1)
        Recheck Cond: (("Status" = 1) AND ("CookTimeMinutes" <= 30))
        Filter: (NOT "IsDeleted")
        Heap Blocks: exact=439
        Buffers: shared hit=442
        ->  Bitmap Index Scan on "IDX_Recipe_CookTime"  (cost=0.00..27.68 rows=1540 width=0) (actual time=0.100..0.100 rows=1530 loops=1)
              Index Cond: (("Status" = 1) AND ("CookTimeMinutes" <= 30))
              Buffers: shared hit=3
Planning Time: 0.139 ms
Execution Time: 1.798 ms
```
</details>

<details><summary>Q5b — `maxCookTime=30` + `cookTime asc`, trang 1: plan sau B5</summary>

```
Sort  (cost=21.05..21.08 rows=12 width=1241) (actual time=0.165..0.167 rows=12 loops=1)
  Sort Key: r."CookTimeMinutes", r."Id"
  Sort Method: quicksort  Memory: 28kB
  Buffers: shared hit=55
  ->  Hash Join  (cost=10.16..20.83 rows=12 width=1241) (actual time=0.139..0.156 rows=12 loops=1)
        Hash Cond: (r."CategoryId" = c."Id")
        Buffers: shared hit=55
        ->  Hash Join  (cost=8.71..19.34 rows=12 width=704) (actual time=0.111..0.117 rows=12 loops=1)
              Hash Cond: ((r."AuthorId")::text = a."Id")
              Buffers: shared hit=54
              ->  Limit  (cost=7.57..18.03 rows=12 width=172) (actual time=0.101..0.103 rows=12 loops=1)
                    Buffers: shared hit=53
                    ->  Incremental Sort  (cost=7.57..1349.70 rows=1540 width=172) (actual time=0.101..0.102 rows=12 loops=1)
                          Sort Key: r."CookTimeMinutes", r."Id"
                          Presorted Key: r."CookTimeMinutes"
                          Full-sort Groups: 1  Sort Method: top-N heapsort  Average Memory: 30kB  Peak Memory: 30kB
                          Buffers: shared hit=53
                          ->  Index Scan using "IDX_Recipe_CookTime" on "Recipes" r  (cost=0.29..1303.09 rows=1540 width=172) (actual time=0.013..0.072 rows=52 loops=1)
                                Index Cond: (("Status" = 1) AND ("CookTimeMinutes" <= 30))
                                Filter: (NOT "IsDeleted")
                                Buffers: shared hit=53
              ->  Hash  (cost=1.06..1.06 rows=6 width=569) (actual time=0.005..0.006 rows=6 loops=1)
                    Buckets: 1024  Batches: 1  Memory Usage: 9kB
                    Buffers: shared hit=1
                    ->  Seq Scan on "AspNetUsers" a  (cost=0.00..1.06 rows=6 width=569) (actual time=0.002..0.003 rows=6 loops=1)
                          Buffers: shared hit=1
        ->  Hash  (cost=1.20..1.20 rows=20 width=37) (actual time=0.014..0.014 rows=20 loops=1)
              Buckets: 1024  Batches: 1  Memory Usage: 10kB
              Buffers: shared hit=1
              ->  Seq Scan on "Categories" c  (cost=0.00..1.20 rows=20 width=37) (actual time=0.004..0.008 rows=20 loops=1)
                    Filter: (NOT "IsDeleted")
                    Buffers: shared hit=1
        SubPlan 1
          ->  Limit  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                ->  Seq Scan on "RecipeImages" r1  (cost=0.00..0.00 rows=1 width=516) (actual time=0.000..0.000 rows=0 loops=12)
                      Filter: ((NOT "IsDeleted") AND "IsPrimary" AND (r."Id" = "RecipeId"))
Planning:
  Buffers: shared hit=11
Planning Time: 0.359 ms
Execution Time: 0.199 ms
```
</details>

<details><summary>Q6a — Tìm kiếm `q=pho bo` — COUNT: plan sau B5</summary>

```
Aggregate  (cost=254.33..254.34 rows=1 width=4) (actual time=0.393..0.393 rows=1 loops=1)
  Buffers: shared hit=93
  ->  Bitmap Heap Scan on "Recipes" r  (cost=34.60..254.15 rows=74 width=0) (actual time=0.266..0.387 rows=85 loops=1)
        Recheck Cond: ("SearchVector" @@ '''pho'':* & ''bo'':*'::tsquery)
        Filter: ((NOT "IsDeleted") AND ("Status" = 1))
        Rows Removed by Filter: 11
        Heap Blocks: exact=88
        Buffers: shared hit=93
        ->  Bitmap Index Scan on "IDX_Recipe_Search"  (cost=0.00..34.58 rows=87 width=0) (actual time=0.255..0.255 rows=96 loops=1)
              Index Cond: ("SearchVector" @@ '''pho'':* & ''bo'':*'::tsquery)
              Buffers: shared hit=5
Planning:
  Buffers: shared hit=1
Planning Time: 0.123 ms
Execution Time: 0.412 ms
```
</details>

<details><summary>Q6b — Tìm kiếm `q=pho bo` — xếp hạng `ts_rank`, trang 1: plan sau B5</summary>

```
Limit  (cost=256.03..256.06 rows=12 width=28) (actual time=0.505..0.508 rows=12 loops=1)
  Buffers: shared hit=93
  ->  Sort  (cost=256.03..256.21 rows=74 width=28) (actual time=0.504..0.505 rows=12 loops=1)
        Sort Key: (ts_rank("SearchVector", '''pho'':* & ''bo'':*'::tsquery)) DESC, "PublishedAt" DESC, "Id"
        Sort Method: top-N heapsort  Memory: 26kB
        Buffers: shared hit=93
        ->  Bitmap Heap Scan on "Recipes" r  (cost=34.60..254.33 rows=74 width=28) (actual time=0.294..0.478 rows=85 loops=1)
              Recheck Cond: ("SearchVector" @@ '''pho'':* & ''bo'':*'::tsquery)
              Filter: ((NOT "IsDeleted") AND ("Status" = 1))
              Rows Removed by Filter: 11
              Heap Blocks: exact=88
              Buffers: shared hit=93
              ->  Bitmap Index Scan on "IDX_Recipe_Search"  (cost=0.00..34.58 rows=87 width=0) (actual time=0.276..0.276 rows=96 loops=1)
                    Index Cond: ("SearchVector" @@ '''pho'':* & ''bo'':*'::tsquery)
                    Buffers: shared hit=5
Planning:
  Buffers: shared hit=1
Planning Time: 0.129 ms
Execution Time: 0.527 ms
```
</details>

<details><summary>Q7 — `/categories/{slug}` `publishedAt desc`, trang 1: plan sau B5</summary>

```
Sort  (cost=28.97..29.00 rows=12 width=1241) (actual time=0.163..0.166 rows=12 loops=1)
  Sort Key: r."PublishedAt" DESC, r."Id"
  Sort Method: quicksort  Memory: 27kB
  Buffers: shared hit=17
  ->  Hash Join  (cost=4.82..28.76 rows=12 width=1241) (actual time=0.122..0.148 rows=12 loops=1)
        Hash Cond: (r."CategoryId" = c."Id")
        Buffers: shared hit=17
        ->  Hash Join  (cost=3.37..27.27 rows=12 width=704) (actual time=0.081..0.092 rows=12 loops=1)
              Hash Cond: ((r."AuthorId")::text = a."Id")
              Buffers: shared hit=16
              ->  Limit  (cost=2.24..25.96 rows=12 width=172) (actual time=0.055..0.058 rows=12 loops=1)
                    Buffers: shared hit=15
                    ->  Incremental Sort  (cost=2.24..869.92 rows=439 width=172) (actual time=0.055..0.056 rows=12 loops=1)
                          Sort Key: r."PublishedAt" DESC, r."Id"
                          Presorted Key: r."PublishedAt"
                          Full-sort Groups: 1  Sort Method: quicksort  Average Memory: 28kB  Peak Memory: 28kB
                          Buffers: shared hit=15
                          ->  Index Scan using "IDX_Recipe_ByCategory" on "Recipes" r  (cost=0.29..850.24 rows=439 width=172) (actual time=0.017..0.038 rows=13 loops=1)
                                Index Cond: (("Status" = 1) AND ("CategoryId" = '4027f93f-cb24-41d9-9f7e-2e88da7ef0e2'::uuid))
                                Filter: (NOT "IsDeleted")
                                Buffers: shared hit=15
              ->  Hash  (cost=1.06..1.06 rows=6 width=569) (actual time=0.016..0.016 rows=6 loops=1)
                    Buckets: 1024  Batches: 1  Memory Usage: 9kB
                    Buffers: shared hit=1
                    ->  Seq Scan on "AspNetUsers" a  (cost=0.00..1.06 rows=6 width=569) (actual time=0.011..0.013 rows=6 loops=1)
                          Buffers: shared hit=1
        ->  Hash  (cost=1.20..1.20 rows=20 width=37) (actual time=0.021..0.022 rows=20 loops=1)
              Buckets: 1024  Batches: 1  Memory Usage: 10kB
              Buffers: shared hit=1
              ->  Seq Scan on "Categories" c  (cost=0.00..1.20 rows=20 width=37) (actual time=0.007..0.013 rows=20 loops=1)
                    Filter: (NOT "IsDeleted")
                    Buffers: shared hit=1
        SubPlan 1
          ->  Limit  (cost=0.00..0.00 rows=1 width=516) (actual time=0.001..0.001 rows=0 loops=12)
                ->  Seq Scan on "RecipeImages" r1  (cost=0.00..0.00 rows=1 width=516) (actual time=0.000..0.000 rows=0 loops=12)
                      Filter: ((NOT "IsDeleted") AND "IsPrimary" AND (r."Id" = "RecipeId"))
Planning:
  Buffers: shared hit=11
Planning Time: 0.533 ms
Execution Time: 0.216 ms
```
</details>
