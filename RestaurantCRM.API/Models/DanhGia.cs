using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class DanhGia
{
    public string MaDanhGia { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaHoaDon { get; set; }

    public string? MaMon { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }

    public virtual ICollection<TraLoiDanhGia> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGia>();
}
