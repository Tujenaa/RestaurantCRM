using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class KhachHang
{
    public string MaKhachHang { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? MatKhau { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? SoThich { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<DoiTuongKhaoSat> DoiTuongKhaoSats { get; set; } = new List<DoiTuongKhaoSat>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();

    public virtual ICollection<PhieuTraLoi> PhieuTraLois { get; set; } = new List<PhieuTraLoi>();
}
