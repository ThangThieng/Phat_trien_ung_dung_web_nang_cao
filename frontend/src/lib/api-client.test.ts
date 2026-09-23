import { ApiError, mapProblemDetailsToForm } from './api-client';

const FIELDS = ['email', 'password'] as const;

function makeValidationError(status: number, errors: Record<string, string[]>) {
  return new ApiError(status, {
    type: 'VALIDATION_ERROR',
    title: 'Dữ liệu không hợp lệ',
    status,
    detail: 'Một hoặc nhiều trường không hợp lệ. Xem "errors".',
    errors,
  });
}

describe('mapProblemDetailsToForm (D-11 / MT-08)', () => {
  it('ánh xạ errors{} của lỗi 400 vào đúng từng ô của form', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(400, { email: ['Email không hợp lệ.'], password: ['Mật khẩu quá ngắn.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(true);
    expect(setError).toHaveBeenCalledWith('email', { message: 'Email không hợp lệ.' });
    expect(setError).toHaveBeenCalledWith('password', { message: 'Mật khẩu quá ngắn.' });
  });

  it('bỏ qua field mà form không khai báo', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(400, { userName: ['Không hợp lệ.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('không xử lý 422 nữa – mã này đã bị loại bỏ khỏi hệ thống', () => {
    const setError = jest.fn();
    const handled = mapProblemDetailsToForm(
      makeValidationError(422, { email: ['Email không hợp lệ.'] }),
      FIELDS,
      setError,
    );

    expect(handled).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('trả false cho lỗi không phải validation (401) để caller hiện lỗi chung', () => {
    const setError = jest.fn();
    const error = new ApiError(401, {
      type: 'AUTH_INVALID_CREDENTIALS',
      detail: 'Email hoặc mật khẩu không đúng.',
    });

    expect(mapProblemDetailsToForm(error, FIELDS, setError)).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });

  it('trả false cho lỗi mạng (không phải ApiError)', () => {
    const setError = jest.fn();

    expect(mapProblemDetailsToForm(new TypeError('fetch failed'), FIELDS, setError)).toBe(false);
    expect(setError).not.toHaveBeenCalled();
  });
});
