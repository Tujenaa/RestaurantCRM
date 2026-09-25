using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class CauHoiKhaoSat
{
    public string MaCauHoi { get; set; } = null!;

    public string? MaKhaoSat { get; set; }

    public string? NoiDungCauHoi { get; set; }

    public string? LoaiCauHoi { get; set; }

    public virtual ICollection<CauTraLoi> CauTraLois { get; set; } = new List<CauTraLoi>();

    public virtual KhaoSat? MaKhaoSatNavigation { get; set; }

    public virtual ICollection<TuyChonCauHoi> TuyChonCauHois { get; set; } = new List<TuyChonCauHoi>();
}
