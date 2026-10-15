using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class DinhLuongItemRequest
    {
        public string MaNguyenLieu { get; set; } = null!;
        public decimal SoLuongCan { get; set; }
    }

    public class TaoMonAnRequest
    {
        public string TenMon { get; set; } = null!;
        public string MaLoaiMon { get; set; } = null!;
        public double DonGia { get; set; }
        public string? MoTa { get; set; }
        public string? DuongDanAnh { get; set; }
        public string? TrangThai { get; set; } = "InStock";

        // Bắt buộc phải có ít nhất 1 định lượng
        public List<DinhLuongItemRequest> DinhLuong { get; set; } = new();
    }

    public class CapNhatMonAnRequest
    {
        public string TenMon { get; set; } = null!;
        public string MaLoaiMon { get; set; } = null!;
        public double DonGia { get; set; }
        public string? MoTa { get; set; }
        public string? DuongDanAnh { get; set; }
        public string? TrangThai { get; set; } = "InStock";

        // Danh sách định lượng cập nhật
        public List<DinhLuongItemRequest> DinhLuong { get; set; } = new();
    }

    public class DinhLuongDetailDto
    {
        public string MaDinhLuong { get; set; } = null!;
        public string MaNguyenLieu { get; set; } = null!;
        public string TenNguyenLieu { get; set; } = null!;
        public string DonViTinh { get; set; } = null!;
        public decimal SoLuongCan { get; set; }
        public double GiaNhapGanNhat { get; set; }
        public double ThanhTienGiaVon => (double)SoLuongCan * GiaNhapGanNhat;
    }

    public class MonAnResponseDto
    {
        public string MaMon { get; set; } = null!;
        public string TenMon { get; set; } = null!;
        public string? MaLoaiMon { get; set; }
        public string? TenLoaiMon { get; set; }
        public double DonGia { get; set; }
        public string? MoTa { get; set; }
        public string? DuongDanAnh { get; set; }
        public string TrangThai { get; set; } = "InStock";

        // Giá vốn ước tính dựa trên định lượng và giá nhập gần nhất
        public double GiaVonUocTinh { get; set; }

        // Số suất ăn tối đa có thể làm từ tồn kho nguyên liệu hiện có
        public int SoSuatKhaDung { get; set; }

        public List<DinhLuongDetailDto> DinhLuong { get; set; } = new();
    }
}
