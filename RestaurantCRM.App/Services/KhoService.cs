using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class KhoService {
        private readonly HttpClient _httpClient;
        public KhoService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<List<TonKhoDto>> GetTonKhoAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<TonKhoDto>>("api/AdminKho") ?? new List<TonKhoDto>();
            } catch {
                return new List<TonKhoDto>();
            }
        }

        public async Task<List<TonKhoDto>> GetCanhBaoAsync(int nguong = 10) {
            try {
                return await _httpClient.GetFromJsonAsync<List<TonKhoDto>>($"api/AdminKho/canh-bao?nguong={nguong}") ?? new List<TonKhoDto>();
            } catch {
                return new List<TonKhoDto>();
            }
        }

        public async Task<(bool Success, string Message)> DieuChinhTonKhoAsync(string maMon, int soLuongMoi, string ghiChu = null) {
            try {
                var payload = new DieuChinhTonKhoDto {
                    SoLuongMoi = soLuongMoi,
                    GhiChu = ghiChu
                };
                var response = await _httpClient.PutAsJsonAsync($"api/AdminKho/dieu-chinh/{maMon}", payload);
                if (response.IsSuccessStatusCode) {
                    return (true, "Điều chỉnh tồn kho thành công.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật tồn kho thất bại." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }
    }
}
