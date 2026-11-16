using System;

namespace RestaurantCRM.API.DTOs
{
    public class BackupItemDto
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }
        public string BackupType { get; set; } = "Thủ công"; // "Tự động" (_Auto_) hoặc "Thủ công" (_Backup_)
    }

    public class CreateBackupRequest
    {
        public string? Note { get; set; }
    }

    public class RestoreBackupRequest
    {
        public string FileName { get; set; } = null!;
    }
}
