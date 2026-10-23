using System;
using System.Collections.Generic;
using System.Globalization;

namespace RestaurantCRM.AdminApp.DTOs.KhachHang
{
    /// <summary>Một dòng khách hàng trả về từ GET api/AdminKhachHang.</summary>
    public class KhachHangDto
    {
        public string MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        /// <summary>API trả DateOnly dạng "yyyy-MM-dd".</summary>
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string TrangThai { get; set; }
        public int SoDonHang { get; set; }
        public double TongChiTieu { get; set; }

        public bool BiKhoa { get { return string.Equals(TrangThai, "Locked", StringComparison.OrdinalIgnoreCase); } }
        public string TrangThaiHienThi { get { return BiKhoa ? "Đã khóa" : "Hoạt động"; } }

        public DateTime? NgaySinhDate
        {
            get
            {
                DateTime d;
                if (!string.IsNullOrWhiteSpace(NgaySinh) &&
                    DateTime.TryParse(NgaySinh, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
                return null;
            }
        }
        public string NgaySinhHienThi { get { var d = NgaySinhDate; return d.HasValue ? d.Value.ToString("dd/MM/yyyy") : ""; } }
    }

    /// <summary>Chi tiết khách hàng (GET api/AdminKhachHang/{id}).</summary>
    public class KhachHangChiTietDto : KhachHangDto
    {
        public int SoDanhGia { get; set; }
        public int SoKhaoSatDaTraLoi { get; set; }
        public List<KhachHangHoaDonDto> LichSuHoaDon { get; set; } = new List<KhachHangHoaDonDto>();
    }

    public class KhachHangHoaDonDto
    {
        public string MaHoaDon { get; set; }
        public DateTime? NgayDat { get; set; }
        public double? TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    /// <summary>Dữ liệu gửi lên khi thêm / sửa khách hàng.</summary>
    public class KhachHangRequest
    {
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        /// <summary>"yyyy-MM-dd" hoặc null.</summary>
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string TrangThai { get; set; }
    }
}