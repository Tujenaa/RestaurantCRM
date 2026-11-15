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
}
