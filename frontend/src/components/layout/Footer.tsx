import Link from 'next/link';
import { ChefHat } from 'lucide-react';
import type { Category } from '@/types/category';

/** Số danh mục hiện ở footer — danh sách đầy đủ ở `/categories`. */
const FOOTER_CATEGORY_LIMIT = 8;

const LINK = 'text-sm text-gray-700 hover:text-orange-700 hover:underline';

/**
 * Footer công khai (Server Component, landmark `<footer>` — NFR-USE-002): giới thiệu ngắn, liên kết khám phá, danh mục (theo
 * `orderIndex` như API trả về), bản quyền. Không có JavaScript.
 */
export default function Footer({ categories }: { categories: Category[] }) {
  const year = new Date().getFullYear();
  return (
    <footer className="mt-16 border-t border-orange-100 bg-white">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 sm:grid-cols-2 lg:grid-cols-[2fr_1fr_2fr]">
        <div>
          <p className="flex items-center gap-2 text-lg font-bold text-orange-700">
            <ChefHat aria-hidden className="h-5 w-5" />
            Culinary Blog
          </p>
          <p className="mt-2 max-w-xs text-sm text-gray-600">
            Chia sẻ và khám phá công thức nấu ăn Việt Nam và thế giới — đủ nguyên liệu, định lượng
            và từng bước.
          </p>
        </div>

        <nav aria-label="Khám phá">
          <h2 className="text-sm font-semibold text-gray-900">Khám phá</h2>
          <ul className="mt-3 space-y-2">
            <li>
              <Link href="/recipes" className={LINK}>
                Tất cả công thức
              </Link>
            </li>
            <li>
              <Link href="/categories" className={LINK}>
                Danh mục
              </Link>
            </li>
            <li>
              <Link href="/search" className={LINK}>
                Tìm kiếm
              </Link>
            </li>
          </ul>
        </nav>

        {categories.length > 0 && (
          <nav aria-label="Danh mục nổi bật">
            <h2 className="text-sm font-semibold text-gray-900">Danh mục</h2>
            <ul className="mt-3 grid grid-cols-2 gap-x-4 gap-y-2">
              {categories.slice(0, FOOTER_CATEGORY_LIMIT).map((category) => (
                <li key={category.id} className="truncate">
                  <Link href={`/categories/${category.slug}`} className={LINK}>
                    {category.name}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        )}
      </div>
      <div className="border-t border-orange-50 py-4 text-center text-sm text-gray-600">
        © {year} Culinary Blog – Nhóm 20
      </div>
    </footer>
  );
}
