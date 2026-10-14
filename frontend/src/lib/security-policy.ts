interface SecurityPolicyOptions {
  nonce?: string;
  development: boolean;
  apiUrl?: string;
  mediaUrl?: string;
}

function origin(url: string | undefined) {
  if (!url || url.startsWith('/')) return '';
  const parsed = new URL(url);
  if (!['http:', 'https:'].includes(parsed.protocol)) throw new Error('Security policy requires HTTP(S) origins');
  return parsed.origin;
}

export function isNonceRoute(path: string) {
  return ['/auth', '/profile', '/dashboard'].some((root) => path === root || path.startsWith(`${root}/`));
}

export function createContentSecurityPolicy({ nonce, development, apiUrl, mediaUrl }: SecurityPolicyOptions) {
  // Public ISR pages need Next.js inline bootstrap scripts; private/auth pages use a fresh nonce.
  const inlineScripts = nonce ? `'nonce-${nonce}'` : "'unsafe-inline'";
  return [
    "default-src 'self'",
    `script-src 'self' ${inlineScripts} https://accounts.google.com${development ? " 'unsafe-eval'" : ''}`,
    "style-src 'self' 'unsafe-inline' https://accounts.google.com",
    `img-src 'self' data: blob: ${origin(mediaUrl)} https://lh3.googleusercontent.com`,
    "font-src 'self'",
    `connect-src 'self' ${origin(apiUrl)} ${origin(mediaUrl)} https://accounts.google.com${development ? ' ws://localhost:3000 ws://127.0.0.1:3000' : ''}`,
    "frame-src https://accounts.google.com",
    "object-src 'none'",
    "frame-ancestors 'none'",
    "base-uri 'self'",
    "form-action 'self'",
  ].join('; ');
}
