using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class CauTraLoi
{
    public string MaCauTraLoi { get; set; } = null!;

    public string? MaPhieuTraLoi { get; set; }

    public string? MaCauHoi { get; set; }

    public string? MaTuyChon { get; set; }

    public string? NoiDungTuDien { get; set; }

    public virtual CauHoiKhaoSat? MaCauHoiNavigation { get; set; }

    public virtual PhieuTraLoi? MaPhieuTraLoiNavigation { get; set; }

    public virtual TuyChonCauHoi? MaTuyChonNavigation { get; set; }
}
