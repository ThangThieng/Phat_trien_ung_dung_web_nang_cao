/**
 * Base URL của Backend API.
 * - Server (SSR/ISR trong container Next.js): API_INTERNAL_BASE_URL (ví dụ http://api:8080/api/v1)
 * - Trình duyệt: NEXT_PUBLIC_API_BASE_URL (ví dụ http://localhost:5000/api/v1)
 */
export function getApiBaseUrl(): string {
  if (typeof window === 'undefined') {
    return (
      process.env.API_INTERNAL_BASE_URL ??
      process.env.NEXT_PUBLIC_API_BASE_URL ??
      'http://localhost:5000/api/v1'
    );
  }
  return process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5000/api/v1';
}

export const SITE_NAME = 'Culinary Blog';
