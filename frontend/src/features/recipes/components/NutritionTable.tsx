import type { Nutrition } from '@/types/recipe';

const ROWS: { key: keyof Nutrition; label: string; unit: string }[] = [
  { key: 'calories', label: 'Năng lượng', unit: 'kcal' },
  { key: 'protein', label: 'Chất đạm', unit: 'g' },
  { key: 'carbohydrates', label: 'Tinh bột', unit: 'g' },
  { key: 'fat', label: 'Chất béo', unit: 'g' },
  { key: 'fiber', label: 'Chất xơ', unit: 'g' },
  { key: 'sodium', label: 'Natri', unit: 'mg' },
];

/** Bảng dinh dưỡng / khẩu phần (Owned Entity RecipeNutrition – SRS §7.2.1). */
export default function NutritionTable({ nutrition }: { nutrition: Nutrition }) {
  const rows = ROWS.filter((row) => nutrition[row.key] !== null);
  if (rows.length === 0) return null;

  return (
    <table className="w-full text-sm">
      <caption className="mb-2 text-left text-xs text-ink-soft">Giá trị trên 1 khẩu phần</caption>
      <tbody className="divide-y divide-line">
        {rows.map((row) => (
          <tr key={row.key}>
            <th scope="row" className="py-2 text-left font-normal text-ink-soft">
              {row.label}
            </th>
            <td className="py-2 text-right font-semibold text-ink">
              {Number(nutrition[row.key]).toLocaleString('vi-VN', { maximumFractionDigits: 1 })}{' '}
              {row.unit}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
