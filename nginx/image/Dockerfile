# Nginx của dự án = image chính thức đã ghim + vá gói Alpine mà image chính thức chưa kịp build lại.
# Trivy 30/10/2026 (kế hoạch Buổi 8 — không chấp nhận lỗ hổng Critical): nginx:1.31.6-alpine còn brotli-libs 1.2.0-r1
# (CVE-2026-111497, vá ở 1.2.1-r0; brotli-libs là phụ thuộc của curl mà healthcheck dùng). Ràng buộc ">=" làm build lỗi
# nếu kho Alpine không có bản vá, thay vì lặng lẽ giữ bản cũ. Khi image chính thức mới đã có bản vá thì bỏ dòng RUN này.
# Build context là riêng thư mục này: .htpasswd và chứng chỉ ở ../ không bao giờ vào context.
FROM nginx:1.31.6-alpine
RUN apk add --no-cache 'brotli-libs>=1.2.1-r0'
