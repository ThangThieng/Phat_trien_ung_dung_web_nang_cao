import type { User } from '@/types/auth';
import { AUTH_STORAGE_KEY, createAuthStore } from './auth-store';

const user = {
  id: 'u1', email: 'user@example.com', displayName: 'User', avatarUrl: null, bio: null, roles: ['Author'],
} satisfies User;

describe('auth store persistence', () => {
  beforeEach(() => window.localStorage.clear());

  it('persists only refreshToken and user', () => {
    const store = createAuthStore();
    store.getState().setSession({ accessToken: 'access-secret', refreshToken: 'refresh-token', user });

    const persisted = JSON.parse(window.localStorage.getItem(AUTH_STORAGE_KEY)!) as { state: Record<string, unknown> };
    expect(persisted.state).toEqual({ refreshToken: 'refresh-token', user });
    expect(JSON.stringify(persisted)).not.toContain('access-secret');
    expect(persisted.state).not.toHaveProperty('accessToken');
  });

  it('migrates the raw Buổi 4 session shape and scrubs a legacy access token', async () => {
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ refreshToken: 'legacy-refresh', accessToken: 'legacy-access', user }));
    const store = createAuthStore();
    await store.persist.rehydrate();

    expect(store.getState()).toMatchObject({ refreshToken: 'legacy-refresh', user, accessToken: null });
    const migrated = window.localStorage.getItem(AUTH_STORAGE_KEY)!;
    expect(migrated).not.toContain('legacy-access');
    expect(JSON.parse(migrated)).toMatchObject({ state: { refreshToken: 'legacy-refresh', user }, version: 1 });
  });

  it('clears memory state and resets persisted session without retaining an access token', () => {
    const store = createAuthStore();
    store.getState().setSession({ accessToken: 'access-secret', refreshToken: 'refresh-token', user });
    store.getState().clear();

    expect(store.getState()).toMatchObject({ status: 'anonymous', accessToken: null, refreshToken: null, user: null });
    const persisted = window.localStorage.getItem(AUTH_STORAGE_KEY)!;
    expect(persisted).not.toContain('access-secret');
    expect(JSON.parse(persisted)).toMatchObject({ state: { refreshToken: null, user: null } });
  });
});
