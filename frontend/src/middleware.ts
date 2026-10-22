import { NextResponse, type NextRequest } from 'next/server';
import { buildContentSecurityPolicy, getCspMode } from './lib/security/csp';

/** Security headers only. Authentication remains a client UX concern and backend authorization concern. */
export function middleware(request: NextRequest) {
  const nonce = crypto.randomUUID();
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-nonce', nonce);

  const response = NextResponse.next({ request: { headers: requestHeaders } });
  const header = getCspMode() === 'report-only' ? 'Content-Security-Policy-Report-Only' : 'Content-Security-Policy';
  response.headers.set(header, buildContentSecurityPolicy(nonce));
  response.headers.set('Permissions-Policy', 'camera=(), microphone=(), geolocation=()');
  return response;
}

export const config = {
  matcher: [
    {
      source: '/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)',
      missing: [
        { type: 'header', key: 'next-router-prefetch' },
        { type: 'header', key: 'purpose', value: 'prefetch' },
      ],
    },
  ],
};
