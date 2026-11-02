import { useState } from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from './interim-ui';

function SheetDemo() {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button type="button" onClick={() => setOpen(true)}>
        Mở
      </button>
      <Sheet open={open} onOpenChange={setOpen}>
        <SheetContent side="left">
          <SheetHeader>
            <SheetTitle>Bộ lọc</SheetTitle>
          </SheetHeader>
          <form>
            <input type="hidden" name="q" value="ga" />
            <a href="/search?q=ga&difficulty=Easy">Dễ</a>
            <a href="/search?q=ga&difficulty=Hard">Khó</a>
          </form>
        </SheetContent>
      </Sheet>
    </>
  );
}

describe('Sheet (bản tạm của components/ui/sheet)', () => {
  it('dialog có tên theo SheetTitle, focus vào phần tử đầu (bỏ input ẩn), khóa cuộn nền', () => {
    render(<SheetDemo />);
    fireEvent.click(screen.getByRole('button', { name: 'Mở' }));
    const dialog = screen.getByRole('dialog', { name: 'Bộ lọc' });
    expect(dialog).toHaveAttribute('aria-modal', 'true');
    expect(screen.getByRole('link', { name: 'Dễ' })).toHaveFocus();
    expect(document.body.style.overflow).toBe('hidden');
  });

  it('Tab ở phần tử cuối quay về đầu, Shift+Tab ở đầu sang cuối', () => {
    render(<SheetDemo />);
    fireEvent.click(screen.getByRole('button', { name: 'Mở' }));
    const close = screen.getByRole('button', { name: 'Đóng' });
    close.focus();
    fireEvent.keyDown(document, { key: 'Tab' });
    expect(screen.getByRole('link', { name: 'Dễ' })).toHaveFocus();
    fireEvent.keyDown(document, { key: 'Tab', shiftKey: true });
    expect(close).toHaveFocus();
  });

  it('bấm nền hoặc nút Đóng → đóng, trả focus về nút đã mở, mở khóa cuộn', () => {
    render(<SheetDemo />);
    const opener = screen.getByRole('button', { name: 'Mở' });
    opener.focus();
    fireEvent.click(opener);
    fireEvent.click(document.querySelector('[data-slot="sheet-overlay"]')!);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(opener).toHaveFocus();
    expect(document.body.style.overflow).toBe('');

    fireEvent.click(opener);
    fireEvent.click(screen.getByRole('button', { name: 'Đóng' }));
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});

describe('Select (bản tạm của components/ui/select)', () => {
  it('dựng <select> gốc có nhãn, name (gửi được trong form GET) và báo giá trị qua onValueChange', () => {
    const onValueChange = jest.fn();
    render(
      <>
        <span id="lbl">Danh mục</span>
        <Select name="categoryId" value="all" onValueChange={onValueChange}>
          <SelectTrigger id="cat" aria-labelledby="lbl">
            <SelectValue placeholder="Tất cả danh mục" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Tất cả danh mục</SelectItem>
            {['Món Việt', 'Món Âu'].map((name) => (
              <SelectItem key={name} value={name}>
                {name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </>,
    );
    const select = screen.getByRole('combobox', { name: 'Danh mục' });
    expect(select).toHaveAttribute('name', 'categoryId');
    expect(select).toHaveAttribute('id', 'cat');
    expect(Array.from((select as HTMLSelectElement).options).map((o) => o.text)).toEqual([
      'Tất cả danh mục',
      'Món Việt',
      'Món Âu',
    ]);
    fireEvent.change(select, { target: { value: 'Món Âu' } });
    expect(onValueChange).toHaveBeenCalledWith('Món Âu');
  });
});
