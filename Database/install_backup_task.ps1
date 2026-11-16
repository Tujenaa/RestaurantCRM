# ==============================================================================
# CAI DAT LICH SAO LUU TU DONG HANG NGAY (Windows Task Scheduler) - RestaurantCRM
# ------------------------------------------------------------------------------
# PHAI chay bang PowerShell "Run as Administrator":
#   powershell -ExecutionPolicy Bypass -File .\install_backup_task.ps1
#   powershell -ExecutionPolicy Bypass -File .\install_backup_task.ps1 -Time "02:30"
#   powershell -ExecutionPolicy Bypass -File .\install_backup_task.ps1 -RunAsCurrentUser
#
# - Tao thu muc Database\Backups va cap quyen ghi cho dich vu SQL Server.
# - Dang ky task chay backup_daily.ps1 hang ngay luc $Time, quyen cao nhat,
#   StartWhenAvailable (may tat luc gio hen thi chay bu khi mo may).
# - Mac dinh chay duoi tai khoan SYSTEM. Neu SYSTEM khong co quyen BACKUP trong
#   SQL Server, them -RunAsCurrentUser de chay bang tai khoan Windows hien tai
#   (kieu S4U: khong can luu mat khau, du de ket noi SQL Server tren cung may).
# - Chay lai nhieu lan khong loi (Register-ScheduledTask -Force).
# ==============================================================================
#Requires -RunAsAdministrator

param (
    [string]$Time = "01:00",
    [string]$TaskName = "RestaurantCRM Daily Backup",
    [string]$ServerInstance = ".\SQLEXPRESS",
    [string]$DatabaseName = "RestaurantCRM",
    [switch]$RunAsCurrentUser
)

$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $PSScriptRoot "backup_daily.ps1"
$backupDir = Join-Path $PSScriptRoot "Backups"

if (-not (Test-Path -LiteralPath $scriptPath)) {
    Write-Host "Khong tim thay $scriptPath" -ForegroundColor Red
    exit 1
}

try {
    $at = [DateTime]::ParseExact($Time, "HH:mm", [Globalization.CultureInfo]::InvariantCulture)
}
catch {
    Write-Host "Gio khong hop le: '$Time' (dinh dang HH:mm, vd 01:00)" -ForegroundColor Red
    exit 1
}

# 1. Thu muc sao luu + quyen ghi cho dich vu SQL Server
if (-not (Test-Path -LiteralPath $backupDir)) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "Da tao thu muc: $backupDir" -ForegroundColor Cyan
}

$sqlServiceAccount = 'NT Service\MSSQL$SQLEXPRESS'
& icacls.exe "$backupDir" /grant "${sqlServiceAccount}:(OI)(CI)M" | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Da cap quyen ghi cho $sqlServiceAccount tren $backupDir" -ForegroundColor Green
} else {
    Write-Host "CANH BAO: icacls that bai (ma $LASTEXITCODE). Kiem tra ten instance SQL Server." -ForegroundColor Yellow
}

# 2. Dang ky task
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`"" `
    -WorkingDirectory $PSScriptRoot
$trigger = New-ScheduledTaskTrigger -Daily -At $at
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 2) `
    -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

if ($RunAsCurrentUser) {
    $userId = "$env:USERDOMAIN\$env:USERNAME"
    $principal = New-ScheduledTaskPrincipal -UserId $userId -LogonType S4U -RunLevel Highest
} else {
    $userId = "SYSTEM"
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
}

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
    -Principal $principal -Description "Sao luu CSDL $DatabaseName hang ngay ($scriptPath)" -Force | Out-Null

Write-Host "Da dang ky task '$TaskName': hang ngay luc $Time, tai khoan $userId" -ForegroundColor Green

# 3. Kiem tra (best-effort) quyen SQL cua SYSTEM
if (-not $RunAsCurrentUser) {
    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    $isSysadmin = $null
    if ($sqlcmd) {
        $ErrorActionPreference = "Continue"
        $out = & $sqlcmd.Source -S $ServerInstance -d master -E -b -h -1 -W -Q "SET NOCOUNT ON; SELECT ISNULL(IS_SRVROLEMEMBER('sysadmin','NT AUTHORITY\SYSTEM'),0);" 2>&1 |
            ForEach-Object { "$_" }
        $ErrorActionPreference = "Stop"
        if ($LASTEXITCODE -eq 0) { $isSysadmin = (($out -join "").Trim() -eq "1") }
    }

    if ($isSysadmin -ne $true) {
        Write-Host ""
        if ($isSysadmin -eq $false) {
            Write-Host "CANH BAO: tai khoan NT AUTHORITY\SYSTEM khong phai sysadmin tren $ServerInstance," -ForegroundColor Yellow
            Write-Host "          task co the loi khi BACKUP (Last Run Result = 0x1, xem backup_log.txt)." -ForegroundColor Yellow
        } else {
            Write-Host "Luu y: khong kiem tra duoc quyen SQL cua SYSTEM (khong co sqlcmd hoac khong ket noi duoc)." -ForegroundColor Yellow
        }
        Write-Host "Neu task loi do quyen, chon MOT trong hai cach:" -ForegroundColor Yellow
        Write-Host "  (a) Chay task bang tai khoan Windows hien tai (dang dang nhap SQL duoc):"
        Write-Host "      powershell -ExecutionPolicy Bypass -File `"$PSCommandPath`" -RunAsCurrentUser"
        Write-Host "  (b) Cap quyen sao luu cho SYSTEM (chay trong SSMS bang tai khoan sysadmin):"
        Write-Host "      IF SUSER_ID('NT AUTHORITY\SYSTEM') IS NULL CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;"
        Write-Host "      USE [$DatabaseName]; IF USER_ID('NT AUTHORITY\SYSTEM') IS NULL CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];"
        Write-Host "      ALTER ROLE db_backupoperator ADD MEMBER [NT AUTHORITY\SYSTEM];"
    }
}

# 4. Huong dan kiem tra
Write-Host ""
Write-Host "Kiem tra ngay:" -ForegroundColor Cyan
Write-Host "  Start-ScheduledTask -TaskName `"$TaskName`""
Write-Host "  Get-ScheduledTaskInfo -TaskName `"$TaskName`" | Select-Object LastRunTime, LastTaskResult, NextRunTime"
Write-Host "    (LastTaskResult = 0: thanh cong; 1: loi; 267009: dang chay)"
Write-Host "  Get-Content `"$(Join-Path $backupDir 'backup_log.txt')`" -Tail 20"
Write-Host "Hoac mo Task Scheduler > Task Scheduler Library > '$TaskName' > cot 'Last Run Result'."
exit 0
