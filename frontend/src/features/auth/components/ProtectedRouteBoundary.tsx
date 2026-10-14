'use client';

import type { ReactNode } from 'react';
import { usePathname } from 'next/navigation';
import RequireAuth from './RequireAuth';

const ADMIN_ROLES = ['Admin'] as const;
const AUTHOR_ROLES = ['Author', 'Admin'] as const;

function within(path: string, root: string) {
  return path === root || path.startsWith(`${root}/`);
}

export default function ProtectedRouteBoundary({ children }: { children: ReactNode }) {
  const path = usePathname();
  if (within(path, '/dashboard')) {
    const adminOnly = within(path, '/dashboard/categories') || within(path, '/dashboard/users');
    return <RequireAuth roles={adminOnly ? ADMIN_ROLES : AUTHOR_ROLES}>{children}</RequireAuth>;
  }
  if (within(path, '/profile')) return <RequireAuth>{children}</RequireAuth>;
  return children;
}
