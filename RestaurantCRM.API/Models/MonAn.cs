using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class MonAn
{
    public string MaMon { get; set; } = null!;

    public string? MaLoaiMon { get; set; }

    public string? TenMon { get; set; }

    public string? MoTa { get; set; }

    public string? DuongDanAnh { get; set; }

    public double? DonGia { get; set; }

    public int? SoLuong { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<KmTheoSp> KmTheoSp { get; set; } = new List<KmTheoSp>();

    public virtual LoaiMon? MaLoaiMonNavigation { get; set; }
}
