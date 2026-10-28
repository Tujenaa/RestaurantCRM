using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class NhaCungCapDto {
        [JsonPropertyName("maNhaCungCap")]
        public string MaNhaCungCap { get; set; }

        [JsonPropertyName("tenNhaCungCap")]
        public string TenNhaCungCap { get; set; }

        [JsonPropertyName("soDienThoai")]
        public string SoDienThoai { get; set; }

        [JsonPropertyName("diaChi")]
        public string DiaChi { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonIgnore]
        public bool IsActive => !string.Equals(TrangThai, "Hidden", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase);

        [JsonIgnore]
        public string TrangThaiHienThi => IsActive ? "🟢 Hoạt động" : "⚪ Đã ẩn";
    }

    public class PhieuNhapDto {
        [JsonPropertyName("maPhieuNhap")]
        public string MaPhieuNhap { get; set; }

        [JsonPropertyName("maNhaCungCap")]
        public string MaNhaCungCap { get; set; }

        [JsonPropertyName("tenNhaCungCap")]
        public string TenNhaCungCap { get; set; }

        [JsonPropertyName("maNhanVien")]
        public string MaNhanVien { get; set; }

        [JsonPropertyName("tenNhanVien")]
        public string TenNhanVien { get; set; }

        [JsonIgnore]
        public string NhanVienHienThi {
            get {
                if (!string.IsNullOrWhiteSpace(TenNhanVien) && !string.IsNullOrWhiteSpace(MaNhanVien)) {
                    return $"{TenNhanVien} ({MaNhanVien})";
                }
                if (!string.IsNullOrWhiteSpace(TenNhanVien)) {
                    return TenNhanVien;
                }
                if (!string.IsNullOrWhiteSpace(MaNhanVien)) {
                    return MaNhanVien;
                }
                return "-";
            }
        }

        [JsonPropertyName("ngayNhap")]
        public DateTime? NgayNhap { get; set; }

        [JsonPropertyName("tongTien")]
        public decimal? TongTien { get; set; }

        [JsonPropertyName("ghiChu")]
        public string GhiChu { get; set; }

        [JsonPropertyName("chiTiet")]
        public List<ChiTietPhieuNhapDto> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhapDto>();
    }

    public class ChiTietPhieuNhapDto {
        [JsonPropertyName("maChiTiet")]
        public string MaChiTiet { get; set; }

        [JsonPropertyName("maPhieuNhap")]
        public string MaPhieuNhap { get; set; }

        [JsonPropertyName("maNguyenLieu")]
        public string MaNguyenLieu { get; set; }

        [JsonPropertyName("tenNguyenLieu")]
        public string TenNguyenLieu { get; set; }

        [JsonPropertyName("donViTinh")]
        public string DonViTinh { get; set; }

        // Tương thích ngược
        [JsonPropertyName("maMon")]
        public string MaMon {
            get => MaNguyenLieu;
            set => MaNguyenLieu = value;
        }

        [JsonIgnore]
        public string MaMonAn {
            get => MaNguyenLieu;
            set => MaNguyenLieu = value;
        }

        [JsonPropertyName("tenMon")]
        public string TenMon {
            get => TenNguyenLieu;
            set => TenNguyenLieu = value;
        }

        [JsonIgnore]
        public string TenMonAn {
            get => TenNguyenLieu;
            set => TenNguyenLieu = value;
        }

        [JsonPropertyName("soLuong")]
        public decimal SoLuong { get; set; }

        [JsonPropertyName("donGiaNhap")]
        public decimal DonGiaNhap { get; set; }

        [JsonPropertyName("thanhTien")]
        public decimal ThanhTien => SoLuong * DonGiaNhap;
    }

    public class PhieuNhapCreateDto {
        [JsonPropertyName("maNhaCungCap")]
        public string MaNhaCungCap { get; set; }

        [JsonPropertyName("maNhanVien")]
        public string MaNhanVien { get; set; }

        [JsonPropertyName("ghiChu")]
        public string GhiChu { get; set; }

        [JsonPropertyName("chiTiet")]
        public List<ChiTietPhieuNhapCreateDto> ChiTiet { get; set; } = new List<ChiTietPhieuNhapCreateDto>();
    }

    public class ChiTietPhieuNhapCreateDto {
        [JsonPropertyName("maNguyenLieu")]
        public string MaNguyenLieu { get; set; }

        // Tương thích ngược
        [JsonPropertyName("maMon")]
        public string MaMon {
            get => MaNguyenLieu;
            set => MaNguyenLieu = value;
        }

        [JsonPropertyName("soLuong")]
        public decimal SoLuong { get; set; }

        [JsonPropertyName("donGiaNhap")]
        public double DonGiaNhap { get; set; }
    }
}
