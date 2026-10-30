import { render, screen } from '@testing-library/react';
import LoginForm from './LoginForm';

jest.mock('next/navigation', () => ({ useRouter: () => ({ replace: jest.fn() }), useSearchParams: () => new URLSearchParams() }));
jest.mock('../auth-context', () => ({ useAuth: () => ({ login: jest.fn() }) }));
jest.mock('./GoogleSignIn', () => () => null);
jest.mock('sonner', () => ({ toast: { success: jest.fn() } }));

describe('LoginForm', () => {
  it('links users to forgot-password', () => {
    render(<LoginForm />);
    expect(screen.getByRole('link', { name: 'Quên mật khẩu?' })).toHaveAttribute('href', '/auth/forgot-password');
  });
});
