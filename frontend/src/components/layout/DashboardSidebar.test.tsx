import { render, screen, within } from '@testing-library/react';
import DashboardSidebar, { ADMIN_NAV, AUTHOR_NAV, activeNavHref } from './DashboardSidebar';

let mockPathname = '/dashboard';
jest.mock('next/navigation', () => ({ usePathname: () => mockPathname }));

const ADMIN_LABELS = ADMIN_NAV.map((item) => item.label);

describe('DashboardSidebar', () => {
  it('hides every admin entry from an Author', () => {
    render(<DashboardSidebar roles={['Author']} />);
    const nav = screen.getByRole('navigation', { name: 'Bảng điều khiển' });
    AUTHOR_NAV.forEach((item) =>
      expect(within(nav).getByRole('link', { name: item.label })).toBeInTheDocument(),
    );
    ADMIN_LABELS.forEach((label) =>
      expect(within(nav).queryByRole('link', { name: label })).not.toBeInTheDocument(),
    );
    expect(within(nav).queryByText('Quản trị')).not.toBeInTheDocument();
  });

  it('shows the admin group to an Admin (seeded with both roles)', () => {
    render(<DashboardSidebar roles={['Author', 'Admin']} />);
    const adminList = screen.getByRole('list', { name: 'Quản trị' });
    expect(
      within(adminList)
        .getAllByRole('link')
        .map((link) => link.textContent),
    ).toEqual(ADMIN_LABELS);
  });

  it('marks only the longest matching entry as the current page', () => {
    mockPathname = '/dashboard/recipes/new';
    render(<DashboardSidebar roles={['Author']} />);
    expect(screen.getByRole('link', { name: 'Viết công thức' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(screen.getByRole('link', { name: 'Công thức của tôi' })).not.toHaveAttribute(
      'aria-current',
    );
    expect(screen.getByRole('link', { name: 'Tổng quan' })).not.toHaveAttribute('aria-current');
  });

  it('calls onNavigate when an entry is chosen (closes the phone menu)', () => {
    const onNavigate = jest.fn();
    render(<DashboardSidebar roles={['Author']} onNavigate={onNavigate} />);
    screen.getByRole('link', { name: 'Hồ sơ' }).click();
    expect(onNavigate).toHaveBeenCalledTimes(1);
  });
});

describe('activeNavHref', () => {
  const items = [...AUTHOR_NAV, ...ADMIN_NAV];

  it.each([
    ['/dashboard', '/dashboard'],
    ['/dashboard/recipes/123/edit', '/dashboard/recipes'],
    ['/dashboard/system', '/dashboard/system'],
    ['/profile', '/profile'],
    ['/dashboard-old', undefined],
  ])('%s → %s', (pathname, expected) => {
    expect(activeNavHref(pathname, items)).toBe(expected);
  });
});
