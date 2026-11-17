import { renderToString } from 'react-dom/server';
import type { Category } from '@/types/category';
import SearchFilterPanel from './SearchFilterPanel';

const MON_VIET = '11111111-1111-4111-8111-111111111111';
const MON_AU = '22222222-2222-4222-8222-222222222222';
const categories: Category[] = [
  { id: MON_VIET, name: 'Món Việt', slug: 'mon-viet', recipeCount: 30 } as Category,
  { id: MON_AU, name: 'Món Âu', slug: 'mon-au', recipeCount: 12 } as Category,
];

/**
 * HTML do server render là thứ người dùng tắt JavaScript nhận được: bộ lọc danh mục phải là `<select>` gốc gửi được trong form
 * GET — Radix Select (shadcn) chỉ dựng phần thao tác được sau khi hydrate.
 */
describe('<SearchFilterPanel /> — HTML do server render (không JavaScript)', () => {
  const html = () => {
    const container = document.createElement('div');
    container.innerHTML = renderToString(
      <SearchFilterPanel
        state={{ q: 'ga', categoryId: MON_AU, difficulty: 'Easy' }}
        categories={categories}
        onNavigate={jest.fn()}
      />,
    );
    return container;
  };

  it('danh mục là <select name="categoryId"> gốc, chọn sẵn giá trị trong URL, đủ các danh mục', () => {
    const select = html().querySelector('select[name="categoryId"]') as HTMLSelectElement;
    expect(select).not.toBeNull();
    expect(select.value).toBe(MON_AU);
    expect([...select.options].map((option) => [option.value, option.text])).toEqual([
      ['', 'Tất cả danh mục'],
      [MON_VIET, 'Món Việt'],
      [MON_AU, 'Món Âu'],
    ]);
    expect(html().querySelector('[role="combobox"]:not(select)')).toBeNull();
  });

  it('form GET giữ các bộ lọc khác và có nút "Lọc theo danh mục" trong <noscript>', () => {
    const form = html().querySelector('form[method="get"]') as HTMLFormElement;
    expect(form.getAttribute('action')).toBe('/search');
    expect(form.querySelector('input[name="q"]')).toHaveValue('ga');
    expect(form.querySelector('input[name="difficulty"]')).toHaveValue('Easy');
    expect(form.querySelector('noscript')?.innerHTML).toContain('Lọc theo danh mục');
  });
});
