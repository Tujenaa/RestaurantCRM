using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RestaurantCRM.API.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminBackupController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _backupFolder;

        public AdminBackupController(IConfiguration config)
        {
            _config = config;
            // Cấu hình đường dẫn chính xác vào thư mục Database\Backups của Solution
            string configuredPath = _config["BackupSettings:Folder"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                _backupFolder = configuredPath;
            }
            else
            {
                // Tìm thư mục gốc chứa RestaurantCRM.sln
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (dir != null && !System.IO.File.Exists(Path.Combine(dir.FullName, "RestaurantCRM.sln")))
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                {
                    _backupFolder = Path.Combine(dir.FullName, "Database", "Backups");
                }
                else
                {
                    _backupFolder = @"D:\HTTT Doanh Nghiep\RestaurantCRM\Database\Backups";
                }
            }

            if (!Directory.Exists(_backupFolder))
            {
                Directory.CreateDirectory(_backupFolder);
            }
        }

        private string GetConnectionString()
        {
            return _config.GetConnectionString("DefaultConnection")
                ?? "Server=.\\SQLEXPRESS;Database=RestaurantCRM;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        private string GetDatabaseName()
        {
            var builder = new SqlConnectionStringBuilder(GetConnectionString());
            return string.IsNullOrEmpty(builder.InitialCatalog) ? "RestaurantCRM" : builder.InitialCatalog;
        }

        // GET: api/AdminBackup - Lấy danh sách các bản sao lưu hiện có
        [HttpGet]
        public IActionResult GetBackups()
        {
            try
            {
                var files = Directory.GetFiles(_backupFolder, "*.bak")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => new BackupItemDto
                    {
                        FileName = f.Name,
                        FilePath = f.FullName,
                        FileSizeBytes = f.Length,
                        FileSizeFormatted = FormatBytes(f.Length),
                        CreatedAt = f.CreationTime,
                        Note = GetNoteForFile(f.FullName)
                    })
                    .ToList();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi đọc danh sách bản sao lưu: " + ex.Message);
            }
        }

        // POST: api/AdminBackup - Tạo bản sao lưu CSDL ngay lập tức
        [HttpPost]
        public async Task<IActionResult> CreateBackup([FromBody] CreateBackupRequest? request)
        {
            string dbName = GetDatabaseName();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{dbName}_Backup_{timestamp}.bak";
            string filePath = Path.Combine(_backupFolder, fileName);
            string backupName = $"{dbName}-Full Database Backup {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            try
            {
                using var conn = new SqlConnection(GetConnectionString());
                await conn.OpenAsync();

                // Lệnh T-SQL sao lưu CSDL
                string sql = $@"BACKUP DATABASE [{dbName}] 
                                TO DISK = @FilePath 
                                WITH FORMAT, INIT, NAME = @BackupName, SKIP, NOREWIND, NOUNLOAD, STATS = 10;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.Parameters.AddWithValue("@BackupName", backupName);
                cmd.CommandTimeout = 300; // 5 phút

                await cmd.ExecuteNonQueryAsync();

                // Lưu ghi chú nếu có
                if (!string.IsNullOrWhiteSpace(request?.Note))
                {
                    var notePath = Path.ChangeExtension(filePath, ".txt");
                    await System.IO.File.WriteAllTextAsync(notePath, request.Note.Trim());
                }

                var info = new FileInfo(filePath);
                return Ok(new
                {
                    message = "Tạo bản sao lưu cơ sở dữ liệu thành công!",
                    fileName = fileName,
                    fileSize = FormatBytes(info.Length),
                    createdAt = info.CreationTime
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi trong quá trình tạo bản sao lưu: " + ex.Message);
            }
        }

        // POST: api/AdminBackup/restore - Phục hồi cơ sở dữ liệu từ file .bak
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.FileName))
            {
                return BadRequest("Tên file sao lưu không được để trống.");
            }

            string filePath = Path.Combine(_backupFolder, request.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Không tìm thấy file sao lưu trên máy chủ.");
            }

            string dbName = GetDatabaseName();

            // Kết nối đến database 'master' để thực thi RESTORE
            var builder = new SqlConnectionStringBuilder(GetConnectionString())
            {
                InitialCatalog = "master"
            };

            try
            {
                using var conn = new SqlConnection(builder.ConnectionString);
                await conn.OpenAsync();

                // Chuyển DB sang SINGLE_USER để ngắt toàn bộ connection đang mở, sau đó RESTORE và trả về MULTI_USER
                string sql = $@"
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{dbName}] FROM DISK = @FilePath WITH REPLACE;
                    ALTER DATABASE [{dbName}] SET MULTI_USER;
                ";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.CommandTimeout = 300;

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { message = $"Đã phục hồi cơ sở dữ liệu [{dbName}] thành công từ bản sao lưu '{request.FileName}'!" });
            }
            catch (Exception ex)
            {
                // Cố gắng mở lại MULTI_USER nếu lỗi
                try
                {
                    using var connRecovery = new SqlConnection(builder.ConnectionString);
                    await connRecovery.OpenAsync();
                    using var cmdRecovery = new SqlCommand($"ALTER DATABASE [{dbName}] SET MULTI_USER;", connRecovery);
                    await cmdRecovery.ExecuteNonQueryAsync();
                }
                catch { }

                return StatusCode(500, "Lỗi khi phục hồi cơ sở dữ liệu: " + ex.Message);
            }
        }

        // GET: api/AdminBackup/download/{fileName} - Tải file backup về máy khách
        [HttpGet("download/{fileName}")]
        public IActionResult DownloadBackup(string fileName)
        {
            string filePath = Path.Combine(_backupFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Không tìm thấy file sao lưu.");
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/octet-stream", fileName);
        }

        // DELETE: api/AdminBackup/{fileName} - Xóa bản sao lưu cũ
        [HttpDelete("{fileName}")]
        public IActionResult DeleteBackup(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_backupFolder, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                string notePath = Path.ChangeExtension(filePath, ".txt");
                if (System.IO.File.Exists(notePath))
                {
                    System.IO.File.Delete(notePath);
                }

                return Ok(new { message = "Đã xóa bản sao lưu thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest("Không thể xóa file sao lưu: " + ex.Message);
            }
        }

        private static string GetNoteForFile(string filePath)
        {
            try
            {
                var notePath = Path.ChangeExtension(filePath, ".txt");
                if (System.IO.File.Exists(notePath))
                {
                    return System.IO.File.ReadAllText(notePath);
                }
            }
            catch { }
            return string.Empty;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }
}
