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
        /// <summary>Tổng số đơn hoàn tất (mẫu số của "Tỷ lệ đơn" ở tab Món hay gọi).</summary>
        public int SoDonHoanTat { get; set; }
        public List<KhachHangHoaDonDto> LichSuHoaDon { get; set; } = new List<KhachHangHoaDonDto>();
        public List<KhachHangDanhGiaDto> LichSuDanhGia { get; set; } = new List<KhachHangDanhGiaDto>();
        public List<KhachHangKhaoSatDto> LichSuKhaoSat { get; set; } = new List<KhachHangKhaoSatDto>();
        public List<KhachHangMonUaThichDto> MonUaThich { get; set; } = new List<KhachHangMonUaThichDto>();
    }

    public class KhachHangHoaDonDto
    {
        public string MaHoaDon { get; set; }
        public DateTime? NgayDat { get; set; }
        public double? TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    public class KhachHangDanhGiaDto
    {
        public string MaDanhGia { get; set; }
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public int? SoSao { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        public bool HienThi { get; set; }
    }

    public class KhachHangKhaoSatDto
    {
        public string MaPhieuTraLoi { get; set; }
        public string MaKhaoSat { get; set; }
        public string TenKhaoSat { get; set; }
        public DateTime? NgayTraLoi { get; set; }
    }

    public class KhachHangMonUaThichDto
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public string TenLoaiMon { get; set; }
        /// <summary>Số đơn hoàn tất có món này (nhiều phần trong 1 đơn vẫn tính 1 lần).</summary>
        public int SoLanGoi { get; set; }
        /// <summary>% = SoLanGoi / tổng số đơn hoàn tất của khách.</summary>
        public double TyLeDon { get; set; }
        /// <summary>Tổng số phần đã mua (tham khảo).</summary>
        public int SoLuongDaMua { get; set; }
        public DateTime? LanGanNhat { get; set; }
        /// <summary>"Hay gọi" | "Đã thử".</summary>
        public string MucDo { get; set; }
        /// <summary>Món nằm trong danh sách khách tự khai yêu thích (♥).</summary>
        public bool YeuThich { get; set; }

        public bool HayGoi { get { return MucDo == "Hay gọi"; } }
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
