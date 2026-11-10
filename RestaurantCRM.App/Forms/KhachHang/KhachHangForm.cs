using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhachHang;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhachHang
{
    /// <summary>
    /// Quản lý khách hàng: danh sách, tìm theo mã / tên / số điện thoại / email, lọc trạng thái & giới tính,
    /// xem chi tiết (kèm lịch sử đơn hàng), thêm, sửa, khóa / mở khóa và xóa.
    /// </summary>
    public partial class KhachHangForm : Form
    {
        private readonly KhachHangService _service = new KhachHangService();
        private List<KhachHangDto> _all = new List<KhachHangDto>();

        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _txtSearch = new TextBox();
        private readonly ComboBox _cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboGioiTinh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboPhanKhuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboSinhNhat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new Label { Text = "" };
        private readonly Label _lblFooter = new Label { Text = "" };
        private readonly Label _kTong = new Label { Text = "Tổng khách hàng: —" };
        private readonly Label _kActive = new Label { Text = "Đang hoạt động: —" };
        private readonly Label _kLocked = new Label { Text = "Tài khoản bị khóa: —" };
        private readonly Label _kChiTieu = new Label { Text = "Tổng chi tiêu: —" };
        private readonly Button _btnThem = new Button { Text = "Thêm khách hàng" };
        private readonly Button _btnLamMoi = new Button { Text = "Làm mới" };
        private readonly Button _btnChiTiet = new Button { Text = "Xem chi tiết" };
        private readonly Button _btnSua = new Button { Text = "Sửa" };
        private readonly Button _btnKhoa = new Button { Text = "Khóa tài khoản" };
        private readonly Button _btnDoiMatKhau = new Button { Text = "Đổi mật khẩu" };
        private readonly Button _btnXoa = new Button { Text = "Xóa" };
        private bool _loading;

        public KhachHangForm()
        {
            InitializeComponent();
            Text = "Khách hàng";
            Size = new Size(1180, 720);
            Font = new Font(ThemeManager.FontFamily, 9.5F);
            StartPosition = FormStartPosition.CenterParent;

            BuildGrid();

            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Hoạt động", "Đã khóa" });
            _cboTrangThai.SelectedIndex = 0;
            _cboGioiTinh.Items.AddRange(new object[] { "Tất cả", "Nam", "Nữ", "Khác" });
            _cboGioiTinh.SelectedIndex = 0;
            _cboPhanKhuc.Items.AddRange(new object[]
            {
                "Tất cả",
                "Khách VIP (≥ 2 triệu)",
                "Thân thiết (≥ 3 đơn)",
                "Đã mua hàng (≥ 1 đơn)",
                "Chưa mua hàng (0 đơn)"
            });
            _cboPhanKhuc.SelectedIndex = 0;
            _cboSinhNhat.Items.AddRange(new object[]
            {
                "Tất cả",
                "Sinh nhật tháng này",
                "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
            });
            _cboSinhNhat.SelectedIndex = 0;

            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = new Label { Text = "Khách hàng" },
                Subtitle = new Label { Text = "Hồ sơ khách hàng, lịch sử mua hàng và trạng thái tài khoản." },
                PrimaryActions = new[] { _btnThem },
                HeaderActions = new Control[] { _btnLamMoi },
                KpiLabels = new[] { _kTong, _kActive, _kLocked, _kChiTieu },
                Search = _txtSearch,
                SearchPlaceholder = "Tìm mã, họ tên, SĐT, email, địa chỉ...",
                Filters = new[]
                {
                    new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }),
                    new FilterSpec(_cboPhanKhuc, new Label { Text = "Phân khúc" }),
                    new FilterSpec(_cboGioiTinh, new Label { Text = "Giới tính" }),
                    new FilterSpec(_cboSinhNhat, new Label { Text = "Sinh nhật" })
                },
                CountLabel = _lblCount,
                Grid = _grid,
                SecondaryActions = new[] { _btnChiTiet, _btnSua, _btnKhoa, _btnDoiMatKhau },
                DangerActions = new[] { _btnXoa },
                FooterInfo = _lblFooter
            });

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboGioiTinh.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboPhanKhuc.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboSinhNhat.SelectedIndexChanged += (s, e) => ApplyFilter();
            _grid.SelectionChanged += (s, e) => UpdateSelectionUi();
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) MoChiTiet(); };
            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.Handled = true; MoChiTiet(); }
                else if (e.KeyCode == Keys.Delete) { e.Handled = true; Xoa(); }
            };

            _btnThem.Click += (s, e) => ThemMoi();
            _btnLamMoi.Click += async (s, e) => await LoadDataAsync();
            _btnChiTiet.Click += (s, e) => MoChiTiet();
            _btnSua.Click += (s, e) => Sua();
            _btnKhoa.Click += (s, e) => DoiTrangThaiKhoa();
            _btnDoiMatKhau.Click += (s, e) => DoiMatKhau();
            _btnXoa.Click += (s, e) => Xoa();

            UpdateSelectionUi();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.RowHeadersVisible = false;
            g.AutoGenerateColumns = false;

            AddCol("Ma", "Mã KH", 80);
            AddCol("HoTen", "Họ tên", 190);
            AddCol("SoDienThoai", "Số điện thoại", 120);
            AddCol("Email", "Email", 200);
            AddCol("GioiTinh", "Giới tính", 80);
            AddCol("NgaySinh", "Ngày sinh", 100).DefaultCellStyle.Format = "dd/MM/yyyy";
            AddCol("SoDon", "Số đơn", 70, DataGridViewContentAlignment.MiddleRight).DefaultCellStyle.Format = "N0";
            AddCol("ChiTieu", "Tổng chi tiêu", 120, DataGridViewContentAlignment.MiddleRight).DefaultCellStyle.Format = "#,##0 đ";
            AddCol("TrangThai", "Trạng thái", 110);
        }

        private DataGridViewColumn AddCol(string name, string header, int width, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, Width = width, SortMode = DataGridViewColumnSortMode.Automatic };
            c.DefaultCellStyle.Alignment = align;
            _grid.Columns.Add(c);
            return c;
        }

        // ── Tải & lọc dữ liệu ────────────────────────────────────────────────

        public async System.Threading.Tasks.Task LoadDataAsync(string chonMa = null)
        {
            if (_loading) return;
            _loading = true;
            _btnLamMoi.Enabled = false;
            _lblCount.Text = "Đang tải...";
            if (chonMa == null) chonMa = SelectedDto()?.MaKhachHang;
            try
            {
                _all = await _service.GetAllAsync();
                UpdateKpis();
                ApplyFilter(chonMa);
            }
            catch (ApiException ex)
            {
                _lblCount.Text = "Không tải được dữ liệu";
                if (!IsDisposed) Notify.Error(this, ex.Message, "Lỗi tải danh sách khách hàng");
            }
            finally
            {
                _loading = false;
                if (!IsDisposed) _btnLamMoi.Enabled = true;
            }
        }

        private void UpdateKpis()
        {
            int locked = _all.Count(k => k.BiKhoa);
            _kTong.Text = "Tổng khách hàng: " + _all.Count.ToString("N0");
            _kActive.Text = "Đang hoạt động: " + (_all.Count - locked).ToString("N0");
            _kLocked.Text = "Tài khoản bị khóa: " + locked.ToString("N0");
            _kChiTieu.Text = "Tổng chi tiêu: " + FormatTien(_all.Sum(k => k.TongChiTieu));
        }

        private void ApplyFilter(string chonMa = null)
        {
            if (chonMa == null) chonMa = SelectedDto()?.MaKhachHang;
            string q = (_txtSearch.Text ?? "").Trim();
            string tt = _cboTrangThai.SelectedItem as string;
            string gt = _cboGioiTinh.SelectedItem as string;
            int phanKhucIdx = _cboPhanKhuc.SelectedIndex;
            int sinhNhatIdx = _cboSinhNhat.SelectedIndex;
            int currentMonth = DateTime.Today.Month;

            var list = _all.Where(k =>
            {
                if (tt == "Hoạt động" && k.BiKhoa) return false;
                if (tt == "Đã khóa" && !k.BiKhoa) return false;
                if (gt != null && gt != "Tất cả" && !string.Equals(k.GioiTinh, gt, StringComparison.OrdinalIgnoreCase)) return false;

                if (phanKhucIdx == 1 && k.TongChiTieu < 2000000) return false;
                if (phanKhucIdx == 2 && k.SoDonHang < 3) return false;
                if (phanKhucIdx == 3 && k.SoDonHang < 1) return false;
                if (phanKhucIdx == 4 && k.SoDonHang > 0) return false;

                if (sinhNhatIdx > 0)
                {
                    var dob = k.NgaySinhDate;
                    if (!dob.HasValue) return false;
                    int targetMonth = sinhNhatIdx == 1 ? currentMonth : (sinhNhatIdx - 1);
                    if (dob.Value.Month != targetMonth) return false;
                }

                return UiStyler.MatchesSearch(q, k.MaKhachHang, k.HoTen, k.SoDienThoai, k.Email, k.DiaChi);
            }).OrderByDescending(k => k.MaKhachHang).ToList();

            _grid.SuspendLayout();
            _grid.Rows.Clear();
            int chonIndex = -1;
            foreach (var k in list)
            {
                int i = _grid.Rows.Add(k.MaKhachHang, k.HoTen, k.SoDienThoai, k.Email, k.GioiTinh,
                    (object)k.NgaySinhDate, k.SoDonHang, k.TongChiTieu, k.TrangThaiHienThi);
                _grid.Rows[i].Tag = k;
                if (chonMa != null && k.MaKhachHang == chonMa) chonIndex = i;
            }
            _grid.ResumeLayout();
            _grid.ClearSelection();
            if (chonIndex >= 0)
            {
                _grid.Rows[chonIndex].Selected = true;
                _grid.CurrentCell = _grid.Rows[chonIndex].Cells[0];
            }

            _lblCount.Text = list.Count == _all.Count
                ? "Hiển thị " + list.Count + " khách hàng"
                : "Hiển thị " + list.Count + " / " + _all.Count + " khách hàng";
            UpdateSelectionUi();
        }

        private static bool Contains(string s, string q) { return s != null && s.ToLowerInvariant().Contains(q); }

        private KhachHangDto SelectedDto()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].Tag as KhachHangDto;
        }

        private void UpdateSelectionUi()
        {
            var k = SelectedDto();
            bool has = k != null;
            _btnChiTiet.Enabled = has;
            _btnSua.Enabled = has;
            _btnKhoa.Enabled = has;
            _btnDoiMatKhau.Enabled = has;
            _btnXoa.Enabled = has;
            _btnKhoa.Text = has && k.BiKhoa ? "Mở khóa" : "Khóa tài khoản";
            _lblFooter.Text = has
                ? k.HoTen + " · " + (string.IsNullOrEmpty(k.SoDienThoai) ? "chưa có SĐT" : k.SoDienThoai) + " · " + k.SoDonHang + " đơn"
                : "Chọn một khách hàng để xem thao tác";
        }

        internal static string FormatTien(double v) { return v.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " đ"; }

        // ── Thao tác ─────────────────────────────────────────────────────────

        private async void ThemMoi()
        {
            using (var dlg = new KhachHangDialog(null))
            {
                if (Notify.ShowDialog(dlg, this) == DialogResult.OK)
                    await LoadDataAsync(dlg.SavedId);
            }
        }

        private async void Sua()
        {
            var k = SelectedDto();
            if (k == null) { Notify.Info(this, "Vui lòng chọn khách hàng cần sửa."); return; }
            using (var dlg = new KhachHangDialog(k))
            {
                if (Notify.ShowDialog(dlg, this) == DialogResult.OK)
                    await LoadDataAsync(k.MaKhachHang);
            }
        }

        private async void MoChiTiet()
        {
            var k = SelectedDto();
            if (k == null) return;
            using (var dlg = new KhachHangChiTietDialog(k.MaKhachHang))
            {
                Notify.ShowDialog(dlg, this);
                if (dlg.DataChanged) await LoadDataAsync(k.MaKhachHang);
            }
        }

        private async void DoiTrangThaiKhoa()
        {
            var k = SelectedDto();
            if (k == null) return;
            bool khoa = !k.BiKhoa;
            string hoi = khoa
                ? "Khóa tài khoản của khách hàng \"" + k.HoTen + "\"?\nKhách sẽ không đăng nhập và đặt hàng trên website được cho tới khi được mở khóa."
                : "Mở khóa tài khoản của khách hàng \"" + k.HoTen + "\"?";
            if (!Notify.Confirm(this, hoi, khoa ? "Khóa tài khoản" : "Mở khóa tài khoản", khoa)) return;

            _btnKhoa.Enabled = false;
            var r = await _service.SetLockedAsync(k.MaKhachHang, khoa);
            if (IsDisposed) return;
            if (r.Success) await LoadDataAsync(k.MaKhachHang);
            else { _btnKhoa.Enabled = true; Notify.Error(this, r.Message); }
        }

        private async void DoiMatKhau()
        {
            var k = SelectedDto();
            if (k == null) return;
            await HienThiDoiMatKhauDialog(k.MaKhachHang, k.HoTen, this);
        }

        internal static async Task HienThiDoiMatKhauDialog(string maKhachHang, string hoTen, IWin32Window owner = null)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Đổi mật khẩu khách hàng";
                dlg.Size = new Size(420, 260);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;
                dlg.Font = new Font(ThemeManager.FontFamily, 9.5F);

                var lblInfo = new Label
                {
                    Text = "Đặt lại mật khẩu đăng nhập cho khách hàng:\n" + hoTen + " (" + maKhachHang + ")",
                    Location = new Point(20, 16),
                    Size = new Size(365, 38),
                    ForeColor = ThemeManager.Ink
                };
                dlg.Controls.Add(lblInfo);

                var lblNew = new Label { Text = "Mật khẩu mới (tối thiểu 6 ký tự):", Location = new Point(20, 62), AutoSize = true };
                var txtNew = new TextBox { Location = new Point(20, 84), Width = 365, UseSystemPasswordChar = true };
                dlg.Controls.Add(lblNew);
                dlg.Controls.Add(txtNew);

                var lblConfirm = new Label { Text = "Xác nhận mật khẩu mới:", Location = new Point(20, 116), AutoSize = true };
                var txtConfirm = new TextBox { Location = new Point(20, 138), Width = 365, UseSystemPasswordChar = true };
                dlg.Controls.Add(lblConfirm);
                dlg.Controls.Add(txtConfirm);

                var chkShow = new CheckBox { Text = "Hiện mật khẩu", Location = new Point(20, 168), AutoSize = true };
                chkShow.CheckedChanged += (s, e) =>
                {
                    txtNew.UseSystemPasswordChar = !chkShow.Checked;
                    txtConfirm.UseSystemPasswordChar = !chkShow.Checked;
                };
                dlg.Controls.Add(chkShow);

                var btnLuu = new Button
                {
                    Text = "Lưu mật khẩu",
                    Location = new Point(190, 185),
                    Size = new Size(105, 30),
                    BackColor = ThemeManager.Primary,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnLuu.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btnLuu);

                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(305, 185),
                    Size = new Size(80, 30),
                    DialogResult = DialogResult.Cancel
                };
                dlg.Controls.Add(btnHuy);
                dlg.CancelButton = btnHuy;

                btnLuu.Click += async (s, e) =>
                {
                    var pass = (txtNew.Text ?? "").Trim();
                    var confirm = (txtConfirm.Text ?? "").Trim();
                    if (string.IsNullOrEmpty(pass) || pass.Length < 6)
                    {
                        MessageBox.Show(dlg, "Mật khẩu mới phải có tối thiểu 6 ký tự.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtNew.Focus();
                        return;
                    }
                    if (pass != confirm)
                    {
                        MessageBox.Show(dlg, "Xác nhận mật khẩu mới không khớp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtConfirm.Focus();
                        return;
                    }

                    btnLuu.Enabled = false;
                    var srv = new KhachHangService();
                    var res = await srv.ChangePasswordAsync(maKhachHang, pass);
                    if (res.Success)
                    {
                        MessageBox.Show(dlg, "Đổi mật khẩu cho khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                    else
                    {
                        btnLuu.Enabled = true;
                        MessageBox.Show(dlg, res.Message ?? "Đổi mật khẩu thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                if (owner != null) dlg.ShowDialog(owner);
                else dlg.ShowDialog();
            }
        }

        private async void Xoa()
        {
            var k = SelectedDto();
            if (k == null) return;

            // Nếu khách hàng có đơn hàng hoặc dữ liệu liên quan: Tạm khóa tài khoản
            if (k.SoDonHang > 0)
            {
                string msgSoft = "Khách hàng \"" + k.HoTen + "\" đã có " + k.SoDonHang + " đơn hàng trong hệ thống.\n\n"
                    + "Để bảo toàn dữ liệu hệ thống sẽ thực hiện khóa tài khoản.\n"
                    + "Khách hàng sẽ không thể đăng nhập đặt món nữa, dữ liệu lịch sử được giữ nguyên.\n\n"
                    + "Bạn có chắc chắn muốn khóa tài khoản khách hàng này không?";

                if (!Notify.Confirm(this, msgSoft, "Khóa tài khoản khách hàng", true)) return;

                _btnXoa.Enabled = false;
                var r = await _service.DeleteAsync(k.MaKhachHang);
                if (IsDisposed) return;
                if (r.Success)
                {
                    Notify.Info(this, r.Message ?? "Đã cập nhật khách hàng thành công.");
                    await LoadDataAsync(k.MaKhachHang);
                }
                else
                {
                    _btnXoa.Enabled = true;
                    Notify.Error(this, r.Message);
                }
                return;
            }

            // Nếu khách hàng chưa có dữ liệu giao dịch: Cho phép xóa vĩnh viễn nhưng PHẢI XÁC NHẬN LẠI (2 lần)
            string msgHard1 = "Khách hàng \"" + k.HoTen + "\" (" + k.MaKhachHang + ") chưa có dữ liệu giao dịch liên quan.\n"
                + "Bạn có muốn thực hiện xóa khách hàng này không?";
            if (!Notify.Confirm(this, msgHard1, "Xác nhận xóa khách hàng (Bước 1/2)", false)) return;

            // Xác nhận lại lần 2 (Double confirmation)
            string msgHard2 = "⚠️ XÁC NHẬN LẦN CUỐI:\n"
                + "Thao tác này sẽ XÓA VĨNH VIỄN khách hàng \"" + k.HoTen + "\" khỏi cơ sở dữ liệu và KHÔNG THỂ HOÀN TÁC!\n\n"
                + "Bạn có CHẮC CHẮN 100% muốn xóa vĩnh viễn không?";
            if (!Notify.Confirm(this, msgHard2, "Cảnh báo xóa vĩnh viễn (Bước 2/2)", true)) return;

            _btnXoa.Enabled = false;
            var resHard = await _service.DeleteAsync(k.MaKhachHang);
            if (IsDisposed) return;
            if (resHard.Success)
            {
                Notify.Info(this, resHard.Message ?? "Đã xóa vĩnh viễn khách hàng.");
                await LoadDataAsync();
            }
            else
            {
                _btnXoa.Enabled = true;
                Notify.Error(this, resHard.Message);
            }
        }
    }
}