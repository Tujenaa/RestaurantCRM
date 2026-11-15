using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class KhachHangViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    public string HoTen { get; set; } = "";
    // Không bắt buộc riêng: chỉ cần có email hoặc số điện thoại (kiểm tra ở controller + API)
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    public string? SoDienThoai { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    public string? Email { get; set; }
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
}
