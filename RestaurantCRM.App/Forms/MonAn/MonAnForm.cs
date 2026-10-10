using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public partial class MonAnForm : Form {
        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private List<MonAnDto> _allMonAn = new List<MonAnDto>();

        public MonAnForm() {
            InitializeComponent();
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5275/") }; // TBD: Use ApiClient or correct config
            _monAnService = new MonAnService(httpClient);
            _loaiMonService = new LoaiMonService(httpClient);
        }

        private async void MonAnForm_Load(object sender, EventArgs e) {
            try {
                await LoadLoaiMon();
                await LoadMonAn();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu. Vui lòng kiểm tra API Server.\nChi tiết: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadLoaiMon() {
            var loaiMonList = await _loaiMonService.GetAllAsync();
            loaiMonList.Insert(0, new LoaiMonDto { MaLoaiMon = "", TenLoaiMon = "Tất cả" });
            cboLoaiMon.DataSource = loaiMonList;
            cboLoaiMon.DisplayMember = "TenLoaiMon";
            cboLoaiMon.ValueMember = "MaLoaiMon";
        }

        private async Task LoadMonAn() {
            _allMonAn = await _monAnService.GetAllAsync();
            FilterData();
        }

        private void FilterData() {
            var keyword = txtTimKiem.Text.ToLower();
            var maLoaiMon = cboLoaiMon.SelectedValue?.ToString() ?? "";

            var filtered = _allMonAn.Where(m => 
                (string.IsNullOrEmpty(keyword) || m.TenMonAn.ToLower().Contains(keyword)) &&
                (string.IsNullOrEmpty(maLoaiMon) || m.MaLoaiMon == maLoaiMon)
            ).ToList();

            dgvMonAn.DataSource = null;
            dgvMonAn.DataSource = filtered;
            
            if (dgvMonAn.Columns["MaMonAn"] != null) {
                dgvMonAn.Columns["HinhAnhUrl"].Visible = false;
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboLoaiMon_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void btnThem_Click(object sender, EventArgs e) {
            var dialog = new MonAnDialog();
            if (dialog.ShowDialog() == DialogResult.OK) {
                LoadMonAn();
            }
        }

        private void btnSua_Click(object sender, EventArgs e) {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = (MonAnDto)dgvMonAn.SelectedRows[0].DataBoundItem;
                var dialog = new MonAnDialog(monAn);
                if (dialog.ShowDialog() == DialogResult.OK) {
                    LoadMonAn();
                }
            } else {
                MessageBox.Show("Vui lòng chọn món ăn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnNgungKinhDoanh_Click(object sender, EventArgs e) {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = (MonAnDto)dgvMonAn.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Bạn có chắc muốn cập nhật trạng thái món '{monAn.TenMonAn}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    monAn.TrangThai = !monAn.TrangThai; // Toggle status
                    var result = await _monAnService.UpdateAsync(monAn.MaMonAn, monAn);
                    if (result.Success) {
                        MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMonAn();
                    } else {
                        MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnLoaiMon_Click(object sender, EventArgs e) {
            using (var form = new LoaiMonForm()) {
                form.ShowDialog(this);
                await LoadLoaiMon();
                await LoadMonAn();
            }
        }
    }
}


