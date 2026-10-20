/**
 * @jest-environment node
 */
import { ERROR_CODES, isErrorCode } from '../api';
import { readBackendFile, readStringConstants } from '../../../test-utils/csharp-source';

/**
 * Chống lệch mã lỗi Frontend ↔ Backend (NFR-USE-003, SRS Phụ lục B). Frontend xử lý lỗi theo `type`, không theo
 * câu chữ; Backend thêm mã mà `ERROR_CODES` chưa có (hoặc ngược lại) thì kiểm thử này đỏ ngay trong PR.
 */
describe('ERROR_CODES mirrors backend ErrorCodes.cs', () => {
  const backendCodes = readStringConstants(readBackendFile('src/CulinaryBlog.Domain/Exceptions/ErrorCodes.cs'));

  it('reads the backend catalogue (guards against a regex that silently matches nothing)', () => {
    expect(backendCodes.length).toBeGreaterThanOrEqual(29);
  });

  it('declares exactly the same set of codes as the backend', () => {
    const missingInFrontend = backendCodes.filter((code) => !(ERROR_CODES as readonly string[]).includes(code));
    const unknownToBackend = ERROR_CODES.filter((code) => !backendCodes.includes(code));
    expect({ missingInFrontend, unknownToBackend }).toEqual({ missingInFrontend: [], unknownToBackend: [] });
  });

  it('has no duplicate codes on either side', () => {
    expect(new Set(ERROR_CODES).size).toBe(ERROR_CODES.length);
    expect(new Set(backendCodes).size).toBe(backendCodes.length);
  });

  it('narrows Problem Details type values to known codes only', () => {
    expect(isErrorCode('RECIPE_CONCURRENCY_CONFLICT')).toBe(true);
    expect(isErrorCode('about:blank')).toBe(false);
    expect(isErrorCode(undefined)).toBe(false);
  });
});
