# Font cho ảnh Open Graph

`inter-latin-700.woff` và `inter-vietnamese-700.woff` — Inter Bold (SIL Open Font License 1.1), lấy từ gói `@fontsource/inter`.

`ImageResponse` (satori) không dùng được `next/font` và nếu không có font nhúng sẵn thì cố tải font động từ Google Fonts khi gặp
ký tự lạ (ở, ộ…). Máy chủ Next.js trong container production không ra Internet, nên chữ tiếng Việt có dấu trên ảnh chia sẻ
sẽ bị thiếu glyph. Nhúng sẵn hai tập con (latin + vietnamese) giải quyết cả hai vấn đề. satori chỉ đọc TTF/OTF/WOFF (không đọc WOFF2).
