/** Kiểu miền Auth – khớp DTO của Backend (.NET), JSON camelCase (SRS §8.1). Chủ file: Dev 4 (module Auth của Dev 1). */

/** SRS §2.3 – vai trò trong JWT và `UserDto.roles` (Guest là người chưa đăng nhập, không phải role). */
export const ROLES = ['Author', 'Admin'] as const;

export type Role = (typeof ROLES)[number];

/** SRS §8.1 – UserDto `{ id, email, displayName, avatarUrl, bio, roles }` (D-1). */
export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  roles: Role[];
}

/** SRS §8.1 – `AuthResponseDto` của register/login/google/refresh. */
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  expiresIn: number;
  user: User;
}

/** `GET /auth/sessions` (FR-AUTH-009) – `SessionDto`, không bao giờ có `tokenHash`. */
export interface Session {
  id: string;
  createdAt: string;
  createdByIp: string | null;
  expiresAt: string;
  isCurrent: boolean;
}

/** `GET /users` (FR-AUTH-008) – `UserAdminDto`, không có PasswordHash/SecurityStamp/UserName. */
export interface UserAdmin {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  roles: Role[];
  isActive: boolean;
  createdAt: string;
  recipeCount: number;
}

/** Query string của `GET /users`. */
export interface UserListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

/** `PATCH /users/{id}/status` (FR-AUTH-008). */
export interface SetUserStatusRequest {
  isActive: boolean;
  reason?: string;
}

/** Response của `PATCH /users/{id}/status` – `UserStatusDto`. */
export interface UserStatus {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
}

