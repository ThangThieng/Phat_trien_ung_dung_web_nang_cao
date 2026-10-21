export type CspMode = 'enforce' | 'report-only';

export function getCspMode(value = process.env.NEXT_PUBLIC_CSP_MODE): CspMode {
  return value === 'report-only' ? 'report-only' : 'enforce';
}

export function buildContentSecurityPolicy(nonce: string, development = process.env.NODE_ENV !== 'production'): string {
  const scriptSources = ["'self'", `'nonce-${nonce}'`, "'strict-dynamic'", 'https://accounts.google.com/gsi/client'];
  if (development) scriptSources.push("'unsafe-eval'");
  return [
    "default-src 'self'",
    `script-src ${scriptSources.join(' ')}`,
    // Next and Google Identity inject runtime style attributes; this does not relax script execution.
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
    "font-src 'self' https://fonts.gstatic.com",
    "img-src 'self' data: blob: https://lh3.googleusercontent.com",
    "connect-src 'self' https://accounts.google.com",
    "frame-src https://accounts.google.com",
    "base-uri 'self'",
    "frame-ancestors 'self'",
    "object-src 'none'",
  ].join('; ');
}
