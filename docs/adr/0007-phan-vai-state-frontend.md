# ADR-0007 — Phân vai state Frontend: Zustand cho phiên, TanStack Query cho dữ liệu server, chỉ refresh token lưu bền

- **Trạng thái:** Đã chấp nhận (Buổi 4: access token chỉ trong bộ nhớ — D-12; Buổi 6: Zustand store — Lab 6)
- **Liên quan:** NFR-SEC-002, NFR-SEC-005, MT-55, X-7 (kế hoạch §3)

## Bối cảnh

Lab 6 yêu cầu một store quản lý phiên và cho chọn *"persist token vào HTTP-only cookie / localStorage"*. SRS NFR-SEC-002 quy định access token **chỉ
nằm trong bộ nhớ**, refresh token là dữ liệu xác thực duy nhất được lưu bền; §5.2/NFR-SEC-005 không dùng cookie cho xác thực (chuyển sang cookie
phải qua CR). Code Buổi 2 lưu cả access token vào `localStorage` (D-12). Phiên trước Buổi 6 nằm trong React Context.

## Phương án

| Phương án | Được | Mất |
|---|---|---|
| A. Zustand `persist` + `partialize` chỉ lưu `{ refreshToken, user }`; access token trong bộ nhớ; dữ liệu server ở TanStack Query | Một chỗ khai báo trường nào được lưu bền; component đăng ký đúng lát state cần đọc | Tải lại trang phải gọi `/auth/refresh` một lần |
| B. HTTP-only cookie | Script không đọc được token | Trái SRS (cần CR), kéo theo CSRF và CORS credentials |
| C. Lưu cả access token vào `localStorage` | Đơn giản | XSS đọc được token có hiệu lực ngay (lỗi D-12) |
| D. Đưa dữ liệu server vào Zustand | Một thư viện | Tự viết lại cache, khử trùng request, làm mới nền mà TanStack Query đã có |

## Quyết định

**A + TanStack Query cho dữ liệu server:**
- **Zustand** (`stores/auth-store.ts`): trạng thái phiên phía client; store tạo theo cây React (không phải biến cấp module — tránh dùng chung giữa
  các request trên server). Adapter đọc định dạng `localStorage` của Buổi 4 để người đang đăng nhập không bị đăng xuất khi cập nhật.
- **TanStack Query:** mọi dữ liệu từ API, khóa theo module (`searchKeys`, `userKeys`…); dữ liệu riêng tư bị xóa khỏi cache khi đăng xuất.
- **React Hook Form** cho biểu mẫu, **`searchParams`** cho bộ lọc/trang — URL là nguồn sự thật.

## Hệ quả

- CSP có nonce là lớp bảo vệ còn lại cho refresh token trong `localStorage` → ADR-0010.
- Refresh 401 chạy single-flight (một lần `/auth/refresh` cho nhiều request song song), đồng bộ giữa các tab bằng sự kiện `storage` + Web Locks.

## Bằng chứng

`frontend/src/stores/auth-store.ts` + kiểm thử; kiểm chứng Buổi 6: `localStorage` chỉ có `refreshToken` + `user`; 5 request song song gặp 401 →
đúng 1 lần `/auth/refresh`.
