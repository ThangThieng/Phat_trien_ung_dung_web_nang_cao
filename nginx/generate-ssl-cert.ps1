# Buổi 7 (NFR-SEC-005, retrofit D-17) – sinh ./ssl/ cho docker-compose.prod.yml (bản Windows).
# Xem chú thích đầy đủ ở generate-ssl-cert.sh (vì sao TLS và DataProtection dùng hai cặp khóa, vì sao không sinh lại PFX).
#
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-ssl-cert.ps1
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-ssl-cert.ps1 -Domain blog.example.vn
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-ssl-cert.ps1 -EnvFile .env.production
param(
    [string]$Domain = 'localhost',
    [string]$EnvFile = '.env.production',
    [switch]$ForceDataProtection
)

$ErrorActionPreference = 'Stop'

$repoDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
# PowerShell không phân biệt hoa/thường tên biến: dùng tên khác hẳn tham số -EnvFile.
$envPath = if ([System.IO.Path]::IsPathRooted($EnvFile)) { $EnvFile } else { Join-Path $repoDir $EnvFile }
$sslDir = Join-Path $repoDir 'ssl'

$password = $env:DATAPROTECTION_CERT_PASSWORD
if ([string]::IsNullOrWhiteSpace($password) -and (Test-Path $envPath)) {
    $line = Select-String -Path $envPath -Pattern '^DATAPROTECTION_CERT_PASSWORD=' | Select-Object -Last 1
    if ($null -ne $line) { $password = ($line.Line -replace '^DATAPROTECTION_CERT_PASSWORD=', '').Trim() }
}

$makePfx = '1'
if ((Test-Path (Join-Path $sslDir 'dataprotection.pfx')) -and -not $ForceDataProtection) {
    $makePfx = '0'
    Write-Output 'Giữ nguyên ssl/dataprotection.pfx (đã có — sinh lại sẽ làm mất mọi khóa DataProtection cũ).'
} elseif ([string]::IsNullOrWhiteSpace($password)) {
    throw "Thiếu DATAPROTECTION_CERT_PASSWORD (trong $envPath hoặc biến môi trường)."
}

docker info --format '{{.ServerVersion}}' *> $null
if ($LASTEXITCODE -ne 0) { throw 'Docker chưa chạy – hãy bật Docker Desktop rồi thử lại.' }

$san = if ($Domain -eq 'localhost') { 'DNS:localhost,IP:127.0.0.1' } else { "DNS:$Domain,DNS:localhost,IP:127.0.0.1" }
New-Item -ItemType Directory -Force $sslDir | Out-Null

# Bind mount thay cho tar qua stdout: PowerShell 5.1 đổi dữ liệu nhị phân trong pipeline thành chuỗi và làm hỏng file.
$script = @'
set -eu
cd /out
openssl req -x509 -newkey rsa:3072 -sha256 -days 825 -nodes -keyout privkey.pem -out fullchain.pem \
    -subj "/CN=$CN" -addext "subjectAltName=$SAN" \
    -addext "keyUsage=critical,digitalSignature,keyEncipherment" -addext "extendedKeyUsage=serverAuth" 2>/dev/null
if [ "$MAKE_PFX" = "1" ]; then
    openssl req -x509 -newkey rsa:3072 -sha256 -days 3650 -nodes -keyout /tmp/dp.key -out /tmp/dp.crt \
        -subj "/CN=CulinaryBlog DataProtection" -addext "keyUsage=critical,keyEncipherment,digitalSignature" 2>/dev/null
    openssl pkcs12 -export -inkey /tmp/dp.key -in /tmp/dp.crt -out dataprotection.pfx -passout env:PFX_PASSWORD \
        -keypbe AES-256-CBC -certpbe AES-256-CBC -macalg sha256
fi
'@
# Đoạn lệnh ghi ra file tạm (UTF-8 không BOM, LF) trong thư mục được mount rồi chạy: PowerShell 5.1 không thoát dấu " khi
# truyền "-c <script>" cho chương trình ngoài, và khi đẩy chuỗi qua stdin thì tự thêm BOM — cả hai đều làm hỏng lệnh sh.
$tempScript = Join-Path $sslDir '.generate-ssl-cert.sh'
[System.IO.File]::WriteAllText($tempScript, ($script -replace "`r", ''), (New-Object System.Text.UTF8Encoding $false))
try {
    docker run --rm -v "${sslDir}:/out" -e "SAN=$san" -e "CN=$Domain" -e "PFX_PASSWORD=$password" -e "MAKE_PFX=$makePfx" `
        --entrypoint sh mcr.microsoft.com/dotnet/sdk:10.0 /out/.generate-ssl-cert.sh
} finally {
    Remove-Item $tempScript -ErrorAction SilentlyContinue
}
if ($LASTEXITCODE -ne 0) { throw 'Sinh chứng chỉ thất bại.' }

Write-Output "Đã tạo ssl/fullchain.pem + ssl/privkey.pem (TLS tự ký cho $Domain, 825 ngày)."
if ($makePfx -eq '1') { Write-Output 'Đã tạo ssl/dataprotection.pfx (D-17) — SAO LƯU file này cùng mật khẩu; mất nó là mất khóa DataProtection.' }
Write-Output 'Chạy: docker compose -f docker-compose.prod.yml --env-file .env.production up -d --build'
