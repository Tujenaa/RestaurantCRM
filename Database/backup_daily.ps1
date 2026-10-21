# ==============================================================================
# SCRIPT TỰ ĐỘNG SAO LƯU CƠ SỞ DỮ LIỆU SQL SERVER HÀNG NGÀY
# Hệ thống: RestaurantCRM
# Sử dụng: Có thể chạy thủ công hoặc lên lịch qua Windows Task Scheduler
# ==============================================================================

param (
    [string]$ServerInstance = ".\SQLEXPRESS",
    [string]$DatabaseName = "RestaurantCRM",
    [string]$BackupDir = "D:\HTTT Doanh Nghiep\RestaurantCRM\Database\Backups",
    [int]$RetentionDays = 30
)

# 1. Tạo thư mục backup nếu chưa tồn tại
if (-not (Test-Path -Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
    Write-Host "Da tao thu muc backup: $BackupDir" -ForegroundColor Cyan
}

# 2. Định danh file backup theo timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFileName = "${DatabaseName}_Backup_${timestamp}.bak"
$backupFilePath = Join-Path -Path $BackupDir -ChildPath $backupFileName

Write-Host "Dang thuc hien sao luu CSDL [$DatabaseName]..." -ForegroundColor Yellow

# 3. Lệnh T-SQL sao lưu
$sqlQuery = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$backupFilePath'
WITH FORMAT, INIT,
NAME = N'$DatabaseName-Scheduled Full Backup $timestamp',
SKIP, NOREWIND, NOUNLOAD, STATS = 10;
"@

try {
    # Thử chạy lệnh qua sqlcmd nếu có sẵn, hoặc qua .NET SqlClient
    $connectionString = "Server=$ServerInstance;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
    $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
    if (-not $connection) {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    }
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $sqlQuery
    $command.CommandTimeout = 600
    $command.ExecuteNonQuery() | Out-Null
    $connection.Close()

    Write-Host "Sao luu thanh cong! File luu tai: $backupFilePath" -ForegroundColor Green

    # Lưu file ghi chú đính kèm
    $noteFilePath = [System.IO.Path]::ChangeExtension($backupFilePath, ".txt")
    "Sao lưu tự động qua PowerShell Script lúc $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" | Out-File -FilePath $noteFilePath -Encoding utf8

} catch {
    Write-Error "Loi trong qua trinh sao luu CSDL: $_"
}

# 4. Tự động dọn dẹp các bản backup cũ hơn $RetentionDays ngày
Write-Host "Dang kiem tra va don dep cac ban backup cu hon $RetentionDays ngay..." -ForegroundColor Cyan
$limitDate = (Get-Date).AddDays(-$RetentionDays)

Get-ChildItem -Path $BackupDir -Filter "*.bak" | Where-Object { $_.CreationTime -lt $limitDate } | ForEach-Object {
    Write-Host "Xoa ban backup cu: $($_.Name)" -ForegroundColor DarkGray
    Remove-Item -Path $_.FullName -Force

    $oldNote = [System.IO.Path]::ChangeExtension($_.FullName, ".txt")
    if (Test-Path $oldNote) {
        Remove-Item -Path $oldNote -Force
    }
}

Write-Host "Hoan tat quy trinh sao luu va don dep!" -ForegroundColor Green
