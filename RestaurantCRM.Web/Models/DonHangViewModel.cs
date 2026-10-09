using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class DonHangViewModel
{
    public string MaDonHang { get; set; } = "";
    public string MaKhachHang { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal TienGiamVoucher { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThai { get; set; } = "";
    public string DiaChiGiao { get; set; } = "";
    public string HinhThucNhan { get; set; } = "Giao hàng tận nơi";
    public string PhuongThucThanhToan { get; set; } = "COD";
    public List<string> TienTrinh { get; set; } = new();
    public int BuocHienTai { get; set; }
    public string TrangThaiLoc { get; set; } = "active";
    public bool CanCancel => TrangThai == "Pending" || TrangThai == "Đang chờ xác nhận";
    public List<DonHangChiTietViewModel> ChiTiet { get; set; } = new();
}

public class DonHangChiTietViewModel
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string DuongDanAnh { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGiaGoc { get; set; }
    public decimal DonGiaSauGiam { get; set; }
    public decimal TienGiamMon { get; set; }
    public decimal ThanhTien { get; set; }

    // Legacy compatibility property
    public decimal DonGia
    {
        get => DonGiaSauGiam > 0 ? DonGiaSauGiam : DonGiaGoc;
        set => DonGiaGoc = value;
    }
}

public class DonHangDanhSachViewModel
{
    public IReadOnlyList<DonHangViewModel> DonHangs { get; set; } = Array.Empty<DonHangViewModel>();
    public string TrangThaiDangChon { get; set; } = "all";
}

public class DatHangViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
    [StringLength(100)]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
    [RegularExpression(@"^[0-9+(). -]{9,15}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(255)]
    public string DiaChiGiao { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn tỉnh hoặc thành phố.")]
    public string TinhThanh { get; set; } = "TP. Hồ Chí Minh";

    public string HinhThucNhan { get; set; } = "delivery"; // delivery = Giao hàng tận nơi, pickup = Đến lấy tại nhà hàng

    public string ThoiGianNhan { get; set; } = "Giao sớm nhất có thể";

    public string PhuongThucThanhToan { get; set; } = "cod"; // cod = Tiền mặt, transfer = Chuyển khoản

    [StringLength(500)]
    public string? GhiChu { get; set; }
}
