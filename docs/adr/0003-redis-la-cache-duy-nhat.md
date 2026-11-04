# ADR-0003 — Redis cache-aside là cơ chế cache duy nhất

- **Trạng thái:** Đã chấp nhận (SRS v1.1.0 — CR-2026, MT-16 + MT-17; gỡ Output Cache: D-6, Buổi 4; xóa theo tiền tố bằng SCAN: Buổi 8)
- **Liên quan:** NFR-PERF-003, NFR-SCALE-001, NFR-REL-002, SRS §6.3

## Bối cảnh

SRS v1.0.0 mô tả **hai kiến trúc cache cho cùng một endpoint**: Chương 3 dùng `IMemoryCache` (danh mục) và Output Cache .NET (công thức,
TTL 15–60 phút); NFR-PERF-003 dùng cache-aside qua Redis với TTL 1–30 phút. Hệ thống chạy nhiều bản sao API sau Nginx (NFR-SCALE-003).

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. Redis cho mọi cache, qua `CachingBehavior`/`CacheInvalidationBehavior` | Xóa cache đúng trên mọi bản sao; một cơ chế duy nhất | Mỗi lần đọc thêm ~1 ms mạng; phụ thuộc Redis |
| B. `IMemoryCache` cho danh mục + Redis cho phần còn lại | Danh mục nhanh nhất | Sửa danh mục ở bản sao 1, bản sao 2 trả dữ liệu cũ tới hết TTL; không có cách xóa |
| C. Hai tầng L1 bộ nhớ + L2 Redis | Rất nhanh | Phức tạp, vẫn có cửa sổ dữ liệu cũ; thừa với ≤ 50 danh mục |

## Quyết định

**A.** Query cài `ICacheable` được `CachingBehavior` đọc/ghi Redis; command cài `ICacheInvalidator` khai báo khóa cần xóa, `CacheInvalidationBehavior`
xóa **sau khi** handler thành công. Bảng TTL ở NFR-PERF-003 là nguồn sự thật duy nhất (30′ danh mục, 2′ chi tiết danh mục, 2′ danh sách,
5′ chi tiết công thức, 1′ tìm kiếm). Redis lỗi → coi như cache miss, đọc PostgreSQL, **không ném exception** (NFR-REL-002).

**Buổi 8 — xóa theo tiền tố:** khóa kết thúc bằng `:` (`recipes:list:`, `categories:detail:`) là tiền tố; `RedisCacheService.RemoveByPrefixAsync`
tìm khóa bằng **`SCAN`** (không dùng `KEYS` — lệnh O(N) chặn mọi client khác) với mẫu có tiền tố instance `culinaryblog:`, rồi `DEL` theo lô.

## Hệ quả

- Output Cache của .NET **không được dùng** ở bất kỳ đâu (khóa mặc định không phân biệt danh tính — xem ADR-0004).
- Trước Buổi 8, "xóa theo tiền tố" thực chất xóa một khóa không tồn tại; lượt audit cache Buổi 8 phát hiện và sửa (danh sách công khai nay
  phản ánh thay đổi ngay, không chờ hết TTL 2 phút).
- `search:{queryHash}` cố ý **không** bị xóa chủ động — SRS ghi "để hết hạn tự nhiên" (TTL 1 phút).

## Bằng chứng

`Application/Common/Behaviors/{Caching,CacheInvalidation}Behavior.cs`, `Infrastructure/Caching/RedisCacheService.cs`, kiểm thử
`ObservabilityInfrastructureTests.RemoveByPrefix_DeletesWholeFamilyOnly` và `UpdatingPublishedRecipe_RefreshesCachedPublicListImmediately`,
bảng đối chiếu đầy đủ: [`docs/cache-report.md`](../cache-report.md).
