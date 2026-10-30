import type { Metadata } from 'next';
import type { Category } from '@/types/category';
import type { Recipe } from '@/types/recipe';
import { SITE_NAME } from './site';

export const TITLE_MAX = 60;
export const DESCRIPTION_MAX = 160;
const TITLE_SUFFIX = ` | ${SITE_NAME}`;

/**
 * Cắt theo ranh giới từ, thêm `…`; kết quả không dài quá `max` ký tự. Giới hạn SEO là quy tắc RENDER chứ không phải ràng
 * buộc dữ liệu (MT-37): người dùng không bị ép đặt tên ngắn để chiều Google.
 */
export function truncateAtWord(text: string, max: number): string {
  const clean = text.replace(/\s+/g, ' ').trim();
  if (clean.length <= max) return clean;
  const head = clean.slice(0, max - 1);
  const lastSpace = head.lastIndexOf(' ');
  const base = lastSpace > 0 ? head.slice(0, lastSpace) : head;
  return `${base.replace(/[\s,.;:–-]+$/, '')}…`;
}

/** `<title>` = "{Tên} | Culinary Blog", tổng ≤ 60 ký tự: phần tên bị cắt, hậu tố thương hiệu được giữ. */
export function buildPageTitle(name: string): string {
  return `${truncateAtWord(name, TITLE_MAX - TITLE_SUFFIX.length)}${TITLE_SUFFIX}`;
}

export function buildDescription(text: string | null | undefined, fallback: string): string {
  return truncateAtWord(text?.trim() ? text : fallback, DESCRIPTION_MAX);
}

/** Ảnh OG 1200×630 do `recipes/[slug]/opengraph-image.tsx` cung cấp (Next tự gắn `og:image`). */
export function buildRecipeMetadata(recipe: Recipe): Metadata {
  const title = buildPageTitle(recipe.title);
  const description = buildDescription(recipe.description, `Công thức ${recipe.title} trên ${SITE_NAME}.`);
  const url = `/recipes/${recipe.slug}`;
  return {
    // `absolute`: root layout có `template: '%s | Culinary Blog'`, không dùng thì hậu tố bị lặp hai lần.
    title: { absolute: title },
    description,
    alternates: { canonical: url },
    openGraph: {
      type: 'article',
      title,
      description,
      url,
      siteName: SITE_NAME,
      locale: 'vi_VN',
      ...(recipe.publishedAt && { publishedTime: recipe.publishedAt }),
      authors: [recipe.author.displayName],
    },
    twitter: { card: 'summary_large_image', title, description },
  };
}

export function buildCategoryMetadata(category: Pick<Category, 'name' | 'slug' | 'description'>): Metadata {
  const title = buildPageTitle(category.name);
  const description = buildDescription(category.description, `Công thức thuộc danh mục ${category.name}.`);
  const url = `/categories/${category.slug}`;
  return {
    title: { absolute: title },
    description,
    alternates: { canonical: url },
    openGraph: { type: 'website', title, description, url, siteName: SITE_NAME, locale: 'vi_VN' },
    twitter: { card: 'summary_large_image', title, description },
  };
}

/** `/search`: trang kết quả (có `q`) không nên vào chỉ mục — NFR-SEO-002. Trang trống thì vẫn index được. */
export function buildSearchMetadata(q: string | undefined): Metadata {
  return {
    title: q ? `Tìm kiếm: ${q}` : 'Tìm kiếm công thức',
    description: 'Tìm công thức nấu ăn theo tên món, nguyên liệu, danh mục, độ khó và thời gian nấu.',
    ...(q ? { robots: { index: false, follow: true } } : { alternates: { canonical: '/search' } }),
  };
}
