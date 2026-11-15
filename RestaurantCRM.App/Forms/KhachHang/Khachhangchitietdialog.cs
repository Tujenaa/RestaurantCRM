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
    /// Xem hồ sơ khách hàng, lịch sử tương tác, các món hay gọi (suy ra từ đơn hàng)
    /// và sở thích khách tự khai báo (hồ sơ sở thích).
    /// </summary>
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
        private readonly DataGridView _soThich = new DataGridView();
        private RestaurantCRM.AdminApp.DTOs.KhaoSat.SoThichCuaKhachDto _st;
        private readonly TabControl _historyTabs = new TabControl();
        private readonly TextBox _txtSearchHistory = new TextBox();
        private readonly PillSearch _searchHistory;
        private readonly Label _emptyOrders = EmptyLabel("Khách hàng chưa có đơn hàng.");
        private readonly Label _emptyRatings = EmptyLabel("Khách hàng chưa gửi đánh giá nào.");
        private readonly Label _emptySurveys = EmptyLabel("Khách hàng chưa tham gia khảo sát nào.");
        private readonly Label _emptyPreferences = EmptyLabel("Chưa có dữ liệu món đã mua.");
        private readonly Label _emptySoThich = EmptyLabel("Khách chưa khai báo sở thích.");
        // Thanh lọc tab "Món hay gọi"
        private readonly CheckBox _chkChiHayGoi = new CheckBox { Text = "Chỉ món hay gọi", Checked = true, AutoSize = true, Margin = new Padding(4, 5, 12, 0) };
        private readonly Label _lblGhiChuHayGoi = new Label { AutoSize = true, UseMnemonic = false, Margin = new Padding(0, 8, 0, 0) };
        private readonly ToolTip _tip = new ToolTip();
        private readonly Button _btnSua;
        private readonly Button _btnKhoa;
        private readonly Button _btnDoiMatKhau;

        /// <summary>true nếu người dùng đã sửa / khóa trong hộp thoại (danh sách cần tải lại).</summary>
        public bool DataChanged { get; private set; }

        public KhachHangChiTietDialog(string maKhachHang)
            : base("Chi tiết khách hàng", "Mã khách hàng: " + maKhachHang, 1000, 600, null, "Đóng")
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
            _stats.SetBounds(L - 4, 152, W + 8, 92);
            _stats.ColumnCount = 4; _stats.RowCount = 1; _stats.BackColor = Color.White;
            _stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) _stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            Body.Controls.Add(_stats);

            _searchHistory = new PillSearch(_txtSearchHistory, "Tìm nhanh trong lịch sử đơn hàng, đánh giá, khảo sát, món ưa thích...");
            _searchHistory.SetBounds(L, 250, 440, 36);
            _txtSearchHistory.TextChanged += (s, e) => RenderHistoryTabs();
            Body.Controls.Add(_searchHistory);

            _historyTabs.SetBounds(L, 292, W, 276);
            _historyTabs.Font = new Font(ThemeManager.FontFamily, 9F);
            _historyTabs.Controls.Add(BuildTab("Đơn hàng", _orders, _emptyOrders));
            _historyTabs.Controls.Add(BuildTab("Đánh giá", _ratings, _emptyRatings));
            _historyTabs.Controls.Add(BuildTab("Khảo sát", _surveys, _emptySurveys));
            // "Món hay gọi" = suy ra từ lịch sử đơn; "Sở thích" = khách tự khai báo (không trộn lẫn)
            var tabMon = BuildTab("Món hay gọi", _preferences, _emptyPreferences);
            _lblGhiChuHayGoi.ForeColor = ThemeManager.Muted;
            _tip.SetToolTip(_chkChiHayGoi, "Hay gọi = gọi ≥ 2 lần và ≥ 30% số đơn hoàn tất");
            Disposed += (s, e) => _tip.Dispose();
            _chkChiHayGoi.CheckedChanged += (s, e) => RenderHistoryTabs();
            var barHayGoi = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.White,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };
            barHayGoi.Controls.Add(_chkChiHayGoi);
            barHayGoi.Controls.Add(_lblGhiChuHayGoi);
            // Thêm sau lưới để thanh nằm trên, lưới chiếm phần còn lại
            tabMon.Controls.Add(barHayGoi);
            _historyTabs.Controls.Add(tabMon);
            _historyTabs.Controls.Add(BuildTab("Sở thích", _soThich, _emptySoThich));

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
            // Món hay gọi: tính theo số đơn hoàn tất có món (không theo số phần)
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mon", HeaderText = "Món", FillWeight = 34 });
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "Loai", HeaderText = "Loại món", FillWeight = 18 });
            _preferences.Columns.Add(SoCol("SoLanGoi", "Lần gọi", 12));
            _preferences.Columns.Add(SoCol("TyLe", "% đơn", 11));
            _preferences.Columns.Add(SoCol("TongSL", "SL", 9));
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "GanNhat", HeaderText = "Gần nhất", FillWeight = 14 });
            _preferences.Columns.Add(new DataGridViewTextBoxColumn { Name = "Muc", HeaderText = "Mức", FillWeight = 13 });
            var cTim = new DataGridViewTextBoxColumn { Name = "Tim", HeaderText = "♥", FillWeight = 6 };
            cTim.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cTim.DefaultCellStyle.ForeColor = ThemeManager.Red;
            cTim.DefaultCellStyle.SelectionForeColor = ThemeManager.Red;
            cTim.ToolTipText = "Món khách tự khai là yêu thích";
            _preferences.Columns.Add(cTim);
            _soThich.Columns.Add(new DataGridViewTextBoxColumn { Name = "CauHoi", HeaderText = "Mục", FillWeight = 32 });
            _soThich.Columns.Add(new DataGridViewTextBoxColumn { Name = "LuaChon", HeaderText = "Khách đã chọn / ghi chú", FillWeight = 68 });
            _soThich.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _soThich.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            Body.Controls.Add(_historyTabs);
            // Dải tab mặc định -> thanh pill giống các màn khác
            TabSkin.Attach(_historyTabs);

            _btnSua = AddLeftButton("Sửa thông tin", ButtonSkin.Kind.Secondary);
            _btnKhoa = AddLeftButton("Khóa tài khoản", ButtonSkin.Kind.Secondary);
            _btnDoiMatKhau = AddLeftButton("Đổi mật khẩu", ButtonSkin.Kind.Secondary);
            _btnSua.Enabled = _btnKhoa.Enabled = _btnDoiMatKhau.Enabled = false;
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
            _btnDoiMatKhau.Click += async (s, e) =>
            {
                if (_kh == null) return;
                await KhachHangForm.HienThiDoiMatKhauDialog(_kh.MaKhachHang, _kh.HoTen, this);
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

            // Sở thích tự khai báo: lỗi riêng phần này thì vẫn hiện phần còn lại
            try
            {
                _st = await _service.GetSoThichAsync(_ma);
            }
            catch (ApiException)
            {
                _st = null;
            }
            if (IsDisposed) return;

            _btnSua.Enabled = _btnKhoa.Enabled = _btnDoiMatKhau.Enabled = true;
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

            RenderHistoryTabs();
        }

        private void RenderHistoryTabs()
        {
            if (_kh == null) return;
            string q = (_txtSearchHistory?.Text ?? "").Trim();

            _orders.Rows.Clear();
            var list = (_kh.LichSuHoaDon ?? new System.Collections.Generic.List<KhachHangHoaDonDto>())
                .FindAll(d => UiStyler.MatchesSearch(q, d.MaHoaDon, d.TrangThai, ThemeManager.CleanStatus(d.TrangThai),
                    d.NgayDat.HasValue ? d.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : ""));
            foreach (var d in list)
                _orders.Rows.Add(d.MaHoaDon, d.NgayDat.HasValue ? d.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    (d.TongTien ?? 0).ToString("#,##0 đ"), ThemeManager.CleanStatus(d.TrangThai));
            _historyTabs.TabPages[0].Text = "Đơn hàng (" + list.Count + ")";
            _emptyOrders.Visible = list.Count == 0;

            _ratings.Rows.Clear();
            var reviews = (_kh.LichSuDanhGia ?? new System.Collections.Generic.List<KhachHangDanhGiaDto>())
                .FindAll(d => UiStyler.MatchesSearch(q, d.TenMon, d.NoiDung, (d.SoSao ?? 0).ToString() + " sao",
                    d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : ""));
            foreach (var d in reviews)
            {
                int stars = Math.Max(0, Math.Min(5, d.SoSao ?? 0));
                _ratings.Rows.Add(
                    d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    string.IsNullOrWhiteSpace(d.TenMon) ? "Đánh giá dịch vụ" : d.TenMon,
                    stars > 0 ? new string('★', stars) + " " + stars + "/5" : "—",
                    string.IsNullOrWhiteSpace(d.NoiDung) ? "(Không có nhận xét bằng chữ)" : d.NoiDung,
                    d.HienThi ? "Hiển thị" : "Đã ẩn");
            }
            _historyTabs.TabPages[1].Text = "Đánh giá (" + reviews.Count + ")";
            _emptyRatings.Visible = reviews.Count == 0;

            _surveys.Rows.Clear();
            var surveys = (_kh.LichSuKhaoSat ?? new System.Collections.Generic.List<KhachHangKhaoSatDto>())
                .FindAll(d => UiStyler.MatchesSearch(q, d.TenKhaoSat, d.MaKhaoSat,
                    d.NgayTraLoi.HasValue ? d.NgayTraLoi.Value.ToString("dd/MM/yyyy HH:mm") : ""));
            foreach (var d in surveys)
                _surveys.Rows.Add(string.IsNullOrWhiteSpace(d.TenKhaoSat) ? d.MaKhaoSat : d.TenKhaoSat,
                    d.NgayTraLoi.HasValue ? d.NgayTraLoi.Value.ToString("dd/MM/yyyy HH:mm") : "—");
            _historyTabs.TabPages[2].Text = "Khảo sát (" + surveys.Count + ")";
            _emptySurveys.Visible = surveys.Count == 0;

            _preferences.Rows.Clear();
            var tatCaMon = _kh.MonUaThich ?? new List<KhachHangMonUaThichDto>();
            int soDon = _kh.SoDonHoanTat;
            int soHayGoi = tatCaMon.Count(d => d.HayGoi);

            // Món gọi ≥ 2 lần thì khách chắc chắn có ≥ 2 đơn: không cần hằng số riêng cho ngưỡng số đơn
            _chkChiHayGoi.Visible = soHayGoi > 0;
            bool chiHayGoi = soHayGoi > 0 && _chkChiHayGoi.Checked;

            if (soDon == 0) _lblGhiChuHayGoi.Text = "";
            else if (soDon < 2) _lblGhiChuHayGoi.Text = $"Chưa đủ dữ liệu ({soDon} đơn)";
            else if (soHayGoi == 0) _lblGhiChuHayGoi.Text = "Chưa có món gọi lặp lại";
            else _lblGhiChuHayGoi.Text = $"{soHayGoi}/{tatCaMon.Count} món · {soDon} đơn";

            var preferences = tatCaMon.FindAll(d => (!chiHayGoi || d.HayGoi)
                && UiStyler.MatchesSearch(q, d.TenMon, d.MaMon, d.TenLoaiMon, d.MucDo));
            using (var fDam = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold))
            {
                foreach (var d in preferences)
                {
                    int i = _preferences.Rows.Add(d.TenMon ?? d.MaMon ?? "Món ăn", d.TenLoaiMon ?? "—",
                        d.SoLanGoi.ToString("N0"), d.TyLeDon.ToString("0.#") + "%", d.SoLuongDaMua.ToString("N0"),
                        d.LanGanNhat.HasValue ? d.LanGanNhat.Value.ToString("dd/MM/yyyy") : "—",
                        d.MucDo ?? "", d.YeuThich ? "♥" : "");
                    var muc = _preferences.Rows[i].Cells["Muc"];
                    if (d.HayGoi)
                    {
                        muc.Style.Font = new Font(fDam, FontStyle.Bold);
                        muc.Style.ForeColor = muc.Style.SelectionForeColor = ThemeManager.Primary;
                    }
                    else muc.Style.ForeColor = ThemeManager.Muted;
                }
            }
            _emptyPreferences.Text = tatCaMon.Count == 0 ? "Chưa có dữ liệu món đã mua." : "Không tìm thấy món";
            _emptyPreferences.Visible = preferences.Count == 0;
            _historyTabs.TabPages[3].Text = $"Món hay gọi ({soHayGoi})";

            RenderSoThich(q);
        }

        /// <summary>Tab "Sở thích": nhóm theo câu hỏi, câu dị ứng / lưu ý tô đỏ và đứng đầu (API đã sắp).</summary>
        private void RenderSoThich(string q)
        {
            _soThich.Rows.Clear();
            var tab = _historyTabs.TabPages[4];
            if (_st == null || !_st.DaKhaiBao)
            {
                _emptySoThich.Text = _st == null ? "Không tải được sở thích của khách." : "Khách chưa khai báo sở thích.";
                _emptySoThich.Visible = true;
                tab.Text = "Sở thích";
                return;
            }

            int n = 0;
            foreach (var c in _st.CauHoi)
            {
                var nhan = c.DapAn.ConvertAll(d => (d.NoiDung ?? d.MaMon ?? "") + (d.NgungBan ? " (ngừng bán)" : ""));
                if (!string.IsNullOrWhiteSpace(c.NoiDung)) nhan.Add(c.NoiDung.Trim());
                string luaChon = string.Join("  ·  ", nhan);
                if (!UiStyler.MatchesSearch(q, c.CauHoi, luaChon)) continue;

                int i = _soThich.Rows.Add(c.CauHoi ?? c.MaCauHoi, luaChon);
                if (c.LaDiUng)
                {
                    var row = _soThich.Rows[i];
                    row.DefaultCellStyle.ForeColor = ThemeManager.Red;
                    row.DefaultCellStyle.SelectionForeColor = ThemeManager.Red;
                    row.DefaultCellStyle.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
                }
                n++;
            }
            _emptySoThich.Text = "Không có mục nào khớp từ khóa.";
            _emptySoThich.Visible = n == 0;
            tab.Text = "Sở thích" + (_st.NgayCapNhat.HasValue ? " · cập nhật " + _st.NgayCapNhat.Value.ToString("dd/MM/yyyy") : "");
        }

        private static DataGridViewTextBoxColumn SoCol(string name, string header, float weight)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, FillWeight = weight };
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            return c;
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
