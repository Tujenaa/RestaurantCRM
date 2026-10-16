using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class TraLoiPhanHoi
{
    public string MaTraLoi { get; set; } = null!;

    public string? MaPhanHoi { get; set; }

    public string? NguoiGui { get; set; }

    public string? MaKhachHang { get; set; }

    public string? MaNhanVien { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayGui { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual PhanHoi? MaPhanHoiNavigation { get; set; }
}
