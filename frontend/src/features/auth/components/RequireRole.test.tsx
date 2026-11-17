import { render, screen } from '@testing-library/react';
import { RequireRole } from './RequireRole';

const mockUseAuth = jest.fn();
jest.mock('../auth-context', () => ({ useAuth: () => mockUseAuth() }));

describe('RequireRole', () => {
  it('renders the real 403 UI when an Author requires Admin', () => {
    mockUseAuth.mockReturnValue({ user: { roles: ['Author'] } });
    render(<RequireRole roles={['Admin']}>admin-only</RequireRole>);
    expect(screen.getByRole('heading', { name: 'Không có quyền truy cập' })).toBeInTheDocument();
    expect(screen.queryByText('admin-only')).not.toBeInTheDocument();
  });

  it('renders its children for an Admin', () => {
    mockUseAuth.mockReturnValue({ user: { roles: ['Admin'] } });
    render(<RequireRole roles={['Admin']}>admin-only</RequireRole>);
    expect(screen.getByText('admin-only')).toBeInTheDocument();
  });
});
