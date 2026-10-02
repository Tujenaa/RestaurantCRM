using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class KhuyenMaiCreateRequest
    {
        public string? TenChuongTrinh { get; set; }
        public string? LoaiKhuyenMai { get; set; } // Ví dụ: "Voucher", "GiamGiaMon"
        public string? LoaiGiam { get; set; } // "%" hoặc "VND"
        public double? GiaTriGiam { get; set; }
        public double? GiaTriDonToiThieu { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public int? SoLuong { get; set; }
        
        // Danh sách các món ăn được áp dụng (Nếu có)
        public List<KhuyenMaiMonRequest>? DanhSachMon { get; set; }
    }

    public class KhuyenMaiMonRequest
    {
        public string MaMon { get; set; } = null!;
        public int? SoLuongApDung { get; set; }
    }

    public class KhuyenMaiStatusRequest
    {
        public string Status { get; set; } = null!;
    }
}
