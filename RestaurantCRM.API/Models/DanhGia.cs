using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class DanhGia
{
    public string MaDanhGia { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaDonHang { get; set; }

    public string? MaMon { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
