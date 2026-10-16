#!/bin/sh
# FR-JOB-001 / MT-39 – sinh nginx/.htpasswd cho Basic Auth của Hangfire Dashboard.
#
# File .htpasswd nằm trong .gitignore và bị gitleaks chặn (NFR-SEC-007) nên MỖI máy phải tự sinh
# một lần trước khi chạy "docker compose up", nếu không Docker sẽ tạo một THƯ MỤC rỗng ở vị trí đó
# và Nginx báo lỗi "auth_basic_user_file ... is a directory".
#
# Dùng image httpd:alpine để không bắt mọi người cài apache2-utils; chỉ cần Docker.
#
#   ./nginx/generate-htpasswd.sh                 # đọc user/mật khẩu từ .env
#   ./nginx/generate-htpasswd.sh admin 'MatKhau' # truyền trực tiếp
set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repo_dir=$(dirname "$script_dir")
env_file="$repo_dir/.env"

user=${1:-}
password=${2:-}

if [ -z "$user" ] && [ -f "$env_file" ]; then
    user=$(grep -E '^HANGFIRE_DASHBOARD_USER=' "$env_file" | cut -d= -f2- | tr -d '\r')
fi
if [ -z "$password" ] && [ -f "$env_file" ]; then
    password=$(grep -E '^HANGFIRE_DASHBOARD_PASSWORD=' "$env_file" | cut -d= -f2- | tr -d '\r')
fi

user=${user:-admin}

if [ -z "$password" ]; then
    echo "Thiếu mật khẩu. Đặt HANGFIRE_DASHBOARD_PASSWORD trong .env hoặc: $0 <user> <password>" >&2
    exit 1
fi

if ! docker info >/dev/null 2>&1; then
    echo "Docker chưa chạy – hãy bật Docker Desktop rồi thử lại." >&2
    exit 1
fi

# -B: bcrypt (không dùng MD5/crypt mặc định). -n: in ra stdout thay vì ghi file trong container.
docker run --rm httpd:alpine htpasswd -nbB "$user" "$password" > "$script_dir/.htpasswd"

echo "Đã tạo nginx/.htpasswd cho user '$user' (bcrypt)."
echo "Nhớ đặt HANGFIRE_GATE_SECRET trong .env rồi chạy: docker compose up -d nginx api"
