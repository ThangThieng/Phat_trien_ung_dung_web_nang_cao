#!/bin/sh
# Retrofit D-14 (Buổi 7) – chạy MỘT lần khi chuyển URL ảnh sang Nginx /media/: đổi URL đã lưu trong cơ sở dữ liệu từ
# http://localhost:9000/culinary-blog/<key> sang <MEDIA_PUBLIC_URL>/<key> (6 cột — xem migrate-media-urls.sql).
#
#   ./tools/media-url-migration/migrate-media-urls.sh --dry-run     # chỉ đếm và in những dòng sẽ đổi
#   ./tools/media-url-migration/migrate-media-urls.sh               # stack dev (docker-compose.yml + .env)
#   DB_NAME=culinaryblog_copy ./tools/media-url-migration/migrate-media-urls.sh   # chạy thử trên bản sao
#   COMPOSE_FILE=docker-compose.prod.yml ENV_FILE=.env.production OLD_PREFIX=https://cu.example/culinary-blog/ \
#       ./tools/media-url-migration/migrate-media-urls.sh
#
# Các bước: (1) sao lưu — sinh script KHÔI PHỤC (đặt lại URL cũ theo Id) vào backups/ (gitignore); (2) đổi URL trong MỘT
# transaction, tự hủy nếu còn sót URL cũ; (3) xóa cache Redis "culinaryblog:*" của API — DTO đã cache còn URL cũ tới hết TTL.
# Chạy SAU khi api/hangfire đã nhận MinIO__PublicBaseUrl mới (docker compose up -d --build). Windows: chạy bằng Git Bash.
set -eu
export MSYS_NO_PATHCONV=1

# Git Bash: Docker là chương trình Windows nên cần đường dẫn dạng D:/… (pwd -W); Linux/macOS: pwd thường.
win_pwd() { pwd -W 2>/dev/null || pwd; }
script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && win_pwd)
repo_dir=$(CDPATH= cd -- "$script_dir/../.." && win_pwd)
env_file=${ENV_FILE:-$repo_dir/.env}
compose_file=${COMPOSE_FILE:-$repo_dir/docker-compose.yml}
dry_run=false
[ "${1:-}" = "--dry-run" ] && dry_run=true

case "$env_file" in /*|?:*) ;; *) env_file="$repo_dir/$env_file" ;; esac
case "$compose_file" in /*|?:*) ;; *) compose_file="$repo_dir/$compose_file" ;; esac
[ -f "$env_file" ] || { echo "Không thấy $env_file" >&2; exit 1; }

read_env() { grep -E "^$1=" "$env_file" | tail -n 1 | cut -d= -f2- | tr -d '\r'; }
with_slash() { case "$1" in */) printf '%s' "$1" ;; *) printf '%s/' "$1" ;; esac; }

media_url=$(read_env MEDIA_PUBLIC_URL)
old_prefix=$(with_slash "${OLD_PREFIX:-http://localhost:9000/culinary-blog/}")
new_prefix=$(with_slash "${NEW_PREFIX:-${media_url:-http://localhost/media}}")
db=${DB_NAME:-$(read_env POSTGRES_DB)}   # DB_NAME: chạy thử trên bản sao của cơ sở dữ liệu
db_user=$(read_env POSTGRES_USER)

if [ "$old_prefix" = "$new_prefix" ]; then
    echo "Tiền tố cũ và mới giống nhau ($old_prefix) — không có gì để đổi." >&2
    exit 1
fi

dc() { docker compose -f "$compose_file" --env-file "$env_file" "$@"; }
psql_run() { dc exec -T postgres psql -v ON_ERROR_STOP=1 -U "$db_user" -d "$db" "$@"; }

echo "Đổi URL ảnh: $old_prefix  →  $new_prefix"

# (1) Sao lưu: script khôi phục chỉ chứa Id + URL cũ.
mkdir -p "$script_dir/backups"
backup="$script_dir/backups/media-urls-$(date +%Y%m%d-%H%M%S).restore.sql"
psql_run -At -v old_prefix="$old_prefix" < "$script_dir/backup-media-urls.sql" > "$backup"
rows=$(grep -c '^UPDATE ' "$backup" || true)
echo "Số giá trị URL sẽ đổi: $rows (bản sao lưu để khôi phục: ${backup#"$repo_dir"/})"

if [ "$rows" -eq 0 ]; then
    echo "Không có URL nào mang tiền tố cũ — không đổi gì."
    exit 0
fi

if [ "$dry_run" = true ]; then
    echo "--dry-run: chưa đổi gì. Vài dòng đầu của bản sao lưu:"
    grep '^UPDATE ' "$backup" | head -n 5
    exit 0
fi

# (2) Đổi URL trong MỘT transaction (--single-transaction): lỗi ở bất kỳ câu nào hoặc còn sót URL cũ → hủy toàn bộ.
psql_run --single-transaction -v old_prefix="$old_prefix" -v new_prefix="$new_prefix" < "$script_dir/migrate-media-urls.sql"

# (3) Cache Redis của API (cache-aside, chỉ là bản sao — xóa an toàn): công thức, danh sách, tìm kiếm, danh mục.
deleted=$(dc exec -T redis sh -c "redis-cli --scan --pattern 'culinaryblog:*' | xargs -r redis-cli del | awk '{s+=\$1} END {print s+0}'")
echo "Đã xóa $deleted khóa cache Redis 'culinaryblog:*'."

echo "Xong. Data Cache của Next.js (≤ 30 phút) được làm mới khi tạo lại container frontend:"
echo "  docker compose -f ${compose_file#"$repo_dir"/} up -d --force-recreate --no-deps frontend"
echo "Hoàn tác (nếu cần): docker compose exec -T postgres psql -U $db_user -d $db < ${backup#"$repo_dir"/}"
