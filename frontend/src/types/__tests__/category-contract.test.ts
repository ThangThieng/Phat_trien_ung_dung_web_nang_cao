import { backendConst, backendDtoKeys } from '../backend-contract.testutil';
import type {
  Category,
  CategoryDetail,
  CategoryDetailParams,
  CategoryRef,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../category';
import { CATEGORY_LIMITS } from '../category';

/**
 * Hợp đồng `types/category.ts` ↔ Backend (SRS §8.2). Hai lớp bảo vệ:
 *  1. `satisfies Record<keyof T, true>` – thêm/bớt một trường ở interface mà quên bảng này thì `tsc` đỏ;
 *  2. so khóa với DTO Backend (`docs/api/openapi.v1.json`, hoặc khai báo record C# khi file đó chưa có) – đổi DTO mà quên Frontend thì test đỏ.
 */
const categoryKeys = {
  id: true,
  name: true,
  slug: true,
  description: true,
  imageUrl: true,
  orderIndex: true,
  recipeCount: true,
} satisfies Record<keyof Category, true>;

const categoryRefKeys = { id: true, name: true, slug: true } satisfies Record<
  keyof CategoryRef,
  true
>;

const categoryDetailKeys = { category: true, recipes: true } satisfies Record<
  keyof CategoryDetail,
  true
>;

const createRequestKeys = {
  name: true,
  description: true,
  imageUrl: true,
  orderIndex: true,
} satisfies Record<keyof CreateCategoryRequest, true>;

const detailParamsKeys = {
  page: true,
  pageSize: true,
  sortBy: true,
  sortOrder: true,
} satisfies Record<keyof CategoryDetailParams, true>;

const sorted = (keys: string[]) => [...keys].sort();

function contractIt(keys: string[] | null) {
  if (keys) return it;
  // Không có openapi.v1.json lẫn mã nguồn Backend (ví dụ build chỉ copy frontend/) → bỏ qua, không đỏ oan.
  return it.skip;
}

describe('types/category.ts ↔ Backend', () => {
  const category = backendDtoKeys('CategoryDto', 'Categories/CategoryDtos.cs');
  contractIt(category)('Category có đúng các khóa của CategoryDto', () => {
    expect(sorted(Object.keys(categoryKeys))).toEqual(sorted(category as string[]));
  });

  const ref = backendDtoKeys('CategoryRefDto', 'Recipes/RecipeDtos.cs');
  contractIt(ref)('CategoryRef có đúng các khóa của CategoryRefDto', () => {
    expect(sorted(Object.keys(categoryRefKeys))).toEqual(sorted(ref as string[]));
  });

  const detail = backendDtoKeys('CategoryDetailDto', 'Categories/CategoryDtos.cs');
  contractIt(detail)('CategoryDetail có đúng các khóa của CategoryDetailDto', () => {
    expect(sorted(Object.keys(categoryDetailKeys))).toEqual(sorted(detail as string[]));
  });

  it('Create/UpdateCategoryRequest dùng cùng một body (FR-CAT-003/004)', () => {
    const update: UpdateCategoryRequest = {
      name: 'a',
      description: null,
      imageUrl: null,
      orderIndex: 0,
    };
    expect(sorted(Object.keys(update))).toEqual(sorted(Object.keys(createRequestKeys)));
  });

  it('CategoryDetailParams chỉ có sắp xếp và phân trang (SRS §8.2, không có bộ lọc)', () => {
    expect(sorted(Object.keys(detailParamsKeys))).toEqual([
      'page',
      'pageSize',
      'sortBy',
      'sortOrder',
    ]);
  });

  it('CATEGORY_LIMITS khớp CategoryLimits của validator Backend', () => {
    const file = 'Categories/CategoryDtos.cs';
    const expected = {
      nameMin: backendConst(file, 'NameMin'),
      nameMax: backendConst(file, 'NameMax'),
      descriptionMax: backendConst(file, 'DescriptionMax'),
      imageUrlMax: backendConst(file, 'ImageUrlMax'),
    };
    if (Object.values(expected).some((v) => v === null)) return; // không có mã nguồn Backend để so
    expect(CATEGORY_LIMITS).toEqual(expected);
  });
});
