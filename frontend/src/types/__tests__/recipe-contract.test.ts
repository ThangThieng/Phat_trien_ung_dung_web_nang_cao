import fs from 'node:fs';
import path from 'node:path';
import {
  RECIPE_DIFFICULTIES,
  RECIPE_STATUSES,
  type Recipe,
  type RecipeIngredient,
  type RecipeStep,
  type RecipeSummary,
  type StepInput,
} from '../recipe';

const recipeKeys = {
  id: true,
  title: true,
  slug: true,
  description: true,
  instructions: true,
  prepTimeMinutes: true,
  cookTimeMinutes: true,
  totalTimeMinutes: true,
  servings: true,
  difficulty: true,
  status: true,
  category: true,
  author: true,
  nutrition: true,
  steps: true,
  ingredients: true,
  images: true,
  publishedAt: true,
  createdAt: true,
  updatedAt: true,
  rowVersion: true,
} satisfies Record<keyof Recipe, true>;

const summaryKeys = {
  id: true,
  title: true,
  slug: true,
  description: true,
  prepTimeMinutes: true,
  cookTimeMinutes: true,
  totalTimeMinutes: true,
  servings: true,
  difficulty: true,
  status: true,
  primaryImageUrl: true,
  category: true,
  author: true,
  publishedAt: true,
  createdAt: true,
} satisfies Record<keyof RecipeSummary, true>;

const ingredientKeys = {
  id: true,
  name: true,
  quantity: true,
  quantityText: true,
  unit: true,
  notes: true,
  orderIndex: true,
} satisfies Record<keyof RecipeIngredient, true>;

const stepKeys = {
  id: true,
  stepNumber: true,
  title: true,
  description: true,
  timerMinutes: true,
  imageUrl: true,
} satisfies Record<keyof RecipeStep, true>;

type OpenApiSchema = { properties?: Record<string, unknown> };
type OpenApiDocument = { components: { schemas: Record<string, OpenApiSchema> } };

const snapshotPath = path.resolve(process.cwd(), '..', 'docs', 'api', 'openapi.v1.json');

function schemaKeys(document: OpenApiDocument, schemaName: string) {
  const schema = document.components.schemas[schemaName];
  if (!schema?.properties) throw new Error(`Snapshot thiếu schema ${schemaName}.`);
  return Object.keys(schema.properties).sort();
}

describe('Recipe OpenAPI contract', () => {
  const document = JSON.parse(fs.readFileSync(snapshotPath, 'utf8')) as OpenApiDocument;

  test.each([
    ['RecipeDetailDto', recipeKeys],
    ['RecipeSummaryDto', summaryKeys],
    ['RecipeIngredientDto', ingredientKeys],
    ['RecipeStepDto', stepKeys],
  ])('%s khớp kiểu TypeScript', (schemaName, keys) => {
    expect(schemaKeys(document, schemaName)).toEqual(Object.keys(keys).sort());
  });

  it('giữ đúng enum SRS', () => {
    expect(RECIPE_DIFFICULTIES).toEqual(['Easy', 'Medium', 'Hard', 'Expert']);
    expect(RECIPE_STATUSES).toEqual(['Draft', 'Published', 'Archived']);
  });

  it('StepInput không cho client gửi stepNumber', () => {
    const input = { title: 'Sơ chế', description: 'Rửa sạch', timerMinutes: null, imageUrl: null } satisfies StepInput;
    expect(input).not.toHaveProperty('stepNumber');
  });
});
