using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class NhaCungCap
{
    public string MaNhaCungCap { get; set; } = null!;

    public string? TenNhaCungCap { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<PhieuNhapHang> PhieuNhapHangs { get; set; } = new List<PhieuNhapHang>();
}
