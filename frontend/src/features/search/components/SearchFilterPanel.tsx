'use client';

import { lazy, Suspense, useId, useSyncExternalStore } from 'react';
import clsx from 'clsx';
import type { Category } from '@/types/category';
import type { RecipeQueryState } from '@/types/search';
import { DIFFICULTIES } from '@/types/search';
import { DIFFICULTY_LABEL } from '@/features/recipes/format';
import { buildHref } from '@/features/recipes/search-params';
import {
  ALL_CATEGORIES,
  COOK_TIME_OPTIONS,
  PREP_TIME_OPTIONS,
  SERVINGS_OPTIONS,
  formatServingsMin,
} from '../filters';
import { SEARCH_PATHNAME } from '../query-keys';

const CategorySelectMenu = lazy(() => import('./CategorySelectMenu'));

interface Option {
  label: string;
  /** Tham số ghi đè khi chọn — luôn kèm `page: 1` (đổi bộ lọc nào cũng về trang 1). */
  overrides: Partial<RecipeQueryState>;
  selected: boolean;
}

/**
 * Một nhóm lựa chọn = danh sách LINK THẬT tới URL đã áp bộ lọc. Không có JavaScript vẫn bấm được, crawler đi theo được; có
 * JavaScript thì `SearchResults` chặn cú bấm và đổi URL bằng `history.pushState` (không tải lại trang). Lựa chọn đang áp
 * dụng có `aria-current="true"`.
 */
function OptionGroup({
  title,
  options,
  state,
}: {
  title: string;
  options: readonly Option[];
  state: Partial<RecipeQueryState>;
}) {
  const titleId = useId();
  return (
    <div role="group" aria-labelledby={titleId}>
      <p id={titleId} className="text-sm font-medium text-gray-700">
        {title}
      </p>
      <ul className="mt-2 flex flex-wrap gap-2">
        {options.map((option) => (
          <li key={option.label}>
            <a
              href={buildHref(SEARCH_PATHNAME, state, { ...option.overrides, page: 1 })}
              aria-current={option.selected ? 'true' : undefined}
              className={clsx(
                'inline-flex h-8 items-center rounded-full border px-3 text-sm transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600',
                option.selected
                  ? 'border-orange-700 bg-orange-700 font-medium text-white'
                  : 'border-gray-300 bg-white text-gray-700 hover:border-orange-400 hover:bg-orange-50',
              )}
            >
              {option.label}
            </a>
          </li>
        ))}
      </ul>
    </div>
  );
}

const limitOptions = (
  key: 'maxCookTime' | 'maxPrepTime',
  values: readonly number[],
  current: number | undefined,
): Option[] => [
  { label: 'Bất kỳ', overrides: { [key]: undefined }, selected: current === undefined },
  ...values.map((value) => ({
    label: `≤ ${value} phút`,
    overrides: { [key]: value },
    selected: current === value,
  })),
];

const subscribeNothing = () => () => {};

/** `false` trong HTML do server render (và mãi `false` khi trình duyệt tắt JavaScript), `true` sau khi trang hydrate. */
function useHydrated(): boolean {
  return useSyncExternalStore(
    subscribeNothing,
    () => true,
    () => false,
  );
}

interface CategorySelectProps {
  labelId: string;
  triggerId: string;
  value: string;
  categories: readonly Category[];
  onChange: (value: string) => void;
}

/** `<select>` gốc của trình duyệt: gửi được trong form GET (`name`), dùng được bằng bàn phím và trình đọc màn hình sẵn có. */
function NativeCategorySelect({
  labelId,
  triggerId,
  value,
  categories,
  onChange,
}: CategorySelectProps) {
  return (
    <select
      id={triggerId}
      name="categoryId"
      aria-labelledby={labelId}
      value={value === ALL_CATEGORIES ? '' : value}
      onChange={(event) => onChange(event.target.value || ALL_CATEGORIES)}
      className="h-9 w-full rounded-lg border border-gray-300 bg-white px-2 text-sm text-gray-800"
    >
      <option value="">Tất cả danh mục</option>
      {categories.map((category) => (
        <option key={category.id} value={category.id}>
          {category.name}
        </option>
      ))}
    </select>
  );
}

/**
 * Chọn danh mục. Radix Select (shadcn) chỉ hoạt động khi có JavaScript — nút mở và danh sách đều do JS dựng, còn `<select>` ẩn
 * nó render kèm `name` chỉ để gửi giá trị trong form, người dùng không thao tác được. Vì vậy HTML do server render dùng `<select>`
 * gốc (cùng `name`, cùng nhãn), chọn xong bấm "Lọc theo danh mục" trong `<noscript>` → form GET: tắt JavaScript vẫn lọc được như
 * các nhóm lựa chọn khác (đều là link thật). Trang hydrate xong thì nạp muộn `Select` của shadcn; trong lúc nạp `<select>` gốc
 * vẫn chọn được.
 */
function CategorySelect(props: CategorySelectProps) {
  const hydrated = useHydrated();
  if (!hydrated) return <NativeCategorySelect {...props} />;
  return (
    <Suspense fallback={<NativeCategorySelect {...props} />}>
      <CategorySelectMenu {...props} />
    </Suspense>
  );
}

/**
 * FR-SRCH-002 — bảng bộ lọc đa tiêu chí của `/search`: danh mục (`Select`), độ khó (đủ 4 mức, có `Expert`), thời gian nấu tối
 * đa, thời gian chuẩn bị tối đa, khẩu phần tối thiểu (ba tiêu chí sau của CR-2026-05 / MT-66 — lọc ở cơ sở dữ liệu).
 * Không có nút "Áp dụng": chọn là áp dụng ngay, mỗi lần đúng một request JSON tới API.
 */
export default function SearchFilterPanel({
  state,
  categories,
  onNavigate,
}: {
  state: Partial<RecipeQueryState>;
  categories: readonly Category[];
  /** Đổi URL tại chỗ (history.pushState) — do `SearchResults` cung cấp. */
  onNavigate: (href: string) => void;
}) {
  const categoryLabelId = useId();
  const categoryTriggerId = useId();

  return (
    <div className="flex flex-col gap-5">
      {/* Form GET chỉ để đổi danh mục khi chưa có JavaScript (nút "Lọc" nằm trong <noscript>); có JS thì đổi là áp dụng. */}
      <form
        action={SEARCH_PATHNAME}
        method="get"
        onSubmit={(event) => event.preventDefault()}
        className="flex flex-col gap-2"
      >
        {state.q && <input type="hidden" name="q" value={state.q} />}
        {state.difficulty && <input type="hidden" name="difficulty" value={state.difficulty} />}
        {state.maxCookTime !== undefined && (
          <input type="hidden" name="maxCookTime" value={state.maxCookTime} />
        )}
        {state.maxPrepTime !== undefined && (
          <input type="hidden" name="maxPrepTime" value={state.maxPrepTime} />
        )}
        {state.minServings !== undefined && (
          <input type="hidden" name="minServings" value={state.minServings} />
        )}
        <span id={categoryLabelId} className="text-sm font-medium text-gray-700">
          Danh mục
        </span>
        <CategorySelect
          labelId={categoryLabelId}
          triggerId={categoryTriggerId}
          value={state.categoryId ?? ALL_CATEGORIES}
          categories={categories}
          onChange={(value) =>
            onNavigate(
              buildHref(SEARCH_PATHNAME, state, {
                categoryId: value === ALL_CATEGORIES ? undefined : value,
                page: 1,
              }),
            )
          }
        />
        <noscript>
          <button
            type="submit"
            className="mt-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm text-gray-700"
          >
            Lọc theo danh mục
          </button>
        </noscript>
      </form>

      <OptionGroup
        title="Độ khó"
        state={state}
        options={[
          {
            label: 'Mọi độ khó',
            overrides: { difficulty: undefined },
            selected: !state.difficulty,
          },
          ...DIFFICULTIES.map((difficulty) => ({
            label: DIFFICULTY_LABEL[difficulty],
            overrides: { difficulty },
            selected: state.difficulty === difficulty,
          })),
        ]}
      />
      <OptionGroup
        title="Thời gian nấu tối đa"
        state={state}
        options={limitOptions('maxCookTime', COOK_TIME_OPTIONS, state.maxCookTime)}
      />
      <OptionGroup
        title="Thời gian chuẩn bị tối đa"
        state={state}
        options={limitOptions('maxPrepTime', PREP_TIME_OPTIONS, state.maxPrepTime)}
      />
      <OptionGroup
        title="Khẩu phần tối thiểu"
        state={state}
        options={[
          {
            label: 'Bất kỳ',
            overrides: { minServings: undefined },
            selected: state.minServings === undefined,
          },
          ...SERVINGS_OPTIONS.map((value) => ({
            label: formatServingsMin(value),
            overrides: { minServings: value },
            selected: state.minServings === value,
          })),
        ]}
      />
    </div>
  );
}
