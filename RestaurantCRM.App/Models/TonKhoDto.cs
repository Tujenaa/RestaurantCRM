using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class TonKhoDto {
        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("maLoaiMon")]
        public string MaLoaiMon { get; set; }

        [JsonPropertyName("tenLoaiMon")]
        public string TenLoaiMon { get; set; }

        [JsonPropertyName("donGia")]
        public decimal DonGia { get; set; }

        [JsonPropertyName("soLuongTon")]
        public int SoLuongTon { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("canhBao")]
        public string CanhBao { get; set; } // "HetHang", "SapHet", "DuHang"

        [JsonIgnore]
        public string TrangThaiHienThi =>
            CanhBao == "HetHang" ? "Hết hàng" :
            CanhBao == "SapHet" ? "Sắp hết hàng" : "Còn hàng";
    }

    public class DieuChinhTonKhoDto {
        [JsonPropertyName("soLuongMoi")]
        public int SoLuongMoi { get; set; }

        [JsonPropertyName("ghiChu")]
        public string GhiChu { get; set; }
    }
}
