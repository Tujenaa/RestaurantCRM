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
        private Label _lblPreviewGia;

        private Button _btnSave;
        private Button _btnCancel;

        public TaoVoucherDialog(KhuyenMaiService service, string preselectedCtId = null, bool defaultTheoMon = false) {
            _service = service;

            Text = "Tạo Voucher / Ưu đãi khuyến mãi";
            Size = new Size(580, 560);
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
            int width = 500;

            var lblHeader = new Label {
                Text = "THIẾT LẬP VOUCHER / ƯU ĐÃI KHUYẾN MÃI",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Location = new Point(left, top),
                AutoSize = true
            };
            Controls.Add(lblHeader);
            top += 42;

            // 1. Chọn chương trình khuyến mãi
            var lblCT = new Label { Text = "1. Chọn chương trình khuyến mãi (*):", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(left, top), AutoSize = true };
            Controls.Add(lblCT);
            top += 24;

            _cboChuongTrinh = new ComboBox {
                Location = new Point(left, top),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_cboChuongTrinh);
            top += 38;

            // 2. Chọn hình thức áp dụng
            var lblLoai = new Label { Text = "2. Chọn phạm vi áp dụng (*):", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(left, top), AutoSize = true };
            Controls.Add(lblLoai);
            top += 24;

            var pnlType = new Panel { Location = new Point(left, top), Size = new Size(width, 36) };
            _rbToanDon = new RadioButton { Text = "Toàn hóa đơn (Nhập mã Voucher)", Location = new Point(0, 5), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            _rbTheoMon = new RadioButton { Text = "Theo món ăn (Tự động giảm giá sản phẩm)", Location = new Point(240, 5), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            
            _rbToanDon.CheckedChanged += (s, e) => ToggleScope();
            _rbTheoMon.CheckedChanged += (s, e) => ToggleScope();

            pnlType.Controls.Add(_rbToanDon);
            pnlType.Controls.Add(_rbTheoMon);
            Controls.Add(pnlType);
            top += 42;

            // Panel Toàn Đơn
            _pnlToanDon = new Panel { Location = new Point(left, top), Size = new Size(width, 240) };
            InitToanDonPanel(_pnlToanDon);
            Controls.Add(_pnlToanDon);

            // Panel Theo Món
            _pnlTheoMon = new Panel { Location = new Point(left, top), Size = new Size(width, 240), Visible = false };
            InitTheoMonPanel(_pnlTheoMon);
            Controls.Add(_pnlTheoMon);

            top += 245;

            // Buttons
            _btnSave = new Button {
                Text = "Tạo ưu đãi ngay",
                Location = new Point(left + 230, top),
                Size = new Size(150, 38),
                BackColor = ThemeManager.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveAsync();

            _btnCancel = new Button {
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                Location = new Point(left + 390, top),
                Size = new Size(110, 38),
                BackColor = Color.FromArgb(240, 243, 248),
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
        }

        private void InitToanDonPanel(Panel p) {
            int y = 0;
            // Mã Voucher
            p.Controls.Add(new Label { Text = "Mã Voucher (Ví dụ: VOUCHER40K, HE2026) (*):", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            y += 22;
            _txtMaVoucher = new TextBox { Location = new Point(0, y), Width = 490, CharacterCasing = CharacterCasing.Upper };
            p.Controls.Add(_txtMaVoucher);
            y += 34;

            // Loại giảm + Mức giảm
            p.Controls.Add(new Label { Text = "Loại giảm:", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            p.Controls.Add(new Label { Text = "Mức giảm (*):", Location = new Point(160, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            y += 22;

            _cboLoaiGiamDon = new ComboBox { Location = new Point(0, y), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboLoaiGiamDon.Items.AddRange(new object[] { "% (Phần trăm)", "VND (Tiền mặt)" });
            _cboLoaiGiamDon.SelectedIndex = 0;
            _cboLoaiGiamDon.SelectedIndexChanged += (s, e) => {
                bool isPercent = _cboLoaiGiamDon.SelectedIndex == 0;
                _nudGiamToiDa.Enabled = isPercent;
                _lblGiamToiDa.ForeColor = isPercent ? ThemeManager.Ink : ThemeManager.Muted;
                _nudGiaTriGiamDon.Maximum = isPercent ? 100 : 10000000;
            };
            p.Controls.Add(_cboLoaiGiamDon);

            _nudGiaTriGiamDon = new NumericUpDown { Location = new Point(160, y), Width = 330, Maximum = 100, ThousandsSeparator = true };
            _nudGiaTriGiamDon.Value = 10;
            p.Controls.Add(_nudGiaTriGiamDon);
            y += 34;

            // Đơn tối thiểu + Giảm tối đa
            p.Controls.Add(new Label { Text = "Đơn hàng tối thiểu (VNĐ):", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            _lblGiamToiDa = new Label { Text = "Mức giảm tối đa (VNĐ - khi giảm %):", Location = new Point(250, y), AutoSize = true, ForeColor = ThemeManager.Ink };
            p.Controls.Add(_lblGiamToiDa);
            y += 22;

            _nudDonToiThieu = new NumericUpDown { Location = new Point(0, y), Width = 230, Maximum = 100000000, Increment = 50000, ThousandsSeparator = true };
            _nudDonToiThieu.Value = 0;
            p.Controls.Add(_nudDonToiThieu);

            _nudGiamToiDa = new NumericUpDown { Location = new Point(250, y), Width = 240, Maximum = 100000000, Increment = 10000, ThousandsSeparator = true };
            _nudGiamToiDa.Value = 50000;
            p.Controls.Add(_nudGiamToiDa);
            y += 34;

            var lblNote = new Label {
                Text = "💡 Ví dụ: Giảm 20% tối đa 50.000đ cho đơn từ 200.000đ trở lên.",
                Location = new Point(0, y),
                AutoSize = true,
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            p.Controls.Add(lblNote);
        }

        private void InitTheoMonPanel(Panel p) {
            int y = 0;
            // Chọn món ăn
            p.Controls.Add(new Label { Text = "Chọn món ăn áp dụng ưu đãi (Mã món + Tên món) (*):", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            y += 22;
            _cboMonAn = new ComboBox { Location = new Point(0, y), Width = 490, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboMonAn.SelectedIndexChanged += (s, e) => UpdatePreviewPrice();
            p.Controls.Add(_cboMonAn);
            y += 34;

            // Loại giảm + Mức giảm
            p.Controls.Add(new Label { Text = "Loại giảm:", Location = new Point(0, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            p.Controls.Add(new Label { Text = "Mức giảm (*):", Location = new Point(160, y), AutoSize = true, ForeColor = ThemeManager.Ink });
            y += 22;

            _cboLoaiGiamMon = new ComboBox { Location = new Point(0, y), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboLoaiGiamMon.Items.AddRange(new object[] { "% (Phần trăm)", "VND (Tiền mặt)" });
            _cboLoaiGiamMon.SelectedIndex = 0;
            _cboLoaiGiamMon.SelectedIndexChanged += (s, e) => {
                bool isPercent = _cboLoaiGiamMon.SelectedIndex == 0;
                _nudGiaTriGiamMon.Maximum = isPercent ? 100 : 10000000;
                UpdatePreviewPrice();
            };
            p.Controls.Add(_cboLoaiGiamMon);

            _nudGiaTriGiamMon = new NumericUpDown { Location = new Point(160, y), Width = 330, Maximum = 100, ThousandsSeparator = true };
            _nudGiaTriGiamMon.Value = 15;
            _nudGiaTriGiamMon.ValueChanged += (s, e) => UpdatePreviewPrice();
            p.Controls.Add(_nudGiaTriGiamMon);
            y += 38;

            _lblPreviewGia = new Label {
                Text = "👉 Giá gốc: 0 đ  ➜  Giá sau giảm: 0 đ",
                Location = new Point(0, y),
                AutoSize = true,
                ForeColor = ThemeManager.Green,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            p.Controls.Add(_lblPreviewGia);
            y += 30;

            var lblMonNote = new Label {
                Text = "💡 Khách hàng trên Web & App sẽ thấy món ăn tự động giảm giá trong suốt thời gian diễn ra chương trình.",
                Location = new Point(0, y),
                Size = new Size(490, 40),
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
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
                _lblPreviewGia.Text = "👉 Vui lòng chọn món ăn để xem dự tính giá sau giảm.";
                return;
            }

            double goc = selectedDish.DonGia.Value;
            double giam = (double)_nudGiaTriGiamMon.Value;
            double sauGiam = goc;

            if (_cboLoaiGiamMon.SelectedIndex == 0) {
                sauGiam = goc * (1 - giam / 100.0);
            } else {
                sauGiam = Math.Max(0, goc - giam);
            }

            _lblPreviewGia.Text = $"👉 Giá gốc: {goc:N0} đ  ➜  Giá sau ưu đãi: {sauGiam:N0} đ (Tiết kiệm {(goc - sauGiam):N0} đ)";
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
                    _lblPreviewGia.Text = "⚠️ Chưa tìm thấy món ăn nào khả dụng. Vui lòng kiểm tra lại thực đơn.";
                    _lblPreviewGia.ForeColor = ThemeManager.Amber;
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
