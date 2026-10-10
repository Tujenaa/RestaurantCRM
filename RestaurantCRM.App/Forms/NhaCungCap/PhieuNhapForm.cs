using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap {
    public partial class PhieuNhapForm : Form {
        private readonly PhieuNhapService _pnService;
        private List<PhieuNhapDto> _allPhieuNhap = new List<PhieuNhapDto>();

        public PhieuNhapForm() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _pnService = new PhieuNhapService(httpClient);
        }

        private async void PhieuNhapForm_Load(object sender, EventArgs e) {
            await LoadData();
        }

        private async Task LoadData() {
            try {
                _allPhieuNhap = await _pnService.GetAllAsync();
                FilterData();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải danh sách phiếu nhập: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData() {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            var filtered = _allPhieuNhap.Where(p =>
                string.IsNullOrEmpty(keyword) ||
                (!string.IsNullOrEmpty(p.MaPhieuNhap) && p.MaPhieuNhap.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(p.TenNhaCungCap) && p.TenNhaCungCap.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(p.TenNhanVien) && p.TenNhanVien.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(p.GhiChu) && p.GhiChu.ToLower().Contains(keyword))
            ).ToList();

            dgvPhieuNhap.DataSource = null;
            dgvPhieuNhap.DataSource = filtered;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            txtTimKiem.Clear();
            await LoadData();
        }

        private async void btnTaoPhieuNhap_Click(object sender, EventArgs e) {
            using (var dialog = new PhieuNhapDialog()) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadData();
                }
            }
        }

        private async void btnXemChiTiet_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một phiếu nhập để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try {
                var detail = await _pnService.GetByIdAsync(selected.MaPhieuNhap);
                if (detail != null) {
                    using (var dialog = new ChiTietPhieuNhapDialog(detail)) {
                        dialog.ShowDialog(this);
                    }
                } else {
                    MessageBox.Show("Không tìm thấy thông tin chi tiết của phiếu nhập này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPhieuNhap_DoubleClick(object sender, EventArgs e) {
            btnXemChiTiet_Click(sender, e);
        }

        private PhieuNhapDto GetSelected() {
            if (dgvPhieuNhap.SelectedRows.Count > 0) {
                return dgvPhieuNhap.SelectedRows[0].DataBoundItem as PhieuNhapDto;
            }
            return null;
        }
    }
}
