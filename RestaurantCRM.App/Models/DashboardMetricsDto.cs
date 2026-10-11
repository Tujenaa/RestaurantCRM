namespace RestaurantCRM.AdminApp.Models {
    public class DashboardMetricsDto {
        // Phân hệ Admin
        public int SoMonAn { get; set; }
        public int SoNhaCungCap { get; set; }
        public int SoPhieuNhapThangNay { get; set; }
        public int SoTaiKhoan { get; set; }

        // Phân hệ Bán hàng
        public double DoanhThuHomNay { get; set; }
        public int DonChoXuLy { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanhHomNay { get; set; }

        // Phân hệ CRM
        public int TongKhachHang { get; set; }
        public double DiemDanhGiaTb { get; set; }
        public int PhanHoiChuaXuLy { get; set; }
        public int KhaoSatDangMo { get; set; }
    }
}
