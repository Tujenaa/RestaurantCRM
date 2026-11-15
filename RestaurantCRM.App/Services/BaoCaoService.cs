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
