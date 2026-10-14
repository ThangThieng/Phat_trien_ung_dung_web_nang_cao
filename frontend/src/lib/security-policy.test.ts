import { createContentSecurityPolicy, isNonceRoute } from './security-policy';

test('production auth CSP uses a nonce and restrictive directives without unsafe scripts', () => {
  const csp = createContentSecurityPolicy({ nonce: 'random-request-nonce', development: false });
  expect(csp).toContain("script-src 'self' 'nonce-random-request-nonce' https://accounts.google.com");
  expect(csp).not.toContain('unsafe-eval');
  expect(csp.split('; ').find((part) => part.startsWith('script-src'))).not.toContain('unsafe-inline');
  ["object-src 'none'", "frame-ancestors 'none'", "base-uri 'self'", "form-action 'self'"].forEach((directive) => expect(csp).toContain(directive));
});

test('direct API and MinIO origins come from configuration without allowing arbitrary origins', () => {
  const csp = createContentSecurityPolicy({ nonce: 'nonce', development: false, apiUrl: 'https://api.example/api/v1', mediaUrl: 'https://media.example/culinary-blog' });
  expect(csp).toContain('https://api.example');
  expect(csp).toContain('https://media.example');
  expect(csp).not.toContain('*');
  expect(() => createContentSecurityPolicy({ development: false, apiUrl: 'data:text/plain,test' })).toThrow();
});

test('development eval is never enabled in production; public ISR fallback is explicit', () => {
  expect(createContentSecurityPolicy({ development: true })).toContain("'unsafe-eval'");
  const publicPolicy = createContentSecurityPolicy({ development: false });
  expect(publicPolicy).toContain("script-src 'self' 'unsafe-inline'");
  expect(publicPolicy).not.toContain('unsafe-eval');
});

test.each(['/auth/login', '/auth/register', '/dashboard/media', '/dashboard/users', '/profile'])('nonce route: %s', (path) => {
  expect(isNonceRoute(path)).toBe(true);
});

test.each(['/recipes/soup', '/', '/categories', '/dashboard-public'])('public ISR route: %s', (path) => {
  expect(isNonceRoute(path)).toBe(false);
});
