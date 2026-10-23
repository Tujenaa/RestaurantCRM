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

        private UcThongKe _cardDoanhThu;
        private UcThongKe _cardLoiNhuan;
        private UcThongKe _cardGiaVon;
        private UcThongKe _cardGiamGia;
        private UcThongKe _cardDonHang;

        private Panel _pnlChartCard;
        private Panel _pnlChartArea;
        private Label _lblChartTitle;

        private DataGridView _dgvTopMon;
        private Label _lblStatus;

        private BaoCaoDoanhThuDto _currentData;

        public BaoCaoBanHangForm() {
            _service = new BaoCaoService();

            Text = "Báo cáo bán hàng | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1180, 800);
            MinimumSize = new Size(1000, 700);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            InitLayout();

            // Mặc định chọn 7 ngày qua
            _cboQuickFilter.SelectedIndex = 2; // 7 ngày qua
        }

        private void InitLayout() {
            var scrollPanel = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(20, 14, 20, 20)
            };
            Controls.Add(scrollPanel);

            int y = 14;
            int fullWidth = 1100;

            // 1. Header Title & Actions (hỗ trợ cả nhúng trong Dashboard tab)
            var pnlHeader = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 68),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Bg
            };

            var lblTitle = new Label {
                Text = "Báo cáo bán hàng & Doanh thu",
                Location = new Point(0, 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                UseMnemonic = false
            };
            pnlHeader.Controls.Add(lblTitle);

            var lblSubtitle = new Label {
                Text = "Theo dõi doanh thu thực thu, chiết khấu khuyến mãi, giá vốn nguyên liệu và biên lợi nhuận.",
                Location = new Point(2, 38),
                AutoSize = true,
                ForeColor = ThemeManager.Muted,
                UseMnemonic = false
            };
            pnlHeader.Controls.Add(lblSubtitle);

            _btnRefresh = new Button {
                Text = "Làm mới",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(95, 32),
                Cursor = Cursors.Hand
            };
            ButtonSkin.SetKind(_btnRefresh, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(_btnRefresh);
            _btnRefresh.Location = new Point(pnlHeader.Width - _btnRefresh.Width - 4, 16);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlHeader.Controls.Add(_btnRefresh);

            Action place = () => {
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSubtitle.Location = new Point(2, embedded ? 12 : 38);
                pnlHeader.Height = embedded ? 48 : 68;
                _btnRefresh.Location = new Point(pnlHeader.Width - _btnRefresh.Width - 4, embedded ? 6 : 16);
            };
            pnlHeader.Resize += (s, e) => place();
            ParentChanged += (s, e) => place();

            scrollPanel.Controls.Add(pnlHeader);
            y += 74;

            // 2. Panel Bộ lọc thời gian (Filter Bar)
            var pnlFilter = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 54),
                BackColor = ThemeManager.Paper,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlFilter.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlFilter.Width - 1, pnlFilter.Height - 1);
                }
            };

            var lblQuick = new Label { Text = "Mốc thời gian:", Location = new Point(14, 16), AutoSize = true, ForeColor = ThemeManager.Ink, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), UseMnemonic = false };
            pnlFilter.Controls.Add(lblQuick);

            _cboQuickFilter = new ComboBox {
                Location = new Point(115, 13),
                Width = 135,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            _cboQuickFilter.Items.AddRange(new object[] { "Hôm nay", "Hôm qua", "7 ngày qua", "30 ngày qua", "Tháng này", "Tháng trước", "Tùy chọn" });
            _cboQuickFilter.SelectedIndexChanged += OnQuickFilterChanged;
            pnlFilter.Controls.Add(_cboQuickFilter);

            var lblTu = new Label { Text = "Từ ngày:", Location = new Point(270, 16), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false };
            pnlFilter.Controls.Add(lblTu);

            _dtpTuNgay = new DateTimePicker {
                Location = new Point(330, 13),
                Width = 120,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            pnlFilter.Controls.Add(_dtpTuNgay);

            var lblDen = new Label { Text = "Đến ngày:", Location = new Point(465, 16), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false };
            pnlFilter.Controls.Add(lblDen);

            _dtpDenNgay = new DateTimePicker {
                Location = new Point(535, 13),
                Width = 120,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            pnlFilter.Controls.Add(_dtpDenNgay);

            _btnLoc = new Button {
                Text = "⚡ Xem báo cáo",
                Location = new Point(675, 10),
                Size = new Size(130, 34),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLoc.FlatAppearance.BorderSize = 0;
            _btnLoc.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(_btnLoc);

            scrollPanel.Controls.Add(pnlFilter);
            y += 66;

            // 3. Hàng Thẻ KPI (KPI Cards)
            var pnlCards = new FlowLayoutPanel {
                Location = new Point(20, y),
                Size = new Size(fullWidth + 20, 132),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                WrapContents = false,
                BackColor = ThemeManager.Bg,
                Margin = new Padding(0)
            };

            _cardDoanhThu = new UcThongKe("Doanh thu thực thu", "0 đ", "Đang tải...", "💰", ThemeManager.Primary);
            _cardLoiNhuan = new UcThongKe("Lợi nhuận gộp", "0 đ", "Biên lãi 0%", "📈", ThemeManager.Green);
            _cardGiaVon = new UcThongKe("Giá vốn nguyên liệu", "0 đ", "BOM xuất kho", "📦", ThemeManager.Amber);
            _cardGiamGia = new UcThongKe("Chiết khấu ưu đãi", "0 đ", "Voucher & Sale", "🏷️", ThemeManager.Red);
            _cardDonHang = new UcThongKe("Đơn hoàn tất", "0", "0 đơn/ngày", "✅", ThemeManager.Teal);

            pnlCards.Controls.Add(_cardDoanhThu);
            pnlCards.Controls.Add(_cardLoiNhuan);
            pnlCards.Controls.Add(_cardGiaVon);
            pnlCards.Controls.Add(_cardGiamGia);
            pnlCards.Controls.Add(_cardDonHang);

            scrollPanel.Controls.Add(pnlCards);
            y += 140;

            // 4. Khối Biểu đồ Doanh thu theo ngày
            _pnlChartCard = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 310),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(20)
            };
            _pnlChartCard.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlChartCard.Width - 1, _pnlChartCard.Height - 1);
                }
            };

            _lblChartTitle = new Label {
                Text = "Doanh thu theo ngày (Thực thu)",
                Location = new Point(18, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                UseMnemonic = false
            };
            _pnlChartCard.Controls.Add(_lblChartTitle);

            _pnlChartArea = new Panel {
                Location = new Point(18, 48),
                Size = new Size(_pnlChartCard.Width - 36, 245),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            _pnlChartArea.Paint += DrawChart;
            _pnlChartCard.Controls.Add(_pnlChartArea);

            scrollPanel.Controls.Add(_pnlChartCard);
            y += 325;

            // 5. Bảng Cơ cấu Doanh thu & Lợi nhuận theo Món ăn
            var pnlGridCard = new Panel {
                Location = new Point(20, y),
                Size = new Size(fullWidth, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16)
            };
            pnlGridCard.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.2f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGridCard.Width - 1, pnlGridCard.Height - 1);
                }
            };

            var lblGridTitle = new Label {
                Text = "⭐ Cơ cấu doanh thu & Lợi nhuận theo món ăn (Top bán chạy)",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                UseMnemonic = false
            };
            pnlGridCard.Controls.Add(lblGridTitle);

            _dgvTopMon = new DataGridView {
                Location = new Point(16, 42),
                Size = new Size(pnlGridCard.Width - 32, 202),
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
            _dgvTopMon.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _dgvTopMon.ColumnHeadersHeight = 36;
            _dgvTopMon.RowTemplate.Height = 32;

            _dgvTopMon.Columns.Add("TenMon", "Tên món ăn");
            _dgvTopMon.Columns.Add("SoLuong", "Số lượng bán");
            _dgvTopMon.Columns.Add("DoanhThu", "Doanh thu (VNĐ)");
            _dgvTopMon.Columns.Add("GiaVon", "Giá vốn (VNĐ)");
            _dgvTopMon.Columns.Add("LoiNhuan", "Lợi nhuận gộp (VNĐ)");
            _dgvTopMon.Columns.Add("TyLeLai", "Biên lãi (%)");

            _dgvTopMon.Columns["SoLuong"].FillWeight = 50;
            _dgvTopMon.Columns["DoanhThu"].FillWeight = 80;
            _dgvTopMon.Columns["GiaVon"].FillWeight = 80;
            _dgvTopMon.Columns["LoiNhuan"].FillWeight = 80;
            _dgvTopMon.Columns["TyLeLai"].FillWeight = 55;

            pnlGridCard.Controls.Add(_dgvTopMon);
            scrollPanel.Controls.Add(pnlGridCard);
            y += 275;

            // 6. Dòng trạng thái cuối trang
            _lblStatus = new Label {
                Text = "Dữ liệu được cập nhật từ hệ thống quản lý bán hàng RestaurantCRM.",
                Location = new Point(22, y),
                AutoSize = true,
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                UseMnemonic = false
            };
            scrollPanel.Controls.Add(_lblStatus);
        }

        private void OnQuickFilterChanged(object sender, EventArgs e) {
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
            // Cập nhật các thẻ KPI
            _cardDoanhThu.SetValue($"{d.TongDoanhThu:N0} đ", $"{d.DonHoanThanh} đơn hoàn tất");
            _cardLoiNhuan.SetValue($"{d.LoiNhuanGop:N0} đ", $"Biên lãi {d.TyLeLoiNhuan:N1}%");
            _cardGiaVon.SetValue($"{d.TongGiaVon:N0} đ", "Giá vốn COGS");
            _cardGiamGia.SetValue($"{d.TongGiamGia:N0} đ", $"Giảm món & Voucher");
            _cardDonHang.SetValue($"{d.DonHoanThanh}", $"Đơn TB: {d.GiaTriDonTb:N0} đ");

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

                    // Tô màu nổi bật cho biên lợi nhuận
                    var cellLai = _dgvTopMon.Rows[rowIdx].Cells["TyLeLai"];
                    if (m.TyLeLai >= 30.0) {
                        cellLai.Style.ForeColor = ThemeManager.Green;
                        cellLai.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    } else if (m.TyLeLai < 20.0) {
                        cellLai.Style.ForeColor = ThemeManager.Red;
                        cellLai.Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
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

            int bottomMargin = 30;
            int topMargin = 30;
            int chartHeight = h - bottomMargin - topMargin;

            double maxVal = items.Max(x => x.DoanhThu);
            if (maxVal <= 0) maxVal = 100000;

            // Vẽ đường trục ngang cơ sở
            using (var linePen = new Pen(ThemeManager.Line, 1f)) {
                g.DrawLine(linePen, 10, h - bottomMargin, w - 10, h - bottomMargin);
            }

            int availableWidth = w - 40;
            int slotWidth = Math.Max(20, availableWidth / count);
            int barWidth = Math.Min(48, Math.Max(12, slotWidth - 14));

            using (var fontDate = new Font("Segoe UI", 8.5F, FontStyle.Regular))
            using (var fontValue = new Font("Segoe UI", 8F, FontStyle.Bold)) {
                for (int i = 0; i < count; i++) {
                    var item = items[i];
                    int slotX = 20 + i * slotWidth;
                    int barX = slotX + (slotWidth - barWidth) / 2;

                    int barHeight = (int)((item.DoanhThu / maxVal) * chartHeight);
                    if (barHeight < 4 && item.DoanhThu > 0) barHeight = 4;
                    int barY = h - bottomMargin - barHeight;

                    // Vẽ cột
                    var barRect = new Rectangle(barX, barY, barWidth, barHeight);
                    if (barHeight > 0) {
                        var barColor = item.DoanhThu > 0 ? Color.FromArgb(14, 124, 134) : ThemeManager.Line;
                        using (var brush = new SolidBrush(barColor)) {
                            g.FillRectangle(brush, barRect);
                        }
                    }

                    // Nhãn số tiền trên đầu cột
                    if (item.DoanhThu > 0) {
                        string valStr = FormatCompactMoney(item.DoanhThu);
                        var valRect = new Rectangle(slotX - 10, Math.Max(2, barY - 20), slotWidth + 20, 18);
                        TextRenderer.DrawText(g, valStr, fontValue, valRect, ThemeManager.Ink2,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                    }

                    // Nhãn ngày / thứ dưới chân cột
                    string dateStr = count <= 14 ? $"{item.Thu}\n{item.NgayHienThi}" : item.NgayHienThi;
                    var dateRect = new Rectangle(slotX - 10, h - bottomMargin + 4, slotWidth + 20, 24);
                    TextRenderer.DrawText(g, dateStr, fontDate, dateRect, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                }
            }
        }

        private string FormatCompactMoney(double amount) {
            if (amount >= 1000000) {
                return (amount / 1000000.0).ToString("0.#") + "M";
            }
            if (amount >= 1000) {
                return (amount / 1000.0).ToString("0.#") + "k";
            }
            return amount.ToString("N0");
        }
    }
}
