using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class PhanHoiItemViewModel
{
    public string TenMon { get; set; } = "";
    public int DanhGia { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayGui { get; set; }
}

public class PhanHoiViewModel
{
    [Required]
    public string MaMon { get; set; } = "";
    [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
    public int DanhGia { get; set; } = 5;
    [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
    [StringLength(1000)]
    public string NoiDung { get; set; } = "";
    public IReadOnlyList<MonAnViewModel> MonAns { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<PhanHoiItemViewModel> PhanHois { get; set; } = Array.Empty<PhanHoiItemViewModel>();
    public IReadOnlyList<DonHangViewModel> DonHangs { get; set; } = Array.Empty<DonHangViewModel>();
    public string MaDonHang { get; set; } = "";
}
