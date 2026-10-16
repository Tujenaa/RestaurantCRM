using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class DinhLuong
{
    public string MaDinhLuong { get; set; } = null!;

    public string MaMon { get; set; } = null!;

    public string MaNguyenLieu { get; set; } = null!;

    public decimal SoLuongCan { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }

    public virtual NguyenLieu? MaNguyenLieuNavigation { get; set; }
}
