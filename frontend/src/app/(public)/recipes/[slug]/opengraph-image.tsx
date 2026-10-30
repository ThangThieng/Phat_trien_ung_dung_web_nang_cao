import { ImageResponse } from 'next/og';
import { getRecipeBySlug } from '@/features/recipes/api';
import { SITE_NAME, absoluteUrl } from '@/lib/seo/site';
import { truncateAtWord } from '@/lib/seo/metadata';

/** NFR-SEO-002: `og:image` 1200×630 (tỉ lệ thẻ chia sẻ) — ảnh gốc 800×600 không đúng tỉ lệ. */
/**
 * Edge runtime: cách Next.js hướng dẫn nhúng font vào `ImageResponse` (`fetch(new URL('…', import.meta.url))` được bundler
 * đổi thành tài nguyên đóng gói sẵn). Ở runtime Node, cùng lời gọi đó ra đường dẫn tương đối và bị lỗi `Invalid URL`.
 */
export const runtime = 'edge';

export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';
export const alt = `Công thức trên ${SITE_NAME}`;

/**
 * Font nhúng sẵn (src/assets/fonts): máy chủ Next.js trong container không ra Internet, còn satori mặc định tải font động từ
 * Google khi gặp ký tự lạ → chữ tiếng Việt có dấu bị thiếu glyph. Hai tập con khác tên, ghép bằng `font-family: Inter, InterVietnamese` (satori không tự dự phòng giữa hai font cùng tên: thử rồi, chỉ còn ký tự của font đầu tiên).
 */
async function loadFonts() {
  const [latin, vietnamese] = await Promise.all([
    fetch(new URL('../../../../assets/fonts/inter-latin-700.woff', import.meta.url)).then((r) => r.arrayBuffer()),
    fetch(new URL('../../../../assets/fonts/inter-vietnamese-700.woff', import.meta.url)).then((r) => r.arrayBuffer()),
  ]);
  return [
    { name: 'Inter', data: latin, weight: 700 as const, style: 'normal' as const },
    { name: 'InterVietnamese', data: vietnamese, weight: 700 as const, style: 'normal' as const },
  ];
}

/** satori chỉ vẽ được PNG/JPEG; tải trước và nhúng dạng data URI để ảnh lỗi/không hỗ trợ không làm hỏng cả thẻ chia sẻ. */
async function loadPhoto(url: string | undefined): Promise<string | null> {
  if (!url) return null;
  try {
    const response = await fetch(absoluteUrl(url), { signal: AbortSignal.timeout(4000) });
    const type = response.headers.get('content-type')?.split(';')[0] ?? '';
    if (!response.ok || !['image/png', 'image/jpeg'].includes(type)) return null;
    // Edge runtime không có `Buffer`: mã hóa base64 từng khối để không tràn đối số của String.fromCharCode.
    const bytes = new Uint8Array(await response.arrayBuffer());
    let binary = '';
    for (let i = 0; i < bytes.length; i += 0x8000) binary += String.fromCharCode(...bytes.subarray(i, i + 0x8000));
    return `data:${type};base64,${btoa(binary)}`;
  } catch {
    return null;
  }
}

export default async function OpenGraphImage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const recipe = await getRecipeBySlug(slug).catch(() => null);
  const image = recipe?.images.find((item) => item.isPrimary) ?? recipe?.images[0];
  const photo = await loadPhoto(image?.mediumUrl ?? image?.originalUrl);
  const title = truncateAtWord(recipe?.title ?? SITE_NAME, 70);
  const fonts = await loadFonts();

  return new ImageResponse(
    (
      <div
        style={{
          width: '100%',
          height: '100%',
          display: 'flex',
          fontFamily: 'Inter, InterVietnamese',
          background: 'linear-gradient(135deg, #ffedd5, #fffbeb)',
          color: '#111827',
        }}
      >
        {photo && (
          // eslint-disable-next-line @next/next/no-img-element -- ImageResponse (satori) không dùng được next/image
          <img src={photo} alt="" width={520} height={630} style={{ objectFit: 'cover' }} />
        )}
        <div
          style={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'space-between',
            padding: 56,
          }}
        >
          <div style={{ fontSize: 34, fontWeight: 700, color: '#c2410c' }}>{SITE_NAME}</div>
          <div style={{ fontSize: photo ? 56 : 72, fontWeight: 800, lineHeight: 1.15 }}>{title}</div>
          <div style={{ fontSize: 28, color: '#4b5563' }}>
            {recipe ? `${recipe.category.name} · ${recipe.totalTimeMinutes} phút` : 'Công thức nấu ăn từ bếp nhà Việt'}
          </div>
        </div>
      </div>
    ),
    { ...size, fonts },
  );
}
