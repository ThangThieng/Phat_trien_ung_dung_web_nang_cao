# stores/

Zustand – **chỉ state phía client** (ví dụ: access token trong bộ nhớ, trạng thái mở/đóng UI dùng chung).
Dữ liệu lấy từ server (công thức, danh mục, người dùng…) **thuộc TanStack Query**, không đưa vào store.

- Store đầu tiên: `auth-store.ts` (Buổi 6 – Dev 1). Thư mục này cố ý chưa có mã ở Buổi 5.
- Mỗi store một file `<tên>-store.ts`; không import từ `app/` (ESLint `import/no-restricted-paths`).
