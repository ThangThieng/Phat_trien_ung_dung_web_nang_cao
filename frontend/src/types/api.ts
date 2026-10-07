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
  /** Ảnh chính cho card: ThumbnailUrl 300×300 (FR-JOB-002), dự phòng OriginalUrl khi job đổi kích thước chưa chạy xong. */
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

/** SRS §8.6 – định lượng hai cột (MT-04): `quantity` số tính toán được, `quantityText` nguyên văn ("vừa đủ", "1/2 muỗng"). */
export interface RecipeIngredient {
  id: string;
  name: string;
  quantity: number | null;
  quantityText: string | null;
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

/** SRS §8.4 – response của POST/PATCH ảnh công thức (mediumUrl/thumbnailUrl null cho tới khi FR-JOB-002 chạy). */
export interface RecipeImageResult {
  imageId: string;
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
