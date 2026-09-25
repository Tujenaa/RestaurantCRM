using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChuongTrinhKhuyenMai
{
    public string MaChuongTrinh { get; set; } = null!;

    public string? TenChuongTrinh { get; set; }

    public string? LoaiKhuyenMai { get; set; }

    public string? LoaiGiam { get; set; }

    public double? GiaTriGiam { get; set; }

    public double? GiaTriDonToiThieu { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietKhuyenMaiMon> ChiTietKhuyenMaiMons { get; set; } = new List<ChiTietKhuyenMaiMon>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
