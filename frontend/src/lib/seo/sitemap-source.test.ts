import { buildSitemapEntries } from './sitemap-source';

const now = new Date('2026-10-28T00:00:00Z');

describe('buildSitemapEntries (NFR-SEO-003)', () => {
  const entries = buildSitemapEntries({
    recipes: [{ slug: 'pho-bo', updatedAt: '2026-10-01T00:00:00Z' }, { slug: 'bun-cha', updatedAt: null }],
    categories: [{ slug: 'mon-chinh' }],
    now,
  });
  const urls = entries.map((entry) => entry.url);

  it('có trang tĩnh, danh mục và mọi công thức Published được cấp', () => {
    expect(urls.some((u) => u.endsWith('/search'))).toBe(true);
    expect(urls.some((u) => u.endsWith('/categories'))).toBe(true);
    expect(urls.some((u) => u.endsWith('/categories/mon-chinh'))).toBe(true);
    expect(urls.some((u) => u.endsWith('/recipes/pho-bo'))).toBe(true);
    expect(urls.some((u) => u.endsWith('/recipes/bun-cha'))).toBe(true);
  });

  it('lastModified lấy từ updatedAt, thiếu thì dùng thời điểm sinh', () => {
    expect(entries.find((e) => e.url.endsWith('/recipes/pho-bo'))?.lastModified).toEqual(new Date('2026-10-01T00:00:00Z'));
    expect(entries.find((e) => e.url.endsWith('/recipes/bun-cha'))?.lastModified).toEqual(now);
  });

  it('mọi URL tuyệt đối, không trùng', () => {
    expect(urls.every((u) => /^https?:\/\//.test(u))).toBe(true);
    expect(new Set(urls).size).toBe(urls.length);
  });
});
