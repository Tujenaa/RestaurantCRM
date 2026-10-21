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
    public class TaiKhoanService
    {
        private readonly HttpClient _http;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public TaiKhoanService()
        {
            _http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl) };
        }

        public async Task<List<NhanVienModel>> GetAllAsync(string search = null, string maVaiTro = null, string trangThai = null)
        {
            try
            {
                var url = "/api/AdminNhanVien?";
                if (!string.IsNullOrEmpty(search)) url += "search=" + Uri.EscapeDataString(search) + "&";
                if (!string.IsNullOrEmpty(maVaiTro)) url += "maVaiTro=" + Uri.EscapeDataString(maVaiTro) + "&";
                if (!string.IsNullOrEmpty(trangThai)) url += "trangThai=" + Uri.EscapeDataString(trangThai) + "&";

                var json = await _http.GetStringAsync(url);
                var list = _serializer.Deserialize<List<Dictionary<string, object>>>(json);
                var result = new List<NhanVienModel>();

                if (list != null)
                {
                    foreach (var dict in list)
                    {
                        var nv = new NhanVienModel
                        {
                            MaNhanVien = dict.ContainsKey("maNhanVien") && dict["maNhanVien"] != null ? dict["maNhanVien"].ToString() : "",
                            MaVaiTro = dict.ContainsKey("maVaiTro") && dict["maVaiTro"] != null ? dict["maVaiTro"].ToString() : "",
                            TenVaiTro = dict.ContainsKey("tenVaiTro") && dict["tenVaiTro"] != null ? dict["tenVaiTro"].ToString() : "",
                            TenDangNhap = dict.ContainsKey("tenDangNhap") && dict["tenDangNhap"] != null ? dict["tenDangNhap"].ToString() : "",
                            HoTen = dict.ContainsKey("hoTen") && dict["hoTen"] != null ? dict["hoTen"].ToString() : "",
                            TrangThai = dict.ContainsKey("trangThai") && dict["trangThai"] != null ? dict["trangThai"].ToString() : "Active",
                            Permissions = new List<string>()
                        };

                        if (dict.ContainsKey("permissions") && dict["permissions"] is System.Collections.ArrayList permList)
                        {
                            foreach (var p in permList) if (p != null) nv.Permissions.Add(p.ToString());
                        }

                        result.Add(nv);
                    }
                }
                return result;
            }
            catch
            {
                return new List<NhanVienModel>();
            }
        }

        public async Task<(bool Success, string Message)> CreateAsync(string maNV, string hoTen, string username, string password, string maVaiTro, string trangThai)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "MaNhanVien", maNV },
                    { "HoTen", hoTen },
                    { "TenDangNhap", username },
                    { "MatKhau", password },
                    { "MaVaiTro", maVaiTro },
                    { "TrangThai", trangThai }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/api/AdminNhanVien", content);
                var msg = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode) return (true, "Tạo tài khoản nhân viên thành công.");
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi xử lý API" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(string maNV, string hoTen, string maVaiTro, string trangThai)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "HoTen", hoTen },
                    { "MaVaiTro", maVaiTro },
                    { "TrangThai", trangThai }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await _http.PutAsync($"/api/AdminNhanVien/{maNV}", content);
                var msg = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode) return (true, "Cập nhật tài khoản thành công.");
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi xử lý API" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string maNV, string matKhauMoi)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "MatKhauMoi", matKhauMoi }
                };

                var content = new StringContent(_serializer.Serialize(payload), Encoding.UTF8, "application/json");
                var resp = await _http.PutAsync($"/api/AdminNhanVien/{maNV}/doi-mat-khau", content);
                var msg = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode) return (true, "Đổi mật khẩu thành công.");
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi xử lý API" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string maNV)
        {
            try
            {
                var resp = await _http.DeleteAsync($"/api/AdminNhanVien/{maNV}");
                var msg = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode) return (true, "Xóa tài khoản thành công.");
                return (false, string.IsNullOrWhiteSpace(msg) ? "Lỗi khi xóa" : msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
