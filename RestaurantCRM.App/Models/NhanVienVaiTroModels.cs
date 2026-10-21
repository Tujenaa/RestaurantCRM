using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Models
{
    public class VaiTroModel
    {
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string MoTa { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public int SoLuongNhanVien { get; set; }
    }

    public class PermissionItemModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
    }

    public class NhanVienModel
    {
        public string MaNhanVien { get; set; }
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string TrangThai { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
