using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class TraLoiDanhGiaViewModel
{
    public string MaTraLoi { get; set; } = "";
    public string NguoiGui { get; set; } = "NhanVien"; // KhachHang hoặc NhanVien
    public string NguoiTraLoi { get; set; } = "Nhà hàng Lẩu Phố";
    public string NoiDung { get; set; } = "";
    public DateTime NgayGui { get; set; }
}

public class PhanHoiItemViewModel
{
    public string MaDanhGia { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string? DuongDanAnh { get; set; }
    public string? MaDonHang { get; set; }
    public string? MaKhachHang { get; set; }
    public string TenKhachHang { get; set; } = "Khách hàng";
    public int DanhGia { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayGui { get; set; }
    public List<TraLoiDanhGiaViewModel> TraLois { get; set; } = new();
}

public class PhanHoiViewModel
{
    // Ngữ cảnh đơn hàng và món ăn
    public string? MaDonHang { get; set; }
    public string? MaMon { get; set; }
    public string CheDoDanhGia { get; set; } = "DonHang"; // "DonHang" (tất cả các món) hoặc "TungMon" (chọn món cụ thể)

    [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
    public int DanhGia { get; set; } = 5;

    [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
    [StringLength(1000, ErrorMessage = "Nội dung không được quá 1000 ký tự.")]
    public string NoiDung { get; set; } = "";

    // Dữ liệu hiển thị
    public DonHangViewModel? DonHangHienTai { get; set; }
    public IReadOnlyList<DonHangViewModel> DonHangs { get; set; } = Array.Empty<DonHangViewModel>();
    public IReadOnlyList<MonAnViewModel> MonAns { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<PhanHoiItemViewModel> PhanHois { get; set; } = Array.Empty<PhanHoiItemViewModel>();
}
