/** Kiểu dùng chung cho mọi miền – khớp DTO của Backend (.NET), JSON camelCase. Chủ file: Dev 4. */

/** SRS §5.2 – phiên bản API nằm trên URL (`/api/v1/...`). */
export type ApiVersion = 'v1';

/** MT-42 – `PagedResult<T>` của Backend (FR-RCP-001, FR-SRCH-004): phân trang offset. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/** FR-FILE-001 – response của `POST /files/upload` (`FileUploadResultDto`). */
export interface UploadedFile {
  key: string;
  url: string;
  contentType: string;
  size: number;
}

/**
 * Application Error Codes – SRS Phụ lục B. Phải khớp đúng tập hằng trong
 * `backend/src/CulinaryBlog.Domain/Exceptions/ErrorCodes.cs` (kiểm thử `types/__tests__/error-codes.test.ts`).
 * `AUTH_PASSWORD_RESET_TOKEN_INVALID` (CR-2026-05, mã thứ 30) được thêm cùng lúc ở hai phía khi API của
 * FR-AUTH-011 ra đời (Buổi 7 — Dev 1), để Frontend không khai báo trước một mã Backend chưa bao giờ trả.
 */
export const ERROR_CODES = [
  // Auth
  'AUTH_EMAIL_EXISTS',
  'AUTH_INVALID_CREDENTIALS',
  'AUTH_TOKEN_EXPIRED',
  'AUTH_TOKEN_INVALID',
  'AUTH_REFRESH_TOKEN_EXPIRED',
  'AUTH_REFRESH_TOKEN_REVOKED',
  'AUTH_GOOGLE_TOKEN_INVALID',
  'AUTH_GOOGLE_UNAVAILABLE',
  'AUTH_ACCOUNT_DISABLED',
  'AUTH_ACCOUNT_LOCKED',
  'AUTH_USER_NOT_FOUND',
  // Recipe
  'RECIPE_NOT_FOUND',
  'RECIPE_SLUG_EXISTS',
  'RECIPE_PUBLISH_INCOMPLETE',
  'RECIPE_FORBIDDEN',
  'RECIPE_CONCURRENCY_CONFLICT',
  'RECIPE_INVALID_STATE_TRANSITION',
  'INGREDIENT_QUANTITY_REQUIRED',
  // Category
  'CATEGORY_NOT_FOUND',
  'CATEGORY_NAME_EXISTS',
  'CATEGORY_DELETE_HAS_RECIPES',
  // File
  'FILE_SIZE_EXCEEDED',
  'FILE_MIME_INVALID',
  'FILE_FORBIDDEN',
  'FILE_STORAGE_UNAVAILABLE',
  // Common
  'VALIDATION_ERROR',
  'RATE_LIMIT_EXCEEDED',
  'INTERNAL_ERROR',
  'CONCURRENCY_CONFLICT',
] as const;

export type ErrorCode = (typeof ERROR_CODES)[number];

/** Thu hẹp `type` của Problem Details về mã đã biết; mã lạ (trang lỗi của proxy, lỗi chưa ánh xạ) → `false`. */
export function isErrorCode(value: unknown): value is ErrorCode {
  return typeof value === 'string' && (ERROR_CODES as readonly string[]).includes(value);
}

/** RFC 7807 Problem Details do Backend trả về (`type` = Application Error Code – SRS Phụ lục B). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  [key: string]: unknown;
}

/** D-11 / MT-08 – lỗi kiểm tra hợp lệ: 400 `VALIDATION_ERROR` kèm lỗi theo từng trường. */
export interface ValidationProblemDetails extends ProblemDetails {
  type: 'VALIDATION_ERROR';
  status: 400;
  errors: Record<string, string[]>;
}
