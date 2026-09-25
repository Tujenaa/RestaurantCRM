using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class DoiTuongKhaoSat
{
    public string MaDoiTuong { get; set; } = null!;

    public string? MaKhaoSat { get; set; }

    public string? MaKhachHang { get; set; }

    public bool? DaHoanThanh { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual KhaoSat? MaKhaoSatNavigation { get; set; }
}
