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
        private UcRightPanel _rightPanel;

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
            Size = new Size(1280, 800);
            MinimumSize = new Size(1060, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            _main = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg };
            Controls.Add(_main);

            _rightPanel = new UcRightPanel();
            Controls.Add(_rightPanel);

            _body = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(24, 16, 24, 24)
            };
            _main.Controls.Add(_body);

            _header = new UcHeader { Dock = DockStyle.Top, Height = 64 };
            _header.SetTitle("Dashboard", _role);
            _main.Controls.Add(_header);

            _userLabel = new Label {
                Text = "👤 " + _displayName + " (" + _role + ")",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(900, 22)
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

            int contentW = Math.Max(720, _body.ClientSize.Width - 48);

            // 1. Header: "Good Morning, Sandy!" & "Your Highlights"
            var headerPanel = new Panel {
                Location = new Point(0, 0),
                Size = new Size(contentW, 64),
                BackColor = Color.Transparent
            };

            var lblGreeting = new Label {
                Text = "Good Morning, " + _displayName + "!",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E1B4B"),
                AutoSize = true,
                Location = new Point(4, 4),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblGreeting);

            var lblHighlights = new Label {
                Text = "Your Highlights",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#8B5CF6"),
                AutoSize = true,
                Location = new Point(6, 36),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblHighlights);

            var lblDateSub = new Label {
                Text = "·  " + DateTime.Now.ToString("dddd, dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(106, 36),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblDateSub);

            _body.Controls.Add(headerPanel);

            // 2. The 3 Highlight Cards (Side-by-side)
            int cardGap = 16;
            int cardW = Math.Max(210, (contentW - cardGap * 2) / 3);

            var cardsContainer = new Panel {
                Location = new Point(0, 70),
                Size = new Size(contentW, 245),
                BackColor = Color.Transparent
            };

            var card1 = new UcHighlightCard(HighlightCardType.TeamStats, "Team Stats", "Dữ liệu 2024-2026", _accent) {
                Location = new Point(0, 0),
                Size = new Size(cardW, 240)
            };
            card1.DetailsClicked += (s, e) => {
                if (_role == "Bán hàng") OnMenuSelected("Báo cáo bán hàng");
                else if (_role == "CRM") OnMenuSelected("Khách hàng");
                else OnMenuSelected("Món ăn");
            };
            cardsContainer.Controls.Add(card1);

            var card2 = new UcHighlightCard(HighlightCardType.Ideation, "Ideation Workshop", "Ý kiến & Khảo sát mới", ColorTranslator.FromHtml("#F59E0B")) {
                Location = new Point(cardW + cardGap, 0),
                Size = new Size(cardW, 240)
            };
            card2.DetailsClicked += (s, e) => {
                if (_role == "Admin" || _role == "CRM") OnMenuSelected("Khảo sát");
                else OnMenuSelected("Khuyến mãi");
            };
            cardsContainer.Controls.Add(card2);

            var card3 = new UcHighlightCard(HighlightCardType.Training, "Upcoming Training", "Đơn hàng & Thực đơn", ColorTranslator.FromHtml("#0284C7")) {
                Location = new Point((cardW + cardGap) * 2, 0),
                Size = new Size(cardW, 240)
            };
            card3.DetailsClicked += (s, e) => {
                if (_role == "Bán hàng" || _role == "Admin") OnMenuSelected("Tạo đơn hàng");
                else OnMenuSelected("Đánh giá");
            };
            cardsContainer.Controls.Add(card3);

            _body.Controls.Add(cardsContainer);

            // 3. Middle Section: Current Team Members Table (Left) & Team Growth Chart (Right)
            int midY = 328;
            int halfW = Math.Max(330, (contentW - cardGap) / 2);

            var membersTable = new UcCurrentMembersTable {
                Location = new Point(0, midY),
                Size = new Size(halfW, 270)
            };
            _body.Controls.Add(membersTable);

            var growthChart = new UcGrowthChart {
                Location = new Point(halfW + cardGap, midY),
                Size = new Size(halfW, 270)
            };
            _body.Controls.Add(growthChart);

            // 4. Metric KPI Cards Flow
            int kpiY = midY + 286;
            var cardsFlow = new FlowLayoutPanel {
                Location = new Point(0, kpiY),
                Size = new Size(contentW, 142),
                WrapContents = false,
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            UcThongKe kpi1 = null, kpi2 = null, kpi3 = null, kpi4 = null;
            if (_role == "Admin") {
                kpi1 = new UcThongKe("Món ăn", "...", "Trong thực đơn", "🍲", ThemeManager.Primary);
                kpi2 = new UcThongKe("Nhà cung cấp", "...", "Đối tác cung ứng", "🏢", ThemeManager.Teal);
                kpi3 = new UcThongKe("Phiếu nhập", "...", "Đã nhập tháng này", "📥", ThemeManager.Violet);
                kpi4 = new UcThongKe("Tài khoản", "...", "Nhân viên hoạt động", "👤", ThemeManager.Sky);
            } else if (_role == "Bán hàng") {
                kpi1 = new UcThongKe("Doanh thu", "...", "Hôm nay", "💰", ThemeManager.Green);
                kpi2 = new UcThongKe("Chờ xử lý", "...", "Đơn hàng mới", "⏳", ThemeManager.Amber);
                kpi3 = new UcThongKe("Đang phục vụ", "...", "Bàn tại quán", "🍽️", ThemeManager.Teal);
                kpi4 = new UcThongKe("Hoàn tất", "...", "Đơn giao thành công", "✅", ThemeManager.Primary);
            } else {
                kpi1 = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ khách", "👥", ThemeManager.Primary);
                kpi2 = new UcThongKe("Đánh giá TB", "...", "Lượt đánh giá", "⭐", ThemeManager.Amber);
                kpi3 = new UcThongKe("Phản hồi mới", "...", "Cần xử lý", "💬", ThemeManager.Violet);
                kpi4 = new UcThongKe("Khảo sát mở", "...", "Đang thu thập ý kiến", "📋", ThemeManager.Teal);
            }

            if (kpi1 != null) cardsFlow.Controls.Add(kpi1);
            if (kpi2 != null) cardsFlow.Controls.Add(kpi2);
            if (kpi3 != null) cardsFlow.Controls.Add(kpi3);
            if (kpi4 != null) cardsFlow.Controls.Add(kpi4);

            _body.Controls.Add(cardsFlow);
            LoadMetricsAsync(kpi1, kpi2, kpi3, kpi4);

            // 5. Quick Actions Panel
            int quickY = kpiY + 154;
            var quickPanel = new RoundedPanel {
                Location = new Point(0, quickY),
                Size = new Size(contentW, 200),
                Radius = 14,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(24, 18, 24, 18)
            };

            var lblQuickTitle = new Label {
                Text = "⚡ Thao tác nhanh & Phân hệ nghiệp vụ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(20, 14),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickTitle);

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

            int btnX = 20, btnY = 48;
            int btnW = 160, btnH = 54;
            int colCount = 0;
            foreach (var act in actions) {
                var btnPanel = new RoundedPanel {
                    Location = new Point(btnX, btnY),
                    Size = new Size(btnW, btnH),
                    Radius = 10,
                    FillColor = ThemeManager.Surface2,
                    BorderColor = ThemeManager.Line,
                    Cursor = Cursors.Hand,
                    Tag = act.Cmd
                };
                var iconLabel = new Label {
                    Text = act.Icon,
                    Font = new Font("Segoe UI Emoji", 13F),
                    Size = new Size(32, 32),
                    Location = new Point(10, 10),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    ForeColor = _accent
                };
                var titleLabel2 = new Label {
                    Text = act.Title,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = ThemeManager.Ink2,
                    Location = new Point(46, 17),
                    AutoSize = false,
                    Size = new Size(btnW - 50, 18),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent
                };
                btnPanel.Controls.Add(iconLabel);
                btnPanel.Controls.Add(titleLabel2);

                EventHandler mouseEnter = (s, e) => {
                    btnPanel.FillColor = ThemeManager.PrimarySoft;
                    btnPanel.BorderColor = Color.FromArgb(120, _accent);
                    titleLabel2.ForeColor = _accent;
                    btnPanel.Invalidate();
                };
                EventHandler mouseLeave = (s, e) => {
                    btnPanel.FillColor = ThemeManager.Surface2;
                    btnPanel.BorderColor = ThemeManager.Line;
                    titleLabel2.ForeColor = ThemeManager.Ink2;
                    btnPanel.Invalidate();
                };
                MouseEventHandler mouseClick = (s, e) => OnMenuSelected((string)btnPanel.Tag);
                btnPanel.MouseEnter += mouseEnter;
                btnPanel.MouseLeave += mouseLeave;
                btnPanel.MouseClick += mouseClick;
                iconLabel.MouseEnter += mouseEnter;
                iconLabel.MouseLeave += mouseLeave;
                iconLabel.MouseClick += mouseClick;
                titleLabel2.MouseEnter += mouseEnter;
                titleLabel2.MouseLeave += mouseLeave;
                titleLabel2.MouseClick += mouseClick;
                iconLabel.Cursor = Cursors.Hand;
                titleLabel2.Cursor = Cursors.Hand;

                quickPanel.Controls.Add(btnPanel);

                colCount++;
                btnX += btnW + 12;
                if (colCount % 4 == 0 || btnX + btnW > quickPanel.ClientSize.Width - 16) {
                    btnX = 20;
                    btnY += btnH + 10;
                    colCount = 0;
                }
            }

            quickPanel.Height = Math.Max(180, btnY + btnH + 20);
            _body.Controls.Add(quickPanel);

            // 6. Helpful Tips Card
            var tipsPanel = new RoundedPanel {
                Location = new Point(0, quickPanel.Bottom + 16),
                Size = new Size(contentW, 52),
                Radius = 12,
                FillColor = ThemeManager.PrimarySoft,
                BorderColor = Color.FromArgb(70, ThemeManager.Primary),
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTip = new Label {
                Text = "💡  Mẹo làm việc: Nhấp vào nút 'DETAILS' trên các thẻ nổi bật để mở trực tiếp phân hệ tương ứng.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.PrimaryDark,
                AutoSize = true,
                Location = new Point(14, 15),
                BackColor = Color.Transparent
            };
            tipsPanel.Controls.Add(lblTip);
            _body.Controls.Add(tipsPanel);

            // Responsive resize handling
            _body.Resize += (s, e) => {
                int w = Math.Max(720, _body.ClientSize.Width - 48);
                headerPanel.Width = w;
                cardsContainer.Width = w;
                int cW = Math.Max(210, (w - cardGap * 2) / 3);
                card1.Width = cW;
                card2.Width = cW; card2.Left = cW + cardGap;
                card3.Width = cW; card3.Left = (cW + cardGap) * 2;

                int hW = Math.Max(330, (w - cardGap) / 2);
                membersTable.Width = hW;
                growthChart.Width = hW; growthChart.Left = hW + cardGap;

                cardsFlow.Width = w;
                quickPanel.Width = w;
                tipsPanel.Width = w;
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


