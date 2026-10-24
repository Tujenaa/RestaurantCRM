using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Controls.BaoCao;
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
        private SalesLineChartRenderer _chartRenderer;

        private DataGridView _dgvTopMon;
        private Label _lblStatus;

        private BaoCaoDoanhThuDto _currentData;
        private readonly ToolTip _chartToolTip;
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
            _chartRenderer = new SalesLineChartRenderer(_pnlChartArea, _chartToolTip);
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
            _chartRenderer?.SetData(d.DoanhThuTheoNgay);

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

        // Tương thích ngược: Cho phép các tham chiếu kiểu cũ (nếu có)
        public class BaoCaoKpiTile : RestaurantCRM.AdminApp.Controls.BaoCao.BaoCaoKpiTile {
            public BaoCaoKpiTile(string title, string value, string sub, string icon, Color accent)
                : base(title, value, sub, icon, accent) { }
        }
    }
}
