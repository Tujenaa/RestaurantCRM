namespace RestaurantCRM.AdminApp.Models {
    public class DashboardMetricsDto {
        // Phân hệ Hệ thống
        public int SoMonAn { get; set; }
        public int SoNhaCungCap { get; set; }
        public int SoPhieuNhapThangNay { get; set; }
        public int SoTaiKhoan { get; set; }

        // Phân hệ Vận Hành
        public double DoanhThuHomNay { get; set; }
        public int DonChoXuLy { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanhHomNay { get; set; }

        // Phân hệ Khách hàng
        public int TongKhachHang { get; set; }
        public double DiemDanhGiaTb { get; set; }
        public int TongDanhGia { get; set; }
        /// <summary>Số đánh giá khách sửa sau lần trả lời gần nhất của nhà hàng.</summary>
        public int SoDanhGiaSuaSauPhanHoi { get; set; }
        public int KhaoSatDangMo { get; set; }
    }
}
