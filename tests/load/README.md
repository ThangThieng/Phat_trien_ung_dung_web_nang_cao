# Load test k6 — Culinary Blog (Buổi 8, NFR-PERF-001/002/003, NFR-REL-002)

Kết quả đo và phân tích: [`docs/load-test-report.md`](../../docs/load-test-report.md) · audit cache + hit rate: [`docs/cache-report.md`](../../docs/cache-report.md).

## Kịch bản

| File | Tải | Mục đích |
|---|---|---|
| `smoke.js` | 1 người dùng ảo, 1 phút | Kịch bản và môi trường chạy đúng; mọi request phải thành công |
| `load.js` | tăng 1 phút → **100 người dùng đồng thời** 5 phút → giảm 30 giây | NFR-PERF-001 (p50 ≤ 150 ms, p95 ≤ 500 ms, p99 ≤ 1000 ms), NFR-PERF-002 (≥ 100 người dùng), hit rate ≥ 80 % |
| `stress.js` | 200 → 400 → 600 → 800 → 1.000 người dùng, mỗi nấc giữ 2 phút | Tìm ngưỡng gãy: nấc đầu tiên p95 > 500 ms hoặc lỗi > 1 % |

Mỗi người dùng ảo lặp: chọn một hành động theo tỉ lệ **70 % đọc / 20 % tìm kiếm / 10 % ghi** (chi tiết trong `lib/traffic.js`), rồi "đọc trang"
0,5–1,5 giây. Phần ghi đăng nhập sẵn 5 Author mẫu ở `setup()` rồi tạo bản nháp có nguyên liệu + bước, hoặc sửa bản nháp của chính mình. Không
bắn 100 % vào một URL — mọi request trúng cache thì p50 đẹp giả tạo.

Ngưỡng đặt trên metric riêng `api_req_duration` / `api_req_failed` (chỉ request của người dùng ảo, không tính bước `setup()`), tách theo tag
`kind` = `read` | `search` | `write`.

## Môi trường đo

`docker-compose.loadtest.yml` là lớp phủ của `docker-compose.prod.yml`: cùng project `culinaryblog`, 3 bản sao API, Nginx HTTPS, cùng dữ liệu
mẫu — chỉ khác ở **hạn mức rate limit được nâng ở cả hai lớp** (Nginx và API). Lý do: k6 chạy từ một máy nên mọi request cùng một IP; giữ hạn mức
thật (100/phút cho `/api/`, 10/phút cho `/auth/*`) thì từ giây thứ hai hầu hết request là 429. Rate limit có kiểm thử riêng trên hạn mức thật
(Buổi 5, Buổi 7, Buổi 8 — Dev 1). k6 chạy **trong** mạng Docker và đi qua Nginx (`https://nginx`, chứng chỉ tự ký).

```bash
# Dừng bản dev rồi dựng môi trường đo (một project, một bộ container)
docker compose --profile dev down
docker compose -f docker-compose.prod.yml -f docker-compose.loadtest.yml --env-file .env.production up -d --build

# Chạy (mật khẩu Author mẫu lấy từ .env.production, không ghi vào repo)
set -a; . ./.env.production; set +a
dc="docker compose -f docker-compose.prod.yml -f docker-compose.loadtest.yml --env-file .env.production --profile loadtest"
docker exec culinaryblog-redis-1 redis-cli CONFIG RESETSTAT        # đo hit rate từ 0
$dc run --rm -e AUTHOR_PASSWORD="$SEED_AUTHOR_PASSWORD" k6 run \
  --summary-trend-stats "avg,min,med,p(90),p(95),p(99),max" \
  --summary-export /results/load-summary.json --out csv=/results/load.csv.gz /scripts/load.js
docker exec culinaryblog-redis-1 redis-cli INFO stats | grep keyspace_   # hit rate = hits / (hits + misses)

# Dọn bản nháp do k6 tạo (tiêu đề "K6 thử tải …") — dữ liệu mẫu giữ nguyên
docker compose -f docker-compose.prod.yml --env-file .env.production exec -T postgres \
  sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB"' < tests/load/cleanup.sql

# Trả về bản dev
docker compose -f docker-compose.prod.yml -f docker-compose.loadtest.yml --env-file .env.production down
docker compose --profile dev up -d --build
```

Trên Git Bash (Windows) đặt `MSYS_NO_PATHCONV=1` trước lệnh `docker compose … run` để đường dẫn `/scripts/…` không bị đổi thành `C:/…`.
Không có `AUTHOR_PASSWORD` thì 10 % ghi được thay bằng đọc danh sách — tổng số request không đổi (lượt đối chứng của báo cáo hit rate).

Kết quả thô (`results/*.csv.gz`, `*-summary.json`) không commit; số liệu tổng hợp nằm trong `docs/load-test-report.md`.
