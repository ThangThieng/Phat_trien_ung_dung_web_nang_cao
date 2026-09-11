import type { RecipeNutrition } from '@/types/api';

const ROWS: { key: keyof RecipeNutrition; label: string; unit: string }[] = [
  { key: 'calories', label: 'Năng lượng', unit: 'kcal' },
  { key: 'protein', label: 'Chất đạm', unit: 'g' },
  { key: 'carbohydrates', label: 'Tinh bột', unit: 'g' },
  { key: 'fat', label: 'Chất béo', unit: 'g' },
  { key: 'fiber', label: 'Chất xơ', unit: 'g' },
  { key: 'sodium', label: 'Natri', unit: 'mg' },
];

/** Bảng dinh dưỡng / khẩu phần (Owned Entity RecipeNutrition – SRS §7.2.1). */
export default function NutritionTable({ nutrition }: { nutrition: RecipeNutrition }) {
  const rows = ROWS.filter((row) => nutrition[row.key] !== null);
  if (rows.length === 0) return null;

  return (
    <table className="w-full text-sm">
      <caption className="mb-2 text-left text-xs text-gray-500">Giá trị trên 1 khẩu phần</caption>
      <tbody className="divide-y divide-gray-100">
        {rows.map((row) => (
          <tr key={row.key}>
            <th scope="row" className="py-2 text-left font-normal text-gray-600">
              {row.label}
            </th>
            <td className="py-2 text-right font-semibold text-gray-900">
              {Number(nutrition[row.key]).toLocaleString('vi-VN', { maximumFractionDigits: 1 })}{' '}
              {row.unit}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
