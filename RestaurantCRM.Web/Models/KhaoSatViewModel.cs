using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class KhaoSatViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn tần suất ghé nhà hàng.")]
    public string TanSuat { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng chọn mức đánh giá.")]
    public string MucDoHaiLong { get; set; } = "";
    public List<string> YeuToQuanTam { get; set; } = new();
    [StringLength(500)]
    public string? MonMongMuon { get; set; }
    [StringLength(1000)]
    public string? GopY { get; set; }
}
