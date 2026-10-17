-- NFR-PERF-004 (Buổi 5 – Dev 3): SQL THẬT do EF Core sinh ra (log Microsoft.EntityFrameworkCore.Database.Command),
-- tham số đã thay bằng giá trị cụ thể. Chạy: docker compose exec -T postgres psql -U culinary -d culinaryblog < docs/explain-analyze-b5.sql
SELECT "Id" AS cat_id FROM "Categories" WHERE "Slug" = 'mon-khai-vi' \gset

\echo === Q1: GET /api/v1/recipes
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*)::int
FROM "Recipes" AS r
WHERE NOT (r."IsDeleted") AND r."Status" = 1;

\echo === Q2: GET /api/v1/recipes
EXPLAIN (ANALYZE, BUFFERS)
SELECT r0."Id", r0."Title", r0."Slug", r0."Description", r0."PrepTimeMinutes", r0."CookTimeMinutes", r0."Servings", r0."Difficulty", r0."Status", (
    SELECT COALESCE(r1."ThumbnailUrl", r1."OriginalUrl")
    FROM "RecipeImages" AS r1
    WHERE NOT (r1."IsDeleted") AND r0."Id" = r1."RecipeId" AND r1."IsPrimary"
    LIMIT 1), r0."CategoryId", c0."Name", c0."Slug", a."Id", a."DisplayName", a."AvatarUrl", r0."PublishedAt", r0."CreatedAt"
FROM (
    SELECT r."Id", r."AuthorId", r."CategoryId", r."CookTimeMinutes", r."CreatedAt", r."Description", r."Difficulty", r."PrepTimeMinutes", r."PublishedAt", r."Servings", r."Slug", r."Status", r."Title"
    FROM "Recipes" AS r
    WHERE NOT (r."IsDeleted") AND r."Status" = 1
    ORDER BY r."CreatedAt" DESC, r."Id"
    LIMIT 12 OFFSET 0
) AS r0
INNER JOIN "AspNetUsers" AS a ON r0."AuthorId" = a."Id"
INNER JOIN (
    SELECT c."Id", c."Name", c."Slug"
    FROM "Categories" AS c
    WHERE NOT (c."IsDeleted")
) AS c0 ON r0."CategoryId" = c0."Id"
ORDER BY r0."CreatedAt" DESC, r0."Id";

\echo === Q3: GET /api/v1/recipes?sortBy=publishedAt&sortOrder=desc
EXPLAIN (ANALYZE, BUFFERS)
SELECT r0."Id", r0."Title", r0."Slug", r0."Description", r0."PrepTimeMinutes", r0."CookTimeMinutes", r0."Servings", r0."Difficulty", r0."Status", (
    SELECT COALESCE(r1."ThumbnailUrl", r1."OriginalUrl")
    FROM "RecipeImages" AS r1
    WHERE NOT (r1."IsDeleted") AND r0."Id" = r1."RecipeId" AND r1."IsPrimary"
    LIMIT 1), r0."CategoryId", c0."Name", c0."Slug", a."Id", a."DisplayName", a."AvatarUrl", r0."PublishedAt", r0."CreatedAt"
FROM (
    SELECT r."Id", r."AuthorId", r."CategoryId", r."CookTimeMinutes", r."CreatedAt", r."Description", r."Difficulty", r."PrepTimeMinutes", r."PublishedAt", r."Servings", r."Slug", r."Status", r."Title"
    FROM "Recipes" AS r
    WHERE NOT (r."IsDeleted") AND r."Status" = 1
    ORDER BY r."PublishedAt" DESC, r."Id"
    LIMIT 12 OFFSET 0
) AS r0
INNER JOIN "AspNetUsers" AS a ON r0."AuthorId" = a."Id"
INNER JOIN (
    SELECT c."Id", c."Name", c."Slug"
    FROM "Categories" AS c
    WHERE NOT (c."IsDeleted")
) AS c0 ON r0."CategoryId" = c0."Id"
ORDER BY r0."PublishedAt" DESC, r0."Id";

\echo === Q4a: GET /api/v1/recipes?categoryId=…&sortBy=publishedAt&sortOrder=desc
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*)::int
FROM "Recipes" AS r
WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."CategoryId" = :'cat_id'::uuid;

\echo === Q4b: GET /api/v1/recipes?categoryId=…&sortBy=publishedAt&sortOrder=desc
EXPLAIN (ANALYZE, BUFFERS)
SELECT r0."Id", r0."Title", r0."Slug", r0."Description", r0."PrepTimeMinutes", r0."CookTimeMinutes", r0."Servings", r0."Difficulty", r0."Status", (
    SELECT COALESCE(r1."ThumbnailUrl", r1."OriginalUrl")
    FROM "RecipeImages" AS r1
    WHERE NOT (r1."IsDeleted") AND r0."Id" = r1."RecipeId" AND r1."IsPrimary"
    LIMIT 1), r0."CategoryId", c0."Name", c0."Slug", a."Id", a."DisplayName", a."AvatarUrl", r0."PublishedAt", r0."CreatedAt"
FROM (
    SELECT r."Id", r."AuthorId", r."CategoryId", r."CookTimeMinutes", r."CreatedAt", r."Description", r."Difficulty", r."PrepTimeMinutes", r."PublishedAt", r."Servings", r."Slug", r."Status", r."Title"
    FROM "Recipes" AS r
    WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."CategoryId" = :'cat_id'::uuid
    ORDER BY r."PublishedAt" DESC, r."Id"
    LIMIT 12 OFFSET 0
) AS r0
INNER JOIN "AspNetUsers" AS a ON r0."AuthorId" = a."Id"
INNER JOIN (
    SELECT c."Id", c."Name", c."Slug"
    FROM "Categories" AS c
    WHERE NOT (c."IsDeleted")
) AS c0 ON r0."CategoryId" = c0."Id"
ORDER BY r0."PublishedAt" DESC, r0."Id";

\echo === Q5a: GET /api/v1/recipes?maxCookTime=30&sortBy=cookTime&sortOrder=asc
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*)::int
FROM "Recipes" AS r
WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."CookTimeMinutes" <= 30;

\echo === Q5b: GET /api/v1/recipes?maxCookTime=30&sortBy=cookTime&sortOrder=asc
EXPLAIN (ANALYZE, BUFFERS)
SELECT r0."Id", r0."Title", r0."Slug", r0."Description", r0."PrepTimeMinutes", r0."CookTimeMinutes", r0."Servings", r0."Difficulty", r0."Status", (
    SELECT COALESCE(r1."ThumbnailUrl", r1."OriginalUrl")
    FROM "RecipeImages" AS r1
    WHERE NOT (r1."IsDeleted") AND r0."Id" = r1."RecipeId" AND r1."IsPrimary"
    LIMIT 1), r0."CategoryId", c0."Name", c0."Slug", a."Id", a."DisplayName", a."AvatarUrl", r0."PublishedAt", r0."CreatedAt"
FROM (
    SELECT r."Id", r."AuthorId", r."CategoryId", r."CookTimeMinutes", r."CreatedAt", r."Description", r."Difficulty", r."PrepTimeMinutes", r."PublishedAt", r."Servings", r."Slug", r."Status", r."Title"
    FROM "Recipes" AS r
    WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."CookTimeMinutes" <= 30
    ORDER BY r."CookTimeMinutes", r."Id"
    LIMIT 12 OFFSET 0
) AS r0
INNER JOIN "AspNetUsers" AS a ON r0."AuthorId" = a."Id"
INNER JOIN (
    SELECT c."Id", c."Name", c."Slug"
    FROM "Categories" AS c
    WHERE NOT (c."IsDeleted")
) AS c0 ON r0."CategoryId" = c0."Id"
ORDER BY r0."CookTimeMinutes", r0."Id";

\echo === Q6a: GET /api/v1/recipes/search?q=pho%20bo
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*)::int
FROM "Recipes" AS r
WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."SearchVector" @@ to_tsquery('simple', 'pho:* & bo:*');

\echo === Q6b: GET /api/v1/recipes/search?q=pho%20bo
EXPLAIN (ANALYZE, BUFFERS)
SELECT r."Id", r."PublishedAt", ts_rank(r."SearchVector", to_tsquery('simple', 'pho:* & bo:*')) AS "Rank"
FROM "Recipes" AS r
WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."SearchVector" @@ to_tsquery('simple', 'pho:* & bo:*')
ORDER BY ts_rank(r."SearchVector", to_tsquery('simple', 'pho:* & bo:*')) DESC, r."PublishedAt" DESC, r."Id"
LIMIT 12 OFFSET 0;

\echo === Q7: GET /api/v1/categories/mon-khai-vi?sortBy=publishedAt&sortOrder=desc
EXPLAIN (ANALYZE, BUFFERS)
SELECT r0."Id", r0."Title", r0."Slug", r0."Description", r0."PrepTimeMinutes", r0."CookTimeMinutes", r0."Servings", r0."Difficulty", r0."Status", (
    SELECT COALESCE(r1."ThumbnailUrl", r1."OriginalUrl")
    FROM "RecipeImages" AS r1
    WHERE NOT (r1."IsDeleted") AND r0."Id" = r1."RecipeId" AND r1."IsPrimary"
    LIMIT 1), r0."CategoryId", c0."Name", c0."Slug", a."Id", a."DisplayName", a."AvatarUrl", r0."PublishedAt", r0."CreatedAt"
FROM (
    SELECT r."Id", r."AuthorId", r."CategoryId", r."CookTimeMinutes", r."CreatedAt", r."Description", r."Difficulty", r."PrepTimeMinutes", r."PublishedAt", r."Servings", r."Slug", r."Status", r."Title"
    FROM "Recipes" AS r
    WHERE NOT (r."IsDeleted") AND r."Status" = 1 AND r."CategoryId" = :'cat_id'::uuid
    ORDER BY r."PublishedAt" DESC, r."Id"
    LIMIT 12 OFFSET 0
) AS r0
INNER JOIN "AspNetUsers" AS a ON r0."AuthorId" = a."Id"
INNER JOIN (
    SELECT c."Id", c."Name", c."Slug"
    FROM "Categories" AS c
    WHERE NOT (c."IsDeleted")
) AS c0 ON r0."CategoryId" = c0."Id"
ORDER BY r0."PublishedAt" DESC, r0."Id";
