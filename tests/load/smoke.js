// Smoke (Buổi 8): 1 người dùng ảo trong 1 phút — kịch bản và môi trường chạy đúng trước khi tăng tải. Mọi request phải thành công.
import { prepare, visit, PERF_THRESHOLDS, TLS } from './lib/traffic.js';

export const options = {
  ...TLS,
  vus: 1,
  duration: '1m',
  thresholds: { ...PERF_THRESHOLDS, api_req_failed: ['rate==0'] },
};

export function setup() {
  return prepare();
}

export default function (data) {
  visit(data);
}
