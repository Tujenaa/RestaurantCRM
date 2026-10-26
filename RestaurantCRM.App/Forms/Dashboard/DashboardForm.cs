using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Forms.BanHang;
using RestaurantCRM.AdminApp.Forms.BaoCao;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Forms.KhachHang;
using RestaurantCRM.AdminApp.Forms.Kho;
using RestaurantCRM.AdminApp.Forms.KhuyenMai;
using RestaurantCRM.AdminApp.Forms.KhaoSat;
using RestaurantCRM.AdminApp.Forms.PhanHoi;
using RestaurantCRM.AdminApp.Forms.MonAn;
using RestaurantCRM.AdminApp.Forms.NhaCungCap;
using RestaurantCRM.AdminApp.Forms.PhanTich;
using RestaurantCRM.AdminApp.Forms.TaiKhoan;
using RestaurantCRM.AdminApp.Forms.VaiTro;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.Dashboard
{
    public class DashboardForm : Form
    {
        private string _role;
        private readonly string _username;
        private readonly string _displayName;
        private readonly List<string> _roles;
        private readonly UcHeader _header;
        private readonly Panel _body;
        private readonly Panel _main;
        private Color _accent;
        private UcSidebar _sidebar;
        private Action _relayout;
        private Panel _host;
        private Form _current;

        public DashboardForm() : this("admin", "Trần An", new List<string> { "Hệ thống" }) { }
        public DashboardForm(string role) : this("admin", "Trần An", new List<string> { role }) { }
        public bool IsLoggedOut { get; private set; } = false;

        public DashboardForm(string username, string displayName, List<string> roles)
            : this(username, displayName, roles, roles != null && roles.Count > 0 ? roles[0] : "Hệ thống") { }

        public DashboardForm(string username, string displayName, List<string> roles, string activeRole)
        {
            _username = username;
            _displayName = displayName;
            _roles = roles;
            if (_roles == null || _roles.Count == 0) throw new ArgumentException("Tài khoản chưa được gán vai trò.", "roles");
            _role = !string.IsNullOrEmpty(activeRole) && _roles.Contains(activeRole) ? activeRole : _roles[0];
            _accent = ThemeManager.AccentFor(_role);

            Text = ThemeManager.DisplayWorkspaceName(_role) + " Workspace | RestaurantCRM";
            var wa = Screen.PrimaryScreen.WorkingArea;
            Size = new Size(Math.Min(1320, wa.Width), Math.Min(820, wa.Height));
            MinimumSize = new Size(980, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            _main = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg };
            Controls.Add(_main);

            _body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0)
            };
            _main.Controls.Add(_body);
            _body.Resize += (s, e) => _relayout?.Invoke();

            // Vùng chứa màn hình chức năng (mở ngay trong cửa sổ chính, không popup)
            _host = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg, Padding = new Padding(8, 0, 8, 8), Visible = false };
            _main.Controls.Add(_host);

            _header = new UcHeader { Dock = DockStyle.Top };
            _header.SetTitle("Dashboard", _role);
            _header.SetUserInfo(_displayName, _role);
            _main.Controls.Add(_header);

            CreateSidebar();
            RenderDashboard();
        }

        private void CreateSidebar()
        {
            _sidebar = new UcSidebar(_role, _roles, _displayName);
            _sidebar.MenuSelected += OnMenuSelected;
            _sidebar.WorkspaceSelected += OpenWorkspace;
            Controls.Add(_sidebar);
            _main.BringToFront();
        }

        private void OpenWorkspace(string role)
        {
            if (role == _role || !_roles.Contains(role)) return;

            Form workspace = null;
            if (role == "Hệ thống" || role == "Admin")
            {
                workspace = new AdminShellForm(_username, _displayName, _roles);
            }
            else if (role == "Vận Hành" || role == "Bán hàng")
            {
                workspace = new BanHangKhoShellForm(_username, _displayName, _roles);
            }
            else if (role == "Khách hàng" || role == "CRM")
            {
                workspace = new CrmShellForm(_username, _displayName, _roles);
            }

            if (workspace != null)
            {
                this.Hide();
                workspace.ShowDialog();
                if ((workspace as DashboardForm)?.IsLoggedOut == true)
                {
                    this.IsLoggedOut = true;
                    this.Close();
                }
                else
                {
                    this.Show();
                }
            }
        }

        private void OnMenuSelected(string item)
        {
            if (item == "Đăng xuất") { CloseModule(); IsLoggedOut = true; Close(); return; }
            if (item == "Dashboard")
            {
                ShowDashboard();
                return;
            }
            if (!PermissionHelper.CanOpen(_role, item))
            {
                MessageBox.Show(this, "Bạn không có quyền mở chức năng này trong phân hệ hiện tại.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Form form;
            try { form = CreateModule(item); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không mở được chức năng \"" + item + "\".\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (form == null) return;
            ShowModule(item, form);
        }

        // ─────────────── Mở màn hình chức năng trong cửa sổ chính ───────────────
        private void ShowModule(string item, Form form)
        {
            CloseModule();
            _current = form;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.StartPosition = FormStartPosition.Manual;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Dock = DockStyle.Fill;
            form.FormClosed += (s, e) =>
            {
                if (_current != form) return;
                _current = null;
                if (!IsDisposed && !Disposing) ShowDashboard();
            };

            _host.SuspendLayout();
            _host.Controls.Add(form);
            _host.ResumeLayout(true);
            _body.Visible = false;
            _host.Visible = true;
            _host.BringToFront();
            _header.SetTitle(TitleFor(item), _role);
            _sidebar.SetActive(item);
            form.Show();
        }

        private void CloseModule()
        {
            var f = _current;
            _current = null;
            if (f == null) return;
            try { f.Close(); } catch { }
            try { if (!f.IsDisposed) f.Dispose(); } catch { }
        }

        private void ShowDashboard()
        {
            CloseModule();
            _host.Visible = false;
            _body.Visible = true;
            _body.BringToFront();
            _header.SetTitle("Dashboard", _role);
            _sidebar.SetActive("Dashboard");
            RenderDashboard();
        }

        private static string TitleFor(string item)
        {
            switch (item)
            {
                case "Thực đơn": return "Thực đơn món ăn";
                case "Kho hàng": return "Kho hàng";
                case "Phiếu nhập hàng": return "Phiếu nhập hàng";
                case "Báo cáo bán hàng": return "Báo cáo doanh thu";
                case "Đơn hàng": return "Đơn hàng";
                default: return item;
            }
        }

        private Form CreateModule(string item)
        {
            switch (item)
            {
                case "Món ăn": case "Thực đơn": return new MonAnForm();
                case "Loại món": return new LoaiMonForm();
                case "Kho hàng": case "Tồn kho": return new KhoForm();
                case "Nguyên vật liệu": case "Nguyên liệu": return new NguyenLieuForm();
                case "Nhà cung cấp": return new NhaCungCapForm();
                case "Phiếu nhập hàng": case "Phiếu nhập": return new PhieuNhapForm();
                case "Khuyến mãi": return new KhuyenMaiForm();
                case "Tài khoản": return new TaiKhoanForm();
                case "Vai trò & phân quyền": return new VaiTroForm();
                case "Sao lưu dữ liệu": return new RestaurantCRM.AdminApp.Forms.Backup.BackupRestoreForm();
                case "Tạo đơn hàng": return new TaoDonHangForm();
                case "Đơn hàng": case "Hóa đơn": return new HoaDonForm();
                case "Khách hàng": return new KhachHangForm();
                case "Đánh giá": return new PhanHoiForm();   // phản hồi / góp ý của khách (DANH_GIA + TRA_LOI_DANH_GIA)
                case "Khảo sát": return new KhaoSatForm();
                case "Phân tích khách hàng": return new PhanTichForm();
                case "Báo cáo bán hàng": return new BaoCaoBanHangForm();
                default: return null;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  DASHBOARD — bố cục giống mẫu:
        //  [4 thẻ số liệu]                         [Hoạt động
        //  [Biểu đồ doanh thu theo tháng       ]     gần đây
        //  [Thao tác nhanh]  [Món bán chạy     ]     ...]
        // ═════════════════════════════════════════════════════════════════
        private void RenderDashboard()
        {
            _body.SuspendLayout();
            var old = new Control[_body.Controls.Count];
            _body.Controls.CopyTo(old, 0);
            _body.Controls.Clear();
            foreach (var c in old) c.Dispose();
            _body.AutoScrollPosition = new Point(0, 0);

            const int padX = 22;      // lề trái/phải (thẻ đã có 8px bóng đổ => mép thẻ cách ~30px)
            const int feedW = 300;
            const int gap = 4;        // khoảng giữa 2 control (cộng 16px bóng => ~20px giữa mép thẻ)

            // 1. Thẻ số liệu
            UcThongKe kpi1, kpi2, kpi3, kpi4;
            if (_role == "Hệ thống" || _role == "Admin")
            {
                kpi1 = new UcThongKe("Món ăn", "...", "Trong thực đơn", "🍲", ThemeManager.Primary);
                kpi2 = new UcThongKe("Nhà cung cấp", "...", "Đối tác cung ứng", "🏢", ThemeManager.Teal);
                kpi3 = new UcThongKe("Phiếu nhập", "...", "Tháng này", "📥", ThemeManager.Violet);
                kpi4 = new UcThongKe("Tài khoản", "...", "Nhân viên", "👤", ThemeManager.Sky);
            }
            else if (_role == "Vận Hành" || _role == "Bán hàng")
            {
                kpi1 = new UcThongKe("Doanh thu hôm nay", "...", "Đơn hoàn thành", "💰", ThemeManager.Green);
                kpi2 = new UcThongKe("Chờ xử lý", "...", "Đơn hàng mới", "⏳", ThemeManager.Amber);
                kpi3 = new UcThongKe("Đang thực hiện", "...", "Chuẩn bị / giao", "🍽", ThemeManager.Teal);
                kpi4 = new UcThongKe("Hoàn tất", "...", "Trong hôm nay", "✅", ThemeManager.Primary);
            }
            else
            {
                kpi1 = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ", "👥", ThemeManager.Primary);
                kpi2 = new UcThongKe("Điểm đánh giá", "...", "Trung bình", "⭐", ThemeManager.Amber);
                kpi3 = new UcThongKe("Lượt đánh giá", "...", "Đang hiển thị", "💬", ThemeManager.Violet);
                kpi4 = new UcThongKe("Khảo sát mở", "...", "Đang diễn ra", "📋", ThemeManager.Teal);
            }
            var kpis = new[] { kpi1, kpi2, kpi3, kpi4 };
            foreach (var k in kpis) _body.Controls.Add(k);

            // 2. Biểu đồ
            var chart = new UcLineChart
            {
                Title = "Doanh thu theo tháng",
                Caption = "Năm " + DateTime.Now.Year,
                Unit = "đ"
            };
            _body.Controls.Add(chart);

            // 3a. Thao tác nhanh
            var quick = BuildQuickActions();
            _body.Controls.Add(quick);

            // 3b. Món bán chạy
            var top = new UcTopList { Title = "Món bán chạy", Subtitle = "TOP 5 THEO SỐ LƯỢNG ĐÃ BÁN", Unit = "phần" };
            _body.Controls.Add(top);

            // 4. Hoạt động gần đây (cột phải)
            var feed = new UcActivityFeed { Title = (_role == "Khách hàng" || _role == "CRM") ? "Đánh giá mới nhất" : "Hoạt động gần đây" };
            _body.Controls.Add(feed);

            // Khoảng trống cuối trang để cuộn không sát mép
            var spacer = new Panel { Size = new Size(1, 1), BackColor = ThemeManager.Bg };
            _body.Controls.Add(spacer);

            _relayout = () =>
            {
                int w = Math.Max(760, _body.ClientSize.Width - padX * 2);
                int x0 = padX - _body.HorizontalScroll.Value;
                int y0 = 0 - _body.VerticalScroll.Value;
                int leftW = w - feedW - gap;

                // hàng thẻ
                int kw = (leftW - gap * 3) / 4;
                for (int i = 0; i < kpis.Length; i++)
                    kpis[i].SetBounds(x0 + i * (kw + gap), y0, kw, 136);

                // biểu đồ
                chart.SetBounds(x0, y0 + 136 + gap, leftW, 320);

                // hàng dưới
                int rowY = chart.Bottom + gap;
                int qw = (leftW - gap) / 2;
                int qCount = 0;
                foreach (Control c in quick.Controls) if (c is QuickActionButton) qCount++;
                int cols = QuickCols(qw);
                int rowH = Math.Max(286, 4 + 72 + ((qCount + cols - 1) / cols) * 58 + 20 + 20);
                quick.SetBounds(x0, rowY, qw, rowH);
                top.SetBounds(x0 + qw + gap, rowY, leftW - qw - gap, rowH);

                // cột hoạt động
                feed.SetBounds(x0 + leftW + gap, y0, feedW, top.Bottom - y0);
                spacer.Location = new Point(x0, top.Bottom + 24);

                // nút thao tác nhanh: lưới 2 cột
                LayoutQuickButtons(quick);
            };
            _relayout();
            _body.ResumeLayout(true);

            LoadMetricsAsync(kpi1, kpi2, kpi3, kpi4);
            LoadChartAsync(chart);
            LoadTopAsync(top);
            LoadFeedAsync(feed);
        }

        // ─────────────── Thao tác nhanh ───────────────
        private RoundedPanel BuildQuickActions()
        {
            var card = new RoundedPanel
            {
                Radius = 14,
                Shadow = 8,
                FillColor = ThemeManager.Paper,
                BorderColor = Color.FromArgb(120, ThemeManager.Line)
            };
            card.Paint += (s, e) =>
            {
                var r = card.CardRect;
                using (var f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                    TextRenderer.DrawText(e.Graphics, "Thao tác nhanh", f, new Point(r.X + 24, r.Y + 20), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                    TextRenderer.DrawText(e.Graphics, "MỞ NHANH CHỨC NĂNG", f, new Point(r.X + 24, r.Y + 44), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };

            (string Title, string Cmd, string Glyph)[] actions;
            if (_role == "Hệ thống" || _role == "Admin")
                actions = new[] {
                    ("Tài khoản", "Tài khoản", ThemeManager.Glyph.Contact),
                    ("Phân quyền", "Vai trò & phân quyền", ThemeManager.Glyph.Shield),
                    ("Sao lưu", "Sao lưu dữ liệu", ThemeManager.Glyph.Save),
                };
            else if (_role == "Vận Hành" || _role == "Bán hàng")
                actions = new[] {
                    ("Tạo đơn hàng", "Tạo đơn hàng", ThemeManager.Glyph.Cart),
                    ("Đơn hàng", "Đơn hàng", ThemeManager.Glyph.List),
                    ("Thực đơn", "Thực đơn", ThemeManager.Glyph.Book),
                    ("Khuyến mãi", "Khuyến mãi", ThemeManager.Glyph.Tag),
                    ("Kho hàng", "Kho hàng", ThemeManager.Glyph.Package),
                    ("Báo cáo", "Báo cáo bán hàng", ThemeManager.Glyph.Chart),
                };
            else
                actions = new[] {
                    ("Khách hàng", "Khách hàng", ThemeManager.Glyph.People),
                    ("Đánh giá", "Đánh giá", ThemeManager.Glyph.Star),
                    ("Khảo sát", "Khảo sát", ThemeManager.Glyph.Document),
                    ("Phân tích", "Phân tích khách hàng", ThemeManager.Glyph.Diagnostic),
                };

            foreach (var a in actions)
            {
                if (!PermissionHelper.CanOpen(_role, a.Cmd)) continue;
                var btn = new QuickActionButton(a.Title, a.Glyph) { Tag = a.Cmd };
                btn.Click += (s, e) => OnMenuSelected((string)((Control)s).Tag);
                card.Controls.Add(btn);
            }
            return card;
        }

        /// <summary>2 cột nếu đủ rộng để chữ không bị cắt, ngược lại 1 cột.</summary>
        private static int QuickCols(int cardWidth)
        {
            int inner = cardWidth - 16 - 40;          // trừ bóng đổ + lề trong
            return (inner - 10) / 2 >= 140 ? 2 : 1;
        }

        private static void LayoutQuickButtons(RoundedPanel card)
        {
            var r = card.CardRect;
            int cols = QuickCols(card.Width);
            int x = r.X + 20, y = r.Y + 72;
            int bw = (r.Width - 40 - 10 * (cols - 1)) / cols, bh = 48;
            int i = 0;
            foreach (Control c in card.Controls)
            {
                if (!(c is QuickActionButton)) continue;
                c.SetBounds(x + (i % cols) * (bw + 10), y + (i / cols) * (bh + 10), bw, bh);
                i++;
            }
        }

        /// <summary>Nút thao tác nhanh tự vẽ: icon trong vòng tròn nhạt + chữ, hover viền xanh.</summary>
        private class QuickActionButton : Control
        {
            private readonly string _glyph;
            private bool _hover;
            public QuickActionButton(string text, string glyph)
            {
                Text = text; _glyph = glyph;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Color.White;
            }
            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                var rc = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = ThemeManager.RoundedRect(rc, 10))
                {
                    using (var br = new SolidBrush(_hover ? ThemeManager.PrimarySoft : ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var pen = new Pen(_hover ? Color.FromArgb(120, ThemeManager.Primary) : ThemeManager.Line)) g.DrawPath(pen, path);
                }
                var ic = new Rectangle(10, (Height - 30) / 2, 30, 30);
                using (var br = new SolidBrush(_hover ? ThemeManager.Primary : Color.White)) g.FillEllipse(br, ic);
                using (var f = ThemeManager.Icon(9.5F))
                    TextRenderer.DrawText(g, _glyph, f, ic, _hover ? Color.White : ThemeManager.Primary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    TextRenderer.DrawText(g, Text, f, new Rectangle(ic.Right + 10, 0, Width - ic.Right - 14, Height),
                        _hover ? ThemeManager.Primary : ThemeManager.Ink, TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        // ─────────────── Tải dữ liệu ───────────────
        private System.Net.Http.HttpClient NewHttp()
        {
            return new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        private void OnUi(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a); else a();
        }

        private class DoanhThuThangDto { public int Thang { get; set; } public double DoanhThu { get; set; } }
        private class MonBanChayDto { public string TenMon { get; set; } public double TongSoLuongBan { get; set; } }
        private class DanhGiaFeedDto
        {
            public string TenKhachHang { get; set; }
            public string TenMon { get; set; }
            public int? SoSao { get; set; }
            public string NoiDung { get; set; }
            public DateTime? NgayDanhGia { get; set; }
        }

        private async void LoadChartAsync(UcLineChart chart)
        {
            var labels = new string[12];
            var values = new double[12];
            for (int i = 0; i < 12; i++) labels[i] = "T" + (i + 1);
            try
            {
                using (var http = NewHttp())
                {
                    var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<DoanhThuThangDto>>(
                        http, "api/ThongKe/DoanhThuTheoThang?nam=" + DateTime.Now.Year);
                    if (data != null)
                        foreach (var d in data)
                            if (d.Thang >= 1 && d.Thang <= 12) values[d.Thang - 1] = d.DoanhThu;
                }
            }
            catch { /* API chưa sẵn sàng -> biểu đồ hiển thị trống */ }
            OnUi(() => { if (!chart.IsDisposed) chart.SetData(labels, values); });
        }

        private async void LoadTopAsync(UcTopList top)
        {
            var items = new List<KeyValuePair<string, double>>();
            try
            {
                using (var http = NewHttp())
                {
                    var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<MonBanChayDto>>(http, "api/ThongKe/MonBanChay?top=5");
                    if (data != null)
                        foreach (var d in data) items.Add(new KeyValuePair<string, double>(d.TenMon, d.TongSoLuongBan));
                }
            }
            catch { }
            OnUi(() => { if (!top.IsDisposed) top.SetItems(items); });
        }

        private async void LoadFeedAsync(UcActivityFeed feed)
        {
            var items = new List<ActivityItem>();
            try
            {
                using (var http = NewHttp())
                {
                    if (_role == "Khách hàng" || _role == "CRM")
                    {
                        var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<DanhGiaFeedDto>>(http, "api/AdminDanhGia");
                        if (data != null)
                            foreach (var d in data)
                            {
                                if (items.Count >= 15) break;
                                string who = string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách ẩn danh" : d.TenKhachHang;
                                string stars = d.SoSao.HasValue ? d.SoSao.Value + "★" : "";
                                items.Add(new ActivityItem
                                {
                                    Who = who,
                                    Time = d.NgayDanhGia,
                                    Text = who + " đánh giá " + stars + (string.IsNullOrEmpty(d.TenMon) ? "" : " món " + d.TenMon),
                                    Status = null
                                });
                            }
                    }
                    else
                    {
                        var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<HoaDonDto>>(http, "api/AdminHoaDon");
                        if (data != null)
                            foreach (var d in data)
                            {
                                if (items.Count >= 15) break;
                                string who = string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách vãng lai" : d.TenKhachHang;
                                items.Add(new ActivityItem
                                {
                                    Who = who,
                                    Time = d.NgayDat,
                                    Text = who + " đặt đơn #" + d.MaHoaDon + " · " + (d.TongThanhToan ?? 0).ToString("N0") + " đ"
                                           + (string.IsNullOrEmpty(d.TrangThai) ? "" : " · " + ThemeManager.CleanStatus(d.TrangThai)),
                                    Status = d.TrangThai
                                });
                            }
                    }
                }
            }
            catch { }
            OnUi(() => { if (!feed.IsDisposed) feed.SetItems(items); });
        }

        private async void LoadMetricsAsync(UcThongKe c1, UcThongKe c2, UcThongKe c3, UcThongKe c4)
        {
            try
            {
                using (var http = NewHttp())
                {
                    var metrics = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<DashboardMetricsDto>(http, "api/ThongKe/DashboardMetrics");
                    if (metrics == null || IsDisposed) return;

                    OnUi(() =>
                    {
                        if (_role == "Hệ thống" || _role == "Admin")
                        {
                            c1?.SetValue(metrics.SoMonAn.ToString("N0"));
                            c2?.SetValue(metrics.SoNhaCungCap.ToString("N0"));
                            c3?.SetValue(metrics.SoPhieuNhapThangNay.ToString("N0"));
                            c4?.SetValue(metrics.SoTaiKhoan.ToString("N0"));
                        }
                        else if (_role == "Vận Hành" || _role == "Bán hàng")
                        {
                            c1?.SetValue(metrics.DoanhThuHomNay.ToString("N0") + " đ");
                            c2?.SetValue(metrics.DonChoXuLy.ToString("N0"));
                            c3?.SetValue(metrics.DonDangGiao.ToString("N0"));
                            c4?.SetValue(metrics.DonHoanThanhHomNay.ToString("N0"));
                        }
                        else
                        {
                            c1?.SetValue(metrics.TongKhachHang.ToString("N0"));
                            c2?.SetValue(metrics.DiemDanhGiaTb.ToString("0.0") + " / 5");
                            c3?.SetValue(metrics.TongDanhGia.ToString("N0"));
                            c4?.SetValue(metrics.KhaoSatDangMo.ToString("N0"));
                        }
                    });
                }
            }
            catch
            {
                OnUi(() => { foreach (var c in new[] { c1, c2, c3, c4 }) c?.SetValue("—", "Chưa kết nối API"); });
            }
        }
    }
}