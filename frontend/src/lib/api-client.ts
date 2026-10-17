import ky, { isHTTPError, isNetworkError } from 'ky';
import type { AfterResponseHook, BeforeErrorHook, BeforeRequestHook, NormalizedOptions } from 'ky';
import type { ProblemDetails } from '@/types/api';
import { ApiError, parseRetryAfter } from './api-error';
import { getApiBaseUrl } from './config';
import { normalizeError } from './normalize-error';

export { ApiError, isAbortError } from './api-error';

/** FR-OBS-002 – mỗi request một mã, Backend ghi vào log (Buổi 8 trả lại trên response). */
export const CORRELATION_ID_HEADER = 'X-Correlation-ID';
export const REQUEST_TIMEOUT_MS = 15_000;
/** 429/503 có `Retry-After` dài hơn mức này thì không chờ — trả lỗi để giao diện báo người dùng. */
export const MAX_RETRY_AFTER_MS = 5_000;
/** Chỉ lỗi tạm thời. 500 là lỗi code phía server — gửi lại cũng ra kết quả như cũ. */
const RETRY_STATUS_CODES = [408, 429, 502, 503, 504];

export interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  /** Object → gửi JSON; `FormData`/`Blob`/`URLSearchParams` → gửi nguyên (trình duyệt tự đặt Content-Type). */
  body?: unknown;
  /** `undefined` = lấy token hiện tại của phiên; `null` = gửi không kèm token. */
  accessToken?: string | null;
  /** Data Cache của Next.js (`{ revalidate: 300 }`) — `ky` chuyển nguyên option này cho `fetch`. */
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

/** Phiên đăng nhập đăng ký callback vào đây — `lib/` không import `features/` hay `stores/` (quy tắc phụ thuộc Buổi 5). */
export function configureAuthCallbacks(callbacks: AuthCallbacks | null) {
  authCallbacks = callbacks;
}

const isServer = () => typeof window === 'undefined';

/**
 * Token mới cho request vừa nhận 401 `AUTH_TOKEN_EXPIRED` với `staleToken`. Single-flight: nhiều request hết hạn cùng
 * lúc chỉ gọi `/auth/refresh` một lần. Request về muộn (token đã được request khác làm mới) dùng luôn token hiện tại.
 */
function renewAccessToken(staleToken: string | null, callbacks: AuthCallbacks): Promise<string> {
  const current = callbacks.getAccessToken();
  if (current && current !== staleToken) return Promise.resolve(current);
  refreshPromise ??= Promise.resolve()
    .then(() => callbacks.refresh())
    .catch((refreshError: unknown) => {
      callbacks.onRefreshFailure();
      throw refreshError;
    })
    .finally(() => {
      refreshPromise = null;
    });
  return refreshPromise;
}

/**
 * Cùng quy tắc refresh cho đường gọi không qua `ky` (tải ảnh bằng XHR để có tiến trình — `ImageUploader`):
 * chỉ refresh khi 401 `AUTH_TOKEN_EXPIRED`, thử lại đúng một lần; chỉ refresh thất bại mới làm mất phiên.
 */
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
    let renewedToken: string;
    try {
      renewedToken = await renewAccessToken(accessToken, callbacks);
    } catch {
      throw error;
    }
    // Ngoài khối catch của refresh: lỗi nghiệp vụ/mạng của lần thử lại phải tới nơi gọi nguyên vẹn.
    return withAuthRefresh(request, renewedToken, true);
  }
}

/** Trạng thái xác thực của MỘT lời gọi `apiFetch`, dùng chung giữa các hook qua `options.context` (ky chép nông). */
interface AuthAttempt {
  token: string | null;
  skipRefresh: boolean;
  refreshed: boolean;
}

const authAttemptOf = (options: NormalizedOptions) =>
  (options.context as { auth?: AuthAttempt }).auth;

/** UUID v4. `crypto.randomUUID` chỉ có trong secure context — mở qua http://<IP LAN> thì dựng từ `getRandomValues`. */
export function createCorrelationId(): string {
  const { crypto } = globalThis;
  if (typeof crypto?.randomUUID === 'function') return crypto.randomUUID();
  const bytes = new Uint8Array(16);
  if (typeof crypto?.getRandomValues === 'function') crypto.getRandomValues(bytes);
  else
    bytes.forEach((_, index) => {
      bytes[index] = Math.floor(Math.random() * 256);
    });
  // RFC 9562: 4 bit cao của byte 6 = 0100 (version 4), 2 bit cao của byte 8 = 10 (variant).
  bytes[6] = (bytes[6] % 16) + 0x40;
  bytes[8] = (bytes[8] % 64) + 0x80;
  const hex = Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0')).join('');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

const attachCorrelationId: BeforeRequestHook = ({ request }) => {
  if (!request.headers.has(CORRELATION_ID_HEADER)) {
    request.headers.set(CORRELATION_ID_HEADER, createCorrelationId());
  }
};

/**
 * MT-34 ở Frontend: code chạy trên server (Server Component, Route Handler) chỉ gọi endpoint công khai và KHÔNG BAO
 * GIỜ gửi token — token nằm ở trình duyệt; dữ liệu riêng tư chỉ đi qua Client Component.
 */
const attachBearerToken: BeforeRequestHook = ({ request, options }) => {
  const token = authAttemptOf(options)?.token;
  if (isServer() || !token) request.headers.delete('Authorization');
  else request.headers.set('Authorization', `Bearer ${token}`);
};

async function readProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return { status: response.status, title: response.statusText };
  }
}

/** 401 `AUTH_TOKEN_EXPIRED` → refresh single-flight → gửi lại đúng một lần với token mới (kể cả POST — request chưa được xử lý). */
const refreshOnExpiredToken: AfterResponseHook = async ({ request, response, options }) => {
  const attempt = authAttemptOf(options);
  const callbacks = authCallbacks;
  if (response.status !== 401 || !attempt || !callbacks || isServer()) return undefined;
  const { type } = await readProblem(response);
  if (type === 'AUTH_TOKEN_INVALID') {
    // Token hỏng (không phải hết hạn): refresh không cứu được → đăng xuất ngay.
    if (attempt.token) callbacks.onRefreshFailure();
    return undefined;
  }
  if (type !== 'AUTH_TOKEN_EXPIRED' || attempt.skipRefresh || attempt.refreshed) return undefined;
  attempt.refreshed = true;
  try {
    attempt.token = await renewAccessToken(attempt.token, callbacks);
  } catch {
    return undefined; // giữ nguyên 401 ban đầu cho nơi gọi
  }
  const headers = new Headers(request.headers);
  headers.set('Authorization', `Bearer ${attempt.token}`);
  return ky.retry({
    request: new Request(request, { headers }),
    delay: 0,
    code: 'AUTH_TOKEN_REFRESHED',
  });
};

/** HTTPError của ky → `ApiError` (Problem Details + mã tra cứu + Retry-After). Lỗi mạng trả lại lỗi gốc của `fetch`. */
const toApiError: BeforeErrorHook = async ({ error }) => {
  if (isHTTPError(error)) {
    const { response } = error;
    let problem: ProblemDetails = { status: response.status, title: response.statusText };
    if (error.data && typeof error.data === 'object') problem = error.data as ProblemDetails;
    else if (typeof error.data === 'string' && error.data) {
      try {
        problem = JSON.parse(error.data) as ProblemDetails;
      } catch {
        // trang lỗi HTML của proxy — giữ problem mặc định
      }
    }
    return new ApiError(response.status, problem, {
      correlationId: response.headers.get(CORRELATION_ID_HEADER) ?? undefined,
      retryAfterSeconds: parseRetryAfter(response.headers.get('Retry-After')),
    });
  }
  if (isNetworkError(error) && error.cause instanceof Error) return error.cause;
  return error;
};

const http = ky.create({
  timeout: REQUEST_TIMEOUT_MS,
  retry: {
    limit: 2,
    // GET là idempotent. Gửi lại POST/PUT/PATCH/DELETE có thể tạo hai công thức hoặc xuất bản hai lần.
    methods: ['get'],
    statusCodes: RETRY_STATUS_CODES,
    afterStatusCodes: [429, 503],
    maxRetryAfter: MAX_RETRY_AFTER_MS,
    // ky kẹp Retry-After dài về maxRetryAfter rồi vẫn gửi lại; ở đây thì dừng hẳn — chờ lâu hơn 5 giây là việc của người dùng.
    shouldRetry: ({ error }) => {
      if (!isHTTPError(error)) return undefined;
      const seconds = parseRetryAfter(error.response.headers.get('Retry-After'));
      return seconds !== undefined && seconds * 1000 > MAX_RETRY_AFTER_MS ? false : undefined;
    },
  },
  hooks: {
    beforeRequest: [attachCorrelationId, attachBearerToken],
    afterResponse: [refreshOnExpiredToken],
    beforeError: [toApiError],
  },
});

const isRawBody = (body: unknown): body is BodyInit =>
  body instanceof FormData || body instanceof Blob || body instanceof URLSearchParams;

function initialToken(options: ApiRequestOptions): string | null {
  if (isServer()) return null;
  if (options.accessToken !== undefined) return options.accessToken;
  return authCallbacks?.getAccessToken() ?? null;
}

/** Gọi Backend REST API (/api/v1). Lỗi HTTP → ném `ApiError` chứa Problem Details. */
export async function apiFetch<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const auth: AuthAttempt = {
    token: initialToken(options),
    skipRefresh: options.skipAuthRefresh ?? false,
    refreshed: false,
  };
  const { body } = options;
  let payload: { body: BodyInit } | { json: unknown } | undefined;
  if (body !== undefined) payload = isRawBody(body) ? { body } : { json: body };

  const response = await http(path, {
    prefix: getApiBaseUrl(),
    method: options.method ?? 'GET',
    headers: { Accept: 'application/json' },
    ...payload,
    next: options.next,
    cache: options.cache,
    signal: options.signal,
    context: { auth },
  });

  if (response.status === 204) return undefined as T;
  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

/** Hàm setError của react-hook-form (thu hẹp để lib/ không phụ thuộc react-hook-form). */
type SetFieldError<TField extends string> = (field: TField, error: { message: string }) => void;

/**
 * D-11 / MT-08: lỗi validation của Backend trả **400** kèm `errors{}` (RFC 7807).
 * Ánh xạ `errors{}` → từng ô của form, chỉ nhận những field mà form khai báo.
 * Trả về `true` nếu đã xử lý (form đã hiện lỗi từng ô), `false` nếu caller cần hiện lỗi chung.
 */
export function mapProblemDetailsToForm<TField extends string>(
  error: unknown,
  fields: readonly TField[],
  setError: SetFieldError<TField>,
): boolean {
  if (!(error instanceof ApiError) || error.status !== 400) return false;

  const entries = Object.entries(error.fieldErrors).filter(([field]) =>
    (fields as readonly string[]).includes(field),
  );
  if (entries.length === 0) return false;

  entries.forEach(([field, messages]) => {
    if (messages[0]) setError(field as TField, { message: messages[0] });
  });
  return true;
}

/** Thông báo tiếng Việt theo mã lỗi (NFR-USE-003) — lớp vỏ mỏng của `normalizeError`, giữ cho code Buổi 2 – 5. */
export function getErrorMessage(error: unknown): string {
  return normalizeError(error).message;
}
