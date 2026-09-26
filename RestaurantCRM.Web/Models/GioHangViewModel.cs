using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class GioHangItemViewModel
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string BieuTuong { get; set; } = "🍲";
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public decimal ThanhTien => DonGia * SoLuong;
}

public class GioHangViewModel
{
    public List<GioHangItemViewModel> Items { get; set; } = new();
    public string? MaVoucher { get; set; }
    public decimal GiamGia { get; set; }
    public decimal PhiGiaoHang => Items.Count == 0 ? 0 : 15000;
    public decimal TamTinh => Items.Sum(x => x.ThanhTien);
    public decimal TongThanhToan => Math.Max(0, TamTinh - GiamGia + PhiGiaoHang);
}

public class GioHangPageViewModel
{
    public GioHangViewModel GioHang { get; set; } = new();
    public DatHangViewModel DatHang { get; set; } = new();
}

public class CheckoutPageViewModel
{
    public GioHangViewModel GioHang { get; set; } = new();
    public DatHangViewModel DatHang { get; set; } = new();
}

public class CapNhatSoLuongViewModel
{
    [Required]
    public string MaMon { get; set; } = "";
    [Range(0, 99)]
    public int SoLuong { get; set; }
}

public class ApDungVoucherViewModel
{
    [StringLength(30)]
    public string MaVoucher { get; set; } = "";
}
