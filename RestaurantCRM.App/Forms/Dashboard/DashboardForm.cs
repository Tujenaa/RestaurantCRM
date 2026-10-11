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

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public class DashboardForm : Form {
        private string _role;
        private readonly string _username;
        private readonly string _displayName;
        private readonly List<string> _roles;
        private readonly UcHeader _header;
        private readonly Panel _body;
        private readonly Panel _main;
        private readonly Label _userLabel;
        private Color _accent;
        private UcSidebar _sidebar;

        public DashboardForm() : this("admin", "Trần An", new List<string> { "Admin" }) { }
        public DashboardForm(string role) : this("admin", "Trần An", new List<string> { role }) { }
        public bool IsLoggedOut { get; private set; } = false;

        public DashboardForm(string username, string displayName, List<string> roles) {
            _username = username;
            _displayName = displayName;
            _roles = roles;
            if (_roles == null || _roles.Count == 0) throw new ArgumentException("Tài khoản chưa được gán vai trò.", "roles");
            _role = _roles[0];
            _accent = ThemeManager.AccentFor(_role);

            Text = _role + " Workspace | RestaurantCRM";
            Size = new Size(1180, 750);
            MinimumSize = new Size(980, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            _main = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg };
            Controls.Add(_main);

            _body = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(28, 20, 28, 28)
            };
            _main.Controls.Add(_body);

            _header = new UcHeader { Dock = DockStyle.Top, Height = 74 };
            _header.SetTitle("Dashboard", _role);
            _main.Controls.Add(_header);

            _userLabel = new Label {
                Text = "👤 " + _displayName + " (" + _role + ")",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(900, 26)
            };
            _header.Controls.Add(_userLabel);
            _header.Resize += (s, e) => _userLabel.Left = _header.ClientSize.Width - _userLabel.Width - 28;

            CreateSidebar();
            RenderDashboard();
        }

        private void CreateSidebar() {
            _sidebar = new UcSidebar(_role, _roles, _displayName);
            _sidebar.MenuSelected += OnMenuSelected;
            _sidebar.WorkspaceSelected += OpenWorkspace;
            Controls.Add(_sidebar);
            _main.BringToFront();
        }

        private void OpenWorkspace(string role) {
            if (_role != "Admin" || !_roles.Contains(role)) return;
            Form workspace = role == "Bán hàng"
                ? (Form)new BanHangKhoShellForm(_username, _displayName)
                : role == "CRM" ? new CrmShellForm(_username, _displayName) : null;
            if (workspace != null) workspace.ShowDialog(this);
        }

        private void OnMenuSelected(string item) {
            if (item == "Đăng xuất") { IsLoggedOut = true; Close(); return; }
            if (item == "Dashboard") {
                _header.SetTitle("Dashboard", _role);
                RenderDashboard();
                return;
            }
            if (!PermissionHelper.CanOpen(_role, item)) {
                MessageBox.Show(this, "Bạn không có quyền mở chức năng này trong phân hệ hiện tại.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Form form = CreateModule(item);
            if (form == null) return;
            form.ShowDialog(this);
        }

        private Form CreateModule(string item) {
            switch (item) {
                case "Món ăn": case "Thực đơn": return new MonAnForm();
                case "Loại món": return new LoaiMonForm();
                case "Kho hàng": case "Tồn kho": return new KhoForm();
                case "Nhà cung cấp": return new NhaCungCapForm();
                case "Phiếu nhập hàng": case "Phiếu nhập": return new PhieuNhapForm();
                case "Khuyến mãi": return new KhuyenMaiForm();
                case "Tài khoản": return new TaiKhoanForm();
                case "Vai trò & phân quyền": return new VaiTroForm();
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

        private void RenderDashboard() {
            _body.Controls.Clear();
            _body.SuspendLayout();

            int cardWidth = Math.Max(700, _body.ClientSize.Width - 56);

            // 1. Hero Welcome Banner
            var heroBanner = new RoundedPanel {
                Location = new Point(0, 0),
                Size = new Size(cardWidth, 90),
                Radius = 14,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(20, 16, 20, 16)
            };

            var lblGreeting = new Label {
                Text = "Xin chào, " + _displayName + " 👋",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(20, 16),
                BackColor = Color.Transparent
            };
            heroBanner.Controls.Add(lblGreeting);

            var lblSubtitle = new Label {
                Text = "Hôm nay là " + DateTime.Now.ToString("dddd, dd/MM/yyyy") + ". Chúc bạn có phiên làm việc năng suất và thuận lợi!",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(21, 48),
                BackColor = Color.Transparent
            };
            heroBanner.Controls.Add(lblSubtitle);

            // Live status badge
            var statusBadge = new Label {
                Text = "● Hệ thống sẵn sàng",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Green,
                BackColor = ThemeManager.GreenSoft,
                Size = new Size(140, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(heroBanner.Width - 160, 24)
            };
            statusBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, statusBadge.Width - 1, statusBadge.Height - 1), 10))
                using (var pen = new Pen(Color.FromArgb(60, ThemeManager.Green), 1)) {
                    e.Graphics.DrawPath(pen, path);
                }
            };
            heroBanner.Controls.Add(statusBadge);

            _body.Controls.Add(heroBanner);

            // 2. Metric KPI Cards Flow
            var cardsFlow = new FlowLayoutPanel {
                Location = new Point(0, 106),
                Size = new Size(cardWidth, 134),
                WrapContents = false,
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            UcThongKe card1 = null, card2 = null, card3 = null, card4 = null;
            if (_role == "Admin") {
                card1 = new UcThongKe("Món ăn", "...", "Trong thực đơn", "🍲", ThemeManager.Primary);
                card2 = new UcThongKe("Nhà cung cấp", "...", "Đối tác cung ứng", "🏢", ThemeManager.Teal);
                card3 = new UcThongKe("Phiếu nhập", "...", "Đã nhập tháng này", "📥", ThemeManager.Violet);
                card4 = new UcThongKe("Tài khoản", "...", "Nhân viên hoạt động", "👤", ThemeManager.Sky);
            } else if (_role == "Bán hàng") {
                card1 = new UcThongKe("Doanh thu", "...", "Hôm nay", "💰", ThemeManager.Green);
                card2 = new UcThongKe("Chờ xử lý", "...", "Đơn hàng mới", "⏳", ThemeManager.Amber);
                card3 = new UcThongKe("Đang phục vụ", "...", "Bàn tại quán", "🍽️", ThemeManager.Teal);
                card4 = new UcThongKe("Hoàn tất", "...", "Đơn giao thành công", "✅", ThemeManager.Primary);
            } else {
                card1 = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ khách", "👥", ThemeManager.Primary);
                card2 = new UcThongKe("Đánh giá TB", "...", "Lượt đánh giá", "⭐", ThemeManager.Amber);
                card3 = new UcThongKe("Phản hồi mới", "...", "Cần xử lý", "💬", ThemeManager.Violet);
                card4 = new UcThongKe("Khảo sát mở", "...", "Đang thu thập ý kiến", "📋", ThemeManager.Teal);
            }

            if (card1 != null) cardsFlow.Controls.Add(card1);
            if (card2 != null) cardsFlow.Controls.Add(card2);
            if (card3 != null) cardsFlow.Controls.Add(card3);
            if (card4 != null) cardsFlow.Controls.Add(card4);

            _body.Controls.Add(cardsFlow);
            LoadMetricsAsync(card1, card2, card3, card4);

            // 3. Quick Actions Panel
            var quickPanel = new RoundedPanel {
                Location = new Point(0, 256),
                Size = new Size(cardWidth, 230),
                Radius = 14,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(24, 20, 24, 20)
            };

            var lblQuickTitle = new Label {
                Text = "⚡ Thao tác nhanh",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(22, 16),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickTitle);

            var lblQuickSub = new Label {
                Text = "Truy cập nhanh các phân hệ nghiệp vụ phổ biến nhất",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(23, 40),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickSub);

            (string Title, string Cmd, string Icon)[] actions;
            if (_role == "Admin") {
                actions = new[] {
                    ("Tạo đơn hàng", "Tạo đơn hàng", "📝"),
                    ("Đơn hàng", "Đơn hàng", "🧾"),
                    ("Món ăn", "Món ăn", "🍲"),
                    ("Nhà cung cấp", "Nhà cung cấp", "🏢"),
                    ("Phiếu nhập hàng", "Phiếu nhập hàng", "📥"),
                    ("Khuyến mãi", "Khuyến mãi", "🎟️"),
                    ("Tài khoản", "Tài khoản", "👤"),
                    ("Phân quyền", "Vai trò & phân quyền", "🛡️")
                };
            } else if (_role == "Bán hàng") {
                actions = new[] {
                    ("Tạo đơn hàng", "Tạo đơn hàng", "📝"),
                    ("Đơn hàng", "Đơn hàng", "🧾"),
                    ("Thực đơn", "Thực đơn", "🍲"),
                    ("Khuyến mãi", "Khuyến mãi", "🎟️"),
                    ("Kiểm tra kho", "Kho hàng", "📦"),
                    ("Báo cáo bán hàng", "Báo cáo bán hàng", "📈")
                };
            } else {
                actions = new[] {
                    ("Khách hàng", "Khách hàng", "👥"),
                    ("Đánh giá dịch vụ", "Đánh giá", "⭐"),
                    ("Góp ý phản hồi", "Phản hồi", "💬"),
                    ("Khảo sát ý kiến", "Khảo sát", "📋"),
                    ("Phân tích khách hàng", "Phân tích khách hàng", "📊")
                };
            }

            int btnX = 22, btnY = 72;
            int btnW = 200, btnH = 46;
            foreach (var act in actions) {
                var btn = new Button {
                    Text = "  " + act.Icon + "  " + act.Title,
                    Tag = act.Cmd,
                    Size = new Size(btnW, btnH),
                    Location = new Point(btnX, btnY),
                    BackColor = ThemeManager.Surface2,
                    ForeColor = ThemeManager.Ink2,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = ThemeManager.Line;
                btn.FlatAppearance.MouseOverBackColor = ThemeManager.PrimarySoft;
                btn.MouseEnter += (s, e) => btn.ForeColor = ThemeManager.Primary;
                btn.MouseLeave += (s, e) => btn.ForeColor = ThemeManager.Ink2;
                btn.Click += (s, e) => OnMenuSelected((string)((Button)s).Tag);
                quickPanel.Controls.Add(btn);

                btnX += btnW + 14;
                if (btnX + btnW > quickPanel.ClientSize.Width - 20) {
                    btnX = 22;
                    btnY += btnH + 12;
                }
            }

            // Adjust height if needed
            quickPanel.Height = Math.Max(220, btnY + btnH + 24);
            _body.Controls.Add(quickPanel);

            // 4. Helpful Tips Card
            var tipsPanel = new RoundedPanel {
                Location = new Point(0, quickPanel.Bottom + 16),
                Size = new Size(cardWidth, 54),
                Radius = 12,
                FillColor = ThemeManager.PrimarySoft,
                BorderColor = Color.FromArgb(70, ThemeManager.Primary),
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTip = new Label {
                Text = "💡  Mẹo làm việc: Bạn có thể nhanh chóng chuyển phân hệ bằng menu 'CHUYỂN PHÂN HỆ' trên thanh bên trái nếu tài khoản được cấp nhiều quyền.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.PrimaryDark,
                AutoSize = true,
                Location = new Point(14, 16),
                BackColor = Color.Transparent
            };
            tipsPanel.Controls.Add(lblTip);
            _body.Controls.Add(tipsPanel);

            // Responsive resize handling
            _body.Resize += (s, e) => {
                int w = Math.Max(700, _body.ClientSize.Width - 56);
                heroBanner.Width = w;
                cardsFlow.Width = w;
                quickPanel.Width = w;
                tipsPanel.Width = w;
                statusBadge.Left = heroBanner.Width - statusBadge.Width - 20;
            };

            _body.ResumeLayout(true);
        }

        private async void LoadMetricsAsync(UcThongKe c1, UcThongKe c2, UcThongKe c3, UcThongKe c4) {
            try {
                using (var http = new System.Net.Http.HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/") }) {
                    var metrics = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<DashboardMetricsDto>(http, "api/ThongKe/DashboardMetrics");
                    if (metrics == null || IsDisposed) return;

                    void UpdateCards() {
                        if (IsDisposed) return;
                        if (_role == "Admin") {
                            c1?.SetValue(metrics.SoMonAn.ToString("N0"), "Trong thực đơn");
                            c2?.SetValue(metrics.SoNhaCungCap.ToString("N0"), "Đối tác cung ứng");
                            c3?.SetValue(metrics.SoPhieuNhapThangNay.ToString("N0"), "Đã nhập tháng này");
                            c4?.SetValue(metrics.SoTaiKhoan.ToString("N0"), "Nhân viên hệ thống");
                        } else if (_role == "Bán hàng") {
                            c1?.SetValue(metrics.DoanhThuHomNay.ToString("N0") + " đ", "Hôm nay");
                            c2?.SetValue(metrics.DonChoXuLy.ToString("N0"), "Đơn chờ xử lý");
                            c3?.SetValue(metrics.DonDangGiao.ToString("N0"), "Đang thực hiện");
                            c4?.SetValue(metrics.DonHoanThanhHomNay.ToString("N0"), "Hoàn thành hôm nay");
                        } else {
                            c1?.SetValue(metrics.TongKhachHang.ToString("N0"), "Tổng hồ sơ khách");
                            c2?.SetValue(metrics.DiemDanhGiaTb.ToString("0.0") + " / 5", "Trung bình đánh giá");
                            c3?.SetValue(metrics.PhanHoiChuaXuLy.ToString("N0"), "Chưa xử lý");
                            c4?.SetValue(metrics.KhaoSatDangMo.ToString("N0"), "Đang diễn ra");
                        }
                    }

                    if (InvokeRequired) Invoke((Action)UpdateCards);
                    else UpdateCards();
                }
            } catch {
                // Giữ nguyên giá trị mặc định nếu API chưa sẵn sàng
            }
        }
    }
}


