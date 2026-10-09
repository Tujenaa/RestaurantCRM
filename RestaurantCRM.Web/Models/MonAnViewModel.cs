using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.Web.Models;

public class MonAnViewModel
{
    public string MaMon { get; set; } = "";
    public string? MaLoaiMon { get; set; }
    public string LoaiMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string DuongDanAnh { get; set; } = "";
    public decimal DonGia { get; set; }
    public decimal DonGiaSauGiam { get; set; }
    public int SoLuong { get; set; } = 0;
    public string TrangThai { get; set; } = "InStock"; // InStock, OutOfStock, Active, Discontinued
    public string BieuTuong { get; set; } = "🍲";
    public string Tag { get; set; } = "";
    public string Tone { get; set; } = "";

    // Computed properties for UI display
    public bool ConHang => (TrangThai == "Active" || TrangThai == "InStock") && SoLuong > 0;

    public string TrangThaiHienThi
    {
        get
        {
            if (TrangThai == "Discontinued") return "Ngừng kinh doanh";
            if (!ConHang || TrangThai == "OutOfStock" || SoLuong <= 0) return "Tạm hết hàng";
            return "Đang kinh doanh";
        }
    }

    public string TrangThaiBadgeClass
    {
        get
        {
            if (TrangThai == "Discontinued") return "badge-discontinued";
            if (!ConHang || TrangThai == "OutOfStock" || SoLuong <= 0) return "badge-outofstock";
            return "badge-instock";
        }
    }
}

public class LoaiMonItemViewModel
{
    public string MaLoaiMon { get; set; } = "";
    public string TenLoaiMon { get; set; } = "";
    public int SoLuongMon { get; set; }
}

public class MonAnDanhSachViewModel
{
    public IReadOnlyList<MonAnViewModel> MonAns { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<MonAnViewModel> MonNoiBat { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<LoaiMonItemViewModel> DanhSachLoaiMon { get; set; } = Array.Empty<LoaiMonItemViewModel>();
    public IReadOnlyList<string> LoaiMons { get; set; } = Array.Empty<string>();
    public string? LoaiDangChon { get; set; }
    public string? TuKhoa { get; set; }
    public bool MenuMode { get; set; }
    public int TongSoMon => MonAns.Count;
}

public class MonAnChiTietViewModel
{
    public MonAnViewModel MonAn { get; set; } = new();
    public IReadOnlyList<MonAnViewModel> MonLienQuan { get; set; } = Array.Empty<MonAnViewModel>();
}

public class ThemGioHangViewModel
{
    [Required]
    public string MaMon { get; set; } = "";
    [Range(1, 999)]
    public int SoLuong { get; set; } = 1;
}
