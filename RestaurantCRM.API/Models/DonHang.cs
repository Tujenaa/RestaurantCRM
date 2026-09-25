using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class DonHang
{
    public string MaDonHang { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaNhanVien { get; set; }

    public string? MaChuongTrinhVoucher { get; set; }

    public string? DiaChiGiao { get; set; }

    public DateTime? NgayDat { get; set; }

    public string? TrangThai { get; set; }

    public double? TongTienHang { get; set; }

    public double? TienGiamVoucher { get; set; }

    public double? TongThanhToan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<LichSuTrangThai> LichSuTrangThais { get; set; } = new List<LichSuTrangThai>();

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhVoucherNavigation { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
