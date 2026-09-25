using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class VaiTro
{
    public string MaVaiTro { get; set; } = null!;

    public string? TenVaiTro { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
