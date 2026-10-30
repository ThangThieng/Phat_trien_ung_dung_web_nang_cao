import { serializeJsonLd } from '@/lib/seo/recipe-json-ld';

/** `<script type="application/ld+json">` — dữ liệu, không phải script thực thi nên không cần nonce CSP. */
export default function JsonLd({ data }: { data: unknown }) {
  // `serializeJsonLd` thoát `<` nên dữ liệu người dùng không đóng được thẻ script — đây là cách duy nhất nhúng JSON-LD.
  // eslint-disable-next-line react/no-danger
  return <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: serializeJsonLd(data) }} />;
}
