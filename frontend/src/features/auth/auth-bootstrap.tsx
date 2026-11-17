'use client';

import { useCallback, useEffect, useRef, type ReactNode } from 'react';
import { toast } from 'sonner';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
import { ApiError, isTransientAuthError } from '@/lib/api-error';
import { getQueryClient } from '@/lib/query-client';
import { useAuthStoreApi } from '@/stores/auth-store-provider';
import type { AuthResponse } from '@/types/auth';
import { refreshSession, subscribeToSessionStorage } from './session-sync';

export const authNavigation = {
  redirect: (url: string) => window.location.assign(url),
  reload: () => window.location.reload(),
};

const MAX_TRANSIENT_ATTEMPTS = 3;
const MAX_TRANSIENT_WAIT_MS = 60_000;

function transientWaitMs(error: unknown, attempt: number) {
  const seconds = error instanceof ApiError ? error.retryAfterSeconds : undefined;
  return Math.min((seconds ?? attempt * 2) * 1000, MAX_TRANSIENT_WAIT_MS);
}

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
    // Tab khác đăng nhập tài khoản khác: tải lại để không giữ dữ liệu của tài khoản cũ.
    const unsubscribe = subscribeToSessionStorage(store, clearSession, () => {
      getQueryClient().clear();
      authNavigation.reload();
    });
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
      // StrictMode dọn effect một lần trước khi refresh xong (`cancelled`), nhưng lần gọi đầu vẫn phải chạy: khóa `started` chặn lần thứ hai.
      for (let attempt = 1; attempt <= MAX_TRANSIENT_ATTEMPTS; attempt += 1) {
        try {
          // eslint-disable-next-line no-await-in-loop -- thử lại tuần tự, chờ theo Retry-After
          await refresh();
          return;
        } catch (error) {
          if (!isTransientAuthError(error)) {
            if (!cancelled) {
              clearSession();
              redirectToLogin();
            }
            return;
          }
          if (cancelled) return;
          if (attempt >= MAX_TRANSIENT_ATTEMPTS) {
            toast.error('Máy chủ đang bận, chưa kiểm tra được phiên đăng nhập. Hãy tải lại trang sau ít phút.');
            return;
          }
          // eslint-disable-next-line no-await-in-loop -- như trên
          await new Promise((resolve) => {
            setTimeout(resolve, transientWaitMs(error, attempt));
          });
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
