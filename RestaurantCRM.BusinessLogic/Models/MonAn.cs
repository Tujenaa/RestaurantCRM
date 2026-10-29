using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class MonAn
{
    public string? MaMon { get; set; }

    public string? MaLoaiMon { get; set; }

    public string? TenMon { get; set; }

    public string? MoTa { get; set; }

    public string? DuongDanAnh { get; set; }

    public double? DonGia { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<DinhLuong> DinhLuong { get; set; } = new List<DinhLuong>();

    public virtual ICollection<KmTheoSp> KmTheoSp { get; set; } = new List<KmTheoSp>();

    public virtual LoaiMon? MaLoaiMonNavigation { get; set; }
}
