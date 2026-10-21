using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class NguyenLieu
{
    public string MaNguyenLieu { get; set; } = null!;

    public string TenNguyenLieu { get; set; } = null!;

    public string DonViTinh { get; set; } = null!;

    public decimal? SoLuongTon { get; set; }

    public double? GiaNhapGanNhat { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<DinhLuong> DinhLuong { get; set; } = new List<DinhLuong>();

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; } = new List<ChiTietPhieuNhap>();
}
