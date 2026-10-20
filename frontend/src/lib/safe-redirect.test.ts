import { toSafeInternalPath } from './safe-redirect';

describe('toSafeInternalPath', () => {
  it.each(['//evil.com', '/\\evil.com', 'https://evil.com', `java${'script'}:alert(1)`, 'data:text/html,boom'])('rejects unsafe callback %s', (value) => {
    expect(toSafeInternalPath(value)).toBe('/dashboard');
  });

  it('retains a local path with its query string and hash', () => {
    expect(toSafeInternalPath('/dashboard?tab=sessions#current')).toBe('/dashboard?tab=sessions#current');
  });
});
