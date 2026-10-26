using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class Quyen
{
    public string MaQuyen { get; set; } = null!;

    public string TenQuyen { get; set; } = null!;

    public string NhomQuyen { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<VaiTroQuyen> VaiTroQuyen { get; set; } = new List<VaiTroQuyen>();
}
