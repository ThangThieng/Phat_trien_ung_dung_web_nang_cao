-- CR-2026-07: bỏ dinh dưỡng sinh ngẫu nhiên. Chạy qua clear-seed-nutrition.sh (sao lưu trước, một transaction, xóa cache sau).
-- Không đụng UpdatedAt/RowVersion: đây là dọn dữ liệu mẫu, không phải người dùng sửa nội dung.
UPDATE "Recipes" r
SET "Nutrition_Calories" = NULL,
    "Nutrition_Protein" = NULL,
    "Nutrition_Carbohydrates" = NULL,
    "Nutrition_Fat" = NULL,
    "Nutrition_Fiber" = NULL,
    "Nutrition_Sodium" = NULL
FROM seed_nutrition_targets t
WHERE r."Id" = t."Id";
