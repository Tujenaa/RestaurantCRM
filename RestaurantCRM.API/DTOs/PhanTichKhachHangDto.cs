using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class NhomDoTuoiDto
    {
        public string TenNhom { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLe { get; set; } // Phần trăm (%)
    }

    public class GioiTinhStatDto
    {
        public string TenGioiTinh { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLe { get; set; } // Phần trăm (%)
    }

    public class MonUaThichDto
    {
        public string MaMon { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public string TenLoaiMon { get; set; } = string.Empty;
        public int SoLuongGoi { get; set; }
        public double DoanhThu { get; set; }
        public double TyLeUaThich { get; set; } // % trên tổng số món đã gọi
    }

    public class PhanTichKhachHangResponse
    {
        public int TongKhachHang { get; set; }
        public int KhachHangCoNgaySinh { get; set; }
        public double DoTuoiTrungBinh { get; set; }
        public List<NhomDoTuoiDto> DoTuoi { get; set; } = new();
        public List<GioiTinhStatDto> GioiTinh { get; set; } = new();
        public List<MonUaThichDto> TopMonUaThich { get; set; } = new();
    }
}
