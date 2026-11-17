import type { MyRecipesParams } from '@/types/recipe';
import type { RecipeListParams } from '@/types/search';

/**
 * Khóa TanStack Query phân tách dữ liệu công khai và riêng tư (NFR-SEC-006).
 * Không được thay `private` bằng khóa danh sách công khai: bản nháp có thể bị
 * giữ lại trong cache khi đổi tài khoản hoặc chuyển màn hình.
 */
export const recipeKeys = {
  all: ['recipes'] as const,
  list: (params: RecipeListParams) => ['recipes', 'public', 'list', params] as const,
  detail: (slug: string) => ['recipes', 'public', 'detail', slug] as const,
  mine: (params: MyRecipesParams) => ['recipes', 'private', 'mine', params] as const,
  mineById: (id: string) => ['recipes', 'private', 'mine', id] as const,
};
