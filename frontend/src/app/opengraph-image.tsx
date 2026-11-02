import { ImageResponse } from 'next/og';
import { SITE_NAME } from '@/lib/seo/site';
import { OG_FONT_FAMILY, loadOgFonts } from '@/lib/seo/og-fonts';

/**
 * NFR-SEO-002: "Mỗi trang phải có … `og:image` (1200×630px)". Trang công thức có ảnh riêng (`recipes/[slug]/opengraph-image`);
 * mọi trang còn lại (trang chủ, danh sách, danh mục, tìm kiếm) dùng thẻ chia sẻ chung này — `lib/seo/metadata.ts` gắn tường minh.
 */
export const runtime = 'edge';

export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';
export const alt = `${SITE_NAME} – Công thức nấu ăn từ bếp nhà Việt`;

export default async function SiteOpenGraphImage() {
  const fonts = await loadOgFonts();
  return new ImageResponse(
    <div
      style={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        padding: 80,
        fontFamily: OG_FONT_FAMILY,
        background: 'linear-gradient(135deg, #ffedd5, #fffbeb)',
        color: '#111827',
      }}
    >
      <div style={{ fontSize: 40, fontWeight: 700, color: '#c2410c' }}>{SITE_NAME}</div>
      <div style={{ marginTop: 24, fontSize: 76, fontWeight: 700, lineHeight: 1.1 }}>
        Công thức nấu ăn từ bếp nhà Việt
      </div>
      <div style={{ marginTop: 32, fontSize: 32, color: '#4b5563' }}>
        Đủ nguyên liệu, định lượng và từng bước — tìm không dấu cũng ra.
      </div>
    </div>,
    { ...size, fonts },
  );
}
