import { apiFetch } from '@/lib/api-client';
import type { PagedResult } from '@/types/api';
import type { SetUserStatusRequest, UserAdmin, UserListParams, UserStatus } from '@/types/auth';

function queryString(params: UserListParams) {
  const query = new URLSearchParams({ page: String(params.page ?? 1), pageSize: String(params.pageSize ?? 12) });
  if (params.search) query.set('search', params.search);
  if (params.isActive !== undefined) query.set('isActive', String(params.isActive));
  return query.toString();
}

/** FR-AUTH-008: administrative data is fresh and never uses Next's data cache. */
export function getUsers(params: UserListParams, signal?: AbortSignal) {
  return apiFetch<PagedResult<UserAdmin>>(`/users?${queryString(params)}`, { cache: 'no-store', signal });
}

export function setUserStatus(id: string, payload: SetUserStatusRequest, accessToken: string | null) {
  return apiFetch<UserStatus>(`/users/${encodeURIComponent(id)}/status`, { method: 'PATCH', body: payload, accessToken });
}
