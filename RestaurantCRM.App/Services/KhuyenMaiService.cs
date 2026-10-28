using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services {
    public class KhuyenMaiService {
        private readonly HttpClient _httpClient;

        public KhuyenMaiService(HttpClient httpClient = null) {
            _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") };
        }

        // ================= CHƯƠNG TRÌNH KHUYẾN MÃI =================
        public async Task<List<ChuongTrinhDto>> GetChuongTrinhListAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<ChuongTrinhDto>>("api/AdminKhuyenMai/ChuongTrinh") ?? new List<ChuongTrinhDto>();
            } catch {
                return new List<ChuongTrinhDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateChuongTrinhAsync(string ten, DateTime? start, DateTime? end) {
            try {
                var body = new {
                    tenChuongTrinh = ten,
                    ngayBatDau = start?.ToString("yyyy-MM-dd"),
                    ngayKetThuc = end?.ToString("yyyy-MM-dd")
                };
                var res = await _httpClient.PostAsJsonAsync("api/AdminKhuyenMai/ChuongTrinh", body);
                if (res.IsSuccessStatusCode) return (true, "Tạo chương trình thành công.");
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Lỗi khi tạo chương trình." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateChuongTrinhAsync(string id, string ten, DateTime? start, DateTime? end) {
            try {
                var body = new {
                    tenChuongTrinh = ten,
                    ngayBatDau = start?.ToString("yyyy-MM-dd"),
                    ngayKetThuc = end?.ToString("yyyy-MM-dd")
                };
                var res = await _httpClient.PutAsJsonAsync($"api/AdminKhuyenMai/ChuongTrinh/{id}", body);
                if (res.IsSuccessStatusCode) return (true, "Cập nhật chương trình thành công.");
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Lỗi khi cập nhật chương trình." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteChuongTrinhAsync(string id) {
            try {
                var res = await _httpClient.DeleteAsync($"api/AdminKhuyenMai/ChuongTrinh/{id}");
                if (res.IsSuccessStatusCode) {
                    try {
                        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                        if (json.TryGetProperty("message", out var m)) return (true, m.GetString());
                    } catch { }
                    return (true, "Đã xóa chương trình.");
                }
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Không thể xóa chương trình." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        // ================= VOUCHER HÓA ĐƠN =================
        public async Task<List<VoucherDto>> GetVoucherListAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<VoucherDto>>("api/AdminKhuyenMai/Voucher") ?? new List<VoucherDto>();
            } catch {
                return new List<VoucherDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateVoucherAsync(
            string maCT, 
            string maVoucher, 
            string loaiGiam, 
            double giaTriGiam, 
            double? donToiThieu, 
            double? giamToiDa, 
            string phamViApDung = "TOAN_DON", 
            string maLoaiMon = null, 
            string maMon = null,
            int? soLuongToiThieu = 1,
            bool choPhepCongDon = true) {
            try {
                var body = new {
                    maChuongTrinh = maCT,
                    maVoucher = maVoucher,
                    loaiGiam = loaiGiam,
                    giaTriGiam = giaTriGiam,
                    giaTriDonToiThieu = donToiThieu,
                    giamToiDa = giamToiDa,
                    phamViApDung = phamViApDung,
                    maLoaiMon = maLoaiMon,
                    maMon = maMon,
                    soLuongToiThieu = soLuongToiThieu,
                    choPhepCongDon = choPhepCongDon
                };
                var res = await _httpClient.PostAsJsonAsync("api/AdminKhuyenMai/Voucher", body);
                if (res.IsSuccessStatusCode) return (true, "Tạo voucher thành công.");
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Lỗi khi tạo voucher." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateVoucherAsync(
            string id,
            string maCT,
            string maVoucher,
            string loaiGiam,
            double giaTriGiam,
            double? donToiThieu,
            double? giamToiDa,
            int? soLuongVoucher = 20) {
            try {
                var body = new {
                    maChuongTrinh = maCT,
                    maVoucher = maVoucher,
                    loaiGiam = loaiGiam,
                    giaTriGiam = giaTriGiam,
                    giaTriDonToiThieu = donToiThieu,
                    giamToiDa = giamToiDa,
                    soLuongToiThieu = soLuongVoucher
                };
                var res = await _httpClient.PutAsJsonAsync($"api/AdminKhuyenMai/Voucher/{id}", body);
                if (res.IsSuccessStatusCode) return (true, "Cập nhật voucher thành công.");
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Lỗi khi cập nhật voucher." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        public async Task<List<LoaiMonSelectDto>> GetLoaiMonListAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<LoaiMonSelectDto>>("api/AdminKhuyenMai/LoaiMon") ?? new List<LoaiMonSelectDto>();
            } catch {
                return new List<LoaiMonSelectDto>();
            }
        }

        public async Task<(bool Success, string Message)> DeleteVoucherAsync(string id) {
            try {
                var res = await _httpClient.DeleteAsync($"api/AdminKhuyenMai/Voucher/{id}");
                if (res.IsSuccessStatusCode) {
                    try {
                        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                        if (json.TryGetProperty("message", out var m)) return (true, m.GetString());
                    } catch { }
                    return (true, "Đã xóa voucher.");
                }
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Không thể xóa voucher." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        // ================= KHUYẾN MÃI MÓN ĂN (TỰ ĐỘNG) =================
        public async Task<List<KmMonDto>> GetKmMonListAsync() {
            try {
                return await _httpClient.GetFromJsonAsync<List<KmMonDto>>("api/AdminKhuyenMai/MonAn") ?? new List<KmMonDto>();
            } catch {
                return new List<KmMonDto>();
            }
        }

        public async Task<(bool Success, string Message)> CreateKmMonAsync(string maCT, string maMon, string loaiGiam, double giaTriGiam) {
            try {
                var body = new {
                    maChuongTrinh = maCT,
                    maMon = maMon,
                    loaiGiam = loaiGiam,
                    giaTriGiam = giaTriGiam
                };
                var res = await _httpClient.PostAsJsonAsync("api/AdminKhuyenMai/MonAn", body);
                if (res.IsSuccessStatusCode) return (true, "Áp dụng khuyến mãi món thành công.");
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Lỗi khi áp dụng khuyến mãi cho món." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteKmMonAsync(string id) {
            try {
                var res = await _httpClient.DeleteAsync($"api/AdminKhuyenMai/MonAn/{id}");
                if (res.IsSuccessStatusCode) {
                    try {
                        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                        if (json.TryGetProperty("message", out var m)) return (true, m.GetString());
                    } catch { }
                    return (true, "Đã xóa khuyến mãi món.");
                }
                var err = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(err) ? "Không thể xóa khuyến mãi món." : err);
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        // ================= MÓN ĂN KHẢ DỤNG ĐỂ CHỌN =================
        public async Task<List<MonAnSelectDto>> GetAvailableDishesAsync() {
            try {
                var list = await _httpClient.GetFromJsonAsync<List<MonAnSelectDto>>("api/AdminKhuyenMai/AvailableDishes");
                if (list != null && list.Count > 0) return list;
            } catch { }

            try {
                var adminDishes = await _httpClient.GetFromJsonAsync<List<MonAnDto>>("api/AdminMonAn");
                if (adminDishes != null && adminDishes.Count > 0) {
                    var result = new List<MonAnSelectDto>();
                    foreach (var d in adminDishes) {
                        if (d.TrangThaiRaw == "Discontinued" || d.TrangThaiRaw == "Inactive") continue;
                        result.Add(new MonAnSelectDto {
                            MaMon = d.MaMonAn,
                            TenMon = d.TenMonAn,
                            DonGia = (double)d.Gia,
                            MaLoaiMon = d.MaLoaiMon,
                            GiaVonUocTinh = (double)d.GiaVonUocTinh
                        });
                    }
                    if (result.Count > 0) return result;
                }
            } catch { }

            return new List<MonAnSelectDto>();
        }
    }
}
