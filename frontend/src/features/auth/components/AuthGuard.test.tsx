import { render } from '@testing-library/react';
import { AuthGuard } from './AuthGuard';

const mockReplace = jest.fn();
const mockUseAuth = jest.fn();

jest.mock('next/navigation', () => ({
  usePathname: () => '/dashboard/users',
  useRouter: () => ({ replace: mockReplace }),
}));
jest.mock('../auth-context', () => ({ useAuth: () => mockUseAuth() }));

describe('AuthGuard', () => {
  beforeEach(() => {
    mockReplace.mockClear();
    mockUseAuth.mockReturnValue({ isReady: true, isAuthenticated: false });
  });

  it('redirects a guest from /dashboard/users to the encoded login callback', () => {
    render(<AuthGuard>protected</AuthGuard>);
    expect(mockReplace).toHaveBeenCalledWith('/auth/login?callbackUrl=%2Fdashboard%2Fusers');
  });
});
