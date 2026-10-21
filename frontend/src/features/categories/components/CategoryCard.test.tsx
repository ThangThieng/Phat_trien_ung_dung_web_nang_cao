import { render, screen } from '@testing-library/react';
import { mockCategories } from '@/mocks/fixtures/categories';
import CategoryCard from './CategoryCard';

describe('<CategoryCard />', () => {
  it('liên kết tới trang danh mục, hiện tên, mô tả và huy hiệu số công thức', () => {
    const category = { ...mockCategories[0], recipeCount: 8 };
    render(<CategoryCard category={category} />);

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', `/categories/${category.slug}`);
    expect(screen.getByRole('heading', { name: category.name })).toBeInTheDocument();
    expect(screen.getByText(category.description as string)).toHaveClass('line-clamp-2');
    expect(screen.getByText('8 công thức')).toBeInTheDocument();
  });

  it('không có mô tả / ảnh vẫn hiển thị được (biểu tượng thay ảnh)', () => {
    render(<CategoryCard category={{ ...mockCategories[0], description: null, imageUrl: null }} />);

    expect(screen.getByRole('link')).toBeInTheDocument();
    expect(screen.queryByRole('img')).not.toBeInTheDocument();
  });

  it('có ảnh thì ảnh trang trí (alt rỗng) — tên danh mục đã nằm trong tiêu đề', () => {
    const { container } = render(
      <CategoryCard
        category={{ ...mockCategories[0], imageUrl: 'http://localhost/media/c.webp' }}
      />,
    );

    expect(container.querySelector('img')).toHaveAttribute('alt', '');
  });
});
