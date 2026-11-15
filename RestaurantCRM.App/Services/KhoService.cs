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

    }
}
