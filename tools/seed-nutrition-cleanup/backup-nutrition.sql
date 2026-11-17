-- Sinh script KHÔI PHỤC cho clear-seed-nutrition.sql: đặt lại đúng 6 cột dinh dưỡng theo Id.
-- Cần chạy sau legacy-recipes.sql (bảng tạm seed_nutrition_targets).
SELECT 'BEGIN;';
SELECT format(
    'UPDATE "Recipes" SET "Nutrition_Calories" = %L, "Nutrition_Protein" = %L, "Nutrition_Carbohydrates" = %L, "Nutrition_Fat" = %L, "Nutrition_Fiber" = %L, "Nutrition_Sodium" = %L WHERE "Id" = %L;',
    "Nutrition_Calories", "Nutrition_Protein", "Nutrition_Carbohydrates", "Nutrition_Fat", "Nutrition_Fiber", "Nutrition_Sodium", "Id")
FROM seed_nutrition_targets
ORDER BY "Id";
SELECT 'COMMIT;';
