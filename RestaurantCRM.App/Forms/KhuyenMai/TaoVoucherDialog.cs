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

        private ComboBox _cboChuongTrinh;
        private RadioButton _rbToanDon;
        private RadioButton _rbTheoMon;

        // Panel Toàn Đơn
        private Panel _pnlToanDon;
        private TextBox _txtMaVoucher;
        private ComboBox _cboLoaiGiamDon;
        private NumericUpDown _nudGiaTriGiamDon;
        private NumericUpDown _nudDonToiThieu;
        private NumericUpDown _nudGiamToiDa;
        private Label _lblGiamToiDa;

        // Panel Theo Món
        private Panel _pnlTheoMon;
        private ComboBox _cboMonAn;
        private ComboBox _cboLoaiGiamMon;
        private NumericUpDown _nudGiaTriGiamMon;

        // Financial Analysis Card Controls
        private Panel _pnlFinanceCard;
        private Label _lblGiaGocVal;
        private Label _lblGiaVonVal;
        private Label _lblGiaSauGiamVal;
        private Label _lblTietKiemVal;
        private Label _lblLaiGopVal;
        private Panel _pnlWarningBanner;
        private Label _lblWarningText;

        private Button _btnSave;
        private Button _btnCancel;

        public TaoVoucherDialog(KhuyenMaiService service, string preselectedCtId = null, bool defaultTheoMon = false) {
            _service = service;

            Text = "Tạo Voucher / Ưu đãi khuyến mãi";
            Size = new Size(620, 720);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Paper;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitControls();

            if (defaultTheoMon) {
                _rbTheoMon.Checked = true;
                _rbToanDon.Checked = false;
                ToggleScope();
            }

            LoadDataAsync(preselectedCtId);
        }

        private void InitControls() {
            int left = 32;
            int top = 20;
            int width = 540;

            var lblHeader = new Label {
                Text = "THIẾT LẬP VOUCHER / ƯU ĐÃI KHUYẾN MÃI",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Location = new Point(left, top),
                AutoSize = true,
                UseMnemonic = false
            };
            Controls.Add(lblHeader);
            top += 40;

            // 1. Chọn chương trình khuyến mãi
            var lblCT = new Label { 
                Text = "1. Chọn chương trình khuyến mãi (*):", 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                ForeColor = ThemeManager.Ink, 
                Location = new Point(left, top), 
                AutoSize = true,
                UseMnemonic = false
            };
            Controls.Add(lblCT);
            top += 24;

            _cboChuongTrinh = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            var pnlCboCt = CreateBoldBorderContainer(_cboChuongTrinh, left, top, width, 34);
            Controls.Add(pnlCboCt);
            top += 44;

            // 2. Chọn hình thức áp dụng
            var lblLoai = new Label { 
                Text = "2. Chọn phạm vi áp dụng (*):", 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                ForeColor = ThemeManager.Ink, 
                Location = new Point(left, top), 
                AutoSize = true,
                UseMnemonic = false
            };
            Controls.Add(lblLoai);
            top += 24;

            var pnlType = new Panel { Location = new Point(left, top), Size = new Size(width, 36) };
            _rbToanDon = new RadioButton { 
                Text = "Toàn hóa đơn (Nhập mã Voucher)", 
                Location = new Point(0, 5), 
                AutoSize = true, 
                Checked = true, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) 
            };
            _rbTheoMon = new RadioButton { 
                Text = "Theo món ăn (Tự động giảm giá sản phẩm)", 
                Location = new Point(255, 5), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) 
            };
            
            _rbToanDon.CheckedChanged += (s, e) => ToggleScope();
            _rbTheoMon.CheckedChanged += (s, e) => ToggleScope();

            pnlType.Controls.Add(_rbToanDon);
            pnlType.Controls.Add(_rbTheoMon);
            Controls.Add(pnlType);
            top += 42;

            // Panel Toàn Đơn
            _pnlToanDon = new Panel { Location = new Point(left, top), Size = new Size(width, 380) };
            InitToanDonPanel(_pnlToanDon);
            Controls.Add(_pnlToanDon);

            // Panel Theo Món
            _pnlTheoMon = new Panel { Location = new Point(left, top), Size = new Size(width, 420), Visible = false };
            InitTheoMonPanel(_pnlTheoMon);
            Controls.Add(_pnlTheoMon);

            top += 430;

            // Buttons
            _btnSave = new Button {
                Text = "Tạo ưu đãi ngay",
                Location = new Point(left + 260, top),
                Size = new Size(160, 40),
                BackColor = ThemeManager.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveAsync();

            _btnCancel = new Button {
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                Location = new Point(left + 430, top),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(240, 243, 248),
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
        }

        private Panel CreateBoldBorderContainer(ComboBox cbo, int x, int y, int width, int height) {
            var pnl = new Panel {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White
            };

            cbo.Location = new Point(3, 4);
            cbo.Width = width - 6;
            cbo.FlatStyle = FlatStyle.Flat;

            pnl.Controls.Add(cbo);
            pnl.Paint += (s, e) => {
                // Viền đậm 2px màu Indigo (#4F46E5) sắc nét, hiện đại
                using (var pen = new Pen(Color.FromArgb(79, 70, 229), 2f)) {
                    e.Graphics.DrawRectangle(pen, 1, 1, pnl.Width - 3, pnl.Height - 3);
                }
            };

            return pnl;
        }

        private void InitToanDonPanel(Panel p) {
            int y = 0;
            // Mã Voucher
            p.Controls.Add(new Label { Text = "Mã Voucher (Ví dụ: VOUCHER40K, HE2026) (*):", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            y += 22;
            _txtMaVoucher = new TextBox { Location = new Point(0, y), Width = 540, CharacterCasing = CharacterCasing.Upper, Font = new Font("Segoe UI", 10F, FontStyle.Regular) };
            p.Controls.Add(_txtMaVoucher);
            y += 34;

            // Loại giảm + Mức giảm
            p.Controls.Add(new Label { Text = "Loại giảm:", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            p.Controls.Add(new Label { Text = "Mức giảm (*):", Location = new Point(160, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            y += 22;

            _cboLoaiGiamDon = new ComboBox { Location = new Point(0, y), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _cboLoaiGiamDon.Items.AddRange(new object[] { "% (Phần trăm)", "VND (Tiền mặt)" });
            _cboLoaiGiamDon.SelectedIndex = 0;
            _cboLoaiGiamDon.SelectedIndexChanged += (s, e) => {
                bool isPercent = _cboLoaiGiamDon.SelectedIndex == 0;
                _nudGiamToiDa.Enabled = isPercent;
                _lblGiamToiDa.ForeColor = isPercent ? ThemeManager.Ink : ThemeManager.Muted;
                _nudGiaTriGiamDon.Maximum = isPercent ? 100 : 10000000;
            };
            p.Controls.Add(_cboLoaiGiamDon);

            _nudGiaTriGiamDon = new NumericUpDown { Location = new Point(160, y), Width = 380, Maximum = 100, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _nudGiaTriGiamDon.Value = 10;
            p.Controls.Add(_nudGiaTriGiamDon);
            y += 34;

            // Đơn tối thiểu + Giảm tối đa
            p.Controls.Add(new Label { Text = "Đơn hàng tối thiểu (VNĐ):", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            _lblGiamToiDa = new Label { Text = "Mức giảm tối đa (VNĐ - khi giảm %):", Location = new Point(270, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false };
            p.Controls.Add(_lblGiamToiDa);
            y += 22;

            _nudDonToiThieu = new NumericUpDown { Location = new Point(0, y), Width = 250, Maximum = 100000000, Increment = 50000, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _nudDonToiThieu.Value = 0;
            p.Controls.Add(_nudDonToiThieu);

            _nudGiamToiDa = new NumericUpDown { Location = new Point(270, y), Width = 270, Maximum = 100000000, Increment = 10000, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _nudGiamToiDa.Value = 50000;
            p.Controls.Add(_nudGiamToiDa);
            y += 40;

            var pnlTip = new Panel {
                Location = new Point(0, y),
                Size = new Size(540, 60),
                BackColor = ThemeManager.PrimarySoft
            };
            pnlTip.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlTip.Width - 1, pnlTip.Height - 1);
                }
            };

            var lblTip = new Label {
                Text = "Gợi ý an toàn: Luôn đặt Đơn hàng tối thiểu và Mức giảm tối đa để tránh trường hợp đơn tiệc lớn bị chiết khấu vượt quá lợi nhuận gộp.",
                Location = new Point(12, 10),
                Size = new Size(516, 40),
                ForeColor = ThemeManager.PrimaryDark,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                UseMnemonic = false
            };
            pnlTip.Controls.Add(lblTip);
            p.Controls.Add(pnlTip);
        }

        private void InitTheoMonPanel(Panel p) {
            int y = 0;
            // Chọn món ăn
            p.Controls.Add(new Label { 
                Text = "Chọn món ăn áp dụng ưu đãi (Mã món + Tên món) (*):", 
                Location = new Point(0, y), 
                AutoSize = true, 
                ForeColor = ThemeManager.Ink,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                UseMnemonic = false 
            });
            y += 24;

            _cboMonAn = new ComboBox { 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            _cboMonAn.SelectedIndexChanged += (s, e) => UpdatePreviewPrice();
            
            // Khung viền đậm cho ComboBox món ăn
            var pnlCboMon = CreateBoldBorderContainer(_cboMonAn, 0, y, 540, 34);
            p.Controls.Add(pnlCboMon);
            y += 42;

            // Loại giảm + Mức giảm
            p.Controls.Add(new Label { Text = "Loại giảm:", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            p.Controls.Add(new Label { Text = "Mức giảm (*):", Location = new Point(160, y), AutoSize = true, ForeColor = ThemeManager.Ink, UseMnemonic = false });
            y += 22;

            _cboLoaiGiamMon = new ComboBox { Location = new Point(0, y), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _cboLoaiGiamMon.Items.AddRange(new object[] { "% (Phần trăm)", "VND (Tiền mặt)" });
            _cboLoaiGiamMon.SelectedIndex = 0;
            _cboLoaiGiamMon.SelectedIndexChanged += (s, e) => {
                bool isPercent = _cboLoaiGiamMon.SelectedIndex == 0;
                _nudGiaTriGiamMon.Maximum = isPercent ? 100 : 10000000;
                UpdatePreviewPrice();
            };
            p.Controls.Add(_cboLoaiGiamMon);

            _nudGiaTriGiamMon = new NumericUpDown { Location = new Point(160, y), Width = 380, Maximum = 100, ThousandsSeparator = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
            _nudGiaTriGiamMon.Value = 15;
            _nudGiaTriGiamMon.ValueChanged += (s, e) => UpdatePreviewPrice();
            p.Controls.Add(_nudGiaTriGiamMon);
            y += 40;

            // ================= KHỐI PHÂN TÍCH TÀI CHÍNH & LỢI NHUẬN =================
            _pnlFinanceCard = new Panel {
                Location = new Point(0, y),
                Size = new Size(540, 222),
                BackColor = ThemeManager.Surface2
            };
            _pnlFinanceCard.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1.5f)) {
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlFinanceCard.Width - 1, _pnlFinanceCard.Height - 1);
                }
            };

            int cy = 12;
            var lblCardTitle = new Label {
                Text = "[ PHÂN TÍCH TÀI CHÍNH & BIÊN LỢI NHUẬN MÓN ĂN ]",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Location = new Point(14, cy),
                AutoSize = true,
                UseMnemonic = false
            };
            _pnlFinanceCard.Controls.Add(lblCardTitle);
            cy += 30;

            // Cột Trái: Giá bán gốc & Giá vốn ước tính (Khoảng cách an toàn, không đè chữ)
            var lblGiaGocTitle = new Label { Text = "Giá bán niêm yết:", Location = new Point(14, cy), AutoSize = true, ForeColor = ThemeManager.Muted, UseMnemonic = false };
            _lblGiaGocVal = new Label { Text = "0 đ", Location = new Point(155, cy), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink, UseMnemonic = false };
            _pnlFinanceCard.Controls.Add(lblGiaGocTitle);
            _pnlFinanceCard.Controls.Add(_lblGiaGocVal);

            // Cột Phải: Giá sau ưu đãi
            var lblSauGiamTitle = new Label { Text = "Giá sau ưu đãi:", Location = new Point(310, cy), AutoSize = true, ForeColor = ThemeManager.Muted, UseMnemonic = false };
            _lblGiaSauGiamVal = new Label { Text = "0 đ", Location = new Point(415, cy), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.PrimaryDark, UseMnemonic = false };
            _pnlFinanceCard.Controls.Add(lblSauGiamTitle);
            _pnlFinanceCard.Controls.Add(_lblGiaSauGiamVal);

            cy += 26;

            // Hàng 2: Giá vốn nguyên liệu & Tiết kiệm cho khách
            var lblGiaVonTitle = new Label { Text = "Giá vốn (Food Cost):", Location = new Point(14, cy), AutoSize = true, ForeColor = ThemeManager.Muted, UseMnemonic = false };
            _lblGiaVonVal = new Label { Text = "0 đ", Location = new Point(155, cy), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink2, UseMnemonic = false };
            _pnlFinanceCard.Controls.Add(lblGiaVonTitle);
            _pnlFinanceCard.Controls.Add(_lblGiaVonVal);

            var lblTietKiemTitle = new Label { Text = "Mức giảm / đĩa:", Location = new Point(310, cy), AutoSize = true, ForeColor = ThemeManager.Muted, UseMnemonic = false };
            _lblTietKiemVal = new Label { Text = "0 đ", Location = new Point(415, cy), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular), ForeColor = ThemeManager.Muted, UseMnemonic = false };
            _pnlFinanceCard.Controls.Add(lblTietKiemTitle);
            _pnlFinanceCard.Controls.Add(_lblTietKiemVal);

            cy += 26;

            // Hàng 3: Lợi nhuận gộp dự kiến
            var lblLaiGopTitle = new Label { Text = "Lợi nhuận gộp dự kiến:", Location = new Point(14, cy), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink, UseMnemonic = false };
            _lblLaiGopVal = new Label { Text = "0 đ", Location = new Point(175, cy), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.Green, UseMnemonic = false };
            _pnlFinanceCard.Controls.Add(lblLaiGopTitle);
            _pnlFinanceCard.Controls.Add(_lblLaiGopVal);

            cy += 32;

            // Banner Cảnh báo / Đánh giá an toàn tài chính
            _pnlWarningBanner = new Panel {
                Location = new Point(14, cy),
                Size = new Size(512, 58),
                BackColor = ThemeManager.GreenSoft
            };
            _pnlWarningBanner.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(60, 0, 0, 0))) {
                    e.Graphics.DrawRectangle(pen, 0, 0, _pnlWarningBanner.Width - 1, _pnlWarningBanner.Height - 1);
                }
            };

            _lblWarningText = new Label {
                Text = "Đang phân tích dữ liệu món ăn...",
                Location = new Point(10, 8),
                Size = new Size(492, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.Green,
                UseMnemonic = false
            };
            _pnlWarningBanner.Controls.Add(_lblWarningText);
            _pnlFinanceCard.Controls.Add(_pnlWarningBanner);

            p.Controls.Add(_pnlFinanceCard);
            y += 230;

            var lblMonNote = new Label {
                Text = "Gợi ý kinh doanh: Nên giữ tỷ suất lãi gộp sau giảm từ 30% trở lên để đảm bảo đủ bù đắp chi phí mặt bằng và nhân sự.",
                Location = new Point(0, y),
                Size = new Size(540, 36),
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                UseMnemonic = false
            };
            p.Controls.Add(lblMonNote);
        }

        private void ToggleScope() {
            bool isToanDon = _rbToanDon.Checked;
            _pnlToanDon.Visible = isToanDon;
            _pnlTheoMon.Visible = !isToanDon;
        }

        private void UpdatePreviewPrice() {
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
            double giam = (double)_nudGiaTriGiamMon.Value;
            int loaiGiam = _cboLoaiGiamMon.SelectedIndex;

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

                // Bind Chương trình
                _cboChuongTrinh.Items.Clear();
                foreach (var ct in _chuongTrinhList) {
                    _cboChuongTrinh.Items.Add(new ComboBoxItem {
                        Value = ct.MaChuongTrinh,
                        Text = $"[{ct.MaChuongTrinh}] {ct.TenChuongTrinh} ({ct.TrangThai})"
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
                }

                // Bind Món ăn
                _cboMonAn.DisplayMember = "DisplayText";
                _cboMonAn.ValueMember = "MaMon";
                _cboMonAn.DataSource = _dishList;

                if (_dishList != null && _dishList.Count > 0) {
                    _cboMonAn.SelectedIndex = 0;
                    UpdatePreviewPrice();
                } else {
                    _lblWarningText.Text = "Chưa tìm thấy món ăn nào khả dụng. Vui lòng kiểm tra lại thực đơn.";
                    _lblWarningText.ForeColor = ThemeManager.Amber;
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "Lỗi khi tải dữ liệu khởi tạo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveAsync() {
            var selectedCt = _cboChuongTrinh.SelectedItem as ComboBoxItem;
            if (selectedCt == null) {
                MessageBox.Show(this, "Vui lòng chọn chương trình khuyến mãi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnSave.Enabled = false;
            try {
                if (_rbToanDon.Checked) {
                    // Tạo Voucher toàn hóa đơn
                    var maVoucher = _txtMaVoucher.Text.Trim();
                    if (string.IsNullOrEmpty(maVoucher)) {
                        MessageBox.Show(this, "Vui lòng nhập mã Voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _txtMaVoucher.Focus();
                        return;
                    }

                    string loaiGiam = _cboLoaiGiamDon.SelectedIndex == 0 ? "%" : "VND";
                    double giaTriGiam = (double)_nudGiaTriGiamDon.Value;
                    double? donToiThieu = _nudDonToiThieu.Value > 0 ? (double?)_nudDonToiThieu.Value : null;
                    double? giamToiDa = (_cboLoaiGiamDon.SelectedIndex == 0 && _nudGiamToiDa.Value > 0) ? (double?)_nudGiamToiDa.Value : null;

                    var (ok, msg) = await _service.CreateVoucherAsync(selectedCt.Value, maVoucher, loaiGiam, giaTriGiam, donToiThieu, giamToiDa);
                    if (ok) {
                        MessageBox.Show(this, $"Tạo voucher '{maVoucher}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(this, msg, "Lỗi tạo voucher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else {
                    // Tạo ưu đãi theo món
                    var dish = _cboMonAn.SelectedItem as MonAnSelectDto;
                    if (dish == null) {
                        MessageBox.Show(this, "Vui lòng chọn món ăn cần áp dụng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string loaiGiam = _cboLoaiGiamMon.SelectedIndex == 0 ? "%" : "VND";
                    double giaTriGiam = (double)_nudGiaTriGiamMon.Value;

                    // Kiểm tra an toàn tài chính trước khi lưu
                    if (dish.DonGia.HasValue) {
                        double goc = dish.DonGia.Value;
                        double giaVon = dish.GiaVonUocTinh ?? 0;
                        double sauGiam = loaiGiam == "%" ? goc * (1 - giaTriGiam / 100.0) : Math.Max(0, goc - giaTriGiam);

                        // Chặn tuyệt đối nếu bán dưới giá vốn nguyên liệu
                        if (giaVon > 0 && sauGiam < giaVon) {
                            double tienLo = giaVon - sauGiam;
                            MessageBox.Show(this, 
                                "NGĂN CHẶN THUA LỖ:\n\n" +
                                $"Giá sau ưu đãi ({sauGiam:N0} đ) THẤP HƠN Giá vốn nguyên liệu ({giaVon:N0} đ).\n" +
                                $"Nhà hàng sẽ chịu lỗ trực tiếp {tienLo:N0} đ trên mỗi đĩa bán ra!\n\n" +
                                "Vui lòng điều chỉnh lại mức giảm giá để bảo đảm doanh thu luôn ở mức lời.", 
                                "Cảnh báo thua lỗ nghiêm trọng", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }

                        // Cảnh báo xác nhận nếu biên lợi nhuận quá mỏng (< 20%)
                        if (giaVon > 0) {
                            double laiGop = sauGiam - giaVon;
                            double tyLeLai = sauGiam > 0 ? (laiGop / sauGiam) * 100.0 : 0;
                            if (tyLeLai < 20.0) {
                                var confirm = MessageBox.Show(this, 
                                    "CẢNH BÁO BIÊN LỢI NHUẬN THẤP:\n\n" +
                                    $"Biên lợi nhuận gộp của món này sau giảm chỉ đạt {tyLeLai:N1}% ({laiGop:N0} đ/đĩa).\n" +
                                    "Mức lãi này có nguy cơ không đủ bù đắp chi phí mặt bằng & nhân sự.\n\n" +
                                    "Bạn có chắc chắn vẫn muốn thiết lập ưu đãi này không?", 
                                    "Xác nhận rủi ro tài chính", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (confirm != DialogResult.Yes) return;
                            }
                        }
                    }

                    var (ok, msg) = await _service.CreateKmMonAsync(selectedCt.Value, dish.MaMon, loaiGiam, giaTriGiam);
                    if (ok) {
                        MessageBox.Show(this, $"Đã thiết lập ưu đãi cho món '{dish.TenMon}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(this, msg, "Lỗi thiết lập ưu đãi món", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
