using Microsoft.EntityFrameworkCore;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.API.Services
{
    /// <summary>
    /// Hồ sơ sở thích khách hàng: một bộ khảo sát đặc biệt (KHAO_SAT.laSoThich = 1), luôn Open, cho mọi khách.
    /// Câu hỏi + đáp án = danh mục sở thích; phiếu của khách = hồ sơ sở thích (ngayTraLoi = lần cập nhật gần nhất).
    /// </summary>
    public static class SoThichService
    {
        public const int SoMonYeuThichToiDa = 30;
        public const int DoDaiGhiChuToiDa = 500;

        public const string ThongBaoKhongXoaDuoc = "Hồ sơ sở thích là bộ đặc biệt, không thể xóa.";
        public const string ThongBaoKhongDoiTrangThai = "Hồ sơ sở thích luôn mở, không thể đổi trạng thái.";
        public const string ThongBaoKhongDatDoiTuong = "Hồ sơ sở thích áp dụng cho mọi khách hàng, không thể đặt đối tượng.";

        /// <summary>Bộ hồ sơ sở thích (kèm câu hỏi, đáp án và món của đáp án), hoặc null nếu chưa tạo.</summary>
        public static Task<KhaoSat?> LayBoSoThichAsync(IRestaurantService service) =>
            service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                        .ThenInclude(t => t.MaMonNavigation)
                .FirstOrDefaultAsync(k => k.LaSoThich);

        public static Task<string?> LayMaBoSoThichAsync(IRestaurantService service) =>
            service.Query<KhaoSat>().Where(k => k.LaSoThich).Select(k => (string?)k.MaKhaoSat).FirstOrDefaultAsync();

        public static Task<bool> LaBoSoThichAsync(IRestaurantService service, string maKhaoSat) =>
            service.Query<KhaoSat>().AnyAsync(k => k.MaKhaoSat == maKhaoSat && k.LaSoThich);

        /// <summary>Câu "Món yêu thích" (R4): trắc nghiệm, chọn nhiều, có đáp án gắn món.</summary>
        public static bool LaCauMonYeuThich(CauHoiKhaoSat c) =>
            AdminKhaoSatService.ChuanHoaLoai(c.LoaiCauHoi) == AdminKhaoSatService.TracNghiem
            && c.ChonNhieu
            && c.TuyChonCauHoi.Any(t => t.MaMon != null);

        public static CauHoiKhaoSat? TimCauMonYeuThich(KhaoSat boSoThich) =>
            AdminKhaoSatService.SapXep(boSoThich.CauHoiKhaoSat, c => c.MaCauHoi).FirstOrDefault(LaCauMonYeuThich);

        /// <summary>Câu dị ứng / lưu ý: nội dung chứa "dị ứng" (không phân biệt hoa thường và dấu).</summary>
        public static bool LaCauDiUng(string? noiDungCauHoi) =>
            !string.IsNullOrWhiteSpace(noiDungCauHoi) &&
            CodeGenerator.RemoveVietnameseAccents(noiDungCauHoi).ToLowerInvariant().Contains("di ung");

        /// <summary>Món không còn kinh doanh (vẫn giữ trong danh sách yêu thích, hiện nhãn "(ngừng bán)").</summary>
        public static bool NgungBan(string? trangThaiMon) =>
            trangThaiMon == "Discontinued" || trangThaiMon == "Inactive";

        /// <summary>Món còn kinh doanh (kể cả tạm hết hàng) mới được bật ♥.</summary>
        public static bool DangKinhDoanh(string? trangThaiMon) =>
            trangThaiMon == "InStock" || trangThaiMon == "OutOfStock";

        /// <summary>Mã đáp án tiếp theo dạng TC091, TC092... (cùng cách sinh mã với màn quản trị khảo sát).</summary>
        public static async Task<int> SoTuyChonTiepTheoAsync(IRestaurantService service)
        {
            const string prefix = "TC";
            var codes = await service.ListAsync<TuyChonCauHoi, string>(t => t.MaTuyChon.StartsWith(prefix), t => t.MaTuyChon);
            int max = 0;
            foreach (var code in codes)
            {
                if (code.Length > prefix.Length && code.Length <= prefix.Length + 6 &&
                    int.TryParse(code.Substring(prefix.Length), out int n) && n > max)
                    max = n;
            }
            return max + 1;
        }

        public static string MaPhieuMoi() => "PTL" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
        public static string MaCauTraLoiMoi() => "CTL" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();

        /// <summary>
        /// Xóa phiếu sở thích (và câu trả lời) của khách. Chỉ đánh dấu xóa, người gọi tự SaveChanges.
        /// Dùng khi xóa vĩnh viễn khách hàng (R12).
        /// </summary>
        public static async Task XoaPhieuSoThichAsync(IRestaurantService service, string maKhachHang)
        {
            var phieus = await service.Query<PhieuTraLoi>()
                .Include(p => p.CauTraLoi)
                .Where(p => p.MaKhachHang == maKhachHang && p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich)
                .ToListAsync();
            foreach (var p in phieus)
                service.RemoveRange(p.CauTraLoi);
            service.RemoveRange(phieus);
        }
    }
}
