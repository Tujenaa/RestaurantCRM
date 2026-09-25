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

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<KhaoSat> KhaoSats { get; set; } = new List<KhaoSat>();

    public virtual VaiTro? MaVaiTroNavigation { get; set; }

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();

    public virtual ICollection<PhieuNhapHang> PhieuNhapHangs { get; set; } = new List<PhieuNhapHang>();
}
