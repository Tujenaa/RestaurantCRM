using System;
using System.Collections.Generic;
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
    /// <summary>NULL = lượt chỉ có nhận xét món, không chấm sao.</summary>
    public int? DanhGia { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayGui { get; set; }
    public List<TraLoiDanhGiaViewModel> TraLois { get; set; } = new();
}

public class DishReviewInputItem
{
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string? DuongDanAnh { get; set; }
    /// <summary>Số sao hiển thị; chỉ có ý nghĩa khi ChamRieng = true.</summary>
    public int DanhGia { get; set; } = 5;
    public string? NoiDung { get; set; }
    /// <summary>true = khách bấm sao riêng hoặc viết nhận xét cho món; false = món theo điểm chung (lưu NULL).</summary>
    public bool ChamRieng { get; set; }
}

public class PhanHoiViewModel
{
    // Ngữ cảnh đơn hàng và món ăn
    public string? MaDonHang { get; set; }
    public string? MaMon { get; set; }
    public string CheDoDanhGia { get; set; } = "DonHang";

    [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
    public int DanhGia { get; set; } = 5;

    [StringLength(1000, ErrorMessage = "Nội dung không được quá 1000 ký tự.")]
    public string? NoiDung { get; set; }

    // Đánh giá chi tiết từng món trong đơn (Chuẩn Master-Detail)
    public List<DishReviewInputItem> ChiTietMonDanhGia { get; set; } = new();

    // Dữ liệu hiển thị
    public DonHangViewModel? DonHangHienTai { get; set; }
    public IReadOnlyList<DonHangViewModel> DonHangs { get; set; } = Array.Empty<DonHangViewModel>();
    public IReadOnlyList<MonAnViewModel> MonAns { get; set; } = Array.Empty<MonAnViewModel>();
    public IReadOnlyList<PhanHoiItemViewModel> PhanHois { get; set; } = Array.Empty<PhanHoiItemViewModel>();
}

public class ChiTietMonDanhGiaItemViewModel
{
    public string MaChiTietDanhGia { get; set; } = "";
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public string? DuongDanAnh { get; set; }
    /// <summary>NULL = món không được chấm riêng.</summary>
    public int? SoSao { get; set; }
    public string? NoiDung { get; set; }
}

public class ChiTietDanhGiaDonHangViewModel
{
    public string MaDanhGia { get; set; } = "";
    public string MaHoaDon { get; set; } = "";
    public string MaKhachHang { get; set; } = "";
    public string TenKhachHang { get; set; } = "Khách hàng";
    public int SoSaoChung { get; set; }
    public int DanhGiaChung { get => SoSaoChung; set => SoSaoChung = value; }
    public string? NoiDungChung { get; set; }
    public DateTime? NgayDanhGia { get; set; }
    public DateTime? NgayGui { get => NgayDanhGia; set => NgayDanhGia = value; }
    public List<ChiTietMonDanhGiaItemViewModel> ChiTietMon { get; set; } = new();
    public List<TraLoiDanhGiaViewModel> HoiThoai { get; set; } = new();
    public List<TraLoiDanhGiaViewModel> TraLois { get => HoiThoai; set => HoiThoai = value; }
}
