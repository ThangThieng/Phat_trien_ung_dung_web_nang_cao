# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Buổi:** 6 - Nền giao diện shadcn/ui và trang tổng quan<br>
**Từ ngày:** 20/10/2026 **đến ngày:** 21/10/2026

**MSSV:** 2312758<br>
**Họ và tên:** Nguyễn Hồng Phúc Thọ<br>
**Nhóm:** 20

**Nhánh mang nội dung vào `develop`:** [`2312758_NguyenHongPhucTho_buoiso7`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7)<br>
**Commit Buổi 6:** [`c481fc1` - feat(ui): complete shadcn foundation and dashboard overview](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/c481fc1)

## Công việc đã hoàn thành

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ % |
|---:|---|---|---:|
| 1 | **Nền shadcn/ui:** khởi tạo shadcn/ui 4.21.4 với alias `@/components/ui` và `@/lib/utils`. Hoàn thiện 8 component yêu cầu của Lab: `Button`, `Badge`, `Card`, `Dialog`, `DropdownMenu`, `Input`, `Sheet`, `Skeleton`; đồng thời bổ sung 8 component phụ trợ: `AlertDialog`, `Form`, `Label`, `Select`, `Sonner`, `Table`, `Tabs`, `Textarea`. | [Nhánh Buổi 7](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7) | 100% |
| 2 | **Theme và khả dụng:** đặt token `--primary: #c2410c` (orange-700); chữ trắng trên màu này có tỉ lệ tương phản 5,2:1, đạt ngưỡng WCAG AA cho chữ thường. `Button` có `isLoading`, tự khóa thao tác và gắn `aria-busy`. | [Commit c481fc1](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/c481fc1) | 100% |
| 3 | **Trang `/dashboard`:** xây dựng trong route group `(dashboard)` để dùng khung dashboard và `AuthGuard`. Ba `Card` đếm Draft/Published/Archived qua ba truy vấn `/recipes/mine` có `pageSize: 1`, đọc `totalCount`, `staleTime: 0` và hiển thị `Skeleton` khi tải. Thêm `RecipeStatusBadge` ánh xạ Draft/Published/Archived tương ứng `secondary/default/outline`. | [Commit c481fc1](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/commit/c481fc1) | 100% |

## Kiểm thử

- `button.test.tsx`: kiểm tra `isLoading` khóa nút và có `aria-busy`.
- `dialog.test.tsx`: kiểm tra phím Escape đóng dialog và focus quay về nút mở dialog.
- `form.test.tsx`: kiểm tra trường lỗi có `aria-invalid` và `aria-describedby`.
- `RecipeStatusBadge.test.tsx`: kiểm tra đủ ba trạng thái Draft, Published và Archived.
- Đã chạy kiểm tra frontend: `npm run lint`, `npx tsc --noEmit` và Jest; các kiểm thử nói trên đạt.

## Ghi chú bàn giao

Buổi 6 được đưa vào `develop` cùng nhánh Buổi 7 vì commit `c481fc1` có nội dung tương đương commit `8159fd6` của PR Buổi 6 cũ. Cách này tránh đưa cùng một thay đổi vào `develop` hai lần qua hai commit khác nhau.
