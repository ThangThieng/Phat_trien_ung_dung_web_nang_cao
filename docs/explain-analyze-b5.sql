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
