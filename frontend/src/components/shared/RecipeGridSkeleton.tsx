/** Loading skeleton cho lưới công thức (NFR-USE-004 – không hiển thị màn hình trống). */
export default function RecipeGridSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10" aria-busy="true" aria-live="polite">
      <span className="sr-only">Đang tải…</span>
      <div className="h-9 w-64 animate-pulse rounded bg-gray-200" />
      <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: count }, (_, i) => (
          <div
            key={`skeleton-${i}`}
            className="overflow-hidden rounded-2xl border border-gray-200 bg-white"
          >
            <div className="aspect-[4/3] animate-pulse bg-gray-200" />
            <div className="space-y-2 p-4">
              <div className="h-3 w-20 animate-pulse rounded bg-gray-200" />
              <div className="h-5 w-3/4 animate-pulse rounded bg-gray-200" />
              <div className="h-4 w-full animate-pulse rounded bg-gray-100" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
