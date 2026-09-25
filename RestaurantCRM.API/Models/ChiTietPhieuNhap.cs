using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChiTietPhieuNhap
{
    public string MaChiTiet { get; set; } = null!;

    public string? MaPhieuNhap { get; set; }

    public string? MaMon { get; set; }

    public int? SoLuong { get; set; }

    public double? DonGiaNhap { get; set; }

    public double? ThanhTien { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }

    public virtual PhieuNhapHang? MaPhieuNhapNavigation { get; set; }
}
