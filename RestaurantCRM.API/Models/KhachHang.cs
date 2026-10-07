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

    public virtual ICollection<DoiTuongKhaoSat> DoiTuongKhaoSat { get; set; } = new List<DoiTuongKhaoSat>();

    public virtual ICollection<HoaDon> HoaDon { get; set; } = new List<HoaDon>();

    public virtual ICollection<PhanHoi> PhanHoi { get; set; } = new List<PhanHoi>();

    public virtual ICollection<PhieuTraLoi> PhieuTraLoi { get; set; } = new List<PhieuTraLoi>();

    public virtual ICollection<TraLoiDanhGia> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGia>();

    public virtual ICollection<TraLoiPhanHoi> TraLoiPhanHoi { get; set; } = new List<TraLoiPhanHoi>();
}
