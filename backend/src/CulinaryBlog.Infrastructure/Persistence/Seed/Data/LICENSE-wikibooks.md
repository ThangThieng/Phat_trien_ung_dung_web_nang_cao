# Giấy phép dữ liệu công thức Wikibooks

Các file `recipes-wikibooks-*.json` là tác phẩm phái sinh từ [Wikibooks Cookbook](https://en.wikibooks.org/wiki/Cookbook:Table_of_Contents), theo giấy phép [Creative Commons Attribution-ShareAlike 4.0](https://creativecommons.org/licenses/by-sa/4.0/). Phần chuyển thể này giữ cùng giấy phép đó.

**Đã chỉnh sửa so với nguồn:** dịch sang tiếng Việt (có hỗ trợ AI), tách thành các trường dữ liệu, thêm tiêu đề bước, thêm nhiệt độ °C cạnh °F, suy ra thời gian chuẩn bị và thời gian nấu từ tổng thời gian và các mốc trong bước (không có mốc thì ước lượng chia đôi), đổi một số từ ngữ trong mô tả.

**Tác giả:** những người đóng góp cho Wikibooks. Mỗi công thức có trường `source` (`pageId`, `revisionId`) và `attribution` trỏ tới đúng phiên bản đã dùng; lịch sử tác giả xem ở trang đó.

**Ảnh:** mỗi ảnh có giấy phép riêng, ghi trong trường `image` (hoặc `legacy-recipe-images.json`), gồm tác giả, giấy phép và liên kết tới trang mô tả trên Wikimedia Commons. Ảnh không nằm trong repo, được tải về MinIO khi chạy.

Thông báo này chỉ áp dụng cho dữ liệu nêu trên, không áp dụng cho mã nguồn của dự án và 100 công thức do nhóm biên soạn (`recipes-0*.json`).
