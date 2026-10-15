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
    }
}
