using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class TuyChonCauHoi
{
    public string MaTuyChon { get; set; } = null!;

    public string? MaCauHoi { get; set; }

    public string? NoiDungTuyChon { get; set; }

    public virtual ICollection<CauTraLoi> CauTraLois { get; set; } = new List<CauTraLoi>();

    public virtual CauHoiKhaoSat? MaCauHoiNavigation { get; set; }
}
