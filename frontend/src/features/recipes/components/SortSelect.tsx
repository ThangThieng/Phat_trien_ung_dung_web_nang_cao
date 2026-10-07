'use client';

import { useId } from 'react';
import { useRouter } from 'next/navigation';
import {
  buildHref,
  DEFAULT_SORT_BY,
  DEFAULT_SORT_ORDER,
  SORT_FIELDS,
  SORT_ORDERS,
  type RecipeQueryState,
  type SortField,
  type SortOrder,
} from '../search-params';

const FIELD_LABEL: Record<SortField, string> = {
  createdAt: 'Ngày tạo',
  publishedAt: 'Ngày xuất bản',
  title: 'Tên món',
  cookTime: 'Thời gian nấu',
  prepTime: 'Thời gian chuẩn bị',
};

const ORDER_LABEL: Record<SortOrder, string> = { asc: 'Tăng dần', desc: 'Giảm dần' };

interface SortSelectProps {
  pathname: string;
  /** Trạng thái URL hiện tại — được giữ nguyên (lọc, từ khóa) khi đổi sắp xếp. */
  state: Partial<RecipeQueryState>;
}

/**
 * FR-SRCH-003: hai dropdown "Cột" và "Chiều" ánh xạ 1-1 với `sortBy` / `sortOrder` — không cần lớp chuyển đổi nào
 * (đó là lý do chọn hai tham số thay cho `sort=-field`). Đổi sắp xếp thì về trang 1.
 */
export default function SortSelect({ pathname, state }: SortSelectProps) {
  const router = useRouter();
  const fieldId = useId();
  const sortBy = state.sortBy ?? DEFAULT_SORT_BY;
  const sortOrder = state.sortOrder ?? DEFAULT_SORT_ORDER;
  const selectClass =
    'rounded-lg border border-gray-300 bg-white px-2 py-1.5 text-sm text-gray-800 focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300';

  return (
    <div className="flex flex-wrap items-center gap-2" role="group" aria-label="Sắp xếp">
      <label htmlFor={fieldId} className="flex items-center gap-2 text-sm text-gray-700">
        <span>Sắp xếp theo</span>
        <select
          id={fieldId}
          value={sortBy}
          onChange={(event) =>
            router.push(
              buildHref(pathname, state, { sortBy: event.target.value as SortField, page: 1 }),
            )
          }
          className={selectClass}
        >
          {SORT_FIELDS.map((field) => (
            <option key={field} value={field}>
              {FIELD_LABEL[field]}
            </option>
          ))}
        </select>
      </label>
      <select
        value={sortOrder}
        aria-label="Thứ tự sắp xếp"
        onChange={(event) =>
          router.push(
            buildHref(pathname, state, { sortOrder: event.target.value as SortOrder, page: 1 }),
          )
        }
        className={selectClass}
      >
        {SORT_ORDERS.map((order) => (
          <option key={order} value={order}>
            {ORDER_LABEL[order]}
          </option>
        ))}
      </select>
    </div>
  );
}
