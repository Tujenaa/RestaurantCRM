using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class NhanVien
{
    public string MaNhanVien { get; set; } = null!;

    public string? MaVaiTro { get; set; }

    public string? TenDangNhap { get; set; }

    public string? MatKhau { get; set; }

    public string? HoTen { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<HoaDon> HoaDon { get; set; } = new List<HoaDon>();

    public virtual ICollection<KhaoSat> KhaoSat { get; set; } = new List<KhaoSat>();

    public virtual VaiTro? MaVaiTroNavigation { get; set; }

    public virtual ICollection<PhanHoi> PhanHoi { get; set; } = new List<PhanHoi>();

    public virtual ICollection<PhieuNhapHang> PhieuNhapHang { get; set; } = new List<PhieuNhapHang>();

    public virtual ICollection<TraLoiDanhGia> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGia>();

    public virtual ICollection<TraLoiPhanHoi> TraLoiPhanHoi { get; set; } = new List<TraLoiPhanHoi>();
}
