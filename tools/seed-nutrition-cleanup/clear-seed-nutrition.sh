#!/bin/sh
# CR-2026-07 (MT-70) – chạy MỘT lần trên cơ sở dữ liệu đang có: bỏ dinh dưỡng sinh ngẫu nhiên của 100 công thức do nhóm biên soạn.
# Seeder không ghi đè dữ liệu đã có (SRS §2.6.1), nên volume cũ vẫn giữ số ngẫu nhiên nếu không chạy script này.
#
#   ./tools/seed-nutrition-cleanup/clear-seed-nutrition.sh --dry-run     # chỉ đếm và in những dòng sẽ đổi
#   ./tools/seed-nutrition-cleanup/clear-seed-nutrition.sh               # stack dev (docker-compose.yml + .env)
#   DB_NAME=culinaryblog_copy ./tools/seed-nutrition-cleanup/clear-seed-nutrition.sh   # chạy thử trên bản sao
#   COMPOSE_FILE=docker-compose.prod.yml ENV_FILE=.env.production ./tools/seed-nutrition-cleanup/clear-seed-nutrition.sh
#
# Chỉ đổi công thức của author1..5@culinaryblog.local có slug thuộc 100 công thức cũ VÀ Title, Description còn đúng bản gốc.
# Các bước: (1) sao lưu: sinh script KHÔI PHỤC vào backups/ (gitignore); (2) UPDATE trong MỘT transaction;
# (3) xóa cache Redis "culinaryblog:*" vì DTO đã cache còn số cũ. Windows: chạy bằng Git Bash.
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
db=${DB_NAME:-$(read_env POSTGRES_DB)}
db_user=$(read_env POSTGRES_USER)

dc() { docker compose -f "$compose_file" --env-file "$env_file" "$@"; }
psql_run() { dc exec -T postgres psql -q -v ON_ERROR_STOP=1 -U "$db_user" -d "$db" "$@"; }

# (1) Sao lưu: script khôi phục chỉ chứa Id + 6 cột dinh dưỡng.
mkdir -p "$script_dir/backups"
backup="$script_dir/backups/seed-nutrition-$(date +%Y%m%d-%H%M%S).restore.sql"
cat "$script_dir/legacy-recipes.sql" "$script_dir/backup-nutrition.sql" | psql_run -At > "$backup"
rows=$(grep -c '^UPDATE ' "$backup" || true)
echo "Số công thức sẽ bỏ dinh dưỡng ngẫu nhiên: $rows (bản sao lưu để khôi phục: ${backup#"$repo_dir"/})"

if [ "$rows" -eq 0 ]; then
    echo "Không có công thức nào cần đổi."
    exit 0
fi

if [ "$dry_run" = true ]; then
    echo "--dry-run: chưa đổi gì. Vài dòng đầu của bản sao lưu:"
    grep '^UPDATE ' "$backup" | head -n 3
    exit 0
fi

# (2) Đổi trong MỘT transaction: lỗi ở bất kỳ câu nào thì hủy toàn bộ.
cat "$script_dir/legacy-recipes.sql" "$script_dir/clear-seed-nutrition.sql" | psql_run --single-transaction

# (3) Cache Redis của API chỉ là bản sao, xóa an toàn.
deleted=$(dc exec -T redis sh -c "redis-cli --scan --pattern 'culinaryblog:*' | xargs -r redis-cli del | awk '{s+=\$1} END {print s+0}'")
echo "Đã xóa $deleted khóa cache Redis 'culinaryblog:*'."
echo "Hoàn tác (nếu cần): docker compose exec -T postgres psql -U $db_user -d $db < ${backup#"$repo_dir"/}"
