'use client';

import { useEffect, useId, useRef, useState, type FormEvent, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { Filter, X } from 'lucide-react';
import type { Category, RecipeDifficulty } from '@/types/api';
import { DIFFICULTY_LABEL } from '../format';
import {
  buildHref,
  buildQueryString,
  countActiveFilters,
  DIFFICULTIES,
  toOptionalInt,
  type RecipeQueryState,
} from '../search-params';

export type FilterField = 'category' | 'difficulty' | 'cookTime' | 'prepTime' | 'servings';

interface FilterPanelProps {
  pathname: string;
  /** Trạng thái URL hiện tại — từ khóa `q` và sắp xếp được giữ nguyên khi đổi bộ lọc. */
  state: Partial<RecipeQueryState>;
  categories: Category[];
  /** Bộ lọc hiển thị: /recipes dùng đủ 5, /search chỉ có danh mục + độ khó (SRS §8.3). */
  fields: readonly FilterField[];
}

interface Draft {
  categoryId: string;
  difficulty: string;
  maxCookTime: string;
  maxPrepTime: string;
  minServings: string;
}

const toDraft = (state: Partial<RecipeQueryState>): Draft => ({
  categoryId: state.categoryId ?? '',
  difficulty: state.difficulty ?? '',
  maxCookTime: state.maxCookTime?.toString() ?? '',
  maxPrepTime: state.maxPrepTime?.toString() ?? '',
  minServings: state.minServings?.toString() ?? '',
});

const inputClass =
  'mt-1 w-full rounded-lg border border-gray-300 bg-white px-2 py-1.5 text-sm text-gray-800 focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300';

/** Nhãn + điều khiển lồng nhau và nối bằng htmlFor/id (airbnb jsx-a11y yêu cầu cả hai). */
function Field({ label, children }: { label: string; children: (id: string) => ReactNode }) {
  const id = useId();
  return (
    <label htmlFor={id} className="block text-sm font-medium text-gray-700">
      {label}
      {children(id)}
    </label>
  );
}

function FilterForm({
  pathname,
  state,
  categories,
  fields,
  onApplied,
}: FilterPanelProps & { onApplied: () => void }) {
  const router = useRouter();
  const [draft, setDraft] = useState<Draft>(() => toDraft(state));
  const set = (key: keyof Draft) => (event: { target: { value: string } }) =>
    setDraft((current) => ({ ...current, [key]: event.target.value }));

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    // Chỉ ghi đè các bộ lọc đang hiển thị: ở /search không đụng tới maxCookTime… (API không nhận).
    const overrides: Partial<RecipeQueryState> = { page: 1 };
    if (fields.includes('category')) overrides.categoryId = draft.categoryId || undefined;
    if (fields.includes('difficulty')) {
      overrides.difficulty = (draft.difficulty || undefined) as RecipeDifficulty | undefined;
    }
    if (fields.includes('cookTime')) overrides.maxCookTime = toOptionalInt(draft.maxCookTime);
    if (fields.includes('prepTime')) overrides.maxPrepTime = toOptionalInt(draft.maxPrepTime);
    if (fields.includes('servings')) overrides.minServings = toOptionalInt(draft.minServings, 1);
    router.push(buildHref(pathname, state, overrides));
    onApplied();
  };

  const handleClear = () => {
    router.push(
      buildHref(pathname, state, {
        categoryId: undefined,
        difficulty: undefined,
        maxCookTime: undefined,
        maxPrepTime: undefined,
        minServings: undefined,
        page: 1,
      }),
    );
    onApplied();
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      {fields.includes('category') && (
        <Field label="Danh mục">
          {(id) => (
            <select
              id={id}
              value={draft.categoryId}
              onChange={set('categoryId')}
              className={inputClass}
            >
              <option value="">Tất cả danh mục</option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          )}
        </Field>
      )}
      {fields.includes('difficulty') && (
        <Field label="Độ khó">
          {(id) => (
            <select
              id={id}
              value={draft.difficulty}
              onChange={set('difficulty')}
              className={inputClass}
            >
              <option value="">Mọi độ khó</option>
              {DIFFICULTIES.map((difficulty) => (
                <option key={difficulty} value={difficulty}>
                  {DIFFICULTY_LABEL[difficulty]}
                </option>
              ))}
            </select>
          )}
        </Field>
      )}
      {fields.includes('cookTime') && (
        <Field label="Thời gian nấu tối đa (phút)">
          {(id) => (
            <input
              id={id}
              type="number"
              inputMode="numeric"
              min={0}
              value={draft.maxCookTime}
              onChange={set('maxCookTime')}
              className={inputClass}
            />
          )}
        </Field>
      )}
      {fields.includes('prepTime') && (
        <Field label="Thời gian chuẩn bị tối đa (phút)">
          {(id) => (
            <input
              id={id}
              type="number"
              inputMode="numeric"
              min={0}
              value={draft.maxPrepTime}
              onChange={set('maxPrepTime')}
              className={inputClass}
            />
          )}
        </Field>
      )}
      {fields.includes('servings') && (
        <Field label="Khẩu phần tối thiểu (người)">
          {(id) => (
            <input
              id={id}
              type="number"
              inputMode="numeric"
              min={1}
              value={draft.minServings}
              onChange={set('minServings')}
              className={inputClass}
            />
          )}
        </Field>
      )}
      <div className="flex gap-2">
        <button
          type="submit"
          className="flex-1 rounded-lg bg-orange-600 px-3 py-2 text-sm font-semibold text-white hover:bg-orange-700"
        >
          Áp dụng
        </button>
        <button
          type="button"
          onClick={handleClear}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-700 hover:border-orange-400"
        >
          Xóa lọc
        </button>
      </div>
    </form>
  );
}

/**
 * FR-SRCH-002: bảng bộ lọc đồng bộ với URL. Từ `lg` trở lên là cột bên trái; dưới `lg` thu thành nút "Bộ lọc" mở drawer
 * (Escape / bấm nền / nút đóng để đóng). Form được dựng lại (`key`) mỗi khi query string đổi nên back/forward luôn hiển thị
 * đúng giá trị đang áp dụng.
 */
export default function FilterPanel(props: FilterPanelProps) {
  const { state } = props;
  const [open, setOpen] = useState(false);
  const closeRef = useRef<HTMLButtonElement>(null);
  const active = countActiveFilters(state);

  useEffect(() => {
    if (!open) return undefined;
    closeRef.current?.focus();
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false);
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [open]);

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        aria-haspopup="dialog"
        className="flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:border-orange-400 lg:hidden"
      >
        <Filter aria-hidden className="h-4 w-4" />
        Bộ lọc
        {active > 0 && (
          <span className="rounded-full bg-orange-600 px-1.5 text-xs text-white">{active}</span>
        )}
      </button>

      {open && (
        <button
          type="button"
          aria-label="Đóng bộ lọc"
          tabIndex={-1}
          onClick={() => setOpen(false)}
          className="fixed inset-0 z-40 bg-black/40 lg:hidden"
        />
      )}

      <aside
        role={open ? 'dialog' : undefined}
        aria-modal={open ? true : undefined}
        aria-label="Bộ lọc công thức"
        className={`${
          open
            ? 'fixed inset-y-0 left-0 z-50 w-80 max-w-full overflow-y-auto p-5 shadow-xl'
            : 'hidden'
        } bg-white lg:static lg:z-auto lg:block lg:w-auto lg:overflow-visible lg:rounded-2xl lg:border lg:border-gray-200 lg:p-4 lg:shadow-none`}
      >
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-base font-semibold text-gray-900">Bộ lọc</h2>
          <button
            ref={closeRef}
            type="button"
            aria-label="Đóng bộ lọc"
            onClick={() => setOpen(false)}
            className="rounded-md p-1 text-gray-600 hover:bg-orange-50 lg:hidden"
          >
            <X aria-hidden className="h-5 w-5" />
          </button>
        </div>
        <FilterForm key={buildQueryString(state)} {...props} onApplied={() => setOpen(false)} />
      </aside>
    </>
  );
}
