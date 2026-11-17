import { shouldSkipOptimizer } from './images';

describe('shouldSkipOptimizer', () => {
  it.each([
    ['http://localhost/media', true],
    ['https://localhost/media', true],
    ['http://127.0.0.1/media', true],
    [undefined, true],
    ['https://blog.example.vn/media', false],
  ])('%s', (url, expected) => {
    expect(shouldSkipOptimizer(undefined, url)).toBe(expected);
  });

  it('honours the explicit flag on a real domain', () => {
    expect(shouldSkipOptimizer('true', 'https://blog.example.vn/media')).toBe(true);
  });
});
