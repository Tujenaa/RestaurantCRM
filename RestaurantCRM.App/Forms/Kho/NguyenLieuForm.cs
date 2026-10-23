using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Kho {
    public partial class NguyenLieuForm : Form {
        private readonly NguyenLieuService _nguyenLieuService;
        private readonly BindingSource _bindingSource = new BindingSource();
        private List<NguyenLieuDto> _allNguyenLieu = new List<NguyenLieuDto>();
        private bool _isLoading = false;

        public NguyenLieuForm() {
            InitializeComponent();

            this.DoubleBuffered = true;
            EnableDoubleBuffered(dgvNguyenLieu);

            dgvNguyenLieu.AutoGenerateColumns = false;
            dgvNguyenLieu.AllowUserToResizeColumns = true;
            dgvNguyenLieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvNguyenLieu.ScrollBars = ScrollBars.Both;
            dgvNguyenLieu.DataSource = _bindingSource;

            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _nguyenLieuService = new NguyenLieuService(httpClient);

            cboBoLoc.SelectedIndex = 0;
            SetupContextMenu();
        }

        private static void EnableDoubleBuffered(Control control) {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, control, new object[] { true });
        }

        private void SetupContextMenu() {
            var contextMenu = new ContextMenuStrip();
            var mnuThem = new ToolStripMenuItem("＋ Thêm nguyên liệu mới", null, btnThem_Click);
            var mnuSua = new ToolStripMenuItem("✎ Sửa thông tin", null, btnSua_Click);
            var mnuSep = new ToolStripSeparator();
            var mnuNgungNhap = new ToolStripMenuItem("🚫 Ngừng nhập / 🔄 Nhập lại", null, btnNgungNhap_Click);

            contextMenu.Items.AddRange(new ToolStripItem[] { mnuThem, mnuSua, mnuSep, mnuNgungNhap });
            dgvNguyenLieu.ContextMenuStrip = contextMenu;
        }

        private async void NguyenLieuForm_Load(object sender, EventArgs e) {
            await LoadData();
        }

        private async Task LoadData() {
            if (_isLoading) return;
            _isLoading = true;
            try {
                _allNguyenLieu = await _nguyenLieuService.GetAllAsync() ?? new List<NguyenLieuDto>();
                UpdateThongKe();
                FilterData(preserveSelection: false);
            } catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải danh sách nguyên vật liệu từ API: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                _isLoading = false;
            }
        }

        private void UpdateThongKe() {
            int tongSo = _allNguyenLieu.Count;
            int dangNhap = _allNguyenLieu.Count(x => x.IsActive);
            int ngungNhap = tongSo - dangNhap;

            lblTongSo.Text = $"Tổng số: {tongSo} nguyên liệu";
            lblDangNhap.Text = $"🟢 Đang nhập: {dangNhap} loại";
            lblNgungNhap.Text = $"⚪ Ngừng nhập: {ngungNhap} loại";
        }

        private void FilterData(bool preserveSelection = true) {
            string keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            int filterIndex = cboBoLoc.SelectedIndex;

            var filtered = _allNguyenLieu.Where(nl => {
                // 1. Lọc theo trạng thái
                if (filterIndex == 1 && !nl.IsActive) return false; // Chỉ Đang nhập (Active)
                if (filterIndex == 2 && nl.IsActive) return false;  // Chỉ Ngừng nhập (Inactive)

                // 2. Lọc theo từ khóa
                if (string.IsNullOrEmpty(keyword)) return true;
                return (!string.IsNullOrEmpty(nl.TenNguyenLieu) && nl.TenNguyenLieu.ToLower().Contains(keyword)) ||
                       (!string.IsNullOrEmpty(nl.MaNguyenLieu) && nl.MaNguyenLieu.ToLower().Contains(keyword)) ||
                       (!string.IsNullOrEmpty(nl.DonViTinh) && nl.DonViTinh.ToLower().Contains(keyword));
            }).ToList();

            string selectedMa = null;
            int scrollIndex = -1;
            if (preserveSelection) {
                try { scrollIndex = dgvNguyenLieu.FirstDisplayedScrollingRowIndex; } catch { }
                selectedMa = GetSelected()?.MaNguyenLieu;
            }

            _bindingSource.DataSource = filtered;
            _bindingSource.ResetBindings(false);

            if (preserveSelection) {
                if (scrollIndex >= 0 && scrollIndex < dgvNguyenLieu.Rows.Count) {
                    try { dgvNguyenLieu.FirstDisplayedScrollingRowIndex = scrollIndex; } catch { }
                }
                if (!string.IsNullOrEmpty(selectedMa)) {
                    for (int i = 0; i < dgvNguyenLieu.Rows.Count; i++) {
                        if (dgvNguyenLieu.Rows[i].DataBoundItem is NguyenLieuDto item && item.MaNguyenLieu == selectedMa) {
                            dgvNguyenLieu.Rows[i].Selected = true;
                            try { dgvNguyenLieu.CurrentCell = dgvNguyenLieu.Rows[i].Cells[0]; } catch { }
                            break;
                        }
                    }
                }
            }

            UpdateActionButtonState();
        }

        private void UpdateActionButtonState() {
            var selected = GetSelected();
            if (selected == null) {
                btnSua.Enabled = false;
                btnNgungNhap.Enabled = false;
                btnNgungNhap.Text = "🚫 Ngừng nhập";
                btnNgungNhap.BackColor = Color.FromArgb(217, 119, 6);
                return;
            }

            btnSua.Enabled = true;
            btnNgungNhap.Enabled = true;

            if (selected.IsActive) {
                btnNgungNhap.Text = "🚫 Ngừng nhập";
                btnNgungNhap.BackColor = Color.FromArgb(217, 119, 6); // Amber warning
            } else {
                btnNgungNhap.Text = "🔄 Nhập lại";
                btnNgungNhap.BackColor = Color.FromArgb(5, 150, 105); // Emerald green
            }
        }

        private void dgvNguyenLieu_SelectionChanged(object sender, EventArgs e) {
            UpdateActionButtonState();
        }

        private void dgvNguyenLieu_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= dgvNguyenLieu.Rows.Count) return;

            var row = dgvNguyenLieu.Rows[e.RowIndex];
            if (row.DataBoundItem is NguyenLieuDto item) {
                if (!item.IsActive) {
                    // Đã ngừng nhập: làm dịu màu dòng để phân biệt rõ
                    row.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 85, 105);
                } else {
                    row.DefaultCellStyle.BackColor = (e.RowIndex % 2 == 0) ? Color.White : Color.FromArgb(250, 250, 250);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 242, 255);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
                }

                if (e.ColumnIndex >= 0 && dgvNguyenLieu.Columns[e.ColumnIndex].DataPropertyName == "GiaNhapGanNhat") {
                    e.Value = item.GiaNhapGanNhat.ToString("#,##0") + " đ";
                    e.FormattingApplied = true;
                }

                if (e.ColumnIndex >= 0 && dgvNguyenLieu.Columns[e.ColumnIndex].DataPropertyName == "SoLuongTon") {
                    e.Value = item.SoLuongTon.ToString("#,##0.##");
                    e.FormattingApplied = true;
                }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData(preserveSelection: false);
        }

        private void cboBoLoc_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData(preserveSelection: false);
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            txtTimKiem.Clear();
            cboBoLoc.SelectedIndex = 0;
            await LoadData();
        }

        private async void btnThem_Click(object sender, EventArgs e) {
            using (var dialog = new NguyenLieuDialog(_nguyenLieuService)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadData();
                }
            }
        }

        private async void btnSua_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một nguyên vật liệu trong danh sách để sửa thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new NguyenLieuDialog(_nguyenLieuService, selected)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadData();
                }
            }
        }

        private async void btnNgungNhap_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một nguyên vật liệu để thay đổi trạng thái.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isCurrentlyActive = selected.IsActive;
            string prompt = isCurrentlyActive
                ? $"Bạn có chắc chắn muốn NGỪNG NHẬP nguyên vật liệu '{selected.TenNguyenLieu}' (Mã: {selected.MaNguyenLieu})?\n\nNguyên liệu này sẽ không còn hiển thị khi lập phiếu nhập kho mới, nhưng toàn bộ lịch sử hóa đơn nhập cũ và báo cáo vẫn được bảo toàn nguyên vẹn."
                : $"Bạn có muốn MỞ LẠI và CHO PHÉP NHẬP nguyên vật liệu '{selected.TenNguyenLieu}' (Mã: {selected.MaNguyenLieu}) vào hệ thống không?";

            string title = isCurrentlyActive ? "Xác nhận ngừng nhập hàng" : "Xác nhận cho phép nhập lại";
            var icon = isCurrentlyActive ? MessageBoxIcon.Question : MessageBoxIcon.Information;

            if (MessageBox.Show(prompt, title, MessageBoxButtons.YesNo, icon) == DialogResult.Yes) {
                var result = await _nguyenLieuService.ToggleTrangThaiAsync(selected.MaNguyenLieu, selected.TrangThai);
                if (result.Success) {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                } else {
                    MessageBox.Show(result.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvNguyenLieu_DoubleClick(object sender, EventArgs e) {
            btnSua_Click(sender, e);
        }

        private NguyenLieuDto GetSelected() {
            if (dgvNguyenLieu.SelectedRows.Count > 0) {
                return dgvNguyenLieu.SelectedRows[0].DataBoundItem as NguyenLieuDto;
            }
            return null;
        }
    }
}
