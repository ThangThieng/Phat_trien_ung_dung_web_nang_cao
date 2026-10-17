import { readFileSync } from 'fs';
import path from 'path';

/** Gốc monorepo: frontend/test-utils → ../.. */
const REPO_ROOT = path.resolve(__dirname, '../..');

export function readBackendFile(relativePath: string): string {
  return readFileSync(path.join(REPO_ROOT, 'backend', relativePath), 'utf8');
}

/** Mọi giá trị `public const string X = "...";` trong một file C#. */
export function readStringConstants(source: string): string[] {
  return [...source.matchAll(/public\s+const\s+string\s+\w+\s*=\s*"([^"]+)"\s*;/g)].map((match) => match[1]);
}

/** Tách theo dấu phẩy ở cấp ngoài cùng (bỏ qua dấu phẩy trong `<...>`, `(...)`). */
function splitTopLevel(parameters: string): string[] {
  const parts: string[] = [];
  let depth = 0;
  let current = '';
  [...parameters].forEach((char) => {
    if (char === '<' || char === '(') depth += 1;
    if (char === '>' || char === ')') depth -= 1;
    if (char === ',' && depth === 0) {
      parts.push(current);
      current = '';
    } else {
      current += char;
    }
  });
  if (current.trim()) parts.push(current);
  return parts;
}

/**
 * Tên thuộc tính JSON (camelCase – cấu hình mặc định của System.Text.Json trong ASP.NET Core)
 * của một positional record C#, ví dụ `record UserDto(string Id, string? AvatarUrl, ...)` → `['id', 'avatarUrl', ...]`.
 */
export function readRecordJsonProperties(source: string, recordName: string): string[] {
  const start = source.search(new RegExp(`\\brecord\\s+${recordName}\\s*\\(`));
  if (start < 0) throw new Error(`record ${recordName} not found`);
  const open = source.indexOf('(', start);
  let depth = 0;
  let end = open;
  for (; end < source.length; end += 1) {
    if (source[end] === '(') depth += 1;
    if (source[end] === ')') depth -= 1;
    if (depth === 0) break;
  }
  return splitTopLevel(source.slice(open + 1, end)).map((parameter) => {
    const name = parameter.replace(/\[[^\]]*\]/g, '').trim().split(/\s+/).pop() ?? '';
    return name.charAt(0).toLowerCase() + name.slice(1);
  });
}
