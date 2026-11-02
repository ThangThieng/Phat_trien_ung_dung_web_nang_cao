import type { Category } from '@/types/category';
import {
  formatResultRange,
  getActiveFilters,
  popularCategories,
  formatCookTimeLimit,
  formatPrepTimeLimit,
} from './filters';

const cat = (id: string, name: string, recipeCount: number) =>
  ({ id, name, slug: id, recipeCount }) as Category;

describe('formatResultRange (FR-SRCH-004)', () => {
  it('trang 2, 12 mục/trang, 57 kết quả → 13–24', () => {
    expect(formatResultRange(2, 12, 12, 57)).toBe('Hiển thị 13–24 trên 57 kết quả');
  });

  it('trang cuối thiếu mục → dùng số mục thật: 49–57', () => {
    expect(formatResultRange(5, 12, 9, 57)).toBe('Hiển thị 49–57 trên 57 kết quả');
  });

  it('không có mục nào', () => {
    expect(formatResultRange(1, 12, 0, 0)).toBe('0 trên 0 kết quả');
  });
});

describe('getActiveFilters', () => {
  const categories = [cat('c1', 'Món Việt', 3)];

  it('đúng thứ tự bảng bộ lọc, nhãn tiếng Việt', () => {
    expect(
      getActiveFilters(
        {
          q: 'ga',
          page: 2,
          minServings: 4,
          difficulty: 'Expert',
          categoryId: 'c1',
          maxPrepTime: 15,
          maxCookTime: 90,
        },
        categories,
      ),
    ).toEqual([
      { key: 'categoryId', label: 'Danh mục: Món Việt' },
      { key: 'difficulty', label: 'Độ khó: Chuyên gia' },
      { key: 'maxCookTime', label: 'Nấu ≤ 1 giờ 30 phút' },
      { key: 'maxPrepTime', label: 'Chuẩn bị ≤ 15 phút' },
      { key: 'minServings', label: 'Từ 4 khẩu phần' },
    ]);
  });

  it('không tính q, trang; 0 phút vẫn là bộ lọc đang bật', () => {
    expect(getActiveFilters({ q: 'ga', page: 3 }, categories)).toEqual([]);
    expect(getActiveFilters({ maxCookTime: 0 }, categories)).toEqual([
      { key: 'maxCookTime', label: 'Nấu ≤ 0 phút' },
    ]);
  });

  it('danh mục không còn trong danh sách (đã xóa) vẫn hiện nhãn để gỡ được', () => {
    expect(getActiveFilters({ categoryId: 'khong-con' }, categories)).toEqual([
      { key: 'categoryId', label: 'Danh mục: đã chọn' },
    ]);
  });

  it('mốc thời gian định dạng theo giờ', () => {
    expect(formatCookTimeLimit(120)).toBe('Nấu ≤ 2 giờ');
    expect(formatPrepTimeLimit(60)).toBe('Chuẩn bị ≤ 1 giờ');
  });
});

describe('popularCategories', () => {
  it('nhiều công thức nhất trước, bỏ danh mục trống, tối đa 4', () => {
    const list = [
      cat('a', 'Chè', 2),
      cat('b', 'Bánh', 9),
      cat('c', 'Trống', 0),
      cat('d', 'Lẩu', 9),
      cat('e', 'Gỏi', 5),
      cat('f', 'Xôi', 1),
    ];
    expect(popularCategories(list).map((c) => c.name)).toEqual(['Bánh', 'Lẩu', 'Gỏi', 'Chè']);
  });
});
