'use client';

import { useCallback, useEffect, useRef, type ReactNode } from 'react';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
import { getQueryClient } from '@/lib/query-client';
import { useAuthStoreApi } from '@/stores/auth-store-provider';
import type { AuthResponse } from '@/types/auth';
import { refreshSession, subscribeToSessionStorage } from './session-sync';

export function AuthBootstrap({ children }: { children: ReactNode }) {
  const store = useAuthStoreApi();
  const started = useRef(false);

  const clearSession = useCallback(() => {
    store.getState().clear();
    getQueryClient().clear();
  }, [store]);

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
        if (window.location.pathname.startsWith('/dashboard')) window.location.assign('/auth/login?callbackUrl=');
      },
    });
    return () => configureAuthCallbacks(null);
  }, [clearSession, refresh, store]);

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
          if (window.location.pathname.startsWith('/dashboard')) window.location.assign('/auth/login?callbackUrl=');
        }
      }
    };
    bootstrap().catch(() => clearSession());
    return () => {
      cancelled = true;
      unsubscribe();
    };
  }, [clearSession, refresh, store]);

  return children;
}
