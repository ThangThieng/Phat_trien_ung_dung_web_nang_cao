'use client';

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ChevronDown } from 'lucide-react';
import { getCategories } from '@/features/categories/api';
import { CATEGORIES_QUERY_KEY } from '@/features/categories/admin-api';

/**
 * Điều hướng danh mục trên header (SRS §5.1 `/categories`, `/categories/[slug]`): menu thả xuống liệt kê danh mục theo
 * `orderIndex` rồi `name` (đúng thứ tự backend trả về ở FR-CAT-001) kèm số công thức đã xuất bản.
 * Dùng chung query key `['categories']` với trang quản trị nên Admin tạo/sửa/xóa danh mục xong thì menu tự làm mới.
 * Lỗi tải danh sách chỉ làm menu chỉ còn link "Tất cả danh mục", không làm hỏng header.
 */
export default function CategoryNav() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const { data: categories } = useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: () => getCategories(),
  });

  // Nhấn Escape hoặc bấm ra ngoài thì đóng menu.
  useEffect(() => {
    if (!open) return undefined;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false);
    };
    const handlePointerDown = (event: MouseEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) setOpen(false);
    };

    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('mousedown', handlePointerDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.removeEventListener('mousedown', handlePointerDown);
    };
  }, [open]);

  const active = pathname.startsWith('/categories');

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-haspopup="true"
        aria-expanded={open}
        aria-controls="category-nav-menu"
        onClick={() => setOpen((value) => !value)}
        className={`flex items-center gap-1 rounded-md px-2 py-1 text-sm font-medium ${
          active ? 'text-orange-700' : 'text-gray-700 hover:text-orange-700'
        }`}
      >
        Danh mục
        <ChevronDown aria-hidden className={`h-4 w-4 transition ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <ul
          id="category-nav-menu"
          aria-label="Danh mục công thức"
          className="absolute left-0 top-full z-50 mt-1 max-h-80 w-64 overflow-y-auto rounded-lg border border-orange-100 bg-white py-1 shadow-lg"
        >
          <li>
            <Link
              href="/categories"
              onClick={() => setOpen(false)}
              className="block px-4 py-2 text-sm font-semibold text-orange-700 hover:bg-orange-50"
            >
              Tất cả danh mục
            </Link>
          </li>
          {categories?.map((category) => (
            <li key={category.id}>
              <Link
                href={`/categories/${category.slug}`}
                onClick={() => setOpen(false)}
                className="flex items-center justify-between gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-orange-50 hover:text-orange-700"
              >
                <span className="truncate">{category.name}</span>
                <span className="shrink-0 text-xs text-gray-400">{category.recipeCount}</span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
