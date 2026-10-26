using System;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class VaiTroQuyen
{
    public string MaVaiTro { get; set; } = null!;

    public string MaQuyen { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    public virtual Quyen MaQuyenNavigation { get; set; } = null!;
}
