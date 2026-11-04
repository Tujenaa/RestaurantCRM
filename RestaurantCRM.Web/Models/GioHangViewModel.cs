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

    // Đơn giá gốc chưa giảm
    public decimal DonGiaGoc { get; set; }

    // Đơn giá sau khi áp dụng khuyến mãi món / loại món
    public decimal DonGiaSauGiam { get; set; }

    // Đơn giá bán (giữ lại để tương thích ngược)
    public decimal DonGia
    {
        get => DonGiaSauGiam > 0 ? DonGiaSauGiam : DonGiaGoc;
        set
        {
            if (DonGiaGoc <= 0) DonGiaGoc = value;
            if (DonGiaSauGiam <= 0) DonGiaSauGiam = value;
        }
    }

    public int SoLuong { get; set; }

    // Chiết khấu trên 1 phần món ăn
    public decimal ChietKhau => Math.Max(0, (DonGiaGoc > 0 ? DonGiaGoc : DonGia) - (DonGiaSauGiam > 0 ? DonGiaSauGiam : DonGia));

    // Tổng tiền chiết khấu của dòng món này
    public decimal TongChietKhau => ChietKhau * SoLuong;

    // Có khuyến mãi hay không
    public bool CoKhuyenMai => ChietKhau > 0;

    // Tỉ lệ % giảm giá
    public decimal PhanTramGiam => (CoKhuyenMai && DonGiaGoc > 0) ? Math.Round((ChietKhau / DonGiaGoc) * 100) : 0;

    // Thành tiền = Đơn giá sau giảm * Số lượng = (Đơn giá gốc - Chiết khấu) * Số lượng
    public decimal ThanhTien => (DonGiaSauGiam > 0 ? DonGiaSauGiam : DonGia) * SoLuong;

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

    // Tổng tiền theo giá gốc của các món được chọn
    public decimal TongTienGoc => ItemsDaChon.Sum(x => (x.DonGiaGoc > 0 ? x.DonGiaGoc : x.DonGia) * x.SoLuong);

    // Tổng tiền chiết khấu món/loại món của các món được chọn
    public decimal TongChietKhauMon => ItemsDaChon.Sum(x => x.TongChietKhau);

    // Tạm tính (sau khi trừ chiết khấu món/loại món)
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
