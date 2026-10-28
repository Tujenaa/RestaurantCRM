using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models
{
    public class BaoCaoDoanhThuTheoQuyDto
    {
        [JsonPropertyName("nam")]
        public int Nam { get; set; }

        [JsonPropertyName("danhSachNam")]
        public List<int> DanhSachNam { get; set; } = new List<int>();

        [JsonPropertyName("tongThuNam")]
        public double TongThuNam { get; set; }

        [JsonPropertyName("tongChiNam")]
        public double TongChiNam { get; set; }

        [JsonPropertyName("loiNhuanNam")]
        public double LoiNhuanNam { get; set; }

        [JsonPropertyName("tyLeLoiNhuanNam")]
        public double TyLeLoiNhuanNam { get; set; }

        [JsonPropertyName("tongDonNam")]
        public int TongDonNam { get; set; }

        [JsonPropertyName("nhanVienXuatSac")]
        public string NhanVienXuatSac { get; set; }

        [JsonPropertyName("doanhThuNhanVienXuatSac")]
        public double DoanhThuNhanVienXuatSac { get; set; }

        [JsonPropertyName("monBanChayNhat")]
        public string MonBanChayNhat { get; set; }

        [JsonPropertyName("doanhThuMonBanChayNhat")]
        public double DoanhThuMonBanChayNhat { get; set; }

        [JsonPropertyName("bangTaiChinhQuy")]
        public List<DongTaiChinhQuyDto> BangTaiChinhQuy { get; set; } = new List<DongTaiChinhQuyDto>();

        [JsonPropertyName("bangNhanVien")]
        public List<DongNhanVienQuyDto> BangNhanVien { get; set; } = new List<DongNhanVienQuyDto>();

        [JsonPropertyName("bangMonAn")]
        public List<DongMonAnDoanhThuDto> BangMonAn { get; set; } = new List<DongMonAnDoanhThuDto>();

        [JsonPropertyName("cacNamSoSanhMonAn")]
        public List<int> CacNamSoSanhMonAn { get; set; } = new List<int>();
    }

    public class DongTaiChinhQuyDto
    {
        [JsonPropertyName("chiTieu")]
        public string ChiTieu { get; set; }

        [JsonPropertyName("quy1")]
        public double Quy1 { get; set; }

        [JsonPropertyName("quy2")]
        public double Quy2 { get; set; }

        [JsonPropertyName("quy3")]
        public double Quy3 { get; set; }

        [JsonPropertyName("quy4")]
        public double Quy4 { get; set; }

        [JsonPropertyName("tongNam")]
        public double TongNam { get; set; }

        [JsonPropertyName("dinhDang")]
        public string DinhDang { get; set; } // "Tien" hoặc "PhanTram"
    }

    public class DongNhanVienQuyDto
    {
        [JsonPropertyName("stt")]
        public int Stt { get; set; }

        [JsonPropertyName("maNhanVien")]
        public string MaNhanVien { get; set; }

        [JsonPropertyName("tenNhanVien")]
        public string TenNhanVien { get; set; }

        [JsonPropertyName("chucVu")]
        public string ChucVu { get; set; }

        [JsonPropertyName("quy1")]
        public double Quy1 { get; set; }

        [JsonPropertyName("quy2")]
        public double Quy2 { get; set; }

        [JsonPropertyName("quy3")]
        public double Quy3 { get; set; }

        [JsonPropertyName("quy4")]
        public double Quy4 { get; set; }

        [JsonPropertyName("tongNam")]
        public double TongNam { get; set; }

        [JsonPropertyName("tyLeDongGop")]
        public double TyLeDongGop { get; set; }
    }

    public class DongMonAnDoanhThuDto
    {
        [JsonPropertyName("stt")]
        public int Stt { get; set; }

        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("danhMuc")]
        public string DanhMuc { get; set; }

        [JsonPropertyName("tongSoLuongBan")]
        public int TongSoLuongBan { get; set; }

        [JsonPropertyName("quy1")]
        public double Quy1 { get; set; }

        [JsonPropertyName("quy2")]
        public double Quy2 { get; set; }

        [JsonPropertyName("quy3")]
        public double Quy3 { get; set; }

        [JsonPropertyName("quy4")]
        public double Quy4 { get; set; }

        [JsonPropertyName("doanhThuNam1")]
        public double DoanhThuNam1 { get; set; }

        [JsonPropertyName("doanhThuNam2")]
        public double DoanhThuNam2 { get; set; }

        [JsonPropertyName("doanhThuNam3")]
        public double DoanhThuNam3 { get; set; }

        [JsonPropertyName("tongDoanhThu")]
        public double TongDoanhThu { get; set; }

        [JsonPropertyName("tyLeDongGop")]
        public double TyLeDongGop { get; set; }
    }
}
