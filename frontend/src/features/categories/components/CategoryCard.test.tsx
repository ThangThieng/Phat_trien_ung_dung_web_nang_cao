import { render, screen } from '@testing-library/react';
import { mockCategories } from '@/mocks/fixtures/categories';
import { readBackendFile } from '../../../../test-utils/csharp-source';
import CategoryCard, { CATEGORY_EMOJI } from './CategoryCard';

/** Cùng quy tắc `SlugHelper.Generate` của Backend: chữ thường, đ → d, bỏ dấu, ký tự khác chữ/số → "-". */
const slugify = (text: string) =>
  text
    .trim()
    .toLowerCase()
    .replace(/đ/g, 'd')
    .normalize('NFD')
    .replace(/\p{Mn}/gu, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

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

  it('mọi danh mục mẫu (seed CR-2026-03, 20 danh mục) đều có biểu tượng riêng khi chưa có ảnh', () => {
    const seed = JSON.parse(
      readBackendFile('src/CulinaryBlog.Infrastructure/Persistence/Seed/Data/categories.json'),
    ) as { name: string }[];
    const slugs = seed.map((c) => slugify(c.name));

    expect(slugs.length).toBeGreaterThanOrEqual(20);
    expect(slugs.filter((slug) => !CATEGORY_EMOJI[slug])).toEqual([]);
  });
});
