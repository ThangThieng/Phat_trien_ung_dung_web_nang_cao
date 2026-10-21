import { render, screen, within } from '@testing-library/react';
import Pagination, { getPageItems } from './Pagination';

const buildHref = (page: number) => `/search?q=pho&difficulty=Easy&maxCookTime=30&page=${page}`;

describe('getPageItems', () => {
  it('liệt kê đủ khi ≤ 7 trang', () => {
    expect(getPageItems(1, 1)).toEqual([1]);
    expect(getPageItems(3, 7)).toEqual([1, 2, 3, 4, 5, 6, 7]);
  });

  it('trang đầu: dấu … ở cuối', () => {
    expect(getPageItems(1, 20)).toEqual([1, 2, 3, 4, 5, 'ellipsis-end', 20]);
    expect(getPageItems(4, 20)).toEqual([1, 2, 3, 4, 5, 'ellipsis-end', 20]);
  });

  it('trang giữa: dấu … ở hai bên', () => {
    expect(getPageItems(10, 20)).toEqual([1, 'ellipsis-start', 9, 10, 11, 'ellipsis-end', 20]);
  });

  it('trang cuối: dấu … ở đầu', () => {
    expect(getPageItems(20, 20)).toEqual([1, 'ellipsis-start', 16, 17, 18, 19, 20]);
    expect(getPageItems(17, 20)).toEqual([1, 'ellipsis-start', 16, 17, 18, 19, 20]);
  });

  it('luôn đúng 7 ô từ 8 trang trở lên — thanh phân trang không đổi bề rộng (CLS)', () => {
    for (let page = 1; page <= 30; page += 1) expect(getPageItems(page, 30)).toHaveLength(7);
  });
});

describe('<Pagination />', () => {
  it('không hiển thị khi chỉ có 1 trang', () => {
    const { container } = render(<Pagination page={1} totalPages={1} buildHref={buildHref} />);
    expect(container).toBeEmptyDOMElement();
  });

  it('là landmark nav "Phân trang", trang hiện tại có aria-current="page"', () => {
    render(<Pagination page={10} totalPages={20} buildHref={buildHref} />);
    const nav = screen.getByRole('navigation', { name: 'Phân trang' });
    const current = within(nav).getByRole('link', { current: 'page' });
    expect(current).toHaveTextContent('10');
    expect(within(nav).getAllByRole('link', { current: false })).not.toContain(current);
  });

  it('dùng link thật và giữ nguyên mọi tham số lọc khác trên URL', () => {
    render(<Pagination page={1} totalPages={20} buildHref={buildHref} />);
    expect(screen.getByRole('link', { name: 'Trang 20' })).toHaveAttribute(
      'href',
      '/search?q=pho&difficulty=Easy&maxCookTime=30&page=20',
    );
    expect(screen.getByRole('link', { name: 'Trang sau' })).toHaveAttribute(
      'href',
      '/search?q=pho&difficulty=Easy&maxCookTime=30&page=2',
    );
  });

  it('trang đầu: nút "Trang trước" bị vô hiệu (không phải link)', () => {
    render(<Pagination page={1} totalPages={3} buildHref={buildHref} />);
    expect(screen.queryByRole('link', { name: 'Trang trước' })).not.toBeInTheDocument();
    expect(screen.getByLabelText('Trang trước')).toHaveAttribute('aria-disabled', 'true');
  });

  it('tôn trọng hasNextPage/hasPreviousPage của PagedResult', () => {
    render(
      <Pagination
        page={2}
        totalPages={3}
        hasPreviousPage={false}
        hasNextPage={false}
        buildHref={buildHref}
      />,
    );
    expect(screen.queryByRole('link', { name: 'Trang trước' })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Trang sau' })).not.toBeInTheDocument();
  });
});
