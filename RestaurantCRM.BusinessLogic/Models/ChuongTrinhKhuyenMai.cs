using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class ChuongTrinhKhuyenMai
{
    public string MaChuongTrinh { get; set; } = null!;

    public string? TenChuongTrinh { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public virtual ICollection<KmTheoDonHang> KmTheoDonHang { get; set; } = new List<KmTheoDonHang>();

    public virtual ICollection<KmTheoLoai> KmTheoLoai { get; set; } = new List<KmTheoLoai>();

    public virtual ICollection<KmTheoSp> KmTheoSp { get; set; } = new List<KmTheoSp>();
}
