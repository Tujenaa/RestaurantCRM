using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class MonAnViewModel
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string LoaiMon { get; set; } = "";
    public string MoTa { get; set; } = "";
    public decimal DonGia { get; set; }
    public string BieuTuong { get; set; } = "🍲";
    public string Tag { get; set; } = "";
    public string Tone { get; set; } = "";
}

public class MonAnDanhSachViewModel
{
    public IReadOnlyList<MonAnViewModel> MonAns { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<MonAnViewModel> MonNoiBat { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<string> LoaiMons { get; set; } = Array.Empty<string>();
    public string? LoaiDangChon { get; set; }
    public string? TuKhoa { get; set; }
    public bool MenuMode { get; set; }
}

public class ThemGioHangViewModel
{
    [Required]
    public string MaMon { get; set; } = "";
    [Range(1, 99)]
    public int SoLuong { get; set; } = 1;
}
