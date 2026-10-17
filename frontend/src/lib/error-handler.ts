import { toast } from 'sonner';
import { isAbortError } from './api-error';
import { mapProblemDetailsToForm } from './api-client';
import { normalizeError } from './normalize-error';
import type { AppError } from './normalize-error';

export { normalizeError, kindFromStatus } from './normalize-error';
export type { AppError } from './normalize-error';
export type { ErrorKind } from './error-messages';

type SetFieldError<TField extends string> = (field: TField, error: { message: string }) => void;

export interface HandleApiErrorOptions<TField extends string> {
  /** Có biểu mẫu → lỗi 400 kèm `errors{}` hiện dưới đúng ô (D-11); ô không khớp thì mới toast. */
  form?: { fields: readonly TField[]; setError: SetFieldError<TField> };
  /** Chỉ chuẩn hóa, không hiển thị gì — nơi gọi tự quyết định giao diện. */
  silent?: boolean;
}

/** `id` gom các lỗi giống nhau: 5 truy vấn cùng mất mạng chỉ hiện MỘT toast. */
const toastId = (error: AppError) => `api-error:${error.code}:${error.status ?? 0}`;

/**
 * Bộ xử lý lỗi dùng chung (Lab 6 – Dev 4). `QueryCache`/`MutationCache` gọi hàm này cho mọi truy vấn, nên màn hình mới
 * tự có xử lý đúng cho 429, 5xx, mất mạng. Trả lại `AppError` để nơi gọi rẽ nhánh tiếp (ví dụ dialog cho `conflict`).
 */
export function handleApiError<TField extends string = string>(
  error: unknown,
  options: HandleApiErrorOptions<TField> = {},
): AppError {
  const appError = normalizeError(error);
  if (options.silent || isAbortError(error)) return appError;

  const id = toastId(appError);
  switch (appError.kind) {
    case 'validation':
      if (
        options.form &&
        mapProblemDetailsToForm(error, options.form.fields, options.form.setError)
      ) {
        break;
      }
      toast.error(appError.message, { id });
      break;
    case 'rate-limited':
      toast.warning(appError.message, { id });
      break;
    case 'server':
    case 'network':
      // NFR-USE-003: câu thân thiện + mã tra cứu để đối chiếu log, không bao giờ hiện stack trace.
      toast.error(appError.message, {
        id,
        description: appError.correlationId ? `Mã tra cứu: ${appError.correlationId}` : undefined,
      });
      break;
    default:
      // forbidden, not-found, unauthenticated, conflict, unknown: câu theo mã lỗi. Màn hình cần dialog riêng cho
      // `conflict` (trang sửa công thức) khai báo `meta: { suppressGlobalError: true }` rồi tự xử lý.
      toast.error(appError.message, { id });
  }
  return appError;
}
