using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class MonAnService {
        private readonly HttpClient _httpClient;

        public MonAnService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        public async Task<List<MonAnDto>> GetAllAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<MonAnDto>>("api/AdminMonAn") ?? new List<MonAnDto>();
            } catch {
                return new List<MonAnDto>();
            }
        }

        public async Task<MonAnDto> GetByIdAsync(string id) {
            try {
                return await _httpClient.GetFromJsonAsync<MonAnDto>($"api/AdminMonAn/{id}");
            } catch {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateAsync(MonAnDto monAn) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/AdminMonAn", monAn);
                if (response.IsSuccessStatusCode) {
                    return (true, "Thêm món ăn thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Thêm món ăn thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(string id, MonAnDto monAn) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"api/AdminMonAn/{id}", monAn);
                if (response.IsSuccessStatusCode) {
                    return (true, "Cập nhật món ăn thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật món ăn thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string id) {
            try {
                var response = await _httpClient.DeleteAsync($"api/AdminMonAn/{id}");
                if (response.IsSuccessStatusCode) {
                    return (true, "Xóa món ăn thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Xóa món ăn thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
