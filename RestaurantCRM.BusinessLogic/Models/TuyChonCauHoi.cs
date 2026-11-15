using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class TuyChonCauHoi
{
    public string MaTuyChon { get; set; } = null!;

    public string? MaCauHoi { get; set; }

    public string? NoiDungTuyChon { get; set; }

    /// <summary>Đáp án gắn với một món (câu "Món yêu thích" của hồ sơ sở thích).</summary>
    public string? MaMon { get; set; }

    public virtual ICollection<CauTraLoi> CauTraLoi { get; set; } = new List<CauTraLoi>();

    public virtual CauHoiKhaoSat? MaCauHoiNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
