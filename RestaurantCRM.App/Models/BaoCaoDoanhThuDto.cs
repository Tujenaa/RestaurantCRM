using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class BaoCaoDoanhThuDto {
        [JsonPropertyName("tuNgay")]
        public string TuNgay { get; set; }

        [JsonPropertyName("denNgay")]
        public string DenNgay { get; set; }

        [JsonPropertyName("tongDoanhThu")]
        public double TongDoanhThu { get; set; }

        [JsonPropertyName("tongTienHangGoc")]
        public double TongTienHangGoc { get; set; }

        [JsonPropertyName("tongTienGiamMon")]
        public double TongTienGiamMon { get; set; }

        [JsonPropertyName("tongTienGiamVoucher")]
        public double TongTienGiamVoucher { get; set; }

        [JsonPropertyName("tongGiamGia")]
        public double TongGiamGia { get; set; }

        [JsonPropertyName("tongGiaVon")]
        public double TongGiaVon { get; set; }

        [JsonPropertyName("loiNhuanGop")]
        public double LoiNhuanGop { get; set; }

        [JsonPropertyName("tyLeLoiNhuan")]
        public double TyLeLoiNhuan { get; set; }

        [JsonPropertyName("tongDonHang")]
        public int TongDonHang { get; set; }

        [JsonPropertyName("donHoanThanh")]
        public int DonHoanThanh { get; set; }

        [JsonPropertyName("donHuy")]
        public int DonHuy { get; set; }

        [JsonPropertyName("donDangXuLy")]
        public int DonDangXuLy { get; set; }

        [JsonPropertyName("giaTriDonTb")]
        public double GiaTriDonTb { get; set; }

        [JsonPropertyName("doanhThuTheoNgay")]
        public List<DoanhThuNgayItemDto> DoanhThuTheoNgay { get; set; } = new List<DoanhThuNgayItemDto>();

        [JsonPropertyName("topMonBanChay")]
        public List<TopMonBanChayDto> TopMonBanChay { get; set; } = new List<TopMonBanChayDto>();
    }

    public class DoanhThuNgayItemDto {
        [JsonPropertyName("ngay")]
        public string Ngay { get; set; }

        [JsonPropertyName("ngayHienThi")]
        public string NgayHienThi { get; set; }

        [JsonPropertyName("thu")]
        public string Thu { get; set; }

        [JsonPropertyName("doanhThu")]
        public double DoanhThu { get; set; }

        [JsonPropertyName("soDon")]
        public int SoDon { get; set; }
    }

    public class TopMonBanChayDto {
        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("soLuong")]
        public int SoLuong { get; set; }

        [JsonPropertyName("doanhThu")]
        public double DoanhThu { get; set; }

        [JsonPropertyName("giaVon")]
        public double GiaVon { get; set; }

        [JsonPropertyName("loiNhuan")]
        public double LoiNhuan { get; set; }

        [JsonPropertyName("tyLeLai")]
        public double TyLeLai { get; set; }
    }
}
