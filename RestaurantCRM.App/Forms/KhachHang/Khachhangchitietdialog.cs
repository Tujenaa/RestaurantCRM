using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhachHang;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhachHang
{
    /// <summary>Xem chi tiết khách hàng: thông tin liên hệ, số liệu mua hàng và lịch sử đơn hàng.</summary>
    public class KhachHangChiTietDialog : CrmDialog
    {
        private readonly KhachHangService _service = new KhachHangService();
        private readonly string _ma;
        private KhachHangChiTietDto _kh;

        private readonly Panel _profile = new Panel();
        private readonly TableLayoutPanel _stats = new TableLayoutPanel();
        private readonly DataGridView _orders = new DataGridView();
        private readonly Label _lblOrders = new Label();
        private readonly Button _btnSua;
        private readonly Button _btnKhoa;

        /// <summary>true nếu người dùng đã sửa / khóa trong hộp thoại (danh sách cần tải lại).</summary>
        public bool DataChanged { get; private set; }

        public KhachHangChiTietDialog(string maKhachHang)
            : base("Chi tiết khách hàng", "Mã khách hàng: " + maKhachHang, 780, 520, null, "Đóng")
        {
            _ma = maKhachHang;
            const int L = Gutter, W = 780 - Gutter * 2;

            // Khối hồ sơ (tự vẽ)
            _profile.SetBounds(L, 4, W, 152);
            _profile.BackColor = Color.White;
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(_profile, true, null);
            _profile.Paint += PaintProfile;
            Body.Controls.Add(_profile);

            // 4 ô số liệu
            _stats.SetBounds(L - 4, 162, W + 8, 100);   // ô KPI cần ~92px để số không tràn
            _stats.ColumnCount = 4; _stats.RowCount = 1; _stats.BackColor = Color.White;
            _stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) _stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            Body.Controls.Add(_stats);

            _lblOrders.SetBounds(L, 272, W, 22);
            _lblOrders.Text = "Lịch sử đơn hàng";
            _lblOrders.Font = new Font(ThemeManager.FontFamily, 10F, FontStyle.Bold);
            _lblOrders.ForeColor = ThemeManager.Ink;
            Body.Controls.Add(_lblOrders);

            var g = _orders;
            g.SetBounds(L, 300, W, 214);
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            g.ReadOnly = true; g.RowHeadersVisible = false; g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.BackgroundColor = Color.White; g.BorderStyle = BorderStyle.None;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ma", HeaderText = "Mã hóa đơn", FillWeight = 22 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngay", HeaderText = "Ngày đặt", FillWeight = 28, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" } });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tien", HeaderText = "Tổng tiền", FillWeight = 26, DefaultCellStyle = { Format = "#,##0 đ", Alignment = DataGridViewContentAlignment.MiddleRight } });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TT", HeaderText = "Trạng thái", FillWeight = 24 });
            Body.Controls.Add(g);
            UiStyler.StyleGrid(g);

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
                _orders.Rows.Add(d.MaHoaDon, (object)d.NgayDat, (object)(d.TongTien ?? 0), ThemeManager.CleanStatus(d.TrangThai));
            _lblOrders.Text = list.Count == 0 ? "Lịch sử đơn hàng — khách chưa có đơn nào" : "Lịch sử đơn hàng (" + list.Count + ")";
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