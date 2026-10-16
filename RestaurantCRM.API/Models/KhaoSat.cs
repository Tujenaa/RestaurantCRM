using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.Models;

public partial class KhaoSat
{
    public string MaKhaoSat { get; set; } = null!;

    public string? MaNhanVien { get; set; }

    public string? TieuDe { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<CauHoiKhaoSat> CauHoiKhaoSat { get; set; } = new List<CauHoiKhaoSat>();

    public virtual ICollection<DoiTuongKhaoSat> DoiTuongKhaoSat { get; set; } = new List<DoiTuongKhaoSat>();

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual ICollection<PhieuTraLoi> PhieuTraLoi { get; set; } = new List<PhieuTraLoi>();
}
