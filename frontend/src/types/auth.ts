/** Kiểu miền Auth – khớp DTO của Backend (.NET), JSON camelCase. Chủ file: Dev 4 (module Auth của Dev 1). */

/** SRS §8.1 – UserDto `{ id, email, displayName, avatarUrl, bio, roles }` (D-1). */
export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  expiresIn: number;
  user: User;
}
