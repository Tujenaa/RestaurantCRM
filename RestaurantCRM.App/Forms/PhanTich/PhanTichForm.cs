using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.PhanTich
{
    /// <summary>
    /// Màn hình phân tích khách hàng đa tab:
    /// - Tab 1: Cơ cấu độ tuổi & Giới tính (Biểu đồ phân bố GDI+ & Bảng chi tiết từng nhóm tuổi).
    /// - Tab 2: Món bán chạy (theo đơn hoàn tất): Top món & bảng lượt gọi, doanh thu.
    /// - Tab 3: Sở thích (khách tự khai): % khách chọn từng đáp án của hồ sơ sở thích, Top 10 món được ♥.
    /// - Tab 4: Xếp hạng & Chi tiêu (Top chi tiêu & Bảng danh sách khách hàng tiêu biểu).
    /// </summary>
    public class PhanTichForm : Form
    {
        private TabControl _tabMain;

        // Tab 1: Độ tuổi & Giới tính
        private UcCustomerDemographics _demographics;
        private UcDataGridView _dgvDoTuoi;
        private Label _lblDoTuoiSummary;

        // Tab 2: Sở thích gọi món
        private UcTopList _topFood;
        private UcDataGridView _dgvFood;
        private TextBox _txtSearchFood;
        private ComboBox _cboLoaiFood;
        private List<MonUaThichItemDto> _allFoodItems = new List<MonUaThichItemDto>();
        private bool _updatingFoodFilter;

        // Tab 2: Khách tự khai (hồ sơ sở thích)
        private UcTopList _topTim;
        private UcDataGridView _dgvTuKhai;
        private Label _lblTuKhai;

        // Tab 3: Chi tiêu khách hàng
        private UcTopList _topSpending;
        private UcDataGridView _dgvSpending;
        private TextBox _txtSearchSpend;
        private ComboBox _cboSegmentSpend;
        private List<RestaurantCRM.AdminApp.DTOs.KhachHang.KhachHangDto> _allTopCustomers = new List<RestaurantCRM.AdminApp.DTOs.KhachHang.KhachHangDto>();

        public PhanTichForm()
        {
            Text = "Phân tích khách hàng | Lẩu Phố";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1220, 800);
            MinimumSize = new Size(1000, 680);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);
            Padding = new Padding(14, 6, 14, 10);

            // 1. Header
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblTitle = new Label
            {
                Text = "Phân tích khách hàng",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            var lblSub = new Label
            {
                Text = "Báo cáo phân tích chuyên sâu về cơ cấu độ tuổi, giới tính, sở thích ẩm thực và chi tiêu của khách hàng.",
                Location = new Point(12, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted
            };

            var btnReload = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(104, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Paper,
                ForeColor = ThemeManager.Ink2,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnReload.FlatAppearance.BorderColor = ThemeManager.Line;
            btnReload.Location = new Point(header.Width - 114, 20);
            header.Resize += (s, e) => { btnReload.Location = new Point(header.Width - 114, 20); };
            btnReload.Click += async (s, e) => await LoadDataAsync();

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            header.Controls.Add(btnReload);

            Action place = () =>
            {
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 12 : 44);
                header.Height = embedded ? 44 : 76;
                btnReload.Location = new Point(header.Width - 114, embedded ? 6 : 20);
            };
            ParentChanged += (s, e) => place();

            // 2. Khung TabControl dạng Pill hiện đại
            var tabHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };

            // Dựng 3 Tab phân tích
            BuildTabDoTuoi();
            BuildTabSoThich();
            BuildTabTuKhai();
            BuildTabChiTieu();

            tabHost.Controls.Add(_tabMain);
            TabSkin.Attach(_tabMain);

            Controls.Add(tabHost);
            Controls.Add(header);
            tabHost.BringToFront();
            place();

            Load += async (s, e) => await LoadDataAsync();
        }

        // ================= TAB 1: CƠ CẤU ĐỘ TUỔI & GIỚI TÍNH =================
        private void BuildTabDoTuoi()
        {
            var tab = new TabPage("Cơ cấu độ tuổi & Giới tính") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F)); // Biểu đồ bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // Bảng chi tiết bên phải

            _demographics = new UcCustomerDemographics
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "BẢNG CHI TIẾT CƠ CẤU NHÓM TUỔI",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            _lblDoTuoiSummary = new Label
            {
                Text = "Đang tải dữ liệu...",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = ThemeManager.Muted,
                Dock = DockStyle.Bottom,
                Height = 24
            };

            _dgvDoTuoi = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvDoTuoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhóm tuổi", Width = 95 });
            _dgvDoTuoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phân khúc đối tượng", FillWeight = 160 });
            var cCount = new DataGridViewTextBoxColumn { HeaderText = "Số lượng khách", Width = 120 };
            cCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cCount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvDoTuoi.Columns.Add(cCount);

            var cPct = new DataGridViewTextBoxColumn { HeaderText = "Tỷ lệ (%)", Width = 110 };
            cPct.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cPct.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cPct.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvDoTuoi.Columns.Add(cPct);

            rightCard.Controls.Add(_dgvDoTuoi);
            rightCard.Controls.Add(_lblDoTuoiSummary);
            rightCard.Controls.Add(lblGridTitle);

            split.Controls.Add(_demographics, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        // ================= TAB 2: SỞ THÍCH ẨM THỰC (MÓN ƯA CHUỘNG) =================
        private void BuildTabSoThich()
        {
            var tab = new TabPage("Món bán chạy") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F)); // Top list bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // DataGridView bên phải

            _topFood = new UcTopList
            {
                Dock = DockStyle.Fill,
                Title = "Món bán chạy (theo đơn hoàn tất)",
                Subtitle = "XẾP HẠNG THEO LƯỢT ĐẶT MÓN",
                Unit = "phần",
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "MÓN BÁN CHẠY (THEO ĐƠN HOÀN TẤT)",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            var filterFoodBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White, Padding = new Padding(0, 2, 0, 4) };
            _txtSearchFood = new TextBox();
            _txtSearchFood.TextChanged += (s, e) => RenderFoodGrid();
            var pillFood = new PillSearch(_txtSearchFood, "Tìm mã, tên món ăn, loại món...") { Location = new Point(0, 2), Width = 250 };
            _cboLoaiFood = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            _cboLoaiFood.Items.Add("Tất cả loại món");
            _cboLoaiFood.SelectedIndex = 0;
            _cboLoaiFood.SelectedIndexChanged += (s, e) => { if (!_updatingFoodFilter) RenderFoodGrid(); };
            var chipFood = new ComboChip(_cboLoaiFood, "Loại món") { Location = new Point(260, 2) };
            filterFoodBar.Controls.Add(_cboLoaiFood);
            filterFoodBar.Controls.Add(pillFood);
            filterFoodBar.Controls.Add(chipFood);

            _dgvFood = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã món", Width = 80 });
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên món ăn", FillWeight = 160 });
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Loại món", Width = 110 });

            var cGoi = new DataGridViewTextBoxColumn { HeaderText = "Lượt gọi", Width = 100 };
            cGoi.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cGoi.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvFood.Columns.Add(cGoi);

            var cDoanhThu = new DataGridViewTextBoxColumn { HeaderText = "Doanh thu (đ)", Width = 130 };
            cDoanhThu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDoanhThu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDoanhThu.DefaultCellStyle.Format = "N0";
            _dgvFood.Columns.Add(cDoanhThu);

            var cTyLe = new DataGridViewTextBoxColumn { HeaderText = "Tỷ lệ ưa thích", Width = 110 };
            cTyLe.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cTyLe.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cTyLe.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvFood.Columns.Add(cTyLe);

            rightCard.Controls.Add(_dgvFood);
            rightCard.Controls.Add(filterFoodBar);
            rightCard.Controls.Add(lblGridTitle);
            _dgvFood.BringToFront();

            split.Controls.Add(_topFood, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        // ================= TAB 3: SỞ THÍCH (KHÁCH TỰ KHAI) =================
        private void BuildTabTuKhai()
        {
            var tab = new TabPage("Sở thích") { BackColor = ThemeManager.Bg };

            // ── Khách tự khai (hồ sơ sở thích) — tách riêng, không trộn với dữ liệu đơn hàng ──
            var splitTuKhai = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            splitTuKhai.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            splitTuKhai.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
            splitTuKhai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _topTim = new UcTopList
            {
                Dock = DockStyle.Fill,
                Title = "Top 10 món được ♥ nhiều nhất",
                Subtitle = "SỐ KHÁCH ĐÃ ♥",
                Unit = "khách",
                Margin = new Padding(0, 0, 8, 0)
            };

            var cardTuKhai = PageKit.NewCard();
            cardTuKhai.Dock = DockStyle.Fill;
            cardTuKhai.Margin = new Padding(4, 0, 0, 0);
            cardTuKhai.Padding = new Padding(16, 14, 16, 14);
            var lblTuKhaiTitle = new Label
            {
                Text = "SỞ THÍCH",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };
            _lblTuKhai = new Label { Dock = DockStyle.Top, AutoSize = false, Height = 44, ForeColor = ThemeManager.Muted, Font = new Font("Segoe UI", 9F), Text = "Đang tải...", UseMnemonic = false };
            _dgvTuKhai = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvTuKhai.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvTuKhai.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Câu hỏi", FillWeight = 170 });
            _dgvTuKhai.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đáp án", FillWeight = 130 });
            var cSoKhach = new DataGridViewTextBoxColumn { HeaderText = "Số khách chọn", FillWeight = 70 };
            cSoKhach.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvTuKhai.Columns.Add(cSoKhach);
            var cTyLeTk = new DataGridViewTextBoxColumn { HeaderText = "Tỷ lệ", FillWeight = 55 };
            cTyLeTk.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cTyLeTk.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvTuKhai.Columns.Add(cTyLeTk);
            var cTren = new DataGridViewTextBoxColumn { HeaderText = "Trên số khách", FillWeight = 70 };
            cTren.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvTuKhai.Columns.Add(cTren);
            cardTuKhai.Controls.Add(_dgvTuKhai);
            cardTuKhai.Controls.Add(_lblTuKhai);
            cardTuKhai.Controls.Add(lblTuKhaiTitle);
            _dgvTuKhai.BringToFront();

            splitTuKhai.Controls.Add(_topTim, 0, 0);
            splitTuKhai.Controls.Add(cardTuKhai, 1, 0);

            tab.Controls.Add(splitTuKhai);
            _tabMain.TabPages.Add(tab);
        }

        // ================= TAB 3: XẾP HẠNG & CHI TIÊU KHÁCH HÀNG =================
        private void BuildTabChiTieu()
        {
            var tab = new TabPage("Xếp hạng & Chi tiêu") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F)); // Top list bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // DataGridView bên phải

            _topSpending = new UcTopList
            {
                Dock = DockStyle.Fill,
                Title = "Khách hàng tiêu biểu",
                Subtitle = "TỔNG TIỀN CÁC ĐƠN ĐÃ HOÀN THÀNH",
                Unit = "đ",
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "BẢNG XẾP HẠNG KHÁCH HÀNG THEO MỨC ĐỘ CHI TIÊU",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            var filterSpendBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White, Padding = new Padding(0, 2, 0, 4) };
            _txtSearchSpend = new TextBox();
            _txtSearchSpend.TextChanged += (s, e) => RenderSpendingGrid();
            var pillSpend = new PillSearch(_txtSearchSpend, "Tìm mã KH, họ tên, số điện thoại...") { Location = new Point(0, 2), Width = 250 };
            _cboSegmentSpend = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            _cboSegmentSpend.Items.AddRange(new object[]
            {
                "Tất cả phân khúc",
                "Khách VIP (≥ 2 triệu)",
                "Thân thiết (≥ 3 đơn)",
                "Đang hoạt động",
                "Tài khoản đã khóa"
            });
            _cboSegmentSpend.SelectedIndex = 0;
            _cboSegmentSpend.SelectedIndexChanged += (s, e) => RenderSpendingGrid();
            var chipSpend = new ComboChip(_cboSegmentSpend, "Phân khúc") { Location = new Point(260, 2) };
            filterSpendBar.Controls.Add(_cboSegmentSpend);
            filterSpendBar.Controls.Add(pillSpend);
            filterSpendBar.Controls.Add(chipSpend);

            _dgvSpending = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã KH", Width = 80 });
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", FillWeight = 160 });
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số điện thoại", Width = 115 });

            var cDon = new DataGridViewTextBoxColumn { HeaderText = "Số đơn", Width = 80 };
            cDon.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDon.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvSpending.Columns.Add(cDon);

            var cChiTieu = new DataGridViewTextBoxColumn { HeaderText = "Tổng chi tiêu (đ)", Width = 140 };
            cChiTieu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cChiTieu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cChiTieu.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cChiTieu.DefaultCellStyle.Format = "N0";
            _dgvSpending.Columns.Add(cChiTieu);

            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Width = 95 });

            rightCard.Controls.Add(_dgvSpending);
            rightCard.Controls.Add(filterSpendBar);
            rightCard.Controls.Add(lblGridTitle);
            _dgvSpending.BringToFront();

            split.Controls.Add(_topSpending, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        private async Task LoadDataAsync()
        {
            var demographicTask = LoadDemographicsAndPreferencesAsync();
            var customerSpendTask = LoadTopCustomersAsync();
            var tuKhaiTask = LoadSoThichTuKhaiAsync();
            await Task.WhenAll(demographicTask, customerSpendTask, tuKhaiTask);
        }

        /// <summary>
        /// "Khách tự khai": với mỗi câu trắc nghiệm của hồ sơ sở thích, % khách chọn từng đáp án.
        /// Mẫu số của mỗi câu = số khách đã trả lời câu đó (khách chỉ bấm ♥ không bị tính là "không chọn" ở câu khẩu vị).
        /// </summary>
        private async Task LoadSoThichTuKhaiAsync()
        {
            try
            {
                var ksService = new KhaoSatService();
                var ma = await ksService.GetMaBoSoThichAsync();
                if (IsDisposed) return;
                _dgvTuKhai.Rows.Clear();
                if (ma == null)
                {
                    _lblTuKhai.Text = "Chưa có hồ sơ sở thích (tạo ở màn hình Khảo sát → \"Hồ sơ sở thích\").";
                    _topTim.Subtitle = "CHƯA CÓ HỒ SƠ SỞ THÍCH";
                    _topTim.SetItems(new List<KeyValuePair<string, double>>());
                    return;
                }

                var tBo = ksService.GetByIdAsync(ma);
                var tKhai = ksService.GetSoThichKhachHangAsync();
                var tKhach = new KhachHangService().GetAllAsync();
                await Task.WhenAll(tBo, tKhai, tKhach);
                if (IsDisposed) return;

                var bo = tBo.Result;
                var khai = tKhai.Result;
                int tongKhach = tKhach.Result?.Count ?? 0;
                _lblTuKhai.Text = "Trên " + khai.Count.ToString("N0") + "/" + tongKhach.ToString("N0") + " khách đã khai báo sở thích." + Environment.NewLine
                                  + "% mỗi câu tính trên số khách đã trả lời câu đó; câu chọn nhiều có tổng có thể vượt 100%.";

                // Đếm số khách chọn từng đáp án và số khách trả lời từng câu
                var demDapAn = new Dictionary<string, int>();
                var demCau = new Dictionary<string, int>();
                foreach (var k in khai)
                {
                    foreach (var t in (k.MaTuyChons ?? new List<string>()).Distinct())
                        demDapAn[t] = (demDapAn.TryGetValue(t, out var n) ? n : 0) + 1;
                    foreach (var c in (k.MaCauHois ?? new List<string>()).Distinct())
                        demCau[c] = (demCau.TryGetValue(c, out var n) ? n : 0) + 1;
                }

                var tim = new List<KeyValuePair<string, double>>();
                foreach (var c in bo?.CauHoi ?? new List<RestaurantCRM.AdminApp.DTOs.KhaoSat.CauHoiDto>())
                {
                    if (!c.LaTracNghiem) continue;
                    int mau = demCau.TryGetValue(c.MaCauHoi, out var m) ? m : 0;
                    if (c.LaCauMonYeuThich)
                    {
                        foreach (var t in c.TuyChon)
                        {
                            int so = demDapAn.TryGetValue(t.MaTuyChon, out var x) ? x : 0;
                            if (so > 0) tim.Add(new KeyValuePair<string, double>((t.TenMon ?? t.NoiDungTuyChon) + (t.NgungBan ? " (ngừng bán)" : ""), so));
                        }
                        continue;
                    }
                    bool dau = true;
                    foreach (var t in c.TuyChon)
                    {
                        int so = demDapAn.TryGetValue(t.MaTuyChon, out var x) ? x : 0;
                        double tyLe = mau > 0 ? so * 100.0 / mau : 0;
                        // Ghi câu hỏi trên mọi dòng để không mất ngữ cảnh khi cuộn; dòng đầu mỗi câu in đậm
                        int r = _dgvTuKhai.Rows.Add(c.NoiDungCauHoi + (c.ChonNhieu ? " (chọn nhiều)" : ""),
                            t.NoiDungTuyChon, so.ToString("N0"), tyLe.ToString("0.0") + "%", mau.ToString("N0") + " khách");
                        if (dau) _dgvTuKhai.Rows[r].Cells[0].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        else _dgvTuKhai.Rows[r].Cells[0].Style.ForeColor = ThemeManager.Muted;
                        dau = false;
                    }
                }

                tim = tim.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(10).ToList();
                _topTim.Subtitle = tim.Count == 0 ? "CHƯA CÓ MÓN NÀO ĐƯỢC ♥" : "SỐ KHÁCH ĐÃ ♥";
                _topTim.SetItems(tim);
            }
            catch (ApiException ex)
            {
                if (IsDisposed) return;
                _lblTuKhai.Text = "Không tải được hồ sơ sở thích: " + ex.Message;
                _topTim.Subtitle = "KHÔNG TẢI ĐƯỢC DỮ LIỆU API";
                _topTim.SetItems(new List<KeyValuePair<string, double>>());
            }
        }

        private async Task LoadDemographicsAndPreferencesAsync()
        {
            try
            {
                using (var http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(15) })
                {
                    var data = await http.GetFromJsonAsync<PhanTichKhachHangDto>("api/ThongKe/PhanTichKhachHang");
                    if (data == null || IsDisposed) return;

                    // 1. Tab Độ tuổi: Cập nhật thẻ biểu đồ & DataGridView
                    _demographics.SetData(data);

                    _dgvDoTuoi.Rows.Clear();
                    if (data.DoTuoi != null)
                    {
                        foreach (var it in data.DoTuoi)
                        {
                            _dgvDoTuoi.Rows.Add(it.TenNhom, it.MoTa, it.SoLuong.ToString("N0"), $"{it.TyLe:0.0}%");
                        }
                    }

                    _lblDoTuoiSummary.Text = $"Độ tuổi trung bình: {data.DoTuoiTrungBinh:0.0} tuổi  ·  Đã cập nhật ngày sinh: {data.KhachHangCoNgaySinh:N0} / {data.TongKhachHang:N0} khách ({((double)data.KhachHangCoNgaySinh / Math.Max(1, data.TongKhachHang) * 100):0.0}%)";

                    // 2. Tab Sở thích gọi món: Cập nhật Top list & DataGridView
                    var foodItems = new List<KeyValuePair<string, double>>();
                    _allFoodItems = data.TopMonUaThich ?? new List<MonUaThichItemDto>();

                    if (_cboLoaiFood != null)
                    {
                        _updatingFoodFilter = true;
                        try
                        {
                            string prev = _cboLoaiFood.SelectedItem as string;
                            var cats = _allFoodItems
                                .Select(x => x.TenLoaiMon)
                                .Where(c => !string.IsNullOrWhiteSpace(c))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(c => c)
                                .ToList();
                            _cboLoaiFood.Items.Clear();
                            _cboLoaiFood.Items.Add("Tất cả loại món");
                            foreach (var c in cats) _cboLoaiFood.Items.Add(c);
                            int idx = !string.IsNullOrEmpty(prev) ? _cboLoaiFood.Items.IndexOf(prev) : 0;
                            _cboLoaiFood.SelectedIndex = idx >= 0 ? idx : 0;
                        }
                        finally { _updatingFoodFilter = false; }
                    }

                    foreach (var m in _allFoodItems)
                    {
                        string label = $"{m.TenMon}  ·  {m.TyLeUaThich:0.0}%";
                        foodItems.Add(new KeyValuePair<string, double>(label, m.SoLuongGoi));
                    }

                    RenderFoodGrid();

                    if (foodItems.Count == 0)
                        _topFood.Subtitle = "CHƯA CÓ DỮ LIỆU ĐẶT MÓN HOÀN THÀNH";
                    else
                        _topFood.Subtitle = "XẾP HẠNG THEO SỐ LƯỢT ĐẶT MÓN";

                    _topFood.SetItems(foodItems);
                }
            }
            catch (Exception)
            {
                if (IsDisposed) return;
                _demographics.SetData(null);
                _dgvDoTuoi.Rows.Clear();
                _lblDoTuoiSummary.Text = "Không tải được dữ liệu độ tuổi từ API";
                _topFood.Subtitle = "KHÔNG TẢI ĐƯỢC DỮ LIỆU API";
                _topFood.SetItems(new List<KeyValuePair<string, double>>());
                _dgvFood.Rows.Clear();
            }
        }

        private async Task LoadTopCustomersAsync()
        {
            try
            {
                var ds = await new KhachHangService().GetAllAsync();
                if (IsDisposed) return;
                var items = new List<KeyValuePair<string, double>>();
                _dgvSpending.Rows.Clear();

                if (ds != null)
                {
                    var topList = new List<RestaurantCRM.AdminApp.DTOs.KhachHang.KhachHangDto>();
                    foreach (var x in ds) if (x.TongChiTieu > 0) topList.Add(x);
                    topList.Sort((a, b) => b.TongChiTieu.CompareTo(a.TongChiTieu));
                    _allTopCustomers = topList;

                    // Top 8 vào thẻ tóm tắt
                    int take = Math.Min(8, topList.Count);
                    for (int i = 0; i < take; i++)
                    {
                        var k = topList[i];
                        items.Add(new KeyValuePair<string, double>(k.HoTen + "  ·  " + k.SoDonHang + " đơn", k.TongChiTieu));
                    }

                    RenderSpendingGrid();
                }

                if (items.Count == 0) _topSpending.Subtitle = "CHƯA CÓ ĐƠN HÀNG HOÀN THÀNH NÀO";
                else _topSpending.Subtitle = "TỔNG TIỀN CÁC ĐƠN ĐÃ HOÀN THÀNH";

                _topSpending.SetItems(items);
            }
            catch (ApiException)
            {
                if (IsDisposed) return;
                _topSpending.Subtitle = "KHÔNG TẢI ĐƯỢC DỮ LIỆU — KIỂM TRA API";
                _topSpending.SetItems(new List<KeyValuePair<string, double>>());
                _dgvSpending.Rows.Clear();
            }
        }

        private void RenderFoodGrid()
        {
            if (_dgvFood == null) return;
            string q = (_txtSearchFood?.Text ?? "").Trim();
            string cat = (_cboLoaiFood != null && _cboLoaiFood.SelectedIndex > 0) ? (_cboLoaiFood.SelectedItem as string) : null;

            _dgvFood.Rows.Clear();
            foreach (var m in _allFoodItems)
            {
                if (!string.IsNullOrEmpty(cat) && !string.Equals(m.TenLoaiMon, cat, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!UiStyler.MatchesSearch(q, m.MaMon, m.TenMon, m.TenLoaiMon))
                    continue;
                _dgvFood.Rows.Add(m.MaMon, m.TenMon, m.TenLoaiMon, m.SoLuongGoi.ToString("N0"), m.DoanhThu, $"{m.TyLeUaThich:0.0}%");
            }
        }

        private void RenderSpendingGrid()
        {
            if (_dgvSpending == null) return;
            string q = (_txtSearchSpend?.Text ?? "").Trim();
            int segIdx = _cboSegmentSpend?.SelectedIndex ?? 0;

            _dgvSpending.Rows.Clear();
            foreach (var k in _allTopCustomers)
            {
                if (segIdx == 1 && k.TongChiTieu < 2000000) continue;
                if (segIdx == 2 && k.SoDonHang < 3) continue;
                if (segIdx == 3 && k.BiKhoa) continue;
                if (segIdx == 4 && !k.BiKhoa) continue;
                if (!UiStyler.MatchesSearch(q, k.MaKhachHang, k.HoTen, k.SoDienThoai, k.Email))
                    continue;
                _dgvSpending.Rows.Add(k.MaKhachHang, k.HoTen, k.SoDienThoai, k.SoDonHang.ToString("N0"), k.TongChiTieu, k.TrangThaiHienThi);
            }
        }
    }
}