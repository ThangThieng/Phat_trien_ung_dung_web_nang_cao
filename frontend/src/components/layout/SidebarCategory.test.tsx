import { render, screen, within } from '@testing-library/react';
import { mockCategories } from '@/mocks/fixtures/categories';
import SidebarCategory from './SidebarCategory';

describe('<SidebarCategory />', () => {
  it('là landmark nav "Danh mục" liệt kê mọi danh mục kèm số công thức', () => {
    render(<SidebarCategory categories={mockCategories} />);
    const nav = screen.getByRole('navigation', { name: 'Danh mục' });
    mockCategories.forEach((category) => {
      const link = within(nav).getByRole('link', {
        name: new RegExp(category.name.replace(/[&]/g, '\\$&')),
      });
      expect(link).toHaveAttribute('href', `/categories/${category.slug}`);
      expect(link).toHaveTextContent(`${category.recipeCount} công thức`);
    });
  });

  it('chỉ mục đang xem có aria-current="page"', () => {
    const active = mockCategories[2];
    render(<SidebarCategory categories={mockCategories} activeSlug={active.slug} />);
    const current = screen
      .getAllByRole('link')
      .filter((l) => l.getAttribute('aria-current') === 'page');
    expect(current).toHaveLength(1);
    expect(current[0]).toHaveAttribute('href', `/categories/${active.slug}`);
  });

  it('không có activeSlug → không mục nào được đánh dấu', () => {
    render(<SidebarCategory categories={mockCategories} />);
    expect(screen.queryByRole('link', { current: 'page' })).not.toBeInTheDocument();
  });
});
