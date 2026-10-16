using System;

namespace RestaurantCRM.AdminApp.Config {
    public static class ApiConfig {
        public static readonly string BaseUrl =
            Environment.GetEnvironmentVariable("RESTAURANTCRM_API_URL") ?? "http://localhost:5275";
    }
}
