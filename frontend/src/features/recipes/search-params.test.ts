import {
  buildHref,
  buildQueryString,
  countActiveFilters,
  fromUrlSearchParams,
  parseCategoryState,
  parseRecipeListState,
  parseSearchState,
  toOptionalInt,
} from './search-params';

const GUID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';

describe('parseRecipeListState', () => {
  it('URL trống → mặc định createdAt desc, trang 1, không có bộ lọc', () => {
    expect(parseRecipeListState({})).toEqual({
      page: 1,
      categoryId: undefined,
      difficulty: undefined,
      maxCookTime: undefined,
      maxPrepTime: undefined,
      minServings: undefined,
      sortBy: 'createdAt',
      sortOrder: 'desc',
    });
  });

  it('đọc đủ 5 bộ lọc + sortBy/sortOrder + page', () => {
    const state = parseRecipeListState({
      page: '3',
      categoryId: GUID,
      difficulty: 'Expert',
      maxCookTime: '30',
      maxPrepTime: '15',
      minServings: '4',
      sortBy: 'title',
      sortOrder: 'asc',
    });
    expect(state).toMatchObject({
      page: 3,
      categoryId: GUID,
      difficulty: 'Expert',
      maxCookTime: 30,
      maxPrepTime: 15,
      minServings: 4,
      sortBy: 'title',
      sortOrder: 'asc',
    });
  });

  it('không phân biệt hoa/thường ở sortBy, sortOrder, difficulty', () => {
    const state = parseRecipeListState({
      sortBy: 'COOKTIME',
      sortOrder: 'ASC',
      difficulty: 'hard',
    });
    expect(state).toMatchObject({ sortBy: 'cookTime', sortOrder: 'asc', difficulty: 'Hard' });
  });

  it.each([
    ['sortBy ngoài whitelist', { sortBy: 'password' }, { sortBy: 'createdAt' }],
    ['sortBy kiểu cũ -createdAt', { sortBy: '-createdAt' }, { sortBy: 'createdAt' }],
    ['sortOrder sai', { sortOrder: 'up' }, { sortOrder: 'desc' }],
    ['page không phải số', { page: 'abc' }, { page: 1 }],
    ['page âm', { page: '-2' }, { page: 1 }],
    ['page = 0', { page: '0' }, { page: 1 }],
    ['difficulty ngoài enum', { difficulty: 'Legendary' }, { difficulty: undefined }],
    ['categoryId không phải GUID', { categoryId: 'abc' }, { categoryId: undefined }],
    ['maxCookTime âm', { maxCookTime: '-1' }, { maxCookTime: undefined }],
    ['maxCookTime không phải số', { maxCookTime: '1e3' }, { maxCookTime: undefined }],
    ['minServings = 0', { minServings: '0' }, { minServings: undefined }],
  ])('giá trị hỏng (%s) → về mặc định, không ném lỗi', (_name, raw, expected) => {
    expect(parseRecipeListState(raw)).toMatchObject(expected);
  });

  it('tham số lặp lấy giá trị đầu tiên', () => {
    expect(parseRecipeListState({ page: ['2', '5'] }).page).toBe(2);
  });
});

describe('parseCategoryState / parseSearchState', () => {
  it('danh mục chỉ có page + sort (SRS §8.2), bỏ qua bộ lọc lạ', () => {
    const state = parseCategoryState({ page: '2', sortBy: 'prepTime', difficulty: 'Easy' });
    expect(state).toEqual({ page: 2, sortBy: 'prepTime', sortOrder: 'desc' });
  });

  it('tìm kiếm không có sắp xếp (SRS §8.3 — luôn theo ts_rank), cắt khoảng trắng q', () => {
    const state = parseSearchState({ q: '  pho bo ', difficulty: 'Easy', sortBy: 'title' });
    expect(state).toEqual({
      q: 'pho bo',
      page: 1,
      categoryId: undefined,
      difficulty: 'Easy',
      maxCookTime: undefined,
      maxPrepTime: undefined,
      minServings: undefined,
    });
  });
});

describe('parseSearchState — bộ lọc thời gian & khẩu phần (CR-2026-05 / MT-66, Buổi 7)', () => {
  it('đọc maxCookTime, maxPrepTime, minServings cùng q, danh mục, độ khó, trang', () => {
    expect(
      parseSearchState({
        q: 'ga',
        page: '2',
        categoryId: GUID,
        difficulty: 'expert',
        maxCookTime: '30',
        maxPrepTime: '15',
        minServings: '4',
      }),
    ).toEqual({
      q: 'ga',
      page: 2,
      categoryId: GUID,
      difficulty: 'Expert',
      maxCookTime: 30,
      maxPrepTime: 15,
      minServings: 4,
    });
  });

  it('0 phút là giá trị hợp lệ (API nhận ≥ 0), không bị coi là "chưa chọn"', () => {
    expect(parseSearchState({ q: 'ga', maxCookTime: '0', maxPrepTime: '0' })).toMatchObject({
      maxCookTime: 0,
      maxPrepTime: 0,
    });
  });

  it.each([
    ['maxCookTime=abc', { maxCookTime: 'abc' }, { maxCookTime: undefined }],
    ['maxCookTime=-1', { maxCookTime: '-1' }, { maxCookTime: undefined }],
    ['maxCookTime=12.5', { maxCookTime: '12.5' }, { maxCookTime: undefined }],
    ['maxPrepTime=30phut', { maxPrepTime: '30phut' }, { maxPrepTime: undefined }],
    ['maxPrepTime vượt kiểu int', { maxPrepTime: '99999999999' }, { maxPrepTime: undefined }],
    ['minServings=0', { minServings: '0' }, { minServings: undefined }],
    ['minServings rỗng', { minServings: '' }, { minServings: undefined }],
  ])('giá trị hỏng (%s) → về mặc định, các tham số khác giữ nguyên', (_name, raw, expected) => {
    expect(parseSearchState({ q: 'pho', difficulty: 'Hard', ...raw })).toMatchObject({
      q: 'pho',
      difficulty: 'Hard',
      ...expected,
    });
  });

  it('khứ hồi parse ↔ buildHref không mất dữ liệu (đủ 8 tham số)', () => {
    const state = parseSearchState({
      q: 'phở bò',
      page: '3',
      categoryId: GUID,
      difficulty: 'Medium',
      maxCookTime: '60',
      maxPrepTime: '15',
      minServings: '2',
    });
    const href = buildHref('/search', state);
    expect(href).toBe(
      `/search?q=ph%E1%BB%9F+b%C3%B2&categoryId=${GUID}&difficulty=Medium&maxCookTime=60&maxPrepTime=15&minServings=2&page=3`,
    );
    const query = Object.fromEntries(new URL(href, 'http://localhost').searchParams);
    expect(parseSearchState(query)).toEqual(state);
  });

  it('đổi một bộ lọc thời gian → về trang 1, giữ q và các bộ lọc khác', () => {
    const state = parseSearchState({ q: 'ga', page: '4', difficulty: 'Easy', minServings: '2' });
    expect(buildHref('/search', state, { maxCookTime: 30, page: 1 })).toBe(
      '/search?q=ga&difficulty=Easy&maxCookTime=30&minServings=2',
    );
  });
});

describe('buildQueryString / buildHref', () => {
  it('bỏ giá trị mặc định (trang 1, createdAt, desc) cho URL ngắn', () => {
    expect(buildQueryString(parseRecipeListState({}))).toBe('');
    expect(buildHref('/recipes', parseRecipeListState({}))).toBe('/recipes');
  });

  it('thứ tự khóa cố định: cùng trạng thái → cùng URL', () => {
    const a = buildQueryString({ page: 2, sortBy: 'title', difficulty: 'Easy', q: 'ga' });
    const b = buildQueryString({ sortBy: 'title', q: 'ga', page: 2, difficulty: 'Easy' });
    expect(a).toBe(b);
    expect(a).toBe('q=ga&difficulty=Easy&sortBy=title&page=2');
  });

  it('đổi bộ lọc/sắp xếp thì overrides.page = 1 loại bỏ page khỏi URL', () => {
    const state = parseRecipeListState({ page: '4', difficulty: 'Hard' });
    expect(buildHref('/recipes', state, { sortBy: 'cookTime', page: 1 })).toBe(
      '/recipes?difficulty=Hard&sortBy=cookTime',
    );
  });

  it('overrides undefined xóa tham số', () => {
    const state = parseRecipeListState({ difficulty: 'Hard', maxCookTime: '20' });
    expect(buildQueryString(state, { difficulty: undefined, maxCookTime: undefined })).toBe('');
  });

  it('giữ q khi đổi bộ lọc ở /search và mã hóa ký tự đặc biệt', () => {
    expect(buildHref('/search', { q: 'phở & bò', page: 1 }, { difficulty: 'Easy' })).toBe(
      '/search?q=ph%E1%BB%9F+%26+b%C3%B2&difficulty=Easy',
    );
  });

  it('round-trip: parse(build(state)) = state', () => {
    const state = parseRecipeListState({
      page: '2',
      categoryId: GUID,
      difficulty: 'Medium',
      maxCookTime: '45',
      minServings: '2',
      sortBy: 'publishedAt',
      sortOrder: 'asc',
    });
    const query = Object.fromEntries(new URLSearchParams(buildQueryString(state)));
    expect(parseRecipeListState(query)).toEqual(state);
  });
});

describe('toOptionalInt / countActiveFilters', () => {
  it('chấp nhận số nguyên trong khoảng, loại phần còn lại', () => {
    expect(toOptionalInt('0')).toBe(0);
    expect(toOptionalInt(' 25 ')).toBe(25);
    expect(toOptionalInt('0', 1)).toBeUndefined();
    expect(toOptionalInt('2.5')).toBeUndefined();
    expect(toOptionalInt('99999999999')).toBeUndefined();
    expect(toOptionalInt(undefined)).toBeUndefined();
    expect(toOptionalInt('')).toBeUndefined();
  });

  it('đếm bộ lọc đang bật, không tính q/trang/sắp xếp', () => {
    expect(countActiveFilters({ q: 'x', page: 3, sortBy: 'title' })).toBe(0);
    expect(countActiveFilters({ categoryId: GUID, maxCookTime: 0, difficulty: 'Easy' })).toBe(3);
  });
});

describe('fromUrlSearchParams', () => {
  it('cùng URL → trình duyệt và server parse ra cùng trạng thái (tham số lặp lấy giá trị đầu)', () => {
    const url = '/search?q=ga&page=2&page=9&maxCookTime=30&difficulty=Easy';
    const browser = parseSearchState(
      fromUrlSearchParams(new URL(url, 'http://localhost').searchParams),
    );
    const server = parseSearchState({
      q: 'ga',
      page: ['2', '9'],
      maxCookTime: '30',
      difficulty: 'Easy',
    });
    expect(browser).toEqual(server);
    expect(browser.page).toBe(2);
  });
});
