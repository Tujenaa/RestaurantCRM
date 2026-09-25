using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class LichSuTrangThai
{
    public string MaLichSu { get; set; } = null!;

    public string? MaDonHang { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? ThoiGian { get; set; }

    public string? GhiChu { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }
}
