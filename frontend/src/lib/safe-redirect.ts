// 19/11/2026: trước là '/dashboard' — đăng nhập từ nút trên thanh đầu trang (không có callbackUrl) luôn rơi vào bảng điều khiển trống
// của tài khoản mới, không thấy công thức nào. Người đã đăng nhập vẫn có mọi quyền của khách (SRS §2.3) nên về trang chủ.
const fallback = '/';

/** Returns a same-origin path only. Auth callback URLs must never become an open redirect. */
export function toSafeInternalPath(callbackUrl: string | null | undefined): string {
  if (
    !callbackUrl ||
    !callbackUrl.startsWith('/') ||
    callbackUrl.startsWith('//') ||
    callbackUrl.startsWith('/\\')
  )
    return fallback;
  if (
    callbackUrl.includes('\\') ||
    [...callbackUrl].some((character) => character.charCodeAt(0) < 32)
  )
    return fallback;
  try {
    const parsed = new URL(callbackUrl, 'https://culinaryblog.local');
    if (parsed.origin !== 'https://culinaryblog.local') return fallback;
    return `${parsed.pathname}${parsed.search}${parsed.hash}`;
  } catch {
    return fallback;
  }
}
