using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class ChuongTrinhKhuyenMai
{
    public string MaChuongTrinh { get; set; } = null!;

    public string? TenChuongTrinh { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public virtual ICollection<KmTheoSp> KmTheoSp { get; set; } = new List<KmTheoSp>();

    public virtual ICollection<KmTheoVoucher> KmTheoVoucher { get; set; } = new List<KmTheoVoucher>();
}
