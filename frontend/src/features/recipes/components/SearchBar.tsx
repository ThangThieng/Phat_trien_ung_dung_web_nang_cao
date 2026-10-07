'use client';

import { useEffect, useRef, useState, type FormEvent } from 'react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { Search } from 'lucide-react';
import { MIN_SEARCH_LENGTH } from '../search-params';

const DEBOUNCE_MS = 300;

/**
 * FR-SRCH-001: thanh tìm kiếm trên header. Enter → điều hướng `/search?q=`. Khi đang ở trang `/search`, gõ tới đâu kết quả
 * cập nhật tới đó sau 300 ms ngừng gõ (debounce) — thay URL bằng `replace` để lịch sử không bị đầy từng ký tự và vẫn giữ
 * bộ lọc đang bật. Dưới ngưỡng 2 ký tự không gọi API (sẽ là 400).
 */
export default function SearchBar({ className = '' }: { className?: string }) {
  const router = useRouter();
  const pathname = usePathname();
  const params = useSearchParams();
  const onSearchPage = pathname === '/search';
  const urlQuery = onSearchPage ? (params.get('q') ?? '') : '';

  const [value, setValue] = useState(urlQuery);
  const lastReplaced = useRef<string | null>(null);

  // Đồng bộ khi URL đổi từ bên ngoài (back/forward, bấm link); bỏ qua giá trị do chính debounce vừa đặt để không ghi đè chữ đang gõ.
  useEffect(() => {
    if (lastReplaced.current !== null && lastReplaced.current === urlQuery.trim()) return;
    setValue(urlQuery);
  }, [urlQuery]);

  useEffect(() => {
    if (!onSearchPage) return undefined;
    const term = value.trim();
    if (term === urlQuery.trim() || term.length < MIN_SEARCH_LENGTH) return undefined;

    const timer = setTimeout(() => {
      const next = new URLSearchParams(params.toString());
      next.set('q', term);
      next.delete('page');
      lastReplaced.current = term;
      router.replace(`/search?${next.toString()}`);
    }, DEBOUNCE_MS);
    return () => clearTimeout(timer);
  }, [value, urlQuery, onSearchPage, params, router]);

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    const term = value.trim();
    lastReplaced.current = null;
    router.push(term ? `/search?q=${encodeURIComponent(term)}` : '/search');
  };

  return (
    <form role="search" onSubmit={handleSubmit} className={className}>
      <div className="relative">
        <Search
          aria-hidden
          className="pointer-events-none absolute top-1/2 left-2.5 h-4 w-4 -translate-y-1/2 text-gray-400"
        />
        <input
          id="site-search"
          aria-label="Tìm công thức"
          type="search"
          value={value}
          onChange={(event) => setValue(event.target.value)}
          placeholder="Tìm công thức…"
          autoComplete="off"
          className="w-48 rounded-full border border-gray-300 bg-white py-1.5 pr-3 pl-8 text-sm text-gray-800 focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300 lg:w-64"
        />
      </div>
    </form>
  );
}
