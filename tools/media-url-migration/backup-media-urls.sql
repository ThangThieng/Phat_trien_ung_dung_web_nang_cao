-- Sao lưu cho migrate-media-urls.sql: sinh ra một script SQL KHÔI PHỤC (đặt lại đúng giá trị cũ theo Id) cho mọi dòng sắp bị
-- đổi. Chỉ chứa Id + URL — không sao chép mật khẩu băm hay dữ liệu cá nhân khác của AspNetUsers ra file.
--   psql -At -v old_prefix=... -f backup-media-urls.sql > backups/<thời điểm>.restore.sql
SELECT 'BEGIN;';
SELECT format('UPDATE %I SET %I = %L WHERE "Id" = %L;', t.table_name, t.column_name, t.url, t.id)
FROM (
    SELECT 'RecipeImages' AS table_name, 'OriginalUrl' AS column_name, "Id"::text AS id, "OriginalUrl" AS url FROM "RecipeImages"
    UNION ALL SELECT 'RecipeImages', 'MediumUrl', "Id"::text, "MediumUrl" FROM "RecipeImages"
    UNION ALL SELECT 'RecipeImages', 'ThumbnailUrl', "Id"::text, "ThumbnailUrl" FROM "RecipeImages"
    UNION ALL SELECT 'RecipeSteps', 'ImageUrl', "Id"::text, "ImageUrl" FROM "RecipeSteps"
    UNION ALL SELECT 'Categories', 'ImageUrl', "Id"::text, "ImageUrl" FROM "Categories"
    UNION ALL SELECT 'AspNetUsers', 'AvatarUrl', "Id"::text, "AvatarUrl" FROM "AspNetUsers"
) AS t
WHERE starts_with(t.url, :'old_prefix')
ORDER BY t.table_name, t.column_name, t.id;
SELECT 'COMMIT;';
