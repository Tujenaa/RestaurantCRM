using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services
{
    public class BackupService
    {
        private readonly HttpClient _http;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public BackupService()
        {
            _http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) };
            _http.Timeout = TimeSpan.FromMinutes(5); // Backup/Restore có thể tốn thời gian
        }

        public async Task<List<BackupItemModel>> GetAllAsync()
        {
            try
            {
                var json = await _http.GetStringAsync("/api/AdminBackup");
                var list = _serializer.Deserialize<List<Dictionary<string, object>>>(json);
                var result = new List<BackupItemModel>();

                if (list != null)
                {
                    foreach (var dict in list)
                    {
                        var item = new BackupItemModel
                        {
                            FileName = dict.ContainsKey("fileName") && dict["fileName"] != null ? dict["fileName"].ToString() : "",
                            FilePath = dict.ContainsKey("filePath") && dict["filePath"] != null ? dict["filePath"].ToString() : "",
                            FileSizeBytes = dict.ContainsKey("fileSizeBytes") && dict["fileSizeBytes"] != null ? Convert.ToInt64(dict["fileSizeBytes"]) : 0,
                            FileSizeFormatted = dict.ContainsKey("fileSizeFormatted") && dict["fileSizeFormatted"] != null ? dict["fileSizeFormatted"].ToString() : "",
                            Note = dict.ContainsKey("note") && dict["note"] != null ? dict["note"].ToString() : ""
                        };

                        if (dict.ContainsKey("createdAt") && dict["createdAt"] != null)
                        {
                            DateTime dt;
                            if (DateTime.TryParse(dict["createdAt"].ToString(), out dt))
                            {
                                item.CreatedAt = dt;
                            }
                        }

                        result.Add(item);
                    }
                }
                return result;
            }
            catch
            {
                return new List<BackupItemModel>();
            }
        }

        public async Task<(bool Success, string Message)> CreateBackupAsync(string note)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "Note", note ?? "" }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/api/AdminBackup", content);
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    return (true, "Đã tạo bản sao lưu thành công!");
                }
                return (false, string.IsNullOrWhiteSpace(body) ? "Lỗi máy chủ khi tạo bản sao lưu" : body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> RestoreBackupAsync(string fileName)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "FileName", fileName }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/api/AdminBackup/restore", content);
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    return (true, "Phục hồi cơ sở dữ liệu thành công!");
                }
                return (false, string.IsNullOrWhiteSpace(body) ? "Lỗi khi phục hồi cơ sở dữ liệu" : body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DownloadBackupAsync(string fileName, string savePath)
        {
            try
            {
                var bytes = await _http.GetByteArrayAsync($"/api/AdminBackup/download/{fileName}");
                File.WriteAllBytes(savePath, bytes);
                return (true, "Tải file sao lưu thành công!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteBackupAsync(string fileName)
        {
            try
            {
                var resp = await _http.DeleteAsync($"/api/AdminBackup/{fileName}");
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    return (true, "Đã xóa bản sao lưu.");
                }
                return (false, string.IsNullOrWhiteSpace(body) ? "Lỗi khi xóa bản sao lưu" : body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
