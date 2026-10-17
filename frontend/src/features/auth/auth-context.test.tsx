import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { StrictMode } from 'react';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
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

  afterEach(() => { jest.restoreAllMocks(); });

  it.each([false, true])('rereads storage after acquiring the cross-tab lock (logout: %s)', async (logout) => {
    const descriptor = Object.getOwnPropertyDescriptor(navigator, 'locks');
    let release!: () => void;
    const gate = new Promise<void>((resolve) => { release = resolve; });
    const request = jest.fn(async (_name: string, callback: () => Promise<string>) => {
      await gate;
      return callback();
    });
    Object.defineProperty(navigator, 'locks', { configurable: true, value: { request } });
    try {
      render(<AuthProvider><Consumer /></AuthProvider>);
      fireEvent.click(screen.getByRole('button', { name: 'Login' }));
      await screen.findByText(session.accessToken);
      const { calls } = jest.mocked(configureAuthCallbacks).mock;
      const [callbacks] = calls[calls.length - 1];
      const pending = callbacks!.refresh();
      const result = pending.then(() => 'refreshed', () => 'rejected');
      expect(request).toHaveBeenCalledWith('culinaryblog-auth-refresh', expect.any(Function));
      expect(apiFetch).toHaveBeenCalledTimes(1);
      if (logout) {
        window.localStorage.removeItem('culinaryblog.auth');
      } else {
        window.localStorage.setItem('culinaryblog.auth', JSON.stringify({ refreshToken: 'R2', user: session.user }));
        jest.mocked(apiFetch).mockResolvedValue({ ...session, refreshToken: 'R3' });
      }
      await act(async () => {
        release();
        expect(await result).toBe(logout ? 'rejected' : 'refreshed');
      });
      if (logout) {
        expect(apiFetch).toHaveBeenCalledTimes(1);
        expect(window.localStorage.getItem('culinaryblog.auth')).toBeNull();
        expect(screen.getByText('signed-out')).toBeInTheDocument();
      } else {
        expect(apiFetch).toHaveBeenLastCalledWith('/auth/refresh', expect.objectContaining({ body: { refreshToken: 'R2' } }));
        const stored = JSON.parse(window.localStorage.getItem('culinaryblog.auth')!);
        expect(stored.refreshToken).toBe('R3');
        expect(stored).not.toHaveProperty('accessToken');
      }
    } finally {
      release();
      if (descriptor) Object.defineProperty(navigator, 'locks', descriptor);
      else Reflect.deleteProperty(navigator, 'locks');
    }
  });

  it('refreshes from memory when localStorage reads and writes are blocked', async () => {
    jest.spyOn(Storage.prototype, 'getItem').mockImplementation(() => { throw new DOMException('blocked', 'SecurityError'); });
    jest.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new DOMException('blocked', 'SecurityError'); });
    render(<AuthProvider><Consumer /></AuthProvider>);
    fireEvent.click(screen.getByRole('button', { name: 'Login' }));
    await screen.findByText(session.accessToken);
    const { calls } = jest.mocked(configureAuthCallbacks).mock;
    const [callbacks] = calls[calls.length - 1];
    await act(async () => { await callbacks!.refresh(); });
    expect(apiFetch).toHaveBeenLastCalledWith('/auth/refresh', expect.objectContaining({ body: { refreshToken: session.refreshToken } }));
    expect(callbacks!.getAccessToken()).toBe(session.accessToken);
  });

  it.each(['', '{broken', 'null', '{}', JSON.stringify({ refreshToken: 42, user: session.user })])('cleans malformed stored session %s gracefully', async (raw) => {
    window.localStorage.setItem('culinaryblog.auth', raw);
    render(<AuthProvider><Consumer /></AuthProvider>);
    await screen.findByText('signed-out');
    expect(window.localStorage.getItem('culinaryblog.auth')).toBeNull();
    expect(apiFetch).not.toHaveBeenCalled();
  });

  it('synchronizes logout from another tab without starting refresh', async () => {
    render(<AuthProvider><Consumer /></AuthProvider>);
    fireEvent.click(screen.getByRole('button', { name: 'Login' }));
    await screen.findByText(session.accessToken);
    act(() => { window.dispatchEvent(new StorageEvent('storage', { key: 'culinaryblog.auth', newValue: null })); });
    await screen.findByText('signed-out');
    expect(window.localStorage.getItem('culinaryblog.auth')).toBeNull();
    expect(apiFetch).toHaveBeenCalledTimes(1);
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
