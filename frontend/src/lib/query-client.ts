import { MutationCache, QueryCache, QueryClient, isServer } from '@tanstack/react-query';
import { handleApiError, normalizeError } from './error-handler';

/** `meta` dùng chung cho truy vấn và mutation. */
export interface AppRequestMeta extends Record<string, unknown> {
  /** Màn hình tự hiển thị lỗi (dialog xung đột, widget tự ẩn khi lỗi) → bộ xử lý toàn cục không toast. */
  suppressGlobalError?: boolean;
}

declare module '@tanstack/react-query' {
  interface Register {
    queryMeta: AppRequestMeta;
    mutationMeta: AppRequestMeta;
  }
}

/** Chỉ thử lại lỗi tạm thời (mạng, 5xx) tối đa 2 lần. 4xx gửi lại vẫn sai y như cũ. */
export function shouldRetryQuery(failureCount: number, error: unknown): boolean {
  const { kind } = normalizeError(error);
  return (kind === 'network' || kind === 'server') && failureCount < 2;
}

export function makeQueryClient() {
  return new QueryClient({
    queryCache: new QueryCache({
      onError: (error, query) => {
        if (isServer || query.meta?.suppressGlobalError) return;
        handleApiError(error);
      },
    }),
    mutationCache: new MutationCache({
      onError: (error, _variables, _context, mutation) => {
        // Mutation tự khai báo onError đã tự xử lý lỗi của nó — không toast lần hai.
        if (isServer || mutation.meta?.suppressGlobalError || mutation.options.onError) return;
        handleApiError(error);
      },
    }),
    defaultOptions: {
      queries: {
        // Tránh refetch ngay sau khi hydrate dữ liệu SSR
        staleTime: 60 * 1000,
        retry: shouldRetryQuery,
      },
    },
  });
}

let browserQueryClient: QueryClient | undefined;

/** Server: mỗi request một client mới. Browser: dùng chung một client. */
export function getQueryClient() {
  if (isServer) return makeQueryClient();
  if (!browserQueryClient) browserQueryClient = makeQueryClient();
  return browserQueryClient;
}
