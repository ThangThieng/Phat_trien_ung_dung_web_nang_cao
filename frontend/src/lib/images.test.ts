import { toImageSrc } from './images';

describe('toImageSrc (CR-2026-10)', () => {
  it.each([
    ['http://localhost/media/recipes/a/b.webp', 'http://localhost/media', '/media/recipes/a/b.webp'],
    ['https://localhost/media/recipes/a/b.webp', 'https://localhost/media/', '/media/recipes/a/b.webp'],
    ['https://blog.example.vn/media/uploads/u/x.png', 'https://blog.example.vn/media', '/media/uploads/u/x.png'],
    // Không phải ảnh của hệ thống: giữ nguyên.
    ['https://lh3.googleusercontent.com/a/photo', 'http://localhost/media', 'https://lh3.googleusercontent.com/a/photo'],
    ['http://localhost/mediaX/a.png', 'http://localhost/media', 'http://localhost/mediaX/a.png'],
    ['http://other.host/media/a.png', 'http://localhost/media', 'http://other.host/media/a.png'],
    ['blob:http://localhost/123', 'http://localhost/media', 'blob:http://localhost/123'],
  ])('%s → %s', (url, base, expected) => {
    expect(toImageSrc(url, base)).toBe(expected);
  });

  it('keeps the URL when no media base is configured', () => {
    expect(toImageSrc('http://localhost/media/a.png', '')).toBe('http://localhost/media/a.png');
  });
});
