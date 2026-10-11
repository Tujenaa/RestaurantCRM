using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhuyenMai {
    public class ChuongTrinhDialog : Form {
        private readonly KhuyenMaiService _service;
        private readonly ChuongTrinhDto _editingItem;

        private TextBox _txtTen;
        private DateTimePicker _dtpStart;
        private DateTimePicker _dtpEnd;
        private CheckBox _chkNoEnd;
        private Button _btnSave;
        private Button _btnCancel;

        public ChuongTrinhDialog(KhuyenMaiService service, ChuongTrinhDto editingItem = null) {
            _service = service;
            _editingItem = editingItem;

            Text = _editingItem == null ? "Tạo chương trình khuyến mãi mới" : "Chỉnh sửa chương trình khuyến mãi";
            Size = new Size(500, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Paper;
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitControls();
        }

        private void InitControls() {
            int left = 32;
            int top = 24;
            int inputWidth = 420;

            var lblTitle = new Label {
                Text = _editingItem == null ? "THÊM CHƯƠNG TRÌNH KHUYẾN MÃI" : "CẬP NHẬT CHƯƠNG TRÌNH KHUYẾN MÃI",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Blue,
                Location = new Point(left, top),
                AutoSize = true
            };
            Controls.Add(lblTitle);
            top += 45;

            // Tên chương trình
            var lblTen = new Label { Text = "Tên chương trình khuyến mãi (*):", Location = new Point(left, top), AutoSize = true, ForeColor = ThemeManager.Ink };
            Controls.Add(lblTen);
            top += 24;

            _txtTen = new TextBox { Location = new Point(left, top), Width = inputWidth, Height = 28 };
            if (_editingItem != null) _txtTen.Text = _editingItem.TenChuongTrinh;
            Controls.Add(_txtTen);
            top += 40;

            // Ngày bắt đầu
            var lblStart = new Label { Text = "Ngày bắt đầu áp dụng:", Location = new Point(left, top), AutoSize = true, ForeColor = ThemeManager.Ink };
            Controls.Add(lblStart);

            var lblEnd = new Label { Text = "Ngày kết thúc:", Location = new Point(left + 220, top), AutoSize = true, ForeColor = ThemeManager.Ink };
            Controls.Add(lblEnd);
            top += 24;

            _dtpStart = new DateTimePicker { Location = new Point(left, top), Width = 195, Format = DateTimePickerFormat.Short };
            _dtpEnd = new DateTimePicker { Location = new Point(left + 220, top), Width = 200, Format = DateTimePickerFormat.Short };

            if (_editingItem != null) {
                if (DateTime.TryParse(_editingItem.NgayBatDau, out var s)) _dtpStart.Value = s;
                if (DateTime.TryParse(_editingItem.NgayKetThuc, out var e)) _dtpEnd.Value = e;
            } else {
                _dtpStart.Value = DateTime.Today;
                _dtpEnd.Value = DateTime.Today.AddMonths(1);
            }

            Controls.Add(_dtpStart);
            Controls.Add(_dtpEnd);
            top += 36;

            _chkNoEnd = new CheckBox {
                Text = "Chương trình dài hạn (Không giới hạn ngày kết thúc)",
                Location = new Point(left, top),
                AutoSize = true,
                ForeColor = ThemeManager.Muted
            };
            _chkNoEnd.CheckedChanged += (s, e) => _dtpEnd.Enabled = !_chkNoEnd.Checked;
            if (_editingItem != null && string.IsNullOrEmpty(_editingItem.NgayKetThuc)) {
                _chkNoEnd.Checked = true;
            }
            Controls.Add(_chkNoEnd);
            top += 50;

            // Buttons
            _btnSave = new Button {
                Text = _editingItem == null ? "Tạo chương trình" : "Lưu thay đổi",
                DialogResult = DialogResult.None,
                Location = new Point(left + 170, top),
                Size = new Size(130, 36),
                BackColor = ThemeManager.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveAsync();

            _btnCancel = new Button {
                Text = "Đóng",
                DialogResult = DialogResult.Cancel,
                Location = new Point(left + 310, top),
                Size = new Size(110, 36),
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

        private async System.Threading.Tasks.Task SaveAsync() {
            var ten = _txtTen.Text.Trim();
            if (string.IsNullOrEmpty(ten)) {
                MessageBox.Show(this, "Vui lòng nhập tên chương trình khuyến mãi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTen.Focus();
                return;
            }

            DateTime? start = _dtpStart.Value.Date;
            DateTime? end = _chkNoEnd.Checked ? null : (DateTime?)_dtpEnd.Value.Date;

            if (end.HasValue && end < start) {
                MessageBox.Show(this, "Ngày kết thúc không được nhỏ hơn ngày bắt đầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnSave.Enabled = false;
            try {
                if (_editingItem == null) {
                    var (ok, msg) = await _service.CreateChuongTrinhAsync(ten, start, end);
                    if (ok) {
                        MessageBox.Show(this, "Tạo chương trình khuyến mãi thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(this, msg, "Lỗi tạo chương trình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else {
                    var (ok, msg) = await _service.UpdateChuongTrinhAsync(_editingItem.MaChuongTrinh, ten, start, end);
                    if (ok) {
                        MessageBox.Show(this, "Cập nhật chương trình thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(this, msg, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } finally {
                _btnSave.Enabled = true;
            }
        }
    }
}
