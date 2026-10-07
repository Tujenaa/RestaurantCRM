using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ThanhToan
{
    public string MaThanhToan { get; set; } = null!;

    public string? MaHoaDon { get; set; }

    public string? PhuongThuc { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public double? SoTien { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }
}
