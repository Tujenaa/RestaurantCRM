using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class KmTheoLoai
{
    public string MaKMLoai { get; set; } = null!;

    public string? MaChuongTrinh { get; set; }

    public string MaLoaiMon { get; set; } = null!;

    public string? MaVoucher { get; set; }

    public double? GiaTriDonToiThieu { get; set; }

    public double? PhanTramGiam { get; set; }

    public double? TienGiam { get; set; }

    public double? GiamToiDa { get; set; }

    public int? SoLuongToiThieu { get; set; } = 1;

    public bool? ChoPhepCongDon { get; set; } = true;

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhNavigation { get; set; }

    public virtual LoaiMon? MaLoaiMonNavigation { get; set; }
}
