/** Kiểu dùng chung của API. Kiểu miền Recipe được quản lý riêng trong recipe.ts. */
import type { RecipeSummary } from './recipe';

export * from './recipe';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
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

/** SRS §8.1 – UserDto `{ id, email, displayName, avatarUrl, bio, roles }` (D-1). */
export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  expiresIn: number;
  user: User;
}

export interface UploadedFile {
  key: string;
  url: string;
  contentType: string;
  size: number;
}
