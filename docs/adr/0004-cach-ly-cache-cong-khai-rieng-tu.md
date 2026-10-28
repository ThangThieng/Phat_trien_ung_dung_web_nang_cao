# ADR-0004 — Tách bạch endpoint công khai và riêng tư; không bao giờ cache dữ liệu phụ thuộc danh tính

- **Trạng thái:** Đã chấp nhận (SRS v1.1.0 — CR-2026, MT-34; vá code: D-4, D-5 ở Buổi 4; endpoint sửa bản nháp: CR-2026-04)
- **Liên quan:** NFR-SEC-006, FR-RCP-001/002/011, FR-CAT-002

## Bối cảnh

Lỗi nghiêm trọng nhất trong 41 mâu thuẫn của SRS v1.0.0 — và là **lỗi bảo mật**: FR-RCP-001 lọc kết quả theo danh tính (Admin thấy mọi bản
nháp, Author thấy bản nháp của mình) **nhưng** cache response bằng Output Cache với khóa `{path}?{queryString}`. Admin mở `/recipes?page=1`
một lần → trong 15 phút mọi Guest gọi cùng URL nhận nguyên response chứa bản nháp của mọi người. Kiểm tra quyền ở handler vô hiệu vì request
thứ hai không bao giờ tới handler. Code Buổi 2 đã mắc đúng lỗi này (D-4, D-5).

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. Endpoint công khai chỉ trả `Published` cho **mọi** người gọi; dữ liệu riêng tư qua endpoint riêng `no-store` | Response công khai thuần, cache an toàn; hit rate cao | Thêm endpoint `/recipes/mine`, `/recipes/mine/{id}` |
| B. Thêm `userId`/`role` vào khóa cache | Một endpoint | Mỗi người một bản cache → hit rate sụp đổ; quên vary ở một chỗ là rò rỉ |
| C. Bỏ cache `/recipes` | An toàn tuyệt đối | Mất p50 ≤ 150 ms (NFR-PERF-001) |

## Quyết định

**A.** `GET /recipes`, `/recipes/{slug}`, `/recipes/search`, `/categories/{slug}` chỉ trả `Status == Published` cho mọi người gọi, **kể cả Admin**
(bản nháp → 404). Bản nháp/lưu trữ của chính mình qua `GET /recipes/mine` và `/recipes/mine/{id}` — **cấm cache**, `Cache-Control: no-store`. Nguyên
tắc ghi vào NFR-SEC-006: *không bao giờ đặt dữ liệu phụ thuộc danh tính vào cache dùng chung dưới khóa công khai.*

## Hệ quả

- Server Component phía Next.js **không bao giờ** gọi endpoint riêng tư (Data Cache cũng là cache dùng chung); trang dashboard là CSR.
- Danh sách query được phép cài `ICacheable` là **danh sách đóng** (6 query công khai) — thêm query vào cache là quyết định có chủ đích.

## Bằng chứng

`ObservabilityRulesTests.OnlyPublicQueries_ImplementICacheable` (ArchitectureTests — Buổi 8), `RecipeCacheIsolationTests`, `RecipeQueryEndpointsTests` (Draft → 404 kể cả với
Admin, `/recipes/mine` có `no-store`), `curl -I /recipes/<slug-Draft>` → 404 (kiểm chứng Buổi 7).
