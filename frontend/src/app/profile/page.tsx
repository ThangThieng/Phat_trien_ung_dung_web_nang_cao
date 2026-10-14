'use client';

import { useAuth } from '@/features/auth/auth-context';

/** Read-only session summary; the shared route boundary prevents rendering before authentication. */
export default function ProfilePage() {
  const { user } = useAuth();
  if (!user) return null;
  return (
    <section className="mx-auto max-w-xl px-4 py-10">
      <h1 className="text-2xl font-bold">Hồ sơ cá nhân</h1>
      <dl className="mt-6 space-y-3">
        <div><dt className="font-semibold">Tên hiển thị</dt><dd>{user.displayName}</dd></div>
        <div><dt className="font-semibold">Email</dt><dd>{user.email}</dd></div>
      </dl>
    </section>
  );
}
