import { recipeFormSchema } from './schemas';

const valid = {
  title: 'Phở bò gia truyền', description: 'Món phở thơm ngon cho bữa sáng cuối tuần.', categoryId: 'category-id', prepTime: 10, cookTime: 40, servings: 2, difficulty: 'Easy', instructions: '', calories: Number.NaN, protein: Number.NaN, carbohydrates: Number.NaN, fat: Number.NaN, fiber: Number.NaN, sodium: Number.NaN,
  ingredients: [{ name: 'Bánh phở', quantity: null, quantityText: null, unit: 'gói', notes: null, orderIndex: 0 }],
  steps: [{ title: 'Nấu nước dùng', description: 'Đun nước dùng đến khi sôi.', timerMinutes: null, imageUrl: null }],
};

describe('recipeFormSchema', () => {
  it('accepts a unit-only ingredient and has no client stepNumber', () => {
    const result = recipeFormSchema.safeParse(valid);
    expect(result.success).toBe(true);
    expect(result.success && result.data.steps[0]).not.toHaveProperty('stepNumber');
  });

  it('rejects an ingredient without numeric, text, or unit quantity', () => {
    const result = recipeFormSchema.safeParse({ ...valid, ingredients: [{ ...valid.ingredients[0], unit: null }] });
    expect(result.success).toBe(false);
  });
});
