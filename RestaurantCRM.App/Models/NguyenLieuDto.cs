using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class NguyenLieuDto {
        [JsonPropertyName("maNguyenLieu")]
        public string MaNguyenLieu { get; set; }

        [JsonPropertyName("tenNguyenLieu")]
        public string TenNguyenLieu { get; set; }

        [JsonPropertyName("donViTinh")]
        public string DonViTinh { get; set; }

        [JsonPropertyName("soLuongTon")]
        public decimal SoLuongTon { get; set; }

        [JsonPropertyName("giaNhapGanNhat")]
        public decimal GiaNhapGanNhat { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonIgnore]
        public string DisplayText => $"{TenNguyenLieu} ({DonViTinh})";
    }

    public class DinhLuongItemDto {
        [JsonPropertyName("maDinhLuong")]
        public string MaDinhLuong { get; set; }

        [JsonPropertyName("maNguyenLieu")]
        public string MaNguyenLieu { get; set; }

        [JsonPropertyName("tenNguyenLieu")]
        public string TenNguyenLieu { get; set; }

        [JsonPropertyName("donViTinh")]
        public string DonViTinh { get; set; }

        [JsonPropertyName("soLuongCan")]
        public decimal SoLuongCan { get; set; }

        [JsonPropertyName("giaNhapGanNhat")]
        public decimal GiaNhapGanNhat { get; set; }

        [JsonPropertyName("thanhTienGiaVon")]
        public decimal ThanhTienGiaVon { get; set; }
    }
}
