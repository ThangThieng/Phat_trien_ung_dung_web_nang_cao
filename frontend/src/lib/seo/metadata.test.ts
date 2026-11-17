import { mockRecipes } from '@/mocks/fixtures/recipes';
import type { Recipe } from '@/types/recipe';
import {
  DESCRIPTION_MAX,
  TITLE_MAX,
  DEFAULT_OG_IMAGE,
  buildCategoryMetadata,
  buildPageMetadata,
  buildPageTitle,
  buildRecipeMetadata,
  buildSearchMetadata,
  truncateAtWord,
} from './metadata';

const recipe = mockRecipes[0] as unknown as Recipe;

describe('truncateAtWord (MT-37: quy tắc render, không phải ràng buộc dữ liệu)', () => {
  it('giữ nguyên chuỗi ngắn', () => expect(truncateAtWord('Phở bò', 60)).toBe('Phở bò'));

  it('cắt ở ranh giới từ, thêm … và không vượt max', () => {
    const out = truncateAtWord('Bún bò Huế chuẩn vị cố đô nấu bằng nồi áp suất', 30);
    expect(out.length).toBeLessThanOrEqual(30);
    expect(out.endsWith('…')).toBe(true);
    expect(out).toBe('Bún bò Huế chuẩn vị cố đô…');
  });

  it('chuỗi không có khoảng trắng vẫn bị cắt', () => {
    expect(truncateAtWord('a'.repeat(100), 20)).toHaveLength(20);
  });
});

describe('buildPageTitle (NFR-SEO-002)', () => {
  it('"{Tên} | Culinary Blog" và tổng ≤ 60 ký tự', () => {
    expect(buildPageTitle('Phở bò')).toBe('Phở bò | Culinary Blog');
    const long = buildPageTitle('Bún bò Huế chuẩn vị cố đô nấu bằng nồi áp suất cho cả gia đình');
    expect(long.length).toBeLessThanOrEqual(TITLE_MAX);
    expect(long.endsWith(' | Culinary Blog')).toBe(true);
    expect(long).toContain('…');
  });
});

describe('buildRecipeMetadata', () => {
  const meta = buildRecipeMetadata(recipe);

  it('dùng title.absolute để template của root layout không lặp hậu tố', () => {
    expect(meta.title).toEqual({ absolute: expect.stringMatching(/ \| Culinary Blog$/) });
  });

  it('description ≤ 160 ký tự, canonical theo slug', () => {
    expect((meta.description ?? '').length).toBeLessThanOrEqual(DESCRIPTION_MAX);
    expect(meta.alternates?.canonical).toBe(`/recipes/${recipe.slug}`);
  });

  it('Open Graph article + Twitter summary_large_image', () => {
    expect(meta.openGraph).toMatchObject({ type: 'article', publishedTime: recipe.publishedAt });
    expect(meta.twitter).toMatchObject({ card: 'summary_large_image' });
  });
});

describe('buildCategoryMetadata / buildSearchMetadata', () => {
  it('danh mục có canonical', () => {
    const meta = buildCategoryMetadata({ name: 'Món chính', slug: 'mon-chinh', description: null });
    expect(meta.alternates?.canonical).toBe('/categories/mon-chinh');
    expect(meta.description).toContain('Món chính');
  });

  it('/search có q → noindex; không có q → index được', () => {
    expect(buildSearchMetadata('pho').robots).toEqual({ index: false, follow: true });
    expect(buildSearchMetadata(undefined).robots).toBeUndefined();
  });
});

describe('buildPageMetadata — NFR-SEO-002 cho mọi trang không phải công thức', () => {
  it('đủ canonical, Open Graph (có ảnh 1200×630), Twitter summary_large_image', () => {
    const meta = buildPageMetadata({
      name: 'Danh mục công thức',
      description: 'Mô tả',
      path: '/categories',
    });
    expect(meta.title).toEqual({ absolute: 'Danh mục công thức | Culinary Blog' });
    expect(meta.alternates?.canonical).toBe('/categories');
    expect(meta.openGraph).toMatchObject({
      type: 'website',
      url: '/categories',
      images: [DEFAULT_OG_IMAGE],
    });
    expect(DEFAULT_OG_IMAGE).toMatchObject({ width: 1200, height: 630 });
    expect(meta.twitter).toMatchObject({ card: 'summary_large_image' });
  });

  it('brand: false giữ nguyên tiêu đề đã có thương hiệu (trang chủ), vẫn ≤ 60 ký tự', () => {
    const meta = buildPageMetadata({
      name: 'Culinary Blog – Công thức nấu ăn từ bếp nhà Việt',
      description: 'x',
      path: '/',
      brand: false,
    });
    expect((meta.title as { absolute: string }).absolute).toBe(
      'Culinary Blog – Công thức nấu ăn từ bếp nhà Việt',
    );
    expect((meta.title as { absolute: string }).absolute.length).toBeLessThanOrEqual(60);
  });

  it('trang danh mục và trang tìm kiếm cũng có og:image', () => {
    expect(
      buildCategoryMetadata({ name: 'Món chính', slug: 'mon-chinh', description: null }).openGraph,
    ).toMatchObject({
      images: [DEFAULT_OG_IMAGE],
    });
    expect(buildSearchMetadata(undefined).openGraph).toMatchObject({ images: [DEFAULT_OG_IMAGE] });
  });
});
