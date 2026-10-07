'use client';

import { useState } from 'react';
import { ArrowDown, ArrowUp, Pencil, Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import type { RecipeIngredient, RecipeStep } from '@/types/api';
import { formatQuantity } from '../format';
import {
  addIngredient,
  addStep,
  deleteIngredient,
  deleteStep,
  reorderSteps,
  updateIngredient,
  updateStep,
} from '../write-api';
import { getErrorMessage } from '@/lib/api-client';

type IngredientDraft = Omit<RecipeIngredient, 'id'>;
type StepDraft = Omit<RecipeStep, 'id' | 'stepNumber'>;

const emptyIngredient: IngredientDraft = {
  name: '', quantity: null, quantityText: null, unit: null, notes: null, orderIndex: 0,
};
const emptyStep: StepDraft = { title: '', description: '', timerMinutes: null, imageUrl: null };
const input = 'rounded-md border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-orange-500';

interface Props {
  recipeId: string;
  accessToken: string;
  ingredients: RecipeIngredient[];
  steps: RecipeStep[];
  onChanged: () => Promise<void>;
}

/** FR-RCP-009/010 - form quản trị nguyên liệu và bước nấu của đúng aggregate Recipe. */
export default function RecipeContentEditor({ recipeId, accessToken, ingredients, steps, onChanged }: Props) {
  const [ingredient, setIngredient] = useState<IngredientDraft>(emptyIngredient);
  const [editingIngredientId, setEditingIngredientId] = useState<string | null>(null);
  const [step, setStep] = useState<StepDraft>(emptyStep);
  const [editingStepId, setEditingStepId] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const run = async (action: () => Promise<unknown>, message: string) => {
    setBusy(true);
    try {
      await action();
      await onChanged();
      toast.success(message);
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setBusy(false);
    }
  };

  const saveIngredient = () => run(async () => {
    const body = { ...ingredient, name: ingredient.name.trim(), orderIndex: ingredient.orderIndex };
    if (editingIngredientId) await updateIngredient(recipeId, editingIngredientId, body, accessToken);
    else await addIngredient(recipeId, body, accessToken);
    setIngredient(emptyIngredient);
    setEditingIngredientId(null);
  }, editingIngredientId ? 'Đã cập nhật nguyên liệu.' : 'Đã thêm nguyên liệu.');

  const saveStep = () => run(async () => {
    const body = { ...step, title: step.title.trim(), description: step.description.trim() };
    if (editingStepId) await updateStep(recipeId, editingStepId, body, accessToken);
    else await addStep(recipeId, body, accessToken);
    setStep(emptyStep);
    setEditingStepId(null);
  }, editingStepId ? 'Đã cập nhật bước nấu.' : 'Đã thêm bước nấu.');

  const moveStep = (index: number, direction: -1 | 1) => {
    const next = [...steps];
    const target = index + direction;
    if (target < 0 || target >= next.length) return;
    [next[index], next[target]] = [next[target], next[index]];
    void run(() => reorderSteps(recipeId, next.map((item) => item.id), accessToken), 'Đã cập nhật thứ tự bước nấu.');
  };

  return (
    <div className="grid gap-8 lg:grid-cols-2">
      <section aria-labelledby="ingredients-heading">
        <h2 id="ingredients-heading" className="text-lg font-semibold text-gray-900">Nguyên liệu</h2>
        <ul className="mt-3 divide-y rounded-xl border border-gray-200 bg-white">
          {ingredients.length === 0 && <li className="p-4 text-sm text-gray-500">Chưa có nguyên liệu.</li>}
          {ingredients.map((item) => (
            <li key={item.id} className="flex items-center justify-between gap-3 p-3 text-sm">
              <span><b>{item.name}</b> - {formatQuantity(item.quantity, item.unit, item.quantityText)}{item.notes ? ` (${item.notes})` : ''}</span>
              <span className="flex shrink-0 gap-1">
                <button type="button" aria-label={`Sửa ${item.name}`} onClick={() => { setEditingIngredientId(item.id); setIngredient({ name: item.name, quantity: item.quantity, quantityText: item.quantityText, unit: item.unit, notes: item.notes, orderIndex: item.orderIndex }); }} className="rounded p-1 text-orange-700 hover:bg-orange-50"><Pencil className="h-4 w-4" /></button>
                <button type="button" aria-label={`Xóa ${item.name}`} disabled={busy} onClick={() => void run(() => deleteIngredient(recipeId, item.id, accessToken), 'Đã xóa nguyên liệu.')} className="rounded p-1 text-red-700 hover:bg-red-50"><Trash2 className="h-4 w-4" /></button>
              </span>
            </li>
          ))}
        </ul>
        <div className="mt-4 grid gap-2 rounded-xl bg-orange-50 p-4 sm:grid-cols-2">
          <input className={input} placeholder="Tên nguyên liệu *" value={ingredient.name} onChange={(e) => setIngredient({ ...ingredient, name: e.target.value })} />
          <input className={input} placeholder="Định lượng chữ (vd: vừa đủ)" value={ingredient.quantityText ?? ''} onChange={(e) => setIngredient({ ...ingredient, quantityText: e.target.value || null })} />
          <input className={input} type="number" min="0" step="0.01" placeholder="Số lượng" value={ingredient.quantity ?? ''} onChange={(e) => setIngredient({ ...ingredient, quantity: e.target.value === '' ? null : Number(e.target.value) })} />
          <input className={input} placeholder="Đơn vị" value={ingredient.unit ?? ''} onChange={(e) => setIngredient({ ...ingredient, unit: e.target.value || null })} />
          <input className={`${input} sm:col-span-2`} placeholder="Ghi chú (tùy chọn)" value={ingredient.notes ?? ''} onChange={(e) => setIngredient({ ...ingredient, notes: e.target.value || null })} />
          <div className="flex gap-2 sm:col-span-2">
            <button type="button" disabled={busy || !ingredient.name.trim() || (!ingredient.quantity && !ingredient.quantityText?.trim())} onClick={() => void saveIngredient()} className="inline-flex items-center gap-1 rounded-md bg-orange-600 px-3 py-2 text-sm font-semibold text-white disabled:opacity-50"><Plus className="h-4 w-4" />{editingIngredientId ? 'Lưu nguyên liệu' : 'Thêm nguyên liệu'}</button>
            {editingIngredientId && <button type="button" onClick={() => { setEditingIngredientId(null); setIngredient(emptyIngredient); }} className="rounded-md px-3 py-2 text-sm">Hủy</button>}
          </div>
        </div>
      </section>

      <section aria-labelledby="steps-heading">
        <h2 id="steps-heading" className="text-lg font-semibold text-gray-900">Các bước chế biến</h2>
        <ol className="mt-3 space-y-2">
          {steps.length === 0 && <li className="rounded-xl border border-gray-200 p-4 text-sm text-gray-500">Chưa có bước nấu.</li>}
          {steps.map((item, index) => (
            <li key={item.id} className="flex items-start gap-2 rounded-xl border border-gray-200 bg-white p-3 text-sm">
              <span className="mt-0.5 rounded-full bg-orange-100 px-2 py-0.5 font-semibold text-orange-800">{item.stepNumber}</span>
              <span className="min-w-0 flex-1"><b>{item.title}</b><span className="block text-gray-600">{item.description}</span>{item.timerMinutes !== null && <span className="text-xs text-gray-500">{item.timerMinutes} phút</span>}</span>
              <span className="flex gap-1">
                <button type="button" aria-label="Đưa bước lên" disabled={busy || index === 0} onClick={() => moveStep(index, -1)} className="rounded p-1 disabled:opacity-30"><ArrowUp className="h-4 w-4" /></button>
                <button type="button" aria-label="Đưa bước xuống" disabled={busy || index === steps.length - 1} onClick={() => moveStep(index, 1)} className="rounded p-1 disabled:opacity-30"><ArrowDown className="h-4 w-4" /></button>
                <button type="button" aria-label={`Sửa ${item.title}`} onClick={() => { setEditingStepId(item.id); setStep({ title: item.title, description: item.description, timerMinutes: item.timerMinutes, imageUrl: item.imageUrl }); }} className="rounded p-1 text-orange-700 hover:bg-orange-50"><Pencil className="h-4 w-4" /></button>
                <button type="button" aria-label={`Xóa ${item.title}`} disabled={busy} onClick={() => void run(() => deleteStep(recipeId, item.id, accessToken), 'Đã xóa bước nấu.')} className="rounded p-1 text-red-700 hover:bg-red-50"><Trash2 className="h-4 w-4" /></button>
              </span>
            </li>
          ))}
        </ol>
        <div className="mt-4 grid gap-2 rounded-xl bg-orange-50 p-4">
          <input className={input} placeholder="Tên bước *" value={step.title} onChange={(e) => setStep({ ...step, title: e.target.value })} />
          <textarea className={input} rows={3} placeholder="Mô tả thực hiện *" value={step.description} onChange={(e) => setStep({ ...step, description: e.target.value })} />
          <input className={input} type="number" min="0" placeholder="Hẹn giờ (phút, tùy chọn)" value={step.timerMinutes ?? ''} onChange={(e) => setStep({ ...step, timerMinutes: e.target.value === '' ? null : Number(e.target.value) })} />
          <div className="flex gap-2"><button type="button" disabled={busy || !step.title.trim() || !step.description.trim()} onClick={() => void saveStep()} className="inline-flex items-center gap-1 rounded-md bg-orange-600 px-3 py-2 text-sm font-semibold text-white disabled:opacity-50"><Plus className="h-4 w-4" />{editingStepId ? 'Lưu bước' : 'Thêm bước'}</button>{editingStepId && <button type="button" onClick={() => { setEditingStepId(null); setStep(emptyStep); }} className="rounded-md px-3 py-2 text-sm">Hủy</button>}</div>
        </div>
      </section>
    </div>
  );
}
