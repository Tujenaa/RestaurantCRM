using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class HoaDonService {
        private readonly HttpClient _httpClient;

        public HoaDonService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        public async Task<List<HoaDonDto>> GetAllAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<HoaDonDto>>("api/AdminHoaDon") ?? new List<HoaDonDto>();
            } catch {
                return new List<HoaDonDto>();
            }
        }

        public async Task<HoaDonDetailDto> GetByIdAsync(string id) {
            try {
                return await _httpClient.GetFromJsonAsync<HoaDonDetailDto>($"api/AdminHoaDon/{id}");
            } catch {
                return null;
            }
        }

        public async Task<List<LichSuTrangThaiDto>> GetLichSuTrangThaiAsync(string id) {
            try {
                return await _httpClient.GetFromJsonAsync<List<LichSuTrangThaiDto>>($"api/AdminHoaDon/{id}/LichSuTrangThai") ?? new List<LichSuTrangThaiDto>();
            } catch {
                return new List<LichSuTrangThaiDto>();
            }
        }

        public async Task<(bool Success, string Message)> UpdateTrangThaiAsync(string id, string trangThaiMoi, string ghiChu = null, string maNhanVien = null) {
            try {
                var payload = new UpdateOrderStatusRequestDto {
                    TrangThaiMoi = trangThaiMoi,
                    GhiChu = ghiChu,
                    MaNhanVien = maNhanVien
                };

                var response = await _httpClient.PutAsJsonAsync($"api/AdminHoaDon/{id}/TrangThai", payload);
                if (response.IsSuccessStatusCode) {
                    return (true, "Cập nhật trạng thái đơn hàng thành công!");
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Cập nhật trạng thái thất bại." : err.Trim('"', '\r', '\n'));
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message, string MaHoaDon)> CreateOrderAsync(CreateOrderRequestDto request) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/HoaDon", request);
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadFromJsonAsync<CreateOrderResponseDto>();
                    return (true, "Tạo đơn hàng thành công!", result?.MaHoaDon);
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Tạo đơn hàng thất bại." : err.Trim('"', '\r', '\n'), null);
            } catch (Exception ex) {
                return (false, "Lỗi kết nối API: " + ex.Message, null);
            }
        }
    }
}
