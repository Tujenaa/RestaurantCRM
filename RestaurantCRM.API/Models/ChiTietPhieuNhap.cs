using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChiTietPhieuNhap
{
    public string MaChiTiet { get; set; } = null!;

    public string? MaPhieuNhap { get; set; }

    public string? MaNguyenLieu { get; set; }

    public decimal? SoLuong { get; set; }

    public double? DonGiaNhap { get; set; }

    public double? ThanhTien { get; set; }

    public virtual NguyenLieu? MaNguyenLieuNavigation { get; set; }

    public virtual PhieuNhapHang? MaPhieuNhapNavigation { get; set; }
}
