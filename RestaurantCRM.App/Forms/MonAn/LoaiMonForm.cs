using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public partial class LoaiMonForm : Form {
        private readonly LoaiMonService _loaiMonService;
        private readonly MonAnService _monAnService;
        private List<LoaiMonDto> _allLoaiMon = new List<LoaiMonDto>();
        private LoaiMonDto _selectedLoaiMon;

        public LoaiMonForm() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _loaiMonService = new LoaiMonService(httpClient);
            _monAnService = new MonAnService(httpClient);
        }

        private async void LoaiMonForm_Load(object sender, EventArgs e) {
            await LoadData();
        }

        private async Task LoadData() {
            try {
                var dsLoaiMon = await _loaiMonService.GetAllAsync();
                
                // Lấy danh sách món ăn để thống kê số lượng món thuộc mỗi loại (hỗ trợ kiểm tra ràng buộc)
                try {
                    var dsMonAn = await _monAnService.GetAllAsync();
                    var dictCount = dsMonAn
                        .Where(m => !string.IsNullOrEmpty(m.MaLoaiMon))
                        .GroupBy(m => m.MaLoaiMon)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var lm in dsLoaiMon) {
                        if (!string.IsNullOrEmpty(lm.MaLoaiMon) && dictCount.TryGetValue(lm.MaLoaiMon, out int count)) {
                            lm.SoLuongMon = count;
                        } else {
                            lm.SoLuongMon = 0;
                        }
                    }
                } catch {
                    // Nếu lỗi khi đếm món ăn thì vẫn hiển thị loại món bình thường
                }

                _allLoaiMon = dsLoaiMon;
                FilterData();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu Loại Món từ API: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData() {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            var filtered = _allLoaiMon.Where(lm =>
                string.IsNullOrEmpty(keyword) ||
                (!string.IsNullOrEmpty(lm.TenLoaiMon) && lm.TenLoaiMon.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(lm.MaLoaiMon) && lm.MaLoaiMon.ToLower().Contains(keyword))
            ).ToList();

            dgvLoaiMon.DataSource = null;
            dgvLoaiMon.DataSource = filtered;

            if (_selectedLoaiMon != null) {
                var exists = filtered.FirstOrDefault(x => x.MaLoaiMon == _selectedLoaiMon.MaLoaiMon);
                if (exists != null) {
                    SelectRowByMa(exists.MaLoaiMon);
                } else {
                    ClearInput();
                }
            } else if (filtered.Count > 0) {
                SelectRowByIndex(0);
            } else {
                ClearInput();
            }
        }

        private void SelectRowByMa(string ma) {
            foreach (DataGridViewRow row in dgvLoaiMon.Rows) {
                if (row.DataBoundItem is LoaiMonDto dto && dto.MaLoaiMon == ma) {
                    row.Selected = true;
                    _selectedLoaiMon = dto;
                    FillInput(dto);
                    break;
                }
            }
        }

        private void SelectRowByIndex(int index) {
            if (index >= 0 && index < dgvLoaiMon.Rows.Count) {
                dgvLoaiMon.Rows[index].Selected = true;
                _selectedLoaiMon = dgvLoaiMon.Rows[index].DataBoundItem as LoaiMonDto;
                if (_selectedLoaiMon != null) {
                    FillInput(_selectedLoaiMon);
                }
            }
        }

        private void FillInput(LoaiMonDto dto) {
            txtMaLoaiMon.Text = dto.MaLoaiMon ?? "";
            txtTenLoaiMon.Text = dto.TenLoaiMon ?? "";
        }

        private void ClearInput() {
            _selectedLoaiMon = null;
            txtMaLoaiMon.Text = "(Tự động sinh)";
            txtTenLoaiMon.Clear();
            txtTenLoaiMon.Focus();
        }

        private void dgvLoaiMon_SelectionChanged(object sender, EventArgs e) {
            if (dgvLoaiMon.SelectedRows.Count > 0) {
                _selectedLoaiMon = dgvLoaiMon.SelectedRows[0].DataBoundItem as LoaiMonDto;
                if (_selectedLoaiMon != null) {
                    FillInput(_selectedLoaiMon);
                }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            txtTimKiem.Clear();
            await LoadData();
        }

        private void btnNhapLai_Click(object sender, EventArgs e) {
            ClearInput();
        }

        private async void btnThem_Click(object sender, EventArgs e) {
            var tenLoai = txtTenLoaiMon.Text.Trim();
            if (string.IsNullOrWhiteSpace(tenLoai)) {
                MessageBox.Show("Vui lòng nhập tên loại món.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenLoaiMon.Focus();
                return;
            }

            // Kiểm tra trùng tên loại món
            if (_allLoaiMon.Any(x => x.TenLoaiMon != null && x.TenLoaiMon.Equals(tenLoai, StringComparison.OrdinalIgnoreCase))) {
                MessageBox.Show($"Loại món \"{tenLoai}\" đã tồn tại trong hệ thống.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = await _loaiMonService.CreateAsync(new LoaiMonDto { TenLoaiMon = tenLoai });
            if (result.Success) {
                MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearInput();
                await LoadData();
            } else {
                MessageBox.Show(result.Message, "Lỗi thêm loại món", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSua_Click(object sender, EventArgs e) {
            if (_selectedLoaiMon == null || string.IsNullOrEmpty(_selectedLoaiMon.MaLoaiMon)) {
                MessageBox.Show("Vui lòng chọn loại món cần sửa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tenMoi = txtTenLoaiMon.Text.Trim();
            if (string.IsNullOrWhiteSpace(tenMoi)) {
                MessageBox.Show("Vui lòng nhập tên loại món.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenLoaiMon.Focus();
                return;
            }

            // Kiểm tra trùng tên với loại món khác
            if (_allLoaiMon.Any(x => x.MaLoaiMon != _selectedLoaiMon.MaLoaiMon && x.TenLoaiMon != null && x.TenLoaiMon.Equals(tenMoi, StringComparison.OrdinalIgnoreCase))) {
                MessageBox.Show($"Tên loại món \"{tenMoi}\" đã bị trùng với loại món khác.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new LoaiMonDto {
                MaLoaiMon = _selectedLoaiMon.MaLoaiMon,
                TenLoaiMon = tenMoi
            };

            var result = await _loaiMonService.UpdateAsync(_selectedLoaiMon.MaLoaiMon, dto);
            if (result.Success) {
                MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var currentMa = _selectedLoaiMon.MaLoaiMon;
                await LoadData();
                SelectRowByMa(currentMa);
            } else {
                MessageBox.Show(result.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnXoa_Click(object sender, EventArgs e) {
            if (_selectedLoaiMon == null || string.IsNullOrEmpty(_selectedLoaiMon.MaLoaiMon)) {
                MessageBox.Show("Vui lòng chọn loại món cần xóa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra ràng buộc dữ liệu tại Client: nếu có món ăn thuộc loại này thì cảnh báo ngay
            if (_selectedLoaiMon.SoLuongMon > 0) {
                MessageBox.Show(
                    $"Không thể xóa loại món \"{_selectedLoaiMon.TenLoaiMon}\" vì hiện tại đang có {_selectedLoaiMon.SoLuongMon} món ăn thuộc loại này.\n\n" +
                    "Ràng buộc dữ liệu: Bạn phải chuyển hoặc xóa các món ăn thuộc loại này sang danh mục khác trước khi xóa loại món.",
                    "Không thể xóa - Vi phạm ràng buộc",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa loại món \"{_selectedLoaiMon.TenLoaiMon}\" ({_selectedLoaiMon.MaLoaiMon}) không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes) {
                var result = await _loaiMonService.DeleteAsync(_selectedLoaiMon.MaLoaiMon);
                if (result.Success) {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInput();
                    await LoadData();
                } else {
                    // Hiển thị thông báo chi tiết từ API Server (ví dụ kiểm tra ràng buộc foreign key)
                    MessageBox.Show(result.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
