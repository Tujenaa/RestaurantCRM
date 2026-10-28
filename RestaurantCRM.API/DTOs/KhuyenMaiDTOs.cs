using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    // ================= DTOs CHO CHƯƠNG TRÌNH KHUYẾN MÃI =================
    public class ChuongTrinhCreateRequest
    {
        public string TenChuongTrinh { get; set; } = null!;
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
    }

    public class ChuongTrinhUpdateRequest
    {
        public string TenChuongTrinh { get; set; } = null!;
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
    }

    // ================= DTOs CHO VOUCHER HÓA ĐƠN =================
    public class VoucherCreateRequest
    {
        public string MaChuongTrinh { get; set; } = null!;
        public string MaVoucher { get; set; } = null!; // Mã chữ hoa như GIAM20K, CHAOHANHPHUC
        public string LoaiGiam { get; set; } = "%"; // "%" hoặc "VND"
        public double GiaTriGiam { get; set; }
        public double? GiaTriDonToiThieu { get; set; }
        public double? GiamToiDa { get; set; } // Giới hạn trần khi giảm %
        public string? PhamViApDung { get; set; } = "TOAN_DON"; // "TOAN_DON", "THEO_LOAI", "THEO_MON"
        public string? MaLoaiMon { get; set; } // Nếu phạm vi là THEO_LOAI
        public string? MaMon { get; set; } // Nếu phạm vi là THEO_MON
        public int? SoLuongToiThieu { get; set; } = 1; // Số lượng món tối thiểu
        public bool? ChoPhepCongDon { get; set; } = true;
    }

    // ================= DTOs CHO KHUYẾN MÃI MÓN ĂN (TỰ ĐỘNG) =================
    public class KmMonCreateRequest
    {
        public string MaChuongTrinh { get; set; } = null!;
        public string MaMon { get; set; } = null!;
        public string LoaiGiam { get; set; } = "%"; // "%" hoặc "VND"
        public double GiaTriGiam { get; set; }
    }

    // Legacy DTOs để không break code cũ nếu có
    public class KhuyenMaiCreateRequest
    {
        public string? TenChuongTrinh { get; set; }
        public string? LoaiKhuyenMai { get; set; } // Ví dụ: "Voucher", "GiamGiaMon"
        public string? LoaiGiam { get; set; } // "%" hoặc "VND"
        public double? GiaTriGiam { get; set; }
        public double? GiaTriDonToiThieu { get; set; }
        public double? GiamToiDa { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public int? SoLuong { get; set; }
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
