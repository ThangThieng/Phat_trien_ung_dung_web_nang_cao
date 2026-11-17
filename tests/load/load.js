// Load (Buổi 8 — NFR-PERF-001/002): 100 người dùng đồng thời trong 5 phút (cộng 1 phút tăng dần, 30 giây giảm dần).
// Ngưỡng: p50 ≤ 150 ms (GET có cache), p95 ≤ 500 ms, p99 ≤ 1000 ms; lỗi < 1 %. Hit rate Redis đo song song bằng redis-cli INFO stats
// (tests/load/README.md) vì k6 không nhìn thấy cache.
import { prepare, visit, PERF_THRESHOLDS, TLS } from './lib/traffic.js';

export const options = {
  ...TLS,
  stages: [
    { duration: '1m', target: 100 },
    { duration: '5m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: PERF_THRESHOLDS,
};

export function setup() {
  return prepare();
}

export default function (data) {
  visit(data);
}
