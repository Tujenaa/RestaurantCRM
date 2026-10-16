using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class NguyenLieuService {
        private readonly HttpClient _httpClient;

        public NguyenLieuService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        public async Task<List<NguyenLieuDto>> GetAllAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<NguyenLieuDto>>("api/AdminNguyenLieu") ?? new List<NguyenLieuDto>();
            } catch {
                return new List<NguyenLieuDto>();
            }
        }

        public async Task<NguyenLieuDto> GetByIdAsync(string id) {
            try {
                return await _httpClient.GetFromJsonAsync<NguyenLieuDto>($"api/AdminNguyenLieu/{id}");
            } catch {
                return null;
            }
        }

        public async Task<(bool Success, string Message, NguyenLieuDto CreatedItem)> CreateAsync(NguyenLieuDto dto) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/AdminNguyenLieu", dto);
                if (response.IsSuccessStatusCode) {
                    var created = await response.Content.ReadFromJsonAsync<NguyenLieuDto>();
                    return (true, "Thêm nguyên vật liệu thành công!", created);
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Thêm nguyên liệu thất bại từ máy chủ." : err.Trim('"', '\r', '\n'), null);
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(string id, NguyenLieuDto dto) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"api/AdminNguyenLieu/{id}", dto);
                if (response.IsSuccessStatusCode) {
                    return (true, "Cập nhật nguyên vật liệu thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật nguyên liệu thất bại." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string id) {
            try {
                var response = await _httpClient.DeleteAsync($"api/AdminNguyenLieu/{id}");
                if (response.IsSuccessStatusCode) {
                    return (true, "Xóa nguyên vật liệu thành công!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Xóa nguyên liệu thất bại." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
