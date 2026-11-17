# FR-JOB-001 / MT-39 – sinh nginx/.htpasswd cho Basic Auth của Hangfire Dashboard (bản Windows).
# Xem chú thích đầy đủ ở generate-htpasswd.sh.
#
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-htpasswd.ps1
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-htpasswd.ps1 -User admin -Password 'MatKhau'
#   powershell -ExecutionPolicy Bypass -File .\nginx\generate-htpasswd.ps1 -EnvFile .env.production
param(
    [string]$User,
    [string]$Password,
    [string]$EnvFile = '.env'
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# PowerShell không phân biệt hoa/thường tên biến: dùng tên khác hẳn tham số -EnvFile.
$envPath = if ([System.IO.Path]::IsPathRooted($EnvFile)) { $EnvFile } else { Join-Path (Split-Path -Parent $scriptDir) $EnvFile }

function Get-EnvValue([string]$key) {
    if (-not (Test-Path $envPath)) { return '' }
    $line = Select-String -Path $envPath -Pattern "^$key=" | Select-Object -First 1
    if ($null -eq $line) { return '' }
    return ($line.Line -replace "^$key=", '').Trim()
}

if ([string]::IsNullOrWhiteSpace($User)) { $User = Get-EnvValue 'HANGFIRE_DASHBOARD_USER' }
if ([string]::IsNullOrWhiteSpace($Password)) { $Password = Get-EnvValue 'HANGFIRE_DASHBOARD_PASSWORD' }
if ([string]::IsNullOrWhiteSpace($User)) { $User = 'admin' }

if ([string]::IsNullOrWhiteSpace($Password)) {
    throw "Thiếu mật khẩu. Đặt HANGFIRE_DASHBOARD_PASSWORD trong .env hoặc truyền -Password."
}

docker info --format '{{.ServerVersion}}' *> $null
if ($LASTEXITCODE -ne 0) { throw 'Docker chưa chạy – hãy bật Docker Desktop rồi thử lại.' }

# -B: bcrypt. Ghi bằng ASCII + LF: Nginx không chấp nhận BOM trong .htpasswd.
$hash = docker run --rm httpd:alpine htpasswd -nbB $User $Password
if ($LASTEXITCODE -ne 0) { throw 'htpasswd thất bại.' }

$outFile = Join-Path $scriptDir '.htpasswd'
[System.IO.File]::WriteAllText($outFile, ($hash -join "`n").Trim() + "`n", (New-Object System.Text.ASCIIEncoding))

Write-Output "Đã tạo nginx/.htpasswd cho user '$User' (bcrypt)."
Write-Output 'Nhớ đặt HANGFIRE_GATE_SECRET trong .env rồi chạy: docker compose up -d nginx api'
