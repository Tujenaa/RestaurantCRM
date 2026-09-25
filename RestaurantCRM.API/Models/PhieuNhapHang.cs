using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class PhieuNhapHang
{
    public string MaPhieuNhap { get; set; } = null!;

    public string? MaNhaCungCap { get; set; }

    public string? MaNhanVien { get; set; }

    public DateTime? NgayNhap { get; set; }

    public double? TongTien { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual NhaCungCap? MaNhaCungCapNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
