using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChiTietKhuyenMaiMon
{
    public string MaChiTietKm { get; set; } = null!;

    public string? MaChuongTrinh { get; set; }

    public string? MaMon { get; set; }

    public int? SoLuongApDung { get; set; }

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
