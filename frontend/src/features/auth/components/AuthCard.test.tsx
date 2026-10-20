import { render } from '@testing-library/react';
import AuthCard from './AuthCard';

const mockReplace = jest.fn();
jest.mock('next/navigation', () => ({ useRouter: () => ({ replace: mockReplace }) }));
jest.mock('../auth-context', () => ({ useAuth: () => ({ isReady: true, isAuthenticated: true }) }));

beforeEach(() => mockReplace.mockClear());

/** Người đã đăng nhập mở trang đăng nhập (kể cả ngay sau khi vừa đăng nhập xong) — phải về đúng callbackUrl. */
describe('AuthCard redirect for a signed-in user', () => {
  it.each([
    ['/auth/login?callbackUrl=%2Fdashboard%2Fsystem%3Ftab%3Djobs', '/dashboard/system?tab=jobs'],
    ['/auth/login', '/recipes'],
    ['/auth/login?callbackUrl=%2F%2Fevil.com', '/recipes'],
  ])('%s → %s', (url, expected) => {
    window.history.replaceState(null, '', url);
    render(
      <AuthCard title="Đăng nhập" subtitle="">
        form
      </AuthCard>,
    );
    expect(mockReplace).toHaveBeenCalledWith(expected);
  });
});
