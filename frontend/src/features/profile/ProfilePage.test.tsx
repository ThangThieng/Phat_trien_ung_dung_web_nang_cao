import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ProfilePage } from './ProfilePage';

const apiFetch = jest.fn(); const logout = jest.fn(); const replace = jest.fn();
jest.mock('@/lib/api-client', () => ({ apiFetch: (...a: unknown[]) => apiFetch(...a), getErrorMessage: () => 'Lỗi API', mapProblemDetailsToForm: () => false }));
jest.mock('@/features/auth/auth-context', () => ({ useAuth: () => ({ logout, accessToken: 'token' }) }));
jest.mock('next/navigation', () => ({ useRouter: () => ({ replace }) }));
jest.mock('@/components/shared/ImageUploader', () => {
 function MockImageUploader({ onChange }: {onChange:(v: unknown)=>void}) { return <button type="button" onClick={() => onChange({ key: 'avatars/new-file', url: 'https://example/new-avatar' })}>Upload avatar</button>; }
 return MockImageUploader;
});
const user = { displayName: 'Quân', bio: 'Bio', avatarUrl: 'https://example/old-avatar', roles: [], id: 'u', email: 'u@test' };
const session = { id: 'other', createdAt: '2026-01-01T00:00:00Z', createdByIp: '127.0.0.1', expiresAt: '2026-02-01T00:00:00Z', isCurrent: false };
describe('ProfilePage', () => {
 const defaultResponse = (path: string) => { if (path === '/auth/me') return Promise.resolve(user); if (path === '/auth/sessions') return Promise.resolve([session]); return Promise.resolve({}); };
 beforeEach(() => { apiFetch.mockReset(); logout.mockReset(); replace.mockReset(); apiFetch.mockImplementation(defaultResponse); });
 async function ready(){ render(<ProfilePage />); await screen.findByDisplayValue('Quân'); }
 it('loads profile data without an act warning', async () => { await ready(); expect(screen.getByDisplayValue('Bio')).toBeInTheDocument(); });
 it('patches edited profile fields', async () => { await ready(); fireEvent.change(screen.getByLabelText('Tên hiển thị'), { target: { value: 'Quân mới' } }); fireEvent.click(screen.getByText('Lưu thay đổi')); await waitFor(() => expect(apiFetch).toHaveBeenCalledWith('/auth/me', expect.objectContaining({ method: 'PATCH', body: expect.objectContaining({ displayName: 'Quân mới' }) }))); });
 it('uses uploaded avatar url in PATCH', async () => { await ready(); fireEvent.click(screen.getByText('Upload avatar')); await waitFor(() => expect(screen.getByText('Upload avatar')).toBeInTheDocument()); fireEvent.click(screen.getByText('Lưu thay đổi')); await waitFor(() => expect(apiFetch).toHaveBeenCalledWith('/auth/me', expect.objectContaining({ body: expect.objectContaining({ avatarUrl: 'https://example/new-avatar' }) }))); });
 it('rolls back the new uploaded key when PATCH fails', async () => { apiFetch.mockImplementation((path:string, options?:{method?:string}) => { if (path === '/auth/me' && options?.method === 'PATCH') return Promise.reject(new Error()); if (path === '/auth/me') return Promise.resolve(user); return Promise.resolve({}); }); await ready(); fireEvent.click(screen.getByText('Upload avatar')); await waitFor(() => expect(screen.getByText('Upload avatar')).toBeInTheDocument()); fireEvent.click(screen.getByText('Lưu thay đổi')); await waitFor(() => expect(apiFetch).toHaveBeenCalledWith('/files/avatars/new-file', { method: 'DELETE' })); });
 it('logs out and redirects after changing password', async () => { await ready(); fireEvent.click(screen.getByText('Bảo mật')); fireEvent.change(screen.getByLabelText('Mật khẩu hiện tại'), {target:{value:'Old#12345'}}); fireEvent.change(screen.getByLabelText('Mật khẩu mới'), {target:{value:'New#12345'}}); fireEvent.change(screen.getByLabelText('Xác nhận mật khẩu mới'), {target:{value:'New#12345'}}); fireEvent.click(screen.getByText('Đổi mật khẩu')); await waitFor(() => expect(logout).toHaveBeenCalled()); expect(replace).toHaveBeenCalledWith('/auth/login'); });
 it('lists and revokes another session', async () => { await ready(); fireEvent.click(screen.getByText('Phiên')); expect(await screen.findByText(/127.0.0.1/)).toBeInTheDocument(); fireEvent.click(screen.getByText('Thu hồi')); await waitFor(() => expect(apiFetch).toHaveBeenCalledWith('/auth/sessions/other', {method:'DELETE'})); });
 it('revoke all logs out and redirects', async () => { await ready(); fireEvent.click(screen.getByText('Phiên')); await screen.findByText(/127.0.0.1/); fireEvent.click(screen.getByText('Thu hồi tất cả')); await waitFor(() => expect(logout).toHaveBeenCalled()); expect(replace).toHaveBeenCalledWith('/auth/login'); });
});
