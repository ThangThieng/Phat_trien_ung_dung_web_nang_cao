import type { ErrorCode } from '@/types/api';

/** Nhóm lỗi quyết định CÁCH giao diện phản ứng (lỗi từng ô, toast, dialog, chuyển trang) — không phụ thuộc câu chữ. */
export type ErrorKind =
  | 'validation'
  | 'unauthenticated'
  | 'forbidden'
  | 'not-found'
  | 'conflict'
  | 'rate-limited'
  | 'server'
  | 'network'
  | 'unknown';

/** Mã do Frontend tự đặt khi không có Problem Details: không tới được máy chủ, hoặc lỗi không nhận ra. */
export type ClientErrorCode = 'NETWORK_ERROR' | 'UNKNOWN_ERROR';

/**
 * NFR-USE-003 *"I18n-ready: error messages sử dụng error code"* — thông báo tiếng Việt cho ĐỦ mã của SRS Phụ lục B.
 * `satisfies Record<ErrorCode, string>`: Backend thêm mã (kiểm thử `error-codes.test.ts` buộc `ERROR_CODES` thêm theo)
 * mà quên thông báo ở đây là lỗi biên dịch. `detail` của Backend chỉ là phương án dự phòng cho mã lạ.
 */
export const ERROR_MESSAGES = {
  // Auth
  AUTH_EMAIL_EXISTS: 'Email này đã được đăng ký. Hãy đăng nhập hoặc dùng email khác.',
  AUTH_INVALID_CREDENTIALS: 'Email hoặc mật khẩu không đúng.',
  AUTH_TOKEN_EXPIRED: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
  AUTH_TOKEN_INVALID: 'Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.',
  AUTH_REFRESH_TOKEN_EXPIRED: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
  AUTH_REFRESH_TOKEN_REVOKED: 'Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.',
  AUTH_GOOGLE_TOKEN_INVALID: 'Đăng nhập Google không hợp lệ. Vui lòng thử lại.',
  AUTH_GOOGLE_UNAVAILABLE: 'Chưa kết nối được tới Google. Vui lòng thử lại sau ít phút.',
  AUTH_ACCOUNT_DISABLED: 'Tài khoản đã bị quản trị viên vô hiệu hóa.',
  AUTH_ACCOUNT_LOCKED: 'Tài khoản tạm bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau.',
  AUTH_USER_NOT_FOUND: 'Không tìm thấy người dùng này.',
  AUTH_PASSWORD_RESET_TOKEN_INVALID: 'Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.',
  // Recipe
  RECIPE_NOT_FOUND: 'Không tìm thấy công thức hoặc công thức đã bị xóa.',
  RECIPE_SLUG_EXISTS: 'Đường dẫn của công thức đã tồn tại. Hãy đổi tiêu đề.',
  RECIPE_PUBLISH_INCOMPLETE: 'Công thức cần ít nhất 1 nguyên liệu và 1 bước trước khi xuất bản.',
  RECIPE_FORBIDDEN: 'Bạn không có quyền thao tác trên công thức này.',
  RECIPE_CONCURRENCY_CONFLICT:
    'Công thức vừa được cập nhật ở nơi khác. Hãy tải lại để xem bản mới nhất.',
  RECIPE_INVALID_STATE_TRANSITION:
    'Không thể chuyển công thức sang trạng thái này từ trạng thái hiện tại.',
  INGREDIENT_QUANTITY_REQUIRED: 'Nguyên liệu cần có số lượng, mô tả lượng hoặc đơn vị.',
  // Category
  CATEGORY_NOT_FOUND: 'Không tìm thấy danh mục.',
  CATEGORY_NAME_EXISTS: 'Tên danh mục đã tồn tại.',
  CATEGORY_DELETE_HAS_RECIPES: 'Danh mục đang có công thức nên chưa xóa được.',
  // File
  FILE_SIZE_EXCEEDED: 'Tệp vượt quá giới hạn 5 MB.',
  FILE_MIME_INVALID: 'Chỉ chấp nhận ảnh JPEG, PNG, WebP hoặc AVIF.',
  FILE_FORBIDDEN: 'Bạn không có quyền xóa tệp này.',
  FILE_STORAGE_UNAVAILABLE: 'Kho ảnh tạm thời gián đoạn. Vui lòng thử lại sau.',
  // Common
  VALIDATION_ERROR: 'Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại các ô được đánh dấu.',
  RATE_LIMIT_EXCEEDED: 'Bạn thao tác quá nhanh. Vui lòng thử lại sau.',
  INTERNAL_ERROR: 'Hệ thống đang gặp sự cố. Vui lòng thử lại sau.',
  CONCURRENCY_CONFLICT: 'Dữ liệu vừa được cập nhật ở nơi khác. Hãy tải lại để xem bản mới nhất.',
} satisfies Record<ErrorCode, string>;

/** Nhóm lỗi theo mã — khớp mã HTTP ở Phụ lục B (423 khóa tài khoản xếp vào `forbidden`, 502/503 vào `server`). */
export const ERROR_KINDS = {
  AUTH_EMAIL_EXISTS: 'conflict',
  AUTH_INVALID_CREDENTIALS: 'unauthenticated',
  AUTH_TOKEN_EXPIRED: 'unauthenticated',
  AUTH_TOKEN_INVALID: 'unauthenticated',
  AUTH_REFRESH_TOKEN_EXPIRED: 'unauthenticated',
  AUTH_REFRESH_TOKEN_REVOKED: 'unauthenticated',
  AUTH_GOOGLE_TOKEN_INVALID: 'unauthenticated',
  AUTH_GOOGLE_UNAVAILABLE: 'server',
  AUTH_ACCOUNT_DISABLED: 'forbidden',
  AUTH_ACCOUNT_LOCKED: 'forbidden',
  AUTH_USER_NOT_FOUND: 'not-found',
  AUTH_PASSWORD_RESET_TOKEN_INVALID: 'validation',
  RECIPE_NOT_FOUND: 'not-found',
  RECIPE_SLUG_EXISTS: 'conflict',
  RECIPE_PUBLISH_INCOMPLETE: 'validation',
  RECIPE_FORBIDDEN: 'forbidden',
  RECIPE_CONCURRENCY_CONFLICT: 'conflict',
  RECIPE_INVALID_STATE_TRANSITION: 'conflict',
  INGREDIENT_QUANTITY_REQUIRED: 'validation',
  CATEGORY_NOT_FOUND: 'not-found',
  CATEGORY_NAME_EXISTS: 'conflict',
  CATEGORY_DELETE_HAS_RECIPES: 'conflict',
  FILE_SIZE_EXCEEDED: 'validation',
  FILE_MIME_INVALID: 'validation',
  FILE_FORBIDDEN: 'forbidden',
  FILE_STORAGE_UNAVAILABLE: 'server',
  VALIDATION_ERROR: 'validation',
  RATE_LIMIT_EXCEEDED: 'rate-limited',
  INTERNAL_ERROR: 'server',
  CONCURRENCY_CONFLICT: 'conflict',
} satisfies Record<ErrorCode, ErrorKind>;

export const CLIENT_ERROR_MESSAGES = {
  NETWORK_ERROR: 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng.',
  UNKNOWN_ERROR: 'Đã có lỗi xảy ra. Vui lòng thử lại.',
} satisfies Record<ClientErrorCode, string>;

/** Thông báo theo nhóm khi mã không có trong Phụ lục B và Backend cũng không gửi `detail` (ví dụ 502 từ Nginx). */
export const KIND_MESSAGES = {
  validation: ERROR_MESSAGES.VALIDATION_ERROR,
  unauthenticated: 'Vui lòng đăng nhập để tiếp tục.',
  forbidden: 'Bạn không có quyền thực hiện thao tác này.',
  'not-found': 'Không tìm thấy nội dung yêu cầu.',
  conflict: ERROR_MESSAGES.CONCURRENCY_CONFLICT,
  'rate-limited': ERROR_MESSAGES.RATE_LIMIT_EXCEEDED,
  server: ERROR_MESSAGES.INTERNAL_ERROR,
  network: CLIENT_ERROR_MESSAGES.NETWORK_ERROR,
  unknown: CLIENT_ERROR_MESSAGES.UNKNOWN_ERROR,
} satisfies Record<ErrorKind, string>;
