using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Forms.NhaCungCap;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Kho {
    public partial class KhoForm : Form {
        private readonly KhoService _khoService;
        private List<TonKhoDto> _allTonKho = new List<TonKhoDto>();

        public KhoForm() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _khoService = new KhoService(httpClient);
            cboBoLoc.SelectedIndex = 0;
        }

        private async void KhoForm_Load(object sender, EventArgs e) {
            await LoadKho();
        }

        private async Task LoadKho() {
            try {
                _allTonKho = await _khoService.GetTonKhoAsync();
                UpdateThongKe();
                FilterData();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu tồn kho từ API: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateThongKe() {
            int tongSo = _allTonKho.Count;
            int hetHang = _allTonKho.Count(x => x.SoLuongTon <= 0);
            int sapHet = _allTonKho.Count(x => x.SoLuongTon > 0 && x.SoLuongTon <= 10);
            int duHang = _allTonKho.Count(x => x.SoLuongTon > 10);

            lblTongMon.Text = $"Tổng số: {tongSo} món";
            lblDuHang.Text = $"Đủ hàng: {duHang} món";
            lblSapHet.Text = $"Sắp hết (<= 10): {sapHet} món";
            lblHetHang.Text = $"Đã hết: {hetHang} món";
        }

        private void FilterData() {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            int filterIndex = cboBoLoc.SelectedIndex;

            var filtered = _allTonKho.Where(item => {
                // Lọc từ khóa
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                    (!string.IsNullOrEmpty(item.TenMon) && item.TenMon.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.MaMon) && item.MaMon.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.TenLoaiMon) && item.TenLoaiMon.ToLower().Contains(keyword));

                if (!matchKeyword) return false;

                // Lọc theo tình trạng
                switch (filterIndex) {
                    case 1: // Sắp hết (<= 10 & > 0)
                        return item.SoLuongTon > 0 && item.SoLuongTon <= 10;
                    case 2: // Đã hết (= 0)
                        return item.SoLuongTon <= 0;
                    case 3: // Còn đủ hàng (> 10)
                        return item.SoLuongTon > 10;
                    default: // Tất cả
                        return true;
                }
            }).ToList();

            dgvKho.DataSource = null;
            dgvKho.DataSource = filtered;
        }

        private void dgvKho_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= dgvKho.Rows.Count) return;

            var row = dgvKho.Rows[e.RowIndex];
            if (row.DataBoundItem is TonKhoDto item) {
                if (item.SoLuongTon <= 0) {
                    // Đã hết hàng: đỏ nhạt
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(153, 27, 27);
                } else if (item.SoLuongTon <= 10) {
                    // Sắp hết hàng: vàng cam nhạt
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                } else {
                    // Đủ hàng: trắng bình thường
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
                }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboBoLoc_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            txtTimKiem.Clear();
            cboBoLoc.SelectedIndex = 0;
            await LoadKho();
        }

        private async void btnKiemKe_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một mặt hàng trong danh sách để điều chỉnh tồn kho.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new DieuChinhTonKhoDialog(selected, _khoService)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadKho();
                }
            }
        }

        private void btnLapPhieuNhap_Click(object sender, EventArgs e) {
            using (var form = new PhieuNhapForm()) {
                form.ShowDialog(this);
                // Sau khi đóng form nhập hàng, tự động tải lại tồn kho
                _ = LoadKho();
            }
        }

        private void dgvKho_DoubleClick(object sender, EventArgs e) {
            btnKiemKe_Click(sender, e);
        }

        private TonKhoDto GetSelected() {
            if (dgvKho.SelectedRows.Count > 0) {
                return dgvKho.SelectedRows[0].DataBoundItem as TonKhoDto;
            }
            return null;
        }
    }
}
