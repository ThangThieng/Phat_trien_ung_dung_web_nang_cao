# ADR-0005 — Hangfire chạy ở container worker riêng, dùng chung image với API

- **Trạng thái:** Đã chấp nhận (hiện thực từ Buổi 2; chuẩn hóa vào SRS v1.2.0 — CR-2026-02, MT-47)
- **Liên quan:** FR-JOB-001/002/003, NFR-SCALE-001, NFR-PERF-001, FR-AUTH-011

## Bối cảnh

SRS v1.0.0/v1.1.0 ghi *"Hangfire chạy in-process trong .NET API"*; hệ thống thật từ Buổi 2 chạy container `hangfire` riêng (`Hangfire__WorkerOnly=true`),
container `api` chỉ đưa job vào hàng đợi (`Hangfire__ServerEnabled=false`). Người đọc SRS để "sửa cho đúng" sẽ gộp worker vào API.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. Worker riêng, cùng image | Đổi kích thước ảnh (ImageSharp, nặng CPU) **không tranh CPU** với request người đọc; API và job scale độc lập | Thêm một container |
| B. In-process | Ít container | Mỗi lượt tải ảnh làm chậm mọi request; `--scale api=3` thành 3 job server |

## Quyết định

**A.** Cùng một image và `Program.cs`; cờ `Hangfire:WorkerOnly` quyết định process chạy HTTP API hay chỉ `BackgroundJobServer`. Hai container dùng
chung khóa DataProtection (volume `dpkeys`, mã hóa bằng chứng chỉ ở production — D-17) vì token đặt lại mật khẩu sinh ở worker.

## Hệ quả

- Job không chạy trong request nên phải tự mang ngữ cảnh: **Buổi 8** `CorrelationIdJobFilter` ghi CorrelationId của request vào tham số job lúc
  enqueue và đặt lại vào log context lúc worker chạy — log của job nối được với request đã sinh ra nó.
- Job ghi DB ngoài pipeline MediatR thì tự xóa cache (Buổi 8: `ImageResizeJob` xóa khóa của công thức sau khi có thumbnail).
- Worker có healthcheck riêng `/health/ready` (PostgreSQL + Redis) từ Buổi 8.

## Bằng chứng

`docker-compose.yml` / `docker-compose.prod.yml` (service `hangfire`), `Program.cs` (nhánh `workerOnly`), `Infrastructure/Jobs/CorrelationIdJobFilter.cs`,
kiểm thử `ObservabilityInfrastructureTests.CorrelationId_TravelsFromEnqueueToJobExecutionLogs`.
