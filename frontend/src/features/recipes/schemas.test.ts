import { recipeBasicsSchema, toCreateRecipeRequest } from './schemas';

const valid = {
  title: 'Phở bò Hà Nội',
  description: 'Nước dùng ninh xương bò trong, thơm quế hồi nướng.',
  categoryId: '44444444-4444-4444-4444-444444444444',
  prepTime: 40,
  cookTime: 360,
  servings: 6,
  difficulty: 'Hard' as const,
  instructions: '',
  calories: Number.NaN,
  protein: Number.NaN,
  carbohydrates: Number.NaN,
  fat: Number.NaN,
  fiber: Number.NaN,
  sodium: Number.NaN,
};

describe('recipeBasicsSchema (mirror CreateRecipeCommandValidator)', () => {
  it('chấp nhận dữ liệu hợp lệ, dinh dưỡng để trống', () => {
    expect(recipeBasicsSchema.safeParse(valid).success).toBe(true);
  });

  it.each([
    ['title', 'Phở'],
    ['description', 'Quá ngắn'],
    ['categoryId', ''],
    ['prepTime', 0],
    ['cookTime', -1],
    ['servings', 0],
    ['prepTime', Number.NaN],
    ['calories', -5],
  ])('từ chối %s = %p', (field, value) => {
    expect(recipeBasicsSchema.safeParse({ ...valid, [field]: value }).success).toBe(false);
  });

  it('cookTime = 0 hợp lệ (món không cần nấu)', () => {
    expect(recipeBasicsSchema.safeParse({ ...valid, cookTime: 0 }).success).toBe(true);
  });
});

describe('toCreateRecipeRequest', () => {
  it('không gửi nutrition khi mọi chỉ số để trống, instructions rỗng thành null', () => {
    const body = toCreateRecipeRequest(valid);
    expect(body.nutrition).toBeNull();
    expect(body.instructions).toBeNull();
  });

  it('chỉ số để trống thành null, chỉ số có nhập giữ nguyên', () => {
    const body = toCreateRecipeRequest({ ...valid, calories: 450, protein: 32.5 });
    expect(body.nutrition).toEqual({
      calories: 450,
      protein: 32.5,
      carbohydrates: null,
      fat: null,
      fiber: null,
      sodium: null,
    });
  });
});
