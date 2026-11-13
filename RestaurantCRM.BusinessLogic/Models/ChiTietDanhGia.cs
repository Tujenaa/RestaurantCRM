using System;

namespace RestaurantCRM.BusinessLogic.Models;

public partial class ChiTietDanhGia
{
    public string MaChiTietDanhGia { get; set; } = null!;

    public string MaDanhGia { get; set; } = null!;

    public string MaMon { get; set; } = null!;

    /// <summary>NULL = khách không đánh giá riêng món này.</summary>
    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public virtual DanhGia MaDanhGiaNavigation { get; set; } = null!;

    public virtual MonAn MaMonNavigation { get; set; } = null!;
}

