using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class HinhAnhMonAn
{
    public string MaHinhAnh { get; set; } = null!;

    public string? MaMon { get; set; }

    public string? DuongDanAnh { get; set; }

    public bool? LaAnhDaiDien { get; set; }

    public int? ThuTuHienThi { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
