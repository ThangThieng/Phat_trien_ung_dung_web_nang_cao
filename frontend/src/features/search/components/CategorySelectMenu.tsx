'use client';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { Category } from '@/types/category';
import { ALL_CATEGORIES } from '../filters';

/**
 * Phần Radix Select (shadcn) của bộ lọc danh mục — tách file để `SearchFilterPanel` nạp muộn bằng `React.lazy` sau khi trang
 * hydrate: Radix Select kéo theo bộ định vị popper, nạp ngay từ đầu làm First Load JS của `/search` vượt ngân sách 200 kB
 * (NFR-PERF-005; đo: 207 kB).
 */
export default function CategorySelectMenu({
  labelId,
  triggerId,
  value,
  categories,
  onChange,
}: {
  labelId: string;
  triggerId: string;
  value: string;
  categories: readonly Category[];
  onChange: (value: string) => void;
}) {
  return (
    <Select value={value} onValueChange={onChange}>
      <SelectTrigger id={triggerId} aria-labelledby={labelId} className="w-full">
        <SelectValue placeholder="Tất cả danh mục" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={ALL_CATEGORIES}>Tất cả danh mục</SelectItem>
        {categories.map((category) => (
          <SelectItem key={category.id} value={category.id}>
            {category.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
