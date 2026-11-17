import { render, screen, within } from '@testing-library/react';
import { getCategories, getCategoryBySlug } from '@/features/categories/api';
import { ApiError } from '@/lib/api-client';
import { mockCategories } from '@/mocks/fixtures/categories';
import { mockRecipeSummaries } from '@/mocks/fixtures/recipes';
import type { CategoryDetail } from '@/types/category';
import CategoryDetailPage, { generateMetadata } from './page';

jest.mock('@/features/categories/api', () => ({
  getCategories: jest.fn(),
  getCategoryBySlug: jest.fn(),
}));
jest.mock('next/navigation', () => ({
  notFound: jest.fn(() => {
    throw new Error('NEXT_NOT_FOUND');
  }),
  useRouter: () => ({ push: jest.fn() }),
}));

const mockedDetail = jest.mocked(getCategoryBySlug);
const mockedCategories = jest.mocked(getCategories);

const category = mockCategories[1];

function detailWith(overrides: Partial<CategoryDetail['recipes']> = {}): CategoryDetail {
  const items = mockRecipeSummaries.slice(0, 3);
  return {
    category,
    recipes: {
      items,
      page: 1,
      pageSize: 12,
      totalCount: items.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
      ...overrides,
    },
  };
}

async function renderPage(search: Record<string, string> = {}) {
  render(
    await CategoryDetailPage({
      params: Promise.resolve({ slug: category.slug }),
      searchParams: Promise.resolve(search),
    }),
  );
}

beforeEach(() => {
  mockedDetail.mockReset().mockResolvedValue(detailWith());
  mockedCategories.mockReset().mockResolvedValue([...mockCategories]);
});

describe('Trang danh mục /categories/[slug]', () => {
  it('hiện tên, mô tả, số công thức và lưới công thức của danh mục', async () => {
    await renderPage();

    expect(screen.getByRole('heading', { level: 1, name: category.name })).toBeInTheDocument();
    expect(screen.getByText(category.description as string)).toBeInTheDocument();
    expect(screen.getByText('3 công thức')).toBeInTheDocument();
    expect(screen.getAllByRole('article')).toHaveLength(3);
  });

  it('SidebarCategory đánh dấu đúng mục đang xem bằng aria-current="page"', async () => {
    await renderPage();

    const sidebar = screen.getByRole('navigation', { name: 'Danh mục' });
    const current = within(sidebar).getAllByRole('link', { current: 'page' });
    expect(current).toHaveLength(1);
    expect(current[0]).toHaveAttribute('href', `/categories/${category.slug}`);
  });

  it('dưới 1200 px: hàng nhãn cuộn ngang cũng đánh dấu mục đang xem', async () => {
    await renderPage();

    const chips = screen.getByRole('navigation', { name: 'Danh mục nhanh' });
    expect(chips).toHaveClass('min-[1200px]:hidden');
    expect(within(chips).getByRole('link', { current: 'page' })).toHaveAttribute(
      'href',
      `/categories/${category.slug}`,
    );
  });

  it('truyền trang và sắp xếp từ URL cho API — chỉ sortBy/sortOrder/page (SRS §8.2)', async () => {
    await renderPage({ page: '2', sortBy: 'title', sortOrder: 'asc', difficulty: 'Easy' });

    expect(mockedDetail).toHaveBeenCalledWith(category.slug, {
      page: 2,
      sortBy: 'title',
      sortOrder: 'asc',
    });
  });

  it('phân trang là link thật và giữ nguyên tham số sắp xếp', async () => {
    mockedDetail.mockResolvedValue(
      detailWith({ page: 2, totalPages: 3, hasNextPage: true, hasPreviousPage: true }),
    );
    await renderPage({ page: '2', sortBy: 'title', sortOrder: 'asc' });

    const pagination = screen.getByRole('navigation', { name: 'Phân trang' });
    expect(within(pagination).getByRole('link', { name: 'Trang 3' })).toHaveAttribute(
      'href',
      `/categories/${category.slug}?sortBy=title&sortOrder=asc&page=3`,
    );
    expect(within(pagination).getByRole('link', { name: 'Trang 1' })).toHaveAttribute(
      'href',
      `/categories/${category.slug}?sortBy=title&sortOrder=asc`,
    );
  });

  it('danh mục chưa có công thức → trạng thái rỗng kèm liên kết về /recipes', async () => {
    mockedDetail.mockResolvedValue(detailWith({ items: [], totalCount: 0, totalPages: 0 }));
    await renderPage();

    expect(screen.queryByRole('article')).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Danh mục này chưa có công thức nào' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Xem tất cả công thức' })).toHaveAttribute(
      'href',
      '/recipes',
    );
  });

  it('trang vượt quá tổng số trang → báo riêng và dẫn về trang đầu (không nhầm với danh mục rỗng)', async () => {
    mockedDetail.mockResolvedValue(
      detailWith({ items: [], page: 9, totalCount: 3, totalPages: 1, hasPreviousPage: true }),
    );
    await renderPage({ page: '9', sortBy: 'title' });

    expect(
      screen.getByRole('heading', { name: 'Trang này không có công thức nào' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Về trang đầu' })).toHaveAttribute(
      'href',
      `/categories/${category.slug}?sortBy=title`,
    );
  });

  it('API danh sách danh mục lỗi → vẫn hiện công thức, chỉ bỏ sidebar và hàng nhãn', async () => {
    mockedCategories.mockRejectedValue(new Error('API down'));
    await renderPage();

    expect(screen.getAllByRole('article')).toHaveLength(3);
    expect(screen.queryByRole('navigation', { name: 'Danh mục' })).not.toBeInTheDocument();
    expect(screen.queryByRole('navigation', { name: 'Danh mục nhanh' })).not.toBeInTheDocument();
  });

  it('slug không tồn tại (API 404) → notFound()', async () => {
    mockedDetail.mockRejectedValue(
      new ApiError(404, { status: 404, title: 'Not Found', type: 'CATEGORY_NOT_FOUND' }),
    );

    await expect(
      CategoryDetailPage({
        params: Promise.resolve({ slug: 'khong-co' }),
        searchParams: Promise.resolve({}),
      }),
    ).rejects.toThrow('NEXT_NOT_FOUND');
  });

  it('lỗi khác 404 thì ném lên cho error boundary, không biến thành 404', async () => {
    mockedDetail.mockRejectedValue(new ApiError(500, { status: 500, title: 'Server Error' }));

    await expect(
      CategoryDetailPage({
        params: Promise.resolve({ slug: category.slug }),
        searchParams: Promise.resolve({}),
      }),
    ).rejects.toBeInstanceOf(ApiError);
  });

  it('generateMetadata: tiêu đề theo tên danh mục, canonical theo slug', async () => {
    const metadata = await generateMetadata({
      params: Promise.resolve({ slug: category.slug }),
      searchParams: Promise.resolve({}),
    });

    // title.absolute: root layout có template '%s | Culinary Blog', dùng absolute để hậu tố không lặp và áp dụng giới hạn 60 ký tự.
    expect(metadata.title).toEqual({ absolute: `${category.name} | Culinary Blog` });
    expect(metadata.alternates?.canonical).toBe(`/categories/${category.slug}`);
  });
});
