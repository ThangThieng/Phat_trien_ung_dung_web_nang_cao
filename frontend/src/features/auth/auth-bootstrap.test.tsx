import { render, screen, waitFor } from '@testing-library/react';
import { apiFetch } from '@/lib/api-client';
import { AuthStoreProvider, useAuthStore } from '@/stores/auth-store-provider';
import { authNavigation, AuthBootstrap } from './auth-bootstrap';

jest.mock('@/lib/api-client', () => ({
  apiFetch: jest.fn(),
  configureAuthCallbacks: jest.fn(),
}));
jest.mock('@/lib/query-client', () => ({ getQueryClient: () => ({ clear: jest.fn() }) }));

const user = { id: 'author', email: 'author@example.com', displayName: 'Author', roles: ['Author'] };
const persistedSession = { state: { refreshToken: 'test-refresh-token', user }, version: 1 };

function SessionState() {
  const status = useAuthStore((state) => state.status);
  const token = useAuthStore((state) => state.accessToken);
  return <output>{`${status}:${token ?? 'none'}`}</output>;
}

function renderBootstrap() {
  return render(<AuthStoreProvider><AuthBootstrap><SessionState /></AuthBootstrap></AuthStoreProvider>);
}

describe('AuthBootstrap refresh recovery', () => {
  let assign: jest.SpyInstance<void, [string]>;

  beforeEach(() => {
    window.localStorage.clear();
    window.history.replaceState({}, '', '/');
    window.localStorage.setItem('culinaryblog.auth', JSON.stringify(persistedSession));
    jest.mocked(apiFetch).mockReset();
    assign = jest.spyOn(authNavigation, 'redirect').mockImplementation(() => undefined);
  });

  afterEach(() => jest.restoreAllMocks());

  it.each(['/recipes?tag=b%C3%A1nh%20m%C3%AC', '/dashboard/users?filter=active'])('clears the persisted session and redirects after refresh fails on %s', async (route) => {
    window.history.replaceState({}, '', route);
    jest.mocked(apiFetch).mockRejectedValue(new Error('refresh rejected'));

    renderBootstrap();

    await waitFor(() => expect(screen.getByText('anonymous:none')).toBeInTheDocument());
    const callbackUrl = encodeURIComponent(route);
    expect(assign).toHaveBeenCalledWith(`/auth/login?callbackUrl=${callbackUrl}`);
    expect(JSON.parse(window.localStorage.getItem('culinaryblog.auth')!).state).toEqual({ refreshToken: null, user: null });
  });

  it('restores authentication without redirecting when refresh succeeds', async () => {
    jest.mocked(apiFetch).mockResolvedValue({
      accessToken: 'new-access-token', refreshToken: 'r2', user,
    });

    renderBootstrap();

    await screen.findByText('authenticated:new-access-token');
    expect(assign).not.toHaveBeenCalled();
  });
});
