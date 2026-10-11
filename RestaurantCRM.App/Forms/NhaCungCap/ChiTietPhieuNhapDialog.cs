using System;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap {
    public partial class ChiTietPhieuNhapDialog : Form {
        public ChiTietPhieuNhapDialog(PhieuNhapDto phieuNhap) {
            InitializeComponent();
            if (phieuNhap != null) {
                lblTieuDe.Text = $"Chi Tiết Phiếu Nhập: {phieuNhap.MaPhieuNhap}";
                lblNhaCungCap.Text = $"Nhà cung cấp: {phieuNhap.TenNhaCungCap ?? "Chưa rõ"}";
                lblNhanVien.Text = $"Nhân viên lập: {phieuNhap.TenNhanVien ?? "Admin"}";
                lblNgayNhap.Text = $"Ngày nhập: {(phieuNhap.NgayNhap.HasValue ? phieuNhap.NgayNhap.Value.ToString("dd/MM/yyyy HH:mm") : "-")}";
                lblGhiChu.Text = $"Ghi chú: {phieuNhap.GhiChu ?? "Không có"}";
                lblTongTien.Text = $"TỔNG GIÁ TRỊ: {(phieuNhap.TongTien.HasValue ? phieuNhap.TongTien.Value.ToString("N0") : "0")} VNĐ";

                dgvChiTiet.DataSource = phieuNhap.ChiTietPhieuNhaps;
            }
        }

        private void btnDong_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
