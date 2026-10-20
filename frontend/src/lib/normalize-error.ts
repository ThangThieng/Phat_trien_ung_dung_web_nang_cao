import { isErrorCode } from '@/types/api';
import type { ErrorCode } from '@/types/api';
import { ApiError, isAbortError } from './api-error';
import {
  CLIENT_ERROR_MESSAGES,
  ERROR_KINDS,
  ERROR_MESSAGES,
  KIND_MESSAGES,
} from './error-messages';
import type { ClientErrorCode, ErrorKind } from './error-messages';

/** Một dạng lỗi duy nhất cho mọi màn hình, dù lỗi đến từ Backend, mạng hay code phía trình duyệt. */
export interface AppError {
  kind: ErrorKind;
  code: ErrorCode | ClientErrorCode;
  /** Mã HTTP; không có khi request không tới được máy chủ. */
  status?: number;
  /** Câu tiếng Việt để hiển thị — không bao giờ chứa stack trace hay câu lỗi của framework (NFR-USE-003). */
  message: string;
  fieldErrors: Record<string, string[]>;
  retryAfterSeconds?: number;
  correlationId?: string;
}

/** Mã HTTP → nhóm lỗi, dùng khi `type` của Problem Details không phải mã Phụ lục B (trang lỗi của Nginx, 413 thô…). */
export function kindFromStatus(status: number): ErrorKind {
  if (status === 400 || status === 413) return 'validation';
  if (status === 401) return 'unauthenticated';
  if (status === 403 || status === 423) return 'forbidden';
  if (status === 404 || status === 410) return 'not-found';
  if (status === 409 || status === 412) return 'conflict';
  if (status === 429) return 'rate-limited';
  if (status >= 500) return 'server';
  return 'unknown';
}

const NETWORK_ERROR_NAMES = new Set(['TypeError', 'NetworkError', 'TimeoutError']);

function fromApiError(error: ApiError): AppError {
  const base = {
    status: error.status,
    fieldErrors: error.fieldErrors,
    retryAfterSeconds: error.retryAfterSeconds,
    correlationId: error.correlationId,
  };
  if (isErrorCode(error.code)) {
    const kind: ErrorKind = ERROR_KINDS[error.code];
    let message: string = ERROR_MESSAGES[error.code];
    if (kind === 'rate-limited' && error.retryAfterSeconds !== undefined) {
      message = `Bạn thao tác quá nhanh, thử lại sau ${Math.max(1, error.retryAfterSeconds)} giây.`;
    }
    return { ...base, kind, code: error.code, message };
  }
  const kind = kindFromStatus(error.status);
  // Mã lạ: `detail` của Backend chỉ dùng cho lỗi 4xx; lỗi 5xx luôn là câu chung để không lộ chi tiết kỹ thuật.
  const detail = error.status < 500 ? error.problem.detail : undefined;
  return { ...base, kind, code: 'UNKNOWN_ERROR', message: detail || KIND_MESSAGES[kind] };
}

export function normalizeError(error: unknown): AppError {
  if (error instanceof ApiError) return fromApiError(error);
  if (!isAbortError(error) && error instanceof Error && NETWORK_ERROR_NAMES.has(error.name)) {
    return {
      kind: 'network',
      code: 'NETWORK_ERROR',
      message: CLIENT_ERROR_MESSAGES.NETWORK_ERROR,
      fieldErrors: {},
    };
  }
  return {
    kind: 'unknown',
    code: 'UNKNOWN_ERROR',
    message: CLIENT_ERROR_MESSAGES.UNKNOWN_ERROR,
    fieldErrors: {},
  };
}
