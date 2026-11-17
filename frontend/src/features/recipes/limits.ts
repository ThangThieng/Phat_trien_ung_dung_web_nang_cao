/** SRS §7.9 – one frontend source for limits also enforced by the backend validators. */
export const RECIPE_LIMITS = {
  title: { min: 5, max: 200 },
  description: { min: 20, max: 2000 },
  instructionsMax: 5000,
  ingredientNameMax: 200,
  quantityTextMax: 50,
  unitMax: 50,
  notesMax: 500,
  quantityMax: 9_999_999.999,
  stepTitleMax: 200,
  stepDescriptionMax: 2000,
  imageUrlMax: 500,
  inlineItemsMax: 100,
  nutritionMax: 999_999.99,
} as const;
