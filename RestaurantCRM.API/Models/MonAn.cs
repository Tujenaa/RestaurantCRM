using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class MonAn
{
    public string MaMon { get; set; } = null!;

    public string? MaLoaiMon { get; set; }

    public string? TenMon { get; set; }

    public string? MoTa { get; set; }

    public double? DonGia { get; set; }

    public int? SoLuong { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietKhuyenMaiMon> ChiTietKhuyenMaiMons { get; set; } = new List<ChiTietKhuyenMaiMon>();

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<HinhAnhMonAn> HinhAnhMonAns { get; set; } = new List<HinhAnhMonAn>();

    public virtual LoaiMon? MaLoaiMonNavigation { get; set; }
}
