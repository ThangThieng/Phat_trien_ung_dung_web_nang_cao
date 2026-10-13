import { fireEvent, render, screen } from '@testing-library/react';
import DashboardShell from './DashboardShell';

const mockReplace = jest.fn();
let mockPathname = '/dashboard/system';
jest.mock('next/navigation', () => ({
  usePathname: () => mockPathname,
  useRouter: () => ({ replace: mockReplace }),
}));

type MockAuth = {
  isReady: boolean;
  isAuthenticated: boolean;
  user: { displayName: string; email: string; roles: string[] } | null;
  logout: jest.Mock;
};
let mockAuth: MockAuth;
jest.mock('@/features/auth/auth-context', () => ({ useAuth: () => mockAuth }));
jest.mock('@/features/system/components/HealthIndicator', () => ({
  __esModule: true,
  default: () => <span>health</span>,
}));

const author = { displayName: 'Lan', email: 'lan@example.com', roles: ['Author'] };

beforeEach(() => {
  mockReplace.mockClear();
  mockPathname = '/dashboard/system';
  window.history.replaceState(null, '', '/dashboard/system?tab=jobs');
});

describe('DashboardShell', () => {
  it('waits while the session is being restored', () => {
    mockAuth = { isReady: false, isAuthenticated: false, user: null, logout: jest.fn() };
    render(<DashboardShell>secret</DashboardShell>);
    expect(screen.queryByText('secret')).not.toBeInTheDocument();
    expect(screen.getByText('Đang kiểm tra phiên đăng nhập…')).toBeInTheDocument();
    expect(mockReplace).not.toHaveBeenCalled();
  });

  it('sends a guest to the login page with the encoded current URL as callbackUrl', () => {
    mockAuth = { isReady: true, isAuthenticated: false, user: null, logout: jest.fn() };
    render(<DashboardShell>secret</DashboardShell>);
    expect(screen.queryByText('secret')).not.toBeInTheDocument();
    expect(mockReplace).toHaveBeenCalledWith(
      `/auth/login?callbackUrl=${encodeURIComponent('/dashboard/system?tab=jobs')}`,
    );
  });

  it('renders the page inside main#main-content with the sidebar for a signed-in user', () => {
    mockAuth = { isReady: true, isAuthenticated: true, user: author, logout: jest.fn() };
    render(<DashboardShell>secret</DashboardShell>);
    expect(screen.getByRole('main')).toHaveAttribute('id', 'main-content');
    expect(screen.getByRole('main')).toHaveTextContent('secret');
    expect(screen.getByText('Lan')).toBeInTheDocument();
    expect(screen.queryByText('health')).not.toBeInTheDocument();
  });

  it('opens and closes the phone menu with a button that reports its state', () => {
    mockAuth = { isReady: true, isAuthenticated: true, user: author, logout: jest.fn() };
    render(<DashboardShell>secret</DashboardShell>);
    const toggle = screen.getByRole('button', { name: 'Mở menu' });
    expect(toggle).toHaveAttribute('aria-expanded', 'false');
    fireEvent.click(toggle);
    expect(screen.getByRole('button', { name: 'Đóng menu' })).toHaveAttribute(
      'aria-expanded',
      'true',
    );
    expect(screen.getAllByRole('navigation', { name: 'Bảng điều khiển' })).toHaveLength(2);
  });

  it('shows the health indicator to an Admin only', () => {
    mockAuth = {
      isReady: true,
      isAuthenticated: true,
      user: { ...author, roles: ['Author', 'Admin'] },
      logout: jest.fn(),
    };
    render(<DashboardShell>secret</DashboardShell>);
    expect(screen.getByText('health')).toBeInTheDocument();
  });
});
