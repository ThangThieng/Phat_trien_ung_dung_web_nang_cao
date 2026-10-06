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
  const headers: Record<string, string> = { Accept: 'application/json' };
  if (options.body !== undefined) headers['Content-Type'] = 'application/json';
  if (options.accessToken) headers.Authorization = `Bearer ${options.accessToken}`;

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    next: options.next,
    cache: options.cache,
    signal: options.signal,
  });

  if (!response.ok) {
    throw new ApiError(response.status, await parseProblem(response));
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
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

/** Thông báo thân thiện cho lỗi mạng / lỗi 5xx (không lộ chi tiết kỹ thuật – NFR-USE-003). */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status >= 500) return 'Hệ thống đang gặp sự cố. Vui lòng thử lại sau.';
    return error.message;
  }
  return 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng.';
}
