'use client';

import type { ReactNode } from 'react';
import dynamic from 'next/dynamic';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthBootstrap } from '@/features/auth/auth-bootstrap';
import { getQueryClient } from '@/lib/query-client';
import { AuthStoreProvider } from '@/stores/auth-store-provider';

// Toast chỉ cần khi người dùng đã tương tác; không đưa Sonner vào First Load JS.
const Toaster = dynamic(() => import('sonner').then(({ Toaster: SonnerToaster }) => SonnerToaster), {
  ssr: false,
});

export default function Providers({ children, nonce }: { children: ReactNode; nonce?: string }) {
  const queryClient = getQueryClient();
  const content = (
    <QueryClientProvider client={queryClient}>
      <AuthStoreProvider>
        <AuthBootstrap>
        {children}
        <Toaster richColors position="top-right" />
        </AuthBootstrap>
      </AuthStoreProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );

  const googleClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;
  return googleClientId ? (
    <GoogleOAuthProvider clientId={googleClientId} nonce={nonce}>{content}</GoogleOAuthProvider>
  ) : (
    content
  );
}
