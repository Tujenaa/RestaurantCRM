using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestaurantCRM.Web.Models;

public class GioHangItemViewModel
{
    public string MaMon { get; set; } = "";
    public string? MaLoaiMon { get; set; }
    public string TenMon { get; set; } = "";
    public string DuongDanAnh { get; set; } = "/images/default.jpg";
    public string BieuTuong { get; set; } = "🍲";
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public decimal ThanhTien => DonGia * SoLuong;

    // Khách hàng có chọn món này để đặt hay không (lưu cùng giỏ hàng)
    public bool DuocChon { get; set; } = true;

    // Trạng thái khả dụng thực tế, được làm mới mỗi lần tải trang giỏ hàng / thanh toán
    [JsonIgnore] public bool KhaDung { get; set; } = true;
    [JsonIgnore] public string? TrangThaiMon { get; set; }
}

public class GioHangViewModel
{
    public List<GioHangItemViewModel> Items { get; set; } = new();
    public List<GioHangItemViewModel> ItemsDaChon => Items.Where(x => x.DuocChon && x.KhaDung).ToList();
    public int SoMonKhaDung => Items.Count(x => x.KhaDung);
    public bool DaChonTatCa => SoMonKhaDung > 0 && Items.Where(x => x.KhaDung).All(x => x.DuocChon);
    public string? MaVoucher { get; set; }
    public decimal GiamGia { get; set; }
    public decimal PhiGiaoHang => 0; // Đã bỏ phí giao hàng
    public decimal TamTinh => ItemsDaChon.Sum(x => x.ThanhTien);
    public decimal TongThanhToan => Math.Max(0, TamTinh - GiamGia + PhiGiaoHang);
}

public class GioHangPageViewModel
{
    public GioHangViewModel GioHang { get; set; } = new();
    public DatHangViewModel DatHang { get; set; } = new();
    public List<VoucherViewModel> AvailableVouchers { get; set; } = new();
}

public class CheckoutPageViewModel
{
    public GioHangViewModel GioHang { get; set; } = new();
    public DatHangViewModel DatHang { get; set; } = new();
    public List<VoucherViewModel> AvailableVouchers { get; set; } = new();
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
    public string? MaVoucher { get; set; }
    public string? ReturnUrl { get; set; }
}
