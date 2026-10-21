using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Forms.BanHang;
using RestaurantCRM.AdminApp.Forms.BaoCao;
using RestaurantCRM.AdminApp.Forms.DanhGia;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Forms.KhachHang;
using RestaurantCRM.AdminApp.Forms.Kho;
using RestaurantCRM.AdminApp.Forms.KhuyenMai;
using RestaurantCRM.AdminApp.Forms.KhaoSat;
using RestaurantCRM.AdminApp.Forms.MonAn;
using RestaurantCRM.AdminApp.Forms.NhaCungCap;
using RestaurantCRM.AdminApp.Forms.PhanHoi;
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

        public DashboardForm() : this("admin", "Trần An", new List<string> { "Admin" }) { }
        public DashboardForm(string role) : this("admin", "Trần An", new List<string> { role }) { }
        public bool IsLoggedOut { get; private set; } = false;

        public DashboardForm(string username, string displayName, List<string> roles)
            : this(username, displayName, roles, roles != null && roles.Count > 0 ? roles[0] : "Admin") { }

        public DashboardForm(string username, string displayName, List<string> roles, string activeRole)
        {
            _username = username;
            _displayName = displayName;
            _roles = roles;
            if (_roles == null || _roles.Count == 0) throw new ArgumentException("Tài khoản chưa được gán vai trò.", "roles");
            _role = !string.IsNullOrEmpty(activeRole) && _roles.Contains(activeRole) ? activeRole : _roles[0];
            _accent = ThemeManager.AccentFor(_role);

            Text = _role + " Workspace | RestaurantCRM";
            Size = new Size(1180, 760);
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
                Padding = new Padding(24, 18, 24, 24)
            };
            _main.Controls.Add(_body);

            _header = new UcHeader { Dock = DockStyle.Top, Height = 68 };
            _header.SetTitle("Tổng quan", _role);
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
            if (role == "Admin")
            {
                workspace = new AdminShellForm(_username, _displayName, _roles);
            }
            else if (role == "Bán hàng")
            {
                workspace = new BanHangKhoShellForm(_username, _displayName, _roles);
            }
            else if (role == "CRM")
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
            if (item == "Đăng xuất") { IsLoggedOut = true; Close(); return; }
            if (item == "Dashboard")
            {
                _header.SetTitle("Dashboard", _role);
                RenderDashboard();
                return;
            }
            if (!PermissionHelper.CanOpen(_role, item))
            {
                MessageBox.Show(this, "Bạn không có quyền mở chức năng này trong phân hệ hiện tại.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Form form = CreateModule(item);
            if (form == null) return;
            form.ShowDialog(this);
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
                case "Đánh giá": return new DanhGiaForm();
                case "Phản hồi": return new PhanHoiForm();
                case "Khảo sát": return new KhaoSatForm();
                case "Phân tích khách hàng": return new PhanTichForm();
                case "Báo cáo bán hàng": return new BaoCaoBanHangForm();
                default: return null;
            }
        }

        private void RenderDashboard()
        {
            _body.Controls.Clear();
            _body.SuspendLayout();

            int contentW = Math.Max(700, _body.ClientSize.Width - 48);

            // ── 1. Welcome Banner ────────────────────────────────────────────
            var welcomeCard = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(contentW, 84),
                BackColor = ThemeManager.Paper
            };
            welcomeCard.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // card border
                using (var pen = new Pen(ThemeManager.Line, 1))
                    g.DrawRectangle(pen, 0, 0, welcomeCard.Width - 1, welcomeCard.Height - 1);
                // accent left bar
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, 4, welcomeCard.Height), _accent, ThemeManager.Violet,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    g.FillRectangle(brush, 0, 0, 4, welcomeCard.Height);
            };

            var lblGreeting = new Label
            {
                Text = "Xin chào, " + _displayName + " 👋",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(20, 14),
                BackColor = Color.Transparent
            };
            welcomeCard.Controls.Add(lblGreeting);

            var lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy") + "  ·  Chúc bạn một ngày làm việc hiệu quả!",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(22, 46),
                BackColor = Color.Transparent
            };
            welcomeCard.Controls.Add(lblDate);

            var statusBadge = new Label
            {
                Text = "● Hệ thống sẵn sàng",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = ThemeManager.Green,
                BackColor = ThemeManager.GreenSoft,
                Size = new Size(148, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(contentW - 164, 28)
            };
            welcomeCard.Controls.Add(statusBadge);
            _body.Controls.Add(welcomeCard);

            // ── 2. KPI Cards (TableLayoutPanel — 4 cột đều nhau) ────────────
            var kpiTable = new TableLayoutPanel
            {
                Location = new Point(0, 100),
                Size = new Size(contentW, 130),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            kpiTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int ci = 0; ci < 4; ci++) kpiTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            UcThongKe kpi1 = null, kpi2 = null, kpi3 = null, kpi4 = null;
            if (_role == "Admin")
            {
                kpi1 = new UcThongKe("Món ăn", "...", "Trong thực đơn", "🍲", ThemeManager.Primary);
                kpi2 = new UcThongKe("Nhà cung cấp", "...", "Đối tác cung ứng", "🏢", ThemeManager.Teal);
                kpi3 = new UcThongKe("Phiếu nhập", "...", "Đã nhập tháng này", "📥", ThemeManager.Violet);
                kpi4 = new UcThongKe("Tài khoản", "...", "Nhân viên hoạt động", "👤", ThemeManager.Sky);
            }
            else if (_role == "Bán hàng")
            {
                kpi1 = new UcThongKe("Doanh thu", "...", "Hôm nay", "💰", ThemeManager.Green);
                kpi2 = new UcThongKe("Chờ xử lý", "...", "Đơn hàng mới", "⏳", ThemeManager.Amber);
                kpi3 = new UcThongKe("Đang phục vụ", "...", "Bàn tại quán", "🍽️", ThemeManager.Teal);
                kpi4 = new UcThongKe("Hoàn tất", "...", "Đơn giao thành công", "✅", ThemeManager.Primary);
            }
            else
            {
                kpi1 = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ khách", "👥", ThemeManager.Primary);
                kpi2 = new UcThongKe("Đánh giá TB", "...", "Trung bình đánh giá", "⭐", ThemeManager.Amber);
                kpi3 = new UcThongKe("Phản hồi mới", "...", "Chưa xử lý", "💬", ThemeManager.Violet);
                kpi4 = new UcThongKe("Khảo sát mở", "...", "Đang diễn ra", "📋", ThemeManager.Teal);
            }

            Action<UcThongKe, int> addKpi = (k, col) => {
                k.Dock = DockStyle.Fill;
                k.Margin = col < 3 ? new Padding(0, 0, 14, 0) : new Padding(0);
                kpiTable.Controls.Add(k, col, 0);
            };
            if (kpi1 != null) addKpi(kpi1, 0);
            if (kpi2 != null) addKpi(kpi2, 1);
            if (kpi3 != null) addKpi(kpi3, 2);
            if (kpi4 != null) addKpi(kpi4, 3);

            _body.Controls.Add(kpiTable);
            LoadMetricsAsync(kpi1, kpi2, kpi3, kpi4);

            // ── 3. Hai cột dưới ──────────────────────────────────────────────
            int midY = 248;
            int gap = 16;
            int colW = Math.Max(320, (contentW - gap) / 2);

            // 3.1 Thao tác nhanh
            var quickPanel = new RoundedPanel
            {
                Location = new Point(0, midY),
                Size = new Size(colW, 260),
                Radius = 12,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line
            };

            var lblQuickTitle = new Label
            {
                Text = "⚡ Thao tác nhanh",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickTitle);

            var lblQuickSub = new Label
            {
                Text = "Truy cập nhanh các phân hệ nghiệp vụ chính",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(17, 36),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickSub);

            (string Title, string Cmd, string Icon)[] actions;
            if (_role == "Admin")
            {
                actions = new[] {
                    ("Tạo đơn hàng", "Tạo đơn hàng", "📝"),
                    ("Đơn hàng", "Đơn hàng", "🧾"),
                    ("Món ăn", "Món ăn", "🍲"),
                    ("Nhà cung cấp", "Nhà cung cấp", "🏢"),
                    ("Phiếu nhập", "Phiếu nhập hàng", "📥"),
                    ("Khuyến mãi", "Khuyến mãi", "🎟️"),
                    ("Tài khoản", "Tài khoản", "👤"),
                    ("Phân quyền", "Vai trò & phân quyền", "🛡️")
                };
            }
            else if (_role == "Bán hàng")
            {
                actions = new[] {
                    ("Tạo đơn hàng", "Tạo đơn hàng", "📝"),
                    ("Đơn hàng", "Đơn hàng", "🧾"),
                    ("Thực đơn", "Thực đơn", "🍲"),
                    ("Khuyến mãi", "Khuyến mãi", "🎟️"),
                    ("Kiểm tra kho", "Kho hàng", "📦"),
                    ("Báo cáo", "Báo cáo bán hàng", "📈")
                };
            }
            else
            {
                actions = new[] {
                    ("Khách hàng", "Khách hàng", "👥"),
                    ("Đánh giá", "Đánh giá", "⭐"),
                    ("Phản hồi", "Phản hồi", "💬"),
                    ("Khảo sát", "Khảo sát", "📋"),
                    ("Phân tích", "Phân tích khách hàng", "📊")
                };
            }

            int btnX = 14, btnY = 60;
            int btnW = Math.Max(120, (colW - 42) / 2);
            int btnH = 46;
            int colIdx = 0;
            foreach (var act in actions)
            {
                var btn = new Button
                {
                    Text = "  " + act.Icon + "  " + act.Title,
                    Location = new Point(btnX, btnY),
                    Size = new Size(btnW, btnH),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ThemeManager.Ink,
                    BackColor = ThemeManager.Surface2,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    Tag = act.Cmd
                };
                btn.FlatAppearance.BorderColor = ThemeManager.Line;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = ThemeManager.PrimarySoft;
                btn.Click += (s, e) => OnMenuSelected((string)((Button)s).Tag);
                quickPanel.Controls.Add(btn);
                colIdx++;
                btnX += btnW + 14;
                if (colIdx % 2 == 0) { btnX = 14; btnY += btnH + 8; }
            }
            quickPanel.Height = Math.Max(240, btnY + btnH + 18);
            _body.Controls.Add(quickPanel);

            // 3.2 Top Món ăn bán chạy
            var membersTable = new UcCurrentMembersTable
            {
                Location = new Point(colW + gap, midY),
                Size = new Size(colW, quickPanel.Height)
            };
            _body.Controls.Add(membersTable);

            _body.ResumeLayout(true);

            // ── Responsive ───────────────────────────────────────────────────
            _body.Resize += (s, e) => {
                int w = Math.Max(700, _body.ClientSize.Width - 48);
                welcomeCard.Width = w;
                statusBadge.Left = welcomeCard.Width - statusBadge.Width - 16;
                kpiTable.Width = w;

                int half = Math.Max(320, (w - gap) / 2);
                quickPanel.Width = half;
                membersTable.Width = half;
                membersTable.Left = half + gap;

                int bW = Math.Max(120, (half - 42) / 2);
                int bX = 14, bY = 60, cI = 0;
                foreach (Control c in quickPanel.Controls)
                {
                    if (c is Button b)
                    {
                        b.Location = new Point(bX, bY);
                        b.Width = bW;
                        cI++;
                        bX += bW + 14;
                        if (cI % 2 == 0) { bX = 14; bY += btnH + 8; }
                    }
                }
                quickPanel.Height = Math.Max(240, bY + btnH + 18);
                membersTable.Height = quickPanel.Height;
            };
        }

        private async void LoadMetricsAsync(UcThongKe c1, UcThongKe c2, UcThongKe c3, UcThongKe c4)
        {
            try
            {
                using (var http = new System.Net.Http.HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") })
                {
                    var metrics = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<DashboardMetricsDto>(http, "api/ThongKe/DashboardMetrics");
                    if (metrics == null || IsDisposed) return;

                    void UpdateCards()
                    {
                        if (IsDisposed) return;
                        if (_role == "Admin")
                        {
                            c1?.SetValue(metrics.SoMonAn.ToString("N0"), "Trong thực đơn");
                            c2?.SetValue(metrics.SoNhaCungCap.ToString("N0"), "Đối tác cung ứng");
                            c3?.SetValue(metrics.SoPhieuNhapThangNay.ToString("N0"), "Đã nhập tháng này");
                            c4?.SetValue(metrics.SoTaiKhoan.ToString("N0"), "Nhân viên hệ thống");
                        }
                        else if (_role == "Bán hàng")
                        {
                            c1?.SetValue(metrics.DoanhThuHomNay.ToString("N0") + " đ", "Hôm nay");
                            c2?.SetValue(metrics.DonChoXuLy.ToString("N0"), "Đơn chờ xử lý");
                            c3?.SetValue(metrics.DonDangGiao.ToString("N0"), "Đang thực hiện");
                            c4?.SetValue(metrics.DonHoanThanhHomNay.ToString("N0"), "Hoàn thành hôm nay");
                        }
                        else
                        {
                            c1?.SetValue(metrics.TongKhachHang.ToString("N0"), "Tổng hồ sơ khách");
                            c2?.SetValue(metrics.DiemDanhGiaTb.ToString("0.0") + " / 5", "Trung bình đánh giá");
                            c3?.SetValue(metrics.PhanHoiChuaXuLy.ToString("N0"), "Chưa xử lý");
                            c4?.SetValue(metrics.KhaoSatDangMo.ToString("N0"), "Đang diễn ra");
                        }
                    }

                    if (InvokeRequired) Invoke((Action)UpdateCards);
                    else UpdateCards();
                }
            }
            catch
            {
                // Giữ nguyên giá trị mặc định nếu API chưa sẵn sàng
            }
        }
    }
}