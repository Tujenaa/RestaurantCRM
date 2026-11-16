# ==============================================================================
# SCRIPT SAO LUU TU DONG CSDL SQL SERVER HANG NGAY - He thong RestaurantCRM
# ------------------------------------------------------------------------------
# - Chay duoc tren Windows PowerShell 5.1 va PowerShell 7, khong can cai module.
# - Uu tien dung sqlcmd; neu may khong co sqlcmd thi dung System.Data.SqlClient.
# - Tao file {Db}_Auto_{yyyyMMdd_HHmmss}.bak (BACKUP ... WITH CHECKSUM), sau do
#   kiem tra bang RESTORE VERIFYONLY ... WITH CHECKSUM.
# - Ghi file ghi chu .txt cung ten de man hinh "Sao luu du lieu" cua app hien thi.
# - Chi khi ban moi thanh cong moi xoa cac ban _Auto_ cu hon $RetentionDays ngay.
#   KHONG BAO GIO xoa ban _Backup_ tao tay tu app.
# - Nhat ky: <BackupDir>\backup_log.txt. Exit code: 0 = thanh cong, 1 = loi
#   (Task Scheduler se hien "Last Run Result" = 0x1 khi loi).
#
# Chay thu (mo PowerShell tai thu muc Database):
#   powershell -ExecutionPolicy Bypass -File .\backup_daily.ps1
#   echo $LASTEXITCODE
# Vi du doi tham so:
#   powershell -ExecutionPolicy Bypass -File .\backup_daily.ps1 -ServerInstance ".\SQLEXPRESS" -RetentionDays 14
# Cai lich chay hang ngay: xem install_backup_task.ps1 (chay bang quyen Administrator).
# ==============================================================================

param (
    [string]$ServerInstance = ".\SQLEXPRESS",
    [string]$DatabaseName = "RestaurantCRM",
    [string]$BackupDir = (Join-Path $PSScriptRoot "Backups"),
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------- Ham tien ich

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $line = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Level, $Message
    if ($Level -eq "ERROR") { Write-Host $line -ForegroundColor Red } else { Write-Host $line }
    try { Add-Content -Path $script:LogFile -Value $line -Encoding UTF8 } catch { }
}

# Escape chuoi de ghep vao literal N'...' cua T-SQL
function ConvertTo-SqlLiteral {
    param([string]$Value)
    return $Value.Replace("'", "''")
}

# Escape ten doi tuong de ghep vao [...] cua T-SQL
function ConvertTo-SqlIdentifier {
    param([string]$Value)
    return "[" + $Value.Replace("]", "]]") + "]"
}

function Invoke-Sql {
    param([string]$Query)

    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($sqlcmd) {
        $sqlArgs = @("-S", $ServerInstance, "-d", "master", "-E", "-b")
        if ($script:SqlcmdSupportsC) { $sqlArgs += "-C" }
        $sqlArgs += @("-Q", $Query)

        # PS 5.1 + ErrorActionPreference=Stop bien stderr cua exe thanh loi -> tam doi sang Continue
        $oldPref = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        try {
            $output = & $sqlcmd.Source @sqlArgs 2>&1 | ForEach-Object { "$_" }
            $exitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $oldPref
        }

        $text = ($output -join [Environment]::NewLine).Trim()
        if ($exitCode -ne 0) {
            throw "sqlcmd tra ve ma loi $exitCode. $text"
        }
        return $text
    }

    # Du phong: System.Data.SqlClient co san trong .NET Framework (PS 5.1) va PS 7
    $connString = "Server=$ServerInstance;Database=master;Integrated Security=True;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection($connString)
    $cmd = $null
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $Query
        $cmd.CommandTimeout = 1800
        [void]$cmd.ExecuteNonQuery()
        return ""
    }
    finally {
        if ($cmd) { $cmd.Dispose() }
        $conn.Dispose()
    }
}

# ---------------------------------------------------------------- Chuan bi

try {
    if (-not (Test-Path -LiteralPath $BackupDir)) {
        New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
    }
    $BackupDir = (Resolve-Path -LiteralPath $BackupDir).Path
}
catch {
    Write-Host "Khong tao duoc thu muc sao luu '$BackupDir': $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$script:LogFile = Join-Path $BackupDir "backup_log.txt"

# sqlcmd ban moi (ODBC 18 / go-sqlcmd) co co -C (TrustServerCertificate); ban cu bao loi neu truyen -C
$script:SqlcmdSupportsC = $false
$sqlcmdInfo = Get-Command sqlcmd -ErrorAction SilentlyContinue
if ($sqlcmdInfo) {
    $oldPref = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $help = (& $sqlcmdInfo.Source "-?" 2>&1 | ForEach-Object { "$_" }) -join " "
        $script:SqlcmdSupportsC = $help -cmatch '(^|[\s\[])-C([\s,\]]|$)'
    }
    catch { $script:SqlcmdSupportsC = $false }
    finally { $ErrorActionPreference = $oldPref }
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFileName = "${DatabaseName}_Auto_${timestamp}.bak"
$backupFilePath = Join-Path $BackupDir $backupFileName
$noteFilePath = [System.IO.Path]::ChangeExtension($backupFilePath, ".txt")

$dbIdent = ConvertTo-SqlIdentifier $DatabaseName
$pathLit = ConvertTo-SqlLiteral $backupFilePath
$nameLit = ConvertTo-SqlLiteral "$DatabaseName-Auto Full Backup $timestamp"

# ---------------------------------------------------------------- Thuc hien

try {
    Write-Log "===== Bat dau sao luu tu dong CSDL [$DatabaseName] tren $ServerInstance ====="
    if ($sqlcmdInfo) {
        Write-Log "Dung sqlcmd: $($sqlcmdInfo.Source) (co -C: $($script:SqlcmdSupportsC))"
    } else {
        Write-Log "Khong tim thay sqlcmd, dung System.Data.SqlClient"
    }

    # 1. Sao luu
    Write-Log "BACKUP DATABASE -> $backupFilePath"
    $backupSql = "BACKUP DATABASE $dbIdent TO DISK = N'$pathLit' WITH INIT, CHECKSUM, NAME = N'$nameLit', STATS = 10;"
    Invoke-Sql $backupSql | Out-Null
    if (-not (Test-Path -LiteralPath $backupFilePath)) {
        throw "Lenh BACKUP ket thuc nhung khong thay file $backupFilePath"
    }
    $sizeMb = [Math]::Round((Get-Item -LiteralPath $backupFilePath).Length / 1MB, 2)
    Write-Log "Sao luu xong ($sizeMb MB)"

    # 2. Kiem tra ban sao luu
    Write-Log "RESTORE VERIFYONLY ... WITH CHECKSUM"
    Invoke-Sql "RESTORE VERIFYONLY FROM DISK = N'$pathLit' WITH CHECKSUM;" | Out-Null
    Write-Log "Kiem tra ban sao luu: hop le"

    # 3. Ghi file ghi chu (UTF-8). Chu co dau dung ma [char] de file .ps1 giu ASCII thuan
    #    (PS 5.1 doc file .ps1 khong BOM theo ANSI nen khong viet truc tiep chu co dau).
    $u = { param([int[]]$codes) -join ($codes | ForEach-Object { [char]$_ }) }
    $luu   = & $u 0x6C,0x01B0,0x75                  # "luu" co dau
    $tu    = & $u 0x74,0x1EF1                       # "tu" co dau
    $dong  = & $u 0x0111,0x1ED9,0x6E,0x67           # "dong" co dau
    $luc   = & $u 0x6C,0x00FA,0x63                  # "luc" co dau
    $da    = & $u 0x0111,0x00E3                     # "da" co dau
    $kiem  = & $u 0x6B,0x69,0x1EC3,0x6D             # "kiem" co dau
    $note = "Sao $luu $tu $dong $luc $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss') ($da $kiem tra)"
    [System.IO.File]::WriteAllText($noteFilePath, $note, (New-Object System.Text.UTF8Encoding($false)))
    Write-Log "Da ghi file ghi chu: $noteFilePath"

    # 4. Don dep ban _Auto_ cu (chi chay khi cac buoc tren thanh cong)
    $limitDate = (Get-Date).AddDays(-$RetentionDays)
    $oldFiles = @(Get-ChildItem -LiteralPath $BackupDir -File -Filter "${DatabaseName}_Auto_*.bak" |
        Where-Object { $_.LastWriteTime -lt $limitDate -and $_.Name -like "${DatabaseName}_Auto_*.bak" })
    Write-Log "Don dep ban _Auto_ cu hon $RetentionDays ngay (truoc $($limitDate.ToString('yyyy-MM-dd HH:mm'))): $($oldFiles.Count) file"
    foreach ($f in $oldFiles) {
        Remove-Item -LiteralPath $f.FullName -Force
        $oldNote = [System.IO.Path]::ChangeExtension($f.FullName, ".txt")
        if (Test-Path -LiteralPath $oldNote) { Remove-Item -LiteralPath $oldNote -Force }
        Write-Log "Da xoa: $($f.Name)"
    }

    Write-Log "===== Hoan tat: $backupFileName ====="
    exit 0
}
catch {
    $msg = $_.Exception.Message
    Write-Log "Sao luu THAT BAI: $msg" "ERROR"

    if ($msg -match "Operating system error 5|Access is denied") {
        Write-Log ("Goi y: dich vu SQL Server khong co quyen ghi vao thu muc sao luu. Chay PowerShell (Administrator): " +
            "icacls `"$BackupDir`" /grant `"NT Service\MSSQL`$SQLEXPRESS:(OI)(CI)M`"") "ERROR"
    }

    # Khong de lai ban sao luu hong/chua kiem tra
    foreach ($p in @($backupFilePath, $noteFilePath)) {
        try {
            if (Test-Path -LiteralPath $p) {
                Remove-Item -LiteralPath $p -Force
                Write-Log "Da xoa file dang do: $p" "ERROR"
            }
        } catch { }
    }
    exit 1
}
