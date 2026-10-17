import fs from 'node:fs';
import path from 'node:path';

/**
 * Dùng cho kiểm thử hợp đồng (`types/__tests__/*-contract.test.ts`): lấy danh sách khóa JSON của một DTO Backend.
 * Nguồn 1 (ưu tiên): `docs/api/openapi.v1.json` – ảnh chụp OpenAPI của Dev 2 (`components.schemas.<Dto>.properties`).
 * Nguồn 2 (khi file đó chưa có): đọc thẳng khai báo `record` C# trong `backend/` – cùng là nguồn sự thật của hợp đồng.
 * Không tìm thấy nguồn nào (ví dụ build Docker chỉ copy `frontend/`) → trả `null` để test tự bỏ qua, không đỏ oan.
 */
const REPO_ROOT = path.resolve(process.cwd(), '..');
const OPENAPI_SNAPSHOT = path.join(REPO_ROOT, 'docs', 'api', 'openapi.v1.json');
const FEATURES = path.join(REPO_ROOT, 'backend', 'src', 'CulinaryBlog.Application', 'Features');

const camel = (name: string) => name.charAt(0).toLowerCase() + name.slice(1);

function readIfExists(file: string): string | null {
  return fs.existsSync(file) ? fs.readFileSync(file, 'utf8') : null;
}

function openApiKeys(schema: string): string[] | null {
  const text = readIfExists(OPENAPI_SNAPSHOT);
  if (!text) return null;
  const properties = JSON.parse(text)?.components?.schemas?.[schema]?.properties;
  return properties ? Object.keys(properties) : null;
}

/** Tách tham số vị trí của `record Name(...)` + thuộc tính tính toán `public T X => ...` trong thân record. */
function recordKeys(source: string, record: string): string[] | null {
  const start = source.search(new RegExp(`record\\s+${record}\\s*\\(`));
  if (start < 0) return null;
  const open = source.indexOf('(', start);

  let depth = 0;
  let generic = 0;
  let end = -1;
  const params: string[] = [];
  let current = '';
  for (let i = open; i < source.length; i += 1) {
    const ch = source[i];
    if (ch === '(') depth += 1;
    if (ch === '<') generic += 1;
    if (ch === '>') generic -= 1;
    if (ch === ')') {
      depth -= 1;
      if (depth === 0) {
        end = i;
        break;
      }
    }
    if (ch === ',' && depth === 1 && generic === 0) {
      params.push(current);
      current = '';
    } else if (i > open) {
      current += ch;
    }
  }
  if (end < 0) return null;
  params.push(current);

  const keys = params
    .map((p) => p.split('=')[0].trim().split(/\s+/).pop() ?? '')
    .filter(Boolean)
    .map(camel);

  const nextRecord = source
    .slice(end)
    .search(/\n\s*public\s+(sealed\s+)?(record|static\s+class|class)\s/);
  const body = source.slice(end, nextRecord < 0 ? undefined : end + nextRecord);
  Array.from(body.matchAll(/public\s+[\w<>?,\s]+?\s(\w+)\s*=>/g)).forEach((m) =>
    keys.push(camel(m[1])),
  );
  return keys;
}

/** Khóa JSON của DTO Backend, hoặc `null` nếu không có nguồn nào để so. */
export function backendDtoKeys(schema: string, csFile: string): string[] | null {
  const fromOpenApi = openApiKeys(schema);
  if (fromOpenApi) return fromOpenApi;
  const source = readIfExists(path.join(FEATURES, csFile));
  return source ? recordKeys(source, schema) : null;
}

/** Giá trị `public const int Name = 12;` / `public const string Name = "x";` trong file C#, hoặc `null`. */
export function backendConst(csFile: string, name: string): string | number | null {
  const source = readIfExists(path.join(FEATURES, csFile));
  const match = source?.match(new RegExp(`const\\s+\\w+\\s+${name}\\s*=\\s*("([^"]*)"|\\d+)\\s*;`));
  if (!match) return null;
  return match[2] !== undefined ? match[2] : Number(match[1]);
}

/** Các khóa chuỗi trong `Columns` của `SortMapper` (whitelist sắp xếp). */
export function backendSortColumns(): string[] | null {
  const source = readIfExists(path.join(FEATURES, 'Recipes', 'SortMapper.cs'));
  if (!source) return null;
  const block = source.slice(source.indexOf('Columns'));
  return Array.from(block.matchAll(/\["(\w+)"\]\s*=/g)).map((m) => m[1]);
}
