using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class DonHangViewModel
{
    public string MaDonHang { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThai { get; set; } = "";
    public List<string> TienTrinh { get; set; } = new();
    public int BuocHienTai { get; set; }
    public string TrangThaiLoc { get; set; } = "active";
    public List<DonHangChiTietViewModel> ChiTiet { get; set; } = new();
}

public class DonHangChiTietViewModel
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
}

public class DonHangDanhSachViewModel
{
    public IReadOnlyList<DonHangViewModel> DonHangs { get; set; } = Array.Empty<DonHangViewModel>();
    public string TrangThaiDangChon { get; set; } = "all";
}

public class DatHangViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
    public string SoDienThoai { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
    [StringLength(255)]
    public string DiaChiGiao { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng chọn tỉnh hoặc thành phố.")]
    public string TinhThanh { get; set; } = "";
    public string ThoiGianNhan { get; set; } = "Giao sớm nhất có thể";
    public string PhuongThucThanhToan { get; set; } = "cod";
    [StringLength(500)]
    public string GhiChu { get; set; } = "";
}
