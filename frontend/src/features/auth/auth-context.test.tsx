import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { StrictMode } from 'react';
import { apiFetch } from '@/lib/api-client';
import { AuthProvider, useAuth } from './auth-context';

jest.mock('@/lib/api-client', () => ({ apiFetch: jest.fn(), configureAuthCallbacks: jest.fn() }));
jest.mock('@/lib/query-client', () => ({ getQueryClient: () => ({ clear: jest.fn() }) }));

const session = {
  accessToken: 'memory-only-access-token',
  refreshToken: 'persistent-refresh-token',
  expiresAt: '2026-10-05T12:00:00Z',
  user: { id: 'author', email: 'author@example.com', displayName: 'Author', roles: ['Author'] },
};

function Consumer() {
  const auth = useAuth();
  return (
    <>
      <span>{auth.isReady ? auth.accessToken ?? 'signed-out' : 'loading'}</span>
      <button type="button" onClick={() => auth.login('author@example.com', 'password')}>Login</button>
    </>
  );
}

describe('access token storage', () => {
  beforeEach(() => {
    window.localStorage.clear();
    jest.mocked(apiFetch).mockReset();
    jest.mocked(apiFetch).mockResolvedValue(session);
  });

  it('keeps the access token in memory while persisting the refresh token', async () => {
    render(<AuthProvider><Consumer /></AuthProvider>);
    fireEvent.click(screen.getByRole('button', { name: 'Login' }));
    await screen.findByText(session.accessToken);
    const stored = JSON.parse(window.localStorage.getItem('culinaryblog.auth')!);
    expect(stored.refreshToken).toBe(session.refreshToken);
    expect(stored).not.toHaveProperty('accessToken');
    expect(stored).not.toHaveProperty('expiresAt');
  });

  it('refreshes once on reload even with StrictMode and removes a legacy persisted access token', async () => {
    window.localStorage.setItem('culinaryblog.auth', JSON.stringify(session));
    render(<StrictMode><AuthProvider><Consumer /></AuthProvider></StrictMode>);
    await screen.findByText(session.accessToken);
    await waitFor(() => expect(apiFetch).toHaveBeenCalledTimes(1));
    expect(apiFetch).toHaveBeenCalledWith('/auth/refresh', expect.objectContaining({
      body: { refreshToken: session.refreshToken }, accessToken: null, skipAuthRefresh: true,
    }));
    expect(JSON.parse(window.localStorage.getItem('culinaryblog.auth')!)).not.toHaveProperty('accessToken');
  });
});
