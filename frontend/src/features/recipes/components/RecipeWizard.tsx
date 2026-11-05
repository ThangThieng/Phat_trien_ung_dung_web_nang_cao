'use client';

/* eslint-disable no-restricted-syntax, no-await-in-loop -- các endpoint con phải gọi tuần tự, dừng ngay ở lỗi đầu tiên */

import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Button } from '@/components/ui/button';
import { apiFetch, ApiError, getErrorMessage } from '@/lib/api-client';
import type { Category } from '@/types/category';
import type { Recipe, RecipeDifficulty, RecipeImageResult } from '@/types/recipe';
import {
  DIFFICULTIES,
  NUTRITION_FIELDS,
  recipeFormSchema,
  toCreateRecipeAggregate,
  type RecipeFormValues,
} from '../schemas';
import {
  addIngredient,
  addStep,
  createRecipe,
  deleteIngredient,
  deleteStep,
  getMyRecipe,
  reorderSteps,
  updateIngredient,
  updateRecipe,
  updateStep,
} from '../write-api';
import RecipeImageGallery from './RecipeImageGallery';
import RecipeLifecycleActions from './RecipeLifecycleActions';

const STEPS = ['Thông tin', 'Nguyên liệu', 'Các bước', 'Ảnh', 'Xem lại'] as const;
const DIFFICULTY_LABELS: Record<RecipeDifficulty, string> = {
  Easy: 'Dễ',
  Medium: 'Trung bình',
  Hard: 'Khó',
  Expert: 'Chuyên gia',
};
const INPUT =
  'mt-1 w-full rounded border border-line-strong px-3 py-2 text-sm focus:ring-2 focus:ring-gold-ink aria-invalid:border-red-600';

// RHF gọi setValueAs cả với null của defaultValues; Number(null) ra 0 thì ô trống thành 0 và không qua được validate.
const isBlank = (value: string | number | null | undefined) => value === '' || value === null || value === undefined;
const numberValue = (value: string | number | null | undefined) => (isBlank(value) ? null : Number(value));
const optionalNumber = (value: string | number | null | undefined) => (isBlank(value) ? Number.NaN : Number(value));
const textOrNull = (value: string | null | undefined) => value || null;

const blankIngredient = (orderIndex: number) => ({
  name: '',
  quantity: null,
  quantityText: null,
  unit: null,
  notes: null,
  orderIndex,
});
const blankStep = () => ({ title: '', description: '', timerMinutes: null, imageUrl: null });

function defaults(recipe?: Recipe): RecipeFormValues {
  const nutrient = (value?: number | null) => value ?? Number.NaN;
  return {
    title: recipe?.title ?? '',
    description: recipe?.description ?? '',
    categoryId: recipe?.category.id ?? '',
    prepTime: recipe?.prepTimeMinutes ?? 0,
    cookTime: recipe?.cookTimeMinutes ?? 0,
    servings: recipe?.servings ?? 1,
    difficulty: recipe?.difficulty ?? 'Easy',
    instructions: recipe?.instructions ?? '',
    calories: nutrient(recipe?.nutrition?.calories),
    protein: nutrient(recipe?.nutrition?.protein),
    carbohydrates: nutrient(recipe?.nutrition?.carbohydrates),
    fat: nutrient(recipe?.nutrition?.fat),
    fiber: nutrient(recipe?.nutrition?.fiber),
    sodium: nutrient(recipe?.nutrition?.sodium),
    ingredients: (recipe?.ingredients ?? []).map((item) => ({
      serverId: item.id,
      name: item.name,
      quantity: item.quantity,
      quantityText: item.quantityText,
      unit: item.unit,
      notes: item.notes,
      orderIndex: item.orderIndex,
    })),
    steps: (recipe?.steps ?? []).map((item) => ({
      serverId: item.id,
      title: item.title,
      description: item.description,
      timerMinutes: item.timerMinutes,
      imageUrl: item.imageUrl,
    })),
  };
}

function imageResults(recipe: Recipe): RecipeImageResult[] {
  return recipe.images.map((image) => ({
    imageId: image.id,
    originalUrl: image.originalUrl,
    mediumUrl: image.mediumUrl,
    thumbnailUrl: image.thumbnailUrl,
    altText: image.altText,
    isPrimary: image.isPrimary,
    orderIndex: image.orderIndex,
  }));
}

/** Điều còn thiếu để xuất bản; rỗng là xuất bản được (FR-RCP-006, altText theo WCAG 1.1.1). */
export function publishBlockers(recipe?: Recipe): string[] {
  if (!recipe) return ['Chưa lưu công thức.'];
  const blockers: string[] = [];
  if (recipe.ingredients.length === 0) blockers.push('Cần ít nhất một nguyên liệu.');
  if (recipe.steps.length === 0) blockers.push('Cần ít nhất một bước chế biến.');
  if (recipe.images.some((image) => !image.altText?.trim())) blockers.push('Mọi ảnh cần có mô tả ảnh (alt text).');
  return blockers;
}

/** Khác biệt giữa bản đang sửa và bản mới trên server, để người dùng quyết định sau xung đột 409. */
function describeDifferences(values: RecipeFormValues, fresh: Recipe): string[] {
  const rows: [string, unknown, unknown][] = [
    ['Tiêu đề', values.title, fresh.title],
    ['Mô tả', values.description, fresh.description],
    ['Thời gian chuẩn bị', values.prepTime, fresh.prepTimeMinutes],
    ['Thời gian nấu', values.cookTime, fresh.cookTimeMinutes],
    ['Khẩu phần', values.servings, fresh.servings],
    ['Độ khó', values.difficulty, fresh.difficulty],
    ['Số nguyên liệu', values.ingredients.length, fresh.ingredients.length],
    ['Số bước', values.steps.length, fresh.steps.length],
  ];
  return rows
    .filter(([, mine, theirs]) => mine !== theirs)
    .map(([label, mine, theirs]) => `${label}: bạn nhập "${String(mine)}", trên máy chủ là "${String(theirs)}"`);
}

function Field({ label, error, className, children }: { label: string; error?: string; className?: string; children: ReactNode }) {
  return (
    // eslint-disable-next-line jsx-a11y/label-has-associated-control -- ô nhập được truyền qua children
    <label className={`block text-sm font-medium text-ink ${className ?? ''}`}>
      {label}
      {children}
      {error && (
        <span role="alert" className="mt-1 block text-sm font-normal text-red-700">
          {error}
        </span>
      )}
    </label>
  );
}

/** Lưu tuần tự có chủ ý: PUT cha trước để phát hiện 409 trước khi sửa bất kỳ dòng con nào (FR-RCP-004, 009, 010). */
async function syncChildren(recipe: Recipe, values: RecipeFormValues, accessToken: string) {
  const oldIngredients = new Map(recipe.ingredients.map((item) => [item.id, item]));
  for (const [index, item] of values.ingredients.entries()) {
    const payload = {
      name: item.name,
      quantity: item.quantity,
      quantityText: item.quantityText || null,
      unit: item.unit || null,
      notes: item.notes || null,
      orderIndex: index,
    };
    const old = item.serverId ? oldIngredients.get(item.serverId) : undefined;
    if (item.serverId) oldIngredients.delete(item.serverId);
    const unchanged =
      old &&
      old.name === payload.name &&
      old.quantity === payload.quantity &&
      old.quantityText === payload.quantityText &&
      old.unit === payload.unit &&
      old.notes === payload.notes &&
      old.orderIndex === payload.orderIndex;
    if (!item.serverId) await addIngredient(recipe.id, payload, accessToken);
    else if (!unchanged) await updateIngredient(recipe.id, item.serverId, payload, accessToken);
  }
  for (const id of oldIngredients.keys()) await deleteIngredient(recipe.id, id, accessToken);

  const oldSteps = new Map(recipe.steps.map((item) => [item.id, item]));
  const orderedIds: string[] = [];
  for (const item of values.steps) {
    const payload = {
      title: item.title,
      description: item.description,
      timerMinutes: item.timerMinutes,
      imageUrl: item.imageUrl || null,
    };
    if (item.serverId) {
      const old = oldSteps.get(item.serverId);
      oldSteps.delete(item.serverId);
      orderedIds.push(item.serverId);
      const unchanged =
        old &&
        old.title === payload.title &&
        old.description === payload.description &&
        old.timerMinutes === payload.timerMinutes &&
        old.imageUrl === payload.imageUrl;
      if (!unchanged) await updateStep(recipe.id, item.serverId, payload, accessToken);
    } else {
      orderedIds.push((await addStep(recipe.id, payload, accessToken)).id);
    }
  }
  for (const id of oldSteps.keys()) await deleteStep(recipe.id, id, accessToken);

  const sameOrder =
    orderedIds.length === recipe.steps.length && orderedIds.every((id, index) => recipe.steps[index]?.id === id);
  if (orderedIds.length > 0 && !sameOrder) await reorderSteps(recipe.id, orderedIds, accessToken);
}

export default function RecipeWizard({
  accessToken,
  initialRecipe,
  onCompleted,
}: {
  accessToken: string;
  initialRecipe?: Recipe;
  onCompleted?: (recipe: Recipe) => void;
}) {
  const router = useRouter();
  const [step, setStep] = useState(0);
  const [recipe, setRecipe] = useState<Recipe | undefined>(initialRecipe);
  const [categories, setCategories] = useState<Category[]>([]);
  const [saving, setSaving] = useState(false);
  const [conflict, setConflict] = useState(false);
  const [differences, setDifferences] = useState<string[] | null>(null);
  const form = useForm<RecipeFormValues>({
    resolver: zodResolver(recipeFormSchema),
    defaultValues: defaults(initialRecipe),
    mode: 'onTouched',
  });
  const { errors } = form.formState;
  const ingredients = useFieldArray({ control: form.control, name: 'ingredients' });
  const steps = useFieldArray({ control: form.control, name: 'steps' });
  const blockers = useMemo(() => publishBlockers(recipe), [recipe]);

  useEffect(() => {
    apiFetch<Category[]>('/categories')
      .then(setCategories)
      .catch(() => setCategories([]));
  }, []);
  useEffect(() => {
    const warn = (event: BeforeUnloadEvent) => {
      if (form.formState.isDirty) event.preventDefault();
    };
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, [form.formState.isDirty]);

  const applyFresh = (fresh: Recipe) => {
    setRecipe(fresh);
    form.reset(defaults(fresh));
  };
  const reload = async () => {
    if (!recipe) return;
    applyFresh(await getMyRecipe(recipe.id, accessToken));
    setConflict(false);
    setDifferences(null);
  };
  const showDifferences = async () => {
    if (!recipe) return;
    const fresh = await getMyRecipe(recipe.id, accessToken);
    setDifferences(describeDifferences(form.getValues(), fresh));
  };
  const saveAggregate = async () => {
    const values = form.getValues();
    setSaving(true);
    try {
      if (recipe) {
        await updateRecipe(recipe.id, { ...toCreateRecipeAggregate(values), rowVersion: recipe.rowVersion }, accessToken);
        await syncChildren(recipe, values, accessToken);
        const fresh = await getMyRecipe(recipe.id, accessToken);
        applyFresh(fresh);
        onCompleted?.(fresh);
        toast.success('Đã lưu các thay đổi.');
      } else {
        const created = await createRecipe(toCreateRecipeAggregate(values), accessToken);
        applyFresh(created);
        onCompleted?.(created);
        toast.success('Đã tạo bản nháp với nguyên liệu và các bước trong một lần lưu.');
      }
      setStep(3);
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) setConflict(true);
      else toast.error(getErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };
  const next = async () => {
    const fieldsOfStep = [
      ['title', 'description', 'categoryId', 'prepTime', 'cookTime', 'servings', 'difficulty', 'instructions',
        ...NUTRITION_FIELDS.map(({ key }) => key)],
      ['ingredients'],
      ['steps'],
    ] as const;
    const fields = fieldsOfStep[step];
    if (fields && !(await form.trigger([...fields]))) return;
    if (step === 2) {
      await saveAggregate();
      return;
    }
    setStep((current) => Math.min(current + 1, STEPS.length - 1));
  };

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        next().catch(() => undefined);
      }}
      className="space-y-6"
    >
      <ol className="flex flex-wrap gap-2" aria-label="Các bước soạn công thức">
        {STEPS.map((label, index) => (
          <li
            key={label}
            aria-current={step === index ? 'step' : undefined}
            className={`rounded-full px-3 py-1 text-sm ${step === index ? 'bg-navy font-semibold text-white' : 'bg-gold-soft text-ink'}`}
          >
            {index + 1}. {label}
          </li>
        ))}
      </ol>

      {step === 0 && (
        <section className="grid gap-4 md:grid-cols-2">
          <Field label="Tiêu đề" error={errors.title?.message} className="md:col-span-2">
            <input className={INPUT} aria-invalid={Boolean(errors.title)} {...form.register('title')} />
          </Field>
          <Field label="Mô tả" error={errors.description?.message} className="md:col-span-2">
            <textarea className={INPUT} rows={3} aria-invalid={Boolean(errors.description)} {...form.register('description')} />
          </Field>
          <Field label="Danh mục" error={errors.categoryId?.message}>
            <select className={INPUT} aria-invalid={Boolean(errors.categoryId)} {...form.register('categoryId')}>
              <option value="">Chọn danh mục</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Độ khó" error={errors.difficulty?.message}>
            <select className={INPUT} {...form.register('difficulty')}>
              {DIFFICULTIES.map((difficulty) => (
                <option key={difficulty} value={difficulty}>
                  {DIFFICULTY_LABELS[difficulty]}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Chuẩn bị (phút)" error={errors.prepTime?.message}>
            <input className={INPUT} type="number" aria-invalid={Boolean(errors.prepTime)} {...form.register('prepTime', { setValueAs: optionalNumber })} />
          </Field>
          <Field label="Nấu (phút)" error={errors.cookTime?.message}>
            <input className={INPUT} type="number" aria-invalid={Boolean(errors.cookTime)} {...form.register('cookTime', { setValueAs: optionalNumber })} />
          </Field>
          <Field label="Khẩu phần" error={errors.servings?.message}>
            <input className={INPUT} type="number" aria-invalid={Boolean(errors.servings)} {...form.register('servings', { setValueAs: optionalNumber })} />
          </Field>
          <fieldset className="grid gap-3 rounded border p-4 md:col-span-2 md:grid-cols-2">
            <legend className="px-1 text-sm font-medium">Dinh dưỡng mỗi khẩu phần (tùy chọn)</legend>
            {NUTRITION_FIELDS.map(({ key, label }) => (
              <Field key={key} label={label} error={errors[key]?.message}>
                <input className={INPUT} type="number" step="0.01" aria-invalid={Boolean(errors[key])} {...form.register(key, { setValueAs: optionalNumber })} />
              </Field>
            ))}
          </fieldset>
          <Field label="Hướng dẫn chung" error={errors.instructions?.message} className="md:col-span-2">
            <textarea className={INPUT} rows={5} {...form.register('instructions')} />
          </Field>
        </section>
      )}

      {step === 1 && (
        <section>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">Nguyên liệu</h2>
            <Button type="button" variant="outline" onClick={() => ingredients.append(blankIngredient(ingredients.fields.length))}>
              Thêm nguyên liệu
            </Button>
          </div>
          <div className="mt-4 space-y-3">
            {ingredients.fields.length === 0 && (
              <p className="text-sm text-ink-soft">Thêm ít nhất một nguyên liệu để có thể xuất bản.</p>
            )}
            {ingredients.fields.map((field, index) => {
              const fieldErrors = errors.ingredients?.[index];
              return (
                <div key={field.id} className="grid gap-2 rounded border p-3 md:grid-cols-3">
                  <Field label={`Tên nguyên liệu ${index + 1}`} error={fieldErrors?.name?.message}>
                    <input className={INPUT} aria-invalid={Boolean(fieldErrors?.name)} {...form.register(`ingredients.${index}.name`)} />
                  </Field>
                  <Field label="Số lượng" error={fieldErrors?.quantity?.message}>
                    <input className={INPUT} type="number" step="0.001" aria-invalid={Boolean(fieldErrors?.quantity)} {...form.register(`ingredients.${index}.quantity`, { setValueAs: numberValue })} />
                  </Field>
                  <Field label="Định lượng chữ" error={fieldErrors?.quantityText?.message}>
                    <input className={INPUT} placeholder="vừa đủ" {...form.register(`ingredients.${index}.quantityText`, { setValueAs: textOrNull })} />
                  </Field>
                  <Field label="Đơn vị" error={fieldErrors?.unit?.message}>
                    <input className={INPUT} {...form.register(`ingredients.${index}.unit`, { setValueAs: textOrNull })} />
                  </Field>
                  <Field label="Ghi chú" error={fieldErrors?.notes?.message}>
                    <input className={INPUT} {...form.register(`ingredients.${index}.notes`, { setValueAs: textOrNull })} />
                  </Field>
                  <div className="flex items-end">
                    <Button type="button" variant="destructive" aria-label={`Xóa nguyên liệu ${index + 1}`} onClick={() => ingredients.remove(index)}>
                      Xóa
                    </Button>
                  </div>
                </div>
              );
            })}
          </div>
        </section>
      )}

      {step === 2 && (
        <section>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">Các bước chế biến</h2>
            <Button type="button" variant="outline" onClick={() => steps.append(blankStep())}>
              Thêm bước
            </Button>
          </div>
          <div className="mt-4 space-y-3">
            {steps.fields.length === 0 && (
              <p className="text-sm text-ink-soft">Thêm ít nhất một bước để có thể xuất bản.</p>
            )}
            {steps.fields.map((field, index) => {
              const fieldErrors = errors.steps?.[index];
              return (
                <div key={field.id} className="grid gap-2 rounded border p-3">
                  <div className="flex items-center gap-2">
                    <strong className="flex-1">Bước {index + 1}</strong>
                    <Button type="button" size="sm" variant="outline" aria-label={`Đưa bước ${index + 1} lên`} disabled={index === 0} onClick={() => steps.move(index, index - 1)}>
                      ↑
                    </Button>
                    <Button type="button" size="sm" variant="outline" aria-label={`Đưa bước ${index + 1} xuống`} disabled={index === steps.fields.length - 1} onClick={() => steps.move(index, index + 1)}>
                      ↓
                    </Button>
                    <Button type="button" size="sm" variant="destructive" aria-label={`Xóa bước ${index + 1}`} onClick={() => steps.remove(index)}>
                      Xóa
                    </Button>
                  </div>
                  <Field label={`Tiêu đề bước ${index + 1}`} error={fieldErrors?.title?.message}>
                    <input className={INPUT} aria-invalid={Boolean(fieldErrors?.title)} {...form.register(`steps.${index}.title`)} />
                  </Field>
                  <Field label={`Mô tả bước ${index + 1}`} error={fieldErrors?.description?.message}>
                    <textarea className={INPUT} aria-invalid={Boolean(fieldErrors?.description)} {...form.register(`steps.${index}.description`)} />
                  </Field>
                  <Field label="Hẹn giờ (phút)" error={fieldErrors?.timerMinutes?.message}>
                    <input className={INPUT} type="number" {...form.register(`steps.${index}.timerMinutes`, { setValueAs: numberValue })} />
                  </Field>
                </div>
              );
            })}
          </div>
        </section>
      )}

      {step === 3 && (
        <section>
          {recipe ? (
            <RecipeImageGallery
              key={recipe.rowVersion}
              recipeId={recipe.id}
              recipeTitle={recipe.title}
              accessToken={accessToken}
              initialImages={imageResults(recipe)}
              onChanged={() => {
                getMyRecipe(recipe.id, accessToken).then(applyFresh).catch(() => undefined);
              }}
            />
          ) : (
            <p role="alert">Hãy hoàn tất ba bước đầu trước.</p>
          )}
        </section>
      )}

      {step === 4 && (
        <section className="space-y-4">
          <h2 className="text-lg font-semibold">Xem lại và xuất bản</h2>
          {blockers.length === 0 ? (
            <p className="text-sm text-ink-soft">Công thức đã đủ nguyên liệu, bước và mô tả ảnh để xuất bản.</p>
          ) : (
            <ul className="list-disc pl-5 text-sm text-red-700">
              {blockers.map((blocker) => (
                <li key={blocker}>{blocker}</li>
              ))}
            </ul>
          )}
          {recipe && (
            <RecipeLifecycleActions
              recipe={recipe}
              accessToken={accessToken}
              canPublish={blockers.length === 0}
              onChanged={applyFresh}
              onConflict={() => {
                reload().catch(() => undefined);
              }}
            />
          )}
        </section>
      )}

      <div className="flex justify-between border-t pt-4">
        <Button type="button" variant="outline" disabled={step === 0 || saving} onClick={() => setStep((current) => current - 1)}>
          Quay lại
        </Button>
        {step < STEPS.length - 1 ? (
          <Button type="submit" isLoading={saving}>
            {step !== 2 && 'Tiếp tục'}
            {step === 2 && (recipe ? 'Lưu và sang bước ảnh' : 'Tạo công thức')}
          </Button>
        ) : (
          <Button
            type="button"
            onClick={() => {
              form.reset(defaults(recipe));
              router.push('/dashboard/recipes');
            }}
          >
            Hoàn tất
          </Button>
        )}
      </div>

      <AlertDialog open={conflict}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Công thức đã được cập nhật ở nơi khác</AlertDialogTitle>
            <AlertDialogDescription>
              Thay đổi của bạn chưa được lưu. Tải lại sẽ bỏ phần bạn vừa nhập và lấy bản mới nhất.
            </AlertDialogDescription>
          </AlertDialogHeader>
          {differences && (
            <div className="max-h-48 overflow-auto text-sm text-ink">
              {differences.length === 0 ? (
                <p>Các trường chính không khác nhau.</p>
              ) : (
                <ul className="list-disc space-y-1 pl-5">
                  {differences.map((difference) => (
                    <li key={difference}>{difference}</li>
                  ))}
                </ul>
              )}
            </div>
          )}
          <AlertDialogFooter>
            <AlertDialogCancel
              onClick={(event) => {
                event.preventDefault();
                showDifferences().catch((error: unknown) => toast.error(getErrorMessage(error)));
              }}
            >
              Xem khác biệt
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={() => {
                reload().catch((error: unknown) => toast.error(getErrorMessage(error)));
              }}
            >
              Tải lại
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </form>
  );
}
