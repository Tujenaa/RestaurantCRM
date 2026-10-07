using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class HoaDon
{
    public string MaHoaDon { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaNhanVien { get; set; }

    public string? MaKmvoucher { get; set; }

    public string? TenKhachHang { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChiGiao { get; set; }

    public DateTime? NgayDat { get; set; }

    public DateTime? NgayHoanTat { get; set; }

    public string? TrangThai { get; set; }

    public double? TongTienHang { get; set; }

    public double? TienGiamVoucher { get; set; }

    public double? TongThanhToan { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<LichSuTrangThai> LichSuTrangThai { get; set; } = new List<LichSuTrangThai>();

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual KmTheoVoucher? MaKmvoucherNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual ICollection<ThanhToan> ThanhToan { get; set; } = new List<ThanhToan>();
}
