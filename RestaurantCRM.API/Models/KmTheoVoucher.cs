using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class KmTheoVoucher
{
    public string MaKmvoucher { get; set; } = null!;

    public string? MaChuongTrinh { get; set; }

    public string? MaVoucher { get; set; }

    public double? GiaTriDonToiThieu { get; set; }

    public double? PhanTramGiam { get; set; }

    public double? TienGiam { get; set; }

    public virtual ICollection<HoaDon> HoaDon { get; set; } = new List<HoaDon>();

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhNavigation { get; set; }
}
