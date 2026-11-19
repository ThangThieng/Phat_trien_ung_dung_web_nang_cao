/**
 * @jest-environment node
 */
import { NextRequest } from 'next/server';
import { GET } from './route';

describe('GET /too-many-requests (CR-2026-10)', () => {
  const call = (query: string, nonce = 'n0nce') =>
    GET(new NextRequest(`http://localhost/too-many-requests${query}`, { headers: { 'x-nonce': nonce } }));

  it('answers with a real 429, Retry-After, no-store and noindex', async () => {
    const response = call('?retryAfter=30&from=%2Frecipes%2Fpho-bo');
    expect(response.status).toBe(429);
    expect(response.headers.get('Retry-After')).toBe('30');
    expect(response.headers.get('Cache-Control')).toBe('no-store');
    expect(response.headers.get('X-Robots-Tag')).toBe('noindex');
    const html = await response.text();
    expect(html).toContain('30 giây');
    expect(html).toContain('href="/recipes/pho-bo"');
    expect(html).toContain('<style nonce="n0nce">');
  });

  it('never links off-site and escapes what it echoes', async () => {
    const offSite = await call('?from=%2F%2Fevil.example').text();
    expect(offSite).toContain('href="/"');
    const injected = await call('?from=%2F%22%3E%3Cscript%3E', '"><script>').text();
    expect(injected).not.toContain('<script>');
  });
});
