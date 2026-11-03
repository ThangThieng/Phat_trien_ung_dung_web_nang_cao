'use client';

/* eslint-disable no-restricted-syntax, no-await-in-loop, jsx-a11y/label-has-associated-control, no-nested-ternary, no-param-reassign -- child endpoints must be synchronized in deterministic order after the parent concurrency check; compact field labels contain controls. */

import { useEffect, useMemo, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { apiFetch, ApiError, getErrorMessage } from '@/lib/api-client';
import { Button } from '@/components/ui/button';
import type { Category } from '@/types/category';
import type { Recipe, RecipeImageResult } from '@/types/recipe';
import { DIFFICULTIES, NUTRITION_FIELDS, recipeFormSchema, toCreateRecipeAggregate, type RecipeFormValues } from '../schemas';
import { addIngredient, addStep, createRecipe, deleteIngredient, deleteStep, getMyRecipe, reorderSteps, updateIngredient, updateRecipe, updateStep } from '../write-api';
import RecipeImageGallery from './RecipeImageGallery';
import RecipeLifecycleActions from './RecipeLifecycleActions';

const STEPS = ['Thông tin', 'Nguyên liệu', 'Các bước', 'Ảnh', 'Xem lại'] as const;
const numberValue = (value: string) => value === '' ? null : Number(value);
const optionalNumber = (value: string) => value === '' ? Number.NaN : Number(value);
const blankIngredient = (orderIndex: number) => ({ name: '', quantity: null, quantityText: null, unit: null, notes: null, orderIndex });
const blankStep = () => ({ title: '', description: '', timerMinutes: null, imageUrl: null });

function defaults(recipe?: Recipe): RecipeFormValues {
  if (!recipe) return { title: '', description: '', categoryId: '', prepTime: 0, cookTime: 0, servings: 1, difficulty: 'Easy', instructions: '', calories: Number.NaN, protein: Number.NaN, carbohydrates: Number.NaN, fat: Number.NaN, fiber: Number.NaN, sodium: Number.NaN, ingredients: [], steps: [] };
  return { title: recipe.title, description: recipe.description, categoryId: recipe.category.id, prepTime: recipe.prepTimeMinutes, cookTime: recipe.cookTimeMinutes, servings: recipe.servings, difficulty: recipe.difficulty, instructions: recipe.instructions ?? '', calories: recipe.nutrition?.calories ?? Number.NaN, protein: recipe.nutrition?.protein ?? Number.NaN, carbohydrates: recipe.nutrition?.carbohydrates ?? Number.NaN, fat: recipe.nutrition?.fat ?? Number.NaN, fiber: recipe.nutrition?.fiber ?? Number.NaN, sodium: recipe.nutrition?.sodium ?? Number.NaN, ingredients: recipe.ingredients.map((item) => ({ serverId: item.id, name: item.name, quantity: item.quantity, quantityText: item.quantityText, unit: item.unit, notes: item.notes, orderIndex: item.orderIndex })), steps: recipe.steps.map((item) => ({ serverId: item.id, title: item.title, description: item.description, timerMinutes: item.timerMinutes, imageUrl: item.imageUrl })) };
}

function imageResults(recipe: Recipe): RecipeImageResult[] {
  return recipe.images.map((image) => ({ imageId: image.id, originalUrl: image.originalUrl, mediumUrl: image.mediumUrl, thumbnailUrl: image.thumbnailUrl, altText: image.altText, isPrimary: image.isPrimary, orderIndex: image.orderIndex }));
}

export default function RecipeWizard({ accessToken, initialRecipe, onCompleted }: { accessToken: string; initialRecipe?: Recipe; onCompleted?: (recipe: Recipe) => void }) {
  const [step, setStep] = useState(0);
  const [recipe, setRecipe] = useState<Recipe | undefined>(initialRecipe);
  const [categories, setCategories] = useState<Category[]>([]);
  const [saving, setSaving] = useState(false);
  const [conflict, setConflict] = useState(false);
  const form = useForm<RecipeFormValues>({ resolver: zodResolver(recipeFormSchema), defaultValues: defaults(initialRecipe), mode: 'onTouched' });
  const ingredients = useFieldArray({ control: form.control, name: 'ingredients' });
  const steps = useFieldArray({ control: form.control, name: 'steps' });
  const canPublish = useMemo(() => Boolean(recipe?.ingredients.length && recipe.steps.length && recipe.images.every((image) => Boolean(image.altText?.trim()))), [recipe]);

  useEffect(() => { apiFetch<Category[]>('/categories').then(setCategories).catch(() => setCategories([])); }, []);
  useEffect(() => {
    const warn = (event: BeforeUnloadEvent) => { if (form.formState.isDirty) { event.preventDefault(); event.returnValue = ''; } };
    window.addEventListener('beforeunload', warn); return () => window.removeEventListener('beforeunload', warn);
  }, [form.formState.isDirty]);

  const reload = async () => { if (!recipe) return; const fresh = await getMyRecipe(recipe.id, accessToken); setRecipe(fresh); form.reset(defaults(fresh)); setConflict(false); };
  const syncExisting = async (values: RecipeFormValues) => {
    if (!recipe) return;
    // Parent first: a stale ETag/rowVersion stops before any child write.
    await updateRecipe(recipe.id, { ...toCreateRecipeAggregate(values), rowVersion: recipe.rowVersion }, accessToken);
    const oldIngredients = new Map(recipe.ingredients.map((item) => [item.id, item]));
    for (const [index, item] of values.ingredients.entries()) {
      const payload = { name: item.name, quantity: item.quantity, quantityText: item.quantityText || null, unit: item.unit || null, notes: item.notes || null, orderIndex: index };
      if (item.serverId) { await updateIngredient(recipe.id, item.serverId, payload, accessToken); oldIngredients.delete(item.serverId); } else await addIngredient(recipe.id, payload, accessToken);
    }
    for (const id of oldIngredients.keys()) await deleteIngredient(recipe.id, id, accessToken);
    const oldSteps = new Map(recipe.steps.map((item) => [item.id, item]));
    const orderedIds: string[] = [];
    for (const item of values.steps) {
      const payload = { title: item.title, description: item.description, timerMinutes: item.timerMinutes, imageUrl: item.imageUrl || null };
      if (item.serverId) { await updateStep(recipe.id, item.serverId, payload, accessToken); oldSteps.delete(item.serverId); orderedIds.push(item.serverId); } else orderedIds.push((await addStep(recipe.id, payload, accessToken)).id);
    }
    for (const id of oldSteps.keys()) await deleteStep(recipe.id, id, accessToken);
    if (orderedIds.length) await reorderSteps(recipe.id, orderedIds, accessToken);
    const fresh = await getMyRecipe(recipe.id, accessToken); setRecipe(fresh); form.reset(defaults(fresh)); onCompleted?.(fresh);
  };
  const saveAggregate = async () => {
    const values = form.getValues(); setSaving(true);
    try {
      if (!recipe) { const created = await createRecipe(toCreateRecipeAggregate(values), accessToken); setRecipe(created); form.reset(defaults(created)); onCompleted?.(created); toast.success('Đã tạo bản nháp với nguyên liệu và các bước trong một lần lưu.'); }
      else { await syncExisting(values); toast.success('Đã lưu các thay đổi.'); }
      setStep(3);
    } catch (error) { if (error instanceof ApiError && error.status === 409) setConflict(true); else toast.error(getErrorMessage(error)); }
    finally { setSaving(false); }
  };
  const next = async () => {
    const fields = step === 0 ? ['title', 'description', 'categoryId', 'prepTime', 'cookTime', 'servings', 'difficulty', 'instructions'] as const : step === 1 ? ['ingredients'] as const : step === 2 ? ['steps'] as const : [];
    if (fields.length && !(await form.trigger(fields))) return;
    if (step === 2) { await saveAggregate(); return; }
    setStep((current) => Math.min(current + 1, 4));
  };
  const input = 'w-full rounded border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-orange-500';
  return <form noValidate onSubmit={(event) => { event.preventDefault(); next().catch(() => undefined); }} className="space-y-6">
    <ol className="flex flex-wrap gap-2" aria-label="Các bước soạn công thức">{STEPS.map((label, index) => <li key={label} aria-current={step === index ? 'step' : undefined} className={`rounded-full px-3 py-1 text-sm ${step === index ? 'bg-orange-600 font-semibold text-white' : 'bg-gray-100 text-gray-600'}`}>{index + 1}. {label}</li>)}</ol>
    {step === 0 && <section className="grid gap-4 md:grid-cols-2"><label className="md:col-span-2">Tiêu đề<input className={input} {...form.register('title')} /></label><label className="md:col-span-2">Mô tả<textarea className={input} rows={3} {...form.register('description')} /></label><label>Danh mục<select className={input} {...form.register('categoryId')}><option value="">Chọn danh mục</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label><label>Độ khó<select className={input} {...form.register('difficulty')}>{DIFFICULTIES.map((difficulty) => <option key={difficulty} value={difficulty}>{difficulty}</option>)}</select></label><label>Chuẩn bị (phút)<input className={input} type="number" {...form.register('prepTime', { setValueAs: optionalNumber })} /></label><label>Nấu (phút)<input className={input} type="number" {...form.register('cookTime', { setValueAs: optionalNumber })} /></label><label>Khẩu phần<input className={input} type="number" {...form.register('servings', { setValueAs: optionalNumber })} /></label><fieldset className="md:col-span-2 grid gap-3 rounded border p-4"><legend>Dinh dưỡng mỗi khẩu phần (tùy chọn)</legend>{NUTRITION_FIELDS.map(({ key, label }) => <label key={key}>{label}<input className={input} type="number" step="0.01" {...form.register(key, { setValueAs: optionalNumber })} /></label>)}</fieldset><label className="md:col-span-2">Hướng dẫn chung<textarea className={input} rows={5} {...form.register('instructions')} /></label></section>}
    {step === 1 && <section><div className="flex items-center justify-between"><h2 className="text-lg font-semibold">Nguyên liệu</h2><Button type="button" variant="outline" onClick={() => ingredients.append(blankIngredient(ingredients.fields.length))}>Thêm nguyên liệu</Button></div><div className="mt-4 space-y-3">{ingredients.fields.length === 0 && <p className="text-sm text-gray-600">Thêm ít nhất một nguyên liệu để có thể xuất bản.</p>}{ingredients.fields.map((field, index) => <div key={field.id} className="grid gap-2 rounded border p-3 md:grid-cols-3"><input className={input} placeholder="Tên *" {...form.register(`ingredients.${index}.name`)} /><input className={input} type="number" step="0.001" placeholder="Số lượng" {...form.register(`ingredients.${index}.quantity`, { setValueAs: numberValue })} /><input className={input} placeholder="Định lượng chữ" {...form.register(`ingredients.${index}.quantityText`, { setValueAs: (value) => value || null })} /><input className={input} placeholder="Đơn vị" {...form.register(`ingredients.${index}.unit`, { setValueAs: (value) => value || null })} /><input className={input} placeholder="Ghi chú" {...form.register(`ingredients.${index}.notes`, { setValueAs: (value) => value || null })} /><Button type="button" variant="destructive" onClick={() => ingredients.remove(index)}>Xóa</Button></div>)}</div></section>}
    {step === 2 && <section><div className="flex items-center justify-between"><h2 className="text-lg font-semibold">Các bước chế biến</h2><Button type="button" variant="outline" onClick={() => steps.append(blankStep())}>Thêm bước</Button></div><div className="mt-4 space-y-3">{steps.fields.length === 0 && <p className="text-sm text-gray-600">Thêm ít nhất một bước để có thể xuất bản.</p>}{steps.fields.map((field, index) => <div key={field.id} className="grid gap-2 rounded border p-3"><div className="flex gap-2"><strong className="flex-1">Bước {index + 1}</strong><Button type="button" size="sm" variant="outline" disabled={index === 0} onClick={() => steps.move(index, index - 1)}>↑</Button><Button type="button" size="sm" variant="outline" disabled={index === steps.fields.length - 1} onClick={() => steps.move(index, index + 1)}>↓</Button><Button type="button" size="sm" variant="destructive" onClick={() => steps.remove(index)}>Xóa</Button></div><input className={input} placeholder="Tiêu đề bước *" {...form.register(`steps.${index}.title`)} /><textarea className={input} placeholder="Mô tả thực hiện *" {...form.register(`steps.${index}.description`)} /><input className={input} type="number" placeholder="Hẹn giờ (phút)" {...form.register(`steps.${index}.timerMinutes`, { setValueAs: numberValue })} /></div>)}</div></section>}
    {step === 3 && <section>{recipe ? <RecipeImageGallery key={recipe.rowVersion} recipeId={recipe.id} recipeTitle={recipe.title} accessToken={accessToken} initialImages={imageResults(recipe)} onChanged={() => { reload().catch(() => undefined); }} /> : <p role="alert">Hãy hoàn tất ba bước đầu trước.</p>}</section>}
    {step === 4 && <section className="space-y-4"><h2 className="text-lg font-semibold">Xem lại và xuất bản</h2><p className="text-sm text-gray-600">{canPublish ? 'Công thức đã đủ nguyên liệu, bước và alt text ảnh để xuất bản.' : 'Cần ít nhất một nguyên liệu, một bước và alt text cho mọi ảnh trước khi xuất bản.'}</p>{recipe && <RecipeLifecycleActions recipe={recipe} accessToken={accessToken} canPublish={canPublish} onChanged={(nextRecipe) => { setRecipe(nextRecipe); form.reset(defaults(nextRecipe)); }} onConflict={() => { reload().catch(() => undefined); }} />}</section>}
    <div className="flex justify-between border-t pt-4"><Button type="button" variant="outline" disabled={step === 0 || saving} onClick={() => setStep((current) => current - 1)}>Quay lại</Button>{step < 4 ? <Button type="submit" isLoading={saving}>{step === 2 ? (recipe ? 'Lưu và sang bước ảnh' : 'Tạo công thức') : 'Tiếp tục'}</Button> : <Button type="button" onClick={() => form.reset(defaults(recipe))}>Hoàn tất</Button>}</div>
    {conflict && <div role="alert" className="rounded border border-red-300 bg-red-50 p-4 text-sm text-red-800">Công thức đã được cập nhật ở tab khác. <button type="button" className="font-semibold underline" onClick={() => { reload().catch(() => undefined); }}>Tải lại và so sánh phiên bản mới</button></div>}
  </form>;
}
