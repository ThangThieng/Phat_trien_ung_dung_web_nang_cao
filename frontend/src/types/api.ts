/** Kiểu dữ liệu khớp DTO của Backend (.NET) – JSON camelCase, enum dạng chuỗi. */

export type RecipeDifficulty = 'Easy' | 'Medium' | 'Hard' | 'Expert';
export type RecipeStatus = 'Draft' | 'Published' | 'Archived';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CategoryRef {
  id: string;
  name: string;
  slug: string;
}

export interface Author {
  id: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface RecipeSummary {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  totalTimeMinutes: number;
  servings: number;
  difficulty: RecipeDifficulty;
  status: RecipeStatus;
  primaryImageUrl: string | null;
  category: CategoryRef;
  author: Author;
  publishedAt: string | null;
  createdAt: string;
}

export interface RecipeNutrition {
  calories: number | null;
  protein: number | null;
  carbohydrates: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
}

export interface RecipeStep {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

export interface RecipeIngredient {
  id: string;
  name: string;
  quantity: number | null;
  unit: string | null;
  notes: string | null;
  orderIndex: number;
}

export interface RecipeImage {
  id: string;
  originalUrl: string;
  mediumUrl: string | null;
  thumbnailUrl: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
}

export interface RecipeDetail extends Omit<RecipeSummary, 'primaryImageUrl'> {
  instructions: string;
  nutrition: RecipeNutrition | null;
  steps: RecipeStep[];
  ingredients: RecipeIngredient[];
  images: RecipeImage[];
  updatedAt: string | null;
  rowVersion: string;
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

export interface User {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  avatarUrl: string | null;
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
