export type CspMode = 'enforce' | 'report-only';

// Sonner v2 inserts this version-pinned stylesheet from JavaScript and cannot receive a nonce.
// A CSP hash permits that exact stylesheet only; do not replace this with unsafe-inline.
const sonnerStyleHash = "'sha256-StEaX+se6YS7pqjzrzMIA0KaX9zF/8zAhvQXZAe5epY='";
// Sonner appends its element before its stylesheet text, so the empty insertion
// must also be allowed. Both hashes are exact and version-pinned.
const emptyStyleHash = "'sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU='";

export function getCspMode(value = process.env.NEXT_PUBLIC_CSP_MODE): CspMode {
  return value === 'report-only' ? 'report-only' : 'enforce';
}

export function buildContentSecurityPolicy(nonce: string, development = process.env.NODE_ENV !== 'production'): string {
  const scriptSources = ["'self'", `'nonce-${nonce}'`, "'strict-dynamic'", 'https://accounts.google.com/gsi/client'];
  if (development) scriptSources.push("'unsafe-eval'");
  const styleSources = development ? ["'self'", "'unsafe-inline'"] : ["'self'", sonnerStyleHash, emptyStyleHash];
  return [
    "default-src 'self'",
    `script-src ${scriptSources.join(' ')}`,
    `style-src ${styleSources.join(' ')}`,
    "font-src 'self'",
    "img-src 'self' data: blob: https://lh3.googleusercontent.com",
    "connect-src 'self' https://accounts.google.com",
    "frame-src https://accounts.google.com",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
    "object-src 'none'",
    ...(development ? [] : ['upgrade-insecure-requests']),
  ].join('; ');
}
