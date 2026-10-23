using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class TraLoiDanhGia
{
    public string MaTraLoi { get; set; } = null!;

    public string? MaDanhGia { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string? NoiDung { get; set; }

    public DateTime? NgayGui { get; set; }

    public virtual DanhGia? MaDanhGiaNavigation { get; set; }

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
