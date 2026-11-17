import { Clock, Users } from 'lucide-react';
import LoadingSkeleton from '@/components/shared/LoadingSkeleton';
import { DIFFICULTY_LABEL, formatMinutes, formatQuantity } from '@/features/recipes/format';
import { mockRecipes } from '@/mocks/fixtures/recipes';
import MockupShell from '../MockupShell';
import { assertDevOnly } from '../dev-only';

/**
 * Ca khó nhất cho bố cục chi tiết: "Cơm tấm sườn bì chả" — 14 nguyên liệu, có định lượng dạng chữ. Trang thật là việc của
 * Dev 2 ở Buổi 7; mockup này để kiểm khung (header, cột nguyên liệu/các bước) ở 320 / 768 / 1200 px.
 */
export default function RecipeMockup() {
  assertDevOnly();
  const recipe = [...mockRecipes].sort((a, b) => b.ingredients.length - a.ingredients.length)[0];
  return (
    <MockupShell>
      <article className="mx-auto max-w-5xl px-4 py-10">
        <p className="text-sm font-medium tracking-wide text-orange-700 uppercase">
          {recipe.category.name}
        </p>
        <h1 className="mt-2 text-3xl font-bold text-gray-900 sm:text-4xl">{recipe.title}</h1>
        <p className="mt-3 text-gray-700">{recipe.description}</p>
        <dl className="mt-4 flex flex-wrap gap-x-6 gap-y-2 text-sm text-gray-700">
          <div className="flex items-center gap-1">
            <Clock aria-hidden className="h-4 w-4" />
            <dt>Chuẩn bị</dt>
            <dd className="font-medium">{formatMinutes(recipe.prepTimeMinutes)}</dd>
          </div>
          <div className="flex items-center gap-1">
            <dt>Nấu</dt>
            <dd className="font-medium">{formatMinutes(recipe.cookTimeMinutes)}</dd>
          </div>
          <div className="flex items-center gap-1">
            <Users aria-hidden className="h-4 w-4" />
            <dt>Khẩu phần</dt>
            <dd className="font-medium">{recipe.servings} người</dd>
          </div>
          <div className="flex items-center gap-1">
            <dt>Độ khó</dt>
            <dd className="font-medium">{DIFFICULTY_LABEL[recipe.difficulty]}</dd>
          </div>
        </dl>
        <div
          aria-hidden
          className="mt-6 flex aspect-[4/3] items-center justify-center rounded-2xl bg-gradient-to-br from-orange-100 to-amber-50 text-7xl md:aspect-[16/9]"
        >
          🍲
        </div>

        <div className="mt-8 grid gap-8 md:grid-cols-[1fr_2fr]">
          <section aria-labelledby="ingredients-heading">
            <h2 id="ingredients-heading" className="text-xl font-semibold text-gray-900">
              Nguyên liệu ({recipe.ingredients.length})
            </h2>
            <ul className="mt-3 divide-y divide-gray-100 rounded-xl border border-gray-200 bg-white">
              {recipe.ingredients.map((ingredient) => (
                <li key={ingredient.id} className="flex justify-between gap-3 px-4 py-2 text-sm">
                  <span className="text-gray-900">
                    {ingredient.name}
                    {ingredient.notes && (
                      <span className="block text-xs text-gray-600">{ingredient.notes}</span>
                    )}
                  </span>
                  <span className="shrink-0 text-right font-medium text-gray-800">
                    {formatQuantity(ingredient.quantity, ingredient.unit, ingredient.quantityText)}
                  </span>
                </li>
              ))}
            </ul>
          </section>
          <section aria-labelledby="steps-heading">
            <h2 id="steps-heading" className="text-xl font-semibold text-gray-900">
              Các bước ({recipe.steps.length})
            </h2>
            <ol className="mt-3 space-y-4">
              {recipe.steps.map((step) => (
                <li
                  key={step.id}
                  className="flex gap-4 rounded-xl border border-gray-200 bg-white p-4"
                >
                  <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-orange-700 text-sm font-semibold text-white">
                    {step.stepNumber}
                  </span>
                  <div>
                    <h3 className="font-semibold text-gray-900">{step.title}</h3>
                    <p className="mt-1 text-sm text-gray-700">{step.description}</p>
                    {step.timerMinutes !== null && (
                      <p className="mt-1 text-xs text-gray-600">
                        ⏱ {formatMinutes(step.timerMinutes)}
                      </p>
                    )}
                  </div>
                </li>
              ))}
            </ol>
          </section>
        </div>
      </article>
      <section aria-label="Trạng thái đang tải" className="border-t border-dashed border-amber-300">
        <LoadingSkeleton variant="recipe-detail" label="Ví dụ khung chờ trang chi tiết" />
      </section>
    </MockupShell>
  );
}
