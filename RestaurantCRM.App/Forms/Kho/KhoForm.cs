using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Forms.NhaCungCap;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Kho {
    public partial class KhoForm : Form {
        private readonly KhoService _khoService;
        private readonly NguyenLieuService _nguyenLieuService;
        private readonly BindingSource _bindingSource = new BindingSource();
        private List<TonKhoDto> _allTonKho = new List<TonKhoDto>();
        private readonly Timer _refreshTimer;
        private bool _isRefreshing = false;

        public KhoForm() {
            InitializeComponent();

            // 1. Chống chớp giật màn hình (Flickering): Kích hoạt DoubleBuffering cho Form và DataGridView
            this.DoubleBuffered = true;
            EnableDoubleBuffered(dgvKho);

            // 2. Tắt AutoGenerateColumns và liên kết qua BindingSource
            dgvKho.AutoGenerateColumns = false;
            dgvKho.AllowUserToResizeColumns = true;
            dgvKho.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvKho.ScrollBars = ScrollBars.Both;
            dgvKho.DataSource = _bindingSource;

            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _khoService = new KhoService(httpClient);
            _nguyenLieuService = new NguyenLieuService(httpClient);
            cboBoLoc.SelectedIndex = 0;

            SetupContextMenu();

            // 3. Tăng chu kỳ timer lên 15 giây và chỉ cập nhật khi dữ liệu có thay đổi thật sự
            _refreshTimer = new Timer { Interval = 15000 };
            _refreshTimer.Tick += async (s, e) => await LoadKhoSilently();
            this.Activated += async (s, e) => await LoadKhoSilently();
            this.FormClosing += (s, e) => {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };
        }

        private void SetupContextMenu() {
            var contextMenu = new ContextMenuStrip();
            var mnuQuanLyNL = new ToolStripMenuItem("🥕 Quản lý danh mục nguyên liệu", null, btnThemNguyenLieu_Click);
            var mnuSua = new ToolStripMenuItem("✎ Sửa thông tin nguyên liệu", null, (s, e) => SuaNguyenLieu_Click(s, e));

            contextMenu.Items.AddRange(new ToolStripItem[] { mnuQuanLyNL, mnuSua });
            dgvKho.ContextMenuStrip = contextMenu;
        }

        private static void EnableDoubleBuffered(Control control) {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, control, new object[] { true });
        }

        private async void KhoForm_Load(object sender, EventArgs e) {
            await LoadKho();
            _refreshTimer.Start();
        }

        private async Task LoadKho() {
            try {
                _allTonKho = await _khoService.GetTonKhoAsync() ?? new List<TonKhoDto>();
                UpdateThongKe();
                FilterData(preserveSelection: false);
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu tồn kho từ API: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadKhoSilently() {
            if (_isRefreshing || IsDisposed) return;
            _isRefreshing = true;
            try {
                var fresh = await _khoService.GetTonKhoAsync();
                if (fresh != null && !IsDisposed) {
                    // Smart Refresh: Chỉ cập nhật bảng khi dữ liệu thực sự có sự thay đổi
                    if (!HasTonKhoChanged(_allTonKho, fresh)) {
                        return; // Dữ liệu không thay đổi -> giữ nguyên giao diện, 0% chớp nháy
                    }

                    _allTonKho = fresh;
                    UpdateThongKe();
                    FilterData(preserveSelection: true);
                }
            } catch {
                // Bỏ qua lỗi ngầm khi polling
            } finally {
                _isRefreshing = false;
            }
        }

        private bool HasTonKhoChanged(List<TonKhoDto> oldList, List<TonKhoDto> newList) {
            if (oldList == null || newList == null) return true;
            if (oldList.Count != newList.Count) return true;
            for (int i = 0; i < oldList.Count; i++) {
                var a = oldList[i];
                var b = newList[i];
                if (a.MaNguyenLieu != b.MaNguyenLieu ||
                    a.SoLuongTon != b.SoLuongTon ||
                    a.GiaNhapGanNhat != b.GiaNhapGanNhat ||
                    a.CanhBao != b.CanhBao ||
                    a.TenNguyenLieu != b.TenNguyenLieu ||
                    a.DonViTinh != b.DonViTinh ||
                    a.TrangThai != b.TrangThai) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateThongKe() {
            int tongSo = _allTonKho.Count;
            int hetHang = _allTonKho.Count(x => x.SoLuongTon <= 0);
            int sapHet = _allTonKho.Count(x => x.SoLuongTon > 0 && x.SoLuongTon <= 10);
            int duHang = _allTonKho.Count(x => x.SoLuongTon > 10);

            lblTongMon.Text = $"Tổng số: {tongSo} loại NL";
            lblDuHang.Text = $"Đủ hàng: {duHang} loại";
            lblSapHet.Text = $"Sắp hết (<= 10): {sapHet} loại";
            lblHetHang.Text = $"Đã hết: {hetHang} loại";
        }

        private void FilterData(bool preserveSelection = false) {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            int filterIndex = cboBoLoc.SelectedIndex;

            var filtered = _allTonKho.Where(item => {
                // Lọc từ khóa
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                    (!string.IsNullOrEmpty(item.TenNguyenLieu) && item.TenNguyenLieu.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.MaNguyenLieu) && item.MaNguyenLieu.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.DonViTinh) && item.DonViTinh.ToLower().Contains(keyword));

                if (!matchKeyword) return false;

                // Lọc theo tình trạng
                switch (filterIndex) {
                    case 1: // Sắp hết (<= 10 & > 0)
                        return item.SoLuongTon > 0 && item.SoLuongTon <= 10;
                    case 2: // Đã hết (<= 0)
                        return item.SoLuongTon <= 0;
                    case 3: // Còn đủ hàng (> 10)
                        return item.SoLuongTon > 10;
                    default: // Tất cả
                        return true;
                }
            }).ToList();

            int scrollIndex = -1;
            string selectedMa = null;
            if (preserveSelection) {
                try { scrollIndex = dgvKho.FirstDisplayedScrollingRowIndex; } catch { }
                selectedMa = GetSelected()?.MaNguyenLieu;
            }

            // Gán trực tiếp qua BindingSource KHÔNG dùng dgvKho.DataSource = null để tránh chớp trắng bảng
            _bindingSource.DataSource = filtered;
            _bindingSource.ResetBindings(false);

            if (preserveSelection) {
                if (scrollIndex >= 0 && scrollIndex < dgvKho.Rows.Count) {
                    try { dgvKho.FirstDisplayedScrollingRowIndex = scrollIndex; } catch { }
                }
                if (!string.IsNullOrEmpty(selectedMa)) {
                    for (int i = 0; i < dgvKho.Rows.Count; i++) {
                        if (dgvKho.Rows[i].DataBoundItem is TonKhoDto item && item.MaNguyenLieu == selectedMa) {
                            dgvKho.Rows[i].Selected = true;
                            try { dgvKho.CurrentCell = dgvKho.Rows[i].Cells[0]; } catch { }
                            break;
                        }
                    }
                }
            }
        }

        private void dgvKho_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= dgvKho.Rows.Count) return;

            var row = dgvKho.Rows[e.RowIndex];
            if (row.DataBoundItem is TonKhoDto item) {
                bool isNgungNhap = string.Equals(item.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase);

                if (isNgungNhap) {
                    // Những món ngừng nhập chuyển màu xám toàn bộ dòng
                    row.DefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(100, 116, 139);
                } else if (item.SoLuongTon <= 0) {
                    // Đã hết hàng: đỏ nhạt dịu mắt
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 202, 202);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(153, 27, 27);
                } else if (item.SoLuongTon <= 10) {
                    // Sắp hết hàng: vàng cam nhạt dịu mắt
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(253, 230, 138);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(146, 64, 14);
                } else {
                    // Đủ hàng: xen kẽ trắng và xám nhẹ
                    row.DefaultCellStyle.BackColor = (e.RowIndex % 2 == 0) ? Color.White : Color.FromArgb(248, 250, 252);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
                }

                // Định dạng trực quan cột Trạng Thái Kho
                if (e.ColumnIndex >= 0 && dgvKho.Columns[e.ColumnIndex].DataPropertyName == "TrangThaiHienThi") {
                    if (item.SoLuongTon <= 0) {
                        e.Value = "❌ Đã hết hàng";
                    } else if (item.SoLuongTon <= 10) {
                        e.Value = "⚠️ Sắp hết hàng";
                    } else {
                        e.Value = "✅ Còn đủ hàng";
                    }
                    e.FormattingApplied = true;
                }

                // Định dạng số lượng tồn: gọn gàng không bị số 0 vô nghĩa ở đuôi
                if (e.ColumnIndex >= 0 && dgvKho.Columns[e.ColumnIndex].DataPropertyName == "SoLuongTon") {
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
            await LoadKho();
        }

        private async void btnLapPhieuNhap_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            string defaultMaNL = (selected != null && selected.TrangThai == "Active") ? selected.MaNguyenLieu : null;

            using (var dialog = new PhieuNhapDialog(defaultMaNL)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    // Sau khi nhập kho thành công, tự động tải lại bảng tồn kho
                    await LoadKho();
                }
            }
        }

        private void dgvKho_DoubleClick(object sender, EventArgs e) {
            SuaNguyenLieu_Click(sender, e);
        }

        private async void btnThemNguyenLieu_Click(object sender, EventArgs e) {
            using (var form = new NguyenLieuForm()) {
                form.ShowDialog(this);
                await LoadKho();
            }
        }

        private async void SuaNguyenLieu_Click(object sender, EventArgs e) {
            var selected = GetSelected();
            if (selected == null) {
                MessageBox.Show("Vui lòng chọn một mặt hàng trong danh sách để sửa thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nlDto = new NguyenLieuDto {
                MaNguyenLieu = selected.MaNguyenLieu,
                TenNguyenLieu = selected.TenNguyenLieu,
                DonViTinh = selected.DonViTinh,
                GiaNhapGanNhat = selected.GiaNhapGanNhat,
                SoLuongTon = selected.SoLuongTon,
                TrangThai = selected.TrangThai
            };

            using (var dialog = new NguyenLieuDialog(_nguyenLieuService, nlDto)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadKho();
                }
            }
        }

        private TonKhoDto GetSelected() {
            if (dgvKho.SelectedRows.Count > 0) {
                return dgvKho.SelectedRows[0].DataBoundItem as TonKhoDto;
            }
            return null;
        }
    }
}
