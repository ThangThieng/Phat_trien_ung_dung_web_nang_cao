import Link from 'next/link';

/** Trang chủ tạm thời – bản ISR đầy đủ (công thức nổi bật, SEO) thuộc Buổi 5. */
export default function Home() {
  return (
    <section className="mx-auto flex max-w-3xl flex-col items-center gap-6 px-4 py-24 text-center">
      <p className="text-6xl" aria-hidden>
        🍜
      </p>
      <h1 className="text-4xl font-bold text-gray-900 sm:text-5xl">Culinary Blog</h1>
      <p className="text-lg text-gray-600">
        Nơi chia sẻ và khám phá công thức nấu ăn – từ món ngon ba miền đến ẩm thực thế giới.
      </p>
      <div className="flex flex-wrap justify-center gap-3">
        <Link
          href="/recipes"
          className="rounded-lg bg-orange-600 px-5 py-2.5 font-semibold text-white hover:bg-orange-700"
        >
          Khám phá công thức
        </Link>
        <Link
          href="/categories"
          className="rounded-lg border border-orange-300 px-5 py-2.5 font-semibold text-orange-800 hover:bg-orange-50"
        >
          Xem danh mục
        </Link>
      </div>
    </section>
  );
}
