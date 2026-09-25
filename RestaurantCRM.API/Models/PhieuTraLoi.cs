using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class PhieuTraLoi
{
    public string MaPhieuTraLoi { get; set; } = null!;

    public string? MaKhaoSat { get; set; }

    public string? MaKhachHang { get; set; }

    public DateTime? NgayTraLoi { get; set; }

    public virtual ICollection<CauTraLoi> CauTraLois { get; set; } = new List<CauTraLoi>();

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual KhaoSat? MaKhaoSatNavigation { get; set; }
}
