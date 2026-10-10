using System;
using System.Collections.Generic;
using System.Drawing;
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
            _username = username; _displayName = displayName; _roles = roles;
            if (_roles == null || _roles.Count == 0) throw new ArgumentException("Tài khoản chưa được gán vai trò.", "roles");
            _role = _roles[0];
            _accent = AccentFor(_role);
            Text = _role + " | RestaurantCRM"; Size = new Size(1024, 680);
            StartPosition = FormStartPosition.CenterScreen; BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);
            _main = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg }; Controls.Add(_main);
            _body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeManager.Bg, Padding = new Padding(28) }; _main.Controls.Add(_body);
            _header = new UcHeader { Dock = DockStyle.Top, Height = 76 }; _header.TitleLabel.Text = "Dashboard"; _main.Controls.Add(_header);
            _userLabel = new Label { Text = _displayName + "  ·  " + _username, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(900, 28) };
            _header.Controls.Add(_userLabel); _header.Resize += (s, e) => _userLabel.Left = _header.ClientSize.Width - _userLabel.Width - 24;
            CreateSidebar(); RenderDashboard();
        }

        private void CreateSidebar() {
            // Each shell keeps its own role. Admin can open an additional workspace
            // in a separate window without changing the active Admin shell.
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
        private static Color AccentFor(string role) { return role == "CRM" ? ThemeManager.Purple : role == "Bán hàng" ? Color.FromArgb(14, 124, 134) : ThemeManager.Blue; }

        private void OnMenuSelected(string item) {
            if (item == "Đăng xuất") { IsLoggedOut = true; Close(); return; }
            if (item == "Dashboard") { _header.TitleLabel.Text = item; RenderDashboard(); return; }
            if (!PermissionHelper.CanOpen(_role, item)) {
                MessageBox.Show(this, "Bạn không có quyền mở chức năng này trong phân hệ hiện tại.", "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Form form = CreateModule(item); if (form == null) return;
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
            string title = _role == "Admin" ? "Tổng quan hệ thống" : _role == "Bán hàng" ? "Tổng quan bán hàng" : "Tổng quan CRM";
            AddLabel(title, 26, 18, 20F, FontStyle.Bold, ThemeManager.Ink);
            AddLabel("Xin chào " + _displayName + ". Đây là tình hình hoạt động tổng quan.", 28, 56, 9.5F, FontStyle.Regular, ThemeManager.Muted);
            var cards = new FlowLayoutPanel { Location = new Point(26, 96), Size = new Size(Math.Max(600, _body.ClientSize.Width - 72), 132), WrapContents = false, BackColor = ThemeManager.Bg, AutoScroll = true };
            if (_role == "Admin") {
                cards.Controls.Add(new UcThongKe("Món ăn", "24", "Trong thực đơn")); cards.Controls.Add(new UcThongKe("Nhà cung cấp", "8", "Đối tác")); cards.Controls.Add(new UcThongKe("Phiếu nhập", "12", "Tháng này")); cards.Controls.Add(new UcThongKe("Tài khoản", "6", "Nhân viên"));
            } else if (_role == "Bán hàng") {
                cards.Controls.Add(new UcThongKe("Doanh thu", "8.42 tr", "Hôm nay")); cards.Controls.Add(new UcThongKe("Chờ xử lý", "5", "Đơn hàng")); cards.Controls.Add(new UcThongKe("Đang giao", "3", "Đơn hàng")); cards.Controls.Add(new UcThongKe("Hoàn tất", "28", "Hôm nay"));
            } else {
                cards.Controls.Add(new UcThongKe("Khách hàng", "128", "Tổng hồ sơ")); cards.Controls.Add(new UcThongKe("Đánh giá TB", "4.6 / 5", "42 lượt đánh giá")); cards.Controls.Add(new UcThongKe("Phản hồi mới", "4", "Cần xử lý")); cards.Controls.Add(new UcThongKe("Khảo sát mở", "2", "Đang hoạt động"));
            }
            _body.Controls.Add(cards);
            var quick = new Panel { Location = new Point(26, 252), Size = new Size(Math.Max(600, _body.ClientSize.Width - 72), 218), BackColor = ThemeManager.Paper, Padding = new Padding(20) };
            quick.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, quick.Width - 1, quick.Height - 1); };
            quick.Controls.Add(new Label { Text = "Truy cập nhanh", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(20, 18) });
            string[] actions = _role == "Admin" ? new[] { "Món ăn", "Nhà cung cấp", "Phiếu nhập hàng", "Khuyến mãi", "Tài khoản", "Vai trò & phân quyền" } : _role == "Bán hàng" ? new[] { "Tạo đơn hàng", "Đơn hàng", "Thực đơn", "Khuyến mãi", "Báo cáo bán hàng" } : new[] { "Khách hàng", "Đánh giá", "Phản hồi", "Khảo sát", "Phân tích khách hàng" };
            int x = 20, y = 64;
            foreach (string action in actions) {
                var button = new Button { Text = action, Size = new Size(185, 42), Location = new Point(x, y), BackColor = Color.White, ForeColor = _accent, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
                button.FlatAppearance.BorderColor = ThemeManager.Line; button.Click += (s, e) => OnMenuSelected(action); quick.Controls.Add(button);
                x += 198; if (x + 185 > quick.ClientSize.Width) { x = 20; y += 54; }
            }
            _body.Controls.Add(quick);
            var notice = new Label { Text = "Dữ liệu hiện tại là mẫu giao diện. Các thao tác trên bảng chỉ lưu trong phiên làm việc.", Location = new Point(26, 494), Size = new Size(Math.Max(600, _body.ClientSize.Width - 72), 48), ForeColor = ThemeManager.Muted, BackColor = Color.FromArgb(234, 241, 255), Padding = new Padding(12) };
            _body.Controls.Add(notice);
            _body.Resize += (s, e) => {
                int w = Math.Max(600, _body.ClientSize.Width - 72);
                cards.Width = w; quick.Width = w; notice.Width = w;
            };
        }
        private void AddLabel(string text, int x, int y, float size, FontStyle style, Color color) {
            _body.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", size, style), ForeColor = color });
        }
    }
}

