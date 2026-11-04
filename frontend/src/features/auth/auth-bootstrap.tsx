'use client';

import { useCallback, useEffect, useRef, type ReactNode } from 'react';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
import { getQueryClient } from '@/lib/query-client';
import { useAuthStoreApi } from '@/stores/auth-store-provider';
import type { AuthResponse } from '@/types/auth';
import { refreshSession, subscribeToSessionStorage } from './session-sync';

export const authNavigation = {
  redirect: (url: string) => window.location.assign(url),
};

export function AuthBootstrap({ children }: { children: ReactNode }) {
  const store = useAuthStoreApi();
  const started = useRef(false);

  const clearSession = useCallback(() => {
    store.getState().clear();
    getQueryClient().clear();
  }, [store]);

  const redirectToLogin = useCallback(() => {
    const callbackUrl = `${window.location.pathname}${window.location.search}`;
    authNavigation.redirect(`/auth/login?callbackUrl=${encodeURIComponent(callbackUrl)}`);
  }, []);

  const refresh = useCallback(() => refreshSession(store, async (refreshToken) => {
    const response = await apiFetch<AuthResponse>('/auth/refresh', {
      method: 'POST', body: { refreshToken }, accessToken: null, skipAuthRefresh: true,
    });
    return response;
  }), [store]);

  useEffect(() => {
    configureAuthCallbacks({
      getAccessToken: () => store.getState().accessToken,
      refresh,
      onRefreshFailure: () => {
        clearSession();
        redirectToLogin();
      },
    });
    return () => configureAuthCallbacks(null);
  }, [clearSession, redirectToLogin, refresh, store]);

  useEffect(() => {
    const unsubscribe = subscribeToSessionStorage(store, clearSession);
    if (started.current) return unsubscribe;
    started.current = true;
    let cancelled = false;
    const bootstrap = async () => {
      await store.persist.rehydrate();
      const session = store.getState();
      // A user may finish login while hydration is pending; that session already has an access token.
      if (session.status === 'authenticated') return;
      if (!session.refreshToken || !session.user) {
        if (!cancelled) store.getState().clear();
        return;
      }
      try {
        await refresh();
      } catch {
        if (!cancelled) {
          clearSession();
          redirectToLogin();
        }
      }
    };
    bootstrap().catch(() => clearSession());
    return () => {
      cancelled = true;
      unsubscribe();
    };
  }, [clearSession, redirectToLogin, refresh, store]);

  return children;
}
