import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import ForgotPasswordForm from './ForgotPasswordForm';

const apiFetch = jest.fn();
jest.mock('@/lib/api-client', () => ({ apiFetch: (...a: unknown[]) => apiFetch(...a), getErrorMessage: () => 'Lỗi API' }));
describe('ForgotPasswordForm', () => { beforeEach(() => apiFetch.mockReset()); it('rejects invalid email without API call', async () => { render(<ForgotPasswordForm />); fireEvent.click(screen.getByText('Gửi liên kết đặt lại')); expect(await screen.findByText('Email không được để trống.')).toBeInTheDocument(); expect(apiFetch).not.toHaveBeenCalled(); }); it('posts a valid email and shows generic success', async () => { apiFetch.mockResolvedValue({}); render(<ForgotPasswordForm />); fireEvent.change(screen.getByLabelText('Email'), {target:{value:'a@test.com'}}); fireEvent.click(screen.getByText('Gửi liên kết đặt lại')); await waitFor(() => expect(apiFetch).toHaveBeenCalledWith('/auth/forgot-password', {method:'POST',body:{email:'a@test.com'},accessToken:null})); expect(await screen.findByRole('status')).toHaveTextContent('Nếu email hợp lệ'); }); });
