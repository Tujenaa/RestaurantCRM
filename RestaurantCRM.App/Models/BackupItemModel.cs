using System;

namespace RestaurantCRM.AdminApp.Models
{
    public class BackupItemModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Note { get; set; }
    }
}
