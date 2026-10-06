# GitHub Project "Culinary Blog – Nhóm 20"

Bảng: https://github.com/users/ThangThieng/projects/9

| File | Việc |
|---|---|
| `sync_board.py` | Đọc README (Phần 2–3), tạo/cập nhật issue cha mỗi sinh viên + sub-issue mỗi đầu việc từng buổi, đưa vào Project. Không bao giờ hạ trạng thái đang có. |
| `auto_complete.py` | Tự chấm: nhánh `{MSSV}_{HoTen}_buoiso{n}` đạt đủ tiêu chí của đầu việc nào (chỉ đầu việc của chính chủ nhánh) thì đầu việc đó sang **Done**, issue đóng, kèm comment minh chứng (nhánh, commit, ngày, checklist). Đạt một phần ở nhánh đúng buổi → **In Progress**. Đồng thời dựng các trường và view chấm bài cho giảng viên. |
| `completion_rules.json` | Tiêu chí kiểm được trên mã nguồn cho từng đầu việc (class, route, migration, kiểm thử, trang FE…), lấy từ mục *Hướng đi / Xong khi* của README. |
| `project_api.py` | Gọi GitHub GraphQL/REST qua `gh`. |

## Chạy tay

```bash
python tools/github_board/auto_complete.py --all --dry-run   # chỉ xem kết quả chấm
python tools/github_board/auto_complete.py --all             # cập nhật Project
python tools/github_board/sync_board.py                       # khi README đổi đầu việc
```

Cần GitHub CLI đã đăng nhập với scope `project` (`gh auth refresh -s project`).

## Tự động

`.github/workflows/project-board.yml` chạy khi có push lên bất kỳ nhánh nào, mỗi 30 phút, và khi bấm *Run workflow*.
Workflow luôn lấy tool + tiêu chí từ `main`. Cần secret **`BOARD_TOKEN`**: Personal access token (classic) của chủ Project với scope `repo` + `project`
(`gh secret set BOARD_TOKEN -R ThangThieng/Phat_trien_ung_dung_web_nang_cao`). `GITHUB_TOKEN` mặc định không ghi được vào Project của tài khoản cá nhân.

## Thêm / sửa tiêu chí

Sửa `completion_rules.json` (khóa = khóa issue, vd. `b5-dev2-1`), thử bằng `--dry-run`, rồi merge vào `main`.
Tiêu chí chỉ xác nhận *sản phẩm có mặt và có kiểm thử*; không thay cho việc đọc code.
