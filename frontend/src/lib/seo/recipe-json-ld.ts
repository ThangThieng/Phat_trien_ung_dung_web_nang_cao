import type { Nutrition, Recipe, RecipeIngredient } from '@/types/recipe';
import { SITE_NAME, SITE_URL, absoluteUrl } from './site';

type JsonLdObject = Record<string, unknown>;

/** Phút → ISO 8601 duration (`PT15M`, `PT1H30M`). 0 hoặc âm → `undefined` (bỏ thuộc tính). */
export function toIsoDuration(minutes: number | null | undefined): string | undefined {
  if (!minutes || minutes <= 0) return undefined;
  const hours = Math.floor(minutes / 60);
  const rest = Math.round(minutes % 60);
  return `PT${hours ? `${hours}H` : ''}${rest ? `${rest}M` : ''}`;
}

/** NFR-SEO-001: ưu tiên `quantityText` (nguyên văn người dùng nhập), không có thì ghép `quantity + unit + name`. */
export function ingredientToText(ingredient: RecipeIngredient): string {
  const { quantityText, quantity, unit, name } = ingredient;
  if (quantityText) return `${quantityText} ${name}`.trim();
  return [quantity === null ? null : String(quantity), unit, name].filter(Boolean).join(' ');
}

function buildNutrition(nutrition: Nutrition | null): JsonLdObject | undefined {
  if (!nutrition) return undefined;
  const entries: [string, number | null, string][] = [
    ['calories', nutrition.calories, 'calories'],
    ['proteinContent', nutrition.protein, 'g'],
    ['carbohydrateContent', nutrition.carbohydrates, 'g'],
    ['fatContent', nutrition.fat, 'g'],
    ['fiberContent', nutrition.fiber, 'g'],
    ['sodiumContent', nutrition.sodium, 'mg'],
  ];
  const present = entries.filter(([, value]) => value !== null);
  if (present.length === 0) return undefined;
  return {
    '@type': 'NutritionInformation',
    ...Object.fromEntries(present.map(([key, value, unit]) => [key, `${value} ${unit}`])),
  };
}

/**
 * JSON-LD Schema.org `Recipe` (NFR-SEO-001).
 * KHÔNG có `aggregateRating`: không có hệ thống đánh giá (MT-24) — dữ liệu giả vi phạm chính sách structured data của Google.
 */
export function buildRecipeJsonLd(recipe: Recipe, siteUrl: string = SITE_URL): JsonLdObject {
  const pageUrl = `${siteUrl}/recipes/${recipe.slug}`;
  const images = recipe.images
    .map((image) => image.mediumUrl ?? image.originalUrl)
    .filter(Boolean)
    .map(absoluteUrl);

  return {
    '@context': 'https://schema.org',
    '@type': 'Recipe',
    name: recipe.title,
    description: recipe.description,
    ...(images.length > 0 && { image: images }),
    author: { '@type': 'Person', name: recipe.author.displayName },
    ...(recipe.publishedAt && { datePublished: recipe.publishedAt }),
    prepTime: toIsoDuration(recipe.prepTimeMinutes),
    cookTime: toIsoDuration(recipe.cookTimeMinutes),
    totalTime: toIsoDuration(recipe.totalTimeMinutes),
    recipeYield: `${recipe.servings} khẩu phần`,
    recipeCategory: recipe.category.name,
    recipeIngredient: recipe.ingredients.map(ingredientToText),
    recipeInstructions: [...recipe.steps]
      .sort((a, b) => a.stepNumber - b.stepNumber)
      .map((step) => ({
        '@type': 'HowToStep',
        name: step.title,
        text: step.description,
        ...(step.imageUrl && { image: absoluteUrl(step.imageUrl) }),
        url: `${pageUrl}#buoc-${step.stepNumber}`,
      })),
    nutrition: buildNutrition(recipe.nutrition),
    mainEntityOfPage: pageUrl,
  };
}

export interface BreadcrumbItem {
  name: string;
  /** Đường dẫn tương đối hoặc URL tuyệt đối. */
  path: string;
}

/** `BreadcrumbList` cho trang công thức và trang danh mục. */
export function buildBreadcrumbJsonLd(items: readonly BreadcrumbItem[]): JsonLdObject {
  return {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items.map((item, index) => ({
      '@type': 'ListItem',
      position: index + 1,
      name: item.name,
      item: absoluteUrl(item.path),
    })),
  };
}

/** `WebSite` + `SearchAction` cho trang chủ (ô tìm kiếm trên kết quả Google). */
export function buildWebSiteJsonLd(siteUrl: string = SITE_URL): JsonLdObject {
  return {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: SITE_NAME,
    url: siteUrl,
    potentialAction: {
      '@type': 'SearchAction',
      target: { '@type': 'EntryPoint', urlTemplate: `${siteUrl}/search?q={search_term_string}` },
      'query-input': 'required name=search_term_string',
    },
  };
}

/**
 * `JSON.stringify` không thoát `</script>`. Dữ liệu (tiêu đề, mô tả) do người dùng nhập và được nhúng nguyên vào HTML,
 * nên không thoát `<` thì một công thức đặt tên `</script><script>…` chèn được script vào trang (XSS qua dữ liệu).
 */
export function serializeJsonLd(data: unknown): string {
  return JSON.stringify(data).replace(/</g, '\\u003c');
}
