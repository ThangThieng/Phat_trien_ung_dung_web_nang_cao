import { toast } from 'sonner';
import { ERROR_CODES } from '@/types/api';
import { ApiError } from './api-error';
import { handleApiError, normalizeError } from './error-handler';
import { ERROR_KINDS, ERROR_MESSAGES } from './error-messages';

jest.mock('sonner', () => ({ toast: { error: jest.fn(), warning: jest.fn() } }));

const toastError = toast.error as jest.Mock;
const toastWarning = toast.warning as jest.Mock;

const STATUS_BY_KIND = {
  validation: 400,
  unauthenticated: 401,
  forbidden: 403,
  'not-found': 404,
  conflict: 409,
  'rate-limited': 429,
  server: 500,
} as const;

function apiError(code: string, status: number, extra: Record<string, unknown> = {}, meta = {}) {
  return new ApiError(
    status,
    { type: code, status, detail: 'detail from backend', ...extra },
    meta,
  );
}

beforeEach(() => {
  toastError.mockClear();
  toastWarning.mockClear();
});

describe('normalizeError', () => {
  it.each(ERROR_CODES)('maps %s to a kind and a Vietnamese message', (code) => {
    const kind = ERROR_KINDS[code];
    const status = STATUS_BY_KIND[kind as keyof typeof STATUS_BY_KIND] ?? 500;
    const result = normalizeError(apiError(code, status));
    expect(result).toMatchObject({ code, kind, status });
    expect(result.message).toBe(ERROR_MESSAGES[code]);
    expect(result.message).not.toBe('detail from backend');
  });

  it('reads Retry-After seconds into the rate-limit message', () => {
    const result = normalizeError(
      apiError('RATE_LIMIT_EXCEEDED', 429, {}, { retryAfterSeconds: 42 }),
    );
    expect(result).toMatchObject({ kind: 'rate-limited', retryAfterSeconds: 42 });
    expect(result.message).toBe('Bạn thao tác quá nhanh, thử lại sau 42 giây.');
  });

  it('falls back to the backend detail for an unknown 4xx code', () => {
    const result = normalizeError(apiError('SOMETHING_NEW', 404));
    expect(result).toMatchObject({
      code: 'UNKNOWN_ERROR',
      kind: 'not-found',
      message: 'detail from backend',
    });
  });

  it('never shows the backend detail of an unknown 5xx (no technical leak)', () => {
    const result = normalizeError(
      new ApiError(502, { title: 'Bad Gateway', detail: 'upstream api:8080 refused' }),
    );
    expect(result.kind).toBe('server');
    expect(result.message).not.toContain('api:8080');
  });

  it.each([
    ['fetch failure', new TypeError('Failed to fetch')],
    ['ky timeout', Object.assign(new Error('Request timed out'), { name: 'TimeoutError' })],
  ])('treats a %s as a network error', (_label, error) => {
    expect(normalizeError(error)).toMatchObject({ kind: 'network', code: 'NETWORK_ERROR' });
  });

  it('keeps field errors and the correlation id', () => {
    const result = normalizeError(
      apiError('VALIDATION_ERROR', 400, { errors: { title: ['Bắt buộc'] }, traceId: 't-1' }),
    );
    expect(result.fieldErrors).toEqual({ title: ['Bắt buộc'] });
    expect(result.correlationId).toBe('t-1');
  });

  it('treats anything else as unknown', () => {
    expect(normalizeError('boom')).toMatchObject({ kind: 'unknown', code: 'UNKNOWN_ERROR' });
  });
});

describe('handleApiError', () => {
  const fields = ['title', 'description'] as const;

  it('puts 400 field errors under the matching inputs and does not toast', () => {
    const setError = jest.fn();
    handleApiError(apiError('VALIDATION_ERROR', 400, { errors: { title: ['Tiêu đề quá ngắn'] } }), {
      form: { fields, setError },
    });
    expect(setError).toHaveBeenCalledWith('title', { message: 'Tiêu đề quá ngắn' });
    expect(toastError).not.toHaveBeenCalled();
  });

  it('toasts a 400 whose fields are not on the form', () => {
    handleApiError(apiError('RECIPE_PUBLISH_INCOMPLETE', 400), {
      form: { fields, setError: jest.fn() },
    });
    expect(toastError).toHaveBeenCalledWith(
      ERROR_MESSAGES.RECIPE_PUBLISH_INCOMPLETE,
      expect.any(Object),
    );
  });

  it('warns with the waiting time on 429', () => {
    handleApiError(apiError('RATE_LIMIT_EXCEEDED', 429, {}, { retryAfterSeconds: 3 }));
    expect(toastWarning).toHaveBeenCalledWith(
      'Bạn thao tác quá nhanh, thử lại sau 3 giây.',
      expect.any(Object),
    );
  });

  it('shows a friendly message plus the lookup code on 5xx', () => {
    handleApiError(apiError('INTERNAL_ERROR', 500, { traceId: '00-abc-01' }));
    expect(toastError).toHaveBeenCalledWith(
      ERROR_MESSAGES.INTERNAL_ERROR,
      expect.objectContaining({
        description: 'Mã tra cứu: 00-abc-01',
      }),
    );
  });

  it('shows the permission message on 403', () => {
    handleApiError(apiError('RECIPE_FORBIDDEN', 403));
    expect(toastError).toHaveBeenCalledWith(ERROR_MESSAGES.RECIPE_FORBIDDEN, expect.any(Object));
  });

  it('collapses identical errors into one toast id', () => {
    handleApiError(new TypeError('Failed to fetch'));
    handleApiError(new TypeError('Failed to fetch'));
    const ids = toastError.mock.calls.map(([, options]) => (options as { id: string }).id);
    expect(new Set(ids).size).toBe(1);
  });

  it('stays quiet for aborted requests and in silent mode, but still returns the normalized error', () => {
    expect(handleApiError(new DOMException('aborted', 'AbortError')).kind).toBe('unknown');
    expect(
      handleApiError(apiError('RECIPE_CONCURRENCY_CONFLICT', 409), { silent: true }).kind,
    ).toBe('conflict');
    expect(toastError).not.toHaveBeenCalled();
  });
});
