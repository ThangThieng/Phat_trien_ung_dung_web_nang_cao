export type CspMode = 'enforce' | 'report-only';

export function getCspMode(value = process.env.NEXT_PUBLIC_CSP_MODE): CspMode {
  return value === 'report-only' ? 'report-only' : 'enforce';
}

/**
 * Thẻ `<style>` do thư viện chèn lúc chạy mà KHÔNG nhận nonce được: Sonner (toast) gọi `__insertCSS` ngay khi được nạp — tạo một
 * `<style>` rỗng rồi đổ nguyên khối CSS cố định của nó vào. CSP chỉ cho qua bằng hash nội dung, nên hash phụ thuộc phiên bản
 * Sonner: `csp.test.ts` tính lại từ chính `node_modules/sonner` và đỏ khi nâng phiên bản mà quên cập nhật.
 * Các `<style>` của Radix UI (khóa cuộn khi mở Dialog/Sheet/Select, ẩn thanh cuộn của Select) nhận nonce qua `get-nonce`.
 */
export const LIBRARY_STYLE_HASHES = [
  "'sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU='", // <style> rỗng (Sonner tạo trước khi đổ CSS vào)
  "'sha256-StEaX+se6YS7pqjzrzMIA0KaX9zF/8zAhvQXZAe5epY='", // CSS của sonner@2.0.8
] as const;

export function buildContentSecurityPolicy(nonce: string, development = process.env.NODE_ENV !== 'production'): string {
  const scriptSources = ["'self'", `'nonce-${nonce}'`, "'strict-dynamic'", 'https://accounts.google.com/gsi/client'];
  if (development) scriptSources.push("'unsafe-eval'");
  // Có nonce/hash thì trình duyệt bỏ qua 'unsafe-inline' — nên dev (Fast Refresh chèn style không nonce) chỉ dùng 'unsafe-inline'.
  const styleSources = development
    ? ["'self'", "'unsafe-inline'"]
    : ["'self'", `'nonce-${nonce}'`, ...LIBRARY_STYLE_HASHES];
  return [
    "default-src 'self'",
    `script-src ${scriptSources.join(' ')}`,
    `style-src ${styleSources.join(' ')}`,
    "font-src 'self'",
    "img-src 'self' data: blob: https://lh3.googleusercontent.com",
    "connect-src 'self' https://accounts.google.com",
    "frame-src https://accounts.google.com",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
    "object-src 'none'",
    ...(development ? [] : ['upgrade-insecure-requests']),
  ].join('; ');
}
