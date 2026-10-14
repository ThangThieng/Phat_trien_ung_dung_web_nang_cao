import type { ReactNode } from 'react';
import { connection } from 'next/server';

export default async function ProfileLayout({ children }: { children: ReactNode }) {
  await connection();
  return children;
}
