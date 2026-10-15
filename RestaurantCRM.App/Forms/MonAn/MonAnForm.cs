using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public partial class MonAnForm : Form {
        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private List<MonAnDto> _allMonAn = new List<MonAnDto>();
        private List<LoaiMonDto> _allLoaiMon = new List<LoaiMonDto>();
        private readonly Timer _refreshTimer;
        private bool _isRefreshing = false;

        public MonAnForm() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _monAnService = new MonAnService(httpClient);
            _loaiMonService = new LoaiMonService(httpClient);

            SetupKpiCards();
            SetupSearchAndFilter();
            SetupGridDesign();

            _refreshTimer = new Timer { Interval = 6000 };
            _refreshTimer.Tick += async (s, e) => await LoadMonAnSilently();
            this.Activated += async (s, e) => await LoadMonAnSilently();
            this.FormClosing += (s, e) => {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };
        }

        private void SetupKpiCards() {
            pnlKpis.Controls.Clear();

            // Card 1: Tổng thực đơn
            var card1 = CreateKpiCard("🍽️", "TỔNG THỰC ĐƠN", "0 món", "Món ăn trong hệ thống", ThemeManager.Ink, out lblKpiTongMon);
            pnlKpis.Controls.Add(card1, 0, 0);

            // Card 2: Đang kinh doanh
            var card2 = CreateKpiCard("🟢", "ĐANG KINH DOANH", "0 món", "Sẵn sàng phục vụ khách", Color.FromArgb(5, 150, 105), out lblKpiKinhDoanh);
            pnlKpis.Controls.Add(card2, 1, 0);

            // Card 3: Hết hàng
            var card3 = CreateKpiCard("⚠️", "HẾT NGUYÊN LIỆU", "0 món", "Khả dụng = 0 suất", Color.FromArgb(217, 119, 6), out lblKpiHetHang);
            pnlKpis.Controls.Add(card3, 2, 0);

            // Card 4: Ngừng kinh doanh
            var card4 = CreateKpiCard("🛑", "NGỪNG KINH DOANH", "0 món", "Đã tạm dừng trên menu", Color.FromArgb(220, 38, 38), out lblKpiNgungKD);
            pnlKpis.Controls.Add(card4, 3, 0);
        }

        private Panel CreateKpiCard(string icon, string title, string initialValue, string sub, Color valueColor, out Label lblValueRef) {
            var card = new RoundedPanel {
                Dock = DockStyle.Fill,
                Radius = 12,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(226, 232, 240),
                BorderWidth = 1,
                Margin = new Padding(4, 0, 4, 0)
            };

            var lblIcon = new Label {
                Text = icon,
                Font = new Font("Segoe UI", 15F),
                Location = new Point(14, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblIcon);

            var lblTitle = new Label {
                Text = title,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(50, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            var lblValue = new Label {
                Text = initialValue,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = valueColor,
                Location = new Point(48, 28),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblValue);
            lblValueRef = lblValue;

            var lblSub = new Label {
                Text = sub,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(50, 52),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblSub);

            return card;
        }

        private void SetupSearchAndFilter() {
            cboTrangThai.Items.Clear();
            cboTrangThai.Items.AddRange(new object[] {
                "Tất cả trạng thái",
                "Đang kinh doanh",
                "Hết hàng",
                "Ngừng kinh doanh"
            });
            cboTrangThai.SelectedIndex = 0;

            pnlFilter.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlFilter.Width - 1, pnlFilter.Height - 1);
                }
            };

            pnlGridCard.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGridCard.Width - 1, pnlGridCard.Height - 1);
                }
            };
        }

        private void SetupGridDesign() {
            dgvMonAn.ReadOnly = true;
            dgvMonAn.AllowUserToAddRows = false;
            dgvMonAn.AllowUserToDeleteRows = false;
            dgvMonAn.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMonAn.MultiSelect = false;
            dgvMonAn.DoubleClick += (s, e) => {
                if (dgvMonAn.SelectedRows.Count > 0) {
                    btnSua_Click(s, e);
                }
            };
        }

        private async void MonAnForm_Load(object sender, EventArgs e) {
            try {
                dgvMonAn.SelectionChanged += (s, ev) => UpdateButtonState();
                dgvMonAn.CellFormatting += dgvMonAn_CellFormatting;
                await LoadLoaiMon();
                await LoadMonAn();
                _refreshTimer.Start();
            }
            catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu. Vui lòng kiểm tra API Server.\nChi tiết: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadLoaiMon() {
            try {
                _allLoaiMon = await _loaiMonService.GetAllAsync() ?? new List<LoaiMonDto>();
                var loaiMonList = new List<LoaiMonDto>(_allLoaiMon);
                loaiMonList.Insert(0, new LoaiMonDto { MaLoaiMon = "", TenLoaiMon = "Tất cả loại món" });

                cboLoaiMon.DataSource = loaiMonList;
                cboLoaiMon.DisplayMember = "TenLoaiMon";
                cboLoaiMon.ValueMember = "MaLoaiMon";
            } catch { }
        }

        private async Task LoadMonAn(string selectId = null) {
            _allMonAn = await _monAnService.GetAllAsync() ?? new List<MonAnDto>();
            UpdateKpis();
            FilterData(selectId);
        }

        private async Task LoadMonAnSilently() {
            if (_isRefreshing || IsDisposed) return;
            _isRefreshing = true;
            try {
                string currentSelectedId = null;
                if (dgvMonAn.CurrentRow?.DataBoundItem is MonAnDto current) {
                    currentSelectedId = current.MaMonAn;
                }
                var fresh = await _monAnService.GetAllAsync();
                if (fresh != null && !IsDisposed) {
                    _allMonAn = fresh;
                    UpdateKpis();
                    FilterData(currentSelectedId);
                }
            } catch {
                // Bỏ qua lỗi ngầm khi polling
            } finally {
                _isRefreshing = false;
            }
        }

        private void UpdateKpis() {
            int tong = _allMonAn.Count;
            int ngungKd = _allMonAn.Count(m => m.TrangThaiRaw == "Discontinued" || m.TrangThaiRaw == "Inactive");
            int hetHang = _allMonAn.Count(m => (m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive") && (m.SoLuongTon <= 0 || m.TrangThaiRaw == "OutOfStock"));
            int dangKd = _allMonAn.Count(m => (m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive") && m.SoLuongTon > 0 && m.TrangThaiRaw != "OutOfStock");

            lblKpiTongMon.Text = $"{tong} món";
            lblKpiKinhDoanh.Text = $"{dangKd} món";
            lblKpiHetHang.Text = $"{hetHang} món";
            lblKpiNgungKD.Text = $"{ngungKd} món";
        }

        private void FilterData(string selectId = null) {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLowerInvariant();
            var maLoaiMon = cboLoaiMon.SelectedValue?.ToString() ?? "";
            var selectedTrangThai = cboTrangThai.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            var filtered = _allMonAn.Where(m => {
                bool matchKeyword = string.IsNullOrEmpty(keyword)
                    || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(keyword))
                    || (m.MaMonAn != null && m.MaMonAn.ToLowerInvariant().Contains(keyword));

                bool matchLoai = string.IsNullOrEmpty(maLoaiMon) || m.MaLoaiMon == maLoaiMon;

                bool isNgungKd = m.TrangThaiRaw == "Discontinued" || m.TrangThaiRaw == "Inactive";
                bool isHetHang = !isNgungKd && (m.SoLuongTon <= 0 || m.TrangThaiRaw == "OutOfStock");
                bool isDangKd = !isNgungKd && !isHetHang;

                bool matchStatus = true;
                if (selectedTrangThai == "Đang kinh doanh") matchStatus = isDangKd;
                else if (selectedTrangThai == "Hết hàng") matchStatus = isHetHang;
                else if (selectedTrangThai == "Ngừng kinh doanh") matchStatus = isNgungKd;

                return matchKeyword && matchLoai && matchStatus;
            }).ToList();

            dgvMonAn.DataSource = null;
            dgvMonAn.DataSource = filtered;
            dgvMonAn.ReadOnly = true;

            lblDemSoLuong.Text = $"Hiển thị: {filtered.Count}/{_allMonAn.Count} món";

            if (dgvMonAn.Columns["MaMonAn"] != null) {
                dgvMonAn.Columns["MaMonAn"].HeaderText = "Mã Món";
                dgvMonAn.Columns["MaMonAn"].FillWeight = 55;
                dgvMonAn.Columns["MaMonAn"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dgvMonAn.Columns["TenMonAn"].HeaderText = "Tên Món Ăn";
                dgvMonAn.Columns["TenMonAn"].FillWeight = 150;

                dgvMonAn.Columns["MaLoaiMon"].HeaderText = "Loại Món";
                dgvMonAn.Columns["MaLoaiMon"].FillWeight = 85;

                dgvMonAn.Columns["Gia"].HeaderText = "Giá Bán";
                dgvMonAn.Columns["Gia"].FillWeight = 75;
                dgvMonAn.Columns["Gia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                if (dgvMonAn.Columns["GiaVonUocTinh"] != null) {
                    dgvMonAn.Columns["GiaVonUocTinh"].HeaderText = "Giá Vốn (COGS)";
                    dgvMonAn.Columns["GiaVonUocTinh"].FillWeight = 80;
                    dgvMonAn.Columns["GiaVonUocTinh"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                dgvMonAn.Columns["SoLuongTon"].HeaderText = "Khả Dụng";
                dgvMonAn.Columns["SoLuongTon"].FillWeight = 75;
                dgvMonAn.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dgvMonAn.Columns["TrangThaiRaw"].HeaderText = "Trạng Thái";
                dgvMonAn.Columns["TrangThaiRaw"].FillWeight = 95;
                dgvMonAn.Columns["TrangThaiRaw"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                if (dgvMonAn.Columns["TrangThai"] != null) dgvMonAn.Columns["TrangThai"].Visible = false;
                if (dgvMonAn.Columns["HinhAnhUrl"] != null) dgvMonAn.Columns["HinhAnhUrl"].Visible = false;
                if (dgvMonAn.Columns["MoTa"] != null) dgvMonAn.Columns["MoTa"].Visible = false;
                if (dgvMonAn.Columns["TenLoaiMon"] != null) dgvMonAn.Columns["TenLoaiMon"].Visible = false;
                if (dgvMonAn.Columns["SoSuatKhaDung"] != null) dgvMonAn.Columns["SoSuatKhaDung"].Visible = false;
                if (dgvMonAn.Columns["DinhLuong"] != null) dgvMonAn.Columns["DinhLuong"].Visible = false;
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
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string colName = dgvMonAn.Columns[e.ColumnIndex].Name;

            // Mã món
            if (colName == "MaMonAn" && e.Value != null) {
                e.CellStyle.ForeColor = Color.FromArgb(79, 70, 229);
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            // Tên món
            else if (colName == "TenMonAn" && e.Value != null) {
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                e.CellStyle.ForeColor = Color.FromArgb(17, 24, 39);
            }
            // Loại món (hiển thị tên loại thay vì mã)
            else if (colName == "MaLoaiMon" && e.Value != null) {
                string maLoai = e.Value.ToString();
                var found = _allLoaiMon.FirstOrDefault(x => x.MaLoaiMon == maLoai);
                if (found != null && !string.IsNullOrEmpty(found.TenLoaiMon)) {
                    e.Value = found.TenLoaiMon;
                    e.FormattingApplied = true;
                }
                e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            }
            // Đơn giá
            else if (colName == "Gia" && e.Value != null) {
                if (decimal.TryParse(e.Value.ToString(), out decimal gia)) {
                    e.Value = $"{gia:N0} đ";
                    e.FormattingApplied = true;
                }
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                e.CellStyle.ForeColor = Color.FromArgb(15, 23, 42);
            }
            // Giá vốn ước tính (COGS)
            else if (colName == "GiaVonUocTinh" && e.Value != null) {
                if (decimal.TryParse(e.Value.ToString(), out decimal giaVon)) {
                    e.Value = $"{giaVon:N0} đ";
                    e.FormattingApplied = true;
                }
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                e.CellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            }
            // Số lượng tồn khả dụng (suất)
            else if (colName == "SoLuongTon" && e.Value != null) {
                if (int.TryParse(e.Value.ToString(), out int sl)) {
                    if (sl <= 0) {
                        e.Value = "0 (Hết NL)";
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    } else if (sl <= 5) {
                        e.Value = $"{sl} suất (Sắp hết)";
                        e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6);
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    } else {
                        e.Value = $"{sl} suất";
                        e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                    e.FormattingApplied = true;
                }
            }
            // Trạng thái
            else if (colName == "TrangThaiRaw" && e.Value != null) {
                string val = e.Value.ToString();
                if (val == "Discontinued" || val == "Inactive") {
                    e.Value = "● Ngừng kinh doanh";
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.FormattingApplied = true;
                } else if (val == "OutOfStock") {
                    e.Value = "● Hết hàng";
                    e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.FormattingApplied = true;
                } else {
                    e.Value = "● Đang kinh doanh";
                    e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
            }
        }

        private void UpdateButtonState() {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = dgvMonAn.SelectedRows[0].DataBoundItem as MonAnDto;
                if (monAn != null) {
                    btnSua.Enabled = true;
                    btnNgungKinhDoanh.Enabled = true;

                    bool isNgungKinhDoanh = monAn.TrangThaiRaw == "Discontinued" || monAn.TrangThaiRaw == "Inactive";
                    if (isNgungKinhDoanh) {
                        btnNgungKinhDoanh.Text = "♻ Kinh Doanh Lại";
                        btnNgungKinhDoanh.BackColor = Color.FromArgb(240, 253, 244);
                        btnNgungKinhDoanh.FlatAppearance.BorderColor = Color.FromArgb(187, 247, 208);
                        btnNgungKinhDoanh.ForeColor = Color.FromArgb(22, 163, 74);
                    } else {
                        btnNgungKinhDoanh.Text = "🛑 Ngừng Kinh Doanh";
                        btnNgungKinhDoanh.BackColor = Color.FromArgb(254, 242, 242);
                        btnNgungKinhDoanh.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
                        btnNgungKinhDoanh.ForeColor = Color.FromArgb(220, 38, 38);
                    }

                    string catName = _allLoaiMon.FirstOrDefault(x => x.MaLoaiMon == monAn.MaLoaiMon)?.TenLoaiMon ?? monAn.MaLoaiMon;
                    lblSelectionInfo.Text = $"Đang chọn: {monAn.TenMonAn} ({monAn.MaMonAn})  ·  Loại: {catName}  ·  Giá bán: {monAn.Gia:N0} đ  ·  Giá vốn: {monAn.GiaVonUocTinh:N0} đ  ·  Khả dụng: {monAn.SoLuongTon} suất";
                    return;
                }
            }

            btnSua.Enabled = false;
            btnNgungKinhDoanh.Enabled = false;
            btnNgungKinhDoanh.Text = "🛑 Ngừng Kinh Doanh";
            btnNgungKinhDoanh.BackColor = Color.FromArgb(241, 245, 249);
            btnNgungKinhDoanh.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnNgungKinhDoanh.ForeColor = Color.FromArgb(148, 163, 184);
            lblSelectionInfo.Text = "Chưa chọn món ăn nào trên bảng";
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboLoaiMon_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboTrangThai_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            btnLamMoi.Enabled = false;
            try {
                await LoadLoaiMon();
                await LoadMonAn();
            } finally {
                btnLamMoi.Enabled = true;
            }
        }

        private async void btnThem_Click(object sender, EventArgs e) {
            var dialog = new MonAnDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                await LoadMonAn();
            }
        }

        private async void btnSua_Click(object sender, EventArgs e) {
            if (dgvMonAn.SelectedRows.Count > 0) {
                var monAn = (MonAnDto)dgvMonAn.SelectedRows[0].DataBoundItem;
                var dialog = new MonAnDialog(monAn);
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadMonAn(monAn.MaMonAn);
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
