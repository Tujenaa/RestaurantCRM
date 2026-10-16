using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class VaiTro
{
    public string MaVaiTro { get; set; } = null!;

    public string? TenVaiTro { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<NhanVien> NhanVien { get; set; } = new List<NhanVien>();
}
