/** Nguồn enum duy nhất cho miền Recipe, dùng lại bởi kiểu dữ liệu, Zod và các ô chọn. */
export const RECIPE_DIFFICULTIES = ['Easy', 'Medium', 'Hard', 'Expert'] as const;
export const RECIPE_STATUSES = ['Draft', 'Published', 'Archived'] as const;

export type RecipeDifficulty = (typeof RECIPE_DIFFICULTIES)[number];
export type RecipeStatus = (typeof RECIPE_STATUSES)[number];

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

/** SRS §8.3 - dữ liệu card trong danh sách, tìm kiếm và danh mục. */
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

/** SRS §7.2.1 - dinh dưỡng trên mỗi khẩu phần. */
export interface Nutrition {
  calories: number | null;
  protein: number | null;
  carbohydrates: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
}

/** @deprecated Dùng `Nutrition`; alias được giữ đến Buổi 8 để không làm gãy màn hình cũ. */
export type RecipeNutrition = Nutrition;

/** SRS §8.5 - stepNumber do server cấp, không có trong StepInput. */
export interface RecipeStep {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

/** SRS §8.6 / MT-04 - định lượng số và nguyên văn cùng tồn tại. */
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

/** SRS §8.4 - kết quả ngay sau khi upload/cập nhật ảnh. */
export interface RecipeImageResult {
  imageId: string;
  originalUrl: string;
  mediumUrl: string | null;
  thumbnailUrl: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
}

/** SRS §8.3 - toàn bộ dữ liệu công thức trả bởi RecipeDetailDto. */
export interface Recipe extends Omit<RecipeSummary, 'primaryImageUrl'> {
  instructions: string | null;
  nutrition: Nutrition | null;
  steps: RecipeStep[];
  ingredients: RecipeIngredient[];
  images: RecipeImage[];
  updatedAt: string | null;
  rowVersion: string;
}

/** @deprecated Dùng `Recipe`; alias được giữ đến Buổi 8 để không làm gãy màn hình cũ. */
export type RecipeDetail = Recipe;

export interface NutritionInput {
  calories: number | null;
  protein: number | null;
  carbohydrates: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
}

/** SRS §8.6 - body thêm/cập nhật nguyên liệu. */
export interface IngredientInput {
  name: string;
  quantity: number | null;
  quantityText: string | null;
  unit: string | null;
  notes: string | null;
  orderIndex: number;
}

/** SRS §8.5 - body bước nấu; client không được gửi stepNumber. */
export interface StepInput {
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

/** FR-RCP-003 - body tạo bản nháp, cho phép tạo nội dung con cùng request. */
export interface CreateRecipeRequest {
  title: string;
  description: string;
  categoryId: string;
  prepTime: number;
  cookTime: number;
  servings: number;
  difficulty: RecipeDifficulty;
  instructions: string | null;
  nutrition: NutritionInput | null;
  ingredients?: IngredientInput[];
  steps?: StepInput[];
}

/** FR-RCP-004 - cập nhật một phần và bắt buộc rowVersion để chống ghi đè. */
export type UpdateRecipeRequest = Partial<Omit<CreateRecipeRequest, 'ingredients' | 'steps'>> & {
  rowVersion: string;
};

/** FR-RCP-010 - thứ tự id đầy đủ sau khi người dùng sắp xếp. */
export interface ReorderStepsRequest {
  stepIds: string[];
}

/** FR-RCP-008 - metadata ảnh có thể cập nhật độc lập. */
export interface UpdateRecipeImageRequest {
  altText?: string | null;
  isPrimary?: boolean;
  orderIndex?: number;
}

/** FR-RCP-011 - tham số danh sách công thức của tác giả. */
export interface MyRecipesParams {
  status?: RecipeStatus;
  page?: number;
  pageSize?: number;
  sortBy?: 'createdAt' | 'publishedAt' | 'title' | 'cookTime';
  sortOrder?: 'asc' | 'desc';
}

export type RecipeStatusAction = 'publish' | 'unpublish' | 'archive' | 'unarchive';
