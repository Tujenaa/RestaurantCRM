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
                dgvMonAn.SelectionChanged += (s, ev) => UpdateButtonState();
                dgvMonAn.CellFormatting += dgvMonAn_CellFormatting;
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

        private async Task LoadMonAn(string selectId = null) {
            _allMonAn = await _monAnService.GetAllAsync();
            FilterData(selectId);
        }

        private void FilterData(string selectId = null) {
            var keyword = txtTimKiem.Text.ToLower();
            var maLoaiMon = cboLoaiMon.SelectedValue?.ToString() ?? "";

            var filtered = _allMonAn.Where(m => 
                (string.IsNullOrEmpty(keyword) || m.TenMonAn.ToLower().Contains(keyword)) &&
                (string.IsNullOrEmpty(maLoaiMon) || m.MaLoaiMon == maLoaiMon)
            ).ToList();

            dgvMonAn.DataSource = null;
            dgvMonAn.DataSource = filtered;
            
            if (dgvMonAn.Columns["MaMonAn"] != null) {
                dgvMonAn.Columns["MaMonAn"].HeaderText = "Mã Món";
                dgvMonAn.Columns["TenMonAn"].HeaderText = "Tên Món Ăn";
                dgvMonAn.Columns["Gia"].HeaderText = "Đơn Giá";
                dgvMonAn.Columns["Gia"].DefaultCellStyle.Format = "N0";
                dgvMonAn.Columns["MaLoaiMon"].HeaderText = "Mã Loại Món";
                dgvMonAn.Columns["SoLuongTon"].HeaderText = "Số Lượng Tồn";
                dgvMonAn.Columns["TrangThaiRaw"].HeaderText = "Trạng Thái";
                if (dgvMonAn.Columns["TrangThai"] != null) dgvMonAn.Columns["TrangThai"].Visible = false;
                if (dgvMonAn.Columns["HinhAnhUrl"] != null) dgvMonAn.Columns["HinhAnhUrl"].Visible = false;
            }

            if (!string.IsNullOrEmpty(selectId)) {
                for (int i = 0; i < dgvMonAn.Rows.Count; i++) {
                    var item = dgvMonAn.Rows[i].DataBoundItem as MonAnDto;
                    if (item != null && item.MaMonAn == selectId) {
                        dgvMonAn.Rows[i].Selected = true;
                        try { dgvMonAn.CurrentCell = dgvMonAn.Rows[i].Cells[0]; } catch { }
                        break;
                    }
                }
            }

            UpdateButtonState();
        }

        private void dgvMonAn_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex >= 0 && dgvMonAn.Columns[e.ColumnIndex].Name == "TrangThaiRaw" && e.Value != null) {
                string val = e.Value.ToString();
                if (val == "Discontinued" || val == "Inactive") {
                    e.Value = "Ngừng kinh doanh";
                    e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
                    e.FormattingApplied = true;
                } else if (val == "OutOfStock") {
                    e.Value = "Hết hàng";
                    e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(217, 119, 6);
                    e.FormattingApplied = true;
                } else if (val == "InStock" || val == "Active") {
                    e.Value = "Đang kinh doanh";
                    e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
                    e.FormattingApplied = true;
                }
            }
        }

        private void UpdateButtonState() {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = dgvMonAn.SelectedRows[0].DataBoundItem as MonAnDto;
                if (monAn != null) {
                    bool isNgungKinhDoanh = monAn.TrangThaiRaw == "Discontinued" || monAn.TrangThaiRaw == "Inactive";
                    if (isNgungKinhDoanh) {
                        btnNgungKinhDoanh.Text = "Kinh Doanh Lại";
                        btnNgungKinhDoanh.BackColor = System.Drawing.Color.FromArgb(220, 252, 231);
                        btnNgungKinhDoanh.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
                    } else {
                        btnNgungKinhDoanh.Text = "Ngừng Kinh Doanh";
                        btnNgungKinhDoanh.BackColor = System.Drawing.Color.FromArgb(254, 226, 226);
                        btnNgungKinhDoanh.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
                    }
                    btnNgungKinhDoanh.Enabled = true;
                    return;
                }
            }
            btnNgungKinhDoanh.Text = "Ngừng Kinh Doanh";
            btnNgungKinhDoanh.UseVisualStyleBackColor = true;
            btnNgungKinhDoanh.ForeColor = System.Drawing.SystemColors.ControlText;
            btnNgungKinhDoanh.Enabled = false;
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboLoaiMon_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnThem_Click(object sender, EventArgs e) {
            var dialog = new MonAnDialog();
            if (dialog.ShowDialog() == DialogResult.OK) {
                await LoadMonAn();
            }
        }

        private async void btnSua_Click(object sender, EventArgs e) {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = (MonAnDto)dgvMonAn.SelectedRows[0].DataBoundItem;
                var dialog = new MonAnDialog(monAn);
                if (dialog.ShowDialog() == DialogResult.OK) {
                    await LoadMonAn();
                }
            } else {
                MessageBox.Show("Vui lòng chọn món ăn để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnNgungKinhDoanh_Click(object sender, EventArgs e) {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = (MonAnDto)dgvMonAn.SelectedRows[0].DataBoundItem;
                bool isNgungKinhDoanh = monAn.TrangThaiRaw == "Discontinued" || monAn.TrangThaiRaw == "Inactive";
                string thaoTac = isNgungKinhDoanh ? "kinh doanh trở lại" : "ngừng kinh doanh";

                if (MessageBox.Show($"Bạn có chắc chắn muốn {thaoTac} món '{monAn.TenMonAn}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    if (isNgungKinhDoanh) {
                        monAn.TrangThaiRaw = monAn.SoLuongTon > 0 ? "InStock" : "OutOfStock";
                    } else {
                        monAn.TrangThaiRaw = "Discontinued";
                    }

                    var result = await _monAnService.UpdateAsync(monAn.MaMonAn, monAn);
                    if (result.Success) {
                        MessageBox.Show($"Đã {thaoTac} món ăn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMonAn(monAn.MaMonAn);
                    } else {
                        MessageBox.Show(result.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } else {
                MessageBox.Show("Vui lòng chọn món ăn cần cập nhật trạng thái.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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


