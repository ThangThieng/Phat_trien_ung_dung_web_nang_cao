const fallback = '/dashboard';

/** Returns a same-origin path only. Auth callback URLs must never become an open redirect. */
export function toSafeInternalPath(callbackUrl: string | null | undefined): string {
  if (!callbackUrl || !callbackUrl.startsWith('/') || callbackUrl.startsWith('//') || callbackUrl.startsWith('/\\')) return fallback;
  if (callbackUrl.includes('\\') || [...callbackUrl].some((character) => character.charCodeAt(0) < 32)) return fallback;
  try {
    const parsed = new URL(callbackUrl, 'https://culinaryblog.local');
    if (parsed.origin !== 'https://culinaryblog.local') return fallback;
    return `${parsed.pathname}${parsed.search}${parsed.hash}`;
  } catch {
    return fallback;
  }
}
