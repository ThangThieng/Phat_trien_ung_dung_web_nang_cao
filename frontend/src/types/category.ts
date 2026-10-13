/** Kiểu miền Category – khớp DTO của Backend (.NET), JSON camelCase. Chủ file: Dev 3. */

import type { PagedResult } from './api';
import type { RecipeSummary } from './recipe';

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
