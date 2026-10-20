'use client';

import { createJSONStorage, persist, type StateStorage } from 'zustand/middleware';
import { createStore, type StoreApi } from 'zustand/vanilla';
import type { User } from '@/types/auth';

export const AUTH_STORAGE_KEY = 'culinaryblog.auth';

export type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface AuthStoreState {
  status: AuthStatus;
  accessToken: string | null;
  refreshToken: string | null;
  user: User | null;
  setSession: (session: AuthSession) => void;
  setUser: (user: User | null) => void;
  setAccessToken: (accessToken: string | null) => void;
  clear: () => void;
}

type PersistedSession = Pick<AuthStoreState, 'refreshToken' | 'user'>;

const anonymousState = (): Pick<AuthStoreState, 'status' | 'accessToken' | 'refreshToken' | 'user'> => ({
  status: 'anonymous', accessToken: null, refreshToken: null, user: null,
});

function isLegacySession(value: unknown): value is PersistedSession {
  if (!value || typeof value !== 'object' || Array.isArray(value)) return false;
  const session = value as Partial<PersistedSession>;
  return typeof session.refreshToken === 'string' && session.refreshToken.length > 0 && !!session.user;
}

/** Converts the Buổi 4 raw session shape into Zustand's `{ state, version }` envelope and drops any old access token. */
export const legacyAwareLocalStorage: StateStorage = {
  getItem: (name) => {
    const raw = window.localStorage.getItem(name);
    if (!raw || name !== AUTH_STORAGE_KEY) return raw;
    try {
      const parsed: unknown = JSON.parse(raw);
      if (!isLegacySession(parsed) || ('state' in parsed)) return raw;
      const migrated = JSON.stringify({ state: { refreshToken: parsed.refreshToken, user: parsed.user }, version: 1 });
      window.localStorage.setItem(name, migrated);
      return migrated;
    } catch {
      window.localStorage.removeItem(name);
      return null;
    }
  },
  setItem: (name, value) => window.localStorage.setItem(name, value),
  removeItem: (name) => window.localStorage.removeItem(name),
};

export type AuthStore = StoreApi<AuthStoreState> & {
  persist: {
    rehydrate: () => Promise<void> | void;
  };
};

export function createAuthStore(): AuthStore {
  return createStore<AuthStoreState>()(
    persist(
      (set) => ({
        status: 'unknown', accessToken: null, refreshToken: null, user: null,
        setSession: ({ accessToken, refreshToken, user }) => set({ status: 'authenticated', accessToken, refreshToken, user }),
        setUser: (user) => set((state) => ({ user, status: user && state.refreshToken ? 'authenticated' : state.status })),
        setAccessToken: (accessToken) => set((state) => ({ accessToken, status: accessToken && state.refreshToken ? 'authenticated' : state.status })),
        clear: () => set(anonymousState()),
      }),
      {
        name: AUTH_STORAGE_KEY,
        version: 1,
        storage: createJSONStorage(() => legacyAwareLocalStorage),
        skipHydration: true,
        partialize: (state): PersistedSession => ({ refreshToken: state.refreshToken, user: state.user }),
        merge: (persisted, current) => {
          const session = persisted as Partial<PersistedSession>;
          if (!session.refreshToken || !session.user) return { ...current, ...anonymousState() };
          return { ...current, status: 'unknown', accessToken: null, refreshToken: session.refreshToken, user: session.user };
        },
      },
    ),
  ) as AuthStore;
}
