import { clampRetryAfter, rateLimitedPath, safeReturnPath } from './rate-limited';

describe('rate-limited page parameters (CR-2026-10)', () => {
  it.each([
    [undefined, 60],
    [null, 60],
    ['abc', 60],
    [0, 60],
    [-5, 60],
    [1, 1],
    [42.2, 43],
    ['42', 42],
    [999999, 3600],
  ])('clampRetryAfter(%p) = %p', (value, expected) => {
    expect(clampRetryAfter(value as number | string | null | undefined)).toBe(expected);
  });

  it.each([
    ['/recipes/pho-bo?x=1', '/recipes/pho-bo?x=1'],
    [null, '/'],
    ['https://evil.example', '/'],
    ['//evil.example/path', '/'],
    ['/\\evil.example', '/'],
    ['/too-many-requests?from=/x', '/'],
  ])('safeReturnPath(%p) = %p', (value, expected) => {
    expect(safeReturnPath(value)).toBe(expected);
  });

  it('builds the redirect target with an encoded return path', () => {
    expect(rateLimitedPath(12, '/search?q=phở')).toBe(
      `/too-many-requests?retryAfter=12&from=${encodeURIComponent('/search?q=phở')}`,
    );
  });
});
