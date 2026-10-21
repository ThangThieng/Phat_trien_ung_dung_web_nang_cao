import { render, screen, within } from '@testing-library/react';
import { getCategories } from '@/features/categories/api';
import { getLatestRecipes } from '@/features/recipes/api';
import { mockCategories } from '@/mocks/fixtures/categories';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import type { RecipeSummary } from '@/types/recipe';
import HomePage from './page';

jest.mock('@/features/recipes/api', () => ({ getLatestRecipes: jest.fn() }));
jest.mock('@/features/categories/api', () => ({ getCategories: jest.fn() }));

const mockedLatest = jest.mocked(getLatestRecipes);
const mockedCategories = jest.mocked(getCategories);

/** 8 thẻ có ảnh (để kiểm `priority` / `loading`): lặp lại 4 công thức mẫu với id và slug khác nhau. */
const eightRecipes: RecipeSummary[] = Array.from({ length: 8 }, (_, index) => ({
  ...mockRecipeSummaries[index % mockRecipeSummaries.length],
  id: `00000000-0000-4000-8000-0000000001${index}`,
  slug: `cong-thuc-${index}`,
  primaryImageUrl: `http://localhost/media/recipes/${index}.webp`,
}));

const paged = (items: RecipeSummary[]) => ({
  items,
  page: 1,
  pageSize: 8,
  totalCount: items.length,
  totalPages: items.length > 0 ? 1 : 0,
  hasNextPage: false,
  hasPreviousPage: false,
});

async function renderHome() {
  render(await HomePage());
}

beforeEach(() => {
  mockedLatest.mockReset().mockResolvedValue(paged(eightRecipes));
  mockedCategories.mockReset().mockResolvedValue([...mockCategories]);
});

describe('Trang chủ /', () => {
  it('lấy công thức mới xuất bản và danh mục, mỗi nguồn một lần', async () => {
    await renderHome();

    expect(mockedLatest).toHaveBeenCalledTimes(1);
    expect(mockedCategories).toHaveBeenCalledTimes(1);
  });

  it('có đúng một h1, khối giới thiệu kèm form tìm kiếm GET tới /search (chạy khi chưa có JavaScript)', async () => {
    await renderHome();

    expect(screen.getAllByRole('heading', { level: 1 })).toHaveLength(1);
    const form = screen.getByRole('search');
    expect(form).toHaveAttribute('action', '/search');
    expect(form).toHaveAttribute('method', 'get');
    const input = within(form).getByLabelText('Tìm công thức');
    expect(input).toHaveAttribute('name', 'q');
    expect(input).toBeRequired();
    expect(input).toHaveAttribute('minlength', '2');
  });

  it('hiện 8 thẻ trong "Công thức mới xuất bản" và liên kết "Xem tất cả" tới /recipes', async () => {
    await renderHome();

    const section = screen.getByRole('region', { name: 'Công thức mới xuất bản' });
    expect(within(section).getAllByRole('article')).toHaveLength(8);
    expect(within(section).getByRole('link', { name: /Xem tất cả/ })).toHaveAttribute(
      'href',
      '/recipes',
    );
  });

  it('chỉ ảnh của thẻ đầu tiên tải trước (LCP), các thẻ khác lazy', async () => {
    await renderHome();

    const images = screen.getAllByRole('img');
    expect(images).toHaveLength(8);
    // `priority` của next/image bỏ `loading="lazy"` (tải ngay, thêm preload); ảnh thường giữ lazy.
    expect(images[0]).not.toHaveAttribute('loading', 'lazy');
    images.slice(1).forEach((image) => expect(image).toHaveAttribute('loading', 'lazy'));
  });

  it('có cả sidebar danh mục (≥ 1200 px) lẫn hàng nhãn cuộn ngang (< 1200 px) — CSS chọn cái hiện ra', async () => {
    await renderHome();

    const sidebar = screen.getByRole('navigation', { name: 'Danh mục' });
    const chips = screen.getByRole('navigation', { name: 'Danh mục nhanh' });
    expect(sidebar).toHaveClass('hidden', 'min-[1200px]:block');
    expect(chips).toHaveClass('min-[1200px]:hidden');
    mockCategories.forEach((category) => {
      expect(
        within(sidebar).getByRole('link', { name: new RegExp(category.name) }),
      ).toHaveAttribute('href', `/categories/${category.slug}`);
    });
  });

  it('API danh mục lỗi → vẫn hiện công thức, chỉ bỏ sidebar và hàng nhãn', async () => {
    mockedCategories.mockRejectedValue(new Error('API down'));
    await renderHome();

    expect(screen.getAllByRole('article')).toHaveLength(8);
    expect(screen.queryByRole('navigation', { name: 'Danh mục' })).not.toBeInTheDocument();
    expect(screen.queryByRole('navigation', { name: 'Danh mục nhanh' })).not.toBeInTheDocument();
  });

  it('chưa có công thức nào → trạng thái rỗng, không có thẻ', async () => {
    mockedLatest.mockResolvedValue(paged([]));
    await renderHome();

    expect(screen.queryByRole('article')).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Chưa có công thức nào được xuất bản' }),
    ).toBeInTheDocument();
  });
});
