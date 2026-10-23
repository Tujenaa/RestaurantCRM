using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.BaoCao {
    public class BaoCaoBanHangForm : Form {
        private readonly BaoCaoService _service;

        private ComboBox _cboQuickFilter;
        private DateTimePicker _dtpTuNgay;
        private DateTimePicker _dtpDenNgay;
        private Button _btnLoc;
        private Button _btnRefresh;

        private BaoCaoKpiTile _cardDoanhThu;
        private BaoCaoKpiTile _cardLoiNhuan;
        private BaoCaoKpiTile _cardGiaVon;
        private BaoCaoKpiTile _cardGiamGia;
        private BaoCaoKpiTile _cardDonHang;

        private Panel _pnlChartCard;
        private Panel _pnlChartArea;
        private Label _lblChartTitle;

        private DataGridView _dgvTopMon;
        private Label _lblStatus;

        private BaoCaoDoanhThuDto _currentData;
        private readonly ToolTip _chartToolTip;
        private int _hoveredIndex = -1;
        private bool _isUpdatingFilter = false;

        public BaoCaoBanHangForm() {
            _service = new BaoCaoService();

            _chartToolTip = new ToolTip {
                InitialDelay = 50,
                AutoPopDelay = 5000,
                ReshowDelay = 50,
                IsBalloon = false
            };

            Text = "Báo cáo bán hàng | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1180, 840);
            MinimumSize = new Size(950, 680);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            BuildUI();

            // Mặc định chọn 7 ngày qua
            _cboQuickFilter.SelectedIndex = 2; // 7 ngày qua
        }

        private void BuildUI() {
            var scrollPanel = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(20, 10, 20, 20)
            };
            Controls.Add(scrollPanel);

            int y = 10;
            int fullWidth = 1100;

            // 1. THANH BỘ LỌC THỜI GIAN (Gọn gàng, tích hợp nút Làm mới & Xem báo cáo trên 1 hàng)
            var pnlFilter = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 52),
                BackColor = ThemeManager.Paper,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlFilter.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlFilter.Width - 1, pnlFilter.Height - 1);
                }
            };

            var lblQuick = new Label { 
                Text = "Mốc thời gian:", 
                Location = new Point(14, 16), 
                AutoSize = true, 
                ForeColor = ThemeManager.Ink, 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                UseMnemonic = false 
            };
            pnlFilter.Controls.Add(lblQuick);

            _cboQuickFilter = new ComboBox {
                Location = new Point(110, 13),
                Width = 135,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            _cboQuickFilter.Items.AddRange(new object[] { "Hôm nay", "Hôm qua", "7 ngày qua", "30 ngày qua", "Tháng này", "Tháng trước", "Tùy chọn" });
            _cboQuickFilter.SelectedIndexChanged += OnQuickFilterChanged;
            pnlFilter.Controls.Add(_cboQuickFilter);

            var lblTu = new Label { Text = "Từ ngày:", Location = new Point(265, 16), AutoSize = true, ForeColor = ThemeManager.Ink2, UseMnemonic = false };
            pnlFilter.Controls.Add(lblTu);

            _dtpTuNgay = new DateTimePicker {
                Location = new Point(325, 13),
                Width = 125,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Tag = UiStyler.NoStyleTag
            };
            _dtpTuNgay.ValueChanged += (s, e) => {
                if (!_isUpdatingFilter) _cboQuickFilter.SelectedIndex = 6;
            };
            pnlFilter.Controls.Add(_dtpTuNgay);

            var lblDen = new Label { Text = "Đến ngày:", Location = new Point(460, 16), AutoSize = true, ForeColor = ThemeManager.Ink2, UseMnemonic = false };
            pnlFilter.Controls.Add(lblDen);

            _dtpDenNgay = new DateTimePicker {
                Location = new Point(530, 13),
                Width = 125,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Tag = UiStyler.NoStyleTag
            };
            _dtpDenNgay.ValueChanged += (s, e) => {
                if (!_isUpdatingFilter) _cboQuickFilter.SelectedIndex = 6;
            };
            pnlFilter.Controls.Add(_dtpDenNgay);

            _btnLoc = new Button {
                Text = "⚡ Xem báo cáo",
                Location = new Point(668, 10),
                Size = new Size(130, 32),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLoc.FlatAppearance.BorderSize = 0;
            _btnLoc.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(_btnLoc);

            _btnRefresh = new Button {
                Text = "🔄 Làm mới",
                Location = new Point(808, 10),
                Size = new Size(105, 32),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(_btnRefresh);

            scrollPanel.Controls.Add(pnlFilter);
            y += 62;

            // 2. HÀNG 5 THẺ KPI TỰ CO GIÃN THEO TỶ LỆ (TABLELAYOUTPANEL 5 CỘT 20%)
            var pnlCardsTable = new TableLayoutPanel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 122),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Margin = new Padding(0)
            };
            pnlCardsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 5; i++) {
                pnlCardsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            }

            _cardDoanhThu = new BaoCaoKpiTile("DOANH THU THỰC THU", "0 đ", "Đang tải...", "💰", ThemeManager.Primary);
            _cardLoiNhuan = new BaoCaoKpiTile("LỢI NHUẬN GỘP", "0 đ", "Biên lãi 0%", "📈", ThemeManager.Green);
            _cardGiaVon = new BaoCaoKpiTile("GIÁ VỐN NGUYÊN LIỆU", "0 đ", "Chi phí COGS", "📦", ThemeManager.Amber);
            _cardGiamGia = new BaoCaoKpiTile("CHIẾT KHẤU ƯU ĐÃI", "0 đ", "Sale & Voucher", "🏷️", ThemeManager.Red);
            _cardDonHang = new BaoCaoKpiTile("ĐƠN HOÀN TẤT", "0", "0 đ/đơn", "✅", ThemeManager.Teal);
            _cardDonHang.Margin = new Padding(0);

            pnlCardsTable.Controls.Add(_cardDoanhThu, 0, 0);
            pnlCardsTable.Controls.Add(_cardLoiNhuan, 1, 0);
            pnlCardsTable.Controls.Add(_cardGiaVon, 2, 0);
            pnlCardsTable.Controls.Add(_cardGiamGia, 3, 0);
            pnlCardsTable.Controls.Add(_cardDonHang, 4, 0);

            scrollPanel.Controls.Add(pnlCardsTable);
            y += 140;

            // 3. KHỐI BIỂU ĐỒ DOANH THU THEO NGÀY (Tăng chiều cao đáy để không bao giờ bị cắt ngày tháng)
            _pnlChartCard = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 335),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 12, 16, 12)
            };
            _pnlChartCard.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlChartCard.Width - 1, _pnlChartCard.Height - 1);
                }
            };

            _lblChartTitle = new Label {
                Text = "Doanh thu theo ngày (Thực thu)",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                UseMnemonic = false
            };
            _pnlChartCard.Controls.Add(_lblChartTitle);

            _pnlChartArea = new Panel {
                Location = new Point(16, 42),
                Size = new Size(_pnlChartCard.Width - 32, 275),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            _pnlChartArea.Paint += DrawChart;
            _pnlChartArea.MouseMove += OnChartMouseMove;
            _pnlChartArea.MouseLeave += OnChartMouseLeave;
            _pnlChartCard.Controls.Add(_pnlChartArea);

            scrollPanel.Controls.Add(_pnlChartCard);
            y += 345;

            // 4. BẢNG CƠ CẤU DOANH THU & LỢI NHUẬN THEO MÓN ĂN
            var pnlGridCard = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlGridCard.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGridCard.Width - 1, pnlGridCard.Height - 1);
                }
            };

            var lblGridTitle = new Label {
                Text = "⭐ Cơ cấu doanh thu & Lợi nhuận theo món ăn (Top bán chạy)",
                Location = new Point(16, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                UseMnemonic = false
            };
            pnlGridCard.Controls.Add(lblGridTitle);

            _dgvTopMon = new DataGridView {
                Location = new Point(16, 38),
                Size = new Size(pnlGridCard.Width - 32, 216),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            _dgvTopMon.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Surface2;
            _dgvTopMon.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Ink;
            _dgvTopMon.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvTopMon.ColumnHeadersHeight = 34;
            _dgvTopMon.RowTemplate.Height = 30;

            _dgvTopMon.Columns.Add("TenMon", "Tên món ăn");
            _dgvTopMon.Columns.Add("SoLuong", "Số lượng bán");
            _dgvTopMon.Columns.Add("DoanhThu", "Doanh thu (VNĐ)");
            _dgvTopMon.Columns.Add("GiaVon", "Giá vốn (VNĐ)");
            _dgvTopMon.Columns.Add("LoiNhuan", "Lợi nhuận gộp (VNĐ)");
            _dgvTopMon.Columns.Add("TyLeLai", "Biên lãi (%)");

            _dgvTopMon.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dgvTopMon.Columns["DoanhThu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvTopMon.Columns["GiaVon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvTopMon.Columns["LoiNhuan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvTopMon.Columns["TyLeLai"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            _dgvTopMon.Columns["TenMon"].FillWeight = 110;
            _dgvTopMon.Columns["SoLuong"].FillWeight = 45;
            _dgvTopMon.Columns["DoanhThu"].FillWeight = 75;
            _dgvTopMon.Columns["GiaVon"].FillWeight = 75;
            _dgvTopMon.Columns["LoiNhuan"].FillWeight = 75;
            _dgvTopMon.Columns["TyLeLai"].FillWeight = 50;

            pnlGridCard.Controls.Add(_dgvTopMon);
            scrollPanel.Controls.Add(pnlGridCard);
            y += 280;

            // 5. DÒNG TRẠNG THÁI CUỐI TRANG
            _lblStatus = new Label {
                Text = "Dữ liệu được cập nhật từ hệ thống quản lý bán hàng RestaurantCRM.",
                Location = new Point(22, y),
                AutoSize = true,
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                UseMnemonic = false
            };
            scrollPanel.Controls.Add(_lblStatus);
        }

        private void OnQuickFilterChanged(object sender, EventArgs e) {
            if (_isUpdatingFilter) return;
            _isUpdatingFilter = true;
            try {
                var today = DateTime.Today;
                switch (_cboQuickFilter.SelectedIndex) {
                    case 0: // Hôm nay
                        _dtpTuNgay.Value = today;
                        _dtpDenNgay.Value = today;
                        break;
                    case 1: // Hôm qua
                        _dtpTuNgay.Value = today.AddDays(-1);
                        _dtpDenNgay.Value = today.AddDays(-1);
                        break;
                    case 2: // 7 ngày qua
                        _dtpTuNgay.Value = today.AddDays(-6);
                        _dtpDenNgay.Value = today;
                        break;
                    case 3: // 30 ngày qua
                        _dtpTuNgay.Value = today.AddDays(-29);
                        _dtpDenNgay.Value = today;
                        break;
                    case 4: // Tháng này
                        _dtpTuNgay.Value = new DateTime(today.Year, today.Month, 1);
                        _dtpDenNgay.Value = today;
                        break;
                    case 5: // Tháng trước
                        var lastMonth = today.AddMonths(-1);
                        _dtpTuNgay.Value = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                        _dtpDenNgay.Value = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                        break;
                    case 6: // Tùy chọn (giữ nguyên ngày đang chọn)
                        return;
                }
            } finally {
                _isUpdatingFilter = false;
            }

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync() {
            var tuNgay = _dtpTuNgay.Value.Date;
            var denNgay = _dtpDenNgay.Value.Date;

            if (tuNgay > denNgay) {
                MessageBox.Show(this, "Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnLoc.Enabled = false;
            _btnLoc.Text = "⏳ Đang tải...";

            try {
                _currentData = await _service.GetBaoCaoDoanhThuAsync(tuNgay, denNgay);
                if (_currentData != null) {
                    RenderData(_currentData);
                    _lblStatus.Text = $"Dữ liệu từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy} | Cập nhật lúc: {DateTime.Now:HH:mm:ss}";
                } else {
                    _lblStatus.Text = "Không thể tải dữ liệu báo cáo từ máy chủ.";
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "Lỗi khi lấy báo cáo doanh thu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                _btnLoc.Enabled = true;
                _btnLoc.Text = "⚡ Xem báo cáo";
            }
        }

        private void RenderData(BaoCaoDoanhThuDto d) {
            // Cập nhật 5 thẻ KPI
            _cardDoanhThu.SetValue($"{d.TongDoanhThu:N0} đ", $"{d.DonHoanThanh} đơn hoàn tất");
            _cardLoiNhuan.SetValue($"{d.LoiNhuanGop:N0} đ", $"Biên lãi {d.TyLeLoiNhuan:N1}%");
            _cardGiaVon.SetValue($"{d.TongGiaVon:N0} đ", "Giá vốn COGS");
            _cardGiamGia.SetValue($"{d.TongGiamGia:N0} đ", "Giảm món & Voucher");
            _cardDonHang.SetValue($"{d.DonHoanThanh:N0}", $"Đơn TB: {d.GiaTriDonTb:N0} đ");

            _lblChartTitle.Text = $"Doanh thu theo ngày (Tổng: {d.TongDoanhThu:N0} đ / {d.DonHoanThanh} đơn)";

            // Vẽ lại biểu đồ
            _pnlChartArea.Invalidate();

            // Đổ dữ liệu vào bảng Top Món
            _dgvTopMon.Rows.Clear();
            if (d.TopMonBanChay != null && d.TopMonBanChay.Count > 0) {
                foreach (var m in d.TopMonBanChay) {
                    int rowIdx = _dgvTopMon.Rows.Add(
                        m.TenMon,
                        $"{m.SoLuong:N0}",
                        $"{m.DoanhThu:N0}",
                        $"{m.GiaVon:N0}",
                        $"{m.LoiNhuan:N0}",
                        $"{m.TyLeLai:N1}%"
                    );

                    var cellLai = _dgvTopMon.Rows[rowIdx].Cells["TyLeLai"];
                    if (m.TyLeLai >= 30.0) {
                        cellLai.Style.ForeColor = ThemeManager.Green;
                        cellLai.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    } else if (m.TyLeLai < 20.0) {
                        cellLai.Style.ForeColor = ThemeManager.Red;
                        cellLai.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                }
            }
        }

        private void DrawChart(object sender, PaintEventArgs e) {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_currentData == null || _currentData.DoanhThuTheoNgay == null || _currentData.DoanhThuTheoNgay.Count == 0) {
                using (var f = new Font("Segoe UI", 10F, FontStyle.Regular)) {
                    TextRenderer.DrawText(g, "Chưa có dữ liệu đơn hàng trong khoảng thời gian này.", f,
                        _pnlChartArea.ClientRectangle, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            var items = _currentData.DoanhThuTheoNgay;
            int count = items.Count;
            if (count == 0) return;

            int w = _pnlChartArea.Width;
            int h = _pnlChartArea.Height;

            int bottomMargin = 52;
            int topMargin = 26;
            int leftMargin = 58;
            int rightMargin = 20;

            int chartWidth = w - leftMargin - rightMargin;
            int chartHeight = h - bottomMargin - topMargin;

            double maxVal = items.Max(x => x.DoanhThu);
            if (maxVal <= 0) maxVal = 100000;

            // 1. Vẽ các đường gióng ngang mờ (Gridlines) & nhãn mốc tiền
            using (var gridPen = new Pen(Color.FromArgb(235, 240, 246), 1f))
            using (var fGrid = new Font("Segoe UI", 7.5F, FontStyle.Regular)) {
                gridPen.DashStyle = DashStyle.Dash;
                for (int level = 0; level <= 3; level++) {
                    int gy = h - bottomMargin - (level * chartHeight / 3);
                    g.DrawLine(gridPen, leftMargin, gy, w - rightMargin, gy);

                    double valLevel = maxVal * level / 3.0;
                    string labelLevel = FormatCompactMoney(valLevel);
                    var rectLabel = new Rectangle(0, gy - 8, leftMargin - 6, 16);
                    TextRenderer.DrawText(g, labelLevel, fGrid, rectLabel, ThemeManager.Muted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            }

            // Vẽ đường trục ngang cơ sở
            using (var baseLinePen = new Pen(Color.FromArgb(203, 213, 225), 1.2f)) {
                g.DrawLine(baseLinePen, leftMargin, h - bottomMargin, w - rightMargin, h - bottomMargin);
            }

            // 2. Tính bề rộng từng slot cột
            int slotWidth = Math.Max(18, chartWidth / count);
            int barWidth = Math.Min(38, Math.Max(8, slotWidth - (count > 20 ? 6 : 10)));

            // Tự động tính bước nhảy (step) cho nhãn ngày tháng để KHÔNG BAO GIỜ bị dính chữ khi xem theo tháng (30 ngày)
            // Mỗi nhãn ngày tháng cần tối thiểu 44px khoảng cách
            int step = 1;
            if (slotWidth < 44) {
                double ratio = 44.0 / slotWidth;
                if (ratio <= 2.2) step = 2;       // Cách 1 ngày (1, 3, 5, 7, 9...)
                else if (ratio <= 3.2) step = 3;  // Cách 2 ngày (1, 4, 7, 10...)
                else step = 5;                    // Cách 4 ngày (1, 6, 11, 16...)
            }

            int lastDrawnDateSlotX = -999;
            int lastDrawnMoneyRightX = -999;

            using (var fontDateThu = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fontDateNgay = new Font("Segoe UI", 8F, FontStyle.Regular))
            using (var fontValue = new Font("Segoe UI", count > 18 ? 7.5F : 8F, FontStyle.Bold))
            using (var tickPen = new Pen(Color.FromArgb(148, 163, 184), 1f))
            using (var hoverBrush = new SolidBrush(Color.FromArgb(24, 13, 148, 136))) {
                for (int i = 0; i < count; i++) {
                    var item = items[i];
                    int slotX = leftMargin + i * slotWidth;
                    int barX = slotX + (slotWidth - barWidth) / 2;

                    // Vẽ vùng highlight dịu nhẹ khi rê chuột vào ngày đó
                    if (i == _hoveredIndex) {
                        g.FillRectangle(hoverBrush, slotX, topMargin, slotWidth, chartHeight);
                    }

                    int barHeight = (int)((item.DoanhThu / maxVal) * chartHeight);
                    int barY = h - bottomMargin - barHeight;

                    // Vẽ vạch đế mờ nếu doanh thu = 0 để biểu đồ không trơ trọi
                    if (item.DoanhThu <= 0) {
                        var zeroRect = new Rectangle(barX, h - bottomMargin - 3, barWidth, 3);
                        using (var br = new SolidBrush(Color.FromArgb(226, 232, 240))) {
                            g.FillRectangle(br, zeroRect);
                        }
                    } else {
                        if (barHeight < 4) barHeight = 4;
                        var barRect = new Rectangle(barX, barY, barWidth, barHeight);
                        var barColor = (i == _hoveredIndex) ? Color.FromArgb(15, 118, 110) : Color.FromArgb(13, 148, 136);
                        using (var brush = new SolidBrush(barColor)) {
                            g.FillRectangle(brush, barRect);
                        }

                        // Nhãn số tiền trên đầu cột (chỉ vẽ nếu không va chạm với nhãn cột trước)
                        string valStr = FormatCompactMoney(item.DoanhThu);
                        int valWidth = Math.Max(slotWidth + 12, 38);
                        int valLeft = slotX + (slotWidth - valWidth) / 2;

                        if (valLeft > lastDrawnMoneyRightX + 2 || i == _hoveredIndex) {
                            var valRect = new Rectangle(valLeft, Math.Max(2, barY - 18), valWidth, 16);
                            TextRenderer.DrawText(g, valStr, fontValue, valRect, ThemeManager.Ink,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                            lastDrawnMoneyRightX = valLeft + valWidth;
                        }
                    }

                    // Quyết định vẽ nhãn ngày: tại các mốc chia hết cho step hoặc ngày cuối cùng
                    bool shouldDrawDate = (i % step == 0);
                    if (i == count - 1 && (slotX - lastDrawnDateSlotX) >= 36) {
                        shouldDrawDate = true;
                    }

                    if (shouldDrawDate) {
                        lastDrawnDateSlotX = slotX;
                        int centerX = slotX + slotWidth / 2;

                        // Vẽ vạch khấc (tick mark) nhỏ trên trục hoành để định vị cột rõ ràng
                        g.DrawLine(tickPen, centerX, h - bottomMargin, centerX, h - bottomMargin + 4);

                        int dateBoxWidth = Math.Max(slotWidth, 44);
                        var rectThu = new Rectangle(centerX - dateBoxWidth / 2, h - bottomMargin + 6, dateBoxWidth, 16);
                        var rectNgay = new Rectangle(centerX - dateBoxWidth / 2, h - bottomMargin + (count <= 14 ? 22 : 8), dateBoxWidth, 16);

                        if (count <= 14) {
                            // Khoảng thời gian ngắn (<= 14 ngày): hiển thị cả Thứ và Ngày rõ nét
                            TextRenderer.DrawText(g, item.Thu, fontDateThu, rectThu, ThemeManager.Ink2,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                            TextRenderer.DrawText(g, item.NgayHienThi, fontDateNgay, rectNgay, ThemeManager.Muted,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                        } else {
                            // Khoảng thời gian dài (theo tháng: 30-31 ngày): hiển thị Ngày dd/MM tại các mốc thoáng đãng
                            TextRenderer.DrawText(g, item.NgayHienThi, fontDateThu, rectNgay, ThemeManager.Ink,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                        }
                    }
                }
            }
        }

        private void OnChartMouseMove(object sender, MouseEventArgs e) {
            if (_currentData?.DoanhThuTheoNgay == null || _currentData.DoanhThuTheoNgay.Count == 0) return;
            var items = _currentData.DoanhThuTheoNgay;
            int count = items.Count;

            int leftMargin = 55;
            int rightMargin = 20;
            int chartWidth = _pnlChartArea.Width - leftMargin - rightMargin;
            int slotWidth = Math.Max(18, chartWidth / count);

            int idx = (e.X - leftMargin) / slotWidth;
            if (idx >= 0 && idx < count && e.X >= leftMargin && e.X <= _pnlChartArea.Width - rightMargin) {
                if (idx != _hoveredIndex) {
                    _hoveredIndex = idx;
                    var item = items[idx];
                    string tip = $"📅 {item.Thu}, ngày {item.NgayHienThi} ({item.Ngay:dd/MM/yyyy})\n💰 Doanh thu: {item.DoanhThu:N0} đ\n🧾 Đơn hoàn tất: {item.SoDon:N0} đơn";
                    _chartToolTip.Show(tip, _pnlChartArea, e.X + 12, Math.Max(10, e.Y - 50), 3500);
                    _pnlChartArea.Invalidate();
                }
            } else {
                if (_hoveredIndex != -1) {
                    _hoveredIndex = -1;
                    _chartToolTip.Hide(_pnlChartArea);
                    _pnlChartArea.Invalidate();
                }
            }
        }

        private void OnChartMouseLeave(object sender, EventArgs e) {
            if (_hoveredIndex != -1) {
                _hoveredIndex = -1;
                _chartToolTip?.Hide(_pnlChartArea);
                _pnlChartArea.Invalidate();
            }
        }

        private string FormatCompactMoney(double amount) {
            if (amount >= 1000000000) {
                return (amount / 1000000000.0).ToString("0.#") + "B";
            }
            if (amount >= 1000000) {
                return (amount / 1000000.0).ToString("0.#") + "M";
            }
            if (amount >= 1000) {
                return (amount / 1000.0).ToString("0.#") + "k";
            }
            return amount.ToString("N0");
        }

        // ================= LỚP THẺ KPI TỰ CO GIÃN THÔNG MINH (CHỐNG TRÀN SỐ TIỀN LỚN) =================
        public class BaoCaoKpiTile : RoundedPanel {
            private readonly Label _lblTitle;
            private readonly Label _lblValue;
            private readonly Label _lblSub;
            private readonly string _icon;
            private readonly Color _accent;

            public BaoCaoKpiTile(string title, string value, string sub, string icon, Color accent) {
                _icon = icon;
                _accent = accent;
                Radius = 14;
                FillColor = ThemeManager.Paper;
                BorderColor = Color.FromArgb(226, 232, 240);
                Margin = new Padding(0, 0, 10, 0);
                Dock = DockStyle.Fill;

                _lblTitle = new Label {
                    Text = title.ToUpper(),
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = ThemeManager.Muted,
                    Location = new Point(14, 12),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Size = new Size(135, 18),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent,
                    UseMnemonic = false
                };
                Controls.Add(_lblTitle);

                _lblValue = new Label {
                    Text = value,
                    Font = new Font("Segoe UI", 15.5F, FontStyle.Bold),
                    ForeColor = ThemeManager.Ink,
                    Location = new Point(12, 34),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Size = new Size(150, 42),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent,
                    UseMnemonic = false
                };
                Controls.Add(_lblValue);

                _lblSub = new Label {
                    Text = "↑ " + sub,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = accent,
                    Location = new Point(14, 82),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Size = new Size(150, 24),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent,
                    UseMnemonic = false
                };
                Controls.Add(_lblSub);

                Resize += (s, e) => UpdateLabelBounds();
                UpdateLabelBounds();
            }

            private void UpdateLabelBounds() {
                int titleWidth = Math.Max(40, Width - 46); // Chừa chỗ cho badge icon bên phải
                int contentWidth = Math.Max(40, Width - 24);
                if (_lblTitle != null) {
                    _lblTitle.Width = titleWidth;
                    _lblTitle.Height = 18;
                }
                if (_lblValue != null) {
                    _lblValue.Width = contentWidth;
                    _lblValue.Height = 42; // Chiều cao 42px hiển thị đầy đủ 100% font lớn không bị mất nửa số
                }
                if (_lblSub != null) {
                    _lblSub.Width = contentWidth;
                    _lblSub.Height = 24;
                }
            }

            public void SetValue(string val, string sub = null) {
                // Tự động điều chỉnh kích cỡ font thông minh theo độ dài số tiền để không bao giờ bị tràn khung
                if (_lblValue != null) {
                    float fontSize = 15.5F;
                    int len = val.Length;
                    if (len > 17) fontSize = 10.5F;       // ví dụ: 120,500,000,000 đ
                    else if (len > 14) fontSize = 12F;     // ví dụ: 1,500,000,000 đ
                    else if (len > 11) fontSize = 13.5F;   // ví dụ: 350,000,000 đ

                    _lblValue.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
                    _lblValue.Height = 42;
                    _lblValue.Text = val;
                }

                if (sub != null && _lblSub != null) {
                    _lblSub.Text = "↑ " + sub;
                }
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Badge icon gọn gàng ở góc trên bên phải (28x28px)
                var badge = new Rectangle(Width - 38, 12, 28, 28);
                using (var path = ThemeManager.RoundedRect(badge, 8))
                using (var brush = new SolidBrush(ThemeManager.SoftFor(_accent)))
                    g.FillPath(brush, path);

                using (var pen = new Pen(Color.FromArgb(40, _accent), 1f))
                using (var path = ThemeManager.RoundedRect(badge, 8))
                    g.DrawPath(pen, path);

                using (var f = new Font(ThemeManager.EmojiFont, 10F))
                    TextRenderer.DrawText(g, _icon, f, badge, _accent,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}
