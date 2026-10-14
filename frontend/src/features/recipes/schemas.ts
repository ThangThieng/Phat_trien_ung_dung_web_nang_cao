import { z } from 'zod';

export const DIFFICULTIES = ['Easy', 'Medium', 'Hard', 'Expert'] as const;

/** Cột Nutrition_* là numeric(8,2) – mirror NutritionInputValidator (backend). */
export const NUTRITION_MAX = 999_999.99;

export const NUTRITION_FIELDS = [
  { key: 'calories', label: 'Năng lượng (kcal)' },
  { key: 'protein', label: 'Chất đạm (g)' },
  { key: 'carbohydrates', label: 'Tinh bột (g)' },
  { key: 'fat', label: 'Chất béo (g)' },
  { key: 'fiber', label: 'Chất xơ (g)' },
  { key: 'sodium', label: 'Natri (mg)' },
] as const;

/**
 * Ô số để trống cho ra NaN (register với valueAsNumber) = "không nhập". Dinh dưỡng là tùy chọn nên NaN hợp lệ;
 * giá trị có nhập phải nằm trong 0 – NUTRITION_MAX. Dùng z.custom vì z.number() của zod 4 từ chối NaN trước khi
 * refine kịp chạy.
 */
const optionalNutrient = z.custom<number>(
  (value) =>
    typeof value === 'number' && (Number.isNaN(value) || (value >= 0 && value <= NUTRITION_MAX)),
  `Giá trị phải từ 0 đến ${NUTRITION_MAX.toLocaleString('vi-VN')}.`,
);

/**
 * FR-RCP-003 bước 1 của wizard – mirror CreateRecipeCommandValidator (SRS §7.9): title 5–200, description 20–2000,
 * instructions ≤ 5000, prepTime > 0, cookTime ≥ 0, servings > 0. Backend vẫn kiểm tra lại (kể cả categoryId tồn tại).
 */
export const recipeBasicsSchema = z.object({
  title: z
    .string()
    .trim()
    .min(5, 'Tiêu đề phải từ 5 đến 200 ký tự.')
    .max(200, 'Tiêu đề phải từ 5 đến 200 ký tự.'),
  description: z
    .string()
    .trim()
    .min(20, 'Mô tả phải từ 20 đến 2000 ký tự.')
    .max(2000, 'Mô tả phải từ 20 đến 2000 ký tự.'),
  categoryId: z.string().min(1, 'Hãy chọn danh mục.'),
  prepTime: z
    .number({ message: 'Nhập số phút chuẩn bị.' })
    .int('Số phút phải là số nguyên.')
    .positive('Thời gian chuẩn bị phải lớn hơn 0.'),
  cookTime: z
    .number({ message: 'Nhập số phút nấu (0 nếu không cần nấu).' })
    .int('Số phút phải là số nguyên.')
    .min(0, 'Thời gian nấu phải >= 0.'),
  servings: z
    .number({ message: 'Nhập số khẩu phần.' })
    .int('Số khẩu phần phải là số nguyên.')
    .positive('Số khẩu phần phải lớn hơn 0.'),
  difficulty: z.enum(DIFFICULTIES, { message: 'Chọn độ khó.' }),
  instructions: z.string().max(5000, 'Hướng dẫn tối đa 5000 ký tự.'),
  calories: optionalNutrient,
  protein: optionalNutrient,
  carbohydrates: optionalNutrient,
  fat: optionalNutrient,
  fiber: optionalNutrient,
  sodium: optionalNutrient,
});

export type RecipeBasicsValues = z.infer<typeof recipeBasicsSchema>;

/** Tên trường mà backend có thể trả trong errors{} của 400 – để gắn lỗi vào đúng ô (D-11). */
export const RECIPE_BASICS_FIELDS = [
  'title',
  'description',
  'categoryId',
  'prepTime',
  'cookTime',
  'servings',
  'difficulty',
  'instructions',
] as const;

const toOptional = (value: number) => (Number.isNaN(value) ? null : value);

/** Chuyển giá trị form sang body POST /recipes (nutrition chỉ gửi khi có ít nhất một chỉ số). */
export function toCreateRecipeRequest(values: RecipeBasicsValues) {
  const nutrition = {
    calories: toOptional(values.calories),
    protein: toOptional(values.protein),
    carbohydrates: toOptional(values.carbohydrates),
    fat: toOptional(values.fat),
    fiber: toOptional(values.fiber),
    sodium: toOptional(values.sodium),
  };
  const hasNutrition = Object.values(nutrition).some((value) => value !== null);

  return {
    title: values.title,
    description: values.description,
    categoryId: values.categoryId,
    prepTime: values.prepTime,
    cookTime: values.cookTime,
    servings: values.servings,
    difficulty: values.difficulty,
    instructions: values.instructions.trim() === '' ? null : values.instructions.trim(),
    nutrition: hasNutrition ? nutrition : null,
  };
}

export type CreateRecipeRequest = ReturnType<typeof toCreateRecipeRequest>;
