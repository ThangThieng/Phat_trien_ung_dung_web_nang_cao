/**
 * @jest-environment node
 */
import type { AuthResponse, Session, User, UserAdmin, UserStatus } from '../auth';
import { ROLES } from '../auth';
import { readBackendFile, readRecordJsonProperties, readStringConstants } from '../../../test-utils/csharp-source';

/**
 * Kiểm thử hợp đồng kiểu Auth ↔ DTO C# (SRS §8.1). Hai lớp bảo vệ:
 * - `satisfies Record<keyof T, true>`: thiếu hoặc thừa khóa so với interface TypeScript → lỗi biên dịch (`npm run typecheck`);
 * - so danh sách khóa với tham số của record C# → Backend đổi DTO mà quên Frontend thì kiểm thử đỏ.
 */
const userKeys = {
  id: true, email: true, displayName: true, avatarUrl: true, bio: true, roles: true,
} satisfies Record<keyof User, true>;

const authResponseKeys = {
  accessToken: true, refreshToken: true, expiresAt: true, expiresIn: true, user: true,
} satisfies Record<keyof AuthResponse, true>;

const sessionKeys = {
  id: true, createdAt: true, createdByIp: true, expiresAt: true, isCurrent: true,
} satisfies Record<keyof Session, true>;

const userAdminKeys = {
  id: true, email: true, displayName: true, avatarUrl: true, roles: true, isActive: true, createdAt: true, recipeCount: true,
} satisfies Record<keyof UserAdmin, true>;

const userStatusKeys = {
  id: true, email: true, displayName: true, isActive: true,
} satisfies Record<keyof UserStatus, true>;

const AUTH = 'src/CulinaryBlog.Application/Features/Auth';

describe.each([
  ['User', userKeys, `${AUTH}/AuthContracts.cs`, 'UserDto'],
  ['AuthResponse', authResponseKeys, `${AUTH}/AuthContracts.cs`, 'AuthResponseDto'],
  ['Session', sessionKeys, `${AUTH}/SessionCommands.cs`, 'SessionDto'],
  ['UserAdmin', userAdminKeys, `${AUTH}/UserAdministration.cs`, 'UserAdminDto'],
  ['UserStatus', userStatusKeys, `${AUTH}/UserAdministration.cs`, 'UserStatusDto'],
])('%s matches backend %s', (_name, keys, file, record) => {
  it(`has the same JSON properties as ${record}`, () => {
    const backendProperties = readRecordJsonProperties(readBackendFile(file), record);
    expect(Object.keys(keys).sort()).toEqual([...backendProperties].sort());
  });
});

describe('ROLES', () => {
  it('equals the role names of the backend Roles class (values found in the JWT and UserDto.roles)', () => {
    const source = readBackendFile('src/CulinaryBlog.Application/Common/Interfaces/ICurrentUser.cs');
    const rolesClass = source.slice(source.indexOf('class Roles'));
    expect([...ROLES].sort()).toEqual(readStringConstants(rolesClass).sort());
  });
});
