// Stress (Buổi 8 — NFR-PERF-002): tăng dần 200 → 400 → 600 → 800 → 1.000 người dùng (mỗi nấc tăng 1 phút, giữ 2 phút) để tìm ngưỡng
// gãy — nấc đầu tiên p95 vượt 500 ms hoặc lỗi vượt 1 %. Ngưỡng đặt abortOnFail = false: đo hết các nấc, kết luận đọc từ CSV theo thời gian.
// Trần 1.000: k6 chạy cùng máy với hệ thống (máy demo 16 GB RAM, VM Docker 12 CPU / 7,6 GB dùng chung). Lượt thử tới 2.000 (21/10/2026) làm
// máy cạn RAM ở nấc 500 và tiến trình đo bị dừng — nấc cao hơn đo sức chứa của máy phát tải chứ không đo hệ thống.
import { prepare, visit, TLS } from './lib/traffic.js';

export const options = {
  ...TLS,
  stages: [
    { duration: '1m', target: 200 },
    { duration: '2m', target: 200 },
    { duration: '1m', target: 400 },
    { duration: '2m', target: 400 },
    { duration: '1m', target: 600 },
    { duration: '2m', target: 600 },
    { duration: '1m', target: 800 },
    { duration: '2m', target: 800 },
    { duration: '1m', target: 1000 },
    { duration: '2m', target: 1000 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    api_req_duration: [{ threshold: 'p(95)<=500', abortOnFail: false }],
    api_req_failed: [{ threshold: 'rate<0.01', abortOnFail: false }],
  },
};

export function setup() {
  return prepare();
}

export default function (data) {
  visit(data);
}
