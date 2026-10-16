using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class KmTheoSp
{
    public string MaKmsp { get; set; } = null!;

    public string? MaChuongTrinh { get; set; }

    public string? MaMon { get; set; }

    public double? PhanTramGiam { get; set; }

    public double? TienGiam { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
