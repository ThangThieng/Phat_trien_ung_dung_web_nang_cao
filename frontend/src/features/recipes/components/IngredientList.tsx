'use client';

import { useState } from 'react';
import type { RecipeIngredient } from '@/types/recipe';

export function scaleQuantity(
  quantity: number,
  originalServings: number,
  targetServings: number,
): string {
  const scaled = (quantity * targetServings) / originalServings;
  return Number.isInteger(scaled)
    ? String(scaled)
    : scaled.toLocaleString('vi-VN', { maximumFractionDigits: 2 });
}

/** Nguyên liệu định lượng được theo khẩu phần; QuantityText luôn là văn bản nguyên gốc. */
export default function IngredientList({
  ingredients,
  servings: initialServings,
}: {
  ingredients: RecipeIngredient[];
  servings: number;
}) {
  const [servings, setServings] = useState(initialServings);
  const [prepared, setPrepared] = useState<Set<string>>(new Set());
  const validServings = Number.isFinite(servings) && servings > 0 ? servings : initialServings;
  const togglePrepared = (id: string) =>
    setPrepared((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  const formatAmount = (ingredient: RecipeIngredient) => {
    if (ingredient.quantityText) return ingredient.quantityText;
    if (ingredient.quantity === null)
      return ingredient.unit ? `vừa đủ (${ingredient.unit})` : 'vừa đủ';
    const quantity = scaleQuantity(ingredient.quantity, initialServings, validServings);
    return ingredient.unit ? `${quantity} ${ingredient.unit}` : quantity;
  };

  return (
    <section
      aria-labelledby="ingredients-heading"
      className="rounded-2xl border border-gray-200 bg-white p-5"
    >
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 id="ingredients-heading" className="text-xl font-bold text-gray-900">
          Nguyên liệu
        </h2>
        <label
          htmlFor="recipe-servings"
          className="flex items-center gap-2 text-sm font-medium text-gray-700"
        >
          Khẩu phần
          <input
            id="recipe-servings"
            aria-label="Số khẩu phần"
            type="number"
            min="1"
            inputMode="numeric"
            value={servings}
            onChange={(event) => setServings(Math.max(1, Number(event.target.value) || 1))}
            className="w-16 rounded-md border border-gray-300 px-2 py-1 text-center"
          />
        </label>
      </div>
      <ul className="mt-4 divide-y divide-gray-100">
        {ingredients.map((ingredient) => {
          const done = prepared.has(ingredient.id);
          const amount = formatAmount(ingredient);
          return (
            <li key={ingredient.id} className="flex items-start gap-3 py-3 text-sm">
              <input
                aria-label={`Đã chuẩn bị ${ingredient.name}`}
                type="checkbox"
                checked={done}
                onChange={() => togglePrepared(ingredient.id)}
                className="mt-0.5 size-4 accent-orange-700"
              />
              <span className={`flex-1 text-gray-900 ${done ? 'line-through opacity-60' : ''}`}>
                {ingredient.name}
                {ingredient.notes && (
                  <span className="block text-xs text-gray-500">{ingredient.notes}</span>
                )}
              </span>
              <span className="shrink-0 font-medium text-gray-700">{amount}</span>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
