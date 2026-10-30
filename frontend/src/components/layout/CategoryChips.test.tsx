import { render, screen, within } from '@testing-library/react';
import { mockCategories } from '@/mocks/fixtures/categories';
import CategoryChips from './CategoryChips';

describe('<CategoryChips />', () => {
  it('là landmark nav "Danh mục nhanh", mỗi danh mục một liên kết kèm số công thức cho trình đọc màn hình', () => {
    render(<CategoryChips categories={mockCategories} />);
    const nav = screen.getByRole('navigation', { name: 'Danh mục nhanh' });
    const links = within(nav).getAllByRole('link');

    expect(links).toHaveLength(mockCategories.length);
    mockCategories.forEach((category, index) => {
      expect(links[index]).toHaveAttribute('href', `/categories/${category.slug}`);
      expect(links[index]).toHaveTextContent(category.name);
      expect(links[index]).toHaveTextContent(`${category.recipeCount} công thức`);
    });
  });

  it('chỉ nhãn đang xem có aria-current="page"', () => {
    const active = mockCategories[1];
    render(<CategoryChips categories={mockCategories} activeSlug={active.slug} />);

    const current = screen.getAllByRole('link', { current: 'page' });
    expect(current).toHaveLength(1);
    expect(current[0]).toHaveAttribute('href', `/categories/${active.slug}`);
  });

  it('không có danh mục → không hiện gì', () => {
    const { container } = render(<CategoryChips categories={[]} />);
    expect(container).toBeEmptyDOMElement();
  });
});
