# Quy ước dịch công thức Wikibooks sang tiếng Việt

Áp dụng cho `translations/batch-*.json`. Văn phong theo 100 công thức do nhóm biên soạn: câu mệnh lệnh ngắn, tự nhiên.

## Nguyên tắc

- **Tên món:** tên riêng giữ nguyên, thêm giải nghĩa trong ngoặc: `Paella Roja (Cơm hải sản đỏ kiểu Tây Ban Nha)`. Món đã có tên Việt quen thuộc thì dùng tên Việt.
- **Con số:** giữ nguyên mọi con số. Nhiệt độ °F giữ và thêm °C: `350 °F (175 °C)`. `validate_translations.py` báo lỗi nếu mất số.
- **Đơn vị:** không quy đổi giá trị. `build_seed.py` ánh xạ cột `Unit`; trong câu văn dùng cùng cách viết: cốc, thìa canh, thìa cà phê, gram, tép, nhúm, lon, bó, lát, miếng, cọng, thanh, gói, lá. Đơn vị đo Mỹ (`pound`, `oz`, `pint`, `quart`, `gallon`) giữ nguyên tên.
- **Tiêu đề bước:** ngắn, bắt đầu bằng động từ: `Chần xương`, `Làm nóng lò`.
- **Không thêm bớt nội dung:** không tự thêm mẹo, không bỏ cảnh báo. Liên kết và chú thích kiểu Wikipedia trong mô tả được lược theo nghĩa.
- **Nguyên liệu khó tìm:** giữ tên gốc trong ngoặc: `đậu đen tách vỏ (urad dal)`.

## Động từ nấu ăn

| Tiếng Anh | Tiếng Việt |
| --- | --- |
| preheat | làm nóng trước |
| bake | nướng (lò) |
| roast | quay, nướng lò |
| grill, broil | nướng vỉ |
| fry | chiên |
| deep-fry | chiên ngập dầu |
| sauté, stir-fry | xào |
| simmer | ninh nhỏ lửa |
| boil | luộc, đun sôi |
| blanch | chần |
| steam | hấp |
| braise, stew | hầm |
| marinate | ướp |
| whisk, beat | đánh |
| fold | trộn nhẹ tay |
| knead | nhào |
| dice | thái hạt lựu |
| chop | băm, thái nhỏ |
| mince | băm nhuyễn |
| slice | thái lát |
| grate | bào |
| drain | để ráo, chắt nước |
| strain | lọc |
| season | nêm |
| garnish | trang trí |

## Nguyên liệu thường gặp

| Tiếng Anh | Tiếng Việt |
| --- | --- |
| all-purpose flour | bột mì đa dụng |
| baking powder | bột nở |
| baking soda | muối nở (baking soda) |
| butter | bơ |
| heavy cream | kem tươi béo |
| sour cream | kem chua |
| brown sugar | đường nâu |
| powdered sugar | đường bột |
| olive oil | dầu ô liu |
| vegetable oil | dầu thực vật |
| stock, broth | nước dùng |
| scallion, green onion | hành lá |
| shallot | hành tím |
| bell pepper | ớt chuông |
| chili pepper | ớt |
| cilantro, fresh coriander | ngò rí |
| parsley | mùi tây |
| ground beef | thịt bò xay |
| turmeric | nghệ bột |
| cumin | thì là Ai Cập |
| paprika | ớt bột paprika |
