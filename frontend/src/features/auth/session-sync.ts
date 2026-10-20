'use client';

import type { AuthSession, AuthStore } from '@/stores/auth-store';

export const REFRESH_LOCK_NAME = 'culinaryblog-auth-refresh';

export interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
  user: AuthSession['user'];
}

/**
 * Coordinates refreshes across tabs. The persisted state is read after the lock
 * is acquired, and the replacement is persisted before the lock is released.
 */
export async function refreshSession(
  store: AuthStore,
  rotate: (refreshToken: string) => Promise<RefreshResponse>,
): Promise<string> {
  const run = async () => {
    await store.persist.rehydrate();
    let persistedSession: string | null = null;
    let storageAvailable = true;
    try {
      persistedSession = window.localStorage.getItem('culinaryblog.auth');
    } catch {
      storageAvailable = false;
    }
    if (storageAvailable && persistedSession === null) {
      store.getState().clear();
      throw new Error('No refresh token');
    }
    const { refreshToken } = store.getState();
    if (!refreshToken) throw new Error('No refresh token');

    const replacement = await rotate(refreshToken);
    store.getState().setSession(replacement);
    return replacement.accessToken;
  };

  if (typeof navigator !== 'undefined' && navigator.locks?.request) {
    return navigator.locks.request(REFRESH_LOCK_NAME, run);
  }
  return run();
}

/** Keeps a tab in sync with persisted session updates and logout from another tab. */
export function subscribeToSessionStorage(store: AuthStore, onRemoteLogout?: () => void): () => void {
  if (typeof window === 'undefined') return () => undefined;

  const onStorage = (event: StorageEvent) => {
    if ((event.storageArea && event.storageArea !== window.localStorage) || event.key !== 'culinaryblog.auth') return;
    if (event.newValue === null) {
      store.getState().clear();
      onRemoteLogout?.();
      return;
    }
    Promise.resolve(store.persist.rehydrate()).catch(() => undefined);
  };

  window.addEventListener('storage', onStorage);
  return () => window.removeEventListener('storage', onStorage);
}
