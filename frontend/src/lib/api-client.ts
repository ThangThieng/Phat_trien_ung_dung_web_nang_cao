import { getApiBaseUrl } from './config';

/** RFC 7807 Problem Details do backend trả về (type = Application Error Code – SRS Phụ lục B). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  [key: string]: unknown;
}

export class ApiError extends Error {
  readonly status: number;

  readonly code: string;

  readonly fieldErrors: Record<string, string[]>;

  readonly problem: ProblemDetails;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.code = problem.type ?? 'UNKNOWN_ERROR';
    this.fieldErrors = problem.errors ?? {};
    this.problem = problem;
  }
}

export interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: unknown;
  accessToken?: string | null;
  /** Tuỳ chọn cache của Next.js (ISR: { revalidate: 300 }). */
  next?: NextFetchRequestConfig;
  cache?: RequestCache;
  signal?: AbortSignal;
  skipAuthRefresh?: boolean;
}

interface AuthCallbacks {
  getAccessToken: () => string | null;
  refresh: () => Promise<string>;
  onRefreshFailure: () => void;
}

let authCallbacks: AuthCallbacks | null = null;
let refreshPromise: Promise<string> | null = null;

export function configureAuthCallbacks(callbacks: AuthCallbacks | null) {
  authCallbacks = callbacks;
}

/** Shared by JSON fetch and progress uploads; only refresh failures invalidate the session. */
export async function withAuthRefresh<T>(
  request: (token: string | null) => Promise<T>,
  accessToken: string | null,
  skipAuthRefresh = false,
): Promise<T> {
  try {
    return await request(accessToken);
  } catch (error) {
    const callbacks = authCallbacks;
    if (!(error instanceof ApiError) || error.status !== 401 || !callbacks) throw error;
    if (error.code === 'AUTH_TOKEN_INVALID' && accessToken) callbacks.onRefreshFailure();
    if (error.code !== 'AUTH_TOKEN_EXPIRED' || skipAuthRefresh) throw error;
    // A late 401 may belong to the previous token after another request already refreshed it.
    let renewedToken = callbacks.getAccessToken();
    if (!renewedToken || renewedToken === accessToken) {
      refreshPromise ??= Promise.resolve().then(() => callbacks.refresh())
        .catch((refreshError: unknown) => {
          callbacks.onRefreshFailure();
          throw refreshError;
        }).finally(() => { refreshPromise = null; });
      try {
        renewedToken = await refreshPromise;
      } catch {
        throw error;
      }
    }
    // Outside the refresh catch: business/network failures must reach the caller unchanged.
    return withAuthRefresh(request, renewedToken, true);
  }
}

async function parseProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return { status: response.status, title: response.statusText };
  }
}

/** Gọi Backend REST API (/api/v1). Lỗi HTTP → ném ApiError chứa Problem Details. */
export async function apiFetch<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const accessToken = options.accessToken !== undefined ? options.accessToken : authCallbacks?.getAccessToken() ?? null;
  return withAuthRefresh(async (token) => {
    const headers: Record<string, string> = { Accept: 'application/json' };
    if (options.body !== undefined) headers['Content-Type'] = 'application/json';
    if (token) headers.Authorization = `Bearer ${token}`;

    const response = await fetch(`${getApiBaseUrl()}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      next: options.next,
      cache: options.cache,
      signal: options.signal,
    });

    if (!response.ok) {
      const problem = await parseProblem(response);
      throw new ApiError(response.status, problem);
    }

    if (response.status === 204) return undefined as T;
    return (await response.json()) as T;
  }, accessToken, options.skipAuthRefresh);
}

/** Thông báo thân thiện cho lỗi mạng / lỗi 5xx (không lộ chi tiết kỹ thuật – NFR-USE-003). */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status >= 500) return 'Hệ thống đang gặp sự cố. Vui lòng thử lại sau.';
    return error.message;
  }
  return 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng.';
}
