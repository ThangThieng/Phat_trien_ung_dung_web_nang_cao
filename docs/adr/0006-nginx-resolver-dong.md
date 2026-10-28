# ADR-0006 — Nginx phân giải tên service động, không dùng khối `upstream`

- **Trạng thái:** Đã chấp nhận (sửa lỗi Buổi 2, commit `064f582`; chuẩn hóa vào SRS v1.2.0 — CR-2026-02, MT-50)
- **Liên quan:** NFR-SCALE-003, NFR-REL-001, FR-OBS-001

## Bối cảnh

SRS v1.1.0 §6.5 đặc tả `upstream api_pool { server api:8080; }`. Nginx bản mã nguồn mở phân giải tên trong khối `upstream` **một lần lúc khởi động**;
trong Docker, IP container đổi mỗi khi container được tạo lại. Buổi 2 đã gặp thật: tạo lại `api` → Nginx trả **502** cho mọi `/api/*` tới khi
khởi động lại Nginx. SRS v1.1.0 viết cấu hình này *sau khi* code đã sửa lỗi.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. `resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến | Hết 502 khi tạo lại container; `--scale api=3` tự luân phiên; bản sao mới được nhận ≤ 10 giây, không reload | Không dùng được tính năng chỉ có trong `upstream` (`least_conn`) |
| B. `upstream { server api:8080 resolve; }` | Giữ khối `upstream` | `resolve` chỉ có ở Nginx Plus / bản rất mới — không bảo đảm |
| C. Giữ `upstream`, reload Nginx sau mỗi deploy | Không sửa cấu hình | Quy trình thủ công; vẫn chết khi container tự khởi động lại |

## Quyết định

**A**, kèm `proxy_next_upstream error timeout http_502 http_503 http_504` (Buổi 5) và `proxy_connect_timeout 2s` (Buổi 7 — trong ≤ 10 giây Nginx
còn giữ IP của bản sao vừa gỡ; SYN tới IP chết bị bỏ lặng lẽ nên phải bỏ nhanh địa chỉ đó).

## Hệ quả

- Resolver động chỉ bảo đảm 502 **không kéo dài**; Docker Compose không có cập nhật cuốn chiếu, nên cập nhật không gián đoạn dùng
  `tools/deploy/rolling-update-api.sh` (đo Buổi 7: 49.657 request dưới tải, 0 lỗi).
- Chú thích ngay trong `nginx/templates/snippets/http-common.conf.template` giải thích vì sao **không** dùng `upstream`.

## Bằng chứng

`nginx/templates/snippets/{http-common,app-locations}.conf.template`; đo Buổi 2 (IP `172.30.2.3` → `172.30.2.2`, không khởi động lại Nginx vẫn 200);
đo Buổi 7 (scale 3 → 5 không reload; `--force-recreate` 502 trong ~3 giây so với cập nhật cuốn chiếu 0 lỗi).
