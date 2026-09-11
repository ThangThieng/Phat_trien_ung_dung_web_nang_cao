import Link from 'next/link';

export default function NotFound() {
  return (
    <div className="mx-auto flex max-w-xl flex-col items-center px-4 py-24 text-center">
      <p className="text-6xl" aria-hidden>
        🥢
      </p>
      <h1 className="mt-4 text-2xl font-bold text-gray-900">Không tìm thấy trang</h1>
      <p className="mt-2 text-gray-600">Nội dung bạn tìm không tồn tại hoặc chưa được xuất bản.</p>
      <Link
        href="/recipes"
        className="mt-6 rounded-lg bg-orange-600 px-4 py-2 font-semibold text-white hover:bg-orange-700"
      >
        Xem công thức
      </Link>
    </div>
  );
}
