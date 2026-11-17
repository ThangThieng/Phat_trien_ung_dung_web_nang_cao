'use client';

import { createContext, useContext, useRef, type ReactNode } from 'react';
import { useStore } from 'zustand';
import { createAuthStore, type AuthStore, type AuthStoreState } from './auth-store';

const AuthStoreContext = createContext<AuthStore | null>(null);

export function AuthStoreProvider({ children }: { children: ReactNode }) {
  const storeRef = useRef<AuthStore | null>(null);
  if (!storeRef.current) storeRef.current = createAuthStore();
  return <AuthStoreContext.Provider value={storeRef.current}>{children}</AuthStoreContext.Provider>;
}

export function useAuthStore<T>(selector: (state: AuthStoreState) => T): T {
  const store = useContext(AuthStoreContext);
  if (!store) throw new Error('useAuthStore phải được dùng bên trong <AuthStoreProvider>.');
  return useStore(store, selector);
}

export function useAuthStoreApi(): AuthStore {
  const store = useContext(AuthStoreContext);
  if (!store) throw new Error('useAuthStoreApi phải được dùng bên trong <AuthStoreProvider>.');
  return store;
}
