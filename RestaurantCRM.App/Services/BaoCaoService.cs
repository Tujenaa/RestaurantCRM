using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class BaoCaoService {
        private readonly HttpClient _httpClient;

        public BaoCaoService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        public async Task<BaoCaoDoanhThuDto> GetBaoCaoDoanhThuAsync(DateTime? tuNgay, DateTime? denNgay) {
            try {
                var url = "api/ThongKe/BaoCaoDoanhThu";
                if (tuNgay.HasValue && denNgay.HasValue) {
                    url += $"?tuNgay={tuNgay.Value:yyyy-MM-dd}&denNgay={denNgay.Value:yyyy-MM-dd}";
                }
                return await _httpClient.GetFromJsonAsync<BaoCaoDoanhThuDto>(url);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Lỗi gọi API Báo cáo doanh thu: " + ex.Message);
                return null;
            }
        }

        public async Task<BaoCaoDoanhThuTheoQuyDto> GetBaoCaoDoanhThuTheoQuyAsync(int nam) {
            try {
                var url = $"api/ThongKe/BaoCaoDoanhThuTheoQuy?nam={nam}";
                return await _httpClient.GetFromJsonAsync<BaoCaoDoanhThuTheoQuyDto>(url);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Lỗi gọi API Báo cáo doanh thu theo quý: " + ex.Message);
                return null;
            }
        }
    }
}
