import { backendConst, backendDtoKeys, backendSortColumns } from '../backend-contract.testutil';
import type { RecipeListParams, RecipeSearchHit, RecipeSearchParams } from '../search';
import type { RecipeSummary as Summary } from '../recipe';
import {
  DEFAULT_PAGE_SIZE,
  DEFAULT_SORT_BY,
  DEFAULT_SORT_ORDER,
  DIFFICULTIES,
  MAX_PAGE_SIZE,
  MIN_SEARCH_LENGTH,
  SORT_FIELDS,
  SORT_ORDERS,
} from '../search';

/**
 * Hợp đồng `types/search.ts` ↔ Backend (SRS §8.1, §8.3). Cùng kỹ thuật với category-contract.test.ts:
 * `satisfies Record<keyof T, true>` + so khóa/hằng số với Backend.
 */
const summaryKeys = {
  id: true,
  title: true,
  slug: true,
  description: true,
  prepTimeMinutes: true,
  cookTimeMinutes: true,
  totalTimeMinutes: true,
  servings: true,
  difficulty: true,
  status: true,
  primaryImageUrl: true,
  category: true,
  author: true,
  publishedAt: true,
  createdAt: true,
} satisfies Record<keyof Summary, true>;

const hitKeys = { ...summaryKeys, relevanceScore: true } satisfies Record<
  keyof RecipeSearchHit,
  true
>;

const listParamsKeys = {
  page: true,
  pageSize: true,
  categoryId: true,
  difficulty: true,
  maxCookTime: true,
  maxPrepTime: true,
  minServings: true,
  sortBy: true,
  sortOrder: true,
} satisfies Record<keyof RecipeListParams, true>;

const searchParamsKeys = {
  q: true,
  page: true,
  pageSize: true,
  categoryId: true,
  difficulty: true,
} satisfies Record<keyof RecipeSearchParams, true>;

const sorted = (keys: readonly string[]) => [...keys].sort();

function contractIt(value: unknown) {
  return value ? it : it.skip;
}

describe('types/search.ts ↔ Backend', () => {
  const summary = backendDtoKeys('RecipeSummaryDto', 'Recipes/RecipeDtos.cs');
  contractIt(summary)('RecipeSummary có đúng các khóa của RecipeSummaryDto', () => {
    expect(sorted(Object.keys(summaryKeys))).toEqual(sorted(summary as string[]));
  });

  const hit = backendDtoKeys('RecipeSearchResultDto', 'Recipes/RecipeDtos.cs');
  contractIt(hit)(
    'RecipeSearchHit = RecipeSummary + relevanceScore, khớp RecipeSearchResultDto',
    () => {
      expect(sorted(Object.keys(hitKeys))).toEqual(sorted(hit as string[]));
      expect(sorted(Object.keys(hitKeys)).filter((k) => !(k in summaryKeys))).toEqual([
        'relevanceScore',
      ]);
    },
  );

  it('RecipeSearchParams chỉ có q, page, pageSize, categoryId, difficulty (SRS §8.3)', () => {
    expect(sorted(Object.keys(searchParamsKeys))).toEqual([
      'categoryId',
      'difficulty',
      'page',
      'pageSize',
      'q',
    ]);
  });

  it('RecipeListParams có đủ 5 bộ lọc + sắp xếp + phân trang (SRS §8.1)', () => {
    expect(sorted(Object.keys(listParamsKeys))).toEqual([
      'categoryId',
      'difficulty',
      'maxCookTime',
      'maxPrepTime',
      'minServings',
      'page',
      'pageSize',
      'sortBy',
      'sortOrder',
    ]);
  });

  const columns = backendSortColumns();
  contractIt(columns)('SORT_FIELDS khớp whitelist SortMapper', () => {
    expect(sorted(SORT_FIELDS)).toEqual(sorted(columns as string[]));
  });

  it('hằng số mặc định khớp Backend', () => {
    expect(SORT_ORDERS).toEqual(['asc', 'desc']);
    expect(DIFFICULTIES).toEqual(['Easy', 'Medium', 'Hard', 'Expert']);
    expect(MIN_SEARCH_LENGTH).toBe(2);
    const sortFile = 'Recipes/SortMapper.cs';
    const rulesFile = 'Recipes/RecipeFilterSpec.cs';
    const backend = {
      sortBy: backendConst(sortFile, 'DefaultSortBy'),
      sortOrder: backendConst(sortFile, 'DefaultSortOrder'),
      pageSize: backendConst(rulesFile, 'DefaultPageSize'),
      maxPageSize: backendConst(rulesFile, 'MaxPageSize'),
    };
    if (Object.values(backend).some((v) => v === null)) return; // không có mã nguồn Backend để so
    expect({
      sortBy: DEFAULT_SORT_BY,
      sortOrder: DEFAULT_SORT_ORDER,
      pageSize: DEFAULT_PAGE_SIZE,
      maxPageSize: MAX_PAGE_SIZE,
    }).toEqual(backend);
  });
});
