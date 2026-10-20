import { AUTH_STORAGE_KEY, createAuthStore } from '@/stores/auth-store';
import type { User } from '@/types/auth';
import { refreshSession, subscribeToSessionStorage } from './session-sync';

const user = {
  id: 'u1', email: 'user@example.com', displayName: 'User', avatarUrl: null, bio: null, roles: ['Author'],
} satisfies User;

describe('session sync', () => {
  const originalLocks = Object.getOwnPropertyDescriptor(navigator, 'locks');

  beforeEach(() => window.localStorage.clear());
  afterEach(() => {
    if (originalLocks) Object.defineProperty(navigator, 'locks', originalLocks);
    else Reflect.deleteProperty(navigator, 'locks');
  });

  it('clears local state after a logout event from another tab', () => {
    const store = createAuthStore();
    store.getState().setSession({ accessToken: 'A', refreshToken: 'R', user });
    const onLogout = jest.fn();
    const unsubscribe = subscribeToSessionStorage(store, onLogout);

    window.dispatchEvent(new StorageEvent('storage', { key: AUTH_STORAGE_KEY, oldValue: 'session', newValue: null, storageArea: window.localStorage }));

    expect(store.getState()).toMatchObject({ status: 'anonymous', accessToken: null, refreshToken: null, user: null });
    expect(onLogout).toHaveBeenCalledTimes(1);
    unsubscribe();
  });

  it('rehydrates a replacement refresh token from another tab', async () => {
    const store = createAuthStore();
    await store.persist.rehydrate();
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ state: { refreshToken: 'R2', user }, version: 1 }));
    const unsubscribe = subscribeToSessionStorage(store);

    window.dispatchEvent(new StorageEvent('storage', { key: AUTH_STORAGE_KEY, newValue: window.localStorage.getItem(AUTH_STORAGE_KEY), storageArea: window.localStorage }));
    await Promise.resolve();

    expect(store.getState()).toMatchObject({ refreshToken: 'R2', user, accessToken: null });
    unsubscribe();
  });

  it('serializes rotation and re-reads the persisted replacement after acquiring the Web Lock', async () => {
    let queue = Promise.resolve();
    Object.defineProperty(navigator, 'locks', {
      configurable: true,
      value: {
        request: jest.fn((_name: string, callback: () => Promise<unknown>) => {
          const result = queue.then(callback);
          queue = result.then(() => undefined, () => undefined);
          return result;
        }),
      },
    });
    const first = createAuthStore();
    const second = createAuthStore();
    first.getState().setSession({ accessToken: 'A1', refreshToken: 'R1', user });
    await second.persist.rehydrate();
    const sent: string[] = [];

    await Promise.all([
      refreshSession(first, async (token) => {
        sent.push(token);
        return { accessToken: 'A2', refreshToken: 'R2', user };
      }),
      refreshSession(second, async (token) => {
        sent.push(token);
        return { accessToken: 'A3', refreshToken: 'R3', user };
      }),
    ]);

    expect(sent).toEqual(['R1', 'R2']);
    expect(second.getState()).toMatchObject({ accessToken: 'A3', refreshToken: 'R3' });
  });
});
