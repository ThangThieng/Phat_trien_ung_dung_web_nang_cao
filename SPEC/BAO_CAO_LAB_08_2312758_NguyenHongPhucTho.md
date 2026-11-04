# BÁO CÁO LAB 08 — Nguyễn Hồng Phúc Thọ (2312758)

## 1. Phần việc

Hoàn thiện giao diện quản lý vòng đời công thức trên nhánh `2312758_NguyenHongPhucTho_buoiso8`.

- `RecipeWizard` dùng một `useForm` của React Hook Form cho năm bước: thông tin, nguyên liệu, bước nấu, ảnh và xem lại.
- Schema Zod trong `frontend/src/features/recipes/schemas.ts` dùng giới hạn từ `limits.ts`, khớp SRS §7.9. Phần tử `useFieldArray` dùng `serverId`, không dùng `id` để không bị RHF ghi đè.
- Tạo mới gửi duy nhất `POST /recipes` kèm mảng `ingredients` và `steps`; số thứ tự bước do server cấp.
- Sửa dùng `PUT /recipes/{id}` với `If-Match`/`rowVersion` trước, sau đó đồng bộ phần khác biệt của nguyên liệu/bước qua endpoint con và sắp lại bước.
- Thư viện ảnh gọi `POST /recipes/{id}/images`, cho chọn ảnh chính; xuất bản bị vô hiệu nếu có ảnh chưa có `altText`.
- Bảng `status-transitions.ts` phản chiếu đúng `RecipeStatusTransitions.cs`: Publish, Unpublish, Archive, Unarchive và xóa mềm. Xóa bắt buộc gõ đúng tiêu đề.
- Khi API trả 409, luồng con bị dừng và giao diện yêu cầu tải lại phiên bản mới.

## 2. Kiểm thử

- `npm run typecheck`: đạt.
- `npm run lint`: đạt.
- Jest mục tiêu: 3 suite / 32 test đạt, gồm kiểm thử schema wizard, bảng chuyển trạng thái và API client.
- `npm run build`: đạt; Next.js compile, lint/typecheck, thu thập dữ liệu và tạo static pages thành công.
- `dotnet build --no-restore`: môi trường hiện tại trả trạng thái lỗi nhưng không kèm lỗi biên dịch; không có tệp Backend nào thay đổi trong phần việc này.

## 3. Tệp chính

- `frontend/src/features/recipes/components/RecipeWizard.tsx`
- `frontend/src/features/recipes/components/RecipeLifecycleActions.tsx`
- `frontend/src/features/recipes/limits.ts`
- `frontend/src/features/recipes/status-transitions.ts`
