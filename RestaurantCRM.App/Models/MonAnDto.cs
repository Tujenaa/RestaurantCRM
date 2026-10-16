using System.Collections.Generic;
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

        [JsonPropertyName("tenLoaiMon")]
        public string TenLoaiMon { get; set; }

        [JsonPropertyName("moTa")]
        public string MoTa { get; set; }
        
        [JsonIgnore]
        public bool TrangThai {
            get => TrangThaiRaw != "Discontinued" && TrangThaiRaw != "Inactive";
            set => TrangThaiRaw = value ? (SoSuatKhaDung > 0 ? "InStock" : "OutOfStock") : "Discontinued";
        }

        [JsonPropertyName("trangThai")]
        public string TrangThaiRaw { get; set; }
        
        [JsonPropertyName("duongDanAnh")]
        public string HinhAnhUrl { get; set; }
        
        // Tương thích ngược với UI cũ: SoLuongTon đại diện cho số suất khả dụng
        [JsonPropertyName("soLuong")]
        public int SoLuongTon { get; set; }

        [JsonPropertyName("soSuatKhaDung")]
        public int SoSuatKhaDung { get; set; }

        [JsonPropertyName("giaVonUocTinh")]
        public decimal GiaVonUocTinh { get; set; }

        [JsonPropertyName("dinhLuong")]
        public List<DinhLuongItemDto> DinhLuong { get; set; } = new List<DinhLuongItemDto>();
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
