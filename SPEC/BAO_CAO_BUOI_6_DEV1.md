# Báo cáo tiến độ Buổi 6 — Dev 1

- Họ tên: Hoàng Bình Quân
- MSSV: 2314236
- Branch: `2314236_HoangBinhQuan_buoiso6`
- Base code: `a8204f5`

## Phần nền đã có và tái sử dụng cho Buổi 6

- Access token chỉ giữ trong bộ nhớ; localStorage lưu refresh token và user.
- Tải lại trang dùng refresh token để phục hồi phiên; các request hết hạn dùng single-flight refresh.
- Web Locks điều phối refresh giữa các tab; storage event đồng bộ đăng xuất.
- Route `/dashboard` và `/profile` được chặn phía client; route Admin kiểm tra role.
- `LoginForm` đã có kiểm tra callback URL nội bộ, nhưng helper này còn cục bộ.
- Middleware có CSP và các security header cơ bản; nonce hiện áp dụng cho route auth/dashboard/profile. Google Identity provider nhận nonce hiện có.
- Baseline kiểm chứng trước checkpoint: frontend tests 67/67 PASS, lint PASS, typecheck PASS. Checkpoint này không đổi code runtime nên không chạy lại full suite.

## Công việc Buổi 6 mới theo README

- Chuyển auth state từ React Context sang Zustand: `createAuthStore()`, `persist` với `partialize` chỉ lưu `refreshToken`/`user`, `skipHydration` và `AuthStoreProvider` riêng cho mỗi React tree.
- Migrate session định dạng Buổi 4 và giữ phiên của người đã đăng nhập; giữ interface `useAuth()`.
- Chuẩn hóa client guards thành `AuthGuard`, `RequireRole`, `GuestOnly`; tạo `safe-redirect` helper dùng chung.
- Mở rộng CSP nonce ra toàn site, thêm `strict-dynamic`, bỏ `unsafe-inline` khỏi `script-src`, để root layout đọc nonce.
- Thêm Playwright CSP E2E để kiểm tra violation, hydration và nonce mới mỗi lần tải.

## Blocker hiện tại

`BLOCKED_BY_TEAM_FOUNDATION`: `origin/develop` (`7ea32cc` tại thời điểm checkpoint) chưa có route groups `(public)/(auth)/(dashboard)`, shadcn foundation và các foundation cần thiết theo thứ tự merge của README. Không merge hoặc cherry-pick trực tiếp nhánh đồng đội để tránh lệch integration flow.

## Trạng thái

- Phần nền có thể tái sử dụng: hoàn thành.
- Implementation đặc thù Buổi 6: chưa bắt đầu.
- Ước lượng tiến độ cá nhân hiện tại: khoảng 35–40% (phần nền tái sử dụng, không phải mức hoàn thành yêu cầu mới).
- `READY_TO_CONTINUE` khi foundation được merge vào `origin/develop`.
