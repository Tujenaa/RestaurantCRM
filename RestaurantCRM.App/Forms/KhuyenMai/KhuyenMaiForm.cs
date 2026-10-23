using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhuyenMai
{
    public class KhuyenMaiForm : Form
    {
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

        public KhuyenMaiForm()
        {
            _service = new KhuyenMaiService();

            Text = "Quản lý Khuyến Mãi & Voucher";
            Size = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitFormLayout();
            LoadAllDataAsync();
        }

        private void InitFormLayout()
        {
            Padding = new Padding(14, 6, 14, 10);

            // ── Hàng đầu: tiêu đề + mô tả + nút ─────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblTitle = new Label
            {
                Text = "Quản lý khuyến mãi & voucher",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(10, 8),
                AutoSize = true,
                UseMnemonic = false
            };
            var lblSub = new Label
            {
                Text = "Tạo chương trình ưu đãi, voucher giảm giá đơn hàng và áp dụng khuyến mãi tự động cho từng món ăn.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(12, 44),
                AutoSize = true
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 14, 8, 0)
            };
            var btnTaoVoucher = new Button { Text = "+ Tạo voucher / ưu đãi" };
            ButtonSkin.SetKind(btnTaoVoucher, ButtonSkin.Kind.Primary);
            PageKit.FitButton(btnTaoVoucher);
            btnTaoVoucher.Margin = new Padding(8, 0, 0, 0);
            btnTaoVoucher.Click += (s, e) => OpenTaoVoucherDialog();

            var btnTaoCT = new Button { Text = "+ Thêm chương trình" };
            ButtonSkin.SetKind(btnTaoCT, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btnTaoCT);
            btnTaoCT.Margin = new Padding(8, 0, 0, 0);
            btnTaoCT.Click += (s, e) => OpenTaoChuongTrinhDialog();

            var btnRefresh = new Button { Text = "Làm mới" };
            ButtonSkin.SetKind(btnRefresh, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btnRefresh);
            btnRefresh.Margin = new Padding(8, 0, 0, 0);
            btnRefresh.Click += (s, e) => LoadAllDataAsync();

            actions.Controls.Add(btnTaoVoucher);
            actions.Controls.Add(btnTaoCT);
            actions.Controls.Add(btnRefresh);
            header.Controls.Add(actions);
            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            Action placeTitle = () => {
                bool embedded = !TopLevel;   // mở trong cửa sổ chính: tiêu đề đã nằm trên header chung
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 16 : 44);
                header.Height = embedded ? 54 : 76;
                actions.Padding = new Padding(0, embedded ? 4 : 14, 8, 0);
            };
            placeTitle();
            ParentChanged += (s, e) => placeTitle();

            // ── Thẻ KPI ─────────────────────────────────────────────────
            var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 104, ColumnCount = 3, RowCount = 1, BackColor = ThemeManager.Bg };
            kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            _lblStatCT = new Label { Text = "0" };
            _lblStatVoucher = new Label { Text = "0" };
            _lblStatMon = new Label { Text = "0" };
            kpis.Controls.Add(new KpiTile(_lblStatCT, "Chương trình đang áp dụng", ThemeManager.Primary) { Dock = DockStyle.Fill }, 0, 0);
            kpis.Controls.Add(new KpiTile(_lblStatVoucher, "Voucher toàn đơn", ThemeManager.Violet) { Dock = DockStyle.Fill }, 1, 0);
            kpis.Controls.Add(new KpiTile(_lblStatMon, "Món ăn đang giảm giá", ThemeManager.Green) { Dock = DockStyle.Fill }, 2, 0);

            // ── Thẻ trắng chứa các tab ──────────────────────────────────
            var card = PageKit.NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(8 + 20, 4 + 10, 8 + 20, 4 + 8 + 12);

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };

            // 1. Tab Chương trình
            var tabCT = new TabPage("Chương trình khuyến mãi");
            InitTabChuongTrinh(tabCT);
            _tabMain.TabPages.Add(tabCT);

            // 2. Tab Voucher toàn đơn
            var tabVoucher = new TabPage("Voucher toàn hóa đơn");
            InitTabVoucher(tabVoucher);
            _tabMain.TabPages.Add(tabVoucher);

            // 3. Tab Món khuyến mãi
            var tabMon = new TabPage("Ưu đãi theo món ăn");
            InitTabMon(tabMon);
            _tabMain.TabPages.Add(tabMon);

            card.Controls.Add(_tabMain);
            Controls.Add(card);
            Controls.Add(kpis);
            Controls.Add(header);
            card.BringToFront();
        }

        // ================= TAB 1: CHƯƠNG TRÌNH KHUYẾN MÃI =================
        private void InitTabChuongTrinh(TabPage tab)
        {
            tab.BackColor = Color.White;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 6, 0, 12), BackColor = Color.White };
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
            PageKit.FitColumns(_dgvChuongTrinh);
            tab.Controls.Add(_dgvChuongTrinh);
            _dgvChuongTrinh.BringToFront();
        }

        // ================= TAB 2: VOUCHER TOÀN HÓA ĐƠN =================
        private void InitTabVoucher(TabPage tab)
        {
            tab.BackColor = Color.White;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 6, 0, 12), BackColor = Color.White };
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
            PageKit.FitColumns(_dgvVoucher);
            tab.Controls.Add(_dgvVoucher);
            _dgvVoucher.BringToFront();
        }

        // ================= TAB 3: ƯU ĐÃI THEO MÓN ĂN =================
        private void InitTabMon(TabPage tab)
        {
            tab.BackColor = Color.White;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 6, 0, 12), BackColor = Color.White };
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
            PageKit.FitColumns(_dgvKmMon);
            tab.Controls.Add(_dgvKmMon);
            _dgvKmMon.BringToFront();
        }

        private Button CreateToolButton(string text, int x, Color bg, Color fg)
        {
            var btn = new Button { Text = text };
            // Nút đầu tiên (thêm) là nút chính; nút đỏ là nút xóa; còn lại nút phụ
            if (fg == ThemeManager.Red) ButtonSkin.SetKind(btn, ButtonSkin.Kind.Danger);
            else if (fg == Color.White) ButtonSkin.SetKind(btn, ButtonSkin.Kind.Primary);
            else ButtonSkin.SetKind(btn, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btn);
            int left = 0;
            // xếp nối tiếp theo thứ tự tạo, cách nhau 8px (bỏ qua tọa độ x cố định cũ)
            if (x > 0) left = _toolX + 8;
            btn.Location = new Point(left, 6);
            _toolX = btn.Right;
            // Khi nút tự giãn theo chữ, xếp lại các nút trên cùng thanh công cụ
            btn.SizeChanged += (s, e) => {
                var p = btn.Parent;
                if (p == null) return;
                int lx = 0;
                foreach (Control c in p.Controls) if (c is Button) { c.Left = lx; lx = c.Right + 8; }
            };
            return btn;
        }
        private int _toolX;

        private DataGridView CreateStyledGrid()
        {
            var grid = new DataGridView
            {
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
                RowTemplate = { Height = 46 },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 44,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(238, 242, 246)
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            grid.DefaultCellStyle.SelectionForeColor = ThemeManager.Ink;
            UiStyler.StyleGrid(grid);
            return grid;
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;

            string colName = grid.Columns[e.ColumnIndex].Name;
            if (colName == "TrangThai" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Đang áp dụng")
                {
                    e.CellStyle.ForeColor = ThemeManager.Green;
                    e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
                else if (status == "Đã kết thúc")
                {
                    e.CellStyle.ForeColor = ThemeManager.Muted;
                }
                else if (status == "Chưa diễn ra")
                {
                    e.CellStyle.ForeColor = ThemeManager.Amber;
                }
            }
            else if ((colName == "DonGiaGoc" || colName == "DonGiaSauGiam" || colName == "GiaTriDonToiThieu" || colName == "GiamToiDa") && e.Value != null)
            {
                if (double.TryParse(e.Value.ToString(), out var num))
                {
                    if (num > 0) e.Value = num.ToString("N0") + " đ";
                    else if (colName == "GiaTriDonToiThieu" || colName == "GiamToiDa") e.Value = "Không giới hạn";
                }
            }
        }

        // ================= DATA LOADING =================
        private async void LoadAllDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _chuongTrinhList = await _service.GetChuongTrinhListAsync();
                _voucherList = await _service.GetVoucherListAsync();
                _kmMonList = await _service.GetKmMonListAsync();

                // 1. Fill ChuongTrinh
                _dgvChuongTrinh.Rows.Clear();
                foreach (var ct in _chuongTrinhList)
                {
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
                foreach (var v in _voucherList)
                {
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
                foreach (var m in _kmMonList)
                {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi khi tải dữ liệu khuyến mãi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ================= ACTIONS =================
        private void OpenTaoChuongTrinhDialog()
        {
            using (var dlg = new ChuongTrinhDialog(_service))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllDataAsync();
                }
            }
        }

        private void EditSelectedChuongTrinh()
        {
            if (_dgvChuongTrinh.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn chương trình cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maCT = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            var item = _chuongTrinhList.FirstOrDefault(x => x.MaChuongTrinh == maCT);
            if (item == null) return;

            using (var dlg = new ChuongTrinhDialog(_service, item))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllDataAsync();
                }
            }
        }

        private async Task DeleteSelectedChuongTrinhAsync()
        {
            if (_dgvChuongTrinh.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn chương trình cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maCT = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            string tenCT = _dgvChuongTrinh.SelectedRows[0].Cells["TenChuongTrinh"].Value?.ToString();

            var confirm = MessageBox.Show(this, $"Bạn có chắc chắn muốn xóa chương trình '{tenCT}' không?\n(Lưu ý: Không thể xóa nếu voucher đã được dùng trong đơn hàng)", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteChuongTrinhAsync(maCT);
            if (ok)
            {
                MessageBox.Show(this, "Đã xóa chương trình khuyến mãi.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show(this, msg, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenTaoVoucherDialog(bool defaultTheoMon = false)
        {
            string preselectedCtId = null;
            if (_dgvChuongTrinh.SelectedRows.Count > 0)
            {
                preselectedCtId = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
            }
            using (var dlg = new TaoVoucherDialog(_service, preselectedCtId, defaultTheoMon))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllDataAsync();
                }
            }
        }

        private async Task DeleteSelectedVoucherAsync()
        {
            if (_dgvVoucher.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn Voucher cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maVoucher = _dgvVoucher.SelectedRows[0].Cells["MaVoucher"].Value?.ToString();
            var item = _voucherList.FirstOrDefault(x => x.MaVoucher == maVoucher);
            if (item == null) return;

            var confirm = MessageBox.Show(this, $"Bạn có chắc chắn muốn xóa Voucher '{maVoucher}' không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteVoucherAsync(item.MaKmvoucher);
            if (ok)
            {
                MessageBox.Show(this, $"Đã xóa voucher '{maVoucher}'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show(this, msg, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteSelectedKmMonAsync()
        {
            if (_dgvKmMon.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn món ưu đãi cần hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maMon = _dgvKmMon.SelectedRows[0].Cells["MaMon"].Value?.ToString();
            var item = _kmMonList.FirstOrDefault(x => x.MaMon == maMon);
            if (item == null) return;

            var confirm = MessageBox.Show(this, $"Bạn có chắc muốn hủy ưu đãi cho món '{item.TenMon}' không?", "Xác nhận hủy ưu đãi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteKmMonAsync(item.MaKmsp);
            if (ok)
            {
                MessageBox.Show(this, $"Đã hủy ưu đãi cho món '{item.TenMon}'.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show(this, msg, "Không thể hủy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}