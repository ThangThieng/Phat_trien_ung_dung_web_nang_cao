import type { RecipeDifficulty } from '@/types/api';

export const DIFFICULTY_LABEL: Record<RecipeDifficulty, string> = {
  Easy: 'Dễ',
  Medium: 'Trung bình',
  Hard: 'Khó',
  Expert: 'Chuyên gia',
};

export const DIFFICULTY_STYLE: Record<RecipeDifficulty, string> = {
  Easy: 'bg-green-100 text-green-800',
  Medium: 'bg-yellow-100 text-yellow-800',
  Hard: 'bg-orange-100 text-orange-800',
  Expert: 'bg-red-100 text-red-800',
};

export function formatMinutes(minutes: number): string {
  if (minutes <= 0) return 'Không cần nấu';
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (hours === 0) return `${rest} phút`;
  return rest === 0 ? `${hours} giờ` : `${hours} giờ ${rest} phút`;
}

export function formatQuantity(quantity: number | null, unit: string | null): string {
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
