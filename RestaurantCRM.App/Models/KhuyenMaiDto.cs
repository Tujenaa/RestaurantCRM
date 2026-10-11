using System;
using System.Text.Json.Serialization;

namespace RestaurantCRM.AdminApp.Models {
    public class ChuongTrinhDto {
        [JsonPropertyName("maChuongTrinh")]
        public string MaChuongTrinh { get; set; }

        [JsonPropertyName("tenChuongTrinh")]
        public string TenChuongTrinh { get; set; }

        [JsonPropertyName("ngayBatDau")]
        public string NgayBatDau { get; set; }

        [JsonPropertyName("ngayKetThuc")]
        public string NgayKetThuc { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("soLuongVoucher")]
        public int SoLuongVoucher { get; set; }

        [JsonPropertyName("soLuongMonKm")]
        public int SoLuongMonKm { get; set; }
    }

    public class VoucherDto {
        [JsonPropertyName("maKmvoucher")]
        public string MaKmvoucher { get; set; }

        [JsonPropertyName("maVoucher")]
        public string MaVoucher { get; set; }

        [JsonPropertyName("maChuongTrinh")]
        public string MaChuongTrinh { get; set; }

        [JsonPropertyName("tenChuongTrinh")]
        public string TenChuongTrinh { get; set; }

        [JsonPropertyName("ngayBatDau")]
        public string NgayBatDau { get; set; }

        [JsonPropertyName("ngayKetThuc")]
        public string NgayKetThuc { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("phanTramGiam")]
        public double? PhanTramGiam { get; set; }

        [JsonPropertyName("tienGiam")]
        public double? TienGiam { get; set; }

        [JsonPropertyName("giaTriDonToiThieu")]
        public double? GiaTriDonToiThieu { get; set; }

        [JsonPropertyName("giamToiDa")]
        public double? GiamToiDa { get; set; }

        [JsonPropertyName("hinhThuc")]
        public string HinhThuc { get; set; }
    }

    public class KmMonDto {
        [JsonPropertyName("maKmsp")]
        public string MaKmsp { get; set; }

        [JsonPropertyName("maChuongTrinh")]
        public string MaChuongTrinh { get; set; }

        [JsonPropertyName("tenChuongTrinh")]
        public string TenChuongTrinh { get; set; }

        [JsonPropertyName("ngayBatDau")]
        public string NgayBatDau { get; set; }

        [JsonPropertyName("ngayKetThuc")]
        public string NgayKetThuc { get; set; }

        [JsonPropertyName("trangThai")]
        public string TrangThai { get; set; }

        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("donGiaGoc")]
        public double DonGiaGoc { get; set; }

        [JsonPropertyName("phanTramGiam")]
        public double? PhanTramGiam { get; set; }

        [JsonPropertyName("tienGiam")]
        public double? TienGiam { get; set; }

        [JsonPropertyName("hinhThuc")]
        public string HinhThuc { get; set; }

        [JsonPropertyName("donGiaSauGiam")]
        public double DonGiaSauGiam { get; set; }
    }

    public class MonAnSelectDto {
        [JsonPropertyName("maMon")]
        public string MaMon { get; set; }

        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }

        [JsonPropertyName("donGia")]
        public double? DonGia { get; set; }

        [JsonPropertyName("maLoaiMon")]
        public string MaLoaiMon { get; set; }

        public string DisplayText => $"[{MaMon}] {TenMon} - ({DonGia:N0} đ)";
    }
}
