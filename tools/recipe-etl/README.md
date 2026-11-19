# recipe-etl

Công cụ tạo dữ liệu mẫu bổ sung từ [Wikibooks Cookbook](https://en.wikibooks.org/wiki/Cookbook:Table_of_Contents) theo CR-2026-07 (SRS v1.2.5, MT-70). Kế hoạch đầy đủ: `SPEC/KE_HOACH_PHAT_TRIEN_8_BUOI.md`, Phụ lục B.

Chạy ngoài ứng dụng và không nằm trong image production. Kết quả là JSON đã qua kiểm tra, được commit vào `backend/.../Persistence/Seed/Data/`, nên API khởi động không cần Internet.

## Tái lập

```bash
pip install -r requirements.txt

python fetch_pages.py --pinned   # đúng các revision đã duyệt trong pins.json (bỏ --pinned để lấy toàn bộ bản mới nhất)
python fetch_images.py           # giấy phép và URL ảnh, không tải file ảnh
python transform.py              # lọc, ánh xạ → out/recipes_en.json, out/report.json
python validate_translations.py  # kiểm tra bản dịch
python build_seed.py             # ghép → Seed/Data/recipes-wikibooks-*.json và các báo cáo duyệt trong out/
python build_legacy_images.py    # ảnh đã duyệt cho công thức cũ → Seed/Data/legacy-recipe-images.json
python -m pytest tests           # không gọi mạng
```

`raw/` và `out/` không commit. API của Wikimedia trả 429 nếu gọi liên tục, nên các script có điều tiết và tôn trọng `Retry-After`.

## Quy tắc lọc

Một trang được chọn khi đủ các điều kiện, theo thứ tự trong `transform.py`: không phải trang con, sách cổ hay trang chưa hoàn chỉnh; nguyên liệu không ở dạng bảng; ≥ 10 nguyên liệu; ≥ 5 bước; có đoạn mô tả từ 40 ký tự; có khẩu phần, thời gian, độ khó; có ảnh giấy phép mở; không trùng món đã có. `exclusions.json` ghi các trang bị loại sau khi xem ảnh, kèm lý do.

## Dữ liệu suy ra, không phải số nguồn công bố

- **Thời gian chuẩn bị và nấu:** nguồn chỉ ghi tổng thời gian. Thời gian nấu là tổng các mốc thời gian trong bước có đúng một mốc, phần còn lại là thời gian chuẩn bị. Bước có đun nấu mà không có mốc nào thì **ước lượng** chia đôi tổng thời gian; chỉ món không đun nấu mới có thời gian nấu 0.
- **Khẩu phần:** khoảng "4–6" lấy cận dưới; sản lượng như "8 ea." được dùng như số khẩu phần.
- **Danh mục:** do `taxonomy.py` gợi ý rồi sửa tay trong `category_overrides.csv`.
- **Dinh dưỡng:** chỉ nhận từ template của nguồn khi đủ 6 chỉ số và kcal lệch không quá 35% so với `4×đạm + 4×tinh bột + 9×béo`. Không đạt thì để `NULL`, không sinh số.

## Bản dịch

`translations/batch-*.json` do Claude dịch trong phiên làm việc, theo `glossary.md`. `validate_translations.py` chặn: sai số dòng, mất con số, không có dấu tiếng Việt, còn từ tiếng Anh phổ biến, vượt giới hạn cột, trùng tên nguyên liệu, trùng slug, và bản gốc đã đổi sau khi dịch (`sourceHash`).

**Kiểm tra tự động không phát hiện được dịch sai nghĩa, nên người trong nhóm phải đọc đối chiếu `out/review_recipes.html` trước khi phát hành.** Trang chi tiết ghi rõ "có hỗ trợ AI".

## Ảnh

`image_overrides.json` chọn ảnh khác ảnh đầu trang Wikibooks cho công thức nhóm thấy ảnh gốc kém hấp dẫn (theo `pageId`, kèm lý do); đổi ảnh của công thức đã seed thì thêm slug và trang mô tả ảnh cũ vào `SeedImageReplacements.cs`. `legacy_image_picks.json` là ảnh đã xem bằng mắt cho công thức cũ. Ảnh chỉ gần đúng món được ghi kèm lý do trong `legacy_image_notes.json` để người duyệt thay khi có ảnh tốt hơn. Không có ảnh gần đúng thì công thức giữ không ảnh. `legacy_image_external.json` là ảnh Flickr tải qua API Openverse; 4 ảnh trong đó có giấy phép phi thương mại, phải thay nếu dùng thương mại.

## Giấy phép

Nội dung nguồn và phần chuyển thể theo CC BY-SA 4.0, xem `Seed/Data/LICENSE-wikibooks.md`.
