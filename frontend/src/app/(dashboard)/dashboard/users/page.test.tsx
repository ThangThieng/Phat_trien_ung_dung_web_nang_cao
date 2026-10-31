import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { PagedResult } from '@/types/api';
import type { UserAdmin } from '@/types/auth';
import UsersPage from './page';

const getUsers = jest.fn();
const setUserStatus = jest.fn();
const requireRole = jest.fn();
const toastError = jest.fn();

jest.mock('@/features/users/api', () => ({ getUsers: (...args: unknown[]) => getUsers(...args), setUserStatus: (...args: unknown[]) => setUserStatus(...args) }));
jest.mock('@/features/auth/auth-context', () => ({ useAuth: () => ({ user: { id: 'admin-1' }, accessToken: 'access-token' }) }));
jest.mock('@/features/auth/components/RequireRole', () => ({ RequireRole: ({ roles, children }: { roles: string[]; children: React.ReactNode }) => { requireRole(roles); return children; } }));
jest.mock('sonner', () => ({ toast: { success: jest.fn(), error: (...args: unknown[]) => toastError(...args) } }));
jest.mock('@/lib/api-client', () => ({ getErrorMessage: () => 'Không thể cập nhật người dùng.' }));

const admin: UserAdmin = { id: 'admin-1', displayName: 'Quản trị viên', email: 'admin@example.test', avatarUrl: 'https://cdn.example/admin.png', roles: ['Admin'], isActive: true, createdAt: '2026-01-01T00:00:00.000Z', recipeCount: 9 };
const member: UserAdmin = { id: 'user-2', displayName: 'Người dùng', email: 'user@example.test', avatarUrl: null, roles: ['Author'], isActive: true, createdAt: '2026-02-02T00:00:00.000Z', recipeCount: 3 };
const locked: UserAdmin = { ...member, id: 'user-3', displayName: 'Người dùng bị khóa', email: 'locked@example.test', isActive: false };
const pageResult: PagedResult<UserAdmin> = { items: [admin, member, locked], totalCount: 3, page: 1, pageSize: 12, totalPages: 2, hasNextPage: true, hasPreviousPage: false };

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  return render(<QueryClientProvider client={client}><UsersPage /></QueryClientProvider>);
}

async function loaded() {
  renderPage();
  await screen.findByText('user@example.test');
}

function confirmButton() { return screen.getByRole('button', { name: 'Xác nhận' }); }

describe('Admin users page', () => {
  beforeEach(() => {
    jest.useFakeTimers();
    getUsers.mockReset().mockResolvedValue(pageResult);
    setUserStatus.mockReset().mockResolvedValue({ ...member, isActive: false });
    requireRole.mockReset(); toastError.mockReset();
  });
  afterEach(() => jest.useRealTimers());

  it('renders the required user table fields, avatar and fallback', async () => {
    await loaded();
    expect(screen.getByText('admin@example.test')).toBeInTheDocument();
    expect(screen.getAllByText('Author')).toHaveLength(2);
    expect(screen.getAllByText('Đang hoạt động')).toHaveLength(3);
    expect(screen.getAllByText('Đã khóa')).toHaveLength(2);
    expect(screen.getAllByText('3')).toHaveLength(2);
    expect(document.querySelector('img[src="https://cdn.example/admin.png"]')).toBeInTheDocument();
    expect(screen.getByLabelText('Ảnh đại diện mặc định của Người dùng')).toHaveTextContent('N');
    expect(screen.getAllByText(new Date(member.createdAt).toLocaleDateString('vi-VN'))).toHaveLength(2);
  });

  it('debounces search for 300ms and sends the search query', async () => {
    await loaded(); getUsers.mockClear();
    fireEvent.change(screen.getByLabelText('Tìm kiếm người dùng'), { target: { value: 'quan' } });
    await act(async () => { jest.advanceTimersByTime(299); });
    expect(getUsers).not.toHaveBeenCalled();
    await act(async () => { jest.advanceTimersByTime(1); });
    await waitFor(() => expect(getUsers).toHaveBeenCalledWith(expect.objectContaining({ page: 1, search: 'quan' }), expect.anything()));
  });

  it.each([['true', true], ['false', false]] as const)('applies status filter %s', async (value, expected) => {
    await loaded(); getUsers.mockClear();
    fireEvent.change(screen.getByLabelText('Trạng thái'), { target: { value } });
    await waitFor(() => expect(getUsers).toHaveBeenCalledWith(expect.objectContaining({ page: 1, isActive: expected }), expect.anything()));
  });

  it('removes the status filter when All is selected', async () => {
    await loaded();
    fireEvent.change(screen.getByLabelText('Trạng thái'), { target: { value: 'true' } });
    await waitFor(() => expect(getUsers).toHaveBeenCalledWith(expect.objectContaining({ isActive: true }), expect.anything()));
    getUsers.mockClear();
    fireEvent.change(screen.getByLabelText('Trạng thái'), { target: { value: 'all' } });
    await waitFor(() => expect(getUsers).toHaveBeenCalledWith(expect.objectContaining({ page: 1, isActive: undefined }), expect.anything()));
  });

  it('preserves search and filter in pagination links', async () => {
    await loaded();
    fireEvent.change(screen.getByLabelText('Trạng thái'), { target: { value: 'false' } });
    fireEvent.change(screen.getByLabelText('Tìm kiếm người dùng'), { target: { value: 'locked' } });
    await act(async () => { jest.advanceTimersByTime(300); });
    await waitFor(() => expect(screen.getByRole('link', { name: 'Trang 2' })).toHaveAttribute('href', '?page=2&pageSize=12&search=locked&isActive=false'));
  });

  it('uses the Admin guard, has no role editing UI, and hides self lock', async () => {
    await loaded();
    expect(requireRole).toHaveBeenCalledWith(['Admin']);
    expect(screen.queryByRole('button', { name: /vai trò|role/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^Khóa$/ })).toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: /^Khóa$/ })).toHaveLength(1);
  });

  it('validates lock reason, optimistically locks, and sends the PATCH payload', async () => {
    let resolveMutation: ((value: unknown) => void) | undefined;
    setUserStatus.mockImplementation(() => new Promise((resolve) => { resolveMutation = resolve; }));
    await loaded();
    fireEvent.click(screen.getByRole('button', { name: 'Khóa' }));
    expect(screen.getByRole('alertdialog')).toBeInTheDocument();
    expect(confirmButton()).toBeDisabled();
    fireEvent.change(screen.getByRole('textbox', { name: 'Lý do' }), { target: { value: 'abc' } });
    expect(confirmButton()).toBeDisabled();
    fireEvent.change(screen.getByRole('textbox', { name: 'Lý do' }), { target: { value: 'x'.repeat(501) } });
    expect(confirmButton()).toBeDisabled();
    fireEvent.change(screen.getByRole('textbox', { name: 'Lý do' }), { target: { value: 'Lý do hợp lệ' } });
    fireEvent.click(confirmButton());
    await waitFor(() => expect(setUserStatus).toHaveBeenCalledWith('user-2', { isActive: false, reason: 'Lý do hợp lệ' }, 'access-token'));
    expect(screen.getAllByText('Đã khóa')).toHaveLength(3);
    await act(async () => { resolveMutation?.({}); });
  });

  it('unlocks with the same validated payload', async () => {
    await loaded();
    fireEvent.click(screen.getByRole('button', { name: 'Mở khóa' }));
    expect(confirmButton()).toBeDisabled();
    fireEvent.change(screen.getByRole('textbox', { name: 'Lý do' }), { target: { value: 'Mở lại tài khoản' } });
    fireEvent.click(confirmButton());
    await waitFor(() => expect(setUserStatus).toHaveBeenCalledWith('user-3', { isActive: true, reason: 'Mở lại tài khoản' }, 'access-token'));
  });

  it('rolls back rejected optimistic status and shows visible feedback', async () => {
    setUserStatus.mockRejectedValue(new Error('rejected'));
    await loaded();
    fireEvent.click(screen.getByRole('button', { name: 'Khóa' }));
    fireEvent.change(screen.getByRole('textbox', { name: 'Lý do' }), { target: { value: 'Lý do hợp lệ' } });
    fireEvent.click(confirmButton());
    await waitFor(() => expect(toastError).toHaveBeenCalledWith('Không thể cập nhật người dùng.'));
    expect(screen.getAllByText('Đang hoạt động')).toHaveLength(3);
  });
});
