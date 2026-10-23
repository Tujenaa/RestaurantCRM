using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = new Label { Text = "Khách hàng" },
                Subtitle = new Label { Text = "Hồ sơ khách hàng, lịch sử mua hàng và trạng thái tài khoản." },
                PrimaryActions = new[] { _btnThem },
                HeaderActions = new Control[] { _btnLamMoi },
                KpiLabels = new[] { _kTong, _kActive, _kLocked, _kChiTieu },
                Search = _txtSearch,
                SearchPlaceholder = "Tìm theo mã, tên, số điện thoại, email...",
                Filters = new[]
                {
                    new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }),
                    new FilterSpec(_cboGioiTinh, new Label { Text = "Giới tính" })
                },
                CountLabel = _lblCount,
                Grid = _grid,
                SecondaryActions = new[] { _btnChiTiet, _btnSua, _btnKhoa },
                DangerActions = new[] { _btnXoa },
                FooterInfo = _lblFooter
            });

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboGioiTinh.SelectedIndexChanged += (s, e) => ApplyFilter();
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
            string q = (_txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string tt = _cboTrangThai.SelectedItem as string;
            string gt = _cboGioiTinh.SelectedItem as string;

            var list = _all.Where(k =>
            {
                if (tt == "Hoạt động" && k.BiKhoa) return false;
                if (tt == "Đã khóa" && !k.BiKhoa) return false;
                if (gt != null && gt != "Tất cả" && !string.Equals(k.GioiTinh, gt, StringComparison.OrdinalIgnoreCase)) return false;
                if (q.Length == 0) return true;
                return Contains(k.MaKhachHang, q) || Contains(k.HoTen, q) || Contains(k.SoDienThoai, q) || Contains(k.Email, q);
            }).ToList();

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

        private async void Xoa()
        {
            var k = SelectedDto();
            if (k == null) return;
            if (k.SoDonHang > 0)
            {
                Notify.Warn(this, "Khách hàng \"" + k.HoTen + "\" đã có " + k.SoDonHang + " đơn hàng nên không thể xóa để giữ lịch sử.\nBạn có thể khóa tài khoản thay thế.", "Không thể xóa");
                return;
            }
            if (!Notify.Confirm(this, "Xóa vĩnh viễn khách hàng \"" + k.HoTen + "\" (" + k.MaKhachHang + ")?\nThao tác này không hoàn tác được.", "Xóa khách hàng", true)) return;

            _btnXoa.Enabled = false;
            var r = await _service.DeleteAsync(k.MaKhachHang);
            if (IsDisposed) return;
            if (r.Success) await LoadDataAsync();
            else { _btnXoa.Enabled = true; Notify.Error(this, r.Message); }
        }
    }
}