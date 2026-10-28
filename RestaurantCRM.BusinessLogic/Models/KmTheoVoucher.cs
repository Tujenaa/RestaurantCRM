using System;
using System.Collections.Generic;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class KmTheoVoucher
{
    public string MaKmvoucher { get; set; } = null!;

    public string? MaChuongTrinh { get; set; }

    public string? MaVoucher { get; set; }

    public double? GiaTriDonToiThieu { get; set; }

    public double? PhanTramGiam { get; set; }

    public double? TienGiam { get; set; }

    public double? GiamToiDa { get; set; }

    public string? PhamViApDung { get; set; } = "TOAN_DON";

    public string? MaLoaiMon { get; set; }

    public string? MaMon { get; set; }

    public int? SoLuongToiThieu { get; set; } = 1;

    public bool? ChoPhepCongDon { get; set; } = true;

    public virtual ICollection<HoaDon> HoaDon { get; set; } = new List<HoaDon>();

    public virtual ChuongTrinhKhuyenMai? MaChuongTrinhNavigation { get; set; }

    public virtual LoaiMon? MaLoaiMonNavigation { get; set; }

    public virtual MonAn? MaMonNavigation { get; set; }
}
