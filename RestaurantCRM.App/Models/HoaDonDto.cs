using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class HoaDonDto {
        [JsonPropertyName("maHoaDon")]
        public string MaHoaDon { get; set; }

        [JsonPropertyName("ngayDat")]
        public DateTime? NgayDat { get; set; }

        [JsonPropertyName("tenKhachHang")]
        public string TenKhachHang { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("tongTienHang")]
        public double? TongTienHang { get; set; }

        [JsonPropertyName("tongThanhToan")]
        public double? TongThanhToan { get; set; }

        [JsonPropertyName("maNhanVien")]
        public string MaNhanVien { get; set; }

        [JsonPropertyName("tenNhanVien")]
        public string TenNhanVien { get; set; }

        // Helper cho hiển thị DataGridView
        public string NgayDatHienThi => NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        public string TongTienHienThi => (TongThanhToan ?? 0).ToString("N0") + " đ";
    }

    public class ChiTietHoaDonDto {
        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("soLuong")]
        public int SoLuong { get; set; }

        [JsonPropertyName("donGiaSauGiam")]
        public double? DonGiaSauGiam { get; set; }

        [JsonPropertyName("thanhTien")]
        public double? ThanhTien { get; set; }

        public string DonGiaHienThi => (DonGiaSauGiam ?? 0).ToString("N0") + " đ";
        public string ThanhTienHienThi => (ThanhTien ?? 0).ToString("N0") + " đ";
    }

    public class KhachHangOrderDto {
        [JsonPropertyName("hoTen")]
        public string HoTen { get; set; }

        [JsonPropertyName("soDienThoai")]
        public string SoDienThoai { get; set; }
    }

    public class ThanhToanOrderDto {
        [JsonPropertyName("phuongThuc")]
        public string PhuongThuc { get; set; }

        [JsonPropertyName("ngayThanhToan")]
        public DateTime? NgayThanhToan { get; set; }

        [JsonPropertyName("soTien")]
        public double? SoTien { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }
    }

    public class HoaDonDetailDto {
        [JsonPropertyName("maHoaDon")]
        public string MaHoaDon { get; set; }

        [JsonPropertyName("ngayDat")]
        public DateTime? NgayDat { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("diaChiGiao")]
        public string DiaChiGiao { get; set; }

        [JsonPropertyName("tongTienHang")]
        public double? TongTienHang { get; set; }

        [JsonPropertyName("tienGiamVoucher")]
        public double? TienGiamVoucher { get; set; }

        [JsonPropertyName("tongThanhToan")]
        public double? TongThanhToan { get; set; }

        [JsonPropertyName("khachHang")]
        public KhachHangOrderDto KhachHang { get; set; }

        [JsonPropertyName("nhanVienPhuTrach")]
        public string NhanVienPhuTrach { get; set; }

        [JsonPropertyName("khuyenMai")]
        public string KhuyenMai { get; set; }

        [JsonPropertyName("thanhToan")]
        public List<ThanhToanOrderDto> ThanhToan { get; set; } = new List<ThanhToanOrderDto>();

        [JsonPropertyName("chiTiet")]
        public List<ChiTietHoaDonDto> ChiTiet { get; set; } = new List<ChiTietHoaDonDto>();
    }

    public class LichSuTrangThaiDto {
        [JsonPropertyName("maLichSu")]
        public string MaLichSu { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("thoiGian")]
        public DateTime? ThoiGian { get; set; }

        [JsonPropertyName("ghiChu")]
        public string GhiChu { get; set; }

        public string ThoiGianHienThi => ThoiGian?.ToString("dd/MM/yyyy HH:mm:ss") ?? "-";
    }

    public class UpdateOrderStatusRequestDto {
        [JsonPropertyName("trangThaiMoi")]
        public string TrangThaiMoi { get; set; }

        [JsonPropertyName("ghiChu")]
        public string GhiChu { get; set; }

        [JsonPropertyName("maNhanVien")]
        public string MaNhanVien { get; set; }
    }
}
