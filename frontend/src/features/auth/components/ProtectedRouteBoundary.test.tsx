import { render, screen } from '@testing-library/react';
import { useAuth } from '@/features/auth/auth-context';
import ProtectedRouteBoundary from './ProtectedRouteBoundary';

const mockReplace = jest.fn();
let mockPath = '/profile';
jest.mock('next/navigation', () => ({
  usePathname: () => mockPath,
  useRouter: () => ({ replace: mockReplace }),
}));
jest.mock('@/features/auth/auth-context', () => ({ useAuth: jest.fn() }));
const mockAuth = jest.mocked(useAuth);

function session(ready: boolean, roles: string[] | null) {
  mockAuth.mockReturnValue({
    user: roles ? { id: 'user', email: 'user@example.com', displayName: 'User', avatarUrl: null, roles } : null,
    accessToken: roles ? 'memory-only' : null,
    isReady: ready,
    isAuthenticated: roles !== null,
    login: jest.fn(), loginWithGoogle: jest.fn(), register: jest.fn(), logout: jest.fn(),
  });
}

beforeEach(() => { mockReplace.mockClear(); mockPath = '/profile'; });

test.each(['/profile', '/dashboard/media', '/dashboard/recipes/edit', '/dashboard/categories', '/dashboard/users'])('guest cannot render protected content at %s', (path) => {
  mockPath = path;
  session(true, null);
  render(<ProtectedRouteBoundary><div>Protected content</div></ProtectedRouteBoundary>);
  expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  expect(mockReplace).toHaveBeenCalledWith(`/auth/login?callbackUrl=${encodeURIComponent(path)}`);
});

test('hydration never flashes protected children or redirects before auth is ready', () => {
  session(false, null);
  render(<ProtectedRouteBoundary><div>Protected content</div></ProtectedRouteBoundary>);
  expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  expect(mockReplace).not.toHaveBeenCalled();
});

test.each(['/dashboard/categories', '/dashboard/categories/edit', '/dashboard/users'])('Author cannot render Admin UI at %s', (path) => {
  mockPath = path;
  session(true, ['Author']);
  render(<ProtectedRouteBoundary><div>Admin content</div></ProtectedRouteBoundary>);
  expect(screen.queryByText('Admin content')).not.toBeInTheDocument();
  expect(screen.getByRole('alert')).toHaveTextContent('không có quyền');
});

test.each(['/dashboard/categories', '/dashboard/users', '/dashboard/media', '/profile'])('Admin can render protected UI at %s', (path) => {
  mockPath = path;
  session(true, ['Admin']);
  render(<ProtectedRouteBoundary><div>Allowed content</div></ProtectedRouteBoundary>);
  expect(screen.getByText('Allowed content')).toBeInTheDocument();
});

test('Author can render their dashboard', () => {
  mockPath = '/dashboard/media';
  session(true, ['Author']);
  render(<ProtectedRouteBoundary><div>Author content</div></ProtectedRouteBoundary>);
  expect(screen.getByText('Author content')).toBeInTheDocument();
});

test('public pages and similarly named paths remain public', () => {
  mockPath = '/dashboard-public';
  session(true, null);
  render(<ProtectedRouteBoundary><div>Public content</div></ProtectedRouteBoundary>);
  expect(screen.getByText('Public content')).toBeInTheDocument();
});
