using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.DTOs.KhaoSat;
using RestaurantCRM.AdminApp.Forms.KhaoSat;

namespace RestaurantCRM.AdminApp.Services
{
    /// <summary>Gọi API quản trị khảo sát (AdminKhaoSatController): khảo sát, câu hỏi, tùy chọn, mở/đóng, thống kê.</summary>
    public class KhaoSatService : ApiService
    {
        private const string Base = "api/AdminKhaoSat";

        private static string E(string s) { return Uri.EscapeDataString(s ?? ""); }

        // ── Khảo sát ─────────────────────────────────────────────────────────

        /// <param name="trangThai">"Draft" / "Open" / "Closed" hoặc null để lấy tất cả.</param>
        public async Task<List<KhaoSatDto>> GetAllAsync(string search = null, string trangThai = null)
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add("search=" + E(search.Trim()));
            if (!string.IsNullOrWhiteSpace(trangThai)) q.Add("trangThai=" + E(trangThai));
            string url = Base + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            return await GetAsync<List<KhaoSatDto>>(url) ?? new List<KhaoSatDto>();
        }

        public Task<KhaoSatChiTietDto> GetByIdAsync(string maKhaoSat)
        {
            return GetAsync<KhaoSatChiTietDto>(Base + "/" + E(maKhaoSat));
        }

        /// <summary>Tạo khảo sát (trạng thái Nháp). Mã mới đọc bằng result.Field("maKhaoSat").</summary>
        public Task<ApiResult> CreateAsync(string tieuDe, string maNhanVien)
        {
            return PostAsync(Base, new KhaoSatCreateRequest { TieuDe = tieuDe, MaNhanVien = maNhanVien }, "Tạo khảo sát thành công.");
        }

        public Task<ApiResult> UpdateAsync(string maKhaoSat, string tieuDe)
        {
            return PutAsync(Base + "/" + E(maKhaoSat), new KhaoSatUpdateRequest { TieuDe = tieuDe }, "Cập nhật khảo sát thành công.");
        }

        /// <param name="trangThai">KhaoSatConst.DangMo / DaDong / Nhap.</param>
        public Task<ApiResult> SetStatusAsync(string maKhaoSat, string trangThai)
        {
            return PutAsync(Base + "/" + E(maKhaoSat) + "/status", trangThai, "Đã cập nhật trạng thái khảo sát.");
        }

        public Task<ApiResult> DeleteAsync(string maKhaoSat)
        {
            return DeleteAsync(Base + "/" + E(maKhaoSat), "Đã xóa khảo sát.");
        }

        // ── Câu hỏi ──────────────────────────────────────────────────────────

        /// <summary>Thêm câu hỏi. Mã mới đọc bằng result.Field("maCauHoi").</summary>
        public Task<ApiResult> AddCauHoiAsync(string maKhaoSat, CauHoiUpsertRequest req)
        {
            return PostAsync(Base + "/" + E(maKhaoSat) + "/CauHoi", req, "Thêm câu hỏi thành công.");
        }

        /// <summary>Sửa câu hỏi; TuyChons là danh sách cuối cùng (API giữ tùy chọn trùng nội dung, thêm mới, xóa tùy chọn bị bỏ).</summary>
        public Task<ApiResult> UpdateCauHoiAsync(string maKhaoSat, string maCauHoi, CauHoiUpsertRequest req)
        {
            return PutAsync(Base + "/" + E(maKhaoSat) + "/CauHoi/" + E(maCauHoi), req, "Cập nhật câu hỏi thành công.");
        }

        public Task<ApiResult> DeleteCauHoiAsync(string maKhaoSat, string maCauHoi)
        {
            return DeleteAsync(Base + "/" + E(maKhaoSat) + "/CauHoi/" + E(maCauHoi), "Đã xóa câu hỏi.");
        }

        // ── Tùy chọn ─────────────────────────────────────────────────────────

        public Task<ApiResult> AddTuyChonAsync(string maKhaoSat, string maCauHoi, string noiDung)
        {
            return PostAsync(Base + "/" + E(maKhaoSat) + "/CauHoi/" + E(maCauHoi) + "/TuyChon",
                new TuyChonUpsertRequest { NoiDungTuyChon = noiDung }, "Thêm tùy chọn thành công.");
        }

        public Task<ApiResult> UpdateTuyChonAsync(string maKhaoSat, string maCauHoi, string maTuyChon, string noiDung)
        {
            return PutAsync(Base + "/" + E(maKhaoSat) + "/CauHoi/" + E(maCauHoi) + "/TuyChon/" + E(maTuyChon),
                new TuyChonUpsertRequest { NoiDungTuyChon = noiDung }, "Cập nhật tùy chọn thành công.");
        }

        public Task<ApiResult> DeleteTuyChonAsync(string maKhaoSat, string maCauHoi, string maTuyChon)
        {
            return DeleteAsync(Base + "/" + E(maKhaoSat) + "/CauHoi/" + E(maCauHoi) + "/TuyChon/" + E(maTuyChon), "Đã xóa tùy chọn.");
        }

        // ── Thống kê ─────────────────────────────────────────────────────────

        public Task<KhaoSatKetQuaDto> GetResultsAsync(string maKhaoSat)
        {
            return GetAsync<KhaoSatKetQuaDto>(Base + "/" + E(maKhaoSat) + "/results");
        }
    }
}