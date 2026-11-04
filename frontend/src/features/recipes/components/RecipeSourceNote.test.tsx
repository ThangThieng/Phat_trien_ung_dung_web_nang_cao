import { render, screen } from '@testing-library/react';
import RecipeSourceNote, { SOURCE_NOTE_PREFIX } from './RecipeSourceNote';

const NOTE = `${SOURCE_NOTE_PREFIX} Wikibooks Cookbook, trang "Cookbook:Soup" (https://en.wikibooks.org/w/index.php?curid=1&oldid=2), giấy phép CC BY-SA 4.0 (https://creativecommons.org/licenses/by-sa/4.0/). Ảnh: An, CC0, https://commons.wikimedia.org/wiki/File:Soup.jpg.`;

describe('RecipeSourceNote', () => {
  it('không hiển thị với công thức không có tiền tố nguồn', () => {
    const { container, rerender } = render(<RecipeSourceNote instructions={null} />);
    expect(container).toBeEmptyDOMElement();

    rerender(<RecipeSourceNote instructions="Thực hiện theo 5 bước bên dưới." />);
    expect(container).toBeEmptyDOMElement();
  });

  it('chuyển URL thành liên kết an toàn và bỏ dấu câu cuối khỏi URL', () => {
    render(<RecipeSourceNote instructions={NOTE} />);

    const links = screen.getAllByRole('link');
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      'https://en.wikibooks.org/w/index.php?curid=1&oldid=2',
      'https://creativecommons.org/licenses/by-sa/4.0/',
      'https://commons.wikimedia.org/wiki/File:Soup.jpg',
    ]);
    links.forEach((link) => expect(link).toHaveAttribute('rel', 'nofollow noopener noreferrer'));
    expect(screen.getByRole('complementary', { name: 'Nguồn dữ liệu' })).toHaveTextContent('CC BY-SA 4.0');
  });

  it('giữ nguyên URL có dấu phẩy và ngoặc trong tên file ảnh', () => {
    const url = 'https://commons.wikimedia.org/wiki/File:Ph%E1%BB%9F_b%C3%B2,_C%E1%BA%A7u_Gi%E1%BA%A5y_(1).jpg';
    render(<RecipeSourceNote instructions={`${SOURCE_NOTE_PREFIX} Ảnh: An, CC BY-SA 3.0, ${url}.`} />);

    expect(screen.getByRole('link')).toHaveAttribute('href', url);
  });

  it('không tiêm được HTML và không tạo liên kết từ giao thức khác https', () => {
    const { container } = render(
      <RecipeSourceNote instructions={`${SOURCE_NOTE_PREFIX} <img src=x onerror=alert(1)> javascript:alert(1) http://a.example`} />,
    );

    expect(container.querySelector('img')).toBeNull();
    expect(screen.queryByRole('link')).toBeNull();
    expect(container).toHaveTextContent('<img src=x onerror=alert(1)>');
  });
});
