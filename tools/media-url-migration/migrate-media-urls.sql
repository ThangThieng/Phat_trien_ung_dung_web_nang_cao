-- Retrofit D-14 (Buổi 7): đổi URL ảnh đã lưu từ địa chỉ MinIO trực tiếp sang đường dẫn công khai qua Nginx.
--   psql -v ON_ERROR_STOP=1 --single-transaction -v old_prefix=... -v new_prefix=... -f migrate-media-urls.sql
-- Chạy qua migrate-media-urls.sh (sao lưu trước, chạy trong MỘT transaction, xóa cache Redis sau).
--
-- Đủ 6 cột chứa URL ảnh của schema (đối chiếu information_schema ngày 21/10/2026): RecipeImages.OriginalUrl/MediumUrl/
-- ThumbnailUrl, RecipeSteps.ImageUrl, Categories.ImageUrl, AspNetUsers.AvatarUrl. Gồm cả bản ghi đã xóa mềm — quản trị viên
-- có thể khôi phục trong 30 ngày (MT-49) và FR-JOB-003 cần đúng URL để xóa file khi dọn vĩnh viễn.
-- starts_with thay cho LIKE: tiền tố chứa "_" hay "%" không bị hiểu thành ký tự đại diện.
-- Không đụng UpdatedAt/RowVersion: đây là đổi địa chỉ hạ tầng, không phải người dùng sửa nội dung.

\set ON_ERROR_STOP on

CREATE TEMP TABLE media_url_changes (table_name text, column_name text, rows_changed bigint) ON COMMIT DROP;

WITH changed AS (
    UPDATE "RecipeImages" SET "OriginalUrl" = :'new_prefix' || substr("OriginalUrl", length(:'old_prefix') + 1)
    WHERE starts_with("OriginalUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'RecipeImages', 'OriginalUrl', count(*) FROM changed;

WITH changed AS (
    UPDATE "RecipeImages" SET "MediumUrl" = :'new_prefix' || substr("MediumUrl", length(:'old_prefix') + 1)
    WHERE starts_with("MediumUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'RecipeImages', 'MediumUrl', count(*) FROM changed;

WITH changed AS (
    UPDATE "RecipeImages" SET "ThumbnailUrl" = :'new_prefix' || substr("ThumbnailUrl", length(:'old_prefix') + 1)
    WHERE starts_with("ThumbnailUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'RecipeImages', 'ThumbnailUrl', count(*) FROM changed;

WITH changed AS (
    UPDATE "RecipeSteps" SET "ImageUrl" = :'new_prefix' || substr("ImageUrl", length(:'old_prefix') + 1)
    WHERE starts_with("ImageUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'RecipeSteps', 'ImageUrl', count(*) FROM changed;

WITH changed AS (
    UPDATE "Categories" SET "ImageUrl" = :'new_prefix' || substr("ImageUrl", length(:'old_prefix') + 1)
    WHERE starts_with("ImageUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'Categories', 'ImageUrl', count(*) FROM changed;

WITH changed AS (
    UPDATE "AspNetUsers" SET "AvatarUrl" = :'new_prefix' || substr("AvatarUrl", length(:'old_prefix') + 1)
    WHERE starts_with("AvatarUrl", :'old_prefix') RETURNING 1)
INSERT INTO media_url_changes SELECT 'AspNetUsers', 'AvatarUrl', count(*) FROM changed;

SELECT table_name AS "bảng", column_name AS "cột", rows_changed AS "số dòng đã đổi" FROM media_url_changes;

-- Kiểm tra trước khi COMMIT: còn bất kỳ URL nào mang tiền tố cũ thì hủy cả transaction.
SELECT set_config('media.old_prefix', :'old_prefix', true);
DO $$
DECLARE
    old_prefix text := current_setting('media.old_prefix');
    remaining bigint;
BEGIN
    SELECT (SELECT count(*) FROM "RecipeImages" WHERE starts_with("OriginalUrl", old_prefix)
                OR starts_with("MediumUrl", old_prefix) OR starts_with("ThumbnailUrl", old_prefix))
         + (SELECT count(*) FROM "RecipeSteps" WHERE starts_with("ImageUrl", old_prefix))
         + (SELECT count(*) FROM "Categories" WHERE starts_with("ImageUrl", old_prefix))
         + (SELECT count(*) FROM "AspNetUsers" WHERE starts_with("AvatarUrl", old_prefix))
      INTO remaining;
    IF remaining > 0 THEN
        RAISE EXCEPTION 'Còn % URL mang tiền tố cũ — hủy toàn bộ thay đổi', remaining;
    END IF;
END $$;
