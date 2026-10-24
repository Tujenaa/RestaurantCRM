using System;

namespace RestaurantCRM.API.DTOs
{
    public class RegisterRequest
    {
        public string HoTen { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string MatKhau { get; set; } = null!;
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
    }
}
