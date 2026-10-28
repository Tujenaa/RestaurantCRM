using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Controls.BaoCao;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.BaoCao
{
    /// <summary>
    /// Giao diện Thống kê Báo cáo Doanh thu Chuyên sâu (Theo Quý, Nhân viên, Sản phẩm)
    /// Đáp ứng trọn vẹn yêu cầu đồ án quản trị nhà hàng với 3 Tab ma trận & xuất Excel.
    /// </summary>
    public class BaoCaoBanHangForm : Form
    {
        private readonly BaoCaoService _service;
        private readonly ToolTip _toolTip;

        // Bộ lọc trên cùng
        private ComboBox _cboNam;
        private Button _btnXem;
        private Button _btnRefresh;
        private Label _lblStatus;

        // 4 Thẻ KPI Tổng quan
        private BaoCaoKpiTile _cardTongThu;
        private BaoCaoKpiTile _cardTongChi;
        private BaoCaoKpiTile _cardLoiNhuan;
        private BaoCaoKpiTile _cardTop;

        // Thanh Tab hiện đại
        private Panel _pnlTabBar;
        private Button _btnTabTaiChinh;
        private Button _btnTabNhanVien;
        private Button _btnTabMonAn;
        private int _currentTabIndex = 0; // 0: Tài chính, 1: Nhân viên, 2: Món ăn

        // Container các Tab
        private Panel _pnlTabContainer;
        private Panel _pnlTabTaiChinh;
        private Panel _pnlTabNhanVien;
        private Panel _pnlTabMonAn;

        // Tab 1: Tài chính Quý
        private Panel _pnlGridCard;
        private UcDataGridView _dgvTaiChinhQuy;
        private Panel _pnlChartCard;
        private Panel _pnlChartArea;
        private QuarterComparisonChartRenderer _chartRenderer;

        // Tab 2: Nhân viên
        private UcDataGridView _dgvNhanVien;

        // Tab 3: Món ăn
        private TextBox _txtSearchMon;
        private RadioButton _radMonTheoQuy;
        private RadioButton _radMonTheoNam;
        private UcDataGridView _dgvMonAn;

        // Dữ liệu báo cáo hiện tại
        private BaoCaoDoanhThuTheoQuyDto _currentData;
        private bool _isDataLoaded = false;

        public BaoCaoBanHangForm()
        {
            _service = new BaoCaoService();

            _toolTip = new ToolTip
            {
                InitialDelay = 100,
                AutoPopDelay = 6000,
                ReshowDelay = 100,
                IsBalloon = false
            };

            Text = "Thống kê Doanh thu & Báo cáo Tài chính | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1220, 860);
            MinimumSize = new Size(1000, 700);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            BuildUI();

            Resize += (s, e) =>
            {
                if (_pnlTabContainer != null && ClientSize.Height > 700)
                {
                    int newHeight = Math.Max(540, ClientSize.Height - _pnlTabContainer.Top - 36);
                    _pnlTabContainer.Height = newHeight;
                }
            };

            // Khởi tạo tải năm hiện tại
            Shown += async (s, e) =>
            {
                InitYears();
                await LoadDataAsync();
            };
        }

        private void InitYears()
        {
            int currentYear = DateTime.Now.Year;
            _cboNam.Items.Clear();
            _cboNam.Items.Add(currentYear.ToString());
            _cboNam.Items.Add((currentYear - 1).ToString());
            _cboNam.Items.Add((currentYear - 2).ToString());
            _cboNam.SelectedIndex = 0;
        }

        private void BuildUI()
        {
            // Panel cuộn chính
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(24, 16, 24, 24)
            };
            Controls.Add(scrollPanel);

            int y = 14;
            int fullWidth = 1140;

            // 1. THANH BỘ LỌC NĂM VÀ NÚT CHỨC NĂNG
            var pnlFilter = new Panel
            {
                Location = new Point(24, y),
                Size = new Size(fullWidth, 54),
                BackColor = ThemeManager.Paper,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlFilter.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlFilter.Width - 1, pnlFilter.Height - 1);
            };

            var lblNam = new Label
            {
                Text = "📅 Năm tài chính:",
                Location = new Point(16, 17),
                AutoSize = true,
                ForeColor = ThemeManager.Ink,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            pnlFilter.Controls.Add(lblNam);

            _cboNam = new ComboBox
            {
                Location = new Point(140, 13),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Tag = UiStyler.NoStyleTag
            };
            _cboNam.SelectedIndexChanged += async (s, e) =>
            {
                if (_isDataLoaded) await LoadDataAsync();
            };
            pnlFilter.Controls.Add(_cboNam);

            _btnXem = new Button
            {
                Text = "⚡ Xem báo cáo",
                Location = new Point(275, 11),
                Size = new Size(130, 32),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnXem.FlatAppearance.BorderSize = 0;
            _btnXem.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(_btnXem);

            _btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Location = new Point(415, 11),
                Size = new Size(105, 32),
                BackColor = ThemeManager.Surface2,
                ForeColor = ThemeManager.Ink2,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderColor = ThemeManager.Line;
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(_btnRefresh);

            _lblStatus = new Label
            {
                Text = "",
                Location = new Point(530, 18),
                AutoSize = true,
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnlFilter.Controls.Add(_lblStatus);

            scrollPanel.Controls.Add(pnlFilter);
            y += 66;

            // 2. HÀNG 4 THẺ KPI TỔNG QUAN NĂM
            var pnlCardsTable = new TableLayoutPanel
            {
                Location = new Point(24, y),
                Size = new Size(fullWidth, 118),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Margin = new Padding(0)
            };
            pnlCardsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 4; i++)
                pnlCardsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            _cardTongThu = new BaoCaoKpiTile("TỔNG DOANH THU", "0 đ", "Đang tải...", "💰", ThemeManager.Primary);
            _cardTongChi = new BaoCaoKpiTile("TỔNG CHI PHÍ", "0 đ", "Chi nhập kho", "📦", ThemeManager.Amber);
            _cardLoiNhuan = new BaoCaoKpiTile("LỢI NHUẬN RÒNG", "0 đ", "Biên lãi 0%", "📈", ThemeManager.Green);
            _cardTop = new BaoCaoKpiTile("TOP NỔI BẬT", "Chưa có", "Doanh thu cao nhất", "👑", ThemeManager.Violet);
            _cardTop.Margin = new Padding(0);

            pnlCardsTable.Controls.Add(_cardTongThu, 0, 0);
            pnlCardsTable.Controls.Add(_cardTongChi, 1, 0);
            pnlCardsTable.Controls.Add(_cardLoiNhuan, 2, 0);
            pnlCardsTable.Controls.Add(_cardTop, 3, 0);

            scrollPanel.Controls.Add(pnlCardsTable);
            y += 132;

            // 3. THANH TAB BAR CHỌN BÁO CÁO (SEGMENTED TABS)
            _pnlTabBar = new Panel
            {
                Location = new Point(24, y),
                Size = new Size(fullWidth, 46),
                BackColor = ThemeManager.Paper,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _pnlTabBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlTabBar.Width - 1, _pnlTabBar.Height - 1);
            };

            _btnTabTaiChinh = CreateTabButton("📊  1. Báo cáo Tài chính theo Quý", 0, 10);
            _btnTabNhanVien = CreateTabButton("👤  2. Doanh thu theo Nhân viên", 1, 260);
            _btnTabMonAn = CreateTabButton("🍲  3. Doanh thu theo Món ăn", 2, 510);

            _pnlTabBar.Controls.Add(_btnTabTaiChinh);
            _pnlTabBar.Controls.Add(_btnTabNhanVien);
            _pnlTabBar.Controls.Add(_btnTabMonAn);

            scrollPanel.Controls.Add(_pnlTabBar);
            y += 56;

            // 4. KHỐI CONTAINER CHỨA NỘI DUNG 3 TAB
            _pnlTabContainer = new Panel
            {
                Location = new Point(24, y),
                Size = new Size(fullWidth, 540),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            scrollPanel.Controls.Add(_pnlTabContainer);

            // Xây dựng 3 nội dung Tab
            BuildTabTaiChinh();
            BuildTabNhanVien();
            BuildTabMonAn();

            SwitchTab(0);
        }

        private Button CreateTabButton(string text, int index, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 6),
                Size = new Size(240, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => SwitchTab(index);
            return btn;
        }

        private void SwitchTab(int index)
        {
            _currentTabIndex = index;

            // Cập nhật giao diện nút Tab
            UpdateButtonTabState(_btnTabTaiChinh, index == 0);
            UpdateButtonTabState(_btnTabNhanVien, index == 1);
            UpdateButtonTabState(_btnTabMonAn, index == 2);

            // Hiện/Ẩn các Panel tương ứng
            _pnlTabTaiChinh.Visible = (index == 0);
            _pnlTabNhanVien.Visible = (index == 1);
            _pnlTabMonAn.Visible = (index == 2);

            if (index == 0 && _pnlChartCard != null && _pnlTabContainer != null)
            {
                int req = _pnlChartCard.Bottom + 20;
                if (_pnlTabContainer.Height < req) _pnlTabContainer.Height = req;
            }
        }

        private void UpdateButtonTabState(Button btn, bool active)
        {
            if (active)
            {
                btn.BackColor = ThemeManager.PrimarySoft;
                btn.ForeColor = ThemeManager.Primary;
            }
            else
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = ThemeManager.Ink2;
            }
        }

        // ================= TAB 1: TÀI CHÍNH THEO QUÝ =================
        private void BuildTabTaiChinh()
        {
            _pnlTabTaiChinh = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Bảng Ma trận Quý (Ở trên - hiển thị trọn vẹn 3 chỉ tiêu, 1 khung không cuộn)
            _pnlGridCard = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(_pnlTabContainer.Width, 218),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 10, 16, 10)
            };
            _pnlGridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlGridCard.Width - 1, _pnlGridCard.Height - 1);
            };

            var lblGridTitle = new Label
            {
                Text = "Bảng ma trận tài chính (Tổng thu, Tổng chi, Lợi nhuận qua 4 Quý)",
                Location = new Point(16, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            _pnlGridCard.Controls.Add(lblGridTitle);

            _dgvTaiChinhQuy = new UcDataGridView
            {
                Location = new Point(16, 34),
                Size = new Size(_pnlGridCard.Width - 32, 170),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = ScrollBars.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            SetupGridTaiChinh();
            _pnlGridCard.Controls.Add(_dgvTaiChinhQuy);
            _pnlTabTaiChinh.Controls.Add(_pnlGridCard);

            // Biểu đồ so sánh cột Quý (Ở dưới - thu nhỏ gọn gàng, hiển thị trọn vẹn 4 Quý)
            _pnlChartCard = new Panel
            {
                Location = new Point(0, 232),
                Size = new Size(_pnlTabContainer.Width, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 10, 16, 10)
            };
            _pnlChartCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlChartCard.Width - 1, _pnlChartCard.Height - 1);
            };

            var lblChartTitle = new Label
            {
                Text = "Biểu đồ trực quan: So sánh Thu vs Chi và Xu hướng Lợi nhuận 4 Quý",
                Location = new Point(16, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            _pnlChartCard.Controls.Add(lblChartTitle);

            _pnlChartArea = new Panel
            {
                Location = new Point(16, 30),
                Size = new Size(_pnlChartCard.Width - 32, 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            _chartRenderer = new QuarterComparisonChartRenderer(_pnlChartArea, _toolTip);
            _pnlChartCard.Controls.Add(_pnlChartArea);

            _pnlTabTaiChinh.Controls.Add(_pnlChartCard);
            _pnlTabContainer.Controls.Add(_pnlTabTaiChinh);

            _pnlTabTaiChinh.Resize += (s, e) =>
            {
                int w = _pnlTabTaiChinh.ClientSize.Width;
                if (_pnlGridCard != null) _pnlGridCard.Width = w;
                if (_pnlChartCard != null) _pnlChartCard.Width = w;
            };
        }

        private void SetupGridTaiChinh()
        {
            _dgvTaiChinhQuy.ScrollBars = ScrollBars.None;
            _dgvTaiChinhQuy.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvTaiChinhQuy.RowHeadersVisible = false;
            _dgvTaiChinhQuy.AllowUserToAddRows = false;
            _dgvTaiChinhQuy.AllowUserToResizeRows = false;
            _dgvTaiChinhQuy.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dgvTaiChinhQuy.ColumnHeadersHeight = 38;
            _dgvTaiChinhQuy.RowTemplate.Height = 42;
            _dgvTaiChinhQuy.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            _dgvTaiChinhQuy.Columns.Clear();

            var colChiTieu = new DataGridViewTextBoxColumn
            {
                Name = "ChiTieu",
                HeaderText = "CHỈ TIÊU TÀI CHÍNH",
                FillWeight = 28,
                MinimumWidth = 160
            };
            colChiTieu.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _dgvTaiChinhQuy.Columns.Add(colChiTieu);

            _dgvTaiChinhQuy.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy1", HeaderText = "QUÝ 1", FillWeight = 14, MinimumWidth = 80 });
            _dgvTaiChinhQuy.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy2", HeaderText = "QUÝ 2", FillWeight = 14, MinimumWidth = 80 });
            _dgvTaiChinhQuy.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy3", HeaderText = "QUÝ 3", FillWeight = 14, MinimumWidth = 80 });
            _dgvTaiChinhQuy.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy4", HeaderText = "QUÝ 4", FillWeight = 14, MinimumWidth = 80 });

            var colTong = new DataGridViewTextBoxColumn
            {
                Name = "TongNam",
                HeaderText = "TỔNG NĂM",
                FillWeight = 16,
                MinimumWidth = 100
            };
            colTong.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _dgvTaiChinhQuy.Columns.Add(colTong);

            for (int i = 1; i < _dgvTaiChinhQuy.Columns.Count; i++)
            {
                _dgvTaiChinhQuy.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgvTaiChinhQuy.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        // ================= TAB 2: DOANH THU THEO NHÂN VIÊN =================
        private void BuildTabNhanVien()
        {
            _pnlTabNhanVien = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var pnlCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Bảng thống kê doanh thu theo Nhân viên phục vụ qua 4 Quý (Xếp hạng doanh số)",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            pnlCard.Controls.Add(lblTitle);

            _dgvNhanVien = new UcDataGridView
            {
                Location = new Point(16, 42),
                Size = new Size(pnlCard.Width - 32, pnlCard.Height - 56),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = ScrollBars.Vertical,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            SetupGridNhanVien();
            pnlCard.Controls.Add(_dgvNhanVien);

            _pnlTabNhanVien.Controls.Add(pnlCard);
            _pnlTabContainer.Controls.Add(_pnlTabNhanVien);
        }

        private void SetupGridNhanVien()
        {
            _dgvNhanVien.ScrollBars = ScrollBars.Vertical;
            _dgvNhanVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvNhanVien.Font = new Font("Segoe UI", 8.5F);
            _dgvNhanVien.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _dgvNhanVien.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            _dgvNhanVien.RowTemplate.Height = 36;
            _dgvNhanVien.ColumnHeadersHeight = 36;
            _dgvNhanVien.Columns.Clear();

            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stt", HeaderText = "STT", FillWeight = 4, MinimumWidth = 30 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaNV", HeaderText = "MÃ NV", FillWeight = 8, MinimumWidth = 55 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenNV", HeaderText = "HỌ TÊN NHÂN VIÊN", FillWeight = 19, MinimumWidth = 110 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "ChucVu", HeaderText = "CHỨC VỤ", FillWeight = 11, MinimumWidth = 70 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy1", HeaderText = "QUÝ 1 (VNĐ)", FillWeight = 11, MinimumWidth = 65 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy2", HeaderText = "QUÝ 2 (VNĐ)", FillWeight = 11, MinimumWidth = 65 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy3", HeaderText = "QUÝ 3 (VNĐ)", FillWeight = 11, MinimumWidth = 65 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy4", HeaderText = "QUÝ 4 (VNĐ)", FillWeight = 11, MinimumWidth = 65 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongNam", HeaderText = "TỔNG NĂM (VNĐ)", FillWeight = 13, MinimumWidth = 80 });
            _dgvNhanVien.Columns.Add(new DataGridViewTextBoxColumn { Name = "TyLe", HeaderText = "TỶ LỆ (%)", FillWeight = 7, MinimumWidth = 50 });

            _dgvNhanVien.Columns["TongNam"].DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            for (int i = 4; i <= 9; i++)
            {
                _dgvNhanVien.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _dgvNhanVien.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        // ================= TAB 3: DOANH THU THEO MÓN ĂN =================
        private void BuildTabMonAn()
        {
            _pnlTabMonAn = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var pnlCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "Bảng thống kê doanh thu theo Sản phẩm / Món ăn",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            pnlCard.Controls.Add(lblTitle);

            // Bộ lọc phụ món ăn (Tìm kiếm & Chế độ xem Quý vs Nhiều năm)
            var lblSearch = new Label { Text = "🔍 Tìm món:", Location = new Point(16, 46), AutoSize = true, ForeColor = ThemeManager.Ink2 };
            pnlCard.Controls.Add(lblSearch);

            _txtSearchMon = new TextBox
            {
                Location = new Point(95, 43),
                Width = 200,
                Font = new Font("Segoe UI", 9F)
            };
            _txtSearchMon.TextChanged += (s, e) => RenderMonAnGrid();
            pnlCard.Controls.Add(_txtSearchMon);

            _radMonTheoQuy = new RadioButton
            {
                Text = "Xem theo 4 Quý của năm",
                Location = new Point(320, 44),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary
            };
            _radMonTheoQuy.CheckedChanged += (s, e) => { if (_radMonTheoQuy.Checked) RenderMonAnGrid(); };
            pnlCard.Controls.Add(_radMonTheoQuy);

            _radMonTheoNam = new RadioButton
            {
                Text = "Xem so sánh 3 Năm liên tiếp",
                Location = new Point(510, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Teal
            };
            _radMonTheoNam.CheckedChanged += (s, e) => { if (_radMonTheoNam.Checked) RenderMonAnGrid(); };
            pnlCard.Controls.Add(_radMonTheoNam);

            _dgvMonAn = new UcDataGridView
            {
                Location = new Point(16, 76),
                Size = new Size(pnlCard.Width - 32, pnlCard.Height - 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = ScrollBars.Vertical,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            pnlCard.Controls.Add(_dgvMonAn);

            _pnlTabMonAn.Controls.Add(pnlCard);
            _pnlTabContainer.Controls.Add(_pnlTabMonAn);
        }

        // ================= TẢI DỮ LIỆU TỪ API =================
        private async Task LoadDataAsync()
        {
            int selectedYear = DateTime.Now.Year;
            if (_cboNam.SelectedItem != null && int.TryParse(_cboNam.SelectedItem.ToString(), out int y))
            {
                selectedYear = y;
            }

            _lblStatus.Text = $"⏳ Đang tổng hợp dữ liệu năm {selectedYear}...";
            _btnXem.Enabled = false;
            _btnRefresh.Enabled = false;

            try
            {
                var data = await _service.GetBaoCaoDoanhThuTheoQuyAsync(selectedYear);
                if (data != null)
                {
                    _currentData = data;
                    _isDataLoaded = true;

                    // Đồng bộ lại danh sách năm có dữ liệu
                    if (data.DanhSachNam != null && data.DanhSachNam.Count > 0)
                    {
                        var curYears = _cboNam.Items.Cast<string>().ToList();
                        foreach (var yItem in data.DanhSachNam)
                        {
                            string yStr = yItem.ToString();
                            if (!curYears.Contains(yStr))
                                _cboNam.Items.Add(yStr);
                        }
                    }

                    RenderData();
                    _lblStatus.Text = $"✅ Dữ liệu năm {selectedYear} cập nhật lúc {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    _lblStatus.Text = "⚠️ Không thể kết nối hoặc không có dữ liệu.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "❌ Lỗi: " + ex.Message;
            }
            finally
            {
                _btnXem.Enabled = true;
                _btnRefresh.Enabled = true;
            }
        }

        private void RenderData()
        {
            if (_currentData == null) return;

            // 1. Cập nhật 4 thẻ KPI
            _cardTongThu.SetValue($"{_currentData.TongThuNam:N0} đ", $"{_currentData.TongDonNam} đơn hoàn tất");
            _cardTongChi.SetValue($"{_currentData.TongChiNam:N0} đ", "Chi nhập kho & giá vốn");
            _cardLoiNhuan.SetValue($"{_currentData.LoiNhuanNam:N0} đ", $"Biên lãi {_currentData.TyLeLoiNhuanNam:N1}%");
            _cardTop.SetValue(_currentData.NhanVienXuatSac, $"Món hot: {_currentData.MonBanChayNhat}");

            // 2. Render Tab 1: Tài chính Quý
            RenderTaiChinhGrid();
            _chartRenderer.SetData(_currentData.BangTaiChinhQuy);

            // 3. Render Tab 2: Nhân viên
            RenderNhanVienGrid();

            // 4. Render Tab 3: Món ăn
            RenderMonAnGrid();
        }

        private void RenderTaiChinhGrid()
        {
            _dgvTaiChinhQuy.Rows.Clear();
            if (_currentData?.BangTaiChinhQuy == null) return;

            // Chỉ nạp đúng 3 chỉ tiêu: Tổng thu, Tổng chi, Lợi nhuận thuần
            foreach (var r in _currentData.BangTaiChinhQuy.Where(x => x.DinhDang != "PhanTram").Take(3))
            {
                int idx = _dgvTaiChinhQuy.Rows.Add(r.ChiTieu, $"{r.Quy1:N0} đ", $"{r.Quy2:N0} đ", $"{r.Quy3:N0} đ", $"{r.Quy4:N0} đ", $"{r.TongNam:N0} đ");
                _dgvTaiChinhQuy.Rows[idx].Height = 42;
            }

            // Highlight dòng Lợi nhuận
            if (_dgvTaiChinhQuy.Rows.Count >= 3)
            {
                var rowLn = _dgvTaiChinhQuy.Rows[2];
                rowLn.DefaultCellStyle.BackColor = ThemeManager.GreenSoft;
                rowLn.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                rowLn.DefaultCellStyle.ForeColor = ThemeManager.Green;
            }

            // Tính toán và định kích thước vừa khít tuyệt đối cho 3 dòng trong 1 khung không cuộn
            int totalGridHeight = _dgvTaiChinhQuy.ColumnHeadersHeight;
            foreach (DataGridViewRow row in _dgvTaiChinhQuy.Rows)
            {
                totalGridHeight += row.Height;
            }
            _dgvTaiChinhQuy.Height = totalGridHeight + 6;

            if (_pnlGridCard != null)
            {
                _pnlGridCard.Height = _dgvTaiChinhQuy.Top + _dgvTaiChinhQuy.Height + 14;
                if (_pnlChartCard != null)
                {
                    _pnlChartCard.Top = _pnlGridCard.Bottom + 14;
                    if (_pnlTabContainer != null)
                    {
                        int reqHeight = _pnlChartCard.Bottom + 20;
                        if (_pnlTabContainer.Height < reqHeight)
                        {
                            _pnlTabContainer.Height = reqHeight;
                        }
                    }
                }
            }
        }

        private void RenderNhanVienGrid()
        {
            _dgvNhanVien.Rows.Clear();
            if (_currentData?.BangNhanVien == null) return;

            foreach (var nv in _currentData.BangNhanVien)
            {
                string tenNv = nv.TenNhanVien;
                if (nv.Stt == 1 && nv.TongNam > 0) tenNv = "🥇 " + tenNv;

                _dgvNhanVien.Rows.Add(
                    nv.Stt,
                    nv.MaNhanVien,
                    tenNv,
                    nv.ChucVu,
                    $"{nv.Quy1:N0} đ",
                    $"{nv.Quy2:N0} đ",
                    $"{nv.Quy3:N0} đ",
                    $"{nv.Quy4:N0} đ",
                    $"{nv.TongNam:N0} đ",
                    $"{nv.TyLeDongGop:N1}%"
                );
            }

            // Dòng tổng cộng
            double tongQ1 = _currentData.BangNhanVien.Sum(x => x.Quy1);
            double tongQ2 = _currentData.BangNhanVien.Sum(x => x.Quy2);
            double tongQ3 = _currentData.BangNhanVien.Sum(x => x.Quy3);
            double tongQ4 = _currentData.BangNhanVien.Sum(x => x.Quy4);
            double tongNam = _currentData.BangNhanVien.Sum(x => x.TongNam);

            int totalRowIdx = _dgvNhanVien.Rows.Add(
                "",
                "TỔNG",
                "TỔNG CỘNG TOÀN HỆ THỐNG",
                "",
                $"{tongQ1:N0} đ",
                $"{tongQ2:N0} đ",
                $"{tongQ3:N0} đ",
                $"{tongQ4:N0} đ",
                $"{tongNam:N0} đ",
                "100.0%"
            );

            var rTotal = _dgvNhanVien.Rows[totalRowIdx];
            rTotal.DefaultCellStyle.BackColor = ThemeManager.PrimarySoft;
            rTotal.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            rTotal.DefaultCellStyle.ForeColor = ThemeManager.PrimaryDark;
        }

        private void RenderMonAnGrid()
        {
            _dgvMonAn.ScrollBars = ScrollBars.Vertical;
            _dgvMonAn.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvMonAn.Font = new Font("Segoe UI", 8.5F);
            _dgvMonAn.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _dgvMonAn.DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
            _dgvMonAn.RowTemplate.Height = 36;
            _dgvMonAn.ColumnHeadersHeight = 36;
            _dgvMonAn.Columns.Clear();
            _dgvMonAn.Rows.Clear();
            if (_currentData?.BangMonAn == null) return;

            string keyword = _txtSearchMon.Text?.Trim().ToLower() ?? "";
            var list = _currentData.BangMonAn
                .Where(m => string.IsNullOrEmpty(keyword)
                         || m.TenMon.ToLower().Contains(keyword)
                         || m.DanhMuc.ToLower().Contains(keyword)
                         || m.MaMon.ToLower().Contains(keyword))
                .ToList();

            bool isTheoQuy = _radMonTheoQuy.Checked;

            if (isTheoQuy)
            {
                // Cột theo Quý của năm chọn (11 cột vừa khít 100% chiều ngang, không cuộn ngang)
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stt", HeaderText = "STT", FillWeight = 4, MinimumWidth = 28 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaMon", HeaderText = "MÃ MÓN", FillWeight = 7, MinimumWidth = 50 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenMon", HeaderText = "TÊN MÓN ĂN", FillWeight = 19, MinimumWidth = 100 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "DanhMuc", HeaderText = "DANH MỤC", FillWeight = 9, MinimumWidth = 60 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "SỐ LƯỢNG", FillWeight = 6, MinimumWidth = 40 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy1", HeaderText = "QUÝ 1 (VNĐ)", FillWeight = 10, MinimumWidth = 60 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy2", HeaderText = "QUÝ 2 (VNĐ)", FillWeight = 10, MinimumWidth = 60 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy3", HeaderText = "QUÝ 3 (VNĐ)", FillWeight = 10, MinimumWidth = 60 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quy4", HeaderText = "QUÝ 4 (VNĐ)", FillWeight = 10, MinimumWidth = 60 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongTien", HeaderText = "TỔNG NĂM (VNĐ)", FillWeight = 12, MinimumWidth = 75 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "TyLe", HeaderText = "TỶ LỆ (%)", FillWeight = 6, MinimumWidth = 40 });

                _dgvMonAn.Columns["TongTien"].DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

                for (int i = 4; i <= 10; i++)
                {
                    _dgvMonAn.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    _dgvMonAn.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                foreach (var m in list)
                {
                    _dgvMonAn.Rows.Add(
                        m.Stt,
                        m.MaMon,
                        m.TenMon,
                        m.DanhMuc,
                        m.TongSoLuongBan,
                        $"{m.Quy1:N0} đ",
                        $"{m.Quy2:N0} đ",
                        $"{m.Quy3:N0} đ",
                        $"{m.Quy4:N0} đ",
                        $"{m.TongDoanhThu:N0} đ",
                        $"{m.TyLeDongGop:N1}%"
                    );
                }
            }
            else
            {
                // Cột so sánh 3 Năm liên tiếp (9 cột vừa khít 100% chiều ngang, không cuộn ngang)
                int n1 = _currentData.CacNamSoSanhMonAn.Count > 0 ? _currentData.CacNamSoSanhMonAn[0] : _currentData.Nam - 2;
                int n2 = _currentData.CacNamSoSanhMonAn.Count > 1 ? _currentData.CacNamSoSanhMonAn[1] : _currentData.Nam - 1;
                int n3 = _currentData.CacNamSoSanhMonAn.Count > 2 ? _currentData.CacNamSoSanhMonAn[2] : _currentData.Nam;

                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stt", HeaderText = "STT", FillWeight = 4, MinimumWidth = 28 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaMon", HeaderText = "MÃ MÓN", FillWeight = 8, MinimumWidth = 55 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenMon", HeaderText = "TÊN MÓN ĂN", FillWeight = 22, MinimumWidth = 120 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "DanhMuc", HeaderText = "DANH MỤC", FillWeight = 11, MinimumWidth = 70 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "TỔNG SL", FillWeight = 7, MinimumWidth = 45 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nam1", HeaderText = $"NĂM {n1} (VNĐ)", FillWeight = 14, MinimumWidth = 70 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nam2", HeaderText = $"NĂM {n2} (VNĐ)", FillWeight = 14, MinimumWidth = 70 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nam3", HeaderText = $"NĂM {n3} (VNĐ)", FillWeight = 14, MinimumWidth = 70 });
                _dgvMonAn.Columns.Add(new DataGridViewTextBoxColumn { Name = "TongTien", HeaderText = "TỔNG TÍCH LŨY", FillWeight = 16, MinimumWidth = 80 });

                _dgvMonAn.Columns["TongTien"].DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

                for (int i = 4; i <= 8; i++)
                {
                    _dgvMonAn.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    _dgvMonAn.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                foreach (var m in list)
                {
                    double tong3Nam = m.DoanhThuNam1 + m.DoanhThuNam2 + m.DoanhThuNam3;
                    _dgvMonAn.Rows.Add(
                        m.Stt,
                        m.MaMon,
                        m.TenMon,
                        m.DanhMuc,
                        m.TongSoLuongBan,
                        $"{m.DoanhThuNam1:N0} đ",
                        $"{m.DoanhThuNam2:N0} đ",
                        $"{m.DoanhThuNam3:N0} đ",
                        $"{tong3Nam:N0} đ"
                    );
                }
            }
        }
    }
}
