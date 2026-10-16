import { useSyncExternalStore } from 'react';

/**
 * `true` khi `query` (ví dụ `(min-width: 1024px)`) đang khớp. Dùng `useSyncExternalStore` nên không lệch giữa các lần render;
 * khi SSR luôn trả `false` (không có `window`) và cập nhật lại sau khi hydrate. Tách từ nhu cầu của `FilterPanel` (drawer < lg).
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (onChange) => {
      const list = window.matchMedia(query);
      list.addEventListener('change', onChange);
      return () => list.removeEventListener('change', onChange);
    },
    () => window.matchMedia(query).matches,
    () => false,
  );
}
