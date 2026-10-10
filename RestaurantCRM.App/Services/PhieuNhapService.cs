using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class PhieuNhapService {
        private readonly HttpClient _httpClient;
        public PhieuNhapService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<List<PhieuNhapDto>> GetAllAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<PhieuNhapDto>>("api/AdminPhieuNhap") ?? new List<PhieuNhapDto>();
            } catch {
                return new List<PhieuNhapDto>();
            }
        }

        public async Task<PhieuNhapDto> GetByIdAsync(string id) {
            try {
                return await _httpClient.GetFromJsonAsync<PhieuNhapDto>($"api/AdminPhieuNhap/{id}");
            } catch {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateAsync(PhieuNhapCreateDto dto) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/AdminPhieuNhap", dto);
                if (response.IsSuccessStatusCode) {
                    return (true, "Lập phiếu nhập thành công và đã tự động cộng tồn kho!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Lập phiếu nhập thất bại." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
