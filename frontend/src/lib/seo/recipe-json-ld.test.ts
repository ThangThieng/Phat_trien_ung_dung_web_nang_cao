import { mockRecipes } from '@/mocks/fixtures/recipes';
import type { Recipe } from '@/types/recipe';
import {
  buildBreadcrumbJsonLd,
  buildRecipeJsonLd,
  buildWebSiteJsonLd,
  ingredientToText,
  serializeJsonLd,
  toIsoDuration,
} from './recipe-json-ld';

const recipe = mockRecipes[0] as unknown as Recipe;
const SITE = 'https://culinary.test';

describe('toIsoDuration (ISO 8601)', () => {
  it.each([
    [15, 'PT15M'],
    [60, 'PT1H'],
    [90, 'PT1H30M'],
    [125, 'PT2H5M'],
  ])('%i phút → %s', (minutes, expected) => expect(toIsoDuration(minutes)).toBe(expected));

  it('0, âm hoặc rỗng → bỏ thuộc tính', () => {
    expect(toIsoDuration(0)).toBeUndefined();
    expect(toIsoDuration(-5)).toBeUndefined();
    expect(toIsoDuration(null)).toBeUndefined();
  });
});

describe('ingredientToText (NFR-SEO-001)', () => {
  const base = { id: 'i', notes: null, orderIndex: 0 };
  it('ưu tiên quantityText nguyên văn', () => {
    expect(ingredientToText({ ...base, name: 'muối', quantity: null, unit: null, quantityText: 'vừa đủ' })).toBe('vừa đủ muối');
  });
  it('không có quantityText thì ghép quantity + unit + name', () => {
    expect(ingredientToText({ ...base, name: 'đường', quantity: 2, unit: 'muỗng', quantityText: null })).toBe('2 muỗng đường');
  });
  it('thiếu số lượng thì chỉ còn tên', () => {
    expect(ingredientToText({ ...base, name: 'hành', quantity: null, unit: null, quantityText: null })).toBe('hành');
  });
});

describe('buildRecipeJsonLd', () => {
  const ld = buildRecipeJsonLd(recipe, SITE) as Record<string, unknown>;

  it('có các thuộc tính bắt buộc của NFR-SEO-001', () => {
    expect(ld['@context']).toBe('https://schema.org');
    expect(ld['@type']).toBe('Recipe');
    expect(ld.name).toBe(recipe.title);
    expect(ld.author).toEqual({ '@type': 'Person', name: recipe.author.displayName });
    expect(ld.datePublished).toBe(recipe.publishedAt);
    expect(ld.prepTime).toMatch(/^PT/);
    expect(ld.cookTime).toMatch(/^PT/);
    expect(ld.totalTime).toMatch(/^PT/);
    expect(ld.recipeCategory).toBe(recipe.category.name);
    expect(ld.recipeIngredient).toHaveLength(recipe.ingredients.length);
  });

  it('KHÔNG nhúng aggregateRating (MT-24)', () => {
    expect(ld).not.toHaveProperty('aggregateRating');
    expect(JSON.stringify(ld)).not.toContain('aggregateRating');
  });

  it('mỗi bước là HowToStep có url #buoc-{n} tuyệt đối theo thứ tự', () => {
    const steps = ld.recipeInstructions as { '@type': string; url: string }[];
    expect(steps).toHaveLength(recipe.steps.length);
    steps.forEach((step, index) => {
      expect(step['@type']).toBe('HowToStep');
      expect(step.url).toBe(`${SITE}/recipes/${recipe.slug}#buoc-${index + 1}`);
    });
  });

  it('ảnh luôn là URL tuyệt đối', () => {
    const withImage: Recipe = {
      ...recipe,
      images: [
        { id: '1', originalUrl: '/media/a.jpg', mediumUrl: '/media/a-medium.jpg', thumbnailUrl: null, altText: null, isPrimary: true, orderIndex: 0 },
      ],
    };
    const images = (buildRecipeJsonLd(withImage, SITE) as { image: string[] }).image;
    expect(images.every((url) => /^https?:\/\//.test(url))).toBe(true);
    expect(images[0]).toContain('/media/a-medium.jpg');
  });

  it('nutrition có @type NutritionInformation khi có dữ liệu, bỏ khi null', () => {
    const nutrition = ld.nutrition as { '@type': string; calories?: string } | undefined;
    if (recipe.nutrition?.calories != null) {
      expect(nutrition?.['@type']).toBe('NutritionInformation');
      expect(nutrition?.calories).toBe(`${recipe.nutrition.calories} calories`);
    }
    const none = buildRecipeJsonLd({ ...recipe, nutrition: null }, SITE) as Record<string, unknown>;
    expect(none.nutrition).toBeUndefined();
  });
});

describe('serializeJsonLd — chống XSS qua dữ liệu người dùng', () => {
  it('thoát < để </script> không đóng thẻ script', () => {
    const hostile = { ...recipe, title: '</script><script>alert(1)</script>' };
    const html = serializeJsonLd(buildRecipeJsonLd(hostile, SITE));
    expect(html).not.toContain('<');
    expect(html).toContain('\\u003c/script>');
    expect(JSON.parse(html).name).toBe(hostile.title); // vẫn parse ngược đúng
  });
});

describe('BreadcrumbList và WebSite', () => {
  it('breadcrumb đánh position từ 1, item là URL tuyệt đối', () => {
    const ld = buildBreadcrumbJsonLd([
      { name: 'Trang chủ', path: '/' },
      { name: 'Món chính', path: '/categories/mon-chinh' },
    ]) as { itemListElement: { position: number; item: string }[] };
    expect(ld.itemListElement.map((i) => i.position)).toEqual([1, 2]);
    expect(ld.itemListElement.every((i) => /^https?:\/\//.test(i.item))).toBe(true);
  });

  it('WebSite có SearchAction trỏ /search?q=', () => {
    const ld = buildWebSiteJsonLd(SITE) as { potentialAction: { target: { urlTemplate: string } } };
    expect(ld.potentialAction.target.urlTemplate).toBe(`${SITE}/search?q={search_term_string}`);
  });
});
