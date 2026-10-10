using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class LoaiMonService {
        private readonly HttpClient _httpClient;
        public LoaiMonService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<List<LoaiMonDto>> GetAllAsync() {
            return await _httpClient.GetFromJsonAsync<List<LoaiMonDto>>("api/AdminLoaiMon") ?? new List<LoaiMonDto>();
        }

        public async Task<(bool Success, string Message)> CreateAsync(LoaiMonDto loaiMon) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/AdminLoaiMon", loaiMon);
                if (response.IsSuccessStatusCode) {
                    return (true, "Thêm loại món thành công.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Thêm thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(string id, LoaiMonDto loaiMon) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"api/AdminLoaiMon/{id}", loaiMon);
                if (response.IsSuccessStatusCode) {
                    return (true, "Cập nhật loại món thành công.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật thất bại từ máy chủ." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string id) {
            try {
                var response = await _httpClient.DeleteAsync($"api/AdminLoaiMon/{id}");
                if (response.IsSuccessStatusCode) {
                    return (true, "Xóa loại món thành công.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Không thể xóa loại món này." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
