# BÁO CÁO LAB - MÔN PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Buổi:** 7 - Chi tiết công thức và Công thức của tôi<br>
**Từ ngày:** 22/10/2026 **đến ngày:** 28/10/2026

**MSSV:** 2312758<br>
**Họ và tên:** Nguyễn Hồng Phúc Thọ<br>
**Nhóm:** 20

**Nhánh:** [`2312758_NguyenHongPhucTho_buoiso7`](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7)

## Công việc

| STT | Công việc được giao | Liên kết đến GitHub branch | Tiến độ % |
|---:|---|---|---:|
| 1 | **Trang `/recipes/[slug]`:** render theo request do CSP nonce; dữ liệu lấy qua Data Cache 300 giây. Không có `loading.tsx`; trang gọi `notFound()` khi API trả 403/404 để giữ HTTP 404 cho công thức Draft. Thành phần gồm `RecipeHero`, `RecipeGallery`, `IngredientList`, `StepChecklist`, `NutritionTable` và `RecipeActions`. | [Nhánh Buổi 7](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7) | 100% |
| 2 | **Tương tác trang chi tiết:** điều chỉnh khẩu phần chỉ nhân giá trị số và giữ nguyên `quantityText`; lightbox ảnh tải muộn qua `next/dynamic`, hỗ trợ Escape và trả focus về nút ảnh; in bằng `@media print`; chia sẻ dùng `navigator.share`, dự phòng sao chép liên kết. | [Nhánh Buổi 7](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7) | 100% |
| 3 | **Trang `/dashboard/recipes`:** đặt trong route group dashboard, lọc trạng thái qua URL, dùng `Table` và `RecipeStatusBadge`, phân trang và chỉ gọi `/recipes/mine`. `recipeKeys` tách nhánh `public` và `private`, truy vấn riêng tư dùng `staleTime: 0`. | [Nhánh Buổi 7](https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao/tree/2312758_NguyenHongPhucTho_buoiso7) | 100% |

## Kết quả kiểm chứng

- Kiểm tra tự động đã đạt: `npm run lint`, `npm run typecheck`, Jest 281/281 và `npm run build`. Build production sau tối ưu báo First Load JS của `/recipes/[slug]` là **194 kB**, dưới ngân sách 200 kB của NFR-PERF-005.
- `IngredientList.test.tsx` kiểm tra khẩu phần 2 → 4 làm `200 g` thành `400 g` và giữ nguyên lượng chữ `vừa đủ`.
- `RecipeGallery.test.tsx` kiểm tra lightbox tải theo thao tác, Escape đóng dialog và focus quay lại nút mở ảnh.
- `dialog.test.tsx` kiểm tra hành vi Escape/focus của Dialog; `RecipeActions.test.tsx` kiểm tra phương án sao chép URL khi Web Share không khả dụng.
- Công cụ chấm Buổi 7 ghi nhận 4/4 yêu cầu chính; kiểm tra trang chi tiết xác nhận route render theo request, không có `loading.tsx`, gọi `notFound()` và tách query key `public/private`.

## Ghi chú hiệu năng

`RecipeLightbox` là file riêng, được import bằng `next/dynamic(..., { ssr: false })` và chỉ render sau khi người dùng bấm ảnh. Các tương tác `RecipeActions` và `StepChecklist` cũng tải muộn; `Toaster` của Sonner không còn nằm trong First Load JS. Ảnh chính 800×600 vẫn dùng `priority` để bảo vệ LCP.
