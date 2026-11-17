'use client';

import { useCallback, useMemo } from 'react';
import { apiFetch } from '@/lib/api-client';
import { getQueryClient } from '@/lib/query-client';
import { useAuthStore, useAuthStoreApi } from '@/stores/auth-store-provider';
import type { AuthResponse, User } from '@/types/auth';

export interface RegisterInput {
  displayName: string;
  email: string;
  password: string;
}

export interface AuthContextValue {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isReady: boolean;
  login: (email: string, password: string) => Promise<User>;
  loginWithGoogle: (idToken: string) => Promise<User>;
  register: (input: RegisterInput) => Promise<User>;
  logout: () => Promise<void>;
}

/** Compatibility hook for existing feature components. State remains in the per-tree Zustand store. */
export function useAuth(): AuthContextValue {
  const store = useAuthStoreApi();
  const user = useAuthStore((state) => state.user);
  const accessToken = useAuthStore((state) => state.accessToken);
  const status = useAuthStore((state) => state.status);

  const applyResponse = useCallback((response: AuthResponse) => {
    store.getState().setSession(response);
    return response.user;
  }, [store]);

  const login = useCallback(async (email: string, password: string) =>
    applyResponse(await apiFetch<AuthResponse>('/auth/login', { method: 'POST', body: { email, password } })), [applyResponse]);

  const register = useCallback(async (input: RegisterInput) =>
    applyResponse(await apiFetch<AuthResponse>('/auth/register', { method: 'POST', body: input })), [applyResponse]);

  const loginWithGoogle = useCallback(async (idToken: string) =>
    applyResponse(await apiFetch<AuthResponse>('/auth/google', { method: 'POST', body: { idToken } })), [applyResponse]);

  const logout = useCallback(async () => {
    const session = store.getState();
    try {
      if (session.refreshToken) {
        await apiFetch<void>('/auth/logout', {
          method: 'POST', body: { refreshToken: session.refreshToken }, accessToken: session.accessToken, skipAuthRefresh: true,
        });
      }
    } finally {
      store.getState().clear();
      getQueryClient().clear();
    }
  }, [store]);

  return useMemo(() => ({
    user,
    accessToken,
    isAuthenticated: status === 'authenticated',
    isReady: status !== 'unknown',
    login,
    loginWithGoogle,
    register,
    logout,
  }), [user, accessToken, status, login, loginWithGoogle, register, logout]);
}
