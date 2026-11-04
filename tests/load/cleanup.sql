-- Buổi 8: dọn bản nháp do k6 tạo (tiêu đề bắt đầu bằng TITLE_PREFIX của lib/traffic.js) — dữ liệu mẫu CR-2026-03 giữ nguyên.
-- Xóa cứng có chủ đích: đây là dữ liệu đo tải, không phải dữ liệu người dùng (xóa mềm sẽ để lại hàng nghìn dòng tới khi purge job
-- chạy sau 30 ngày). FK ON DELETE CASCADE dọn Steps/Ingredients/Images; bản nháp k6 không có ảnh nên không có tệp MinIO nào.
--   docker compose -f docker-compose.prod.yml --env-file .env.production exec -T postgres \
--     sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB"' < tests/load/cleanup.sql
BEGIN;
SELECT count(*) AS k6_recipes_before FROM "Recipes" WHERE "Title" LIKE 'K6 thử tải %';
DELETE FROM "Recipes" WHERE "Title" LIKE 'K6 thử tải %';
-- "Status" là smallint: RecipeStatus.Published = 1.
SELECT count(*) AS recipes_after, count(*) FILTER (WHERE "Status" = 1) AS published_after FROM "Recipes";
COMMIT;
