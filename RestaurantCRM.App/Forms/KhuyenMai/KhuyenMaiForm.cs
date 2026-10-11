using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhuyenMai {
    public class KhuyenMaiForm : Form {
        private readonly KhuyenMaiService _service;

        private List<ChuongTrinhDto> _chuongTrinhList = new List<ChuongTrinhDto>();
        private List<VoucherDto> _voucherList = new List<VoucherDto>();
        private List<KmMonDto> _kmMonList = new List<KmMonDto>();

        // UI
        private Label _lblStatCT;
        private Label _lblStatVoucher;
        private Label _lblStatMon;
        private TabControl _tabMain;

        // DataGridViews
        private DataGridView _dgvChuongTrinh;
        private DataGridView _dgvVoucher;
        private DataGridView _dgvKmMon;

        public KhuyenMaiForm() {
            _service = new KhuyenMaiService();

            Text = "Quản lý Khuyến Mãi & Voucher";
            Size = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitFormLayout();
            LoadAllDataAsync();
        }

        private void InitFormLayout() {
            // Header panel
            var header = new Panel {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(24, 16, 24, 16)
            };

            var lblTitle = new Label {
                Text = "QUẢN LÝ KHUYẾN MÃI & VOUCHER",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Location = new Point(24, 16),
                AutoSize = true
            };
            var lblSub = new Label {
                Text = "Tạo chương trình ưu đãi, voucher giảm giá đơn hàng và áp dụng khuyến mãi tự động cho từng món ăn.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(26, 48),
                AutoSize = true
            };

            // Buttons on Top Header
            var btnTaoVoucher = new Button {
                Text = "🎟️ + TẠO VOUCHER / ƯU ĐÃI",
                Size = new Size(220, 42),
                Location = new Point(header.Width - 480, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = ThemeManager.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTaoVoucher.FlatAppearance.BorderSize = 0;
            btnTaoVoucher.Click += (s, e) => OpenTaoVoucherDialog();

            var btnTaoCT = new Button {
                Text = "📋 + Thêm chương trình",
                Size = new Size(160, 42),
                Location = new Point(header.Width - 250, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(235, 243, 255),
                ForeColor = ThemeManager.Blue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTaoCT.FlatAppearance.BorderSize = 0;
            btnTaoCT.Click += (s, e) => OpenTaoChuongTrinhDialog();

            var btnRefresh = new Button {
                Text = "🔄 Tải lại",
                Size = new Size(75, 42),
                Location = new Point(header.Width - 80, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(240, 243, 248),
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadAllDataAsync();

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            header.Controls.Add(btnTaoVoucher);
            header.Controls.Add(btnTaoCT);
            header.Controls.Add(btnRefresh);
            Controls.Add(header);

            // Summary Stats Panel
            var pnlStats = new Panel {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(24, 10, 24, 10),
                BackColor = ThemeManager.Bg
            };

            _lblStatCT = CreateStatCard(pnlStats, "Chương trình đang áp dụng", "0", 0, ThemeManager.Blue);
            _lblStatVoucher = CreateStatCard(pnlStats, "Voucher toàn đơn", "0", 250, ThemeManager.Purple);
            _lblStatMon = CreateStatCard(pnlStats, "Món ăn đang giảm giá", "0", 500, ThemeManager.Green);
            Controls.Add(pnlStats);

            // Main TabControl
            _tabMain = new TabControl {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Point(16, 8)
            };

            // 1. Tab Chương trình
            var tabCT = new TabPage("📋 Chương trình khuyến mãi");
            InitTabChuongTrinh(tabCT);
            _tabMain.TabPages.Add(tabCT);

            // 2. Tab Voucher toàn đơn
            var tabVoucher = new TabPage("🎟️ Voucher toàn hóa đơn");
            InitTabVoucher(tabVoucher);
            _tabMain.TabPages.Add(tabVoucher);

            // 3. Tab Món khuyến mãi
            var tabMon = new TabPage("🍲 Ưu đãi theo món ăn");
            InitTabMon(tabMon);
            _tabMain.TabPages.Add(tabMon);

            Controls.Add(_tabMain);
            _tabMain.BringToFront();
        }

        private Label CreateStatCard(Panel parent, string title, string val, int x, Color color) {
            var card = new Panel {
                Location = new Point(x, 6),
                Size = new Size(230, 52),
                BackColor = ThemeManager.Paper
            };
            var lblT = new Label { Text = title, Location = new Point(10, 6), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = ThemeManager.Muted };
            var lblV = new Label { Text = val, Location = new Point(10, 24), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = color };
            card.Controls.Add(lblT);
            card.Controls.Add(lblV);
            parent.Controls.Add(card);
            return lblV;
        }

        // ================= TAB 1: CHƯƠNG TRÌNH KHUYẾN MÃI =================
        private void InitTabChuongTrinh(TabPage tab) {
            tab.BackColor = ThemeManager.Bg;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 5, 10, 5), BackColor = ThemeManager.Paper };
            var btnAdd = CreateToolButton("➕ Thêm chương trình", 0, ThemeManager.Blue, Color.White);
            btnAdd.Click += (s, e) => OpenTaoChuongTrinhDialog();

            var btnEdit = CreateToolButton("✏️ Chỉnh sửa", 170, Color.FromArgb(240, 243, 248), ThemeManager.Ink);
            btnEdit.Click += (s, e) => EditSelectedChuongTrinh();

            var btnDelete = CreateToolButton("🗑️ Xóa", 270, Color.FromArgb(254, 243, 242), ThemeManager.Red);
            btnDelete.Click += async (s, e) => await DeleteSelectedChuongTrinhAsync();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDelete);
            tab.Controls.Add(toolbar);

            _dgvChuongTrinh = CreateStyledGrid();
            _dgvChuongTrinh.Columns.Add("MaChuongTrinh", "Mã CT");
            _dgvChuongTrinh.Columns.Add("TenChuongTrinh", "Tên chương trình khuyến mãi");
            _dgvChuongTrinh.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvChuongTrinh.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvChuongTrinh.Columns.Add("TrangThai", "Trạng thái");
            _dgvChuongTrinh.Columns.Add("SoLuongVoucher", "Số Voucher");
            _dgvChuongTrinh.Columns.Add("SoLuongMonKm", "Số món KM");

            _dgvChuongTrinh.Columns["MaChuongTrinh"].Width = 120;
            _dgvChuongTrinh.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvChuongTrinh.Columns["NgayBatDau"].Width = 120;
            _dgvChuongTrinh.Columns["NgayKetThuc"].Width = 120;
            _dgvChuongTrinh.Columns["TrangThai"].Width = 130;
            _dgvChuongTrinh.Columns["SoLuongVoucher"].Width = 110;
            _dgvChuongTrinh.Columns["SoLuongMonKm"].Width = 110;

            _dgvChuongTrinh.CellFormatting += Dgv_CellFormatting;
            tab.Controls.Add(_dgvChuongTrinh);
            _dgvChuongTrinh.BringToFront();
        }

        // ================= TAB 2: VOUCHER TOÀN HÓA ĐƠN =================
        private void InitTabVoucher(TabPage tab) {
            tab.BackColor = ThemeManager.Bg;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 5, 10, 5), BackColor = ThemeManager.Paper };
            var btnAdd = CreateToolButton("➕ Thêm Voucher mới", 0, ThemeManager.Purple, Color.White);
            btnAdd.Click += (s, e) => OpenTaoVoucherDialog();

            var btnDelete = CreateToolButton("🗑️ Xóa Voucher", 170, Color.FromArgb(254, 243, 242), ThemeManager.Red);
            btnDelete.Click += async (s, e) => await DeleteSelectedVoucherAsync();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnDelete);
            tab.Controls.Add(toolbar);

            _dgvVoucher = CreateStyledGrid();
            _dgvVoucher.Columns.Add("MaVoucher", "Mã Voucher");
            _dgvVoucher.Columns.Add("TenChuongTrinh", "Chương trình áp dụng");
            _dgvVoucher.Columns.Add("HinhThuc", "Mức giảm giá");
            _dgvVoucher.Columns.Add("GiaTriDonToiThieu", "Đơn tối thiểu");
            _dgvVoucher.Columns.Add("GiamToiDa", "Giảm tối đa");
            _dgvVoucher.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvVoucher.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvVoucher.Columns.Add("TrangThai", "Trạng thái");

            _dgvVoucher.Columns["MaVoucher"].Width = 150;
            _dgvVoucher.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvVoucher.Columns["HinhThuc"].Width = 130;
            _dgvVoucher.Columns["GiaTriDonToiThieu"].Width = 130;
            _dgvVoucher.Columns["GiamToiDa"].Width = 130;
            _dgvVoucher.Columns["NgayBatDau"].Width = 110;
            _dgvVoucher.Columns["NgayKetThuc"].Width = 110;
            _dgvVoucher.Columns["TrangThai"].Width = 120;

            _dgvVoucher.CellFormatting += Dgv_CellFormatting;
            tab.Controls.Add(_dgvVoucher);
            _dgvVoucher.BringToFront();
        }

        // ================= TAB 3: ƯU ĐÃI THEO MÓN ĂN =================
        private void InitTabMon(TabPage tab) {
            tab.BackColor = ThemeManager.Bg;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(10, 5, 10, 5), BackColor = ThemeManager.Paper };
            var btnAdd = CreateToolButton("➕ Thêm ưu đãi món", 0, ThemeManager.Green, Color.White);
            btnAdd.Click += (s, e) => OpenTaoVoucherDialog(true);

            var btnDelete = CreateToolButton("🗑️ Hủy ưu đãi món", 170, Color.FromArgb(254, 243, 242), ThemeManager.Red);
            btnDelete.Click += async (s, e) => await DeleteSelectedKmMonAsync();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnDelete);
            tab.Controls.Add(toolbar);

            _dgvKmMon = CreateStyledGrid();
            _dgvKmMon.Columns.Add("MaMon", "Mã món");
            _dgvKmMon.Columns.Add("TenMon", "Tên món ăn");
            _dgvKmMon.Columns.Add("TenChuongTrinh", "Chương trình áp dụng");
            _dgvKmMon.Columns.Add("DonGiaGoc", "Giá gốc");
            _dgvKmMon.Columns.Add("HinhThuc", "Mức giảm");
            _dgvKmMon.Columns.Add("DonGiaSauGiam", "Giá sau ưu đãi");
            _dgvKmMon.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvKmMon.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvKmMon.Columns.Add("TrangThai", "Trạng thái");

            _dgvKmMon.Columns["MaMon"].Width = 100;
            _dgvKmMon.Columns["TenMon"].Width = 180;
            _dgvKmMon.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvKmMon.Columns["DonGiaGoc"].Width = 110;
            _dgvKmMon.Columns["HinhThuc"].Width = 110;
            _dgvKmMon.Columns["DonGiaSauGiam"].Width = 130;
            _dgvKmMon.Columns["NgayBatDau"].Width = 105;
            _dgvKmMon.Columns["NgayKetThuc"].Width = 105;
            _dgvKmMon.Columns["TrangThai"].Width = 115;

            _dgvKmMon.CellFormatting += Dgv_CellFormatting;
            tab.Controls.Add(_dgvKmMon);
            _dgvKmMon.BringToFront();
        }

        private Button CreateToolButton(string text, int x, Color bg, Color fg) {
            var btn = new Button {
                Text = text,
                Location = new Point(x, 5),
                Size = new Size(155, 34),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private DataGridView CreateStyledGrid() {
            var grid = new DataGridView {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeManager.Paper,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 36 },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 38,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(238, 242, 246)
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Ink;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(234, 242, 255);
            grid.DefaultCellStyle.SelectionForeColor = ThemeManager.Ink;
            return grid;
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            var grid = sender as DataGridView;
            if (grid == null) return;

            string colName = grid.Columns[e.ColumnIndex].Name;
            if (colName == "TrangThai" && e.Value != null) {
                string status = e.Value.ToString();
                if (status == "Đang áp dụng") {
                    e.CellStyle.ForeColor = ThemeManager.Green;
                    e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                } else if (status == "Đã kết thúc") {
                    e.CellStyle.ForeColor = ThemeManager.Muted;
                } else if (status == "Chưa diễn ra") {
                    e.CellStyle.ForeColor = ThemeManager.Amber;
                }
            } else if ((colName == "DonGiaGoc" || colName == "DonGiaSauGiam" || colName == "GiaTriDonToiThieu" || colName == "GiamToiDa") && e.Value != null) {
                if (double.TryParse(e.Value.ToString(), out var num)) {
                    if (num > 0) e.Value = num.ToString("N0") + " đ";
                    else if (colName == "GiaTriDonToiThieu" || colName == "GiamToiDa") e.Value = "Không giới hạn";
                }
            }
        }

        // ================= DATA LOADING =================
        private async void LoadAllDataAsync() {
            try {
                Cursor = Cursors.WaitCursor;

                _chuongTrinhList = await _service.GetChuongTrinhListAsync();
                _voucherList = await _service.GetVoucherListAsync();
                _kmMonList = await _service.GetKmMonListAsync();

                // 1. Fill ChuongTrinh
                _dgvChuongTrinh.Rows.Clear();
                foreach (var ct in _chuongTrinhList) {
                    _dgvChuongTrinh.Rows.Add(
                        ct.MaChuongTrinh,
                        ct.TenChuongTrinh,
                        string.IsNullOrEmpty(ct.NgayBatDau) ? "-" : ct.NgayBatDau,
                        string.IsNullOrEmpty(ct.NgayKetThuc) ? "Vô thời hạn" : ct.NgayKetThuc,
                        ct.TrangThai,
                        ct.SoLuongVoucher,
                        ct.SoLuongMonKm
                    );
                }

                // 2. Fill Voucher
                _dgvVoucher.Rows.Clear();
                foreach (var v in _voucherList) {
                    _dgvVoucher.Rows.Add(
                        v.MaVoucher,
                        $"[{v.MaChuongTrinh}] {v.TenChuongTrinh}",
                        v.HinhThuc,
                        v.GiaTriDonToiThieu ?? 0,
                        v.GiamToiDa ?? 0,
                        string.IsNullOrEmpty(v.NgayBatDau) ? "-" : v.NgayBatDau,
                        string.IsNullOrEmpty(v.NgayKetThuc) ? "Vô thời hạn" : v.NgayKetThuc,
                        v.TrangThai
                    );
                }

                // 3. Fill KmMon
                _dgvKmMon.Rows.Clear();
                foreach (var m in _kmMonList) {
                    _dgvKmMon.Rows.Add(
                        m.MaMon,
                        m.TenMon,
                        $"[{m.MaChuongTrinh}] {m.TenChuongTrinh}",
                        m.DonGiaGoc,
                        m.HinhThuc,
                        m.DonGiaSauGiam,
                        string.IsNullOrEmpty(m.NgayBatDau) ? "-" : m.NgayBatDau,
                        string.IsNullOrEmpty(m.NgayKetThuc) ? "Vô thời hạn" : m.NgayKetThuc,
                        m.TrangThai
                    );
                }

                // Update Stats
                _lblStatCT.Text = _chuongTrinhList.Count(x => x.TrangThai == "Đang áp dụng").ToString() + " / " + _chuongTrinhList.Count;
                _lblStatVoucher.Text = _voucherList.Count.ToString();
                _lblStatMon.Text = _kmMonList.Count.ToString();
            } catch (Exception ex) {
                MessageBox.Show(this, "Lỗi khi tải dữ liệu khuyến mãi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                Cursor = Cursors.Default;
            }
        }

        // ================= ACTIONS =================
        private void OpenTaoChuongTrinhDialog() {
            using (var dlg = new ChuongTrinhDialog(_service)) {
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    LoadAllDataAsync();
                }
            }
        }

        private void EditSelectedChuongTrinh() {
            if (_dgvChuongTrinh.SelectedRows.Count == 0) {
                MessageBox.Show(this, "Vui lòng chọn chương trình cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maCT = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            var item = _chuongTrinhList.FirstOrDefault(x => x.MaChuongTrinh == maCT);
            if (item == null) return;

            using (var dlg = new ChuongTrinhDialog(_service, item)) {
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    LoadAllDataAsync();
                }
            }
        }

        private async Task DeleteSelectedChuongTrinhAsync() {
            if (_dgvChuongTrinh.SelectedRows.Count == 0) {
                MessageBox.Show(this, "Vui lòng chọn chương trình cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maCT = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            string tenCT = _dgvChuongTrinh.SelectedRows[0].Cells["TenChuongTrinh"].Value?.ToString();

            var confirm = MessageBox.Show(this, $"Bạn có chắc chắn muốn xóa chương trình '{tenCT}' không?\n(Lưu ý: Không thể xóa nếu voucher đã được dùng trong đơn hàng)", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteChuongTrinhAsync(maCT);
            if (ok) {
                MessageBox.Show(this, "Đã xóa chương trình khuyến mãi.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            } else {
                MessageBox.Show(this, msg, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenTaoVoucherDialog(bool defaultTheoMon = false) {
            string preselectedCtId = null;
            if (_dgvChuongTrinh.SelectedRows.Count > 0) {
                preselectedCtId = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            }
            using (var dlg = new TaoVoucherDialog(_service, preselectedCtId, defaultTheoMon)) {
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    LoadAllDataAsync();
                }
            }
        }

        private async Task DeleteSelectedVoucherAsync() {
            if (_dgvVoucher.SelectedRows.Count == 0) {
                MessageBox.Show(this, "Vui lòng chọn Voucher cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maVoucher = _dgvVoucher.SelectedRows[0].Cells["MaVoucher"].Value?.ToString();
            var item = _voucherList.FirstOrDefault(x => x.MaVoucher == maVoucher);
            if (item == null) return;

            var confirm = MessageBox.Show(this, $"Bạn có chắc chắn muốn xóa Voucher '{maVoucher}' không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteVoucherAsync(item.MaKmvoucher);
            if (ok) {
                MessageBox.Show(this, $"Đã xóa voucher '{maVoucher}'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            } else {
                MessageBox.Show(this, msg, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteSelectedKmMonAsync() {
            if (_dgvKmMon.SelectedRows.Count == 0) {
                MessageBox.Show(this, "Vui lòng chọn món ưu đãi cần hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maMon = _dgvKmMon.SelectedRows[0].Cells["MaMon"].Value?.ToString();
            var item = _kmMonList.FirstOrDefault(x => x.MaMon == maMon);
            if (item == null) return;

            var confirm = MessageBox.Show(this, $"Bạn có chắc muốn hủy ưu đãi cho món '{item.TenMon}' không?", "Xác nhận hủy ưu đãi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteKmMonAsync(item.MaKmsp);
            if (ok) {
                MessageBox.Show(this, $"Đã hủy ưu đãi cho món '{item.TenMon}'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            } else {
                MessageBox.Show(this, msg, "Không thể hủy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
