import fs from 'fs';
import path from 'path';

/**
 * D-7 (MT-33.3) tồn tại vì con số 3600 / 600 được viết ở Buổi 2 và không ai đối chiếu với SRS. Kiểm thử này là lệnh
 * `grep -rn "revalidate: \(3600\|600\)" frontend/src` của kế hoạch Buổi 7, nhưng chạy trong `npm test`: ai đưa lại hai con số
 * lệch (hoặc `export const revalidate` ở trang — CSP nonce buộc trang render theo request, MT-67) sẽ thấy kiểm thử đỏ.
 */
const SRC = path.resolve(__dirname, '..');

function sourceFiles(dir: string): string[] {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) return sourceFiles(full);
    return /\.(ts|tsx)$/.test(entry.name) && !/\.test\.(ts|tsx)$/.test(entry.name) ? [full] : [];
  });
}

describe('revalidate của Frontend (quy tắc vàng — kế hoạch §2.3)', () => {
  // Ngoại lệ duy nhất: `GET /recipes/sitemap` có TTL Redis 1 giờ (MT-48, NFR-SEO-003) nên `revalidate: 3600` ở đây ĐÚNG —
  // quy tắc vàng là revalidate ≤ TTL của CHÍNH endpoint đó, không phải ≤ 1800 chung cho mọi endpoint.
  const SITEMAP_SOURCE = 'lib/seo/sitemap-source.ts';
  const files = sourceFiles(SRC)
    .map((file) => ({
      name: path.relative(SRC, file).replace(/\\/g, '/'),
      text: fs.readFileSync(file, 'utf8'),
    }))
    .filter(({ name }) => name !== SITEMAP_SOURCE);

  it('sitemap-source dùng đúng 3600 giây (= TTL Redis của GET /recipes/sitemap)', () => {
    const text = fs.readFileSync(path.join(SRC, SITEMAP_SOURCE), 'utf8');
    expect(text).toMatch(/revalidate:\s*3600/);
  });

  it('không còn revalidate: 3600 hoặc 600 (các giá trị lệch TTL của Buổi 2)', () => {
    const offenders = files.filter(({ text }) => /revalidate\s*[:=]\s*(3600|600)\b/.test(text));
    expect(offenders.map((f) => f.name)).toEqual([]);
  });

  it('mọi revalidate theo số ≤ 1800 giây (TTL lớn nhất của bảng NFR-PERF-003: categories:all 30 phút)', () => {
    const values = files.flatMap(({ name, text }) =>
      [...text.matchAll(/revalidate\s*[:=]\s*(\d+)/g)].map((match) => ({
        file: name,
        seconds: Number(match[1]),
      })),
    );
    expect(values.length).toBeGreaterThan(0);
    expect(values.filter((v) => v.seconds > 1800)).toEqual([]);
  });
});
