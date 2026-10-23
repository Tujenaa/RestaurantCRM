using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.DTOs.KhachHang;

namespace RestaurantCRM.AdminApp.Services
{
    /// <summary>Gọi API quản lý khách hàng (AdminKhachHangController).</summary>
    public class KhachHangService : ApiService
    {
        private const string Base = "api/AdminKhachHang";

        /// <param name="search">Tìm theo mã, tên, số điện thoại, email.</param>
        /// <param name="trangThai">"Active" / "Locked" hoặc null để lấy tất cả.</param>
        public Task<List<KhachHangDto>> GetAllAsync(string search = null, string trangThai = null)
        {
            string url = Base;
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (!string.IsNullOrWhiteSpace(trangThai)) q.Add("trangThai=" + Uri.EscapeDataString(trangThai));
            if (q.Count > 0) url += "?" + string.Join("&", q);
            return GetListAsync(url);
        }

        private async Task<List<KhachHangDto>> GetListAsync(string url)
        {
            return await GetAsync<List<KhachHangDto>>(url) ?? new List<KhachHangDto>();
        }

        public Task<KhachHangChiTietDto> GetByIdAsync(string maKhachHang)
        {
            return GetAsync<KhachHangChiTietDto>(Base + "/" + Uri.EscapeDataString(maKhachHang));
        }

        public Task<ApiResult> CreateAsync(KhachHangRequest req)
        {
            return PostAsync(Base, req, "Thêm khách hàng thành công.");
        }

        public Task<ApiResult> UpdateAsync(string maKhachHang, KhachHangRequest req)
        {
            return PutAsync(Base + "/" + Uri.EscapeDataString(maKhachHang), req, "Cập nhật khách hàng thành công.");
        }

        /// <param name="khoa">true = khóa tài khoản, false = mở khóa.</param>
        public Task<ApiResult> SetLockedAsync(string maKhachHang, bool khoa)
        {
            return PutAsync(Base + "/" + Uri.EscapeDataString(maKhachHang) + "/Status",
                new { TrangThai = khoa ? "Locked" : "Active" },
                khoa ? "Đã khóa tài khoản khách hàng." : "Đã mở khóa tài khoản khách hàng.");
        }

        public Task<ApiResult> DeleteAsync(string maKhachHang)
        {
            return DeleteAsync(Base + "/" + Uri.EscapeDataString(maKhachHang), "Đã xóa khách hàng.");
        }
    }
}