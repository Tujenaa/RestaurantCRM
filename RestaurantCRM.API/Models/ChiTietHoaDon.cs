using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChiTietHoaDon
{
    public string MaChiTiet { get; set; } = null!;

    public string? MaHoaDon { get; set; }

    public string? MaMon { get; set; }

    public string? MaKmsp { get; set; }

    public string? TenMon { get; set; }

    public int? SoLuong { get; set; }

    public double? DonGiaGoc { get; set; }

    public double? TienGiamMon { get; set; }

    public double? DonGiaSauGiam { get; set; }

    public double? ThanhTien { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }

    public virtual KmTheoSp? MaKmspNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
