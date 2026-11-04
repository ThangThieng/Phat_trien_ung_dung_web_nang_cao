# ADR-0001 — Xóa mềm toàn hệ thống, dọn vĩnh viễn sau 30 ngày

- **Trạng thái:** Đã chấp nhận (SRS v1.1.0 — CR-2026, MT-05; job dọn: FR-JOB-003, Buổi 4)
- **Liên quan:** FR-RCP-007, FR-JOB-003, NFR-REL-003, D-3

## Bối cảnh

SRS v1.0.0 mâu thuẫn ở 5 chỗ: FR-RCP-007 ghi xóa **cứng** (cascade xóa bước, nguyên liệu, ảnh MinIO), trong khi `BaseEntity` có `IsDeleted`
với Global Query Filter, NFR-REL-003 hứa "có thể khôi phục", Chương 8 ghi `DELETE /recipes/{id}` là xóa mềm, Phụ lục A ghi 404 cho tài
nguyên "đã soft-delete". Đây là mâu thuẫn nặng nhất vì kéo theo cascade, xóa tệp MinIO và tính duy nhất của slug.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. Xóa mềm toàn bộ | Khôi phục được; đúng thiết kế `BaseEntity` | Slug đã dùng vẫn chiếm chỗ; ảnh còn trong MinIO |
| B. Xóa cứng riêng Recipe | DB và MinIO sạch ngay | Phá `BaseEntity`; mất dữ liệu; phải sửa 4 mục SRS khác |
| C. Xóa mềm + job dọn vĩnh viễn sau N ngày | Có "thùng rác"; vẫn giải phóng dung lượng đúng lúc | Thêm một recurring job |

## Quyết định

**A, mở rộng thành C ở Buổi 4:** `DELETE /recipes/{id}` chỉ gán `IsDeleted = true`; `PermanentPurgeJob` (03:30 UTC hằng ngày, khóa phân tán
RedLock) xóa cứng công thức đã xóa mềm quá 30 ngày **trong một transaction**, rồi **sau khi commit** mới xóa tệp MinIO.

- Slug dùng **partial unique index** `WHERE "IsDeleted" = false` (D-3) — xóa mềm xong có thể tạo lại công thức cùng tên.
- Xóa mềm **không** xóa tệp ảnh: nếu xóa ngay thì khôi phục sẽ mất ảnh.

## Hệ quả

- Mọi truy vấn phải qua Global Query Filter; truy vấn bỏ qua filter (`IgnoreQueryFilters`) chỉ có ở job dọn.
- Khôi phục do quản trị viên vận hành trên DB trong 30 ngày (MT-49) — chưa có chức năng tự phục vụ.
- Thứ tự "commit DB rồi mới xóa tệp" bảo đảm không bao giờ xóa ảnh của một công thức mà transaction bị rollback.

## Bằng chứng

`backend/src/CulinaryBlog.Infrastructure/Jobs/PermanentPurgeJob.cs`, migration `B4_Recipe_PartialUniqueSlug`, kiểm thử
`Jobs/PermanentPurgeJobTests`, `RecipeLifecycleEndpointsTests` (xóa mềm → 404 ở endpoint công khai).
