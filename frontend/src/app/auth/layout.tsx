import type { ReactNode } from 'react';
import { connection } from 'next/server';

export default async function AuthLayout({ children }: { children: ReactNode }) {
  // Request-specific CSP nonces must never be baked into a prerendered document.
  await connection();
  return children;
}
