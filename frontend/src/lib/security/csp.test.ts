import { buildContentSecurityPolicy, getCspMode } from './csp';

describe('content security policy', () => {
  it('uses nonce and strict-dynamic without unsafe script directives in production', () => {
    const policy = buildContentSecurityPolicy('request-nonce', false);
    expect(policy).toContain("script-src 'self' 'nonce-request-nonce' 'strict-dynamic' https://accounts.google.com/gsi/client");
    expect(policy).not.toContain("'unsafe-inline'");
    expect(policy).not.toContain("'unsafe-eval'");
  });

  it('only permits unsafe-eval for development and supports report-only rollout mode', () => {
    expect(buildContentSecurityPolicy('nonce', true)).toContain("'unsafe-eval'");
    expect(getCspMode('report-only')).toBe('report-only');
    expect(getCspMode('other')).toBe('enforce');
  });
});
