using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhachHang;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhachHang
{
    /// <summary>Xem hồ sơ khách hàng, lịch sử tương tác và các món thường mua.</summary>
    public class KhachHangChiTietDialog : CrmDialog
    {
        private readonly KhachHangService _service = new KhachHangService();
        private readonly string _ma;
        private KhachHangChiTietDto _kh;

        private readonly Panel _profile = new Panel();
        private readonly TableLayoutPanel _stats = new TableLayoutPanel();
        private readonly DataGridView _orders = new DataGridView();
        private readonly DataGridView _ratings = new DataGridView();
        private readonly DataGridView _surveys = new DataGridView();
        private readonly DataGridView _preferences = new DataGridView();
        private readonly TabControl _historyTabs = new TabControl();
        private readonly Label _emptyOrders = EmptyLabel("Khách hàng chưa có đơn hàng.");
        private readonly Label _emptyRatings = EmptyLabel("Khách hàng chưa gửi đánh giá nào.");
        private readonly Label _emptySurveys = EmptyLabel("Khách hàng chưa tham gia khảo sát nào.");
        private readonly Label _emptyPreferences = EmptyLabel("Chưa có dữ liệu món đã mua.");
        private readonly Button _btnSua;
        private readonly Button _btnKhoa;

        /// <summary>true nếu người dùng đã sửa / khóa trong hộp thoại (danh sách cần tải lại).</summary>
        public bool DataChanged { get; private set; }

        public KhachHangChiTietDialog(string maKhachHang)
            : base("Chi tiết khách hàng", "Mã khách hàng: " + maKhachHang, 1000, 560, null, "Đóng")
        {
            _ma = maKhachHang;
            const int L = Gutter, W = 1000 - Gutter * 2;

            // Khối hồ sơ (tự vẽ)
            _profile.SetBounds(L, 4, W, 144);
            _profile.BackColor = Color.White;
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(_profile, true, null);
            _profile.Paint += PaintProfile;
            Body.Controls.Add(_profile);

            // 4 ô số liệu
            _stats.SetBounds(L - 4, 152, W + 8, 96);
            _stats.ColumnCount = 4; _stats.RowCount = 1; _stats.BackColor = Color.White;
            _stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) _stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            Body.Controls.Add(_stats);

            _historyTabs.SetBounds(L, 258, W, 272);
            _historyTabs.Font = new Font(ThemeManager.FontFamily, 9F);
            _historyTabs.Controls.Add(BuildTab("Đơn hàng", _orders, _emptyOrders));
            _historyTabs.Controls.Add(BuildTab("Đánh giá", _ratings, _emptyRatings));
            _historyTabs.Controls.Add(BuildTab("Khảo sát", _surveys, _emptySurveys));
            _historyTabs.Controls.Add(BuildTab("Sở thích", _preferences, _emptyPreferences));

            _orders.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ma", HeaderText = "Mã hóa đơn", FillWeight = 22 });
            _orders.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngay", HeaderText = "Ngày đặt", FillWeight = 28 });
            _orders.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tien", HeaderText = "Tổng tiền", FillWeight = 26 });
            _orders.Columns.Add(new DataGridViewTextBoxColumn { Name = "TT", HeaderText = "Trạng thái", FillWeight = 24 });
            _ratings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngay", HeaderText = "Ngày đánh giá", FillWeight = 25 });
            _ratings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mon", HeaderText = "Món ăn", FillWeight = 28 });
            _ratings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao", HeaderText = "Số sao", FillWeight = 14 });
            _ratings.Columns.Add(new DataGridViewTextBoxColumn { Name = "NoiDung", HeaderText = "Nội dung", FillWeight = 60 });
            _ratings.Columns.Add(new DataGridViewTextBoxColumn { Name = "HienThi", HeaderText = "Trạng thái", FillWeight = 18 });
            _ratings.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _ratings.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _surveys.Columns.Add(new DataGridViewTextBoxColumn { Name = "KhaoSat", HeaderText = "Khảo sát", FillWeight = 65 });
            _surveys.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngay", HeaderText = "Ngày trả lời", FillWeight = 35 });
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mon", HeaderText = "Món ăn", FillWeight = 70 });
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoLuong", HeaderText = "Số lượng đã mua", FillWeight = 30 });
            Body.Controls.Add(_historyTabs);

            _btnSua = AddLeftButton("Sửa thông tin", ButtonSkin.Kind.Secondary);
            _btnKhoa = AddLeftButton("Khóa tài khoản", ButtonSkin.Kind.Secondary);
            _btnSua.Enabled = _btnKhoa.Enabled = false;
            _btnSua.Click += async (s, e) =>
            {
                if (_kh == null) return;
                using (var dlg = new KhachHangDialog(_kh))
                    if (dlg.ShowDialog(this) == DialogResult.OK) { DataChanged = true; await LoadAsync(); }
            };
            _btnKhoa.Click += async (s, e) =>
            {
                if (_kh == null) return;
                bool khoa = !_kh.BiKhoa;
                if (!Notify.Confirm(this, (khoa ? "Khóa" : "Mở khóa") + " tài khoản của \"" + _kh.HoTen + "\"?", khoa ? "Khóa tài khoản" : "Mở khóa tài khoản", khoa)) return;
                _btnKhoa.Enabled = false;
                var r = await _service.SetLockedAsync(_kh.MaKhachHang, khoa);
                if (IsDisposed) return;
                if (!r.Success) { _btnKhoa.Enabled = true; Notify.Error(this, r.Message); return; }
                DataChanged = true;
                await LoadAsync();
            };

            Shown += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                _kh = await _service.GetByIdAsync(_ma);
            }
            catch (ApiException ex)
            {
                if (IsDisposed) return;
                Notify.Error(this, ex.Message, "Không tải được chi tiết khách hàng");
                Close();
                return;
            }
            if (IsDisposed || _kh == null) return;

            _btnSua.Enabled = _btnKhoa.Enabled = true;
            _btnKhoa.Text = _kh.BiKhoa ? "Mở khóa" : "Khóa tài khoản";
            _profile.Invalidate();

            _stats.SuspendLayout();
            foreach (Control c in _stats.Controls) c.Dispose();
            _stats.Controls.Clear();
            AddStat(0, "Số đơn hàng", _kh.SoDonHang.ToString("N0"), ThemeManager.Primary);
            AddStat(1, "Tổng chi tiêu", KhachHangForm.FormatTien(_kh.TongChiTieu), ThemeManager.Green);
            AddStat(2, "Đánh giá đã gửi", _kh.SoDanhGia.ToString("N0"), ThemeManager.Amber);
            AddStat(3, "Khảo sát đã trả lời", _kh.SoKhaoSatDaTraLoi.ToString("N0"), ThemeManager.Violet);
            _stats.ResumeLayout();

            _orders.Rows.Clear();
            var list = _kh.LichSuHoaDon ?? new System.Collections.Generic.List<KhachHangHoaDonDto>();
            foreach (var d in list)
                _orders.Rows.Add(d.MaHoaDon, d.NgayDat.HasValue ? d.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    (d.TongTien ?? 0).ToString("#,##0 đ"), ThemeManager.CleanStatus(d.TrangThai));
            _historyTabs.TabPages[0].Text = "Đơn hàng (" + list.Count + ")";
            _emptyOrders.Visible = list.Count == 0;

            _ratings.Rows.Clear();
            var reviews = _kh.LichSuDanhGia ?? new System.Collections.Generic.List<KhachHangDanhGiaDto>();
            foreach (var d in reviews)
            {
                int stars = Math.Max(0, Math.Min(5, d.SoSao ?? 0));
                _ratings.Rows.Add(
                    d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    string.IsNullOrWhiteSpace(d.TenMon) ? "Đánh giá dịch vụ" : d.TenMon,
                    stars > 0 ? new string('★', stars) + " " + stars + "/5" : "—",
                    string.IsNullOrWhiteSpace(d.NoiDung) ? "(Không có nhận xét bằng chữ)" : d.NoiDung,
                    d.HienThi ? "Đang hiển thị" : "Đã ẩn");
            }
            _historyTabs.TabPages[1].Text = "Đánh giá (" + reviews.Count + ")";
            _emptyRatings.Visible = reviews.Count == 0;

            _surveys.Rows.Clear();
            var surveys = _kh.LichSuKhaoSat ?? new System.Collections.Generic.List<KhachHangKhaoSatDto>();
            foreach (var d in surveys)
                _surveys.Rows.Add(string.IsNullOrWhiteSpace(d.TenKhaoSat) ? d.MaKhaoSat : d.TenKhaoSat,
                    d.NgayTraLoi.HasValue ? d.NgayTraLoi.Value.ToString("dd/MM/yyyy HH:mm") : "—");
            _historyTabs.TabPages[2].Text = "Khảo sát (" + surveys.Count + ")";
            _emptySurveys.Visible = surveys.Count == 0;

            _preferences.Rows.Clear();
            var preferences = _kh.MonUaThich ?? new System.Collections.Generic.List<KhachHangMonUaThichDto>();
            foreach (var d in preferences)
                _preferences.Rows.Add(d.TenMon ?? d.MaMon ?? "Món ăn", d.SoLuongDaMua.ToString("N0"));
            _historyTabs.TabPages[3].Text = "Sở thích (" + preferences.Count + ")";
            _emptyPreferences.Visible = preferences.Count == 0;
        }

        private static Label EmptyLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ThemeManager.Muted,
                BackColor = Color.White,
                UseMnemonic = false
            };
        }

        private TabPage BuildTab(string title, DataGridView grid, Label empty)
        {
            var page = new TabPage(title) { BackColor = Color.White, Padding = new Padding(4) };
            empty.Visible = false;
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            UiStyler.StyleGrid(grid);
            page.Controls.Add(grid);
            page.Controls.Add(empty);
            empty.BringToFront();
            return page;
        }

        private void AddStat(int col, string caption, string value, Color accent)
        {
            var tile = new Label { Text = caption + ": " + value };
            var k = new KpiTile(tile, null, accent) { Dock = DockStyle.Fill, Margin = new Padding(4) };
            _stats.Controls.Add(k, col, 0);
        }

        private void PaintProfile(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            var r = new Rectangle(0, 0, _profile.Width - 1, _profile.Height - 1);
            using (var path = SkinUtil.Round(r, 14))
            {
                using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                using (var pen = new Pen(ThemeManager.Line)) g.DrawPath(pen, path);
            }
            if (_kh == null)
            {
                TextRenderer.DrawText(g, "Đang tải...", Font, r, ThemeManager.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            CrmPaint.Avatar(g, _kh.HoTen, new Rectangle(18, 18, 52, 52));
            using (var fName = new Font(ThemeManager.FontFamily, 13F, FontStyle.Bold))
            {
                var flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                // Đo KHÔNG kèm EndEllipsis: với Size.Empty, Windows trả về độ rộng của chuỗi đã bị cắt "N..."
                var nameSize = TextRenderer.MeasureText(g, _kh.HoTen ?? "", fName, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                int nameW = Math.Min(nameSize.Width, r.Width - 240);
                TextRenderer.DrawText(g, _kh.HoTen ?? "", fName, new Rectangle(84, 20, nameW, nameSize.Height), ThemeManager.Ink, flags);
                CrmPaint.StatusPill(g, _kh.TrangThaiHienThi, 84 + nameW + 10, 21);
            }
            using (var fSmall = new Font(ThemeManager.FontFamily, 9F))
                TextRenderer.DrawText(g, _kh.MaKhachHang, fSmall, new Point(85, 48), ThemeManager.Muted, TextFormatFlags.NoPrefix);

            // Thông tin liên hệ: 4 cột + địa chỉ một dòng
            using (var fCap = new Font(ThemeManager.FontFamily, 8.5F))
            using (var fVal = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold))
            {
                int x0 = 18, colW = (r.Width - x0 * 2) / 4;
                DrawInfo(g, fCap, fVal, "Số điện thoại", Or(_kh.SoDienThoai), x0, 80, colW - 14);
                DrawInfo(g, fCap, fVal, "Email", Or(_kh.Email), x0 + colW, 80, colW + colW / 2 - 14);
                DrawInfo(g, fCap, fVal, "Giới tính", Or(_kh.GioiTinh), x0 + colW * 5 / 2, 80, colW / 2 - 10);
                DrawInfo(g, fCap, fVal, "Ngày sinh", Or(_kh.NgaySinhHienThi), x0 + colW * 3, 80, colW - 14);
                DrawInfo(g, fCap, fVal, "Địa chỉ", Or(_kh.DiaChi), x0, 122, r.Width - x0 * 2, true);
            }
        }

        private static void DrawInfo(Graphics g, Font fCap, Font fVal, string caption, string value, int x, int y, int w, bool inline = false)
        {
            var flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            if (inline)
            {
                int cw = TextRenderer.MeasureText(g, caption + ":", fCap, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding).Width + 2;
                TextRenderer.DrawText(g, caption + ":", fCap, new Rectangle(x, y + 2, cw, 18), ThemeManager.Muted, flags);
                TextRenderer.DrawText(g, value, fVal, new Rectangle(x + cw + 6, y, w - cw - 6, 20), ThemeManager.Ink, flags);
                return;
            }
            TextRenderer.DrawText(g, caption, fCap, new Rectangle(x, y, w, 16), ThemeManager.Muted, flags);
            TextRenderer.DrawText(g, value, fVal, new Rectangle(x, y + 18, w, 20), ThemeManager.Ink, flags);
        }

        private static string Or(string s) { return string.IsNullOrWhiteSpace(s) ? "—" : s; }
    }
}
