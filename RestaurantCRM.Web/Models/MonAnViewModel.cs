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
    public double DiemDanhGiaTrungBinh { get; set; } = 5.0;
    public int SoLuongDanhGia { get; set; } = 0;

    // Computed properties for UI display
    public bool ConHang => (TrangThai == "Active" || TrangThai == "InStock") && SoLuong > 0;

    public bool NgungKinhDoanh => string.Equals(TrangThai, "Discontinued", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase);

    public bool HetHang => !NgungKinhDoanh && (!ConHang || string.Equals(TrangThai, "OutOfStock", StringComparison.OrdinalIgnoreCase) || SoLuong <= 0);

    public string TrangThaiHienThi
    {
        get
        {
            if (NgungKinhDoanh) return "Ngừng kinh doanh";
            if (HetHang) return "Hết hàng";
            return "Đang kinh doanh";
        }
    }

    public string TrangThaiBadgeClass
    {
        get
        {
            if (NgungKinhDoanh) return "badge-discontinued";
            if (HetHang) return "badge-outofstock";
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
    public string? TenLoaiHienTai { get; set; }
    public string? TuKhoa { get; set; }
    public string? SortDangChon { get; set; }
    public bool MenuMode { get; set; }

    // Thuộc tính phân trang (Pagination)
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public int TotalDishes { get; set; } = 0;
    public int TotalPages { get; set; } = 1;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int TongSoMon => TotalDishes > 0 ? TotalDishes : MonAns.Count;
}

public class MonAnChiTietViewModel
{
    public MonAnViewModel MonAn { get; set; } = new();
    public IReadOnlyList<MonAnViewModel> MonLienQuan { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<PhanHoiItemViewModel> DanhGias { get; set; } = Array.Empty<PhanHoiItemViewModel>();
}

public class ThemGioHangViewModel
{
    [Required]
    public string MaMon { get; set; } = "";
    [Range(1, 999)]
    public int SoLuong { get; set; } = 1;
}
