import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { createContentSecurityPolicy, isNonceRoute } from '@/lib/security-policy';

export function middleware(request: NextRequest) {
  const nonce = isNonceRoute(request.nextUrl.pathname) ? btoa(crypto.randomUUID()) : undefined;
  const csp = createContentSecurityPolicy({
    nonce,
    development: process.env.NODE_ENV !== 'production',
    apiUrl: process.env.NEXT_PUBLIC_API_BASE_URL,
    mediaUrl: process.env.NEXT_PUBLIC_MINIO_PUBLIC_URL ?? 'http://localhost:9000',
  });
  const headers = new Headers(request.headers);
  // Never trust an incoming nonce/CSP header supplied by a browser.
  headers.delete('x-nonce');
  if (nonce) headers.set('x-nonce', nonce);
  headers.set('Content-Security-Policy', csp);
  const response = NextResponse.next({ request: { headers } });
  response.headers.set('Content-Security-Policy', csp);
  response.headers.set('X-Content-Type-Options', 'nosniff');
  response.headers.set('Referrer-Policy', 'strict-origin-when-cross-origin');
  if (nonce) response.headers.set('Cache-Control', 'private, no-store');
  return response;
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)'],
};
