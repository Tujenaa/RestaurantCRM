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
    public partial class NhaCungCapForm : Form {
        private readonly NhaCungCapService _nccService;
        private List<NhaCungCapDto> _allNcc = new List<NhaCungCapDto>();

        public NhaCungCapForm() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _nccService = new NhaCungCapService(httpClient);
        }

        private async void NhaCungCapForm_Load(object sender, EventArgs e) {
            await LoadData();
        }

        private async Task LoadData() {
            try {
                _allNcc = await _nccService.GetAllAsync();
                FilterData();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu Nhà Cung Cấp từ API Server.\nChi tiết: " + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData() {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            var filtered = _allNcc.Where(n =>
                string.IsNullOrEmpty(keyword) ||
                (!string.IsNullOrEmpty(n.TenNhaCungCap) && n.TenNhaCungCap.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(n.MaNhaCungCap) && n.MaNhaCungCap.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(n.SoDienThoai) && n.SoDienThoai.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(n.Email) && n.Email.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(n.DiaChi) && n.DiaChi.ToLower().Contains(keyword))
            ).ToList();

            dgvNhaCungCap.DataSource = null;
            dgvNhaCungCap.DataSource = filtered;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            txtTimKiem.Clear();
            await LoadData();
        }

        private async void btnThem_Click(object sender, EventArgs e) {
            using (var dialog = new NhaCungCapDialog()) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    try {
                        var result = await _nccService.CreateAsync(dialog.NhaCungCap);
                        if (result.Success) {
                            MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadData();
                        } else {
                            MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show("Lỗi khi thêm nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnSua_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new NhaCungCapDialog(selected)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    try {
                        var result = await _nccService.UpdateAsync(selected.MaNhaCungCap, dialog.NhaCungCap);
                        if (result.Success) {
                            MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadData();
                        } else {
                            MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show("Lỗi khi cập nhật nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnXoa_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhà cung cấp \"{selected.TenNhaCungCap}\" ({selected.MaNhaCungCap})?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes) {
                try {
                    var result = await _nccService.DeleteAsync(selected.MaNhaCungCap);
                    if (result.Success) {
                        MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadData();
                    } else {
                        MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private NhaCungCapDto GetSelected() {
            if (dgvNhaCungCap.SelectedRows.Count > 0) {
                return dgvNhaCungCap.SelectedRows[0].DataBoundItem as NhaCungCapDto;
            }
            return null;
        }

        private void dgvNhaCungCap_DoubleClick(object sender, EventArgs e) {
            btnSua_Click(sender, e);
        }
    }
}
