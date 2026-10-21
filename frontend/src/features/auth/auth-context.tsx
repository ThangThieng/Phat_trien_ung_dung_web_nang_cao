'use client';

import type { ReactNode } from 'react';
import { AuthStoreProvider } from '@/stores/auth-store-provider';
import { AuthBootstrap } from './auth-bootstrap';

/** @deprecated Use `AuthStoreProvider` + `AuthBootstrap` at the app boundary. */
export function AuthProvider({ children }: { children: ReactNode }) {
  return <AuthStoreProvider><AuthBootstrap>{children}</AuthBootstrap></AuthStoreProvider>;
}

export { useAuth, type AuthContextValue, type RegisterInput } from './use-auth';
