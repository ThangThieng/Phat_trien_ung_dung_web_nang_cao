'use client';

import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { apiFetch, configureAuthCallbacks } from '@/lib/api-client';
import { getQueryClient } from '@/lib/query-client';
import type { AuthResponse, User } from '@/types/api';
import { coordinateRefresh } from './refresh-coordinator';

const STORAGE_KEY = 'culinaryblog.auth';
let authRefreshPromise: Promise<string> | null = null;

interface AuthSession {
  refreshToken: string;
  user: User;
}

interface ActiveSession extends AuthSession {
  accessToken: string;
  expiresAt: string;
}

interface RegisterInput {
  displayName: string;
  email: string;
  password: string;
}

interface AuthContextValue {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  /** false cho tới khi đọc xong session từ storage (tránh nháy UI khi hydrate). */
  isReady: boolean;
  login: (email: string, password: string) => Promise<User>;
  loginWithGoogle: (idToken: string) => Promise<User>;
  register: (input: RegisterInput) => Promise<User>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function writeSession(session: AuthSession | null) {
  try {
    if (session) window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    else window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Storage bị chặn (private mode) – session chỉ sống trong memory
  }
}

function readSession(): AuthSession | null | undefined {
  let raw: string | null;
  try {
    raw = window.localStorage.getItem(STORAGE_KEY);
  } catch {
    return undefined; // Distinguish inaccessible storage from a session removed by another tab.
  }
  if (raw === null) return null;
  try {
    const parsed = JSON.parse(raw) as Partial<AuthSession> | null;
    if (!parsed || typeof parsed.refreshToken !== 'string' || !parsed.refreshToken.trim()
      || !parsed.user || typeof parsed.user.id !== 'string' || typeof parsed.user.email !== 'string'
      || typeof parsed.user.displayName !== 'string' || !Array.isArray(parsed.user.roles)
      || !parsed.user.roles.every((role) => typeof role === 'string')) throw new Error('Invalid stored session');
    const stored = { refreshToken: parsed.refreshToken, user: parsed.user };
    writeSession(stored); // Strip legacy access token even before the reload refresh completes.
    return stored;
  } catch {
    writeSession(null);
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<ActiveSession | null>(null);
  const sessionRef = useRef<ActiveSession | null>(null);
  const sessionVersion = useRef(0);
  const [isReady, setIsReady] = useState(false);

  const updateSession = useCallback((next: ActiveSession | null) => {
    sessionVersion.current += 1;
    sessionRef.current = next;
    setSession(next);
    writeSession(next ? { refreshToken: next.refreshToken, user: next.user } : null);
  }, []);

  const refresh = useCallback(() => {
    if (authRefreshPromise) return authRefreshPromise;
    authRefreshPromise = coordinateRefresh(() => {
      const latest = readSession();
      if (latest === null) {
        updateSession(null);
        getQueryClient().clear();
      }
      return (latest === undefined ? sessionRef.current : latest)?.refreshToken ?? null;
    }, async (refreshToken) => {
      const version = sessionVersion.current;
      const response = await apiFetch<AuthResponse>('/auth/refresh', {
        method: 'POST', body: { refreshToken }, accessToken: null, skipAuthRefresh: true,
      });
      const next: ActiveSession = response;
      const latest = readSession();
      if (version !== sessionVersion.current || latest === null
        || (latest && latest.refreshToken !== refreshToken)) throw new Error('Session changed during refresh');
      updateSession(next);
      return next.accessToken;
    }).finally(() => { authRefreshPromise = null; });
    return authRefreshPromise;
  }, [updateSession]);

  const clearSession = useCallback(() => {
    updateSession(null);
    getQueryClient().clear();
  }, [updateSession]);

  useEffect(() => {
    const onStorage = (event: StorageEvent) => {
      if ((event.key === STORAGE_KEY || event.key === null) && event.newValue === null) clearSession();
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, [clearSession]);

  useEffect(() => {
    configureAuthCallbacks({
      getAccessToken: () => sessionRef.current?.accessToken ?? null,
      refresh,
      onRefreshFailure: () => {
        clearSession();
        window.location.assign('/auth/login?callbackUrl=');
      },
    });
    return () => configureAuthCallbacks(null);
  }, [refresh, clearSession]);

  useEffect(() => {
    let cancelled = false;
    if (!readSession()) {
      setIsReady(true);
      return () => { cancelled = true; };
    }
    refresh().catch(() => {
      if (!cancelled) {
        clearSession();
        window.location.assign('/auth/login?callbackUrl=');
      }
    }).finally(() => { if (!cancelled) setIsReady(true); });
    return () => { cancelled = true; };
  }, [refresh, clearSession]);

  const applyAuthResponse = useCallback((response: AuthResponse) => {
    const next: ActiveSession = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: response.expiresAt,
      user: response.user,
    };
    updateSession(next);
    return response.user;
  }, [updateSession]);

  const login = useCallback(
    async (email: string, password: string) =>
      applyAuthResponse(
        await apiFetch<AuthResponse>('/auth/login', { method: 'POST', body: { email, password } }),
      ),
    [applyAuthResponse],
  );

  const register = useCallback(
    async (input: RegisterInput) =>
      applyAuthResponse(
        await apiFetch<AuthResponse>('/auth/register', { method: 'POST', body: input }),
      ),
    [applyAuthResponse],
  );

  const loginWithGoogle = useCallback(
    async (idToken: string) =>
      applyAuthResponse(
        await apiFetch<AuthResponse>('/auth/google', { method: 'POST', body: { idToken } }),
      ),
    [applyAuthResponse],
  );

  const logout = useCallback(async () => {
    try {
      if (session) {
        await apiFetch<void>('/auth/logout', {
          method: 'POST',
          body: { refreshToken: session.refreshToken },
          accessToken: session.accessToken,
          skipAuthRefresh: true,
        });
      }
    } finally {
      updateSession(null);
    }
  }, [session, updateSession]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      accessToken: session?.accessToken ?? null,
      isAuthenticated: session !== null,
      isReady,
      login,
      loginWithGoogle,
      register,
      logout,
    }),
    [session, isReady, login, loginWithGoogle, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth phải được dùng bên trong <AuthProvider>.');
  return context;
}
