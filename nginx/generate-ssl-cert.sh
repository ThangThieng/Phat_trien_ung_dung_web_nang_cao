#!/bin/sh
# Buổi 7 (NFR-SEC-005, retrofit D-17) – sinh thư mục ./ssl/ cho docker-compose.prod.yml. KHÔNG commit (.gitignore: ssl/,
# *.pfx, *.key; gitleaks chặn khóa riêng):
#   fullchain.pem + privkey.pem — chứng chỉ TLS tự ký cho Nginx (SAN: <domain>, localhost, 127.0.0.1), hạn 825 ngày.
#   dataprotection.pfx          — cặp khóa RSA RIÊNG mã hóa khóa DataProtection trên volume (D-17); mật khẩu lấy từ
#                                 DATAPROTECTION_CERT_PASSWORD trong .env.production.
#
# Vì sao hai cặp khóa: chứng chỉ TLS thật (Let's Encrypt) đổi khóa mỗi ≤ 90 ngày. Nếu dùng chính nó mã hóa khóa DataProtection
# thì sau lần gia hạn, API và worker không giải mã được các khóa cũ nữa. Chứng chỉ của DataProtection phải giữ lâu dài và sao
# lưu riêng — nên script KHÔNG sinh lại dataprotection.pfx khi file đã có (FORCE_DATAPROTECTION=1 để ép, chỉ khi chấp nhận mất
# mọi khóa cũ).
#
# Chỉ cần Docker: openssl chạy trong image mcr.microsoft.com/dotnet/sdk:10.0 mà backend/Dockerfile đã dùng.
#   ./nginx/generate-ssl-cert.sh                  # localhost
#   ./nginx/generate-ssl-cert.sh blog.example.vn  # domain khác
#   ENV_FILE=.env.production ./nginx/generate-ssl-cert.sh
# Production thật: thay fullchain.pem/privkey.pem bằng chứng chỉ của CA, giữ nguyên dataprotection.pfx.
set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repo_dir=$(dirname "$script_dir")
env_file="${ENV_FILE:-$repo_dir/.env.production}"
case "$env_file" in /*) ;; *) env_file="$repo_dir/$env_file" ;; esac
ssl_dir="$repo_dir/ssl"
domain=${1:-localhost}

password=${DATAPROTECTION_CERT_PASSWORD:-}
if [ -z "$password" ] && [ -f "$env_file" ]; then
    password=$(grep -E '^DATAPROTECTION_CERT_PASSWORD=' "$env_file" | tail -n 1 | cut -d= -f2- | tr -d '\r')
fi

make_pfx=1
if [ -f "$ssl_dir/dataprotection.pfx" ] && [ "${FORCE_DATAPROTECTION:-0}" != "1" ]; then
    make_pfx=0
    echo "Giữ nguyên ssl/dataprotection.pfx (đã có — sinh lại sẽ làm mất mọi khóa DataProtection cũ)."
elif [ -z "$password" ]; then
    echo "Thiếu DATAPROTECTION_CERT_PASSWORD (trong $env_file hoặc biến môi trường)." >&2
    exit 1
fi

if ! docker info >/dev/null 2>&1; then
    echo "Docker chưa chạy – hãy bật Docker Desktop rồi thử lại." >&2
    exit 1
fi

san="DNS:$domain,DNS:localhost,IP:127.0.0.1"
[ "$domain" = "localhost" ] && san="DNS:localhost,IP:127.0.0.1"

mkdir -p "$ssl_dir"
# Sinh trong container rồi trả về qua tar trên stdout: không cần bind mount (đường dẫn Windows/Git Bash), file thuộc user hiện tại.
docker run --rm -i -e SAN="$san" -e CN="$domain" -e PFX_PASSWORD="$password" -e MAKE_PFX="$make_pfx" \
    --entrypoint sh mcr.microsoft.com/dotnet/sdk:10.0 -c '
set -eu
mkdir -p /tmp/out && cd /tmp/out
openssl req -x509 -newkey rsa:3072 -sha256 -days 825 -nodes -keyout privkey.pem -out fullchain.pem \
    -subj "/CN=$CN" -addext "subjectAltName=$SAN" \
    -addext "keyUsage=critical,digitalSignature,keyEncipherment" -addext "extendedKeyUsage=serverAuth" 2>/dev/null
chmod 600 privkey.pem && chmod 644 fullchain.pem
if [ "$MAKE_PFX" = "1" ]; then
    openssl req -x509 -newkey rsa:3072 -sha256 -days 3650 -nodes -keyout dp.key -out dp.crt \
        -subj "/CN=CulinaryBlog DataProtection" -addext "keyUsage=critical,keyEncipherment,digitalSignature" 2>/dev/null
    openssl pkcs12 -export -inkey dp.key -in dp.crt -out dataprotection.pfx -passout env:PFX_PASSWORD \
        -keypbe AES-256-CBC -certpbe AES-256-CBC -macalg sha256
    rm dp.key dp.crt
    # 644: container api/hangfire chạy bằng user "app" phải đọc được file mount vào; PFX đã được mật khẩu bảo vệ.
    chmod 644 dataprotection.pfx
fi
tar -cf - .' | tar -C "$ssl_dir" -xf -

echo "Đã tạo ssl/fullchain.pem + ssl/privkey.pem (TLS tự ký cho $domain, 825 ngày)."
[ "$make_pfx" = "1" ] && echo "Đã tạo ssl/dataprotection.pfx (D-17) — SAO LƯU file này cùng mật khẩu; mất nó là mất khóa DataProtection."
echo "Chạy: docker compose -f docker-compose.prod.yml --env-file .env.production up -d --build"
