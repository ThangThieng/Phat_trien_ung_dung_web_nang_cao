'use client';

import type { ReactNode } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from 'sonner';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthBootstrap } from '@/features/auth/auth-bootstrap';
import { getQueryClient } from '@/lib/query-client';
import { AuthStoreProvider } from '@/stores/auth-store-provider';

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
