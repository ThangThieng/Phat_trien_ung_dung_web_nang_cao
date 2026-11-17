#!/bin/sh
# Buổi 7 (NFR-SCALE-003, MT-50) – cập nhật/tạo lại các bản sao "api" của docker-compose.prod.yml KHÔNG gián đoạn.
#
# Vì sao cần: "docker compose up -d --force-recreate --no-deps api" dừng CẢ BA bản sao cùng lúc (Docker Compose không có cập
# nhật cuốn chiếu như Swarm/Kubernetes). Đo ngày 21/10/2026 với request liên tục qua Nginx: 502 trong ~3 giây (7/372 request
# /health/live, 2/113 request /api/v1/categories). Resolver động của Nginx (MT-50) chỉ bảo đảm 502 không kéo dài; muốn KHÔNG có
# 502 thì luôn phải còn bản sao sẵn sàng trong lúc thay.
#
# Cách làm:
#   1. Tạo thêm N bản sao mới (image/cấu hình hiện tại) bên cạnh N bản sao cũ: --scale api=2N --no-recreate.
#   2. Chờ bản sao mới healthy (/health/ready) rồi chờ thêm > 10 giây — Nginx phân giải lại tên "api" mỗi 10 giây
#      (resolver valid=10s) nên đã gửi traffic sang bản sao mới.
#   3. Dừng êm từng bản sao cũ (SIGTERM: Kestrel ngừng nhận kết nối mới, request đang chạy được xử lý xong) rồi xóa. Request
#      Nginx gửi tới bản sao vừa dừng bị từ chối và được chuyển sang bản sao khác (proxy_next_upstream error timeout).
#
#   ./tools/deploy/rolling-update-api.sh             # dùng docker-compose.prod.yml + .env.production
#   docker compose -f docker-compose.prod.yml --env-file .env.production build api   # (nếu đổi code) build trước rồi chạy script
set -eu
export MSYS_NO_PATHCONV=1

win_pwd() { pwd -W 2>/dev/null || pwd; }
repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && win_pwd)
compose_file=${COMPOSE_FILE:-$repo_dir/docker-compose.prod.yml}
env_file=${ENV_FILE:-$repo_dir/.env.production}
dc() { docker compose -f "$compose_file" --env-file "$env_file" "$@"; }

old_ids=$(dc ps -q api)
count=$(printf '%s\n' "$old_ids" | grep -c . || true)
if [ "$count" -eq 0 ]; then
    echo "Không có bản sao api nào đang chạy — dùng: docker compose -f docker-compose.prod.yml --env-file .env.production up -d" >&2
    exit 1
fi
echo "Đang có $count bản sao api. Tạo $count bản sao mới bên cạnh…"

dc up -d --no-deps --no-recreate --scale "api=$((count * 2))" api >/dev/null 2>&1

new_ids=$(dc ps -q api | grep -v -F "$old_ids" || true)
deadline=$(( $(date +%s) + 180 ))
for id in $new_ids; do
    while [ "$(docker inspect -f '{{.State.Health.Status}}' "$id")" != "healthy" ]; do
        if [ "$(date +%s)" -gt "$deadline" ]; then
            echo "Bản sao mới $(docker inspect -f '{{.Name}}' "$id") không healthy sau 180 giây — giữ nguyên bản sao cũ, gỡ bản mới." >&2
            for bad in $new_ids; do docker rm -f "$bad" >/dev/null; done
            exit 1
        fi
        sleep 2
    done
    echo "  $(docker inspect -f '{{.Name}}' "$id" | tr -d /) healthy"
done

echo "Chờ 12 giây để Nginx phân giải lại tên api (resolver valid=10s)…"
sleep 12

for id in $old_ids; do
    name=$(docker inspect -f '{{.Name}}' "$id" | tr -d /)
    docker stop -t 30 "$id" >/dev/null
    docker rm "$id" >/dev/null
    echo "  đã dừng êm và gỡ $name"
done

echo "Xong:"
dc ps api --format '  {{.Name}}  {{.Status}}'
