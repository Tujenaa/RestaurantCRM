using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class LoaiMon
{
    public string MaLoaiMon { get; set; } = null!;

    public string? TenLoaiMon { get; set; }

    public virtual ICollection<MonAn> MonAn { get; set; } = new List<MonAn>();
}
