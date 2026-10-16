'use client';

import type { ReactNode } from 'react';
import { useEffect, useState } from 'react';
import { usePathname } from 'next/navigation';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from 'sonner';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider } from '@/features/auth/auth-context';
import { getQueryClient } from '@/lib/query-client';
import { isNonceRoute } from '@/lib/security-policy';

export default function Providers({ children }: { children: ReactNode }) {
  const [nonce, setNonce] = useState<string>();
  const pathname = usePathname();
  useEffect(() => {
    const documentNonce = document.querySelector<HTMLScriptElement>('script[nonce]')?.nonce;
    // A client-side transition cannot replace the public document's CSP header.
    if (isNonceRoute(pathname) && !documentNonce) {
      window.location.replace(window.location.href);
      return;
    }
    setNonce(documentNonce);
  }, [pathname]);
  const queryClient = getQueryClient();
  const content = (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        {children}
        <Toaster richColors position="top-right" />
      </AuthProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );

  const googleClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;
  return googleClientId ? <GoogleOAuthProvider clientId={googleClientId} nonce={nonce}>{content}</GoogleOAuthProvider> : content;
}
