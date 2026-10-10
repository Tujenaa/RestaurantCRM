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

        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonIgnore]
        public string MaMonAn {
            get => MaMon;
            set => MaMon = value;
        }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonIgnore]
        public string TenMonAn {
            get => TenMon;
            set => TenMon = value;
        }

        [JsonPropertyName("soLuong")]
        public int SoLuong { get; set; }

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
        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("soLuong")]
        public int SoLuong { get; set; }

        [JsonPropertyName("donGiaNhap")]
        public double DonGiaNhap { get; set; }
    }
}
