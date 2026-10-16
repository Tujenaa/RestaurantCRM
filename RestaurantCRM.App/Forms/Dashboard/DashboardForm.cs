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
            Size = new Size(1180, 760);
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
                case "Nguyên vật liệu": case "Nguyên liệu": return new NguyenLieuForm();
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

            int contentW = Math.Max(700, _body.ClientSize.Width - 48);

            // 1. Welcome Card (Tối giản, trang nhã, không màu mè)
            var welcomeCard = new RoundedPanel {
                Location = new Point(0, 0),
                Size = new Size(contentW, 76),
                Radius = 12,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(20, 14, 20, 14)
            };

            var lblGreeting = new Label {
                Text = "Xin chào, " + _displayName + " 👋",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent
            };
            welcomeCard.Controls.Add(lblGreeting);

            var lblSub = new Label {
                Text = "Hôm nay là " + DateTime.Now.ToString("dddd, dd/MM/yyyy") + "  ·  Chúc bạn một ngày làm việc hiệu quả và thuận lợi!",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(18, 42),
                BackColor = Color.Transparent
            };
            welcomeCard.Controls.Add(lblSub);

            // Status badge góc phải
            var statusBadge = new Label {
                Text = "● Hệ thống sẵn sàng",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = ThemeManager.Green,
                BackColor = ThemeManager.GreenSoft,
                Size = new Size(140, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(welcomeCard.Width - 156, 24)
            };
            welcomeCard.Controls.Add(statusBadge);
            _body.Controls.Add(welcomeCard);

            // 2. 4 Thẻ KPI thống kê chuyên nghiệp (UcThongKe)
            var cardsFlow = new FlowLayoutPanel {
                Location = new Point(0, 90),
                Size = new Size(contentW, 136),
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

            // 3. Hai cột: Thao tác nhanh (Trái) & Bảng thành viên / hồ sơ gần đây (Phải)
            int midY = 238;
            int gap = 16;
            int colW = Math.Max(320, (contentW - gap) / 2);

            // 3.1 Bảng thao tác nhanh
            var quickPanel = new RoundedPanel {
                Location = new Point(0, midY),
                Size = new Size(colW, 260),
                Radius = 12,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(18, 14, 18, 14)
            };

            var lblQuickTitle = new Label {
                Text = "⚡ Thao tác nhanh",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(14, 12),
                BackColor = Color.Transparent
            };
            quickPanel.Controls.Add(lblQuickTitle);

            var lblQuickSub = new Label {
                Text = "Truy cập nhanh các phân hệ nghiệp vụ chính",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(15, 34),
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
                    ("Phiếu nhập", "Phiếu nhập hàng", "📥"),
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
                    ("Báo cáo", "Báo cáo bán hàng", "📈")
                };
            } else {
                actions = new[] {
                    ("Khách hàng", "Khách hàng", "👥"),
                    ("Đánh giá", "Đánh giá", "⭐"),
                    ("Phản hồi", "Phản hồi", "💬"),
                    ("Khảo sát", "Khảo sát", "📋"),
                    ("Phân tích", "Phân tích khách hàng", "📊")
                };
            }

            int btnX = 14, btnY = 58;
            int btnW = Math.Max(120, (colW - 38) / 2);
            int btnH = 44;
            int colIdx = 0;
            foreach (var act in actions) {
                var btn = new Button {
                    Text = "  " + act.Icon + "  " + act.Title,
                    Location = new Point(btnX, btnY),
                    Size = new Size(btnW, btnH),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ThemeManager.Ink,
                    BackColor = ThemeManager.Surface2,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    Tag = act.Cmd
                };
                btn.FlatAppearance.BorderColor = ThemeManager.Line;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = ThemeManager.PrimarySoft;
                btn.Click += (s, e) => OnMenuSelected((string)((Button)s).Tag);

                quickPanel.Controls.Add(btn);
                colIdx++;
                btnX += btnW + 10;
                if (colIdx % 2 == 0) {
                    btnX = 14;
                    btnY += btnH + 8;
                }
            }
            quickPanel.Height = Math.Max(240, btnY + btnH + 14);
            _body.Controls.Add(quickPanel);

            // 3.2 Bảng thành viên / hồ sơ gần đây (Phải)
            var membersTable = new UcCurrentMembersTable {
                Location = new Point(colW + gap, midY),
                Size = new Size(colW, quickPanel.Height)
            };
            _body.Controls.Add(membersTable);

            // 4. Mẹo hướng dẫn chân trang
            var tipsPanel = new RoundedPanel {
                Location = new Point(0, quickPanel.Bottom + 16),
                Size = new Size(contentW, 46),
                Radius = 10,
                FillColor = ThemeManager.Surface2,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(16, 11, 16, 11)
            };

            var lblTip = new Label {
                Text = "💡  Mẹo: Bạn có thể sử dụng menu thanh bên trái hoặc các nút Thao tác nhanh để mở nhanh các chức năng.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(14, 13),
                BackColor = Color.Transparent
            };
            tipsPanel.Controls.Add(lblTip);
            _body.Controls.Add(tipsPanel);

            // Responsive resize handling
            _body.Resize += (s, e) => {
                int w = Math.Max(700, _body.ClientSize.Width - 48);
                welcomeCard.Width = w;
                statusBadge.Left = welcomeCard.Width - statusBadge.Width - 18;
                cardsFlow.Width = w;

                int half = Math.Max(320, (w - gap) / 2);
                quickPanel.Width = half;
                membersTable.Width = half;
                membersTable.Left = half + gap;

                int bW = Math.Max(120, (half - 38) / 2);
                int bX = 14, bY = 58, cI = 0;
                foreach (Control c in quickPanel.Controls) {
                    if (c is Button b) {
                        b.Location = new Point(bX, bY);
                        b.Width = bW;
                        cI++;
                        bX += bW + 10;
                        if (cI % 2 == 0) {
                            bX = 14;
                            bY += btnH + 8;
                        }
                    }
                }
                quickPanel.Height = Math.Max(240, bY + btnH + 14);
                membersTable.Height = quickPanel.Height;
                tipsPanel.Top = quickPanel.Bottom + 16;
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


