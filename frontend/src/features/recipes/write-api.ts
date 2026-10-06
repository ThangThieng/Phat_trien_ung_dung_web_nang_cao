import { getApiBaseUrl } from '@/lib/config';
import { ApiError, apiFetch } from '@/lib/api-client';
import type { ProblemDetails } from '@/lib/api-client';
import type { RecipeDetail, RecipeImageResult } from '@/types/api';
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
