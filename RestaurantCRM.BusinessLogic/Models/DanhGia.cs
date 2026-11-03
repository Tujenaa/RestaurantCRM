using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class DanhGia
{
    public string MaDanhGia { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public string? MaHoaDon { get; set; }

    public string? MaMon { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public bool TrangThai { get; set; } = true;

    /// <summary>Nhân viên đã xử lý phản hồi (bấm "Đã xem" hoặc đã trả lời).</summary>
    public bool DaXuLy { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }

    public virtual ICollection<ChiTietDanhGia> ChiTietDanhGia { get; set; } = new List<ChiTietDanhGia>();

    public virtual ICollection<TraLoiDanhGia> TraLoiDanhGia { get; set; } = new List<TraLoiDanhGia>();
}
