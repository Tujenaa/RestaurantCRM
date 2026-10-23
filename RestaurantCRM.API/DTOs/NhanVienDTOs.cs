namespace RestaurantCRM.API.DTOs
{
    public class NhanVienDto
    {
        public string MaNhanVien { get; set; } = null!;
        public string? MaVaiTro { get; set; }
        public string? TenVaiTro { get; set; }
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public string? TrangThai { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class CreateNhanVienRequest
    {
        public string? MaNhanVien { get; set; }
        public string MaVaiTro { get; set; } = null!;
        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? TrangThai { get; set; } = "Active";
    }

    public class UpdateNhanVienRequest
    {
        public string MaVaiTro { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? TrangThai { get; set; }
    }

    public class NhanVienChangePasswordRequest
    {
        public string MatKhauMoi { get; set; } = null!;
    }
}
