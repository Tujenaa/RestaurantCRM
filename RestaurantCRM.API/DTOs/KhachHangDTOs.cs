namespace RestaurantCRM.API.DTOs
{
    public class UpdateProfileRequest
    {
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SoThich { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string MatKhauCu { get; set; } = null!;
        public string MatKhauMoi { get; set; } = null!;
    }

    public class AdminCreateKhachHangRequest
    {
        public string HoTen { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
    }

    public class AdminLockKhachHangRequest
    {
        public string TrangThai { get; set; } = null!; // "Active" hoặc "Locked"
    }
}
