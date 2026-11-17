import { render, screen, within } from '@testing-library/react';
import { getCategories } from '@/features/categories/api';
import { mockCategories } from '@/mocks/fixtures/categories';
import CategoriesPage from './page';

jest.mock('@/features/categories/api', () => ({ getCategories: jest.fn() }));

const mockedCategories = jest.mocked(getCategories);

describe('Trang danh sách danh mục /categories', () => {
  it('mỗi danh mục một thẻ trong danh sách, liên kết tới trang danh mục và có huy hiệu số công thức', async () => {
    mockedCategories.mockResolvedValue([...mockCategories]);
    render(await CategoriesPage());

    expect(
      screen.getByRole('heading', { level: 1, name: 'Danh mục công thức' }),
    ).toBeInTheDocument();
    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(mockCategories.length);
    mockCategories.forEach((category, index) => {
      expect(within(items[index]).getByRole('link')).toHaveAttribute(
        'href',
        `/categories/${category.slug}`,
      );
      expect(
        within(items[index]).getByText(`${category.recipeCount} công thức`),
      ).toBeInTheDocument();
    });
  });

  it('chưa có danh mục → trạng thái rỗng', async () => {
    mockedCategories.mockResolvedValue([]);
    render(await CategoriesPage());

    expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Chưa có danh mục nào' })).toBeInTheDocument();
  });
});
