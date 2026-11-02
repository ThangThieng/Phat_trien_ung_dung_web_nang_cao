import { X } from 'lucide-react';
import type { Category } from '@/types/category';
import type { RecipeQueryState } from '@/types/search';
import { buildHref } from '@/features/recipes/search-params';
import { CLEAR_ALL_FILTERS, getActiveFilters } from '../filters';
import { SEARCH_PATHNAME } from '../query-keys';

/**
 * Nhãn của các bộ lọc đang bật, mỗi nhãn là link gỡ đúng bộ lọc đó (về trang 1, giữ phần còn lại), kèm "Xóa bộ lọc".
 * Link thật nên chạy cả khi chưa có JavaScript; có JavaScript thì `SearchResults` đổi URL tại chỗ.
 */
export default function ActiveFilterChips({
  state,
  categories,
}: {
  state: Partial<RecipeQueryState>;
  categories: readonly Category[];
}) {
  const active = getActiveFilters(state, categories);
  if (active.length === 0) return null;

  return (
    <ul aria-label="Bộ lọc đang áp dụng" className="mt-4 flex flex-wrap items-center gap-2">
      {active.map(({ key, label }) => (
        <li key={key}>
          <a
            href={buildHref(SEARCH_PATHNAME, state, { [key]: undefined, page: 1 })}
            aria-label={`Gỡ bộ lọc ${label}`}
            className="inline-flex h-8 items-center gap-1.5 rounded-full border border-orange-200 bg-orange-50 pr-2 pl-3 text-sm text-orange-900 hover:border-orange-400 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600"
          >
            {label}
            <X aria-hidden className="h-3.5 w-3.5" />
          </a>
        </li>
      ))}
      <li>
        <a
          href={buildHref(SEARCH_PATHNAME, state, CLEAR_ALL_FILTERS)}
          className="text-sm font-medium text-orange-700 underline-offset-2 hover:underline"
        >
          Xóa bộ lọc
        </a>
      </li>
    </ul>
  );
}
