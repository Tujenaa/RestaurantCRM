using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class LoaiMon
{
    public string? MaLoaiMon { get; set; }

    public string? TenLoaiMon { get; set; }

    public virtual ICollection<MonAn> MonAn { get; set; } = new List<MonAn>();

    public virtual ICollection<KmTheoLoai> KmTheoLoai { get; set; } = new List<KmTheoLoai>();
}
