using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class MonAnDto {
        [JsonPropertyName("maMon")]
        public string MaMonAn { get; set; }
        
        [JsonPropertyName("tenMon")]
        public string TenMonAn { get; set; }
        
        [JsonPropertyName("donGia")]
        public decimal Gia { get; set; }
        
        [JsonPropertyName("maLoaiMon")]
        public string MaLoaiMon { get; set; }
        
        [JsonIgnore]
        public bool TrangThai {
            get => TrangThaiRaw != "Discontinued" && TrangThaiRaw != "Inactive";
            set => TrangThaiRaw = value ? (SoLuongTon > 0 ? "InStock" : "OutOfStock") : "Discontinued";
        }

        [JsonPropertyName("trangThai")]
        public string TrangThaiRaw { get; set; }
        
        [JsonPropertyName("duongDanAnh")]
        public string HinhAnhUrl { get; set; }
        
        [JsonPropertyName("soLuong")]
        public int SoLuongTon { get; set; }
    }

    public class LoaiMonDto {
        [JsonPropertyName("maLoaiMon")]
        public string MaLoaiMon { get; set; }
        
        [JsonPropertyName("tenLoaiMon")]
        public string TenLoaiMon { get; set; }

        [JsonIgnore]
        public int SoLuongMon { get; set; }
    }
}
