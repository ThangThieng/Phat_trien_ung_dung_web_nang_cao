import { getApiBaseUrl } from '@/lib/config';
import { ApiError, apiFetch } from '@/lib/api-client';
import type { ProblemDetails } from '@/lib/api-client';
import type { RecipeDetail, RecipeImageResult, RecipeIngredient, RecipeStep } from '@/types/api';
import type { CreateRecipeRequest } from './schemas';

/** FR-RCP-003 – POST /recipes → 201 RecipeDetail ở trạng thái Draft. */
export function createRecipe(body: CreateRecipeRequest, accessToken: string) {
  return apiFetch<RecipeDetail>('/recipes', { method: 'POST', body, accessToken });
}

export interface UploadRecipeImageOptions {
  altText?: string;
  isPrimary?: boolean;
}

/**
 * FR-RCP-008 – POST /recipes/{id}/images (multipart: file, altText?, isPrimary?). Không dùng apiFetch vì body là
 * FormData — trình duyệt tự đặt Content-Type kèm boundary.
 */
export async function uploadRecipeImage(
  recipeId: string,
  file: File,
  accessToken: string,
  options: UploadRecipeImageOptions = {},
): Promise<RecipeImageResult> {
  const form = new FormData();
  form.append('file', file);
  if (options.altText) form.append('altText', options.altText);
  if (options.isPrimary !== undefined) form.append('isPrimary', String(options.isPrimary));

  const response = await fetch(`${getApiBaseUrl()}/recipes/${recipeId}/images`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${accessToken}`, Accept: 'application/json' },
    body: form,
  });

  if (!response.ok) {
    let problem: ProblemDetails;
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      problem = { status: response.status, title: response.statusText };
    }
    throw new ApiError(response.status, problem);
  }

  return (await response.json()) as RecipeImageResult;
}

/** FR-RCP-008 – PATCH metadata gộp { altText?, isPrimary?, orderIndex? }. */
export function updateRecipeImage(
  recipeId: string,
  imageId: string,
  body: { altText?: string; isPrimary?: boolean; orderIndex?: number },
  accessToken: string,
) {
  return apiFetch<RecipeImageResult>(`/recipes/${recipeId}/images/${imageId}`, {
    method: 'PATCH',
    body,
    accessToken,
  });
}

/** FR-RCP-008 – DELETE ảnh (tệp MinIO xóa bất đồng bộ phía server). */
export function deleteRecipeImage(recipeId: string, imageId: string, accessToken: string) {
  return apiFetch<void>(`/recipes/${recipeId}/images/${imageId}`, {
    method: 'DELETE',
    accessToken,
  });
}

export function getMyRecipe(recipeId: string, accessToken: string) {
  return apiFetch<RecipeDetail>(`/recipes/mine/${recipeId}`, { accessToken, cache: 'no-store' });
}

export function updateRecipe(recipeId: string, body: CreateRecipeRequest & { rowVersion: string }, accessToken: string) {
  return apiFetch<RecipeDetail>(`/recipes/${recipeId}`, { method: 'PUT', body, accessToken });
}

export type IngredientInput = Pick<RecipeIngredient, 'name' | 'quantity' | 'quantityText' | 'unit' | 'notes' | 'orderIndex'>;
export type StepInput = Pick<RecipeStep, 'title' | 'description' | 'timerMinutes' | 'imageUrl'>;

export function addIngredient(recipeId: string, body: IngredientInput, accessToken: string) {
  return apiFetch<RecipeIngredient>(`/recipes/${recipeId}/ingredients`, { method: 'POST', body, accessToken });
}
export function updateIngredient(recipeId: string, ingredientId: string, body: IngredientInput, accessToken: string) {
  return apiFetch<RecipeIngredient>(`/recipes/${recipeId}/ingredients/${ingredientId}`, { method: 'PUT', body, accessToken });
}
export function deleteIngredient(recipeId: string, ingredientId: string, accessToken: string) {
  return apiFetch<void>(`/recipes/${recipeId}/ingredients/${ingredientId}`, { method: 'DELETE', accessToken });
}
export function addStep(recipeId: string, body: StepInput, accessToken: string) {
  return apiFetch<RecipeStep>(`/recipes/${recipeId}/steps`, { method: 'POST', body, accessToken });
}
export function updateStep(recipeId: string, stepId: string, body: StepInput, accessToken: string) {
  return apiFetch<RecipeStep>(`/recipes/${recipeId}/steps/${stepId}`, { method: 'PUT', body, accessToken });
}
export function deleteStep(recipeId: string, stepId: string, accessToken: string) {
  return apiFetch<void>(`/recipes/${recipeId}/steps/${stepId}`, { method: 'DELETE', accessToken });
}
export function reorderSteps(recipeId: string, stepIds: string[], accessToken: string) {
  return apiFetch<RecipeStep[]>(`/recipes/${recipeId}/steps/reorder`, { method: 'PATCH', body: { stepIds }, accessToken });
}
export function publishRecipe(recipeId: string, accessToken: string) {
  return apiFetch<RecipeDetail>(`/recipes/${recipeId}/publish`, { method: 'PATCH', accessToken });
}
export function unpublishRecipe(recipeId: string, accessToken: string) {
  return apiFetch<RecipeDetail>(`/recipes/${recipeId}/unpublish`, { method: 'PATCH', accessToken });
}
