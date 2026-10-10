using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;

namespace RestaurantCRM.AdminApp.Services {
    public class AuthService {
        private readonly HttpClient _httpClient;
        public string LastError { get; private set; }

        public AuthService() {
            _httpClient = new HttpClient {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
        }

        public async Task<LoginResult> LoginAsync(string username, string password) {
            LastError = null;
            try {
                var serializer = new JavaScriptSerializer();
                var payload = new Dictionary<string, string> {
                    { "TenDangNhap", username },
                    { "MatKhau", password }
                };
                var json = serializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/AdminAuth/Login", content);
                if (response.IsSuccessStatusCode) {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = serializer.Deserialize<Dictionary<string, object>>(responseString);
                    if (responseData == null) {
                        LastError = "API trả về dữ liệu đăng nhập không hợp lệ.";
                        return null;
                    }
                    var loginResult = new LoginResult {
                        Message = GetString(responseData, "message"),
                        Token = GetString(responseData, "token"),
                        Role = GetString(responseData, "role"),
                        RoleId = GetNullableInt(responseData, "roleId")
                    };
                    object userInfoValue;
                    if (responseData.TryGetValue("userInfo", out userInfoValue)) {
                        var userInfo = userInfoValue as Dictionary<string, object>;
                        if (userInfo != null) {
                            loginResult.UserInfo = new UserInfo {
                                Id = GetString(userInfo, "id"),
                                HoTen = GetString(userInfo, "hoTen"),
                                Username = GetString(userInfo, "username")
                            };
                        }
                    }
                    if (loginResult == null || loginResult.UserInfo == null) {
                        LastError = "API trả về dữ liệu đăng nhập không hợp lệ.";
                    }
                    return loginResult;
                }
                LastError = await response.Content.ReadAsStringAsync();
                if (String.IsNullOrWhiteSpace(LastError)) LastError = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return null;
            } catch (HttpRequestException) {
                LastError = "Không kết nối được API. Hãy chạy RestaurantCRM.API và kiểm tra địa chỉ " + ApiConfig.BaseUrl + ".";
                return null;
            } catch (Exception ex) {
                LastError = "Không thể xử lý phản hồi từ API: " + ex.Message;
                return null;
            }
        }

        private static string GetString(Dictionary<string, object> values, string key) {
            object value;
            return values != null && values.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value)
                : null;
        }

        private static int? GetNullableInt(Dictionary<string, object> values, string key) {
            int parsed;
            return Int32.TryParse(GetString(values, key), out parsed) ? (int?)parsed : null;
        }
    }

    public class LoginResult {
        public string Message { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public int? RoleId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class UserInfo {
        public string Id { get; set; }
        public string HoTen { get; set; }
        public string Username { get; set; }
    }
}

