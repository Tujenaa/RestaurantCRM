using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Models
{
    public class NhomDoTuoiItemDto
    {
        public string TenNhom { get; set; }
        public string MoTa { get; set; }
        public int SoLuong { get; set; }
        public double TyLe { get; set; } // %
    }

    public class GioiTinhItemDto
    {
        public string TenGioiTinh { get; set; }
        public int SoLuong { get; set; }
        public double TyLe { get; set; } // %
    }

    public class MonUaThichItemDto
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public string TenLoaiMon { get; set; }
        public int SoLuongGoi { get; set; }
        public double DoanhThu { get; set; }
        public double TyLeUaThich { get; set; } // %
    }

    public class PhanTichKhachHangDto
    {
        public int TongKhachHang { get; set; }
        public int KhachHangCoNgaySinh { get; set; }
        public double DoTuoiTrungBinh { get; set; }
        public List<NhomDoTuoiItemDto> DoTuoi { get; set; } = new List<NhomDoTuoiItemDto>();
        public List<GioiTinhItemDto> GioiTinh { get; set; } = new List<GioiTinhItemDto>();
        public List<MonUaThichItemDto> TopMonUaThich { get; set; } = new List<MonUaThichItemDto>();
    }
}
