using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhachHang;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhachHang
{
    /// <summary>Hộp thoại thêm / sửa khách hàng, kiểm tra dữ liệu trước khi gửi API.</summary>
    public class KhachHangDialog : CrmDialog
    {
        private static readonly Regex PhoneRegex = new Regex(@"^0\d{9,10}$");
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private readonly KhachHangService _service = new KhachHangService();
        private readonly KhachHangDto _edit;
        private readonly TextBox _txtHoTen = new TextBox { MaxLength = 100 };
        private readonly TextBox _txtSdt = new TextBox { MaxLength = 11 };
        private readonly TextBox _txtEmail = new TextBox { MaxLength = 100 };
        private readonly ComboBox _cboGioiTinh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _dtNgaySinh = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        private readonly TextBox _txtDiaChi = new TextBox { MaxLength = 255 };
        private readonly ComboBox _cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblError;

        /// <summary>Mã khách hàng vừa thêm / sửa (để danh sách chọn lại dòng đó).</summary>
        public string SavedId { get; private set; }

        public KhachHangDialog(KhachHangDto edit)
            : base(edit == null ? "Thêm khách hàng" : "Sửa thông tin khách hàng",
                   edit == null ? "Nhập thông tin khách hàng. Trường có dấu * là bắt buộc." : "Mã khách hàng: " + edit.MaKhachHang,
                   580, 372, edit == null ? "Thêm khách hàng" : "Lưu thay đổi")
        {
            _edit = edit;
            const int L = Gutter, W = 580 - Gutter * 2, C = (W - 20) / 2, R = L + C + 20;

            Field(Body, "Họ và tên", _txtHoTen, L, 6, W, true);
            Field(Body, "Số điện thoại", _txtSdt, L, 72, C, true);
            Field(Body, "Email", _txtEmail, R, 72, C);
            Field(Body, "Giới tính", _cboGioiTinh, L, 138, C);
            Field(Body, "Ngày sinh", _dtNgaySinh, R, 138, C);
            Field(Body, "Địa chỉ", _txtDiaChi, L, 204, W);
            Field(Body, "Trạng thái tài khoản", _cboTrangThai, L, 270, C);
            if (edit == null) Hint(Body, "Mật khẩu đăng nhập website mặc định: 123456", R, 302, C);

            _lblError = new Label
            {
                AutoSize = false,
                Location = new Point(L, 338),
                Size = new Size(W, 34),
                UseMnemonic = false,
                ForeColor = ThemeManager.Red,
                Font = new Font(ThemeManager.FontFamily, 9F),
                Text = ""
            };
            Body.Controls.Add(_lblError);

            _cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            _cboTrangThai.Items.AddRange(new object[] { "Hoạt động", "Đã khóa" });
            _dtNgaySinh.MaxDate = DateTime.Today;
            _dtNgaySinh.MinDate = DateTime.Today.AddYears(-120);

            if (edit != null)
            {
                _txtHoTen.Text = edit.HoTen ?? "";
                _txtSdt.Text = edit.SoDienThoai ?? "";
                _txtEmail.Text = edit.Email ?? "";
                _txtDiaChi.Text = edit.DiaChi ?? "";
                _cboGioiTinh.SelectedItem = _cboGioiTinh.Items.Contains(edit.GioiTinh ?? "") ? edit.GioiTinh : null;
                _cboTrangThai.SelectedIndex = edit.BiKhoa ? 1 : 0;
                var ns = edit.NgaySinhDate;
                if (ns.HasValue && ns.Value <= _dtNgaySinh.MaxDate && ns.Value >= _dtNgaySinh.MinDate) { _dtNgaySinh.Value = ns.Value; _dtNgaySinh.Checked = true; }
                else { _dtNgaySinh.Value = new DateTime(2000, 1, 1); _dtNgaySinh.Checked = false; }
            }
            else
            {
                _cboTrangThai.SelectedIndex = 0;
                _dtNgaySinh.Value = new DateTime(2000, 1, 1);
                _dtNgaySinh.Checked = false;
            }

            // Số điện thoại chỉ cho nhập chữ số
            _txtSdt.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            foreach (Control c in new Control[] { _txtHoTen, _txtSdt, _txtEmail, _txtDiaChi })
                c.TextChanged += (s, e) => _lblError.Text = "";

            PrimaryButton.Click += async (s, e) => await SaveAsync();
            AcceptButton = PrimaryButton;
            Shown += (s, e) => _txtHoTen.Focus();
        }

        private bool KiemTra(out KhachHangRequest req)
        {
            req = null;
            string hoTen = _txtHoTen.Text.Trim();
            string sdt = _txtSdt.Text.Trim();
            string email = _txtEmail.Text.Trim();

            if (hoTen.Length == 0) return Fail(_txtHoTen, "Vui lòng nhập họ và tên khách hàng.");
            if (hoTen.Length < 2) return Fail(_txtHoTen, "Họ và tên quá ngắn.");
            if (sdt.Length == 0) return Fail(_txtSdt, "Vui lòng nhập số điện thoại.");
            if (!PhoneRegex.IsMatch(sdt)) return Fail(_txtSdt, "Số điện thoại phải bắt đầu bằng 0 và có 10–11 chữ số.");
            if (email.Length > 0 && !EmailRegex.IsMatch(email)) return Fail(_txtEmail, "Email không đúng định dạng (ví dụ: ten@gmail.com).");
            if (_dtNgaySinh.Checked && _dtNgaySinh.Value.Date > DateTime.Today) return Fail(_dtNgaySinh, "Ngày sinh không được lớn hơn hôm nay.");

            req = new KhachHangRequest
            {
                HoTen = hoTen,
                SoDienThoai = sdt,
                Email = email.Length == 0 ? null : email,
                GioiTinh = _cboGioiTinh.SelectedItem as string,
                NgaySinh = _dtNgaySinh.Checked ? _dtNgaySinh.Value.ToString("yyyy-MM-dd") : null,
                DiaChi = string.IsNullOrWhiteSpace(_txtDiaChi.Text) ? null : _txtDiaChi.Text.Trim(),
                TrangThai = _cboTrangThai.SelectedIndex == 1 ? "Locked" : "Active"
            };
            return true;
        }

        private bool Fail(Control c, string message)
        {
            _lblError.Text = message;
            c.Focus();
            return false;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            KhachHangRequest req;
            if (!KiemTra(out req)) return;

            SetBusy(true);
            ApiResult r = _edit == null ? await _service.CreateAsync(req) : await _service.UpdateAsync(_edit.MaKhachHang, req);
            if (IsDisposed) return;
            SetBusy(false);

            if (!r.Success)
            {
                _lblError.Text = r.Message;
                if (r.Message.Contains("Số điện thoại")) _txtSdt.Focus();
                else if (r.Message.Contains("Email")) _txtEmail.Focus();
                return;
            }

            SavedId = _edit != null ? _edit.MaKhachHang : null;
            if (_edit == null) SavedId = r.Field("maKhachHang");
            if (_edit == null && string.IsNullOrWhiteSpace(SavedId))
            {
                // Dự phòng: tìm lại theo số điện thoại vừa nhập
                try
                {
                    var found = await _service.GetAllAsync(req.SoDienThoai);
                    if (found.Count > 0) SavedId = found[0].MaKhachHang;
                }
                catch { }
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}