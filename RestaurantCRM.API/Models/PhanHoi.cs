using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class PhanHoi
{
    public string MaPhanHoi { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaNhanVien { get; set; }

    public string? NoiDung { get; set; }

    public int? DanhGia { get; set; }

    public DateTime? NgayPhanHoi { get; set; }

    public string? TrangThai { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
