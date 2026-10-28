using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class BaoCaoDoanhThuTheoQuyDto
    {
        public int Nam { get; set; }
        public List<int> DanhSachNam { get; set; } = new List<int>();

        // KPI tổng quan năm
        public double TongThuNam { get; set; }
        public double TongChiNam { get; set; }
        public double LoiNhuanNam { get; set; }
        public double TyLeLoiNhuanNam { get; set; }
        public int TongDonNam { get; set; }
        public string NhanVienXuatSac { get; set; } = "Chưa có";
        public double DoanhThuNhanVienXuatSac { get; set; }
        public string MonBanChayNhat { get; set; } = "Chưa có";
        public double DoanhThuMonBanChayNhat { get; set; }

        // Bảng 1: Bảng tổng thu, tổng chi, lợi nhuận theo Quý
        public List<DongTaiChinhQuyDto> BangTaiChinhQuy { get; set; } = new List<DongTaiChinhQuyDto>();

        // Bảng 2: Bảng doanh thu theo Nhân viên
        public List<DongNhanVienQuyDto> BangNhanVien { get; set; } = new List<DongNhanVienQuyDto>();

        // Bảng 3: Bảng doanh thu theo Món ăn
        public List<DongMonAnDoanhThuDto> BangMonAn { get; set; } = new List<DongMonAnDoanhThuDto>();
        public List<int> CacNamSoSanhMonAn { get; set; } = new List<int>();
    }

    public class DongTaiChinhQuyDto
    {
        public string ChiTieu { get; set; } = "";
        public double Quy1 { get; set; }
        public double Quy2 { get; set; }
        public double Quy3 { get; set; }
        public double Quy4 { get; set; }
        public double TongNam { get; set; }
        public string DinhDang { get; set; } = "Tien"; // "Tien" hoặc "PhanTram"
    }

    public class DongNhanVienQuyDto
    {
        public int Stt { get; set; }
        public string MaNhanVien { get; set; } = "";
        public string TenNhanVien { get; set; } = "";
        public string ChucVu { get; set; } = "";
        public double Quy1 { get; set; }
        public double Quy2 { get; set; }
        public double Quy3 { get; set; }
        public double Quy4 { get; set; }
        public double TongNam { get; set; }
        public double TyLeDongGop { get; set; }
    }

    public class DongMonAnDoanhThuDto
    {
        public int Stt { get; set; }
        public string MaMon { get; set; } = "";
        public string TenMon { get; set; } = "";
        public string DanhMuc { get; set; } = "";
        public int TongSoLuongBan { get; set; }

        // Dữ liệu theo Quý của năm chọn
        public double Quy1 { get; set; }
        public double Quy2 { get; set; }
        public double Quy3 { get; set; }
        public double Quy4 { get; set; }

        // Dữ liệu theo 3 năm gần nhất (ví dụ 2024, 2025, 2026)
        public double DoanhThuNam1 { get; set; } // Nam - 2
        public double DoanhThuNam2 { get; set; } // Nam - 1
        public double DoanhThuNam3 { get; set; } // Nam hiện tại

        public double TongDoanhThu { get; set; }
        public double TyLeDongGop { get; set; }
    }
}
