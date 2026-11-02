export type CspMode = 'enforce' | 'report-only';

export function getCspMode(value = process.env.NEXT_PUBLIC_CSP_MODE): CspMode {
  return value === 'report-only' ? 'report-only' : 'enforce';
}

export function buildContentSecurityPolicy(nonce: string, development = process.env.NODE_ENV !== 'production'): string {
  const scriptSources = ["'self'", `'nonce-${nonce}'`, "'strict-dynamic'", 'https://accounts.google.com/gsi/client'];
  if (development) scriptSources.push("'unsafe-eval'");
  const styleSources = ["'self'"];
  if (development) styleSources.push("'unsafe-inline'");
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
