using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class TaiKhoanAdmin
{
    public string MaAdmin { get; set; } = null!;

    public string? TenDangNhap { get; set; }

    public string? MatKhau { get; set; }

    public string? HoTen { get; set; }
}
