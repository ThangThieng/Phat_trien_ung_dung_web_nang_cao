import type { Metadata } from 'next';
import { HydrationBoundary, dehydrate } from '@tanstack/react-query';
import { makeQueryClient } from '@/lib/query-client';
import { getCategories } from '@/features/categories/api';
import { searchRecipes, toRecipeSearchParams } from '@/features/recipes/api';
import { parseSearchState, type RawSearchParams } from '@/features/recipes/search-params';
import SearchResults from '@/features/search/components/SearchResults';
import { searchKeys } from '@/features/search/query-keys';
import type { Category } from '@/types/category';
import { MIN_SEARCH_LENGTH } from '@/types/search';

// SRS §5.1: /search render SSR theo request (kết quả phụ thuộc query string).
export const dynamic = 'force-dynamic';

type SearchParams = Promise<RawSearchParams>;

export async function generateMetadata({
  searchParams,
}: {
  searchParams: SearchParams;
}): Promise<Metadata> {
  const { q } = parseSearchState(await searchParams);
  return { title: q ? `Tìm kiếm: ${q}` : 'Tìm kiếm công thức' };
}

/**
 * FR-SRCH-001/002/004 — lần tải đầu render trên server với kết quả có sẵn trong HTML (crawler, người mở link chia sẻ, trình
 * duyệt chưa chạy JavaScript). Server prefetch đúng truy vấn mà `SearchResults` sẽ dùng (cùng `searchKeys.results(...)` dựng
 * từ cùng URL), rồi `dehydrate` gửi kèm HTML → phía client `useQuery` có dữ liệu ngay, không gọi API lần hai. Các lần đổi bộ
 * lọc/trang sau đó chạy hoàn toàn ở client (xem `SearchResults`) — Server Component này không chạy lại.
 */
export default async function SearchPage({ searchParams }: { searchParams: SearchParams }) {
  const state = parseSearchState(await searchParams);
  const params = toRecipeSearchParams(state);
  const queryClient = makeQueryClient();

  const [categories] = await Promise.all([
    getCategories().catch((): Category[] => []),
    // FR-SRCH-001: q < 2 ký tự là 400 ở API nên không gọi. Lỗi khi prefetch không chặn trang: truy vấn lỗi không được
    // dehydrate, client tự gọi lại và hiện lỗi trong khung kết quả. `retry: false` để API sập không kéo dài thời gian SSR.
    params.q.length >= MIN_SEARCH_LENGTH
      ? queryClient.prefetchQuery({
          queryKey: searchKeys.results(params),
          queryFn: () => searchRecipes(params),
          retry: false,
        })
      : undefined,
  ]);

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <SearchResults categories={categories} />
    </HydrationBoundary>
  );
}
