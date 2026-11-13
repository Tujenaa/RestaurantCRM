using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.API.Services
{
    /// <summary>
    /// Quy tắc kiểm tra dữ liệu hồ sơ khách hàng, dùng chung cho phía quản trị (AdminKhachHangController)
    /// và phía khách (KhachHangController: đăng ký, cập nhật hồ sơ, đổi mật khẩu).
    /// </summary>
    public static class KhachHangValidator
    {
        public const string TaiKhoanBiKhoa = "Tài khoản đã bị khóa.";

        public static readonly string[] TrangThaiHopLe = { "Active", "Locked" };
        public static readonly string[] GioiTinhHopLe = { "Nam", "Nữ", "Khác" };
        public static readonly Regex SoDienThoaiRegex = new Regex(@"^0\d{9,10}$");
        public static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public static string? ChuanHoa(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        /// <summary>Trả về thông báo lỗi đầu tiên, hoặc null nếu dữ liệu hợp lệ.</summary>
        public static string? KiemTraDuLieu(string? hoTen, string? soDienThoai, string? email, DateOnly? ngaySinh,
            string? gioiTinh, string? trangThai, bool batBuocHoTen)
        {
            if (batBuocHoTen && string.IsNullOrWhiteSpace(hoTen))
                return "Họ tên không được để trống.";
            if (!string.IsNullOrWhiteSpace(hoTen) && hoTen.Trim().Length > 100)
                return "Họ tên tối đa 100 ký tự.";

            var sdt = ChuanHoa(soDienThoai);
            if (sdt != null && !SoDienThoaiRegex.IsMatch(sdt))
                return "Số điện thoại không hợp lệ (bắt đầu bằng 0, gồm 10–11 chữ số).";

            var mail = ChuanHoa(email);
            if (mail != null && (!EmailRegex.IsMatch(mail) || mail.Length > 100))
                return "Email không hợp lệ.";

            if (ngaySinh.HasValue)
            {
                var homNay = DateOnly.FromDateTime(DateTime.Today);
                if (ngaySinh.Value > homNay) return "Ngày sinh không được lớn hơn ngày hiện tại.";
                if (ngaySinh.Value < homNay.AddYears(-120)) return "Ngày sinh không hợp lệ.";
            }

            var gt = ChuanHoa(gioiTinh);
            if (gt != null && !GioiTinhHopLe.Contains(gt))
                return "Giới tính chỉ nhận 'Nam', 'Nữ' hoặc 'Khác'.";

            var tt = ChuanHoa(trangThai);
            if (tt != null && !TrangThaiHopLe.Contains(tt))
                return "Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Locked'.";

            return null;
        }

        /// <summary>Phải có ít nhất một trong hai: email hoặc số điện thoại.</summary>
        public static string? KiemTraLienHe(string? soDienThoai, string? email)
        {
            return ChuanHoa(soDienThoai) == null && ChuanHoa(email) == null
                ? "Vui lòng nhập ít nhất email hoặc số điện thoại."
                : null;
        }

        public static string? KiemTraMatKhau(string? matKhau)
        {
            return string.IsNullOrWhiteSpace(matKhau) || matKhau.Trim().Length < 6
                ? "Mật khẩu phải có tối thiểu 6 ký tự."
                : null;
        }

        /// <summary>
        /// Khách phải tồn tại và đang hoạt động thì mới được tạo / sửa dữ liệu.
        /// Trả về thông báo lỗi, hoặc null nếu hợp lệ.
        /// </summary>
        public static async Task<string?> KiemTraHoatDongAsync(IRestaurantService service, string? maKhachHang)
        {
            if (string.IsNullOrWhiteSpace(maKhachHang)) return "Thiếu mã khách hàng.";
            var kh = await service.Query<KhachHang>()
                .Where(k => k.MaKhachHang == maKhachHang)
                .Select(k => new { k.TrangThai })
                .FirstOrDefaultAsync();
            if (kh == null) return "Không tìm thấy khách hàng.";
            return kh.TrangThai == "Active" ? null : TaiKhoanBiKhoa;
        }
    }
}
