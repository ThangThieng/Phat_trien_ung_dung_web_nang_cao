// Buổi 8 — Dev 4: hỗn hợp truy cập dùng chung cho smoke/load/stress (NFR-PERF-001/002/003).
//
// Tỉ lệ mô phỏng một blog thật — phần lớn là Guest đọc nội dung công khai, chính là đặc điểm khiến cache công khai (MT-34) có giá trị:
//   70 % đọc (Guest):  30 % chi tiết công thức · 20 % danh sách trang chủ · 12 % trang danh mục · 8 % danh sách danh mục
//   20 % tìm kiếm (Guest): từ khóa phổ biến trong dữ liệu mẫu, 30 % kèm bộ lọc (MT-66)
//   10 % ghi (Author đã đăng nhập ở setup): 60 % tạo bản nháp có mảng nguyên liệu + bước, 40 % sửa bản nháp của chính mình
// Không bắn 100 % vào một URL: mọi request trúng cache thì p50 đẹp giả tạo. Mức độ phổ biến lệch về đầu danh sách
// (index = n·u², u ngẫu nhiên đều) — vài công thức/từ khóa "hot" được xem nhiều, phần đuôi dài ít người xem.
import http from 'k6/http';
import { check, fail, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

export const BASE_URL = __ENV.BASE_URL || 'https://nginx';
export const API = `${BASE_URL}/api/v1`;

// Tiền tố tiêu đề công thức do k6 tạo — tests/load/cleanup.sql xóa đúng các dòng này sau khi đo.
export const TITLE_PREFIX = 'K6 thử tải';

// Từ khóa có thật trong tiêu đề 100 công thức mẫu (CR-2026-03), xếp theo tần suất xuất hiện.
const KEYWORDS = ['gà', 'tôm', 'bò', 'thịt', 'bánh', 'nướng', 'cá', 'xào', 'gỏi', 'lẩu', 'chả', 'cuốn', 'trứng', 'canh', 'sốt',
  'bún', 'cua', 'kho', 'cơm', 'súp', 'nấm', 'chiên', 'hấp', 'phở', 'sườn', 'đậu', 'chay', 'chè', 'pho', 'ga'];
const DIFFICULTIES = ['Easy', 'Medium', 'Hard'];

const JSON_HEADERS = { 'Content-Type': 'application/json' };

// Chỉ request của người dùng ảo (không tính bước setup) — ngưỡng NFR-PERF-001 đặt trên hai metric này, tách theo tag kind.
const apiDuration = new Trend('api_req_duration', true);
const apiFailed = new Rate('api_req_failed');

function record(res, kind) {
  apiDuration.add(res.timings.duration, { kind });
  apiFailed.add(res.status === 0 || res.status >= 400, { kind });
}

/** Chỉ số lệch về đầu danh sách: phần tử đầu "hot", phần đuôi dài ít truy cập. */
function skewed(length) {
  const u = Math.random();
  return Math.min(length - 1, Math.floor(length * u * u));
}

function pick(items) {
  return items[Math.floor(Math.random() * items.length)];
}

function get(url, name, kind) {
  const res = http.get(url, { tags: { name, kind } });
  record(res, kind);
  check(res, { [`${name} 200`]: (r) => r.status === 200 });
  return res;
}

/** Dữ liệu dùng chung cho mọi VU: slug công thức, danh mục, token của Author (đăng nhập MỘT lần). */
export function prepare() {
  const recipes = [];
  for (let page = 1; page <= 20; page++) {
    const res = http.get(`${API}/recipes?page=${page}&pageSize=50`, { tags: { name: 'setup' } });
    if (res.status !== 200) fail(`setup: GET /recipes trả ${res.status}`);
    const body = res.json();
    body.items.forEach((r) => recipes.push(r.slug));
    if (!body.hasNextPage) break;
  }

  const catRes = http.get(`${API}/categories`, { tags: { name: 'setup' } });
  if (catRes.status !== 200) fail(`setup: GET /categories trả ${catRes.status}`);
  const categories = catRes.json().map((c) => ({ id: c.id, slug: c.slug }));

  // Author mẫu author1..N@culinaryblog.local — mật khẩu lấy từ .env.production (SEED_AUTHOR_PASSWORD), truyền qua -e, không ghi vào repo.
  const password = __ENV.AUTHOR_PASSWORD;
  const authorCount = Number(__ENV.AUTHOR_COUNT || 5);
  const authors = [];
  if (password) {
    for (let i = 1; i <= authorCount; i++) {
      const res = http.post(`${API}/auth/login`, JSON.stringify({ email: `author${i}@culinaryblog.local`, password }), {
        headers: JSON_HEADERS,
        tags: { name: 'setup' },
      });
      if (res.status !== 200) fail(`setup: đăng nhập author${i} trả ${res.status}`);
      authors.push(res.json().accessToken);
    }
  }

  if (recipes.length === 0 || categories.length === 0) fail('setup: cơ sở dữ liệu chưa có dữ liệu mẫu (CR-2026-03).');
  return { recipes, categories, authors, issuedAt: Date.now() };
}

// Access token sống 15 phút (Jwt:AccessTokenMinutes). Kịch bản dài hơn thế (stress 16+ phút) thì token của setup() hết hạn và mọi lượt
// ghi thành 401 — lỗi của kịch bản, không phải của hệ thống. Sau 12 phút mỗi người dùng ảo tự đăng nhập lại MỘT lần cho Author của mình
// (mỗi VU một token riêng; dùng chung refresh token giữa các VU sẽ kích hoạt phát hiện dùng lại — FR-AUTH-004 — và thu hồi cả họ token).
const TOKEN_RENEW_MS = 12 * 60 * 1000;
let renewed = null; // { token, at }

function authorToken(data) {
  const index = (__VU - 1) % data.authors.length;
  const now = Date.now();
  const issuedAt = renewed ? renewed.at : data.issuedAt;
  if (now - issuedAt < TOKEN_RENEW_MS) {
    return renewed ? renewed.token : data.authors[index];
  }

  const res = http.post(
    `${API}/auth/login`,
    JSON.stringify({ email: `author${index + 1}@culinaryblog.local`, password: __ENV.AUTHOR_PASSWORD }),
    { headers: JSON_HEADERS, tags: { name: 'POST /auth/login (gia hạn token)', kind: 'auth' } },
  );
  record(res, 'auth');
  if (check(res, { 'POST /auth/login 200': (r) => r.status === 200 })) {
    renewed = { token: res.json().accessToken, at: now };
    myDrafts.forEach((d) => { d.token = renewed.token; });
  }
  return renewed ? renewed.token : data.authors[index];
}

// Bản nháp do VU này tạo (để 40 % lượt ghi là "sửa bản nháp của mình"): { id, rowVersion, token }.
const myDrafts = [];

function createDraft(data, token) {
  const category = pick(data.categories);
  const n = `${__VU}-${__ITER}`;
  const payload = {
    title: `${TITLE_PREFIX} canh chua cá lóc ${n}`,
    description: 'Bản nháp do kịch bản k6 tạo để đo hiệu năng ghi; bị xóa bằng tests/load/cleanup.sql sau khi đo.',
    categoryId: category.id,
    prepTime: 15,
    cookTime: 25,
    servings: 4,
    difficulty: pick(DIFFICULTIES),
    instructions: null,
    nutrition: null,
    ingredients: [
      { name: 'Cá lóc', quantity: 500, unit: 'g' },
      { name: 'Me chua', quantity: 30, unit: 'g' },
      { name: 'Cà chua', quantity: 2, unit: 'quả' },
      { name: 'Dọc mùng', quantityText: '1 bó' },
    ],
    steps: [
      { title: 'Sơ chế', description: 'Làm sạch cá, cắt khúc; dầm me lấy nước chua.' },
      { title: 'Nấu canh', description: 'Đun sôi nước me, cho cà chua, cá rồi dọc mùng; nêm vừa ăn.' },
    ],
  };
  const res = http.post(`${API}/recipes`, JSON.stringify(payload), {
    headers: { ...JSON_HEADERS, Authorization: `Bearer ${token}` },
    tags: { name: 'POST /recipes', kind: 'write' },
  });
  record(res, 'write');
  if (check(res, { 'POST /recipes 201': (r) => r.status === 201 })) {
    const body = res.json();
    myDrafts.push({ id: body.id, rowVersion: body.rowVersion, token });
  }
}

function updateDraft(draft) {
  const res = http.put(
    `${API}/recipes/${draft.id}`,
    JSON.stringify({ title: `${TITLE_PREFIX} canh chua cá lóc ${__VU}-${__ITER} (sửa)`, rowVersion: draft.rowVersion }),
    { headers: { ...JSON_HEADERS, Authorization: `Bearer ${draft.token}` }, tags: { name: 'PUT /recipes/{id}', kind: 'write' } },
  );
  record(res, 'write');
  if (check(res, { 'PUT /recipes/{id} 200': (r) => r.status === 200 })) {
    draft.rowVersion = res.json().rowVersion;
  }
}

/** Một lượt của một người dùng ảo: chọn một hành động theo tỉ lệ 70/20/10 rồi "đọc" 0,5–1,5 giây. */
export function visit(data) {
  const r = Math.random();

  if (r < 0.30) {
    get(`${API}/recipes/${data.recipes[skewed(data.recipes.length)]}`, 'GET /recipes/{slug}', 'read');
  } else if (r < 0.50) {
    const page = Math.random() < 0.75 ? 1 : 2 + Math.floor(Math.random() * 2);
    get(`${API}/recipes?page=${page}&pageSize=12`, 'GET /recipes', 'read');
  } else if (r < 0.62) {
    const category = data.categories[skewed(data.categories.length)];
    get(`${API}/categories/${category.slug}?page=1&pageSize=12`, 'GET /categories/{slug}', 'read');
  } else if (r < 0.70) {
    get(`${API}/categories`, 'GET /categories', 'read');
  } else if (r < 0.90) {
    let url = `${API}/recipes/search?q=${encodeURIComponent(KEYWORDS[skewed(KEYWORDS.length)])}&page=1&pageSize=12`;
    if (Math.random() < 0.30) {
      url += Math.random() < 0.5 ? `&maxCookTime=${pick([30, 60])}` : `&difficulty=${pick(DIFFICULTIES)}`;
    }
    get(url, 'GET /recipes/search', 'search');
  } else if (data.authors.length > 0) {
    const token = authorToken(data);
    const mine = myDrafts.length > 0 && Math.random() < 0.40;
    if (mine) {
      updateDraft(pick(myDrafts));
    } else {
      createDraft(data, token);
    }
  } else {
    // Không có mật khẩu Author (-e AUTHOR_PASSWORD): phần ghi thay bằng đọc để tổng số request không đổi.
    get(`${API}/recipes?page=1&pageSize=12`, 'GET /recipes', 'read');
  }

  sleep(0.5 + Math.random());
}

/** Ngưỡng NFR-PERF-001: p50 ≤ 150 ms cho GET có cache, p95 ≤ 500 ms và p99 ≤ 1000 ms cho mọi request. */
export const PERF_THRESHOLDS = {
  'api_req_duration{kind:read}': ['p(50)<=150'],
  'api_req_duration{kind:search}': ['p(50)<=150'],
  'api_req_duration{kind:write}': ['p(95)<=500'],
  api_req_duration: ['p(50)<=150', 'p(95)<=500', 'p(99)<=1000'],
  api_req_failed: ['rate<0.01'],
};

export const TLS = { insecureSkipTLSVerify: true }; // chứng chỉ tự ký của ./ssl (nginx/generate-ssl-cert.sh)
