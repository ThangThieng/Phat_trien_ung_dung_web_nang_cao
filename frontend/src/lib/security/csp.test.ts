import { buildContentSecurityPolicy, getCspMode } from './csp';

describe('content security policy', () => {
  it('uses nonce and strict-dynamic without unsafe script directives in production', () => {
    const policy = buildContentSecurityPolicy('request-nonce', false);
    const scriptSource = policy.split(';').find((directive) => directive.trim().startsWith('script-src'))!;
    expect(scriptSource.trim()).toBe("script-src 'self' 'nonce-request-nonce' 'strict-dynamic' https://accounts.google.com/gsi/client");
    expect(scriptSource).not.toContain("'unsafe-inline'");
    expect(scriptSource).not.toContain("'unsafe-eval'");
    expect(policy).toContain("style-src 'self'");
    expect(policy).not.toContain("style-src 'self' 'unsafe-inline'");
    expect(policy).toContain("form-action 'self'");
    expect(policy).toContain("frame-ancestors 'none'");
    expect(policy).toContain('upgrade-insecure-requests');
    expect(policy).not.toContain('fonts.googleapis.com');
    expect(policy).not.toContain('fonts.gstatic.com');
  });

  it('only permits unsafe-eval for development and supports report-only rollout mode', () => {
    expect(buildContentSecurityPolicy('nonce', true)).toContain("'unsafe-eval'");
    expect(buildContentSecurityPolicy('nonce', true)).toContain("style-src 'self' 'unsafe-inline'");
    expect(buildContentSecurityPolicy('nonce', true)).not.toContain('upgrade-insecure-requests');
    expect(getCspMode('report-only')).toBe('report-only');
    expect(getCspMode('other')).toBe('enforce');
  });
});
