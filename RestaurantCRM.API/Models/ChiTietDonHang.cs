using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChiTietDonHang
{
    public string MaChiTiet { get; set; } = null!;

    public string? MaDonHang { get; set; }

    public string? MaMon { get; set; }

    public string? MaChuongTrinhKm { get; set; }

    public int? SoLuong { get; set; }

    public double? DonGiaGoc { get; set; }

    public double? TienGiamMon { get; set; }

    public double? DonGiaSauGiam { get; set; }

    public double? ThanhTien { get; set; }

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhKmNavigation { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
