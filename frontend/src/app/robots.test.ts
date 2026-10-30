import robots from './robots';

describe('robots (NFR-SEO-003)', () => {
  const result = robots();
  const rule = Array.isArray(result.rules) ? result.rules[0] : result.rules;

  it('cho phép / và chặn khu vực riêng tư', () => {
    expect(rule.allow).toBe('/');
    expect(rule.disallow).toEqual(expect.arrayContaining(['/dashboard/', '/profile', '/auth/', '/api/', '/mockups']));
  });

  it('khai báo Sitemap ở domain chính', () => {
    expect(result.sitemap).toMatch(/^https?:\/\/.+\/sitemap\.xml$/);
  });
});
