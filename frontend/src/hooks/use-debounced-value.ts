import { useEffect, useState } from 'react';

/**
 * Trả về `value` sau khi nó đứng yên `delayMs` mili giây (mặc định 300 ms). Tách từ `SearchBar` (FR-SRCH-001) để mọi ô nhập
 * cần debounce dùng chung một cách làm. Thư mục `hooks/` không được import từ `app/` (ESLint `import/no-restricted-paths`).
 */
export function useDebouncedValue<T>(value: T, delayMs = 300): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timer);
  }, [value, delayMs]);

  return debounced;
}
