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

        // UI Stats
        private Label _lblStatCT;
        private Label _lblStatVoucherDon;
        private Label _lblStatVoucherLoai;
        private Label _lblStatVoucherMon;

        private TabControl _tabMain;

        // DataGridViews cho 4 tab
        private DataGridView _dgvChuongTrinh;
        private DataGridView _dgvVoucherDon;
        private DataGridView _dgvVoucherLoai;
        private DataGridView _dgvVoucherMon;

        // Controls tìm kiếm & lọc trạng thái cho 4 tab
        private TextBox _txtSearchCT;
        private ComboBox _cboStatusCT;

        private TextBox _txtSearchDon;
        private ComboBox _cboStatusDon;

        private TextBox _txtSearchLoai;
        private ComboBox _cboStatusLoai;

        private TextBox _txtSearchMon;
        private ComboBox _cboStatusMon;

        public KhuyenMaiForm()
        {
            _service = new KhuyenMaiService();

            Text = "Quản lý Khuyến Mãi & Voucher";
            Size = new Size(1220, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitFormLayout();
            LoadAllDataAsync();
        }

        private void InitFormLayout()
        {
            Padding = new Padding(14, 6, 14, 10);

            // ── 1. Header (Tiêu đề + Thao tác nhanh) ─────────────────────
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
                Text = "Quản trị chương trình ưu đãi, voucher toàn hóa đơn, voucher theo nhóm loại món và theo từng món ăn.",
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

            var btnTaoVoucher = new Button { Text = "+ Tạo Voucher mới" };
            ButtonSkin.SetKind(btnTaoVoucher, ButtonSkin.Kind.Primary);
            PageKit.FitButton(btnTaoVoucher);
            btnTaoVoucher.Margin = new Padding(8, 0, 0, 0);
            btnTaoVoucher.Click += (s, e) => OpenTaoVoucherDialog();

            var btnTaoCT = new Button { Text = "+ Thêm chương trình" };
            ButtonSkin.SetKind(btnTaoCT, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btnTaoCT);
            btnTaoCT.Margin = new Padding(8, 0, 0, 0);
            btnTaoCT.Click += (s, e) => OpenTaoChuongTrinhDialog();

            var btnRefresh = new Button { Text = "Làm mới dữ liệu" };
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
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 16 : 44);
                header.Height = embedded ? 54 : 76;
                actions.Padding = new Padding(0, embedded ? 4 : 14, 8, 0);
            };
            placeTitle();
            ParentChanged += (s, e) => placeTitle();

            // ── 2. Thẻ KPI Thống kê ──────────────────────────────────────
            var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 96, ColumnCount = 4, RowCount = 1, BackColor = ThemeManager.Bg };
            kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            _lblStatCT = new Label { Text = "0" };
            _lblStatVoucherDon = new Label { Text = "0" };
            _lblStatVoucherLoai = new Label { Text = "0" };
            _lblStatVoucherMon = new Label { Text = "0" };

            kpis.Controls.Add(new KpiTile(_lblStatCT, "Chương trình đang chạy", ThemeManager.Primary) { Dock = DockStyle.Fill }, 0, 0);
            kpis.Controls.Add(new KpiTile(_lblStatVoucherDon, "Voucher toàn đơn", ThemeManager.Violet) { Dock = DockStyle.Fill }, 1, 0);
            kpis.Controls.Add(new KpiTile(_lblStatVoucherLoai, "Voucher theo loại món", ThemeManager.Amber) { Dock = DockStyle.Fill }, 2, 0);
            kpis.Controls.Add(new KpiTile(_lblStatVoucherMon, "Voucher theo món ăn", ThemeManager.Green) { Dock = DockStyle.Fill }, 3, 0);

            // ── 3. Khung Card chứa 4 Tab ────────────────────────────────
            var card = PageKit.NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(16, 12, 16, 16);

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };

            // 1. Tab Chương trình khuyến mãi
            var tabCT = new TabPage("Chương trình khuyến mãi");
            InitTabChuongTrinh(tabCT);
            _tabMain.TabPages.Add(tabCT);

            // 2. Tab Voucher theo đơn hàng
            var tabDon = new TabPage("Voucher theo đơn hàng");
            InitTabVoucherDon(tabDon);
            _tabMain.TabPages.Add(tabDon);

            // 3. Tab Voucher theo loại món
            var tabLoai = new TabPage("Voucher theo loại món");
            InitTabVoucherLoai(tabLoai);
            _tabMain.TabPages.Add(tabLoai);

            // 4. Tab Voucher theo món ăn
            var tabMon = new TabPage("Voucher theo món ăn");
            InitTabVoucherMon(tabMon);
            _tabMain.TabPages.Add(tabMon);

            card.Controls.Add(_tabMain);
            Controls.Add(card);
            Controls.Add(kpis);
            Controls.Add(header);
            card.BringToFront();
        }

        // ================= HELPER TẠO THANH CÔNG CỤ (CHỐNG CHỒNG LẤN) =================
        private Panel CreateTabToolbar(
            Button btnAdd, Button btnEdit, Button btnDelete,
            TextBox txtSearch, ComboBox cboStatus,
            string searchPlaceholder)
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };

            // Bên trái: Cụm nút Thêm, Sửa, Xóa
            var leftFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 4)
            };
            if (btnAdd != null) leftFlow.Controls.Add(btnAdd);
            if (btnEdit != null) leftFlow.Controls.Add(btnEdit);
            if (btnDelete != null) leftFlow.Controls.Add(btnDelete);

            // Bên phải: Ô tìm kiếm + Dropdown lọc trạng thái (Chống chồng lấn)
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 430,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 4)
            };

            cboStatus.Location = new Point(280, 8);
            cboStatus.Width = 145;
            cboStatus.Height = 32;
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang áp dụng", "Chưa diễn ra", "Đã kết thúc" });
            cboStatus.SelectedIndex = 0;

            txtSearch.Location = new Point(10, 8);
            txtSearch.Width = 260;
            txtSearch.Height = 32;
            txtSearch.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            UiStyler.SetPlaceholder(txtSearch, searchPlaceholder);

            rightPanel.Controls.Add(txtSearch);
            rightPanel.Controls.Add(cboStatus);

            toolbar.Controls.Add(rightPanel);
            toolbar.Controls.Add(leftFlow);
            return toolbar;
        }

        // ================= TAB 1: CHƯƠNG TRÌNH KHUYẾN MÃI =================
        private void InitTabChuongTrinh(TabPage tab)
        {
            tab.BackColor = Color.White;

            var btnAdd = CreateToolButton("➕ Thêm chương trình", ButtonSkin.Kind.Primary);
            btnAdd.Click += (s, e) => OpenTaoChuongTrinhDialog();

            var btnEdit = CreateToolButton("✏️ Chỉnh sửa", ButtonSkin.Kind.Secondary);
            btnEdit.Click += (s, e) => EditSelectedChuongTrinh();

            var btnDelete = CreateToolButton("🗑️ Xóa / Dừng", ButtonSkin.Kind.Danger);
            btnDelete.Click += async (s, e) => await DeleteSelectedChuongTrinhAsync();

            _txtSearchCT = new TextBox();
            _cboStatusCT = new ComboBox();

            _txtSearchCT.TextChanged += (s, e) => RenderChuongTrinhGrid();
            _cboStatusCT.SelectedIndexChanged += (s, e) => RenderChuongTrinhGrid();

            var toolbar = CreateTabToolbar(btnAdd, btnEdit, btnDelete, _txtSearchCT, _cboStatusCT, "🔍 Tìm mã hoặc tên CTKM...");
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
            _dgvChuongTrinh.Columns["NgayBatDau"].Width = 110;
            _dgvChuongTrinh.Columns["NgayKetThuc"].Width = 110;
            _dgvChuongTrinh.Columns["TrangThai"].Width = 120;
            _dgvChuongTrinh.Columns["SoLuongVoucher"].Width = 100;
            _dgvChuongTrinh.Columns["SoLuongMonKm"].Width = 100;

            _dgvChuongTrinh.CellFormatting += Dgv_CellFormatting;
            PageKit.FitColumns(_dgvChuongTrinh);
            tab.Controls.Add(_dgvChuongTrinh);
            _dgvChuongTrinh.BringToFront();
        }

        // ================= TAB 2: VOUCHER THEO ĐƠN HÀNG (TOAN_DON) =================
        private void InitTabVoucherDon(TabPage tab)
        {
            tab.BackColor = Color.White;

            var btnAdd = CreateToolButton("➕ Thêm Voucher đơn", ButtonSkin.Kind.Primary);
            btnAdd.Click += (s, e) => OpenTaoVoucherDialog(scope: "TOAN_DON");

            var btnEdit = CreateToolButton("✏️ Chỉnh sửa", ButtonSkin.Kind.Secondary);
            btnEdit.Click += (s, e) => EditSelectedVoucher(_dgvVoucherDon);

            var btnDelete = CreateToolButton("🗑️ Xóa / Hủy mã", ButtonSkin.Kind.Danger);
            btnDelete.Click += async (s, e) => await DeleteSelectedVoucherAsync(_dgvVoucherDon);

            _txtSearchDon = new TextBox();
            _cboStatusDon = new ComboBox();

            _txtSearchDon.TextChanged += (s, e) => RenderVoucherDonGrid();
            _cboStatusDon.SelectedIndexChanged += (s, e) => RenderVoucherDonGrid();

            var toolbar = CreateTabToolbar(btnAdd, btnEdit, btnDelete, _txtSearchDon, _cboStatusDon, "🔍 Tìm mã voucher đơn hàng...");
            tab.Controls.Add(toolbar);

            _dgvVoucherDon = CreateStyledGrid();
            _dgvVoucherDon.Columns.Add("MaVoucher", "Mã Voucher");
            _dgvVoucherDon.Columns.Add("TenChuongTrinh", "Chương trình áp dụng");
            _dgvVoucherDon.Columns.Add("HinhThuc", "Mức giảm giá");
            _dgvVoucherDon.Columns.Add("GiaTriDonToiThieu", "Đơn tối thiểu");
            _dgvVoucherDon.Columns.Add("GiamToiDa", "Giảm tối đa");
            _dgvVoucherDon.Columns.Add("LuotDung", "Lượt áp dụng");
            _dgvVoucherDon.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvVoucherDon.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvVoucherDon.Columns.Add("TrangThai", "Trạng thái");

            _dgvVoucherDon.Columns["MaVoucher"].Width = 140;
            _dgvVoucherDon.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvVoucherDon.Columns["HinhThuc"].Width = 130;
            _dgvVoucherDon.Columns["GiaTriDonToiThieu"].Width = 130;
            _dgvVoucherDon.Columns["GiamToiDa"].Width = 120;
            _dgvVoucherDon.Columns["LuotDung"].Width = 120;
            _dgvVoucherDon.Columns["NgayBatDau"].Width = 105;
            _dgvVoucherDon.Columns["NgayKetThuc"].Width = 105;
            _dgvVoucherDon.Columns["TrangThai"].Width = 120;

            _dgvVoucherDon.CellFormatting += Dgv_CellFormatting;
            PageKit.FitColumns(_dgvVoucherDon);
            tab.Controls.Add(_dgvVoucherDon);
            _dgvVoucherDon.BringToFront();
        }

        // ================= TAB 3: VOUCHER THEO LOẠI MÓN (THEO_LOAI) =================
        private void InitTabVoucherLoai(TabPage tab)
        {
            tab.BackColor = Color.White;

            var btnAdd = CreateToolButton("➕ Thêm Voucher loại món", ButtonSkin.Kind.Primary);
            btnAdd.Click += (s, e) => OpenTaoVoucherDialog(scope: "THEO_LOAI");

            var btnEdit = CreateToolButton("✏️ Chỉnh sửa", ButtonSkin.Kind.Secondary);
            btnEdit.Click += (s, e) => EditSelectedVoucher(_dgvVoucherLoai);

            var btnDelete = CreateToolButton("🗑️ Xóa / Hủy mã", ButtonSkin.Kind.Danger);
            btnDelete.Click += async (s, e) => await DeleteSelectedVoucherAsync(_dgvVoucherLoai);

            _txtSearchLoai = new TextBox();
            _cboStatusLoai = new ComboBox();

            _txtSearchLoai.TextChanged += (s, e) => RenderVoucherLoaiGrid();
            _cboStatusLoai.SelectedIndexChanged += (s, e) => RenderVoucherLoaiGrid();

            var toolbar = CreateTabToolbar(btnAdd, btnEdit, btnDelete, _txtSearchLoai, _cboStatusLoai, "🔍 Tìm theo mã hoặc loại món...");
            tab.Controls.Add(toolbar);

            _dgvVoucherLoai = CreateStyledGrid();
            _dgvVoucherLoai.Columns.Add("MaVoucher", "Mã Voucher");
            _dgvVoucherLoai.Columns.Add("TenLoaiMon", "Nhóm loại món áp dụng");
            _dgvVoucherLoai.Columns.Add("TenChuongTrinh", "Chương trình áp dụng");
            _dgvVoucherLoai.Columns.Add("HinhThuc", "Mức giảm giá");
            _dgvVoucherLoai.Columns.Add("GiaTriDonToiThieu", "Giá tối thiểu được giảm");
            _dgvVoucherLoai.Columns.Add("GiamToiDa", "Giảm tối đa");
            _dgvVoucherLoai.Columns.Add("LuotDung", "Lượt áp dụng");
            _dgvVoucherLoai.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvVoucherLoai.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvVoucherLoai.Columns.Add("TrangThai", "Trạng thái");

            _dgvVoucherLoai.Columns["MaVoucher"].Width = 135;
            _dgvVoucherLoai.Columns["TenLoaiMon"].Width = 170;
            _dgvVoucherLoai.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvVoucherLoai.Columns["HinhThuc"].Width = 120;
            _dgvVoucherLoai.Columns["GiaTriDonToiThieu"].Width = 150;
            _dgvVoucherLoai.Columns["GiamToiDa"].Width = 110;
            _dgvVoucherLoai.Columns["LuotDung"].Width = 110;
            _dgvVoucherLoai.Columns["NgayBatDau"].Width = 100;
            _dgvVoucherLoai.Columns["NgayKetThuc"].Width = 100;
            _dgvVoucherLoai.Columns["TrangThai"].Width = 115;

            _dgvVoucherLoai.CellFormatting += Dgv_CellFormatting;
            PageKit.FitColumns(_dgvVoucherLoai);
            tab.Controls.Add(_dgvVoucherLoai);
            _dgvVoucherLoai.BringToFront();
        }

        // ================= TAB 4: VOUCHER THEO MÓN ĂN (THEO_MON) =================
        private void InitTabVoucherMon(TabPage tab)
        {
            tab.BackColor = Color.White;

            var btnAdd = CreateToolButton("➕ Thêm Voucher món", ButtonSkin.Kind.Primary);
            btnAdd.Click += (s, e) => OpenTaoVoucherDialog(scope: "THEO_MON");

            var btnEdit = CreateToolButton("✏️ Chỉnh sửa", ButtonSkin.Kind.Secondary);
            btnEdit.Click += (s, e) => EditSelectedVoucher(_dgvVoucherMon);

            var btnDelete = CreateToolButton("🗑️ Xóa / Hủy mã", ButtonSkin.Kind.Danger);
            btnDelete.Click += async (s, e) => await DeleteSelectedVoucherAsync(_dgvVoucherMon);

            _txtSearchMon = new TextBox();
            _cboStatusMon = new ComboBox();

            _txtSearchMon.TextChanged += (s, e) => RenderVoucherMonGrid();
            _cboStatusMon.SelectedIndexChanged += (s, e) => RenderVoucherMonGrid();

            var toolbar = CreateTabToolbar(btnAdd, btnEdit, btnDelete, _txtSearchMon, _cboStatusMon, "🔍 Tìm theo mã hoặc tên món ăn...");
            tab.Controls.Add(toolbar);

            _dgvVoucherMon = CreateStyledGrid();
            _dgvVoucherMon.Columns.Add("MaVoucher", "Mã Voucher");
            _dgvVoucherMon.Columns.Add("TenMon", "Tên món ăn áp dụng");
            _dgvVoucherMon.Columns.Add("TenChuongTrinh", "Chương trình áp dụng");
            _dgvVoucherMon.Columns.Add("HinhThuc", "Mức giảm ưu đãi");
            _dgvVoucherMon.Columns.Add("GiamToiDa", "Giảm tối đa");
            _dgvVoucherMon.Columns.Add("LuotDung", "Lượt áp dụng");
            _dgvVoucherMon.Columns.Add("NgayBatDau", "Bắt đầu");
            _dgvVoucherMon.Columns.Add("NgayKetThuc", "Kết thúc");
            _dgvVoucherMon.Columns.Add("TrangThai", "Trạng thái");

            _dgvVoucherMon.Columns["MaVoucher"].Width = 135;
            _dgvVoucherMon.Columns["TenMon"].Width = 180;
            _dgvVoucherMon.Columns["TenChuongTrinh"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvVoucherMon.Columns["HinhThuc"].Width = 140;
            _dgvVoucherMon.Columns["GiamToiDa"].Width = 120;
            _dgvVoucherMon.Columns["LuotDung"].Width = 120;
            _dgvVoucherMon.Columns["NgayBatDau"].Width = 100;
            _dgvVoucherMon.Columns["NgayKetThuc"].Width = 100;
            _dgvVoucherMon.Columns["TrangThai"].Width = 115;

            _dgvVoucherMon.CellFormatting += Dgv_CellFormatting;
            PageKit.FitColumns(_dgvVoucherMon);
            tab.Controls.Add(_dgvVoucherMon);
            _dgvVoucherMon.BringToFront();
        }

        private Button CreateToolButton(string text, ButtonSkin.Kind kind)
        {
            var btn = new Button { Text = text, Margin = new Padding(0, 0, 8, 0) };
            ButtonSkin.SetKind(btn, kind);
            PageKit.FitButton(btn);
            return btn;
        }

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
            if (grid == null || e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;

            var row = grid.Rows[e.RowIndex];
            string status = null;
            if (grid.Columns.Contains("TrangThai") && row.Cells["TrangThai"].Value != null)
            {
                status = row.Cells["TrangThai"].Value.ToString();
            }

            bool isEnded = status == "Đã kết thúc";

            // Nếu chương trình / voucher đã kết thúc: Chuyển xám và làm mờ toàn bộ dòng
            if (isEnded)
            {
                e.CellStyle.ForeColor = Color.FromArgb(148, 163, 184); // Màu xám mờ
                e.CellStyle.BackColor = Color.FromArgb(248, 250, 252); // Nền xám nhạt
                e.CellStyle.SelectionForeColor = Color.FromArgb(203, 213, 225);
            }

            string colName = grid.Columns[e.ColumnIndex].Name;
            if (colName == "TrangThai" && e.Value != null)
            {
                if (status == "Đang áp dụng")
                {
                    e.CellStyle.ForeColor = ThemeManager.Green;
                    e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
                else if (isEnded)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(100, 116, 139);
                    e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
                }
                else if (status == "Chưa diễn ra")
                {
                    e.CellStyle.ForeColor = ThemeManager.Amber;
                }
            }
            else if ((colName == "GiaTriDonToiThieu" || colName == "GiamToiDa") && e.Value != null)
            {
                if (double.TryParse(e.Value.ToString(), out var num))
                {
                    if (num > 0) e.Value = num.ToString("N0") + " đ";
                    else e.Value = "Không giới hạn";
                }
            }
        }

        // ================= DATA LOADING & RENDER TỪNG TAB =================
        private async void LoadAllDataAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _chuongTrinhList = await _service.GetChuongTrinhListAsync();
                _voucherList = await _service.GetVoucherListAsync();

                // Render 4 tab
                RenderChuongTrinhGrid();
                RenderVoucherDonGrid();
                RenderVoucherLoaiGrid();
                RenderVoucherMonGrid();

                // Cập nhật thẻ KPI
                _lblStatCT.Text = _chuongTrinhList.Count(x => x.TrangThai == "Đang áp dụng").ToString() + " / " + _chuongTrinhList.Count;
                _lblStatVoucherDon.Text = _voucherList.Count(x => x.PhamViApDung == "TOAN_DON" && x.TrangThai == "Đang áp dụng").ToString() + " / " + _voucherList.Count(x => x.PhamViApDung == "TOAN_DON");
                _lblStatVoucherLoai.Text = _voucherList.Count(x => x.PhamViApDung == "THEO_LOAI" && x.TrangThai == "Đang áp dụng").ToString() + " / " + _voucherList.Count(x => x.PhamViApDung == "THEO_LOAI");
                _lblStatVoucherMon.Text = _voucherList.Count(x => x.PhamViApDung == "THEO_MON" && x.TrangThai == "Đang áp dụng").ToString() + " / " + _voucherList.Count(x => x.PhamViApDung == "THEO_MON");
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

        private void RenderChuongTrinhGrid()
        {
            if (_dgvChuongTrinh == null) return;
            string keyword = _txtSearchCT?.Text?.Trim().ToLowerInvariant() ?? "";
            string statusFilter = _cboStatusCT?.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            var filtered = _chuongTrinhList.Where(ct => {
                if (!string.IsNullOrEmpty(keyword))
                {
                    bool matchMa = ct.MaChuongTrinh?.ToLowerInvariant().Contains(keyword) ?? false;
                    bool matchTen = ct.TenChuongTrinh?.ToLowerInvariant().Contains(keyword) ?? false;
                    if (!matchMa && !matchTen) return false;
                }
                if (statusFilter != "Tất cả trạng thái" && ct.TrangThai != statusFilter) return false;
                return true;
            }).ToList();

            _dgvChuongTrinh.Rows.Clear();
            foreach (var ct in filtered)
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
        }

        private void RenderVoucherDonGrid()
        {
            if (_dgvVoucherDon == null) return;
            string keyword = _txtSearchDon?.Text?.Trim().ToLowerInvariant() ?? "";
            string statusFilter = _cboStatusDon?.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            var filtered = _voucherList.Where(v => v.PhamViApDung == "TOAN_DON" && (
                (string.IsNullOrEmpty(keyword) || (v.MaVoucher?.ToLowerInvariant().Contains(keyword) ?? false) || (v.TenChuongTrinh?.ToLowerInvariant().Contains(keyword) ?? false))
                && (statusFilter == "Tất cả trạng thái" || v.TrangThai == statusFilter)
            )).ToList();

            _dgvVoucherDon.Rows.Clear();
            foreach (var v in filtered)
            {
                string luotDungText = (v.SoLuongToiDa.HasValue && v.SoLuongToiDa.Value > 0)
                    ? $"{v.SoLuongDaDung} / {v.SoLuongToiDa.Value} lượt"
                    : "Không giới hạn";

                int idx = _dgvVoucherDon.Rows.Add(
                    v.MaVoucher,
                    $"[{v.MaChuongTrinh}] {v.TenChuongTrinh}",
                    v.HinhThuc,
                    v.GiaTriDonToiThieu ?? 0,
                    v.GiamToiDa ?? 0,
                    luotDungText,
                    string.IsNullOrEmpty(v.NgayBatDau) ? "-" : v.NgayBatDau,
                    string.IsNullOrEmpty(v.NgayKetThuc) ? "Vô thời hạn" : v.NgayKetThuc,
                    v.TrangThai
                );
                _dgvVoucherDon.Rows[idx].Tag = v;
            }
        }

        private void RenderVoucherLoaiGrid()
        {
            if (_dgvVoucherLoai == null) return;
            string keyword = _txtSearchLoai?.Text?.Trim().ToLowerInvariant() ?? "";
            string statusFilter = _cboStatusLoai?.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            var filtered = _voucherList.Where(v => v.PhamViApDung == "THEO_LOAI" && (
                (string.IsNullOrEmpty(keyword)
                 || (v.MaVoucher?.ToLowerInvariant().Contains(keyword) ?? false)
                 || (v.TenLoaiMon?.ToLowerInvariant().Contains(keyword) ?? false)
                 || (v.TenChuongTrinh?.ToLowerInvariant().Contains(keyword) ?? false))
                && (statusFilter == "Tất cả trạng thái" || v.TrangThai == statusFilter)
            )).ToList();

            _dgvVoucherLoai.Rows.Clear();
            foreach (var v in filtered)
            {
                string luotDungText = (v.SoLuongToiDa.HasValue && v.SoLuongToiDa.Value > 0)
                    ? $"{v.SoLuongDaDung} / {v.SoLuongToiDa.Value} lượt"
                    : "Không giới hạn";

                int idx = _dgvVoucherLoai.Rows.Add(
                    v.MaVoucher,
                    v.TenLoaiMon ?? v.MaLoaiMon,
                    $"[{v.MaChuongTrinh}] {v.TenChuongTrinh}",
                    v.HinhThuc,
                    v.GiaTriDonToiThieu ?? 0,
                    v.GiamToiDa ?? 0,
                    luotDungText,
                    string.IsNullOrEmpty(v.NgayBatDau) ? "-" : v.NgayBatDau,
                    string.IsNullOrEmpty(v.NgayKetThuc) ? "Vô thời hạn" : v.NgayKetThuc,
                    v.TrangThai
                );
                _dgvVoucherLoai.Rows[idx].Tag = v;
            }
        }

        private void RenderVoucherMonGrid()
        {
            if (_dgvVoucherMon == null) return;
            string keyword = _txtSearchMon?.Text?.Trim().ToLowerInvariant() ?? "";
            string statusFilter = _cboStatusMon?.SelectedItem?.ToString() ?? "Tất cả trạng thái";

            var filtered = _voucherList.Where(v => v.PhamViApDung == "THEO_MON" && (
                (string.IsNullOrEmpty(keyword)
                 || (v.MaVoucher?.ToLowerInvariant().Contains(keyword) ?? false)
                 || (v.TenMon?.ToLowerInvariant().Contains(keyword) ?? false)
                 || (v.TenChuongTrinh?.ToLowerInvariant().Contains(keyword) ?? false))
                && (statusFilter == "Tất cả trạng thái" || v.TrangThai == statusFilter)
            )).ToList();

            _dgvVoucherMon.Rows.Clear();
            foreach (var v in filtered)
            {
                string luotDungText = (v.SoLuongToiDa.HasValue && v.SoLuongToiDa.Value > 0)
                    ? $"{v.SoLuongDaDung} / {v.SoLuongToiDa.Value} lượt"
                    : "Không giới hạn";

                int idx = _dgvVoucherMon.Rows.Add(
                    v.MaVoucher,
                    v.TenMon ?? v.MaMon,
                    $"[{v.MaChuongTrinh}] {v.TenChuongTrinh}",
                    v.HinhThuc,
                    v.GiamToiDa ?? 0,
                    luotDungText,
                    string.IsNullOrEmpty(v.NgayBatDau) ? "-" : v.NgayBatDau,
                    string.IsNullOrEmpty(v.NgayKetThuc) ? "Vô thời hạn" : v.NgayKetThuc,
                    v.TrangThai
                );
                _dgvVoucherMon.Rows[idx].Tag = v;
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

            var confirm = MessageBox.Show(this,
                $"Bạn có chắc chắn muốn xóa/dừng chương trình '{tenCT}' không?\n\n(Hệ thống sẽ bảo vệ dữ liệu kế toán: nếu đã có đơn hàng sử dụng, chương trình sẽ được đóng kết thúc hiệu lực an toàn)",
                "Xác nhận xóa / dừng chương trình", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteChuongTrinhAsync(maCT);
            if (ok)
            {
                MessageBox.Show(this, msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show(this, msg, "Không thể xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenTaoVoucherDialog(string scope = "TOAN_DON")
        {
            string preselectedCtId = null;
            if (_dgvChuongTrinh.SelectedRows.Count > 0)
            {
                string status = _dgvChuongTrinh.SelectedRows[0].Cells["TrangThai"].Value?.ToString();
                if (status != "Đã kết thúc")
                {
                    preselectedCtId = _dgvChuongTrinh.SelectedRows[0].Cells["MaChuongTrinh"].Value?.ToString();
                }
            }
            using (var dlg = new TaoVoucherDialog(_service, preselectedCtId, scope, null))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllDataAsync();
                }
            }
        }

        private void EditSelectedVoucher(DataGridView targetGrid)
        {
            if (targetGrid == null || targetGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn Voucher cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var item = targetGrid.SelectedRows[0].Tag as VoucherDto 
                       ?? _voucherList.FirstOrDefault(x => x.MaVoucher == targetGrid.SelectedRows[0].Cells["MaVoucher"].Value?.ToString());
            if (item == null) return;

            using (var dlg = new TaoVoucherDialog(_service, item.MaChuongTrinh, item.PhamViApDung, item))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadAllDataAsync();
                }
            }
        }

        private async Task DeleteSelectedVoucherAsync(DataGridView targetGrid)
        {
            if (targetGrid == null || targetGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn Voucher cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var item = targetGrid.SelectedRows[0].Tag as VoucherDto 
                       ?? _voucherList.FirstOrDefault(x => x.MaVoucher == targetGrid.SelectedRows[0].Cells["MaVoucher"].Value?.ToString());
            if (item == null) return;

            var confirm = MessageBox.Show(this,
                $"Bạn có chắc chắn muốn xóa / hủy Voucher '{item.MaVoucher}' không?\n\n(Lưu ý: Nếu voucher đã từng được áp dụng trong đơn hàng, hệ thống sẽ ẨN voucher và kết thúc hiệu lực để bảo toàn dữ liệu hóa đơn)",
                "Xác nhận xóa / ẩn Voucher", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var (ok, msg) = await _service.DeleteVoucherAsync(item.MaKmvoucher);
            if (ok)
            {
                MessageBox.Show(this, msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show(this, msg, "Không thể xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}