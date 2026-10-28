using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;
using RestaurantCRM.AdminApp.Services.KhuyenMai;

namespace RestaurantCRM.AdminApp.Forms.KhuyenMai {
    public class TaoVoucherDialog : Form {
        private readonly KhuyenMaiService _service;
        private List<ChuongTrinhDto> _chuongTrinhList = new List<ChuongTrinhDto>();
        private List<MonAnSelectDto> _dishList = new List<MonAnSelectDto>();
        private List<LoaiMonSelectDto> _loaiMonList = new List<LoaiMonSelectDto>();

        // Controls
        private ComboBox _cboChuongTrinh;
        private RadioButton _rbToanDon;
        private RadioButton _rbTheoLoai;
        private RadioButton _rbTheoMon;

        private TextBox _txtMaVoucher;

        // Container chọn đối tượng (Loại món / Món ăn)
        private Panel _pnlTargetContainer;
        private Label _lblTargetSelect;
        private Panel _pnlCboLoaiMon;
        private ComboBox _cboLoaiMon;
        private Panel _pnlCboMonAn;
        private ComboBox _cboMonAn;

        // Điều kiện áp dụng
        private Label _lblDonToiThieu;
        private Label _lblDonToiThieuHint;
        private NumericUpDown _nudDonToiThieu;
        private Label _lblSoLuongVoucher;
        private Label _lblSoLuongVoucherHint;
        private NumericUpDown _nudSoLuongVoucher;

        // Mức giảm giá
        private ComboBox _cboLoaiGiam;
        private NumericUpDown _nudGiaTriGiam;
        private Label _lblGiamToiDa;
        private NumericUpDown _nudGiamToiDa;

        // Financial Analysis Card (dành cho Theo Món)
        private Panel _pnlFinanceCard;
        private Label _lblGiaGocVal;
        private Label _lblGiaVonVal;
        private Label _lblGiaSauGiamVal;
        private Label _lblTietKiemVal;
        private Label _lblLaiGopVal;
        private Panel _pnlWarningBanner;
        private Label _lblWarningText;

        private readonly VoucherDto _voucherToEdit;
        private Button _btnSave;
        private Button _btnCancel;

        public TaoVoucherDialog(KhuyenMaiService service, string preselectedCtId = null, bool defaultTheoMon = false)
            : this(service, preselectedCtId, defaultTheoMon ? "THEO_MON" : "TOAN_DON", null) { }

        public TaoVoucherDialog(KhuyenMaiService service, string preselectedCtId, string defaultScope, VoucherDto voucherToEdit = null) {
            _service = service;
            _voucherToEdit = voucherToEdit;

            Text = _voucherToEdit != null ? $"Chỉnh sửa Voucher [{_voucherToEdit.MaVoucher}]" : "Thiết lập voucher / ưu đãi khuyến mãi";
            ClientSize = new Size(1000, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Paper;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitControls();

            string scope = _voucherToEdit != null ? _voucherToEdit.PhamViApDung : defaultScope;
            if (scope == "THEO_MON") {
                _rbTheoMon.Checked = true;
            } else if (scope == "THEO_LOAI") {
                _rbTheoLoai.Checked = true;
            } else {
                _rbToanDon.Checked = true;
            }

            if (_voucherToEdit != null) {
                // Khóa đổi loại phạm vi khi sửa để bảo toàn tính toàn vẹn dữ liệu
                _rbToanDon.Enabled = false;
                _rbTheoLoai.Enabled = false;
                _rbTheoMon.Enabled = false;
                _txtMaVoucher.Text = _voucherToEdit.MaVoucher;

                if (_voucherToEdit.PhanTramGiam.HasValue && _voucherToEdit.PhanTramGiam > 0) {
                    _cboLoaiGiam.SelectedIndex = 0;
                    _nudGiaTriGiam.Value = (decimal)_voucherToEdit.PhanTramGiam.Value;
                } else if (_voucherToEdit.TienGiam.HasValue && _voucherToEdit.TienGiam > 0) {
                    _cboLoaiGiam.SelectedIndex = 1;
                    _nudGiaTriGiam.Value = (decimal)_voucherToEdit.TienGiam.Value;
                }

                if (_voucherToEdit.GiamToiDa.HasValue) _nudGiamToiDa.Value = (decimal)_voucherToEdit.GiamToiDa.Value;
                if (_voucherToEdit.GiaTriDonToiThieu.HasValue) _nudDonToiThieu.Value = (decimal)_voucherToEdit.GiaTriDonToiThieu.Value;
                if (_voucherToEdit.SoLuongToiThieu.HasValue) _nudSoLuongVoucher.Value = Math.Max(0, _voucherToEdit.SoLuongToiThieu.Value);
            }

            ToggleScope();

            string ctId = _voucherToEdit != null ? _voucherToEdit.MaChuongTrinh : preselectedCtId;
            LoadDataAsync(ctId);
        }

        // Bố cục mới
        private Panel _grpMin;
        private Panel _grpQty;
        private TableLayoutPanel _gridCond;
        private Label _lblScopeHint;
        private Label _lblHeaderTitle;

        private static Label MakeLabel(string text, bool bold = true) {
            return new Label {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = ThemeManager.Ink,
                Margin = new Padding(0, 0, 0, 4),
                UseMnemonic = false
            };
        }

        private static Label MakeHint(string text, int maxWidth = 400) {
            return new Label {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(maxWidth, 0),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = ThemeManager.Muted,
                Margin = new Padding(0, 4, 0, 0),
                UseMnemonic = false
            };
        }

        /// <summary>Một ô nhập: nhãn + control + gợi ý, xếp dọc, không chồng lấn.</summary>
        private static TableLayoutPanel MakeField(Label lbl, Control input, Label hint) {
            var t = new TableLayoutPanel {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 6)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            input.Dock = DockStyle.Top;
            input.Margin = new Padding(0);
            t.Controls.Add(lbl);
            t.Controls.Add(input);
            if (hint != null) t.Controls.Add(hint);
            return t;
        }

        private static TableLayoutPanel MakeGrid(params float[] percents) {
            var g = new TableLayoutPanel {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = percents.Length,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            foreach (var p in percents) g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, p));
            return g;
        }

        private static TableLayoutPanel NewCard(string title, Control content) {
            var card = new TableLayoutPanel {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14, 10, 14, 4),
                Margin = new Padding(0, 0, 0, 10)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.Controls.Add(new Label {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Margin = new Padding(0, 0, 0, 6),
                UseMnemonic = false
            });
            content.Dock = DockStyle.Fill;
            card.Controls.Add(content);
            card.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };
            return card;
        }

        private RadioButton MakeSegment(string text) {
            var rb = new RadioButton {
                Text = text,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Height = 34,
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                ForeColor = ThemeManager.Ink
            };
            rb.FlatAppearance.BorderColor = ThemeManager.Line;
            rb.FlatAppearance.CheckedBackColor = ThemeManager.Blue;
            rb.CheckedChanged += (s, e) => RefreshSegments();
            return rb;
        }

        private void RefreshSegments() {
            foreach (var rb in new[] { _rbToanDon, _rbTheoLoai, _rbTheoMon }) {
                if (rb == null) continue;
                rb.ForeColor = rb.Checked ? Color.White : ThemeManager.Ink;
                rb.BackColor = rb.Checked ? ThemeManager.Blue : Color.White;
            }
        }

        private static ComboBox MakeCombo() {
            return new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard,
                Tag = UiStyler.NoStyleTag,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                IntegralHeight = false,
                DropDownHeight = 260
            };
        }

        private static NumericUpDown MakeNumeric() {
            return new NumericUpDown {
                ThousandsSeparator = true,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = UiStyler.NoStyleTag,
                TextAlign = HorizontalAlignment.Left,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
        }

        private void InitControls() {
            // Vùng nội dung cuộn được (thêm đầu tiên để Fill chiếm phần còn lại)
            var body = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(16, 12, 16, 0)
            };
            Controls.Add(body);

            // Bố cục 2 cột: trái (1-3), phải (4-6) để vừa gọn một khung
            var stack = new TableLayoutPanel {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                BackColor = ThemeManager.Paper
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            var left = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1,
                Dock = DockStyle.Top, Margin = new Padding(0, 0, 8, 0), BackColor = ThemeManager.Paper
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var right = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1,
                Dock = DockStyle.Top, Margin = new Padding(8, 0, 0, 0), BackColor = ThemeManager.Paper
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            stack.Controls.Add(left, 0, 0);
            stack.Controls.Add(right, 1, 0);
            body.Controls.Add(stack);

            // ===== Thẻ 1: Thông tin chung =====
            _cboChuongTrinh = MakeCombo();
            _txtMaVoucher = new TextBox {
                CharacterCasing = CharacterCasing.Upper,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = UiStyler.NoStyleTag,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            var infoContent = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill
            };
            infoContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            infoContent.Controls.Add(MakeField(MakeLabel("Chương trình khuyến mãi (*)"), _cboChuongTrinh,
                MakeHint("Chỉ hiển thị các chương trình đang áp dụng hoặc chưa diễn ra")));
            infoContent.Controls.Add(MakeField(MakeLabel("Mã voucher (*)"), _txtMaVoucher,
                MakeHint("Ví dụ: HE2026, LAU20, BO15K")));
            left.Controls.Add(NewCard("1. Thông tin chung", infoContent));

            // ===== Thẻ 2: Phạm vi áp dụng =====
            _rbToanDon = MakeSegment("Toàn hóa đơn");
            _rbTheoLoai = MakeSegment("Theo loại món");
            _rbTheoMon = MakeSegment("Theo từng món ăn");
            _rbToanDon.Checked = true;
            _rbToanDon.CheckedChanged += (s, e) => ToggleScope();
            _rbTheoLoai.CheckedChanged += (s, e) => ToggleScope();
            _rbTheoMon.CheckedChanged += (s, e) => ToggleScope();

            var segGrid = MakeGrid(33.33f, 33.33f, 33.34f);
            segGrid.Controls.Add(_rbToanDon, 0, 0);
            segGrid.Controls.Add(_rbTheoLoai, 1, 0);
            segGrid.Controls.Add(_rbTheoMon, 2, 0);
            _rbTheoMon.Margin = new Padding(0);

            _lblScopeHint = new Label {
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeManager.Muted,
                Margin = new Padding(0, 6, 0, 4),
                UseMnemonic = false
            };
            var scopeContent = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill
            };
            scopeContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            scopeContent.Controls.Add(segGrid);
            scopeContent.Controls.Add(_lblScopeHint);
            left.Controls.Add(NewCard("2. Phạm vi áp dụng", scopeContent));

            // ===== Thẻ 3: Đối tượng áp dụng (loại món / món ăn) =====
            _lblTargetSelect = MakeLabel("Chọn đối tượng áp dụng (*)");
            _cboLoaiMon = MakeCombo();
            _cboMonAn = MakeCombo();
            _cboMonAn.SelectedIndexChanged += (s, e) => UpdatePreviewPrice();

            _pnlCboLoaiMon = CreateComboWrapper(_cboLoaiMon);
            _pnlCboMonAn = CreateComboWrapper(_cboMonAn);
            var targetHost = new Panel { Height = 28, Dock = DockStyle.Top, Tag = UiStyler.NoStyleTag };
            targetHost.Controls.Add(_pnlCboLoaiMon);
            targetHost.Controls.Add(_pnlCboMonAn);

            var targetContent = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill
            };
            targetContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var targetField = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1,
                Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6)
            };
            targetField.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            targetField.Controls.Add(_lblTargetSelect);
            targetField.Controls.Add(targetHost);
            targetContent.Controls.Add(targetField);
            _pnlTargetContainer = NewCard("3. Đối tượng áp dụng", targetContent);
            _pnlTargetContainer.Visible = false;
            left.Controls.Add(_pnlTargetContainer);

            // ===== Thẻ 4: Điều kiện áp dụng =====
            _lblDonToiThieu = MakeLabel("Giá tối thiểu được giảm (VNĐ)");
            _lblDonToiThieuHint = MakeHint("Chỉ món có đơn giá ≥ mức này mới được giảm (0đ = tất cả món)");
            _nudDonToiThieu = MakeNumeric();
            _nudDonToiThieu.Maximum = 100000000;
            _nudDonToiThieu.Increment = 10000;
            _nudDonToiThieu.Value = 0;

            _lblSoLuongVoucher = MakeLabel("Số lượng voucher áp dụng");
            _lblSoLuongVoucherHint = MakeHint("Tổng lượt khách được dùng mã (0 = không giới hạn)");
            _nudSoLuongVoucher = MakeNumeric();
            _nudSoLuongVoucher.Minimum = 0;
            _nudSoLuongVoucher.Maximum = 100000;
            _nudSoLuongVoucher.Increment = 5;
            _nudSoLuongVoucher.Value = 20;

            _gridCond = MakeGrid(50f, 50f);
            _lblDonToiThieuHint.MaximumSize = new Size(190, 0);
            _lblSoLuongVoucherHint.MaximumSize = new Size(190, 0);
            _grpMin = MakeField(_lblDonToiThieu, _nudDonToiThieu, _lblDonToiThieuHint);
            _grpQty = MakeField(_lblSoLuongVoucher, _nudSoLuongVoucher, _lblSoLuongVoucherHint);
            _gridCond.Controls.Add(_grpMin, 0, 0);
            _gridCond.Controls.Add(_grpQty, 1, 0);
            right.Controls.Add(NewCard("4. Điều kiện áp dụng", _gridCond));

            // ===== Thẻ 5: Mức giảm giá =====
            _cboLoaiGiam = MakeCombo();
            _cboLoaiGiam.Items.AddRange(new object[] { "% (Phần trăm)", "VND (Tiền mặt)" });
            _cboLoaiGiam.SelectedIndex = 0;

            _nudGiaTriGiam = MakeNumeric();
            _nudGiaTriGiam.Maximum = 100;
            _nudGiaTriGiam.Value = 10;
            _nudGiaTriGiam.ValueChanged += (s, e) => UpdatePreviewPrice();

            _cboLoaiGiam.SelectedIndexChanged += (s, e) => {
                bool isPercent = _cboLoaiGiam.SelectedIndex == 0;
                _nudGiaTriGiam.Maximum = isPercent ? 100 : 10000000;
                UpdatePreviewPrice();
            };

            _lblGiamToiDa = MakeLabel("Mức giảm tối đa (VNĐ)");
            _nudGiamToiDa = MakeNumeric();
            _nudGiamToiDa.Maximum = 100000000;
            _nudGiamToiDa.Increment = 10000;
            _nudGiamToiDa.Value = 50000;

            _lblGiamToiDa.Text = "Giảm tối đa (VNĐ)";
            var gridDiscount = MakeGrid(34f, 26f, 40f);
            gridDiscount.Controls.Add(MakeField(MakeLabel("Hình thức giảm"), _cboLoaiGiam, null), 0, 0);
            gridDiscount.Controls.Add(MakeField(MakeLabel("Mức giảm (*)"), _nudGiaTriGiam, null), 1, 0);
            gridDiscount.Controls.Add(MakeField(_lblGiamToiDa, _nudGiamToiDa, null), 2, 0);
            right.Controls.Add(NewCard("5. Mức giảm giá", gridDiscount));

            // ===== Thẻ 6: Phân tích tài chính (chỉ khi Theo món) =====
            _lblGiaGocVal = MakeValue("0 đ", FontStyle.Bold, ThemeManager.Ink);
            _lblGiaSauGiamVal = MakeValue("0 đ", FontStyle.Bold, ThemeManager.PrimaryDark);
            _lblGiaVonVal = MakeValue("0 đ", FontStyle.Bold, ThemeManager.Ink2);
            _lblTietKiemVal = MakeValue("0 đ", FontStyle.Regular, ThemeManager.Muted);
            _lblLaiGopVal = MakeValue("0 đ", FontStyle.Bold, ThemeManager.Green);

            var fin = new TableLayoutPanel {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, Dock = DockStyle.Fill
            };
            fin.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fin.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            // Mỗi dòng một cặp nhãn - giá trị: thẳng hàng, không tràn khung
            AddFinRow(fin, 0, "Giá bán niêm yết:", _lblGiaGocVal, "Giá sau ưu đãi:", _lblGiaSauGiamVal);
            AddFinRow(fin, 1, "Mức giảm / đĩa:", _lblTietKiemVal, "Lãi gộp dự kiến:", _lblLaiGopVal);
            fin.Controls.Add(new Label {
                Text = "Giá vốn (Food Cost):", AutoSize = true, ForeColor = ThemeManager.Muted,
                Margin = new Padding(0, 4, 10, 4), UseMnemonic = false
            }, 0, 2);
            fin.Controls.Add(_lblGiaVonVal, 1, 2);
            fin.SetColumnSpan(_lblGiaVonVal, 3);

            _pnlWarningBanner = new Panel {
                Dock = DockStyle.Fill,
                Height = 54,
                BackColor = ThemeManager.GreenSoft,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 6, 0, 6)
            };
            _lblWarningText = new Label {
                Text = "Đang phân tích dữ liệu món ăn...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.Green,
                UseMnemonic = false
            };
            _pnlWarningBanner.Controls.Add(_lblWarningText);
            fin.Controls.Add(_pnlWarningBanner, 0, 3);
            fin.SetColumnSpan(_pnlWarningBanner, 4);

            _pnlFinanceCard = NewCard("6. Phân tích tài chính & biên lợi nhuận", fin);
            _pnlFinanceCard.Visible = false;
            right.Controls.Add(_pnlFinanceCard);

            // ===== Header (gắn sau body) =====
            var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = ThemeManager.Blue };
            _lblHeaderTitle = new Label {
                Text = _voucherToEdit != null
                    ? $"Chỉnh sửa voucher [{_voucherToEdit.MaVoucher}]"
                    : "Thiết lập voucher / ưu đãi",
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 12),
                UseMnemonic = false
            };
            var lblHeaderSub = new Label {
                Text = "Điền lần lượt các mục bên dưới, sau đó nhấn Lưu",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(225, 232, 255),
                Location = new Point(26, 44),
                UseMnemonic = false
            };
            header.Controls.Add(_lblHeaderTitle);
            header.Controls.Add(lblHeaderSub);

            // ===== Footer =====
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            footer.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1f)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };
            var flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(16, 9, 16, 9),
                BackColor = Color.White
            };

            _btnSave = new Button {
                Text = "Lưu voucher",
                Size = new Size(170, 40),
                BackColor = ThemeManager.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveAsync();

            _btnCancel = new Button {
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(240, 243, 248),
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            flow.Controls.Add(_btnSave);
            flow.Controls.Add(_btnCancel);
            footer.Controls.Add(flow);

            Controls.Add(footer);
            Controls.Add(header);
            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
            RefreshSegments();
        }

        private static Label MakeValue(string text, FontStyle style, Color color) {
            return new Label {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, style),
                ForeColor = color,
                Margin = new Padding(0, 4, 16, 4),
                UseMnemonic = false
            };
        }

        private static void AddFinRow(TableLayoutPanel t, int row, string t1, Label v1, string t2, Label v2) {
            t.Controls.Add(new Label {
                Text = t1, AutoSize = true, ForeColor = ThemeManager.Muted,
                Margin = new Padding(0, 4, 10, 4), UseMnemonic = false
            }, 0, row);
            t.Controls.Add(v1, 1, row);
            if (t2 != null) {
                t.Controls.Add(new Label {
                    Text = t2, AutoSize = true, ForeColor = ThemeManager.Muted,
                    Margin = new Padding(0, 4, 10, 4), UseMnemonic = false
                }, 2, row);
                t.Controls.Add(v2, 3, row);
            }
        }

        private static Panel CreateComboWrapper(ComboBox cbo) {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Tag = UiStyler.NoStyleTag };
            cbo.Dock = DockStyle.Top;
            pnl.Controls.Add(cbo);
            return pnl;
        }

        private void ToggleScope() {
            if (_gridCond == null) return;

            if (_rbToanDon.Checked) {
                _lblScopeHint.Text = "Giảm trực tiếp trên tổng hóa đơn khi đạt giá trị đơn tối thiểu.";
                _pnlTargetContainer.Visible = false;
                _pnlFinanceCard.Visible = false;
                SetCondColumns(true);
                _lblDonToiThieu.Text = "Đơn hàng tối thiểu (VNĐ)";
                _lblDonToiThieuHint.Text = "Áp dụng khi tổng hóa đơn đạt từ mức này trở lên (0đ = mọi đơn)";
                _lblSoLuongVoucherHint.Text = "Tổng lượt khách được dùng mã (0 = không giới hạn)";
            } else if (_rbTheoLoai.Checked) {
                _lblScopeHint.Text = "Giảm cho các món thuộc một loại món (ví dụ: Lẩu, Nước uống).";
                _pnlTargetContainer.Visible = true;
                _lblTargetSelect.Text = "Chọn loại món áp dụng (*)";
                _pnlCboLoaiMon.Visible = true;
                _pnlCboMonAn.Visible = false;
                _pnlFinanceCard.Visible = false;
                SetCondColumns(true);
                _lblDonToiThieu.Text = "Giá tối thiểu được giảm (VNĐ)";
                _lblDonToiThieuHint.Text = "Chỉ món trong nhóm có đơn giá ≥ mức này mới được giảm (0đ = tất cả món)";
                _lblSoLuongVoucherHint.Text = "Ví dụ: 20 lượt. Khi đủ số lượt dùng mã sẽ tự động ẩn";
            } else if (_rbTheoMon.Checked) {
                _lblScopeHint.Text = "Giảm cho một món ăn cụ thể, có phân tích biên lợi nhuận bên dưới.";
                _pnlTargetContainer.Visible = true;
                _lblTargetSelect.Text = "Chọn món ăn áp dụng (*)";
                _pnlCboLoaiMon.Visible = false;
                _pnlCboMonAn.Visible = true;
                _pnlFinanceCard.Visible = true;
                SetCondColumns(false);
                _lblSoLuongVoucherHint.Text = "Tổng số lượt ưu đãi cho món này (0 = không giới hạn lượt dùng)";
                UpdatePreviewPrice();
            }
        }

        private void SetCondColumns(bool showMin) {
            _grpMin.Visible = showMin;
            if (showMin) {
                _gridCond.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50);
                _gridCond.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50);
            } else {
                _gridCond.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 0);
                _gridCond.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100);
            }
        }

        private void UpdatePreviewPrice() {
            if (!_rbTheoMon.Checked) return;

            var selectedDish = _cboMonAn.SelectedItem as MonAnSelectDto;
            if (selectedDish == null || !selectedDish.DonGia.HasValue) {
                var empty = VoucherMarginResult.CreateEmpty();
                _lblGiaGocVal.Text = "0 đ";
                _lblGiaVonVal.Text = "0 đ";
                _lblGiaSauGiamVal.Text = "0 đ";
                _lblTietKiemVal.Text = "0 đ";
                _lblLaiGopVal.Text = empty.LaiGopDisplayText;
                _lblWarningText.Text = empty.WarningText;
                return;
            }

            double goc = selectedDish.DonGia.Value;
            double giaVon = selectedDish.GiaVonUocTinh ?? 0;
            double giam = (double)_nudGiaTriGiam.Value;
            int loaiGiam = _cboLoaiGiam.SelectedIndex;

            var result = VoucherMarginCalculator.Calculate(goc, giaVon, loaiGiam, giam);

            _lblGiaGocVal.Text = $"{result.DonGiaGoc:N0} đ";
            _lblGiaSauGiamVal.Text = $"{result.GiaSauGiam:N0} đ";
            _lblTietKiemVal.Text = $"-{result.TietKiem:N0} đ";

            if (result.GiaVon > 0) {
                _lblGiaVonVal.Text = $"{result.GiaVon:N0} đ ({result.TyLeFoodCost:N1}% giá gốc)";
            } else {
                _lblGiaVonVal.Text = "Chưa có định lượng";
            }

            _lblLaiGopVal.Text = result.LaiGopDisplayText;
            _lblWarningText.Text = result.WarningText;

            switch (result.Health) {
                case MarginHealth.NoRecipe:
                    _lblLaiGopVal.ForeColor = ThemeManager.Muted;
                    _pnlWarningBanner.BackColor = ThemeManager.AmberSoft;
                    _lblWarningText.ForeColor = ThemeManager.Amber;
                    _lblWarningText.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                    break;
                case MarginHealth.NegativeMargin:
                    _lblLaiGopVal.ForeColor = ThemeManager.Red;
                    _pnlWarningBanner.BackColor = ThemeManager.RedSoft;
                    _lblWarningText.ForeColor = ThemeManager.Red;
                    _lblWarningText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    break;
                case MarginHealth.ThinMargin:
                    _lblLaiGopVal.ForeColor = ThemeManager.Amber;
                    _pnlWarningBanner.BackColor = ThemeManager.AmberSoft;
                    _lblWarningText.ForeColor = ThemeManager.Amber;
                    _lblWarningText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    break;
                case MarginHealth.SafeMargin:
                    _lblLaiGopVal.ForeColor = ThemeManager.Green;
                    _pnlWarningBanner.BackColor = ThemeManager.GreenSoft;
                    _lblWarningText.ForeColor = ThemeManager.Green;
                    _lblWarningText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    break;
            }
        }

        private async void LoadDataAsync(string preselectedCtId) {
            try {
                _chuongTrinhList = await _service.GetChuongTrinhListAsync();
                _dishList = await _service.GetAvailableDishesAsync();
                _loaiMonList = await _service.GetLoaiMonListAsync();

                // Bind Chương trình (Chỉ lấy các chương trình đang hoạt động hoặc chưa diễn ra)
                _cboChuongTrinh.Items.Clear();
                foreach (var ct in _chuongTrinhList) {
                    bool isEnded = ct.TrangThai == "Đã kết thúc";
                    // Nếu đã kết thúc và không phải là chương trình được chọn từ trước thì ẩn đi
                    if (isEnded && ct.MaChuongTrinh != preselectedCtId) {
                        continue;
                    }

                    _cboChuongTrinh.Items.Add(new ComboBoxItem {
                        Value = ct.MaChuongTrinh,
                        Text = isEnded ? $"[ĐÃ HẾT HẠN] [{ct.MaChuongTrinh}] {ct.TenChuongTrinh}" : $"[{ct.MaChuongTrinh}] {ct.TenChuongTrinh} ({ct.TrangThai})"
                    });
                }

                if (_cboChuongTrinh.Items.Count > 0) {
                    int selectIdx = 0;
                    if (!string.IsNullOrEmpty(preselectedCtId)) {
                        for (int i = 0; i < _cboChuongTrinh.Items.Count; i++) {
                            if (((ComboBoxItem)_cboChuongTrinh.Items[i]).Value == preselectedCtId) {
                                selectIdx = i;
                                break;
                            }
                        }
                    }
                    _cboChuongTrinh.SelectedIndex = selectIdx;
                } else {
                    _cboChuongTrinh.Items.Add(new ComboBoxItem {
                        Value = "",
                        Text = "-- Không có chương trình khuyến mãi nào đang diễn ra --"
                    });
                    _cboChuongTrinh.SelectedIndex = 0;
                }

                // Bind Loại món
                _cboLoaiMon.DisplayMember = "DisplayText";
                _cboLoaiMon.ValueMember = "MaLoaiMon";
                _cboLoaiMon.DataSource = _loaiMonList;
                if (_loaiMonList != null && _loaiMonList.Count > 0) {
                    _cboLoaiMon.SelectedIndex = 0;
                }

                // Bind Món ăn
                _cboMonAn.DisplayMember = "DisplayText";
                _cboMonAn.ValueMember = "MaMon";
                _cboMonAn.DataSource = _dishList;
                if (_dishList != null && _dishList.Count > 0) {
                    _cboMonAn.SelectedIndex = 0;
                    UpdatePreviewPrice();
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "Lỗi khi tải dữ liệu khởi tạo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveAsync() {
            var selectedCt = _cboChuongTrinh.SelectedItem as ComboBoxItem;
            if (selectedCt == null || string.IsNullOrEmpty(selectedCt.Value)) {
                MessageBox.Show(this, "Vui lòng chọn một chương trình khuyến mãi đang hoạt động để tạo voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ctObj = _chuongTrinhList.FirstOrDefault(x => x.MaChuongTrinh == selectedCt.Value);
            if (ctObj != null && ctObj.TrangThai == "Đã kết thúc") {
                MessageBox.Show(this, "Chương trình khuyến mãi này đã kết thúc. Bạn không thể tạo thêm voucher cho chương trình đã hết hạn.", "Chương trình đã kết thúc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var maVoucher = _txtMaVoucher.Text.Trim();
            if (string.IsNullOrEmpty(maVoucher)) {
                MessageBox.Show(this, "Vui lòng nhập mã Voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMaVoucher.Focus();
                return;
            }

            string loaiGiam = _cboLoaiGiam.SelectedIndex == 0 ? "%" : "VND";
            double giaTriGiam = (double)_nudGiaTriGiam.Value;
            double? giamToiDa = _nudGiamToiDa.Value > 0 ? (double?)_nudGiamToiDa.Value : null;

            string phamVi = "TOAN_DON";
            string maLoaiMon = null;
            string maMon = null;
            double? donToiThieu = null;
            int soLuongVoucher = (int)_nudSoLuongVoucher.Value;

            if (_rbTheoLoai.Checked) {
                phamVi = "THEO_LOAI";
                var selectedLoai = _cboLoaiMon.SelectedItem as LoaiMonSelectDto;
                if (selectedLoai == null) {
                    MessageBox.Show(this, "Vui lòng chọn loại món áp dụng cho voucher nhóm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                maLoaiMon = selectedLoai.MaLoaiMon;
                donToiThieu = _nudDonToiThieu.Value > 0 ? (double?)_nudDonToiThieu.Value : null;
            } else if (_rbTheoMon.Checked) {
                phamVi = "THEO_MON";
                var dish = _cboMonAn.SelectedItem as MonAnSelectDto;
                if (dish == null) {
                    MessageBox.Show(this, "Vui lòng chọn món ăn cần áp dụng ưu đãi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                maMon = dish.MaMon;

                // Kiểm tra an toàn tài chính chống bán dưới giá vốn Food Cost
                if (dish.DonGia.HasValue) {
                    double goc = dish.DonGia.Value;
                    double giaVon = dish.GiaVonUocTinh ?? 0;
                    double sauGiam = loaiGiam == "%" ? goc * (1 - giaTriGiam / 100.0) : Math.Max(0, goc - giaTriGiam);

                    if (giaVon > 0 && sauGiam < giaVon) {
                        double tienLo = giaVon - sauGiam;
                        MessageBox.Show(this,
                            "NGĂN CHẶN THUA LỖ:\n\n" +
                            $"Giá sau ưu đãi ({sauGiam:N0} đ) THẤP HƠN Giá vốn nguyên liệu ({giaVon:N0} đ).\n" +
                            $"Nhà hàng sẽ chịu lỗ trực tiếp {tienLo:N0} đ trên mỗi đĩa bán ra!\n\n" +
                            "Vui lòng điều chỉnh lại mức giảm giá hoặc thiết lập mức giảm tối đa.",
                            "Cảnh báo thua lỗ nghiêm trọng", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    if (giaVon > 0) {
                        double laiGop = sauGiam - giaVon;
                        double tyLeLai = sauGiam > 0 ? (laiGop / sauGiam) * 100.0 : 0;
                        if (tyLeLai < 20.0) {
                            var confirm = MessageBox.Show(this,
                                "CẢNH BÁO BIÊN LỢI NHUẬN THẤP:\n\n" +
                                $"Biên lợi nhuận gộp của món này sau giảm chỉ đạt {tyLeLai:N1}% ({laiGop:N0} đ/đĩa).\n" +
                                "Mức lãi này có nguy cơ không đủ bù đắp chi phí mặt bằng & nhân sự.\n\n" +
                                "Bạn có chắc chắn vẫn muốn tạo voucher này không?",
                                "Xác nhận rủi ro tài chính", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (confirm != DialogResult.Yes) return;
                        }
                    }
                }
            } else {
                // TOAN_DON
                phamVi = "TOAN_DON";
                donToiThieu = _nudDonToiThieu.Value > 0 ? (double?)_nudDonToiThieu.Value : null;
            }

            _btnSave.Enabled = false;
            try {
                bool ok;
                string msg;

                if (_voucherToEdit != null) {
                    (ok, msg) = await _service.UpdateVoucherAsync(
                        _voucherToEdit.MaKmvoucher,
                        selectedCt.Value,
                        maVoucher,
                        loaiGiam,
                        giaTriGiam,
                        donToiThieu,
                        giamToiDa,
                        soLuongVoucher);
                } else {
                    (ok, msg) = await _service.CreateVoucherAsync(
                        selectedCt.Value,
                        maVoucher,
                        loaiGiam,
                        giaTriGiam,
                        donToiThieu,
                        giamToiDa,
                        phamVi,
                        maLoaiMon,
                        maMon,
                        soLuongVoucher,
                        true);
                }

                if (ok) {
                    string actionText = _voucherToEdit != null ? "Cập nhật" : "Tạo";
                    MessageBox.Show(this, $"{actionText} voucher ưu đãi '{maVoucher}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                } else {
                    MessageBox.Show(this, msg, "Lỗi lưu voucher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } finally {
                _btnSave.Enabled = true;
            }
        }

        private class ComboBoxItem {
            public string Value { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }
    }
}
