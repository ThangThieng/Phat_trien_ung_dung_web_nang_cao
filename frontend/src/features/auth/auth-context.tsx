'use client';

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { apiFetch } from '@/lib/api-client';
import type { AuthResponse, User } from '@/types/api';

const STORAGE_KEY = 'culinaryblog.auth';

interface AuthSession {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

interface RegisterInput {
  fullName: string;
  email: string;
  userName: string;
  password: string;
}

interface AuthContextValue {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  /** false cho tới khi đọc xong session từ storage (tránh nháy UI khi hydrate). */
  isReady: boolean;
  login: (email: string, password: string) => Promise<User>;
  register: (input: RegisterInput) => Promise<User>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function readSession(): AuthSession | null {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const session = JSON.parse(raw) as AuthSession;
    // Access token hết hạn → bỏ session (luồng Refresh Token Rotation – FR-AUTH-004 – bổ sung ở Buổi 4)
    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      window.localStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return session;
  } catch {
    return null;
  }
}

function writeSession(session: AuthSession | null) {
  try {
    if (session) window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    else window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Storage bị chặn (private mode) – session chỉ sống trong memory
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    setSession(readSession());
    setIsReady(true);
  }, []);

  // Tự đăng xuất phía client khi access token hết hạn
  useEffect(() => {
    if (!session) return undefined;
    const remaining = new Date(session.expiresAt).getTime() - Date.now();
    const timer = window.setTimeout(
      () => {
        writeSession(null);
        setSession(null);
      },
      Math.max(remaining, 0),
    );
    return () => window.clearTimeout(timer);
  }, [session]);

  const applyAuthResponse = useCallback((response: AuthResponse) => {
    const next: AuthSession = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: response.expiresAt,
      user: response.user,
    };
    writeSession(next);
    setSession(next);
    return response.user;
  }, []);

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

  const logout = useCallback(() => {
    writeSession(null);
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      accessToken: session?.accessToken ?? null,
      isAuthenticated: session !== null,
      isReady,
      login,
      register,
      logout,
    }),
    [session, isReady, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth phải được dùng bên trong <AuthProvider>.');
  return context;
}
