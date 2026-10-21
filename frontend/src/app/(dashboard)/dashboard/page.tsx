/** Dashboard landing route. Authentication UX is provided by the route-group layout's AuthGuard. */
export default function DashboardPage() {
  return (
    <section className="mx-auto max-w-5xl px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900">Bảng điều khiển</h1>
      <p className="mt-2 text-gray-600">Quản lý nội dung và phiên đăng nhập của bạn.</p>
    </section>
  );
}
