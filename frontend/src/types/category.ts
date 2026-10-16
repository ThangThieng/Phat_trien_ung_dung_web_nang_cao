/** Kiểu miền Category – khớp DTO của Backend (.NET), JSON camelCase. Chủ file: Dev 3. */

import type { PagedResult } from './api';
import type { RecipeSummary } from './recipe';
import type { SortBy, SortOrder } from './search';

export interface CategoryRef {
  id: string;
  name: string;
  slug: string;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
}

export interface CategoryDetail {
  category: Category;
  recipes: PagedResult<RecipeSummary>;
}

/** Tham số `GET /categories/{slug}` – SRS §8.2: chỉ nhận page, pageSize, sortBy, sortOrder. */
export interface CategoryDetailParams {
  page?: number;
  pageSize?: number;
  sortBy?: SortBy;
  sortOrder?: SortOrder;
}

/** Giới hạn đầu vào khớp `CategoryLimits` + validator backend (SRS §7.9 – MT-37). */
export const CATEGORY_LIMITS = {
  nameMin: 2,
  nameMax: 100,
  descriptionMax: 2000,
  imageUrlMax: 500,
} as const;

/** Body `POST /categories` (FR-CAT-003) – khớp `CreateCategoryCommand`. */
export interface CreateCategoryRequest {
  name: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
}

/** Body `PUT /categories/{id}` (FR-CAT-004) – khớp `UpdateCategoryCommand` (Id nằm trên URL). */
export type UpdateCategoryRequest = CreateCategoryRequest;
