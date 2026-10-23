using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.DTOs.PhanHoi;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Services
{
    /// <summary>Gọi API quản lý phản hồi khách hàng (AdminDanhGiaController).</summary>
    public class PhanHoiService : ApiService
    {
        private const string Base = "api/AdminDanhGia";

        public const string LocTatCa = "TatCa";
        public const string LocChuaTraLoi = "ChuaTraLoi";
        public const string LocDaTraLoi = "DaTraLoi";
        public const string LocDaAn = "DaAn";

        private static string E(string s) { return Uri.EscapeDataString(s ?? ""); }

        /// <param name="trangThai">TatCa | ChuaTraLoi | DaTraLoi | DaAn (mặc định TatCa).</param>
        public async Task<List<PhanHoiDto>> GetAllAsync(string search = null, string trangThai = LocTatCa, int? soSao = null)
        {
            var q = new List<string> { "trangThai=" + E(string.IsNullOrWhiteSpace(trangThai) ? LocTatCa : trangThai) };
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + E(search.Trim()));
            if (soSao.HasValue) q.Add("soSao=" + soSao.Value);
            return await GetAsync<List<PhanHoiDto>>(Base + "?" + string.Join("&", q)) ?? new List<PhanHoiDto>();
        }

        public Task<PhanHoiDto> GetByIdAsync(string maDanhGia)
        {
            return GetAsync<PhanHoiDto>(Base + "/" + E(maDanhGia));
        }

        /// <summary>Gửi câu trả lời với tư cách nhân viên đang đăng nhập.</summary>
        public Task<ApiResult> ReplyAsync(string maDanhGia, string noiDung)
        {
            return PostAsync(Base + "/" + E(maDanhGia) + "/Reply",
                new PhanHoiReplyRequest { NguoiGuiId = UserSession.CurrentUserId, NoiDung = noiDung }, "Đã gửi trả lời.");
        }

        public Task<ApiResult> DeleteReplyAsync(string maDanhGia, string maTraLoi)
        {
            return DeleteAsync(Base + "/" + E(maDanhGia) + "/Reply/" + E(maTraLoi), "Đã xóa câu trả lời.");
        }

        public Task<ApiResult> SetVisibleAsync(string maDanhGia, bool hienThi)
        {
            return PutAsync(Base + "/" + E(maDanhGia) + "/TrangThai", new PhanHoiTrangThaiRequest { HienThi = hienThi },
                hienThi ? "Đã hiển thị lại phản hồi." : "Đã ẩn phản hồi.");
        }

        public Task<ApiResult> DeleteAsync(string maDanhGia)
        {
            return DeleteAsync(Base + "/" + E(maDanhGia), "Đã xóa phản hồi.");
        }
    }
}