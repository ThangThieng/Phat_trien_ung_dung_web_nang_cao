import Link from 'next/link';
import Image from 'next/image';
import type { Category } from '@/types/category';

/** Số danh mục hiện ở footer — danh sách đầy đủ ở `/categories`. */
const FOOTER_CATEGORY_LIMIT = 8;

const LINK = 'text-sm text-cream hover:text-gold hover:underline';

/**
 * Footer công khai (Server Component, landmark `<footer>` — NFR-USE-002): giới thiệu ngắn, liên kết khám phá, danh mục (theo
 * `orderIndex` như API trả về), bản quyền. Không có JavaScript.
 */
export default function Footer({ categories }: { categories: Category[] }) {
  const year = new Date().getFullYear();
  return (
    <footer className="rule-gold mt-16 border-t-[3px] bg-navy text-cream">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-10 sm:grid-cols-2 lg:grid-cols-[2fr_1fr_2fr]">
        <div>
          <Image src="/brand/logo-gold-256.webp" alt="Culinary Blog" width={92} height={92} unoptimized />
          <p className="mt-2 max-w-xs text-sm text-cream">
            Chia sẻ và khám phá công thức nấu ăn Việt Nam và thế giới — đủ nguyên liệu, định lượng
            và từng bước.
          </p>
        </div>

        <nav aria-label="Khám phá">
          <h2 className="text-sm font-semibold text-gold">Khám phá</h2>
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
            <h2 className="text-sm font-semibold text-gold">Danh mục</h2>
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
      <div className="border-t border-navy-line py-4 text-center text-sm text-cream">
        © {year} Culinary Blog – Nhóm 20
        <p className="mt-1 text-xs">
          Một phần công thức lấy từ{' '}
          <a
            href="https://en.wikibooks.org/wiki/Cookbook:Table_of_Contents"
            target="_blank"
            rel="noopener noreferrer"
            className="underline hover:text-gold"
          >
            Wikibooks Cookbook
          </a>{' '}
          theo giấy phép{' '}
          <a
            href="https://creativecommons.org/licenses/by-sa/4.0/"
            target="_blank"
            rel="noopener noreferrer"
            className="underline hover:text-gold"
          >
            CC BY-SA 4.0
          </a>
          .
        </p>
      </div>
    </footer>
  );
}
