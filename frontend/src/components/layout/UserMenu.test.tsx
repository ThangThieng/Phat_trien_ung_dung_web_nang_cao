import { fireEvent, render, screen } from '@testing-library/react';
import UserMenu, { initialsOf } from './UserMenu';

jest.mock('next/navigation', () => ({ usePathname: () => '/' }));
jest.mock('sonner', () => ({ toast: { info: jest.fn() } }));

type MockAuth = {
  isReady: boolean;
  user: { displayName: string; email: string; roles: string[] } | null;
  logout: jest.Mock;
};
let mockAuth: MockAuth;
jest.mock('@/features/auth/auth-context', () => ({ useAuth: () => mockAuth }));

const author = { displayName: 'Nguyễn Văn Lan', email: 'lan@example.com', roles: ['Author'] };
const admin = { displayName: 'Quản Trị', email: 'admin@example.com', roles: ['Author', 'Admin'] };

describe('initialsOf', () => {
  it('lấy chữ đầu của từ đầu và từ cuối', () => {
    expect(initialsOf('Nguyễn Văn Lan')).toBe('NL');
    expect(initialsOf('lan')).toBe('L');
    expect(initialsOf('  ')).toBe('?');
  });
});

describe('<UserMenu />', () => {
  it('khách: link Đăng nhập và Đăng ký', () => {
    mockAuth = { isReady: true, user: null, logout: jest.fn() };
    render(<UserMenu />);
    expect(screen.getByRole('link', { name: 'Đăng nhập' })).toHaveAttribute('href', '/auth/login');
    expect(screen.getByRole('link', { name: 'Đăng ký' })).toHaveAttribute('href', '/auth/register');
  });

  it('Author: menu có Hồ sơ, Dashboard, Đăng xuất — không có mục Admin', () => {
    mockAuth = { isReady: true, user: author, logout: jest.fn() };
    render(<UserMenu />);
    const trigger = screen.getByRole('button', { name: /Tài khoản: Nguyễn Văn Lan/ });
    expect(trigger).toHaveAttribute('aria-haspopup', 'menu');
    expect(trigger).toHaveAttribute('aria-expanded', 'false');
    fireEvent.click(trigger);
    expect(trigger).toHaveAttribute('aria-expanded', 'true');
    const names = screen.getAllByRole('menuitem').map((item) => item.textContent);
    expect(names).toEqual(['Hồ sơ', 'Dashboard', 'Đăng xuất']);
    expect(screen.getAllByRole('menuitem')[0]).toHaveFocus();
  });

  it('Admin: thêm Quản lý danh mục và Người dùng', () => {
    mockAuth = { isReady: true, user: admin, logout: jest.fn() };
    render(<UserMenu />);
    fireEvent.click(screen.getByRole('button', { name: /Tài khoản/ }));
    expect(screen.getByRole('menuitem', { name: 'Quản lý danh mục' })).toHaveAttribute(
      'href',
      '/dashboard/categories',
    );
    expect(screen.getByRole('menuitem', { name: 'Người dùng' })).toHaveAttribute(
      'href',
      '/dashboard/users',
    );
    expect(screen.getByRole('menuitem', { name: 'Thư viện ảnh' })).toHaveAttribute(
      'href',
      '/dashboard/media',
    );
  });

  it('bàn phím: mũi tên xuống/lên di chuyển, Escape đóng và trả focus về nút', () => {
    mockAuth = { isReady: true, user: author, logout: jest.fn() };
    render(<UserMenu />);
    const trigger = screen.getByRole('button', { name: /Tài khoản/ });
    fireEvent.click(trigger);
    const menu = screen.getByRole('menu');
    const items = screen.getAllByRole('menuitem');
    fireEvent.keyDown(menu, { key: 'ArrowDown' });
    expect(items[1]).toHaveFocus();
    fireEvent.keyDown(menu, { key: 'ArrowUp' });
    fireEvent.keyDown(menu, { key: 'ArrowUp' });
    expect(items[items.length - 1]).toHaveFocus();
    fireEvent.keyDown(menu, { key: 'Escape' });
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
    expect(trigger).toHaveFocus();
  });

  it('Đăng xuất gọi logout()', async () => {
    const logout = jest.fn().mockResolvedValue(undefined);
    mockAuth = { isReady: true, user: author, logout };
    render(<UserMenu />);
    fireEvent.click(screen.getByRole('button', { name: /Tài khoản/ }));
    fireEvent.click(screen.getByRole('menuitem', { name: 'Đăng xuất' }));
    expect(logout).toHaveBeenCalledTimes(1);
  });

  it('đang khôi phục phiên: ô giữ chỗ, không nhấp nháy nút đăng nhập', () => {
    mockAuth = { isReady: false, user: null, logout: jest.fn() };
    render(<UserMenu />);
    expect(screen.queryByRole('link', { name: 'Đăng nhập' })).not.toBeInTheDocument();
  });
});
