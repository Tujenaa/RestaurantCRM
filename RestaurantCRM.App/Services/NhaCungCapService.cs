using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class NhaCungCapService {
        private readonly HttpClient _httpClient;

        public NhaCungCapService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        public async Task<List<NhaCungCapDto>> GetAllAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<NhaCungCapDto>>("api/AdminNhaCungCap") ?? new List<NhaCungCapDto>();
            } catch {
                return new List<NhaCungCapDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateAsync(NhaCungCapDto dto) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/AdminNhaCungCap", dto);
                if (response.IsSuccessStatusCode) {
                    return (true, "Thêm nhà cung cấp thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Thêm nhà cung cấp thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(string id, NhaCungCapDto dto) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"api/AdminNhaCungCap/{id}", dto);
                if (response.IsSuccessStatusCode) {
                    return (true, "Cập nhật nhà cung cấp thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string id) {
            try {
                var response = await _httpClient.DeleteAsync($"api/AdminNhaCungCap/{id}");
                if (response.IsSuccessStatusCode) {
                    return (true, "Xóa nhà cung cấp thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Xóa nhà cung cấp thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
