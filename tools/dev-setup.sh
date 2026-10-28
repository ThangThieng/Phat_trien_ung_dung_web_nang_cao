#!/bin/sh
# Buổi 8 (NFR-MAINT-003 — README "dựng môi trường dev trong < 5 phút"): chuẩn bị máy mới một lần rồi (tùy chọn) dựng stack dev.
#
#   ./tools/dev-setup.sh          # tạo .env (nếu chưa có) + nginx/.htpasswd + hook gitleaks
#   ./tools/dev-setup.sh --up     # … rồi docker compose --profile dev up -d --build và chờ 8 service healthy
#
# Chạy bằng sh/bash (Linux, macOS, Git Bash trên Windows). Cần Docker đang chạy và openssl (Git Bash có sẵn).
#
# - .env đã có thì KHÔNG ghi đè giá trị nào; chỉ điền những khóa bí mật còn TRỐNG (JWT_SIGNING_KEY, AUTH_SECRET, REVALIDATE_SECRET,
#   HANGFIRE_GATE_SECRET, HANGFIRE_DASHBOARD_PASSWORD, SEED_ADMIN_PASSWORD, SEED_AUTHOR_PASSWORD). Mật khẩu PostgreSQL/MinIO giữ giá trị
#   mẫu của .env.example (chỉ dùng cho máy dev) — volume dữ liệu đã khởi tạo với mật khẩu nào thì phải giữ mật khẩu đó.
# - Mật khẩu tài khoản mẫu sinh ngẫu nhiên nhưng luôn đạt NFR-SEC-001 (≥ 8 ký tự, có hoa, thường, số, ký tự đặc biệt) và được in ra màn
#   hình MỘT lần để đăng nhập thử. .env không bao giờ được commit (.gitignore + gitleaks — NFR-SEC-007).
set -eu

repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$repo_dir"
env_file="$repo_dir/.env"

random_b64() { openssl rand -base64 "$1" | tr -d '\n=/+' | cut -c1-"$2"; }
random_hex() { openssl rand -hex "$1"; }
# Mật khẩu đạt chính sách Identity của hệ thống: tiền tố cố định bảo đảm đủ 4 loại ký tự, phần sau ngẫu nhiên.
random_password() { printf 'Cb#9%s' "$(random_b64 24 14)"; }

if ! docker info >/dev/null 2>&1; then
    echo "Docker chưa chạy — bật Docker Desktop rồi chạy lại." >&2
    exit 1
fi

if [ ! -f "$env_file" ]; then
    cp .env.example "$env_file"
    echo "Đã tạo .env từ .env.example."
fi

# Điền khóa còn trống (không đụng tới khóa đã có giá trị).
fill() {
    key=$1
    value=$2
    if grep -qE "^${key}=[[:space:]]*$" "$env_file"; then
        tmp="$env_file.tmp.$$"
        awk -v k="$key" -v v="$value" 'BEGIN { FS = OFS = "=" } $1 == k && $2 ~ /^[ \t]*$/ { print k "=" v; next } { print }' "$env_file" > "$tmp"
        mv "$tmp" "$env_file"
        filled="${filled:-} $key"
    elif ! grep -qE "^${key}=" "$env_file"; then
        printf '%s=%s\n' "$key" "$value" >> "$env_file"
        filled="${filled:-} $key"
    fi
}

admin_password=$(random_password)
author_password=$(random_password)
fill JWT_SIGNING_KEY "$(random_b64 96 88)"
fill AUTH_SECRET "$(random_b64 48 44)"
fill REVALIDATE_SECRET "$(random_hex 24)"
fill HANGFIRE_GATE_SECRET "$(random_hex 32)"
fill HANGFIRE_DASHBOARD_PASSWORD "$(random_password)"
fill SEED_ADMIN_PASSWORD "$admin_password"
fill SEED_AUTHOR_PASSWORD "$author_password"
echo "Khóa đã sinh:${filled:- (không — .env đã đủ)}"

# Basic Auth của Hangfire Dashboard: thiếu file thì Docker tạo THƯ MỤC trùng tên và Nginx báo lỗi khi mở /hangfire.
if [ ! -f nginx/.htpasswd ]; then
    if [ -d nginx/.htpasswd ]; then
        rmdir nginx/.htpasswd 2>/dev/null || { echo "nginx/.htpasswd là thư mục không rỗng — xóa tay rồi chạy lại." >&2; exit 1; }
    fi
    sh nginx/generate-htpasswd.sh
fi

git config core.hooksPath .githooks 2>/dev/null && echo "Đã bật pre-commit gitleaks (.githooks)."

case " ${filled:-} " in
    *" SEED_ADMIN_PASSWORD "*) echo "Tài khoản mẫu: admin@culinaryblog.local / $admin_password" ;;
esac
case " ${filled:-} " in
    *" SEED_AUTHOR_PASSWORD "*) echo "               author1..5@culinaryblog.local / $author_password" ;;
esac

if [ "${1:-}" = "--up" ]; then
    start=$(date +%s)
    docker compose --profile dev up -d --build
    echo "Chờ 8 service healthy…"
    deadline=$((start + 600))
    while :; do
        healthy=$(docker compose --profile dev ps --format '{{.Health}}' | grep -c '^healthy$' || true)
        [ "$healthy" -ge 8 ] && break
        if [ "$(date +%s)" -ge "$deadline" ]; then
            docker compose --profile dev ps
            echo "Quá 10 phút mà mới có $healthy/8 service healthy." >&2
            exit 1
        fi
        sleep 5
    done
    echo "Sẵn sàng sau $(( $(date +%s) - start )) giây: http://localhost (Nginx) · http://localhost:5000/scalar/v1 · http://localhost:5341 (Seq) · http://localhost:8025 (MailHog)"
fi
