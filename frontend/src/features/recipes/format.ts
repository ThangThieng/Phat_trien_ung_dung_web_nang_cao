import type { RecipeDifficulty, RecipeStatus } from '@/types/recipe';

/** Nhãn hiển thị của trạng thái; giá trị Draft/Published/Archived là hợp đồng API nên giữ nguyên khi gửi và lọc. */
export const RECIPE_STATUS_LABEL: Record<RecipeStatus, string> = {
  Draft: 'Bản nháp',
  Published: 'Đã xuất bản',
  Archived: 'Đã lưu trữ',
};

export const DIFFICULTY_LABEL: Record<RecipeDifficulty, string> = {
  Easy: 'Dễ',
  Medium: 'Trung bình',
  Hard: 'Khó',
  Expert: 'Chuyên gia',
};

export const DIFFICULTY_STYLE: Record<RecipeDifficulty, string> = {
  Easy: 'bg-navy text-gold',
  Medium: 'bg-navy text-gold',
  Hard: 'bg-navy text-gold',
  Expert: 'bg-navy text-gold',
};

export function formatMinutes(minutes: number): string {
  if (minutes <= 0) return 'Không cần nấu';
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (hours === 0) return `${rest} phút`;
  return rest === 0 ? `${hours} giờ` : `${hours} giờ ${rest} phút`;
}

/**
 * FR-RCP-009 – quy tắc hiển thị: ưu tiên `quantityText` (nguyên văn người viết nhập); không có thì format từ
 * `quantity` + `unit`.
 */
export function formatQuantity(
  quantity: number | null,
  unit: string | null,
  quantityText: string | null = null,
): string {
  if (quantityText) return quantityText;
  if (quantity === null) return unit ? `vừa đủ (${unit})` : 'vừa đủ';
  const value = Number.isInteger(quantity) ? quantity.toString() : quantity.toLocaleString('vi-VN');
  return unit ? `${value} ${unit}` : value;
}

export function formatDate(iso: string | null): string {
  if (!iso) return '';
  return new Date(iso).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}
