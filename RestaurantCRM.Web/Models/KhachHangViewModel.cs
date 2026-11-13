using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class KhachHangViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    public string HoTen { get; set; } = "";
    // Không bắt buộc riêng: chỉ cần có email hoặc số điện thoại (kiểm tra ở controller + API)
    [Phone(ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
    public string? SoDienThoai { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    public string? Email { get; set; }
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? MatKhau { get; set; }
    public string? MatKhauXacNhan { get; set; }
    public string? Tab { get; set; }
    public int SoDonHang { get; set; }
    public DateTime NgayThamGia { get; set; } = new(2025, 3, 1);
}
