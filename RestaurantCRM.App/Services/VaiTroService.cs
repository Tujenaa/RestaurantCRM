using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services
{
    public class VaiTroService
    {
        private readonly HttpClient _http;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public VaiTroService()
        {
            _http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) };
        }

        public async Task<List<VaiTroModel>> GetAllAsync()
        {
            try
            {
                var json = await _http.GetStringAsync("/api/AdminVaiTro");
                var list = _serializer.Deserialize<List<Dictionary<string, object>>>(json);
                var result = new List<VaiTroModel>();

                if (list != null)
                {
                    foreach (var dict in list)
                    {
                        var vt = new VaiTroModel
                        {
                            MaVaiTro = dict.ContainsKey("maVaiTro") && dict["maVaiTro"] != null ? dict["maVaiTro"].ToString() : "",
                            TenVaiTro = dict.ContainsKey("tenVaiTro") && dict["tenVaiTro"] != null ? dict["tenVaiTro"].ToString() : "",
                            MoTa = dict.ContainsKey("moTa") && dict["moTa"] != null ? dict["moTa"].ToString() : "",
                            SoLuongNhanVien = dict.ContainsKey("soLuongNhanVien") && dict["soLuongNhanVien"] != null ? Convert.ToInt32(dict["soLuongNhanVien"]) : 0,
                            Permissions = new List<string>()
                        };

                        if (dict.ContainsKey("permissions") && dict["permissions"] is System.Collections.ArrayList permList)
                        {
                            foreach (var p in permList) if (p != null) vt.Permissions.Add(p.ToString());
                        }

                        result.Add(vt);
                    }
                }
                return result;
            }
            catch
            {
                return new List<VaiTroModel>();
            }
        }

        public async Task<List<PermissionItemModel>> GetAvailablePermissionsAsync()
        {
            try
            {
                var json = await _http.GetStringAsync("/api/AdminVaiTro/Permissions");
                var list = _serializer.Deserialize<List<Dictionary<string, object>>>(json);
                var result = new List<PermissionItemModel>();

                if (list != null)
                {
                    foreach (var dict in list)
                    {
                        result.Add(new PermissionItemModel
                        {
                            Code = dict.ContainsKey("code") && dict["code"] != null ? dict["code"].ToString() : "",
                            Name = dict.ContainsKey("name") && dict["name"] != null ? dict["name"].ToString() : "",
                            Group = dict.ContainsKey("group") && dict["group"] != null ? dict["group"].ToString() : "",
                            Description = dict.ContainsKey("description") && dict["description"] != null ? dict["description"].ToString() : ""
                        });
                    }
                }
                return result;
            }
            catch
            {
                return new List<PermissionItemModel>();
            }
        }

        public async Task<(bool Success, string Message)> SaveAsync(string maVaiTro, string tenVaiTro, List<string> permissions, bool isNew)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "MaVaiTro", maVaiTro },
                    { "TenVaiTro", tenVaiTro },
                    { "Permissions", permissions ?? new List<string>() }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                HttpResponseMessage resp;
                if (isNew)
                {
                    resp = await _http.PostAsync("/api/AdminVaiTro", content);
                }
                else
                {
                    resp = await _http.PutAsync($"/api/AdminVaiTro/{maVaiTro}", content);
                }

                var msg = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    return (true, "Lưu vai trò thành công");
                }
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi xử lý API" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string maVaiTro)
        {
            try
            {
                var resp = await _http.DeleteAsync($"/api/AdminVaiTro/{maVaiTro}");
                var msg = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    return (true, "Đã xóa vai trò");
                }
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi khi xóa" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
