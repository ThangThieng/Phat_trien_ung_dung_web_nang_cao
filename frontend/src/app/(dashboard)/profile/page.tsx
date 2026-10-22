'use client';

import { ProfilePage } from '@/features/profile/ProfilePage';
import { AuthGuard } from '@/features/auth/components/AuthGuard';

export default function Page() { return <AuthGuard><ProfilePage /></AuthGuard>; }
