using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class TraLoiDanhGia
{
    public string MaTraLoi { get; set; } = null!;
    public string? MaDanhGia { get; set; }
    public string? NguoiGui { get; set; }
    public string? MaKhachHang { get; set; }
    public string? MaNhanVien { get; set; }
    public string? NoiDung { get; set; }
    public DateTime? NgayGui { get; set; }

    public virtual DanhGia? MaDanhGiaNavigation { get; set; }
    public virtual KhachHang? MaKhachHangNavigation { get; set; }
    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
